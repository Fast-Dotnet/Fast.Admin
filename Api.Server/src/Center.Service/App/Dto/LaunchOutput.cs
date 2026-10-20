// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.App.Dto;

/// <summary>
/// Launch 输出
/// </summary>
public class LaunchOutput
{
    /// <summary>
    /// 版本
    /// </summary>
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    public string AppNo { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// 主题色
    /// </summary>
    [Required]
    public string ThemeColor { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    public AppEnvironmentEnum AppType { get; set; }

    /// <summary>
    /// 环境类型
    /// </summary>
    public EnvironmentTypeEnum EnvironmentType { get; set; }

    /// <summary>
    /// 登录组件
    /// </summary>
    public string LoginComponent { get; set; }

    /// <summary>
    /// 请求超时时间（毫秒）
    /// </summary>
    public int RequestTimeout { get; set; }

    /// <summary>
    /// 请求加密
    /// </summary>
    public bool RequestEncipher { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }
}
