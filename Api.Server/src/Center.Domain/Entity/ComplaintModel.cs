// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 投诉表Model类
/// </summary>
[SugarTable("Complaint", "投诉表")]
[SugarDbType(DatabaseTypeEnum.Center)]
public class ComplaintModel : IUpdateVersion
{
    /// <summary>
    /// 投诉Id
    /// </summary>
    [SugarColumn(ColumnDescription = "投诉Id", IsPrimaryKey = true)]
    public long ComplaintId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id")]
    public long AppId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [SugarColumn(ColumnDescription = "应用名称", Length = 30)]
    public string AppName { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [SugarColumn(ColumnDescription = "应用标识", Length = 50)]
    public string OpenId { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 20)]
    public string NickName { get; set; }

    /// <summary>
    /// 投诉类型
    /// </summary>
    [SugarColumn(ColumnDescription = "投诉类型")]
    public ComplaintTypeEnum ComplaintType { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarColumn(ColumnDescription = "手机", ColumnDataType = "varchar(11)")]
    public string Mobile { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "联系电话", Length = 20)]
    public string ContactPhone { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "联系邮箱", Length = 50)]
    public string ContactEmail { get; set; }

    /// <summary>
    /// 投诉描述
    /// </summary>
    [SugarColumn(ColumnDescription = "投诉描述", Length = 200)]
    public string Description { get; set; }

    /// <summary>
    /// 附件图片
    /// </summary>
    [SugarColumn(ColumnDescription = "附件图片", ColumnDataType = StaticConfig.CodeFirst_BigString, IsJson = true)]
    public List<string> AttachmentImages { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    [SugarColumn(ColumnDescription = "处理时间", CreateTableFieldSort = 996)]
    public DateTime? HandleTime { get; set; }

    /// <summary>
    /// 处理描述
    /// </summary>
    [SugarColumn(ColumnDescription = "处理描述", Length = 200)]
    public string HandleDescription { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 200)]
    public string Remark { get; set; }

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
    public long? TenantId { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    [SugarColumn(ColumnDescription = "租户名称", Length = 30)]
    public string TenantName { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    [SugarColumn(ColumnDescription = "更新版本控制字段", IsEnableUpdateVersionValidation = true, CreateTableFieldSort = 998)]
    public long RowVersion { get; set; }
}
