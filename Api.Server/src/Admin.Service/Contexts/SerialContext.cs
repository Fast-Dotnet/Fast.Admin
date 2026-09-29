// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections.Concurrent;
using Fast.Admin.Domain;
using Yitter.IdGenerator;

namespace Fast.Admin.Service;

/// <summary>
/// 序号规则上下文
/// </summary>
[SuppressSniffer]
public class SerialContext
{
    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    /// 请求作用域系统序号规则缓存
    /// </summary>
    private static readonly AsyncLocal<ConcurrentDictionary<SerialRuleTypeEnum, SerialRuleModel>> SerialRuleRequestAsyncLocal =
        new();

    /// <summary>
    /// 系统序号规则缓存
    /// </summary>
    private static ConcurrentDictionary<SerialRuleTypeEnum, SerialRuleModel> SerialRuleList
    {
        get
        {
            SerialRuleRequestAsyncLocal.Value ??= new ConcurrentDictionary<SerialRuleTypeEnum, SerialRuleModel>();
            return SerialRuleRequestAsyncLocal.Value;
        }
    }

    /// <summary>
    /// 请求作用域系统序号配置缓存
    /// </summary>
    private static readonly AsyncLocal<ConcurrentDictionary<SerialRuleTypeEnum, SerialSettingModel>>
        SerialSettingRequestAsyncLocal = new();

    /// <summary>
    /// 系统序号配置缓存
    /// </summary>
    private static ConcurrentDictionary<SerialRuleTypeEnum, SerialSettingModel> SerialSettingList
    {
        get
        {
            SerialSettingRequestAsyncLocal.Value ??= new ConcurrentDictionary<SerialRuleTypeEnum, SerialSettingModel>();
            return SerialSettingRequestAsyncLocal.Value;
        }
    }

    /// <summary>
    /// 生成工号
    /// </summary>
    /// <remarks>因涉及到登录，所以必须存在租户编码</remarks>
    /// <returns>生成的工号</returns>
    public static string GenEmployeeNo(ISqlSugarClient db, string tenantCode)
    {
        return GenerateSerialNo(db, SerialRuleTypeEnum.EmployeeNo, tenantCode);
    }

    /// <summary>
    /// 生成序号
    /// </summary>
    /// <returns>生成的序号</returns>
    public static string GenerateSerialNo(ISqlSugarClient db, SerialRuleTypeEnum ruleType, string tenantCode = null)
    {
        if (!db.Ado.IsAnyTran())
        {
            throw new SqlSugarException("请保证当前上下文在事务中，才能正确的调用此方法！");
        }

        DateTime dateTime = DateTime.Now;
        tenantCode ??= "";

        // 获取序号规则配置
        SerialRuleModel serialRuleModel = SerialRuleList.GetOrAdd(ruleType, key =>
        {
            SerialRuleModel result = db.Queryable<SerialRuleModel>().Where(wh => wh.RuleType == key).Single();

            if (result == null)
            {
                throw new UserFriendlyException($"未能找到【{key.GetDescription()}】规则配置！");
            }

            return result;
        });

        // 获取序号配置
        SerialSettingModel serialSettingModel = SerialSettingList.GetOrAdd(ruleType, key =>
        {
            return db.Queryable<SerialSettingModel>().Where(wh => wh.RuleType == key).Single();
        });

        lock (_lock)
        {
            if (serialSettingModel == null)
            {
                serialSettingModel = new SerialSettingModel
                {
                    SerialSettingId = YitIdHelper.NextId(),
                    RuleType = ruleType,
                    LastSerial = null,
                    LastSerialNo = null,
                    LastTime = null
                };
                serialSettingModel = db.Insertable(serialSettingModel).ExecuteReturnEntity();
            }

            // 分隔符
            string spacer = serialRuleModel.Spacer switch
            {
                SerialSpacerEnum.None => "",
                SerialSpacerEnum.Underscore => "_",
                SerialSpacerEnum.Hyphen => "-",
                SerialSpacerEnum.Dot => ".",
                _ => ""
            };

            string curSerialNo = $"{tenantCode}{serialRuleModel.Prefix}{spacer}";

            int lastSerial = serialSettingModel.LastSerial.GetValueOrDefault();

            // 组装年月日
            switch (serialRuleModel.DateType)
            {
                default:
                case SerialDateTypeEnum.Year:
                    curSerialNo += dateTime.ToString("yyyy");
                    if (serialSettingModel.LastTime == null || serialSettingModel.LastTime.Value.Year != dateTime.Year)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Month:
                    curSerialNo += dateTime.ToString("yyyyMM");
                    if (serialSettingModel.LastTime == null
                        || serialSettingModel.LastTime.Value.Year != dateTime.Year
                        || serialSettingModel.LastTime.Value.Month != dateTime.Month)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Day:
                    curSerialNo += dateTime.ToString("yyyyMMdd");
                    if (serialSettingModel.LastTime == null || serialSettingModel.LastTime.Value.Date != dateTime.Date)
                    {
                        lastSerial = 0;
                    }

                    break;
                case SerialDateTypeEnum.Hour:
                    curSerialNo += dateTime.ToString("yyyyMMddHH");
                    if (serialSettingModel.LastTime == null
                        || serialSettingModel.LastTime.Value.Year != dateTime.Year
                        || serialSettingModel.LastTime.Value.Month != dateTime.Month
                        || serialSettingModel.LastTime.Value.Day != dateTime.Day
                        || serialSettingModel.LastTime.Value.Hour != dateTime.Hour)
                    {
                        lastSerial = 0;
                    }

                    break;
            }

            // 拼接分隔符
            curSerialNo += spacer;

            int curSerial = lastSerial + 1;

            // 组装左侧的0
            string serialLength = $"D{serialRuleModel.Length}";
            curSerialNo += curSerial.ToString(serialLength);

            serialSettingModel.LastSerial = curSerial;
            serialSettingModel.LastSerialNo = curSerialNo;
            serialSettingModel.LastTime = dateTime;


            serialSettingModel = db.Updateable(serialSettingModel)
                .UpdateColumns(e => new {e.LastSerial, e.LastSerialNo, e.LastTime})
                .ExecuteReturnEntity();
            SerialSettingList.AddOrUpdate(ruleType, serialSettingModel, (_, _) => serialSettingModel);

            return curSerialNo;
        }
    }
}
