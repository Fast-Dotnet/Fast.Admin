// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Admin.Service.Department.Dto;

/// <summary>
/// 获取部门详情输出
/// </summary>
public class QueryDepartmentDetailOutput : PagedOutput
{
    /// <summary>
    /// 部门Id
    /// </summary>
    public long DepartmentId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 父级名称
    /// </summary>
    public string ParentName { get; set; }

    /// <summary>
    /// 父级Id集合
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<long> ParentIds { get; set; }

    /// <summary>
    /// 父级名称集合
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<string> ParentNames { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DepartmentName { get; set; }

    /// <summary>
    /// 部门编码
    /// </summary>
    public string DepartmentCode { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    public string Contacts { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 数据公开
    /// </summary>
    public bool DataPublic { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
