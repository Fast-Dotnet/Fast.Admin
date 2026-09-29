// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using Fast.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Fast.Core;

/// <summary>
/// 请求中间件
/// </summary>
[SuppressSniffer]
public class RequestMiddleware
{
    /// <summary>
    /// 请求委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 请求中间件
    /// </summary>
    public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 中间件执行方法
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        HttpRequest httpRequest = httpContext.Request;

        // 排除 WebSocket
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            await _next(httpContext);
            return;
        }

        // 排除 multipart/form-data 格式
        if (httpRequest.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
        {
            // 写入 HttpContext.Items
            httpContext.Items[$"{nameof(Fast)}.RequestParams"] = "文件上传...";

            await _next(httpContext);
            return;
        }

        bool isReadBody = !(httpRequest.Method == HttpMethod.Get.Method || httpRequest.Method == HttpMethod.Delete.Method);

        // Url请求参数
        IDictionary<string, string> queryParamDic = null;
        // Body请求参数
        string bodyParam = "";
        // 解密数据
        string decryptedData = "";

        if (!isReadBody)
        {
            queryParamDic = httpRequest.Query.ToDictionary(e => e.Key, e => e.Value.ToString());
        }
        else
        {
            // 允许读取请求的Body
            httpContext.Request.EnableBuffering();
            using var streamReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
            bodyParam = await streamReader.ReadToEndAsync();
            // 重置指针
            httpContext.Request.Body.Position = 0;
        }

        // 请求加密只用于兼容客户端的数据封装，密钥可由请求时间戳推导，不能替代 HTTPS、身份认证和防重放校验
        // 登录密码等敏感字段在生产环境仍必须通过 HTTPS 传输；此开关开启时仅在 HTTPS 之上增加一层协议加密
        bool requestEncipher = false;

        // 判断是否存在加密头部标识
        if (httpRequest.Headers.TryGetValue($"{nameof(Fast)}-request-Encipher", out StringValues requestEncipherStr))
        {
            bool.TryParse(requestEncipherStr, out requestEncipher);
        }

        try
        {
            if (requestEncipher)
            {
                if (!isReadBody && queryParamDic?.Count > 0)
                {
                    // 解密数据
                    decryptedData = CryptoUtil.AESDecrypt(queryParamDic["data"], queryParamDic["timestamp"],
                        $"FIV{queryParamDic["timestamp"]}");

                    // 反序列化成键值对
                    Dictionary<string, string> model = decryptedData.ToObject<Dictionary<string, string>>();

                    // 替换 QueryString
                    httpRequest.QueryString = QueryString.Create(model);
                }
                else if (isReadBody && !string.IsNullOrWhiteSpace(bodyParam))
                {
                    // 解析Json，取出 data 和 timestamp 字段
                    RestfulResult<string> encryptedData = bodyParam.ToObject<RestfulResult<string>>();

                    // 解密数据
                    decryptedData = CryptoUtil.AESDecrypt(encryptedData.Data, encryptedData.Timestamp.ToString(),
                        $"FIV{encryptedData.Timestamp}");

                    // 写入 Body
                    httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(decryptedData));

                    // 重置指针
                    httpContext.Request.Body.Position = 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "请求数据格式错误！");
            throw new HttpRequestException("请求数据格式错误！", ex);
        }
        finally
        {
            // 写入 HttpContext.Items
            httpContext.Items[$"{nameof(Fast)}.RequestParams"] = decryptedData;
        }

        await _next(httpContext);
    }
}
