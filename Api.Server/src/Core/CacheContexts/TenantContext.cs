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
/// 租户上下文
/// </summary>
[SuppressSniffer]
public class TenantContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetService<ILogger<TenantContext>>();

    /// <summary>
    /// 获取租户
    /// </summary>
    /// <returns>租户信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static TenantModel GetTenantSync(string tenantNo, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}", out object obj) == true
            && obj is TenantModel tenantModel)
        {
            return tenantModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Tenant, tenantNo);

        tenantModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            TenantModel result = repository.Queryable<TenantModel>().Where(wh => wh.TenantNo == tenantNo).Single();

            if (result == null && throwError)
            {
                string message = $"未能找到对应租户【{tenantNo}】信息！";
                logger.LogError($"TenantNo：{tenantNo}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}"] = tenantModel;
        }

        return tenantModel;
    }

    /// <summary>
    /// 获取租户
    /// </summary>
    /// <returns>租户信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<TenantModel> GetTenant(string tenantNo, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}", out object obj) == true
            && obj is TenantModel tenantModel)
        {
            return tenantModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Tenant, tenantNo);

        tenantModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            TenantModel result = await repository.Queryable<TenantModel>().Where(wh => wh.TenantNo == tenantNo).SingleAsync();

            if (result == null && throwError)
            {
                string message = $"未能找到对应租户【{tenantNo}】信息！";
                logger.LogError($"TenantNo：{tenantNo}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}"] = tenantModel;
        }

        return tenantModel;
    }

    /// <summary>
    /// 删除租户
    /// </summary>
    public static async Task DeleteTenant(string tenantNo)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            if (httpContext.Items.ContainsKey($"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}"))
            {
                httpContext.Items.Remove($"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}.{tenantNo}");
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Tenant, tenantNo);

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 删除所有租户
    /// </summary>
    public static async Task DeleteAllTenant()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys
                .Where(wh => wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(TenantModel.TenantNo)}."))
                .ToList();
            foreach (object key in keys)
            {
                httpContext.Items.Remove(key);
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Tenant, "*");
        await centerCache.DelByPatternAsync(cacheKey);
    }
}
