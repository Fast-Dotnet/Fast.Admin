// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Core;
using Fast.DependencyInjection;
using Fast.Logging;
using Fast.NET.Core;
using Fast.Scheduler;
using Fast.Scheduler.BackgroundServices;
using Fast.Serialization;
using Fast.SqlSugar;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 初始化框架
builder.Initialize();

// 添加序列化服务
builder.Services.AddSerialization();

// 添加日志服务
builder.Services.AddLoggingService(builder.Configuration);

// 添加依赖注入服务
builder.Services.AddDependencyInjection();

// 添加缓存服务
builder.Services.AddCache();

RedisSettingsOptions redisOptions = builder.Configuration.GetSection("RedisSettings").Get<RedisSettingsOptions>();
if (redisOptions != null)
{
    // 添加分布式缓存
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions = new ConfigurationOptions
        {
            Password = redisOptions.DbPwd,
            DefaultDatabase = redisOptions.DbName ?? 2,
            AbortOnConnectFail = false,
            EndPoints = {{redisOptions.ServiceIp, redisOptions.Port ?? 6379}}
        };
        options.InstanceName = $"{nameof(Fast)}:";
    });
}

// 添加雪花Id
builder.Services.AddSnowflake(builder.Configuration);

// 添加 SqlSugar
builder.Services.AddSqlSugar(builder.Configuration, builder.Environment);

builder.Services.AddHttpClient();

// 添加 Quartz 服务
builder.Services.AddQuartzService(builder.Configuration);

// 添加删除日志托管服务
builder.Services.AddHostedService<DeleteLogBackgroundService>();

// 添加 SqlSugar 日志后台服务
builder.Services.AddHostedService<SqlSugarLogBackgroundService>();

// 添加应用程序生命周期托管服务
builder.Services.AddHostedService<ApplicationLifecycleHostedService>();

// 添加调度后台托管服务
builder.Services.AddHostedService<SchedulerHostedService>();

WebApplication app = builder.Build();

app.Run();
