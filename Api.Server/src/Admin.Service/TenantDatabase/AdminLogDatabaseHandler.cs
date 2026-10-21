// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.Extensions.Hosting;

namespace Fast.Admin.Service.TenantDatabase;

/// <summary>
/// AdminLog 数据库初始化处理器
/// </summary>
public class AdminLogDatabaseHandler : IInitTenantDatabaseHandler
{
    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public async Task HandleAsync(TenantModel tenantModel, DatabaseTypeEnum? databaseType = null)
    {
        if (databaseType != null && databaseType != DatabaseTypeEnum.AdminLog)
        {
            return;
        }

        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

        MainDatabaseModel databaseModel = await db.Queryable<MainDatabaseModel>()
            .Includes(e => e.SlaveDatabaseList)
            .Where(wh => wh.TenantId == tenantModel.TenantId && wh.DatabaseType == DatabaseTypeEnum.AdminLog)
            .SingleAsync();
        if (databaseModel == null)
        {
            throw new UserFriendlyException("未能找到对应 AdminLog 库所存在的 Database 信息！");
        }

        if (databaseModel.IsInitialized)
        {
            return;
        }

        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

        using var newDb = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(new ConnectionSettingsOptions
        {
            ConnectionId = databaseModel.MainId.ToString(),
            DbType = databaseModel.DbType.ToDbType(),
            ServiceIp = FastContext.HostEnvironment.IsDevelopment()
                // 开发环境使用公网地址
                ? databaseModel.PublicIp
                // 生产环境使用内网地址
                : databaseModel.IntranetIp,
            Port = databaseModel.Port,
            DbName = databaseModel.DbName,
            DbUser = databaseModel.DbUser,
            DbPwd = databaseModel.DbPwd,
            CustomConnectionStr = databaseModel.CustomConnectionStr,
            CommandTimeOut = databaseModel.CommandTimeOut,
            SugarSqlExecMaxSeconds = databaseModel.SugarSqlExecMaxSeconds,
            DiffLog = databaseModel.DiffLog,
            DisableAop = databaseModel.DisableAop,
            SlaveConnectionList = databaseModel.SlaveDatabaseList.Select(dSl => new SlaveConnectionInfo
                {
                    ServiceIp = FastContext.HostEnvironment.IsDevelopment()
                        // 开发环境使用公网地址
                        ? string.IsNullOrWhiteSpace(dSl.PublicIp) ? databaseModel.PublicIp : dSl.PublicIp
                        // 生产环境使用内网地址
                        :
                        string.IsNullOrWhiteSpace(dSl.IntranetIp) ? databaseModel.IntranetIp : dSl.IntranetIp,
                    Port = dSl.Port ?? databaseModel.Port,
                    DbName = string.IsNullOrWhiteSpace(dSl.DbName) ? databaseModel.DbName : dSl.DbName,
                    DbUser = string.IsNullOrWhiteSpace(dSl.DbUser) ? databaseModel.DbUser : dSl.DbUser,
                    DbPwd = string.IsNullOrWhiteSpace(dSl.DbPwd) ? databaseModel.DbPwd : dSl.DbPwd,
                    CustomConnectionStr = databaseModel.CustomConnectionStr,
                    HitRate = dSl.HitRate
                })
                .ToList()
        }));

        MAppContext.ConsoleWrite(console =>
        {
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.Green;
            console.Write("info");
            console.ResetColor();
            console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.DarkGray;
            console.WriteLine($"      开始初始化租户【{tenantModel.TenantName}】AdminLog 数据库...");
        });

        // 创建库
        newDb.DbMaintenance.CreateDatabase();

        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), newDb);

        // 获取所有不分表的Model类型
        Type[] tableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => !wh.IsSplitTable)
            .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.AdminLog)
            .Select(sl => sl.EntityType)
            .ToArray();
        // 获取所有分表的Model类型
        Type[] splitTableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => wh.IsSplitTable)
            .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.AdminLog)
            .Select(sl => sl.EntityType)
            .ToArray();

        // 创建表
        newDb.CodeFirst.InitTables(tableTypes);
        newDb.CodeFirst.SplitTables().InitTables(splitTableTypes);

        databaseModel.IsInitialized = true;
        await db.Updateable(databaseModel).ExecuteCommandAsync();

        MAppContext.ConsoleWrite(console =>
        {
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.Green;
            console.Write("info");
            console.ResetColor();
            console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.DarkGray;
            console.WriteLine($"      初始化租户【{tenantModel.TenantName}】AdminLog 数据库成功。");
        });
    }
}
