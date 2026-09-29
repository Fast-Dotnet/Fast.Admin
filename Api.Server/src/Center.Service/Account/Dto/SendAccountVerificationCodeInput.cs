// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Account.Dto;

/// <summary>
/// 发送账号校验验证码输入
/// </summary>
public class SendAccountVerificationCodeInput
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <remarks>手机号/邮箱</remarks>
    [StringRequired(ErrorMessage = "账号不能为空")]
    [MaxLength(50, ErrorMessage = "账号不能超过50位字符")]
    public string Account { get; set; }

    /// <summary>
    /// 图片验证码Key
    /// </summary>
    [StringRequired(ErrorMessage = "图片验证码Key不能为空")]
    [MaxLength(32, ErrorMessage = "图片验证码Key不能超过32位字符")]
    public string CaptchaKey { get; set; }

    /// <summary>
    /// 图片验证码
    /// </summary>
    [StringRequired(ErrorMessage = "图片验证码不能为空")]
    [MaxLength(4, ErrorMessage = "图片验证码不能超过4位字符")]
    [RegularExpression(RegexConst.ImageCaptchaCode, ErrorMessage = "图片验证码必须为4位字母或数字")]
    public string CaptchaCode { get; set; }
}
