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
/// 商户号上下文
/// </summary>
[SuppressSniffer]
public class MerchantContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetService<ILogger<MerchantContext>>();

    /// <summary>
    /// 获取商户号
    /// </summary>
    /// <returns>商户信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static MerchantModel GetMerchantSync(string merchantNo, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(merchantNo))
        {
            throw new UserFriendlyException("商户号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}", out object obj)
            == true
            && obj is MerchantModel merchantModel)
        {
            return merchantModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Merchant, merchantNo);

        merchantModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            MerchantModel result = repository.Queryable<MerchantModel>().Where(wh => wh.MerchantNo == merchantNo).Single();

            if (result == null && throwError)
            {
                string message = $"未能找到对应商户号【{merchantNo}】信息！";
                logger.LogError($"MerchantNo：{merchantNo}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}"] = merchantModel;
        }

        return merchantModel;
    }

    /// <summary>
    /// 获取商户号
    /// </summary>
    /// <returns>商户信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<MerchantModel> GetMerchant(string merchantNo, bool throwError = true)
    {
        if (string.IsNullOrWhiteSpace(merchantNo))
        {
            throw new UserFriendlyException("商户号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue($"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}", out object obj)
            == true
            && obj is MerchantModel merchantModel)
        {
            return merchantModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Merchant, merchantNo);

        merchantModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            MerchantModel result = await repository
                .Queryable<MerchantModel>()
                .Where(wh => wh.MerchantNo == merchantNo)
                .SingleAsync();

            if (result == null && throwError)
            {
                string message = $"未能找到对应商户号【{merchantNo}】信息！";
                logger.LogError($"MerchantNo：{merchantNo}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}"] = merchantModel;
        }

        return merchantModel;
    }

    /// <summary>
    /// 删除商户号
    /// </summary>
    public static async Task DeleteMerchant(string merchantNo)
    {
        if (string.IsNullOrWhiteSpace(merchantNo))
        {
            throw new UserFriendlyException("商户号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            if (httpContext.Items.ContainsKey($"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}"))
            {
                httpContext.Items.Remove($"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}.{merchantNo}");
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Merchant, merchantNo);

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 删除所有商户号
    /// </summary>
    public static async Task DeleteAllMerchant()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys
                .Where(wh => wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(MerchantModel.MerchantNo)}."))
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
