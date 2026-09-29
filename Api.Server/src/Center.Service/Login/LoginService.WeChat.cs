// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using Fast.Center.Domain;
using Fast.Center.Service.Login.Dto;
using Fast.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SKIT.FlurlHttpClient.Wechat.Api.Utilities;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Login;

public partial class LoginService
{
    /// <summary>
    /// 创建客户端用户
    /// </summary>
    private static ClientUserModel CreateClientUser(long appId, AppEnvironmentEnum appType, string openId, string unionId)
    {
        ClientUserTypeEnum userType = appType switch
        {
            AppEnvironmentEnum.Android => ClientUserTypeEnum.Mobile,
            AppEnvironmentEnum.IOS => ClientUserTypeEnum.Mobile | ClientUserTypeEnum.Apple,
            AppEnvironmentEnum.WeChatMiniProgram => ClientUserTypeEnum.Mobile | ClientUserTypeEnum.MiniProgram,
            AppEnvironmentEnum.WeChatOfficialAccount => ClientUserTypeEnum.OfficialAccount,
            AppEnvironmentEnum.WeChatServiceAccount => ClientUserTypeEnum.ServiceAccount,
            AppEnvironmentEnum.WeChatOpenPlatform => ClientUserTypeEnum.OpenPlatform,
            AppEnvironmentEnum.WorkWeChat => ClientUserTypeEnum.WorkWeChat,
            AppEnvironmentEnum.QuickApp => ClientUserTypeEnum.Mobile,
            _ => throw new UserFriendlyException("暂不支持此类微信客户端！")
        };

        return new ClientUserModel
        {
            UserId = YitIdHelper.NextId(),
            AppId = appId,
            UserType = userType,
            OpenId = openId,
            UnionId = unionId,
            NickName = "微信用户",
            Avatar = CommonConst.DefaultAvatar,
            Sex = GenderEnum.Unknown
        };
    }

    /// <summary>
    /// 更新手机号
    /// </summary>
    private async Task UpdateMobile(ClientUserModel clientUserModel, string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile) || clientUserModel.Mobile == mobile)
        {
            return;
        }

        clientUserModel.Mobile = mobile;
        clientUserModel.MobileUpdateTime = DateTime.Now;
        // 采用条件更新，避免并发问题
        await _repository.Updateable<ClientUserModel>()
            .SetColumns(e => new ClientUserModel {Mobile = mobile, MobileUpdateTime = DateTime.Now})
            .Where(wh => wh.UserId == clientUserModel.UserId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 处理微信登录
    /// </summary>
    /// <returns>微信登录结果</returns>
    private async Task<LoginOutput> HandleWeChatLogin(ApplicationModel applicationModel, ClientUserModel clientUserModel)
    {
        DateTime dateTime = DateTime.Now;

        // 判断客户端用户是否已绑定手机号
        if (string.IsNullOrWhiteSpace(clientUserModel.Mobile))
        {
            return new LoginOutput {Status = LoginStatusEnum.NotAccount, Message = "客户端用户未绑定手机号，请先授权手机号！"};
        }

        AccountModel accountModel = await _repository.Queryable<AccountModel>()
            .Where(wh => wh.Mobile == clientUserModel.Mobile)
            .SingleAsync();

        if (accountModel == null)
        {
            throw new UserFriendlyException("账号不存在！");
        }

        // 保证一个客户端用户只绑定一个账号，并持久化首次绑定关系
        if (accountModel.ClientUserId != clientUserModel.UserId)
        {
            await _repository.Ado.UseTranAsync(async () =>
            {
                await _repository.Updateable<AccountModel>()
                    .SetColumns(e => e.ClientUserId == null)
                    .Where(wh => wh.ClientUserId == clientUserModel.UserId)
                    .ExecuteCommandAsync();

                accountModel.ClientUserId = clientUserModel.UserId;
                await _repository.Updateable(accountModel).ExecuteCommandAsync();
            }, ex => throw ex);
        }

        List<TenantUserModel> tenantUserList = await _repository.Queryable<TenantUserModel>()
            .InnerJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
            .ClearFilter<IBaseTEntity>()
            .Where(t1 => t1.AccountId == accountModel.AccountId)
            .Where((t1, t2) => t2.Status == CommonStatusEnum.Enable)
            .ToListAsync();
        if (tenantUserList.Count == 0)
        {
            throw new UserFriendlyException("账号未绑定任何租户！");
        }

        // 获取设备信息
        UserAgentInfo userAgentInfo = _httpContext.RequestUserAgentInfo();
        // 获取万网信息
        WanNetIPInfo wanNetIpInfo = await _httpContext.RemoteIpv4InfoAsync();
        clientUserModel.LastLoginDevice = userAgentInfo.Device;
        clientUserModel.LastLoginOS = userAgentInfo.OS;
        clientUserModel.LastLoginBrowser = userAgentInfo.Browser;
        clientUserModel.LastLoginProvince = wanNetIpInfo.Province;
        clientUserModel.LastLoginCity = wanNetIpInfo.City;
        clientUserModel.LastLoginIp = wanNetIpInfo.Ip;
        clientUserModel.LastLoginTime = dateTime;
        // 更新客户端用户登录时间，这里代表客户端用户登录成功了，后续逻辑不包含客户端用户
        await _repository.Updateable(clientUserModel)
            .UpdateColumns(it => new
            {
                it.LastLoginDevice,
                it.LastLoginOS,
                it.LastLoginBrowser,
                it.LastLoginProvince,
                it.LastLoginCity,
                it.LastLoginIp,
                it.LastLoginTime
            })
            .ExecuteCommandAsync();

        // 单租户自动登录
        bool autoLogin = bool.Parse(await ConfigContext.GetConfig(ConfigConst.SingleTenantWhenAutoLogin));

        if (tenantUserList.Count == 1 && autoLogin)
        {
            // 处理登录
            return await HandleLogin(applicationModel, accountModel, tenantUserList.Single(), dateTime);
        }

        // 微信登录自动选择最后一次登录租户
        if (accountModel.LastLoginTenantId != null)
        {
            TenantUserModel tenantUserModel = tenantUserList.FirstOrDefault(f => f.TenantId == accountModel.LastLoginTenantId);
            if (tenantUserModel != null)
            {
                // 处理登录
                return await HandleLogin(applicationModel, accountModel, tenantUserModel, dateTime);
            }
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

        // 多个租户，或未开启单租户自动登录
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
    /// 微信登录
    /// </summary>
    [HttpPost("/weChatLogin")]
    [ApiInfo("微信登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginOutput> WeChatLogin(WeChatLoginInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        // 解析微信Code，获取OpenId
        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.OpenId, AppSecret = applicationModel.OpenSecret})
            .Build();
        SnsJsCode2SessionResponse response =
            await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.WeChatCode});
        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

        // 根据 OpenId 获取微信用户信息
        ClientUserModel clientUserModel = await _repository.Queryable<ClientUserModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.OpenId == response.OpenId)
            .SingleAsync();
        if (clientUserModel == null)
        {
            // 这里的 IV 和 EncryptedData 在没有授权的情况下是为空的
            if (string.IsNullOrWhiteSpace(input.IV) || string.IsNullOrWhiteSpace(input.EncryptedData))
            {
                return new LoginOutput {Status = LoginStatusEnum.AuthExpired, Message = "授权已过期，请重新授权登录！"};
            }

            // 尝试解析加密数据
            byte[] decryptBytes = AESUtility.DecryptWithCBC(Convert.FromBase64String(response.SessionKey),
                Convert.FromBase64String(input.IV), Convert.FromBase64String(input.EncryptedData));
            string decryptStr = Encoding.UTF8.GetString(decryptBytes);
            DecryptWeChatUserInfo decryptData = decryptStr.ToObject<DecryptWeChatUserInfo>();
            if (decryptData == null)
            {
                throw new UserFriendlyException("解析加密用户信息失败！");
            }

            // 保存微信用户
            clientUserModel =
                CreateClientUser(applicationModel.AppId, GlobalContext.DeviceType, response.OpenId, response.UnionId);
            clientUserModel.SessionKey = response.SessionKey;
            clientUserModel.NickName = decryptData.NickName;
            clientUserModel.Sex = decryptData.Gender;
            await _repository.Insertable(clientUserModel).ExecuteCommandAsync();
        }

        // 处理登录
        return await HandleWeChatLogin(applicationModel.Application, clientUserModel);
    }

    /// <summary>
    /// 微信授权登录
    /// </summary>
    [HttpPost("/weChatAuthLogin")]
    [ApiInfo("微信授权登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<LoginOutput> WeChatAuthLogin(WeChatAuthLoginInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        // 解析微信Code，获取OpenId
        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.OpenId, AppSecret = applicationModel.OpenSecret})
            .Build();
        SnsJsCode2SessionResponse response =
            await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.WeChatCode});
        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

        // 根据 OpenId 获取微信用户信息
        ClientUserModel clientUserModel = await _repository.Queryable<ClientUserModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.OpenId == response.OpenId)
            .SingleAsync();
        if (clientUserModel == null)
        {
            return new LoginOutput {Status = LoginStatusEnum.NotAccount, Message = "未找到微信用户信息，请先授权登录！"};
        }

        // 换取用户手机号
        WxaBusinessGetUserPhoneNumberResponse phoneNumberResponse = await apiClient.ExecuteWxaBusinessGetUserPhoneNumberAsync(
            new WxaBusinessGetUserPhoneNumberRequest {AccessToken = applicationModel.WeChatAccessToken, Code = input.Code});
        if (!phoneNumberResponse.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取用户手机号失败：ErrorCode：{phoneNumberResponse.ErrorCode}。ErrorMessage：{phoneNumberResponse.ErrorMessage}");
        }

        // 更新手机号
        await UpdateMobile(clientUserModel, phoneNumberResponse.PhoneInfo.PurePhoneNumber);

        // 处理登录
        return await HandleWeChatLogin(applicationModel.Application, clientUserModel);
    }

    /// <summary>
    /// 微信客户端登录
    /// </summary>
    [HttpPost("/weChatClientLogin")]
    [ApiInfo("微信客户端登录", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<WeChatClientLoginOutput> WeChatClientLogin(WeChatClientLoginInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await EnsureApplication();

        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.OpenId, AppSecret = applicationModel.OpenSecret})
            .Build();

        ClientUserModel clientUserModel = null;

        // 微信小程序
        if (applicationModel.AppType == AppEnvironmentEnum.WeChatMiniProgram)
        {
            // 解析微信Code，获取OpenId
            SnsJsCode2SessionResponse response =
                await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.WeChatCode});
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            // 根据 OpenId 获取微信用户信息
            clientUserModel = await _repository.Queryable<ClientUserModel>()
                .Where(wh => wh.AppId == applicationModel.AppId)
                .Where(wh => wh.OpenId == response.OpenId)
                .SingleAsync();
            if (clientUserModel == null)
            {
                // 保存微信用户
                clientUserModel = CreateClientUser(applicationModel.AppId, GlobalContext.DeviceType, response.OpenId,
                    response.UnionId);
                clientUserModel.SessionKey = response.SessionKey;

                // 这里的 IV 和 EncryptedData 在没有授权的情况下是为空的
                if (string.IsNullOrWhiteSpace(input.IV) != string.IsNullOrWhiteSpace(input.EncryptedData))
                {
                    throw new UserFriendlyException("IV 和加密用户数据必须同时提供！");
                }

                if (!string.IsNullOrWhiteSpace(input.IV) && !string.IsNullOrWhiteSpace(input.EncryptedData))
                {
                    // 尝试解析加密数据
                    byte[] decryptBytes = AESUtility.DecryptWithCBC(Convert.FromBase64String(response.SessionKey),
                        Convert.FromBase64String(input.IV), Convert.FromBase64String(input.EncryptedData));
                    string decryptStr = Encoding.UTF8.GetString(decryptBytes);
                    DecryptWeChatUserInfo decryptData = decryptStr.ToObject<DecryptWeChatUserInfo>();
                    if (decryptData == null)
                    {
                        throw new UserFriendlyException("解析加密用户信息失败！");
                    }

                    clientUserModel.NickName = decryptData.NickName;
                    clientUserModel.Sex = decryptData.Gender;
                }

                await _repository.Insertable(clientUserModel).ExecuteCommandAsync();
            }

            if (!string.IsNullOrWhiteSpace(input.Code))
            {
                // 换取用户手机号
                WxaBusinessGetUserPhoneNumberResponse phoneNumberResponse =
                    await apiClient.ExecuteWxaBusinessGetUserPhoneNumberAsync(
                        new WxaBusinessGetUserPhoneNumberRequest
                        {
                            AccessToken = applicationModel.WeChatAccessToken, Code = input.Code
                        });

                if (!phoneNumberResponse.IsSuccessful())
                {
                    throw new UserFriendlyException(
                        $"解析Code失败，获取用户手机号失败：ErrorCode：{phoneNumberResponse.ErrorCode}。ErrorMessage：{phoneNumberResponse.ErrorMessage}");
                }

                // 更新手机号
                await UpdateMobile(clientUserModel, phoneNumberResponse.PhoneInfo.PurePhoneNumber);
            }
        }
        // 微信服务号
        else if (applicationModel.AppType == AppEnvironmentEnum.WeChatServiceAccount)
        {
            // 根据 Code 换取用户 AccessToken
            SnsOAuth2AccessTokenResponse tokenResponse =
                await apiClient.ExecuteSnsOAuth2AccessTokenAsync(new SnsOAuth2AccessTokenRequest {Code = input.WeChatCode});
            if (!tokenResponse.IsSuccessful())
            {
                return new WeChatClientLoginOutput
                {
                    Status = LoginStatusEnum.AuthExpired,
                    Message =
                        $"解析Code失败，获取用户微信 AccessToken 失败：ErrorCode：{tokenResponse.ErrorCode}。ErrorMessage：{tokenResponse.ErrorMessage}"
                };
            }

            SnsUserInfoResponse response = await apiClient.ExecuteSnsUserInfoAsync(new SnsUserInfoRequest
            {
                AccessToken = tokenResponse.AccessToken, OpenId = tokenResponse.OpenId
            });
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"获取微信用户信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            // 根据 OpenId 获取微信用户信息
            clientUserModel = await _repository.Queryable<ClientUserModel>()
                .Where(wh => wh.AppId == applicationModel.AppId)
                .Where(wh => wh.OpenId == response.OpenId)
                .SingleAsync();
            if (clientUserModel == null)
            {
                // 保存微信用户
                clientUserModel = CreateClientUser(applicationModel.AppId, GlobalContext.DeviceType, response.OpenId,
                    response.UnionId);
                clientUserModel.NickName = response.Nickname;
                clientUserModel.Avatar = response.HeadImageUrl;
                await _repository.Insertable(clientUserModel).ExecuteCommandAsync();
            }
            else
            {
                clientUserModel.NickName = response.Nickname;
                clientUserModel.Avatar = response.HeadImageUrl;
            }
        }

        if (clientUserModel == null)
        {
            throw new UserFriendlyException("暂不支持此类客户端！");
        }

        // 获取设备信息
        UserAgentInfo userAgentInfo = _httpContext.RequestUserAgentInfo();
        // 获取万网信息
        WanNetIPInfo wanNetIpInfo = await _httpContext.RemoteIpv4InfoAsync();
        clientUserModel.LastLoginDevice = userAgentInfo.Device;
        clientUserModel.LastLoginOS = userAgentInfo.OS;
        clientUserModel.LastLoginBrowser = userAgentInfo.Browser;
        clientUserModel.LastLoginProvince = wanNetIpInfo.Province;
        clientUserModel.LastLoginCity = wanNetIpInfo.City;
        clientUserModel.LastLoginIp = wanNetIpInfo.Ip;
        clientUserModel.LastLoginTime = DateTime.Now;
        // 更新客户端用户登录时间
        await _repository.Updateable(clientUserModel)
            .UpdateColumns(it => new
            {
                it.LastLoginDevice,
                it.LastLoginOS,
                it.LastLoginBrowser,
                it.LastLoginProvince,
                it.LastLoginCity,
                it.LastLoginIp,
                it.LastLoginTime
            })
            .ExecuteCommandAsync();

        TenantModel tenantMode = null;
        if (applicationModel.Application.TenantId != null)
        {
            tenantMode = await _repository.Queryable<TenantModel>().InSingleAsync(applicationModel.Application.TenantId);
        }

        // 客户端登录
        await _user.ClientLogin(new AuthUserInfo
        {
            DeviceType = GlobalContext.DeviceType,
            DeviceId = GlobalContext.DeviceId,
            AppNo = applicationModel.Application.AppNo,
            AppName = applicationModel.Application.AppName,
            AccountId = clientUserModel.UserId,
            Mobile = clientUserModel.Mobile,
            NickName = clientUserModel.NickName,
            Avatar = clientUserModel.Avatar,
            TenantId = applicationModel.Application.TenantId ?? 0,
            TenantNo = tenantMode?.TenantNo ?? applicationModel.Application.AppNo,
            TenantName = applicationModel.Application.TenantName,
            TenantCode = tenantMode?.TenantCode ?? "",
            IsSystemTenant = false,
            EmployeeId = clientUserModel.UserId,
            EmployeeName = clientUserModel.NickName,
            ClientUserId = clientUserModel.UserId,
            ClientUserOpenId = clientUserModel.OpenId,
            IsSuperAdmin = false,
            IsAdmin = false,
            LastLoginDevice = clientUserModel.LastLoginDevice,
            LastLoginOS = clientUserModel.LastLoginOS,
            LastLoginBrowser = clientUserModel.LastLoginBrowser,
            LastLoginProvince = clientUserModel.LastLoginProvince,
            LastLoginCity = clientUserModel.LastLoginCity,
            LastLoginIp = clientUserModel.LastLoginIp,
            LastLoginTime = clientUserModel.LastLoginTime.Value,
            ButtonCodeList = [PermissionConst.ClientService]
        });

        return new WeChatClientLoginOutput
        {
            OpenId = clientUserModel.OpenId,
            UnionId = clientUserModel.UnionId,
            Mobile = clientUserModel.Mobile,
            NickName = clientUserModel.NickName,
            Avatar = clientUserModel.Avatar
        };
    }
}
