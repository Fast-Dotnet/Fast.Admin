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
/// 组织架构种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedOrganizationManagement(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var orgCLMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.None,
            AppId = applicationModel.AppId,
            MenuCode = "Organization:Catalog",
            MenuName = "组织架构",
            MenuTitle = "组织架构",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Catalog,
            HasDesktop = true,
            DesktopIcon = "organization",
            HasWeb = true,
            WebIcon = "fa-icon-Organization",
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
        orgCLMenuModel = await db.Insertable(orgCLMenuModel).ExecuteReturnEntityAsync();

        await SeedPositions(db, applicationModel, dateTime, orgCLMenuModel);
        await SeedJobLevels(db, applicationModel, dateTime, orgCLMenuModel);
        await SeedRoles(db, applicationModel, dateTime, orgCLMenuModel);
        await SeedDepartments(db, applicationModel, dateTime, orgCLMenuModel);
        await SeedEmployees(db, applicationModel, dateTime, orgCLMenuModel);
        await SeedOnlineUsers(db, applicationModel, dateTime, orgCLMenuModel);
    }
}
