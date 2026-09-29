// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.ApplicationOpenId.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Fast.Center.Service.ApplicationOpenId;

/// <summary>
/// 应用标识服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "applicationOpenId")]
[PlatformOnly]
public class ApplicationOpenIdService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<ApplicationOpenIdModel> _repository;

    public ApplicationOpenIdService(IUser user, ISqlSugarRepository<ApplicationOpenIdModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 获取应用标识分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取应用标识分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.AppOpenId.Paged)]
    public async Task<PagedResult<QueryApplicationOpenIdPagedOutput>> QueryApplicationOpenIdPaged(
        QueryApplicationOpenIdPagedInput input)
    {
        ISugarQueryable<ApplicationOpenIdModel> queryable = _repository.Entities.Includes(e => e.Application);
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);
        if (!_user.IsSuperAdmin && tenantModel.TenantType != TenantTypeEnum.System)
        {
            queryable = queryable.Where(wh => wh.Application.TenantId == _user.TenantId);
        }

        return await queryable.WhereIF(input.AppId != null, wh => wh.AppId == input.AppId)
            .WhereIF(input.AppType != null, wh => wh.AppType == input.AppType)
            .WhereIF(input.EnvironmentType != null, wh => wh.EnvironmentType == input.EnvironmentType)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryApplicationOpenIdPagedOutput
            {
                RecordId = sl.RecordId,
                OpenId = sl.OpenId,
                AppType = sl.AppType,
                EnvironmentType = sl.EnvironmentType,
                RequestTimeout = sl.RequestTimeout,
                RequestEncipher = sl.RequestEncipher,
                WeChatMerchantNo = sl.WeChatMerchantNo,
                AlipayMerchantNo = sl.AlipayMerchantNo,
                WeChatAccessTokenRefreshTime = sl.WeChatAccessTokenRefreshTime,
                WeChatJsApiTicketRefreshTime = sl.WeChatJsApiTicketRefreshTime,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取应用标识详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取应用标识详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.AppOpenId.Detail)]
    public async Task<QueryApplicationOpenIdDetailOutput> QueryApplicationOpenIdDetail(
        [Required(ErrorMessage = "记录Id不能为空")] long? recordId)
    {
        QueryApplicationOpenIdDetailOutput result = await _repository.Entities.Includes(e => e.Application)
            .Where(wh => wh.RecordId == recordId)
            .Select(sl => new QueryApplicationOpenIdDetailOutput
            {
                RecordId = sl.RecordId,
                AppId = sl.AppId,
                AppName = sl.Application.AppName,
                OpenId = sl.OpenId,
                AppType = sl.AppType,
                OpenSecret = sl.OpenSecret,
                EnvironmentType = sl.EnvironmentType,
                LoginComponent = sl.LoginComponent,
                WebSocketUrl = sl.WebSocketUrl,
                RequestTimeout = sl.RequestTimeout,
                RequestEncipher = sl.RequestEncipher,
                WeChatMerchantId = sl.WeChatMerchantId,
                WeChatMerchantNo = sl.WeChatMerchantNo,
                AlipayMerchantId = sl.AlipayMerchantId,
                AlipayMerchantNo = sl.AlipayMerchantNo,
                WeChatAccessTokenRefreshTime = sl.WeChatAccessTokenRefreshTime,
                WeChatJsApiTicketRefreshTime = sl.WeChatJsApiTicketRefreshTime,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        result.TemplateIdList = await _repository.Queryable<ApplicationTemplateIdModel>()
            .Where(wh => wh.OpenId == result.OpenId)
            .Select(sl => new EditApplicationTemplateIdInput
            {
                RecordId = sl.RecordId, TemplateType = sl.TemplateType, TemplateId = sl.TemplateId
            })
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// 添加应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("添加应用标识", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.AppOpenId.Add)]
    public async Task AddApplicationOpenId(AddApplicationOpenIdInput input)
    {
        if (await _repository.AnyAsync(a => a.OpenId == input.OpenId))
        {
            throw new UserFriendlyException("应用标识重复！");
        }

        ApplicationModel applicationModel = await _repository.Queryable<ApplicationModel>().InSingleAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var applicationOpenIdModel = new ApplicationOpenIdModel
        {
            AppId = applicationModel.AppId,
            OpenId = input.OpenId,
            AppType = input.AppType,
            OpenSecret = input.OpenSecret,
            EnvironmentType = input.EnvironmentType,
            LoginComponent = input.LoginComponent,
            WebSocketUrl = input.WebSocketUrl,
            RequestTimeout = input.RequestTimeout,
            RequestEncipher = input.RequestEncipher,
            WeChatMerchantId = input.WeChatMerchantId,
            WeChatMerchantNo = input.WeChatMerchantNo,
            AlipayMerchantId = input.AlipayMerchantId,
            AlipayMerchantNo = input.AlipayMerchantNo,
            Remark = input.Remark
        };

        if (!string.IsNullOrWhiteSpace(input.OpenSecret))
        {
            WechatApiClient apiClient = WechatApiClientBuilder
                .Create(new WechatApiClientOptions {AppId = input.OpenId, AppSecret = input.OpenSecret})
                .Build();
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest());
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            applicationOpenIdModel.WeChatAccessToken = response.AccessToken;
            applicationOpenIdModel.WeChatAccessTokenExpiresIn = response.ExpiresIn;
            applicationOpenIdModel.WeChatAccessTokenRefreshTime = DateTime.Now;

            if (input.AppType == AppEnvironmentEnum.WeChatServiceAccount)
            {
                CgibinTicketGetTicketResponse ticketResponse =
                    await apiClient.ExecuteCgibinTicketGetTicketAsync(
                        new CgibinTicketGetTicketRequest {AccessToken = response.AccessToken});
                if (!ticketResponse.IsSuccessful())
                {
                    throw new UserFriendlyException(
                        $"调用获取Ticket接口失败。ErrorCode：{ticketResponse.ErrorCode}。ErrorMessage：{ticketResponse.ErrorMessage}");
                }

                applicationOpenIdModel.WeChatJsApiTicket = ticketResponse.Ticket;
                applicationOpenIdModel.WeChatJsApiTicketExpiresIn = ticketResponse.ExpiresIn;
                applicationOpenIdModel.WeChatJsApiTicketRefreshTime = DateTime.Now;
            }
        }

        await _repository.InsertAsync(applicationOpenIdModel);
        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationOpenIdModel.OpenId);
    }

    /// <summary>
    /// 编辑应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑应用标识", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.AppOpenId.Edit)]
    public async Task EditApplicationOpenId(EditApplicationOpenIdInput input)
    {
        if (await _repository.AnyAsync(a => a.OpenId == input.OpenId && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("应用标识重复！");
        }

        var templateIds = input.TemplateIdList.Select(sl => sl.TemplateId).Distinct().ToList();
        if (templateIds.Count != input.TemplateIdList.Count)
        {
            throw new UserFriendlyException("模板Id重复！");
        }

        var templateTypes = input.TemplateIdList.Select(sl => sl.TemplateType).Distinct().ToList();
        if (templateTypes.Count != input.TemplateIdList.Count)
        {
            throw new UserFriendlyException("模板类型重复！");
        }

        if (await _repository.Queryable<ApplicationTemplateIdModel>()
                .AnyAsync(a => templateIds.Contains(a.TemplateId) && a.OpenId != input.OpenId))
        {
            throw new UserFriendlyException("模板Id重复！");
        }

        ApplicationModel applicationModel = await _repository.Queryable<ApplicationModel>().InSingleAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        ApplicationOpenIdModel applicationOpenIdModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationOpenIdModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        List<ApplicationTemplateIdModel> templateIdList = await _repository.Queryable<ApplicationTemplateIdModel>()
            .Where(wh => wh.OpenId == input.OpenId)
            .ToListAsync();

        applicationOpenIdModel.OpenId = input.OpenId;
        applicationOpenIdModel.AppId = applicationModel.AppId;
        applicationOpenIdModel.AppType = input.AppType;
        applicationOpenIdModel.OpenSecret = input.OpenSecret;
        applicationOpenIdModel.EnvironmentType = input.EnvironmentType;
        applicationOpenIdModel.LoginComponent = input.LoginComponent;
        applicationOpenIdModel.WebSocketUrl = input.WebSocketUrl;
        applicationOpenIdModel.RequestTimeout = input.RequestTimeout;
        applicationOpenIdModel.RequestEncipher = input.RequestEncipher;
        applicationOpenIdModel.WeChatMerchantId = input.WeChatMerchantId;
        applicationOpenIdModel.WeChatMerchantNo = input.WeChatMerchantNo;
        applicationOpenIdModel.AlipayMerchantId = input.AlipayMerchantId;
        applicationOpenIdModel.AlipayMerchantNo = input.AlipayMerchantNo;
        applicationOpenIdModel.Remark = input.Remark;
        applicationOpenIdModel.RowVersion = input.RowVersion;

        if (!string.IsNullOrWhiteSpace(input.OpenSecret))
        {
            WechatApiClient apiClient = WechatApiClientBuilder
                .Create(new WechatApiClientOptions {AppId = input.OpenId, AppSecret = input.OpenSecret})
                .Build();
            CgibinStableTokenResponse response = await apiClient.ExecuteCgibinStableTokenAsync(new CgibinStableTokenRequest());
            if (!response.IsSuccessful())
            {
                throw new UserFriendlyException(
                    $"调用刷新AccessToken接口失败。ErrorCode：{response.ErrorCode}。ErrorMessage：{response.ErrorMessage}");
            }

            applicationOpenIdModel.WeChatAccessToken = response.AccessToken;
            applicationOpenIdModel.WeChatAccessTokenExpiresIn = response.ExpiresIn;
            applicationOpenIdModel.WeChatAccessTokenRefreshTime = DateTime.Now;

            if (input.AppType == AppEnvironmentEnum.WeChatServiceAccount)
            {
                CgibinTicketGetTicketResponse ticketResponse =
                    await apiClient.ExecuteCgibinTicketGetTicketAsync(
                        new CgibinTicketGetTicketRequest {AccessToken = response.AccessToken});
                if (!ticketResponse.IsSuccessful())
                {
                    throw new UserFriendlyException(
                        $"调用获取Ticket接口失败。ErrorCode：{ticketResponse.ErrorCode}。ErrorMessage：{ticketResponse.ErrorMessage}");
                }

                applicationOpenIdModel.WeChatJsApiTicket = ticketResponse.Ticket;
                applicationOpenIdModel.WeChatJsApiTicketExpiresIn = ticketResponse.ExpiresIn;
                applicationOpenIdModel.WeChatJsApiTicketRefreshTime = DateTime.Now;
            }
        }

        var addApplicationTemplateIdList = new List<ApplicationTemplateIdModel>();
        var updateApplicationTemplateIdList = new List<ApplicationTemplateIdModel>();
        foreach (EditApplicationTemplateIdInput item in input.TemplateIdList)
        {
            ApplicationTemplateIdModel applicationTemplateIdModel;
            if (item.RecordId == null)
            {
                // 新增的
                applicationTemplateIdModel = new ApplicationTemplateIdModel
                {
                    AppId = applicationOpenIdModel.AppId,
                    OpenId = applicationOpenIdModel.OpenId,
                    TemplateType = item.TemplateType,
                    TemplateId = item.TemplateId
                };
                addApplicationTemplateIdList.Add(applicationTemplateIdModel);
            }
            else
            {
                // 更新的
                applicationTemplateIdModel = templateIdList.SingleOrDefault(s => s.RecordId == item.RecordId);
                if (applicationTemplateIdModel == null)
                {
                    throw new UserFriendlyException("数据不存在！");
                }

                applicationTemplateIdModel.TemplateType = item.TemplateType;
                applicationTemplateIdModel.TemplateId = item.TemplateId;
                updateApplicationTemplateIdList.Add(applicationTemplateIdModel);
            }
        }

        // 删除的
        var deleteApplicationTemplateIdList = templateIdList
            .Where(wh => input.TemplateIdList.All(a => a.RecordId != wh.RecordId))
            .ToList();

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(applicationOpenIdModel);
            await _repository.Deleteable(deleteApplicationTemplateIdList).ExecuteCommandAsync();
            await _repository.Updateable(updateApplicationTemplateIdList).ExecuteCommandAsync();
            await _repository.Insertable(addApplicationTemplateIdList).ExecuteCommandAsync();
        }, ex => throw ex);

        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationOpenIdModel.OpenId);
    }

    /// <summary>
    /// 删除应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("删除应用标识", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.AppOpenId.Delete)]
    public async Task DeleteApplicationOpenId(RecordIdInput input)
    {
        ApplicationOpenIdModel applicationOpenIdModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationOpenIdModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(applicationOpenIdModel);
        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationOpenIdModel.OpenId);
    }
}
