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
/// 应用上下文
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
    public static ApplicationOpenIdModel GetApplicationSync(string openId, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}", out object obj)
            == true
            && obj is ApplicationOpenIdModel applicationOpenIdModel)
        {
            return applicationOpenIdModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, openId);

        applicationOpenIdModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            ApplicationOpenIdModel result = repository.Queryable<ApplicationOpenIdModel>()
                .Includes(e => e.Application)
                .Where(wh => wh.OpenId == openId)
                .Single();

            if (result == null && throwError)
            {
                string message = $"未能找到对应应用【{openId}】信息！";
                logger.LogError($"OpenId：{openId}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}"] = applicationOpenIdModel;
        }

        return applicationOpenIdModel;
    }

    /// <summary>
    /// 获取应用
    /// </summary>
    /// <returns>应用信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<ApplicationOpenIdModel> GetApplication(string openId, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}", out object obj)
            == true
            && obj is ApplicationOpenIdModel applicationOpenIdModel)
        {
            return applicationOpenIdModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, openId);

        applicationOpenIdModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            ApplicationOpenIdModel result = await repository.Queryable<ApplicationOpenIdModel>()
                .Includes(e => e.Application)
                .Where(wh => wh.OpenId == openId)
                .SingleAsync();

            if (result == null && throwError)
            {
                string message = $"未能找到对应应用【{openId}】信息！";
                logger.LogError($"OpenId：{openId}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}"] = applicationOpenIdModel;
        }

        return applicationOpenIdModel;
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    public static async Task DeleteApplication(string openId)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            if (httpContext.Items.ContainsKey($"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}"))
            {
                httpContext.Items.Remove($"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}.{openId}");
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.App, openId);

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
                    wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(ApplicationOpenIdModel.OpenId)}."))
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
