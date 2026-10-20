// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 商户号表Model类
/// </summary>
[SugarTable("Merchant", "商户号表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(MerchantNo)}", nameof(MerchantNo), OrderByType.Asc, true)]
public class MerchantModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 商户号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "商户号Id", IsPrimaryKey = true)]
    public long MerchantId { get; set; }

    /// <summary>
    /// 商户号类型
    /// </summary>
    [SugarColumn(ColumnDescription = "商户号类型")]
    public PaymentChannelEnum MerchantType { get; set; }

    /// <summary>
    /// 商户名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "商户名称", Length = 30)]
    public string MerchantName { get; set; }

    /// <summary>
    /// 商户号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "商户号", Length = 32)]
    public string MerchantNo { get; set; }

    /// <summary>
    /// 商户密钥
    /// </summary>
    [SugarColumn(ColumnDescription = "商户密钥", Length = 200)]
    public string MerchantSecret { get; set; }

    /// <summary>
    /// 公钥序号
    /// </summary>
    [SugarColumn(ColumnDescription = "公钥序号", Length = 200)]
    public string PublicSerialNum { get; set; }

    /// <summary>
    /// 公钥
    /// </summary>
    [SugarColumn(ColumnDescription = "公钥", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PublicKey { get; set; }

    /// <summary>
    /// 证书序号
    /// </summary>
    [SugarColumn(ColumnDescription = "证书序号", Length = 200)]
    public string CertSerialNum { get; set; }

    /// <summary>
    /// 证书
    /// </summary>
    [SugarColumn(ColumnDescription = "证书", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Cert { get; set; }

    /// <summary>
    /// 证书私钥
    /// </summary>
    [SugarColumn(ColumnDescription = "证书私钥", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string CertPrivateKey { get; set; }

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
}
