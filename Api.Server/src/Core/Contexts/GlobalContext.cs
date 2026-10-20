// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fast.Core;

/// <summary>
/// 系统通用上下文
/// </summary>
[SuppressSniffer]
public class GlobalContext
{
    /// <summary>
    /// 初始化配置
    /// </summary>
    public static InitializationSettingsOptions InitializationSettings { get; set; }

    /// <summary>
    /// 菜单、按钮初始化权限，Key 为权限编码，Value 为角色标志组合
    /// </summary>
    public static Dictionary<string, RoleTypeEnum> RolePermissions { get; private set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// 加载默认及下游初始化权限，同一权限编码的角色取并集。
    /// </summary>
    public static void LoadRolePermissions(IConfiguration configuration)
    {
        var permissions = new Dictionary<string, RoleTypeEnum>(StringComparer.Ordinal);
        IConfigurationSection[] sections =
        [
            configuration.GetRequiredSection("RolePermissions"),
            configuration.GetSection("CustomRolePermissions")
        ];

        foreach (IConfigurationSection section in sections)
        {
            if (section.Value != null)
            {
                throw new InvalidOperationException($"配置节 {section.Path} 必须为权限字典。");
            }

            foreach (IConfigurationSection child in section.GetChildren())
            {
                MergePermission(child, child.Key);
            }
        }

        RolePermissions = permissions;

        void MergePermission(IConfigurationSection section, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                throw new InvalidOperationException($"配置节点 {section.Path} 的权限编码不能为空。");
            }

            IConfigurationSection[] children = section.GetChildren().ToArray();

            // 角色数组对应数字索引节点，其余节点继续拼接权限编码。
            bool isRoleArray = children.Length > 0
                               && children.All(child => int.TryParse(child.Key, out int index)
                                                        && index >= 0
                                                        && !child.GetChildren().Any());

            if (children.Length > 0 && !isRoleArray)
            {
                foreach (IConfigurationSection child in children)
                {
                    MergePermission(child, $"{permissionCode}:{child.Key}");
                }

                return;
            }

            string[] roleNames = section.Get<string[]>(options => options.ErrorOnUnknownConfiguration = true)
                                 ?? throw new InvalidOperationException($"配置节点 {section.Path} 必须为角色数组。");

            permissions.TryGetValue(permissionCode, out RoleTypeEnum roleFlags);
            foreach (string roleName in roleNames)
            {
                if (roleName == null || !Enum.IsDefined(typeof(RoleTypeEnum), roleName))
                {
                    throw new InvalidOperationException($"配置节点 {section.Path} 包含无效角色名称：{roleName}。");
                }

                roleFlags |= Enum.Parse<RoleTypeEnum>(roleName);
            }

            permissions[permissionCode] = roleFlags;
        }
    }

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
