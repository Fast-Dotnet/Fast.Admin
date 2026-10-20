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
/// 消息模板上下文
/// </summary>
[SuppressSniffer]
public class MessageTemplateContext
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static ICache<CenterCCL> centerCache = FastContext.GetService<ICache<CenterCCL>>();

    /// <summary>
    /// 日志
    /// </summary>
    internal static ILogger logger = FastContext.GetService<ILogger<MessageTemplateContext>>();

    /// <summary>
    /// 获取消息模板
    /// </summary>
    /// <remarks>应用标识为 <see langword="null"/>、空字符串或纯空白时，仅查询全局模板。</remarks>
    /// <returns>模板信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static MessageTemplateModel GetMessageTemplateSync(MessageTemplateTypeEnum templateType, string appKey = null,
        bool throwError = true)
    {
        if (!Enum.IsDefined(templateType))
        {
            throw new UserFriendlyException("消息模板类型不正确！");
        }

        appKey = string.IsNullOrWhiteSpace(appKey) ? null : appKey;

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue(
                $"{nameof(Fast)}.{nameof(MessageTemplateModel)}.{(long)templateType}.{appKey ?? "Global"}", out object obj)
            == true
            && obj is MessageTemplateModel messageTemplateModel)
        {
            return messageTemplateModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.MessageTemplate, appKey ?? "Global", templateType.ToString());

        messageTemplateModel = centerCache.GetAndSet(cacheKey, () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            MessageTemplateModel result = repository.Queryable<MessageTemplateModel>()
                .Where(wh => wh.AppKey == appKey && wh.TemplateType == templateType)
                .Single();

            if (result == null && throwError)
            {
                string message = $"应用【{appKey ?? "全局"}】的消息模板【{templateType}】不存在！";
                logger.LogError($"消息模板不可用，AppKey：{appKey}；TemplateType：{templateType}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(MessageTemplateModel)}.{(long)templateType}.{appKey ?? "Global"}"] =
                messageTemplateModel;
        }

        return messageTemplateModel;
    }

    /// <summary>
    /// 获取消息模板
    /// </summary>
    /// <remarks>应用标识为 <see langword="null"/>、空字符串或纯空白时，仅查询全局模板。</remarks>
    /// <returns>模板信息；未找到且不要求抛出异常时为 <see langword="null"/></returns>
    public static async Task<MessageTemplateModel> GetMessageTemplate(MessageTemplateTypeEnum templateType, string appKey = null,
        bool throwError = true)
    {
        if (!Enum.IsDefined(templateType))
        {
            throw new UserFriendlyException("消息模板类型不正确！");
        }

        appKey = string.IsNullOrWhiteSpace(appKey) ? null : appKey;

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        if (httpContext?.Items.TryGetValue(
                $"{nameof(Fast)}.{nameof(MessageTemplateModel)}.{(long)templateType}.{appKey ?? "Global"}", out object obj)
            == true
            && obj is MessageTemplateModel messageTemplateModel)
        {
            return messageTemplateModel;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.MessageTemplate, appKey ?? "Global", templateType.ToString());

        messageTemplateModel = await centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            ISqlSugarClient repository = FastContext.GetService<ISqlSugarClient>();

            MessageTemplateModel result = await repository.Queryable<MessageTemplateModel>()
                .Where(wh => wh.AppKey == appKey && wh.TemplateType == templateType)
                .SingleAsync();

            if (result == null && throwError)
            {
                string message = $"应用【{appKey ?? "全局"}】的消息模板【{templateType}】不存在！";
                logger.LogError($"消息模板不可用，AppKey：{appKey}；TemplateType：{templateType}；{message}");
                throw new UserFriendlyException(message);
            }

            return result;
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[$"{nameof(Fast)}.{nameof(MessageTemplateModel)}.{(long)templateType}.{appKey ?? "Global"}"] =
                messageTemplateModel;
        }

        return messageTemplateModel;
    }

    /// <summary>
    /// 清除指定消息模板的请求缓存与共享缓存，不删除数据库记录。
    /// </summary>
    /// <param name="appKey">应用标识，为 <see langword="null"/>、空字符串或纯空白时清除全局模板缓存</param>
    /// <param name="templateType">模板类型</param>
    public static async Task DeleteMessageTemplate(MessageTemplateTypeEnum templateType, string appKey = null)
    {
        if (!Enum.IsDefined(templateType))
        {
            throw new UserFriendlyException("消息模板类型不正确！");
        }

        appKey = string.IsNullOrWhiteSpace(appKey) ? null : appKey;

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            httpContext.Items.Remove($"{nameof(Fast)}.{nameof(MessageTemplateModel)}.{(long)templateType}.{appKey ?? "Global"}");
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.MessageTemplate, appKey ?? "Global", templateType.ToString());

        await centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 清除所有消息模板缓存，不删除数据库记录。
    /// </summary>
    public static async Task DeleteAllMessageTemplate()
    {
        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 清空 HttpContext.Items 中的
            var keys = httpContext.Items.Keys
                .Where(wh => wh is string key && key.StartsWith($"{nameof(Fast)}.{nameof(MessageTemplateModel)}."))
                .ToList();
            foreach (object key in keys)
            {
                httpContext.Items.Remove(key);
            }
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.MessageTemplate, "*", "*");
        await centerCache.DelByPatternAsync(cacheKey);
    }
}
