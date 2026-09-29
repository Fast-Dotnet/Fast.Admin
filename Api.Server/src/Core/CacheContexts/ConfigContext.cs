// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fast.Core;

/// <summary>
/// 配置上下文
/// </summary>
[SuppressSniffer]
public class ConfigContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetService<ILogger<ConfigContext>>();

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns>配置值</returns>
    public static string GetConfigSync(string configCode)
    {
        if (string.IsNullOrWhiteSpace(configCode))
        {
            throw new UserFriendlyException("配置编码不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        string configValue = httpContext?.Items[$"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}"]?.ToString();

        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return configValue;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Config, configCode);

        ConfigModel configModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarRepository<ConfigModel> repository = FastContext.GetService<ISqlSugarRepository<ConfigModel>>();

            ConfigModel result = repository.Entities.Where(wh => wh.ConfigCode == configCode).Single();

            if (result == null)
            {
                string message = $"未能找到对应配置【{configCode}】信息！";
                logger.LogError($"ConfigCode：{configCode}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}"] = configModel.ConfigValue;
        }

        if (string.IsNullOrWhiteSpace(configModel.ConfigValue))
        {
            string message = $"配置【{configCode}】信息值为空！";
            logger.LogError($"ConfigCode：{configCode}；{message}");
            throw new UserFriendlyException(message);
        }

        return configModel.ConfigValue;
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns>配置值</returns>
    public static async Task<string> GetConfig(string configCode)
    {
        if (string.IsNullOrWhiteSpace(configCode))
        {
            throw new UserFriendlyException("配置编码不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        string configValue = httpContext?.Items[$"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}"]?.ToString();

        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return configValue;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Config, configCode);

        ConfigModel configModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarRepository<ConfigModel> repository = FastContext.GetService<ISqlSugarRepository<ConfigModel>>();

            ConfigModel result = await repository.Entities.Where(wh => wh.ConfigCode == configCode).SingleAsync();

            if (result == null)
            {
                string message = $"未能找到对应配置【{configCode}】信息！";
                logger.LogError($"ConfigCode：{configCode}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}"] = configModel.ConfigValue;
        }

        if (string.IsNullOrWhiteSpace(configModel.ConfigValue))
        {
            string message = $"配置【{configCode}】信息值为空！";
            logger.LogError($"ConfigCode：{configCode}；{message}");
            throw new UserFriendlyException(message);
        }

        return configModel.ConfigValue;
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    public static async Task DeleteConfig(string configCode)
    {
        if (string.IsNullOrWhiteSpace(configCode))
        {
            throw new UserFriendlyException("配置编码不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            if (httpContext.Items.ContainsKey($"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}"))
            {
                httpContext.Items.Remove($"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}.{configCode}");
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Config, configCode);

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 删除所有配置
    /// </summary>
    public static async Task DeleteAllConfig()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys
                .Where(wh => wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(ConfigModel.ConfigCode)}."))
                .ToList();
            foreach (object key in keys)
            {
                httpContext.Items.Remove(key);
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Config, "*");
        await centerCache.DelByPatternAsync(cacheKey);
    }
}
