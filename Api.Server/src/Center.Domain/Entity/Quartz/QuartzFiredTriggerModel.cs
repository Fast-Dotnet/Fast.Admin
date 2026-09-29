// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// Quartz 触发器快照表Model类
/// </summary>
[SugarTable("QRTZ_FIRED_TRIGGERS", "Quartz 触发器快照表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex("IDX_QRTZ_FT_INST_JOB_REQ_RCVRY", nameof(SchedName), OrderByType.Asc, nameof(InstanceName), OrderByType.Asc,
    nameof(RequestsRecovery), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_FT_G_J", nameof(SchedName), OrderByType.Asc, nameof(JobGroup), OrderByType.Asc, nameof(JobName),
    OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_FT_G_T", nameof(SchedName), OrderByType.Asc, nameof(TriggerGroup), OrderByType.Asc, nameof(TriggerName),
    OrderByType.Asc)]
public class QuartzFiredTriggerModel : IDatabaseEntity
{
    /// <summary>
    /// 调度器名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "SCHED_NAME", ColumnDescription = "调度器名称", Length = 120, IsPrimaryKey = true)]
    public string SchedName { get; set; }

    /// <summary>
    /// 触发器实例唯一Id
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "ENTRY_ID", ColumnDescription = "触发器实例唯一Id", Length = 140, IsPrimaryKey = true)]
    public string EntryId { get; set; }

    /// <summary>
    /// 触发器名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_NAME", ColumnDescription = "触发器名称", Length = 150)]
    public string TriggerName { get; set; }

    /// <summary>
    /// 调度器分组
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_GROUP", ColumnDescription = "调度器分组", Length = 150)]
    public string TriggerGroup { get; set; }

    /// <summary>
    /// 实例名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "INSTANCE_NAME", ColumnDescription = "实例名称", Length = 150)]
    public string InstanceName { get; set; }

    /// <summary>
    /// 触发时间（毫秒）
    /// </summary>
    [SugarColumn(ColumnName = "FIRED_TIME", ColumnDescription = "触发时间（毫秒）")]
    public long FiredTime { get; set; }

    /// <summary>
    /// 调度时间（毫秒）
    /// </summary>
    [SugarColumn(ColumnName = "SCHED_TIME", ColumnDescription = "调度时间（毫秒）")]
    public long SchedTime { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    [SugarColumn(ColumnName = "PRIORITY", ColumnDescription = "优先级")]
    public int Priority { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    /// <remarks>
    /// <para>ACQUIRED</para>
    /// <para>EXECUTING</para>
    /// <para>COMPLETE</para>
    /// <para>BLOCKED </para>
    /// </remarks>
    [Required]
    [SugarColumn(ColumnName = "STATE", ColumnDescription = "状态", Length = 16)]
    public string State { get; set; }

    /// <summary>
    /// 作业名称
    /// </summary>
    [SugarColumn(ColumnName = "JOB_NAME", ColumnDescription = "作业名称", Length = 150)]
    public string JobName { get; set; }

    /// <summary>
    /// 作业分组
    /// </summary>
    [SugarColumn(ColumnName = "JOB_GROUP", ColumnDescription = "作业分组", Length = 150)]
    public string JobGroup { get; set; }

    /// <summary>
    /// 是否禁止并发执行
    /// </summary>
    [SugarColumn(ColumnName = "IS_NONCONCURRENT", ColumnDescription = "是否禁止并发执行")]
    public bool? IsNonConcurrent { get; set; }

    /// <summary>
    /// 是否请求恢复
    /// </summary>
    /// <remarks><see langword="true"/> 表示当调度器崩溃或中断后允许重新恢复执行</remarks>
    [SugarColumn(ColumnName = "REQUESTS_RECOVERY", ColumnDescription = "是否请求恢复")]
    public bool? RequestsRecovery { get; set; }
}
