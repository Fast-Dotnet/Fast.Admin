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
/// 应用版本种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedApplicationVersions(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel systemCLMenuModel)
    {
        var appMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.AppVersion.Paged,
            MenuName = "应用版本",
            MenuTitle = "应用版本",
            ParentId = systemCLMenuModel.MenuId,
            ParentIds = [0, systemCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = GlobalContext.RolePermissions[PermissionConst.AppVersion.Paged],
            HasDesktop = true,
            DesktopIcon = "desktop",
            HasWeb = true,
            WebIcon = "fa-icon-Desktop",
            WebRouter = "/system/applicationVersion",
            WebComponent = "system/applicationVersion/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/app.png",
            MobileRouter = "pages_system/applicationVersion/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        appMenuModel = await db.Insertable(appMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppVersion.Paged,
                    ButtonName = "列表",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppVersion.Paged],
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppVersion.Detail,
                    ButtonName = "详情",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppVersion.Detail],
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppVersion.Add,
                    ButtonName = "新增",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppVersion.Add],
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppVersion.Edit,
                    ButtonName = "编辑",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppVersion.Edit],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 4,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
