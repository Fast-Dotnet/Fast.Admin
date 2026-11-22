// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.CenterLog.Domain;
using Fast.Core;
using Fast.NET.Core;
using Fast.Shared;
using Fast.SqlSugar;
using SqlSugar;

namespace Fast.Scheduler.LocalJob;

/// <summary>
/// 禁用连续30天未在所属租户登录的普通职员本地作业
/// </summary>
public class DisableInactiveTenantUserLocalJob : ISchedulerJob
{
    /// <inheritdoc />
    public SchedulerLocalJobInfo GetLocalJob()
    {
        return new SchedulerLocalJobInfo
        {
            JobName = "禁用长期未登录职员本地作业",
            JobGroup = SchedulerJobGroupEnum.System,
            BeginTime = new DateTime(1970, 01, 01),
            EndTime = null,
            TriggerType = TriggerTypeEnum.Cron,
            Cron = "0 5 0 * * ?",
            Week = null,
            DailyStartTime = null,
            DailyEndTime = null,
            IntervalSecond = null,
            RunTimes = null,
            WarnTime = null,
            RetryTimes = null,
            RetryMillisecond = null,
            MailMessage = MailMessageEnum.Error,
            Description = "每天00:05检查最近30天无成功登录记录的普通职员，仅禁用当前租户登录身份，不撤销会话。"
        };
    }

    /// <inheritdoc />
    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
    {
        // 进入方法的一瞬间记录时间
        DateTime dateTime = DateTime.Now;
        DateTime cutoff = dateTime.Date.AddDays(-30);
        DateTime firstMonth = new(cutoff.Year, cutoff.Month, 1);

        // 系统级作业只管理正常营业租户的普通职员，不处理平台账号和管理员身份。
        List<long> tenantIds = await db.Queryable<TenantModel>()
            .Where(t => t.Status == CommonStatusEnum.Enable && t.TenantType == TenantTypeEnum.Common)
            .Select(t => t.TenantId)
            .ToListAsync();
        if (tenantIds.Count == 0)
        {
            return "没有需要检查的普通租户。";
        }

        var employeeList = await db.Queryable<TenantUserModel>()
            .ClearFilter<IBaseTEntity>()
            .Where(t => tenantIds.Contains(t.TenantId) && t.Status == CommonStatusEnum.Enable)
            .Where(t => t.UserType == UserTypeEnum.None)
            .Where(t => t.CreatedTime != null && t.CreatedTime < cutoff)
            .Select(t => new {t.TenantId, t.EmployeeId})
            .ToListAsync();
        if (employeeList.Count == 0)
        {
            return "没有达到30天检查条件的职员。";
        }

        // 解析服务
        ISqlSugarEntityService _sqlSugarEntityService = serviceProvider.GetService<ISqlSugarEntityService>();
        // 获取 CenterLog 库的连接字符串配置
        ConnectionSettingsOptions connectionSetting =
            await _sqlSugarEntityService.GetConnectionSetting(CommonConst.Default.TenantId, CommonConst.Default.TenantNo,
                DatabaseTypeEnum.CenterLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        using var logDb = new SqlSugarClient(connectionConfig);
        // 加载Aop
        SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), logDb);

        // 仅查询最近30天覆盖的月分表。
        var tableInfos = logDb.SplitHelper<VisitLogModel>()
            .GetTables()
            .Where(t => t.Date >= firstMonth && t.Date <= dateTime)
            .ToList();

        var recentLogins = new HashSet<(long TenantId, long EmployeeId)>();
        foreach (SplitTableInfo table in tableInfos)
        {
            var rows = await logDb.Queryable<VisitLogModel>()
                .AS(table.TableName)
                .ClearFilter<IBaseTEntity>()
                .Where(t => t.VisitType == VisitTypeEnum.Login && t.CreatedTime >= cutoff && t.CreatedTime <= dateTime)
                .Select(t => new {t.TenantId, t.CreatedUserId})
                .Distinct()
                .ToListAsync();

            foreach (var row in rows)
            {
                if (row.CreatedUserId is { } employeeId)
                {
                    recentLogins.Add((row.TenantId, employeeId));
                }
            }
        }

        int disabledCount = 0;

        foreach (var employee in employeeList)
        {
            if (recentLogins.Contains((employee.TenantId, employee.EmployeeId)))
            {
                continue;
            }

            // 仅修改租户登录身份状态。条件更新保证重复执行不会把已禁用账号再次修改。
            disabledCount += await db.Updateable<TenantUserModel>()
                .SetColumns(t => new TenantUserModel {Status = CommonStatusEnum.Disable, RowVersion = t.RowVersion + 1})
                .Where(t => t.TenantId == employee.TenantId && t.EmployeeId == employee.EmployeeId)
                .Where(t => t.Status == CommonStatusEnum.Enable && t.UserType == UserTypeEnum.None)
                .Where(t => t.CreatedTime != null && t.CreatedTime < cutoff)
                .ExecuteCommandAsync();
        }

        return
            $"符合检查条件：{employeeList.Count}人；最近30天有登录：{employeeList.Count(t => recentLogins.Contains((t.TenantId, t.EmployeeId)))}人；禁用：{disabledCount}人。";
    }
}
