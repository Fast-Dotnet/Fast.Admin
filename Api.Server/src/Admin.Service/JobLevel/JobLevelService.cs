// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.JobLevel.Dto;
using Fast.AdminLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.JobLevel;

/// <summary>
/// 职级服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "jobLevel")]
public class JobLevelService : IDynamicApplication
{
    private readonly ISqlSugarRepository<JobLevelModel> _repository;

    public JobLevelService(ISqlSugarRepository<JobLevelModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 职级选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("职级选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> JobLevelSelector()
    {
        var data = await _repository.Entities.OrderByDescending(ob => ob.Level)
            .Select(sl => new {sl.JobLevelId, sl.JobLevelName, sl.Level})
            .ToListAsync();

        return data.Select(sl =>
                new ElSelectorOutput<long> {Value = sl.JobLevelId, Label = sl.JobLevelName, Data = new {sl.Level}})
            .ToList();
    }

    /// <summary>
    /// 获取职级分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取职级分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.JobLevel.Paged)]
    public async Task<PagedResult<QueryJobLevelPagedOutput>> QueryJobLevelPaged(PagedInput input)
    {
        return await _repository.Entities.OrderByIF(input.IsOrderBy, ob => ob.Level, OrderByType.Desc)
            .Select(sl => new QueryJobLevelPagedOutput
            {
                JobLevelId = sl.JobLevelId,
                JobLevelName = sl.JobLevelName,
                Level = sl.Level,
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
    /// 获取职级详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取职级详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.JobLevel.Detail)]
    public async Task<QueryJobLevelDetailOutput> QueryJobLevelDetail([Required(ErrorMessage = "职级Id不能为空")] long? jobLevelId)
    {
        QueryJobLevelDetailOutput result = await _repository.Entities.Where(wh => wh.JobLevelId == jobLevelId)
            .Select(sl => new QueryJobLevelDetailOutput
            {
                JobLevelId = sl.JobLevelId,
                JobLevelName = sl.JobLevelName,
                Level = sl.Level,
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
    /// 添加职级
    /// </summary>
    [HttpPost]
    [ApiInfo("添加职级", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.JobLevel.Add)]
    public async Task AddJobLevel(AddJobLevelInput input)
    {
        if (await _repository.AnyAsync(a => a.JobLevelName == input.JobLevelName))
        {
            throw new UserFriendlyException("职级名称重复！");
        }

        var jobLevelModel = new JobLevelModel {JobLevelName = input.JobLevelName, Level = input.Level, Remark = input.Remark};

        await _repository.InsertAsync(jobLevelModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加职级",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = jobLevelModel.JobLevelId,
            BizNo = null,
            Description = $"添加职级：{jobLevelModel.JobLevelName}"
        });
    }

    /// <summary>
    /// 编辑职级
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑职级", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.JobLevel.Edit)]
    public async Task EditJobLevel(EditJobLevelInput input)
    {
        if (await _repository.AnyAsync(a => a.JobLevelName == input.JobLevelName && a.JobLevelId != input.JobLevelId))
        {
            throw new UserFriendlyException("职级名称重复！");
        }

        JobLevelModel jobLevelModel = await _repository.SingleOrDefaultAsync(input.JobLevelId);
        if (jobLevelModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        jobLevelModel.JobLevelName = input.JobLevelName;
        jobLevelModel.Level = input.Level;
        jobLevelModel.Remark = input.Remark;
        jobLevelModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(jobLevelModel);

        await _repository.Updateable<EmployeeOrgModel>()
            .SetColumns(_ => new EmployeeOrgModel {JobLevelName = jobLevelModel.JobLevelName})
            .Where(wh => wh.JobLevelId == jobLevelModel.JobLevelId)
            .ExecuteCommandAsync();

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑职级",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = jobLevelModel.JobLevelId,
            BizNo = null,
            Description = $"编辑职级：{jobLevelModel.JobLevelName}"
        });
    }

    /// <summary>
    /// 删除职级
    /// </summary>
    [HttpPost]
    [ApiInfo("删除职级", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.JobLevel.Delete)]
    public async Task DeleteJobLevel(JobLevelIdInput input)
    {
        // 检查是否有职员关联
        if (await _repository.Queryable<EmployeeOrgModel>().AnyAsync(a => a.JobLevelId == input.JobLevelId))
        {
            throw new UserFriendlyException("职级存在职员关联，无法删除！");
        }

        JobLevelModel jobLevelModel = await _repository.SingleOrDefaultAsync(input.JobLevelId);
        if (jobLevelModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(jobLevelModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "删除职级",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = jobLevelModel.JobLevelId,
            BizNo = null,
            Description = $"删除职级：{jobLevelModel.JobLevelName}"
        });
    }
}
