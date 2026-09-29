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
/// 删除 SQL 日志本地作业
/// </summary>
public class DeleteSqlLogLocalJob : ISchedulerJob
{
    /// <inheritdoc />
    public SchedulerLocalJobInfo GetLocalJob()
    {
        return new SchedulerLocalJobInfo
        {
            JobName = "删除Sql日志本地作业",
            JobGroup = SchedulerJobGroupEnum.System,
            BeginTime = new DateTime(1970, 01, 01),
            EndTime = null,
            TriggerType = TriggerTypeEnum.Cron,
            Cron = "0 40 1 * * ?",
            Week = null,
            DailyStartTime = null,
            DailyEndTime = null,
            IntervalSecond = null,
            RunTimes = null,
            WarnTime = null,
            RetryTimes = null,
            RetryMillisecond = null,
            MailMessage = MailMessageEnum.Error,
            Description = "删除Sql日志本地作业，每天01:40执行。"
        };
    }

    /// <inheritdoc />
    public async Task<string> Execute(IServiceProvider serviceProvider, ISqlSugarClient db, SchedulerJobLocalLogInfo logInfo)
    {
        // 进入方法的一瞬间记录时间
        DateTime dateTime = DateTime.Now;

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
        // 设置超时时间30分钟
        logDb.Ado.CommandTimeOut = 60 * 30;

        DateTime expireDate = dateTime.Date.AddDays(-30);

        int deleteCount = 0;

        var tableInfos = logDb.SplitHelper<SqlExecutionLogModel>().GetTables().OrderBy(ob => ob.Date).ToList();

        // 删除90天前的Sql执行日志
        foreach (SplitTableInfo tableInfo in tableInfos)
        {
            // 删除数据
            deleteCount += await logDb.Deleteable<SqlExecutionLogModel>()
                .AS(tableInfo.TableName)
                .Where(wh => wh.CreatedTime < expireDate)
                .ExecuteCommandAsync();

            // 查询是否不存在数据
            if (!await logDb.Queryable<SqlExecutionLogModel>().AS(tableInfo.TableName).AnyAsync())
            {
                logDb.DbMaintenance.DropTable(tableInfo.TableName);
            }
        }

        return $"删除Sql日志，共计：{deleteCount}条。";
    }
}
