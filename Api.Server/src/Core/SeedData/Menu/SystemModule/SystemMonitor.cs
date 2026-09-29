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
/// 系统监控种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedSystemMonitor(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        await db.Insertable(new MenuModel
            {
                MenuId = YitIdHelper.NextId(),
                Edition = EditionEnum.Custom,
                AppId = applicationModel.AppId,
                MenuCode = PermissionConst.SystemMonitor,
                MenuName = "系统监控",
                MenuTitle = "系统监控",
                ParentId = 0,
                ParentIds = [0],
                MenuType = MenuTypeEnum.Menu,
                RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
                HasDesktop = true,
                DesktopIcon = "test",
                HasWeb = true,
                WebIcon = "fa-icon-Test",
                WebRouter = "/system/systemMonitor",
                WebComponent = "system/systemMonitor/index",
                WebTab = true,
                WebKeepAlive = true,
                HasMobile = true,
                MobileIcon = "https://image.fastdotnet.com/menu/mobile/database.png",
                MobileRouter = "pages_dev/systemMonitor/page/index",
                Visible = true,
                Sort = menuSort,
                Status = CommonStatusEnum.Enable,
                CreatedTime = dateTime
            })
            .ExecuteCommandAsync();
    }
}
