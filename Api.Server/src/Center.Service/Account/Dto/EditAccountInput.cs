// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Account.Dto;

/// <summary>
/// 编辑账号输入
/// </summary>
public class EditAccountInput : UpdateVersionInput
{
    /// <summary>
    /// 客户端用户Id
    /// </summary>
    public long? ClientUserId { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [StringRequired(ErrorMessage = "手机不能为空")]
    [MaxLength(11, ErrorMessage = "手机号不能超过11位字符")]
    [RegularExpression(RegexConst.Mobile, ErrorMessage = "手机格式不正确")]
    public string Mobile { get; set; }

    /// <summary>
    /// 短信验证码
    /// </summary>
    [RegularExpression(@"^\d{6}$", ErrorMessage = "短信验证码必须为6位数字")]
    public string MobileVerificationCode { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [StringRequired(ErrorMessage = "邮箱不能为空")]
    [MaxLength(50, ErrorMessage = "邮箱不能超过50位字符")]
    [RegularExpression(RegexConst.EmailAddress, ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; }

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    [RegularExpression(@"^\d{6}$", ErrorMessage = "邮箱验证码必须为6位数字")]
    public string EmailVerificationCode { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [StringRequired(ErrorMessage = "昵称不能为空")]
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }
}
