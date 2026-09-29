// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 文件表Model类
/// </summary>
[SugarTable("File", "文件表")]
[SugarDbType(DatabaseTypeEnum.Center)]
[SugarIndex($"IX_{{table}}_{nameof(TenantId)}", nameof(TenantId), OrderByType.Asc)]
[SugarIndex($"UX_{{table}}_{nameof(FileHash)}", nameof(FileHash), OrderByType.Asc, nameof(TenantId), OrderByType.Asc, true)]
public class FileModel : IBaseTEntity
{
    /// <summary>
    /// 文件Id
    /// </summary>
    [SugarColumn(ColumnDescription = "文件Id", IsPrimaryKey = true)]
    public long FileId { get; set; }

    /// <summary>
    /// 文件唯一标识
    /// </summary>
    [SugarColumn(ColumnDescription = "文件唯一标识", Length = 255)]
    public string FileObjectName { get; set; }

    /// <summary>
    /// 原始文件名
    /// </summary>
    [SugarColumn(ColumnDescription = "原始文件名", Length = 255)]
    public string FileOriginName { get; set; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    [SugarColumn(ColumnDescription = "文件后缀", Length = 16)]
    public string FileSuffix { get; set; }

    /// <summary>
    /// 文件Mime类型
    /// </summary>
    [SugarColumn(ColumnDescription = "文件Mime类型", Length = 100)]
    public string FileMimeType { get; set; }

    /// <summary>
    /// 文件大小kb
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小kb")]
    public long FileSizeKb { get; set; }

    /// <summary>
    /// 存储路径
    /// </summary>
    [SugarColumn(ColumnDescription = "存储路径", Length = 200)]
    public string FilePath { get; set; }

    /// <summary>
    /// 访问地址
    /// </summary>
    [SugarColumn(ColumnDescription = "访问地址", Length = 200)]
    public string FileLocation { get; set; }

    /// <summary>
    /// 文件哈希
    /// </summary>
    [SugarColumn(ColumnDescription = "文件哈希", Length = 64)]
    public string FileHash { get; set; }

    /// <summary>
    /// 上传设备
    /// </summary>
    [SugarColumn(ColumnDescription = "上传设备", Length = 50)]
    public string UploadDevice { get; set; }

    /// <summary>
    /// 上传操作系统（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "上传操作系统（版本）", Length = 50)]
    public string UploadOS { get; set; }

    /// <summary>
    /// 上传浏览器（版本）
    /// </summary>
    [SugarColumn(ColumnDescription = "上传浏览器（版本）", Length = 50)]
    public string UploadBrowser { get; set; }

    /// <summary>
    /// 上传省份
    /// </summary>
    [SugarColumn(ColumnDescription = "上传省份", Length = 20)]
    public string UploadProvince { get; set; }

    /// <summary>
    /// 上传城市
    /// </summary>
    [SugarColumn(ColumnDescription = "上传城市", Length = 20)]
    public string UploadCity { get; set; }

    /// <summary>
    /// 上传Ip
    /// </summary>
    [SugarColumn(ColumnDescription = "上传Ip", Length = 15)]
    public string UploadIp { get; set; }

    /// <summary>
    /// 创建者用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者用户Id", CreateTableFieldSort = 991)]
    public long? CreatedUserId { get; set; }

    /// <summary>
    /// 创建者用户名称
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者用户名称", Length = 20, CreateTableFieldSort = 992)]
    public string CreatedUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "创建时间", CreateTableFieldSort = 993)]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", CreateTableFieldSort = 997)]
    public long TenantId { get; set; }
}
