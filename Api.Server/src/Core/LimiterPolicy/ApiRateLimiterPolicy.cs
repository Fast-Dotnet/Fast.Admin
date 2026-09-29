// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Fast.Core;

/// <summary>
/// API 限流规则基类
/// </summary>
internal abstract class ApiRateLimiterPolicy : IRateLimiterPolicy<string>
{
    /// <summary>
    /// 限流分区前缀
    /// </summary>
    protected string PartitionPrefix { get; }

    private readonly int _permitLimit;
    private readonly int _windowSeconds;

    /// <summary>
    /// API 限流规则基类
    /// </summary>
    /// <param name="partitionPrefix">分区前缀</param>
    /// <param name="permitLimit">单个限流分区的请求限额</param>
    /// <param name="windowSeconds">统计窗口秒数</param>
    protected ApiRateLimiterPolicy(string partitionPrefix, int permitLimit, int windowSeconds)
    {
        PartitionPrefix = partitionPrefix;
        _permitLimit = permitLimit;
        _windowSeconds = windowSeconds;
    }

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        string partitionKey = GetPartitionKey(httpContext);
        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = _permitLimit,
                Window = TimeSpan.FromSeconds(_windowSeconds),
                SegmentsPerWindow = Math.Min(6, _windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    /// <summary>
    /// 获取限流分区键
    /// </summary>
    /// <remarks>默认按照 Ip 与设备Id组合分区，供登录和未登录请求使用。</remarks>
    protected virtual string GetPartitionKey(HttpContext httpContext)
    {
        // 转发头必须先经过可信代理中间件处理，不能直接用任意X-Forwarded-For绕过限流。
        string ipAddress = httpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
        string deviceId = httpContext.Request.Headers[HttpHeaderConst.DeviceId].ToString().UrlDecode().Trim();

        // 未提供设备Id时空字符串会生成固定摘要，确保匿名请求仍受组合限流约束；
        // 请求头由客户端控制，使用固定长度摘要作为分区键，避免超长设备Id持续占用内存
        return $"{PartitionPrefix}:ip:{ipAddress}:device:{GetFingerprint(deviceId)}";
    }

    /// <summary>
    /// 获取固定长度的限流分区指纹
    /// </summary>
    protected static string GetFingerprint(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => null;
}
