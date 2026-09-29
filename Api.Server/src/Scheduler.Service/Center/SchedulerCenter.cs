// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using Fast.Center.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;

namespace Fast.Scheduler;

/// <summary>
/// <see cref="ISchedulerCenter"/> 默认实现
/// </summary>
public class SchedulerCenter : ISchedulerCenter, ISingletonDependency
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 调度器工厂
    /// </summary>
    private readonly IDependencySchedulerFactory _schedulerFactory;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    public SchedulerCenter(IServiceProvider serviceProvider, IDependencySchedulerFactory schedulerFactory,
        ILogger<ISchedulerCenter> logger)
    {
        _serviceProvider = serviceProvider;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    /// <summary>
    /// 异步线程锁
    /// </summary>
    private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    /// <summary>
    /// 调度器期望待机控制日历名称
    /// <para>由管理宿主写入，执行宿主读取，用于保存用户希望调度器达到的状态。</para>
    /// <para>复用 Quartz 持久化日历，避免额外增加调度状态表。</para>
    /// </summary>
    private const string SchedulerDesiredStandbyCalendarName = "__FAST_SCHEDULER_STANDBY__";

    /// <summary>
    /// 调度器实际待机状态日历名称
    /// <para>仅在执行宿主成功切换状态后更新，用于区分期望状态是否已经生效。</para>
    /// </summary>
    private const string SchedulerActualStandbyCalendarName = "__FAST_SCHEDULER_ACTUAL_STANDBY__";

    private const string SchedulerRunningStatus = "Running (运行)";

    private const string SchedulerStandbyStatus = "Standby (待机)";

    private const string SchedulerOfflineStatus = "Offline (离线)";

    /// <summary>
    /// 获取调度器是否期望待机
    /// </summary>
    private static async Task<bool> GetSchedulerDesiredStandbyState(IScheduler scheduler)
    {
        return await scheduler.GetCalendar(SchedulerDesiredStandbyCalendarName) != null;
    }

    /// <summary>
    /// 获取调度器是否实际待机
    /// </summary>
    private static async Task<bool> GetSchedulerActualStandbyState(IScheduler scheduler)
    {
        return await scheduler.GetCalendar(SchedulerActualStandbyCalendarName) != null;
    }

    /// <summary>
    /// 设置调度器实际待机状态
    /// </summary>
    private static async Task SetSchedulerActualStandbyState(IScheduler scheduler, bool standby)
    {
        if (standby)
        {
            await scheduler.AddCalendar(SchedulerActualStandbyCalendarName, new BaseCalendar(), true, false);
        }
        else
        {
            await scheduler.DeleteCalendar(SchedulerActualStandbyCalendarName);
        }
    }

    /// <summary>
    /// 获取调度执行宿主是否在线
    /// <para>Quartz 执行实例启动后会定期向 QRTZ_SCHEDULER_STATE 写入集群心跳。</para>
    /// </summary>
    private static async Task<bool> GetExecutionHostOnline(string schedulerName, string localSchedulerInstanceId)
    {
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

        List<QuartzSchedulerStateModel> schedulerStateList = await db
            .Queryable<QuartzSchedulerStateModel>()
            .Where(wh => wh.SchedName == schedulerName)
            .ToListAsync();
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return schedulerStateList.Any(state =>
        {
            // 当前进程只负责管理调度器，排除自身心跳后剩余实例即为执行宿主
            if (state.InstanceName == localSchedulerInstanceId)
            {
                return false;
            }

            // 允许最多丢失一次心跳，并保证至少有 15 秒容错时间，避免短暂抖动被误判为离线
            long offlineThreshold = Math.Max(state.CheckInInterval * 2, 15000L);
            return state.LastCheckInTime >= currentTime - offlineThreshold;
        });
    }

    /// <summary>
    /// 调度作业属性验证
    /// </summary>
    private void SchedulerJobVerify(SchedulerJobInfo jobInfo)
    {
        jobInfo.IsSystem = jobInfo.TenantId == null;

        if (string.IsNullOrWhiteSpace(jobInfo.JobName))
        {
            throw new UserFriendlyException("调度作业名称不能为空！");
        }

        // 结束时间不能小于开始时间
        if (jobInfo.EndTime != null && jobInfo.EndTime <= jobInfo.BeginTime)
        {
            throw new UserFriendlyException("结束时间不能小于等于开始时间！");
        }

        // 触发器类型判断
        switch (jobInfo.TriggerType)
        {
            case TriggerTypeEnum.Cron:
                // Cron 默认 星期，每天开始时间，每天结束时间，执行间隔时间，执行次数 为空
                jobInfo.Week = null;
                jobInfo.DailyStartTime = null;
                jobInfo.DailyEndTime = null;
                jobInfo.IntervalSecond = null;
                jobInfo.RunTimes = null;

                if (string.IsNullOrWhiteSpace(jobInfo.Cron))
                {
                    throw new UserFriendlyException("Cron表达式不能为空！");
                }

                // 验证表达式是否正确
                if (!CronExpression.IsValidExpression(jobInfo.Cron))
                {
                    throw new UserFriendlyException("Cron表达式不正确！");
                }

                if (jobInfo.Cron == "* * * * * ?")
                {
                    throw new UserFriendlyException("不允许频繁执行调度作业！");
                }

                var cronTrigger = new CronTriggerImpl("TestName", "TestGroup", jobInfo.Cron);
                var calendar = new BaseCalendar(TimeZoneInfo.Local);
                // 获取两条就好
                IReadOnlyList<DateTimeOffset> list = TriggerUtils.ComputeFireTimes(cronTrigger, calendar, 2);
                TimeSpan diffTime = list[1] - list[0];
                if (diffTime.TotalSeconds < 30)
                {
                    throw new UserFriendlyException("不允许低于30秒内循环执行调度作业！");
                }

                break;
            case TriggerTypeEnum.Daily:
                // Daily 默认Cron为空
                jobInfo.Cron = null;

                if (jobInfo.DailyStartTime == null)
                {
                    throw new UserFriendlyException("每天开始时间不能为空！");
                }

                if (jobInfo.DailyEndTime == null)
                {
                    throw new UserFriendlyException("每天结束时间不能为空！");
                }

                if (jobInfo.DailyEndTime <= jobInfo.DailyStartTime)
                {
                    throw new UserFriendlyException("每天结束时间不能小于等于开始时间！");
                }

                if (jobInfo.Week == null || jobInfo.Week == WeekEnum.None)
                {
                    throw new UserFriendlyException("具体星期几不能为空！");
                }

                if (jobInfo.IntervalSecond == null)
                {
                    throw new UserFriendlyException("执行间隔时间不能为空！");
                }

                // 判断执行间隔时间，不能低于30秒
                if (jobInfo.IntervalSecond < 30)
                {
                    throw new UserFriendlyException("不允许低于30秒内循环执行调度作业！");
                }

                break;
            case TriggerTypeEnum.Simple:
                // Simple 默认 Cron，星期，每天开始时间，每天结束时间 为空
                jobInfo.Cron = null;
                jobInfo.Week = null;
                jobInfo.DailyStartTime = null;
                jobInfo.DailyEndTime = null;

                if (jobInfo.IntervalSecond == null)
                {
                    throw new UserFriendlyException("执行间隔时间不能为空！");
                }

                // 判断执行间隔时间，不能低于30秒
                if (jobInfo.IntervalSecond < 30)
                {
                    throw new UserFriendlyException("不允许低于30秒内循环执行调度作业！");
                }

                break;
            default:
                throw new UserFriendlyException("不支持的触发器类型！");
        }

        // 调度作业类型判断
        switch (jobInfo.JobType)
        {
            case SchedulerJobTypeEnum.Local:
                break;
            case SchedulerJobTypeEnum.IntranetUrl:
            case SchedulerJobTypeEnum.OuterNetUrl:
                if (string.IsNullOrWhiteSpace(jobInfo.RequestUrl))
                {
                    throw new UserFriendlyException("请求Url不能为空！");
                }

                if (!Uri.TryCreate(jobInfo.RequestUrl, UriKind.Absolute, out Uri requestUri)
                    || (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps)
                    || !string.IsNullOrWhiteSpace(requestUri.UserInfo))
                {
                    throw new UserFriendlyException("请求Url不正确，仅支持不含用户凭据的 HTTP/HTTPS 地址！");
                }

                if (jobInfo.RequestMethod == null)
                {
                    throw new UserFriendlyException("请求方式不能为空！");
                }

                break;
            default:
                throw new UserFriendlyException("不支持的调度作业类型！");
        }
    }

    /// <summary>
    /// 添加调度作业
    /// </summary>
    private async Task AddSchedulerJob(IScheduler scheduler, JobKey jobKey, SchedulerJobInfo jobInfo)
    {
        // 作业配置
        IJobConfigurator jobConfigurator;

        // 调度作业类型判断
        switch (jobInfo.JobType)
        {
            case SchedulerJobTypeEnum.Local:
                jobConfigurator = JobBuilder.Create<LocalJob>();

                break;
            case SchedulerJobTypeEnum.IntranetUrl:
            case SchedulerJobTypeEnum.OuterNetUrl:
                jobConfigurator = JobBuilder.Create<UrlJob>();

                break;
            default:
                throw new UserFriendlyException("不支持的调度作业类型！");
        }

        // 作业数据
        var jobDataMap = new JobDataMap(jobInfo.ToDictionary(true));

        // 构建作业
        IJobDetail jobDetail = jobConfigurator.WithIdentity(jobKey)
            // 作业数据
            .SetJobData(jobDataMap)
            // 描述
            .WithDescription(jobInfo.Description)
            // 持久化，表示该作业没有触发器也能存在
            .StoreDurably()
            // 这里注释，是因为 JobBase 已经增加了特性
            //// 持久化保留作业
            //.PersistJobDataAfterExecution()
            //// 不允许并发执行
            //.DisallowConcurrentExecution()
            // 请求恢复，当应用发生故障的时候，是否重新执行
            .RequestRecovery()
            .Build();

        // 构建触发器
        TriggerBuilder triggerBuilder = TriggerBuilder.Create()
            .WithIdentity(jobKey.Name, jobKey.Group)
            // 开始时间
            .StartAt(jobInfo.BeginTime)
            // 结束时间
            //.EndAt(jobInfo.EndTime)
            // 作业名称
            .ForJob(jobDetail);

        // 触发器类型判断
        switch (jobInfo.TriggerType)
        {
            case TriggerTypeEnum.Cron:
                // 设置 Cron 类型的触发器
                triggerBuilder
                    // 指定 Cron 表达式
                    .WithCronSchedule(jobInfo.Cron, builder =>
                    {
                        builder
                            // 错过立即执行，剩余按计划
                            .WithMisfireHandlingInstructionFireAndProceed();
                    });

                break;
            case TriggerTypeEnum.Daily:
                // 设置 Daily 类型的触发器
                triggerBuilder.WithDailyTimeIntervalSchedule(builder =>
                {
                    builder
                        // 执行日期（星期）
                        .OnDaysOfTheWeek(jobInfo.Week!.Value.ToDayOfWeeks())
                        // 执行开始时间
                        .StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(jobInfo.DailyStartTime!.Value.Hours,
                            jobInfo.DailyStartTime.Value.Minutes, jobInfo.DailyStartTime.Value.Seconds))
                        // 执行结束时间
                        .EndingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(jobInfo.DailyEndTime!.Value.Hours,
                            jobInfo.DailyEndTime.Value.Minutes, jobInfo.DailyEndTime.Value.Seconds))
                        // 执行时间间隔，单位秒
                        .WithIntervalInSeconds(Math.Abs(jobInfo.IntervalSecond!.Value))
                        // 错过立即执行，剩余按计划
                        .WithMisfireHandlingInstructionFireAndProceed();

                    // 判断执行次数是否存在
                    if (jobInfo.RunTimes is > 0)
                    {
                        builder
                            // 执行次数，默认从0开始
                            .EndingDailyAfterCount(jobInfo.RunTimes.Value);
                    }
                });

                break;
            case TriggerTypeEnum.Simple:
                // 判断执行次数是否存在
                if (jobInfo.RunTimes == null || jobInfo.RunTimes <= 0)
                {
                    // 设置 Simple 类型的触发器（无限循环）
                    triggerBuilder.WithSimpleSchedule(builder =>
                    {
                        builder
                            // 执行时间间隔，单位秒
                            .WithIntervalInSeconds(Math.Abs(jobInfo.IntervalSecond!.Value))
                            // 错过立即执行，重新排计划
                            .WithMisfireHandlingInstructionFireNow()
                            // 无限循环
                            .RepeatForever();
                    });
                }
                else
                {
                    // 设置 Simple 类型的触发器
                    triggerBuilder.WithSimpleSchedule(builder =>
                    {
                        builder
                            // 执行时间间隔，单位秒
                            .WithIntervalInSeconds(Math.Abs(jobInfo.IntervalSecond!.Value))
                            // 错过立即执行，重新排计划
                            .WithMisfireHandlingInstructionFireNow()
                            // 执行次数，默认从0开始
                            .WithRepeatCount(jobInfo.RunTimes.Value);
                    });
                }

                break;
            default:
                throw new UserFriendlyException("不支持的触发器类型！");
        }

        // 触发器
        ITrigger trigger = triggerBuilder.Build();

        // 添加作业
        await scheduler.ScheduleJob(jobDetail, trigger);
    }

    /// <summary>
    /// 添加调度作业
    /// </summary>
    private async Task AddSchedulerJob(SchedulerJobInfo jobInfo)
    {
        // 属性验证
        SchedulerJobVerify(jobInfo);

        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(jobInfo.TenantId);

        // 获取调度作业Key
        var jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup.ToString());

        // 检查作业是否存在
        if (await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业已存在！");
        }

        // 添加调度作业
        await AddSchedulerJob(scheduler, jobKey, jobInfo);
    }

    /// <summary>
    /// 添加本地作业
    /// </summary>
    private async Task AddLocalJob(SchedulerLocalJobInfo localJob, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        // 作业Key
        var jobKey = new JobKey(localJob.JobName, localJob.JobGroup.ToString());

        long runNumber = 0L;
        string exception = null;
        List<string> logs = null;

        // 判断调度作业是否存在
        if (await scheduler.CheckExists(jobKey))
        {
            // 获取作业详情
            IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

            // 暂停旧的调度作业
            await scheduler.PauseJob(jobKey);
            // 删除旧的调度作业
            await scheduler.DeleteJob(jobKey);

            runNumber = jobDetail!.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber));
            exception = jobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.Exception));
            logs = jobDetail.JobDataMap[nameof(SchedulerJobInfo.Logs)] as List<string>;
        }

        // 添加本地作业
        await AddSchedulerJob(new SchedulerJobInfo
        {
            TenantId = tenantId,
            JobName = localJob.JobName,
            JobGroup = localJob.JobGroup,
            JobType = SchedulerJobTypeEnum.Local,
            BeginTime = localJob.BeginTime,
            EndTime = localJob.EndTime,
            TriggerType = localJob.TriggerType,
            Cron = localJob.Cron,
            Week = localJob.Week,
            DailyStartTime = localJob.DailyStartTime,
            DailyEndTime = localJob.DailyEndTime,
            IntervalSecond = localJob.IntervalSecond,
            RunTimes = localJob.RunTimes,
            WarnTime = localJob.WarnTime,
            RetryTimes = localJob.RetryTimes,
            RetryMillisecond = localJob.RetryMillisecond,
            MailMessage = localJob.MailMessage,
            Description = localJob.Description,
            IsAllTenant = localJob.IsAllTenant,
            RunNumber = runNumber,
            Exception = exception,
            Logs = logs
        });
    }

    /// <inheritdoc />
    public async Task InitializeScheduler()
    {
        // 获取锁
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            if (SchedulerContext.Initialized)
            {
                throw new UserFriendlyException("不能多次调用 'InitializeSchedule' 进行初始化调度程序！");
            }

            // 在初始化的时候，创建所有租户调度器
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
            // 加载Aop
            SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

            var allLocalJobList = new List<SchedulerLocalJobInfo>();
            Type ISchedulerJobType = typeof(ISchedulerJob);
            var schedulerJobTypes = MAppContext.EffectiveTypes.Where(wh =>
                    ISchedulerJobType.IsAssignableFrom(wh) && wh.IsClass && !wh.IsInterface && !wh.IsAbstract)
                .ToList();
            foreach (Type schedulerJobType in schedulerJobTypes)
            {
                if (ActivatorUtilities.CreateInstance(_serviceProvider, schedulerJobType) is ISchedulerJob schedulerJobInstance)
                {
                    SchedulerLocalJobInfo localJobEntity = schedulerJobInstance.GetLocalJob();
                    // 放入缓存集合中
                    SchedulerContext.LocalSchedulerJobList.Add(localJobEntity);

                    var jobKey = new JobKey(localJobEntity.JobName, localJobEntity.JobGroup.ToString());

                    // 放入缓存集合中
                    SchedulerContext.LocalSchedulerJobTypes.TryAdd(jobKey.ToString(), schedulerJobType);

                    allLocalJobList.Add(localJobEntity);
                }
            }

            foreach (SchedulerLocalJobInfo localJobEntity in allLocalJobList.Where(wh => !wh.IsAllTenant).ToList())
            {
                // 添加本地作业
                await AddLocalJob(localJobEntity);
            }

            // 启动核心调度器
            await StartScheduler();

            var tenantList = await db.Queryable<TenantModel>()
                // 数据库已经初始化的
                .LeftJoin<MainDatabaseModel>((t1, t2) => t1.TenantId == t2.TenantId && t2.DatabaseType == DatabaseTypeEnum.Admin)
                .Where((t1, t2) => t2.IsInitialized)
                .Where(t1 => t1.Status == CommonStatusEnum.Enable)
                .Select(t1 => new {t1.TenantId, t1.TenantNo, t1.TenantCode, t1.TenantName})
                .ToListAsync();

            foreach (var item in tenantList)
            {
                // 启动租户调度器
                await StartScheduler(item.TenantId);
                // 放入租户调度器缓存中
                SchedulerContext.SchedulerTenantList.TryAdd(item.TenantId,
                    (item.TenantName, item.TenantNo, item.TenantCode, Guid.NewGuid().ToString()));

                // 循环租户本地作业
                foreach (SchedulerLocalJobInfo localJobEntity in allLocalJobList.Where(wh => wh.IsAllTenant).ToList())
                {
                    // 添加本地作业
                    await AddLocalJob(localJobEntity, item.TenantId);
                }
            }

            // 调度程序初始化完成
            SchedulerContext.Initialized = true;

            _logger.LogInformation("Initialize schedule.");
        }
        finally
        {
            // 释放锁
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task SyncScheduler()
    {
        // 获取锁
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            // 获取未设置调度器的TenantId
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
            // 加载Aop
            SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

            var tenantList = await db.Queryable<TenantModel>()
                // 数据库已经初始化的
                .LeftJoin<MainDatabaseModel>((t1, t2) => t1.TenantId == t2.TenantId && t2.DatabaseType == DatabaseTypeEnum.Admin)
                .Where((t1, t2) => t2.IsInitialized)
                .Where(t1 => t1.Status == CommonStatusEnum.Enable)
                .Where(t1 => !SchedulerContext.SchedulerTenantList.Keys.ToList().Contains(t1.TenantId))
                .Select(t1 => new {t1.TenantId, t1.TenantNo, t1.TenantCode, t1.TenantName})
                .ToListAsync();

            foreach (var item in tenantList)
            {
                // 启动租户调度器
                await StartScheduler(item.TenantId);
                // 放入租户调度器缓存中
                SchedulerContext.SchedulerTenantList.TryAdd(item.TenantId,
                    (item.TenantName, item.TenantNo, item.TenantCode, Guid.NewGuid().ToString()));

                // 循环租户本地作业
                foreach (SchedulerLocalJobInfo localJobEntity in SchedulerContext.LocalSchedulerJobList
                             .Where(wh => wh.IsAllTenant)
                             .ToList())
                {
                    // 添加本地作业
                    await AddLocalJob(localJobEntity, item.TenantId);
                }
            }

            _logger.LogInformation($"Sync schedule. Add {tenantList.Count} new schedule.");
        }
        finally
        {
            // 释放锁
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task SyncSchedulerState()
    {
        // 管理宿主只维护数据库中的期望状态，不能启动或停止本进程中的 Quartz 调度线程
        if (!SchedulerContext.IsExecutionHost)
        {
            return;
        }

        IReadOnlyList<IScheduler> schedulerList = await _schedulerFactory.GetAllSchedulers();
        foreach (IScheduler scheduler in schedulerList)
        {
            // 期望状态由管理宿主写入，实际状态由执行宿主在状态切换成功后确认
            bool desiredStandby = await GetSchedulerDesiredStandbyState(scheduler);
            bool actualStandby = await GetSchedulerActualStandbyState(scheduler);

            // 将当前执行实例同步到管理宿主要求的状态
            if (desiredStandby && !scheduler.InStandbyMode)
            {
                await scheduler.Standby();
            }
            else if (!desiredStandby && scheduler.InStandbyMode)
            {
                await scheduler.Start();
            }

            // 只有上面的状态切换没有发生异常，才记录已经生效的实际状态
            if (desiredStandby != actualStandby)
            {
                await SetSchedulerActualStandbyState(scheduler, desiredStandby);
            }
        }
    }

    /// <inheritdoc />
    public async Task<QuerySchedulerDetailOutput> QuerySchedulerDetail(long? tenantId = null)
    {
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);
        SchedulerMetaData metaData = await scheduler.GetMetaData();

        // 期望状态表示用户设置，实际状态表示执行宿主最后一次成功应用的状态
        bool desiredStandby = await GetSchedulerDesiredStandbyState(scheduler);
        bool actualStandby = await GetSchedulerActualStandbyState(scheduler);

        // 执行宿主离线时，实际状态优先显示为 Offline，不根据期望状态推测正在运行
        string localSchedulerInstanceId = SchedulerContext.IsExecutionHost ? null : metaData.SchedulerInstanceId;
        bool executionOnline = await GetExecutionHostOnline(metaData.SchedulerName, localSchedulerInstanceId);
        string desiredStatus = desiredStandby ? SchedulerStandbyStatus : SchedulerRunningStatus;
        string actualStatus = !executionOnline ? SchedulerOfflineStatus :
            actualStandby ? SchedulerStandbyStatus : SchedulerRunningStatus;
        bool schedulerStarted = executionOnline && !actualStandby;

        return new QuerySchedulerDetailOutput
        {
            IsStarted = schedulerStarted,
            QuartzVersion = metaData.Version,
            DesiredStatus = desiredStatus,
            ExecutionOnline = executionOnline,
            ActualStatus = actualStatus,
            // 以下旧字段继续返回，避免影响现有调用方；状态语义与新增字段保持一致
            SchedulerStatus = actualStatus,
            SchedulerShutdown = !executionOnline,
            SchedulerInStandbyMode = desiredStandby,
            SchedulerStarted = schedulerStarted,
            SchedulerInstanceId = metaData.SchedulerInstanceId,
            SchedulerName = metaData.SchedulerName,
            SchedulerRemote = metaData.SchedulerRemote,
            SchedulerType = metaData.SchedulerType.FullName,
            JobStoreType = metaData.JobStoreType.FullName,
            SupportsPersistence = metaData.JobStoreSupportsPersistence,
            Clustered = metaData.JobStoreClustered,
            ThreadPoolSize = metaData.ThreadPoolSize,
            ThreadPoolType = metaData.ThreadPoolType.FullName,
            JobExecutedNumber = metaData.NumberOfJobsExecuted,
            RunTimes =
                metaData.RunningSince != null
                    ? DateTimeOffset.Now.Subtract(metaData.RunningSince.Value)
                        .ToString("dd\\ \\天\\ hh\\ \\时\\ mm\\ \\分\\ ss\\ \\秒")
                    : "--",
            JobCountNumber = (await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())).Count,
            TriggerCountNumber = (await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())).Count,
            JobExecuteNumber = (await scheduler.GetCurrentlyExecutingJobs()).Count
        };
    }

    /// <inheritdoc />
    public async Task<bool> StartScheduler(long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        // 管理宿主不启动本地调度线程，只删除待机标记，将期望状态设置为运行
        if (!SchedulerContext.IsExecutionHost)
        {
            await scheduler.DeleteCalendar(SchedulerDesiredStandbyCalendarName);
            return !await GetSchedulerDesiredStandbyState(scheduler);
        }

        // 执行宿主首次启动调度器后才会注册 Quartz 集群心跳
        if (!scheduler.IsStarted)
        {
            await scheduler.Start();
        }

        // 初始化时仍需尊重管理宿主之前保存的期望待机状态
        if (await GetSchedulerDesiredStandbyState(scheduler))
        {
            if (!scheduler.InStandbyMode)
            {
                await scheduler.Standby();
            }

            await SetSchedulerActualStandbyState(scheduler, true);
            return false;
        }

        // 没有待机标记时，恢复调度线程并确认实际状态为运行
        if (scheduler.InStandbyMode)
        {
            await scheduler.Start();
        }

        await SetSchedulerActualStandbyState(scheduler, false);
        return !scheduler.InStandbyMode;
    }

    /// <inheritdoc />
    public async Task<bool> StopScheduler(long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        // 管理宿主只保存期望待机状态，执行宿主会在下一次状态同步时应用
        if (!SchedulerContext.IsExecutionHost)
        {
            await scheduler.AddCalendar(SchedulerDesiredStandbyCalendarName, new BaseCalendar(), true, false);
            return await GetSchedulerDesiredStandbyState(scheduler);
        }

        // 执行宿主进入待机后不再获取新作业，已经开始的作业仍可继续完成
        if (!scheduler.InStandbyMode)
        {
            await scheduler.Standby();
        }

        // 状态切换成功后，再确认实际状态为待机
        await SetSchedulerActualStandbyState(scheduler, true);
        return scheduler.InStandbyMode;
    }

    /// <inheritdoc />
    public async Task StopSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        await scheduler.PauseJob(new JobKey(input.JobName, input.JobGroup.ToString()));
    }

    /// <inheritdoc />
    public async Task ResumeSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 检查作业是否存在
        if (!await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业不存在！");
        }

        // 获取作业详情
        IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

        DateTime? endTime = jobDetail!.JobDataMap.GetNullableDateTime(nameof(SchedulerJobInfo.EndTime));
        // 判断作业是否已经过期
        if (endTime != null && endTime <= DateTime.Now)
        {
            throw new UserFriendlyException("调度作业已过期！");
        }

        // 恢复作业
        await scheduler.ResumeJob(jobKey);
    }

    /// <inheritdoc />
    public async Task TriggerSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        await scheduler.TriggerJob(new JobKey(input.JobName, input.JobGroup.ToString()));
    }

    /// <inheritdoc />
    public async Task<bool> ExistsSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        return await scheduler.CheckExists(new JobKey(input.JobName, input.JobGroup.ToString()));
    }

    /// <inheritdoc />
    public async Task<List<string>> QuerySchedulerJobLogs(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 检查作业是否存在
        if (!await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业不存在！");
        }

        // 获取作业详情
        IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

        List<string> logs = jobDetail!.JobDataMap[nameof(SchedulerJobInfo.Logs)] as List<string> ?? [];
        // 倒序排序
        logs.Reverse();
        return logs;
    }

    /// <inheritdoc />
    public async Task<long> QuerySchedulerJobRunNumber(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 检查作业是否存在
        if (!await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业不存在！");
        }

        // 获取作业详情
        IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

        return jobDetail!.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber));
    }

    /// <inheritdoc />
    public async Task<List<QueryAllSchedulerJobOutput>> QueryAllSchedulerJob(SchedulerJobGroupEnum? jobGroup = null,
        long? tenantId = null)
    {
        var result = new List<QueryAllSchedulerJobOutput>();

        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        // 获取调度器所有作业分组名称
        IReadOnlyCollection<string> jobGroupNames = await scheduler.GetJobGroupNames();

        jobGroupNames = jobGroupNames
            // 传参筛选
            .Where(jobGroup != null, wh => wh.Equals(jobGroup.ToString(), StringComparison.OrdinalIgnoreCase))
            //（分组名称排序）
            .OrderBy(ob => ob)
            .ToList();

        // 循环作业分组名称
        foreach (string jobGroupName in jobGroupNames)
        {
            var jobGroupInfo = new QueryAllSchedulerJobOutput
            {
                JobGroup = Enum.Parse<SchedulerJobGroupEnum>(jobGroupName), JobInfoList = []
            };

            // 获取当前分组的所有调度作业
            IReadOnlyCollection<JobKey> jobKeyList = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGroupName));

            // 循环调度作业Key（作业名称排序）
            foreach (JobKey jobKey in jobKeyList.OrderBy(ob => ob.Name))
            {
                // 获取作业详情
                IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

                // 获取触发器
                IReadOnlyCollection<ITrigger> triggers = await scheduler.GetTriggersOfJob(jobKey);
                ITrigger trigger = triggers.AsEnumerable().LastOrDefault();

                if (trigger == null)
                {
                    throw new UserFriendlyException("调度作业对应的触发器不存在！");
                }

                // 从触发器获取时间间隔
                string interval;

                // 触发器类型
                TriggerTypeEnum triggerType;
                if (trigger is CronTriggerImpl cronTrigger)
                {
                    triggerType = TriggerTypeEnum.Cron;
                    // 从触发器获取 Cron表达式
                    interval = cronTrigger.CronExpressionString;
                }
                else if (trigger is DailyTimeIntervalTriggerImpl dailyTimeIntervalTrigger)
                {
                    triggerType = TriggerTypeEnum.Daily;
                    // 从触发器获取 执行间隔时间（默认就是秒）
                    interval = TimeSpan.FromSeconds(dailyTimeIntervalTrigger.RepeatInterval).ToString();
                }
                else if (trigger is SimpleTriggerImpl simpleTrigger)
                {
                    triggerType = TriggerTypeEnum.Simple;
                    // 从触发器获取 执行间隔时间
                    interval = simpleTrigger.RepeatInterval.ToString();
                }
                else
                {
                    throw new UserFriendlyException("触发器类型不正确！");
                }

                jobGroupInfo.JobInfoList.Add(new QueryAllSchedulerJobOutput.SchedulerJobInfoDto
                {
                    JobName = jobKey.Name,
                    PreviousFireTime = trigger.GetPreviousFireTimeUtc()?.LocalDateTime,
                    NextFireTime = trigger.GetNextFireTimeUtc()?.LocalDateTime,
                    JobType = jobDetail!.JobDataMap.GetEnum<SchedulerJobTypeEnum>(nameof(SchedulerJobInfo.JobType)),
                    BeginTime = trigger.StartTimeUtc.LocalDateTime,
                    EndTime = trigger.EndTimeUtc?.LocalDateTime
                              ?? jobDetail.JobDataMap.GetNullableDateTime(nameof(SchedulerJobInfo.EndTime)),
                    TriggerType = triggerType,
                    Interval = interval,
                    Description = jobDetail.Description,
                    // 触发器状态
                    TriggerState = await scheduler.GetTriggerState(trigger.Key),
                    RequestUrl = jobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.RequestUrl)),
                    RequestMethod =
                        jobDetail.JobDataMap.GetNullableEnum<HttpRequestMethodEnum>(nameof(SchedulerJobInfo.RequestMethod)),
                    RunNumber = jobDetail.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber)),
                    Exception = jobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.Exception))
                });
            }

            result.Add(jobGroupInfo);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<SchedulerJobInfo> QuerySchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 检查作业是否存在
        if (!await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业不存在！");
        }

        // 获取作业详情
        IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

        // 获取触发器
        IReadOnlyCollection<ITrigger> triggers = await scheduler.GetTriggersOfJob(jobKey);
        ITrigger trigger = triggers.AsEnumerable().LastOrDefault();

        if (trigger == null)
        {
            throw new UserFriendlyException("调度作业对应的触发器不存在！");
        }

        // 从触发器获取 Cron表达式
        string cron = null;
        // 从触发器获取 星期
        List<DayOfWeek> weeks = null;
        // 从触发器获取 每天开始时间
        TimeSpan? dailyStartTime = null;
        // 从触发器获取 每天结束时间
        TimeSpan? dailyEndTime = null;
        // 从触发器获取 执行间隔时间
        double? intervalSecond = null;
        // 从触发器获取 执行次数
        int? runTimes = null;

        // 触发器类型
        TriggerTypeEnum triggerType;
        if (trigger is CronTriggerImpl cronTrigger)
        {
            triggerType = TriggerTypeEnum.Cron;
            cron = cronTrigger.CronExpressionString;
        }
        else if (trigger is DailyTimeIntervalTriggerImpl dailyTimeIntervalTrigger)
        {
            triggerType = TriggerTypeEnum.Daily;
            weeks = dailyTimeIntervalTrigger.DaysOfWeek.ToList();
            dailyStartTime = new TimeSpan(dailyTimeIntervalTrigger.StartTimeOfDay.Hour,
                dailyTimeIntervalTrigger.StartTimeOfDay.Minute, dailyTimeIntervalTrigger.StartTimeOfDay.Second);
            dailyEndTime = new TimeSpan(dailyTimeIntervalTrigger.EndTimeOfDay.Hour, dailyTimeIntervalTrigger.EndTimeOfDay.Minute,
                dailyTimeIntervalTrigger.EndTimeOfDay.Second);
            intervalSecond = dailyTimeIntervalTrigger.RepeatInterval;
            runTimes = dailyTimeIntervalTrigger.RepeatCount == -1 ? null : dailyTimeIntervalTrigger.RepeatCount;
        }
        else if (trigger is SimpleTriggerImpl simpleTrigger)
        {
            triggerType = TriggerTypeEnum.Simple;
            intervalSecond = simpleTrigger.RepeatInterval.TotalSeconds;
            runTimes = simpleTrigger.RepeatCount == -1 ? null : simpleTrigger.RepeatCount;
        }
        else
        {
            throw new UserFriendlyException("触发器类型不正确！");
        }

        // 是否为系统作业
        bool isSystem = jobDetail!.JobDataMap.GetBoolean(nameof(SchedulerJobInfo.IsSystem));
        // 租户Id，系统作业百分百不存在 TenantId
        long? localTenantId = isSystem ? null : jobDetail.JobDataMap.GetLong(nameof(SchedulerJobInfo.TenantId));

        var result = new SchedulerJobInfo
        {
            IsSystem = isSystem,
            TenantId = localTenantId,
            JobName = jobKey.Name,
            JobGroup = Enum.Parse<SchedulerJobGroupEnum>(jobKey.Group),
            JobType = jobDetail.JobDataMap.GetEnum<SchedulerJobTypeEnum>(nameof(SchedulerJobInfo.JobType)),
            BeginTime = trigger.StartTimeUtc.LocalDateTime,
            EndTime =
                trigger.EndTimeUtc?.LocalDateTime ?? jobDetail.JobDataMap.GetNullableDateTime(nameof(SchedulerJobInfo.EndTime)),
            TriggerType = triggerType,
            Cron = cron,
            Week = weeks?.ToWeekEnum(),
            DailyStartTime = dailyStartTime,
            DailyEndTime = dailyEndTime,
            IntervalSecond = intervalSecond != null ? Convert.ToInt32(intervalSecond.Value) : null,
            RunTimes = runTimes,
            WarnTime = jobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.WarnTime)),
            RetryTimes = jobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RetryTimes)),
            RetryMillisecond = jobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RetryMillisecond)),
            MailMessage = jobDetail.JobDataMap.GetEnum<MailMessageEnum>(nameof(SchedulerJobInfo.MailMessage)),
            Description = jobDetail.Description,
            // 触发器状态
            TriggerState = await scheduler.GetTriggerState(trigger.Key),
            RequestUrl = jobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.RequestUrl)),
            RequestMethod = jobDetail.JobDataMap.GetNullableEnum<HttpRequestMethodEnum>(nameof(SchedulerJobInfo.RequestMethod)),
            RequestParams = jobDetail.JobDataMap[nameof(SchedulerJobInfo.RequestParams)] as IDictionary<string, object>,
            RequestHeader = jobDetail.JobDataMap[nameof(SchedulerJobInfo.RequestHeader)] as IDictionary<string, string>,
            RequestTimeout = jobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RequestTimeout)),
            RunNumber = jobDetail.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber)),
            Exception = jobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.Exception)),
            Logs = jobDetail.JobDataMap[nameof(SchedulerJobInfo.Logs)] as List<string>
        };

        return result;
    }

    /// <inheritdoc />
    public async Task AddSchedulerJob(AddSchedulerJobInput input)
    {
        await AddSchedulerJob(new SchedulerJobInfo
        {
            IsSystem = input.TenantId == null,
            TenantId = input.TenantId,
            JobName = input.JobName,
            JobGroup = input.JobGroup,
            JobType = input.JobType,
            BeginTime = input.BeginTime,
            EndTime = input.EndTime,
            TriggerType = input.TriggerType,
            Cron = input.Cron,
            Week = input.Week,
            DailyStartTime = input.DailyStartTime,
            DailyEndTime = input.DailyEndTime,
            IntervalSecond = input.IntervalSecond,
            RunTimes = input.RunTimes,
            WarnTime = input.WarnTime,
            RetryTimes = input.RetryTimes,
            RetryMillisecond = input.RetryMillisecond,
            MailMessage = input.MailMessage,
            Description = input.Description,
            RequestUrl = input.RequestUrl,
            RequestMethod = input.RequestMethod,
            RequestTimeout = input.RequestTimeout,
            RequestParams = input.RequestParams,
            RequestHeader = input.RequestHeader,
            IsAllTenant = false
        });
    }

    /// <inheritdoc />
    public async Task EditSchedulerJob(EditSchedulerJobInput input)
    {
        if (string.IsNullOrWhiteSpace(input.OldJobName))
        {
            throw new UserFriendlyException("旧的调度作业名称不能为空！");
        }

        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(input.TenantId);

        // 获取旧的调度作业Key
        var oldJobKey = new JobKey(input.OldJobName, input.OldJobGroup.ToString());

        // 获取旧作业
        IJobDetail oldJobDetail = await scheduler.GetJobDetail(oldJobKey);

        // 获取旧的调度作业触发器
        IReadOnlyCollection<ITrigger> oldTriggers = await scheduler.GetTriggersOfJob(oldJobKey);
        ITrigger oldTrigger = oldTriggers.AsEnumerable().LastOrDefault();

        if (oldTrigger == null)
        {
            throw new UserFriendlyException("调度作业对应的触发器不存在！");
        }

        // 组装新的添加实体
        var newJobEntity = new SchedulerJobInfo
        {
            IsSystem = input.TenantId == null,
            TenantId = input.TenantId,
            JobName = input.JobName,
            JobGroup = input.JobGroup,
            JobType = input.JobType,
            BeginTime = input.BeginTime,
            EndTime = input.EndTime,
            TriggerType = input.TriggerType,
            Cron = input.Cron,
            Week = input.Week,
            DailyStartTime = input.DailyStartTime,
            DailyEndTime = input.DailyEndTime,
            IntervalSecond = input.IntervalSecond,
            RunTimes = input.RunTimes,
            WarnTime = input.WarnTime,
            RetryTimes = input.RetryTimes,
            RetryMillisecond = input.RetryMillisecond,
            MailMessage = input.MailMessage,
            Description = input.Description,
            RequestUrl = input.RequestUrl,
            RequestMethod = input.RequestMethod,
            RequestTimeout = input.RequestTimeout,
            RequestParams = input.RequestParams,
            RequestHeader = input.RequestHeader,
            IsAllTenant = oldJobDetail.JobDataMap.GetNullableBoolean(nameof(SchedulerJobInfo.IsAllTenant)) ?? false,
            // 触发器状态
            TriggerState = await scheduler.GetTriggerState(oldTrigger.Key),
            RunNumber = oldJobDetail.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber)),
            Exception = oldJobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.Exception)),
            Logs = oldJobDetail.JobDataMap[nameof(SchedulerJobInfo.Logs)] as List<string>
        };

        // 属性验证
        SchedulerJobVerify(newJobEntity);

        // 创建新的调度作业Key
        var newJobKey = new JobKey(newJobEntity.JobName, newJobEntity.JobGroup.ToString());

        // 本地作业禁止修改 JobKey
        if (newJobEntity.JobType == SchedulerJobTypeEnum.Local)
        {
            if (!newJobKey.Equals(oldJobKey))
            {
                throw new UserFriendlyException("本地作业禁止修改调度作业名称和分组！");
            }
        }

        // 判断是否等于旧的调度作业Key
        if (!newJobKey.Equals(oldJobKey))
        {
            // 检查作业是否存在
            if (await scheduler.CheckExists(newJobKey))
            {
                throw new UserFriendlyException("调度作业已存在！");
            }
        }

        // 暂停旧的调度作业
        await scheduler.PauseJob(oldJobKey);
        // 删除旧的调度作业
        await scheduler.DeleteJob(oldJobKey);

        // 添加新的调度作业
        await AddSchedulerJob(scheduler, newJobKey, newJobEntity);
    }

    /// <inheritdoc />
    public async Task DeleteSchedulerJob(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 获取作业详情
        IJobDetail jobDetail = await scheduler.GetJobDetail(jobKey);

        SchedulerJobTypeEnum jobType = jobDetail!.JobDataMap.GetEnum<SchedulerJobTypeEnum>(nameof(SchedulerJobInfo.JobType));
        if (jobType == SchedulerJobTypeEnum.Local)
        {
            throw new UserFriendlyException("禁止删除本地作业！");
        }

        // 先暂停
        await scheduler.PauseJob(jobKey);
        await scheduler.DeleteJob(jobKey);
    }

    /// <inheritdoc />
    public async Task DeleteSchedulerJobException(SchedulerJobKeyInput input, long? tenantId = null)
    {
        // 获取调度器
        IScheduler scheduler = await _schedulerFactory.GetScheduler(tenantId);

        var jobKey = new JobKey(input.JobName, input.JobGroup.ToString());

        // 检查作业是否存在
        if (!await scheduler.CheckExists(jobKey))
        {
            throw new UserFriendlyException("调度作业不存在！");
        }

        // 获取选项
        IOptions<QuartzOptions> options = _serviceProvider.GetService<IOptions<QuartzOptions>>();

        // 获取数据表前缀
        string tablePrefix = options.Value.GetValueOrDefault(
            $"{StdSchedulerFactory.PropertyJobStorePrefix}.{StdSchedulerFactory.PropertyTablePrefix}",
            AdoConstants.DefaultTablePrefix);

        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

        var whereConditionalModel = new List<IConditionalModel>
        {
            new ConditionalModel
            {
                FieldName = AdoConstants.ColumnSchedulerName,
                ConditionalType = ConditionalType.Equal,
                FieldValue = scheduler.SchedulerName
            },
            new ConditionalModel
            {
                FieldName = AdoConstants.ColumnJobName, ConditionalType = ConditionalType.Equal, FieldValue = jobKey.Name
            },
            new ConditionalModel
            {
                FieldName = AdoConstants.ColumnJobGroup,
                ConditionalType = ConditionalType.Equal,
                FieldValue = jobKey.Group
            }
        };
        var selectModels = new List<SelectModel>
        {
            new() {FieldName = AdoConstants.ColumnJobDataMap, AsName = nameof(QueryableJobDataResult.JobData)}
        };
        QueryableJobDataResult queryResult = await db.Queryable<QueryableJobDataResult>()
            .AS($"{tablePrefix}{AdoConstants.TableJobDetails}")
            .Where(whereConditionalModel)
            .Select(selectModels)
            .SingleAsync();

        if (queryResult.JobData != null)
        {
            var jobData = JObject.Parse(Encoding.UTF8.GetString(queryResult.JobData.ToArray()));
            // 移除异常日志
            if (jobData.ContainsKey(nameof(SchedulerJobInfo.Exception)))
            {
                jobData[nameof(SchedulerJobInfo.Exception)] = string.Empty;
            }

            // 执行更新
            var modelDict = new Dictionary<string, object>
            {
                {AdoConstants.ColumnSchedulerName, scheduler.SchedulerName},
                {AdoConstants.ColumnJobName, jobKey.Name},
                {AdoConstants.ColumnJobGroup, jobKey.Group},
                {AdoConstants.ColumnJobDataMap, Encoding.UTF8.GetBytes(jobData.ToString())}
            };
            await db.Updateable(modelDict)
                .AS($"{tablePrefix}{AdoConstants.TableJobDetails}")
                .WhereColumns(AdoConstants.ColumnSchedulerName, AdoConstants.ColumnJobName, AdoConstants.ColumnJobGroup)
                .ExecuteCommandAsync();
        }
    }

    private protected class QueryableJobDataResult
    {
        public byte[] JobData { get; set; }
    }
}
