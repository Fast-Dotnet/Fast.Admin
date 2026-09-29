// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.SysSerial.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.SysSerial;

/// <summary>
/// 系统序号规则服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "sysSerial")]
[PlatformOnly]
public class SysSerialService : IDynamicApplication
{
    private readonly ISqlSugarRepository<SysSerialRuleModel> _repository;

    public SysSerialService(ISqlSugarRepository<SysSerialRuleModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取系统序号规则分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取系统序号规则分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.SysSerial.Paged)]
    public async Task<PagedResult<QuerySysSerialRulePagedOutput>> QuerySysSerialRulePaged(PagedInput input)
    {
        return await _repository.Entities.LeftJoin<SysSerialSettingModel>((t1, t2) => t1.RuleType == t2.RuleType)
            .SelectMergeTable((t1, t2) => new QuerySysSerialRulePagedOutput
            {
                SerialRuleId = t1.SerialRuleId,
                RuleType = t1.RuleType,
                Prefix = t1.Prefix,
                DateType = t1.DateType,
                Spacer = t1.Spacer,
                Length = t1.Length,
                LastSerial = t2.LastSerial,
                LastSerialNo = t2.LastSerialNo,
                LastTime = t2.LastTime,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .OrderByIF(input.IsOrderBy, ob => ob.RuleType, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取系统序号规则详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取系统序号规则详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.SysSerial.Detail)]
    public async Task<QuerySysSerialRuleDetailOutput> QuerySysSerialRuleDetail(
        [Required(ErrorMessage = "系统序号规则Id不能为空")] long? serialRuleId)
    {
        QuerySysSerialRuleDetailOutput result = await _repository.Entities.Where(wh => wh.SerialRuleId == serialRuleId)
            .Select(sl => new QuerySysSerialRuleDetailOutput
            {
                SerialRuleId = sl.SerialRuleId,
                RuleType = sl.RuleType,
                Prefix = sl.Prefix,
                DateType = sl.DateType,
                Spacer = sl.Spacer,
                Length = sl.Length,
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
    /// 添加系统序号规则
    /// </summary>
    [HttpPost]
    [ApiInfo("添加系统序号规则", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.SysSerial.Add)]
    public async Task AddSysSerialRule(AddSysSerialRuleInput input)
    {
        if (await _repository.AnyAsync(a => a.RuleType == input.RuleType))
        {
            throw new UserFriendlyException("系统序号规则类型重复！");
        }

        var sysSerialRuleModel = new SysSerialRuleModel
        {
            RuleType = input.RuleType,
            Prefix = input.Prefix,
            DateType = input.DateType,
            Spacer = input.Spacer,
            Length = input.Length
        };

        await _repository.InsertAsync(sysSerialRuleModel);
    }

    /// <summary>
    /// 编辑系统序号规则
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑系统序号规则", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.SysSerial.Edit)]
    public async Task EditSysSerialRule(EditSysSerialRuleInput input)
    {
        SysSerialRuleModel merchantModel = await _repository.SingleOrDefaultAsync(s => s.SerialRuleId == input.SerialRuleId);
        if (merchantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        merchantModel.Prefix = input.Prefix;
        merchantModel.DateType = input.DateType;
        merchantModel.Spacer = input.Spacer;
        merchantModel.Length = input.Length;
        merchantModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(merchantModel);
    }
}
