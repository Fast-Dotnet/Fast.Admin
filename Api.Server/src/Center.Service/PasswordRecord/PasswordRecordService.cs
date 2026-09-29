// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.PasswordRecord.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.PasswordRecord;

/// <summary>
/// 密码记录服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "passwordRecord")]
[PlatformOnly]
public class PasswordRecordService : IDynamicApplication
{
    private readonly ISqlSugarRepository<PasswordRecordModel> _repository;

    public PasswordRecordService(ISqlSugarRepository<PasswordRecordModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取密码记录分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取密码记录分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.PasswordRecordPaged)]
    public async Task<PagedResult<QueryPasswordRecordPagedOutput>> QueryPasswordRecordPaged(QueryPasswordRecordPagedInput input)
    {
        return await _repository.Entities.LeftJoin<AccountModel>((t1, t2) => t1.AccountId == t2.AccountId)
            .WhereIF(input.AccountId != null, t1 => t1.AccountId == input.AccountId)
            .WhereIF(input.OperationType != null, t1 => t1.OperationType == input.OperationType)
            .SelectMergeTable((t1, t2) => new QueryPasswordRecordPagedOutput
            {
                RecordId = t1.RecordId,
                AccountId = t1.AccountId,
                OperationType = t1.OperationType,
                Type = t1.Type,
                CreatedTime = t1.CreatedTime,
                AccountKey = t2.AccountKey,
                Mobile = t2.Mobile,
                Email = t2.Email,
                NickName = t2.NickName,
                Avatar = t2.Avatar
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
