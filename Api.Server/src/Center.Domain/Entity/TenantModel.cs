// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 租户信息表Model类
/// </summary>
[SugarTable("Tenant", "租户信息表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(TenantNo)}", nameof(TenantNo), OrderByType.Asc, true)]
[SugarIndex($"UX_{{table}}_{nameof(TenantName)}", nameof(TenantName), OrderByType.Asc, true)]
public class TenantModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsPrimaryKey = true)]
    public long TenantId { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户编号", Length = 11)]
    public string TenantNo { get; set; }

    /// <summary>
    /// 租户编码
    /// </summary>
    /// <remarks>单号生成前缀</remarks>
    [Required]
    [SugarColumn(ColumnDescription = "租户编码", ColumnDataType = "varchar(5)")]
    public string TenantCode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户名称", Length = 30)]
    public string TenantName { get; set; }

    /// <summary>
    /// 租户简称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户简称", Length = 20)]
    public string ShortName { get; set; }

    /// <summary>
    /// 租户英文名称
    /// </summary>
    /// <remarks>根据 <see cref="TenantName"/> 生成的拼音</remarks>
    [Required]
    [SugarColumn(ColumnDescription = "租户英文名称", Length = 100)]
    public string SpellName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [SugarColumn(ColumnDescription = "版本")]
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 租户管理员账号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户管理员账号Id")]
    public long AdminAccountId { get; set; }

    /// <summary>
    /// 租户管理员名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户管理员名称", Length = 20)]
    public string AdminName { get; set; }

    /// <summary>
    /// 租户管理员手机
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户管理员手机", ColumnDataType = "varchar(11)")]
    public string AdminMobile { get; set; }

    /// <summary>
    /// 租户管理员邮箱
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户管理员邮箱", Length = 50)]
    public string AdminEmail { get; set; }

    /// <summary>
    /// 租户管理员电话
    /// </summary>
    [SugarColumn(ColumnDescription = "租户管理员电话", Length = 20)]
    public string AdminPhone { get; set; }

    /// <summary>
    /// 租户机器人名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "租户机器人名称", Length = 20)]
    public string RobotName { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [SugarColumn(ColumnDescription = "租户类型")]
    public TenantTypeEnum TenantType { get; set; }

    /// <summary>
    /// LogoUrl
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "LogoUrl", Length = 200)]
    public string LogoUrl { get; set; }

    /// <summary>
    /// 阿里云签名名称
    /// </summary>
    [SugarColumn(ColumnDescription = "阿里云签名名称", Length = 30)]
    public string AliyunSignName { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
