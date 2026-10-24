// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.ComponentModel.DataAnnotations;
using Fast.Cache;
using Fast.Center.Domain;
using Fast.Core;
using Fast.DynamicApplication;
using Fast.Shared;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SqlSugar;

namespace Fast.File.Applications;

/// <summary>
/// 预览文件
/// </summary>
public partial class FileApplication
{
    /// <summary>
    /// 预览文件
    /// </summary>
    [ApiDescriptionSettings(false)]
    [HttpGet("/file/{fileName}")]
    [ApiInfo("预览文件", HttpRequestActionEnum.Download)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [DisableRateLimiting]
    public async Task<IActionResult> Preview([FromRoute] [Required(ErrorMessage = "文件名称不能为空")] string fileName)
    {
        return await LocalPreview(fileName);
    }

    /// <summary>
    /// 预览图片
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <param name="size">
    /// 图片尺寸
    /// <para><c>thumb</c>：缩略图</para>
    /// <para><c>small</c>：小图</para>
    /// <para><c>normal</c>：正常尺寸</para>
    /// </param>
    [ApiDescriptionSettings(false)]
    [HttpGet("/file/{fileName}@!{size}")]
    [ApiInfo("预览图片", HttpRequestActionEnum.Download)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [DisableRateLimiting]
    public async Task<IActionResult> Preview([FromRoute] [Required(ErrorMessage = "文件名称不能为空")] string fileName,
        [FromRoute] [Required(ErrorMessage = "文件大小不能为空")] string size)
    {
        if (!ImageSizes.ContainsKey(size))
        {
            // 不支持的图片尺寸
            return new NotFoundResult();
        }

        size = $"@{size}";
        return await LocalPreview(fileName, size);
    }

    /// <summary>
    /// 预览文件
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <param name="size">
    /// 图片尺寸
    /// <para><c>thumb</c>：缩略图</para>
    /// <para><c>small</c>：小图</para>
    /// <para><c>normal</c>：正常尺寸</para>
    /// </param>
    /// <returns>文件预览响应</returns>
    private async Task<IActionResult> LocalPreview(string fileName, string size = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new NotFoundResult();
        }

        // 获取文件后缀
        string fileSuffix = Path.GetExtension(fileName);
        string fileIdStr = fileName[..^fileSuffix.Length];
        if (!long.TryParse(fileIdStr, out long fileId))
        {
            // 文件不存在
            return new NotFoundResult();
        }

        // 这里作为预览文件，必须禁用 AOP，所以直接使用 NEW 的方式
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        FileModel fileInfoModel = await db.Queryable<FileModel>().InSingleAsync(fileId);
        if (fileInfoModel == null)
        {
            // 文件不存在
            return new NotFoundResult();
        }

        // 图片预览
        if (FileContext.Images.Contains(fileInfoModel.FileMimeType.ToLowerInvariant()))
        {
            _httpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            _httpContext.Response.Headers.XContentTypeOptions = "nosniff";

            string localFileName = $"{fileInfoModel.FileId}{size}.{fileInfoModel.FileSuffix}";

            string localFilePath = FileContext.GetLocalPath(_rootPath, fileInfoModel.FilePath, localFileName);
            if (!System.IO.File.Exists(localFilePath))
            {
                // 文件丢失或已被删除
                return new NotFoundResult();
            }

            var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new FileStreamResult(stream, fileInfoModel.FileMimeType);
        }

        //// PDF预览
        //if (string.Equals(fileInfoModel.FileMimeType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        //{
        //    _httpContext.Response.Headers.ContentDisposition = "inline";
        //    _httpContext.Response.Headers.CacheControl = "public, no-cache";
        //    _httpContext.Response.Headers.XContentTypeOptions = "nosniff";

        //    string localFileName = $"{fileInfoModel.FileId}.{fileInfoModel.FileSuffix}";

        //    string localFilePath = FileContext.GetLocalPath(_rootPath, fileInfoModel.FilePath, localFileName);
        //    if (!System.IO.File.Exists(localFilePath))
        //    {
        //        // 文件丢失或已被删除
        //        return new NotFoundResult();
        //    }

        //    var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
        //        FileOptions.Asynchronous | FileOptions.SequentialScan);
        //    return new FileStreamResult(stream, fileInfoModel.FileMimeType)
        //    {
        //        EnableRangeProcessing = true, EntityTag = new EntityTagHeaderValue($"\"{fileInfoModel.FileHash}\"")
        //    };
        //}

        // 该文件不支持公开预览，请登录后下载
        return new NotFoundResult();
    }

    /// <summary>
    /// 播放媒体资源
    /// </summary>
    [ApiDescriptionSettings(false)]
    [HttpGet("/file/media/{token}")]
    [ApiInfo("播放媒体资源", HttpRequestActionEnum.Download)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [DisableRateLimiting]
    public async Task<IActionResult> PreviewMedia([FromRoute] [Required(ErrorMessage = "Token不能为空")] string token)
    {
        if (!FileContext.TryValidateMediaAssetToken(token, out MediaAssetTokenPayload tokenPayload))
        {
            // Token 格式、签名或有效期无效
            return new NotFoundResult();
        }

        ICache<AuthCCL> _authCache = _httpContext.RequestServices.GetService<ICache<AuthCCL>>();
        string sessionCacheKey = CacheConst.GetCacheKey(CacheConst.AuthUser, tokenPayload.AppNo, tokenPayload.TenantNo,
            tokenPayload.DeviceType.ToString(), tokenPayload.EmployeeNo, tokenPayload.SessionId);
        if (!tokenPayload.IsAnonymous && !await _authCache.ExistsAsync(sessionCacheKey))
        {
            // 这里是401
            return new UnauthorizedResult();
        }

        // 这里作为预览文件，必须禁用 AOP，所以直接使用 NEW 的方式
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        FileModel fileInfoModel = await db.Queryable<FileModel>().InSingleAsync(tokenPayload.FileId);
        if (fileInfoModel == null)
        {
            // 文件不存在
            return new NotFoundResult();
        }

        // 仅允许音频、视频
        if (!FileContext.Audios.Contains(fileInfoModel.FileMimeType.ToLowerInvariant())
            && !FileContext.Videos.Contains(fileInfoModel.FileMimeType.ToLowerInvariant()))
        {
            // 文件不存在
            return new NotFoundResult();
        }

        _httpContext.Response.Headers.CacheControl = "private, no-store";
        _httpContext.Response.Headers.XContentTypeOptions = "nosniff";

        string localFilePath = FileContext.GetLocalPath(_rootPath, fileInfoModel.FilePath, fileInfoModel.FileObjectName);
        if (!System.IO.File.Exists(localFilePath))
        {
            // 文件丢失或已被删除
            return new NotFoundResult();
        }

        var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
            FileOptions.Asynchronous);
        return new FileStreamResult(stream, fileInfoModel.FileMimeType) {EnableRangeProcessing = true};
    }
}
