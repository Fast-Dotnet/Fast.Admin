// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Service.RequestLog.Dto;
using Fast.CenterLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.RequestLog;

/// <summary>
/// 请求日志服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "requestLog")]
public class RequestLogService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<RequestLogModel> _repository;

    public RequestLogService(IUser user, ISqlSugarRepository<RequestLogModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 获取请求日志分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取请求日志分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.RequestLogPaged)]
    [DisabledRequestLog]
    public async Task<PagedResult<RequestLogModel>> QueryRequestLogPaged(QueryRequestLogPagedInput input)
    {
        if (input.SearchTimeList is not {Count: > 1})
        {
            throw new UserFriendlyException("请选择具体的时间范围！");
        }

        ISugarQueryable<RequestLogModel> queryable = _repository.Entities
            .WhereIF(input.AccountId != null, wh => wh.AccountId == input.AccountId)
            .WhereIF(input.IsSuccess != null, wh => wh.IsSuccess == input.IsSuccess)
            .WhereIF(input.OperationAction != null, wh => wh.OperationAction == input.OperationAction)
            .WhereIF(input.RequestMethod != null, wh => wh.RequestMethod == input.RequestMethod);

        if (_user.IsSuperAdmin)
        {
            queryable = queryable.WhereIF(input.TenantId != null, wh => wh.TenantId == input.TenantId);
        }
        else if (_user.IsAdmin)
        {
            queryable = queryable.Where(wh => wh.TenantId == _user.TenantId);
        }
        else
        {
            queryable = queryable.Where(wh => wh.AccountId == _user.AccountId);
        }

        return await queryable.SplitTable()
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
