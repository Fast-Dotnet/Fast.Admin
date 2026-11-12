// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Admin.Service.Role.Dto;

/// <summary>
/// 获取角色详情输出
/// </summary>
public class QueryRoleDetailOutput : PagedOutput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    public RoleTypeEnum RoleType { get; set; }

    /// <summary>
    /// 是否使用系统菜单
    /// </summary>
    public bool IsSystemMenu { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 数据范围类型
    /// </summary>
    public DataScopeTypeEnum DataScopeType { get; set; }

    /// <summary>
    /// 自定义数据范围部门Id集合
    /// </summary>
    public List<long> DataScopeDepartmentIds { get; set; }

    /// <summary>
    /// 自定义数据范围部门名称集合
    /// </summary>
    public List<string> DataScopeDepartmentNames { get; set; }

    /// <summary>
    /// 可分配的角色Id集合
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<long> AssignableRoleIds { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
