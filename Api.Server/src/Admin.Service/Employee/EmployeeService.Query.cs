// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Employee.Dto;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.Employee;

public partial class EmployeeService
{
    /// <summary>
    /// 职员选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("职员选择器", HttpRequestActionEnum.Query)]
    public async Task<PagedResult<ElSelectorOutput<long>>> EmployeeSelector(PagedInput input)
    {
        PagedResult<QueryEmployeeSelectorDto> data = await _repository.Entities
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
        PagedResult<QueryEmployeePagedOutput> result = await _repository.Entities
            .LeftJoin<EmployeeOrgModel>((t1, t2) => t1.EmployeeId == t2.EmployeeId && t2.IsPrimary)
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

        QueryEmployeeDetailOutput result = await _repository.Entities.Where(wh => wh.EmployeeId == employeeId)
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
}
