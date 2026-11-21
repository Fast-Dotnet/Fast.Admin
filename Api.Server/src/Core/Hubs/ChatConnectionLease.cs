// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace Fast.Core;

/// <summary>
/// ChatHub 连接存活租约
/// </summary>
public sealed class ChatConnectionLease
{
    /// <summary>
    /// 租约有效期
    /// </summary>
    public static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(3);

    /// <summary>
    /// 租约缺失确认时间
    /// </summary>
    public static readonly TimeSpan MissingConfirmationDuration = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 分布式缓存
    /// </summary>
    private readonly IDistributedCache _distributedCache;

    /// <summary>
    /// 连接信息
    /// </summary>
    private readonly ConcurrentDictionary<string, CancellationToken> _connections = new();

    /// <summary>
    /// ChatHub 连接存活租约
    /// </summary>
    public ChatConnectionLease(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    /// <summary>
    /// 获取连接租约缓存键。
    /// </summary>
    public static string GetLeaseKey(string connectionId)
    {
        return $"SignalR:Chat:Lease:{connectionId}";
    }

    /// <summary>
    /// 获取租约首次缺失时间缓存键。
    /// </summary>
    public static string GetMissingKey(string connectionId)
    {
        return $"SignalR:Chat:Missing:{connectionId}";
    }

    /// <summary>
    /// 登记连接，首次租约写入成功后才允许继续登录。
    /// </summary>
    public async Task RegisterAsync(string connectionId, CancellationToken connectionAborted)
    {
        await RenewAsync(connectionId, connectionAborted);
        _connections[connectionId] = connectionAborted;
    }

    /// <summary>
    /// 停止续租；已写入的租约由缓存自动过期。
    /// </summary>
    public void Unregister(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    /// <summary>
    /// 为本实例中尚未断开的连接续租。
    /// </summary>
    public async Task RenewAllAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<string, CancellationToken> connection in _connections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.Value.IsCancellationRequested)
            {
                _connections.TryRemove(connection.Key, out _);
                continue;
            }

            await RenewAsync(connection.Key, cancellationToken);
        }
    }

    private Task RenewAsync(string connectionId, CancellationToken cancellationToken)
    {
        return _distributedCache.SetStringAsync(GetLeaseKey(connectionId), "1",
            new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = LeaseDuration}, cancellationToken);
    }
}
