// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.ApplicationClient.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.ApplicationClient;

/// <summary>
/// 应用标识服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "ApplicationClient")]
[PlatformOnly]
public class ApplicationClientService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ApplicationClientModel> _repository;

    public ApplicationClientService(ISqlSugarRepository<ApplicationClientModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取应用标识详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取应用标识详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.App.ClientDetail)]
    public async Task<QueryApplicationClientDetailOutput> QueryApplicationClientDetail(
        [Required(ErrorMessage = "记录Id不能为空")] long? recordId)
    {
        QueryApplicationClientDetailOutput result = await _repository.Entities.Includes(e => e.Application)
            .Where(wh => wh.RecordId == recordId)
            .Select(sl => new QueryApplicationClientDetailOutput
            {
                RecordId = sl.RecordId,
                AppId = sl.AppId,
                AppName = sl.Application.AppName,
                AppKey = sl.AppKey,
                AppType = sl.AppType,
                AppSecret = sl.AppSecret,
                EnvironmentType = sl.EnvironmentType,
                LoginComponent = sl.LoginComponent,
                RequestTimeout = sl.RequestTimeout,
                RequestEncipher = sl.RequestEncipher,
                WeChatMerchantId = sl.WeChatMerchantId,
                WeChatMerchantNo = sl.WeChatMerchantNo,
                AlipayMerchantId = sl.AlipayMerchantId,
                AlipayMerchantNo = sl.AlipayMerchantNo,
                Remark = sl.Remark,
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

        return result;
    }

    /// <summary>
    /// 添加应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("添加应用标识", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.App.ClientAdd)]
    public async Task AddApplicationClient(AddApplicationClientInput input)
    {
        if (await _repository.AnyAsync(a => a.AppKey == input.AppKey))
        {
            throw new UserFriendlyException("应用标识重复！");
        }

        ApplicationModel applicationModel = await _repository.Queryable<ApplicationModel>().InSingleAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var applicationClientModel = new ApplicationClientModel
        {
            AppId = applicationModel.AppId,
            AppKey = input.AppKey,
            AppType = input.AppType,
            AppSecret = input.AppSecret,
            EnvironmentType = input.EnvironmentType,
            LoginComponent = input.LoginComponent,
            RequestTimeout = input.RequestTimeout,
            RequestEncipher = input.RequestEncipher,
            WeChatMerchantId = input.WeChatMerchantId,
            WeChatMerchantNo = input.WeChatMerchantNo,
            AlipayMerchantId = input.AlipayMerchantId,
            AlipayMerchantNo = input.AlipayMerchantNo,
            Remark = input.Remark
        };

        await _repository.InsertAsync(applicationClientModel);
        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationClientModel.AppKey);
    }

    /// <summary>
    /// 编辑应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑应用标识", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.App.ClientEdit)]
    public async Task EditApplicationClient(EditApplicationClientInput input)
    {
        if (await _repository.AnyAsync(a => a.AppKey == input.AppKey && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("应用标识重复！");
        }

        ApplicationModel applicationModel = await _repository.Queryable<ApplicationModel>().InSingleAsync(input.AppId);
        if (applicationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        ApplicationClientModel applicationClientModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationClientModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        applicationClientModel.AppKey = input.AppKey;
        applicationClientModel.AppId = applicationModel.AppId;
        applicationClientModel.AppType = input.AppType;
        applicationClientModel.AppSecret = input.AppSecret;
        applicationClientModel.EnvironmentType = input.EnvironmentType;
        applicationClientModel.LoginComponent = input.LoginComponent;
        applicationClientModel.RequestTimeout = input.RequestTimeout;
        applicationClientModel.RequestEncipher = input.RequestEncipher;
        applicationClientModel.WeChatMerchantId = input.WeChatMerchantId;
        applicationClientModel.WeChatMerchantNo = input.WeChatMerchantNo;
        applicationClientModel.AlipayMerchantId = input.AlipayMerchantId;
        applicationClientModel.AlipayMerchantNo = input.AlipayMerchantNo;
        applicationClientModel.Remark = input.Remark;
        applicationClientModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(applicationClientModel);
        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationClientModel.AppKey);
    }

    /// <summary>
    /// 删除应用标识
    /// </summary>
    [HttpPost]
    [ApiInfo("删除应用标识", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.App.ClientDelete)]
    public async Task DeleteApplicationClient(DeleteApplicationClientInput input)
    {
        ApplicationClientModel applicationClientModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (applicationClientModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(applicationClientModel);
        // 删除缓存
        await ApplicationContext.DeleteApplication(applicationClientModel.AppKey);
    }
}
