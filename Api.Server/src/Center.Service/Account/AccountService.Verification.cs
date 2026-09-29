// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CSRedis;
using Fast.Center.Domain;
using Fast.Center.Service.Account.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Account;

public partial class AccountService
{
    /// <summary>
    /// 账号校验缓存Dto
    /// </summary>
    private class AccountVerificationCacheDto
    {
        /// <summary>
        /// 账号Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 手机号验证码过期时间
        /// </summary>
        public DateTime MobileExpiresTime { get; set; }

        /// <summary>
        /// 手机号已验证
        /// </summary>
        public bool MobileVerified { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 邮箱验证码过期时间
        /// </summary>
        public DateTime EmailExpiresTime { get; set; }

        /// <summary>
        /// 邮箱已验证
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// 密码Hash
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientIdentity { get; set; }
    }

    /// <summary>
    /// 发送账号校验验证码
    /// </summary>
    [HttpPost]
    [ApiInfo("发送账号校验验证码", HttpRequestActionEnum.Auth)]
    public async Task SendAccountVerificationCode(SendAccountVerificationCodeInput input)
    {
        await EnsureApplication();
        await _captchaService.VerifyImageCaptcha(input.CaptchaKey, input.CaptchaCode);

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(_user.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 验证账号状态
        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("账号已被平台禁用！");
        }

        // 验证状态
        if (accountModel.IdentityVerification)
        {
            // 退出登录
            await _user.Logout();
            throw new UserFriendlyException("账号已校验完成，请刷新用户信息！");
        }

        string account = input.Account.Trim().ToLowerInvariant();

        MessageSendChannelEnum sendChannel;
        if (Regex.IsMatch(account, RegexConst.Mobile))
        {
            sendChannel = MessageSendChannelEnum.Sms;
            if (await _repository.AnyAsync(a => a.Mobile == account && a.AccountId != _user.AccountId))
            {
                throw new UserFriendlyException("手机号已存在账号信息！");
            }
        }
        else if (Regex.IsMatch(account, RegexConst.EmailAddress))
        {
            sendChannel = MessageSendChannelEnum.Email;
            if (await _repository.AnyAsync(a => a.Email == account && a.AccountId != _user.AccountId))
            {
                throw new UserFriendlyException("邮箱已存在账号信息！");
            }
        }
        else
        {
            throw new UserFriendlyException("请输入正确的手机号或邮箱！");
        }

        // 同一个IP地址，1小时内最多允许20次
        await EnforceSendQuota($"Ip:{FastContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv6()
                                         .ToString()
                                     ?? "unknown"}", (20, 3600));
        string recipient = $"Recipient:{sendChannel}:{accountModel.AccountKey}";
        // 1小时5次，24小时10次
        await EnforceSendQuota(recipient, (5, 3600), (10, 86400));

        // 不同客户端独立保存校验进度，发送与提交共用锁，避免覆盖已验证的结果。
        string clientIdentity = GlobalContext.ClientIdentity;
        string cacheKey = CacheConst.GetCacheKey(CacheConst.AccountIdentityVerification, accountModel.AccountKey, clientIdentity);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 120);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password)));
        AccountVerificationCacheDto dto = await _cache.GetAsync<AccountVerificationCacheDto>(cacheKey);
        if (dto == null
            || dto.AccountId != accountModel.AccountId
            || dto.ClientIdentity != clientIdentity
            || dto.PasswordHash != passwordHash)
        {
            dto = new AccountVerificationCacheDto
            {
                AccountId = accountModel.AccountId, PasswordHash = passwordHash, ClientIdentity = clientIdentity
            };
        }

        DateTime expiresTime = DateTime.Now.AddMinutes(5);
        switch (sendChannel)
        {
            case MessageSendChannelEnum.Email:
                await _mailService.SendVerificationCode(MailTypeEnum.Validity, account);
                dto.Email = account;
                dto.EmailExpiresTime = expiresTime;
                dto.EmailVerified = false;
                break;
            case MessageSendChannelEnum.Sms:
                await _smsService.SendVerificationCode(SmsTypeEnum.Validity, account);
                dto.Mobile = account;
                dto.MobileExpiresTime = expiresTime;
                dto.MobileVerified = false;
                break;
        }

        // 重发只重置当前通道，另一通道的校验结果仍受其原始过期时间限制。
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// 账号校验
    /// </summary>
    [HttpPost]
    [ApiInfo("账号校验", HttpRequestActionEnum.Auth)]
    public async Task AccountVerification(AccountVerificationInput input)
    {
        await EnsureApplication();

        string mobile = input.Mobile.Trim();
        string email = input.Email.Trim().ToLowerInvariant();
        AccountModel accountModel = await _repository.SingleOrDefaultAsync(_user.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 验证账号状态
        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("账号已被平台禁用！");
        }

        // 验证状态
        if (accountModel.IdentityVerification)
        {
            throw new UserFriendlyException("账号已校验完成，请勿重复校验！");
        }

        if (await _repository.AnyAsync(a => a.Mobile == mobile && a.AccountId != _user.AccountId))
        {
            throw new UserFriendlyException("手机号已存在账号信息！");
        }

        if (await _repository.AnyAsync(a => a.Email == email && a.AccountId != _user.AccountId))
        {
            throw new UserFriendlyException("邮箱已存在账号信息！");
        }

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.AccountIdentityVerification, accountModel.AccountKey,
            GlobalContext.ClientIdentity);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 120);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        AccountVerificationCacheDto dto = await _cache.GetAsync<AccountVerificationCacheDto>(cacheKey);
        if (dto == null || dto.AccountId != accountModel.AccountId || dto.ClientIdentity != GlobalContext.ClientIdentity)
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        if (!string.Equals(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password))), dto.PasswordHash))
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        if (dto.Mobile != mobile
            || dto.Email != email
            || dto.MobileExpiresTime <= DateTime.Now
            || dto.EmailExpiresTime <= DateTime.Now)
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        // 分别保存已验证的结果，邮箱输错或后续更新失败时，不会要求重发已通过的短信验证码。
        if (!dto.MobileVerified)
        {
            await _smsService.VerifyVerificationCode(SmsTypeEnum.Validity, mobile, input.MobileVerificationCode);
            dto.MobileVerified = true;
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        }

        if (!dto.EmailVerified)
        {
            await _mailService.VerifyVerificationCode(MailTypeEnum.Validity, email, input.EmailVerificationCode);
            dto.EmailVerified = true;
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        }

        // 更新手机号，邮箱，校验标识
        accountModel.Mobile = mobile;
        accountModel.Email = email;
        accountModel.IdentityVerification = true;

        await _repository.Updateable(accountModel)
            .UpdateColumns(e => new {e.Mobile, e.Email, e.IdentityVerification})
            .ExecuteCommandWithOptLockAsync(true);

        await _cache.DelAsync(cacheKey);

        // 退出登录
        await _user.Logout();
        await _user.RevokeAccount(accountModel.AccountId);
        await AccountForceOffline(accountModel.AccountId, "账号已修改，请重新登录");
    }

    /// <summary>
    /// 发送编辑账号验证码
    /// </summary>
    [HttpPost]
    [ApiInfo("发送编辑账号验证码", HttpRequestActionEnum.Auth)]
    public async Task SendEditAccountVerificationCode(SendAccountVerificationCodeInput input)
    {
        await EnsureApplication();
        await _captchaService.VerifyImageCaptcha(input.CaptchaKey, input.CaptchaCode);

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(_user.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 验证账号状态
        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("账号已被平台禁用！");
        }

        string account = input.Account.Trim().ToLowerInvariant();
        MessageSendChannelEnum sendChannel;
        if (Regex.IsMatch(account, RegexConst.Mobile))
        {
            sendChannel = MessageSendChannelEnum.Sms;
            if (accountModel.Mobile == account)
            {
                throw new UserFriendlyException("手机号未发生变化！");
            }

            if (await _repository.AnyAsync(a => a.Mobile == account && a.AccountId != _user.AccountId))
            {
                throw new UserFriendlyException("手机号已存在账号信息！");
            }
        }
        else if (Regex.IsMatch(account, RegexConst.EmailAddress))
        {
            sendChannel = MessageSendChannelEnum.Email;
            if (string.Equals(accountModel.Email, account, StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException("邮箱未发生变化！");
            }

            if (await _repository.AnyAsync(a => a.Email == account && a.AccountId != _user.AccountId))
            {
                throw new UserFriendlyException("邮箱已存在账号信息！");
            }
        }
        else
        {
            throw new UserFriendlyException("请输入正确的手机号或邮箱！");
        }

        // 同一个IP地址，1小时内最多允许20次
        await EnforceSendQuota($"Ip:{FastContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv6()
                                         .ToString()
                                     ?? "unknown"}", (20, 3600));
        string recipient = $"EditAccount:{sendChannel}:{accountModel.AccountKey}";
        // 1小时5次，24小时10次
        await EnforceSendQuota(recipient, (5, 3600), (10, 86400));

        // 不同客户端独立保存校验进度，发送与提交共用锁，避免覆盖已验证的结果。
        string clientIdentity = GlobalContext.ClientIdentity;
        string cacheKey = CacheConst.GetCacheKey(CacheConst.AccountIdentityVerification, accountModel.AccountKey, clientIdentity);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 120);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        string passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password)));
        AccountVerificationCacheDto dto = await _cache.GetAsync<AccountVerificationCacheDto>(cacheKey);
        if (dto == null
            || dto.AccountId != accountModel.AccountId
            || dto.ClientIdentity != clientIdentity
            || dto.PasswordHash != passwordHash)
        {
            dto = new AccountVerificationCacheDto
            {
                AccountId = accountModel.AccountId, PasswordHash = passwordHash, ClientIdentity = clientIdentity
            };
        }

        DateTime expiresTime = DateTime.Now.AddMinutes(5);
        switch (sendChannel)
        {
            case MessageSendChannelEnum.Email:
                await _mailService.SendVerificationCode(MailTypeEnum.Validity, account);
                dto.Email = account;
                dto.EmailExpiresTime = expiresTime;
                dto.EmailVerified = false;
                break;
            case MessageSendChannelEnum.Sms:
                await _smsService.SendVerificationCode(SmsTypeEnum.Validity, account);
                dto.Mobile = account;
                dto.MobileExpiresTime = expiresTime;
                dto.MobileVerified = false;
                break;
        }

        // 重发只重置当前通道，另一通道的校验结果仍受其原始过期时间限制。
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
    }
}
