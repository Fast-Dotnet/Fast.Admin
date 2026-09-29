// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Config.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Config;

/// <summary>
/// 配置服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "config")]
[PlatformOnly]
public class ConfigService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<ConfigModel> _repository;

    public ConfigService(IUser user, ISqlSugarRepository<ConfigModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 获取配置分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取配置分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Config.Paged)]
    public async Task<PagedResult<QueryConfigPagedOutput>> QueryConfigPaged(PagedInput input)
    {
        return await _repository.Entities.Select(sl => new QueryConfigPagedOutput
            {
                ConfigId = sl.ConfigId,
                ConfigCode = sl.ConfigCode,
                ConfigName = sl.ConfigName,
                ConfigValue = sl.ConfigValue,
                Remark = sl.Remark,
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
    /// 获取配置详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取配置详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Config.Detail)]
    public async Task<QueryConfigDetailOutput> QueryConfigDetail([Required(ErrorMessage = "配置Id不能为空")] long? configId)
    {
        QueryConfigDetailOutput result = await _repository.Entities.Where(wh => wh.ConfigId == configId)
            .Select(sl => new QueryConfigDetailOutput
            {
                ConfigId = sl.ConfigId,
                ConfigCode = sl.ConfigCode,
                ConfigName = sl.ConfigName,
                ConfigValue = sl.ConfigValue,
                Remark = sl.Remark,
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
    /// 添加配置
    /// </summary>
    [HttpPost]
    [ApiInfo("添加配置", HttpRequestActionEnum.Add)]
    public async Task AddConfig(AddConfigInput input)
    {
        if (_user?.IsSuperAdmin == false)
        {
            throw new UserFriendlyException("非超级管理员禁止操作！");
        }

        if (await _repository.AnyAsync(a => a.ConfigCode == input.ConfigCode))
        {
            throw new UserFriendlyException("配置编码重复！");
        }

        var configModel = new ConfigModel
        {
            ConfigCode = input.ConfigCode,
            ConfigName = input.ConfigName,
            ConfigValue = input.ConfigValue,
            Remark = input.Remark
        };

        await _repository.InsertAsync(configModel);
    }

    /// <summary>
    /// 编辑配置
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑配置", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Config.Edit)]
    public async Task EditConfig(EditConfigInput input)
    {
        ConfigModel configModel = await _repository.SingleOrDefaultAsync(input.ConfigId);
        if (configModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        configModel.ConfigName = input.ConfigName;
        configModel.ConfigValue = input.ConfigValue;
        configModel.Remark = input.Remark;
        configModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(configModel);
        // 删除缓存
        await ConfigContext.DeleteConfig(configModel.ConfigCode);
    }

    /// <summary>
    /// 删除配置缓存
    /// </summary>
    [HttpPost]
    [ApiInfo("删除配置缓存", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Config.Edit)]
    public async Task DeleteConfigCache(DeleteConfigCacheInput input)
    {
        // 删除缓存
        await ConfigContext.DeleteConfig(input.ConfigCode);
    }

    /// <summary>
    /// 删除所有配置缓存
    /// </summary>
    [HttpPost]
    [ApiInfo("删除所有配置缓存", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Config.Edit)]
    public async Task DeleteAllConfigCache()
    {
        // 删除缓存
        await ConfigContext.DeleteAllConfig();
    }
}
