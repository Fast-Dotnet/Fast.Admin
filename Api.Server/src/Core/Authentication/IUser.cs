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
/// <remarks>按作用域注册；同一请求共享此实例，首次赋值后默认不再覆盖，除非显式要求强制更新。</remarks>
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
    /// WebSocket 连接 ID
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
    /// 将授权信息复制到当前请求的用户上下文。
    /// </summary>
    /// <param name="authUserInfo">授权用户信息</param>
    /// <param name="forceUserInfo">强制覆盖用户信息，默认 <see langword="false"/></param>
    /// <remarks>已经赋值且未要求强制更新时直接返回。</remarks>
    /// <exception cref="UnauthorizedAccessException">需要赋值但授权信息为 null 时抛出。</exception>
    void SetAuthUser(AuthUserInfo authUserInfo, bool forceUserInfo = false);

    /// <summary>
    /// 从缓存中获取授权用户信息
    /// </summary>
    /// <param name="deviceType">登录设备类型</param>
    /// <param name="appNo">应用编号</param>
    /// <param name="tenantNo">租户编号</param>
    /// <param name="employeeNo">职员编号或客户端主体标识</param>
    /// <param name="sessionId">需要匹配的会话 ID</param>
    /// <returns>匹配会话的授权信息；缓存不存在或会话不匹配时为 null</returns>
    Task<AuthUserInfo> GetAuthUserInfo(AppEnvironmentEnum deviceType, string appNo, string tenantNo, string employeeNo,
        string sessionId);

    /// <summary>
    /// 为管理端用户创建独立会话，并更新当前授权上下文。
    /// </summary>
    /// <param name="authUserInfo">登录所需的账号、租户、职员与设备信息；方法会写入新会话 ID</param>
    /// <remarks>启用单点登录时，撤销同应用、租户和职员的其他设备会话。</remarks>
    Task Login(AuthUserInfo authUserInfo);

    /// <summary>
    /// 为客户端用户创建会话，并使用 client:用户ID 作为跨端主体标识。
    /// </summary>
    /// <param name="authUserInfo">客户端用户、租户与设备信息；方法会更新职员编号字段和会话 ID</param>
    Task ClientLogin(AuthUserInfo authUserInfo);

    /// <summary>
    /// 机器人登录
    /// </summary>
    /// <remarks>仅供调度作业使用，基于当前用户上下文生成有效期为 1 分钟的令牌。</remarks>
    /// <returns>机器人登录令牌</returns>
    Task<string> RobotLogin();

    /// <summary>
    /// 刷新授权信息
    /// </summary>
    /// <param name="input">待刷新的应用、租户、职员和权限信息</param>
    Task RefreshAuth(RefreshAuthDto input);

    /// <summary>
    /// 刷新账号信息
    /// </summary>
    /// <param name="input">待刷新的管理账号信息</param>
    Task RefreshAccount(RefreshAccountDto input);

    /// <summary>
    /// 刷新客户端用户信息
    /// </summary>
    /// <param name="input">待刷新的客户端用户信息</param>
    Task RefreshClientUser(RefreshClientUserDto input);

    /// <summary>
    /// 刷新职员信息
    /// </summary>
    /// <param name="input">待刷新的职员信息</param>
    Task RefreshEmployee(RefreshEmployeeDto input);

    /// <summary>
    /// 撤销账号在所有租户和设备上的会话
    /// </summary>
    /// <param name="accountId">需要撤销会话的管理账号 ID</param>
    Task RevokeAccount(long accountId);

    /// <summary>
    /// 撤销租户的全部会话
    /// </summary>
    /// <param name="tenantNo">租户编号；空值不执行操作</param>
    Task RevokeTenant(string tenantNo);

    /// <summary>
    /// 撤销指定租户职员的全部会话
    /// </summary>
    /// <param name="tenantNo">租户编号</param>
    /// <param name="employeeNo">职员编号；任一编号为空时不执行操作</param>
    Task RevokeEmployee(string tenantNo, string employeeNo);

    /// <summary>
    /// 撤销统一主体在三端的会话，不操作管理账号
    /// </summary>
    /// <param name="userId">客户端用户 ID；非正数不执行操作</param>
    Task RevokeClientUser(long userId);

    /// <summary>
    /// 统一退出登录
    /// </summary>
    Task Logout();
}
