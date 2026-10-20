// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using SqlSugar;

namespace Fast.Core;

/// <summary>
/// 职员离职扩展处理接口
/// </summary>
/// <remarks>
/// 下游实现此接口后，由 Fast 依赖注入按请求作用域注册；未提供实现时跳过扩展处理。
/// 多个实现依次执行，任一实现抛出异常都会中止离职流程并回滚尚未提交的事务。
/// </remarks>
public interface IEmployeeResignedHandler : IScopedDependency
{
    /// <summary>
    /// 在职员离职信息更新后、事务提交前处理相关业务数据。
    /// </summary>
    /// <param name="db">已开启事务的当前租户业务库客户端</param>
    /// <param name="employeeId">离职职员Id</param>
    /// <remarks>
    /// 必须使用传入的 db 执行需要一同回滚的数据库操作，不得自行提交、回滚或释放该客户端。
    /// 处理失败时直接向外抛出异常，由离职流程负责回滚。
    /// </remarks>
    Task HandleAsync(ISqlSugarClient db, long employeeId);
}
