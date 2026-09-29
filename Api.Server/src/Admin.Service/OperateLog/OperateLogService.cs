// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.OperateLog.Dto;
using Fast.AdminLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.OperateLog;

/// <summary>
/// 操作日志服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "operateLog")]
public class OperateLogService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<DepartmentModel> _adminRepository;
    private readonly ISqlSugarRepository<OperateLogModel> _repository;

    public OperateLogService(IUser user, ISqlSugarRepository<DepartmentModel> adminRepository,
        ISqlSugarRepository<OperateLogModel> repository)
    {
        _user = user;
        _adminRepository = adminRepository;
        _repository = repository;
    }

    /// <summary>
    /// 获取操作日志分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取操作日志分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.OperateLogPaged)]
    public async Task<PagedResult<OperateLogModel>> QueryOperateLogPaged(QueryOperateLogPagedInput input)
    {
        if (input.SearchTimeList is not {Count: > 1})
        {
            throw new UserFriendlyException("请选择具体的时间范围！");
        }

        ISugarQueryable<OperateLogModel> queryable = _repository.Entities
            .WhereIF(input.OperateType != null, wh => wh.OperateType == input.OperateType)
            .WhereIF(input.EmployeeId != null, wh => wh.CreatedUserId == input.EmployeeId)
            .WhereIF(input.BizId != null, wh => wh.BizId == input.BizId);
        List<long> customDepartmentIds = _user.DataScopeDepartmentIdList ?? [];

        // 仅本人数据
        if (_user.DataScopeType == DataScopeTypeEnum.Self)
        {
            queryable = queryable.Where(wh => wh.CreatedUserId == _user.EmployeeId
                                              || customDepartmentIds.Contains(wh.DepartmentId ?? 0));
        }
        // 本部门数据
        else if (_user.DataScopeType == DataScopeTypeEnum.Dept)
        {
            queryable = queryable.Where(wh => wh.DepartmentId == _user.DepartmentId
                                              || customDepartmentIds.Contains(wh.DepartmentId ?? 0));
        }
        // 本部门及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.DeptWithChild)
        {
            List<long> departmentIds = await _adminRepository.Queryable<DepartmentModel>()
                .Where(wh => wh.DepartmentId == _user.DepartmentId
                             || SqlFunc.JsonArrayAny(wh.ParentIds, _user.DepartmentId ?? 0)
                             || customDepartmentIds.Contains(wh.DepartmentId))
                .Select(sl => sl.DepartmentId)
                .ToListAsync();
            queryable = queryable.Where(wh => departmentIds.Contains(wh.DepartmentId ?? 0));
        }
        // 本机构及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.OrgWithChild)
        {
            List<long> departmentIds = await _adminRepository.Queryable<DepartmentModel>()
                .Where(wh => wh.OrgId
                             == SqlFunc.Subqueryable<EmployeeOrgModel>()
                                 // 主部门
                                 .Where(e => e.EmployeeId == _user.EmployeeId && e.IsPrimary)
                                 .Where(e => e.OrgId == wh.OrgId)
                                 .Select(e => e.OrgId)
                             || customDepartmentIds.Contains(wh.DepartmentId))
                .Select(sl => sl.DepartmentId)
                .ToListAsync();
            queryable = queryable.Where(wh => departmentIds.Contains(wh.DepartmentId ?? 0));
        }
        // 自定义部门数据
        else if (_user.DataScopeType == DataScopeTypeEnum.CustomDept)
        {
            queryable = queryable.Where(wh => customDepartmentIds.Contains(wh.DepartmentId ?? 0));
        }
        else if (_user.DataScopeType != DataScopeTypeEnum.All)
        {
            queryable = queryable.Where(_ => false);
        }

        return await queryable.SplitTable()
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
