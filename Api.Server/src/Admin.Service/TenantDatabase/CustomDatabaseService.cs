// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.TenantDatabase;

/// <summary>
/// 租户数据库自定义初始化逻辑
/// </summary>
public partial class TenantDatabaseService
{
    /// <summary>
    /// 初始化数据库（自定义）
    /// </summary>
    [NonAction]
    public async Task InitCustomDatabase(TenantModel tenantModel, DatabaseTypeEnum databaseType, ISqlSugarClient db,
        ISqlSugarClient newDb)
    {
        switch (databaseType)
        {
            case DatabaseTypeEnum.Center:
                {
                }
                break;
            case DatabaseTypeEnum.CenterLog:
                {
                }
                break;
            case DatabaseTypeEnum.Admin:
                {
                }
                break;
            case DatabaseTypeEnum.AdminLog:
                {
                }
                break;
            case DatabaseTypeEnum.Gateway:
                {
                }
                break;
            case DatabaseTypeEnum.Deploy:
                {
                }
                break;
        }
    }
}
