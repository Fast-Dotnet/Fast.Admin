// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Core;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SqlSugar;

namespace Fast.Scheduler.LocalJob;

/// <summary>
/// 刷新微信 AccessToken 本地作业
/// </summary>
public class RefreshWeChatAccessTokenLocalJob : ISchedulerJob
{
    /// <inheritdoc />
    public SchedulerLocalJobInfo GetLocalJob()
    {
        return new SchedulerLocalJobInfo
        {
            JobName = "刷新微信 AccessToken 本地作业",
            JobGroup = SchedulerJobGroupEnum.System,
            BeginTime = new DateTime(1970, 01, 01),
            EndTime = null,
            TriggerType = TriggerTypeEnum.Simple,
            Cron = null,
            Week = null,
            DailyStartTime = null,
            DailyEndTime = null,
            IntervalSecond = 7200,
            RunTimes = null,
            WarnTime = null,
            RetryTimes = null,
            RetryMillisecond = null,
            MailMessage = MailMessageEnum.Error,
            Description = "刷新微信 AccessToken 本地作业，每7200秒执行一次。"
        };
    }

    /// <inheritdoc />
    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
    {
        // 进入方法的一瞬间记录时间
        DateTime dateTime = DateTime.Now;

        int successCount = 0;
        int errorCount = 0;

        // 获取所有微信小程序信息
        var applicationOpenIdList = await db.Queryable<ApplicationOpenIdModel>()
            .Where(wh => (wh.AppType & (AppEnvironmentEnum.MiniProgram | AppEnvironmentEnum.WeChatServiceAccount)) != 0)
            .Where(wh => !string.IsNullOrWhiteSpace(wh.OpenSecret))
            .Select(sl => new {sl.RecordId, sl.AppType, sl.OpenId, sl.OpenSecret})
            .ToListAsync();

        foreach (var item in applicationOpenIdList)
        {
            try
            {
                WechatApiClient apiClient = WechatApiClientBuilder
                    .Create(new WechatApiClientOptions {AppId = item.OpenId, AppSecret = item.OpenSecret})
                    .Build();
                CgibinStableTokenResponse response =
                    await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest {ForceRefresh = true});
                if (!response.IsSuccessful())
                {
                    errorCount++;
                    await logInfo.ErrorLog(logInfo.JobName, null,
                        $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
                    continue;
                }

                await db.Updateable<ApplicationOpenIdModel>()
                    .SetColumns(_ => new ApplicationOpenIdModel
                    {
                        WeChatAccessToken = response.AccessToken,
                        WeChatAccessTokenExpiresIn = response.ExpiresIn,
                        WeChatAccessTokenRefreshTime = dateTime
                    })
                    .Where(wh => wh.RecordId == item.RecordId)
                    .ExecuteCommandAsync();
                // 删除缓存
                await ApplicationContext.DeleteApplication(item.OpenId);

                // 公众号才需要 ticket
                if (item.AppType == AppEnvironmentEnum.WeChatServiceAccount)
                {
                    CgibinTicketGetTicketResponse ticketResponse =
                        await apiClient.ExecuteCgibinTicketGetTicketAsync(
                            new CgibinTicketGetTicketRequest {AccessToken = response.AccessToken});
                    if (!ticketResponse.IsSuccessful())
                    {
                        errorCount++;
                        await logInfo.ErrorLog(logInfo.JobName, null,
                            $"调用获取Ticket接口失败。ErrorCode：{ticketResponse.ErrorCode}。ErrorMessage：{ticketResponse.ErrorMessage}");
                        continue;
                    }

                    await db.Updateable<ApplicationOpenIdModel>()
                        .SetColumns(_ => new ApplicationOpenIdModel
                        {
                            WeChatJsApiTicket = ticketResponse.Ticket,
                            WeChatJsApiTicketExpiresIn = ticketResponse.ExpiresIn,
                            WeChatJsApiTicketRefreshTime = dateTime
                        })
                        .Where(wh => wh.RecordId == item.RecordId)
                        .ExecuteCommandAsync();
                    // 删除缓存
                    await ApplicationContext.DeleteApplication(item.OpenId);
                }

                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                await logInfo.ErrorLog(logInfo.JobName, ex, $"执行异常 AppId={item.OpenId}");
            }
        }

        return $"刷新完成，总数：{applicationOpenIdList.Count}，成功：{successCount}个，失败：{errorCount}个";
    }
}
