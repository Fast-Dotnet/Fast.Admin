// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Center.Domain;

namespace Fast.Admin.Service.Role;

/// <summary>
/// 角色服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "role")]
public partial class RoleService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<RoleModel> _repository;
    private readonly ISqlSugarClient _centerRepository;

    public RoleService(IUser user, ISqlSugarRepository<RoleModel> repository, ISqlSugarClient centerRepository)
    {
        _user = user;
        _repository = repository;
        _centerRepository = centerRepository;
    }

    /// <summary>
    /// 规范并校验可分配角色Id集合
    /// </summary>
    /// <param name="roleId">当前角色Id</param>
    /// <param name="roleIds">可分配角色Id集合</param>
    /// <returns>去重并验证后的可分配角色Id集合</returns>
    private async Task<List<long>> NormalizeAssignableRoleIds(long roleId, List<long> roleIds)
    {
        var result = (roleIds ?? []).Distinct().ToList();
        if (result.Contains(roleId))
        {
            throw new UserFriendlyException("可分配角色不能包含角色自身！");
        }

        if (result.Count > 0
            && await _repository.Queryable<RoleModel>().Where(wh => result.Contains(wh.RoleId)).CountAsync() != result.Count)
        {
            throw new UserFriendlyException("可分配角色数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 规范并校验自定义数据范围部门Id集合
    /// </summary>
    /// <returns>去重并验证后的自定义数据范围部门Id集合</returns>
    private async Task<List<long>> NormalizeDataScopeDepartmentIds(DataScopeTypeEnum dataScopeType, List<long> departmentIds)
    {
        if (dataScopeType != DataScopeTypeEnum.CustomDept)
        {
            return [];
        }

        var result = (departmentIds ?? []).Distinct().ToList();
        if (result.Count > 0
            && await _repository.Queryable<DepartmentModel>().Where(wh => result.Contains(wh.DepartmentId)).CountAsync()
            != result.Count)
        {
            throw new UserFriendlyException("自定义数据范围部门不存在！");
        }

        return result;
    }

    /// <summary>
    /// 获取角色授权所需的当前应用与租户上下文
    /// </summary>
    /// <returns>角色授权所需的当前应用和租户</returns>
    private async Task<(ApplicationOpenIdModel Application, TenantModel Tenant)> GetAuthorizationContext()
    {
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);
        return (applicationModel, tenantModel);
    }

    /// <summary>
    /// 清除所有关联指定角色职员的授权缓存
    /// </summary>
    private async Task RevokeRoleEmployees(long roleId)
    {
        List<long> employeeIds = await _repository.Queryable<EmployeeRoleModel>()
            .Where(wh => wh.RoleId == roleId)
            .Select(sl => sl.EmployeeId)
            .Distinct()
            .ToListAsync();
        if (employeeIds.Count == 0)
        {
            return;
        }

        List<string> employeeNos = await _repository.Queryable<EmployeeModel>()
            .Where(wh => employeeIds.Contains(wh.EmployeeId))
            .Select(sl => sl.EmployeeNo)
            .ToListAsync();
        foreach (string employeeNo in employeeNos.Where(wh => !string.IsNullOrWhiteSpace(wh)))
        {
            await _user.RevokeEmployee(_user.TenantNo, employeeNo);
        }
    }
}
