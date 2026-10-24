// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.ApplicationVersion.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.ApplicationVersion;

/// <summary>
/// 应用版本服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "applicationVersion")]
[PlatformOnly]
public class ApplicationVersionService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ApplicationVersionModel> _repository;

    public ApplicationVersionService(ISqlSugarRepository<ApplicationVersionModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取应用版本分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取应用版本分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.AppVersion.Paged)]
    public async Task<PagedResult<QueryApplicationVersionPagedOutput>> QueryApplicationVersionPaged(
        QueryApplicationVersionPagedInput input)
    {
        return await _repository.Entities.LeftJoin<ApplicationClientModel>((t1, t2) => t1.AppKey == t2.AppKey)
            .LeftJoin<ApplicationModel>((t1, t2, t3) => t2.AppId == t3.AppId)
            .WhereIF(input.PackageType != null, t1 => t1.PackageType == input.PackageType)
            .WhereIF(input.Status != null, t1 => t1.Status == input.Status)
            .SelectMergeTable((t1, t2, t3) => new QueryApplicationVersionPagedOutput
            {
                RecordId = t1.RecordId,
                AppName = t3.AppName,
                AppKey = t1.AppKey,
                AppType = t2.AppType,
                EnvironmentType = t2.EnvironmentType,
                PackageType = t1.PackageType,
                VersionName = t1.VersionName,
                VersionCode = t1.VersionCode,
                Status = t1.Status,
                MinNativeVersionCode = t1.MinNativeVersionCode,
                MaxNativeVersionCode = t1.MaxNativeVersionCode,
                UpdateContent = t1.UpdateContent,
                IsForceUpdate = t1.IsForceUpdate,
                Remark = t1.Remark,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .OrderByIF(input.IsOrderBy, ob => ob.VersionCode, OrderByType.Desc)
            .OrderByIF(input.IsOrderBy, ob => ob.PackageType)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取应用版本详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取应用版本详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.AppVersion.Detail)]
    public async Task<QueryApplicationVersionDetailOutput> QueryApplicationVersionDetail(
        [Required(ErrorMessage = "记录Id不能为空")] long? recordId)
    {
        QueryApplicationVersionDetailOutput result = await _repository.Entities
            .LeftJoin<ApplicationClientModel>((t1, t2) => t1.AppKey == t2.AppKey)
            .LeftJoin<ApplicationModel>((t1, t2, t3) => t2.AppId == t3.AppId)
            .Where(t1 => t1.RecordId == recordId)
            .Select((t1, t2, t3) => new QueryApplicationVersionDetailOutput
            {
                RecordId = t1.RecordId,
                AppName = t3.AppName,
                AppKey = t1.AppKey,
                AppType = t2.AppType,
                EnvironmentType = t2.EnvironmentType,
                PackageType = t1.PackageType,
                VersionName = t1.VersionName,
                VersionCode = t1.VersionCode,
                Status = t1.Status,
                FileUrl = t1.FileUrl,
                MinNativeVersionCode = t1.MinNativeVersionCode,
                MaxNativeVersionCode = t1.MaxNativeVersionCode,
                UpdateContent = t1.UpdateContent,
                IsForceUpdate = t1.IsForceUpdate,
                Remark = t1.Remark,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加应用版本
    /// </summary>
    [HttpPost]
    [ApiInfo("添加应用版本", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.AppVersion.Add)]
    public async Task AddApplicationVersion(AddApplicationVersionInput input)
    {
        if (input.PackageType == ApplicationPackageTypeEnum.Wgt)
        {
            if (input.MinNativeVersionCode is not > 0
                || input.MaxNativeVersionCode is not > 0
                || input.MinNativeVersionCode > input.MaxNativeVersionCode)
            {
                throw new UserFriendlyException("WGT 必须提供有效的原生版本兼容区间！");
            }
        }

        if (input.PackageType == ApplicationPackageTypeEnum.FullPackage)
        {
            if (input.MinNativeVersionCode.HasValue || input.MaxNativeVersionCode.HasValue)
            {
                throw new UserFriendlyException("整包不能设置原生版本兼容区间！");
            }
        }

        if (await _repository.AnyAsync(a =>
                a.AppKey == input.AppKey && a.PackageType == input.PackageType && a.VersionCode == input.VersionCode))
        {
            throw new UserFriendlyException("应用版本重复！");
        }

        // 同版本整包和 WGT 共用展示版本
        if (await _repository.AnyAsync(a =>
                a.AppKey == input.AppKey && a.VersionCode == input.VersionCode && a.VersionName != input.VersionName))
        {
            throw new UserFriendlyException("同一版本号的整包与 WGT 必须使用相同版本名称！");
        }

        var applicationVersionModel = new ApplicationVersionModel
        {
            AppKey = input.AppKey,
            PackageType = input.PackageType,
            VersionName = input.VersionName,
            VersionCode = input.VersionCode,
            Status = CommonStatusEnum.Enable,
            FileUrl = input.FileUrl,
            MinNativeVersionCode = input.MinNativeVersionCode,
            MaxNativeVersionCode = input.MaxNativeVersionCode,
            UpdateContent = input.UpdateContent,
            IsForceUpdate = input.IsForceUpdate,
            Remark = input.Remark
        };

        await _repository.InsertAsync(applicationVersionModel);
    }

    /// <summary>
    /// 编辑应用版本
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑应用版本", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.AppVersion.Edit)]
    public async Task EditApplicationVersion(EditApplicationVersionInput input)
    {
        if (input.PackageType == ApplicationPackageTypeEnum.Wgt)
        {
            if (input.MinNativeVersionCode is not > 0
                || input.MaxNativeVersionCode is not > 0
                || input.MinNativeVersionCode > input.MaxNativeVersionCode)
            {
                throw new UserFriendlyException("WGT 必须提供有效的原生版本兼容区间！");
            }
        }

        if (input.PackageType == ApplicationPackageTypeEnum.FullPackage)
        {
            if (input.MinNativeVersionCode.HasValue || input.MaxNativeVersionCode.HasValue)
            {
                throw new UserFriendlyException("整包不能设置原生版本兼容区间！");
            }
        }

        if (await _repository.AnyAsync(a =>
                a.AppKey == input.AppKey
                && a.PackageType == input.PackageType
                && a.VersionCode == input.VersionCode
                && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("应用版本重复！");
        }

        // 同版本整包和 WGT 共用展示版本
        if (await _repository.AnyAsync(a =>
                a.AppKey == input.AppKey
                && a.VersionCode == input.VersionCode
                && a.VersionName != input.VersionName
                && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("同一版本号的整包与 WGT 必须使用相同版本名称！");
        }

        ApplicationVersionModel applicationVersionModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationVersionModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        applicationVersionModel.AppKey = input.AppKey;
        applicationVersionModel.PackageType = input.PackageType;
        applicationVersionModel.VersionName = input.VersionName;
        applicationVersionModel.VersionCode = input.VersionCode;
        applicationVersionModel.Status = input.Status;
        applicationVersionModel.FileUrl = input.FileUrl;
        applicationVersionModel.MinNativeVersionCode = input.MinNativeVersionCode;
        applicationVersionModel.MaxNativeVersionCode = input.MaxNativeVersionCode;
        applicationVersionModel.UpdateContent = input.UpdateContent;
        applicationVersionModel.IsForceUpdate = input.IsForceUpdate;
        applicationVersionModel.Remark = input.Remark;
        applicationVersionModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(applicationVersionModel);
    }
}
