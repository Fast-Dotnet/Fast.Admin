// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Admin.Service.Employee.Dto;

/// <summary>
/// 编辑职员常用菜单输入
/// </summary>
public class EditCommonMenusInput
{
    /// <summary>
    /// 按展示顺序排列的菜单编码；空集合表示清空常用菜单
    /// </summary>
    [Required(ErrorMessage = "常用菜单集合不能为空")]
    public List<string> CommonMenuCodes { get; set; }
}
