// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Center.Domain;
using Fast.Center.Service.Dictionary.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Dictionary;

/// <summary>
/// 字典服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "Dictionary")]
public class DictionaryService : IDynamicApplication
{
    private readonly ICache<CenterCCL> _centerCache;
    private readonly ISqlSugarRepository<DictionaryTypeModel> _typeRepository;
    private readonly ISqlSugarRepository<DictionaryItemModel> _itemRepository;

    public DictionaryService(ICache<CenterCCL> centerCache, ISqlSugarRepository<DictionaryTypeModel> typeRepository,
        ISqlSugarRepository<DictionaryItemModel> itemRepository)
    {
        _centerCache = centerCache;
        _typeRepository = typeRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// 获取字典
    /// </summary>
    [HttpGet]
    [ApiInfo("获取字典", HttpRequestActionEnum.Query)]
    [AllowAnonymous]
    [DisabledRequestLog]
    public async Task<Dictionary<string, List<FaTableEnumColumnCtx>>> QueryDictionary()
    {
        var dictionaryTypeList = await _centerCache.GetAndSetAsync(CacheConst.Center.Dictionary, async () =>
        {
            return await _typeRepository.Entities.Includes(e => e.DictionaryItemList)
                .Where(wh => wh.Status == CommonStatusEnum.Enable)
                .Select(sl => new
                {
                    sl.DictionaryKey,
                    sl.ValueType,
                    DictionaryItemList = sl
                        .DictionaryItemList.OrderBy(ob => ob.Order)
                        .Select(iSl => new
                        {
                            iSl.Label,
                            iSl.Value,
                            iSl.Type,
                            iSl.Order,
                            iSl.Tips,
                            iSl.Visible,
                            iSl.Status
                        })
                        .ToList()
                })
                .ToListAsync();
        });

        return dictionaryTypeList.ToDictionary(sl => sl.DictionaryKey, sl => sl.DictionaryItemList.Select(iSl =>
            {
                object localValue = sl.ValueType switch
                {
                    DictionaryValueTypeEnum.Int => int.Parse(iSl.Value),
                    DictionaryValueTypeEnum.Long => long.Parse(iSl.Value),
                    DictionaryValueTypeEnum.Boolean => iSl.Value.Equals("true", StringComparison.OrdinalIgnoreCase),
                    _ => iSl.Value
                };

                return new FaTableEnumColumnCtx
                {
                    Label = iSl.Label,
                    Value = localValue,
                    Show = iSl.Visible,
                    Disabled = iSl.Status == CommonStatusEnum.Disable,
                    Tips = iSl.Tips,
                    Type = iSl.Type.GetDescription()
                };
            })
            .ToList());
    }

    /// <summary>
    /// 字典分页选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("字典分页选择器", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Dictionary.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<ElSelectorOutput<long>>> SelectorPaged(PagedInput input)
    {
        var pagedData = await _typeRepository.Entities
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue), wh => wh.DictionaryName.Contains(input.SearchValue))
            .OrderBy(ob => ob.DictionaryName)
            .Select(sl => new {sl.DictionaryName, sl.DictionaryId, sl.ValueType})
            .ToPagedListAsync(input);

        return pagedData.ToPagedData(sl => new ElSelectorOutput<long>
        {
            Label = sl.DictionaryName, Value = sl.DictionaryId, Data = new {sl.ValueType}
        });
    }

    /// <summary>
    /// 获取字典分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取字典分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Dictionary.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<QueryDictionaryPagedOutput>> QueryDictionaryPaged(PagedInput input)
    {
        return await _typeRepository.Entities.OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryDictionaryPagedOutput
            {
                DictionaryId = sl.DictionaryId,
                DictionaryKey = sl.DictionaryKey,
                DictionaryName = sl.DictionaryName,
                ValueType = sl.ValueType,
                HasFlags = sl.HasFlags,
                Status = sl.Status,
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
    /// 获取字典详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取字典详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Dictionary.Detail)]
    [PlatformOnly]
    public async Task<QueryDictionaryDetailOutput> QueryDictionaryDetail([Required(ErrorMessage = "字典Id不能为空")] long? dictionaryId)
    {
        QueryDictionaryDetailOutput result = await _typeRepository.Entities.Includes(e => e.DictionaryItemList)
            .Where(wh => wh.DictionaryId == dictionaryId)
            .Select(sl => new QueryDictionaryDetailOutput
            {
                DictionaryId = sl.DictionaryId,
                DictionaryKey = sl.DictionaryKey,
                DictionaryName = sl.DictionaryName,
                ValueType = sl.ValueType,
                HasFlags = sl.HasFlags,
                Status = sl.Status,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion,
                DictionaryItemList = sl.DictionaryItemList.OrderBy(ob => ob.Order)
                    .Select(iSl => new EditDictionaryItemInput
                    {
                        DictionaryItemId = iSl.DictionaryItemId,
                        Label = iSl.Label,
                        Value = iSl.Value,
                        Type = iSl.Type,
                        Order = iSl.Order,
                        Tips = iSl.Tips,
                        Visible = iSl.Visible,
                        Status = iSl.Status
                    })
                    .ToList()
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加字典
    /// </summary>
    [HttpPost]
    [ApiInfo("添加字典", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Dictionary.Add)]
    [PlatformOnly]
    public async Task AddDictionary(AddDictionaryInput input)
    {
        // 判断key是否重复
        if (await _typeRepository.AnyAsync(a => a.DictionaryKey == input.DictionaryKey))
        {
            throw new UserFriendlyException("字典Key已存在！");
        }

        var dictionaryTypeModel = new DictionaryTypeModel
        {
            DictionaryId = YitIdHelper.NextId(),
            DictionaryKey = input.DictionaryKey,
            DictionaryName = input.DictionaryName,
            ValueType = input.ValueType,
            HasFlags = false,
            Status = CommonStatusEnum.Enable,
            Remark = input.Remark
        };

        var dictionaryItemList = new List<DictionaryItemModel>();
        foreach (AddDictionaryInput.AddDictionaryItemInput item in input.DictionaryItemList)
        {
            dictionaryItemList.Add(new DictionaryItemModel
            {
                DictionaryId = dictionaryTypeModel.DictionaryId,
                Label = item.Label,
                Value = item.Value,
                Type = item.Type,
                Order = item.Order,
                Tips = item.Tips,
                Visible = item.Visible,
                Status = CommonStatusEnum.Enable
            });
        }

        await _typeRepository.Ado.UseTranAsync(async () =>
        {
            await _typeRepository.InsertAsync(dictionaryTypeModel);
            await _itemRepository.InsertAsync(dictionaryItemList);
        }, ex => throw ex);

        // 删除缓存
        await _centerCache.DelAsync(CacheConst.Center.Dictionary);
    }

    /// <summary>
    /// 编辑字典
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑字典", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Dictionary.Edit)]
    [PlatformOnly]
    public async Task EditDictionary(EditDictionaryInput input)
    {
        DictionaryTypeModel dictionaryTypeModel = await _typeRepository
            .Entities.Includes(e => e.DictionaryItemList)
            .InSingleAsync(input.DictionaryId);
        if (dictionaryTypeModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        dictionaryTypeModel.DictionaryName = input.DictionaryName;
        dictionaryTypeModel.ValueType = input.ValueType;
        dictionaryTypeModel.Status = input.Status;
        dictionaryTypeModel.Remark = input.Remark;
        dictionaryTypeModel.RowVersion = input.RowVersion;

        List<DictionaryItemModel> oldDictionaryItemList = dictionaryTypeModel.DictionaryItemList ?? [];
        List<EditDictionaryItemInput> newDictionaryItemList = input.DictionaryItemList ?? [];

        // 新增的项
        var addDictionaryItemList = newDictionaryItemList.Where(wh => wh.DictionaryItemId == null)
            .Select(sl => new DictionaryItemModel
            {
                DictionaryId = dictionaryTypeModel.DictionaryId,
                Label = sl.Label,
                Value = sl.Value,
                Type = sl.Type,
                Order = sl.Order,
                Tips = sl.Tips,
                Visible = sl.Visible,
                Status = CommonStatusEnum.Enable
            })
            .ToList();

        // 更新的项
        var updateDictionaryItemList = newDictionaryItemList
            .Where(wh => oldDictionaryItemList.Any(a => a.DictionaryItemId == wh.DictionaryItemId))
            .Select(sl =>
            {
                DictionaryItemModel dictionaryItemModel =
                    oldDictionaryItemList.First(f => f.DictionaryItemId == sl.DictionaryItemId);
                dictionaryItemModel.Label = sl.Label;
                dictionaryItemModel.Value = sl.Value;
                dictionaryItemModel.Type = sl.Type;
                dictionaryItemModel.Order = sl.Order;
                dictionaryItemModel.Tips = sl.Tips;
                dictionaryItemModel.Visible = sl.Visible;
                dictionaryItemModel.Status = sl.Status;
                return dictionaryItemModel;
            })
            .ToList();

        // 删除的项
        var deleteDictionaryItemList = oldDictionaryItemList
            .Where(wh => newDictionaryItemList.All(a => a.DictionaryItemId != wh.DictionaryItemId))
            .ToList();

        await _typeRepository.Ado.UseTranAsync(async () =>
        {
            await _typeRepository.UpdateAsync(dictionaryTypeModel);
            await _itemRepository.DeleteAsync(deleteDictionaryItemList);
            await _itemRepository.UpdateAsync(updateDictionaryItemList);
            await _itemRepository.InsertAsync(addDictionaryItemList);
        }, ex => throw ex);

        // 删除缓存
        await _centerCache.DelAsync(CacheConst.Center.Dictionary);
    }

    /// <summary>
    /// 删除字典
    /// </summary>
    [HttpPost]
    [ApiInfo("删除字典", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Dictionary.Delete)]
    [PlatformOnly]
    public async Task DeleteDictionary(DictionaryIdInput input)
    {
        DictionaryTypeModel dictionaryTypeModel = await _typeRepository
            .Entities.Includes(e => e.DictionaryItemList)
            .InSingleAsync(input.DictionaryId);
        if (dictionaryTypeModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _typeRepository.Ado.UseTranAsync(async () =>
        {
            await _itemRepository.DeleteAsync(dictionaryTypeModel.DictionaryItemList);
            await _typeRepository.DeleteAsync(dictionaryTypeModel);
        }, ex => throw ex);

        // 删除缓存
        await _centerCache.DelAsync(CacheConst.Center.Dictionary);
    }
}
