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
/// 职位管理种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedPositions(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel orgCLMenuModel)
    {
        var positionMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.None,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.Position.Paged,
            MenuName = "职位管理",
            MenuTitle = "职位管理",
            ParentId = orgCLMenuModel.MenuId,
            ParentIds = [0, orgCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
            HasDesktop = true,
            DesktopIcon = "menu",
            HasWeb = true,
            WebIcon = null,
            WebRouter = "/system/position",
            WebComponent = "system/position/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/user.png",
            MobileRouter = "pages_system/position/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        positionMenuModel = await db.Insertable(positionMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.None,
                    AppId = applicationModel.AppId,
                    MenuId = positionMenuModel.MenuId,
                    ButtonCode = PermissionConst.Position.Paged,
                    ButtonName = "列表",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
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
                    MenuId = positionMenuModel.MenuId,
                    ButtonCode = PermissionConst.Position.Detail,
                    ButtonName = "详情",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
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
                    MenuId = positionMenuModel.MenuId,
                    ButtonCode = PermissionConst.Position.Add,
                    ButtonName = "新增",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
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
                    MenuId = positionMenuModel.MenuId,
                    ButtonCode = PermissionConst.Position.Edit,
                    ButtonName = "编辑",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
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
                    MenuId = positionMenuModel.MenuId,
                    ButtonCode = PermissionConst.Position.Delete,
                    ButtonName = "删除",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.HR,
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
