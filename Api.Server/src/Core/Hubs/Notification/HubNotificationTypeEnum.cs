// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.ComponentModel;

namespace Fast.Core;

/// <summary>
/// 实时消息通知类型枚举
/// </summary>
[FastEnum("实时消息通知类型枚举")]
public enum HubNotificationTypeEnum
{
    /// <summary>
    /// 信息
    /// </summary>
    [Description("信息")]
    Info = 1,

    /// <summary>
    /// 警告
    /// </summary>
    [Description("警告")]
    Warning = 2,

    /// <summary>
    /// 错误
    /// </summary>
    [Description("错误")]
    Error = 4
}
