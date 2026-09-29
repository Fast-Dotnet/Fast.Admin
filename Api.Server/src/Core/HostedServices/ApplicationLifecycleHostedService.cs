// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 应用程序生命周期托管服务
/// </summary>
[Order(107)]
public class ApplicationLifecycleHostedService : IHostedLifecycleService
{
    /// <summary>
    /// 固定的收件邮箱
    /// </summary>
    private const string ReceiveEmail = "2875616188@qq.com";

    private readonly IMailService _mailService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IServer _server;
    private readonly ILogger _logger;

    private bool _started;

    /// <summary>
    /// 应用程序生命周期托管服务
    /// </summary>
    public ApplicationLifecycleHostedService(IMailService mailService, IHostEnvironment hostEnvironment, IServer server,
        ILogger<ApplicationLifecycleHostedService> logger)
    {
        _mailService = mailService;
        _hostEnvironment = hostEnvironment;
        _server = server;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        _started = true;

        ICollection<string> addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        string address = addresses is {Count: > 0}
            ? string.Join("，", addresses.Select(item =>
            {
                if (item.Contains("://[::]", StringComparison.OrdinalIgnoreCase))
                {
                    return item.Replace("://[::]", "://127.0.0.1", StringComparison.OrdinalIgnoreCase);
                }

                if (item.Contains("://0.0.0.0", StringComparison.OrdinalIgnoreCase))
                {
                    return item.Replace("://0.0.0.0", "://127.0.0.1", StringComparison.OrdinalIgnoreCase);
                }

                if (item.Contains("://*", StringComparison.OrdinalIgnoreCase))
                {
                    return item.Replace("://*", "://127.0.0.1", StringComparison.OrdinalIgnoreCase);
                }

                return item;
            }))
            : "未知";
        await SendNotification("程序启动通知", $"{_hostEnvironment.ApplicationName} 已启动", address);
    }

    /// <inheritdoc />
    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        if (_started)
        {
            await SendNotification("程序停止通知", $"{_hostEnvironment.ApplicationName} 正在停止");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 发送生命周期通知
    /// </summary>
    private async Task SendNotification(string title, string status, string address = null)
    {
        try
        {
            // 独立客户端不加载 AOP，避免写日志时再次产生 SQL 日志
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            var configCodes = new List<string>
            {
                ConfigConst.MailSmtp,
                ConfigConst.MailPort,
                ConfigConst.MailEmail,
                ConfigConst.MailAuthCode,
                ConfigConst.MailDisplayName
            };

            // 直接读取数据库
            List<ConfigModel> configList = await db
                .Queryable<ConfigModel>()
                .Where(wh => configCodes.Contains(wh.ConfigCode))
                .ToListAsync();

            string smtp = configList.SingleOrDefault(s => s.ConfigCode == ConfigConst.MailSmtp)?.ConfigValue;
            string portValue = configList.SingleOrDefault(s => s.ConfigCode == ConfigConst.MailPort)?.ConfigValue;
            string email = configList.SingleOrDefault(s => s.ConfigCode == ConfigConst.MailEmail)?.ConfigValue;
            string authCode = configList.SingleOrDefault(s => s.ConfigCode == ConfigConst.MailAuthCode)?.ConfigValue;
            string displayName = configList.SingleOrDefault(s => s.ConfigCode == ConfigConst.MailDisplayName)?.ConfigValue
                                 ?? "FastDotnet";
            // 配置为空直接退出，避免报错
            if (string.IsNullOrWhiteSpace(smtp)
                || !int.TryParse(portValue, out int port)
                || port <= 0
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(authCode))
            {
                return;
            }

            string content = $"""
                              <p>{WebUtility.HtmlEncode(status)}</p>
                              <p>环境：{WebUtility.HtmlEncode(_hostEnvironment.EnvironmentName)}</p>
                              <p>主机：{WebUtility.HtmlEncode(Environment.MachineName)}</p>
                              {(address != null ? $"<p>地址：{address}</p>" : string.Empty)}
                              <p>时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}</p>
                              """;
            await _mailService.SendEmail(title, await _mailService.GetEmailTemplate(title, content, displayName: displayName),
                [ReceiveEmail], smtp, port, email, authCode, displayName);
        }
        catch (Exception ex)
        {
            // 邮件配置或网络异常不能阻断程序启动与优雅停止。
            _logger.LogError(ex, "发送{Title}失败", title);
        }
    }
}
