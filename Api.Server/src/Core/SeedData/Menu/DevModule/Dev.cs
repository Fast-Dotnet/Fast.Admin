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
/// 开发工具种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedDevTools(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var devCLMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = "Dev:Catalog",
            MenuName = "开发管理",
            MenuTitle = "开发管理",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Catalog,
            HasDesktop = true,
            DesktopIcon = "terminal",
            HasWeb = true,
            WebIcon = "fa-icon-Terminal",
            WebRouter = null,
            WebComponent = null,
            WebTab = false,
            WebKeepAlive = false,
            HasMobile = true,
            MobileIcon = "fa-icon-terminal",
            MobileRouter = null,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        devCLMenuModel = await db.Insertable(devCLMenuModel).ExecuteReturnEntityAsync();

        await SeedDevConfig(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevMenu(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevSerial(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevDictionary(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevTable(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevScheduler(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevPasswordRecord(db, applicationModel, dateTime, devCLMenuModel);
        await SeedDevMessageSendRecord(db, applicationModel, dateTime, devCLMenuModel);
    }
}
