// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AlibabaCloud.SDK.Dysmsapi20180501;
using AlibabaCloud.SDK.Dysmsapi20180501.Models;
using CSRedis;
using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// <see cref="ISmsService"/> 默认实现
/// </summary>
public class SMSService : ISmsService, ISingletonDependency
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache _cache;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化短信服务
    /// </summary>
    public SMSService(ICache cache, ILogger<ISmsService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 验证码缓存Dto
    /// </summary>
    private class VerificationCodeCacheDto
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientIdentity { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        /// <remarks>超过5次自动失效</remarks>
        public int ErrorCount { get; set; }
    }

    /// <inheritdoc/>
    public async Task<int> GetVerificationCodeRetryAfterSeconds(SmsTypeEnum smsType, string mobile)
    {
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Sms, smsType.ToString(), mobile.Trim().ToLowerInvariant());
        return (int)Math.Max(0, await _cache.Client.TtlAsync($"{cacheKey}:SendCooldown"));
    }

    /// <inheritdoc />
    public async Task SendVerificationCode(SmsTypeEnum smsType, string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile) || !Regex.IsMatch(mobile, RegexConst.Mobile))
        {
            throw new UserFriendlyException("手机号码不正确！");
        }

        mobile = mobile.Trim();

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Sms, smsType.ToString(), mobile);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 120);
        if (codeLock == null)
        {
            // 仅抢锁失败时读取锁剩余时间，毫秒向上取整，避免不足1秒被显示为0。
            long lockMilliseconds = await _cache.Client.PTtlAsync($"CSRedisClientLock:{cacheKey}:Lock");
            int lockSeconds = (int)Math.Ceiling(lockMilliseconds / 1000d);
            throw new UserFriendlyException(lockSeconds > 0
                ? $"操作过于频繁，请在 {TimeSpan.FromSeconds(lockSeconds).ToDescription()} 后重试！"
                : "操作过于频繁，请稍后重试！");
        }

        int retryAfterSeconds = await GetVerificationCodeRetryAfterSeconds(smsType, mobile);
        if (retryAfterSeconds > 0)
        {
            throw new UserFriendlyException($"操作过于频繁，请在 {TimeSpan.FromSeconds(retryAfterSeconds).ToDescription()} 后重试！");
        }

        // 发送前占用冷却，失败时保留重试限制，成功后重新计时。
        await _cache.Client.SetAsync($"{cacheKey}:SendCooldown", "1", 60);
        VerificationCodeCacheDto dto = await _cache.GetAsync<VerificationCodeCacheDto>(cacheKey);

        // 生成验证码
        dto ??= new VerificationCodeCacheDto();
        dto.VerificationCode = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        dto.ClientIdentity = GlobalContext.ClientIdentity;
        dto.SendTime = DateTime.Now;
        dto.ErrorCount = 0;

        // 获取模板Code
        string templateCode = await ConfigContext.GetConfig(ConfigConst.SmsVerificationTemplateCode);
        await SendSms(mobile, templateCode, new {Code = dto.VerificationCode});
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        await _cache.Client.SetAsync($"{cacheKey}:SendCooldown", "1", 60);
    }

    /// <inheritdoc/>
    public async Task VerifyVerificationCode(SmsTypeEnum smsType, string mobile, string verificationCode)
    {
        ArgumentNullException.ThrowIfNull(verificationCode);

        if (string.IsNullOrWhiteSpace(mobile) || !Regex.IsMatch(mobile, RegexConst.Mobile))
        {
            throw new UserFriendlyException("手机号码不正确！");
        }

        mobile = mobile.Trim();

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Sms, smsType.ToString(), mobile);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 30);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        VerificationCodeCacheDto dto = await _cache.GetAsync<VerificationCodeCacheDto>(cacheKey);
        if (dto is not {ErrorCount: < 5}
            || dto.ClientIdentity != GlobalContext.ClientIdentity
            || dto.SendTime.AddMinutes(5) <= DateTime.Now)
        {
            throw new UserFriendlyException("短信验证码无效或已过期！");
        }

        if (!string.Equals(dto.VerificationCode, verificationCode, StringComparison.Ordinal))
        {
            dto.ErrorCount++;
            // 错误次数达到上限后删除验证码
            if (dto.ErrorCount >= 5)
            {
                await _cache.DelAsync(cacheKey);
            }
            else
            {
                // // 更新错误次数并保持原有过期时间
                await _cache.SetAsync(cacheKey, dto, dto.SendTime.AddMinutes(5) - DateTime.Now);
            }

            throw new UserFriendlyException("短信验证码无效或已过期！");
        }

        await _cache.DelAsync(cacheKey);
    }

    /// <inheritdoc />
    public async Task SendSms(string mobile, string templateCode, object templateParam, string accessKeyId = null,
        string accessKeySecret = null, string signName = null)
    {
        ArgumentNullException.ThrowIfNull(templateCode);

        if (string.IsNullOrWhiteSpace(mobile) || !Regex.IsMatch(mobile, RegexConst.Mobile))
        {
            throw new UserFriendlyException("手机号码不正确！");
        }

        mobile = mobile.Trim();

        DateTime sendTime = DateTime.Now;
        bool isSuccess = false;
        try
        {
            accessKeyId ??= await ConfigContext.GetConfig(ConfigConst.SmsAccessKeyId);
            accessKeySecret ??= await ConfigContext.GetConfig(ConfigConst.SmsAccessKeySecret);
            signName ??= await ConfigContext.GetConfig(ConfigConst.SmsSignName);
            if (string.IsNullOrWhiteSpace(accessKeyId)
                || string.IsNullOrWhiteSpace(accessKeySecret)
                || string.IsNullOrWhiteSpace(signName)
                || string.IsNullOrWhiteSpace(templateCode))
            {
                throw new UserFriendlyException("短信发送配置不完整，请联系管理员！");
            }

            var client = new Client(new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                RegionId = "ap-southeast-1",
                Endpoint = "dysmsapi.ap-southeast-1.aliyuncs.com",
                ConnectTimeout = 5000,
                ReadTimeout = 10000
            });
            // 沿用SDK默认不自动重试的行为，避免超时后重复发送计费短信。
            SendMessageWithTemplateResponse response = await client.SendMessageWithTemplateAsync(
                new SendMessageWithTemplateRequest
                {
                    To = $"86{mobile}",
                    From = signName,
                    TemplateCode = templateCode,
                    TemplateParam = templateParam?.ToJsonString()
                });

            if (!string.Equals(response.Body.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    $"短信发送失败。ResponseCode: {response.Body.ResponseCode}, ResponseDescription: {response.Body.ResponseDescription}, RequestId: {response.Body.RequestId}");
                throw new UserFriendlyException("短信发送失败，请稍后重试！");
            }

            // 发送成功
            isSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"短信发送失败。\r\nMobile：{mobile}\r\nTemplateCode：{templateCode}\r\nTemplateParam：{templateParam?.ToJsonString()}");
            throw;
        }
        finally
        {
            // 写入消息发送记录
            try
            {
                // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
                using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
                var messageSendRecordModel = new MessageSendRecordModel
                {
                    Channel = MessageSendChannelEnum.Sms,
                    Receiver = mobile.Trim(),
                    Title = templateCode,
                    RecordValue = templateParam?.ToJsonString(),
                    IsSuccess = isSuccess,
                    CreatedTime = sendTime
                };
                messageSendRecordModel.RecordCreate(FastContext.HttpContext);

                await db.Insertable(messageSendRecordModel).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                // 数据库异常可能包含SQL参数；只保留异常类型，且不能因此重发已发送的消息。
                _logger.LogError(ex, $"短信发送记录写入失败。Mobile：{mobile}");
            }
        }
    }
}
