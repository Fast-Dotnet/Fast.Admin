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
    /// 获取指定接收方和业务类型的验证码发送冷却时间。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="mobile">手机号</param>
    /// <returns>剩余等待秒数；没有有效冷却记录时为 0</returns>
    Task<int> GetVerificationCodeRetryAfterSeconds(MessageTemplateTypeEnum templateType, string mobile);

    /// <summary>
    /// 发送与当前客户端绑定的验证码，并设置发送冷却。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="signName">短信签名</param>
    /// <param name="mobile">手机号</param>
    /// <param name="appKey">应用标识，为 <see langword="null"/> 时查询全局模板</param>
    /// <remarks>验证码有效期为 5 分钟，发送冷却为 60 秒；发送失败也会保留本次冷却限制。</remarks>
    /// <exception cref="UserFriendlyException">接收地址不合法、仍在冷却期或发送配置不可用时抛出。</exception>
    Task SendVerificationCode(MessageTemplateTypeEnum templateType, string signName, string mobile, string appKey = null);

    /// <summary>
    /// 校验验证码，并在校验成功后删除对应记录。
    /// </summary>
    /// <param name="templateType">消息模板类型</param>
    /// <param name="mobile">手机号</param>
    /// <param name="verificationCode">用户输入的验证码</param>
    /// <remarks>校验绑定当前客户端；累计错误达到 5 次时验证码失效，未达到上限的失败不会延长有效期。</remarks>
    /// <exception cref="ArgumentNullException">验证码为 <see langword="null"/> 时抛出。</exception>
    /// <exception cref="UserFriendlyException">接收地址不合法、验证码错误或失效，或并发校验未取得锁时抛出。</exception>
    Task VerifyVerificationCode(MessageTemplateTypeEnum templateType, string mobile, string verificationCode);

    /// <summary>
    /// 发送模板短信
    /// </summary>
    /// <param name="signName">短信签名</param>
    /// <param name="mobile">手机号</param>
    /// <param name="templateCode">模板编码</param>
    /// <param name="templateParam">模板参数对象</param>
    /// <param name="accessKeyId">访问密钥Id，为 <see langword="null"/> 时读取配置</param>
    /// <param name="accessKeySecret">访问密钥Secret，为 <see langword="null"/> 时读取配置</param>
    Task SendSms(string signName, string mobile, string templateCode, object templateParam, string accessKeyId = null,
        string accessKeySecret = null);
}
