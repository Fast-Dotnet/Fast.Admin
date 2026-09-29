// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 账号信息表Model类
/// </summary>
[SugarTable("Account", "账号信息表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(Mobile)}", nameof(Mobile), OrderByType.Asc, true)]
[SugarIndex($"UX_{{table}}_{nameof(Email)}", nameof(Email), OrderByType.Asc, true)]
public class AccountModel : IUpdateVersion
{
    /// <summary>
    /// 账号Id
    /// </summary>
    [SugarColumn(ColumnDescription = "账号Id", IsPrimaryKey = true)]
    public long AccountId { get; set; }

    /// <summary>
    /// 账号Key
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "账号Key", Length = 12)]
    public string AccountKey { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "邮箱", Length = 50)]
    public string Email { get; set; }

    /// <summary>
    /// 身份验证
    /// </summary>
    [SugarColumn(ColumnDescription = "身份验证")]
    public bool IdentityVerification { get; set; }

    /// <summary>
    /// 客户端用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端用户Id")]
    public long? ClientUserId { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "密码", Length = 200)]
    public string Password { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "昵称", Length = 20)]
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", Length = 200)]
    public string Avatar { get; set; }

    /// <summary>
    /// 初次登录租户
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录租户")]
    public long? FirstLoginTenantId { get; set; }

    /// <summary>
    /// 初次登录设备
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录设备", Length = 50)]
    public string FirstLoginDevice { get; set; }

    /// <summary>
    /// 初次登录操作系统（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录操作系统（版本）", Length = 50)]
    public string FirstLoginOS { get; set; }

    /// <summary>
    /// 初次登录浏览器（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录浏览器（版本）", Length = 50)]
    public string FirstLoginBrowser { get; set; }

    /// <summary>
    /// 初次登录省份
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录省份", Length = 20)]
    public string FirstLoginProvince { get; set; }

    /// <summary>
    /// 初次登录城市
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录城市", Length = 20)]
    public string FirstLoginCity { get; set; }

    /// <summary>
    /// 初次登录Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录Ip", Length = 15)]
    public string FirstLoginIp { get; set; }

    /// <summary>
    /// 初次登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "初次登录时间")]
    public DateTime? FirstLoginTime { get; set; }

    /// <summary>
    /// 最后登录租户
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录租户")]
    public long? LastLoginTenantId { get; set; }

    /// <summary>
    /// 最后登录设备
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录设备", Length = 50)]
    public string LastLoginDevice { get; set; }

    /// <summary>
    /// 最后登录操作系统（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录操作系统（版本）", Length = 50)]
    public string LastLoginOS { get; set; }

    /// <summary>
    /// 最后登录浏览器（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录浏览器（版本）", Length = 50)]
    public string LastLoginBrowser { get; set; }

    /// <summary>
    /// 最后登录省份
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录省份", Length = 20)]
    public string LastLoginProvince { get; set; }

    /// <summary>
    /// 最后登录城市
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录城市", Length = 20)]
    public string LastLoginCity { get; set; }

    /// <summary>
    /// 最后登录Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录Ip", Length = 15)]
    public string LastLoginIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最后登录时间")]
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 密码错误次数
    /// </summary>
    [SugarColumn(ColumnDescription = "密码错误次数")]
    public int? PasswordErrorTime { get; set; }

    /// <summary>
    /// 锁定开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "锁定开始时间")]
    public DateTime? LockStartTime { get; set; }

    /// <summary>
    /// 锁定结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "锁定结束时间")]
    public DateTime? LockEndTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "创建时间", CreateTableFieldSort = 993)]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", CreateTableFieldSort = 996)]
    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
