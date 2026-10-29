// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用凭据表Model类
/// </summary>
[SugarTable("ApplicationToken", "应用凭据表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(AppKey)}", nameof(AppKey), OrderByType.Asc, nameof(TokenType), OrderByType.Asc, true)]
public class ApplicationTokenModel : IDatabaseEntity
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string AppKey { get; set; }

    /// <summary>
    /// 凭据类型
    /// </summary>
    [SugarColumn(ColumnDescription = "凭据类型")]
    public ApplicationTokenTypeEnum TokenType { get; set; }

    /// <summary>
    /// 凭据值
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "凭据值", Length = 512)]
    public string TokenValue { get; set; }

    /// <summary>
    /// 刷新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "刷新时间")]
    public DateTime? RefreshTime { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间")]
    public DateTime? ExpireTime { get; set; }
}
