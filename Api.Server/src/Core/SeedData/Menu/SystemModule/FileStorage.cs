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
/// 文件存储种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedFileStorage(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var fileMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Basic,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.FilePaged,
            MenuName = "文件存储",
            MenuTitle = "文件存储",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.Default | RoleTypeEnum.IT | RoleTypeEnum.HR | RoleTypeEnum.Finance,
            HasDesktop = true,
            DesktopIcon = "desktop",
            HasWeb = true,
            WebIcon = "el-icon-FolderOpened",
            WebRouter = "/system/file",
            WebComponent = "system/file/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/app.png",
            MobileRouter = "pages_system/file/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        fileMenuModel = await db.Insertable(fileMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Basic,
                    AppId = applicationModel.AppId,
                    MenuId = fileMenuModel.MenuId,
                    ButtonCode = PermissionConst.FilePaged,
                    ButtonName = "列表",
                    RoleType =
                        RoleTypeEnum.Admin | RoleTypeEnum.Default | RoleTypeEnum.IT | RoleTypeEnum.HR | RoleTypeEnum.Finance,
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 1,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
