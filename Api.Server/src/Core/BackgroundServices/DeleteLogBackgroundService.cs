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
/// 删除日志后台服务
/// </summary>
[Order(1)]
public class DeleteLogBackgroundService : BackgroundService
{
    /// <summary>
    /// 最大保留天数
    /// </summary>
    private const int MaxRetainDay = 90;

    /// <summary>
    /// 应用程序生命周期
    /// </summary>
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 删除日志托管服务
    /// </summary>
    public DeleteLogBackgroundService(IHostApplicationLifetime hostApplicationLifetime,
        ILogger<DeleteLogBackgroundService> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    /// <summary>
    /// 处理空文件夹
    /// </summary>
    /// <param name="stopDirectory">停止递归扫描的根目录</param>
    /// <param name="directoryInfo">当前扫描目录</param>
    private void HandleEmptyDirectory(string stopDirectory, DirectoryInfo directoryInfo)
    {
        while (directoryInfo != null)
        {
            // 如果和日志文件相同则停止
            if (string.Equals(directoryInfo.FullName, stopDirectory, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            // 检查目录是否为空
            if (!directoryInfo.EnumerateFileSystemInfos().Any())
            {
                // 上级目录
                DirectoryInfo parent = directoryInfo.Parent;
                directoryInfo.Delete();
                directoryInfo = parent;
            }
            else
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // 日志组件会在启动阶段创建当前日志文件；等待应用完全启动并写入启动日志后再检查空文件
            if (!_hostApplicationLifetime.ApplicationStarted.IsCancellationRequested)
            {
                var applicationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await using CancellationTokenRegistration registration =
                    _hostApplicationLifetime.ApplicationStarted.Register(() => applicationStarted.TrySetResult());
                await applicationStarted.Task.WaitAsync(stoppingToken);
            }

            string logPath = Path.Combine(Environment.CurrentDirectory, "logs");

            if (Directory.Exists(logPath))
            {
                // 空文件数量
                int emptyFileNum = 0;
                int oldFileNum = 0;

                try
                {
                    // SearchPattern 由运行时按当前平台处理路径分隔符，可同时支持 Windows、Linux 和 macOS
                    IEnumerable<string> matchedFiles = Directory.EnumerateFiles(logPath, "*.log", SearchOption.AllDirectories);

                    foreach (string filePath in matchedFiles)
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                        var fileInfo = new FileInfo(filePath);

                        // 删除空文件
                        if (fileInfo.Length == 0)
                        {
                            try
                            {
                                File.Delete(filePath);
                                HandleEmptyDirectory(logPath, new DirectoryInfo(Path.GetDirectoryName(filePath)));
                                emptyFileNum++;
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                        // 删除超过最大保留天数的文件
                        else if ((DateTime.Now.Date - fileInfo.LastWriteTime.Date).TotalDays > MaxRetainDay)
                        {
                            try
                            {
                                File.Delete(filePath);
                                HandleEmptyDirectory(logPath, new DirectoryInfo(Path.GetDirectoryName(filePath)));
                                oldFileNum++;
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Delete log error...");
                }

                MAppContext.ConsoleWrite(console =>
                {
                    console.BackgroundColor = ConsoleColor.Black;
                    console.ForegroundColor = ConsoleColor.Green;
                    console.Write("info");
                    console.ResetColor();
                    console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                    console.BackgroundColor = ConsoleColor.Black;
                    console.ForegroundColor = ConsoleColor.DarkGray;
                    console.WriteLine($"      删除日志文件，空文件 {emptyFileNum} 个，超过最长保留{MaxRetainDay}天文件 {oldFileNum} 个。");
                });
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 宿主停止时取消启动等待或日志扫描属于正常停机流程
        }
    }
}
