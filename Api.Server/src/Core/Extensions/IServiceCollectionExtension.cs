// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Globalization;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fast.Core;

/// <summary>
/// <see cref="IServiceCollection"/> 扩展方法
/// </summary>
public static class IServiceCollectionExtension
{
    /// <summary>
    /// 添加托管服务
    /// </summary>
    /// <returns>用于继续链式配置的服务集合</returns>
    public static IServiceCollection AddHostedService(this IServiceCollection services)
    {
        Type IHostedServiceType = typeof(IHostedService);

        var hostedServiceTypes = MAppContext.EffectiveTypes
            .Where(wh => IHostedServiceType.IsAssignableFrom(wh) && wh.IsClass && !wh.IsInterface && !wh.IsAbstract)
            // 只自动注册项目自身的托管服务；第三方托管服务必须通过对应扩展方法显式启用，
            // 避免 QuartzHostedService 等服务在核心数据库初始化前启动并连接尚未创建的数据库。
            .Where(wh => wh.Assembly.GetName().Name?.StartsWith($"{nameof(Fast)}.", StringComparison.Ordinal) == true)
            .Select(sl => new {Type = sl, Order = sl.GetCustomAttribute<OrderAttribute>()?.Order ?? 0})
            .OrderBy(ob => ob.Order)
            .Select(sl => sl.Type)
            .ToList();

        MethodInfo addHostedService = typeof(ServiceCollectionHostedServiceExtensions).GetMethods()
            .Where(wh => wh.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService))
            .Where(wh => wh.IsGenericMethodDefinition)
            .First(wh => wh.GetParameters().Length == 1);

        foreach (Type hostedServiceType in hostedServiceTypes)
        {
            addHostedService.MakeGenericMethod(hostedServiceType).Invoke(null, [services]);
        }

        return services;
    }

    /// <summary>
    /// 注册 API 限流规则
    /// </summary>
    /// <returns>用于继续链式配置的服务集合</returns>
    public static IServiceCollection AddApiRateLimit(this IServiceCollection services)
    {
        // API 限流配置验证
        services.AddConfigurableOptions<ApiRateLimitSettingsOptions>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                string policyName = httpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
                ApiRateLimitSettingsOptions settings = httpContext.RequestServices
                    .GetRequiredService<IOptions<ApiRateLimitSettingsOptions>>()
                    .Value;
                // 仅使用经过可信代理中间件处理的连接地址，不能直接信任客户端伪造的转发头。
                string remoteIp = httpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
                return policyName switch
                {
                    CommonConst.GlobalApiRateLimit => RateLimitPartition.GetSlidingWindowLimiter($"global-ip:{remoteIp}",
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = settings.IpPermitLimit.GetValueOrDefault(60),
                            Window = TimeSpan.FromSeconds(settings.WindowSeconds.GetValueOrDefault(60)),
                            SegmentsPerWindow = Math.Min(6, settings.WindowSeconds.GetValueOrDefault(60)),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),
                    CommonConst.LoginApiRateLimit => RateLimitPartition.GetSlidingWindowLimiter($"login-ip:{remoteIp}",
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = settings.LoginIpPermitLimit.GetValueOrDefault(30),
                            Window = TimeSpan.FromSeconds(settings.WindowSeconds.GetValueOrDefault(60)),
                            SegmentsPerWindow = Math.Min(6, settings.WindowSeconds.GetValueOrDefault(60)),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),
                    _ => RateLimitPartition.GetNoLimiter("api-ip:not-applicable")
                };
            });
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = Math
                        .Ceiling(retryAfter.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);
                }

                return ValueTask.CompletedTask;
            };
            options.AddPolicy<string, GlobalApiRateLimiterPolicy>(CommonConst.GlobalApiRateLimit);
            options.AddPolicy<string, LoginApiRateLimiterPolicy>(CommonConst.LoginApiRateLimit);
        });

        return services;
    }
}
