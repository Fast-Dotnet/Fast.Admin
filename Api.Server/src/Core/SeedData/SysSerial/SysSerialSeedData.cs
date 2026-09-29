// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 系统序号种子数据
/// </summary>
internal static class SysSerialSeedData
{
    /// <summary>
    /// 系统序号种子数据
    /// </summary>
    public static async Task SeedData(ISqlSugarClient db)
    {
        await db.Insertable(new List<SysSerialRuleModel>
            {
                new()
                {
                    SerialRuleId = YitIdHelper.NextId(),
                    RuleType = SysSerialRuleTypeEnum.AppNo,
                    Prefix = "App",
                    DateType = SerialDateTypeEnum.Year,
                    Spacer = SerialSpacerEnum.None,
                    Length = 2
                },
                new()
                {
                    SerialRuleId = YitIdHelper.NextId(),
                    RuleType = SysSerialRuleTypeEnum.TenantNo,
                    Prefix = "Tnt",
                    DateType = SerialDateTypeEnum.Month,
                    Spacer = SerialSpacerEnum.None,
                    Length = 2
                }
            })
            .ExecuteCommandAsync();
    }
}
