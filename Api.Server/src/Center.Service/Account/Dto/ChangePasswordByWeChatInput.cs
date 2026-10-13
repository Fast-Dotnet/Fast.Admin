// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Account.Dto;

/// <summary>
/// 通过已绑定微信修改密码输入
/// </summary>
public class ChangePasswordByWeChatInput : UpdateVersionInput
{
    /// <summary>
    /// wx.login 获取到的Code
    /// </summary>
    [StringRequired(ErrorMessage = "微信登录Code不能为空")]
    public string WeChatCode { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [StringRequired(ErrorMessage = "新密码不能为空")]
    [RegularExpression(RegexConst.MediumPassword, ErrorMessage = "新密码长度必须为8~20位，且必须包含大小写字母、数字")]
    public string NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [StringRequired(ErrorMessage = "确认密码不能为空")]
    [Compare(nameof(NewPassword), ErrorMessage = "新密码和确认密码不一致")]
    public string ConfirmPassword { get; set; }
}
