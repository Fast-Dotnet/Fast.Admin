// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using Fast.Center.Domain;
using Fast.Center.Service.WeChat.Dto;
using Fast.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SKIT.FlurlHttpClient.Wechat.Api.Utilities;

namespace Fast.Center.Service.WeChat;

/// <summary>
/// 微信服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Auth, Name = "weChat")]
public class WeChatService : IDynamicApplication
{
    /// <summary>
    /// 换取微信用户身份信息
    /// </summary>
    [HttpPost]
    [ApiInfo("换取微信用户身份信息", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    public async Task<WeChatCode2SessionOutput> WeChatCode2Session(WeChatCode2SessionInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.OpenId, AppSecret = applicationModel.OpenSecret})
            .Build();

        // 解析微信Code，获取OpenId
        SnsJsCode2SessionResponse response =
            await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.Code});
        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

        var result = new WeChatCode2SessionOutput
        {
            OpenId = response.OpenId,
            UnionId = response.UnionId,
            SessionKey = response.SessionKey,
            NickName = "微信用户",
            Avatar = CommonConst.DefaultAvatar,
            Sex = GenderEnum.Unknown
        };


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

            result.NickName = decryptData.NickName;
            result.Sex = decryptData.Gender;
            result.Country = decryptData.Country;
            result.Province = decryptData.Province;
            result.City = decryptData.City;
            result.Language = decryptData.Language;
        }

        return result;
    }

    /// <summary>
    /// 换取微信用户手机号
    /// </summary>
    [HttpPost]
    [ApiInfo("换取微信用户手机号", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [EnableRateLimiting(CommonConst.LoginApiRateLimit)]
    public async Task<WeChatCode2PhoneNumberOutput> WeChatCode2PhoneNumber(WeChatCode2PhoneNumberInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.OpenId, AppSecret = applicationModel.OpenSecret})
            .Build();

        // 换取用户手机号
        WxaBusinessGetUserPhoneNumberResponse response = await apiClient.ExecuteWxaBusinessGetUserPhoneNumberAsync(
            new WxaBusinessGetUserPhoneNumberRequest {AccessToken = applicationModel.WeChatAccessToken, Code = input.Code});

        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取用户手机号失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

        return new WeChatCode2PhoneNumberOutput
        {
            PurePhoneNumber = response.PhoneInfo.PurePhoneNumber,
            PhoneNumber = response.PhoneInfo.PhoneNumber,
            CountryCode = response.PhoneInfo.CountryCode
        };
    }
}
