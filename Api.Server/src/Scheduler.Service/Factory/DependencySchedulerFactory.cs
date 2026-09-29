// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Util;


// ReSharper disable once CheckNamespace
namespace Quartz;

/// <summary>
/// <see cref="IDependencySchedulerFactory"/> 默认实现
/// </summary>
internal sealed class DependencySchedulerFactory : StdSchedulerFactory, IDependencySchedulerFactory
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 容器配置处理器
    /// </summary>
    private readonly ContainerConfigurationProcessor _processor;

    /// <summary>
    /// 调度器仓库
    /// </summary>
    private readonly ISchedulerRepository _schedulerRepository;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 异步线程锁
    /// </summary>
    private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    public DependencySchedulerFactory(IServiceProvider serviceProvider, ContainerConfigurationProcessor processor,
        ISchedulerRepository schedulerRepository, ILogger<IDependencySchedulerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _processor = processor;
        _schedulerRepository = schedulerRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IScheduler> GetScheduler(long? tenantId = null, CancellationToken cancellationToken = new())
    {
        // 获取锁
        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 调度器名称
            string schedulerName = tenantId != null ? $"TenantScheduler_{tenantId}" : "CoreScheduler";

            // 判断是否已经存在调度器
            IScheduler scheduler = _schedulerRepository.Lookup(schedulerName);

            if (scheduler != null)
            {
                return scheduler;
            }

            // 获取选项
            IOptions<QuartzOptions> options = _serviceProvider.GetService<IOptions<QuartzOptions>>();

            // 放入选项
            options.Value[PropertySchedulerInstanceName] = schedulerName;

            /*
             * 初始化配置
             * 这里是直接覆盖选项
             * 虽然我不知道覆盖后续会有问题问题发生
             * 但是居于目前 v3.13.1 版本，经过本人两天研究源码发现除了在改变源码的情况下，没有更好的解决方案了
             */
            Initialize(options.Value.ToNameValueCollection());

            // 获取调度器
            scheduler = await GetScheduler(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(tenantId != null
                ? $"Create tenant scheduler by {tenantId} success."
                : "Create scheduler success.");

            return scheduler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, tenantId != null ? $"Create tenant scheduler by {tenantId} error." : "Create scheduler error.");
            throw;
        }
        finally
        {
            // 释放锁
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public override async Task<IScheduler> GetScheduler(string schedulerName, CancellationToken cancellationToken = new())
    {
        // 获取锁
        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 判断是否已经存在调度器
            IScheduler scheduler = _schedulerRepository.Lookup(schedulerName);

            if (scheduler != null)
            {
                return scheduler;
            }

            // 获取选项
            IOptions<QuartzOptions> options = _serviceProvider.GetService<IOptions<QuartzOptions>>();

            // 放入选项
            options.Value[PropertySchedulerInstanceName] = schedulerName;

            /*
             * 初始化配置
             * 这里是直接覆盖选项
             * 虽然我不知道覆盖后续会有问题问题发生
             * 但是居于目前 v3.13.1 版本，经过本人两天研究源码发现除了在改变源码的情况下，没有更好的解决方案了
             */
            Initialize(options.Value.ToNameValueCollection());

            // 获取调度器
            scheduler = await GetScheduler(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation($"Create {schedulerName} scheduler success.");

            return scheduler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Create {schedulerName} scheduler error.");
            throw;
        }
        finally
        {
            // 释放锁
            semaphoreSlim.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IScheduler> TryGetScheduler(string schedulerName)
    {
        return await Task.FromResult(_schedulerRepository.Lookup(schedulerName));
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IScheduler>> GetAllSchedulers(CancellationToken cancellationToken = new())
    {
        return await Task.FromResult(_schedulerRepository.LookupAll());
    }

    /// <inheritdoc />
    public override async Task<IScheduler> GetScheduler(CancellationToken cancellationToken = default)
    {
        // 获取调度器，这里需要注意的是：如果原来的调度器被停止了，则调用 GetScheduler 会返回一个新的调度器
        IScheduler scheduler = await base.GetScheduler(cancellationToken).ConfigureAwait(false);

        // 初始化调度器
        await InitializeScheduler(scheduler, cancellationToken).ConfigureAwait(false);

        return scheduler;
    }

    /// <summary>
    /// 初始化调度器
    /// </summary>
    /// <param name="scheduler">Quartz 调度器</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌</param>
    private async Task InitializeScheduler(IScheduler scheduler, CancellationToken cancellationToken)
    {
        foreach (ISchedulerListener listener in _serviceProvider.GetServices<ISchedulerListener>())
        {
            scheduler.ListenerManager.AddSchedulerListener(listener);
        }

        IEnumerable<IJobListener> jobListeners = _serviceProvider.GetServices<IJobListener>();
        JobListenerConfiguration[] jobListenerConfigurations = _serviceProvider.GetServices<JobListenerConfiguration>().ToArray();
        foreach (IJobListener listener in jobListeners)
        {
            JobListenerConfiguration configuration =
                jobListenerConfigurations.SingleOrDefault(x => x.ListenerType == listener.GetType());
            scheduler.ListenerManager.AddJobListener(listener, configuration?.Matchers ?? []);
        }

        IEnumerable<ITriggerListener> triggerListeners = _serviceProvider.GetServices<ITriggerListener>();
        TriggerListenerConfiguration[] triggerListenerConfigurations = _serviceProvider
            .GetServices<TriggerListenerConfiguration>()
            .ToArray();
        foreach (ITriggerListener listener in triggerListeners)
        {
            TriggerListenerConfiguration configuration =
                triggerListenerConfigurations.SingleOrDefault(x => x.ListenerType == listener.GetType());
            scheduler.ListenerManager.AddTriggerListener(listener, configuration?.Matchers ?? []);
        }

        IEnumerable<CalendarConfiguration> calendars = _serviceProvider.GetServices<CalendarConfiguration>();
        foreach (CalendarConfiguration configuration in calendars)
        {
            await scheduler.AddCalendar(configuration.Name, configuration.Calendar, configuration.Replace,
                    configuration.UpdateTriggers, cancellationToken)
                .ConfigureAwait(false);
        }

        await _processor.ScheduleJobs(scheduler, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override ISchedulerRepository GetSchedulerRepository()
    {
        return _schedulerRepository;
    }

    /// <inheritdoc />
    protected override IDbConnectionManager GetDBConnectionManager()
    {
        return _serviceProvider.GetService<IDbConnectionManager>();
    }

    /// <inheritdoc />
    protected override string GetNamedConnectionString(string connectionStringName)
    {
        IConfiguration configuration = _serviceProvider.GetService<IConfiguration>();
        string connectionString = configuration?.GetConnectionString(connectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        return base.GetNamedConnectionString(connectionStringName);
    }

    /// <inheritdoc />
    protected override T InstantiateType<T>(Type implementationType)
    {
        T service = _serviceProvider.GetService<T>();
        if (service is null)
        {
            service = ObjectUtils.InstantiateType<T>(implementationType);
        }

        return service;
    }
}
