// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Shared;

/// <summary>
/// 星期枚举
/// </summary>
[Flags]
[FastEnum("星期枚举")]
public enum WeekEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 星期一
    /// </summary>
    [Description("星期一")]
    Monday = 1,

    /// <summary>
    /// 星期二
    /// </summary>
    [Description("星期二")]
    Tuesday = 2,

    /// <summary>
    /// 星期三
    /// </summary>
    [Description("星期三")]
    Wednesday = 4,

    /// <summary>
    /// 星期四
    /// </summary>
    [Description("星期四")]
    Thursday = 8,

    /// <summary>
    /// 星期五
    /// </summary>
    [Description("星期五")]
    Friday = 16,

    /// <summary>
    /// 星期六
    /// </summary>
    [Description("星期六")]
    Saturday = 32,

    /// <summary>
    /// 星期日
    /// </summary>
    [Description("星期日")]
    Sunday = 64
}

/// <summary>
/// <see cref="WeekEnum"/> 扩展方法
/// </summary>
public static class WeekEnumExtension
{
    /// <summary>
    /// 星期映射
    /// </summary>
    private static readonly (WeekEnum weekEnum, DayOfWeek dayOfWeek)[] WeekMap =
    [
        (WeekEnum.Monday, DayOfWeek.Monday), (WeekEnum.Tuesday, DayOfWeek.Tuesday), (WeekEnum.Wednesday, DayOfWeek.Wednesday),
        (WeekEnum.Thursday, DayOfWeek.Thursday), (WeekEnum.Friday, DayOfWeek.Friday), (WeekEnum.Saturday, DayOfWeek.Saturday),
        (WeekEnum.Sunday, DayOfWeek.Sunday)
    ];

    /// <summary>
    /// 转换为 <see cref="DayOfWeek"/> 集合
    /// </summary>
    /// <returns>转换后的星期集合</returns>
    public static List<DayOfWeek> ToDayOfWeeks(this WeekEnum week)
    {
        return WeekMap.Where(wh => (week & wh.weekEnum) != 0).Select(sl => sl.dayOfWeek).ToList();
    }

    /// <summary>
    /// 转换为 <see cref="DayOfWeek"/>
    /// </summary>
    /// <returns>转换后的星期</returns>
    public static DayOfWeek ToDayOfWeek(this WeekEnum week)
    {
        return week.ToDayOfWeeks().FirstOrDefault();
    }

    /// <summary>
    /// 转换为 <see cref="WeekEnum"/>
    /// </summary>
    /// <returns>匹配的星期枚举值；没有匹配项时为 <see langword="null"/></returns>
    public static WeekEnum? ToWeekEnum(this DayOfWeek daysOfWeek)
    {
        foreach ((WeekEnum weekEnum, DayOfWeek dayOfWeek) item in WeekMap)
        {
            if (item.dayOfWeek == daysOfWeek)
            {
                return item.weekEnum;
            }
        }

        return null;
    }

    /// <summary>
    /// 转换为 <see cref="WeekEnum"/>
    /// </summary>
    /// <returns>组合后的星期枚举值；集合为空时为 <see langword="null"/></returns>
    public static WeekEnum? ToWeekEnum(this List<DayOfWeek> daysOfWeek)
    {
        WeekEnum week = WeekEnum.None;

        foreach ((WeekEnum weekEnum, DayOfWeek dayOfWeek) item in WeekMap)
        {
            if (daysOfWeek.Contains(item.dayOfWeek))
            {
                week |= item.weekEnum;
            }
        }

        return week == WeekEnum.None ? null : week;
    }
}
