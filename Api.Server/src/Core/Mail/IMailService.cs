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
    /// <param name="displayName">发件人显示名称，为 <see langword="null"/> 时读取配置</param>
    /// <returns>公用邮件模板</returns>
    Task<string> GetEmailTemplate(string title, string msg, string type = null, string displayName = null);

    /// <summary>
    /// 获取指定接收方和业务类型的验证码发送冷却时间。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="email">邮箱</param>
    /// <returns>剩余等待秒数；没有有效冷却记录时为 0</returns>
    Task<int> GetVerificationCodeRetryAfterSeconds(MessageTemplateTypeEnum templateType, string email);

    /// <summary>
    /// 发送与当前客户端绑定的验证码，并设置发送冷却。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="email">邮箱</param>
    /// <returns>验证码发送结果</returns>
    /// <remarks>验证码有效期为 5 分钟，发送冷却为 60 秒；发送失败也会保留本次冷却限制。</remarks>
    /// <exception cref="UserFriendlyException">接收地址不合法、仍在冷却期或发送配置不可用时抛出。</exception>
    Task<bool> SendVerificationCode(MessageTemplateTypeEnum templateType, string email);

    /// <summary>
    /// 校验验证码，并在校验成功后删除对应记录。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="email">邮箱</param>
    /// <param name="verificationCode">用户输入的验证码</param>
    /// <remarks>校验绑定当前客户端；累计错误达到 5 次时验证码失效，未达到上限的失败不会延长有效期。</remarks>
    /// <exception cref="ArgumentNullException">验证码为 <see langword="null"/> 时抛出。</exception>
    /// <exception cref="UserFriendlyException">接收地址不合法、验证码错误或失效，或并发校验未取得锁时抛出。</exception>
    Task VerifyVerificationCode(MessageTemplateTypeEnum templateType, string email, string verificationCode);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="title">邮件标题</param>
    /// <param name="content">邮件正文</param>
    /// <param name="receiveEmails">收件邮箱，为 <see langword="null"/> 时使用默认收件人</param>
    /// <param name="smtp">发件服务器地址，为 <see langword="null"/> 时读取配置</param>
    /// <param name="port">发件服务器端口，为 <see langword="null"/> 时读取配置</param>
    /// <param name="email">发件邮箱，为 <see langword="null"/> 时读取配置</param>
    /// <param name="authCode">发件邮箱授权码，为 <see langword="null"/> 时读取配置</param>
    /// <param name="displayName">发件人显示名称，为 <see langword="null"/> 时读取配置</param>
    /// <returns>邮件发送结果；没有收件人时为 <see langword="false"/></returns>
    /// <remarks>参数校验或发送失败时抛出异常。</remarks>
    Task<bool> SendEmail(string title, string content, List<string> receiveEmails = null, string smtp = null, int? port = null,
        string email = null, string authCode = null, string displayName = null);


    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="title">邮件标题</param>
    /// <param name="content">邮件正文</param>
    /// <param name="receiveEmails">收件邮箱，为 <see langword="null"/> 时使用默认收件人</param>
    /// <param name="smtp">发件服务器地址，为 <see langword="null"/> 时读取配置</param>
    /// <param name="port">发件服务器端口，为 <see langword="null"/> 时读取配置</param>
    /// <param name="email">发件邮箱，为 <see langword="null"/> 时读取配置</param>
    /// <param name="authCode">发件邮箱授权码，为 <see langword="null"/> 时读取配置</param>
    /// <param name="displayName">发件人显示名称，为 <see langword="null"/> 时读取配置</param>
    /// <returns>邮件发送结果；没有收件人时为 <see langword="false"/></returns>
    /// <remarks>参数校验或发送失败时抛出异常。</remarks>
    Task<bool> SendEmail(string title, BodyBuilder content, List<string> receiveEmails = null, string smtp = null,
        int? port = null, string email = null, string authCode = null, string displayName = null);
}
