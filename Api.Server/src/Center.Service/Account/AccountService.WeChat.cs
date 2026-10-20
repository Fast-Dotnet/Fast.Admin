// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text;
using Fast.Center.Domain;
using Fast.Center.Service.Account.Dto;
using Fast.Serialization;
using Microsoft.AspNetCore.Mvc;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SKIT.FlurlHttpClient.Wechat.Api.Utilities;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Account;

public partial class AccountService
{
    /// <summary>
    /// 绑定微信用户
    /// </summary>
    [HttpPost]
    [ApiInfo("绑定微信用户", HttpRequestActionEnum.Auth)]
    public async Task BindWeChat(BindWeChatInput input)
    {
        ApplicationClientModel applicationModel = await EnsureApplication();
        if (applicationModel.AppType != AppEnvironmentEnum.WeChatMiniProgram)
        {
            throw new UserFriendlyException("当前应用不支持此方法！");
        }

        // 解析微信Code，获取OpenId
        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.AppKey, AppSecret = applicationModel.AppSecret})
            .Build();
        SnsJsCode2SessionResponse response =
            await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.WeChatCode});
        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

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

        // 添加客户端用户
        bool addClientUser = false;
        ClientUserModel clientUserModel = await _repository.Queryable<ClientUserModel>()
            .Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.OpenId == response.OpenId)
            .SingleAsync();
        if (clientUserModel == null)
        {
            addClientUser = true;
            clientUserModel = new ClientUserModel
            {
                UserId = YitIdHelper.NextId(),
                AppId = applicationModel.AppId,
                UserType = ClientUserTypeEnum.Mobile | ClientUserTypeEnum.MiniProgram,
                OpenId = response.OpenId,
                UnionId = response.UnionId,
                SessionKey = response.SessionKey,
                NickName = "微信用户",
                Avatar = CommonConst.DefaultAvatar,
                Sex = GenderEnum.Unknown
            };

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

        if (accountModel.ClientUserId != null && accountModel.ClientUserId != clientUserModel.UserId)
        {
            throw new UserFriendlyException("当前账号已绑定其他微信，请先解除绑定！");
        }

        if (await _repository.Queryable<AccountModel>()
                .AnyAsync(a => a.ClientUserId == clientUserModel.UserId && a.AccountId != accountModel.AccountId))
        {
            throw new UserFriendlyException("该微信已绑定其他账号！");
        }

        await _repository.Ado.UseTranAsync(async () =>
        {
            if (addClientUser)
            {
                await _repository.Insertable(clientUserModel).ExecuteCommandAsync();
            }

            // 重复绑定同一微信时直接成功，不重复更新账号版本
            if (accountModel.ClientUserId != clientUserModel.UserId)
            {
                accountModel.ClientUserId = clientUserModel.UserId;
                accountModel.RowVersion = input.RowVersion;

                await _repository.Updateable(accountModel)
                    .UpdateColumns(e => e.ClientUserId)
                    .ExecuteCommandWithOptLockAsync(true);
            }
        }, ex => throw ex);
    }

    /// <summary>
    /// 解绑微信用户
    /// </summary>
    [HttpPost]
    [ApiInfo("解绑微信用户", HttpRequestActionEnum.Auth)]
    public async Task UnBindWeChat(BindWeChatInput input)
    {
        ApplicationClientModel applicationModel = await EnsureApplication();
        if (applicationModel.AppType != AppEnvironmentEnum.WeChatMiniProgram)
        {
            throw new UserFriendlyException("当前应用不支持此方法！");
        }

        // 解析微信Code，获取OpenId
        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = applicationModel.AppKey, AppSecret = applicationModel.AppSecret})
            .Build();
        SnsJsCode2SessionResponse response =
            await apiClient.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest {JsCode = input.WeChatCode});
        if (!response.IsSuccessful())
        {
            throw new UserFriendlyException(
                $"解析Code失败，获取微信登录信息失败：ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
        }

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

        if (accountModel.ClientUserId == null)
        {
            throw new UserFriendlyException("当前账号未绑定微信，请先绑定！");
        }

        accountModel.ClientUserId = null;
        accountModel.RowVersion = input.RowVersion;

        await _repository.Updateable(accountModel).UpdateColumns(e => e.ClientUserId).ExecuteCommandWithOptLockAsync(true);

        await _user.Logout();
        await _user.RevokeAccount(accountModel.AccountId);
        await AccountForceOffline(accountModel.AccountId, "微信已解绑，请重新登录");
    }
}
