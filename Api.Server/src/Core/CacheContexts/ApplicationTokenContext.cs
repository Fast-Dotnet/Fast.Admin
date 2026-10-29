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
/// 应用凭据上下文
/// </summary>
[SuppressSniffer]
public class ApplicationTokenContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetRequiredService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetRequiredService<ILogger<ApplicationTokenContext>>();

    /// <summary>
    /// 获取应用凭据
    /// </summary>
    /// <returns>凭据信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static ApplicationTokenModel GetApplicationTokenSync(string appKey, ApplicationTokenTypeEnum tokenType,
        bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        if (!Enum.IsDefined(tokenType))
        {
            throw new UserFriendlyException("应用凭据类型不正确！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationTokenModel)}.{(byte)tokenType}.{appKey}",
                out object obj)
            == true
            && obj is ApplicationTokenModel applicationTokenModel)
        {
            return applicationTokenModel;
        }

        ISqlSugarClient db = FastContext.GetRequiredService<ISqlSugarClient>();
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.ApplicationToken, appKey, tokenType.ToString());
        applicationTokenModel = centerCache.GetAndSet(cacheKey,
            () => db.Queryable<ApplicationTokenModel>().Where(wh => wh.AppKey == appKey && wh.TokenType == tokenType).Single());

        if (applicationTokenModel == null && throwError)
        {
            string message = $"应用【{appKey}】的凭据【{tokenType}】不存在！";
            logger.LogError($"应用凭据不可用，AppKey：{appKey}；TokenType：{tokenType}；{message}");
            throw new UserFriendlyException(message);
        }

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationTokenModel)}.{(byte)tokenType}.{appKey}"] =
                applicationTokenModel;
        }

        return applicationTokenModel;
    }

    /// <summary>
    /// 获取应用凭据
    /// </summary>
    /// <returns>凭据信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<ApplicationTokenModel> GetApplicationToken(string appKey, ApplicationTokenTypeEnum tokenType,
        bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        if (!Enum.IsDefined(tokenType))
        {
            throw new UserFriendlyException("应用凭据类型不正确！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(ApplicationTokenModel)}.{(byte)tokenType}.{appKey}",
                out object obj)
            == true
            && obj is ApplicationTokenModel applicationTokenModel)
        {
            return applicationTokenModel;
        }

        ISqlSugarClient db = FastContext.GetRequiredService<ISqlSugarClient>();
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.ApplicationToken, appKey, tokenType.ToString());
        applicationTokenModel = await centerCache.GetAndSetAsync(cacheKey,
            async () => await db.Queryable<ApplicationTokenModel>()
                .Where(wh => wh.AppKey == appKey && wh.TokenType == tokenType)
                .SingleAsync());

        if (applicationTokenModel == null && throwError)
        {
            string message = $"应用【{appKey}】的凭据【{tokenType}】不存在！";
            logger.LogError($"应用凭据不可用，AppKey：{appKey}；TokenType：{tokenType}；{message}");
            throw new UserFriendlyException(message);
        }

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ApplicationTokenModel)}.{(byte)tokenType}.{appKey}"] =
                applicationTokenModel;
        }

        return applicationTokenModel;
    }

    /// <summary>
    /// 删除应用凭据
    /// </summary>
    public static async Task DeleteApplicationToken(string appKey, ApplicationTokenTypeEnum tokenType)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            throw new UserFriendlyException("应用标识不能为空！");
        }

        if (!Enum.IsDefined(tokenType))
        {
            throw new UserFriendlyException("应用凭据类型不正确！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            httpContext.Items.Remove($"{nameof(Fast)}.{nameof(ApplicationTokenModel)}.{(byte)tokenType}.{appKey}");
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.ApplicationToken, appKey, tokenType.ToString());

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 删除所有应用凭据
    /// </summary>
    public static async Task DeleteAllApplicationToken()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys
                .Where(wh => wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(ApplicationTokenModel)}."))
                .ToList();
            foreach (object key in keys)
            {
                httpContext.Items.Remove(key);
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.ApplicationToken, "*", "*");
        await centerCache.DelByPatternAsync(cacheKey);
    }
}
