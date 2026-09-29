// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.CenterLog.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// Sugar实体处理
/// </summary>
public class SqlSugarEntityHandler : ISqlSugarEntityHandler
{
    /// <summary>
    /// 授权用户
    /// </summary>
    private readonly IUser _user;

    /// <summary>
    /// 业务租户
    /// </summary>
    private readonly ITenant _tenant;

    /// <summary>
    /// SqlSugar 实体服务
    /// </summary>
    private readonly ISqlSugarEntityService _sqlSugarEntityService;

    /// <summary>
    /// 请求上下文
    /// </summary>
    private readonly HttpContext _httpContext;

    /// <summary>
    /// SQL 日志专用通道
    /// </summary>
    private readonly SqlSugarLogChannel _sqlSugarLogChannel;

    /// <summary>
    /// 初始化 SqlSugar 实体处理器
    /// </summary>
    public SqlSugarEntityHandler(IUser user, ITenant tenant, ISqlSugarEntityService sqlSugarEntityService,
        IHttpContextAccessor httpContextAccessor, SqlSugarLogChannel sqlSugarLogChannel)
    {
        _user = user;
        _tenant = tenant;
        _sqlSugarEntityService = sqlSugarEntityService;
        _httpContext = httpContextAccessor.HttpContext;
        _sqlSugarLogChannel = sqlSugarLogChannel;
    }

    /// <inheritdoc />
    public async Task<ConnectionSettingsOptions> GetConnectionSettings<TEntity>(ISqlSugarClient sqlSugarClient,
        SugarDbTypeAttribute sugarDbType, Type entityType)
    {
        string databaseTypeStr = sugarDbType.Type?.ToString();
        if (string.IsNullOrWhiteSpace(databaseTypeStr))
            return null;

        DatabaseTypeEnum databaseType = Enum.Parse<DatabaseTypeEnum>(databaseTypeStr, true);

        switch (databaseType)
        {
            case DatabaseTypeEnum.Center:
                return SqlSugarContext.ConnectionSettings;
            case DatabaseTypeEnum.CenterLog:
            case DatabaseTypeEnum.Gateway:
            case DatabaseTypeEnum.Deploy:
                return await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId,
                    CommonConst.Default.TenantNo, databaseType);
            case DatabaseTypeEnum.Admin:
            case DatabaseTypeEnum.AdminLog:
                return await _sqlSugarEntityService.GetConnectionSetting(_tenant.TenantId.GetValueOrDefault(), _tenant.TenantNo, databaseType);
            default:
                throw new SqlSugarException("未知的 Database 类型！");
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(string rawSql, SugarParameter[] parameters, TimeSpan executeTime, string handlerSql)
    {
        // 获取 CenterLog 库的连接字符串配置
        ConnectionSettingsOptions connectionSetting =
            await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                DatabaseTypeEnum.CenterLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        // 组装数据
        var sqlExecutionLogModel = new SqlExecutionLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            ExecuteSeconds = executeTime.TotalSeconds,
            PureSql = handlerSql,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName
        };
        sqlExecutionLogModel.RecordCreate(_httpContext);

        // 只等待日志进入有界通道；通道满时自然施加背压，不在业务请求中等待日志数据库写入
        await _sqlSugarLogChannel.WriteAsync(connectionConfig, sqlExecutionLogModel);
    }

    /// <inheritdoc />
    public async Task ExecuteTimeoutAsync(string fileName, int fileLine, string methodName, string rawSql,
        SugarParameter[] parameters, TimeSpan executeTime, string handlerSql, string message)
    {
        // 获取 CenterLog 库的连接字符串配置
        ConnectionSettingsOptions connectionSetting =
            await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                DatabaseTypeEnum.CenterLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        // 组装数据
        var sqlTimeoutLogModel = new SqlTimeoutLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            FileName = fileName,
            FileLine = fileLine,
            MethodName = methodName,
            TimeoutSeconds = executeTime.TotalSeconds,
            PureSql = handlerSql,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName
        };
        sqlTimeoutLogModel.RecordCreate(_httpContext);

        // 只等待日志进入有界通道；通道满时自然施加背压，不在业务请求中等待日志数据库写入
        await _sqlSugarLogChannel.WriteAsync(connectionConfig, sqlTimeoutLogModel);
    }

    /// <inheritdoc />
    public async Task ExecuteDiffLogAsync(DiffType diffType, string tableName, string tableDescription, object businessData,
        List<List<DiffLogColumnInfo>> beforeColumnList, List<List<DiffLogColumnInfo>> afterColumnList, string rawSql,
        SugarParameter[] parameters, TimeSpan? executeTime, string handlerSql)
    {
        // 获取 CenterLog 库的连接字符串配置
        ConnectionSettingsOptions connectionSetting =
            await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                DatabaseTypeEnum.CenterLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        DiffLogTypeEnum diffLogType = diffType switch
        {
            DiffType.insert => DiffLogTypeEnum.Insert,
            DiffType.update => DiffLogTypeEnum.Update,
            DiffType.delete => DiffLogTypeEnum.Delete,
            _ => DiffLogTypeEnum.Unknown
        };

        // 组装数据
        var sqlDiffLogModel = new SqlDiffLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            DiffType = diffLogType,
            TableName = tableName,
            TableDescription = tableDescription,
            BeforeColumnList = beforeColumnList,
            AfterColumnList = afterColumnList,
            ExecuteSeconds = executeTime?.TotalSeconds,
            PureSql = handlerSql,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName
        };
        sqlDiffLogModel.RecordCreate(_httpContext);

        // 只等待日志进入有界通道；通道满时自然施加背压，不在业务请求中等待日志数据库写入
        await _sqlSugarLogChannel.WriteAsync(connectionConfig, sqlDiffLogModel);
    }

    /// <inheritdoc />
    public async Task ExecuteErrorAsync(string fileName, int fileLine, string methodName, string rawSql,
        SugarParameter[] parameters, string handlerSql, SqlSugarException exception)
    {
        // 获取 CenterLog 库的连接字符串配置
        ConnectionSettingsOptions connectionSetting =
            await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                DatabaseTypeEnum.CenterLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        // 组装数据
        var sqlExceptionLogModel = new SqlExceptionLogModel
        {
            RecordId = YitIdHelper.NextId(),
            AccountId = _user.AccountId,
            Mobile = _user.Mobile,
            NickName = _user.NickName,
            FileName = fileName,
            FileLine = fileLine,
            MethodName = methodName,
            Message = exception.Message,
            Source = exception.Source,
            StackTrace = exception.StackTrace,
            PureSql = handlerSql,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now,
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName
        };
        sqlExceptionLogModel.RecordCreate(_httpContext);

        // 只等待日志进入有界通道；通道满时自然施加背压，不在业务请求中等待日志数据库写入
        await _sqlSugarLogChannel.WriteAsync(connectionConfig, sqlExceptionLogModel);
    }

    /// <inheritdoc />
    public bool IsSuperAdmin()
    {
        return _user.IsSuperAdmin;
    }

    /// <inheritdoc />
    public bool IsAdmin()
    {
        return _user.IsAdmin;
    }

    /// <inheritdoc />
    public long? AssignTenantId()
    {
        return _tenant.TenantId;
    }

    /// <inheritdoc />
    public long? AssignDepartmentId()
    {
        return _user.DepartmentId;
    }

    /// <inheritdoc />
    public string AssignDepartmentName()
    {
        return _user.DepartmentName;
    }

    /// <inheritdoc />
    public long? AssignUserId()
    {
        return _user.EmployeeId;
    }

    /// <inheritdoc />
    public string AssignUserName()
    {
        return string.IsNullOrWhiteSpace(_user.EmployeeName) ? _user.NickName : _user.EmployeeName;
    }
}
