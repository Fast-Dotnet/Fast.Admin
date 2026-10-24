// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.ComponentModel.DataAnnotations;
using Fast.Center.Domain;
using Fast.Core;
using Fast.File.Applications.Dto;
using Fast.Shared;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SqlSugar;

namespace Fast.File.Applications;

/// <summary>
/// 下载文件
/// </summary>
public partial class FileApplication
{
    /// <summary>
    /// 下载安装包
    /// </summary>
    [HttpGet("/file/package/{fileId:long}")]
    [ApiInfo("下载安装包", HttpRequestActionEnum.Download)]
    [AllowAnonymous]
    [DisabledRequestLog]
    public async Task<IActionResult> DownloadPackage([FromRoute] long fileId, [FromQuery] [Range(1, long.MaxValue)] long recordId,
        [FromQuery] [Required] [StringLength(50)] string appKey)
    {
        // 这里作为匿名下载，必须禁用 AOP，所以直接使用 NEW 的方式
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        ApplicationVersionModel applicationVersionModel = await db.Queryable<ApplicationVersionModel>()
            .Where(wh => wh.RecordId == recordId && wh.AppKey == appKey && wh.Status == CommonStatusEnum.Enable)
            .SingleAsync();
        if (applicationVersionModel == null)
        {
            // 数据不存在
            return new NotFoundResult();
        }

        ApplicationClientModel applicationClientModel = await db.Queryable<ApplicationClientModel>()
            .Where(wh => wh.AppKey == appKey)
            .SingleAsync();
        if (applicationClientModel == null)
        {
            // 数据不存在
            return new NotFoundResult();
        }

        FileModel fileInfoModel = await db.Queryable<FileModel>().InSingleAsync(fileId);
        if (fileInfoModel == null)
        {
            // 文件不存在
            return new NotFoundResult();
        }

        if (!string.Equals(fileInfoModel.FileLocation, applicationVersionModel.FileUrl, StringComparison.OrdinalIgnoreCase))
        {
            // 文件版本地址对应不上
            return new NotFoundResult();
        }

        // 匿名下载仅允许安装包
        // 防止通过可猜测的 FileId 越权读取或在同源下执行活动内容
        if (!FileContext.Packages.Contains(fileInfoModel.FileMimeType.ToLowerInvariant()))
        {
            // 该文件不支持公开预览，请登录后下载
            return new NotFoundResult();
        }

        // 判断文件类型是否使用平台和更新方式
        if (applicationClientModel.AppType is not (AppEnvironmentEnum.Android
            or AppEnvironmentEnum.HarmonyOS
            or AppEnvironmentEnum.IOS
            or AppEnvironmentEnum.Windows))
        {
            // 不支持的平台
            return new NotFoundResult();
        }

        if (applicationVersionModel.PackageType == ApplicationPackageTypeEnum.FullPackage)
        {
            if (applicationClientModel.AppType == AppEnvironmentEnum.Android)
            {
                if (!string.Equals(fileInfoModel.FileSuffix, "apk", StringComparison.OrdinalIgnoreCase))
                {
                    // 不支持的平台
                    return new NotFoundResult();
                }
            }
            else if (applicationClientModel.AppType == AppEnvironmentEnum.HarmonyOS)
            {
                if (!string.Equals(fileInfoModel.FileSuffix, "app", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(fileInfoModel.FileSuffix, "hap", StringComparison.OrdinalIgnoreCase))
                {
                    // 不支持的平台
                    return new NotFoundResult();
                }
            }
            else if (applicationClientModel.AppType == AppEnvironmentEnum.IOS)
            {
                if (!string.Equals(fileInfoModel.FileSuffix, "ipa", StringComparison.OrdinalIgnoreCase))
                {
                    // 不支持的平台
                    return new NotFoundResult();
                }
            }
            else if (applicationClientModel.AppType == AppEnvironmentEnum.Windows)
            {
                if (!string.Equals(fileInfoModel.FileSuffix, "exe", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(fileInfoModel.FileSuffix, "msi", StringComparison.OrdinalIgnoreCase))
                {
                    // 不支持的平台
                    return new NotFoundResult();
                }
            }
            else
            {
                // 不支持的平台
                return new NotFoundResult();
            }
        }
        else if (applicationVersionModel.PackageType == ApplicationPackageTypeEnum.Wgt)
        {
            if (!string.Equals(fileInfoModel.FileSuffix, "wgt", StringComparison.OrdinalIgnoreCase))
            {
                // 不支持的平台
                return new NotFoundResult();
            }
        }
        else
        {
            // 不支持的平台
            return new NotFoundResult();
        }

        _httpContext.Response.Headers.CacheControl = "no-store";
        _httpContext.Response.Headers.XContentTypeOptions = "nosniff";

        string filePath = FileContext.GetLocalPath(_rootPath, fileInfoModel.FilePath, fileInfoModel.FileObjectName);
        if (!System.IO.File.Exists(filePath))
        {
            // 文件丢失或已被删除
            return new NotFoundResult();
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return new FileStreamResult(stream, fileInfoModel.FileMimeType)
        {
            FileDownloadName = fileInfoModel.FileOriginName,
            EnableRangeProcessing = true,
            EntityTag = new EntityTagHeaderValue($"\"{fileInfoModel.FileHash}\"")
        };
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [HttpPost]
    [ApiInfo("下载文件", HttpRequestActionEnum.Download)]
    public async Task<IActionResult> Download(DownloadFileInput input)
    {
        if (input.FileId == null && string.IsNullOrWhiteSpace(input.FileUrl))
        {
            throw new UserFriendlyException("文件Id和文件地址必须二选一！");
        }

        FileModel fileInfoModel = await _repository.Entities.WhereIF(input.FileId != null, wh => wh.FileId == input.FileId)
            .WhereIF(input.FileId == null, wh => wh.FileLocation == input.FileUrl)
            .SingleAsync();
        if (fileInfoModel == null)
        {
            throw new UserFriendlyException("文件不存在！");
        }

        // 登录后下载的文件不应被共享缓存
        _httpContext.Response.Headers.CacheControl = "private, no-store";

        string filePath = FileContext.GetLocalPath(_rootPath, fileInfoModel.FilePath, fileInfoModel.FileObjectName);
        if (!System.IO.File.Exists(filePath))
        {
            throw new UserFriendlyException("文件丢失或已被删除！");
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _httpContext.Response.Headers.XContentTypeOptions = "nosniff";
        return new FileStreamResult(stream, fileInfoModel.FileMimeType) {FileDownloadName = fileInfoModel.FileOriginName};
    }
}
