// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Fast.Scheduler;

/// <summary>
/// <see cref="IServiceCollection"/> 的 Quartz 扩展方法
/// </summary>
[SuppressSniffer]
public static class IServiceCollectionExtension
{
    /// <summary>
    /// 添加 Quartz 服务
    /// </summary>
    /// <param name="services">要注册调度服务的服务集合</param>
    /// <param name="configuration">应用配置</param>
    /// <param name="isExecutionHost">是否为调度执行宿主</param>
    /// <returns>用于继续链式配置的服务集合</returns>
    public static IServiceCollection AddQuartzService(this IServiceCollection services, IConfiguration configuration,
        bool isExecutionHost = true)
    {
        SchedulerContext.IsExecutionHost = isExecutionHost;

        services.AddQuartz(options =>
        {
            // 设置调度器Id
            options.SchedulerId = "AUTO";

            // 设置关闭时中断作业
            options.InterruptJobsOnShutdown = true;

            // 设置关闭时等待作业中断
            options.InterruptJobsOnShutdownWithWait = true;

            // 默认最大批处理作业数量为 1，这里修改为 10
            options.MaxBatchSize = 10;

            if (isExecutionHost)
            {
                // 默认最大并发为 10，这里修改为 100
                options.UseDefaultThreadPool(100);
            }
            else
            {
                // 管理节点只操作持久化存储，不执行调度作业
                options.UseZeroSizeThreadPool();
            }

            // 配置持久化存储策略
            options.UsePersistentStore(x =>
            {
                // 强制作业数据映射的值被视为字符串，避免对象意外序列化后格式破坏导致的问题，默认为 false
                //x.UseProperties = true;

                // 启用集群模式
                x.UseClustering();

                // 数据库连接字符串
                string connectionString = SqlSugarDatabaseUtil.GetConnectionStr(SqlSugarContext.ConnectionSettings.DbType!.Value,
                    SqlSugarContext.ConnectionSettings);

                switch (SqlSugarContext.ConnectionSettings.DbType)
                {
                    case DbType.MySql:
                        // 使用 MySql 作为持久化存储的提供者
                        x.UseMySql(o =>
                        {
                            // 数据库连接字符串
                            o.ConnectionString = connectionString;
                        }, SqlSugarContext.ConnectionSettings.DbName);
                        break;
                    case DbType.SqlServer:
                        // 使用 Sql Server 作为持久化存储的提供者
                        x.UseSqlServer(o =>
                        {
                            // 数据库连接字符串
                            o.ConnectionString = connectionString;
                        }, SqlSugarContext.ConnectionSettings.DbName);
                        break;
                    case DbType.Sqlite:
                        // 使用 SqlLite 作为持久化存储的提供者
                        x.UseSQLite(o =>
                        {
                            // 数据库连接字符串
                            o.ConnectionString = connectionString;
                        }, SqlSugarContext.ConnectionSettings.DbName);
                        break;
                    case DbType.Oracle:
                        // 使用 Oracle 作为持久化存储的提供者
                        x.UseOracle(o =>
                        {
                            // 数据库连接字符串
                            o.ConnectionString = connectionString;
                        }, SqlSugarContext.ConnectionSettings.DbName);
                        break;
                    case DbType.PostgreSQL:
                        // 使用 Postgres SQL 作为持久化存储的提供者
                        x.UsePostgres(o =>
                        {
                            // 数据库连接字符串
                            o.ConnectionString = connectionString;
                        }, SqlSugarContext.ConnectionSettings.DbName);
                        break;
                }

                // 使用 Newtonsoft.NET 序列化器
                x.UseNewtonsoftJsonSerializer();
            });
        });

        // 调度器工厂
        services.TryAddSingleton<ContainerConfigurationProcessor>();
        services.TryAddSingleton<IDependencySchedulerFactory, DependencySchedulerFactory>();
        services.AddHostedService<SchedulerShutdownHostedService>();

        if (isExecutionHost)
        {
            // 本地作业服务
            Type ISchedulerJobType = typeof(ISchedulerJob);
            var schedulerJobTypes = MAppContext.EffectiveTypes.Where(wh =>
                    ISchedulerJobType.IsAssignableFrom(wh) && wh.IsClass && !wh.IsInterface && !wh.IsAbstract)
                .ToList();
            foreach (Type type in schedulerJobTypes)
            {
                services.TryAddScoped(type);
            }
        }

        return services;
    }

    /// <summary>
    /// 添加 Quartz 托管服务
    /// </summary>
    /// <returns>用于继续链式配置的服务集合</returns>
    public static IServiceCollection AddQuartzHostedService(this IServiceCollection services)
    {
        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;

            // when we need to init another IHostedServices first
            options.StartDelay = TimeSpan.FromSeconds(10);

            // start the task after the application has completed its startup process.
            options.AwaitApplicationStarted = true;
        });

        return services;
    }
}
