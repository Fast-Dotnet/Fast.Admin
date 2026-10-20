// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Service.ExceptionLog.Dto;
using Fast.CenterLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.ExceptionLog;

/// <summary>
/// 异常日志服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "exceptionLog")]
[PlatformOnly]
public class ExceptionLogService : IDynamicApplication
{
    private readonly ISqlSugarRepository<ExceptionLogModel> _repository;

    public ExceptionLogService(ISqlSugarRepository<ExceptionLogModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取异常日志分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取异常日志分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.ExceptionLogPaged)]
    public async Task<PagedResult<ExceptionLogModel>> QueryExceptionLogPaged(QueryExceptionLogPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.AccountId != null, wh => wh.AccountId == input.AccountId)
            .WhereIF(input.TenantId != null, wh => wh.TenantId == input.TenantId)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
