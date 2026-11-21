// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Application.Dto;

/// <summary>
/// 获取应用下载输出
/// </summary>
public class QueryAppDownloadOutput
{
    /// <summary>
    /// 下载页面地址
    /// </summary>
    public string DownloadUrl { get; set; }

    /// <summary>
    /// PNG 二维码 Data URL
    /// </summary>
    public string QrCode { get; set; }
}
