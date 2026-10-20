// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Shared;

/// <summary>
/// 消息发送渠道枚举
/// </summary>
[FastEnum("消息发送渠道枚举")]
public enum MessageSendChannelEnum : byte
{
    /// <summary>
    /// 邮件
    /// </summary>
    [Description("邮件")]
    Email = 1,

    /// <summary>
    /// 短信
    /// </summary>
    [Description("短信")]
    Sms = 2,

    /// <summary>
    /// 微信服务号订阅消息
    /// </summary>
    [Description("微信服务号订阅消息")]
    WeChatServiceAccountSubscribe = 3,

    /// <summary>
    /// 微信公众号模板消息
    /// </summary>
    [Description("微信公众号模板消息")]
    WeChatOfficialAccountTemplate = 4,

    /// <summary>
    /// 微信小程序订阅消息
    /// </summary>
    [Description("微信小程序订阅消息")]
    WeChatMiniProgramSubscribe = 5
}
