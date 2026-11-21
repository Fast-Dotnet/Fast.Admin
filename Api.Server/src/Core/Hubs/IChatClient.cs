// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Core;

/// <summary>
/// 集线器客户端接口
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// 连接成功
    /// </summary>
    Task ConnectSuccess();

    /// <summary>
    /// 登录失败
    /// </summary>
    Task LoginFail(string message);

    /// <summary>
    /// 其他地方登录
    /// </summary>
    Task ElsewhereLogin(TenantOnlineUserModel onlineUser);

    /// <summary>
    /// 强制下线
    /// </summary>
    Task ForceOffline(ForceOfflineOutput input);

    /// <summary>
    /// 接收服务端实时消息通知
    /// </summary>
    Task ReceiveNotification(RealtimeNotificationInput input);
}
