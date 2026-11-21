// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.SignalR;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 实时消息通知服务
/// </summary>
public sealed class NotificationService : INotificationService, IScopedDependency
{
    /// <summary>
    /// 授权用户信息
    /// </summary>
    private readonly IUser _user;

    /// <summary>
    /// SqlSugar 客户端
    /// </summary>
    private readonly ISqlSugarClient _repository;

    /// <summary>
    /// 集线器
    /// </summary>
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    /// <summary>
    /// 实时消息通知服务
    /// </summary>
    public NotificationService(IUser user, ISqlSugarClient repository, IHubContext<ChatHub, IChatClient> hubContext)
    {
        _user = user;
        _repository = repository;
        _hubContext = hubContext;
    }

    /// <inheritdoc />
    public async Task SendAsync(HubNotificationTypeEnum type, string message, params long[] employeeIds)
    {
        await SendAsync(type, [message], employeeIds);
    }

    /// <inheritdoc />
    public async Task SendAsync(HubNotificationTypeEnum type, IEnumerable<string> messages, params long[] employeeIds)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(employeeIds);

        // 不允许从无租户授权上下文中猜测或跨租户发送。
        if (_user.TenantId <= 0)
        {
            throw new InvalidOperationException("当前请求不存在有效的租户授权信息。");
        }

        var recipients = employeeIds.Where(id => id > 0).Distinct().ToList();
        var contents = messages.Where(message => !string.IsNullOrWhiteSpace(message)).Select(message => message.Trim()).ToList();
        if (recipients.Count == 0 || contents.Count == 0)
        {
            return;
        }

        long tenantId = _user.TenantId;
        List<string> connectionIds = await _repository.Queryable<TenantOnlineUserModel>()
            .ClearFilter<IBaseTEntity>()
            .Where(wh => wh.IsOnline && recipients.Contains(wh.EmployeeId))
            .Where(wh => wh.TenantId == tenantId)
            .Select(sl => sl.ConnectionId)
            .ToListAsync();

        connectionIds = connectionIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (connectionIds.Count == 0)
        {
            return;
        }

        await _hubContext.Clients.Clients(connectionIds)
            .ReceiveNotification(new RealtimeNotificationInput {Type = type.ToString().ToLowerInvariant(), Messages = contents});
    }
}
