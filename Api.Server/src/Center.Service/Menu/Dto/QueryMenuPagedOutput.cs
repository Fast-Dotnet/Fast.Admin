// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections;
using Fast.Center.Domain;

namespace Fast.Center.Service.Menu.Dto;

/// <summary>
/// 获取菜单列表输出
/// </summary>
public class QueryMenuPagedOutput : PagedOutput, ITreeNode<long>
{
    /// <summary>
    /// 菜单Id
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; }

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
    /// 菜单类型
    /// </summary>
    public MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    public RoleTypeEnum RoleType { get; set; }

    /// <summary>
    /// 是否桌面端
    /// </summary>
    public bool HasDesktop { get; set; }

    /// <summary>
    /// 桌面端图标
    /// </summary>
    public string DesktopIcon { get; set; }

    /// <summary>
    /// 桌面端路由地址
    /// </summary>
    public string DesktopRouter { get; set; }

    /// <summary>
    /// 是否Web端
    /// </summary>
    public bool HasWeb { get; set; }

    /// <summary>
    /// Web端图标
    /// </summary>
    public string WebIcon { get; set; }

    /// <summary>
    /// Web端路由地址
    /// </summary>
    public string WebRouter { get; set; }

    /// <summary>
    /// Web端组件地址
    /// </summary>
    public string WebComponent { get; set; }

    /// <summary>
    /// Web端页面是否在导航栏显示
    /// </summary>
    public bool WebTab { get; set; }

    /// <summary>
    /// Web端页面是否缓存
    /// </summary>
    public bool WebKeepAlive { get; set; }

    /// <summary>
    /// 是否移动端
    /// </summary>
    public bool HasMobile { get; set; }

    /// <summary>
    /// 移动端图标
    /// </summary>
    public string MobileIcon { get; set; }

    /// <summary>
    /// 移动端路由地址
    /// </summary>
    public string MobileRouter { get; set; }

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
    /// 状态
    /// </summary>
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<QueryMenuPagedOutput> Children { get; set; }

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
        Children = (List<QueryMenuPagedOutput>)children;
    }
}
