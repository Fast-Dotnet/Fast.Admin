// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 应用标识上下文
/// </summary>
[SuppressSniffer]
public class ApplicationContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetService<ILogger<ApplicationContext>>();

    /// <summary>
    /// 获取应用
    /// </summary>
    /// <returns>应用信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static ApplicationClientModel GetApplicationSync(string appKey, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationClientModel)}.{appKey}", out object obj) == true
            && obj is ApplicationClientModel applicationClientModel)
        {
            return applicationClientModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, appKey);

        applicationClientModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            ApplicationClientModel result = repository.Queryable<ApplicationClientModel>()
                .Includes(e => e.Application)
                .Where(wh => wh.AppKey == appKey)
                .Single();

            if (result == null && throwError)
            {
                string message = $"未能找到对应应用【{appKey}】信息！";
                logger.LogError($"AppKey：{appKey}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationClientModel)}.{appKey}"] = applicationClientModel;
        }

        return applicationClientModel;
    }

    /// <summary>
    /// 获取应用
    /// </summary>
    /// <returns>应用信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<ApplicationClientModel> GetApplication(string appKey, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationClientModel)}.{appKey}", out object obj) == true
            && obj is ApplicationClientModel applicationClientModel)
        {
            return applicationClientModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, appKey);

        applicationClientModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            ApplicationClientModel result = await repository.Queryable<ApplicationClientModel>()
                .Includes(e => e.Application)
                .Where(wh => wh.AppKey == appKey)
                .SingleAsync();

            if (result == null && throwError)
            {
                string message = $"未能找到对应应用【{appKey}】信息！";
                logger.LogError($"AppKey：{appKey}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationClientModel)}.{appKey}"] = applicationClientModel;
        }

        return applicationClientModel;
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    public static async Task DeleteApplication(string appKey)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            httpContext.Items.Remove($"{nameof(Fast)}.{nameof(ApplicationClientModel)}.{appKey}");
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, appKey);

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 删除所有应用
    /// </summary>
    public static async Task DeleteAllApplication()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys.Where(wh =>
                    wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(ApplicationClientModel)}."))
                .ToList();
            foreach (object key in keys)
            {
                httpContext.Items.Remove(key);
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, "*");
        await centerCache.DelByPatternAsync(cacheKey);
    }
}
