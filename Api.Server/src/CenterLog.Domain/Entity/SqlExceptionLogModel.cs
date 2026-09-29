// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.CenterLog.Domain;

/// <summary>
/// Sql异常日志表Model类
/// </summary>
[SugarTable("Sql_ExceptionLog", "Sql异常日志表")]
[SugarDbType(DatabaseTypeEnum.CenterLog)]
[SugarIndex($"IX_{{table}}_{nameof(CreatedTime)}", nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex($"IX_{{table}}_{nameof(TenantId)}", nameof(TenantId), OrderByType.Asc)]
public class SqlExceptionLogModel : BaseRecordEntity
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true, IsIdentity = true)]
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
    /// 文件名称
    /// </summary>
    [SugarColumn(ColumnDescription = "文件名称", Length = 200)]
    public string FileName { get; set; }

    /// <summary>
    /// 文件行数
    /// </summary>
    [SugarColumn(ColumnDescription = "文件行数")]
    public int FileLine { get; set; }

    /// <summary>
    /// 方法名
    /// </summary>
    [SugarColumn(ColumnDescription = "方法名", Length = 200)]
    public string MethodName { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "异常信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Message { get; set; }

    /// <summary>
    /// 异常源
    /// </summary>
    [SugarColumn(ColumnDescription = "异常源", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Source { get; set; }

    /// <summary>
    /// 异常堆栈信息
    /// </summary>
    [SugarColumn(ColumnDescription = "异常堆栈信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string StackTrace { get; set; }

    /// <summary>
    /// 纯SQL，参数化后的SQL
    /// </summary>
    [SugarColumn(ColumnDescription = "纯SQL，参数化后的SQL", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PureSql { get; set; }

    /// <summary>
    /// 异常时间
    /// </summary>
    [SugarSearchTime]
    [Required]
    [SugarColumn(ColumnDescription = "异常时间", CreateTableFieldSort = 993)]
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
