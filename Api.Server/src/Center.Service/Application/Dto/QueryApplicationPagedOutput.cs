// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Application.Dto;

/// <summary>
/// 获取应用分页列表输出
/// </summary>
public class QueryApplicationPagedOutput : PagedOutput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 应用编号
    /// </summary>
    [SugarSearchValue]
    public string AppNo { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [SugarSearchValue]
    public string AppName { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// 主题色
    /// </summary>
    public string ThemeColor { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// 应用标识列表
    /// </summary>
    public List<QueryApplicationClientOutput> ClientList { get; set; } = [];
}
