// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Microsoft.Extensions.Options;

namespace Fast.Core;

/// <summary>
/// 登录 API 限流规则
/// </summary>
internal sealed class LoginApiRateLimiterPolicy : ApiRateLimiterPolicy
{
    /// <summary>
    /// 登录 API 限流规则
    /// </summary>
    public LoginApiRateLimiterPolicy(IOptions<ApiRateLimitSettingsOptions> options) : base("login-client",
        options.Value.LoginPermitLimit.GetValueOrDefault(10), options.Value.WindowSeconds.GetValueOrDefault(60))
    {
    }
}
