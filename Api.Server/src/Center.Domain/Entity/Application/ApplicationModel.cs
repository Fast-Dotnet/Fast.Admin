// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用表Model类
/// </summary>
[SugarTable("Application", "应用表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(AppNo)}", nameof(AppNo), OrderByType.Asc, true)]
[SugarIndex($"UX_{{table}}_{nameof(AppName)}", nameof(AppName), OrderByType.Asc, true)]
public class ApplicationModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 应用Id
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", IsPrimaryKey = true)]
    public long AppId { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [SugarColumn(ColumnDescription = "版本")]
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "应用编号", Length = 11)]
    public string AppNo { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "应用名称", Length = 30)]
    public string AppName { get; set; }

    /// <summary>
    /// LogoUrl
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "LogoUrl", Length = 200)]
    public string LogoUrl { get; set; }

    /// <summary>
    /// 主题色
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "主题色", Length = 7)]
    public string ThemeColor { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 200)]
    public string Remark { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", CreateTableFieldSort = 997)]
    public long? TenantId { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    [SugarColumn(ColumnDescription = "租户名称", Length = 30, CreateTableFieldSort = 997)]
    public string TenantName { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
