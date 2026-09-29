// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Core;
using Fast.DependencyInjection;
using Fast.DynamicApplication;
using Fast.JwtBearer;
using Fast.Logging;
using Fast.NET.Core;
using Fast.OpenApi;
using Fast.Runtime;
using Fast.Scheduler;
using Fast.Serialization;
using Fast.SqlSugar;
using Fast.Swagger;
using Fast.UnifyResult;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 初始化框架
builder.Initialize();

// 添加序列化服务
builder.Services.AddSerialization();

// 添加日志服务
builder.Services.AddLoggingService(builder.Configuration);

// 添加跨域服务
builder.Services.AddCorsAccessor(builder.Configuration);

// 添加 Gzip 压缩服务
builder.Services.AddGzipCompression();

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

// 添加 API 限流
builder.Services.AddApiRateLimit();

// 添加 JwtBearer 授权
builder.Services.AddJwtBearer(builder.Configuration);

// 添加即时通讯
builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol(options => options.PayloadSerializerSettings = JsonContext.SerializerOptions);

// Add Controllers
builder.Services.AddControllers()
    // 平台控制面租户边界
    .AddMvcFilter<PlatformAccessFilter>()
    // 请求日志拦截
    .AddMvcFilter<RequestActionFilter>()
    .AddSerialization();

// 添加动态Api服务
builder.Services.AddDynamicApplication();

// 添加规范化返回服务
builder.Services.AddUnifyResult();

// 添加 Swagger 服务
builder.Services.AddSwaggerDocuments(builder.Configuration);

// 添加 OpenApi 服务
builder.Services.AddOpenApi(builder.Configuration);

// 添加调度管理服务，主 API 不执行调度作业
builder.Services.AddQuartzService(builder.Configuration, false);

// 添加 Swagger Newtonsoft.Json 库支持
builder.Services.AddSwaggerGenNewtonsoftSupport();

// 添加托管服务
builder.Services.AddHostedService();

WebApplication app = builder.Build();

// 启用请求头转发
app.UseForwardedHeaders(new ForwardedHeadersOptions {ForwardedHeaders = ForwardedHeaders.All});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// 启用 Body 重复读功能
app.EnableBuffering();

// 启用 WebSocket
app.UseWebSockets();

// 请求中间件
app.UseMiddleware<RequestMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseRateLimiter();

app.UseAuthorization();

// 启用集线器
app.UseMapHub();

// 启用 Swagger 文档
app.UseSwaggerDocuments();

// 配置 Swagger Knife4UI
app.UseKnife4UI(options =>
{
    options.RoutePrefix = "knife4j";
    foreach (SwaggerOpenApiInfo groupInfo in SwaggerDocumentBuilder.GetOpenApiGroups())
    {
        options.SwaggerEndpoint("/" + groupInfo.RouteTemplate, groupInfo.Title);
    }
});

app.MapControllers().RequireRateLimiting(CommonConst.GlobalApiRateLimit);

app.Run();
