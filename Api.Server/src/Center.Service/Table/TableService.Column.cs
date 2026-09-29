// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Table.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Table;

public partial class TableService
{
    /// <summary>
    /// 获取表格列配置详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取表格列配置详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Table.Detail)]
    [PlatformOnly]
    public async Task<List<FaTableColumnCtx>> QueryTableColumnConfigDetail([Required(ErrorMessage = "表格Id不能为空")] long? tableId)
    {
        return await _columnRepository.Entities.Where(wh => wh.TableId == tableId)
            .OrderBy(ob => ob.Order)
            .Select(sl => new FaTableColumnCtx
            {
                ColumnId = sl.ColumnId,
                Prop = sl.Prop,
                Label = sl.Label,
                Fixed = sl.Fixed,
                AutoWidth = sl.AutoWidth,
                Width = sl.Width,
                SmallWidth = sl.SmallWidth,
                Order = sl.Order,
                Show = sl.Show,
                Copy = sl.Copy,
                Sortable = sl.Sortable,
                SortableField = sl.SortableField,
                Type = sl.Type,
                Link = sl.Link,
                ClickEmit = sl.ClickEmit,
                Tag = sl.Tag,
                Enum = sl.Enum,
                DateFix = sl.DateFix,
                DateFormat = sl.DateFormat,
                AuthTag = sl.AuthTag,
                DataDeleteField = sl.DataDeleteField,
                Slot = sl.Slot,
                OtherConfig = sl.OtherConfig,
                PureSearch = sl.PureSearch,
                SearchEl = sl.SearchEl,
                SearchKey = sl.SearchKey,
                SearchLabel = sl.SearchLabel,
                SearchOrder = sl.SearchOrder,
                SearchSlot = sl.SearchSlot,
                SearchConfig = sl.SearchConfig
            })
            .ToListAsync();
    }

    /// <summary>
    /// 编辑表格列配置
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑表格列配置", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Table.Edit)]
    [PlatformOnly]
    public async Task EditTableColumnConfig(EditTableColumnConfigInput input)
    {
        var columnIds = input.Columns.Where(wh => wh.ColumnId != null).Select(sl => sl.ColumnId).Distinct().ToList();

        if (columnIds.Count != input.Columns.Count(c => c.ColumnId != null))
        {
            throw new UserFriendlyException("传入的列重复！");
        }

        TableConfigModel tableConfigModel = await _tableRepository.SingleOrDefaultAsync(input.TableId);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 查询表格配置的所有列
        List<TableColumnConfigModel> tableColumnList = await _columnRepository
            .Entities.Where(wh => wh.TableId == input.TableId)
            .ToListAsync();

        // 更新的
        var updateTableColumnList = input.Columns.Where(wh => wh.ColumnId != null)
            .Select(item =>
            {
                TableColumnConfigModel tableColumnModel = tableColumnList.SingleOrDefault(s => s.ColumnId == item.ColumnId);
                if (tableColumnModel == null)
                {
                    throw new UserFriendlyException("数据不存在！");
                }

                tableColumnModel.Prop = item.Prop;
                tableColumnModel.Label = item.Label;
                tableColumnModel.Fixed = item.Fixed;
                tableColumnModel.AutoWidth = item.AutoWidth;
                tableColumnModel.Width = item.Width;
                tableColumnModel.SmallWidth = item.SmallWidth;
                tableColumnModel.Order = item.Order;
                tableColumnModel.Show = item.Show;
                tableColumnModel.Copy = item.Copy;
                tableColumnModel.Sortable = item.Sortable;
                tableColumnModel.SortableField = item.SortableField;
                tableColumnModel.Type = item.Type;
                tableColumnModel.Link = item.Link;
                tableColumnModel.ClickEmit = item.ClickEmit;
                tableColumnModel.Tag = item.Tag;
                tableColumnModel.Enum = item.Enum;
                tableColumnModel.DateFix = item.DateFix;
                tableColumnModel.DateFormat = item.DateFormat;
                tableColumnModel.AuthTag = item.AuthTag;
                tableColumnModel.DataDeleteField = item.DataDeleteField;
                tableColumnModel.Slot = item.Slot;
                tableColumnModel.OtherConfig = item.OtherConfig;
                tableColumnModel.PureSearch = item.PureSearch;
                tableColumnModel.SearchEl = item.SearchEl;
                tableColumnModel.SearchKey = item.SearchKey;
                tableColumnModel.SearchLabel = item.SearchLabel;
                tableColumnModel.SearchOrder = item.SearchOrder;
                tableColumnModel.SearchSlot = item.SearchSlot;
                tableColumnModel.SearchConfig = item.SearchConfig;

                return tableColumnModel;
            })
            .ToList();

        // 添加的
        var addTableColumnList = input.Columns.Where(wh => wh.ColumnId == null)
            .Select(sl => new TableColumnConfigModel
            {
                TableId = tableConfigModel.TableId,
                Prop = sl.Prop,
                Label = sl.Label,
                Fixed = sl.Fixed,
                AutoWidth = sl.AutoWidth,
                Width = sl.Width,
                SmallWidth = sl.SmallWidth,
                Order = sl.Order,
                Show = sl.Show,
                Copy = sl.Copy,
                Sortable = sl.Sortable,
                SortableField = sl.SortableField,
                Type = sl.Type,
                Link = sl.Link,
                ClickEmit = sl.ClickEmit,
                Tag = sl.Tag,
                Enum = sl.Enum,
                DateFix = sl.DateFix,
                DateFormat = sl.DateFormat,
                AuthTag = sl.AuthTag,
                DataDeleteField = sl.DataDeleteField,
                Slot = sl.Slot,
                OtherConfig = sl.OtherConfig,
                PureSearch = sl.PureSearch,
                SearchEl = sl.SearchEl,
                SearchKey = sl.SearchKey,
                SearchLabel = sl.SearchLabel,
                SearchOrder = sl.SearchOrder,
                SearchSlot = sl.SearchSlot,
                SearchConfig = sl.SearchConfig
            })
            .ToList();

        var deleteTableColumnList = tableColumnList.Where(wh => !columnIds.Contains(wh.ColumnId)).ToList();

        tableConfigModel.RowVersion = input.RowVersion;

        await _tableRepository.Ado.UseTranAsync(async () =>
        {
            await _tableRepository.Updateable(tableConfigModel).ExecuteCommandAsync();
            await _columnRepository.DeleteAsync(deleteTableColumnList);
            await _columnRepository.UpdateAsync(updateTableColumnList);
            await _columnRepository.InsertAsync(addTableColumnList);
        }, ex => throw ex);

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.TableConfig, tableConfigModel.TableKey);
        await _centerCache.DelAsync(cacheKey);
    }
}
