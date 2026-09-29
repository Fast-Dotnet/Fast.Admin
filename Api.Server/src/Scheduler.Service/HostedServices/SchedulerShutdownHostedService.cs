// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// 调度器关闭托管服务
/// </summary>
internal sealed class SchedulerShutdownHostedService : IHostedService
{
    /// <summary>
    /// 调度器工厂
    /// </summary>
    private readonly IDependencySchedulerFactory _schedulerFactory;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 调度器关闭托管服务
    /// </summary>
    public SchedulerShutdownHostedService(IDependencySchedulerFactory schedulerFactory,
        ILogger<SchedulerShutdownHostedService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<IScheduler> schedulerList = await _schedulerFactory.GetAllSchedulers(cancellationToken);
        foreach (IScheduler scheduler in schedulerList)
        {
            string schedulerName = scheduler.SchedulerName;
            string schedulerInstanceId = scheduler.SchedulerInstanceId;

            if (!scheduler.IsShutdown)
            {
                await scheduler.Shutdown(true, cancellationToken);
            }

            if (!SchedulerContext.IsExecutionHost)
            {
                continue;
            }

            try
            {
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

                // Quartz 正常关闭不会删除集群心跳，执行宿主需要主动移除当前实例状态
                using var cleanupCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                await db.Deleteable<QuartzSchedulerStateModel>()
                    .Where(wh => wh.SchedName == schedulerName && wh.InstanceName == schedulerInstanceId)
                    .ExecuteCommandAsync(cleanupCancellationTokenSource.Token);

                _logger.LogInformation("Remove scheduler {SchedulerName} instance {SchedulerInstanceId} state on shutdown.",
                    schedulerName, schedulerInstanceId);
            }
            catch (Exception ex)
            {
                // 清理失败时保留 Quartz 原有的心跳超时机制，避免影响宿主正常退出
                _logger.LogError(ex, "Remove scheduler {SchedulerName} instance {SchedulerInstanceId} state on shutdown failed.",
                    schedulerName, schedulerInstanceId);
            }
        }
    }
}
