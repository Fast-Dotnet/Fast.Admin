// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using System.Net.Sockets;

namespace Fast.Core;

/// <summary>
/// 元数据上下文
/// </summary>
[SuppressSniffer]
public class MetadataContext
{
    /// <summary>
    /// 元数据信息
    /// </summary>
    public static ServerMetadataInfo MetadataInfo
    {
        get
        {
            field ??= GetServerMetadata().Result;

            return field;
        }
    }

    /// <summary>
    /// 获取服务器Ip地址
    /// </summary>
    /// <remarks>从Metadata Service中获取</remarks>
    /// <returns>服务器元数据；无法识别云平台时返回本机信息</returns>
    private static async Task<ServerMetadataInfo> GetServerMetadata()
    {
        // 阿里云
        try
        {
            (string instanceName, _) =
                await RemoteRequestUtil.GetAsync("http://100.100.100.200/latest/meta-data/instance-id", timeout: 1);
            (string region, _) =
                await RemoteRequestUtil.GetAsync("http://100.100.100.200/latest/meta-data/region-id", timeout: 1);
            (string zone, _) = await RemoteRequestUtil.GetAsync("http://100.100.100.200/latest/meta-data/zone-id", timeout: 1);
            (string innerIp, _) =
                await RemoteRequestUtil.GetAsync("http://100.100.100.200/latest/meta-data/private-ipv4", timeout: 1);
            (string publicIp, _) = await RemoteRequestUtil.GetAsync("http://100.100.100.200/latest/meta-data/eipv4", timeout: 1);
            return new ServerMetadataInfo
            {
                Provider = "AliYun",
                InstanceName = instanceName,
                Region = region,
                Zone = zone,
                InnerIp = innerIp,
                PublicIp = publicIp
            };
        }
        catch
        {
            // ignored
        }

        // 腾讯云
        try
        {
            (string instanceName, _) =
                await RemoteRequestUtil.GetAsync("http://metadata.tencentyun.com/latest/meta-data/instance-id", timeout: 1);
            (string region, _) =
                await RemoteRequestUtil.GetAsync("http://metadata.tencentyun.com/latest/meta-data/placement/region", timeout: 1);
            (string zone, _) =
                await RemoteRequestUtil.GetAsync("http://metadata.tencentyun.com/latest/meta-data/placement/zone", timeout: 1);
            (string innerIp, _) =
                await RemoteRequestUtil.GetAsync("http://metadata.tencentyun.com/latest/meta-data/local-ipv4", timeout: 1);
            (string publicIp, _) =
                await RemoteRequestUtil.GetAsync("http://metadata.tencentyun.com/latest/meta-data/public-ipv4", timeout: 1);
            return new ServerMetadataInfo
            {
                Provider = "TencentCloud",
                InstanceName = instanceName,
                Region = region,
                Zone = zone,
                InnerIp = innerIp,
                PublicIp = publicIp
            };
        }
        catch
        {
            // ignored
        }

        // 华为云
        try
        {
            (string instanceName, _) =
                await RemoteRequestUtil.GetAsync("http://169.254.169.254/latest/meta-data/instance-id", timeout: 1);
            (string region, _) = await RemoteRequestUtil.GetAsync("http://169.254.169.254/latest/meta-data/region", timeout: 1);
            (string zone, _) =
                await RemoteRequestUtil.GetAsync("http://169.254.169.254/latest/meta-data/availability-zone", timeout: 1);
            (string innerIp, _) =
                await RemoteRequestUtil.GetAsync("http://169.254.169.254/latest/meta-data/local-ipv4", timeout: 1);
            (string publicIp, _) =
                await RemoteRequestUtil.GetAsync("http://169.254.169.254/latest/meta-data/public-ipv4s", timeout: 1);

            return new ServerMetadataInfo
            {
                Provider = "HuaweiCloud",
                InstanceName = instanceName,
                Region = region,
                Zone = zone,
                InnerIp = innerIp,
                PublicIp = publicIp
            };
        }
        catch
        {
            // ignored
        }

        return new ServerMetadataInfo
        {
            Provider = "Local",
            InstanceName = Environment.MachineName,
            Region = "Local",
            Zone = "Local",
            InnerIp = (await Dns.GetHostEntryAsync(Dns.GetHostName())).AddressList
                      .FirstOrDefault(f => f.AddressFamily == AddressFamily.InterNetwork)
                      ?.ToString()
                      ?? "127.0.0.1"
        };
    }
}
