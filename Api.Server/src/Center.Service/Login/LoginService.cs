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
using Fast.Center.Service.Login.Dto;
using Fast.CenterLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Login;

/// <summary>
/// 登录服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Auth, Name = "login")]
public partial class LoginService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ICache _cache;
    private readonly ICaptchaService _captchaService;
    private readonly HttpContext _httpContext;
    private readonly ISqlSugarClient _repository;

    public LoginService(IUser user, IHttpContextAccessor httpContextAccessor, ICache cache, ICaptchaService captchaService,
        ISqlSugarClient repository)
    {
        _user = user;
        _httpContext = httpContextAccessor.HttpContext;
        _cache = cache;
        _captchaService = captchaService;
        _repository = repository;
    }

    /// <summary>
    /// 判断登录图片验证码是否启用
    /// </summary>
    private async Task<bool> IsLoginCaptchaEnabled()
    {
        return bool.Parse(await ConfigContext.GetConfig(ConfigConst.LoginCaptchaOpen));
    }

    /// <summary>
    /// 生成HashCode
    /// </summary>
    private string GenerateHashCode(string code)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code ?? string.Empty)));
    }

    /// <summary>
    /// 租户登录凭证缓存Dto
    /// </summary>
    private class TenantLoginTicketCacheDto
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
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 设备标识
        /// </summary>
        public string ClientIdentity { get; set; }

        /// <summary>
        /// 密码Hash
        /// </summary>
        public string PasswordHash { get; set; }
    }

    /// <summary>
    /// 生成临时租户登录凭证
    /// </summary>
    private async Task<string> GetTenantLoginTicket(AccountModel accountModel)
    {
        string loginTicket = Guid.NewGuid().ToString("N");
        string cacheKey = CacheConst.GetCacheKey(CacheConst.TenantLoginTicket, loginTicket);
        await _cache.SetAsync(cacheKey,
            new TenantLoginTicketCacheDto
            {
                AccountId = accountModel.AccountId,
                Mobile = accountModel.Mobile,
                Email = accountModel.Email,
                ClientIdentity = GlobalContext.ClientIdentity,
                PasswordHash = GenerateHashCode(accountModel.Password)
            }, TimeSpan.FromMinutes(5));

        return loginTicket;
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
    /// 验证并一次性消费租户登录凭证
    /// </summary>
    private async Task VerifyTenantLoginTicket(string loginTicket, AccountModel account)
    {
        if (!Guid.TryParseExact(loginTicket, "N", out _))
        {
            throw new UserFriendlyException("登录凭据已失效，请返回重新登录！");
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.TenantLoginTicket, loginTicket);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 30);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        TenantLoginTicketCacheDto cacheDto = await _cache.GetAsync<TenantLoginTicketCacheDto>(cacheKey);
        if (cacheDto == null
            || cacheDto.AccountId != account.AccountId
            || cacheDto.Mobile != account.Mobile
            || cacheDto.Email != account.Email
            || cacheDto.ClientIdentity != GlobalContext.ClientIdentity
            || cacheDto.PasswordHash != GenerateHashCode(account.Password))
        {
            throw new UserFriendlyException("登录凭据已失效，请返回重新登录！");
        }

        await _cache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="accountModel">账号信息</param>
    /// <param name="password">待验证的原始密码</param>
    /// <param name="dateTime">操作时间</param>
    private async Task VerifyPassword(AccountModel accountModel, string password, DateTime dateTime)
    {
        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("账号已被平台禁用！");
        }

        if (string.IsNullOrWhiteSpace(accountModel.Password))
        {
            throw new UserFriendlyException("未设定密码，请重置密码后重试！");
        }

        if (accountModel.LockEndTime != null && accountModel.LockEndTime > dateTime)
        {
            TimeSpan unLockTimeSpan = accountModel.LockEndTime.Value - dateTime;
            throw new UserFriendlyException($"账号已被锁定，请 {unLockTimeSpan.ToDescription()} 后再重试！");
        }

        /*
         * 连续错误3次，锁定1分钟
         * 连续错误5次，锁定5分钟
         * 连续错误10次，锁定账号
         * 登录成功后清除锁定信息
         */
        if (!CryptoUtil.VerifyPasswordPBKDF2SHA256(password, accountModel.Password))
        {
            accountModel.PasswordErrorTime ??= 0;
            // 错误次数+1
            accountModel.PasswordErrorTime++;

            switch (accountModel.PasswordErrorTime)
            {
                // 错误3次，锁定1分钟
                case 3:
                    accountModel.LockStartTime ??= dateTime;
                    accountModel.LockEndTime = accountModel.LockStartTime.Value.AddMinutes(1);
                    break;
                // 错误5次，锁定5分钟
                case 5:
                    accountModel.LockStartTime ??= dateTime;
                    accountModel.LockEndTime = dateTime.AddMinutes(5);
                    break;
                // 判断是否连续错误10次以上
                case >= 10:
                    // 错误10次，直接禁用账号
                    accountModel.Status = CommonStatusEnum.Disable;
                    break;
            }

            // 采用条件更新，避免并发问题
            await _repository.Updateable(accountModel)
                .UpdateColumns(e => new {e.PasswordErrorTime, e.LockStartTime, e.LockEndTime, e.Status})
                .ExecuteCommandAsync();
            if (accountModel.Status == CommonStatusEnum.Disable)
            {
                await _user.RevokeAccount(accountModel.AccountId);
                throw new UserFriendlyException("密码连续输入错误10次，账号已被禁用，请联系管理员！");
            }

            throw new UserFriendlyException("密码不正确！");
        }

        // 清除锁定信息
        if (accountModel.PasswordErrorTime != null)
        {
            accountModel.PasswordErrorTime = null;
            accountModel.LockStartTime = null;
            accountModel.LockEndTime = null;
            // 采用条件更新，避免并发问题
            await _repository.Updateable(accountModel)
                .UpdateColumns(e => new {e.PasswordErrorTime, e.LockStartTime, e.LockEndTime})
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 处理登录
    /// </summary>
    /// <returns>登录结果</returns>
    private async Task<LoginOutput> HandleLogin(ApplicationModel applicationModel, AccountModel accountModel,
        TenantUserModel tenantUserModel, DateTime dateTime)
    {
        // 验证账号状态
        if (accountModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("账号已被平台禁用！");
        }

        if (tenantUserModel == null)
        {
            throw new UserFriendlyException("用户不存在！");
        }

        // 验证租户用户状态
        if (tenantUserModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("用户已被禁用！");
        }

        // 验证是否为机器人
        if (tenantUserModel.UserType == UserTypeEnum.Robot)
        {
            throw new UserFriendlyException("无效用户！");
        }

        // 查询租户
        TenantModel tenantModel = await _repository.Queryable<TenantModel>()
            .Where(wh => wh.TenantId == tenantUserModel.TenantId)
            .SingleAsync();

        if (tenantModel == null)
        {
            throw new UserFriendlyException("租户不存在！");
        }

        if (tenantModel.Status == CommonStatusEnum.Disable)
        {
            throw new UserFriendlyException("租户已被禁用！");
        }

        // 验证版本
        if (tenantModel.Edition < applicationModel.Edition)
        {
            throw new UserFriendlyException(
                $"当前租户版本【{tenantModel.Edition.GetDescription()}】不支持访问该应用，请升级至【{applicationModel.Edition.GetDescription()}】或更高版本。");
        }

        // 获取设备信息
        UserAgentInfo userAgentInfo = _httpContext.RequestUserAgentInfo();
        // 获取万网信息
        WanNetIPInfo wanNetIpInfo = await _httpContext.RemoteIpv4InfoAsync();

        if (accountModel.FirstLoginTime == null)
        {
            accountModel.FirstLoginTenantId = tenantModel.TenantId;
            accountModel.FirstLoginDevice = userAgentInfo.Device;
            accountModel.FirstLoginOS = userAgentInfo.OS;
            accountModel.FirstLoginBrowser = userAgentInfo.Browser;
            accountModel.FirstLoginProvince = wanNetIpInfo.Province;
            accountModel.FirstLoginCity = wanNetIpInfo.City;
            accountModel.FirstLoginIp = wanNetIpInfo.Ip;
            accountModel.FirstLoginTime = dateTime;
        }

        accountModel.LastLoginTenantId = tenantModel.TenantId;
        accountModel.LastLoginDevice = userAgentInfo.Device;
        accountModel.LastLoginOS = userAgentInfo.OS;
        accountModel.LastLoginBrowser = userAgentInfo.Browser;
        accountModel.LastLoginProvince = wanNetIpInfo.Province;
        accountModel.LastLoginCity = wanNetIpInfo.City;
        accountModel.LastLoginIp = wanNetIpInfo.Ip;
        accountModel.LastLoginTime = dateTime;
        // 登录不更新错误密码信息，并且启用版本标识
        await _repository.Updateable(accountModel)
            .IgnoreColumns(it => new {it.PasswordErrorTime, it.LockStartTime, it.LockEndTime, it.Status})
            .ExecuteCommandWithOptLockAsync(true);

        // 登录后身份验证开关
        bool loginIdentityVerificationOpen = bool.Parse(await ConfigContext.GetConfig(ConfigConst.LoginIdentityVerificationOpen));

        // 登录
        await _user.Login(new AuthUserInfo
        {
            DeviceType = GlobalContext.DeviceType,
            DeviceId = GlobalContext.DeviceId,
            AppNo = applicationModel.AppNo,
            AppName = applicationModel.AppName,
            AccountId = accountModel.AccountId,
            AccountKey = accountModel.AccountKey,
            Mobile = accountModel.Mobile,
            NickName = accountModel.NickName,
            Avatar = accountModel.Avatar,
            // 开启验证并且未验证
            IdentityVerification = loginIdentityVerificationOpen && !accountModel.IdentityVerification,
            TenantId = tenantModel.TenantId,
            TenantNo = tenantModel.TenantNo,
            TenantName = tenantModel.TenantName,
            TenantCode = tenantModel.TenantCode,
            IsSystemTenant = tenantModel.TenantType == TenantTypeEnum.System,
            UserKey = tenantUserModel.UserKey,
            EmployeeId = tenantUserModel.EmployeeId,
            EmployeeNo = tenantUserModel.EmployeeNo,
            EmployeeName = tenantUserModel.EmployeeName,
            DepartmentId = tenantUserModel.DepartmentId,
            DepartmentName = tenantUserModel.DepartmentName,
            IsSuperAdmin = tenantUserModel.UserType == UserTypeEnum.SuperAdmin,
            IsAdmin = tenantUserModel.UserType == UserTypeEnum.Admin,
            LastLoginDevice = accountModel.LastLoginDevice,
            LastLoginOS = accountModel.LastLoginOS,
            LastLoginBrowser = accountModel.LastLoginBrowser,
            LastLoginProvince = accountModel.LastLoginProvince,
            LastLoginCity = accountModel.LastLoginCity,
            LastLoginIp = accountModel.LastLoginIp,
            LastLoginTime = accountModel.LastLoginTime.Value
        });

        // 添加访问日志
        var visitLogModel = new VisitLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            VisitType = VisitTypeEnum.Login,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _user.TenantId,
            TenantName = _user.TenantName
        };
        visitLogModel.RecordCreate(_httpContext);
        await _httpContext.RequestServices.GetService<ISqlSugarRepository<VisitLogModel>>().InsertAsync(visitLogModel);

        return new LoginOutput
        {
            Status = LoginStatusEnum.Success,
            Message = "登录成功",
            AccountKey = accountModel.AccountKey,
            NickName = accountModel.NickName,
            Avatar = accountModel.Avatar,
            TenantList =
            [
                new LoginTenantOutput
                {
                    UserKey = tenantUserModel.UserKey,
                    TenantName = tenantModel.TenantName,
                    ShortName = tenantModel.ShortName,
                    SpellName = tenantModel.SpellName,
                    Edition = tenantModel.Edition,
                    LogoUrl = tenantModel.LogoUrl,
                    EmployeeNo = tenantUserModel.EmployeeNo,
                    EmployeeName = tenantUserModel.EmployeeName,
                    IdPhoto = tenantUserModel.IdPhoto,
                    DepartmentId = tenantUserModel.DepartmentId,
                    DepartmentName = tenantUserModel.DepartmentName,
                    UserType = tenantUserModel.UserType,
                    Status = tenantUserModel.Status
                }
            ]
        };
    }

    /// <summary>
    /// 获取登录图片验证码
    /// </summary>
    [HttpPost("/getLoginCaptcha")]
    [ApiInfo("获取登录图片验证码", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginCaptchaOutput> GetLoginCaptcha(bool isForce = false)
    {
        // 查询应用信息
        await EnsureApplication();

        // 判断是否为强制启用
        if (!(isForce || await IsLoginCaptchaEnabled()))
        {
            return new LoginCaptchaOutput {Enabled = false};
        }

        (string captchaKey, string captchaImage) = await _captchaService.GetImageCaptcha();
        return new LoginCaptchaOutput {Enabled = true, CaptchaKey = captchaKey, CaptchaImage = captchaImage};
    }
}
