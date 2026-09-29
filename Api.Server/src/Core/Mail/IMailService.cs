// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using MimeKit;

namespace Fast.Core;

/// <summary>
/// 邮件服务
/// </summary>
public interface IMailService
{
    /// <summary>
    /// 获取公用邮件模板
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="msg">消息正文</param>
    /// <param name="type">
    /// <para>info</para>
    /// <para>warn</para>
    /// <para>error</para>
    /// </param>
    /// <param name="displayName">发件人显示名称，为null时读取配置</param>
    /// <returns>公用邮件模板</returns>
    Task<string> GetEmailTemplate(string title, string msg, string type = null, string displayName = null);

    /// <summary>
    /// 获取当前验证码发送的剩余等待秒数
    /// </summary>
    /// <param name="mailType">邮件类型</param>
    /// <param name="email">邮箱</param>
    Task<int> GetVerificationCodeRetryAfterSeconds(MailTypeEnum mailType, string email);

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="mailType">邮件类型</param>
    /// <param name="email">邮箱</param>
    Task SendVerificationCode(MailTypeEnum mailType, string email);

    /// <summary>
    /// 验证并一次性消费验证码
    /// </summary>
    /// <param name="mailType">邮件类型</param>
    /// <param name="email">邮箱</param>
    /// <param name="verificationCode">验证码</param>
    Task VerifyVerificationCode(MailTypeEnum mailType, string email, string verificationCode);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="title">邮件标题</param>
    /// <param name="content">邮件正文</param>
    /// <param name="receiveEmails">收件邮箱，为null时使用默认收件人</param>
    /// <param name="smtp">发件服务器地址，为null时读取配置</param>
    /// <param name="port">发件服务器端口，为null时读取配置</param>
    /// <param name="email">发件邮箱，为null时读取配置</param>
    /// <param name="authCode">发件邮箱授权码，为null时读取配置</param>
    /// <param name="displayName">发件人显示名称，为null时读取配置</param>
    Task SendEmail(string title, string content, List<string> receiveEmails = null, string smtp = null, int? port = null,
        string email = null, string authCode = null, string displayName = null);


    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="title">邮件标题</param>
    /// <param name="content">邮件正文</param>
    /// <param name="receiveEmails">收件邮箱，为null时使用默认收件人</param>
    /// <param name="smtp">发件服务器地址，为null时读取配置</param>
    /// <param name="port">发件服务器端口，为null时读取配置</param>
    /// <param name="email">发件邮箱，为null时读取配置</param>
    /// <param name="authCode">发件邮箱授权码，为null时读取配置</param>
    /// <param name="displayName">发件人显示名称，为null时读取配置</param>
    Task SendEmail(string title, BodyBuilder content, List<string> receiveEmails = null, string smtp = null, int? port = null,
        string email = null, string authCode = null, string displayName = null);
}
