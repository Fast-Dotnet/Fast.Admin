// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.ApplicationVersion.Dto;

/// <summary>
/// 查询应用版本分页列表输入
/// </summary>
public class QueryApplicationVersionPagedInput : PagedInput
{
    /// <summary>
    /// 包类型
    /// </summary>
    public ApplicationPackageTypeEnum? PackageType { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public CommonStatusEnum? Status { get; set; }
}
