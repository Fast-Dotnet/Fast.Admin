// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Menu.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Menu;

/// <summary>
/// 菜单服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "menu")]
[PlatformOnly]
public class MenuService : IDynamicApplication
{
    private readonly ISqlSugarRepository<MenuModel> _repository;

    public MenuService(ISqlSugarRepository<MenuModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 菜单选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("菜单选择器", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Menu.Paged)]
    public async Task<List<ElSelectorOutput<long>>> MenuSelector()
    {
        var data = await _repository.Entities.OrderBy(ob => ob.Sort)
            .Select(sl => new {sl.MenuId, sl.MenuName, sl.MenuCode, sl.ParentId})
            .ToListAsync();

        return data.Select(sl =>
                new ElSelectorOutput<long>
                    {
                        Value = sl.MenuId, Label = sl.MenuName, ParentId = sl.ParentId, Data = new {sl.MenuCode}
                    })
            .ToList()
            .Build();
    }

    /// <summary>
    /// 获取菜单列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取菜单列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Menu.Paged)]
    public async Task<List<QueryMenuPagedOutput>> QueryMenuPaged(QueryMenuPagedInput input)
    {
        List<QueryMenuPagedOutput> data = await _repository.Entities.LeftJoin<ApplicationModel>((t1, t2) => t1.AppId == t2.AppId)
            .WhereIF(input.Edition != null, t1 => t1.Edition == input.Edition)
            .WhereIF(input.AppId != null, t1 => t1.AppId == input.AppId)
            .WhereIF(input.MenuType != null, t1 => t1.MenuType == input.MenuType)
            .WhereIF(input.HasDesktop != null, t1 => t1.HasDesktop == input.HasDesktop)
            .WhereIF(input.HasWeb != null, t1 => t1.HasWeb == input.HasWeb)
            .WhereIF(input.Visible != null, t1 => t1.Visible == input.Visible)
            .WhereIF(input.HasMobile != null, t1 => t1.HasMobile == input.HasMobile)
            .WhereIF(input.Status != null, t1 => t1.Status == input.Status)
            .OrderByIF(input.IsOrderBy, t1 => t1.Sort)
            .Select((t1, t2) => new QueryMenuPagedOutput
            {
                MenuId = t1.MenuId,
                Edition = t1.Edition,
                AppId = t1.AppId,
                AppName = t2.AppName,
                MenuCode = t1.MenuCode,
                MenuName = t1.MenuName,
                MenuTitle = t1.MenuTitle,
                ParentId = t1.ParentId,
                MenuType = t1.MenuType,
                RoleType = t1.RoleType,
                HasDesktop = t1.HasDesktop,
                DesktopIcon = t1.DesktopIcon,
                DesktopRouter = t1.DesktopRouter,
                HasWeb = t1.HasWeb,
                WebIcon = t1.WebIcon,
                WebRouter = t1.WebRouter,
                WebComponent = t1.WebComponent,
                WebTab = t1.WebTab,
                WebKeepAlive = t1.WebKeepAlive,
                HasMobile = t1.HasMobile,
                MobileIcon = t1.MobileIcon,
                MobileRouter = t1.MobileRouter,
                Link = t1.Link,
                Visible = t1.Visible,
                Sort = t1.Sort,
                Status = t1.Status,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .SugarPaged(input)
            .ToListAsync();

        return new TreeBuildUtil<QueryMenuPagedOutput, long>().Build(data);
    }

    /// <summary>
    /// 获取菜单详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取菜单详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Menu.Detail)]
    public async Task<QueryMenuDetailOutput> QueryMenuDetail([Required(ErrorMessage = "菜单Id不能为为空")] long? menuId)
    {
        QueryMenuDetailOutput result = await _repository.Entities.LeftJoin<ApplicationModel>((t1, t2) => t1.AppId == t2.AppId)
            .Where(t1 => t1.MenuId == menuId)
            .Select((t1, t2) => new QueryMenuDetailOutput
            {
                MenuId = t1.MenuId,
                Edition = t1.Edition,
                AppId = t1.AppId,
                AppName = t2.AppName,
                MenuCode = t1.MenuCode,
                MenuName = t1.MenuName,
                MenuTitle = t1.MenuTitle,
                ParentId = t1.ParentId,
                MenuType = t1.MenuType,
                RoleType = t1.RoleType,
                HasDesktop = t1.HasDesktop,
                DesktopIcon = t1.DesktopIcon,
                DesktopRouter = t1.DesktopRouter,
                HasWeb = t1.HasWeb,
                WebIcon = t1.WebIcon,
                WebRouter = t1.WebRouter,
                WebComponent = t1.WebComponent,
                WebTab = t1.WebTab,
                WebKeepAlive = t1.WebKeepAlive,
                HasMobile = t1.HasMobile,
                MobileIcon = t1.MobileIcon,
                MobileRouter = t1.MobileRouter,
                Link = t1.Link,
                Visible = t1.Visible,
                Sort = t1.Sort,
                Status = t1.Status,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        result.ButtonList = await _repository.Queryable<ButtonModel>()
            .Where(wh => wh.MenuId == menuId)
            .OrderBy(ob => ob.Sort)
            .Select(sl => new EditMenuButtonInput
            {
                ButtonId = sl.ButtonId,
                Edition = sl.Edition,
                ButtonCode = sl.ButtonCode,
                ButtonName = sl.ButtonName,
                RoleType = sl.RoleType,
                HasDesktop = sl.HasDesktop,
                HasWeb = sl.HasWeb,
                HasMobile = sl.HasMobile,
                Sort = sl.Sort,
                Status = sl.Status
            })
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// 添加菜单
    /// </summary>
    [HttpPost]
    [ApiInfo("添加菜单", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Menu.Add)]
    public async Task AddMenu(AddMenuInput input)
    {
        ApplicationModel applicationModel = await _repository
            .Queryable<ApplicationModel>()
            .SingleAsync(s => s.AppId == input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (await _repository.AnyAsync(a => a.AppId == applicationModel.AppId && a.MenuName == input.MenuName))
        {
            throw new UserFriendlyException("菜单名称重复！");
        }

        var menuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = input.Edition,
            AppId = applicationModel.AppId,
            MenuCode = input.MenuCode,
            MenuName = input.MenuName,
            MenuTitle = input.MenuTitle,
            MenuType = input.MenuType,
            RoleType = input.RoleType,
            HasDesktop = input.HasDesktop,
            DesktopIcon = input.DesktopIcon,
            DesktopRouter = input.DesktopRouter,
            HasWeb = input.HasWeb,
            WebIcon = input.WebIcon,
            WebRouter = input.WebRouter,
            WebComponent = input.WebComponent,
            WebTab = input.WebTab,
            WebKeepAlive = input.WebKeepAlive,
            HasMobile = input.HasMobile,
            MobileIcon = input.MobileIcon,
            MobileRouter = input.MobileRouter,
            Link = input.Link,
            Visible = input.Visible,
            Sort = input.Sort,
            Status = CommonStatusEnum.Enable
        };

        if (input.ParentId > 0)
        {
            MenuModel parentMenu = await _repository.SingleOrDefaultAsync(s => s.MenuId == input.ParentId);
            if (parentMenu == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            menuModel.ParentId = parentMenu.MenuId;
            menuModel.ParentIds = [..parentMenu.ParentIds, parentMenu.MenuId];
        }
        else
        {
            menuModel.ParentId = 0;
            menuModel.ParentIds = [0];
        }

        var addButtonList = input.ButtonList.Select(item => new ButtonModel
            {
                Edition = item.Edition,
                AppId = applicationModel.AppId,
                MenuId = menuModel.MenuId,
                ButtonCode = item.ButtonCode,
                ButtonName = item.ButtonName,
                RoleType = item.RoleType,
                HasDesktop = item.HasDesktop,
                HasWeb = item.HasWeb,
                HasMobile = item.HasMobile,
                Sort = item.Sort,
                Status = item.Status
            })
            .ToList();

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.InsertAsync(menuModel);
            await _repository.Insertable(addButtonList).ExecuteCommandAsync();
        }, ex => throw ex);
    }

    /// <summary>
    /// 编辑菜单
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑菜单", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Menu.Edit)]
    public async Task EditMenu(EditMenuInput input)
    {
        var buttonCodes = input.ButtonList.Select(sl => sl.ButtonCode).Distinct().ToList();
        if (buttonCodes.Count != input.ButtonList.Count)
        {
            throw new UserFriendlyException("按钮重复！");
        }

        ApplicationModel applicationModel = await _repository
            .Queryable<ApplicationModel>()
            .SingleAsync(s => s.AppId == input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (await _repository.AnyAsync(a =>
                a.AppId == applicationModel.AppId && a.MenuName == input.MenuName && a.MenuId != input.MenuId))
        {
            throw new UserFriendlyException("菜单名称重复！");
        }

        MenuModel menuModel = await _repository.SingleOrDefaultAsync(input.MenuId);
        if (menuModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        List<ButtonModel> buttonList = await _repository
            .Queryable<ButtonModel>()
            .Where(wh => wh.MenuId == input.MenuId)
            .ToListAsync();

        if (input.ParentId > 0)
        {
            MenuModel parentMenu = await _repository.SingleOrDefaultAsync(s => s.MenuId == input.ParentId);
            if (parentMenu == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            menuModel.ParentId = parentMenu.MenuId;
            menuModel.ParentIds = [.. parentMenu.ParentIds, parentMenu.MenuId];
        }
        else
        {
            menuModel.ParentId = 0;
            menuModel.ParentIds = [0];
        }

        menuModel.Edition = input.Edition;
        menuModel.AppId = applicationModel.AppId;
        menuModel.MenuCode = input.MenuCode;
        menuModel.MenuName = input.MenuName;
        menuModel.MenuTitle = input.MenuTitle;
        menuModel.MenuType = input.MenuType;
        menuModel.RoleType = input.RoleType;
        menuModel.HasDesktop = input.HasDesktop;
        menuModel.DesktopIcon = input.DesktopIcon;
        menuModel.DesktopRouter = input.DesktopRouter;
        menuModel.HasWeb = input.HasWeb;
        menuModel.WebIcon = input.WebIcon;
        menuModel.WebRouter = input.WebRouter;
        menuModel.WebComponent = input.WebComponent;
        menuModel.WebTab = input.WebTab;
        menuModel.WebKeepAlive = input.WebKeepAlive;
        menuModel.HasMobile = input.HasMobile;
        menuModel.MobileIcon = input.MobileIcon;
        menuModel.MobileRouter = input.MobileRouter;
        menuModel.Link = input.Link;
        menuModel.Visible = input.Visible;
        menuModel.Sort = input.Sort;
        menuModel.Status = input.Status;
        menuModel.RowVersion = input.RowVersion;

        var addButtonList = new List<ButtonModel>();
        var updateButtonList = new List<ButtonModel>();
        foreach (EditMenuButtonInput item in input.ButtonList)
        {
            ButtonModel buttonModel;
            if (item.ButtonId == null)
            {
                // 新增的
                buttonModel = new ButtonModel
                {
                    Edition = item.Edition,
                    AppId = applicationModel.AppId,
                    MenuId = menuModel.MenuId,
                    ButtonCode = item.ButtonCode,
                    ButtonName = item.ButtonName,
                    RoleType = item.RoleType,
                    HasDesktop = item.HasDesktop,
                    HasWeb = item.HasWeb,
                    HasMobile = item.HasMobile,
                    Sort = item.Sort,
                    Status = item.Status
                };
                addButtonList.Add(buttonModel);
            }
            else
            {
                // 更新的
                buttonModel = buttonList.SingleOrDefault(s => s.ButtonId == item.ButtonId);
                if (buttonModel == null)
                {
                    throw new UserFriendlyException("数据不存在！");
                }

                buttonModel.Edition = item.Edition;
                buttonModel.AppId = applicationModel.AppId;
                buttonModel.MenuId = menuModel.MenuId;
                buttonModel.ButtonCode = item.ButtonCode;
                buttonModel.ButtonName = item.ButtonName;
                buttonModel.RoleType = item.RoleType;
                buttonModel.HasDesktop = item.HasDesktop;
                buttonModel.HasWeb = item.HasWeb;
                buttonModel.HasMobile = item.HasMobile;
                buttonModel.Sort = item.Sort;
                buttonModel.Status = item.Status;
                updateButtonList.Add(buttonModel);
            }
        }

        // 删除的
        var deleteButtonList = buttonList.Where(wh => input.ButtonList.All(a => a.ButtonId != wh.ButtonId)).ToList();

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(menuModel);
            await _repository.Deleteable(deleteButtonList).ExecuteCommandAsync();
            await _repository.Updateable(updateButtonList).ExecuteCommandAsync();
            await _repository.Insertable(addButtonList).ExecuteCommandAsync();
        }, ex => throw ex);
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    [HttpPost]
    [ApiInfo("删除菜单", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Menu.Delete)]
    public async Task DeleteMenu(MenuIdInput input)
    {
        if (await _repository.AnyAsync(a => a.ParentId == input.MenuId))
        {
            throw new UserFriendlyException("菜单存在子菜单，无法删除！");
        }

        if (await _repository.Queryable<ButtonModel>().AnyAsync(a => a.MenuId == input.MenuId))
        {
            throw new UserFriendlyException("菜单存在按钮信息，无法删除！");
        }

        MenuModel menuModel = await _repository.SingleOrDefaultAsync(input.MenuId);
        if (menuModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(menuModel);
    }

    /// <summary>
    /// 菜单更改状态
    /// </summary>
    [HttpPost]
    [ApiInfo("菜单更改状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Menu.Status)]
    public async Task ChangeStatus(MenuIdInput input)
    {
        MenuModel menuModel = await _repository.SingleOrDefaultAsync(input.MenuId);
        if (menuModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        menuModel.Status = menuModel.Status switch
        {
            CommonStatusEnum.Enable => CommonStatusEnum.Disable,
            CommonStatusEnum.Disable => CommonStatusEnum.Enable,
            _ => menuModel.Status
        };
        menuModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(menuModel);
    }
}
