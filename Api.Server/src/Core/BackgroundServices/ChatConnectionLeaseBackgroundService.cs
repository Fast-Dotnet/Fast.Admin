// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fast.Core;

/// <summary>
/// 定期续租本实例的 ChatHub 连接
/// </summary>
public sealed class ChatConnectionLeaseBackgroundService : BackgroundService
{
    /// <summary>
    /// ChatHub 连接存活租约
    /// </summary>
    private readonly ChatConnectionLease _chatConnectionLease;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<ChatConnectionLeaseBackgroundService> _logger;

    /// <summary>
    /// 定期续租本实例的 ChatHub 连接
    /// </summary>
    public ChatConnectionLeaseBackgroundService(ChatConnectionLease chatConnectionLease,
        ILogger<ChatConnectionLeaseBackgroundService> logger)
    {
        _chatConnectionLease = chatConnectionLease;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await _chatConnectionLease.RenewAllAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ChatHub 连接租约续期失败，将在下一周期重试。");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 应用正常停止。
        }
    }
}
