// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Reflection;
using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 同步字典托管服务
/// </summary>
[Order(106)]
public class SyncDictionaryHostedService : IHostedService
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache<CenterCCL> _centerCache;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 同步字典托管服务
    /// </summary>
    public SyncDictionaryHostedService(ICache<CenterCCL> centerCache, ILogger<SyncDictionaryHostedService> logger)
    {
        _centerCache = centerCache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DateTime dateTime = DateTime.Now;

        string serviceName = Assembly.GetEntryAssembly()!.GetName().Name;
        var addDictionaryTypeList = new List<DictionaryTypeModel>();
        var addDictionaryItemList = new List<DictionaryItemModel>();
        var updateDictionaryTypeList = new List<DictionaryTypeModel>();
        var updateDictionaryItemList = new List<DictionaryItemModel>();
        var deleteDictionaryItemList = new List<DictionaryItemModel>();

        MAppContext.ConsoleWrite(console =>
        {
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.Green;
            console.Write("info");
            console.ResetColor();
            console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
            console.BackgroundColor = ConsoleColor.Black;
            console.ForegroundColor = ConsoleColor.DarkGray;
            console.WriteLine("      开始同步字典信息...");
        });

        try
        {
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));

            List<DictionaryTypeModel> dictionaryTypeList = await db
                .Queryable<DictionaryTypeModel>()
                .ToListAsync(cancellationToken);
            List<DictionaryItemModel> dictionaryItemList = await db
                .Queryable<DictionaryItemModel>()
                .ToListAsync(cancellationToken);

            // 获取所有带 FastEnumAttribute 特性的枚举
            var enumTypes = MAppContext.EffectiveTypes.Where(wh => wh.IsEnum)
                .Select(sl => new
                {
                    Type = sl,
                    FastEnumAttribute = sl.GetCustomAttribute<FastEnumAttribute>(),
                    FlagsAttribute = sl.GetCustomAttribute<FlagsAttribute>()
                })
                .Where(wh => wh.FastEnumAttribute != null)
                .ToList();

            if (!dictionaryTypeList.Any(a => a.DictionaryKey == "BooleanEnum" && a.ServiceName == serviceName))
            {
                long dictionaryId = YitIdHelper.NextId();
                addDictionaryTypeList.Add(new DictionaryTypeModel
                {
                    DictionaryId = dictionaryId,
                    DictionaryKey = "BooleanEnum",
                    ServiceName = serviceName,
                    DictionaryName = "Boolean类型枚举",
                    ValueType = DictionaryValueTypeEnum.Boolean,
                    HasFlags = false,
                    Status = CommonStatusEnum.Enable,
                    Remark = null,
                    CreatedTime = dateTime
                });
                addDictionaryItemList.AddRange(new List<DictionaryItemModel>
                {
                    new()
                    {
                        DictionaryItemId = YitIdHelper.NextId(),
                        DictionaryId = dictionaryId,
                        Label = "是",
                        Value = "true",
                        Type = TagTypeEnum.Success,
                        Order = 1,
                        Visible = true,
                        Status = CommonStatusEnum.Enable,
                        CreatedTime = dateTime
                    },
                    new()
                    {
                        DictionaryItemId = YitIdHelper.NextId(),
                        DictionaryId = dictionaryId,
                        Label = "否",
                        Value = "false",
                        Type = TagTypeEnum.Danger,
                        Order = 2,
                        Visible = true,
                        Status = CommonStatusEnum.Enable,
                        CreatedTime = dateTime
                    }
                });
            }

            // 循环所有枚举类型
            foreach (var enumType in enumTypes)
            {
                List<EnumItem<long>> enumItemList = enumType.Type.EnumToList<long>();

                DictionaryTypeModel dictionaryTypeInfo =
                    dictionaryTypeList.SingleOrDefault(s => s.DictionaryKey == enumType.Type.Name);

                var dictionaryTypeModel = new DictionaryTypeModel
                {
                    DictionaryKey = enumType.Type.Name,
                    ServiceName = serviceName,
                    DictionaryName =
                        enumType.FastEnumAttribute?.ChName ?? enumType.FastEnumAttribute?.EnName ?? enumType.Type.Name,
                    ValueType = Enum.GetUnderlyingType(enumType.Type) == typeof(long)
                        ? DictionaryValueTypeEnum.Long
                        : DictionaryValueTypeEnum.Int,
                    HasFlags = enumType.FlagsAttribute != null,
                    Status = CommonStatusEnum.Enable,
                    Remark = enumType.FastEnumAttribute?.Remark,
                    UpdatedTime = dateTime
                };

                if (dictionaryTypeInfo != null)
                {
                    dictionaryTypeModel.DictionaryId = dictionaryTypeInfo.DictionaryId;
                    // 不相同才修改
                    if (!dictionaryTypeInfo.Equals(dictionaryTypeModel))
                    {
                        dictionaryTypeInfo.DictionaryName = dictionaryTypeModel.DictionaryName;
                        // 这里只会存在 long 或者 int
                        dictionaryTypeInfo.ValueType = dictionaryTypeModel.ValueType;
                        dictionaryTypeInfo.HasFlags = dictionaryTypeModel.HasFlags;
                        if (string.IsNullOrWhiteSpace(dictionaryTypeInfo.Remark))
                        {
                            dictionaryTypeInfo.Remark = dictionaryTypeModel.Remark;
                        }

                        dictionaryTypeInfo.UpdatedTime = dateTime;
                        updateDictionaryTypeList.Add(dictionaryTypeInfo);
                    }

                    deleteDictionaryItemList.AddRange(dictionaryItemList
                        .Where(wh => wh.DictionaryId == dictionaryTypeInfo.DictionaryId)
                        .Where(wh => enumItemList.All(a => a.Value.ToString() != wh.Value))
                        .ToList());

                    int orderIndex = 1;

                    foreach (EnumItem<long> enumItem in enumItemList)
                    {
                        FieldInfo fieldInfo = enumType.Type.GetField(enumItem.Name);
                        TagTypeAttribute tagType = fieldInfo.GetCustomAttribute<TagTypeAttribute>();

                        var dictionaryItemModel = new DictionaryItemModel
                        {
                            DictionaryId = dictionaryTypeInfo.DictionaryId,
                            Label = enumItem.Describe ?? enumItem.Name,
                            Value = enumItem.Value.ToString(),
                            Type = tagType?.TagType ?? TagTypeEnum.Primary,
                            Order = orderIndex,
                            Visible = true,
                            Status = CommonStatusEnum.Enable,
                            UpdatedTime = dateTime
                        };

                        DictionaryItemModel dictionaryItemInfo = dictionaryItemList
                            .Where(wh => wh.DictionaryId == dictionaryTypeInfo.DictionaryId)
                            .SingleOrDefault(s => s.Value == enumItem.Value.ToString());
                        if (dictionaryItemInfo != null)
                        {
                            dictionaryItemModel.DictionaryItemId = dictionaryItemInfo.DictionaryItemId;
                            // 不相同才修改
                            if (!dictionaryItemInfo.Equals(dictionaryItemModel))
                            {
                                dictionaryItemInfo.Label = dictionaryItemModel.Label;
                                dictionaryItemInfo.Value = dictionaryItemModel.Value;
                                dictionaryItemInfo.Type = dictionaryItemModel.Type;
                                dictionaryItemInfo.Order = dictionaryItemModel.Order;
                                dictionaryItemInfo.UpdatedTime = dateTime;
                                updateDictionaryItemList.Add(dictionaryItemInfo);
                            }
                        }
                        else
                        {
                            dictionaryItemModel.DictionaryItemId = YitIdHelper.NextId();
                            dictionaryItemModel.CreatedTime = dateTime;
                            addDictionaryItemList.Add(dictionaryItemModel);
                        }

                        orderIndex++;
                    }
                }
                else
                {
                    dictionaryTypeModel.DictionaryId = YitIdHelper.NextId();
                    dictionaryTypeModel.CreatedTime = dateTime;
                    addDictionaryTypeList.Add(dictionaryTypeModel);

                    int orderIndex = 1;

                    foreach (EnumItem<long> enumItem in enumItemList)
                    {
                        FieldInfo fieldInfo = enumType.Type.GetField(enumItem.Name);
                        TagTypeAttribute tagType = fieldInfo.GetCustomAttribute<TagTypeAttribute>();

                        var dictionaryItemModel = new DictionaryItemModel
                        {
                            DictionaryItemId = YitIdHelper.NextId(),
                            DictionaryId = dictionaryTypeModel.DictionaryId,
                            Label = enumItem.Describe ?? enumItem.Name,
                            Value = enumItem.Value.ToString(),
                            Type = tagType?.TagType ?? TagTypeEnum.Primary,
                            Order = orderIndex,
                            Visible = true,
                            Status = CommonStatusEnum.Enable,
                            CreatedTime = dateTime,
                            UpdatedTime = dateTime
                        };
                        addDictionaryItemList.Add(dictionaryItemModel);

                        orderIndex++;
                    }
                }
            }

            // 加载Aop
            SugarEntityFilter.LoadSugarAop(FastContext.HostEnvironment.IsDevelopment(), db);

            if (deleteDictionaryItemList.Count > 0)
            {
                await db.Deleteable(deleteDictionaryItemList).ExecuteCommandAsync(cancellationToken);
            }

            // 只删除当前服务的
            var deleteDictionaryTypeList = dictionaryTypeList.Where(wh => wh.ServiceName == serviceName)
                .Where(wh => !enumTypes.Select(sl => sl.Type.Name).ToHashSet().Contains(wh.DictionaryKey)
                             && wh.DictionaryKey != "BooleanEnum")
                .ToList();
            if (deleteDictionaryTypeList.Count > 0)
            {
                await db.Deleteable(deleteDictionaryTypeList).ExecuteCommandAsync(cancellationToken);
            }

            await db.Updateable(updateDictionaryTypeList).ExecuteCommandAsync(cancellationToken);
            await db.Updateable(updateDictionaryItemList).ExecuteCommandAsync(cancellationToken);
            await db.Insertable(addDictionaryTypeList).ExecuteCommandAsync(cancellationToken);
            await db.Insertable(addDictionaryItemList).ExecuteCommandAsync(cancellationToken);

            // 删除缓存
            await _centerCache.DelAsync(CacheConst.Center.Dictionary);

            MAppContext.ConsoleWrite(console =>
            {
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.Green;
                console.Write("info");
                console.ResetColor();
                console.WriteLine($": {DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz dddd}");
                console.BackgroundColor = ConsoleColor.Black;
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine(
                    $"      同步字典信息成功。新增 {addDictionaryTypeList.Count}/{addDictionaryItemList.Count} 个，更新 {updateDictionaryTypeList.Count}/{updateDictionaryItemList.Count} 个，删除 {deleteDictionaryTypeList.Count}/{deleteDictionaryItemList.Count} 个。");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync dictionary error...");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
