// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Database.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Fast.Center.Service.Database;

/// <summary>
/// 数据库服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "database")]
[PlatformOnly]
public class DatabaseService : IDynamicApplication
{
    private readonly ISqlSugarRepository<MainDatabaseModel> _repository;
    private readonly ISqlSugarEntityService _sqlSugarEntityService;

    public DatabaseService(ISqlSugarRepository<MainDatabaseModel> repository, ISqlSugarEntityService sqlSugarEntityService)
    {
        _repository = repository;
        _sqlSugarEntityService = sqlSugarEntityService;
    }

    /// <summary>
    /// 获取数据库分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取数据库分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Database.Paged)]
    public async Task<PagedResult<QueryDatabasePagedOutput>> QueryDatabasePaged(QueryDatabasePagedInput input)
    {
        return await _repository.Entities.Includes(e => e.SlaveDatabaseList)
            .LeftJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
            .WhereIF(input.DatabaseType != null, t1 => t1.DatabaseType == input.DatabaseType)
            .WhereIF(input.DbType != null, t1 => t1.DbType == input.DbType)
            .WhereIF(input.TenantId != null, t1 => t1.TenantId == input.TenantId)
            .SelectMergeTable((t1, t2) => new QueryDatabasePagedOutput
            {
                MainId = t1.MainId,
                DatabaseType = t1.DatabaseType,
                DbType = t1.DbType,
                PublicIp = t1.PublicIp,
                IntranetIp = t1.IntranetIp,
                Port = t1.Port,
                DbName = t1.DbName,
                DbUser = t1.DbUser,
                CommandTimeOut = t1.CommandTimeOut,
                SugarSqlExecMaxSeconds = t1.SugarSqlExecMaxSeconds,
                DiffLog = t1.DiffLog,
                DisableAop = t1.DisableAop,
                IsInitialized = t1.IsInitialized,
                TenantId = t1.TenantId,
                TenantName = t2.TenantName,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .OrderByIF(input.IsOrderBy, ob => ob.TenantId, OrderByType.Desc)
            .OrderByIF(input.IsOrderBy, ob => ob.DatabaseType)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取数据库详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取数据库详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Database.Detail)]
    public async Task<QueryDatabaseDetailOutput> QueryDatabaseDetail([Required(ErrorMessage = "主库Id不能为空")] long? mainId)
    {
        QueryDatabaseDetailOutput result = await _repository.Entities.Includes(e => e.SlaveDatabaseList)
            .LeftJoin<TenantModel>((t1, t2) => t1.TenantId == t2.TenantId)
            .Where(t1 => t1.MainId == mainId)
            .Select((t1, t2) => new QueryDatabaseDetailOutput
            {
                MainId = t1.MainId,
                DatabaseType = t1.DatabaseType,
                DbType = t1.DbType,
                PublicIp = t1.PublicIp,
                IntranetIp = t1.IntranetIp,
                Port = t1.Port,
                DbName = t1.DbName,
                DbUser = t1.DbUser,
                DbPwd = t1.DbPwd,
                CustomConnectionStr = t1.CustomConnectionStr,
                CommandTimeOut = t1.CommandTimeOut,
                SugarSqlExecMaxSeconds = t1.SugarSqlExecMaxSeconds,
                DiffLog = t1.DiffLog,
                DisableAop = t1.DisableAop,
                IsInitialized = t1.IsInitialized,
                TenantId = t1.TenantId,
                TenantName = t2.TenantName,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion,
                SlaveDatabaseList = t1.SlaveDatabaseList.Select(dSl => new EditSlaveDatabaseInput
                    {
                        SlaveId = dSl.SlaveId,
                        PublicIp = dSl.PublicIp,
                        IntranetIp = dSl.IntranetIp,
                        Port = dSl.Port,
                        DbName = dSl.DbName,
                        DbUser = dSl.DbUser,
                        DbPwd = dSl.DbPwd,
                        CustomConnectionStr = dSl.CustomConnectionStr,
                        HitRate = dSl.HitRate
                    })
                    .ToList()
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加数据库
    /// </summary>
    [HttpPost]
    [ApiInfo("添加数据库", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Database.Add)]
    public async Task AddDatabase(AddDatabaseInput input)
    {
        if (await _repository.Entities.ClearFilter<IBaseTEntity>()
                .AnyAsync(a => a.TenantId == input.TenantId && a.DatabaseType == input.DatabaseType))
        {
            throw new UserFriendlyException("当前数据库类型已经存在主库信息！");
        }

        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(new ConnectionSettingsOptions
        {
            ConnectionId = DateTime.Now.ToShortTimeString(),
            DbType = input.DbType.ToDbType(),
            ServiceIp = FastContext.HostEnvironment.IsDevelopment() ? input.PublicIp : input.IntranetIp,
            Port = input.Port,
            DbName = input.DbName,
            DbUser = input.DbUser,
            DbPwd = input.DbPwd,
            CustomConnectionStr = input.CustomConnectionStr,
            CommandTimeOut = 5,
            DisableAop = true
        }));

        if (!input.IsCreateDatabase)
        {
            if (!db.Ado.IsValidConnection())
            {
                throw new UserFriendlyException("未能连接到数据库！");
            }
        }

        TenantModel tenantModel = await _repository
            .Queryable<TenantModel>()
            .Where(wh => wh.TenantId == input.TenantId)
            .SingleAsync();
        if (tenantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (input.IsCreateDatabase)
        {
            // 创建数据库
            db.DbMaintenance.CreateDatabase();
        }

        db.Close();

        bool isLog = (input.DatabaseType & (DatabaseTypeEnum.CenterLog | DatabaseTypeEnum.AdminLog)) != 0;

        var mainDatabaseModel = new MainDatabaseModel
        {
            DatabaseType = input.DatabaseType,
            DbType = input.DbType,
            PublicIp = input.PublicIp,
            IntranetIp = input.IntranetIp,
            Port = input.Port,
            DbName = input.DbName,
            DbUser = input.DbUser,
            DbPwd = input.DbPwd,
            CustomConnectionStr = input.CustomConnectionStr,
            CommandTimeOut = input.CommandTimeOut,
            SugarSqlExecMaxSeconds = input.SugarSqlExecMaxSeconds,
            DiffLog = !isLog && input.DiffLog,
            DisableAop = isLog || input.DisableAop,
            IsInitialized = false,
            TenantId = input.TenantId
        };

        await _repository.InsertAsync(mainDatabaseModel);

        // 删除缓存
        await _sqlSugarEntityService.DeleteCache(tenantModel.TenantNo, input.DatabaseType);
    }

    /// <summary>
    /// 编辑数据库
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑数据库", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Database.Edit)]
    public async Task EditDatabase(EditDatabaseInput input)
    {
        if (input.SlaveDatabaseList.Count > 0)
        {
            int totalHitRate = input.SlaveDatabaseList.Sum(s => s.HitRate);
            if (totalHitRate != 100)
            {
                throw new UserFriendlyException("从库命中率相加必须等于100");
            }
        }

        var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(new ConnectionSettingsOptions
        {
            ConnectionId = DateTime.Now.ToShortTimeString(),
            DbType = input.DbType.ToDbType(),
            ServiceIp = FastContext.HostEnvironment.IsDevelopment() ? input.PublicIp : input.IntranetIp,
            Port = input.Port,
            DbName = input.DbName,
            DbUser = input.DbUser,
            DbPwd = input.DbPwd,
            CustomConnectionStr = input.CustomConnectionStr,
            CommandTimeOut = 5,
            DisableAop = true
        }));

        if (!input.IsCreateDatabase)
        {
            if (!db.Ado.IsValidConnection())
            {
                throw new UserFriendlyException("未能连接到数据库！");
            }
        }

        MainDatabaseModel mainDatabaseModel = await _repository.Queryable<MainDatabaseModel>()
            .Includes(e => e.SlaveDatabaseList)
            .Where(wh => wh.MainId == input.MainId)
            .SingleAsync();

        if (mainDatabaseModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        TenantModel tenantModel = await _repository.Queryable<TenantModel>()
            .Where(wh => wh.TenantId == mainDatabaseModel.TenantId)
            .SingleAsync();
        if (tenantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (input.IsCreateDatabase)
        {
            // 创建数据库
            db.DbMaintenance.CreateDatabase();
        }

        db.Close();

        bool isLog = (mainDatabaseModel.DatabaseType & (DatabaseTypeEnum.CenterLog | DatabaseTypeEnum.AdminLog)) != 0;

        mainDatabaseModel.DbType = input.DbType;
        mainDatabaseModel.PublicIp = input.PublicIp;
        mainDatabaseModel.IntranetIp = input.IntranetIp;
        mainDatabaseModel.Port = input.Port;
        mainDatabaseModel.DbName = input.DbName;
        mainDatabaseModel.DbUser = input.DbUser;
        mainDatabaseModel.DbPwd = input.DbPwd;
        mainDatabaseModel.CustomConnectionStr = input.CustomConnectionStr;
        mainDatabaseModel.CommandTimeOut = input.CommandTimeOut;
        mainDatabaseModel.SugarSqlExecMaxSeconds = input.SugarSqlExecMaxSeconds;
        mainDatabaseModel.DiffLog = !isLog && input.DiffLog;
        mainDatabaseModel.DisableAop = isLog || input.DisableAop;
        mainDatabaseModel.RowVersion = input.RowVersion;

        var addSlaveDatabaseList = new List<SlaveDatabaseModel>();
        var updateSlaveDatabaseList = new List<SlaveDatabaseModel>();
        foreach (EditSlaveDatabaseInput item in input.SlaveDatabaseList)
        {
            SlaveDatabaseModel slaveDatabaseModel;
            if (item.SlaveId == null)
            {
                // 新增的
                slaveDatabaseModel = new SlaveDatabaseModel
                {
                    MainId = mainDatabaseModel.MainId,
                    PublicIp = item.PublicIp,
                    IntranetIp = item.IntranetIp,
                    Port = item.Port,
                    DbName = item.DbName,
                    DbUser = item.DbUser,
                    DbPwd = item.DbPwd,
                    CustomConnectionStr = item.CustomConnectionStr,
                    HitRate = item.HitRate
                };
                addSlaveDatabaseList.Add(slaveDatabaseModel);
            }
            else
            {
                // 更新的
                slaveDatabaseModel = mainDatabaseModel.SlaveDatabaseList.SingleOrDefault(s => s.SlaveId == item.SlaveId);
                if (slaveDatabaseModel == null)
                {
                    throw new UserFriendlyException("数据不存在！");
                }

                slaveDatabaseModel.PublicIp = item.PublicIp;
                slaveDatabaseModel.IntranetIp = item.IntranetIp;
                slaveDatabaseModel.Port = item.Port;
                slaveDatabaseModel.DbName = item.DbName;
                slaveDatabaseModel.DbUser = item.DbUser;
                slaveDatabaseModel.DbPwd = item.DbPwd;
                slaveDatabaseModel.CustomConnectionStr = item.CustomConnectionStr;
                slaveDatabaseModel.HitRate = item.HitRate;
                updateSlaveDatabaseList.Add(slaveDatabaseModel);
            }
        }

        // 删除的
        var deleteSlaveDatabaseList = mainDatabaseModel.SlaveDatabaseList
            .Where(wh => input.SlaveDatabaseList.All(a => a.SlaveId != wh.SlaveId))
            .ToList();

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(mainDatabaseModel);
            await _repository.Deleteable(deleteSlaveDatabaseList).ExecuteCommandAsync();
            await _repository.Updateable(updateSlaveDatabaseList).ExecuteCommandAsync();
            await _repository.Insertable(addSlaveDatabaseList).ExecuteCommandAsync();
        }, ex => throw ex);

        // 删除缓存
        await _sqlSugarEntityService.DeleteCache(tenantModel.TenantNo, mainDatabaseModel.DatabaseType);
    }

    /// <summary>
    /// 删除数据库
    /// </summary>
    [HttpPost]
    [ApiInfo("删除数据库", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Database.Delete)]
    public async Task DeleteDatabase(MainIdInput input)
    {
        MainDatabaseModel mainDatabaseModel = await _repository.SingleOrDefaultAsync(input.MainId);

        if (mainDatabaseModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (await _repository.Queryable<SlaveDatabaseModel>().AnyAsync(a => a.MainId == mainDatabaseModel.MainId))
        {
            throw new UserFriendlyException("还存在从库信息，无法删除！");
        }

        TenantModel tenantModel = await _repository.Queryable<TenantModel>()
            .Where(wh => wh.TenantId == mainDatabaseModel.TenantId)
            .SingleAsync();
        if (tenantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(mainDatabaseModel);

        // 删除缓存
        await _sqlSugarEntityService.DeleteCache(tenantModel.TenantNo, mainDatabaseModel.DatabaseType);
    }
}
