// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Diagnostics;
using Fast.CenterLog.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 请求日志过滤器
/// </summary>
public class RequestActionFilter : IAsyncActionFilter
{
    /// <summary>
    /// SqlSugar 实体服务
    /// </summary>
    private readonly ISqlSugarEntityService _sqlSugarEntityService;

    /// <summary>
    /// 业务租户
    /// </summary>
    private readonly ITenant _tenant;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 请求日志拦截
    /// </summary>
    public RequestActionFilter(ISqlSugarEntityService sqlSugarEntityService, ITenant tenant, ILogger<IAsyncActionFilter> logger)
    {
        _sqlSugarEntityService = sqlSugarEntityService;
        _tenant = tenant;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        DateTime dateTime = DateTime.Now;
        HttpContext httpContext = context.HttpContext;
        HttpRequest httpRequest = httpContext.Request;
        IUser _user = httpContext.RequestServices.GetService<IUser>();

        UserAgentInfo userAgentInfo = null;
        WanNetIPInfo wanInfo = null;

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        // 获取 UserAgent 和 Ip 信息
        try
        {
            userAgentInfo = httpContext.RequestUserAgentInfo();
            wanInfo = await httpContext.RemoteIpv4InfoAsync();
        }
        catch (OperationCanceledException)
        {
        }

        ActionExecutedContext actionContext = await next();
        stopwatch.Stop();

        try
        {
            var actionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
            IList<object> endpointMetadata = actionContext.ActionDescriptor.EndpointMetadata;

            // 判断是否存在禁用请求日志特性，支持 Controller 和 Action
            if (endpointMetadata.OfType<DisabledRequestLogAttribute>().Any())
                return;

            // 获取 ApiInfo 特性，Controller 和 Action 同时存在时优先使用 Action
            ApiInfoAttribute apiInfoAttribute = endpointMetadata.OfType<ApiInfoAttribute>().LastOrDefault();
            if (!Enum.TryParse(httpRequest.Method, true, out HttpRequestMethodEnum requestMethod))
            {
                // 默认 Get 请求
                requestMethod = HttpRequestMethodEnum.Get;
            }

            // 获取 CenterLog 库的连接字符串配置
            ConnectionSettingsOptions connectionSetting =
                await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                    DatabaseTypeEnum.CenterLog);
            ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

            var requestLogModel = new RequestLogModel
            {
                RecordId = YitIdHelper.NextId(),
                AccountId = _user?.AccountId,
                Mobile = _user?.Mobile,
                NickName = _user?.NickName,
                IsSuccess = actionContext.Exception == null,
                OperationAction = apiInfoAttribute?.Action ?? HttpRequestActionEnum.None,
                OperationName = apiInfoAttribute?.Name,
                ClassName = context.Controller.ToString(),
                MethodName = actionDescriptor?.ActionName,
                Location = httpRequest.Path,
                RequestMethod = requestMethod,
                Param = context.ActionArguments.Count < 1 ? "" : context.ActionArguments.ToJsonString(),
                // 成功响应可能包含敏感数据且体积不受控，请求审计仅保存异常详情
                Result = actionContext.Exception?.ToString(),
                ElapsedTime = stopwatch.ElapsedMilliseconds,
                DepartmentId = _user?.DepartmentId,
                DepartmentName = _user?.DepartmentName,
                CreatedUserId = _user?.EmployeeId,
                CreatedUserName = _user?.EmployeeName,
                CreatedTime = dateTime,
                TenantId = _tenant.TenantId,
                TenantName = _tenant.TenantName,
                // 可能为空
                Device = userAgentInfo?.Device,
                OS = userAgentInfo?.OS,
                Browser = userAgentInfo?.Browser,
                Province = wanInfo?.Province,
                City = wanInfo?.City,
                Ip = wanInfo?.Ip ?? "unknown"
            };

            // 独立客户端不加载 AOP，防止写审计日志再次触发审计；等待写入完成后才结束请求
            using var db = new SqlSugarClient(connectionConfig);
            await db.Insertable(requestLogModel).SplitTable().ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存请求审计日志失败。");
        }
    }
}
