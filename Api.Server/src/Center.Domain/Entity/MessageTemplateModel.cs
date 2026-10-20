// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 消息模板表Model类
/// </summary>
[SugarTable("MessageTemplate", "消息模板表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(TemplateType)}", nameof(AppKey), OrderByType.Asc, nameof(TemplateType), OrderByType.Asc, true)]
public class MessageTemplateModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string AppKey { get; set; }

    /// <summary>
    /// 模板类型
    /// </summary>
    [SugarColumn(ColumnDescription = "模板类型")]
    public MessageTemplateTypeEnum TemplateType { get; set; }

    /// <summary>
    /// 模板Code
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "模板Code", Length = 50)]
    public string TemplateCode { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
