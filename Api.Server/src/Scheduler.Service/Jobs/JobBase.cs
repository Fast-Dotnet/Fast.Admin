// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Diagnostics;
using System.Reflection;
using System.Web;
using Fast.Cache;
using Fast.Center.Domain;
using Fast.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// 作业基础实现
/// </summary>
/// <typeparam name="T">调度作业日志类型</typeparam>
[DisallowConcurrentExecution]
[PersistJobDataAfterExecution]
internal abstract class JobBase<T> : IJob where T : SchedulerJobLogInfo, new()
{
    /// <summary>
    /// 最大保留日志数量
    /// </summary>
    private const int _maxLogCount = 100;

    /// <summary>
    /// 执行超过多少秒记录警告日志
    /// </summary>
    private const int _warnTime = 10;

    /// <summary>
    /// 重试间隔
    /// </summary>
    private const int _retryMillisecond = 1000;

    /// <summary>
    /// 计时器
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// 服务提供者
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 邮件服务
    /// </summary>
    protected readonly IMailService _mailService;

    /// <summary>
    /// JSON 序列化配置
    /// </summary>
    protected readonly JsonSerializerSettings _jsonSerializerSettings;

    /// <summary>
    /// 日志
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// 日志信息
    /// </summary>
    protected readonly T _logInfo;

    /// <summary>
    /// 邮件消息
    /// </summary>
    protected MailMessageEnum MailMessage { get; private set; }

    protected JobBase(IServiceProvider serviceProvider, IMailService mailService, IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
        ILogger<IJob> logger, T logInfo)
    {
        _serviceProvider = serviceProvider;
        _jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
        // JSON 美化
        _jsonSerializerSettings.Formatting = Formatting.Indented;
        _mailService = mailService;
        _logger = logger;
        _logInfo = logInfo;
    }

    /// <summary>
    /// 执行调度作业
    /// </summary>
    protected abstract Task JobExecute(IServiceProvider serviceProvider, ISqlSugarClient db, IJobExecutionContext context);

    /// <summary>
    /// 信息日志
    /// </summary>
    protected async Task InfoLog(string title, string msg)
    {
        _logger.LogInformation(msg);

        // 发送邮件
        if ((MailMessage & MailMessageEnum.Info) != 0)
        {
            title = $"【信息】调度作业-{title}";
            string emailContent = await _mailService.GetEmailTemplate(title, msg, "info");
            await _mailService.SendEmail(title, emailContent);
        }
    }

    /// <summary>
    /// 警告日志
    /// </summary>
    protected async Task WarnLog(string title, string msg)
    {
        _logger.LogWarning(msg);

        // 发送邮件
        if ((MailMessage & MailMessageEnum.Warn) != 0)
        {
            title = $"【警告】调度作业-{title}";
            string emailContent = await _mailService.GetEmailTemplate(title, msg, "warn");
            await _mailService.SendEmail(title, emailContent);
        }
    }

    /// <summary>
    /// 错误日志
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="exception">待处理的异常</param>
    /// <param name="msg">消息正文</param>
    protected async Task ErrorLog(string title, Exception exception, string msg)
    {
        if (exception == null)
        {
            _logger.LogError(msg);

            // 发送邮件
            if ((MailMessage & MailMessageEnum.Error) != 0)
            {
                title = $"【异常】调度作业-{title}";
                msg = $"""
                       <p class='error'>错误提示：</p>
                       {msg}
                       """;
                string emailContent = await _mailService.GetEmailTemplate(title, msg, "error");
                await _mailService.SendEmail(title, emailContent);
            }
        }
        else
        {
            _logger.LogError(exception, msg);

            // 发送邮件
            if ((MailMessage & MailMessageEnum.Error) != 0)
            {
                title = $"【异常】调度作业-{title}";
                msg = $"""
                       <p class='error'>错误提示：</p>
                       {msg}
                       <p class='error'>异常信息：</p>
                       <pre class='error'>{JsonConvert.SerializeObject(exception, _jsonSerializerSettings)}</pre>
                       """;
                string emailContent = await _mailService.GetEmailTemplate(title, msg, "error");
                await _mailService.SendEmail(title, emailContent);
            }
        }
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        // 结束时间
        DateTime? endTime = context.JobDetail.JobDataMap.GetNullableDateTime(nameof(SchedulerJobInfo.EndTime));
        // 如果结束时间超过当前时间，则暂停当前作业
        if (endTime != null && endTime <= DateTime.Now)
        {
            // 暂停作业
            await context.Scheduler.PauseJob(new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group));
            return;
        }

        // 创建请求作用域
        using IServiceScope scope = _serviceProvider.CreateScope();

        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

        // 作业名称
        _logInfo.JobName = $"{context.JobDetail.Key.Group}.{context.JobDetail.Key.Name}";

        // 邮件消息
        MailMessage = context.JobDetail.JobDataMap.GetNullableEnum<MailMessageEnum>(nameof(SchedulerJobInfo.MailMessage))
                      ?? MailMessageEnum.None;

        // 租户Id
        _logInfo.TenantId = context.JobDetail.JobDataMap.GetNullableLong(nameof(SchedulerJobInfo.TenantId));

        // 数据库连接字符串处理
        if (_logInfo.TenantId != null)
        {
            (string tenantName, string tenantNo, string tenantCode, string deviceId) =
                SchedulerContext.SchedulerTenantList.GetValueOrDefault(_logInfo.TenantId.Value);
            _logInfo.TenantName = tenantName;
            _logInfo.TenantNo = tenantNo;
            _logInfo.TenantCode = tenantCode;

            // 解析服务
            ICache<CenterCCL> centerCache = scope.ServiceProvider.GetService<ICache<CenterCCL>>();

            // 获取机器人信息
            string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Rabot, tenantNo);
            TenantUserModel robotInfo = await centerCache.GetAndSetAsync(cacheKey, async () =>
            {
                TenantUserModel result = await db.Queryable<TenantUserModel>()
                    .Where(wh => wh.TenantId == _logInfo.TenantId.Value)
                    .Where(wh => wh.UserType == UserTypeEnum.Robot)
                    .SingleAsync();

                if (result == null)
                {
                    await ErrorLog(_logInfo.JobName, null, $"<pre class='error'>未能找到对应租户【{tenantNo}】机器人信息！</pre>");
                }

                return result;
            });
            _logInfo.RobotInfo = robotInfo;

            // 注入 IUser
            IUser _user = scope.ServiceProvider.GetService<IUser>();
            // 设置授权用户
            _user.SetAuthUser(new AuthUserInfo
            {
                DeviceType = AppEnvironmentEnum.Api,
                DeviceId = deviceId,
                SessionId = Guid.NewGuid().ToString("D"),
                AppNo = "Scheduler",
                AppName = "调度程序",
                NickName = robotInfo.EmployeeName,
                Avatar = robotInfo.IdPhoto,
                TenantId = _logInfo.TenantId.Value,
                TenantNo = tenantNo,
                TenantName = tenantName,
                TenantCode = tenantCode,
                IsSystemTenant = false,
                EmployeeId = robotInfo.EmployeeId,
                EmployeeNo = robotInfo.EmployeeNo,
                EmployeeName = robotInfo.EmployeeName,
                DepartmentId = robotInfo.DepartmentId,
                DepartmentName = robotInfo.DepartmentName,
                IsSuperAdmin = false,
                IsAdmin = true,
                LastLoginIp = MetadataContext.MetadataInfo.PublicIp,
                LastLoginTime = DateTime.Now
            });

            // 判断是否是全部租户的，如果是则随机等待 500 ~ 5000 毫秒
            bool isAllTenant = context.JobDetail.JobDataMap.GetNullableBoolean(nameof(SchedulerJobInfo.IsAllTenant)) ?? false;
            if (isAllTenant)
            {
                // 尝试获取本地调度作业的实现类
                Type localSchedulerJobType =
                    SchedulerContext.LocalSchedulerJobTypes.GetValueOrDefault(
                        new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group).ToString());

                if (localSchedulerJobType?.GetCustomAttribute<DisableWaitAttribute>() == null)
                {
                    var random = new Random();
                    int delay = random.Next(500, 5001);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"All tenant job randomly wait {delay} milliseconds.");
                    Console.ResetColor();

                    await Task.Delay(delay);
                }
            }
        }

        // 警告秒数
        int warnTime = context.JobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.WarnTime)) ?? _warnTime;
        warnTime = Math.Abs(warnTime);

        // 运行次数
        long runNumber = context.JobDetail.JobDataMap.GetLong(nameof(SchedulerJobInfo.RunNumber));
        runNumber = Math.Abs(runNumber);

        // 重试次数
        int retryTimes = context.JobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RetryTimes)) ?? 0;
        retryTimes = Math.Abs(retryTimes);
        // 重试间隔
        int retryMillisecond = context.JobDetail.JobDataMap.GetNullableInt(nameof(SchedulerJobInfo.RetryMillisecond))
                               ?? _retryMillisecond;
        retryMillisecond = Math.Abs(retryMillisecond);

        // 日志
        List<string> logs = context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.Logs)] as List<string> ?? [];

        // 运行次数增加
        runNumber++;

        // 开始监听代码运行时间
        _stopwatch.Restart();
        // 作业开始时间
        _logInfo.BeginTime = DateTime.Now;

        try
        {
            // 重试策略
            await RetryUtil.InvokeAsync(async () => await JobExecute(scope.ServiceProvider, db, context), retryTimes,
                retryMillisecond, exceptionTypes: [typeof(UserFriendlyException)], retryAction: async (total, times) =>
                {
                    // 输出重试警告日志
                    await WarnLog(_logInfo.JobName,
                        $"<p class='warn'>Retrying {times}/{total} times</p><pre class='warn'>{JsonConvert.SerializeObject(_logInfo, _jsonSerializerSettings)}");
                });

            // 处理作业执行结果
            if (string.IsNullOrWhiteSpace(_logInfo.Result))
            {
                _logInfo.Result = "<span class='result'>作业执行成功。</span>";
            }

            await InfoLog(_logInfo.JobName,
                $"<p>作业执行成功。</p><pre>{JsonConvert.SerializeObject(_logInfo, _jsonSerializerSettings)}</pre>");
        }
        catch (Exception ex)
        {
            _logInfo.ErrorMsg =
                $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message).GetSubStringWithEllipsis(3000, true)}</span>";
            context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.Exception)] =
                $"<div class='logList error'><span class='time'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</span>{_logInfo.ToJsonString()}</div>";

            if (ex is UserFriendlyException userFriendlyException)
            {
                await WarnLog(_logInfo.JobName,
                    $"<p class='warn'>{userFriendlyException.Message}</p><pre class='warn'>{JsonConvert.SerializeObject(_logInfo, _jsonSerializerSettings)}");
            }
            else
            {
                await ErrorLog(_logInfo.JobName, ex,
                    $"<pre class='error'>{JsonConvert.SerializeObject(_logInfo, _jsonSerializerSettings)}</pre>");
            }
        }
        finally
        {
            // 停止监听
            _stopwatch.Stop();
            // 获取执行总秒数
            double seconds = _stopwatch.Elapsed.TotalSeconds;
            // 获取执行总毫秒数
            double milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            // 作业耗时
            _logInfo.ExecuteTime = milliseconds;
            // 作业结束时间
            _logInfo.EndTime = DateTime.Now;

            // 记录执行次数
            context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.RunNumber)] = runNumber;

            string executeTime = seconds >= 1 ? $"{seconds}秒" : $"{milliseconds}毫秒";

            string className = string.IsNullOrWhiteSpace(_logInfo.ErrorMsg) ? "" : "error";

            // 添加日志
            logs.Add(
                $"<div class='logList {className}'><span class='time'>{_logInfo.BeginTime:yyyy-MM-dd HH:mm:ss} 至 {_logInfo.EndTime:yyyy-MM-dd HH:mm:ss}</span><span class='execTime'>【耗时】{executeTime}</span>{_logInfo.ToJsonString()}</div>");

            // 判断如果超出最大保留日志数则截取日志
            if (logs.Count > _maxLogCount)
            {
                logs.RemoveRange(0, logs.Count - _maxLogCount);
            }

            // 记录日志
            context.JobDetail.JobDataMap[nameof(SchedulerJobInfo.Logs)] = logs;

            // 判断如果超过警告秒数则记录警告日志
            if (seconds >= warnTime)
            {
                await WarnLog(_logInfo.JobName,
                    $"<p class='warn'>调度作业执行耗时过长</p><pre class='warn'>{JsonConvert.SerializeObject(_logInfo, _jsonSerializerSettings)}</pre>");
            }
        }
    }
}
