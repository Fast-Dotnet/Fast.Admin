// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Role.Dto;
using Fast.AdminLog.Domain;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace Fast.Admin.Service.Role;

public partial class RoleService
{
    /// <summary>
    /// 添加角色
    /// </summary>
    [HttpPost]
    [ApiInfo("添加角色", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Role.Add)]
    public async Task AddRole(AddRoleInput input)
    {
        if (await _repository.AnyAsync(a => a.RoleName == input.RoleName))
        {
            throw new UserFriendlyException("角色名称重复！");
        }

        if (await _repository.AnyAsync(a => a.RoleCode == input.RoleCode))
        {
            throw new UserFriendlyException("角色编码重复！");
        }

        long roleId = YitIdHelper.NextId();
        var roleModel = new RoleModel
        {
            RoleId = roleId,
            RoleType = input.RoleType,
            IsSystemMenu = input.IsSystemMenu,
            RoleName = input.RoleName,
            RoleCode = input.RoleCode,
            Sort = input.Sort,
            DataScopeType = input.DataScopeType,
            DataScopeDepartmentIds = await NormalizeDataScopeDepartmentIds(input.DataScopeType, input.DataScopeDepartmentIds),
            AssignableRoleIds = await NormalizeAssignableRoleIds(roleId, input.AssignableRoleIds),
            Remark = input.Remark
        };

        var templateMenuIds = new List<long>();
        var templateButtonIds = new List<long>();
        if (roleModel.IsSystemMenu)
        {
            (ApplicationOpenIdModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();
            templateMenuIds = await _centerRepository.Queryable<MenuModel>()
                .Where(wh => wh.AppId == applicationModel.AppId)
                .Where(wh => wh.Status == CommonStatusEnum.Enable)
                .Where(wh => tenantModel.Edition >= wh.Edition)
                .Where(wh => wh.MenuType != MenuTypeEnum.Catalog)
                .Where(wh => (wh.RoleType & roleModel.RoleType) != 0)
                .Select(sl => sl.MenuId)
                .ToListAsync();
            templateButtonIds = await _centerRepository.Queryable<ButtonModel>()
                .Where(wh => wh.AppId == applicationModel.AppId)
                .Where(wh => wh.Status == CommonStatusEnum.Enable)
                .Where(wh => tenantModel.Edition >= wh.Edition)
                .Where(wh => templateMenuIds.Contains(wh.MenuId))
                .Where(wh => (wh.RoleType & roleModel.RoleType) != 0)
                .Select(sl => sl.ButtonId)
                .ToListAsync();
        }

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.InsertAsync(roleModel);
            if (templateMenuIds.Count > 0)
            {
                await _repository
                    .Insertable(templateMenuIds.Select(menuId => new RoleMenuModel {RoleId = roleModel.RoleId, MenuId = menuId})
                        .ToList())
                    .ExecuteCommandAsync();
            }

            if (templateButtonIds.Count > 0)
            {
                await _repository
                    .Insertable(templateButtonIds
                        .Select(buttonId => new RoleButtonModel {RoleId = roleModel.RoleId, ButtonId = buttonId})
                        .ToList())
                    .ExecuteCommandAsync();
            }
        }, ex => throw ex);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加角色",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = roleModel.RoleId,
            BizNo = null,
            Description = $"添加角色：{roleModel.RoleName}"
        });
    }

    /// <summary>
    /// 编辑角色
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑角色", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Role.Edit)]
    public async Task EditRole(EditRoleInput input)
    {
        RoleModel roleModel = await _repository.SingleOrDefaultAsync(input.RoleId);
        if (roleModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (await _repository.AnyAsync(a => a.RoleName == input.RoleName && a.RoleId != input.RoleId))
        {
            throw new UserFriendlyException("角色名称重复！");
        }

        if (await _repository.AnyAsync(a => a.RoleCode == input.RoleCode && a.RoleId != input.RoleId))
        {
            throw new UserFriendlyException("角色编码重复！");
        }

        roleModel.RoleType = input.RoleType;
        roleModel.IsSystemMenu = input.IsSystemMenu;
        roleModel.RoleName = input.RoleName;
        roleModel.RoleCode = input.RoleCode;
        roleModel.Sort = input.Sort;
        roleModel.DataScopeType = input.DataScopeType;
        roleModel.DataScopeDepartmentIds = await NormalizeDataScopeDepartmentIds(input.DataScopeType,
            input.DataScopeDepartmentIds);
        roleModel.AssignableRoleIds = await NormalizeAssignableRoleIds(roleModel.RoleId, input.AssignableRoleIds);
        roleModel.Remark = input.Remark;
        roleModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(roleModel);

        await _repository.Updateable<EmployeeRoleModel>()
            .SetColumns(_ => new EmployeeRoleModel {RoleName = roleModel.RoleName})
            .Where(wh => wh.RoleId == roleModel.RoleId)
            .ExecuteCommandAsync();

        await RevokeRoleEmployees(roleModel.RoleId);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑角色",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = roleModel.RoleId,
            BizNo = null,
            Description = $"编辑角色：{roleModel.RoleName}"
        });
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpPost]
    [ApiInfo("删除角色", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Role.Delete)]
    public async Task DeleteRole(RoleIdInput input)
    {
        RoleModel roleModel = await _repository.SingleOrDefaultAsync(input.RoleId);
        if (roleModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 检查是否有职员关联
        if (await _repository.Queryable<EmployeeRoleModel>().AnyAsync(a => a.RoleId == input.RoleId))
        {
            throw new UserFriendlyException("角色存在职员关联，无法删除！");
        }

        await _repository.Ado.UseTranAsync(async () =>
        {
            // 删除角色菜单关联
            await _repository.Deleteable<RoleMenuModel>().Where(wh => wh.RoleId == input.RoleId).ExecuteCommandAsync();

            // 删除角色按钮关联
            await _repository.Deleteable<RoleButtonModel>().Where(wh => wh.RoleId == input.RoleId).ExecuteCommandAsync();

            // 删除角色
            await _repository.DeleteAsync(roleModel);
        }, ex => throw ex);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "删除角色",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = roleModel.RoleId,
            BizNo = null,
            Description = $"删除角色：{roleModel.RoleName}"
        });
    }
}
