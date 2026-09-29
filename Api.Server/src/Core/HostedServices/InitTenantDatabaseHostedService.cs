// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 租户数据库初始化托管服务
/// </summary>
[Order(104)]
public class InitTenantDatabaseHostedService : IHostedService
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化租户数据库托管服务
    /// </summary>
    public InitTenantDatabaseHostedService(IServiceProvider serviceProvider, ILogger<InitTenantDatabaseHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            // 创建请求作用域
            using IServiceScope scope = _serviceProvider.CreateScope();
            ITenantDatabaseService tenantDatabaseService = scope.ServiceProvider.GetService<ITenantDatabaseService>();

            // 初始化租户数据库
            foreach (TenantModel tenantModel in await db.Queryable<TenantModel>()
                         .Where(wh => wh.TenantType == TenantTypeEnum.System)
                         .ToListAsync(cancellationToken))
            {
                await tenantDatabaseService.InitDatabase(tenantModel.TenantId, DatabaseTypeEnum.Admin);
                await tenantDatabaseService.InitDatabase(tenantModel.TenantId, DatabaseTypeEnum.AdminLog);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "系统租户数据库初始化失败，应用停止启动。");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
