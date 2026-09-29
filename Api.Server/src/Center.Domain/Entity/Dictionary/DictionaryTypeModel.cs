// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 字典类型表Model类
/// </summary>
[SugarTable("DictionaryType", "字典类型表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(DictionaryKey)}", nameof(DictionaryKey), OrderByType.Asc, true)]
public class DictionaryTypeModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 字典Id
    /// </summary>
    [SugarColumn(ColumnDescription = "字典Id", IsPrimaryKey = true)]
    public long DictionaryId { get; set; }

    /// <summary>
    /// 服务名称
    /// </summary>
    [SugarColumn(ColumnDescription = "服务名称", Length = 50)]
    public string ServiceName { get; set; }

    /// <summary>
    /// 字典Key
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "字典Key", Length = 50)]
    public string DictionaryKey { get; set; }

    /// <summary>
    /// 字典名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "字典名称", Length = 50)]
    public string DictionaryName { get; set; }

    /// <summary>
    /// 字典值类型
    /// </summary>
    [SugarColumn(ColumnDescription = "字典值类型")]
    public DictionaryValueTypeEnum ValueType { get; set; }

    /// <summary>
    /// Flags枚举
    /// </summary>
    [SugarColumn(ColumnDescription = "Flags枚举")]
    public bool HasFlags { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public CommonStatusEnum Status { get; set; }

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
    /// 字典项信息
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DictionaryItemModel.DictionaryId), nameof(DictionaryId))]
    public List<DictionaryItemModel> DictionaryItemList { get; set; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return DictionaryId.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is not DictionaryTypeModel oldDictionaryTypeModel)
        {
            return false;
        }

        return DictionaryId == oldDictionaryTypeModel.DictionaryId
               && DictionaryKey == oldDictionaryTypeModel.DictionaryKey
               && DictionaryName == oldDictionaryTypeModel.DictionaryName
               && ValueType == oldDictionaryTypeModel.ValueType
               && HasFlags == oldDictionaryTypeModel.HasFlags;
    }
}
