// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.TenantDatabase.Dto;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Yitter.IdGenerator;

namespace Fast.Admin.Service.TenantDatabase;

/// <summary>
/// <see cref="ITenantDatabaseService"/> 默认实现
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "tenantDatabase")]
[PlatformOnly]
public partial class TenantDatabaseService : ITenantDatabaseService, ITransientDependency, IDynamicApplication
{
    private readonly IUser _user;

    public TenantDatabaseService(IUser user)
    {
        _user = user;
    }

    /// <inheritdoc />
    [NonAction]
    public async Task InitDatabase(long tenantId, DatabaseTypeEnum databaseType)
    {
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

        TenantModel tenantModel = await db.Queryable<TenantModel>().Where(wh => wh.TenantId == tenantId).SingleAsync();

        if (tenantModel == null)
        {
            throw new UserFriendlyException("租户不存在！");
        }

        MainDatabaseModel databaseModel = await db.Queryable<MainDatabaseModel>()
            .Includes(e => e.SlaveDatabaseList)
            .Where(wh => wh.TenantId == tenantId && wh.DatabaseType == databaseType)
            .SingleAsync();
        if (databaseModel == null)
        {
            throw new UserFriendlyException($"未能找到对应类型【{databaseType.ToString()}】所存在的 Database 信息！");
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
            console.WriteLine($"      开始初始化租户【{tenantModel.TenantName}】数据库...");
        });

        // 创建库
        newDb.DbMaintenance.CreateDatabase();

        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), newDb);

        // 获取所有不分表的Model类型
        Type[] tableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => !wh.IsSplitTable)
            .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == databaseType)
            .Select(sl => sl.EntityType)
            .ToArray();
        // 获取所有分表的Model类型
        Type[] splitTableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => wh.IsSplitTable)
            .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == databaseType)
            .Select(sl => sl.EntityType)
            .ToArray();

        // 创建表
        newDb.CodeFirst.InitTables(tableTypes);
        newDb.CodeFirst.SplitTables().InitTables(splitTableTypes);

        if (databaseType == DatabaseTypeEnum.Admin)
        {
            // 查询所有系统规则序号
            List<SerialRuleModel> serialRuleList = await newDb.Queryable<SerialRuleModel>().ToListAsync();

            // 初始化工号序号
            if (serialRuleList.All(a => a.RuleType != SerialRuleTypeEnum.EmployeeNo))
            {
                await newDb.Insertable(new SerialRuleModel
                    {
                        SerialRuleId = YitIdHelper.NextId(),
                        RuleType = SerialRuleTypeEnum.EmployeeNo,
                        Prefix = null,
                        DateType = SerialDateTypeEnum.Day,
                        Spacer = SerialSpacerEnum.None,
                        Length = 2
                    })
                    .ExecuteCommandAsync();
            }

            // 初始化公司（组织架构）
            await newDb.Insertable(new OrganizationModel
                {
                    OrgId = YitIdHelper.NextId(),
                    ParentId = 0,
                    ParentIds = [0],
                    ParentNames = [],
                    OrgName = tenantModel.TenantName,
                    OrgCode = $"{tenantModel.TenantCode.ToLower()}_hq",
                    Sort = 1,
                    DataPublic = false,
                    Remark = null
                })
                .ExecuteCommandAsync();

            // 初始化租户默认角色
            await newDb.Insertable(new List<RoleModel>
                {
                    new()
                    {
                        RoleId = YitIdHelper.NextId(),
                        RoleType = RoleTypeEnum.Admin,
                        IsSystemMenu = true,
                        RoleName = "管理员",
                        RoleCode = "manager_role",
                        Sort = 1,
                        DataScopeType = DataScopeTypeEnum.All,
                        AssignableRoleIds = [],
                        Remark = null
                    },
                    new()
                    {
                        RoleId = YitIdHelper.NextId(),
                        RoleType = RoleTypeEnum.Default,
                        IsSystemMenu = true,
                        RoleName = "默认",
                        RoleCode = "default_role",
                        Sort = 5,
                        DataScopeType = DataScopeTypeEnum.Self,
                        AssignableRoleIds = [],
                        Remark = null
                    },
                    new()
                    {
                        RoleId = YitIdHelper.NextId(),
                        RoleType = RoleTypeEnum.IT,
                        IsSystemMenu = true,
                        RoleName = "技术",
                        RoleCode = "it_role",
                        Sort = 2,
                        DataScopeType = DataScopeTypeEnum.OrgWithChild,
                        AssignableRoleIds = [],
                        Remark = null
                    },
                    new()
                    {
                        RoleId = YitIdHelper.NextId(),
                        RoleType = RoleTypeEnum.HR,
                        IsSystemMenu = true,
                        RoleName = "人事",
                        RoleCode = "hr_role",
                        Sort = 3,
                        DataScopeType = DataScopeTypeEnum.DeptWithChild,
                        AssignableRoleIds = [],
                        Remark = null
                    },
                    new()
                    {
                        RoleId = YitIdHelper.NextId(),
                        RoleType = RoleTypeEnum.Finance,
                        IsSystemMenu = true,
                        RoleName = "财务",
                        RoleCode = "finance_role",
                        Sort = 4,
                        DataScopeType = DataScopeTypeEnum.DeptWithChild,
                        AssignableRoleIds = [],
                        Remark = null
                    }
                })
                .ExecuteCommandAsync();

            // 判断是否为普通租户
            if (tenantModel.TenantType == TenantTypeEnum.Common)
            {
                AccountModel accountModel = await db.Queryable<AccountModel>()
                    .Where(wh => wh.Mobile == tenantModel.AdminMobile)
                    .SingleAsync();
                if (accountModel == null)
                {
                    string passwordHash = CryptoUtil.HashPasswordPBKDF2SHA256(CommonConst.Default.Password);
                    long accountId = YitIdHelper.NextId();
                    accountModel = new AccountModel
                    {
                        AccountId = accountId,
                        AccountKey = NumberUtil.IdToCodeByLong(accountId),
                        Mobile = tenantModel.AdminMobile,
                        Email = tenantModel.AdminEmail,
                        Password = passwordHash,
                        NickName = tenantModel.AdminName,
                        Avatar = tenantModel.LogoUrl,
                        Status = CommonStatusEnum.Enable
                    };
                    accountModel = await db.Insertable(accountModel).ExecuteReturnEntityAsync();

                    #region PasswordRecordModel

                    // 初始化密码记录表
                    await db.Insertable(new List<PasswordRecordModel>
                        {
                            new()
                            {
                                AccountId = accountModel.AccountId,
                                OperationType = PasswordOperationTypeEnum.Create,
                                Type = PasswordTypeEnum.PBKDF2_SHA256,
                                Password = passwordHash
                            }
                        })
                        .ExecuteCommandAsync();

                    #endregion
                }

                // 回填管理员账号Id
                tenantModel.AdminAccountId = accountModel.AccountId;

                // 初始化租户管理员用户
                long employeeId = YitIdHelper.NextId();
                long robotEmployeeId = YitIdHelper.NextId();
                await db.Insertable(new List<TenantUserModel>
                    {
                        new()
                        {
                            EmployeeId = employeeId,
                            UserKey = NumberUtil.IdToCodeByLong(employeeId),
                            AccountId = accountModel.AccountId,
                            EmployeeNo = $"{tenantModel.TenantCode}_Admin",
                            EmployeeName = tenantModel.AdminName,
                            IdPhoto = tenantModel.LogoUrl,
                            DepartmentId = null,
                            DepartmentName = null,
                            UserType = UserTypeEnum.Admin,
                            Status = CommonStatusEnum.Enable,
                            TenantId = tenantModel.TenantId
                        },
                        new()
                        {
                            EmployeeId = robotEmployeeId,
                            UserKey = NumberUtil.IdToCodeByLong(robotEmployeeId),
                            AccountId = -99,
                            EmployeeNo = $"{tenantModel.TenantCode}_Robot",
                            EmployeeName = tenantModel.RobotName,
                            UserType = UserTypeEnum.Robot,
                            Status = CommonStatusEnum.Disable,
                            TenantId = tenantModel.TenantId
                        }
                    })
                    .ExecuteCommandAsync();
            }

            await db.Updateable(tenantModel).ExecuteCommandAsync();
        }

        await InitCustomDatabase(tenantModel, databaseType, db, newDb);

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
            console.WriteLine($"      初始化租户【{tenantModel.TenantName}】数据库成功。");
        });
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    [HttpPost]
    [ApiInfo("初始化数据库", HttpRequestActionEnum.Submit)]
    [Permission(PermissionConst.Database.Edit)]
    public async Task InitDatabase(InitDatabaseInput input)
    {
        if (_user?.IsSuperAdmin == false)
        {
            throw new UserFriendlyException("非超级管理员禁止操作！");
        }

        await InitDatabase(input.TenantId, input.DatabaseType);
    }
}
