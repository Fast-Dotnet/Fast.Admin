// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Web;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// 本地调度作业
/// </summary>
internal sealed class LocalJob : JobBase<SchedulerJobLogInfo>
{
    public LocalJob(IServiceProvider serviceProvider, ISchedulerCenter schedulerCenter, IMailService mailService,
        IOptions<MvcNewtonsoftJsonOptions> jsonOptions, ILogger<IJob> logger) : base(serviceProvider, mailService, jsonOptions,
        logger, new SchedulerJobUrlLogInfo())
    {
    }

    /// <inheritdoc />
    protected override async Task JobExecute(IServiceProvider serviceProvider, ISqlSugarClient db, IJobExecutionContext context)
    {
        // 作业类型
        SchedulerJobTypeEnum jobType =
            context.JobDetail.JobDataMap.GetEnum<SchedulerJobTypeEnum>(nameof(SchedulerJobInfo.JobType));

        if (jobType != SchedulerJobTypeEnum.Local)
        {
            throw new UserFriendlyException("当前调度作业只支持【本地】！");
        }

        var jobKey = new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group);

        // 尝试从缓存中获取本地调度作业的实现类
        Type localSchedulerJobType = SchedulerContext.LocalSchedulerJobTypes.GetValueOrDefault(jobKey.ToString());

        if (localSchedulerJobType == null)
        {
            throw new SchedulerException("未能在缓存中找到【本地】调度作业！");
        }

        // 解析服务
        if (serviceProvider.GetService(localSchedulerJobType) is not ISchedulerJob _schedulerJob)
        {
            throw new UserFriendlyException("解析【本地】调度作业服务异常，服务不存在！");
        }

        // 执行本地调度作业
        string result = await _schedulerJob.Execute(serviceProvider, db,
            new SchedulerJobLocalLogInfo
            {
                JobName = _logInfo.JobName,
                InfoLog = InfoLog,
                WarnLog = WarnLog,
                ErrorLog = ErrorLog,
                TenantId = _logInfo.TenantId,
                TenantName = _logInfo.TenantName,
                TenantNo = _logInfo.TenantNo,
                TenantCode = _logInfo.TenantCode,
                RobotInfo = _logInfo.RobotInfo
            });

        if (!string.IsNullOrWhiteSpace(result))
        {
            _logInfo.Result =
                $"<span class='result'>{HttpUtility.HtmlEncode(result).GetSubStringWithEllipsis(1000, true)}</span>";
        }
    }
}
