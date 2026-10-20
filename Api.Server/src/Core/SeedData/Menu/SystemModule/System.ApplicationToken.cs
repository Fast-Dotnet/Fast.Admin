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
/// 应用凭据种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedApplicationTokens(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel systemCLMenuModel)
    {
        var appTokenMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.AppToken.Paged,
            MenuName = "应用凭据",
            MenuTitle = "应用凭据",
            ParentId = systemCLMenuModel.MenuId,
            ParentIds = [0, systemCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = GlobalContext.RolePermissions[PermissionConst.AppToken.Paged],
            HasDesktop = true,
            DesktopIcon = "creditCard",
            HasWeb = true,
            WebIcon = "el-icon-CreditCard",
            WebRouter = "/system/applicationToken",
            WebComponent = "system/applicationToken/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = false,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        appTokenMenuModel = await db.Insertable(appTokenMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appTokenMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppToken.Paged,
                    ButtonName = "列表",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppToken.Paged],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 1,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appTokenMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppToken.Add,
                    ButtonName = "新增",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppToken.Add],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 2,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appTokenMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppToken.Delete,
                    ButtonName = "删除",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppToken.Delete],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 3,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = appTokenMenuModel.MenuId,
                    ButtonCode = PermissionConst.AppToken.Refresh,
                    ButtonName = "刷新",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.AppToken.Refresh],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 4,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
