// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 部门管理种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedDepartments(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel orgCLMenuModel)
    {
        var departmentMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.None,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.Department.Paged,
            MenuName = "部门管理",
            MenuTitle = "部门管理",
            ParentId = orgCLMenuModel.MenuId,
            ParentIds = [0, orgCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Paged],
            HasDesktop = true,
            DesktopIcon = "menu",
            HasWeb = true,
            WebIcon = null,
            WebRouter = "/admin/department",
            WebComponent = "admin/department/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/user.png",
            MobileRouter = "pages_admin/department/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        departmentMenuModel = await db.Insertable(departmentMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = departmentMenuModel.MenuId,
                    ButtonCode = PermissionConst.Department.Paged,
                    ButtonName = "列表",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Paged],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 1,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = departmentMenuModel.MenuId,
                    ButtonCode = PermissionConst.Department.Detail,
                    ButtonName = "详情",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Detail],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 2,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = departmentMenuModel.MenuId,
                    ButtonCode = PermissionConst.Department.Add,
                    ButtonName = "新增",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Add],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 3,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = departmentMenuModel.MenuId,
                    ButtonCode = PermissionConst.Department.Edit,
                    ButtonName = "编辑",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Edit],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 4,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = departmentMenuModel.MenuId,
                    ButtonCode = PermissionConst.Department.Delete,
                    ButtonName = "删除",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.Department.Delete],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 5,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
