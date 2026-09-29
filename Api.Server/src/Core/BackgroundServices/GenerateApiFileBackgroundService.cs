// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.OpenApi;
using Fast.Swagger;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fast.Core;

/// <summary>
/// 生成 API 文件后台服务
/// </summary>
[Order(999)]
public class GenerateApiFileBackgroundService : BackgroundService
{
    /// <summary>
    /// 托管应用程序生命周期
    /// </summary>
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    /// <summary>
    /// 服务器
    /// </summary>
    private readonly IServer _server;

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
    /// 生成 API 文件托管服务
    /// </summary>
    public GenerateApiFileBackgroundService(IHostApplicationLifetime hostApplicationLifetime, IServer server,
        IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider, IOptions<SwaggerSettingsOptions> options,
        ILogger<GenerateApiFileBackgroundService> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _server = server;
        _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
        _swaggerSettings = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 只有开发环境才会生成
        if (!FastContext.HostEnvironment.IsDevelopment())
        {
            return;
        }

        // 只有启用了 Swagger 才会生成
        if (_swaggerSettings.Enable != true)
        {
            return;
        }

        try
        {
            // OpenAPI 生成需要访问已经监听的本机地址，因此等待应用启动完成后再继续
            if (!_hostApplicationLifetime.ApplicationStarted.IsCancellationRequested)
            {
                var applicationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await using CancellationTokenRegistration registration =
                    _hostApplicationLifetime.ApplicationStarted.Register(() => applicationStarted.TrySetResult());
                await applicationStarted.Task.WaitAsync(stoppingToken);
            }

            IServerAddressesFeature feature = _server.Features.Get<IServerAddressesFeature>();
            // 默认获取第一个地址，并且处理 [::]
            string address = feature?.Addresses.FirstOrDefault()?.Replace("[::]", "127.0.0.1");
            if (string.IsNullOrWhiteSpace(address))
            {
                return;
            }

            // 获取 Swagger 分组
            var groupList = _swaggerSettings.GroupOpenApiInfos?.Select(sl => sl.Group).ToList();

            // 直接等待 OpenAPI 文件生成完成，由托管服务统一观察异常和停止信号
            await OpenApiUtil.GenerateOpenApi(address, _apiDescriptionGroupCollectionProvider, groupList);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 宿主停止时取消启动等待或文件生成属于正常停机流程
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Generate api file error...");
        }
    }
}
