// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.CenterLog.Domain;

/// <summary>
/// Sql差异日志表Model类
/// </summary>
[SugarTable("Sql_DiffLog_{year}{month}{day}", "Sql差异日志表")]
[SplitTable(SplitType.Month)]
[SugarDbType(DatabaseTypeEnum.CenterLog)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedTime)}", nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(TenantId)}", nameof(TenantId), OrderByType.Asc)]
public class SqlDiffLogModel : BaseRecordEntity
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 账号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "账号Id")]
    public long? AccountId { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 20)]
    public string NickName { get; set; }

    /// <summary>
    /// 差异日志类型
    /// </summary>
    [SugarColumn(ColumnDescription = "差异日志类型")]
    public DiffLogTypeEnum DiffType { get; set; }

    /// <summary>
    /// 表名称
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "表名称", Length = 100)]
    public string TableName { get; set; }

    /// <summary>
    /// 表描述
    /// </summary>
    [SugarColumn(ColumnDescription = "表描述", Length = 100)]
    public string TableDescription { get; set; }

    /// <summary>
    /// 旧的列信息
    /// </summary>
    [SugarColumn(ColumnDescription = "旧的列信息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsJson = true)]
    public List<List<DiffLogColumnInfo>> BeforeColumnList { get; set; }

    /// <summary>
    /// 新的列信息
    /// </summary>
    [SugarColumn(ColumnDescription = "新的列信息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsJson = true)]
    public List<List<DiffLogColumnInfo>> AfterColumnList { get; set; }

    /// <summary>
    /// 执行秒数
    /// </summary>
    [SugarColumn(ColumnDescription = "执行秒数")]
    public double? ExecuteSeconds { get; set; }

    /// <summary>
    /// 纯SQL，参数化后的SQL
    /// </summary>
    [SugarColumn(ColumnDescription = "纯SQL，参数化后的SQL", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PureSql { get; set; }

    /// <summary>
    /// 差异时间
    /// </summary>
    [SplitField]
    [SugarSearchTime]
    [Required]
    [SugarColumn(ColumnDescription = "差异时间", CreateTableFieldSort = 993)]
    public override DateTime? CreatedTime { get; set; }

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
}
