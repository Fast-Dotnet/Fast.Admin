// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using System.Text.RegularExpressions;
using Fast.CenterLog.Domain;
using Fast.SqlSugar;
using Fast.UnifyResult;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 全局异常处理
/// </summary>
public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    /// <summary>
    /// SqlSugar 实体服务
    /// </summary>
    private readonly ISqlSugarEntityService _sqlSugarEntityService;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 全局异常处理
    /// </summary>
    public GlobalExceptionHandler(ISqlSugarEntityService sqlSugarEntityService, ILogger<IGlobalExceptionHandler> logger)
    {
        _sqlSugarEntityService = sqlSugarEntityService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnExceptionAsync(ExceptionContext context, bool isUserFriendlyException, bool isValidationException)
    {
        // 行版本更新异常直接忽略
        if (context.Exception is VersionExceptions)
        {
            return;
        }

        HttpContext httpContext = context.HttpContext;

        // 已取消的请求及入站连接终止不记录为业务错误。
        if (httpContext.RequestAborted.IsCancellationRequested
            && context.Exception is OperationCanceledException or ConnectionAbortedException or ConnectionResetException)
        {
            return;
        }

        var message = new StringBuilder();

        try
        {
            message.AppendLine(context.Exception.Message);
            message.AppendLine($"Host：{httpContext.Request.Scheme}://{httpContext.Request.Host}");
            message.AppendLine($"Url：{httpContext.Request.Method}, {httpContext.Request.Path}");

            string deviceType = httpContext.Request.Headers[HttpHeaderConst.DeviceType].ToString().UrlDecode();
            string deviceId = httpContext.Request.Headers[HttpHeaderConst.DeviceId].ToString().UrlDecode().Trim();

            message.AppendLine($"device: {deviceType}, {deviceId}");
            if (httpContext.Items.TryGetValue($"{nameof(Fast)}.RequestParams", out object requestParams))
            {
                message.AppendLine($"请求参数: {requestParams?.ToString()}");
            }
            else
            {
                message.AppendLine("请求参数: 无");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "组装异常日志上下文失败。");
        }

        // 判断是否为友好异常
        if (isUserFriendlyException)
        {
            // 只写入最深的一条堆栈信息
            string firstLine = context.Exception.StackTrace?.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            // 如果有匹配的堆栈信息，选择第一条（最深的那一条）
            if (!string.IsNullOrWhiteSpace(firstLine))
            {
                message.AppendLine($"{firstLine}");
            }
            else
            {
                message.AppendLine("未找到堆栈信息...");
            }

            // 写入警告日志
            _logger.LogWarning(message.ToString());
        }
        // 判断是否为验证异常
        else if (isValidationException)
        {
            // 写入警告日志
            _logger.LogWarning(message.ToString());
        }
        else
        {
            try
            {
                string className = context.Exception.TargetSite?.DeclaringType?.FullName;
                string methodName = context.Exception.TargetSite?.Name ?? "";
                if (!string.IsNullOrEmpty(className))
                {
                    Match match = Regex.Match(className, "<(.*?)>");
                    if (match.Success)
                    {
                        methodName = match.Groups[1].Value;
                    }
                }

                // 获取 CenterLog 库的连接字符串配置
                ConnectionSettingsOptions connectionSetting =
                    await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                        DatabaseTypeEnum.CenterLog);
                ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

                IUser _user = httpContext.RequestServices.GetService<IUser>();
                ITenant _tenant = httpContext.RequestServices.GetService<ITenant>();
                var exceptionLogModel = new ExceptionLogModel
                {
                    RecordId = YitIdHelper.NextId(),
                    AccountId = _user?.AccountId,
                    Mobile = _user?.Mobile,
                    NickName = _user?.NickName,
                    ClassName = className,
                    MethodName = methodName,
                    Message = context.Exception.Message,
                    Source = context.Exception.Source,
                    StackTrace = context.Exception.StackTrace,
                    ParamsObj =
                        context.Exception.TargetSite?.GetParameters()
                            .Select(sl => new
                            {
                                PropertyName = sl.Name,
                                TypeName = sl.ParameterType.Name,
                                TypeFullName = sl.ParameterType.FullName
                            })
                            .ToList()
                            .ToJsonString(),
                    DepartmentId = _user?.DepartmentId > 0 ? _user.DepartmentId : null,
                    DepartmentName = _user?.DepartmentName,
                    CreatedUserId = _user?.EmployeeId > 0 ? _user.EmployeeId : null,
                    CreatedUserName = _user?.EmployeeName,
                    CreatedTime = DateTime.Now,
                    TenantId = _tenant?.TenantId,
                    TenantName = _tenant?.TenantName
                };
                exceptionLogModel.RecordCreate(httpContext);

                // 独立客户端不加载 AOP，避免异常审计失败后递归生成新异常审计；主异常返回前等待持久化完成
                using var db = new SqlSugarClient(connectionConfig);
                await db.Insertable(exceptionLogModel).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Global Exception 准备异常审计日志失败，已保留主异常日志。");
            }
            finally
            {
                // 记录原始异常；数据库审计不可用时也不能丢失主异常或再次抛错
                _logger.LogError(context.Exception, message.ToString());
            }
        }
    }
}
