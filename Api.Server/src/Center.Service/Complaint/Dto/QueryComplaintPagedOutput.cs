// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.Complaint.Dto;

/// <summary>
/// 获取投诉分页列表输出
/// </summary>
public class QueryComplaintPagedOutput
{
    /// <summary>
    /// 投诉Id
    /// </summary>
    public long ComplaintId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [SugarSearchValue]
    public string AppName { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    public string AppKey { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarSearchValue]
    public string NickName { get; set; }

    /// <summary>
    /// 投诉类型
    /// </summary>
    public ComplaintTypeEnum ComplaintType { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    public string Mobile { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [SugarSearchValue]
    public string ContactPhone { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    [SugarSearchValue]
    public string ContactEmail { get; set; }

    /// <summary>
    /// 投诉描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 附件图片
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<string> AttachmentImages { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime? HandleTime { get; set; }

    /// <summary>
    /// 处理描述
    /// </summary>
    public string HandleDescription { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarSearchTime]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// 更新版本控制字段
    /// </summary>
    public long RowVersion { get; set; }
}
