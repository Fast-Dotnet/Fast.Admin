// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Region;

/// <summary>
/// 地区服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "region")]
public class RegionService : IDynamicApplication
{
    private readonly ICache<CenterCCL> _cache;
    private readonly ISqlSugarRepository<RegionModel> _repository;

    public RegionService(ICache<CenterCCL> cache, ISqlSugarRepository<RegionModel> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    /// <summary>
    /// 地区选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("地区选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> RegionSelector()
    {
        return await _cache.GetAndSetAsync(CacheConst.Center.Region, async () =>
        {
            var data = await _repository.Entities
                .Where(wh => (wh.RegionLevel & (RegionLevelEnum.Province | RegionLevelEnum.City | RegionLevelEnum.District)) != 0)
                .OrderBy(ob => ob.RegionName)
                .Select(sl => new
                {
                    sl.RegionId,
                    sl.ParentId,
                    sl.RegionCode,
                    sl.RegionName,
                    sl.AreaCode,
                    sl.PostalCode,
                    sl.Latitude,
                    sl.Longitude,
                    sl.FullRegionName
                })
                .ToListAsync();

            return data.Select(sl => new ElSelectorOutput<long>
                {
                    Value = sl.RegionId,
                    Label = sl.RegionName,
                    ParentId = sl.ParentId,
                    Data = new
                    {
                        sl.RegionCode,
                        sl.AreaCode,
                        sl.PostalCode,
                        sl.Latitude,
                        sl.Longitude,
                        sl.FullRegionName
                    }
                })
                .ToList()
                .Build();
        });
    }

    /// <summary>
    /// 省份选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("省份选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> ProvinceSelector()
    {
        return await _cache.GetAndSetAsync(CacheConst.Center.Province, async () =>
        {
            var data = await _repository.Entities.Where(wh => wh.RegionLevel == RegionLevelEnum.Province)
                .OrderBy(ob => ob.RegionName)
                .Select(sl => new
                {
                    sl.RegionId,
                    sl.ParentId,
                    sl.RegionCode,
                    sl.RegionName,
                    sl.Latitude,
                    sl.Longitude,
                    sl.FullRegionName
                })
                .ToListAsync();

            return data.Select(sl => new ElSelectorOutput<long>
                {
                    Value = sl.RegionId,
                    Label = sl.RegionName,
                    ParentId = sl.ParentId,
                    Data = new {sl.RegionCode, sl.Latitude, sl.Longitude, sl.FullRegionName}
                })
                .ToList()
                .Build();
        });
    }

    /// <summary>
    /// 城市选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("城市选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> CitySelector()
    {
        return await _cache.GetAndSetAsync(CacheConst.Center.City, async () =>
        {
            var data = await _repository.Entities
                .Where(wh => (wh.RegionLevel & (RegionLevelEnum.Province | RegionLevelEnum.City)) != 0)
                .OrderBy(ob => ob.RegionName)
                .Select(sl => new
                {
                    sl.RegionId,
                    sl.ParentId,
                    sl.RegionCode,
                    sl.RegionName,
                    sl.AreaCode,
                    sl.PostalCode,
                    sl.Latitude,
                    sl.Longitude,
                    sl.FullRegionName
                })
                .ToListAsync();

            return data.Select(sl => new ElSelectorOutput<long>
                {
                    Value = sl.RegionId,
                    Label = sl.RegionName,
                    ParentId = sl.ParentId,
                    Data = new
                    {
                        sl.RegionCode,
                        sl.AreaCode,
                        sl.PostalCode,
                        sl.Latitude,
                        sl.Longitude,
                        sl.FullRegionName
                    }
                })
                .ToList()
                .Build();
        });
    }
}
