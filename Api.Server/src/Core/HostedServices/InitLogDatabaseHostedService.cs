// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.CenterLog.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 日志数据库初始化托管服务
/// </summary>
[Order(103)]
public class InitLogDatabaseHostedService : IHostedService
{
    /// <summary>
    /// SqlSugar 实体服务
    /// </summary>
    private readonly ISqlSugarEntityService _sqlSugarEntityService;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化日志数据库托管服务
    /// </summary>
    public InitLogDatabaseHostedService(ISqlSugarEntityService sqlSugarEntityService,
        ILogger<InitLogDatabaseHostedService> logger)
    {
        _sqlSugarEntityService = sqlSugarEntityService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 获取 CenterLog 库连接字符串
            ConnectionSettingsOptions connectionSettings =
                await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                    DatabaseTypeEnum.CenterLog);

            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(connectionSettings));

            // 创建库
            db.DbMaintenance.CreateDatabase();

            // 查询核心表是否存在
            if (db.DbMaintenance.IsAnyTable<ExceptionLogModel>())
            {
                return;
            }

            // 加载Aop
            SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

            MAppContext.ConsoleWrite(console =>
            {
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.Green;
                console.Write("info");
                console.ResetColor();
                console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine("      开始初始化日志数据库...");
            });

            // 获取所有不分表的Model类型
            Type[] tableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => !wh.IsSplitTable)
                .Where(wh => (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.CenterLog)
                .Select(sl => sl.EntityType)
                .ToArray();
            // 获取所有分表的Model类型
            Type[] splitTableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => wh.IsSplitTable)
                .Where(wh => (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.CenterLog)
                .Select(sl => sl.EntityType)
                .ToArray();

            // 创建表
            db.CodeFirst.InitTables(tableTypes);
            db.CodeFirst.SplitTables().InitTables(splitTableTypes);

            MAppContext.ConsoleWrite(console =>
            {
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.Green;
                console.Write("info");
                console.ResetColor();
                console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine("      初始化日志数据库成功。");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "中心日志数据库初始化失败，应用停止启动。");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
