// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Scheduler.BackgroundServices;

/// <summary>
/// 调度后台托管服务
/// </summary>
public class SchedulerHostedService : BackgroundService
{
    /// <summary>
    /// 托管应用程序生命周期
    /// </summary>
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    /// <summary>
    /// 调度中心
    /// </summary>
    private readonly ISchedulerCenter _schedulerCenter;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 调度后台托管服务
    /// </summary>
    public SchedulerHostedService(IHostApplicationLifetime hostApplicationLifetime, ISchedulerCenter schedulerCenter,
        ILogger<SchedulerHostedService> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _schedulerCenter = schedulerCenter;
        _logger = logger;
    }

    /// <summary>
    /// 执行
    /// </summary>
    private async Task SyncSchedulerAsync()
    {
        try
        {
            // 同步调度程序
            await _schedulerCenter.SyncScheduler();
        }
        catch (Exception ex)
        {
            // 同步调度器错误
            _logger.LogError(ex, $"Sync scheduler error. {ex.Message}");
        }
    }

    /// <summary>
    /// 同步调度程序运行状态
    /// </summary>
    private async Task SyncSchedulerStateAsync()
    {
        try
        {
            await _schedulerCenter.SyncSchedulerState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Sync scheduler state error. {ex.Message}");
        }
    }

    /// <summary>
    /// 在应用启动完成后初始化调度器，并每半小时同步一次调度定义
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!_hostApplicationLifetime.ApplicationStarted.IsCancellationRequested)
            {
                var startedCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await using CancellationTokenRegistration startedRegistration =
                    _hostApplicationLifetime.ApplicationStarted.Register(() => startedCompletion.TrySetResult());
                await using CancellationTokenRegistration stoppingRegistration =
                    stoppingToken.Register(() => startedCompletion.TrySetCanceled(stoppingToken));
                await startedCompletion.Task;
            }

            try
            {
                // 初始化调度程序；初始化失败时不能继续提供半可用的调度服务
                await _schedulerCenter.InitializeScheduler();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Initialize scheduler error. Scheduler host will stop.");
                _hostApplicationLifetime.StopApplication();
                return;
            }

            // 启动完成后立即同步一次，确保运行期间新增的租户调度器能够及时加载
            await SyncSchedulerAsync();

            DateTime dateTime = DateTime.Now;
            var nextExecTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
                dateTime.Minute < 30 ? 30 : 0, 0);
            if (nextExecTime <= dateTime)
            {
                nextExecTime = nextExecTime.AddHours(1);
            }

            _logger.LogInformation("Next execute sync scheduler time {NextExecuteTime:yyyy-MM-dd HH:mm:ss}", nextExecTime);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SyncSchedulerStateAsync();

                if (DateTime.Now < nextExecTime)
                {
                    continue;
                }

                // 串行等待每次同步完成，禁止 Timer 回调重叠造成重复调度或数据库并发写入
                await SyncSchedulerAsync();
                do
                {
                    nextExecTime = nextExecTime.AddMinutes(30);
                } while (nextExecTime <= DateTime.Now);

                _logger.LogInformation("Next execute sync scheduler time {NextExecuteTime:yyyy-MM-dd HH:mm:ss}", nextExecTime);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 宿主停止时取消启动等待、延迟或定时器属于正常停机流程
        }
    }
}
