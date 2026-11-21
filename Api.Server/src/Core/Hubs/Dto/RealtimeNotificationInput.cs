// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 实时消息通知输入
/// </summary>
public sealed class RealtimeNotificationInput
{
    /// <summary>
    /// 消息类型：info、warning、error
    /// </summary>
    public string Type { get; set; } = "info";

    /// <summary>
    /// 通知内容，单条消息也通过集合传输
    /// </summary>
    public List<string> Messages { get; set; } = [];
}
