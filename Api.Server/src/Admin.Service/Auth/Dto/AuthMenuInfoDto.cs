// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections;
using Fast.Center.Domain;

namespace Fast.Admin.Service.Auth.Dto;

/// <summary>
/// 授权菜单信息Dto
/// </summary>
public class AuthMenuInfoDto : ITreeNode<long>
{
    /// <summary>
    /// 菜单Id
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单标题
    /// </summary>
    public string MenuTitle { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public List<long> ParentIds { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string Router { get; set; }

    /// <summary>
    /// 组件地址
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 导航栏显示
    /// </summary>
    public bool Tab { get; set; }

    /// <summary>
    /// 缓存页面
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 内链/外链地址
    /// </summary>
    public string Link { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<AuthMenuInfoDto> Children { get; set; } = [];

    /// <inheritdoc />
    public long GetId()
    {
        return MenuId;
    }

    /// <inheritdoc />
    public long GetPid()
    {
        return ParentId;
    }

    /// <inheritdoc />
    public long GetSort()
    {
        return Sort;
    }

    /// <inheritdoc />
    public void SetChildren(IList children)
    {
        Children = (List<AuthMenuInfoDto>)children;
    }
}
