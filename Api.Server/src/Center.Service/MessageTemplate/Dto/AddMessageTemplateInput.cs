// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.MessageTemplate.Dto;

/// <summary>
/// 添加消息模板输入
/// </summary>
public class AddMessageTemplateInput
{
    /// <summary>
    /// 模板类型
    /// </summary>
    [EnumRequired(ErrorMessage = "模板类型不能为空")]
    public MessageTemplateTypeEnum TemplateType { get; set; }

    /// <summary>
    /// 模板Code
    /// </summary>
    [StringRequired(ErrorMessage = "模板Code不能为空")]
    public string TemplateCode { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }
}
