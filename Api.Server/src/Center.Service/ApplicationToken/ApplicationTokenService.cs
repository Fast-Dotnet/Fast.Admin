// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.ApplicationToken.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Fast.Center.Service.ApplicationToken;

/// <summary>
/// 应用凭据服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "applicationToken")]
[PlatformOnly]
public class ApplicationTokenService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ApplicationTokenModel> _repository;

    public ApplicationTokenService(ISqlSugarRepository<ApplicationTokenModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取应用凭据分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取应用凭据分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.AppToken.Paged)]
    public async Task<PagedResult<QueryApplicationTokenPagedOutput>> QueryApplicationTokenPaged(PagedInput input)
    {
        return await _repository.Entities.LeftJoin<ApplicationClientModel>((t1, t2) => t1.AppKey == t2.AppKey)
            .LeftJoin<ApplicationModel>((t1, t2, t3) => t2.AppId == t3.AppId)
            .SelectMergeTable((t1, t2, t3) => new QueryApplicationTokenPagedOutput
            {
                RecordId = t1.RecordId,
                AppName = t3.AppName,
                AppKey = t1.AppKey,
                AppType = t2.AppType,
                EnvironmentType = t2.EnvironmentType,
                TokenType = t1.TokenType,
                RefreshTime = t1.RefreshTime,
                ExpireTime = t1.ExpireTime
            })
            .OrderByIF(input.IsOrderBy, ob => ob.RefreshTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 添加应用凭据
    /// </summary>
    [HttpPost]
    [ApiInfo("添加应用凭据", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.AppToken.Add)]
    public async Task AddApplicationToken(AddApplicationTokenInput input)
    {
        if (await _repository.AnyAsync(a => a.AppKey == input.AppKey && a.TokenType == input.TokenType))
        {
            throw new UserFriendlyException("应用凭据重复！");
        }

        ApplicationClientModel applicationClientModel = await _repository.Queryable<ApplicationClientModel>()
            .Where(wh => wh.AppKey == input.AppKey)
            .SingleAsync();
        if (applicationClientModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (string.IsNullOrWhiteSpace(applicationClientModel.AppSecret))
        {
            throw new UserFriendlyException("应用密钥为空！");
        }

        var applicationTokenModel = new ApplicationTokenModel {AppKey = input.AppKey, TokenType = input.TokenType};

        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions {AppId = input.AppKey, AppSecret = applicationClientModel.AppSecret})
            .Build();

        DateTime dateTime = DateTime.Now;

        if (input.TokenType == ApplicationTokenTypeEnum.AccessToken)
        {
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest
            {
                // 不强制刷新
                ForceRefresh = false
            });
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            applicationTokenModel.TokenValue = response.AccessToken;
            applicationTokenModel.RefreshTime = dateTime;
            applicationTokenModel.ExpireTime = dateTime.AddSeconds(response.ExpiresIn);
        }
        else if (input.TokenType == ApplicationTokenTypeEnum.JsApiTicket)
        {
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest
            {
                // 不强制刷新
                ForceRefresh = false
            });
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            CgibinTicketGetTicketResponse ticketResponse =
                await apiClient.ExecuteCgibinTicketGetTicketAsync(
                    new CgibinTicketGetTicketRequest {AccessToken = response.AccessToken});
            if (!ticketResponse.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用获取Ticket接口失败。ErrorCode：{ticketResponse.ErrorCode}。ErrorMessage：{ticketResponse.ErrorMessage}");
            }

            applicationTokenModel.TokenValue = ticketResponse.Ticket;
            applicationTokenModel.RefreshTime = dateTime;
            applicationTokenModel.ExpireTime = dateTime.AddSeconds(ticketResponse.ExpiresIn);
        }

        await _repository.InsertAsync(applicationTokenModel);
    }

    /// <summary>
    /// 删除应用凭据
    /// </summary>
    [HttpPost]
    [ApiInfo("删除应用凭据", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.AppToken.Delete)]
    public async Task DeleteApplicationToken(ApplicationTokenIdInput input)
    {
        ApplicationTokenModel applicationTokenModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationTokenModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(applicationTokenModel);
        // 删除缓存
        await ApplicationTokenContext.DeleteApplicationToken(applicationTokenModel.AppKey, applicationTokenModel.TokenType);
    }

    /// <summary>
    /// 刷新应用凭据
    /// </summary>
    [HttpPost]
    [ApiInfo("刷新应用凭据", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.AppToken.Refresh)]
    public async Task RefreshApplicationToken(ApplicationTokenIdInput input)
    {
        ApplicationTokenModel applicationTokenModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationTokenModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        ApplicationClientModel applicationClientModel = await _repository.Queryable<ApplicationClientModel>()
            .Where(wh => wh.AppKey == applicationTokenModel.AppKey)
            .SingleAsync();
        if (applicationClientModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        WechatApiClient apiClient = WechatApiClientBuilder
            .Create(new WechatApiClientOptions
            {
                AppId = applicationTokenModel.AppKey, AppSecret = applicationClientModel.AppSecret
            })
            .Build();

        DateTime dateTime = DateTime.Now;

        if (applicationTokenModel.TokenType == ApplicationTokenTypeEnum.AccessToken)
        {
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest
            {
                // 不强制刷新
                ForceRefresh = false
            });
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            applicationTokenModel.TokenValue = response.AccessToken;
            applicationTokenModel.RefreshTime = dateTime;
            applicationTokenModel.ExpireTime = dateTime.AddSeconds(response.ExpiresIn);
        }
        else if (applicationTokenModel.TokenType == ApplicationTokenTypeEnum.JsApiTicket)
        {
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest
            {
                // 不强制刷新
                ForceRefresh = false
            });
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            CgibinTicketGetTicketResponse ticketResponse =
                await apiClient.ExecuteCgibinTicketGetTicketAsync(
                    new CgibinTicketGetTicketRequest {AccessToken = response.AccessToken});
            if (!ticketResponse.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用获取Ticket接口失败。ErrorCode：{ticketResponse.ErrorCode}。ErrorMessage：{ticketResponse.ErrorMessage}");
            }

            applicationTokenModel.TokenValue = ticketResponse.Ticket;
            applicationTokenModel.RefreshTime = dateTime;
            applicationTokenModel.ExpireTime = dateTime.AddSeconds(ticketResponse.ExpiresIn);
        }

        await _repository.UpdateAsync(applicationTokenModel);
        // 删除缓存
        await ApplicationTokenContext.DeleteApplicationToken(applicationTokenModel.AppKey, applicationTokenModel.TokenType);
    }
}
