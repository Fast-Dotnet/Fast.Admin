// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 密码记录表Model类
/// </summary>
[SugarTable("PasswordRecord", "密码记录表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"IX_{{table}}_{nameof(AccountId)}", nameof(AccountId), OrderByType.Asc)]
public class PasswordRecordModel : IDatabaseEntity
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
    public long AccountId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    [SugarColumn(ColumnDescription = "操作类型")]
    public PasswordOperationTypeEnum OperationType { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [SugarColumn(ColumnDescription = "类型")]
    public PasswordTypeEnum Type { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "密码", Length = 200)]
    public string Password { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "创建时间", CreateTableFieldSort = 993)]
    public DateTime? CreatedTime { get; set; }
}
