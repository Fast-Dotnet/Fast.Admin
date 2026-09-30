// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 媒体资源访问 Token 载荷
/// </summary>
public class MediaAssetTokenPayload
{
    /// <summary>
    /// 匿名访问票据
    /// </summary>
    public bool IsAnonymous { get; set; }

    /// <summary>
    /// 文件Id
    /// </summary>
    public long FileId { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    public string AppNo { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public string TenantNo { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public AppEnvironmentEnum DeviceType { get; set; }

    /// <summary>
    /// 职员编号
    /// </summary>
    public string EmployeeNo { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// 过期时间戳，单位：秒
    /// </summary>
    public long ExpiresAt { get; set; }
}
