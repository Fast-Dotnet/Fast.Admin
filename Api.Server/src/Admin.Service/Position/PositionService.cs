// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Position.Dto;
using Fast.AdminLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.Position;

/// <summary>
/// 职位服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "position")]
public class PositionService : IDynamicApplication
{
    private readonly ISqlSugarRepository<PositionModel> _repository;

    public PositionService(ISqlSugarRepository<PositionModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 职位选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("职位选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> PositionSelector()
    {
        var data = await _repository.Entities.OrderBy(ob => ob.Sort)
            .Select(sl => new {sl.PositionId, sl.PositionName})
            .ToListAsync();

        return data.Select(sl => new ElSelectorOutput<long> {Value = sl.PositionId, Label = sl.PositionName}).ToList();
    }

    /// <summary>
    /// 获取职位分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取职位分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Position.Paged)]
    public async Task<PagedResult<QueryPositionPagedOutput>> QueryPositionPaged(PagedInput input)
    {
        return await _repository.Entities.OrderByIF(input.IsOrderBy, ob => ob.Sort)
            .Select(sl => new QueryPositionPagedOutput
            {
                PositionId = sl.PositionId,
                PositionName = sl.PositionName,
                Sort = sl.Sort,
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
    /// 获取职位详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取职位详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Position.Detail)]
    public async Task<QueryPositionDetailOutput> QueryPositionDetail([Required(ErrorMessage = "职位Id不能为空")] long? positionId)
    {
        QueryPositionDetailOutput result = await _repository.Entities.Where(t1 => t1.PositionId == positionId)
            .Select(sl => new QueryPositionDetailOutput
            {
                PositionId = sl.PositionId,
                PositionName = sl.PositionName,
                Sort = sl.Sort,
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
    /// 添加职位
    /// </summary>
    [HttpPost]
    [ApiInfo("添加职位", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Position.Add)]
    public async Task AddPosition(AddPositionInput input)
    {
        if (await _repository.AnyAsync(a => a.PositionName == input.PositionName))
        {
            throw new UserFriendlyException("职位名称重复！");
        }

        var positionModel = new PositionModel {PositionName = input.PositionName, Sort = input.Sort, Remark = input.Remark};

        await _repository.InsertAsync(positionModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加职位",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = positionModel.PositionId,
            BizNo = null,
            Description = $"添加职位：{positionModel.PositionName}"
        });
    }

    /// <summary>
    /// 编辑职位
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑职位", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Position.Edit)]
    public async Task EditPosition(EditPositionInput input)
    {
        if (await _repository.AnyAsync(a => a.PositionName == input.PositionName && a.PositionId != input.PositionId))
        {
            throw new UserFriendlyException("职位名称重复！");
        }

        PositionModel positionModel = await _repository.SingleOrDefaultAsync(input.PositionId);
        if (positionModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        positionModel.PositionName = input.PositionName;
        positionModel.Sort = input.Sort;
        positionModel.Remark = input.Remark;
        positionModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(positionModel);

        await _repository.Updateable<EmployeeOrgModel>()
            .SetColumns(_ => new EmployeeOrgModel {PositionName = positionModel.PositionName})
            .Where(wh => wh.PositionId == positionModel.PositionId)
            .ExecuteCommandAsync();

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑职位",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = positionModel.PositionId,
            BizNo = null,
            Description = $"编辑职位：{positionModel.PositionName}"
        });
    }

    /// <summary>
    /// 删除职位
    /// </summary>
    [HttpPost]
    [ApiInfo("删除职位", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Position.Delete)]
    public async Task DeletePosition(PositionIdInput input)
    {
        // 检查是否有员工关联
        if (await _repository.Queryable<EmployeeOrgModel>().AnyAsync(a => a.PositionId == input.PositionId))
        {
            throw new UserFriendlyException("职位存在员工关联，无法删除！");
        }

        PositionModel positionModel = await _repository.SingleOrDefaultAsync(input.PositionId);
        if (positionModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(positionModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "删除职位",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = positionModel.PositionId,
            BizNo = null,
            Description = $"删除职位：{positionModel.PositionName}"
        });
    }
}
