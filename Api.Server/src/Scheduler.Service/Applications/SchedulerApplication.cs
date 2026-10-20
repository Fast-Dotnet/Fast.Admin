// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Impl.Triggers;

namespace Fast.Scheduler;

/// <summary>
/// 调度作业
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Scheduler, Name = "scheduler")]
[PlatformOnly]
public class SchedulerApplication : IDynamicApplication
{
    /// <summary>
    /// 调度中心
    /// </summary>
    private readonly ISchedulerCenter _schedulerCenter;

    /// <summary>
    /// 调度作业
    /// </summary>
    public SchedulerApplication(ISchedulerCenter schedulerCenter)
    {
        _schedulerCenter = schedulerCenter;
    }

    /// <summary>
    /// 运行并验证Cron表达式
    /// </summary>
    [HttpGet]
    [ApiInfo("运行并验证Cron表达式", HttpRequestActionEnum.Other)]
    public List<string> RunVerifyCron(string cron)
    {
        // 验证表达式是否正确
        if (!CronExpression.IsValidExpression(cron))
        {
            return ["请检查Cron表达式是否拼写正确！"];
        }

        var result = new List<string>();

        var cronTrigger = new CronTriggerImpl("TestName", "TestGroup", cron);
        var calendar = new BaseCalendar(TimeZoneInfo.Local);
        // 默认获取10条
        IReadOnlyList<DateTimeOffset> list = TriggerUtils.ComputeFireTimes(cronTrigger, calendar, 10);

        foreach (DateTimeOffset item in list)
        {
            result.Add(item.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        return result;
    }

    /// <summary>
    /// 获取调度器详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取调度器详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Scheduler.Detail)]
    public async Task<QuerySchedulerDetailOutput> QuerySchedulerDetail(long? tenantId = null)
    {
        return await _schedulerCenter.QuerySchedulerDetail(tenantId);
    }

    /// <summary>
    /// 启动调度器
    /// </summary>
    [HttpPost]
    [ApiInfo("启动调度器", HttpRequestActionEnum.Other)]
    [Permission(PermissionConst.Scheduler.Start)]
    public async Task StartScheduler(long? tenantId = null)
    {
        await _schedulerCenter.StartScheduler(tenantId);
    }

    /// <summary>
    /// 停止调度器
    /// </summary>
    [HttpPost]
    [ApiInfo("停止调度器", HttpRequestActionEnum.Other)]
    [Permission(PermissionConst.Scheduler.Stop)]
    public async Task StopScheduler(long? tenantId = null)
    {
        await _schedulerCenter.StopScheduler(tenantId);
    }

    /// <summary>
    /// 暂停调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("暂停调度作业", HttpRequestActionEnum.Other)]
    [Permission(PermissionConst.Scheduler.StopJob)]
    public async Task StopSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        await _schedulerCenter.StopSchedulerJob(input, tenantId);
    }

    /// <summary>
    /// 恢复调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("恢复调度作业", HttpRequestActionEnum.Other)]
    [Permission(PermissionConst.Scheduler.ResumeJob)]
    public async Task ResumeSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        await _schedulerCenter.ResumeSchedulerJob(input, tenantId);
    }

    /// <summary>
    /// 立即执行调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("立即执行调度作业", HttpRequestActionEnum.Other)]
    [Permission(PermissionConst.Scheduler.Trigger)]
    public async Task TriggerSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        await _schedulerCenter.TriggerSchedulerJob(input, tenantId);
    }

    /// <summary>
    /// 获取调度作业日志
    /// </summary>
    [HttpPost]
    [ApiInfo("获取调度作业日志", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Scheduler.Detail)]
    public async Task<List<string>> QuerySchedulerJobLogs(SchedulerJobKeyInput input, long? tenantId = null)
    {
        return await _schedulerCenter.QuerySchedulerJobLogs(input, tenantId);
    }

    /// <summary>
    /// 获取调度作业运行次数
    /// </summary>
    [HttpPost]
    [ApiInfo("获取调度作业运行次数", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Scheduler.Detail)]
    public async Task<long> QuerySchedulerJobRunNumber(SchedulerJobKeyInput input, long? tenantId = null)
    {
        return await _schedulerCenter.QuerySchedulerJobRunNumber(input, tenantId);
    }

    /// <summary>
    /// 获取全部调度作业
    /// </summary>
    [HttpGet]
    [ApiInfo("获取全部调度作业", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Scheduler.Paged)]
    public async Task<List<QueryAllSchedulerJobOutput>> QueryAllSchedulerJob(SchedulerJobGroupEnum? jobGroup = null,
        long? tenantId = null)
    {
        return await _schedulerCenter.QueryAllSchedulerJob(jobGroup, tenantId);
    }

    /// <summary>
    /// 获取调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("获取调度作业", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Scheduler.Detail)]
    public async Task<SchedulerJobInfo> QuerySchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        return await _schedulerCenter.QuerySchedulerJob(input, tenantId);
    }

    /// <summary>
    /// 添加调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("添加调度作业", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Scheduler.Add)]
    public async Task AddSchedulerJob(AddSchedulerJobInput input)
    {
        await _schedulerCenter.AddSchedulerJob(input);
    }

    /// <summary>
    /// 编辑调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑调度作业", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Scheduler.Edit)]
    public async Task EditSchedulerJob(EditSchedulerJobInput input)
    {
        await _schedulerCenter.EditSchedulerJob(input);
    }

    /// <summary>
    /// 删除调度作业
    /// </summary>
    [HttpPost]
    [ApiInfo("删除调度作业", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Scheduler.Delete)]
    public async Task DeleteSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        await _schedulerCenter.DeleteSchedulerJob(input, tenantId);
    }

    /// <summary>
    /// 移除调度作业异常信息
    /// </summary>
    [HttpPost]
    [ApiInfo("移除调度作业异常信息", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Scheduler.Delete)]
    public async Task DeleteSchedulerJobException(SchedulerJobKeyInput input, long? tenantId = null)
    {
        await _schedulerCenter.DeleteSchedulerJobException(input, tenantId);
    }
}
