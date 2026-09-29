// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Employee.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Fast.Admin.Service.Employee;

/// <summary>
/// 职员服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "employee")]
public partial class EmployeeService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<EmployeeModel> _repository;
    private readonly ISqlSugarClient _centerRepository;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public EmployeeService(IUser user, ISqlSugarRepository<EmployeeModel> repository, ISqlSugarClient centerRepository,
        IHubContext<ChatHub, IChatClient> hubContext)
    {
        _user = user;
        _repository = repository;
        _centerRepository = centerRepository;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 按当前用户的数据权限获取职员
    /// </summary>
    /// <returns>当前用户数据权限范围内的职员</returns>
    private async Task<EmployeeModel> GetEmployeeWithinDataScope(long employeeId)
    {
        bool hasAccess = await _repository.Entities
            .LeftJoin<EmployeeOrgModel>((t1, t2) => t1.EmployeeId == t2.EmployeeId && t2.IsPrimary)
            .SelectMergeTable((t1, t2) => new QueryEmployeeSelectorDto
            {
                EmployeeId = t1.EmployeeId, DepartmentId = t2.DepartmentId
            })
            .DataScope(e => e.DepartmentId, e => e.EmployeeId, allowPublicData: false)
            .AnyAsync(e => e.EmployeeId == employeeId);
        if (!hasAccess)
        {
            throw new UserFriendlyException("数据不存在或无权操作！");
        }

        EmployeeModel employeeModel = await _repository.SingleOrDefaultAsync(employeeId);
        if (employeeModel == null)
        {
            throw new UserFriendlyException("数据不存在或无权操作！");
        }

        return employeeModel;
    }

    /// <summary>
    /// 校验当前用户是否有权分配请求中的角色
    /// </summary>
    /// <param name="requestedRoleIds">请求分配的角色Id集合</param>
    /// <param name="existingRoleIds">职员当前角色Id集合，编辑时用于计算角色变更范围</param>
    private async Task ValidateRoleAssignment(List<long> requestedRoleIds, List<long> existingRoleIds = null)
    {
        if (_user.IsSuperAdmin || _user.IsAdmin)
        {
            return;
        }

        List<long> currentRoleIds = _user.RoleIdList ?? [];
        var roleList = await _repository.Queryable<RoleModel>()
            .Where(wh => currentRoleIds.Contains(wh.RoleId))
            .Select(sl => new {sl.AssignableRoleIds})
            .ToListAsync();
        var assignableRoleIds = roleList.Where(wh => wh.AssignableRoleIds?.Count > 0)
            .SelectMany(sl => sl.AssignableRoleIds)
            .Except(currentRoleIds)
            .Distinct()
            .ToList();
        IEnumerable<long> changedRoleIds = existingRoleIds == null
            ? requestedRoleIds
            : requestedRoleIds.Except(existingRoleIds).Concat(existingRoleIds.Except(requestedRoleIds));
        if (changedRoleIds.Except(assignableRoleIds).Any())
        {
            throw new UserFriendlyException("无权分配超出自身权限范围的角色！");
        }
    }
}
