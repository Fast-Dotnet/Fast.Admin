// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.ApplicationClient.Dto;

/// <summary>
/// 应用标识Id输入
/// </summary>
public class ApplicationClientIdInput : UpdateVersionInput
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [StringRequired(ErrorMessage = "记录Id不能为空")]
    public long RecordId { get; set; }
}
