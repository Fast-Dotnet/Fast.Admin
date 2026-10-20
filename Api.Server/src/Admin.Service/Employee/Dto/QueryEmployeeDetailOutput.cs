// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;

namespace Fast.Admin.Service.Employee.Dto;

/// <summary>
/// 获取职员详情输出
/// </summary>
public class QueryEmployeeDetailOutput : PagedOutput
{
    /// <summary>
    /// 职员Id
    /// </summary>
    public long EmployeeId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    public string EmployeeNo { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string EmployeeName { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    public string Mobile { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public EmployeeStatusEnum Status { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderEnum Sex { get; set; }

    /// <summary>
    /// 证件照
    /// </summary>
    public string IdPhoto { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// 离职日期
    /// </summary>
    public DateTime? ResignDate { get; set; }

    /// <summary>
    /// 离职原因
    /// </summary>
    public string ResignReason { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 机构信息
    /// </summary>
    public List<EmployeeOrgModel> OrgList { get; set; }

    /// <summary>
    /// 角色信息
    /// </summary>
    public List<EmployeeRoleModel> RoleList { get; set; }
}
