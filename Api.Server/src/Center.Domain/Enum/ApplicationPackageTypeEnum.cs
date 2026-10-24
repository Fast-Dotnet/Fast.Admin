// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Domain;

/// <summary>
/// 应用包类型枚举
/// </summary>
[FastEnum("应用包类型枚举")]
public enum ApplicationPackageTypeEnum
{
    /// <summary>
    /// 整包
    /// </summary>
    [Description("整包")]
    FullPackage = 1,

    /// <summary>
    /// WGT 热更新包
    /// </summary>
    [Description("热更新")]
    Wgt = 2
}
