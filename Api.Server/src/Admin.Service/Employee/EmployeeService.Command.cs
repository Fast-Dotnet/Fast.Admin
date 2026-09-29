// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Employee.Dto;
using Fast.AdminLog.Domain;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace Fast.Admin.Service.Employee;

public partial class EmployeeService
{
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
        if (await _repository.AnyAsync(a => a.Mobile == input.Mobile && a.EmployeeId != input.EmployeeId))
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

        if (input.OrgList?.Count < 1)
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
}
