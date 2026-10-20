// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Shared;

/// <summary>
/// 角色类型枚举
/// </summary>
[Flags]
[FastEnum("角色类型枚举")]
public enum RoleTypeEnum
{
    /// <summary>
    /// 默认
    /// </summary>
    [TagType(TagTypeEnum.Success)]
    [Description("默认")]
    Default = 1 << 0,

    /// <summary>
    /// 管理员
    /// </summary>
    [TagType(TagTypeEnum.Danger)]
    [Description("管理员")]
    Admin = 1 << 1,

    /// <summary>
    /// 技术
    /// </summary>
    [TagType(TagTypeEnum.Warning)]
    [Description("技术")]
    IT = 1 << 2,

    /// <summary>
    /// 人事
    /// </summary>
    [TagType(TagTypeEnum.Primary)]
    [Description("人事")]
    HR = 1 << 3,

    /// <summary>
    /// 财务
    /// </summary>
    [TagType(TagTypeEnum.Primary)]
    [Description("财务")]
    Finance = 1 << 4,

    /// <summary>
    /// 审计
    /// </summary>
    [TagType(TagTypeEnum.Primary)]
    [Description("审计")]
    Audit = 1 << 5
}
