// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Runtime;
using Microsoft.AspNetCore.Http;

namespace Fast.Center.Domain;

/// <summary>
/// 退款记录表Model类
/// </summary>
[SugarTable("RefundRecord", "退款记录表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"IX_{{table}}_{nameof(BizOrderNo)}", nameof(BizOrderId), OrderByType.Desc, nameof(BizOrderNo), OrderByType.Desc)]
[SugarIndex($"IX_{{table}}_{nameof(RefundId)}", nameof(RefundId), OrderByType.Desc)]
public class RefundRecordModel : IBaseTEntity, IUpdateVersion
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string AppOpenId { get; set; }

    /// <summary>
    /// 退款商户号
    /// </summary>
    [SugarColumn(ColumnDescription = "退款商户号", Length = 32)]
    public string MerchantNo { get; set; }

    /// <summary>
    /// 业务订单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "业务订单Id")]
    public long BizOrderId { get; set; }

    /// <summary>
    /// 业务订单号
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "业务订单号", Length = 30)]
    public string BizOrderNo { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "支付渠道")]
    public PaymentChannelEnum PaymentChannel { get; set; }

    /// <summary>
    /// 唯一用户标识
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "唯一用户标识", Length = 28)]
    public string OpenId { get; set; }

    /// <summary>
    /// 统一用户标识
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "统一用户标识", Length = 28)]
    public string UnionId { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    [SugarColumn(ColumnDescription = "订单金额", Length = 18, DecimalDigits = 2)]
    public decimal OrderAmount { get; set; }

    /// <summary>
    /// 回调地址
    /// </summary>
    [SugarColumn(ColumnDescription = "回调地址", Length = 200)]
    public string NotifyUrl { get; set; }

    /// <summary>
    /// 是否已退款
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已退款")]
    public bool IsRefunded { get; set; }

    /// <summary>
    /// 交易流水号
    /// </summary>
    [SugarColumn(ColumnDescription = "交易流水号", Length = 128)]
    public string RefundId { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    [SugarColumn(ColumnDescription = "退款金额", Length = 18, DecimalDigits = 2)]
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// 退款时间
    /// </summary>
    [SugarColumn(ColumnDescription = "退款时间")]
    public DateTime? RefundTime { get; set; }

    /// <summary>
    /// 退款状态
    /// </summary>
    [SugarColumn(ColumnDescription = "退款状态", Length = 10)]
    public string RefundStatus { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备", Length = 50, CreateTableFieldSort = 983)]
    public string Device { get; set; }

    /// <summary>
    /// 操作系统（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "操作系统（版本）", Length = 50, CreateTableFieldSort = 984)]
    public string OS { get; set; }

    /// <summary>
    /// 浏览器（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览器（版本）", Length = 50, CreateTableFieldSort = 985)]
    public string Browser { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    [SugarColumn(ColumnDescription = "省份", Length = 20, CreateTableFieldSort = 986)]
    public string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [SugarColumn(ColumnDescription = "城市", Length = 20, CreateTableFieldSort = 987)]
    public string City { get; set; }

    /// <summary>
    /// Ip地址
    /// </summary>
    [SugarColumn(ColumnDescription = "Ip地址", Length = 15, CreateTableFieldSort = 988)]
    public string Ip { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarSearchTime]
    [Required]
    [SugarColumn(ColumnDescription = "创建时间", CreateTableFieldSort = 993)]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", CreateTableFieldSort = 996)]
    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", CreateTableFieldSort = 997)]
    public long TenantId { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }

    /// <summary>
    /// 记录表创建
    /// </summary>
    public void RecordCreate(HttpContext httpContext)
    {
        UserAgentInfo userAgentInfo = httpContext.RequestUserAgentInfo();
        WanNetIPInfo wanInfo = httpContext.RemoteIpv4Info();

        Device = userAgentInfo.Device;
        OS = userAgentInfo.OS;
        Browser = userAgentInfo.Browser;
        Province = wanInfo.Province;
        City = wanInfo.City;
        Ip = wanInfo.Ip;
    }
}
