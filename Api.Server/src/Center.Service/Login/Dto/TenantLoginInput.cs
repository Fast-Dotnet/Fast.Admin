// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Login.Dto;

/// <summary>
/// 租户登录输入
/// </summary>
public class TenantLoginInput
{
    /// <summary>
    /// 用户Key
    /// </summary>
    [StringRequired(ErrorMessage = "用户Key不能为空")]
    [MaxLength(50, ErrorMessage = "用户Key不能超过50位字符")]
    public string UserKey { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [StringLength(20, MinimumLength = 6, ErrorMessage = "密码长度必须为 6~20 位字符")]
    public string Password { get; set; }

    /// <summary>
    /// 登录凭据
    /// </summary>
    /// <remarks>无登录凭证的时候必须提供密码</remarks>
    [MaxLength(32, ErrorMessage = "登录凭据不能超过32位字符")]
    public string LoginTicket { get; set; }

    /// <summary>
    /// 图片验证码Key
    /// </summary>
    [MaxLength(32, ErrorMessage = "图片验证码Key不能超过32位字符")]
    public string CaptchaKey { get; set; }

    /// <summary>
    /// 图片验证码
    /// </summary>
    [MaxLength(4, ErrorMessage = "图片验证码不能超过4位字符")]
    [RegularExpression(RegexConst.ImageCaptchaCode, ErrorMessage = "图片验证码必须为4位字母或数字")]
    public string CaptchaCode { get; set; }
}
