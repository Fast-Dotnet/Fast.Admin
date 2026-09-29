// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 表格列配置缓存表Model类
/// </summary>
[SugarTable("TableColumnConfigCache", "表格列配置缓存表")]
[SugarDbType(DatabaseTypeEnum.Center)]
public class TableColumnConfigCacheModel : IBaseTEntity
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", IsPrimaryKey = true)]
    public long UserId { get; set; }

    /// <summary>
    /// 表格Id
    /// </summary>
    [SugarColumn(ColumnDescription = "表格Id", IsPrimaryKey = true)]
    public long TableId { get; set; }

    /// <summary>
    /// 列Id
    /// </summary>
    [SugarColumn(ColumnDescription = "列Id", IsPrimaryKey = true)]
    public long ColumnId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "名称", Length = 50)]
    public string Label { get; set; }

    /// <summary>
    /// 固定
    /// </summary>
    /// <remarks>
    /// <para>left：左侧</para>
    /// <para>right：右侧</para>
    /// </remarks>
    [SugarColumn(ColumnDescription = "固定", Length = 5)]
    public string Fixed { get; set; }

    /// <summary>
    /// 自动宽度
    /// </summary>
    [SugarColumn(ColumnDescription = "自动宽度")]
    public bool AutoWidth { get; set; }

    /// <summary>
    /// 宽度
    /// </summary>
    [SugarColumn(ColumnDescription = "宽度")]
    public int Width { get; set; }

    /// <summary>
    /// 小的宽度
    /// </summary>
    [SugarColumn(ColumnDescription = "小的宽度")]
    public int SmallWidth { get; set; }

    /// <summary>
    /// 顺序
    /// </summary>
    [SugarColumn(ColumnDescription = "顺序")]
    public int Order { get; set; }

    /// <summary>
    /// 显示
    /// </summary>
    [SugarColumn(ColumnDescription = "显示")]
    public bool Show { get; set; }

    /// <summary>
    /// 复制
    /// </summary>
    [SugarColumn(ColumnDescription = "复制")]
    public bool Copy { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public bool Sortable { get; set; }

    /// <summary>
    /// 搜索项名称
    /// </summary>
    [SugarColumn(ColumnDescription = "搜索项名称", Length = 50)]
    public string SearchLabel { get; set; }

    /// <summary>
    /// 搜索项排序
    /// </summary>
    [SugarColumn(ColumnDescription = "搜索项排序")]
    public int SearchOrder { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "创建时间", CreateTableFieldSort = 993)]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", CreateTableFieldSort = 997)]
    public long TenantId { get; set; }
}
