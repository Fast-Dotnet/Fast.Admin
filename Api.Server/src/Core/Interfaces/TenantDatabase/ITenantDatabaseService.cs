// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 租户数据库服务
/// </summary>
/// <remarks>由 Admin 服务实现，调用会执行实际数据库初始化。</remarks>
public interface ITenantDatabaseService
{
    /// <summary>
    /// 初始化指定租户和数据库类型对应的数据库；已初始化时直接返回。
    /// </summary>
    /// <param name="tenantId">租户 ID</param>
    /// <param name="databaseType">需要初始化的数据库类型</param>
    /// <exception cref="UserFriendlyException">租户或对应数据库配置不存在时抛出。</exception>
    Task InitDatabase(long tenantId, DatabaseTypeEnum databaseType);
}
