// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 当前作用域的业务租户
/// </summary>
/// <remarks>
/// <para>仅表示当前业务租户，不代表操作人已经登录或具有管理权限。</para>
/// <para>按作用域注册，同一请求使用同一个租户上下文实例。</para>
/// </remarks>
public interface ITenant
{
    /// <summary>
    /// 租户Id
    /// </summary>
    long? TenantId { get; }

    /// <summary>
    /// 租户编号
    /// </summary>
    string TenantNo { get; }

    /// <summary>
    /// 租户名称
    /// </summary>
    string TenantName { get; }

    /// <summary>
    /// 租户编码
    /// </summary>
    string TenantCode { get; }

    /// <summary>
    /// 是否系统租户
    /// </summary>
    bool IsSystemTenant { get; }
}
