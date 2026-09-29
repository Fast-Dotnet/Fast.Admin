// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 数据库初始化托管服务
/// </summary>
[Order(102)]
public class InitDatabaseHostedService : IHostedService
{
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化数据库托管服务
    /// </summary>
    public InitDatabaseHostedService(ILogger<InitDatabaseHostedService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            // 创建库
            db.DbMaintenance.CreateDatabase();

            // 查询核心表是否存在
            if (db.DbMaintenance.IsAnyTable<AccountModel>())
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
                console.WriteLine("      开始初始化数据库...");
            });

            // 获取所有不分表的Model类型
            Type[] tableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => !wh.IsSplitTable)
                .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.Center)
                .Select(sl => sl.EntityType)
                .ToArray();
            // 获取所有分表的Model类型
            Type[] splitTableTypes = SqlSugarContext.SqlSugarEntityList.Where(wh => wh.IsSplitTable)
                .Where(wh => wh.SugarDbType == null || (DatabaseTypeEnum)wh.SugarDbType == DatabaseTypeEnum.Center)
                .Select(sl => sl.EntityType)
                .ToArray();

            // 创建表
            db.CodeFirst.InitTables(tableTypes);
            db.CodeFirst.SplitTables().InitTables(splitTableTypes);

            var dateTime = new DateTime(2025, 01, 01);
            string initialAdminPassword = CryptoUtil.HashPasswordPBKDF2SHA256(CommonConst.Default.AdminPassword);

            // 表结构创建不参与数据事务；种子数据统一进入事务，失败后下次启动可以安全重试
            await db.Ado.BeginTranAsync();
            try
            {
                // 初始化系统租户
                var systemTenantModel = new TenantModel
                {
                    TenantId = CommonConst.Default.TenantId,
                    TenantNo = CommonConst.Default.TenantNo,
                    TenantCode = "Fa",
                    Status = CommonStatusEnum.Enable,
                    TenantName = "FastDotnet工作室",
                    ShortName = "Fast",
                    SpellName = "fast dotnet gong zuo shi",
                    Edition = EditionEnum.Internal,
                    AdminAccountId = CommonConst.Default.SuperAdminAccountId,
                    AdminName = "超级管理员",
                    AdminMobile = "15580001115",
                    AdminEmail = "2875616188@qq.com",
                    AdminPhone = null,
                    RobotName = "机器人",
                    TenantType = TenantTypeEnum.System,
                    LogoUrl = CommonConst.DefaultLogo,
                    AllowDeleteData = true,
                    CreatedTime = dateTime
                };
                systemTenantModel = await db.Insertable(systemTenantModel).ExecuteReturnEntityAsync();

                #region 超级管理员

                var superAdminAccountModel = new AccountModel
                {
                    AccountId = CommonConst.Default.SuperAdminAccountId,
                    AccountKey = NumberUtil.IdToCodeByLong(CommonConst.Default.SuperAdminAccountId),
                    Mobile = "15580001115",
                    Email = "2875616188@qq.com",
                    Password = initialAdminPassword,
                    NickName = "小方",
                    Avatar = CommonConst.DefaultLogo,
                    Status = CommonStatusEnum.Enable,
                    CreatedTime = dateTime
                };
                superAdminAccountModel = await db.Insertable(superAdminAccountModel).ExecuteReturnEntityAsync();

                long superAdminUserId = YitIdHelper.NextId();
                long robotUserId = YitIdHelper.NextId();
                await db.Insertable(new List<TenantUserModel>
                    {
                        new()
                        {
                            EmployeeId = superAdminUserId,
                            UserKey = NumberUtil.IdToCodeByLong(superAdminUserId),
                            AccountId = superAdminAccountModel.AccountId,
                            EmployeeNo = "SuperAdmin",
                            EmployeeName = "超级管理员",
                            IdPhoto = CommonConst.DefaultLogo,
                            DepartmentId = null,
                            DepartmentName = null,
                            UserType = UserTypeEnum.SuperAdmin,
                            Status = CommonStatusEnum.Enable,
                            CreatedTime = dateTime,
                            TenantId = systemTenantModel.TenantId
                        },
                        new()
                        {
                            EmployeeId = robotUserId,
                            UserKey = NumberUtil.IdToCodeByLong(robotUserId),
                            AccountId = -99,
                            EmployeeNo = $"{systemTenantModel.TenantCode}_Robot",
                            EmployeeName = systemTenantModel.RobotName,
                            UserType = UserTypeEnum.Robot,
                            Status = CommonStatusEnum.Disable,
                            CreatedTime = dateTime,
                            TenantId = systemTenantModel.TenantId
                        }
                    })
                    .ExecuteCommandAsync(cancellationToken);

                #endregion

                #region PasswordRecordModel

                // 初始化密码记录表
                await db.Insertable(new List<PasswordRecordModel>
                    {
                        new()
                        {
                            AccountId = superAdminAccountModel.AccountId,
                            OperationType = PasswordOperationTypeEnum.Create,
                            Type = PasswordTypeEnum.PBKDF2_SHA256,
                            Password = initialAdminPassword,
                            CreatedTime = dateTime
                        }
                    })
                    .ExecuteCommandAsync(cancellationToken);

                #endregion

                // 系统数据库
                await DatabaseSeedData.SystemDatabaseSeedData(db, systemTenantModel.TenantId, systemTenantModel.TenantCode,
                    dateTime);

                // 配置
                await ConfigSeedData.SystemConfigSeedData(db, dateTime);

                // 系统序号规则
                await SysSerialSeedData.SeedData(db);

                // 应用
                ApplicationModel applicationModel = await ApplicationSeedData.SeedData(db, dateTime);

                // 菜单
                await MenuSeedData.DefaultMenuSeedData(db, applicationModel, dateTime);

                // 提交事务
                await db.Ado.CommitTranAsync();
            }
            catch
            {
                // 回滚事务
                await db.Ado.RollbackTranAsync();
                throw;
            }

            MAppContext.ConsoleWrite(console =>
            {
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.Green;
                console.Write("info");
                console.ResetColor();
                console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine("      初始化数据库成功。");
            });

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // 核心库初始化失败时继续启动只会产生半可用实例，并把真实故障延迟到业务请求
            _logger.LogError(ex, "核心数据库初始化失败，应用停止启动。");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
