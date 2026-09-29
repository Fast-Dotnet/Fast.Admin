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
/// 系统管理种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedSystemManagement(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var systemCLMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = "System:Catalog",
            MenuName = "系统管理",
            MenuTitle = "系统管理",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Catalog,
            HasDesktop = true,
            DesktopIcon = "systemSetting",
            HasWeb = true,
            WebIcon = "fa-icon-SystemSetting",
            WebRouter = null,
            WebComponent = null,
            WebTab = false,
            WebKeepAlive = false,
            HasMobile = false,
            MobileIcon = "fa-icon-organization",
            MobileRouter = null,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        systemCLMenuModel = await db.Insertable(systemCLMenuModel).ExecuteReturnEntityAsync();

        await SeedTenants(db, applicationModel, dateTime, systemCLMenuModel);
        await SeedDatabases(db, applicationModel, dateTime, systemCLMenuModel);
        await SeedApplications(db, applicationModel, dateTime, systemCLMenuModel);
        await SeedApplicationOpenIds(db, applicationModel, dateTime, systemCLMenuModel);
    }
}
