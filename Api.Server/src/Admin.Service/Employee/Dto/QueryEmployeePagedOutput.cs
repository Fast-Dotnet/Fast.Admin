// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;

namespace Fast.Admin.Service.Employee.Dto;

/// <summary>
/// 获取职员分页列表输出
/// </summary>
public class QueryEmployeePagedOutput : PagedOutput
{
    /// <summary>
    /// 职员Id
    /// </summary>
    public long EmployeeId { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [SugarSearchValue]
    public string EmployeeNo { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [SugarSearchValue]
    public string EmployeeName { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    public string Mobile { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public EmployeeStatusEnum Status { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarSearchValue]
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
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    public long? OrgId { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<string> OrgNames { get; set; }

    /// <summary>
    /// 部门Id
    /// </summary>
    public long? DepartmentId { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DepartmentName { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<string> DepartmentNames { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    public long? PositionId { get; set; }

    /// <summary>
    /// 职位名称
    /// </summary>
    public string PositionName { get; set; }

    /// <summary>
    /// 职级Id
    /// </summary>
    public long? JobLevelId { get; set; }

    /// <summary>
    /// 职级名称
    /// </summary>
    public string JobLevelName { get; set; }

    /// <summary>
    /// 是否为负责人
    /// </summary>
    public bool? IsPrincipal { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleNames { get; set; }

    /// <summary>
    /// 账号状态
    /// </summary>
    public CommonStatusEnum AccountStatus { get; set; }

    /// <summary>
    /// 账号手机
    /// </summary>
    public string AccountMobile { get; set; }

    /// <summary>
    /// 账号邮箱
    /// </summary>
    public string AccountEmail { get; set; }

    /// <summary>
    /// 账号昵称
    /// </summary>
    public string AccountNickName { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
}
