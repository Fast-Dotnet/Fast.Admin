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
using Yitter.IdGenerator;

namespace Fast.Center.Service.Table;

public partial class TableService
{
    /// <summary>
    /// 获取表格配置分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取表格配置分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Table.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<QueryTableConfigPagedOutput>> QueryTableConfigPaged(PagedInput input)
    {
        return await _tableRepository.Entities.OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryTableConfigPagedOutput
            {
                TableId = sl.TableId,
                TableKey = sl.TableKey,
                TableName = sl.TableName,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取表格配置详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取表格配置详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Table.Detail)]
    [PlatformOnly]
    public async Task<QueryTableConfigDetailOutput> QueryTableConfigDetail([Required(ErrorMessage = "表格Id不能为空")] long? tableId)
    {
        QueryTableConfigDetailOutput result = await _tableRepository.Entities.Where(wh => wh.TableId == tableId)
            .Select(sl => new QueryTableConfigDetailOutput
            {
                TableId = sl.TableId,
                TableKey = sl.TableKey,
                TableName = sl.TableName,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("添加表格配置", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Table.Add)]
    [PlatformOnly]
    public async Task AddTableConfig(AddTableConfigInput input)
    {
        // 判断表格名称是否重复
        if (await _tableRepository.AnyAsync(a => a.TableName == input.TableName))
        {
            throw new UserFriendlyException("表格名称不能重复！");
        }

        long tableId = YitIdHelper.NextId();
        var tableConfigModel = new TableConfigModel
        {
            TableId = tableId,
            TableKey = NumberUtil.IdToCodeByLong(tableId),
            TableName = input.TableName,
            Remark = input.Remark
        };

        await _tableRepository.InsertAsync(tableConfigModel);
    }

    /// <summary>
    /// 编辑表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑表格配置", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Table.Edit)]
    [PlatformOnly]
    public async Task EditTableConfig(EditTableConfigInput input)
    {
        // 判断表格名称是否重复
        if (await _tableRepository.AnyAsync(a => a.TableName == input.TableName && a.TableId != input.TableId))
        {
            throw new UserFriendlyException("表格名称不能重复！");
        }

        TableConfigModel tableConfigModel = await _tableRepository.SingleOrDefaultAsync(input.TableId);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        tableConfigModel.TableName = input.TableName;
        tableConfigModel.Remark = input.Remark;
        tableConfigModel.RowVersion = input.RowVersion;

        await _tableRepository.Updateable(tableConfigModel)
            // 避免表格同步循环问题，这里不更新时间
            .IgnoreColumns(it => new {it.UpdatedTime})
            .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("删除表格配置", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Table.Delete)]
    [PlatformOnly]
    public async Task DeleteTableConfig(TableIdInput input)
    {
        TableConfigModel tableConfigModel = await _tableRepository.SingleOrDefaultAsync(input.TableId);
        if (tableConfigModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _tableRepository.Ado.UseTranAsync(async () =>
        {
            await _columnCacheRepository.DeleteAsync(wh => wh.TableId == tableConfigModel.TableId);
            await _columnRepository.DeleteAsync(wh => wh.TableId == tableConfigModel.TableId);
            await _tableRepository.DeleteAsync(tableConfigModel);
        }, ex => throw ex);

        // 清除缓存
        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.UserTableConfigCache, tableConfigModel.TableKey, "*", "*");
        await _centerCache.DelByPatternAsync(cacheKey);
    }

    /// <summary>
    /// 复制表格配置
    /// </summary>
    [HttpPost]
    [ApiInfo("复制表格配置", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Table.Edit)]
    [PlatformOnly]
    public async Task CopyTableConfig(CopyTableConfigInput input)
    {
        // 判断表格名称是否重复
        if (await _tableRepository.AnyAsync(a => a.TableName == input.TableName))
        {
            throw new UserFriendlyException("表格名称不能重复！");
        }

        if (!await _tableRepository.AnyAsync(a => a.TableId == input.TableId))
        {
            throw new UserFriendlyException("数据不存在！");
        }

        long tableId = YitIdHelper.NextId();
        var tableConfigModel = new TableConfigModel
        {
            TableId = tableId,
            TableKey = NumberUtil.IdToCodeByLong(tableId),
            TableName = input.TableName,
            Remark = input.Remark
        };

        // 查询表格所有列
        List<TableColumnConfigModel> columnConfigList = await _columnRepository.Entities.Where(wh => wh.TableId == input.TableId)
            .OrderBy(ob => ob.Order)
            .ToListAsync();

        // 重置列Id和表格Id
        columnConfigList.ForEach(item =>
        {
            item.ColumnId = YitIdHelper.NextId();
            item.TableId = tableConfigModel.TableId;
        });

        await _tableRepository.Ado.UseTranAsync(async () =>
        {
            await _tableRepository.InsertAsync(tableConfigModel);
            await _columnRepository.InsertAsync(columnConfigList);
        }, ex => throw ex);
    }
}
