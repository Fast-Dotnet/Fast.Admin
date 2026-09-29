// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.CenterLog.Domain;

/// <summary>
/// 访问日志表Model类
/// </summary>
[SugarTable("VisitLog_{year}{month}{day}", "访问日志表")]
[SplitTable(SplitType.Month)]
[SugarDbType(DatabaseTypeEnum.CenterLog)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedUserId)}", nameof(CreatedUserId), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedTime)}", nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(TenantId)}", nameof(TenantId), OrderByType.Asc)]
public class VisitLogModel : BaseRecordEntity, IBaseTEntity
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
    public long AccountId { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    [Required]
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 20)]
    public string NickName { get; set; }

    /// <summary>
    /// 访问类型
    /// </summary>
    [SugarColumn(ColumnDescription = "访问类型")]
    public VisitTypeEnum VisitType { get; set; }

    /// <summary>
    /// 访问时间
    /// </summary>
    [SplitField]
    [SugarSearchTime]
    [Required]
    [SugarColumn(ColumnDescription = "访问时间", CreateTableFieldSort = 993)]
    public override DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", CreateTableFieldSort = 997)]
    public long TenantId { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    [SugarColumn(ColumnDescription = "租户名称", Length = 30, CreateTableFieldSort = 997)]
    public string TenantName { get; set; }
}
