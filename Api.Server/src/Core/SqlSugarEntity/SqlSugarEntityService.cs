// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// <see cref="ISqlSugarEntityService"/> 默认实现
/// </summary>
public class SqlSugarEntityService : ISqlSugarEntityService, ISingletonDependency
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache<CenterCCL> _centerCache;

    /// <summary>
    /// 运行环境
    /// </summary>
    private readonly IHostEnvironment _hostEnvironment;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// SqlSugar 实体服务
    /// </summary>
    public SqlSugarEntityService(ICache<CenterCCL> centerCache, IHostEnvironment hostEnvironment,
        ILogger<ISqlSugarEntityService> logger)
    {
        _centerCache = centerCache;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ConnectionSettingsOptions> GetConnectionSetting(long tenantId, string tenantNo,
        DatabaseTypeEnum databaseType)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        // 优先从 HttpContext.Items 中获取
        object connectionSettingsObj =
            httpContext?.Items[
                $"{nameof(Fast)}.{nameof(SqlSugar)}.{nameof(ConnectionSettingsOptions)}.{databaseType.ToString()}"];

        if (connectionSettingsObj is ConnectionSettingsOptions connectionSettings)
        {
            return connectionSettings;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Database, tenantNo, databaseType.ToString());

        ConnectionSettingsOptions result = await _centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            MainDatabaseModel data = await db.Queryable<MainDatabaseModel>()
                .Includes(e => e.SlaveDatabaseList)
                .Where(wh => wh.TenantId == tenantId && wh.DatabaseType == databaseType)
                .SingleAsync();

            if (data == null)
            {
                string message = $"未能找到对应类型【{databaseType.ToString()}】所存在的 Database 信息！";
                _logger.LogError($"TenantId：{tenantId}；TenantNo：{tenantNo}；{message}");
                throw new UserFriendlyException(message);
            }

            return new ConnectionSettingsOptions
            {
                ConnectionId = data.MainId.ToString(),
                DbType = data.DbType.ToDbType(),
                ServiceIp = _hostEnvironment.IsDevelopment()
                    // 开发环境使用公网地址
                    ? data.PublicIp
                    // 生产环境使用内网地址
                    : data.IntranetIp,
                Port = data.Port,
                DbName = data.DbName,
                DbUser = data.DbUser,
                DbPwd = data.DbPwd,
                CustomConnectionStr = data.CustomConnectionStr,
                CommandTimeOut = data.CommandTimeOut,
                SugarSqlExecMaxSeconds = data.SugarSqlExecMaxSeconds,
                DiffLog = data.DiffLog,
                DisableAop = data.DisableAop,
                SlaveConnectionList = data.SlaveDatabaseList.Select(dSl => new SlaveConnectionInfo
                    {
                        ServiceIp = _hostEnvironment.IsDevelopment()
                            // 开发环境使用公网地址
                            ? string.IsNullOrWhiteSpace(dSl.PublicIp) ? data.PublicIp : dSl.PublicIp
                            // 生产环境使用内网地址
                            :
                            string.IsNullOrWhiteSpace(dSl.IntranetIp) ? data.IntranetIp : dSl.IntranetIp,
                        Port = dSl.Port ?? data.Port,
                        DbName = string.IsNullOrWhiteSpace(dSl.DbName) ? data.DbName : dSl.DbName,
                        DbUser = string.IsNullOrWhiteSpace(dSl.DbUser) ? data.DbUser : dSl.DbUser,
                        DbPwd = string.IsNullOrWhiteSpace(dSl.DbPwd) ? data.DbPwd : dSl.DbPwd,
                        CustomConnectionStr = data.CustomConnectionStr,
                        HitRate = dSl.HitRate
                    })
                    .ToList()
            };
        });

        if (httpContext != null)
        {
            // 放入 HttpContext.Items 中
            httpContext.Items[
                $"{nameof(Fast)}.{nameof(SqlSugar)}.{nameof(ConnectionSettingsOptions)}.{databaseType.ToString()}"] = result;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task DeleteCache(string tenantNo, DatabaseTypeEnum databaseType)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        HttpContext httpContext = FastContext.HttpContext;
        if (httpContext != null)
        {
            // 删除 HttpContext.Items 中的
            if (httpContext.Items.ContainsKey(
                    $"{nameof(Fast)}.{nameof(SqlSugar)}.{nameof(ConnectionSettingsOptions)}.{databaseType.ToString()}"))
            {
                httpContext.Items.Remove(
                    $"{nameof(Fast)}.{nameof(SqlSugar)}.{nameof(ConnectionSettingsOptions)}.{databaseType.ToString()}");
            }
        }


        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Database, tenantNo, databaseType.ToString());

        await _centerCache.DelAsync(cacheKey);
    }

    /// <inheritdoc />
    public async Task DeleteAllCache(string tenantNo)
    {
        if (string.IsNullOrWhiteSpace(tenantNo))
        {
            throw new UserFriendlyException("租户编号不能为空！");
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.Database, tenantNo, "*");
        await _centerCache.DelByPatternAsync(cacheKey);
    }
}
