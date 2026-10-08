// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// <see cref="JobDataMap"/> 扩展方法
/// </summary>
[SuppressSniffer]
public static class JobDataMapExtension
{
    /// <summary>
    /// 获取以 <see langword="int"/> 为基础类型的枚举值
    /// </summary>
    /// <typeparam name="T">以 int 为基础类型的枚举</typeparam>
    /// <param name="jobData">作业数据集合</param>
    /// <param name="key">数据键</param>
    /// <returns>枚举值</returns>
    public static T GetEnum<T>(this JobDataMap jobData, string key) where T : Enum
    {
        // 获取值
        int value = jobData.GetInt(key);

        // 获取枚举类型
        Type enumType = typeof(T);

        int flagValues = 0;
        foreach (object enumValue in Enum.GetValues(enumType))
        {
            flagValues |= Convert.ToInt32(enumValue);
        }

        if ((value & flagValues) == value)
        {
            return (T)Enum.ToObject(enumType, value);
        }

        if (Enum.IsDefined(enumType, value))
        {
            return (T)Enum.ToObject(enumType, value);
        }

        throw new ArgumentNullException(key);
    }

    /// <summary>
    /// 获取以 <see langword="int"/> 为基础类型的可空枚举值
    /// </summary>
    /// <typeparam name="T">以 int 为基础类型的枚举</typeparam>
    /// <param name="jobData">作业数据集合</param>
    /// <param name="key">数据键</param>
    /// <returns>枚举值；键不存在、值为空或不满足枚举值检查时为 null</returns>
    /// <remarks>不是容错解析：值不能转换为 int 时，转换异常仍会向外传播。</remarks>
    public static T? GetNullableEnum<T>(this JobDataMap jobData, string key) where T : struct, Enum
    {
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!jobData.ContainsKey(key))
        {
            return null;
        }

        if (jobData[key] == null)
        {
            return null;
        }

        // 获取值
        int value = Convert.ToInt32(jobData[key]);

        // 获取枚举类型
        Type enumType = typeof(T);
        Type underlyingType = Nullable.GetUnderlyingType(enumType) ?? enumType;

        int flagValues = 0;
        foreach (object enumValue in Enum.GetValues(underlyingType))
        {
            flagValues |= Convert.ToInt32(enumValue);
        }

        if ((value & flagValues) == value)
        {
            return (T)Enum.ToObject(underlyingType, value);
        }

        if (Enum.IsDefined(underlyingType, value))
        {
            return (T)Enum.ToObject(underlyingType, value);
        }

        return null;
    }

    /// <summary>
    /// 获取可空的 <see langword="int"/> 值
    /// </summary>
    /// <param name="jobData">作业数据集合</param>
    /// <param name="key">数据键</param>
    /// <returns>转换后的数值；键不存在或值为空时为 null</returns>
    /// <remarks>格式无效或数值溢出时，Convert 的异常会向外传播。</remarks>
    public static int? GetNullableInt(this JobDataMap jobData, string key)
    {
        // 判断是否存在
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!jobData.ContainsKey(key))
        {
            return null;
        }

        if (jobData[key] == null)
        {
            return null;
        }

        return Convert.ToInt32(jobData[key]);
    }

    /// <summary>
    /// 获取可空的 <see langword="long"/> 值
    /// </summary>
    /// <param name="jobData">作业数据集合</param>
    /// <param name="key">数据键</param>
    /// <returns>转换后的数值；键不存在或值为空时为 null</returns>
    /// <remarks>格式无效或数值溢出时，Convert 的异常会向外传播。</remarks>
    public static long? GetNullableLong(this JobDataMap jobData, string key)
    {
        // 判断是否存在
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!jobData.ContainsKey(key))
        {
            return null;
        }

        if (jobData[key] == null)
        {
            return null;
        }

        return Convert.ToInt64(jobData[key]);
    }

    /// <summary>
    /// 获取可空的 <see cref="DateTime"/> 值
    /// </summary>
    /// <param name="jobData">作业数据集合</param>
    /// <param name="key">数据键</param>
    /// <returns>转换后的数值；键不存在或值为空时为 null</returns>
    /// <remarks>格式无效或数值溢出时，Convert 的异常会向外传播。</remarks>
    public static DateTime? GetNullableDateTime(this JobDataMap jobData, string key)
    {
        // 判断是否存在
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!jobData.ContainsKey(key))
        {
            return null;
        }

        if (jobData[key] == null)
        {
            return null;
        }

        return DateTime.Parse(jobData[key].ToString());
    }

    /// <summary>
    /// 获取可空的 <see langword="bool"/> 值
    /// </summary>
    /// <returns>对应的布尔值；键不存在或值为空时为 <see langword="null"/></returns>
    public static bool? GetNullableBoolean(this JobDataMap jobData, string key)
    {
        // 判断是否存在
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!jobData.ContainsKey(key))
        {
            return null;
        }

        if (jobData[key] == null)
        {
            return null;
        }

        return Convert.ToBoolean(jobData[key].ToString());
    }
}
