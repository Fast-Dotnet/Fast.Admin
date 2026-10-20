// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 初始化配置选项
/// </summary>
public class InitializationSettingsOptions
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    /// 主题色
    /// </summary>
    public string ThemeColor { get; set; }

    /// <summary>
    /// CenterLog 库命名
    /// </summary>
    public string DatabaseNaming_CenterLog { get; set; }

    /// <summary>
    /// Gateway 库命名
    /// </summary>
    public string DatabaseNaming_Gateway { get; set; }

    /// <summary>
    /// Deploy 库命名
    /// </summary>
    public string DatabaseNaming_Deploy { get; set; }

    /// <summary>
    /// Admin 库命名
    /// </summary>
    public string DatabaseNaming_Admin { get; set; }

    /// <summary>
    /// AdminLog 库命名
    /// </summary>
    public string DatabaseNaming_AdminLog { get; set; }

    /// <summary>
    /// 默认管理员密码
    /// </summary>
    public string DefaultAdminPassword { get; set; }

    /// <summary>
    /// 默认密码
    /// </summary>
    public string DefaultPassword { get; set; }

    /// <summary>
    /// 默认Logo
    /// </summary>
    public string DefaultLogo { get; set; }
}
