// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

//using Fast.Center.Enum;
//using Fast.Shared;
//using SqlSugar;

//namespace Fast.Scheduler.LocalJob.Test;

///// <summary>
///// <see cref="TestDailyLocalJob"/> 测试 Daily 类型本地作业 
///// </summary>
//public class TestDailyLocalJob : ISchedulerJob
//{
//    /// <inheritdoc />
//    public SchedulerLocalJobInfo GetLocalJob()
//    {
//        return new SchedulerLocalJobInfo
//        {
//            JobName = "测试 Daily 类型本地作业",
//            JobGroup = SchedulerJobGroupEnum.System,
//            BeginTime = new DateTime(1970, 01, 01),
//            EndTime = null,
//            TriggerType = TriggerTypeEnum.Daily,
//            Cron = null,
//            Week =
//                WeekEnum.Monday
//                | WeekEnum.Tuesday
//                | WeekEnum.Wednesday
//                | WeekEnum.Thursday
//                | WeekEnum.Friday
//                | WeekEnum.Saturday
//                | WeekEnum.Sunday,
//            DailyStartTime = new TimeSpan(0, 0, 0),
//            DailyEndTime = new TimeSpan(23, 59, 59),
//            IntervalSecond = 60,
//            RunTimes = null,
//            WarnTime = null,
//            RetryTimes = null,
//            RetryMillisecond = null,
//            MailMessage = MailMessageEnum.None,
//            Description = "测试 Daily 类型本地作业，每60秒执行一次。"
//        };
//    }

//    /// <inheritdoc />
//    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
//    {
//        // 等待7秒
//        await Task.Delay(7 * 1000);

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("Test daily local job.");
//        Console.ResetColor();

//        return await Task.FromResult<string>(null);
//    }
//}



