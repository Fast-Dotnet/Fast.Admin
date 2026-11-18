// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Admin.Service.Department.Dto;

/// <summary>
/// 获取部门分页列表输入
/// </summary>
public class QueryDepartmentPagedInput
{
    /// <summary>
    /// 搜索值
    /// </summary>
    public string SearchValue { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    public long? OrgId { get; set; }

    /// <summary>
    /// 树形数据
    /// </summary>
    public bool TreeData { get; set; } = true;
}
