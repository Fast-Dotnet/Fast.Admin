// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Tenant.Dto;

/// <summary>
/// 添加租户输入
/// </summary>
public class AddTenantInput
{
    /// <summary>
    /// 租户编码
    /// </summary>
    [StringRequired(ErrorMessage = "租户编码不能为空")]
    [MaxLength(5, ErrorMessage = "租户编码不能超过5个字符")]
    public string TenantCode { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    [StringRequired(ErrorMessage = "租户名称不能为空")]
    public string TenantName { get; set; }

    /// <summary>
    /// 租户简称
    /// </summary>
    [StringRequired(ErrorMessage = "租户简称不能为空")]
    public string ShortName { get; set; }

    /// <summary>
    /// 租户英文名称
    /// </summary>
    [StringRequired(ErrorMessage = "租户英文名称不能为空")]
    public string SpellName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [EnumRequired(ErrorMessage = "版本不能为空", AllowZero = true)]
    public EditionEnum Edition { get; set; }

    /// <summary>
    /// 租户管理员名称
    /// </summary>
    [StringRequired(ErrorMessage = "租户管理员名称不能为空")]
    public string AdminName { get; set; }

    /// <summary>
    /// 租户管理员手机
    /// </summary>
    [StringRequired(ErrorMessage = "租户管理员手机不能为空")]
    [RegularExpression(RegexConst.Mobile, ErrorMessage = "手机格式不正确")]
    public string AdminMobile { get; set; }

    /// <summary>
    /// 租户管理员邮箱
    /// </summary>
    [StringRequired(ErrorMessage = "租户管理员邮箱不能为空")]
    [RegularExpression(RegexConst.EmailAddress, ErrorMessage = "邮箱格式不正确")]
    public string AdminEmail { get; set; }

    /// <summary>
    /// 租户管理员电话
    /// </summary>
    public string AdminPhone { get; set; }

    /// <summary>
    /// 租户机器人名称
    /// </summary>
    [StringRequired(ErrorMessage = "租户机器人名称不能为空")]
    public string RobotName { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    [StringRequired(ErrorMessage = "LogoUrl不能为空")]
    public string LogoUrl { get; set; }
}
