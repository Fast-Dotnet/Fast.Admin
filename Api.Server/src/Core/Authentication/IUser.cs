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
/// <remarks>作用域注册，保证当前请求管道中是唯一的，并且只会加载一次</remarks>
public interface IUser
{
    /// <summary>
    /// 会话Id
    /// </summary>
    string SessionId { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    AppEnvironmentEnum DeviceType { get; set; }

    /// <summary>
    /// 设备Id
    /// </summary>
    string DeviceId { get; set; }

    /// <summary>
    /// WebStock 连接Id
    /// </summary>
    string ConnectionId { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    string AppNo { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    string AppName { get; set; }

    #region 账号

    /// <summary>
    /// 账号Id
    /// </summary>
    long AccountId { get; set; }

    /// <summary>
    /// 账号Key
    /// </summary>
    string AccountKey { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    string Mobile { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    string Avatar { get; set; }

    /// <summary>
    /// 账号是否已完成身份校验
    /// </summary>
    bool IdentityVerification { get; set; }

    #endregion

    #region 客户端用户

    /// <summary>
    /// 客户端用户Id
    /// </summary>
    long ClientUserId { get; set; }

    /// <summary>
    /// 客户端唯一用户标识
    /// </summary>
    string ClientUserOpenId { get; set; }

    #endregion

    #region 租户

    /// <summary>
    /// 租户Id
    /// </summary>
    long TenantId { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    string TenantNo { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    string TenantName { get; set; }

    /// <summary>
    /// 租户编码
    /// </summary>
    string TenantCode { get; set; }

    /// <summary>
    /// 是否系统租户
    /// </summary>
    bool IsSystemTenant { get; set; }

    #endregion

    /// <summary>
    /// 用户Key
    /// </summary>
    string UserKey { get; set; }

    /// <summary>
    /// 职员Id
    /// </summary>
    long EmployeeId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    string EmployeeNo { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    string EmployeeName { get; set; }

    /// <summary>
    /// 部门Id
    /// </summary>
    long? DepartmentId { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    string DepartmentName { get; set; }

    /// <summary>
    /// 是否超级管理员
    /// </summary>
    bool IsSuperAdmin { get; set; }

    /// <summary>
    /// 是否管理员
    /// </summary>
    bool IsAdmin { get; set; }

    /// <summary>
    /// 最后登录设备
    /// </summary>
    string LastLoginDevice { get; set; }

    /// <summary>
    /// 最后登录操作系统（版本）
    /// </summary>
    string LastLoginOS { get; set; }

    /// <summary>
    /// 最后登录浏览器（版本）
    /// </summary>
    string LastLoginBrowser { get; set; }

    /// <summary>
    /// 最后登录省份
    /// </summary>
    string LastLoginProvince { get; set; }

    /// <summary>
    /// 最后登录城市
    /// </summary>
    string LastLoginCity { get; set; }

    /// <summary>
    /// 最后登录Ip
    /// </summary>
    string LastLoginIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    DateTime LastLoginTime { get; set; }

    /// <summary>
    /// 角色Id集合
    /// </summary>
    List<long> RoleIdList { get; set; }

    /// <summary>
    /// 角色名称集合
    /// </summary>
    List<string> RoleNameList { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    RoleTypeEnum RoleType { get; set; }

    /// <summary>
    /// 数据范围类型
    /// </summary>
    DataScopeTypeEnum DataScopeType { get; set; }

    /// <summary>
    /// 自定义数据范围部门Id集合
    /// </summary>
    List<long> DataScopeDepartmentIdList { get; set; }

    /// <summary>
    /// 菜单编码集合
    /// </summary>
    List<string> MenuCodeList { get; set; }

    /// <summary>
    /// 按钮编码集合
    /// </summary>
    List<string> ButtonCodeList { get; set; }

    /// <summary>
    /// 设置授权用户
    /// </summary>
    /// <param name="authUserInfo">授权用户信息</param>
    /// <param name="forceUserInfo">强制覆盖用户信息，默认 <see langword="false"/></param>
    /// <remarks>只会赋值一次</remarks>
    void SetAuthUser(AuthUserInfo authUserInfo, bool forceUserInfo = false);

    /// <summary>
    /// 从缓存中获取授权用户信息
    /// </summary>
    /// <returns>缓存中的授权用户信息</returns>
    Task<AuthUserInfo> GetAuthUserInfo(AppEnvironmentEnum deviceType, string appNo, string tenantNo, string employeeNo,
        string sessionId);

    /// <summary>
    /// 统一登录
    /// </summary>
    Task Login(AuthUserInfo authUserInfo);

    /// <summary>
    /// 客户端统一登录
    /// </summary>
    Task ClientLogin(AuthUserInfo authUserInfo);

    /// <summary>
    /// 机器人登录
    /// </summary>
    /// <remarks>非调度作业请勿使用</remarks>
    /// <returns>机器人登录令牌</returns>
    Task<string> RobotLogin();

    /// <summary>
    /// 刷新授权信息
    /// </summary>
    Task RefreshAuth(RefreshAuthDto input);

    /// <summary>
    /// 刷新账号信息
    /// </summary>
    Task RefreshAccount(RefreshAccountDto input);

    /// <summary>
    /// 刷新客户端用户信息
    /// </summary>
    Task RefreshClientUser(RefreshClientUserDto input);

    /// <summary>
    /// 刷新职员信息
    /// </summary>
    Task RefreshEmployee(RefreshEmployeeDto input);

    /// <summary>
    /// 撤销账号在所有租户和设备上的会话
    /// </summary>
    Task RevokeAccount(long accountId);

    /// <summary>
    /// 撤销租户的全部会话
    /// </summary>
    Task RevokeTenant(string tenantNo);

    /// <summary>
    /// 撤销指定租户职员的全部会话
    /// </summary>
    Task RevokeEmployee(string tenantNo, string employeeNo);

    /// <summary>
    /// 撤销统一主体在三端的会话，不操作管理账号
    /// </summary>
    Task RevokeClientUser(long userId);

    /// <summary>
    /// 统一退出登录
    /// </summary>
    Task Logout();
}
