// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 主数据库表Model类
/// </summary>
[SugarTable("DatabaseMain", "主数据库表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(DatabaseType)}", nameof(DatabaseType), OrderByType.Asc, nameof(TenantId), OrderByType.Asc,
    true)]
public class MainDatabaseModel : BaseTEntity, IUpdateVersion
{
    /// <summary>
    /// 主库Id
    /// </summary>
    [SugarColumn(ColumnDescription = "主库Id", IsPrimaryKey = true)]
    public long MainId { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型")]
    public DatabaseTypeEnum DatabaseType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型")]
    public SugarDbType DbType { get; set; }

    /// <summary>
    /// 公网Ip地址
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "公网Ip地址", Length = 15)]
    public string PublicIp { get; set; }

    /// <summary>
    /// 内网Ip地址
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "内网Ip地址", Length = 15)]
    public string IntranetIp { get; set; }

    /// <summary>
    /// 端口号
    /// </summary>
    [SugarColumn(ColumnDescription = "端口号")]
    public int Port { get; set; }

    /// <summary>
    /// 数据库名称
    /// </summary>
    /// <remarks>或 SQLite 文件路径</remarks>
    [Required]
    [SugarColumn(ColumnDescription = "数据库名称", Length = 50)]
    public string DbName { get; set; }

    /// <summary>
    /// 数据库用户
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库用户", Length = 20)]
    public string DbUser { get; set; }

    /// <summary>
    /// 数据库密码
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库密码", Length = 64)]
    public string DbPwd { get; set; }

    /// <summary>
    /// 自定义连接字符串
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义连接字符串", Length = 200)]
    public string CustomConnectionStr { get; set; }

    /// <summary>
    /// 超时时间，单位秒
    /// </summary>
    [SugarColumn(ColumnDescription = "超时时间，单位秒")]
    public int CommandTimeOut { get; set; }

    /// <summary>
    /// SqlSugar SQL执行警告阈值（秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "SqlSugar SQL执行警告阈值（秒）")]
    public int SugarSqlExecMaxSeconds { get; set; }

    /// <summary>
    /// 差异日志
    /// </summary>
    [SugarColumn(ColumnDescription = "差异日志")]
    public bool DiffLog { get; set; }

    /// <summary>
    /// 是否禁用 SqlSugar AOP
    /// </summary>
    /// <remarks>
    /// <para>使用 <see cref="ISqlSugarEntityHandler"/> 将日志保存到数据库时，必须为 AOP 涉及的日志表单独配置分库，并禁用 AOP</para>
    /// <para>也可以通过 <c>new</c> <see cref="SqlSugarClient"/> 的方式保存日志，否则可能产生递归调用</para>
    /// </remarks>
    [SugarColumn(ColumnDescription = "是否禁用 SqlSugar AOP")]
    public bool DisableAop { get; set; }

    /// <summary>
    /// 是否初始化
    /// </summary>
    [SugarColumn(ColumnDescription = "是否初始化")]
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }

    /// <summary>
    /// 从库信息
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(SlaveDatabaseModel.MainId), nameof(MainId))]
    public List<SlaveDatabaseModel> SlaveDatabaseList { get; set; }
}
