// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 验证码服务
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// 生成与当前客户端绑定的图片验证码，有效期为 5 分钟。
    /// </summary>
    /// <returns>验证码标识和带 PNG Data URL 前缀的图片内容</returns>
    Task<(string captchaKey, string captchaImage)> GetImageCaptcha();

    /// <summary>
    /// 校验图片验证码，并在读取到当前客户端的有效记录后立即消费。
    /// </summary>
    /// <param name="captchaKey">获取验证码时返回的标识，格式为不带分隔符的 GUID</param>
    /// <param name="verificationCode">用户输入的四位验证码，比较时忽略大小写</param>
    /// <remarks>当前客户端的有效记录在比较答案前删除，因此输入错误后也必须重新获取验证码。</remarks>
    /// <exception cref="UserFriendlyException">输入格式错误、验证码失效、客户端不匹配或并发校验未取得锁时抛出。</exception>
    Task VerifyImageCaptcha(string captchaKey, string verificationCode);
}
