// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用凭据类型枚举
/// </summary>
[FastEnum("应用凭据类型枚举")]
public enum ApplicationTokenTypeEnum : byte
{
    /// <summary>
    /// 接口调用凭据
    /// </summary>
    [Description("AccessToken")]
    AccessToken = 1,

    /// <summary>
    /// JS-SDK 调用凭据
    /// </summary>
    [Description("JsApiTicket")]
    JsApiTicket = 2
}
