// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 公共常量
/// </summary>
[SuppressSniffer]
public static class CommonConst
{
    /// <summary>
    /// 全局 API 限流规则
    /// </summary>
    public const string GlobalApiRateLimit = "GlobalApiRateLimit";

    /// <summary>
    /// 登录 API 限流规则
    /// </summary>
    public const string LoginApiRateLimit = "LoginApiRateLimit";

    /// <summary>
    /// 默认
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// 超级管理员账户Id
        /// </summary>
        public const long SuperAdminAccountId = 10086;

        /// <summary>
        /// 租户Id
        /// </summary>
        public const long TenantId = 18080;

        /// <summary>
        /// 租户编号
        /// </summary>
        public const string TenantNo = "Fast2018";
    }

    /// <summary>
    /// 默认头像
    /// </summary>
    public const string DefaultAvatar =
        "https://thirdwx.qlogo.cn/mmopen/vi_32/POgEwh4mIHO4nibH0KlMECNjjGxQUq24ZEaGT4poC6icRiccVGKSyXwibcPq4BWmiaIGuG1icwxaQX6grC9VemZoJ8rg/132";
}
