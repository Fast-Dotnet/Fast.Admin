// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Login.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fast.Center.Service.Login;

public partial class LoginService
{
    /// <summary>
    /// 尝试登录
    /// </summary>
    [HttpPost("/tryLogin")]
    [ApiInfo("尝试登录", HttpRequestActionEnum.Auth)]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginOutput> TryLogin(TryLoginInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        TenantUserModel tenantUserModel = await _repository.Queryable<TenantUserModel>()
            .InnerJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
            .ClearFilter<IBaseTEntity>()
            .Where(t1 => t1.UserKey == input.UserKey)
            .Where((t1, t2) => t2.Status == CommonStatusEnum.Enable)
            .SingleAsync();

        if (tenantUserModel == null)
        {
            return new LoginOutput {Status = LoginStatusEnum.NotAccount, Message = "未找到用户信息，请先授权登录！"};
        }

        if (tenantUserModel.AccountId != _user.AccountId)
        {
            throw new UserFriendlyException("禁止切换到其他账号的租户！");
        }

        AccountModel accountModel = await _repository.Queryable<AccountModel>()
            .Where(wh => wh.AccountId == tenantUserModel.AccountId)
            .SingleAsync();

        if (accountModel == null)
        {
            throw new UserFriendlyException("账号不存在！");
        }

        // 先撤销当前租户会话，再签发目标租户会话
        await _user.Logout();

        // 处理登录
        return await HandleLogin(applicationModel.Application, accountModel, tenantUserModel, DateTime.Now);
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost("/logout")]
    [ApiInfo("退出登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task Logout()
    {
        await _user.Logout();
    }
}
