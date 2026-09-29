// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

//using Fast.Center.Enum;
//using SqlSugar;

//namespace Fast.Scheduler.LocalJob.Test;

///// <summary>
///// <see cref="TestCronLocalJob"/> 测试 Cron 类型本地作业 
///// </summary>
//public class TestCronLocalJob : ISchedulerJob
//{
//    /// <inheritdoc />
//    public SchedulerLocalJobInfo GetLocalJob()
//    {
//        return new SchedulerLocalJobInfo
//        {
//            JobName = "测试 Cron 类型本地作业",
//            JobGroup = SchedulerJobGroupEnum.System,
//            BeginTime = new DateTime(1970, 01, 01),
//            EndTime = null,
//            TriggerType = TriggerTypeEnum.Cron,
//            Cron = "0 0/1 * * * ?",
//            Week = null,
//            DailyStartTime = null,
//            DailyEndTime = null,
//            IntervalSecond = null,
//            RunTimes = null,
//            WarnTime = null,
//            RetryTimes = null,
//            RetryMillisecond = null,
//            MailMessage = MailMessageEnum.None,
//            Description = "测试 Cron 类型本地作业，每1分钟执行一次。"
//        };
//    }

//    /// <inheritdoc />
//    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
//    {
//        // 等待7秒
//        await Task.Delay(7 * 1000);

//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine("Test cron local job.");
//        Console.ResetColor();

//        return await Task.FromResult<string>(null);
//    }
//}



