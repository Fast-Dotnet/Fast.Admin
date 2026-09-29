// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.AdminLog.Domain;

/// <summary>
/// 操作日志表Model类
/// </summary>
[SugarTable("OperateLog_{year}{month}{day}", "操作日志表")]
[SplitTable(SplitType.Month)]
[SugarDbType(DatabaseTypeEnum.AdminLog)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedUserId)}", nameof(CreatedUserId), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedTime)}", nameof(CreatedTime), OrderByType.Asc)]
public class OperateLogModel : BaseRecordEntity
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "工号", Length = 20)]
    public string EmployeeNo { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [Required]
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "标题", Length = 50)]
    public string Title { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    [SugarColumn(ColumnDescription = "操作类型")]
    public OperateLogTypeEnum OperateType { get; set; }

    /// <summary>
    /// 业务Id
    /// </summary>
    [SugarColumn(ColumnDescription = "业务Id")]
    public long? BizId { get; set; }

    /// <summary>
    /// 业务编码
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "业务编码", Length = 30)]
    public string BizNo { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 500)]
    public string Description { get; set; }

    /// <summary>
    /// 操作者用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "操作者用户Id", CreateTableFieldSort = 991)]
    public override long? CreatedUserId { get; set; }

    /// <summary>
    /// 操作者用户名称
    /// </summary>
    [SugarColumn(ColumnDescription = "操作者用户名称", Length = 20, CreateTableFieldSort = 992)]
    public override string CreatedUserName { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [SplitField]
    [SugarSearchTime]
    [Required]
    [SugarColumn(ColumnDescription = "操作时间", CreateTableFieldSort = 993)]
    public override DateTime? CreatedTime { get; set; }
}
