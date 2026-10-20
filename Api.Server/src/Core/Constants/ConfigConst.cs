// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 配置常量
/// </summary>
[SuppressSniffer]
public static class ConfigConst
{
    /// <summary>
    /// 单租户自动登录
    /// </summary>
    public const string SingleTenantWhenAutoLogin = "SINGLE_TENANT_WHEN_AUTO_LOGIN";

    /// <summary>
    /// 单点登录
    /// </summary>
    public const string SingleLogin = "SINGLE_LOGIN";

    /// <summary>
    /// 登录验证码开关
    /// </summary>
    public const string LoginCaptchaOpen = "LOGIN_CAPTCHA_OPEN";

    /// <summary>
    /// 登录后身份验证开关
    /// </summary>
    public const string LoginIdentityVerificationOpen = "LOGIN_IDENTITY_VERIFICATION_OPEN";

    /// <summary>
    /// 邮件服务器地址
    /// </summary>
    public const string MailSmtp = "MAIL_SMTP";

    /// <summary>
    /// 邮件服务器端口
    /// </summary>
    public const string MailPort = "MAIL_PORT";

    /// <summary>
    /// 发件邮箱
    /// </summary>
    public const string MailEmail = "MAIL_EMAIL";

    /// <summary>
    /// 邮件授权码
    /// </summary>
    public const string MailAuthCode = "MAIL_AUTH_CODE";

    /// <summary>
    /// 发件人名称
    /// </summary>
    public const string MailDisplayName = "MAIL_DISPLAY_NAME";

    /// <summary>
    /// 默认收件邮箱
    /// </summary>
    /// <remarks>配置值使用 JSON 数组格式。["", ""]</remarks>
    public const string MailReceiveEmails = "MAIL_RECEIVE_EMAILS";

    /// <summary>
    /// 阿里云 AccessKey ID
    /// </summary>
    public const string AliyunAccessKeyID = "ALIYUN_ACCESS_KEY_ID";

    /// <summary>
    /// 阿里云 AccessKey Secret
    /// </summary>
    public const string AliyunAccessKeySecret = "ALIYUN_ACCESS_KEY_SECRET";

    /// <summary>
    /// 高德地图Key
    /// </summary>
    public const string GaoDeMapKey = "GAO_DE_MAP_KEY";
}
