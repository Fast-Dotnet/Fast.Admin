// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Shared;

/// <summary>
/// 消息模板类型枚举
/// </summary>
[FastEnum("消息模板类型枚举")]
public enum MessageTemplateTypeEnum : long
{
    /// <summary>
    /// 用户注册验证码
    /// </summary>
    [Description("注册验证码")]
    RegisterVerifyCode = 1L,

    /// <summary>
    /// 用户登录验证码
    /// </summary>
    [Description("登录验证码")]
    LoginVerifyCode = 2L,

    /// <summary>
    /// 密码操作验证码
    /// </summary>
    [Description("密码操作验证码")]
    PasswordOperationVerifyCode = 3L,

    /// <summary>
    /// 手机号验证码
    /// </summary>
    [Description("手机号验证码")]
    MobileVerifyCode = 4L,

    /// <summary>
    /// 注销账号验证码
    /// </summary>
    [Description("注销账号验证码")]
    AccountCancellationVerifyCode = 5L

    /* 业务模板类型，建议从 1001L 开始 */
}
