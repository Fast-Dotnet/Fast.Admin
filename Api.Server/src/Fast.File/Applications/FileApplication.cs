// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Core;
using Fast.DynamicApplication;
using Fast.File.Applications.Dto;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Fast.File.Applications;

/// <summary>
/// 文件服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.File, Name = "file")]
public partial class FileApplication : IDynamicApplication
{
    /// <summary>
    /// 应用根目录
    /// </summary>
    private readonly string _rootPath;

    /// <summary>
    /// 单张图片允许的最大像素总数（宽 × 高），用于避免超大图片解码时占用过多服务器内存
    /// </summary>
    private const long MaxImagePixels = 40_000_000;

    private readonly IUser _user;
    private readonly ISqlSugarRepository<FileModel> _repository;
    private readonly UploadFileSettingsOptions _uploadFileSettingsOptions;
    private readonly HttpContext _httpContext;

    /// <summary>
    /// 图片尺寸
    /// </summary>
    private readonly Dictionary<string, int> ImageSizes = new() {{"thumb", 100}, {"small", 300}, {"normal", 600}};

    /// <summary>
    /// 文件服务
    /// </summary>
    /// <param name="hostEnvironment">宿主环境</param>
    /// <param name="user">当前登录用户</param>
    /// <param name="repository">数据仓储</param>
    /// <param name="uploadFileSettingsOptions">文件上传配置</param>
    /// <param name="httpContextAccessor">HTTP 请求上下文访问器</param>
    public FileApplication(IWebHostEnvironment hostEnvironment, IUser user, ISqlSugarRepository<FileModel> repository,
        IOptions<UploadFileSettingsOptions> uploadFileSettingsOptions, IHttpContextAccessor httpContextAccessor)
    {
        _rootPath = hostEnvironment.ContentRootPath;
        _user = user;
        _repository = repository;
        _uploadFileSettingsOptions = uploadFileSettingsOptions.Value;
        _httpContext = httpContextAccessor.HttpContext;
    }

    /// <summary>
    /// 获取文件分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取文件分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.FilePaged)]
    public async Task<PagedResult<QueryFilePagedOutput>> QueryFilePaged(QueryFilePagedInput input)
    {
        ISugarQueryable<FileModel, TenantModel> queryable =
            _repository.Entities.LeftJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId);

        if (_user.IsSystemTenant)
        {
            queryable = queryable.ClearFilter<IBaseTEntity>()
                .WhereIF(input.TenantId != null, t1 => t1.TenantId == input.TenantId);
        }
        else
        {
            queryable = queryable.ClearFilter<IBaseTEntity>().Where(t1 => t1.TenantId == _user.TenantId);
        }

        if (!_user.IsSuperAdmin && !_user.IsAdmin)
        {
            queryable = queryable.Where(t1 => t1.CreatedUserId == _user.EmployeeId);
        }

        return await queryable.SelectMergeTable((t1, t2) => new QueryFilePagedOutput
            {
                FileId = t1.FileId,
                FileObjectName = t1.FileObjectName,
                FileOriginName = t1.FileOriginName,
                FileSuffix = t1.FileSuffix,
                FileMimeType = t1.FileMimeType,
                FileSizeKb = t1.FileSizeKb,
                FilePath = t1.FilePath,
                FileLocation = t1.FileLocation,
                FileHash = t1.FileHash,
                UploadDevice = t1.UploadDevice,
                UploadOS = t1.UploadOS,
                UploadBrowser = t1.UploadBrowser,
                UploadProvince = t1.UploadProvince,
                UploadCity = t1.UploadCity,
                UploadIp = t1.UploadIp,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                TenantName = t2.TenantName
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
