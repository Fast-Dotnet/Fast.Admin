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
/// 开发接口种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedDevApi(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var apiCLMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = "DevApi:Catalog",
            MenuName = "Api",
            MenuTitle = "Api",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Catalog,
            HasDesktop = true,
            DesktopIcon = "api",
            HasWeb = true,
            WebIcon = "fa-icon-Api",
            WebRouter = null,
            WebComponent = null,
            WebTab = false,
            WebKeepAlive = false,
            HasMobile = false,
            MobileIcon = "fa-icon-api",
            MobileRouter = null,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        apiCLMenuModel = await db.Insertable(apiCLMenuModel).ExecuteReturnEntityAsync();

        #region Api

        var apiMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.ApiPaged,
            MenuName = "接口管理",
            MenuTitle = "接口管理",
            ParentId = apiCLMenuModel.MenuId,
            ParentIds = [0, apiCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.IT,
            HasDesktop = true,
            DesktopRouter = null,
            DesktopIcon = "api",
            HasWeb = true,
            WebIcon = "fa-icon-Gateway",
            WebRouter = "/dev/api",
            WebComponent = "dev/api/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = false,
            MobileIcon = null,
            MobileRouter = null,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        apiMenuModel = await db.Insertable(apiMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new ButtonModel
            {
                ButtonId = YitIdHelper.NextId(),
                Edition = EditionEnum.Internal,
                AppId = applicationModel.AppId,
                MenuId = apiMenuModel.MenuId,
                ButtonCode = PermissionConst.ApiPaged,
                ButtonName = "列表",
                RoleType = RoleTypeEnum.IT,
                HasDesktop = true,
                HasWeb = true,
                HasMobile = false,
                Sort = 1,
                Status = CommonStatusEnum.Enable,
                CreatedTime = dateTime
            })
            .ExecuteCommandAsync();

        await db.Insertable(new List<MenuModel>
            {
                new()
                {
                    MenuId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuCode = PermissionConst.ApiSwagger,
                    MenuName = "Swagger",
                    MenuTitle = "Swagger",
                    ParentId = apiCLMenuModel.MenuId,
                    ParentIds = [0, apiCLMenuModel.MenuId],
                    MenuType = MenuTypeEnum.Internal,
                    RoleType = RoleTypeEnum.IT,
                    HasDesktop = true,
                    DesktopIcon = "api",
                    DesktopRouter = null,
                    HasWeb = true,
                    WebIcon = "fa-icon-Swagger",
                    WebRouter = null,
                    WebComponent = null,
                    WebTab = false,
                    WebKeepAlive = false,
                    HasMobile = false,
                    MobileIcon = null,
                    MobileRouter = null,
                    Link = "http://127.0.0.1:38081",
                    Visible = true,
                    Sort = menuSort,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    MenuId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuCode = PermissionConst.ApiKnife4j,
                    MenuName = "Knife4j",
                    MenuTitle = "Knife4j",
                    ParentId = apiCLMenuModel.MenuId,
                    ParentIds = [0, apiCLMenuModel.MenuId],
                    MenuType = MenuTypeEnum.Internal,
                    RoleType = RoleTypeEnum.IT,
                    HasDesktop = true,
                    DesktopIcon = "api",
                    DesktopRouter = null,
                    HasWeb = true,
                    WebIcon = "fa-icon-Swagger",
                    WebRouter = null,
                    WebComponent = null,
                    WebTab = false,
                    WebKeepAlive = false,
                    HasMobile = false,
                    MobileIcon = null,
                    MobileRouter = null,
                    Link = "http://127.0.0.1:38081/knife4j",
                    Visible = true,
                    Sort = menuSort,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();

        #endregion
    }
}
