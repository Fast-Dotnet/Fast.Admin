// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.MessageTemplate.Dto;

/// <summary>
/// 获取消息模板分页列表输出
/// </summary>
public class QueryMessageTemplatePagedOutput : PagedOutput
{
    /// <summary>
    /// 应用标识
    /// </summary>
    [SugarSearchValue]
    public string AppKey { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [SugarSearchValue]
    public string AppName { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    public AppEnvironmentEnum? AppType { get; set; }

    /// <summary>
    /// 环境类型
    /// </summary>
    public EnvironmentTypeEnum? EnvironmentType { get; set; }

    /// <summary>
    /// 记录Id
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 模板类型
    /// </summary>
    public MessageTemplateTypeEnum TemplateType { get; set; }

    /// <summary>
    /// 模板Code
    /// </summary>
    [SugarSearchValue]
    public string TemplateCode { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }
}
