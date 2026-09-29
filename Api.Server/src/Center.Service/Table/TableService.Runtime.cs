// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Table.Dto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Fast.Center.Service.Table;

public partial class TableService
{
    /// <summary>
    /// 获取表格配置缓存
    /// </summary>
    /// <returns>表格配置缓存</returns>
    internal async Task<TableConfigModel> QueryTableConfigCache(string tableKey)
    {
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.TableConfig, tableKey);

        return await _centerCache.GetAndSetAsync(cacheKey, async () =>
        {
            return await _tableRepository.Entities.Includes(e => e.TableColumnConfigList.OrderBy(ob => ob.Order).ToList())
                .Where(wh => wh.TableKey == tableKey)
                .SingleAsync();
        });
    }

    /// <summary>
    /// 获取用户表格列配置缓存
    /// </summary>
    /// <returns>当前用户的表格列配置缓存</returns>
    internal async Task<List<TableColumnConfigCacheModel>> QueryUserTableColumnConfigCache(long tableId, string tableKey)
    {
        string cacheKey =
            CacheConst.GetCacheKey(CacheConst.Center.UserTableConfigCache, tableKey, _user.TenantNo, _user.EmployeeNo);
        return await _centerCache.GetAndSetAsync(cacheKey, async () =>
               {
                   return await _columnCacheRepository.Entities
                       .Where(wh => wh.UserId == _user.EmployeeId && wh.TableId == tableId)
                       .OrderBy(ob => ob.Order)
                       .ToListAsync();
               })
               ?? [];
    }

    /// <summary>
    /// 获取表格列配置
    /// </summary>
    [HttpGet]
    [ApiInfo("获取表格列配置", HttpRequestActionEnum.Query)]
    [DisabledRequestLog]
    public async Task<QueryTableColumnConfigOutput> QueryTableColumnConfig([Required(ErrorMessage = "表格Key不能为空")] string tableKey)
    {
        TableConfigModel tableConfigModel = await QueryTableConfigCache(tableKey);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("表格列配置不存在！");
        }

        var result = new QueryTableColumnConfigOutput
        {
            TableKey = tableConfigModel.TableKey,
            Columns = new List<IDictionary<string, object>>(),
            UpdatedTime = tableConfigModel.UpdatedTime,
            Change = false,
            Cache = false
        };

        // 权限判断
        if (!_user.IsSuperAdmin)
        {
            tableConfigModel.TableColumnConfigList = tableConfigModel.TableColumnConfigList
                .Where(wh => !wh.AuthTag.Any() || wh.AuthTag.Any(a => _user.ButtonCodeList.Contains(a)))
                .ToList();
        }

        // 循环源列数据
        foreach (TableColumnConfigModel item in tableConfigModel.TableColumnConfigList)
        {
            object columnFixed = string.IsNullOrWhiteSpace(item.Fixed) ? false : item.Fixed;

            var column = new Dictionary<string, object>
            {
                {"columnId", item.ColumnId},
                {"prop", item.Prop},
                {"label", string.IsNullOrWhiteSpace(item.Label) ? null : item.Label},
                {"fixed", columnFixed},
                {"autoWidth", item.AutoWidth},
                {"width", item.Width},
                {"smallWidth", item.SmallWidth},
                {"order", item.Order},
                {"show", item.Show},
                {"copy", item.Copy},
                {"sortable", item.Sortable},
                // 如果配置原本不支持排序，则直接禁用
                {"disabledSortable", !item.Sortable},
                {"sortableField", string.IsNullOrWhiteSpace(item.SortableField) ? null : item.SortableField},
                {"type", string.IsNullOrWhiteSpace(item.Type) ? "default" : item.Type},
                {"link", item.Link},
                {"clickEmit", string.IsNullOrWhiteSpace(item.ClickEmit) ? null : item.ClickEmit},
                {"tag", item.Tag},
                {"enum", string.IsNullOrWhiteSpace(item.Enum) ? null : item.Enum},
                {"dateFix", item.DateFix},
                {"dateFormat", string.IsNullOrWhiteSpace(item.DateFormat) ? null : item.DateFormat},
                {"dataDeleteField", string.IsNullOrWhiteSpace(item.DataDeleteField) ? null : item.DataDeleteField},
                {"slot", string.IsNullOrWhiteSpace(item.Slot) ? null : item.Slot},
                {"pureSearch", item.PureSearch}
            };

            // 其他不常用配置选项
            if (item.OtherConfig?.Any() == true)
            {
                foreach (FaTableColumnAdvancedCtx oItem in item.OtherConfig)
                {
                    switch (oItem.Type)
                    {
                        default:
                        case ColumnAdvancedTypeEnum.String:
                            try
                            {
                                column.TryAdd(oItem.Prop, JToken.Parse(oItem.Value));
                            }
                            catch
                            {
                                column.TryAdd(oItem.Prop, oItem.Value);
                            }

                            break;
                        case ColumnAdvancedTypeEnum.Number:
                            column.TryAdd(oItem.Prop, oItem.Value.ParseToInt());
                            break;
                        case ColumnAdvancedTypeEnum.Boolean:
                            column.TryAdd(oItem.Prop, oItem.Value.ParseToBool());
                            break;
                        case ColumnAdvancedTypeEnum.Function:
                            column.TryAdd(oItem.Prop, oItem.Value);
                            break;
                    }
                }

                column.TryAdd("otherAdvancedConfig", item.OtherConfig.Select(sl => new {sl.Prop, sl.Type}));
            }

            // 搜素项
            if (!string.IsNullOrWhiteSpace(item.SearchEl))
            {
                var searchConfig = new Dictionary<string, object>
                {
                    {"el", item.SearchEl},
                    {"key", string.IsNullOrWhiteSpace(item.SearchKey) ? null : item.SearchKey},
                    {"label", string.IsNullOrWhiteSpace(item.SearchLabel) ? null : item.SearchLabel},
                    {"order", item.SearchOrder},
                    {"slot", string.IsNullOrWhiteSpace(item.SearchSlot) ? null : item.SearchSlot}
                };

                var searchPropsConfig = new Dictionary<string, object>();

                if (item.SearchConfig?.Any() == true)
                {
                    foreach (FaTableColumnAdvancedCtx oItem in item.SearchConfig)
                    {
                        switch (oItem.Type)
                        {
                            default:
                            case ColumnAdvancedTypeEnum.String:
                                try
                                {
                                    searchPropsConfig.TryAdd(oItem.Prop, JToken.Parse(oItem.Value));
                                }
                                catch
                                {
                                    searchPropsConfig.TryAdd(oItem.Prop, oItem.Value);
                                }

                                break;
                            case ColumnAdvancedTypeEnum.Number:
                                searchPropsConfig.TryAdd(oItem.Prop, oItem.Value.ParseToInt());
                                break;
                            case ColumnAdvancedTypeEnum.Boolean:
                                searchPropsConfig.TryAdd(oItem.Prop, oItem.Value.ParseToBool());
                                break;
                            case ColumnAdvancedTypeEnum.Function:
                                searchPropsConfig.TryAdd(oItem.Prop, oItem.Value);
                                break;
                        }
                    }

                    column.TryAdd("searchAdvancedConfig", item.SearchConfig.Select(sl => new {sl.Prop, sl.Type}));
                }

                if (searchPropsConfig.Count > 0)
                {
                    searchConfig.TryAdd("props", searchPropsConfig);
                }

                column.Add("search", searchConfig);
            }

            result.Columns.Add(column);
        }

        // 尝试获取缓存
        List<TableColumnConfigCacheModel> tableColumnCacheList =
            await QueryUserTableColumnConfigCache(tableConfigModel.TableId, tableConfigModel.TableKey);

        // 判断是否存在缓存
        if (tableColumnCacheList?.Any() == true)
        {
            result.Cache = true;
            result.UpdatedTime = tableColumnCacheList.Max(m => m.CreatedTime);
            result.Change = tableConfigModel.UpdatedTime > result.UpdatedTime;

            // 深拷贝一份
            result.CacheColumns = result.Columns.Select(IDictionary<string, object> (sl) => new Dictionary<string, object>(sl))
                .ToList();

            // 循环缓存数据
            foreach (TableColumnConfigCacheModel item in tableColumnCacheList)
            {
                int columnIdx = result.CacheColumns.FindIndex(f => $"{f["columnId"]}" == item.ColumnId.ToString());

                if (columnIdx == -1)
                {
                    continue;
                }

                result.CacheColumns[columnIdx]["label"] = string.IsNullOrWhiteSpace(item.Label) ? null : item.Label;
                result.CacheColumns[columnIdx]["fixed"] = string.IsNullOrWhiteSpace(item.Fixed) ? false : item.Fixed;
                result.CacheColumns[columnIdx]["autoWidth"] = item.AutoWidth;
                result.CacheColumns[columnIdx]["width"] = item.Width;
                result.CacheColumns[columnIdx]["smallWidth"] = item.SmallWidth;
                result.CacheColumns[columnIdx]["order"] = item.Order;
                result.CacheColumns[columnIdx]["show"] = item.Show;
                result.CacheColumns[columnIdx]["copy"] = item.Copy;
                result.CacheColumns[columnIdx]["sortable"] = item.Sortable;

                if (result.CacheColumns[columnIdx].ContainsKey("search"))
                {
                    if (result.CacheColumns[columnIdx]["search"] is JObject searchJObject)
                    {
                        var newSearchDic = new Dictionary<string, object>();
                        foreach (JProperty property in searchJObject.Properties())
                        {
                            newSearchDic.Add(property.Name, property.Value);
                        }

                        newSearchDic["label"] = string.IsNullOrWhiteSpace(item.SearchLabel) ? null : item.SearchLabel;
                        newSearchDic["order"] = item.SearchOrder;

                        result.CacheColumns[columnIdx]["search"] = JObject.FromObject(newSearchDic);
                    }
                }
            }

            result.CacheColumns = result.CacheColumns.OrderBy(ob => ob["order"]).ToList();
        }

        return result;
    }
}
