// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Fast.Core;

/// <summary>
/// 微信消息服务
/// </summary>
public interface IWeChatMessageService
{
    /// <summary>
    /// 发送微信服务号订阅通知
    /// </summary>
    /// <param name="appKey">应用标识</param>
    /// <param name="appSecret">应用密钥</param>
    /// <param name="templateId">模板 ID</param>
    /// <param name="openId">接收用户的 OpenId</param>
    /// <param name="data">订阅通知模板数据，键为模板字段名，值为对应的字段内容</param>
    /// <param name="pageUrl">用户点击通知后跳转的网页地址，为空时不指定网页跳转</param>
    /// <param name="miniProgramAppId">跳转的小程序 AppId，为空时不指定小程序跳转</param>
    /// <param name="pagePath">跳转的小程序页面路径，为空时使用小程序默认页面</param>
    Task SendServiceAccountSubscribeMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageSubscribeBusinessSendRequest.Types.DataItem> data, string pageUrl = null,
        string miniProgramAppId = null, string pagePath = null);

    /// <summary>
    /// 发送微信公众号模板消息
    /// </summary>
    /// <param name="appKey">应用标识</param>
    /// <param name="appSecret">应用密钥</param>
    /// <param name="templateId">模板 ID</param>
    /// <param name="openId">接收用户的 OpenId</param>
    /// <param name="data">模板消息数据，键为模板字段名，值为对应的字段内容</param>
    /// <param name="url">用户点击消息后跳转的网页地址，为空时不指定网页跳转</param>
    /// <param name="miniProgramAppId">跳转的小程序 AppId，为空时不跳转小程序</param>
    /// <param name="pagePath">跳转的小程序页面路径，为空时使用小程序默认页面</param>
    Task SendOfficialAccountTemplateMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem> data, string url = null,
        string miniProgramAppId = null, string pagePath = null);

    /// <summary>
    /// 发送小程序订阅消息
    /// </summary>
    /// <param name="appKey">应用标识</param>
    /// <param name="appSecret">应用密钥</param>
    /// <param name="templateId">模板 ID</param>
    /// <param name="openId">接收用户的 OpenId</param>
    /// <param name="data">订阅消息模板数据，键为模板字段名，值为对应的字段内容</param>
    /// <param name="pagePath">用户点击订阅消息后跳转的小程序页面路径，为空时不指定跳转页面</param>
    Task SendMiniProgramSubscribeMessage(string appKey, string appSecret, string templateId, string openId,
        IDictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> data, string pagePath = null);
}
