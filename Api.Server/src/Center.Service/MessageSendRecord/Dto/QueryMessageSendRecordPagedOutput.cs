// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.MessageSendRecord.Dto;

/// <summary>
/// 获取消息发送记录分页列表输出
/// </summary>
public class QueryMessageSendRecordPagedOutput
{
    /// <summary>
    /// 记录Id
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public MessageSendChannelEnum Channel { get; set; }

    /// <summary>
    /// 收件人
    /// </summary>
    [SugarSearchValue]
    public string Receiver { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [SugarSearchValue]
    public string Title { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    public virtual string Device { get; set; }

    /// <summary>
    /// 操作系统（版本）
    /// </summary>
    public virtual string OS { get; set; }

    /// <summary>
    /// 浏览器（版本）
    /// </summary>
    public virtual string Browser { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public virtual string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public virtual string City { get; set; }

    /// <summary>
    /// Ip
    /// </summary>
    public virtual string Ip { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarSearchTime]
    public DateTime? CreatedTime { get; set; }
}
