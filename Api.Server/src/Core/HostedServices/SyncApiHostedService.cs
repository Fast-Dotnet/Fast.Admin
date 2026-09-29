// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Reflection;
using Fast.Center.Domain;
using Fast.DynamicApplication;
using Fast.JwtBearer;
using Fast.SqlSugar;
using Fast.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 同步 API 托管服务
/// </summary>
[Order(105)]
public class SyncApiHostedService : IHostedService
{
    /// <summary>
    /// 接口描述提供程序
    /// </summary>
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;

    /// <summary>
    /// Swagger 配置
    /// </summary>
    private readonly SwaggerSettingsOptions _swaggerSettings;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 同步 API 托管服务
    /// </summary>
    public SyncApiHostedService(IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
        IOptions<SwaggerSettingsOptions> options, ILogger<SyncApiHostedService> logger)
    {
        _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
        _swaggerSettings = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DateTime dateTime = DateTime.Now;

        string serviceName = Assembly.GetEntryAssembly()!.GetName().Name;
        var addApiInfoList = new List<ApiInfoModel>();
        var updateApiInfoList = new List<ApiInfoModel>();
        var apiUrlList = new List<string>();

        MAppContext.ConsoleWrite(console =>
        {
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.Green;
            console.Write("info");
            console.ResetColor();
            console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.DarkGray;
            console.WriteLine("      开始同步接口信息...");
        });

        try
        {
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            List<ApiInfoModel> apiInfoList = await db.Queryable<ApiInfoModel>()
                .Where(wh => wh.ServiceName == serviceName)
                .ToListAsync(cancellationToken);

            // 循环所有接口
            foreach (ApiDescription apiDescriptionGroup in _apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items
                         .SelectMany(sl => sl.Items)
                         .ToList())
            {
                IList<object> endpointMetadata = apiDescriptionGroup.ActionDescriptor.EndpointMetadata;
                // 获取接口描述配置，Action 配置优先于 Controller 配置
                ApiDescriptionSettingsAttribute apiDescriptionSettingsAttribute = endpointMetadata
                    .OfType<ApiDescriptionSettingsAttribute>()
                    .LastOrDefault();
                // 判断是否忽略当前接口
                if (apiDescriptionSettingsAttribute?.IgnoreApi == true)
                {
                    continue;
                }

                // 判断是否允许匿名访问
                bool allowAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
                // 判断是否允许跳过权限验证
                bool allowForbidden = endpointMetadata.OfType<AllowForbiddenAttribute>().Any();
                // 获取权限特性，Action 配置优先于 Controller 配置
                PermissionAttribute permissionAttribute = endpointMetadata.OfType<PermissionAttribute>().LastOrDefault();
                // 获取接口信息特性，Action 配置优先于 Controller 配置
                ApiInfoAttribute apiInfoAttribute = endpointMetadata.OfType<ApiInfoAttribute>().LastOrDefault();

                // 获取分组名称
                string groupName = apiDescriptionGroup.GroupName ?? "Default";

                string moduleName = apiDescriptionSettingsAttribute?.Name
                                    ?? (apiDescriptionGroup.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;
                int sort = apiDescriptionSettingsAttribute?.Order ?? 0;
                SwaggerOpenApiInfo groupOpenApiInfo =
                    _swaggerSettings.GroupOpenApiInfos.FirstOrDefault(f => f.Group == groupName);

                string apiUrl = $"/{apiDescriptionGroup.RelativePath}";

                // 判断原有的Url是否存在
                ApiInfoModel apiInfo = apiInfoList.SingleOrDefault(s => s.ApiUrl == apiUrl);

                HttpRequestMethodEnum method = Enum.Parse<HttpRequestMethodEnum>(apiDescriptionGroup.HttpMethod, true);
                HttpRequestActionEnum action = apiInfoAttribute?.Action ?? HttpRequestActionEnum.None;
                bool hasPermission = !allowForbidden && permissionAttribute?.TagList?.Count > 0;

                var apiInfoModel = new ApiInfoModel
                {
                    ServiceName = serviceName,
                    GroupName = groupName,
                    GroupTitle = groupOpenApiInfo?.Title,
                    Version = groupOpenApiInfo?.Version,
                    Description = groupOpenApiInfo?.Description,
                    ModuleName = moduleName,
                    ApiUrl = apiUrl,
                    ApiName = apiInfoAttribute?.Name,
                    Method = method,
                    Action = action,
                    HasAuth = !allowAnonymous,
                    HasPermission = hasPermission,
                    Tags = permissionAttribute?.TagList ?? [],
                    Sort = sort
                };
                apiUrlList.Add(apiInfoModel.ApiUrl);

                if (apiInfo != null)
                {
                    apiInfoModel.ApiId = apiInfo.ApiId;
                    // 不相同才修改
                    if (!apiInfo.Equals(apiInfoModel))
                    {
                        apiInfo.ServiceName = apiInfoModel.ServiceName;
                        apiInfo.GroupName = apiInfoModel.GroupName;
                        apiInfo.GroupTitle = apiInfoModel.GroupTitle;
                        apiInfo.Version = apiInfoModel.Version;
                        apiInfo.Description = apiInfoModel.Description;
                        apiInfo.ModuleName = apiInfoModel.ModuleName;
                        apiInfo.ApiUrl = apiInfoModel.ApiUrl;
                        apiInfo.ApiName = apiInfoModel.ApiName;
                        apiInfo.Method = apiInfoModel.Method;
                        apiInfo.Action = apiInfoModel.Action;
                        apiInfo.HasAuth = apiInfoModel.HasAuth;
                        apiInfo.HasPermission = apiInfoModel.HasPermission;
                        apiInfo.Tags = apiInfoModel.Tags;
                        apiInfo.Sort = apiInfoModel.Sort;
                        apiInfo.UpdatedTime = dateTime;
                        updateApiInfoList.Add(apiInfo);
                    }
                }
                else
                {
                    apiInfoModel.ApiId = YitIdHelper.NextId();
                    apiInfoModel.CreatedTime = dateTime;
                    addApiInfoList.Add(apiInfoModel);
                }
            }

            // 加载Aop
            SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

            var deleteApiInfoList = apiInfoList.Where(wh => !apiUrlList.Contains(wh.ApiUrl)).ToList();

            if (deleteApiInfoList.Count > 0)
            {
                await db.Deleteable(deleteApiInfoList).ExecuteCommandAsync(cancellationToken);
            }

            await db.Updateable(updateApiInfoList).ExecuteCommandAsync(cancellationToken);
            await db.Insertable(addApiInfoList).ExecuteCommandAsync(cancellationToken);

            CacheContext.ApiInfoList = apiInfoList;
            CacheContext.ApiInfoList.AddRange(addApiInfoList);

            MAppContext.ConsoleWrite(console =>
            {
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.Green;
                console.Write("info");
                console.ResetColor();
                console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine(
                    $"      同步接口信息成功。新增 {addApiInfoList.Count} 个，更新 {updateApiInfoList.Count} 个，删除 {deleteApiInfoList.Count} 个。");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync api error...");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
