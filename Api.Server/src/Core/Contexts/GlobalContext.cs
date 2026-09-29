// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Fast.Core;

/// <summary>
/// 系统通用上下文
/// </summary>
[SuppressSniffer]
public class GlobalContext
{
    /// <summary>
    /// 客户端标识
    /// </summary>
    public static string ClientIdentity =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{FastContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv6()
                   .ToString()
               ?? "unknown"}:{Origin}:{DeviceType}:{DeviceId}")));

    /// <summary>
    /// 来源
    /// </summary>
    public static string Origin
    {
        get
        {
            string result;
            HttpContext httpContext = FastContext.HttpContext;
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                result = httpContext.Request.Query[HttpHeaderConst.Origin].ToString().UrlDecode();
            }
            else
            {
                result = httpContext.Request.Headers[HttpHeaderConst.Origin].ToString().UrlDecode();
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            throw new UserFriendlyException("未知的设备信息！");
        }
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public static AppEnvironmentEnum DeviceType
    {
        get
        {
            string result;
            HttpContext httpContext = FastContext.HttpContext;
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                result = httpContext.Request.Query[HttpHeaderConst.DeviceType].ToString().UrlDecode();
            }
            else
            {
                result = httpContext.Request.Headers[HttpHeaderConst.DeviceType].ToString().UrlDecode();
            }

            if (!string.IsNullOrWhiteSpace(result) && Enum.TryParse(result, true, out AppEnvironmentEnum environment))
            {
                return environment;
            }

            throw new UserFriendlyException("未知的设备信息！");
        }
    }

    /// <summary>
    /// 设备Id
    /// </summary>
    public static string DeviceId
    {
        get
        {
            string result;
            HttpContext httpContext = FastContext.HttpContext;
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                result = httpContext.Request.Query[HttpHeaderConst.DeviceId].ToString().UrlDecode().Trim();
            }
            else
            {
                result = httpContext.Request.Headers[HttpHeaderConst.DeviceId].ToString().UrlDecode().Trim();
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            throw new UserFriendlyException("未知的设备信息！");
        }
    }

    /// <summary>
    /// 是否为Web端
    /// </summary>
    public static bool IsWeb => (DeviceType & AppEnvironmentEnum.Web) != 0;

    /// <summary>
    /// 是否为桌面端
    /// </summary>
    public static bool IsDesktop => (DeviceType & AppEnvironmentEnum.Desktop) != 0;

    /// <summary>
    /// 是否为移动端
    /// </summary>
    public static bool IsMobile => (DeviceType & AppEnvironmentEnum.MobileThree) != 0;
}
