// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 实时消息通知服务
/// </summary>
/// <remarks>仅向当前授权用户所属租户内在线的职员设备发送，不保留离线消息</remarks>
public interface INotificationService
{
    /// <summary>
    /// 向指定职员发送一条通知
    /// </summary>
    /// <param name="type">通知类型</param>
    /// <param name="message">通知内容</param>
    /// <param name="employeeIds">一个或多个职员Id</param>
    Task SendAsync(HubNotificationTypeEnum type, string message, params long[] employeeIds);

    /// <summary>
    /// 向指定职员发送多条通知
    /// </summary>
    /// <param name="type">通知类型</param>
    /// <param name="messages">通知内容集合</param>
    /// <param name="employeeIds">一个或多个职员Id</param>
    Task SendAsync(HubNotificationTypeEnum type, IEnumerable<string> messages, params long[] employeeIds);
}
