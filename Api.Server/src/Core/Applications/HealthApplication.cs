// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Diagnostics;
using System.Runtime.InteropServices;
using Fast.DynamicApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 健康检查
/// </summary>
[ApiDescriptionSettings(false)]
public class HealthApplication : IDynamicApplication
{
    /// <summary>
    /// SqlSugar 客户端
    /// </summary>
    private readonly ISqlSugarClient _repository;

    /// <summary>
    /// 分布式缓存
    /// </summary>
    private readonly IDistributedCache _distributedCache;

    /// <summary>
    /// 健康检查
    /// </summary>
    public HealthApplication(ISqlSugarClient repository, IDistributedCache distributedCache)
    {
        _repository = repository;
        _distributedCache = distributedCache;
    }

    /// <summary>
    /// 健康检查
    /// </summary>
    [HttpGet("/health")]
    [HttpGet("/health/index")]
    [ApiInfo("健康检查", HttpRequestActionEnum.Other)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseEncipher(false)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var databaseStopwatch = Stopwatch.StartNew();
        bool databaseHealthy = false;
        string databaseMessage = "数据库连接失败";
        try
        {
            _repository.Ado.CheckConnection();
            databaseHealthy = true;
            databaseMessage = "数据库连接正常";
        }
        catch
        {
            // 健康检查只返回组件状态，避免向匿名调用方暴露数据库异常详情
        }
        finally
        {
            databaseStopwatch.Stop();
        }

        var redisStopwatch = Stopwatch.StartNew();
        bool redisHealthy = false;
        string redisMessage = "分布式缓存读写失败";
        string cacheKey = $"Fast:Health:{Guid.NewGuid():N}";
        byte[] cacheValue = Guid.NewGuid().ToByteArray();
        try
        {
            await _distributedCache.SetAsync(cacheKey, cacheValue,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)}, cancellationToken);
            byte[] cachedValue = await _distributedCache.GetAsync(cacheKey, cancellationToken);
            redisHealthy = cachedValue != null && cachedValue.SequenceEqual(cacheValue);
            redisMessage = redisHealthy ? "分布式缓存读写正常" : "分布式缓存读写校验失败";
        }
        catch
        {
            // 健康检查只返回组件状态，避免向匿名调用方暴露 Redis 异常详情
        }
        finally
        {
            redisStopwatch.Stop();
            try
            {
                await _distributedCache.RemoveAsync(cacheKey, CancellationToken.None);
            }
            catch
            {
                // Redis 异常已体现在健康状态中，清理探针键失败时不覆盖原始结果
            }
        }

        bool isHealthy = databaseHealthy && redisHealthy;
        return new JsonResult(new
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            // 运行时版本
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            // 当前时间
            CurrentTime = DateTime.Now,
            // 运行时间
            RunTimes = MachineUtil.GetProgramRunTimes(),
            Checks = new
            {
                Database = new
                {
                    Status = databaseHealthy ? "Healthy" : "Unhealthy",
                    Message = databaseMessage,
                    Duration = databaseStopwatch.Elapsed.TotalMilliseconds
                },
                Redis = new
                {
                    Status = redisHealthy ? "Healthy" : "Unhealthy",
                    Message = redisMessage,
                    Duration = redisStopwatch.Elapsed.TotalMilliseconds
                }
            }
        }) {StatusCode = isHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable};
    }
}
