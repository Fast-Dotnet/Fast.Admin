// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.App.Dto;

/// <summary>
/// 检查应用更新输入
/// </summary>
public class CheckAppUpdateInput
{
    /// <summary>
    /// 安装包版本号
    /// </summary>
    [IntRequired(ErrorMessage = "安装包版本号不能为空")]
    [Range(1, int.MaxValue)]
    public int NativeVersionCode { get; set; }

    /// <summary>
    /// 资源版本号
    /// </summary>
    /// <remarks>未安装过 WGT 时可不传；热更新后须传资源包内置的递增版本号。</remarks>
    [Range(1, int.MaxValue)]
    public int? ResourceVersionCode { get; set; }
}
