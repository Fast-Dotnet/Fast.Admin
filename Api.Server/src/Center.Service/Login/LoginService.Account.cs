// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text.RegularExpressions;
using Fast.Center.Domain;
using Fast.Center.Service.Login.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fast.Center.Service.Login;

public partial class LoginService
{
    /// <summary>
    /// 登录
    /// </summary>
    [HttpPost("/login")]
    [ApiInfo("登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginOutput> Login(LoginInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        // 目前只有 Web 端启用了图片验证码
        if (GlobalContext.IsWeb && await IsLoginCaptchaEnabled())
        {
            await _captchaService.VerifyImageCaptcha(input.CaptchaKey, input.CaptchaCode);
        }

        // 判断账号是否为手机号
        bool isMobile = new Regex(RegexConst.Mobile).IsMatch(input.Account);

        AccountModel accountModel = null;
        List<TenantUserModel> tenantUserList = [];

        if (isMobile)
        {
            // 根据手机号，查询账号
            accountModel = await _repository.Queryable<AccountModel>().Where(wh => wh.Mobile == input.Account).SingleAsync();

            if (accountModel != null)
            {
                tenantUserList = await _repository.Queryable<TenantUserModel>()
                    .InnerJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
                    .ClearFilter<IBaseTEntity>()
                    .Where(t1 => t1.AccountId == accountModel.AccountId)
                    .Where((t1, t2) => t2.Status == CommonStatusEnum.Enable)
                    .ToListAsync();
            }
        }
        else
        {
            // 根据账号或登录工号查询租户用户信息
            TenantUserModel tenantUserModel = await _repository.Queryable<TenantUserModel>()
                .InnerJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
                .ClearFilter<IBaseTEntity>()
                .Where(t1 => t1.EmployeeNo == input.Account)
                .Where((t1, t2) => t2.Status == CommonStatusEnum.Enable)
                .SingleAsync();

            if (tenantUserModel != null)
            {
                // 查询账号
                accountModel = await _repository.Queryable<AccountModel>()
                    .Where(wh => wh.AccountId == tenantUserModel.AccountId)
                    .SingleAsync();
                tenantUserList.Add(tenantUserModel);
            }
        }

        if (accountModel == null)
        {
            throw new UserFriendlyException("账号不存在！");
        }

        DateTime dateTime = DateTime.Now;

        // 验证密码
        await VerifyPassword(accountModel, input.Password, dateTime);

        if (tenantUserList.Count == 0)
        {
            throw new UserFriendlyException("账号未绑定任何租户！");
        }

        // 单租户自动登录
        bool autoLogin = bool.Parse(await ConfigContext.GetConfig(ConfigConst.SingleTenantWhenAutoLogin));

        // 单租户自动登录
        if (tenantUserList.Count == 1 && autoLogin)
        {
            // 处理登录
            return await HandleLogin(applicationModel.Application, accountModel, tenantUserList.Single(), dateTime);
        }

        var tenantIds = tenantUserList.Select(sl => sl.TenantId).Distinct().ToList();
        List<TenantModel> tenantList = await _repository
            .Queryable<TenantModel>()
            .Where(wh => tenantIds.Contains(wh.TenantId))
            .ToListAsync();

        var resultTenantList = new List<LoginTenantOutput>();

        foreach (TenantUserModel tenantUserModel in tenantUserList)
        {
            TenantModel tenantModel = tenantList.Single(s => s.TenantId == tenantUserModel.TenantId);
            resultTenantList.Add(new LoginTenantOutput
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
            });
        }

        // 多个账号，或未开启单租户自动登录
        return new LoginOutput
        {
            Status = LoginStatusEnum.SelectTenant,
            Message = "请选择租户登录",
            LoginTicket = await GetTenantLoginTicket(accountModel),
            AccountKey = accountModel.AccountKey,
            NickName = accountModel.NickName,
            Avatar = accountModel.Avatar,
            TenantList = resultTenantList
        };
    }

    /// <summary>
    /// 获取登录用户
    /// </summary>
    [HttpGet("/queryLoginUser")]
    [ApiInfo("获取登录用户", HttpRequestActionEnum.Query)]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<List<LoginTenantOutput>> QueryLoginUser()
    {
        return await _repository.Queryable<AccountModel>()
            .InnerJoin<TenantUserModel>((t1, t2) => t1.AccountId == t2.AccountId)
            .InnerJoin<TenantModel>((t1, t2, t3) => t2.TenantId == t3.TenantId)
            .ClearFilter<IBaseTEntity>()
            .Where(t1 => t1.AccountId == _user.AccountId)
            .Where((t1, t2, t3) => t3.Status == CommonStatusEnum.Enable)
            .Select((t1, t2, t3) => new LoginTenantOutput
            {
                UserKey = t2.UserKey,
                TenantName = t3.TenantName,
                ShortName = t3.ShortName,
                SpellName = t3.SpellName,
                Edition = t3.Edition,
                LogoUrl = t3.LogoUrl,
                EmployeeNo = t2.EmployeeNo,
                EmployeeName = t2.EmployeeName,
                IdPhoto = t2.IdPhoto,
                DepartmentId = t2.DepartmentId,
                DepartmentName = t2.DepartmentName,
                UserType = t2.UserType,
                Status = t2.Status
            })
            .ToListAsync();
    }

    /// <summary>
    /// 租户登录
    /// </summary>
    [HttpPost("/tenantLogin")]
    [ApiInfo("租户登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginOutput> TenantLogin(TenantLoginInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Password) && string.IsNullOrWhiteSpace(input.LoginTicket))
        {
            throw new UserFriendlyException("密码不能为空！");
        }

        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        // 目前只有 Web 端启用了图片验证码，这里登录凭证为空的情况下是存在租户直接登录的
        if (string.IsNullOrWhiteSpace(input.LoginTicket) && GlobalContext.IsWeb && await IsLoginCaptchaEnabled())
        {
            await _captchaService.VerifyImageCaptcha(input.CaptchaKey, input.CaptchaCode);
        }

        // 查询租户用户
        TenantUserModel tenantUserModel = await _repository.Queryable<TenantUserModel>()
            .InnerJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
            .ClearFilter<IBaseTEntity>()
            .Where(t1 => t1.UserKey == input.UserKey)
            .Where((t1, t2) => t2.Status == CommonStatusEnum.Enable)
            .SingleAsync();

        if (tenantUserModel == null)
        {
            throw new UserFriendlyException("用户不存在！");
        }

        // 查询账号
        AccountModel accountModel = await _repository.Queryable<AccountModel>()
            .Where(wh => wh.AccountId == tenantUserModel.AccountId)
            .SingleAsync();

        if (accountModel == null)
        {
            throw new UserFriendlyException("账号不存在！");
        }

        DateTime dateTime = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(input.LoginTicket))
        {
            // 验证登录凭证
            await VerifyTenantLoginTicket(input.LoginTicket, accountModel);
        }
        else
        {
            // 验证密码
            await VerifyPassword(accountModel, input.Password, dateTime);
        }

        // 处理登录
        return await HandleLogin(applicationModel.Application, accountModel, tenantUserModel, dateTime);
    }
}
