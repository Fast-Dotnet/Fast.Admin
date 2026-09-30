// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 规范化响应数据提供器
/// </summary>
public class UnifyResponseProvider : IUnifyResponseProvider
{
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 规范化响应数据提供器
    /// </summary>
    public UnifyResponseProvider(ILogger<IUnifyResponseProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(int statusCode, string message)> ResponseExceptionAsync(ExceptionContext context,
        ExceptionMetadata metadata, HttpContext httpContext)
    {
        // 默认 500 错误
        int statusCode = StatusCodes.Status500InternalServerError;

        string message = context.Exception.Message;

        switch (context.Exception)
        {
            // 友好异常处理
            case UserFriendlyException userFriendlyException:
                {
                    message = userFriendlyException.Message;
                    if (userFriendlyException.OriginErrorCode != null)
                    {
                        statusCode = userFriendlyException.OriginErrorCode.ToString().ParseToInt();
                    }
                    else if (userFriendlyException.ErrorCode != null)
                    {
                        statusCode = Convert.ToInt32(userFriendlyException.ErrorCode);
                    }
                    else
                    {
                        statusCode = userFriendlyException.StatusCode;
                    }

                    // 处理可能为0的情况
                    statusCode = statusCode == 0 ? StatusCodes.Status400BadRequest : statusCode;
                    break;
                }
            // SqlSugar 并发处理
            case VersionExceptions:
                statusCode = StatusCodes.Status400BadRequest;
                message = "数据已更改，请刷新后重试！";
                break;
            // 操作异常
            case InvalidOperationException invalidOperationException:
                statusCode = StatusCodes.Status500InternalServerError;
                if (invalidOperationException.InnerException is SqlException sqlException)
                {
                    message = sqlException.Message;
                }
                else
                {
                    message = invalidOperationException.Message;
                }

                break;
        }

        return await Task.FromResult((statusCode, message));
    }

    /// <inheritdoc />
    public async Task ResponseValidationExceptionAsync(ActionExecutingContext context, ValidationMetadata metadata,
        HttpContext httpContext)
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<object> ResponseDataAsync(long timestamp, object data, HttpContext httpContext)
    {
        // 响应加密特性
        ResponseEncipherAttribute responseEncipherAttribute = httpContext
            .GetEndpoint()
            ?.Metadata.GetMetadata<ResponseEncipherAttribute>();

        bool responseEncipher;

        if (responseEncipherAttribute == null)
        {
            // 获取应用信息
            ApplicationOpenIdModel applicationOpenIdModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
            responseEncipher = applicationOpenIdModel.RequestEncipher;
        }
        else if (responseEncipherAttribute.Enable)
        {
            responseEncipher = true;
        }
        else
        {
            responseEncipher = false;
        }

        // 判断是否开启响应加密
        if (!responseEncipher)
        {
            return await Task.FromResult(data);
        }

        // 添加加密头部标识
        httpContext.Response.Headers.TryAdd($"{nameof(Fast)}-Response-Encipher", "True");

        string dataStr = data.ToJsonString();

        // 加密数据
        string encryptedStr = CryptoUtil.AESEncrypt(dataStr, timestamp.ToString(), $"FIV{timestamp}");

        return await Task.FromResult(encryptedStr);
    }
}
