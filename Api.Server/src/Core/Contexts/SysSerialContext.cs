// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections.Concurrent;
using Fast.Center.Domain;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 系统序号规则上下文
/// </summary>
[SuppressSniffer]
public class SysSerialContext
{
    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    /// 请求作用域系统序号规则缓存
    /// </summary>
    private static readonly AsyncLocal<ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialRuleModel>>
        SysSerialRuleRequestAsyncLocal = new();

    /// <summary>
    /// 系统序号规则缓存
    /// </summary>
    private static ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialRuleModel> SysSerialRuleList
    {
        get
        {
            SysSerialRuleRequestAsyncLocal.Value ??= new ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialRuleModel>();
            return SysSerialRuleRequestAsyncLocal.Value;
        }
    }

    /// <summary>
    /// 请求作用域系统序号配置缓存
    /// </summary>
    private static readonly AsyncLocal<ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialSettingModel>>
        SysSerialSettingRequestAsyncLocal = new();

    /// <summary>
    /// 系统序号配置缓存
    /// </summary>
    private static ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialSettingModel> SysSerialSettingList
    {
        get
        {
            SysSerialSettingRequestAsyncLocal.Value ??= new ConcurrentDictionary<SysSerialRuleTypeEnum, SysSerialSettingModel>();
            return SysSerialSettingRequestAsyncLocal.Value;
        }
    }

    /// <summary>
    /// 生成应用编号
    /// </summary>
    /// <returns>生成的应用编号</returns>
    public static string GenAppNo(ISqlSugarClient db)
    {
        return GenerateSerialNo(db, SysSerialRuleTypeEnum.AppNo);
    }

    /// <summary>
    /// 生成租户编号
    /// </summary>
    /// <returns>生成的租户编号</returns>
    public static string GenTenantNo(ISqlSugarClient db)
    {
        return GenerateSerialNo(db, SysSerialRuleTypeEnum.TenantNo);
    }

    /// <summary>
    /// 生成序号
    /// </summary>
    /// <returns>生成的序号</returns>
    private static string GenerateSerialNo(ISqlSugarClient db, SysSerialRuleTypeEnum ruleType)
    {
        if (!db.Ado.IsAnyTran())
        {
            throw new SqlSugarException("请保证当前上下文在事务中，才能正确的调用此方法！");
        }

        DateTime dateTime = DateTime.Now;

        // 获取序号规则配置
        SysSerialRuleModel sysSerialRuleModel = SysSerialRuleList.GetOrAdd(ruleType, key =>
        {
            SysSerialRuleModel result = db.Queryable<SysSerialRuleModel>().Where(wh => wh.RuleType == key).Single();

            if (result == null)
            {
                throw new UserFriendlyException($"未能找到【{key.GetDescription()}】规则配置！");
            }

            return result;
        });

        // 获取序号配置
        SysSerialSettingModel sysSerialSettingModel = SysSerialSettingList.GetOrAdd(ruleType, key =>
        {
            return db.Queryable<SysSerialSettingModel>().Where(wh => wh.RuleType == key).Single();
        });

        lock (_lock)
        {
            if (sysSerialSettingModel == null)
            {
                sysSerialSettingModel = new SysSerialSettingModel
                {
                    SerialSettingId = YitIdHelper.NextId(),
                    RuleType = ruleType,
                    LastSerial = null,
                    LastSerialNo = null,
                    LastTime = null
                };
                sysSerialSettingModel = db.Insertable(sysSerialSettingModel).ExecuteReturnEntity();
            }

            // 分隔符
            string spacer = sysSerialRuleModel.Spacer switch
            {
                SerialSpacerEnum.None => "",
                SerialSpacerEnum.Underscore => "_",
                SerialSpacerEnum.Hyphen => "-",
                SerialSpacerEnum.Dot => ".",
                _ => ""
            };

            string curSerialNo = $"{sysSerialRuleModel.Prefix ?? ""}{spacer}";

            long lastSerial = sysSerialSettingModel.LastSerial.GetValueOrDefault();

            switch (sysSerialRuleModel.DateType)
            {
                default:
                case SerialDateTypeEnum.Year:
                    curSerialNo += dateTime.ToString("yyyy");
                    if (sysSerialSettingModel.LastTime == null || sysSerialSettingModel.LastTime.Value.Year != dateTime.Year)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Month:
                    curSerialNo += dateTime.ToString("yyyyMM");
                    if (sysSerialSettingModel.LastTime == null
                        || sysSerialSettingModel.LastTime.Value.Year != dateTime.Year
                        || sysSerialSettingModel.LastTime.Value.Month != dateTime.Month)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Day:
                    curSerialNo += dateTime.ToString("yyyyMMdd");
                    if (sysSerialSettingModel.LastTime == null || sysSerialSettingModel.LastTime.Value.Date != dateTime.Date)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Hour:
                    curSerialNo += dateTime.ToString("yyyyMMddHH");
                    if (sysSerialSettingModel.LastTime == null
                        || sysSerialSettingModel.LastTime.Value.Year != dateTime.Year
                        || sysSerialSettingModel.LastTime.Value.Month != dateTime.Month
                        || sysSerialSettingModel.LastTime.Value.Day != dateTime.Day
                        || sysSerialSettingModel.LastTime.Value.Hour != dateTime.Hour)
                    {
                        lastSerial = 0;
                    }

                    break;
            }

            // 拼接分隔符
            curSerialNo += spacer;

            long curSerial = lastSerial + 1;

            // 组装左侧的0
            string serialLength = $"D{sysSerialRuleModel.Length}";
            curSerialNo += curSerial.ToString(serialLength);

            sysSerialSettingModel.LastSerial = curSerial;
            sysSerialSettingModel.LastSerialNo = curSerialNo;
            sysSerialSettingModel.LastTime = dateTime;


            sysSerialSettingModel = db.Updateable(sysSerialSettingModel)
                .UpdateColumns(e => new {e.LastSerial, e.LastSerialNo, e.LastTime})
                .ExecuteReturnEntity();
            SysSerialSettingList.AddOrUpdate(ruleType, sysSerialSettingModel, (_, _) => sysSerialSettingModel);

            return curSerialNo;
        }
    }
}
