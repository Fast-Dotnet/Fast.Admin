// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.CenterLog.Domain;

/// <summary>
/// 请求日志表Model类
/// </summary>
[SugarTable("RequestLog_{year}{month}{day}", "请求日志表")]
[SplitTable(SplitType.Week)]
[SugarDbType(DatabaseTypeEnum.CenterLog)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedUserId)}", nameof(CreatedUserId), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(CreatedTime)}", nameof(CreatedTime), OrderByType.Asc)]
[SugarIndex($"IX_{{split_table}}_{nameof(TenantId)}", nameof(TenantId), OrderByType.Asc)]
public class RequestLogModel : BaseRecordEntity
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
    /// 是否执行成功
    /// </summary>
    [SugarColumn(ColumnDescription = "是否执行成功")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 操作行为
    /// </summary>
    [SugarColumn(ColumnDescription = "操作行为")]
    public HttpRequestActionEnum OperationAction { get; set; }

    /// <summary>
    /// 操作名称
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "操作名称", Length = 100)]
    public string OperationName { get; set; }

    /// <summary>
    /// 类名
    /// </summary>
    [SugarColumn(ColumnDescription = "类名", Length = 200)]
    public string ClassName { get; set; }

    /// <summary>
    /// 方法名
    /// </summary>
    [SugarColumn(ColumnDescription = "方法名", Length = 200)]
    public string MethodName { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    [SugarSearchValue]
    [SugarColumn(ColumnDescription = "请求地址", Length = 500)]
    public string Location { get; set; }

    /// <summary>
    /// 请求方式
    /// </summary>
    [SugarColumn(ColumnDescription = "请求方式")]
    public HttpRequestMethodEnum RequestMethod { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    [SugarColumn(ColumnDescription = "请求参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Param { get; set; }

    /// <summary>
    /// 返回结果
    /// </summary>
    [SugarColumn(ColumnDescription = "返回结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Result { get; set; }

    /// <summary>
    /// 耗时（毫秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "耗时（毫秒）")]
    public long? ElapsedTime { get; set; }

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
