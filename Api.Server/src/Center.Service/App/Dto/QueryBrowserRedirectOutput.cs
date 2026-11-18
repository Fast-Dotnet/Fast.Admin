// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.App.Dto;

/// <summary>
/// 获取浏览器重定向输出
/// </summary>
public class QueryBrowserRedirectOutput
{
    /// <summary>
    /// 目标来源
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 目标路径
    /// </summary>
    public string Path { get; set; }
}
