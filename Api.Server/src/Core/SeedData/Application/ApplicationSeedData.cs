// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 应用种子数据
/// </summary>
internal static class ApplicationSeedData
{
    /// <summary>
    /// 应用种子数据
    /// </summary>
    /// <returns>默认应用种子数据</returns>
    public static async Task<ApplicationModel> SeedData(ISqlSugarClient db, DateTime dateTime)
    {
        var applicationModel = new ApplicationModel
        {
            AppId = YitIdHelper.NextId(),
            Edition = EditionEnum.None,
            AppNo = "App201801",
            AppName = "Fast.Admin",
            LogoUrl = CommonConst.DefaultLogo,
            ThemeColor = "#409EFF",
            CreatedTime = dateTime
        };
        applicationModel = await db.Insertable(applicationModel).ExecuteReturnEntityAsync();
        await db.Insertable(new List<ApplicationOpenIdModel>
            {
                new()
                {
                    AppId = applicationModel.AppId,
                    OpenId = "127.0.0.1:2001",
                    AppType = AppEnvironmentEnum.Web,
                    EnvironmentType =
                        FastContext.HostEnvironment.IsDevelopment()
                            ? EnvironmentTypeEnum.Development
                            : EnvironmentTypeEnum.Production,
                    LoginComponent = "ClassicLogin",
                    WebSocketUrl = "/hubs/chatHub",
                    RequestTimeout = 60000,
                    RequestEncipher = true,
                    CreatedTime = dateTime
                },
                new()
                {
                    AppId = applicationModel.AppId,
                    OpenId = "DesktopOpenId",
                    AppType = AppEnvironmentEnum.Desktop,
                    EnvironmentType =
                        FastContext.HostEnvironment.IsDevelopment()
                            ? EnvironmentTypeEnum.Development
                            : EnvironmentTypeEnum.Production,
                    RequestTimeout = 60000,
                    RequestEncipher = false,
                    CreatedTime = dateTime
                },
                new()
                {
                    AppId = applicationModel.AppId,
                    OpenId = "WeChatMiniProgramOpenId",
                    AppType = AppEnvironmentEnum.WeChatMiniProgram,
                    EnvironmentType =
                        FastContext.HostEnvironment.IsDevelopment()
                            ? EnvironmentTypeEnum.Development
                            : EnvironmentTypeEnum.Production,
                    WebSocketUrl = "/hubs/chatHub",
                    RequestTimeout = 60000,
                    RequestEncipher = true,
                    CreatedTime = dateTime
                },
                new()
                {
                    AppId = applicationModel.AppId,
                    OpenId = "AndroidOpenId",
                    AppType = AppEnvironmentEnum.Android,
                    EnvironmentType =
                        FastContext.HostEnvironment.IsDevelopment()
                            ? EnvironmentTypeEnum.Development
                            : EnvironmentTypeEnum.Production,
                    WebSocketUrl = "/hubs/chatHub",
                    RequestTimeout = 60000,
                    RequestEncipher = true,
                    CreatedTime = dateTime
                },
                new()
                {
                    AppId = applicationModel.AppId,
                    OpenId = "IOSOpenId",
                    AppType = AppEnvironmentEnum.IOS,
                    EnvironmentType =
                        FastContext.HostEnvironment.IsDevelopment()
                            ? EnvironmentTypeEnum.Development
                            : EnvironmentTypeEnum.Production,
                    WebSocketUrl = "/hubs/chatHub",
                    RequestTimeout = 60000,
                    RequestEncipher = true,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();

        return applicationModel;
    }
}
