// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Role.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.Role;

public partial class RoleService
{
    /// <summary>
    /// 角色选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("角色选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> RoleSelector()
    {
        ISugarQueryable<RoleModel> queryable = _repository.Entities;
        if (!_user.IsSuperAdmin && !_user.IsAdmin)
        {
            List<long> roleIds = _user.RoleIdList ?? [];
            var roleList = await _repository.Queryable<RoleModel>()
                .Where(wh => roleIds.Contains(wh.RoleId))
                .Select(sl => new {sl.AssignableRoleIds})
                .ToListAsync();
            var assignableRoleIds = roleList.Where(wh => wh.AssignableRoleIds?.Count > 0)
                .SelectMany(sl => sl.AssignableRoleIds)
                .Except(roleIds)
                .Distinct()
                .ToList();

            if (assignableRoleIds.Count == 0)
            {
                return [];
            }

            queryable = queryable.Where(wh => assignableRoleIds.Contains(wh.RoleId));
        }

        var data = await queryable.OrderBy(ob => ob.Sort).Select(sl => new {sl.RoleId, sl.RoleName, sl.RoleCode}).ToListAsync();

        return data.Select(sl => new ElSelectorOutput<long> {Value = sl.RoleId, Label = sl.RoleName, Data = new {sl.RoleCode}})
            .ToList();
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取角色分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Role.Paged)]
    public async Task<PagedResult<QueryRolePagedOutput>> QueryRolePaged(QueryRolePagedInput input)
    {
        return await _repository.Entities.WhereIF(input.RoleType != null, wh => wh.RoleType == input.RoleType)
            .WhereIF(input.DataScopeType != null, wh => wh.DataScopeType == input.DataScopeType)
            .OrderByIF(input.IsOrderBy, ob => ob.Sort)
            .Select(sl => new QueryRolePagedOutput
            {
                RoleId = sl.RoleId,
                RoleType = sl.RoleType,
                IsSystemMenu = sl.IsSystemMenu,
                RoleName = sl.RoleName,
                RoleCode = sl.RoleCode,
                Sort = sl.Sort,
                DataScopeType = sl.DataScopeType,
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
    /// 获取角色详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取角色详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Role.Detail)]
    public async Task<QueryRoleDetailOutput> QueryRoleDetail([Required(ErrorMessage = "角色Id不能为空")] long? roleId)
    {
        QueryRoleDetailOutput result = await _repository.Entities.Where(wh => wh.RoleId == roleId)
            .Select(sl => new QueryRoleDetailOutput
            {
                RoleId = sl.RoleId,
                RoleType = sl.RoleType,
                IsSystemMenu = sl.IsSystemMenu,
                RoleName = sl.RoleName,
                RoleCode = sl.RoleCode,
                Sort = sl.Sort,
                DataScopeType = sl.DataScopeType,
                DataScopeDepartmentIds = sl.DataScopeDepartmentIds,
                AssignableRoleIds = sl.AssignableRoleIds,
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
}
