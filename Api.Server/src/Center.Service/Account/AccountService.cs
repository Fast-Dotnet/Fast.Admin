// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using CSRedis;
using Fast.Cache;
using Fast.Center.Domain;
using Fast.Center.Service.Account.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Fast.Center.Service.Account;

/// <summary>
/// 账号服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "account")]
public partial class AccountService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ICache _cache;
    private readonly ISqlSugarRepository<AccountModel> _repository;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ICaptchaService _captchaService;
    private readonly IMailService _mailService;
    private readonly ISmsService _smsService;

    public AccountService(IUser user, ICache cache, ISqlSugarRepository<AccountModel> repository,
        IHubContext<ChatHub, IChatClient> hubContext, ICaptchaService captchaService, IMailService mailService,
        ISmsService smsService)
    {
        _user = user;
        _cache = cache;
        _repository = repository;
        _hubContext = hubContext;
        _captchaService = captchaService;
        _mailService = mailService;
        _smsService = smsService;
    }

    /// <summary>
    /// 确保应用安全
    /// </summary>
    private async Task<ApplicationOpenIdModel> EnsureApplication()
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        return applicationModel;
    }

    /// <summary>
    /// 强制发送配额
    /// 共享Redis计数用于多实例发送配额，刷新设备Id或验证Key不能重置IP、账号和收件人配额
    /// </summary>
    private async Task EnforceSendQuota(string identity, params (int limit, int windowSeconds)[] quotas)
    {
        const string script = """
                              if tonumber(ARGV[3]) == 1 then
                                  local n = redis.call('INCR', KEYS[1])
                                  if n == 1 then
                                      redis.call('EXPIRE', KEYS[1], ARGV[1])
                                  end
                                  if n <= tonumber(ARGV[2]) then
                                      return 0
                                  end
                              elseif tonumber(redis.call('GET', KEYS[1]) or '0') < tonumber(ARGV[2]) then
                                  return 0
                              end
                              return math.ceil(redis.call('PTTL', KEYS[1]) / 1000)
                              """;
        string identityHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
        int retryAfterSeconds = 0;
        foreach ((int limit, int windowSeconds) in quotas)
        {
            // 已被前一个窗口拒绝时，后续窗口只检查等待时间，不再扣除配额。
            object result = await _cache.Client.EvalAsync(script, $"Login:SendQuota:{identityHash}:{windowSeconds}",
                windowSeconds, limit, retryAfterSeconds == 0 ? 1 : 0);
            retryAfterSeconds = Math.Max(retryAfterSeconds, Convert.ToInt32(result));
        }

        // 同时触发小时和全天配额时，提示较长的剩余等待时间。
        if (retryAfterSeconds > 0)
        {
            throw new UserFriendlyException($"操作过于频繁，请在 {TimeSpan.FromSeconds(retryAfterSeconds).ToDescription()} 后重试！");
        }
    }

    /// <summary>
    /// 账号选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("账号选择器", HttpRequestActionEnum.Query)]
    public async Task<PagedResult<ElSelectorOutput<long>>> AccountSelector(PagedInput input)
    {
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);
        if (tenantModel.TenantType == TenantTypeEnum.System)
        {
            PagedResult<AccountModel> data = await _repository.Entities
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue),
                    wh => wh.Mobile.Contains(input.SearchValue)
                          || wh.Email.Contains(input.SearchValue)
                          || wh.NickName.Contains(input.SearchValue))
                .OrderBy(ob => ob.Mobile)
                .Select(sl => new AccountModel
                {
                    AccountId = sl.AccountId,
                    Mobile = sl.Mobile,
                    Email = sl.Email,
                    AccountKey = sl.AccountKey,
                    NickName = sl.NickName,
                    Avatar = sl.Avatar
                })
                .OrderBy(ob => ob.NickName)
                .ToPagedListAsync(input);

            return data.ToPagedData(sl => new ElSelectorOutput<long>
            {
                Value = sl.AccountId, Label = sl.Mobile, Data = new {sl.Email, sl.AccountKey, sl.NickName, sl.Avatar}
            });
        }
        else
        {
            PagedResult<AccountModel> data = await _repository.Queryable<TenantUserModel>()
                .InnerJoin<AccountModel>((t1, t2) => t1.AccountId == t2.AccountId)
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue),
                    (t1, t2) => t2.Mobile.Contains(input.SearchValue)
                                || t2.Email.Contains(input.SearchValue)
                                || t2.NickName.Contains(input.SearchValue))
                .OrderBy((t1, t2) => t2.NickName)
                .Select((t1, t2) => new AccountModel
                {
                    AccountId = t2.AccountId,
                    Mobile = t2.Mobile,
                    Email = t2.Email,
                    AccountKey = t2.AccountKey,
                    NickName = t2.NickName,
                    Avatar = t2.Avatar
                })
                .Distinct()
                .ToPagedListAsync(input);

            return data.ToPagedData(sl => new ElSelectorOutput<long>
            {
                Value = sl.AccountId, Label = sl.Mobile, Data = new {sl.Email, sl.AccountKey, sl.NickName, sl.Avatar}
            });
        }
    }

    /// <summary>
    /// 获取账号分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取账号分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Account.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<QueryAccountPagedOutput>> QueryAccountPaged(QueryAccountPagedInput input)
    {
        DateTime dateTime = DateTime.Now;

        ISugarQueryable<AccountModel, TenantModel, TenantModel> queryable = _repository.Queryable<AccountModel>()
            .LeftJoin<TenantModel>((t1, t2) => t1.FirstLoginTenantId == t2.TenantId)
            .LeftJoin<TenantModel>((t1, t2, t3) => t1.LastLoginTenantId == t3.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Mobile), t1 => t1.Mobile.Contains(input.Mobile))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Email), t1 => t1.Email.Contains(input.Email))
            .WhereIF(input.Status != null, t1 => t1.Status == input.Status)
            .WhereIF(!string.IsNullOrWhiteSpace(input.FirstLoginCity), t1 => t1.FirstLoginCity.Contains(input.FirstLoginCity))
            .WhereIF(!string.IsNullOrWhiteSpace(input.FirstLoginIp), t1 => t1.FirstLoginIp.Contains(input.FirstLoginIp))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LastLoginCity), t1 => t1.LastLoginCity.Contains(input.LastLoginCity))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LastLoginIp), t1 => t1.LastLoginIp.Contains(input.LastLoginIp))
            .WhereIF(input.IsLock == true, t1 => t1.LockEndTime != null && t1.LockEndTime >= dateTime)
            .WhereIF(input.IsLock == false, t1 => t1.LockEndTime == null || t1.LockEndTime < dateTime);

        return await queryable.SelectMergeTable((t1, t2, t3) => new QueryAccountPagedOutput
            {
                AccountId = t1.AccountId,
                Mobile = t1.Mobile,
                Email = t1.Email,
                IdentityVerification = t1.IdentityVerification,
                Status = t1.Status,
                NickName = t1.NickName,
                Avatar = t1.Avatar,
                FirstLoginTenantName = t2.TenantName,
                FirstLoginDevice = t1.FirstLoginDevice,
                FirstLoginOS = t1.FirstLoginOS,
                FirstLoginBrowser = t1.FirstLoginBrowser,
                FirstLoginProvince = t1.FirstLoginProvince,
                FirstLoginCity = t1.FirstLoginCity,
                FirstLoginIp = t1.FirstLoginIp,
                FirstLoginTime = t1.FirstLoginTime,
                LastLoginTenantName = t3.TenantName,
                LastLoginDevice = t1.LastLoginDevice,
                LastLoginOS = t1.LastLoginOS,
                LastLoginBrowser = t1.LastLoginBrowser,
                LastLoginProvince = t1.LastLoginProvince,
                LastLoginCity = t1.LastLoginCity,
                LastLoginIp = t1.LastLoginIp,
                LastLoginTime = t1.LastLoginTime,
                PasswordErrorTime = t1.PasswordErrorTime,
                LockStartTime = t1.LockStartTime,
                LockEndTime = t1.LockEndTime,
                IsLock = SqlFunc.IF(t1.LockEndTime != null && t1.LockEndTime >= dateTime).Return(true).End(false),
                CreatedTime = t1.CreatedTime,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取账号详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取账号详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Account.Detail)]
    [PlatformOnly]
    public async Task<QueryAccountDetailOutput> QueryAccountDetail([Required(ErrorMessage = "账号Id不能为空")] long? accountId)
    {
        QueryAccountDetailOutput result = await _repository.Queryable<AccountModel>()
            .LeftJoin<TenantModel>((t1, t2) => t1.FirstLoginTenantId == t2.TenantId)
            .LeftJoin<TenantModel>((t1, t2, t3) => t1.LastLoginTenantId == t3.TenantId)
            .Where(t1 => t1.AccountId == accountId)
            .Select((t1, t2, t3) => new QueryAccountDetailOutput
            {
                AccountId = t1.AccountId,
                Mobile = t1.Mobile,
                Email = t1.Email,
                ClientUserId = t1.ClientUserId,
                Status = t1.Status,
                NickName = t1.NickName,
                Avatar = t1.Avatar,
                FirstLoginTenantName = t2.TenantName,
                FirstLoginDevice = t1.FirstLoginDevice,
                FirstLoginOS = t1.FirstLoginOS,
                FirstLoginBrowser = t1.FirstLoginBrowser,
                FirstLoginProvince = t1.FirstLoginProvince,
                FirstLoginCity = t1.FirstLoginCity,
                FirstLoginIp = t1.FirstLoginIp,
                FirstLoginTime = t1.FirstLoginTime,
                LastLoginTenantName = t3.TenantName,
                LastLoginDevice = t1.LastLoginDevice,
                LastLoginOS = t1.LastLoginOS,
                LastLoginBrowser = t1.LastLoginBrowser,
                LastLoginProvince = t1.LastLoginProvince,
                LastLoginCity = t1.LastLoginCity,
                LastLoginIp = t1.LastLoginIp,
                LastLoginTime = t1.LastLoginTime,
                PasswordErrorTime = t1.PasswordErrorTime,
                LockStartTime = t1.LockStartTime,
                LockEndTime = t1.LockEndTime,
                CreatedTime = t1.CreatedTime,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 获取编辑账号详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取编辑账号详情", HttpRequestActionEnum.Query)]
    public async Task<EditAccountInput> QueryEditAccountDetail()
    {
        EditAccountInput result = await _repository.Queryable<AccountModel>()
            .Where(wh => wh.AccountId == _user.AccountId)
            .Select(sl => new EditAccountInput
            {
                Mobile = sl.Mobile,
                Email = sl.Email,
                NickName = sl.NickName,
                Avatar = sl.Avatar,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 编辑账号
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑账号", HttpRequestActionEnum.Edit)]
    public async Task EditAccount(EditAccountInput input)
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

        if (await _repository.AnyAsync(a => a.Mobile == mobile && a.AccountId != _user.AccountId))
        {
            throw new UserFriendlyException("手机号已存在账号信息！");
        }

        if (await _repository.AnyAsync(a => a.Email == email && a.AccountId != _user.AccountId))
        {
            throw new UserFriendlyException("邮箱已存在账号信息！");
        }

        bool mobileChange = accountModel.Mobile != mobile;
        bool emailChange = !string.Equals(accountModel.Email, email, StringComparison.OrdinalIgnoreCase);
        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.EditAccountVerification, accountModel.AccountKey,
            GlobalContext.ClientIdentity);

        // 只有手机号或邮箱发生变化的时候才判断
        if (mobileChange || emailChange)
        {
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

            if (!string.Equals(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(accountModel.Password))),
                    dto.PasswordHash))
            {
                throw new UserFriendlyException("验证码无效或已过期！");
            }

            if (mobileChange)
            {
                if (string.IsNullOrWhiteSpace(input.MobileVerificationCode))
                {
                    throw new UserFriendlyException("请输入短信验证码！");
                }

                if (dto.Mobile != mobile || dto.MobileExpiresTime <= DateTime.Now)
                {
                    throw new UserFriendlyException("短信验证码无效或已过期！");
                }

                if (!dto.MobileVerified)
                {
                    await _smsService.VerifyVerificationCode(SmsTypeEnum.Validity, mobile, input.MobileVerificationCode);
                    dto.Mobile = mobile;
                    dto.MobileVerified = true;
                    await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
                }
            }

            if (emailChange)
            {
                if (string.IsNullOrWhiteSpace(input.EmailVerificationCode))
                {
                    throw new UserFriendlyException("请输入邮件验证码！");
                }

                if (dto.Email != email || dto.EmailExpiresTime <= DateTime.Now)
                {
                    throw new UserFriendlyException("邮件验证码无效或已过期！");
                }

                if (!dto.EmailVerified)
                {
                    await _mailService.VerifyVerificationCode(MailTypeEnum.Validity, email, input.MobileVerificationCode);
                    dto.Email = mobile;
                    dto.EmailVerified = true;
                    await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
                }
            }
        }

        accountModel.Mobile = mobile;
        accountModel.Email = email;
        accountModel.NickName = input.NickName;
        accountModel.Avatar = input.Avatar;
        accountModel.RowVersion = input.RowVersion;

        // 同步客户端用户信息
        ClientUserModel clientUserModel = null;
        if (accountModel.ClientUserId != null)
        {
            clientUserModel = await _repository.Queryable<ClientUserModel>().InSingleAsync(accountModel.ClientUserId);

            if (clientUserModel == null)
            {
                // 自动解绑
                accountModel.ClientUserId = null;
            }
            else
            {
                clientUserModel.NickName = input.NickName;
                clientUserModel.Avatar = input.Avatar;
            }
        }

        await _repository.Ado.UseTranAsync(async () =>
        {
            if (clientUserModel != null)
            {
                await _repository.Updateable(clientUserModel).ExecuteCommandAsync();
            }

            await _repository.UpdateAsync(accountModel);
        }, ex => throw ex);

        await _cache.DelAsync(cacheKey);

        if (mobileChange || emailChange)
        {
            await _user.Logout();
            await _user.RevokeAccount(accountModel.AccountId);
            await AccountForceOffline(accountModel.AccountId, "账号已修改，请重新登录");
        }
        else
        {
            // 刷新缓存
            await _user.RefreshAccount(new RefreshAccountDto
            {
                DeviceType = _user.DeviceType,
                AppNo = _user.AppNo,
                Mobile = accountModel.Mobile,
                NickName = input.NickName,
                Avatar = input.Avatar,
                TenantNo = _user.TenantNo,
                EmployeeNo = _user.EmployeeNo
            });
        }
    }

    /// <summary>
    /// 强制下线账号
    /// </summary>
    private async Task AccountForceOffline(long accountId, string message)
    {
        List<string> connectionIds = await _repository.Queryable<TenantOnlineUserModel>()
            .ClearFilter<IBaseTEntity>()
            .Where(wh => wh.IsOnline)
            .Where(wh => wh.AccountId == accountId)
            .Select(sl => sl.ConnectionId)
            .ToListAsync();
        if (connectionIds.Count == 0)
        {
            return;
        }

        // 强制下线当前账号所有在线用户
        await _hubContext.Clients.Clients(connectionIds)
            .ForceOffline(new ForceOfflineOutput
            {
                IsAdmin = _user.IsSuperAdmin || _user.IsAdmin,
                NickName = _user.NickName,
                EmployeeNo = _user.EmployeeNo,
                OfflineTime = DateTime.Now,
                Message = message
            });
    }
}
