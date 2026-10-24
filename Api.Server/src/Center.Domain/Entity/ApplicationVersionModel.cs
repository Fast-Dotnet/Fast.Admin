// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用版本表Model类
/// </summary>
[SugarTable("ApplicationVersion", "应用版本表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"UX_{{table}}_{nameof(VersionCode)}", nameof(AppKey), OrderByType.Asc, nameof(PackageType), OrderByType.Asc,
    nameof(VersionCode), OrderByType.Asc, true)]
public class ApplicationVersionModel : BaseEntity, IUpdateVersion
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [SugarColumn(ColumnDescription = "记录Id", IsPrimaryKey = true)]
    public long RecordId { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string AppKey { get; set; }

    /// <summary>
    /// 包类型
    /// </summary>
    [SugarColumn(ColumnDescription = "包类型")]
    public ApplicationPackageTypeEnum PackageType { get; set; }

    /// <summary>
    /// 版本名称
    /// </summary>
    /// <remarks>用于展示，例如 1.0.1；版本大小通过 VersionCode 比较。</remarks>
    [Required]
    [SugarColumn(ColumnDescription = "版本名称", Length = 30)]
    public string VersionName { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    /// <remarks>
    /// 使用正整数递增。同一应用标识下，整包和热更新包共用版本序列；
    /// 同一目标版本的两种包使用相同版本号，并应提供一致的应用资源。
    /// </remarks>
    [SugarColumn(ColumnDescription = "版本号")]
    public int VersionCode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 包文件地址
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "包文件地址", Length = 200)]
    public string FileUrl { get; set; }

    /// <summary>
    /// 最低兼容原生版本号
    /// </summary>
    /// <remarks>
    /// WGT 热更新包发布时必须提供，包含该版本。
    /// 与客户端原生安装包版本比较，整包记录为空。
    /// </remarks>
    [SugarColumn(ColumnDescription = "最低兼容原生版本号")]
    public int? MinNativeVersionCode { get; set; }

    /// <summary>
    /// 最高兼容原生版本号
    /// </summary>
    /// <remarks>
    /// WGT 热更新包发布时必须提供，包含该版本，且不得小于最低兼容原生版本号。
    /// 与客户端原生安装包版本比较，整包记录为空。
    /// </remarks>
    [SugarColumn(ColumnDescription = "最高兼容原生版本号")]
    public int? MaxNativeVersionCode { get; set; }

    /// <summary>
    /// 更新说明
    /// </summary>
    /// <remarks>
    /// 面向用户展示的更新内容，使用纯文本，可通过换行分隔。
    /// </remarks>
    [SugarColumn(ColumnDescription = "更新说明", Length = 4000)]
    public string UpdateContent { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    /// <remarks>
    /// 客户端命中此更新版本时，要求完成更新后才能继续使用。
    /// 不表示绕过操作系统的安装确认，也不表示静默安装。
    /// </remarks>
    [SugarColumn(ColumnDescription = "是否强制更新")]
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 200)]
    public string Remark { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
