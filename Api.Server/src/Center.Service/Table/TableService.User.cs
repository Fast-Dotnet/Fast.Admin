// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Table.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Table;

public partial class TableService
{
    /// <summary>
    /// 同步用户表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("同步用户表格配置", HttpRequestActionEnum.Edit)]
    public async Task SyncUserTableConfig(SyncUserTableConfigInput input)
    {
        TableConfigModel tableConfigModel = await QueryTableConfigCache(input.TableKey);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("表格列配置不存在！");
        }

        // 获取缓存
        List<TableColumnConfigCacheModel> tableColumnCacheList =
            await QueryUserTableColumnConfigCache(tableConfigModel.TableId, tableConfigModel.TableKey);

        var columnIds = tableColumnCacheList.Select(sl => sl.ColumnId).ToList();
        var sourceColumnIds = tableConfigModel.TableColumnConfigList.Select(sl => sl.ColumnId).ToList();

        DateTime dateTime = DateTime.Now;

        // 添加的
        var addTableColumnCacheList = tableConfigModel.TableColumnConfigList.Where(wh => !columnIds.Contains(wh.ColumnId))
            .Select(sl => new TableColumnConfigCacheModel
            {
                UserId = _user.EmployeeId,
                TableId = sl.TableId,
                ColumnId = sl.ColumnId,
                Label = sl.Label,
                Fixed = sl.Fixed,
                AutoWidth = sl.AutoWidth,
                Width = sl.Width,
                SmallWidth = sl.SmallWidth,
                Order = sl.Order,
                Show = sl.Show,
                Copy = sl.Copy,
                Sortable = sl.Sortable,
                SearchLabel = sl.SearchLabel,
                SearchOrder = sl.SearchOrder,
                CreatedTime = dateTime,
                TenantId = _user.TenantId
            })
            .ToList();

        // 删除的
        var deleteTableColumnCacheList = tableColumnCacheList.Where(wh => !sourceColumnIds.Contains(wh.ColumnId)).ToList();

        // 更新的
        var sourceDict = tableConfigModel.TableColumnConfigList.ToDictionary(k => k.ColumnId);
        var updateTableColumnCacheList = tableColumnCacheList.Where(wh => sourceColumnIds.Contains(wh.ColumnId)).ToList();

        foreach (TableColumnConfigCacheModel item in updateTableColumnCacheList)
        {
            if (!sourceDict.TryGetValue(item.ColumnId, out TableColumnConfigModel sourceItem))
            {
                continue;
            }

            item.Label = sourceItem.Label;
            item.Fixed = sourceItem.Fixed;
            item.AutoWidth = sourceItem.AutoWidth;
            item.Width = sourceItem.Width;
            item.SmallWidth = sourceItem.SmallWidth;
            item.Order = sourceItem.Order;
            item.Show = sourceItem.Show;
            item.Copy = sourceItem.Copy;
            item.Sortable = sourceItem.Sortable;
            item.SearchLabel = sourceItem.SearchLabel;
            item.SearchOrder = sourceItem.SearchOrder;
            item.CreatedTime = dateTime;
        }

        await _columnCacheRepository.Ado.UseTranAsync(async () =>
        {
            await _columnCacheRepository.DeleteAsync(deleteTableColumnCacheList);
            await _columnCacheRepository.InsertAsync(addTableColumnCacheList);
            await _columnCacheRepository.UpdateAsync(updateTableColumnCacheList);
        }, ex => throw ex);

        // 删除缓存
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.UserTableConfigCache, tableConfigModel.TableKey,
            _user.TenantNo, _user.EmployeeNo);
        await _centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 保存用户表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("保存用户表格配置", HttpRequestActionEnum.Edit)]
    public async Task SaveUserTableConfig(SaveUserTableConfigInput input)
    {
        TableConfigModel tableConfigModel = await QueryTableConfigCache(input.TableKey);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("表格列配置不存在！");
        }

        // 获取缓存
        List<TableColumnConfigCacheModel> tableColumnCacheList =
            await QueryUserTableColumnConfigCache(tableConfigModel.TableId, tableConfigModel.TableKey);
        var addTableColumnCacheList = new List<TableColumnConfigCacheModel>();
        DateTime dateTime = DateTime.Now;

        // 保存的时候没有删除的
        foreach (SaveUserTableConfigInput.SaveUserTableColumnConfigDto item in input.Columns)
        {
            TableColumnConfigModel tableColumnModel =
                tableConfigModel.TableColumnConfigList.SingleOrDefault(s => s.ColumnId == item.ColumnId);
            if (tableColumnModel == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            TableColumnConfigCacheModel tableColumnCacheModel =
                tableColumnCacheList.SingleOrDefault(s => s.ColumnId == item.ColumnId);
            if (tableColumnCacheModel == null)
            {
                tableColumnCacheModel = new TableColumnConfigCacheModel
                {
                    UserId = _user.EmployeeId,
                    TableId = tableColumnModel.TableId,
                    ColumnId = tableColumnModel.ColumnId,
                    Label = string.IsNullOrWhiteSpace(item.Label) ? tableColumnModel.Label : item.Label,
                    Fixed = string.IsNullOrWhiteSpace(item.Fixed) ? tableColumnModel.Fixed : item.Fixed,
                    AutoWidth = item.AutoWidth,
                    Width = item.Width ?? tableColumnModel.Width,
                    SmallWidth = item.SmallWidth ?? tableColumnModel.SmallWidth,
                    Order = item.Order ?? tableColumnModel.Order,
                    Show = item.Show,
                    Copy = item.Copy,
                    Sortable = item.Sortable,
                    SearchLabel =
                        string.IsNullOrWhiteSpace(item.SearchLabel) ? tableColumnModel.SearchLabel : item.SearchLabel,
                    SearchOrder = item.SearchOrder ?? tableColumnModel.SearchOrder,
                    CreatedTime = dateTime,
                    TenantId = _user.TenantId
                };
                addTableColumnCacheList.Add(tableColumnCacheModel);
            }
            else
            {
                tableColumnCacheModel.Label = string.IsNullOrWhiteSpace(item.Label) ? tableColumnModel.Label : item.Label;
                tableColumnCacheModel.Fixed = string.IsNullOrWhiteSpace(item.Fixed) ? tableColumnModel.Fixed : item.Fixed;
                tableColumnCacheModel.AutoWidth = item.AutoWidth;
                tableColumnCacheModel.Width = item.Width ?? tableColumnModel.Width;
                tableColumnCacheModel.SmallWidth = item.SmallWidth ?? tableColumnModel.SmallWidth;
                tableColumnCacheModel.Order = item.Order ?? tableColumnModel.Order;
                tableColumnCacheModel.Show = item.Show;
                tableColumnCacheModel.Copy = item.Copy;
                tableColumnCacheModel.Sortable = item.Sortable;
                tableColumnCacheModel.SearchLabel = string.IsNullOrWhiteSpace(item.SearchLabel)
                    ? tableColumnModel.SearchLabel
                    : item.SearchLabel;
                tableColumnCacheModel.SearchOrder = item.SearchOrder ?? tableColumnModel.SearchOrder;
            }
        }

        await _columnCacheRepository.Ado.UseTranAsync(async () =>
        {
            await _columnCacheRepository.UpdateAsync(tableColumnCacheList);
            await _columnCacheRepository.InsertAsync(addTableColumnCacheList);
        }, ex => throw ex);

        // 删除缓存
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.UserTableConfigCache, tableConfigModel.TableKey,
            _user.TenantNo, _user.EmployeeNo);
        await _centerCache.DelAsync(cacheKey);
    }

    /// <summary>
    /// 清除用户表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("清除用户表格配置", HttpRequestActionEnum.Delete)]
    public async Task ClearUserTableConfig(SyncUserTableConfigInput input)
    {
        TableConfigModel tableConfigModel = await QueryTableConfigCache(input.TableKey);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("表格列配置不存在！");
        }

        await _columnCacheRepository.DeleteAsync(wh => wh.TableId == tableConfigModel.TableId);

        // 删除缓存
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.UserTableConfigCache, tableConfigModel.TableKey,
            _user.TenantNo, _user.EmployeeNo);
        await _centerCache.DelAsync(cacheKey);
    }
}
