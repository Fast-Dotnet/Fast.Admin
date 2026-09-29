// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// Quartz 触发器表Model类
/// </summary>
[SugarTable("QRTZ_TRIGGERS", "Quartz 触发器表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex("IDX_QRTZ_T_G_J", nameof(SchedName), OrderByType.Asc, nameof(JobGroup), OrderByType.Asc, nameof(JobName),
    OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_C", nameof(SchedName), OrderByType.Asc, nameof(CalendarName), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_N_G_STATE", nameof(SchedName), OrderByType.Asc, nameof(TriggerGroup), OrderByType.Asc,
    nameof(TriggerState), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_STATE", nameof(SchedName), OrderByType.Asc, nameof(TriggerState), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_N_STATE", nameof(SchedName), OrderByType.Asc, nameof(TriggerName), OrderByType.Asc, nameof(TriggerGroup),
    OrderByType.Asc, nameof(TriggerState), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_NEXT_FIRE_TIME", nameof(SchedName), OrderByType.Asc, nameof(NextFireTime), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_NFT_ST", nameof(SchedName), OrderByType.Asc, nameof(TriggerState), OrderByType.Asc, nameof(NextFireTime),
    OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_NFT_ST_MISFIRE", nameof(SchedName), OrderByType.Asc, nameof(MisfireInstr), OrderByType.Asc,
    nameof(NextFireTime), OrderByType.Asc, nameof(TriggerState), OrderByType.Asc)]
[SugarIndex("IDX_QRTZ_T_NFT_ST_MISFIRE_GRP", nameof(SchedName), OrderByType.Asc, nameof(MisfireInstr), OrderByType.Asc,
    nameof(NextFireTime), OrderByType.Asc, nameof(TriggerGroup), OrderByType.Asc, nameof(TriggerState), OrderByType.Asc)]
public class QuartzTriggerModel : IDatabaseEntity
{
    /// <summary>
    /// 调度器名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "SCHED_NAME", ColumnDescription = "调度器名称", Length = 120, IsPrimaryKey = true)]
    public string SchedName { get; set; }

    /// <summary>
    /// 触发器名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_NAME", ColumnDescription = "触发器名称", Length = 150, IsPrimaryKey = true)]
    public string TriggerName { get; set; }

    /// <summary>
    /// 调度器分组
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_GROUP", ColumnDescription = "调度器分组", Length = 150, IsPrimaryKey = true)]
    public string TriggerGroup { get; set; }

    /// <summary>
    /// 作业名称
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "JOB_NAME", ColumnDescription = "作业名称", Length = 150)]
    public string JobName { get; set; }

    /// <summary>
    /// 作业分组
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "JOB_GROUP", ColumnDescription = "作业分组", Length = 150)]
    public string JobGroup { get; set; }

    /// <summary>
    /// 触发器描述
    /// </summary>
    [SugarColumn(ColumnName = "DESCRIPTION", ColumnDescription = "触发器描述", Length = 250)]
    public string Description { get; set; }

    /// <summary>
    /// 下次触发时间（时间戳）
    /// </summary>
    [SugarColumn(ColumnName = "NEXT_FIRE_TIME", ColumnDescription = "下次触发时间（时间戳）")]
    public long? NextFireTime { get; set; }

    /// <summary>
    /// 上次触发时间（时间戳）
    /// </summary>
    [SugarColumn(ColumnName = "PREV_FIRE_TIME", ColumnDescription = "上次触发时间（时间戳）")]
    public long? PrevFireTime { get; set; }

    /// <summary>
    /// 触发优先级
    /// </summary>
    [SugarColumn(ColumnName = "PRIORITY", ColumnDescription = "触发优先级")]
    public int? Priority { get; set; }

    /// <summary>
    /// 触发器状态
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_STATE", ColumnDescription = "触发器状态", Length = 16)]
    public string TriggerState { get; set; }

    /// <summary>
    /// 触发器类型
    /// </summary>
    /// <remarks>
    /// <para>CRON</para>
    /// <para>SIMPLE</para>
    /// <para>BLOB</para>
    /// </remarks>
    [Required]
    [SugarColumn(ColumnName = "TRIGGER_TYPE", ColumnDescription = "触发器类型", Length = 8)]
    public string TriggerType { get; set; }

    /// <summary>
    /// 开始时间（时间戳）
    /// </summary>
    [SugarColumn(ColumnName = "START_TIME", ColumnDescription = "开始时间（时间戳）")]
    public long StartTime { get; set; }

    /// <summary>
    /// 结束时间（时间戳）
    /// </summary>
    [SugarColumn(ColumnName = "END_TIME", ColumnDescription = "结束时间（时间戳）")]
    public long? EndTime { get; set; }

    /// <summary>
    /// 日历名称
    /// </summary>
    [SugarColumn(ColumnName = "CALENDAR_NAME", ColumnDescription = "日历名称", Length = 200)]
    public string CalendarName { get; set; }

    /// <summary>
    /// 错过触发时的处理策略
    /// </summary>
    [SugarColumn(ColumnName = "MISFIRE_INSTR", ColumnDescription = "错过触发时的处理策略")]
    public int? MisfireInstr { get; set; }

    /// <summary>
    /// 触发器数据
    /// </summary>
    [SugarColumn(ColumnName = "JOB_DATA", ColumnDescription = "触发器数据")]
    public byte[] JobData { get; set; }
}
