// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Api;

/// <summary>
/// API 服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "api")]
[PlatformOnly]
public class ApiService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ApiInfoModel> _repository;

    public ApiService(ISqlSugarRepository<ApiInfoModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取接口分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取接口分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.ApiPaged)]
    public async Task<PagedResult<ApiInfoModel>> QueryApiPaged(PagedInput input)
    {
        return await _repository.Entities.OrderByIF(input.IsOrderBy, ob => ob.Sort, OrderByType.Desc)
            .OrderByIF(input.IsOrderBy, ob => ob.ApiUrl)
            .ToPagedListAsync(input);
    }
}
