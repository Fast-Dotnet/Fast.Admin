// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.PayRecord;

/// <summary>
/// 支付记录服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "payRecord")]
public class PayRecordService : IDynamicApplication
{
    private readonly ISqlSugarRepository<PayRecordModel> _repository;

    public PayRecordService(ISqlSugarRepository<PayRecordModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取支付记录分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取支付记录分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.PayRecordPaged)]
    public async Task<PagedResult<PayRecordModel>> QueryPayRecordPaged(PagedInput input)
    {
        return await _repository.Entities.OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }
}
