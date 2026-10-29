// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用标识表Model类
/// </summary>
[SugarTable("ApplicationClient", "应用标识表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(AppKey)}", nameof(AppKey), OrderByType.Asc, true)]
public class ApplicationClientModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id")]
    public long AppId { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string AppKey { get; set; }

    /// <summary>
    /// 应用密钥
    /// </summary>
    [SugarColumn(ColumnDescription = "应用密钥", Length = 32)]
    public string AppSecret { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    [SugarColumn(ColumnDescription = "应用类型")]
    public AppEnvironmentEnum AppType { get; set; }

    /// <summary>
    /// 环境类型
    /// </summary>
    [SugarColumn(ColumnDescription = "环境类型")]
    public EnvironmentTypeEnum EnvironmentType { get; set; }

    /// <summary>
    /// 登录组件
    /// </summary>
    [SugarColumn(ColumnDescription = "登录组件", Length = 50)]
    public string LoginComponent { get; set; }

    /// <summary>
    /// 请求超时时间（毫秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "请求超时时间（毫秒）")]
    public int RequestTimeout { get; set; }

    /// <summary>
    /// 请求加密
    /// </summary>
    [SugarColumn(ColumnDescription = "请求加密")]
    public bool RequestEncipher { get; set; }

    /// <summary>
    /// 微信商户号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "微信商户号Id")]
    public long? WeChatMerchantId { get; set; }

    /// <summary>
    /// 微信商户号
    /// </summary>
    [SugarColumn(ColumnDescription = "微信商户号", Length = 32)]
    public string WeChatMerchantNo { get; set; }

    /// <summary>
    /// 支付宝商户号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "支付宝商户号Id")]
    public long? AlipayMerchantId { get; set; }

    /// <summary>
    /// 支付宝商户号
    /// </summary>
    [SugarColumn(ColumnDescription = "支付宝商户号", Length = 32)]
    public string AlipayMerchantNo { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 200)]
    public string Remark { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }

    /// <summary>
    /// 应用
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AppId), nameof(ApplicationModel.AppId))]
    public ApplicationModel Application { get; set; }
}
