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
/// 财务管理种子数据
/// </summary>
internal static partial class MenuSeedData
{
    private static async Task SeedFinanceManagement(ISqlSugarClient db, ApplicationModel applicationModel, DateTime dateTime)
    {
        var financeCLMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Professional,
            AppId = applicationModel.AppId,
            MenuCode = "Finance:Catalog",
            MenuName = "财务管理",
            MenuTitle = "财务管理",
            ParentId = 0,
            ParentIds = [0],
            MenuType = MenuTypeEnum.Catalog,
            HasDesktop = true,
            DesktopIcon = "money",
            HasWeb = true,
            WebIcon = "fa-icon-Money",
            WebRouter = null,
            WebComponent = null,
            WebTab = false,
            WebKeepAlive = false,
            HasMobile = false,
            MobileIcon = "fa-icon-money",
            MobileRouter = null,
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        financeCLMenuModel = await db.Insertable(financeCLMenuModel).ExecuteReturnEntityAsync();

        #region 商户号

        var merchantMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Internal,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.Merchant.Paged,
            MenuName = "商户号",
            MenuTitle = "商户号",
            ParentId = financeCLMenuModel.MenuId,
            ParentIds = [0, financeCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
            HasDesktop = true,
            DesktopIcon = "menu",
            HasWeb = true,
            WebIcon = null,
            WebRouter = "/system/merchant",
            WebComponent = "system/merchant/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/user.png",
            MobileRouter = "pages_system/merchant/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        merchantMenuModel = await db.Insertable(merchantMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ButtonModel>
            {
                new()
                {
                    ButtonId = YitIdHelper.NextId(),
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = merchantMenuModel.MenuId,
                    ButtonCode = PermissionConst.Merchant.Paged,
                    ButtonName = "列表",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = merchantMenuModel.MenuId,
                    ButtonCode = PermissionConst.Merchant.Detail,
                    ButtonName = "详情",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = merchantMenuModel.MenuId,
                    ButtonCode = PermissionConst.Merchant.Add,
                    ButtonName = "新增",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = merchantMenuModel.MenuId,
                    ButtonCode = PermissionConst.Merchant.Edit,
                    ButtonName = "编辑",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
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
                    Edition = EditionEnum.Internal,
                    AppId = applicationModel.AppId,
                    MenuId = merchantMenuModel.MenuId,
                    ButtonCode = PermissionConst.Merchant.Delete,
                    ButtonName = "删除",
                    RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT,
                    HasDesktop = true,
                    HasWeb = true,
                    HasMobile = true,
                    Sort = 5,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();

        #endregion

        #region 支付记录

        var payRecordMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Professional,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.PayRecordPaged,
            MenuName = "支付记录",
            MenuTitle = "支付记录",
            ParentId = financeCLMenuModel.MenuId,
            ParentIds = [0, financeCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
            HasDesktop = true,
            DesktopIcon = "lock",
            HasWeb = true,
            WebIcon = null,
            WebRouter = "/system/payRecord",
            WebComponent = "system/payRecord/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/passwordMap.png",
            MobileRouter = "pages_system/payRecord/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        payRecordMenuModel = await db.Insertable(payRecordMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new ButtonModel
            {
                ButtonId = YitIdHelper.NextId(),
                Edition = EditionEnum.Professional,
                AppId = applicationModel.AppId,
                MenuId = payRecordMenuModel.MenuId,
                ButtonCode = PermissionConst.PayRecordPaged,
                ButtonName = "列表",
                RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
                HasDesktop = true,
                HasWeb = true,
                HasMobile = true,
                Sort = 1,
                Status = CommonStatusEnum.Enable,
                CreatedTime = dateTime
            })
            .ExecuteCommandAsync();

        #endregion

        #region 退款记录

        var refundRecordMenuModel = new MenuModel
        {
            MenuId = YitIdHelper.NextId(),
            Edition = EditionEnum.Professional,
            AppId = applicationModel.AppId,
            MenuCode = PermissionConst.RefundRecordPaged,
            MenuName = "退款记录",
            MenuTitle = "退款记录",
            ParentId = financeCLMenuModel.MenuId,
            ParentIds = [0, financeCLMenuModel.MenuId],
            MenuType = MenuTypeEnum.Menu,
            RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
            HasDesktop = true,
            DesktopIcon = "lock",
            HasWeb = true,
            WebIcon = null,
            WebRouter = "/system/refundRecord",
            WebComponent = "system/refundRecord/index",
            WebTab = true,
            WebKeepAlive = true,
            HasMobile = true,
            MobileIcon = "https://image.fastdotnet.com/menu/mobile/passwordMap.png",
            MobileRouter = "pages_system/refundRecord/page/index",
            Visible = true,
            Sort = menuSort,
            Status = CommonStatusEnum.Enable,
            CreatedTime = dateTime
        };
        refundRecordMenuModel = await db.Insertable(refundRecordMenuModel).ExecuteReturnEntityAsync();
        await db.Insertable(new ButtonModel
            {
                ButtonId = YitIdHelper.NextId(),
                Edition = EditionEnum.Professional,
                AppId = applicationModel.AppId,
                MenuId = refundRecordMenuModel.MenuId,
                ButtonCode = PermissionConst.RefundRecordPaged,
                ButtonName = "列表",
                RoleType = RoleTypeEnum.Admin | RoleTypeEnum.IT | RoleTypeEnum.Finance,
                HasDesktop = true,
                HasWeb = true,
                HasMobile = true,
                Sort = 1,
                Status = CommonStatusEnum.Enable,
                CreatedTime = dateTime
            })
            .ExecuteCommandAsync();

        #endregion
    }
}
