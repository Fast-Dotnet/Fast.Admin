// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CSRedis;
using Fast.Center.Domain;
using Fast.SqlSugar;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// <see cref="IMailService"/> 默认实现
/// </summary>
public class MailService : IMailService, ISingletonDependency
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache _cache;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化邮件服务
    /// </summary>
    public MailService(ICache cache, ILogger<IMailService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GetEmailTemplate(string title, string msg, string type = null, string displayName = null)
    {
        (string accentColor, string badgeBackgroundColor, string badgeText) = type?.Trim().ToLowerInvariant() switch
        {
            "warn" => ("#d97706", "#fff7ed", "重要提醒"),
            "error" => ("#dc2626", "#fef2f2", "异常通知"),
            _ => ("#2563eb", "#eff6ff", "系统通知")
        };

        displayName ??= await ConfigContext.GetConfig(ConfigConst.MailDisplayName);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = "FastDotnet";
        }

        string encodedTitle = WebUtility.HtmlEncode(title);
        string encodedDisplayName = WebUtility.HtmlEncode(displayName);
        DateTime sendTime = DateTime.Now;

        return $$"""
                 <!doctype html>
                 <html lang="zh-CN">
                 <head>
                 	<meta charset="utf-8" />
                 	<meta name="viewport" content="width=device-width, initial-scale=1" />
                 	<meta name="color-scheme" content="light only" />
                 	<title>{{encodedTitle}}</title>
                 	<style>
                 		body, table, td, p { margin: 0; padding: 0; }
                 		.mail-content p { margin: 0 0 14px; }
                 		.mail-content p:last-child { margin-bottom: 0; }
                 		.mail-content pre {
                 			margin: 16px 0 0;
                 			padding: 16px;
                 			overflow-wrap: anywhere;
                 			white-space: pre-wrap;
                 			word-break: break-word;
                 			border: 1px solid #e2e8f0;
                 			border-radius: 8px;
                 			color: #475569;
                 			background-color: #f8fafc;
                 			font:
                 				12px/1.6 Consolas,
                 				Monaco,
                 				monospace;
                 		}
                 		.mail-content .warn { color: #d97706 !important; font-weight: 700 !important; }
                 		.mail-content .error { color: #dc2626 !important; font-weight: 700 !important; }
                 		@media only screen and (max-width: 680px) {
                 			.mail-shell { padding: 24px 12px !important; }
                 			.mail-card-content { padding: 28px 22px !important; }
                 		}
                 	</style>
                 </head>
                 <body style="width: 100%; margin: 0; padding: 0; color: #334155; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Arial, sans-serif;">
                 	<div style="display: none; max-height: 0; overflow: hidden; opacity: 0; color: transparent">
                 		{{encodedTitle}} · {{encodedDisplayName}}
                 	</div>
                 	<table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="width: 100%; margin: 0 auto; background-color: #f1f5f9">
                 		<tr>
                 			<td class="mail-shell" align="center" style="padding: 40px 16px">
                 				<table role="presentation" align="center" width="640" cellpadding="0" cellspacing="0" style="width: 100%; max-width: 640px; margin: 0 auto">
                 					<tr>
                 						<td align="center" style="padding: 0 4px 20px">
                 							<table role="presentation" align="center" cellpadding="0" cellspacing="0" style="margin: 0 auto">
                 								<tr>
                 									<td width="58" height="58" align="center" style="width: 58px; height: 58px">
                 										<img src="https://cdn.fastdotnet.com/logo/fast/logo.png" alt="{{encodedDisplayName}}" width="58" height="58" style="display: block; width: 58px; height: 58px; border: 0; outline: none; text-decoration: none" />
                 									</td>
                 									<td style="padding-left: 12px; color: #0f172a; font-size: 24px; font-weight: 700; letter-spacing: -0.2px">
                 										{{encodedDisplayName}}
                 									</td>
                 								</tr>
                 							</table>
                 						</td>
                 					</tr>
                 					<tr>
                 						<td style="border: 1px solid #e2e8f0; border-radius: 16px;background-color: #ffffff; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);overflow: hidden;">
                 							<table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                 								<tr>
                 									<td height="5" style="height: 5px; background-color:{{accentColor}}; font-size:0; line-height:0;">
                 										&nbsp;
                 									</td>
                 								</tr>
                 								<tr>
                 									<td class="mail-card-content" style="padding: 36px 42px 32px">
                 										<table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                 											<tr>
                 												<td>
                 													<span style="display: inline-block; padding: 5px 10px; border-radius: 999px; color: {{accentColor}}; background-color: {{badgeBackgroundColor}}; font-size: 12px; font-weight: 700; line-height: 18px;">
                 														{{badgeText}}
                 													</span>
                 												</td>
                 											</tr>
                 											<tr>
                 												<td style="padding: 16px 0 22px; color: #0f172a; font-size: 24px; font-weight: 750; line-height: 1.4;">
                 													{{encodedTitle}}
                 												</td>
                 											</tr>
                 											<tr>
                 												<td class="mail-content" style="padding-top: 22px; border-top: 1px solid #e2e8f0; color: #475569; font-size: 15px; line-height: 1.75;">
                 													{{msg}}
                 												</td>
                 											</tr>
                 										</table>
                 									</td>
                 								</tr>
                 								<tr>
                 									<td style="padding: 18px 42px; border-top: 1px solid #e2e8f0; color: #94a3b8; background-color: #f8fafc; font-size: 12px; line-height: 1.7; ">
                 										If you have any questions, please contact the administrator.<br />
                 										This email was sent automatically. Please do not reply.<br />
                 										发送时间：{{sendTime:yyyy-MM-dd HH:mm:ss}}
                 									</td>
                 								</tr>
                 							</table>
                 						</td>
                 					</tr>
                 					<tr>
                 						<td align="center" style="padding: 22px 16px 0; color: #94a3b8; font-size: 12px; line-height: 1.7">
                 							<a href="https://fastdotnet.com" target="_blank" style="color: #64748b; text-decoration: none">
                 								{{encodedDisplayName}}
                 							</a>
                 							<br />
                            Copyright © 2018 ~ {{sendTime:yyyy}} Fast All rights reserved.
                 						</td>
                 					</tr>
                 				</table>
                 			</td>
                 		</tr>
                 	</table>
                 </body>
                 </html>

                 """;
    }

    /// <summary>
    /// 验证码缓存Dto
    /// </summary>
    private class VerificationCodeCacheDto
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientIdentity { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        /// <remarks>超过5次自动失效</remarks>
        public int ErrorCount { get; set; }
    }

    /// <inheritdoc />
    public async Task<int> GetVerificationCodeRetryAfterSeconds(MailTypeEnum mailType, string email)
    {
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Mail, mailType.ToString(), email.Trim().ToLowerInvariant());
        return (int)Math.Max(0, await _cache.Client.TtlAsync($"{cacheKey}:SendCooldown"));
    }

    /// <inheritdoc />
    public async Task SendVerificationCode(MailTypeEnum mailType, string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, RegexConst.EmailAddress))
        {
            throw new UserFriendlyException("邮箱地址不正确！");
        }

        email = email.Trim().ToLowerInvariant();

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Mail, mailType.ToString(), email);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 120);
        if (codeLock == null)
        {
            // 仅抢锁失败时读取锁剩余时间，毫秒向上取整，避免不足1秒被显示为0。
            long lockMilliseconds = await _cache.Client.PTtlAsync($"CSRedisClientLock:{cacheKey}:Lock");
            int lockSeconds = (int)Math.Ceiling(lockMilliseconds / 1000d);
            throw new UserFriendlyException(lockSeconds > 0
                ? $"操作过于频繁，请在 {TimeSpan.FromSeconds(lockSeconds).ToDescription()} 后重试！"
                : "操作过于频繁，请稍后重试！");
        }

        int retryAfterSeconds = await GetVerificationCodeRetryAfterSeconds(mailType, email);
        if (retryAfterSeconds > 0)
        {
            throw new UserFriendlyException($"操作过于频繁，请在 {TimeSpan.FromSeconds(retryAfterSeconds).ToDescription()} 后重试！");
        }

        // 发送前占用冷却，失败时保留重试限制，成功后重新计时。
        await _cache.Client.SetAsync($"{cacheKey}:SendCooldown", "1", 60);
        VerificationCodeCacheDto dto = await _cache.GetAsync<VerificationCodeCacheDto>(cacheKey);

        // 生成验证码
        dto ??= new VerificationCodeCacheDto();
        dto.VerificationCode = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        dto.ClientIdentity = GlobalContext.ClientIdentity;
        dto.SendTime = DateTime.Now;
        dto.ErrorCount = 0;

        string mailTypeDescription = mailType.GetDescription();
        string title = $"【{mailTypeDescription}】邮箱验证码";
        string content = $$"""
                           <p>您好：</p>
                           <p>您正在进行<strong>{{mailTypeDescription}}</strong>操作，请使用以下验证码完成身份校验。</p>
                           <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin: 24px 0;">
                             <tr>
                               <td align="center" style="padding: 22px 16px; border: 1px solid #bfdbfe; border-radius: 10px; color: #1d4ed8; background-color: #eff6ff; font-size: 32px; font-weight: 700; letter-spacing: 10px; line-height: 1;">
                                 {{WebUtility.HtmlEncode(dto.VerificationCode)}}
                               </td>
                             </tr>
                           </table>
                           <p style="color: #64748b;">验证码 5 分钟内有效，请勿向任何人泄露。如非本人操作，请忽略此邮件。</p>
                           """;

        await SendEmail(title, await GetEmailTemplate(title, content), [email]);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        await _cache.Client.SetAsync($"{cacheKey}:SendCooldown", "1", 60);
    }

    /// <inheritdoc />
    public async Task VerifyVerificationCode(MailTypeEnum mailType, string email, string verificationCode)
    {
        ArgumentNullException.ThrowIfNull(verificationCode);

        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, RegexConst.EmailAddress))
        {
            throw new UserFriendlyException("邮箱地址不正确！");
        }

        email = email.Trim().ToLowerInvariant();

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Mail, mailType.ToString(), email);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 30);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        VerificationCodeCacheDto dto = await _cache.GetAsync<VerificationCodeCacheDto>(cacheKey);
        if (dto is not {ErrorCount: < 5}
            || dto.ClientIdentity != GlobalContext.ClientIdentity
            || dto.SendTime.AddMinutes(5) <= DateTime.Now)
        {
            throw new UserFriendlyException("邮件验证码无效或已过期！");
        }

        if (!string.Equals(dto.VerificationCode, verificationCode, StringComparison.Ordinal))
        {
            dto.ErrorCount++;
            // 错误次数达到上限后删除验证码
            if (dto.ErrorCount >= 5)
            {
                await _cache.DelAsync(cacheKey);
            }
            else
            {
                // 更新错误次数并保持原有过期时间
                await _cache.SetAsync(cacheKey, dto, dto.SendTime.AddMinutes(5) - DateTime.Now);
            }

            throw new UserFriendlyException("邮件验证码无效或已过期！");
        }

        await _cache.DelAsync(cacheKey);
    }

    /// <inheritdoc />
    public async Task SendEmail(string title, string content, List<string> receiveEmails = null, string smtp = null,
        int? port = null, string email = null, string authCode = null, string displayName = null)
    {
        await SendEmail(title, new BodyBuilder {HtmlBody = content}, receiveEmails, smtp, port, email, authCode, displayName);
    }

    /// <inheritdoc />
    public async Task SendEmail(string title, BodyBuilder content, List<string> receiveEmails = null, string smtp = null,
        int? port = null, string email = null, string authCode = null, string displayName = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (receiveEmails == null)
        {
            string mailReceiveEmails = await ConfigContext.GetConfig(ConfigConst.MailReceiveEmails);
            receiveEmails = mailReceiveEmails.ToObject<List<string>>();
        }

        if (receiveEmails is not {Count: > 0})
        {
            return;
        }

        DateTime sendTime = DateTime.Now;
        bool isSuccess = false;
        try
        {
            smtp ??= await ConfigContext.GetConfig(ConfigConst.MailSmtp);
            email ??= await ConfigContext.GetConfig(ConfigConst.MailEmail);
            authCode ??= await ConfigContext.GetConfig(ConfigConst.MailAuthCode);
            displayName ??= await ConfigContext.GetConfig(ConfigConst.MailDisplayName);

            if (string.IsNullOrWhiteSpace(smtp))
            {
                throw new UserFriendlyException("发件服务器地址为空！");
            }

            if (port is null or <= 0)
            {
                string portValue = await ConfigContext.GetConfig(ConfigConst.MailPort);
                if (!int.TryParse(portValue, out int _port) || _port <= 0)
                {
                    throw new ArgumentException("发件服务器端口不正确！");
                }

                port = _port;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("发件邮箱为空！");
            }

            if (string.IsNullOrWhiteSpace(authCode))
            {
                throw new ArgumentException("发件邮箱授权码为空！");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "FastDotnet";
            }

            // 创建邮件内容
            var message = new MimeMessage();

            // 发件人
            message.From.Add(new MailboxAddress(displayName, email));

            // 收件人
            foreach (string receiveEmail in receiveEmails)
            {
                message.To.Add(new MailboxAddress(null, receiveEmail));
            }

            // 标题
            message.Subject = title;

            // 正文
            message.Body = content.ToMessageBody();

            // 配置 Smtp 客户端
            using var smtpClient = new SmtpClient();
            // 设置超时时间为15秒
            smtpClient.Timeout = 15000;
            // 连接发件邮箱服务器
            await smtpClient.ConnectAsync(smtp, port.Value, SecureSocketOptions.SslOnConnect);
            // 登录邮箱
            await smtpClient.AuthenticateAsync(email, authCode);
            // 发送邮件
            await smtpClient.SendAsync(message);
            // 关闭连接
            await smtpClient.DisconnectAsync(true);
            // 发送成功
            isSuccess = true;
        }
        catch (SmtpCommandException ex) when (ex.ErrorCode == SmtpErrorCode.RecipientNotAccepted
                                              && ex.StatusCode == SmtpStatusCode.MailboxUnavailable)
        {
            throw new UserFriendlyException("收件邮箱不存在或不可用，请检查邮箱地址！");
        }
        catch (SmtpProtocolException)
        {
            throw new UserFriendlyException("邮件服务器通信异常，请稍后重试！");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"邮件发送失败。\r\nReceiveEmails：{string.Join(",", receiveEmails)}\r\nTitle：{title}\r\nContent：{content}");
            throw;
        }
        finally
        {
            // 写入消息发送记录
            try
            {
                // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
                var messageSendRecordList = receiveEmails.Select(receiver => new MessageSendRecordModel
                    {
                        Channel = MessageSendChannelEnum.Email,
                        Receiver = receiver,
                        Title = title,
                        RecordValue = content.HtmlBody ?? content.TextBody ?? string.Empty,
                        IsSuccess = isSuccess,
                        CreatedTime = sendTime
                    })
                    .ToList();
                HttpContext httpContext = FastContext.HttpContext;
                foreach (MessageSendRecordModel item in messageSendRecordList)
                {
                    item.RecordCreate(httpContext);
                }

                await db.Insertable(messageSendRecordList).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                // 数据库异常可能包含SQL参数；只保留异常类型，且不能因此重发已发送的消息。
                _logger.LogError(ex, $"邮件发送记录写入失败。ReceiveEmails：{string.Join(",", receiveEmails)}");
            }
        }
    }
}
