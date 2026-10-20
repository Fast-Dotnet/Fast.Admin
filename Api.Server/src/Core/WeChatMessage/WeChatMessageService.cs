// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Logging;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// <see cref="IWeChatMessageService"/> 默认实现
/// </summary>
public class WeChatMessageService : IWeChatMessageService, ISingletonDependency
{
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化微信消息服务
    /// </summary>
    public WeChatMessageService(ILogger<IMailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendServiceAccountSubscribeMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageSubscribeBusinessSendRequest.Types.DataItem> data, string pageUrl = null,
        string miniProgramAppId = null, string pagePath = null)
    {
        ArgumentNullException.ThrowIfNull(appKey);
        ArgumentNullException.ThrowIfNull(appSecret);
        ArgumentNullException.ThrowIfNull(templateId);
        ArgumentNullException.ThrowIfNull(openId);
        ArgumentNullException.ThrowIfNull(data);

        DateTime sendTime = DateTime.Now;
        bool isSuccess = false;

        try
        {
            ApplicationTokenModel applicationTokenModel =
                await ApplicationTokenContext.GetApplicationToken(appKey, ApplicationTokenTypeEnum.AccessToken);

            WechatApiClient apiClient = WechatApiClientBuilder
                .Create(new WechatApiClientOptions {AppId = appKey, AppSecret = appSecret})
                .Build();

            CgibinMessageSubscribeBusinessSendResponse response = await apiClient.ExecuteCgibinMessageSubscribeBusinessSendAsync(
                new CgibinMessageSubscribeBusinessSendRequest
                {
                    AccessToken = applicationTokenModel.TokenValue,
                    TemplateId = templateId,
                    ToUserOpenId = openId,
                    Data = data,
                    Page = pageUrl,
                    MiniProgram = string.IsNullOrWhiteSpace(miniProgramAppId)
                        ? null
                        : new CgibinMessageSubscribeBusinessSendRequest.Types.MiniProgram
                        {
                            AppId = miniProgramAppId, PagePath = pagePath ?? string.Empty
                        }
                });

            // 43101：用户拒绝接受消息，如果用户之前曾经订阅过，则表示用户取消了订阅关系
            if (!response.IsSuccessful() && response.ErrorCode != 43101)
            {
                _logger.LogError($"微信服务号订阅通知发送失败：{response.ErrorCode}；{response.ErrorMessage}");
            }
            else
            {
                // 发送成功
                isSuccess = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"微信服务号订阅通知发送失败。\r\nAppKey：{appKey}\r\nTemplateId：{templateId}\r\nOpenId：{openId}");
            throw;
        }
        finally
        {
            // 写入消息发送记录
            try
            {
                // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

                var messageSendRecordModel = new MessageSendRecordModel
                {
                    Channel = MessageSendChannelEnum.WeChatServiceAccountSubscribe,
                    Receiver = openId,
                    Title = templateId,
                    RecordValue = data.ToJsonString(),
                    IsSuccess = isSuccess,
                    CreatedTime = sendTime
                };

                messageSendRecordModel.RecordCreate(FastContext.HttpContext);

                await db.Insertable(messageSendRecordModel).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                // 数据库异常可能包含SQL参数；只保留异常类型，且不能因此重发已发送的消息。
                _logger.LogError(ex, $"微信服务号订阅通知记录写入失败。OpenId：{openId}");
            }
        }
    }

    /// <inheritdoc />
    public async Task SendOfficialAccountTemplateMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem> data, string url = null,
        string miniProgramAppId = null, string pagePath = null)
    {
        ArgumentNullException.ThrowIfNull(appKey);
        ArgumentNullException.ThrowIfNull(appSecret);
        ArgumentNullException.ThrowIfNull(templateId);
        ArgumentNullException.ThrowIfNull(openId);
        ArgumentNullException.ThrowIfNull(data);

        DateTime sendTime = DateTime.Now;
        bool isSuccess = false;

        try
        {
            ApplicationTokenModel applicationTokenModel =
                await ApplicationTokenContext.GetApplicationToken(appKey, ApplicationTokenTypeEnum.AccessToken);

            WechatApiClient apiClient = WechatApiClientBuilder
                .Create(new WechatApiClientOptions {AppId = appKey, AppSecret = appSecret})
                .Build();

            CgibinMessageTemplateSendResponse response = await apiClient.ExecuteCgibinMessageTemplateSendAsync(
                new CgibinMessageTemplateSendRequest
                {
                    AccessToken = applicationTokenModel.TokenValue,
                    TemplateId = templateId,
                    ToUserOpenId = openId,
                    Data = data,
                    Url = url,
                    MiniProgram = string.IsNullOrWhiteSpace(miniProgramAppId)
                        ? null
                        : new CgibinMessageTemplateSendRequest.Types.MiniProgram
                        {
                            AppId = miniProgramAppId, PagePath = pagePath ?? string.Empty
                        }
                });

            // 43101：用户拒绝接受消息，如果用户之前曾经订阅过，则表示用户取消了订阅关系
            if (!response.IsSuccessful() && response.ErrorCode != 43101)
            {
                _logger.LogError($"微信公众号模板消息发送失败：{response.ErrorCode}；{response.ErrorMessage}");
            }
            else
            {
                // 发送成功
                isSuccess = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"微信公众号模板消息发送失败。\r\nAppKey：{appKey}\r\nTemplateId：{templateId}\r\nOpenId：{openId}");
            throw;
        }
        finally
        {
            // 写入消息发送记录
            try
            {
                // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

                var messageSendRecordModel = new MessageSendRecordModel
                {
                    Channel = MessageSendChannelEnum.WeChatOfficialAccountTemplate,
                    Receiver = openId,
                    Title = templateId,
                    RecordValue = data.ToJsonString(),
                    IsSuccess = isSuccess,
                    CreatedTime = sendTime
                };

                messageSendRecordModel.RecordCreate(FastContext.HttpContext);

                await db.Insertable(messageSendRecordModel).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                // 数据库异常可能包含SQL参数；只保留异常类型，且不能因此重发已发送的消息。
                _logger.LogError(ex, $"微信公众号模板消息记录写入失败。OpenId：{openId}");
            }
        }
    }

    /// <inheritdoc />
    public async Task SendMiniProgramSubscribeMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> data, string pagePath = null)
    {
        ArgumentNullException.ThrowIfNull(appKey);
        ArgumentNullException.ThrowIfNull(appSecret);
        ArgumentNullException.ThrowIfNull(templateId);
        ArgumentNullException.ThrowIfNull(openId);
        ArgumentNullException.ThrowIfNull(data);

        DateTime sendTime = DateTime.Now;
        bool isSuccess = false;
        try
        {
            ApplicationTokenModel applicationTokenModel =
                await ApplicationTokenContext.GetApplicationToken(appKey, ApplicationTokenTypeEnum.AccessToken);

            WechatApiClient apiClient = WechatApiClientBuilder
                .Create(new WechatApiClientOptions {AppId = appKey, AppSecret = appSecret})
                .Build();

            CgibinMessageSubscribeSendResponse response = await apiClient.ExecuteCgibinMessageSubscribeSendAsync(
                new CgibinMessageSubscribeSendRequest
                {
                    AccessToken = applicationTokenModel.TokenValue,
                    TemplateId = templateId,
                    ToUserOpenId = openId,
                    MiniProgramPagePath = pagePath,
                    Data = data
                });

            // 43101：用户拒绝接受消息，如果用户之前曾经订阅过，则表示用户取消了订阅关系
            if (!response.IsSuccessful() && response.ErrorCode != 43101)
            {
                _logger.LogError($"微信发送订阅消息失败：{response.ErrorCode}；{response.ErrorMessage}");
            }
            else
            {
                // 发送成功
                isSuccess = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"微信发送订阅消息失败。\r\nAppKey：{appKey}\r\nTemplateId：{templateId}\r\nOpenId：{openId}");
            throw;
        }
        finally
        {
            // 写入消息发送记录
            try
            {
                // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
                var messageSendRecordModel = new MessageSendRecordModel
                {
                    Channel = MessageSendChannelEnum.WeChatMiniProgramSubscribe,
                    Receiver = openId,
                    Title = templateId,
                    RecordValue = data?.ToJsonString(),
                    IsSuccess = isSuccess,
                    CreatedTime = sendTime
                };
                messageSendRecordModel.RecordCreate(FastContext.HttpContext);

                await db.Insertable(messageSendRecordModel).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                // 数据库异常可能包含SQL参数；只保留异常类型，且不能因此重发已发送的消息。
                _logger.LogError(ex, $"微信发送订阅消息记录写入失败。OpenId：{openId}");
            }
        }
    }
}
