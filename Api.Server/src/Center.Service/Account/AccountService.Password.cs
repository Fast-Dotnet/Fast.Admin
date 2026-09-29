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
using Fast.CenterLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Account;

public partial class AccountService
{
    /// <summary>
    /// 密码事务提交后发送通知，通知失败不改变密码操作结果。
    /// </summary>
    private async Task SendPasswordChangedNotification(AccountModel account, string operation)
    {
        if (string.IsNullOrWhiteSpace(account.Email))
        {
            return;
        }

        try
        {
            const string title = "账号密码变更通知";
            string content = $"""
                              <p>您的账号已完成：{operation}。</p>
                              <p>操作时间：{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}。</p>
                              <p>如非本人或授权管理员操作，请及时联系管理员处理。</p>
                              """;
            await _mailService.SendEmail(title, await _mailService.GetEmailTemplate(title, content, "warn"), [account.Email]);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 校验最近使用的密码
    /// </summary>
    private async Task EnsurePasswordNotReused(long accountId, string newPassword)
    {
        // 查询最近3次密码修改记录
        List<string> passwordRecordList = await _repository.Queryable<PasswordRecordModel>()
            .Where(wh => wh.AccountId == accountId)
            .OrderByDescending(ob => ob.CreatedTime)
            .Take(3)
            .Select(sl => sl.Password)
            .ToListAsync();
        if (passwordRecordList.Any(history => CryptoUtil.VerifyPasswordPBKDF2SHA256(newPassword, history)))
        {
            throw new UserFriendlyException("新密码不能与最近3次使用的密码相同！");
        }
    }

    /// <summary>
    /// 校验密码复杂度
    /// </summary>
    private void VerifyPasswordComplexity(string newPassword, string confirmPassword)
    {
        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            throw new UserFriendlyException("新密码和确认密码不一致！");
        }

        // 客户端提交原始密码后，服务端可以直接校验真实复杂度
        if (string.IsNullOrEmpty(newPassword) || !Regex.IsMatch(newPassword, RegexConst.MediumPassword))
        {
            throw new UserFriendlyException("新密码长度必须为8~20位，且必须包含大小写字母、数字！");
        }
    }

    /// <summary>
    /// 账号解除锁定
    /// </summary>
    [HttpPost]
    [ApiInfo("账号解除锁定", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Account.Unlock)]
    [PlatformOnly]
    public async Task Unlock(AccountIdInput input)
    {
        AccountModel accountModel = await _repository.SingleOrDefaultAsync(input.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        DateTime dateTime = DateTime.Now;

        // 判断是否存在锁定
        if (accountModel.LockEndTime == null || accountModel.LockEndTime < dateTime)
        {
            throw new UserFriendlyException("账号未锁定！");
        }

        accountModel.PasswordErrorTime = null;
        accountModel.LockStartTime = null;
        accountModel.LockEndTime = null;
        accountModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(accountModel);
    }

    /// <summary>
    /// 账号更改状态
    /// </summary>
    [HttpPost]
    [ApiInfo("账号更改状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Account.Status)]
    [PlatformOnly]
    public async Task ChangeStatus(AccountIdInput input)
    {
        if (_user.AccountId == input.AccountId)
        {
            throw new UserFriendlyException("禁止更改当前登录账号状态！");
        }

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(input.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 更改状态
        accountModel.Status = accountModel.Status switch
        {
            CommonStatusEnum.Enable => CommonStatusEnum.Disable,
            CommonStatusEnum.Disable => CommonStatusEnum.Enable,
            _ => accountModel.Status
        };
        accountModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(accountModel);

        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            await _user.RevokeAccount(accountModel.AccountId);
            await AccountForceOffline(accountModel.AccountId, "账号已被禁用");
        }
    }

    /// <summary>
    /// 账号修改密码
    /// </summary>
    [HttpPost]
    [ApiInfo("账号修改密码", HttpRequestActionEnum.Edit)]
    public async Task ChangePassword(ChangePasswordInput input)
    {
        VerifyPasswordComplexity(input.NewPassword, input.ConfirmPassword);

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(_user.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (!CryptoUtil.VerifyPasswordPBKDF2SHA256(input.OldPassword, accountModel.Password))
        {
            throw new UserFriendlyException("旧密码不正确！");
        }

        await EnsurePasswordNotReused(accountModel.AccountId, input.NewPassword);

        // 更新密码
        accountModel.Password = CryptoUtil.HashPasswordPBKDF2SHA256(input.NewPassword);
        accountModel.RowVersion = input.RowVersion;

        HttpContext httpContext = FastContext.HttpContext;
        ISqlSugarRepository<VisitLogModel> _visitLogRepository =
            httpContext.RequestServices.GetService<ISqlSugarRepository<VisitLogModel>>();

        // 添加访问日志
        var visitLogModel = new VisitLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            VisitType = VisitTypeEnum.ChangePassword,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _user.TenantId,
            TenantName = _user.TenantName
        };
        visitLogModel.RecordCreate(httpContext);

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(accountModel);
            await _repository.Insertable(new PasswordRecordModel
                {
                    AccountId = accountModel.AccountId,
                    OperationType = PasswordOperationTypeEnum.Change,
                    Type = PasswordTypeEnum.PBKDF2_SHA256,
                    Password = accountModel.Password
                })
                .ExecuteCommandAsync();
            await _visitLogRepository.InsertAsync(visitLogModel);
        }, ex => throw ex);

        // 退出登录
        await _user.Logout();
        await _user.RevokeAccount(accountModel.AccountId);
        await AccountForceOffline(accountModel.AccountId, "密码已修改，请重新登录");

        // 发送通知
        await SendPasswordChangedNotification(accountModel, "用户修改密码");
    }

    /// <summary>
    /// 账号重置密码
    /// </summary>
    [HttpPost]
    [ApiInfo("账号重置密码", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Account.ResetPassword)]
    [PlatformOnly]
    public async Task ResetPassword(AccountIdInput input)
    {
        if (_user.AccountId == input.AccountId)
        {
            throw new UserFriendlyException("禁止重置当前登录账号密码！");
        }

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(input.AccountId);
        if (accountModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 更新密码
        accountModel.Password = CryptoUtil.HashPasswordPBKDF2SHA256(CommonConst.Default.Password);
        accountModel.RowVersion = input.RowVersion;

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(accountModel);
            await _repository.Insertable(new PasswordRecordModel
                {
                    AccountId = accountModel.AccountId,
                    OperationType = PasswordOperationTypeEnum.Reset,
                    Type = PasswordTypeEnum.PBKDF2_SHA256,
                    Password = accountModel.Password
                })
                .ExecuteCommandAsync();
        }, ex => throw ex);

        await _user.RevokeAccount(accountModel.AccountId);
        await AccountForceOffline(accountModel.AccountId, "密码已重置，请重新登录");

        // 发送通知
        await SendPasswordChangedNotification(accountModel, "管理员重置密码");
    }

    /// <summary>
    /// 密码重置缓存Dto
    /// </summary>
    private class PasswordResetCacheDto
    {
        /// <summary>
        /// 账号Id
        /// </summary>
        public long? AccountId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 密码Hash
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public MessageSendChannelEnum Channel { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientIdentity { get; set; }
    }

    /// <summary>
    /// 发送密码重置验证码
    /// </summary>
    [HttpPost]
    [ApiInfo("发送密码重置验证码", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    public async Task<SendPasswordResetCodeOutput> SendPasswordResetCode(SendPasswordResetCodeInput input)
    {
        await EnsureApplication();
        await _captchaService.VerifyImageCaptcha(input.CaptchaKey, input.CaptchaCode);

        string account = input.Account.Trim().ToLowerInvariant();

        MessageSendChannelEnum sendChannel;
        if (Regex.IsMatch(account, RegexConst.Mobile))
        {
            sendChannel = MessageSendChannelEnum.Sms;
        }
        else if (Regex.IsMatch(account, RegexConst.EmailAddress))
        {
            sendChannel = MessageSendChannelEnum.Email;
        }
        else
        {
            throw new UserFriendlyException("请输入正确的手机号或邮箱！");
        }

        // 同一个IP地址，1小时内最多允许20次
        await EnforceSendQuota($"Ip:{FastContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv6()
                                         .ToString()
                                     ?? "unknown"}", (20, 3600));

        // 冷却和公开配额只依赖输入目标，账号不存在、被禁用或发送失败时也执行相同限制。
        string recipient = $"PasswordResetRecipient:{sendChannel}:{account}";
        // 60秒1次，1小时5次，24小时10次
        await EnforceSendQuota(recipient, (1, 60), (5, 3600), (10, 86400));

        // 生成验证Key
        string verificationKey = Guid.NewGuid().ToString("N");

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.PasswordReset, verificationKey);
        // 所有合法格式的账号均返回同样的凭据；未实际发送的凭据不能用于重置密码。
        var dto = new PasswordResetCacheDto {Channel = sendChannel, ClientIdentity = GlobalContext.ClientIdentity};
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        var output = new SendPasswordResetCodeOutput
        {
            VerificationKey = verificationKey, Message = "如账号存在且验证通道可用，验证码将发送至账号绑定的联系方式。"
        };

        AccountModel accountModel = await _repository.Queryable<AccountModel>()
            .WhereIF(sendChannel == MessageSendChannelEnum.Sms, wh => wh.Mobile == account)
            .WhereIF(sendChannel == MessageSendChannelEnum.Email, wh => wh.Email == account)
            .SingleAsync();
        if (accountModel != null && accountModel.Status != CommonStatusEnum.Disable)
        {
            try
            {
                // 保留账号维度的共享配额；内部配额和供应商异常不能暴露账号是否存在。
                recipient = $"Recipient:{sendChannel}:{accountModel.AccountKey}";
                // 1小时5次，24小时10次
                await EnforceSendQuota(recipient, (5, 3600), (10, 86400));

                switch (sendChannel)
                {
                    case MessageSendChannelEnum.Email:
                        await _mailService.SendVerificationCode(MailTypeEnum.ChangePassword, accountModel.Email);
                        break;
                    case MessageSendChannelEnum.Sms:
                        await _smsService.SendVerificationCode(SmsTypeEnum.ChangePassword, accountModel.Mobile);
                        break;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return output;
            }

            dto.AccountId = accountModel.AccountId;
            dto.Mobile = accountModel.Mobile;
            dto.Email = accountModel.Email;
            dto.PasswordHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password ?? string.Empty)));
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        }

        return output;
    }

    /// <summary>
    /// 通过匿名验证码重置密码
    /// </summary>
    [HttpPost]
    [ApiInfo("通过验证码重置密码", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    public async Task ResetPasswordByVerificationCode(PasswordResetInput input)
    {
        await EnsureApplication();
        VerifyPasswordComplexity(input.NewPassword, input.ConfirmPassword);

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.PasswordReset, input.VerificationKey);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 30);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        PasswordResetCacheDto dto = await _cache.GetAsync<PasswordResetCacheDto>(cacheKey);
        if (dto == null || dto.AccountId == null || dto.ClientIdentity != GlobalContext.ClientIdentity)
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        AccountModel accountModel = await _repository.SingleOrDefaultAsync(dto.AccountId);
        if (accountModel == null || accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        if (!string.Equals(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password ?? string.Empty))),
                dto.PasswordHash)
            || (dto.Channel == MessageSendChannelEnum.Sms && dto.Mobile != accountModel.Mobile)
            || (dto.Channel == MessageSendChannelEnum.Email && dto.Email != accountModel.Email))
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        switch (dto.Channel)
        {
            case MessageSendChannelEnum.Email:
                await _mailService.VerifyVerificationCode(MailTypeEnum.ChangePassword, accountModel.Email,
                    input.VerificationCode);
                break;
            case MessageSendChannelEnum.Sms:
                await _smsService.VerifyVerificationCode(SmsTypeEnum.ChangePassword, accountModel.Mobile, input.VerificationCode);
                break;
        }

        // 正常验证码只能消费一次，即使后续密码策略校验失败也需要重新发送
        await _cache.DelAsync(cacheKey);

        await EnsurePasswordNotReused(accountModel.AccountId, input.NewPassword);

        // 判断是否新旧密码一致
        if (!string.IsNullOrWhiteSpace(accountModel.Password)
            && CryptoUtil.VerifyPasswordPBKDF2SHA256(input.NewPassword, accountModel.Password))
        {
            throw new UserFriendlyException("新密码不能与当前密码相同！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        ISqlSugarRepository<VisitLogModel> _visitLogRepository =
            httpContext.RequestServices.GetService<ISqlSugarRepository<VisitLogModel>>();

        // 添加访问日志
        var visitLogModel = new VisitLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = accountModel.AccountId,
            Mobile = accountModel.Mobile,
            NickName = accountModel.NickName,
            VisitType = VisitTypeEnum.ChangePassword,
            CreatedTime = DateTime.Now
        };
        visitLogModel.RecordCreate(httpContext);

        // 更新密码
        accountModel.Password = CryptoUtil.HashPasswordPBKDF2SHA256(input.NewPassword);
        accountModel.PasswordErrorTime = null;
        accountModel.LockStartTime = null;
        accountModel.LockEndTime = null;

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.Updateable(accountModel)
                .UpdateColumns(e => new {e.Password, e.PasswordErrorTime, e.LockStartTime, e.LockEndTime})
                .ExecuteCommandWithOptLockAsync(true);
            await _repository.Insertable(new PasswordRecordModel
                {
                    AccountId = accountModel.AccountId,
                    OperationType = PasswordOperationTypeEnum.Change,
                    Type = PasswordTypeEnum.PBKDF2_SHA256,
                    Password = accountModel.Password
                })
                .ExecuteCommandAsync();
            await _visitLogRepository.InsertAsync(visitLogModel);
        }, ex => throw ex);

        await _user.RevokeAccount(accountModel.AccountId);
        await AccountForceOffline(accountModel.AccountId, "密码已重置，请重新登录");

        // 发送通知
        await SendPasswordChangedNotification(accountModel, $"通过{dto.Channel.GetDescription()}验证码找回并重置密码");
    }
}
