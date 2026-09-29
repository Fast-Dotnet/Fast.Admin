// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.CenterLog.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 逐条持久化 <see cref="SqlSugarEntityHandler"/> 产生的 SQL 日志
/// </summary>
/// <remarks>
/// 服务固定使用单消费者，不批量合并日志；停止宿主时先关闭写入端，再等待通道中的现有日志排空
/// </remarks>
[Order(101)]
public sealed class SqlSugarLogBackgroundService : BackgroundService
{
    /// <summary>
    /// SQL 日志专用通道
    /// </summary>
    private readonly SqlSugarLogChannel _sqlSugarLogChannel;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 SQL 日志消费者
    /// </summary>
    public SqlSugarLogBackgroundService(SqlSugarLogChannel sqlSugarLogChannel, ILogger<SqlSugarLogBackgroundService> logger)
    {
        _sqlSugarLogChannel = sqlSugarLogChannel;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (SqlSugarLogChannel.SqlSugarLogWorkItem workItem in _sqlSugarLogChannel.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // 独立客户端不加载 AOP，避免写日志时再次产生 SQL 日志
                    using var db = new SqlSugarClient(workItem.ConnectionConfig);
                    switch (workItem.LogModel)
                    {
                        case SqlExecutionLogModel sqlExecutionLogModel:
                            await db.Insertable(sqlExecutionLogModel).SplitTable().ExecuteCommandAsync();
                            break;
                        case SqlTimeoutLogModel sqlTimeoutLogModel:
                            await db.Insertable(sqlTimeoutLogModel).ExecuteCommandAsync(stoppingToken);
                            break;
                        case SqlDiffLogModel sqlDiffLogModel:
                            await db.Insertable(sqlDiffLogModel).SplitTable().ExecuteCommandAsync();
                            break;
                        case SqlExceptionLogModel sqlExceptionLogModel:
                            await db.Insertable(sqlExceptionLogModel).ExecuteCommandAsync(stoppingToken);
                            break;
                        default:
                            _logger.LogError("不支持的 SQL 日志类型 {LogType}。", workItem.LogModel.GetType().FullName);
                            break;
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "SQL 日志 {LogType} 写入失败。", workItem.LogModel.GetType().Name);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 宿主停止时取消通道读取或当前日志写入属于正常停机流程
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // 正常停机先拒绝新日志，再等待通道排空；超过宿主停机时限后由基类取消消费者
        _sqlSugarLogChannel.Complete();

        if (ExecuteTask == null)
        {
            return;
        }

        try
        {
            await ExecuteTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
