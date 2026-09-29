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
/// 消息发送记录种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedDevMessageSendRecord(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime,
        MenuModel devCLMenuModel)
    {
        var messageSendRecordModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.MessageSendRecord.Paged,
            MenuName = "消息记录",
            MenuTitle = "消息记录",
            ParentId = devCLMenuModel.MenuId,
            ParentIds = [0, devCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
            HasDesktop = true,
            DesktopIcon = "message",
            HasWeb = true,
            WebIcon = "el-icon-Message",
            WebRouter = "/dev/messageSendRecord",
            WebComponent = "dev/messageSendRecord/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = false,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/messageSendRecord.png",
            MobileRouter = "pages_dev/messageSendRecord/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        messageSendRecordModel = await db.Insertable(messageSendRecordModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = messageSendRecordModel.MenuId,
                    ButtonCode = PermissionConst.MessageSendRecord.Paged,
                    ButtonName = "列表",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
                    HasDesktop = true,
                    HasWeb = true,
                    Sort = 1,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                },
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = messageSendRecordModel.MenuId,
                    ButtonCode = PermissionConst.MessageSendRecord.Detail,
                    ButtonName = "详情",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
                    HasDesktop = true,
                    HasWeb = true,
                    Sort = 2,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
