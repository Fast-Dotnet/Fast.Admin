// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Database.Dto;

/// <summary>
/// 初始化数据库输入
/// </summary>
public class InitDatabaseInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [LongRequired(ErrorMessage = "租户Id不能为空")]
    public long TenantId { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [EnumRequired(ErrorMessage = "数据库类型")]
    public DatabaseTypeEnum DatabaseType { get; set; }
}
