// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Globalization;
using Fast.Center.Domain;
using Fast.Core;
using Fast.SqlSugar;
using Microsoft.Extensions.Caching.Distributed;
using SqlSugar;

namespace Fast.Scheduler.LocalJob;

/// <summary>
/// 清理失效在线连接及过期离线记录本地作业
/// </summary>
public class CleanTenantOnlineUserLocalJob : ISchedulerJob
{
    /// <inheritdoc />
    public SchedulerLocalJobInfo GetLocalJob()
    {
        return new SchedulerLocalJobInfo
        {
            JobName = "清理失效在线连接本地作业",
            JobGroup = SchedulerJobGroupEnum.System,
            BeginTime = new DateTime(1970, 1, 1),
            EndTime = null,
            TriggerType = TriggerTypeEnum.Simple,
            Cron = null,
            Week = null,
            DailyStartTime = null,
            DailyEndTime = null,
            IntervalSecond = 60,
            RunTimes = null,
            WarnTime = null,
            RetryTimes = null,
            RetryMillisecond = null,
            MailMessage = MailMessageEnum.Error,
            Description = "每分钟检查失效连接，并删除离线超过24小时的记录。"
        };
    }

    /// <inheritdoc />
    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
    {
        IDistributedCache cache = serviceProvider.GetRequiredService<IDistributedCache>();

        DateTime dateTime = DateTime.Now;
        DateTime loginCutoff = dateTime.Subtract(ChatConnectionLease.LeaseDuration);
        long nowMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 新连接先留出一个租约周期，不参与本轮清理。
        List<string> connectionIds = await db.Queryable<TenantOnlineUserModel>()
            .ClearFilter<IBaseTEntity>()
            .Where(wh => wh.IsOnline && wh.LastLoginTime < loginCutoff)
            .Select(sl => sl.ConnectionId)
            .ToListAsync();

        var expiredConnectionIds = new List<string>();

        // 所有存活判断先完成，再更新数据库。
        foreach (string connectionId in connectionIds.Distinct())
        {
            string leaseKey = ChatConnectionLease.GetLeaseKey(connectionId);
            string missingKey = ChatConnectionLease.GetMissingKey(connectionId);

            if (await cache.GetStringAsync(leaseKey) != null)
            {
                await cache.RemoveAsync(missingKey);
                continue;
            }

            string missingValue = await cache.GetStringAsync(missingKey);

            if (missingValue == null)
            {
                await cache.SetStringAsync(missingKey, nowMilliseconds.ToString(CultureInfo.InvariantCulture),
                    new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)});

                continue;
            }

            if (!long.TryParse(missingValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out long firstMissingMilliseconds))
            {
                throw new InvalidOperationException("ChatHub 租约缺失时间缓存格式错误。");
            }

            if (nowMilliseconds - firstMissingMilliseconds < ChatConnectionLease.MissingConfirmationDuration.TotalMilliseconds)
            {
                continue;
            }

            // 确认期间已恢复续租的连接，本轮不下线。
            if (await cache.GetStringAsync(leaseKey) == null)
            {
                expiredConnectionIds.Add(connectionId);
            }
        }

        int offlineCount = 0;

        foreach (string[] batch in expiredConnectionIds.Chunk(200))
        {
            offlineCount += await db.Updateable<TenantOnlineUserModel>()
                .SetColumns(_ => new TenantOnlineUserModel {IsOnline = false, OfflineTime = dateTime})
                .Where(wh => batch.Contains(wh.ConnectionId) && wh.IsOnline && wh.LastLoginTime < loginCutoff)
                .ExecuteCommandAsync();
        }

        DateTime offlineCutoff = dateTime.AddHours(-24);

        int deleteCount = await db.Deleteable<TenantOnlineUserModel>()
            .Where(wh => !wh.IsOnline && wh.OfflineTime != null && wh.OfflineTime < offlineCutoff)
            .ExecuteCommandAsync();

        return $"失效连接下线：{offlineCount}条；删除过期离线记录：{deleteCount}条。";
    }
}
