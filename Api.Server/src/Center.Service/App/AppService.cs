// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.App.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.App;

/// <summary>
/// App
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "app")]
public class AppService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ApplicationModel> _repository;

    public AppService(ISqlSugarRepository<ApplicationModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Launch
    /// </summary>
    [HttpPost("/launch")]
    [ApiInfo("Launch", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseEncipher]
    public async Task<LaunchOutput> Launch()
    {
        ApplicationOpenIdModel applicationOpenIdModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
        if (applicationOpenIdModel == null || applicationOpenIdModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("非法访问！");
        }

        return new LaunchOutput
        {
            Edition = applicationOpenIdModel.Application.Edition,
            AppNo = applicationOpenIdModel.Application.AppNo,
            AppName = applicationOpenIdModel.Application.AppName,
            LogoUrl = applicationOpenIdModel.Application.LogoUrl,
            ThemeColor = applicationOpenIdModel.Application.ThemeColor,
            AppType = applicationOpenIdModel.AppType,
            EnvironmentType = applicationOpenIdModel.EnvironmentType,
            LoginComponent = applicationOpenIdModel.LoginComponent,
            WebSocketUrl = applicationOpenIdModel.WebSocketUrl,
            RequestTimeout = applicationOpenIdModel.RequestTimeout,
            RequestEncipher = applicationOpenIdModel.RequestEncipher,
            TenantName = applicationOpenIdModel.Application.TenantName
        };
    }
}
