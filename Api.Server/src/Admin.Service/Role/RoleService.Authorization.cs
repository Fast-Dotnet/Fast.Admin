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

namespace Fast.Admin.Service.Role;

public partial class RoleService
{
    /// <summary>
    /// 角色授权
    /// </summary>
    [HttpPost]
    [ApiInfo("角色授权", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Role.Edit)]
    public async Task RoleAuth(RoleAuthInput input)
    {
        List<long> currentRoleIds = _user.RoleIdList ?? [];
        List<long> assignableRoleIds = null;
        if (!_user.IsSuperAdmin && !_user.IsAdmin)
        {
            var currentRoleList = await _repository.Queryable<RoleModel>()
                .Where(wh => currentRoleIds.Contains(wh.RoleId))
                .Select(sl => new {sl.AssignableRoleIds})
                .ToListAsync();
            assignableRoleIds = currentRoleList.Where(wh => wh.AssignableRoleIds?.Count > 0)
                .SelectMany(sl => sl.AssignableRoleIds)
                .Except(currentRoleIds)
                .Distinct()
                .ToList();
        }

        RoleModel roleModel = await _repository.Entities.Where(wh => wh.RoleId == input.RoleId)
            .WhereIF(assignableRoleIds != null, wh => assignableRoleIds.Contains(wh.RoleId))
            .SingleAsync();
        if (roleModel == null)
        {
            throw new UserFriendlyException("角色不存在或无权操作！");
        }

        var menuIds = (input.MenuIds ?? []).Distinct().ToList();
        var buttonIds = (input.ButtonIds ?? []).Distinct().ToList();
        (ApplicationOpenIdModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();

        var menuList = await _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition)
            .Where(wh => wh.MenuType != MenuTypeEnum.Catalog)
            .Where(wh => menuIds.Contains(wh.MenuId))
            .Select(sl => new {sl.MenuId})
            .ToListAsync();
        if (menuList.Count != menuIds.Count)
        {
            throw new UserFriendlyException("授权菜单不属于当前应用、已禁用或超出租户版本！");
        }

        var buttonList = await _centerRepository.Queryable<ButtonModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition)
            .Where(wh => buttonIds.Contains(wh.ButtonId))
            .Select(sl => new {sl.ButtonId, sl.MenuId})
            .ToListAsync();
        if (buttonList.Count != buttonIds.Count)
        {
            throw new UserFriendlyException("授权按钮不属于当前应用、已禁用或超出租户版本！");
        }

        if (buttonList.Any(button => !menuIds.Contains(button.MenuId)))
        {
            throw new UserFriendlyException("授权按钮必须属于已选择的菜单！");
        }

        if (!_user.IsSuperAdmin && !_user.IsAdmin)
        {
            List<long> authorizedMenuIds = await _repository.Queryable<RoleMenuModel>()
                .Where(wh => currentRoleIds.Contains(wh.RoleId))
                .Select(sl => sl.MenuId)
                .Distinct()
                .ToListAsync();
            List<long> authorizedButtonIds = await _repository.Queryable<RoleButtonModel>()
                .Where(wh => currentRoleIds.Contains(wh.RoleId))
                .Select(sl => sl.ButtonId)
                .Distinct()
                .ToListAsync();
            if (menuIds.Except(authorizedMenuIds).Any() || buttonIds.Except(authorizedButtonIds).Any())
            {
                throw new UserFriendlyException("无权授予超出自身权限范围的菜单或按钮！");
            }
        }

        List<long> applicationMenuIds = await _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Select(sl => sl.MenuId)
            .ToListAsync();
        List<long> applicationButtonIds = await _centerRepository.Queryable<ButtonModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Select(sl => sl.ButtonId)
            .ToListAsync();

        roleModel.RowVersion = input.RowVersion;

        await _repository.Ado.UseTranAsync(async () =>
        {
            // 使用角色版本锁定本次授权，避免并发授权互相覆盖
            await _repository.UpdateAsync(roleModel);

            // 只替换当前应用权限，保留角色在其他应用的授权
            if (applicationMenuIds.Count > 0)
            {
                await _repository.Deleteable<RoleMenuModel>()
                    .Where(wh => wh.RoleId == roleModel.RoleId && applicationMenuIds.Contains(wh.MenuId))
                    .ExecuteCommandAsync();
            }

            // 添加新的菜单权限
            if (menuIds.Any())
            {
                await _repository
                    .Insertable(menuIds.Select(menuId => new RoleMenuModel {RoleId = roleModel.RoleId, MenuId = menuId}).ToList())
                    .ExecuteCommandAsync();
            }

            if (applicationButtonIds.Count > 0)
            {
                await _repository.Deleteable<RoleButtonModel>()
                    .Where(wh => wh.RoleId == roleModel.RoleId && applicationButtonIds.Contains(wh.ButtonId))
                    .ExecuteCommandAsync();
            }

            // 添加新的按钮权限
            if (buttonIds.Any())
            {
                await _repository
                    .Insertable(buttonIds.Select(buttonId => new RoleButtonModel {RoleId = roleModel.RoleId, ButtonId = buttonId})
                        .ToList())
                    .ExecuteCommandAsync();
            }
        }, ex => throw ex);

        await RevokeRoleEmployees(roleModel.RoleId);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "角色授权",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = roleModel.RoleId,
            BizNo = null,
            Description = $"角色授权：{roleModel.RoleName}"
        });
    }

    /// <summary>
    /// 获取角色授权菜单
    /// </summary>
    [HttpPost]
    [ApiInfo("获取角色授权菜单", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Role.Edit)]
    public async Task<RoleAuthInput> QueryRoleAuthMenu(RoleIdInput input)
    {
        RoleModel roleModel = await _repository.SingleOrDefaultAsync(input.RoleId);
        if (roleModel == null)
        {
            throw new UserFriendlyException("角色不存在或无权操作！");
        }

        (ApplicationOpenIdModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();
        List<long> validMenuIds = await _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition)
            .Where(wh => wh.MenuType != MenuTypeEnum.Catalog)
            .Select(sl => sl.MenuId)
            .ToListAsync();
        List<long> validButtonIds = await _centerRepository.Queryable<ButtonModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition)
            .Where(wh => validMenuIds.Contains(wh.MenuId))
            .Select(sl => sl.ButtonId)
            .ToListAsync();

        var result = new RoleAuthInput
        {
            RoleId = roleModel.RoleId,
            RoleName = roleModel.RoleName,
            RowVersion = roleModel.RowVersion,
            MenuIds = await _repository
                .Queryable<RoleMenuModel>()
                .Where(wh => wh.RoleId == roleModel.RoleId && validMenuIds.Contains(wh.MenuId))
                .Select(sl => sl.MenuId)
                .ToListAsync(),
            ButtonIds = await _repository.Queryable<RoleButtonModel>()
                .Where(wh => wh.RoleId == roleModel.RoleId && validButtonIds.Contains(wh.ButtonId))
                .Select(sl => sl.ButtonId)
                .ToListAsync()
        };

        return result;
    }

    /// <summary>
    /// 获取授权菜单
    /// </summary>
    [HttpGet]
    [ApiInfo("获取授权菜单", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Role.Edit, PermissionConst.Employee.Edit)]
    public async Task<List<ElSelectorOutput<long>>> QueryAuthMenu()
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        // 查询租户信息
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);

        // 查询当前用户角色。RoleType 只作为初始化模板，运行时授权统一读取关联表
        List<long> roleIds = _user.RoleIdList ?? [];

        ISugarQueryable<MenuModel> menuQueryable = _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition)
            .Where(wh => wh.MenuType != MenuTypeEnum.Catalog);

        ISugarQueryable<ButtonModel> buttonQueryable = _centerRepository.Queryable<ButtonModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition);

        if (!_user.IsSuperAdmin && !_user.IsAdmin)
        {
            // 查询当前用户角色对应的菜单Id
            List<long> roleMenuIds = await _repository.Queryable<RoleMenuModel>()
                .Where(wh => roleIds.Contains(wh.RoleId))
                .Select(sl => sl.MenuId)
                .ToListAsync();
            menuQueryable = menuQueryable.WhereIF(roleMenuIds.Count > 0, wh => roleMenuIds.Contains(wh.MenuId));

            // 查询当前用户角色对应的按钮Id
            List<long> roleButtonIds = await _repository.Queryable<RoleButtonModel>()
                .Where(wh => roleIds.Contains(wh.RoleId))
                .Select(sl => sl.ButtonId)
                .ToListAsync();
            buttonQueryable = buttonQueryable.WhereIF(roleButtonIds.Count > 0, wh => roleButtonIds.Contains(wh.ButtonId));
        }

        // 查询所有菜单
        var menuList = await menuQueryable.Clone()
            .OrderBy(ob => ob.Sort)
            .Select(sl => new
            {
                sl.MenuId,
                sl.MenuName,
                sl.HasMobile,
                sl.HasWeb,
                sl.HasDesktop
            })
            .ToListAsync();

        // 查询所有按钮
        var buttonList = await buttonQueryable.Clone()
            .InnerJoin(menuQueryable.Clone(), (t1, t2) => t1.MenuId == t2.MenuId)
            .OrderBy(t1 => t1.Sort)
            .Select(t1 => new
            {
                t1.ButtonId,
                t1.MenuId,
                t1.ButtonName,
                t1.HasMobile,
                t1.HasWeb,
                t1.HasDesktop
            })
            .ToListAsync();

        var result = new List<ElSelectorOutput<long>>();

        foreach (var menuInfo in menuList.ToList())
        {
            var item = new ElSelectorOutput<long>
            {
                Value = menuInfo.MenuId,
                Label = menuInfo.MenuName,
                Data = new {menuInfo.HasMobile, menuInfo.HasWeb, menuInfo.HasDesktop},
                Children = []
            };
            foreach (var buttonInfo in buttonList.Where(wh => wh.MenuId == menuInfo.MenuId).ToList())
            {
                item.Children.Add(new ElSelectorOutput<long>
                {
                    Value = buttonInfo.ButtonId,
                    Label = buttonInfo.ButtonName,
                    Data = new {buttonInfo.HasMobile, buttonInfo.HasWeb, buttonInfo.HasDesktop}
                });
            }

            result.Add(item);
        }

        return result;
    }
}
