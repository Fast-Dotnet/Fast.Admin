// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text.RegularExpressions;
using Fast.Admin.Domain;
using Fast.Admin.Service.Employee.Dto;
using Fast.AdminLog.Domain;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Yitter.IdGenerator;

namespace Fast.Admin.Service.Employee;

/// <summary>
/// 职员服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "employee")]
public class EmployeeService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<EmployeeModel> _repository;
    private readonly ISqlSugarClient _centerRepository;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public EmployeeService(IUser user, ISqlSugarRepository<EmployeeModel> repository, ISqlSugarClient centerRepository,
        IHubContext<ChatHub, IChatClient> hubContext)
    {
        _user = user;
        _repository = repository;
        _centerRepository = centerRepository;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 按当前用户的数据权限获取职员
    /// </summary>
    /// <returns>当前用户数据权限范围内的职员</returns>
    private async Task<EmployeeModel> GetEmployeeWithinDataScope(long employeeId)
    {
        bool hasAccess = await _repository.Queryable<EmployeeModel>()
            .LeftJoin<EmployeeOrgModel>((t1, t2) => t1.EmployeeId == t2.EmployeeId && t2.IsPrimary)
            .SelectMergeTable((t1, t2) => new QueryEmployeeSelectorDto
            {
                EmployeeId = t1.EmployeeId, DepartmentId = t2.DepartmentId
            })
            .DataScope(e => e.DepartmentId, e => e.EmployeeId, allowPublicData: false)
            .AnyAsync(e => e.EmployeeId == employeeId);
        if (!hasAccess)
        {
            throw new UserFriendlyException("数据不存在或无权操作！");
        }

        EmployeeModel employeeModel = await _repository.SingleOrDefaultAsync(employeeId);
        if (employeeModel == null)
        {
            throw new UserFriendlyException("数据不存在或无权操作！");
        }

        return employeeModel;
    }

    /// <summary>
    /// 校验当前用户是否有权分配请求中的角色
    /// </summary>
    /// <param name="requestedRoleIds">请求分配的角色Id集合</param>
    /// <param name="existingRoleIds">职员当前角色Id集合，编辑时用于计算角色变更范围</param>
    private async Task ValidateRoleAssignment(List<long> requestedRoleIds, List<long> existingRoleIds = null)
    {
        if (_user.IsSuperAdmin || _user.IsAdmin)
        {
            return;
        }

        List<long> currentRoleIds = _user.RoleIdList ?? [];
        var roleList = await _repository.Queryable<RoleModel>()
            .Where(wh => currentRoleIds.Contains(wh.RoleId))
            .Select(sl => new {sl.AssignableRoleIds})
            .ToListAsync();
        var assignableRoleIds = roleList.Where(wh => wh.AssignableRoleIds?.Count > 0)
            .SelectMany(sl => sl.AssignableRoleIds)
            .Except(currentRoleIds)
            .Distinct()
            .ToList();
        IEnumerable<long> changedRoleIds = existingRoleIds == null
            ? requestedRoleIds
            : requestedRoleIds.Except(existingRoleIds).Concat(existingRoleIds.Except(requestedRoleIds));
        if (changedRoleIds.Except(assignableRoleIds).Any())
        {
            throw new UserFriendlyException("无权分配超出自身权限范围的角色！");
        }
    }

    /// <summary>
    /// 职员选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("职员选择器", HttpRequestActionEnum.Query)]
    public async Task<PagedResult<ElSelectorOutput<long>>> EmployeeSelector(PagedInput input)
    {
        PagedResult<QueryEmployeeSelectorDto> data = await _repository.Queryable<EmployeeModel>()
            .LeftJoin<EmployeeOrgModel>((t1, t2) => t1.EmployeeId == t2.EmployeeId && t2.IsPrimary)
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue),
                t1 => t1.EmployeeNo.Contains(input.SearchValue)
                      || t1.EmployeeName.Contains(input.SearchValue)
                      || t1.Mobile.Contains(input.SearchValue))
            .Where(t1 => t1.Status != EmployeeStatusEnum.Resigned)
            .SelectMergeTable((t1, t2) => new QueryEmployeeSelectorDto
            {
                EmployeeId = t1.EmployeeId,
                DepartmentId = t2.DepartmentId,
                EmployeeNo = t1.EmployeeNo,
                EmployeeName = t1.EmployeeName,
                Mobile = t1.Mobile,
                IdPhoto = t1.IdPhoto
            })
            .OrderBy(ob => ob.EmployeeName)
            .DataScope(e => e.DepartmentId, e => e.EmployeeId, allowPublicData: false)
            .ToPagedListAsync(input);

        return data.ToPagedData(sl => new ElSelectorOutput<long>
        {
            Value = sl.EmployeeId, Label = sl.EmployeeName, Data = new {sl.EmployeeNo, sl.Mobile, sl.IdPhoto}
        });
    }

    /// <summary>
    /// 获取职员分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取职员分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Employee.Paged)]
    public async Task<PagedResult<QueryEmployeePagedOutput>> QueryEmployeePaged(QueryEmployeePagedInput input)
    {
        PagedResult<QueryEmployeePagedOutput> result = await _repository.Queryable<EmployeeModel>()
            .LeftJoin<EmployeeOrgModel>((t1, t2) => t1.EmployeeId == t2.EmployeeId && t2.IsPrimary)
            .WhereIF(input.Status == null, t1 => t1.Status != EmployeeStatusEnum.Resigned)
            .WhereIF(input.Status != null, t1 => t1.Status == input.Status)
            .WhereIF(input.Sex != null, t1 => t1.Sex == input.Sex)
            .WhereIF(input.DepartmentId != null, (t1, t2) => t2.DepartmentId == input.DepartmentId)
            .SelectMergeTable((t1, t2) => new QueryEmployeePagedOutput
            {
                EmployeeId = t1.EmployeeId,
                EmployeeNo = t1.EmployeeNo,
                EmployeeName = t1.EmployeeName,
                Mobile = t1.Mobile,
                Status = t1.Status,
                Email = t1.Email,
                Sex = t1.Sex,
                IdPhoto = t1.IdPhoto,
                EntryDate = t1.EntryDate,
                ResignDate = t1.ResignDate,
                Remark = t1.Remark,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion,
                OrgId = t2.OrgId,
                OrgName = t2.OrgName,
                OrgNames = t2.OrgNames,
                DepartmentId = t2.DepartmentId,
                DepartmentName = t2.DepartmentName,
                DepartmentNames = t2.DepartmentNames,
                PositionId = t2.PositionId,
                PositionName = t2.PositionName,
                JobLevelId = t2.JobLevelId,
                JobLevelName = t2.JobLevelName,
                IsPrincipal = t2.IsPrincipal
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .DataScope(e => e.DepartmentId, e => e.EmployeeId, allowPublicData: false)
            .ToPagedListAsync(input);

        var employeeIds = result.Rows.Select(sl => sl.EmployeeId).ToList();

        var userList = await _centerRepository.Queryable<TenantUserModel>()
            .LeftJoin<AccountModel>((t1, t2) => t1.AccountId == t2.AccountId)
            .Where(t1 => employeeIds.Contains(t1.EmployeeId))
            .Select((t1, t2) => new
            {
                t1.EmployeeId,
                t1.Status,
                t2.Mobile,
                t2.Email,
                t2.NickName,
                t2.LastLoginTime
            })
            .ToListAsync();

        List<EmployeeRoleModel> roleList = await _repository.Queryable<EmployeeRoleModel>()
            .Where(wh => employeeIds.Contains(wh.EmployeeId))
            .ToListAsync();

        foreach (QueryEmployeePagedOutput item in result.Rows)
        {
            var userInfo = userList.SingleOrDefault(s => s.EmployeeId == item.EmployeeId);
            if (userInfo != null)
            {
                item.AccountStatus = userInfo.Status;
                item.AccountMobile = userInfo.Mobile;
                item.AccountEmail = userInfo.Email;
                item.AccountNickName = userInfo.NickName;
                item.LastLoginTime = userInfo.LastLoginTime;
            }

            item.RoleNames = string.Join(",",
                roleList.Where(wh => wh.EmployeeId == item.EmployeeId)
                    .OrderBy(ob => ob.RoleName)
                    .Select(sl => sl.RoleName)
                    .ToList());
        }

        return result;
    }

    /// <summary>
    /// 获取职员详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取职员详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Employee.Detail)]
    public async Task<QueryEmployeeDetailOutput> QueryEmployeeDetail([Required(ErrorMessage = "职员Id不能为空")] long? employeeId)
    {
        await GetEmployeeWithinDataScope(employeeId!.Value);

        QueryEmployeeDetailOutput result = await _repository.Queryable<EmployeeModel>()
            .Where(wh => wh.EmployeeId == employeeId)
            .Select(sl => new QueryEmployeeDetailOutput
            {
                EmployeeId = sl.EmployeeId,
                EmployeeNo = sl.EmployeeNo,
                EmployeeName = sl.EmployeeName,
                Mobile = sl.Mobile,
                Status = sl.Status,
                Email = sl.Email,
                Sex = sl.Sex,
                IdPhoto = sl.IdPhoto,
                EntryDate = sl.EntryDate,
                ResignDate = sl.ResignDate,
                ResignReason = sl.ResignReason,
                Remark = sl.Remark,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在或无权操作！");
        }

        result.OrgList = await _repository.Queryable<EmployeeOrgModel>().Where(wh => wh.EmployeeId == employeeId).ToListAsync();

        result.RoleList = await _repository.Queryable<EmployeeRoleModel>().Where(wh => wh.EmployeeId == employeeId).ToListAsync();

        return result;
    }

    /// <summary>
    /// 获取本职员详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取本职员详情", HttpRequestActionEnum.Query)]
    public async Task<QueryEmployeeDetailOutput> QuerySelfEmployeeDetail()
    {
        return await QueryEmployeeDetail(_user.EmployeeId);
    }

    /// <summary>
    /// 添加职员
    /// </summary>
    [HttpPost]
    [ApiInfo("添加职员", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Employee.Add)]
    public async Task AddEmployee(AddEmployeeInput input)
    {
        if (await _repository.AnyAsync(a => a.Mobile == input.Mobile))
        {
            throw new UserFriendlyException("手机号重复！");
        }

        OrganizationModel organizationModel = await _repository
            .Queryable<OrganizationModel>()
            .SingleAsync(s => s.OrgId == input.OrgId);
        if (organizationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        DepartmentModel departmentModel = await _repository
            .Queryable<DepartmentModel>()
            .SingleAsync(s => s.DepartmentId == input.DepartmentId);
        if (departmentModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        PositionModel positionModel = await _repository
            .Queryable<PositionModel>()
            .SingleAsync(s => s.PositionId == input.PositionId);
        if (positionModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        JobLevelModel jobLevelModel = await _repository
            .Queryable<JobLevelModel>()
            .SingleAsync(s => s.JobLevelId == input.JobLevelId);
        if (jobLevelModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var roleIds = (input.RoleList ?? []).Select(sl => sl.RoleId).Distinct().ToList();
        List<RoleModel> roleList = await _repository
            .Queryable<RoleModel>()
            .Where(wh => roleIds.Contains(wh.RoleId))
            .ToListAsync();
        if (roleList.Count != roleIds.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await ValidateRoleAssignment(roleIds);

        var employeeModel = new EmployeeModel
        {
            EmployeeId = YitIdHelper.NextId(),
            EmployeeName = input.EmployeeName,
            Mobile = input.Mobile,
            // 新增默认正式员工
            Status = EmployeeStatusEnum.Formal,
            Email = input.Email,
            Sex = input.Sex,
            IdPhoto = input.IdPhoto,
            EntryDate = input.EntryDate,
            ResignDate = null,
            ResignReason = null,
            Remark = input.Remark
        };

        var employeeOrgModel = new EmployeeOrgModel
        {
            EmployeeId = employeeModel.EmployeeId,
            OrgId = organizationModel.OrgId,
            OrgName = organizationModel.OrgName,
            OrgNames = [.. organizationModel.ParentNames, organizationModel.OrgName],
            DepartmentId = departmentModel.DepartmentId,
            DepartmentName = departmentModel.DepartmentName,
            DepartmentNames = [.. departmentModel.ParentNames, departmentModel.DepartmentName],
            IsPrimary = true,
            PositionId = positionModel.PositionId,
            PositionName = positionModel.PositionName,
            JobLevelId = jobLevelModel.JobLevelId,
            JobLevelName = jobLevelModel.JobLevelName,
            IsPrincipal = input.IsPrincipal
        };

        var employeeRoleList = new List<EmployeeRoleModel>();
        foreach (EmployeeRoleModel item in input.RoleList)
        {
            RoleModel roleModel = roleList.Single(s => s.RoleId == item.RoleId);
            employeeRoleList.Add(new EmployeeRoleModel
            {
                EmployeeId = employeeModel.EmployeeId, RoleId = roleModel.RoleId, RoleName = roleModel.RoleName
            });
        }

        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);

        await _repository.Ado.UseTranAsync(async () =>
        {
            string employeeNo = SerialContext.GenEmployeeNo(_repository, tenantModel.TenantCode);
            employeeModel.EmployeeNo = employeeNo;
            await _repository.InsertAsync(employeeModel);

            // 如果当前职员是负责人，则清除该部门原有负责人
            if (employeeOrgModel.IsPrincipal)
            {
                await _repository.Updateable<EmployeeOrgModel>()
                    .SetColumns(_ => new EmployeeOrgModel {IsPrincipal = false})
                    .Where(wh => wh.DepartmentId == employeeOrgModel.DepartmentId)
                    .ExecuteCommandAsync();
            }

            await _repository.Insertable(employeeOrgModel).ExecuteCommandAsync();

            // 删除旧的角色数据
            await _repository.Deleteable<EmployeeRoleModel>()
                .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
                .ExecuteCommandAsync();
            await _repository.Insertable(employeeRoleList).ExecuteCommandAsync();
        }, ex => throw ex);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加职员",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description =
                $"职员名称：{employeeModel.EmployeeName}，职员手机：{employeeModel.Mobile}，职员邮箱：{employeeModel.Email}，职员部门：{employeeOrgModel.DepartmentName}"
        });
    }

    /// <summary>
    /// 编辑本职员
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑本职员", HttpRequestActionEnum.Edit)]
    public async Task EditSelfEmployee(EditEmployeeInput input)
    {
        if (await _repository.AnyAsync(a => a.Mobile == input.Mobile && a.EmployeeId != _user.EmployeeId))
        {
            throw new UserFriendlyException("手机号重复！");
        }

        EmployeeModel employeeModel = await _repository.SingleOrDefaultAsync(_user.EmployeeId);
        if (employeeModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        employeeModel.EmployeeName = input.EmployeeName;
        employeeModel.Mobile = input.Mobile;
        employeeModel.Email = input.Email;
        employeeModel.Sex = input.Sex;
        employeeModel.IdPhoto = input.IdPhoto;
        employeeModel.RowVersion = input.RowVersion;

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            TenantUserModel tenantUserModel = await _centerRepository.Queryable<TenantUserModel>()
                .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
                .SingleAsync();
            if (tenantUserModel != null)
            {
                tenantUserModel.EmployeeName = employeeModel.EmployeeName;
                tenantUserModel.IdPhoto = employeeModel.IdPhoto;
                await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();
            }

            await _repository.UpdateAsync(employeeModel);

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        await _user.RefreshEmployee(new RefreshEmployeeDto
        {
            DeviceType = _user.DeviceType,
            AppNo = _user.AppNo,
            TenantNo = _user.TenantNo,
            EmployeeNo = _user.EmployeeNo,
            EmployeeName = employeeModel.EmployeeName,
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            RoleIdList = _user.RoleIdList,
            RoleNameList = _user.RoleNameList,
            RoleType = _user.RoleType,
            DataScopeType = _user.DataScopeType,
            DataScopeDepartmentIdList = _user.DataScopeDepartmentIdList
        });

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑本职员",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"编辑本职员：{employeeModel.EmployeeName}"
        });
    }

    /// <summary>
    /// 编辑职员
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑职员", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Edit)]
    public async Task EditEmployee(EditEmployeeInput input)
    {
        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (await _repository.AnyAsync(a => a.Mobile == input.Mobile && a.EmployeeId != input.EmployeeId))
        {
            throw new UserFriendlyException("手机号重复！");
        }

        if (input.OrgList == null || input.OrgList.Count < 1)
        {
            throw new UserFriendlyException("请至少填写一个部门！");
        }

        if (input.OrgList.Count(c => c.IsPrimary) > 1)
        {
            throw new UserFriendlyException("只能存在一个主部门！");
        }

        if ((input.RoleList ?? []).Select(sl => sl.RoleId).Distinct().Count() != (input.RoleList?.Count ?? 0))
        {
            throw new UserFriendlyException("角色重复！");
        }

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止修改已离职的职员资料！");
        }

        var orgIds = input.OrgList.Select(sl => sl.OrgId).Distinct().ToList();
        List<OrganizationModel> organizationList = await _repository
            .Queryable<OrganizationModel>()
            .Where(wh => orgIds.Contains(wh.OrgId))
            .ToListAsync();
        if (organizationList.Count != orgIds.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var departmentIds = input.OrgList.Select(sl => sl.DepartmentId).Distinct().ToList();
        List<DepartmentModel> departmentList = await _repository.Queryable<DepartmentModel>()
            .Where(wh => departmentIds.Contains(wh.DepartmentId))
            .ToListAsync();
        if (departmentList.Count != departmentIds.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var positionId = input.OrgList.Select(sl => sl.PositionId).Distinct().ToList();
        List<PositionModel> positionList = await _repository.Queryable<PositionModel>()
            .Where(wh => positionId.Contains(wh.PositionId))
            .ToListAsync();
        if (positionList.Count != positionId.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var jobLevelId = input.OrgList.Select(sl => sl.JobLevelId).Distinct().ToList();
        List<JobLevelModel> jobLevelList = await _repository.Queryable<JobLevelModel>()
            .Where(wh => jobLevelId.Contains(wh.JobLevelId))
            .ToListAsync();
        if (jobLevelList.Count != jobLevelId.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var roleIds = (input.RoleList ?? []).Select(sl => sl.RoleId).ToList();
        List<RoleModel> roleList = await _repository
            .Queryable<RoleModel>()
            .Where(wh => roleIds.Contains(wh.RoleId))
            .ToListAsync();
        if (roleList.Count != roleIds.Count)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        List<long> existingRoleIds = await _repository.Queryable<EmployeeRoleModel>()
            .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
            .Select(sl => sl.RoleId)
            .ToListAsync();
        await ValidateRoleAssignment(roleIds, existingRoleIds);

        employeeModel.EmployeeName = input.EmployeeName;
        employeeModel.Mobile = input.Mobile;
        employeeModel.Email = input.Email;
        employeeModel.Sex = input.Sex;
        employeeModel.IdPhoto = input.IdPhoto;
        employeeModel.RowVersion = input.RowVersion;

        var employeeOrgList = new List<EmployeeOrgModel>();
        var employeeRoleList = new List<EmployeeRoleModel>();
        if (employeeModel.EmployeeId != _user.EmployeeId)
        {
            employeeModel.EntryDate = input.EntryDate;
            employeeModel.Remark = input.Remark;

            foreach (EmployeeOrgModel item in input.OrgList)
            {
                OrganizationModel organizationModel = organizationList.Single(s => s.OrgId == item.OrgId);
                DepartmentModel departmentModel = departmentList.Single(s => s.DepartmentId == item.DepartmentId);
                PositionModel positionModel = positionList.Single(s => s.PositionId == item.PositionId);
                JobLevelModel jobLevelModel = jobLevelList.Single(s => s.JobLevelId == item.JobLevelId);

                employeeOrgList.Add(new EmployeeOrgModel
                {
                    EmployeeId = employeeModel.EmployeeId,
                    OrgId = organizationModel.OrgId,
                    OrgName = organizationModel.OrgName,
                    OrgNames = [.. organizationModel.ParentNames, organizationModel.OrgName],
                    DepartmentId = departmentModel.DepartmentId,
                    DepartmentName = departmentModel.DepartmentName,
                    DepartmentNames = [.. departmentModel.ParentNames, departmentModel.DepartmentName],
                    IsPrimary = item.IsPrimary,
                    PositionId = positionModel.PositionId,
                    PositionName = positionModel.PositionName,
                    JobLevelId = jobLevelModel.JobLevelId,
                    JobLevelName = jobLevelModel.JobLevelName,
                    IsPrincipal = item.IsPrincipal
                });
            }

            foreach (EmployeeRoleModel item in input.RoleList ?? [])
            {
                RoleModel roleModel = roleList.Single(s => s.RoleId == item.RoleId);
                employeeRoleList.Add(new EmployeeRoleModel
                {
                    EmployeeId = employeeModel.EmployeeId, RoleId = roleModel.RoleId, RoleName = roleModel.RoleName
                });
            }
        }

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            TenantUserModel tenantUserModel = await _centerRepository.Queryable<TenantUserModel>()
                .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
                .SingleAsync();
            if (tenantUserModel != null)
            {
                tenantUserModel.EmployeeName = employeeModel.EmployeeName;
                tenantUserModel.IdPhoto = employeeModel.IdPhoto;
            }

            if (employeeModel.EmployeeId != _user.EmployeeId)
            {
                // 删除旧的部门数据
                await _repository.Deleteable<EmployeeOrgModel>()
                    .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
                    .ExecuteCommandAsync();
                // 删除旧的角色数据
                await _repository.Deleteable<EmployeeRoleModel>()
                    .Where(wh => wh.EmployeeId == employeeModel.EmployeeId)
                    .ExecuteCommandAsync();

                // 处理部门负责人
                var principalDepartmentIds = employeeOrgList.Where(wh => wh.IsPrincipal).Select(sl => sl.DepartmentId).ToList();
                if (principalDepartmentIds.Any())
                {
                    await _repository.Updateable<EmployeeOrgModel>()
                        .SetColumns(_ => new EmployeeOrgModel {IsPrincipal = false})
                        .Where(wh => principalDepartmentIds.Contains(wh.DepartmentId))
                        .ExecuteCommandAsync();
                }

                await _repository.Insertable(employeeOrgList).ExecuteCommandAsync();
                await _repository.Insertable(employeeRoleList).ExecuteCommandAsync();

                if (tenantUserModel != null)
                {
                    tenantUserModel.DepartmentId = employeeOrgList.Single(s => s.IsPrimary).DepartmentId;
                    tenantUserModel.DepartmentName = employeeOrgList.Single(s => s.IsPrimary).DepartmentName;
                }
            }

            if (tenantUserModel != null)
            {
                await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();
            }

            await _repository.UpdateAsync(employeeModel);

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        if (employeeModel.EmployeeId != _user.EmployeeId)
        {
            await _user.RevokeEmployee(_user.TenantNo, employeeModel.EmployeeNo);
        }
        else
        {
            // 编辑本人时同步当前登录会话，避免获取用户信息仍返回旧资料。
            await _user.RefreshEmployee(new RefreshEmployeeDto
            {
                DeviceType = _user.DeviceType,
                AppNo = _user.AppNo,
                TenantNo = _user.TenantNo,
                EmployeeNo = _user.EmployeeNo,
                EmployeeName = employeeModel.EmployeeName,
                DepartmentId = _user.DepartmentId,
                DepartmentName = _user.DepartmentName,
                RoleIdList = _user.RoleIdList,
                RoleNameList = _user.RoleNameList,
                RoleType = _user.RoleType,
                DataScopeType = _user.DataScopeType,
                DataScopeDepartmentIdList = _user.DataScopeDepartmentIdList
            });
        }

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑职员",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"编辑职员：{employeeModel.EmployeeName}"
        });
    }

    /// <summary>
    /// 职员更改状态
    /// </summary>
    [HttpPost]
    [ApiInfo("职员更改状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task ChangeStatus(ChangeStatusInput input)
    {
        if (input.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止直接修改为离职状态！");
        }

        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.ResignDate != null)
        {
            employeeModel.ResignDate = null;
            employeeModel.ResignReason = null;
        }

        employeeModel.Status = input.Status;
        employeeModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(employeeModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "更改职员状态",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，状态 -> {employeeModel.Status.GetDescription()}"
        });
    }

    /// <summary>
    /// 职员离职
    /// </summary>
    [HttpPost]
    [ApiInfo("职员离职", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task EmployeeResigned(EmployeeResignedInput input)
    {
        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.EmployeeId == _user.EmployeeId)
        {
            throw new UserFriendlyException("禁止将当前登录职员设为离职！");
        }

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("该职员已离职，请勿重复操作！");
        }

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            TenantUserModel tenantUserModel = await _centerRepository
                .Queryable<TenantUserModel>()
                .InSingleAsync(employeeModel.EmployeeId);
            if (tenantUserModel != null)
            {
                tenantUserModel.Status = CommonStatusEnum.Disable;
                await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();
            }

            employeeModel.Status = EmployeeStatusEnum.Resigned;
            employeeModel.ResignDate = input.ResignDate;
            employeeModel.ResignReason = input.ResignReason;
            employeeModel.RowVersion = input.RowVersion;

            await _repository.UpdateAsync(employeeModel);

            // 处理职员离职操作
            foreach (IEmployeeResignedHandler handler in FastContext.GetServices<IEmployeeResignedHandler>())
            {
                await handler.HandleAsync(_repository, employeeModel.EmployeeId);
            }

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        await _user.RevokeEmployee(_user.TenantNo, employeeModel.EmployeeNo);
        await ForceEmployeeOffline(employeeModel.EmployeeId, "职员已离职");

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "职员离职",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，离职 -> {employeeModel.ResignDate:yyyy-MM-dd HH:mm:ss}"
        });
    }

    /// <summary>
    /// 绑定登录账号
    /// </summary>
    [HttpPost]
    [ApiInfo("绑定登录账号", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Edit)]
    public async Task BindLoginAccount(BindLoginAccountInput input)
    {
        if (!new Regex(RegexConst.Mobile).IsMatch(input.Mobile))
        {
            throw new UserFriendlyException("手机号码不正确！");
        }

        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止为已离职的职员绑定登录账号！");
        }

        if (await _centerRepository.Queryable<TenantUserModel>().AnyAsync(a => a.EmployeeId == employeeModel.EmployeeId))
        {
            throw new UserFriendlyException("已存在登录账号！");
        }

        EmployeeOrgModel employeeOrgModel = await _repository.Queryable<EmployeeOrgModel>()
            .SingleAsync(s => s.EmployeeId == employeeModel.EmployeeId && s.IsPrimary);

        if (string.IsNullOrWhiteSpace(employeeModel.Email))
        {
            employeeModel.Email = input.Email;
        }

        employeeModel.RowVersion = input.RowVersion;

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            AccountModel accountModel = await _centerRepository.Queryable<AccountModel>()
                .Where(wh => wh.Mobile == input.Mobile)
                .SingleAsync();
            if (accountModel == null)
            {
                if (await _centerRepository.Queryable<AccountModel>().AnyAsync(a => a.Email == input.Email))
                {
                    throw new UserFriendlyException("邮箱已存在账号信息！");
                }

                string passwordHash = CryptoUtil.HashPasswordPBKDF2SHA256(GlobalContext.InitializationSettings.DefaultPassword);
                long accountId = YitIdHelper.NextId();
                accountModel = new AccountModel
                {
                    AccountId = accountId,
                    AccountKey = NumberUtil.IdToCodeByLong(accountId),
                    Mobile = input.Mobile,
                    Email = input.Email,
                    Password = passwordHash,
                    Status = CommonStatusEnum.Enable,
                    NickName = employeeModel.EmployeeName,
                    Avatar = employeeModel.IdPhoto
                };
                await _centerRepository.Insertable(accountModel).ExecuteCommandAsync();

                #region PasswordRecordModel

                // 初始化密码记录表
                await _centerRepository.Insertable(new List<PasswordRecordModel>
                    {
                        new()
                        {
                            AccountId = accountModel.AccountId,
                            OperationType = PasswordOperationTypeEnum.Create,
                            Type = PasswordTypeEnum.PBKDF2_SHA256,
                            Password = passwordHash
                        }
                    })
                    .ExecuteCommandAsync();

                #endregion
            }

            var tenantUserModel = new TenantUserModel
            {
                EmployeeId = employeeModel.EmployeeId,
                UserKey = NumberUtil.IdToCodeByLong(employeeModel.EmployeeId),
                AccountId = accountModel.AccountId,
                EmployeeNo = employeeModel.EmployeeNo,
                EmployeeName = employeeModel.EmployeeName,
                IdPhoto = employeeModel.IdPhoto,
                DepartmentId = employeeOrgModel?.DepartmentId,
                DepartmentName = employeeOrgModel?.DepartmentName,
                UserType = UserTypeEnum.None,
                Status = CommonStatusEnum.Enable
            };
            await _centerRepository.Insertable(tenantUserModel).ExecuteCommandAsync();

            await _repository.UpdateAsync(employeeModel);

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "职员绑定登录账号",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，手机：{input.Mobile}，邮箱：{input.Email}"
        });
    }

    /// <summary>
    /// 更改登录状态
    /// </summary>
    [HttpPost]
    [ApiInfo("更改登录状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task ChangeLoginStatus(EmployeeIdInput input)
    {
        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.RowVersion != input.RowVersion)
        {
            throw new UserFriendlyException("职员信息已发生变化，请刷新后重试！");
        }

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止操作已离职的职员！");
        }

        TenantUserModel tenantUserModel = await _centerRepository
            .Queryable<TenantUserModel>()
            .InSingleAsync(employeeModel.EmployeeId);
        if (tenantUserModel == null)
        {
            throw new UserFriendlyException("未绑定登录账号！");
        }

        if (_user.AccountId == tenantUserModel.AccountId)
        {
            throw new UserFriendlyException("禁止更改当前登录账号状态！");
        }

        if (input.AccountStatus != null && input.AccountStatus != tenantUserModel.Status)
        {
            throw new UserFriendlyException("登录账号状态已发生变化，请刷新后重试！");
        }

        tenantUserModel.Status = tenantUserModel.Status switch
        {
            CommonStatusEnum.Enable => CommonStatusEnum.Disable,
            CommonStatusEnum.Disable => CommonStatusEnum.Enable,
            _ => tenantUserModel.Status
        };

        await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();

        if (tenantUserModel.Status == CommonStatusEnum.Disable)
        {
            await _user.RevokeEmployee(_user.TenantNo, employeeModel.EmployeeNo);
            await ForceEmployeeOffline(employeeModel.EmployeeId, "账号已被禁用");
        }

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "更改职员登录状态",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，{tenantUserModel.Status.GetDescription()}登录账号"
        });
    }

    /// <summary>
    /// 强制下线职员的全部在线会话
    /// </summary>
    private async Task ForceEmployeeOffline(long employeeId, string message)
    {
        List<string> connectionIds = await _centerRepository.Queryable<TenantOnlineUserModel>()
            .Where(wh => wh.IsOnline)
            .Where(wh => wh.TenantId == _user.TenantId)
            .Where(wh => wh.EmployeeId == employeeId)
            .Select(sl => sl.ConnectionId)
            .ToListAsync();
        if (connectionIds.Count == 0)
        {
            return;
        }

        await _hubContext.Clients.Clients(connectionIds)
            .ForceOffline(new ForceOfflineOutput
            {
                IsAdmin = _user.IsSuperAdmin || _user.IsAdmin,
                NickName = _user.NickName,
                EmployeeNo = _user.EmployeeNo,
                OfflineTime = DateTime.Now,
                Message = message
            });
    }

    /// <summary>
    /// 获取职员常用菜单
    /// </summary>
    [HttpGet]
    [ApiInfo("获取职员常用菜单", HttpRequestActionEnum.Query)]
    public async Task<List<string>> QueryCommonMenus()
    {
        TenantUserModel tenantUserModel = await _centerRepository.Queryable<TenantUserModel>()
            .Where(wh => wh.EmployeeId == _user.EmployeeId)
            .SingleAsync();
        return tenantUserModel?.CommonMenuCodes ?? [];
    }

    /// <summary>
    /// 编辑职员常用菜单
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑职员常用菜单", HttpRequestActionEnum.Edit)]
    public async Task EditCommonMenus(EditCommonMenusInput input)
    {
        TenantUserModel tenantUserModel = await _centerRepository.Queryable<TenantUserModel>()
            .Where(wh => wh.EmployeeId == _user.EmployeeId)
            .SingleAsync();
        if (tenantUserModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (input.CommonMenuCodes.Any(string.IsNullOrWhiteSpace))
        {
            throw new UserFriendlyException("菜单编码不能为空！");
        }

        var menuCodes = input.CommonMenuCodes.Distinct(StringComparer.Ordinal).ToList();

        ApplicationClientModel application = await ApplicationContext.GetApplication(GlobalContext.Origin);
        TenantModel tenant = await TenantContext.GetTenant(_user.TenantNo);
        List<string> authorizedCodes = _user.MenuCodeList ?? [];

        // 与登录菜单查询一致：管理员按租户版本访问菜单，普通职员还需校验授权编码
        List<string> availableCodes = await _centerRepository.Queryable<MenuModel>()
            .Where(wh => wh.AppId == application.AppId && wh.Status == CommonStatusEnum.Enable)
            .Where(wh => wh.HasMobile && wh.Visible && wh.MenuType != MenuTypeEnum.Catalog)
            .Where(wh => tenant.Edition >= wh.Edition)
            .WhereIF(!_user.IsSuperAdmin && !_user.IsAdmin, m => authorizedCodes.Contains(m.MenuCode))
            .Where(m => menuCodes.Contains(m.MenuCode))
            .Select(m => m.MenuCode)
            .ToListAsync();
        if (menuCodes.Except(availableCodes, StringComparer.Ordinal).Any())
        {
            throw new UserFriendlyException("包含不可访问的菜单，请刷新工作台后重试！");
        }

        tenantUserModel.CommonMenuCodes = menuCodes;

        await _centerRepository.Updateable(tenantUserModel)
            .UpdateColumns(e => e.CommonMenuCodes)
            .ExecuteCommandWithOptLockAsync(true);
    }
}
