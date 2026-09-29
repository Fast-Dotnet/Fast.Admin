// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Fast.Core;

/// <summary>
/// 全局 API 限流规则
/// </summary>
internal sealed class GlobalApiRateLimiterPolicy : ApiRateLimiterPolicy
{
    /// <summary>
    /// 全局 API 限流规则
    /// </summary>
    public GlobalApiRateLimiterPolicy(IOptions<ApiRateLimitSettingsOptions> options) : base("global-client",
        options.Value.PermitLimit.GetValueOrDefault(120), options.Value.WindowSeconds.GetValueOrDefault(60))
    {
    }

    /// <inheritdoc />
    protected override string GetPartitionKey(HttpContext httpContext)
    {
        string sessionId = GetSessionId(httpContext);
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            // 已登录请求按服务端生成的会话分区，避免依赖客户端可修改的设备Id
            return $"{PartitionPrefix}:session:{GetFingerprint(sessionId)}";
        }

        return base.GetPartitionKey(httpContext);
    }

    /// <summary>
    /// 从已通过 JWT 签名验证的授权信息中获取会话Id
    /// </summary>
    private static string GetSessionId(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        string data = httpContext.User.FindFirst("Data")?.Value;
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        try
        {
            Dictionary<string, string> payload = data.Base64ToString().ToObject<Dictionary<string, string>>();
            return payload != null && payload.TryGetValue(nameof(AuthUserInfo.SessionId), out string sessionId)
                ? sessionId?.Trim()
                : null;
        }
        catch
        {
            // 无法解析的授权信息交由后续授权处理器拒绝，此处回退到匿名分区
            return null;
        }
    }
}
