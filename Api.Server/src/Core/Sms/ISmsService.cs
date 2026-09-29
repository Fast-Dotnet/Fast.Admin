// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 短信服务
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// 获取当前验证码发送的剩余等待秒数
    /// </summary>
    /// <param name="smsType">短信类型</param>
    /// <param name="mobile">手机号</param>
    Task<int> GetVerificationCodeRetryAfterSeconds(SmsTypeEnum smsType, string mobile);

    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="smsType">短信类型</param>
    /// <param name="mobile">手机号</param>
    Task SendVerificationCode(SmsTypeEnum smsType, string mobile);

    /// <summary>
    /// 验证并一次性消费验证码
    /// </summary>
    /// <param name="smsType">短信类型</param>
    /// <param name="mobile">手机号</param>
    /// <param name="verificationCode">验证码</param>
    Task VerifyVerificationCode(SmsTypeEnum smsType, string mobile, string verificationCode);

    /// <summary>
    /// 发送模板短信
    /// </summary>
    /// <param name="mobile">手机号</param>
    /// <param name="templateCode">模板编码</param>
    /// <param name="templateParam">模板参数对象</param>
    /// <param name="accessKeyId">访问密钥Id，为null时读取配置</param>
    /// <param name="accessKeySecret">访问密钥Secret，为null时读取配置</param>
    /// <param name="signName">短信签名，为null时读取配置</param>
    Task SendSms(string mobile, string templateCode, object templateParam, string accessKeyId = null,
        string accessKeySecret = null, string signName = null);
}
