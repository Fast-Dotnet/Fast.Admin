// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.Menu.Dto;

/// <summary>
/// 获取菜单列表输入
/// </summary>
public class QueryMenuPagedInput
{
    /// <summary>
    /// 搜索值
    /// </summary>
    public string SearchValue { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public EditionEnum? Edition { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public MenuTypeEnum? MenuType { get; set; }

    /// <summary>
    /// 是否桌面端
    /// </summary>
    public bool? HasDesktop { get; set; }

    /// <summary>
    /// 是否Web端
    /// </summary>
    public bool? HasWeb { get; set; }

    /// <summary>
    /// 是否移动端
    /// </summary>
    public bool? HasMobile { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool? Visible { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public CommonStatusEnum? Status { get; set; }

    /// <summary>
    /// 树形数据
    /// </summary>
    public bool TreeData { get; set; } = true;
}
