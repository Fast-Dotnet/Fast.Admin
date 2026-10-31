// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 授权用户信息
/// </summary>
[SuppressSniffer]
public class AuthUserInfo
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual string SessionId { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public virtual AppEnvironmentEnum DeviceType { get; set; }

    /// <summary>
    /// 设备Id
    /// </summary>
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// WebStock 连接Id
    /// </summary>
    public virtual string ConnectionId { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    public virtual string AppNo { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public virtual string AppName { get; set; }

    #region 账号

    /// <summary>
    /// 账号Id
    /// </summary>
    public virtual long AccountId { get; set; }

    /// <summary>
    /// 账号Key
    /// </summary>
    public virtual string AccountKey { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    public virtual string Mobile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public virtual string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public virtual string Avatar { get; set; }

    /// <summary>
    /// 账号是否已完成身份校验
    /// </summary>
    public virtual bool IdentityVerification { get; set; }

    #endregion

    #region 客户端用户

    /// <summary>
    /// 客户端用户Id
    /// </summary>
    public virtual long? ClientUserId { get; set; }

    /// <summary>
    /// 客户端唯一用户标识
    /// </summary>
    public virtual string ClientUserOpenId { get; set; }

    #endregion

    #region 租户

    /// <summary>
    /// 租户Id
    /// </summary>
    public virtual long TenantId { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public virtual string TenantNo { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public virtual string TenantName { get; set; }

    /// <summary>
    /// 租户编码
    /// </summary>
    public virtual string TenantCode { get; set; }

    /// <summary>
    /// 是否系统租户
    /// </summary>
    public virtual bool IsSystemTenant { get; set; }

    #endregion

    /// <summary>
    /// 用户Key
    /// </summary>
    public virtual string UserKey { get; set; }

    /// <summary>
    /// 职员Id
    /// </summary>
    public virtual long EmployeeId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public virtual string EmployeeNo { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public virtual string EmployeeName { get; set; }

    /// <summary>
    /// 部门Id
    /// </summary>
    public virtual long? DepartmentId { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public virtual string DepartmentName { get; set; }

    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public virtual bool IsSuperAdmin { get; set; }

    /// <summary>
    /// 是否管理员
    /// </summary>
    public virtual bool IsAdmin { get; set; }

    /// <summary>
    /// 最后登录设备
    /// </summary>
    public virtual string LastLoginDevice { get; set; }

    /// <summary>
    /// 最后登录操作系统（版本）
    /// </summary>
    public virtual string LastLoginOS { get; set; }

    /// <summary>
    /// 最后登录浏览器（版本）
    /// </summary>
    public virtual string LastLoginBrowser { get; set; }

    /// <summary>
    /// 最后登录省份
    /// </summary>
    public virtual string LastLoginProvince { get; set; }

    /// <summary>
    /// 最后登录城市
    /// </summary>
    public virtual string LastLoginCity { get; set; }

    /// <summary>
    /// 最后登录Ip
    /// </summary>
    public virtual string LastLoginIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public virtual DateTime LastLoginTime { get; set; }

    /// <summary>
    /// 角色Id集合
    /// </summary>
    public virtual List<long> RoleIdList { get; set; } = new();

    /// <summary>
    /// 角色名称集合
    /// </summary>
    public virtual List<string> RoleNameList { get; set; } = new();

    /// <summary>
    /// 角色类型
    /// </summary>
    public virtual RoleTypeEnum RoleType { get; set; }

    /// <summary>
    /// 数据范围类型
    /// </summary>
    public virtual DataScopeTypeEnum DataScopeType { get; set; }

    /// <summary>
    /// 自定义数据范围部门Id集合
    /// </summary>
    public virtual List<long> DataScopeDepartmentIdList { get; set; } = new();

    /// <summary>
    /// 菜单编码集合
    /// </summary>
    public virtual List<string> MenuCodeList { get; set; } = new();

    /// <summary>
    /// 按钮编码集合
    /// </summary>
    public virtual List<string> ButtonCodeList { get; set; } = new();
}
