// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Application;

/// <summary>
/// 应用服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "application")]
public class ApplicationService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<ApplicationModel> _repository;

    public ApplicationService(IUser user, ISqlSugarRepository<ApplicationModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 应用选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("应用选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> ApplicationSelector()
    {
        ISugarQueryable<ApplicationModel> queryable = _repository.Entities;
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);
        if (!_user.IsSuperAdmin && tenantModel.TenantType != TenantTypeEnum.System)
        {
            queryable = queryable.Where(wh => wh.TenantId == _user.TenantId);
        }

        var data = await queryable.OrderBy(ob => ob.AppName)
            .Select(sl => new
            {
                sl.AppId,
                sl.Edition,
                sl.AppName,
                sl.AppNo,
                sl.LogoUrl
            })
            .ToListAsync();

        return data.Select(sl =>
                new ElSelectorOutput<long> {Value = sl.AppId, Label = sl.AppName, Data = new {sl.AppNo, sl.Edition, sl.LogoUrl}})
            .ToList();
    }

    /// <summary>
    /// 获取应用分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取应用分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.App.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<QueryApplicationPagedOutput>> QueryApplicationPaged(QueryApplicationPagedInput input)
    {
        ISugarQueryable<ApplicationModel> queryable = _repository.Entities;
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);
        if (!_user.IsSuperAdmin && tenantModel.TenantType != TenantTypeEnum.System)
        {
            queryable = queryable.Where(wh => wh.TenantId == _user.TenantId);
        }

        return await queryable.WhereIF(input.Edition != null, wh => wh.Edition == input.Edition)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryApplicationPagedOutput
            {
                AppId = sl.AppId,
                Edition = sl.Edition,
                AppNo = sl.AppNo,
                AppName = sl.AppName,
                LogoUrl = sl.LogoUrl,
                ThemeColor = sl.ThemeColor,
                Remark = sl.Remark,
                TenantName = sl.TenantName,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取应用详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取应用详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.App.Detail)]
    [PlatformOnly]
    public async Task<QueryApplicationDetailOutput> QueryApplicationDetail([Required(ErrorMessage = "应用Id不能为空")] long? appId)
    {
        QueryApplicationDetailOutput result = await _repository.Entities.Where(wh => wh.AppId == appId)
            .Select(sl => new QueryApplicationDetailOutput
            {
                AppId = sl.AppId,
                Edition = sl.Edition,
                AppNo = sl.AppNo,
                AppName = sl.AppName,
                LogoUrl = sl.LogoUrl,
                ThemeColor = sl.ThemeColor,
                Remark = sl.Remark,
                TenantId = sl.TenantId,
                TenantName = sl.TenantName,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加应用
    /// </summary>
    [HttpPost]
    [ApiInfo("添加应用", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.App.Add)]
    [PlatformOnly]
    public async Task AddApplication(AddApplicationInput input)
    {
        if (await _repository.AnyAsync(a => a.AppName == input.AppName))
        {
            throw new UserFriendlyException("应用名称重复！");
        }

        var applicationModel = new ApplicationModel
        {
            Edition = input.Edition,
            AppName = input.AppName,
            LogoUrl = input.LogoUrl,
            ThemeColor = input.ThemeColor?.ToUpper(),
            Remark = input.Remark,
            TenantId = input.TenantId,
            TenantName = input.TenantName
        };

        await _repository.Ado.UseTranAsync(async () =>
        {
            applicationModel.AppNo = SysSerialContext.GenAppNo(_repository);

            await _repository.InsertAsync(applicationModel);
        }, ex => throw ex);
    }

    /// <summary>
    /// 编辑应用
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑应用", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.App.Edit)]
    [PlatformOnly]
    public async Task EditApplication(EditApplicationInput input)
    {
        if (await _repository.AnyAsync(a => a.AppName == input.AppName && a.AppId != input.AppId))
        {
            throw new UserFriendlyException("应用名称重复！");
        }

        ApplicationModel applicationModel = await _repository.SingleOrDefaultAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        applicationModel.Edition = input.Edition;
        applicationModel.AppName = input.AppName;
        applicationModel.LogoUrl = input.LogoUrl;
        applicationModel.ThemeColor = input.ThemeColor?.ToUpper();
        applicationModel.Remark = input.Remark;
        applicationModel.TenantId = input.TenantId;
        applicationModel.TenantName = input.TenantName;
        applicationModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(applicationModel);

        foreach (string openId in await _repository.Queryable<ApplicationOpenIdModel>().Select(sl => sl.OpenId).ToListAsync())
        {
            // 删除缓存
            await ApplicationContext.DeleteApplication(openId);
        }
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    [HttpPost]
    [ApiInfo("删除应用", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.App.Delete)]
    [PlatformOnly]
    public async Task DeleteApplication(AppIdInput input)
    {
        ApplicationModel applicationModel = await _repository.SingleOrDefaultAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (await _repository.Queryable<ApplicationOpenIdModel>().AnyAsync(a => a.AppId == input.AppId))
        {
            throw new UserFriendlyException("应用存在OpenId信息，无法删除！");
        }

        await _repository.DeleteAsync(applicationModel);
    }
}
