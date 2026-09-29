// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net.Http.Headers;
using System.Web;
using Fast.Center.Domain;
using Fast.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// URL 调度作业
/// </summary>
internal sealed class UrlJob : JobBase<SchedulerJobUrlLogInfo>
{
    public UrlJob(IServiceProvider serviceProvider, IMailService mailService, IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
        ILogger<IJob> logger) : base(serviceProvider, mailService, jsonOptions, logger, new SchedulerJobUrlLogInfo())
    {
    }

    /// <inheritdoc />
    protected override async Task JobExecute(IServiceProvider serviceProvider, ISqlSugarClient db, IJobExecutionContext context)
    {
        // 作业类型
        SchedulerJobTypeEnum jobType =
            context.JobDetail.JobDataMap.GetEnum<SchedulerJobTypeEnum>(nameof(SchedulerJobInfo.JobType));

        if ((jobType & (SchedulerJobTypeEnum.IntranetUrl | SchedulerJobTypeEnum.OuterNetUrl)) == 0)
        {
            throw new UserFriendlyException("当前调度作业只支持【内网Url】【外网Url】！");
        }

        // 请求方式，默认Get请求
        HttpRequestMethodEnum requestMethod =
            context.JobDetail.JobDataMap.GetNullableEnum<HttpRequestMethodEnum>(nameof(SchedulerJobInfo.RequestMethod))
            ?? HttpRequestMethodEnum.Get;
        _logInfo.RequestMethod = requestMethod.ToString();

        // 请求超时时间，默认不超时
        int? requestTimeout = context.JobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RequestTimeout));
        _logInfo.RequestTimeout = requestTimeout;

        // 请求参数
        var requestParams = new Dictionary<string, object>();
        var jobRequestParams =
            context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.RequestParams)] as IDictionary<string, object>;
        jobRequestParams ??= new Dictionary<string, object>();
        // 请求参数处理
        foreach ((string key, object value) in jobRequestParams)
        {
            requestParams.TryAdd(key, value);
        }

        _logInfo.RequestParams = $"<span class='params'>{HttpUtility.HtmlEncode(requestParams.ToJsonString())}</span>";

        // 请求头部
        var requestHeader = new Dictionary<string, string>();
        var jobRequestHeader =
            context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.RequestHeader)] as IDictionary<string, string>;
        jobRequestHeader ??= new Dictionary<string, string>();
        // 请求头部处理
        foreach ((string key, string value) in jobRequestHeader)
        {
            requestHeader.TryAdd(key, value);
        }

        _logInfo.RequestHeader = $"<span class='headers'>{HttpUtility.HtmlEncode(requestHeader.ToJsonString())}</span>";

        // 请求Url
        string requestUrl = context.JobDetail.JobDataMap.GetString(nameof(SchedulerJobInfo.RequestUrl));

        if (_logInfo.TenantId != null)
        {
            IUser user = serviceProvider.GetService<IUser>();

            requestHeader.TryAdd(HttpHeaderConst.DeviceType, user.DeviceType.ToString());
            requestHeader.TryAdd(HttpHeaderConst.DeviceId, user.DeviceId);

            // 重新赋值请求参数
            _logInfo.RequestHeader = $"<span class='headers'>{HttpUtility.HtmlEncode(requestHeader.ToJsonString())}</span>";

            // 安全期间，只有内网才添加授权信息
            if (jobType == SchedulerJobTypeEnum.IntranetUrl)
            {
                string accessToken = await user.RobotLogin();
                requestHeader.TryAdd("Authorization", accessToken);
            }
        }

        _logInfo.RequestUrl = $"<span class='url'>{HttpUtility.HtmlEncode(requestUrl)}</span>";

        // 响应数据
        string responseData;
        HttpResponseHeaders responseHeaders;

        // 发送请求
        switch (requestMethod)
        {
            case HttpRequestMethodEnum.Get:
                (responseData, responseHeaders) =
                    await RemoteRequestUtil.GetAsync(requestUrl, requestParams, requestHeader, timeout: requestTimeout);
                break;
            case HttpRequestMethodEnum.Post:
                (responseData, responseHeaders) =
                    await RemoteRequestUtil.PostAsync(requestUrl, requestParams, requestHeader, timeout: requestTimeout);
                break;
            case HttpRequestMethodEnum.Put:
                (responseData, responseHeaders) =
                    await RemoteRequestUtil.PutAsync(requestUrl, requestParams, requestHeader, timeout: requestTimeout);
                break;
            case HttpRequestMethodEnum.Delete:
                (responseData, responseHeaders) =
                    await RemoteRequestUtil.DeleteAsync(requestUrl, requestHeader, timeout: requestTimeout);
                break;
            default:
                throw new UserFriendlyException("请求方式不支持！");
        }

        _logInfo.Result =
            $"<span class='result'>{HttpUtility.HtmlEncode(responseData).GetSubStringWithEllipsis(1000, true)}</span>";

        var responseHeader = new Dictionary<string, string>();
        foreach (KeyValuePair<string, IEnumerable<string>> header in responseHeaders)
        {
            responseHeader.TryAdd(header.Key, string.Join(",", header.Value));
        }

        _logInfo.ResponseHeader = responseHeader;
    }
}
