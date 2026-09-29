// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Department.Dto;
using Fast.AdminLog.Domain;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.Department;

/// <summary>
/// 部门服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "department")]
public class DepartmentService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<DepartmentModel> _repository;
    private readonly ISqlSugarRepository<TenantUserModel> _tenantUserRepository;

    public DepartmentService(IUser user, ISqlSugarRepository<DepartmentModel> repository,
        ISqlSugarRepository<TenantUserModel> tenantUserRepository)
    {
        _user = user;
        _repository = repository;
        _tenantUserRepository = tenantUserRepository;
    }

    /// <summary>
    /// 部门选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("部门选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> DepartmentSelector(long? orgId)
    {
        ISugarQueryable<DepartmentModel> queryable = _repository.Entities.WhereIF(orgId != null, wh => wh.OrgId == orgId);
        List<long> customDepartmentIds = _user.DataScopeDepartmentIdList ?? [];

        // 管理员，全部权限
        if (_user.IsSuperAdmin || _user.IsAdmin || _user.DataScopeType == DataScopeTypeEnum.All)
        {
        }
        // 本机构及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.OrgWithChild)
        {
            queryable = queryable.Where(wh => wh.OrgId
                                              == SqlFunc.Subqueryable<EmployeeOrgModel>()
                                                  // 主部门
                                                  .Where(e => e.EmployeeId == _user.EmployeeId && e.IsPrimary)
                                                  .Where(e => e.OrgId == wh.OrgId)
                                                  .Select(e => e.OrgId)
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 本部门及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.DeptWithChild)
        {
            queryable = queryable.Where(wh => wh.DepartmentId == _user.DepartmentId
                                              || SqlFunc.JsonArrayAny(wh.ParentIds, _user.DepartmentId ?? 0)
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 自定义部门数据
        else if (_user.DataScopeType == DataScopeTypeEnum.CustomDept)
        {
            queryable = queryable.Where(wh => customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 本部门数据或仅本人数据
        else
        {
            queryable = queryable.Where(wh => wh.DepartmentId == _user.DepartmentId
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }

        var data = await queryable.OrderBy(ob => ob.Sort)
            .Select(sl => new
            {
                sl.DepartmentId,
                sl.OrgId,
                sl.OrgName,
                sl.ParentId,
                sl.ParentName,
                sl.ParentIds,
                sl.ParentNames,
                sl.DepartmentName,
                sl.DepartmentCode,
                sl.Contacts,
                sl.Phone
            })
            .ToListAsync();

        return data.Select(sl => new ElSelectorOutput<long>
            {
                Value = sl.DepartmentId,
                Label = sl.DepartmentName,
                ParentId = sl.ParentId,
                Data = new
                {
                    sl.OrgId,
                    sl.OrgName,
                    sl.ParentName,
                    sl.ParentIds,
                    sl.ParentNames,
                    sl.DepartmentCode,
                    sl.Contacts,
                    sl.Phone
                }
            })
            .ToList()
            .Build();
    }

    /// <summary>
    /// 获取部门列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取部门列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Department.Paged)]
    public async Task<List<QueryDepartmentPagedOutput>> QueryDepartmentPaged(QueryDepartmentPagedInput input)
    {
        ISugarQueryable<DepartmentModel> queryable = _repository.Entities
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue),
                wh => wh.DepartmentName.Contains(input.SearchValue) || wh.DepartmentCode.Contains(input.SearchValue))
            .WhereIF(input.OrgId != null, wh => wh.OrgId == input.OrgId);
        List<long> customDepartmentIds = _user.DataScopeDepartmentIdList ?? [];

        // 管理员，全部权限
        if (_user.IsSuperAdmin || _user.IsAdmin || _user.DataScopeType == DataScopeTypeEnum.All)
        {
        }
        // 本机构及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.OrgWithChild)
        {
            queryable = queryable.Where(wh => wh.OrgId
                                              == SqlFunc.Subqueryable<EmployeeOrgModel>()
                                                  // 主部门
                                                  .Where(e => e.EmployeeId == _user.EmployeeId && e.IsPrimary)
                                                  .Where(e => e.OrgId == wh.OrgId)
                                                  .Select(e => e.OrgId)
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 本部门及以下数据
        else if (_user.DataScopeType == DataScopeTypeEnum.DeptWithChild)
        {
            queryable = queryable.Where(wh => wh.DepartmentId == _user.DepartmentId
                                              || SqlFunc.JsonArrayAny(wh.ParentIds, _user.DepartmentId ?? 0)
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 自定义部门数据
        else if (_user.DataScopeType == DataScopeTypeEnum.CustomDept)
        {
            queryable = queryable.Where(wh => customDepartmentIds.Contains(wh.DepartmentId));
        }
        // 本部门数据或仅本人数据
        else
        {
            queryable = queryable.Where(wh => wh.DepartmentId == _user.DepartmentId
                                              || customDepartmentIds.Contains(wh.DepartmentId));
        }

        List<QueryDepartmentPagedOutput> data = await queryable.OrderByIF(input.IsOrderBy, ob => ob.Sort)
            .Select(sl => new QueryDepartmentPagedOutput
            {
                DepartmentId = sl.DepartmentId,
                OrgId = sl.OrgId,
                OrgName = sl.OrgName,
                ParentId = sl.ParentId,
                ParentName = sl.ParentName,
                ParentIds = sl.ParentIds,
                ParentNames = sl.ParentNames,
                DepartmentName = sl.DepartmentName,
                DepartmentCode = sl.DepartmentCode,
                Contacts = sl.Contacts,
                Phone = sl.Phone,
                Email = sl.Email,
                Sort = sl.Sort,
                DataPublic = sl.DataPublic,
                Remark = sl.Remark,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SugarPaged(input)
            .ToListAsync();

        return new TreeBuildUtil<QueryDepartmentPagedOutput, long>().Build(data);
    }

    /// <summary>
    /// 获取部门详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取部门详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Department.Detail)]
    public async Task<QueryDepartmentDetailOutput> QueryDepartmentDetail([Required(ErrorMessage = "部门Id不能为空")] long? departmentId)
    {
        QueryDepartmentDetailOutput result = await _repository.Entities.Where(wh => wh.DepartmentId == departmentId)
            .Select(sl => new QueryDepartmentDetailOutput
            {
                DepartmentId = sl.DepartmentId,
                OrgId = sl.OrgId,
                OrgName = sl.OrgName,
                ParentId = sl.ParentId,
                ParentName = sl.ParentName,
                ParentIds = sl.ParentIds,
                ParentNames = sl.ParentNames,
                DepartmentName = sl.DepartmentName,
                DepartmentCode = sl.DepartmentCode,
                Contacts = sl.Contacts,
                Phone = sl.Phone,
                Email = sl.Email,
                Sort = sl.Sort,
                DataPublic = sl.DataPublic,
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
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加部门
    /// </summary>
    [HttpPost]
    [ApiInfo("添加部门", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Department.Add)]
    public async Task AddDepartment(AddDepartmentInput input)
    {
        if (await _repository.AnyAsync(a => a.DepartmentName == input.DepartmentName))
        {
            throw new UserFriendlyException("部门名称重复！");
        }

        if (await _repository.AnyAsync(a => a.DepartmentCode == input.DepartmentCode))
        {
            throw new UserFriendlyException("部门编码重复！");
        }

        OrganizationModel organizationModel = await _repository
            .Queryable<OrganizationModel>()
            .SingleAsync(s => s.OrgId == input.OrgId);

        if (organizationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var departmentModel = new DepartmentModel
        {
            OrgId = organizationModel.OrgId,
            OrgName = organizationModel.OrgName,
            DepartmentName = input.DepartmentName,
            DepartmentCode = input.DepartmentCode,
            Contacts = input.Contacts,
            Phone = input.Phone,
            Email = input.Email,
            Sort = input.Sort,
            DataPublic = input.DataPublic,
            Remark = input.Remark
        };

        if (input.ParentId > 0)
        {
            DepartmentModel parentDepartment = await _repository.SingleOrDefaultAsync(s => s.DepartmentId == input.ParentId);
            if (parentDepartment == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            departmentModel.ParentId = parentDepartment.DepartmentId;
            departmentModel.ParentName = parentDepartment.DepartmentName;
            departmentModel.ParentIds = [.. parentDepartment.ParentIds, parentDepartment.DepartmentId];
            departmentModel.ParentNames = [.. parentDepartment.ParentNames, parentDepartment.DepartmentName];
        }
        else
        {
            departmentModel.ParentId = 0;
            departmentModel.ParentName = null;
            departmentModel.ParentIds = [0];
            departmentModel.ParentNames = [];
        }

        await _repository.InsertAsync(departmentModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加部门",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = departmentModel.DepartmentId,
            BizNo = null,
            Description = $"添加部门：{departmentModel.DepartmentName}"
        });
    }

    /// <summary>
    /// 编辑部门
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑部门", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Department.Edit)]
    public async Task EditDepartment(EditDepartmentInput input)
    {
        if (input.ParentId == input.DepartmentId)
        {
            throw new UserFriendlyException("不能将自己设为父部门！");
        }

        if (await _repository.AnyAsync(a => a.DepartmentName == input.DepartmentName && a.DepartmentId != input.DepartmentId))
        {
            throw new UserFriendlyException("部门名称重复！");
        }

        if (await _repository.AnyAsync(a => a.DepartmentCode == input.DepartmentCode && a.DepartmentId != input.DepartmentId))
        {
            throw new UserFriendlyException("部门编码重复！");
        }

        DepartmentModel departmentModel = await _repository.SingleOrDefaultAsync(input.DepartmentId);
        if (departmentModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        OrganizationModel organizationModel = await _repository
            .Queryable<OrganizationModel>()
            .SingleAsync(s => s.OrgId == input.OrgId);

        if (organizationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        string description = $"编辑部门：{input.DepartmentName}";

        departmentModel.OrgId = organizationModel.OrgId;
        departmentModel.OrgName = organizationModel.OrgName;
        departmentModel.DepartmentName = input.DepartmentName;
        departmentModel.DepartmentCode = input.DepartmentCode;
        departmentModel.Contacts = input.Contacts;
        departmentModel.Phone = input.Phone;
        departmentModel.Email = input.Email;
        departmentModel.Sort = input.Sort;
        departmentModel.DataPublic = input.DataPublic;
        departmentModel.Remark = input.Remark;
        departmentModel.RowVersion = input.RowVersion;

        if (input.ParentId > 0)
        {
            DepartmentModel parentDepartment = await _repository.SingleOrDefaultAsync(s => s.DepartmentId == input.ParentId);
            if (parentDepartment == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            if (departmentModel.ParentId != input.ParentId)
            {
                description += $"父级部门 -> {parentDepartment.DepartmentName}";
            }

            departmentModel.ParentId = parentDepartment.DepartmentId;
            departmentModel.ParentName = parentDepartment.DepartmentName;
            departmentModel.ParentIds = [.. parentDepartment.ParentIds, parentDepartment.DepartmentId];
            departmentModel.ParentNames = [.. parentDepartment.ParentNames, parentDepartment.DepartmentName];
        }
        else
        {
            if (departmentModel.ParentId != input.ParentId)
            {
                description += "删除父级部门";
            }

            departmentModel.ParentId = 0;
            departmentModel.ParentName = null;
            departmentModel.ParentIds = [0];
            departmentModel.ParentNames = [];
        }

        await _repository.UpdateAsync(departmentModel);

        // 更新所有子级
        List<DepartmentModel> childrenList = await _repository.Entities
            .Where(wh => SqlFunc.JsonArrayAny(wh.ParentIds, departmentModel.DepartmentId))
            .ToListAsync();

        void updateChildrenName(long parentId)
        {
            DepartmentModel parentModel = childrenList.Single(s => s.DepartmentId == parentId);
            foreach (DepartmentModel item in childrenList.Where(wh => wh.ParentId == parentId))
            {
                item.ParentIds = [.. parentModel.ParentIds, parentModel.DepartmentId];
                item.ParentNames = [.. parentModel.ParentNames, parentModel.DepartmentName];
                updateChildrenName(item.DepartmentId);
            }
        }

        // 处理顶层
        foreach (DepartmentModel item in childrenList.Where(wh => wh.ParentId == departmentModel.DepartmentId))
        {
            item.ParentName = departmentModel.DepartmentName;
            item.ParentIds = [.. departmentModel.ParentIds, departmentModel.DepartmentId];
            item.ParentNames = [.. departmentModel.ParentNames, departmentModel.DepartmentName];

            updateChildrenName(item.DepartmentId);
        }

        await _repository.Updateable(childrenList)
            .UpdateColumns(e => new {e.ParentName, e.ParentIds, e.ParentNames})
            .ExecuteCommandAsync();

        await _repository.Updateable<EmployeeOrgModel>()
            .SetColumns(_ => new EmployeeOrgModel
            {
                DepartmentName = departmentModel.DepartmentName,
                DepartmentNames = new List<string>(departmentModel.ParentNames) {departmentModel.DepartmentName}
            })
            .Where(wh => wh.DepartmentId == departmentModel.DepartmentId)
            .ExecuteCommandAsync();

        await _tenantUserRepository.Updateable<TenantUserModel>()
            .SetColumns(_ => new TenantUserModel {DepartmentName = departmentModel.DepartmentName})
            .Where(wh => wh.DepartmentId == departmentModel.DepartmentId)
            .ExecuteCommandAsync();

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑部门",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = departmentModel.DepartmentId,
            BizNo = null,
            Description = description
        });
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    [HttpPost]
    [ApiInfo("删除部门", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Department.Delete)]
    public async Task DeleteDepartment(DepartmentIdInput input)
    {
        if (await _repository.AnyAsync(a => a.ParentId == input.DepartmentId))
        {
            throw new UserFriendlyException("部门存在子部门，无法删除！");
        }

        // 检查是否有员工关联
        if (await _repository.Queryable<EmployeeOrgModel>().AnyAsync(a => a.DepartmentId == input.DepartmentId))
        {
            throw new UserFriendlyException("部门存在员工关联，无法删除！");
        }

        DepartmentModel departmentModel = await _repository.SingleOrDefaultAsync(input.DepartmentId);
        if (departmentModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(departmentModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "删除部门",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = departmentModel.DepartmentId,
            BizNo = null,
            Description = $"删除部门：{departmentModel.DepartmentName}"
        });
    }
}
