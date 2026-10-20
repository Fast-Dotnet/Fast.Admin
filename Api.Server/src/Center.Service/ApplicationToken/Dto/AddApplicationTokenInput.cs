// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.ApplicationToken.Dto;

/// <summary>
/// 添加应用凭据输入
/// </summary>
public class AddApplicationTokenInput
{
    /// <summary>
    /// 应用标识
    /// </summary>
    [StringRequired(ErrorMessage = "应用标识不能为空")]
    public string AppKey { get; set; }

    /// <summary>
    /// 凭据类型
    /// </summary>
    [EnumRequired(ErrorMessage = "凭据类型不能为空")]
    public ApplicationTokenTypeEnum TokenType { get; set; }
}
