// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 缓存常量
/// </summary>
[SuppressSniffer]
public static class CacheConst
{
    /// <summary>
    /// 获取缓存Key
    /// </summary>
    /// <returns>缓存键</returns>
    public static string GetCacheKey(string cacheKey, params object[] args)
    {
        return string.Format(cacheKey, args);
    }

    /// <summary>
    /// 授权用户
    /// </summary>
    /// <remarks>{0}应用编号，{1}租户编号，{2}登录环境，{3}工号，{4}会话Id</remarks>
    public const string AuthUser = "{0}:{1}:Auth:{2}:{3}:{4}";

    /// <summary>
    /// 租户登录凭证
    /// </summary>
    /// <remarks>{0}凭据Key</remarks>
    public const string TenantLoginTicket = "Login:TenantTicket:{0}";

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <remarks>{0}验证Key</remarks>
    public const string PasswordReset = "Login:PasswordReset:{0}";

    /// <summary>
    /// 账号身份验证
    /// </summary>
    /// <remarks>{0}账号Key，{1}客户端标识</remarks>
    public const string AccountIdentityVerification = "Account:IdentityVerification:{0}:{1}";

    /// <summary>
    /// 编辑账号联系方式验证
    /// </summary>
    /// <remarks>{0}账号Key，{1}客户端标识</remarks>
    public const string EditAccountVerification = "Account:EditVerification:{0}:{1}";

    /// <summary>
    /// 图片验证码
    /// </summary>
    /// <remarks>{0}验证码Key</remarks>
    public const string ImageCaptcha = "Captcha:{0}";

    /// <summary>
    /// 邮件
    /// </summary>
    /// <remarks>{0}类型，{1}邮箱</remarks>
    public const string Mail = "Mail:{0}:{1}";

    /// <summary>
    /// 短信
    /// </summary>
    /// <remarks>{0}类型，{1}手机号</remarks>
    public const string Sms = "SMS:{0}:{1}";

    /// <summary>
    /// 管理后台
    /// </summary>
    public static class Center
    {
        /// <summary>
        /// 数据库
        /// </summary>
        /// <remarks>{0}租户编号，{1}数据库名类型</remarks>
        public const string Database = "Database:{0}:{1}";

        /// <summary>
        /// 配置
        /// </summary>
        /// <remarks>{0}配置编码</remarks>
        public const string Config = "Config:{0}";

        /// <summary>
        /// 租户
        /// </summary>
        /// <remarks>{0}租户编号</remarks>
        public const string Tenant = "Tenant:{0}";

        /// <summary>
        /// 机器人
        /// </summary>
        /// <remarks>{0}租户编号</remarks>
        public const string Rabot = "Rabot:{0}";

        /// <summary>
        /// 应用
        /// </summary>
        /// <remarks>{0}应用标识</remarks>
        public const string App = "App:{0}";

        /// <summary>
        /// 应用凭据
        /// </summary>
        /// <remarks>{0}应用标识，{1}凭据类型</remarks>
        public const string ApplicationToken = "ApplicationToken:{0}:{1}";

        /// <summary>
        /// 消息模板
        /// </summary>
        /// <remarks>{0}租户，{1}应用标识，{2}模板类型</remarks>
        public const string MessageTemplate = "MessageTemplate:{0}:{1}:{2}";

        /// <summary>
        /// 商户号
        /// </summary>
        /// <remarks>{0}商户号</remarks>
        public const string Merchant = "Merchant:{0}";

        /// <summary>
        /// 字典
        /// </summary>
        public const string Dictionary = "Dictionary";

        /// <summary>
        /// 表格配置
        /// </summary>
        /// <remarks>{0}表格Key</remarks>
        public const string TableConfig = "TableConfig:{0}";

        /// <summary>
        /// 用户表格配置缓存
        /// </summary>
        /// <remarks>{0}表格Key，{1}租户编号, {2}工号</remarks>
        public const string UserTableConfigCache = "TableConfig:{0}:{1}:{2}";

        /// <summary>
        /// 地区
        /// </summary>
        public const string Region = "Region";

        /// <summary>
        /// 省份
        /// </summary>
        public const string Province = "Province";

        /// <summary>
        /// 城市
        /// </summary>
        public const string City = "City";
    }
}
