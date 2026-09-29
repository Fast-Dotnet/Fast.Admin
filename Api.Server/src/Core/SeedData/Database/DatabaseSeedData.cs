// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 系统数据库种子数据
/// </summary>
public static class DatabaseSeedData
{
    /// <summary>
    /// 获取数据库名称
    /// </summary>
    private static string GetDatabaseName(bool isDevelopment, SugarDbType dbType, string dbName)
    {
        if (dbType == SugarDbType.Sqlite)
        {
            // 这里默认使用 Data 目录
            return Path.Combine("Data", $"{dbName}{(isDevelopment ? "_Dev" : "")}.db");
        }

        return $"{dbName}{(isDevelopment ? "_Dev" : "")}";
    }

    /// <summary>
    /// 系统数据库种子数据
    /// </summary>
    public static async Task SystemDatabaseSeedData(ISqlSugarClient db, long tenantId, string tenantCode, DateTime dateTime)
    {
        bool isDevelopment = FastContext.HostEnvironment.IsDevelopment();
        SugarDbType dbType = SqlSugarContext.ConnectionSettings.DbType != null
            ? SqlSugarContext.ConnectionSettings.DbType.Value.ToSugarDbType()
            : SugarDbType.Sqlite;
        await db.Insertable(new List<MainDatabaseModel>
            {
                // 初始化日志库
                new()
                {
                    MainId = YitIdHelper.NextId(),
                    DatabaseType = DatabaseTypeEnum.CenterLog,
                    DbType = dbType,
                    PublicIp = SqlSugarContext.ConnectionSettings.ServiceIp,
                    IntranetIp = "127.0.0.1",
                    Port = SqlSugarContext.ConnectionSettings.Port ?? 1433,
                    DbName = GetDatabaseName(isDevelopment, dbType, "FaCenter_Log"),
                    DbUser = SqlSugarContext.ConnectionSettings.DbUser,
                    DbPwd = SqlSugarContext.ConnectionSettings.DbPwd,
                    CommandTimeOut = SqlSugarContext.ConnectionSettings.CommandTimeOut!.Value,
                    SugarSqlExecMaxSeconds = SqlSugarContext.ConnectionSettings.SugarSqlExecMaxSeconds!.Value,
                    DiffLog = false,
                    DisableAop = true,
                    IsInitialized = true,
                    CreatedTime = dateTime,
                    TenantId = tenantId
                },
                // 初始化网关系统库
                new()
                {
                    MainId = YitIdHelper.NextId(),
                    DatabaseType = DatabaseTypeEnum.Gateway,
                    DbType = dbType,
                    PublicIp = SqlSugarContext.ConnectionSettings.ServiceIp,
                    IntranetIp = "127.0.0.1",
                    Port = SqlSugarContext.ConnectionSettings.Port ?? 1433,
                    DbName = GetDatabaseName(isDevelopment, dbType, "FaGateway"),
                    DbUser = SqlSugarContext.ConnectionSettings.DbUser,
                    DbPwd = SqlSugarContext.ConnectionSettings.DbPwd,
                    CommandTimeOut = SqlSugarContext.ConnectionSettings.CommandTimeOut!.Value,
                    SugarSqlExecMaxSeconds = SqlSugarContext.ConnectionSettings.SugarSqlExecMaxSeconds!.Value,
                    DiffLog = true,
                    DisableAop = false,
                    IsInitialized = false,
                    CreatedTime = dateTime,
                    TenantId = tenantId
                },
                // 初始化部署系统库
                new()
                {
                    MainId = YitIdHelper.NextId(),
                    DatabaseType = DatabaseTypeEnum.Deploy,
                    DbType = dbType,
                    PublicIp = SqlSugarContext.ConnectionSettings.ServiceIp,
                    IntranetIp = "127.0.0.1",
                    Port = SqlSugarContext.ConnectionSettings.Port ?? 1433,
                    DbName = GetDatabaseName(isDevelopment, dbType, "FaDeploy"),
                    DbUser = SqlSugarContext.ConnectionSettings.DbUser,
                    DbPwd = SqlSugarContext.ConnectionSettings.DbPwd,
                    CommandTimeOut = SqlSugarContext.ConnectionSettings.CommandTimeOut!.Value,
                    SugarSqlExecMaxSeconds = SqlSugarContext.ConnectionSettings.SugarSqlExecMaxSeconds!.Value,
                    DiffLog = true,
                    DisableAop = false,
                    IsInitialized = false,
                    CreatedTime = dateTime,
                    TenantId = tenantId
                },
                // 初始化业务库
                new()
                {
                    MainId = YitIdHelper.NextId(),
                    DatabaseType = DatabaseTypeEnum.Admin,
                    DbType = dbType,
                    PublicIp = SqlSugarContext.ConnectionSettings.ServiceIp,
                    IntranetIp = "127.0.0.1",
                    Port = SqlSugarContext.ConnectionSettings.Port ?? 1433,
                    DbName = GetDatabaseName(isDevelopment, dbType, "FaAdmin"),
                    DbUser = SqlSugarContext.ConnectionSettings.DbUser,
                    DbPwd = SqlSugarContext.ConnectionSettings.DbPwd,
                    CommandTimeOut = SqlSugarContext.ConnectionSettings.CommandTimeOut!.Value,
                    SugarSqlExecMaxSeconds = SqlSugarContext.ConnectionSettings.SugarSqlExecMaxSeconds!.Value,
                    DiffLog = true,
                    DisableAop = false,
                    IsInitialized = false,
                    CreatedTime = dateTime,
                    TenantId = tenantId
                },
                // 初始化业务日志库
                new()
                {
                    MainId = YitIdHelper.NextId(),
                    DatabaseType = DatabaseTypeEnum.AdminLog,
                    DbType = dbType,
                    PublicIp = SqlSugarContext.ConnectionSettings.ServiceIp,
                    IntranetIp = "127.0.0.1",
                    Port = SqlSugarContext.ConnectionSettings.Port ?? 1433,
                    DbName = GetDatabaseName(isDevelopment, dbType, "FaAdmin_Log"),
                    DbUser = SqlSugarContext.ConnectionSettings.DbUser,
                    DbPwd = SqlSugarContext.ConnectionSettings.DbPwd,
                    CommandTimeOut = SqlSugarContext.ConnectionSettings.CommandTimeOut!.Value,
                    SugarSqlExecMaxSeconds = SqlSugarContext.ConnectionSettings.SugarSqlExecMaxSeconds!.Value,
                    DiffLog = false,
                    DisableAop = true,
                    IsInitialized = false,
                    CreatedTime = dateTime,
                    TenantId = tenantId
                }
            })
            .ExecuteCommandAsync();
    }
}
