// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.SysSerial.Dto;

/// <summary>
/// 获取系统序号规则详情输出
/// </summary>
public class QuerySysSerialRuleDetailOutput : PagedOutput
{
    /// <summary>
    /// 序号规则Id
    /// </summary>
    public long SerialRuleId { get; set; }

    /// <summary>
    /// 规则类型
    /// </summary>
    public SysSerialRuleTypeEnum RuleType { get; set; }

    /// <summary>
    /// 前缀
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// 时间类型
    /// </summary>
    public SerialDateTypeEnum DateType { get; set; }

    /// <summary>
    /// 分隔符
    /// </summary>
    public SerialSpacerEnum Spacer { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 最后一个序号
    /// </summary>
    public long? LastSerial { get; set; }

    /// <summary>
    /// 最后一个序号编号
    /// </summary>
    public string LastSerialNo { get; set; }

    /// <summary>
    /// 最后一个序号生成时间
    /// </summary>
    public DateTime? LastTime { get; set; }
}
