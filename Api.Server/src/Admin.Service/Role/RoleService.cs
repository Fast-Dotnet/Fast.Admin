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

/// <summary>
/// 角色服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "role")]
public class RoleService : IDynamicApplication
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
    private async Task<(ApplicationClientModel Application, TenantModel Tenant)> GetAuthorizationContext()
    {
        ApplicationClientModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
        if (!applicationModel.AppType.HasFlag(GlobalContext.DeviceType))
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

    /// <summary>
    /// 角色选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("角色选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> RoleSelector()
    {
        ISugarQueryable<RoleModel> queryable = _repository.Queryable<RoleModel>();
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
        return await _repository.Queryable<RoleModel>()
            .WhereIF(input.RoleType != null, wh => wh.RoleType == input.RoleType)
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
        QueryRoleDetailOutput result = await _repository.Queryable<RoleModel>()
            .Where(wh => wh.RoleId == roleId)
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

        if (result.DataScopeDepartmentIds is {Count: > 0})
        {
            result.DataScopeDepartmentNames = await _repository.Queryable<DepartmentModel>()
                .Where(wh => result.DataScopeDepartmentIds.Contains(wh.DepartmentId))
                .Select(sl => sl.DepartmentName)
                .ToListAsync();
        }

        return result;
    }

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
            (ApplicationClientModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();
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

        RoleModel roleModel = await _repository.Queryable<RoleModel>()
            .Where(wh => wh.RoleId == input.RoleId)
            .WhereIF(assignableRoleIds != null, wh => assignableRoleIds.Contains(wh.RoleId))
            .SingleAsync();
        if (roleModel == null)
        {
            throw new UserFriendlyException("角色不存在或无权操作！");
        }

        var menuIds = (input.MenuIds ?? []).Distinct().ToList();
        var buttonIds = (input.ButtonIds ?? []).Distinct().ToList();
        (ApplicationClientModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();

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

        (ApplicationClientModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();
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
        (ApplicationClientModel applicationModel, TenantModel tenantModel) = await GetAuthorizationContext();

        // 查询当前用户角色。RoleType 只作为初始化模板，运行时授权统一读取关联表
        List<long> roleIds = _user.RoleIdList ?? [];

        ISugarQueryable<MenuModel> availableMenus = _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.Status == CommonStatusEnum.Enable)
            .Where(wh => tenantModel.Edition >= wh.Edition);

        ISugarQueryable<MenuModel> menuQueryable = availableMenus.Clone().Where(wh => wh.MenuType != MenuTypeEnum.Catalog);

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
                sl.ParentId,
                sl.ParentIds,
                sl.Sort,
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

        var catalogList = await availableMenus.Clone()
            .Where(wh => wh.MenuType == MenuTypeEnum.Catalog && wh.ParentId == 0)
            .Select(sl => new {sl.MenuId, sl.MenuName, sl.Sort})
            .ToListAsync();
        var catalogs = catalogList.ToDictionary(catalog => catalog.MenuId);
        var buttonsByMenu = buttonList.ToLookup(button => button.MenuId);
        var modules = new Dictionary<long, ElSelectorOutput<long>>();
        var result = new List<(int Sort, ElSelectorOutput<long> Node)>();

        foreach (var menuInfo in menuList)
        {
            var item = new ElSelectorOutput<long>
            {
                Value = menuInfo.MenuId,
                Label = menuInfo.MenuName,
                Data = new {menuInfo.HasMobile, menuInfo.HasWeb, menuInfo.HasDesktop},
                Children = buttonsByMenu[menuInfo.MenuId]
                    .Select(button => new ElSelectorOutput<long>
                    {
                        Value = button.ButtonId,
                        Label = button.ButtonName,
                        Data = new {button.HasMobile, button.HasWeb, button.HasDesktop}
                    })
                    .ToList()
            };

            long moduleId = menuInfo.ParentIds?.FirstOrDefault(catalogs.ContainsKey) ?? 0;
            if (menuInfo.ParentId == 0 || !catalogs.TryGetValue(moduleId == 0 ? menuInfo.ParentId : moduleId, out var catalog))
            {
                result.Add((menuInfo.Sort, item));
                continue;
            }

            if (!modules.TryGetValue(catalog.MenuId, out ElSelectorOutput<long> module))
            {
                module = new ElSelectorOutput<long>
                {
                    Value = catalog.MenuId, Label = catalog.MenuName, Data = new {IsModule = true}, Children = []
                };
                modules.Add(catalog.MenuId, module);
                result.Add((catalog.Sort, module));
            }

            module.Children.Add(item);
        }

        return result.OrderBy(node => node.Sort).Select(node => node.Node).ToList();
    }
}
