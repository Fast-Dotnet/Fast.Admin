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
/// 消息模板种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedMessageTemplates(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel systemCLMenuModel)
    {
        var messageTemplateMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.MessageTemplate.Paged,
            MenuName = "消息模板",
            MenuTitle = "消息模板",
            ParentId = systemCLMenuModel.MenuId,
            ParentIds = [0, systemCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Paged],
            HasDesktop = true,
            DesktopIcon = "message",
            HasWeb = true,
            WebIcon = "el-icon-ChatDotRound",
            WebRouter = "/system/messageTemplate",
            WebComponent = "system/messageTemplate/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = false,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        messageTemplateMenuModel = await db.Insertable(messageTemplateMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = messageTemplateMenuModel.MenuId,
                    ButtonCode = PermissionConst.MessageTemplate.Paged,
                    ButtonName = "列表",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Paged],
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
                    MenuId = messageTemplateMenuModel.MenuId,
                    ButtonCode = PermissionConst.MessageTemplate.Detail,
                    ButtonName = "详情",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Detail],
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
                    MenuId = messageTemplateMenuModel.MenuId,
                    ButtonCode = PermissionConst.MessageTemplate.Add,
                    ButtonName = "新增",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Add],
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
                    MenuId = messageTemplateMenuModel.MenuId,
                    ButtonCode = PermissionConst.MessageTemplate.Edit,
                    ButtonName = "编辑",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Edit],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 4,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = messageTemplateMenuModel.MenuId,
                    ButtonCode = PermissionConst.MessageTemplate.Delete,
                    ButtonName = "删除",
                    RoleType = GlobalContext.RolePermissions[PermissionConst.MessageTemplate.Delete],
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = false,
                    Sort = 5,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
