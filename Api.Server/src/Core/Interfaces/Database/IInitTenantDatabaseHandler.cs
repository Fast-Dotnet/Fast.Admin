// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 初始化租户数据库处理接口
/// </summary>
/// <remarks>
/// 实现类按作用域注册，负责各自数据库的初始化；下游可增加新的实现。
/// 实现可创建数据库、初始化表结构并写入种子数据。
/// 调用方按 <see cref="Order"/> 升序依次等待执行，任一实现失败时中止后续处理并向外传播异常。
/// 各实现自行管理初始化操作的事务，多个处理器之间没有统一事务。
/// </remarks>
public interface IInitTenantDatabaseHandler : IScopedDependency
{
    /// <summary>
    /// 执行顺序
    /// </summary>
    /// <remarks>数值越小越先执行；相同值保持服务注册顺序，有前后依赖的实现应设置不同值。</remarks>
    int Order { get; }

    /// <summary>
    /// 初始化指定租户中由当前处理器负责的数据库；已初始化时直接返回。
    /// </summary>
    /// <param name="tenantId">租户Id</param>
    /// <param name="databaseType">指定时仅处理匹配的数据库类型；为 <see langword="null"/> 时处理当前实现负责的数据库。</param>
    /// <exception cref="UserFriendlyException">租户或当前实现负责的数据库配置不存在时抛出。</exception>
    Task HandleAsync(long tenantId, DatabaseTypeEnum? databaseType = null);
}
