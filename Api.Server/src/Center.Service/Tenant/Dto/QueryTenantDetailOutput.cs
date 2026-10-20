// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.Tenant.Dto;

/// <summary>
/// 获取租户详情输出
/// </summary>
public class QueryTenantDetailOutput : PagedOutput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public string TenantNo { get; set; }

    /// <summary>
    /// 租户编码
    /// </summary>
    public string TenantCode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// 租户简称
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
    /// 租户英文名称
    /// </summary>
    public string SpellName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 租户管理员账号Id
    /// </summary>
    public long AdminAccountId { get; set; }

    /// <summary>
    /// 租户管理员名称
    /// </summary>
    public string AdminName { get; set; }

    /// <summary>
    /// 租户管理员手机
    /// </summary>
    public string AdminMobile { get; set; }

    /// <summary>
    /// 租户管理员邮箱
    /// </summary>
    public string AdminEmail { get; set; }

    /// <summary>
    /// 租户管理员电话
    /// </summary>
    public string AdminPhone { get; set; }

    /// <summary>
    /// 租户机器人名称
    /// </summary>
    public string RobotName { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    public TenantTypeEnum TenantType { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// 阿里云签名名称
    /// </summary>
    public string AliyunSignName { get; set; }

    /// <summary>
    /// 初次登录设备
    /// </summary>
    public string FirstLoginDevice { get; set; }

    /// <summary>
    /// 初次登录操作系统（版本）
    /// </summary>
    public string FirstLoginOS { get; set; }

    /// <summary>
    /// 初次登录浏览器（版本）
    /// </summary>
    public string FirstLoginBrowser { get; set; }

    /// <summary>
    /// 初次登录省份
    /// </summary>
    public string FirstLoginProvince { get; set; }

    /// <summary>
    /// 初次登录城市
    /// </summary>
    public string FirstLoginCity { get; set; }

    /// <summary>
    /// 初次登录Ip
    /// </summary>
    public string FirstLoginIp { get; set; }

    /// <summary>
    /// 初次登录时间
    /// </summary>
    public DateTime? FirstLoginTime { get; set; }

    /// <summary>
    /// 最后登录设备
    /// </summary>
    public string LastLoginDevice { get; set; }

    /// <summary>
    /// 最后登录操作系统（版本）
    /// </summary>
    public string LastLoginOS { get; set; }

    /// <summary>
    /// 最后登录浏览器（版本）
    /// </summary>
    public string LastLoginBrowser { get; set; }

    /// <summary>
    /// 最后登录省份
    /// </summary>
    public string LastLoginProvince { get; set; }

    /// <summary>
    /// 最后登录城市
    /// </summary>
    public string LastLoginCity { get; set; }

    /// <summary>
    /// 最后登录Ip
    /// </summary>
    public string LastLoginIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 密码错误次数
    /// </summary>
    public int? PasswordErrorTime { get; set; }

    /// <summary>
    /// 锁定开始时间
    /// </summary>
    public DateTime? LockStartTime { get; set; }

    /// <summary>
    /// 锁定结束时间
    /// </summary>
    public DateTime? LockEndTime { get; set; }
}
