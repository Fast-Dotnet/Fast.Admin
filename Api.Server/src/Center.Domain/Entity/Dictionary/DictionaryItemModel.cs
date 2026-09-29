// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 字典项表Model类
/// </summary>
[SugarTable("DictionaryItem", "字典项表")]
[SugarDbType(DatabaseTypeEnum.Center)]
public class DictionaryItemModel : BaseEntity
{
    /// <summary>
    /// 字典项Id
    /// </summary>
    [SugarColumn(ColumnDescription = "字典项Id", IsPrimaryKey = true)]
    public long DictionaryItemId { get; set; }

    /// <summary>
    /// 字典Id
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "字典Id")]
    public long DictionaryId { get; set; }

    /// <summary>
    /// 字典项名称
    /// </summary>
    [SugarColumn(ColumnDescription = "字典项名称", Length = 50)]
    public string Label { get; set; }

    /// <summary>
    /// 字典项值
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "字典项值", Length = 50)]
    public string Value { get; set; }

    /// <summary>
    /// 标签类型
    /// </summary>
    [SugarColumn(ColumnDescription = "标签类型")]
    public TagTypeEnum Type { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    /// <remarks>从小到大</remarks>
    [Required]
    [SugarColumn(ColumnDescription = "排序")]
    public int Order { get; set; }

    /// <summary>
    /// 提示
    /// </summary>
    [SugarColumn(ColumnDescription = "提示", Length = 100)]
    public string Tips { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    [SugarColumn(ColumnDescription = "是否显示")]
    public bool Visible { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public CommonStatusEnum Status { get; set; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return DictionaryItemId.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is not DictionaryItemModel oldDictionaryItemModel)
        {
            return false;
        }

        return DictionaryItemId == oldDictionaryItemModel.DictionaryItemId
               && DictionaryId == oldDictionaryItemModel.DictionaryId
               && Label == oldDictionaryItemModel.Label
               && Value == oldDictionaryItemModel.Value
               && Type == oldDictionaryItemModel.Type
               && Order == oldDictionaryItemModel.Order;
    }
}
