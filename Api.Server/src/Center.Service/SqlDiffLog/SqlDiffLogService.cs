// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Service.SqlDiffLog.Dto;
using Fast.CenterLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.SqlDiffLog;

/// <summary>
/// SQL 差异日志服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "sqlDiffLog")]
public class SqlDiffLogService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<SqlDiffLogModel> _repository;

    public SqlDiffLogService(IUser user, ISqlSugarRepository<SqlDiffLogModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 获取 SQL 差异日志分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取Sql差异日志分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.SqlDiffLogPaged)]
    public async Task<PagedResult<SqlDiffLogModel>> QuerySqlDiffLogPaged(QuerySqlDiffLogPagedInput input)
    {
        if (input.SearchTimeList is not {Count: > 1})
        {
            throw new UserFriendlyException("请选择具体的时间范围！");
        }

        ISugarQueryable<SqlDiffLogModel> queryable = _repository.Entities
            .WhereIF(input.AccountId != null, wh => wh.AccountId == input.AccountId)
            .WhereIF(input.DiffType != null, wh => wh.DiffType == input.DiffType);

        if (_user.IsSuperAdmin)
        {
            queryable = queryable.WhereIF(input.TenantId != null, wh => wh.TenantId == input.TenantId);
        }
        else
        {
            queryable = queryable.Where(wh => wh.TenantId == _user.TenantId);
        }

        return await queryable.SplitTable()
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
