// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Admin.Domain;
using Fast.Admin.Service.Organization.Dto;
using Fast.AdminLog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Admin.Service.Organization;

/// <summary>
/// 机构服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Admin, Name = "organization")]
public class OrganizationService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<OrganizationModel> _repository;

    public OrganizationService(IUser user, ISqlSugarRepository<OrganizationModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 机构选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("机构选择器", HttpRequestActionEnum.Query)]
    public async Task<List<ElSelectorOutput<long>>> OrganizationSelector()
    {
        ISugarQueryable<OrganizationModel> queryable = _repository.Entities;
        List<long> customDepartmentIds = _user.DataScopeDepartmentIdList ?? [];
        List<long> customOrgIds = customDepartmentIds.Count == 0
            ? []
            : await _repository.Queryable<DepartmentModel>()
                .Where(wh => customDepartmentIds.Contains(wh.DepartmentId))
                .Select(sl => sl.OrgId)
                .Distinct()
                .ToListAsync();

        // 管理员，全部权限
        if (_user.IsSuperAdmin || _user.IsAdmin || _user.DataScopeType == DataScopeTypeEnum.All)
        {
        }
        // 自定义部门数据
        else if (_user.DataScopeType == DataScopeTypeEnum.CustomDept)
        {
            queryable = queryable.Where(wh => customOrgIds.Contains(wh.OrgId));
        }
        // 本机构及以下数据
        // 本部门及以下数据
        // 本部门数据
        // 仅本人数据
        else
        {
            queryable = queryable.Where(wh => customOrgIds.Contains(wh.OrgId)
                                              || wh.OrgId
                                              == SqlFunc.Subqueryable<EmployeeOrgModel>()
                                                  // 主部门
                                                  .Where(e => e.EmployeeId == _user.EmployeeId && e.IsPrimary)
                                                  .Where(e => e.OrgId == wh.OrgId)
                                                  .Select(e => e.OrgId));
        }

        var data = await queryable.OrderBy(ob => ob.Sort)
            .Select(sl => new
            {
                sl.OrgId,
                sl.ParentId,
                sl.ParentName,
                sl.ParentIds,
                sl.ParentNames,
                sl.OrgName,
                sl.OrgCode,
                sl.Contacts,
                sl.Phone
            })
            .ToListAsync();

        return data.Select(sl => new ElSelectorOutput<long>
            {
                Value = sl.OrgId,
                Label = sl.OrgName,
                ParentId = sl.ParentId,
                Data = new
                {
                    sl.ParentName,
                    sl.ParentIds,
                    sl.ParentNames,
                    sl.OrgCode,
                    sl.Contacts,
                    sl.Phone
                }
            })
            .ToList()
            .Build();
    }

    /// <summary>
    /// 获取机构详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取机构详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Department.Detail)]
    public async Task<QueryOrganizationDetailOutput> QueryOrganizationDetail([Required(ErrorMessage = "机构Id不能为空")] long? orgId)
    {
        QueryOrganizationDetailOutput result = await _repository.Entities.Where(wh => wh.OrgId == orgId)
            .Select(sl => new QueryOrganizationDetailOutput
            {
                OrgId = sl.OrgId,
                ParentId = sl.ParentId,
                ParentName = sl.ParentName,
                ParentIds = sl.ParentIds,
                ParentNames = sl.ParentNames,
                OrgName = sl.OrgName,
                OrgCode = sl.OrgCode,
                Contacts = sl.Contacts,
                Phone = sl.Phone,
                Email = sl.Email,
                Sort = sl.Sort,
                DataPublic = sl.DataPublic,
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
    /// 添加机构
    /// </summary>
    [HttpPost]
    [ApiInfo("添加机构", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Department.Add)]
    public async Task AddOrganization(AddOrganizationInput input)
    {
        if (await _repository.AnyAsync(a => a.OrgName == input.OrgName))
        {
            throw new UserFriendlyException("机构名称重复！");
        }

        if (await _repository.AnyAsync(a => a.OrgCode == input.OrgCode))
        {
            throw new UserFriendlyException("机构编码重复！");
        }

        var organizationModel = new OrganizationModel
        {
            OrgName = input.OrgName,
            OrgCode = input.OrgCode,
            Contacts = input.Contacts,
            Phone = input.Phone,
            Email = input.Email,
            Sort = input.Sort,
            DataPublic = input.DataPublic,
            Remark = input.Remark
        };

        if (input.ParentId > 0)
        {
            OrganizationModel parentOrganization = await _repository.SingleOrDefaultAsync(s => s.OrgId == input.ParentId);
            if (parentOrganization == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            organizationModel.ParentId = parentOrganization.OrgId;
            organizationModel.ParentName = parentOrganization.OrgName;
            organizationModel.ParentIds = [..parentOrganization.ParentIds, parentOrganization.OrgId];
            organizationModel.ParentNames = [.. parentOrganization.ParentNames, parentOrganization.OrgName];
        }
        else
        {
            organizationModel.ParentId = 0;
            organizationModel.ParentName = null;
            organizationModel.ParentIds = [0];
            organizationModel.ParentNames = [];
        }

        await _repository.InsertAsync(organizationModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "添加机构",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = organizationModel.OrgId,
            BizNo = null,
            Description = $"添加机构：{organizationModel.OrgName}"
        });
    }

    /// <summary>
    /// 编辑机构
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑机构", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Department.Edit)]
    public async Task EditOrganization(EditOrganizationInput input)
    {
        if (input.ParentId == input.OrgId)
        {
            throw new UserFriendlyException("不能将自己设为父机构！");
        }

        if (await _repository.AnyAsync(a => a.OrgName == input.OrgName && a.OrgId != input.OrgId))
        {
            throw new UserFriendlyException("机构名称重复！");
        }

        if (await _repository.AnyAsync(a => a.OrgCode == input.OrgCode && a.OrgId != input.OrgId))
        {
            throw new UserFriendlyException("机构编码重复！");
        }

        OrganizationModel organizationModel = await _repository.SingleOrDefaultAsync(input.OrgId);
        if (organizationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        string description = $"编辑机构：{input.OrgName}";

        organizationModel.OrgName = input.OrgName;
        organizationModel.OrgCode = input.OrgCode;
        organizationModel.Contacts = input.Contacts;
        organizationModel.Phone = input.Phone;
        organizationModel.Email = input.Email;
        organizationModel.Sort = input.Sort;
        organizationModel.DataPublic = input.DataPublic;
        organizationModel.Remark = input.Remark;
        organizationModel.RowVersion = input.RowVersion;

        if (input.ParentId > 0)
        {
            OrganizationModel parentOrganization = await _repository.SingleOrDefaultAsync(s => s.OrgId == input.ParentId);
            if (parentOrganization == null)
            {
                throw new UserFriendlyException("数据不存在！");
            }

            if (organizationModel.ParentId != input.ParentId)
            {
                description += $"父级机构 -> {parentOrganization.OrgName}";
            }

            organizationModel.ParentId = parentOrganization.OrgId;
            organizationModel.ParentName = parentOrganization.OrgName;
            organizationModel.ParentIds = [.. parentOrganization.ParentIds, parentOrganization.OrgId];
            organizationModel.ParentNames = [.. parentOrganization.ParentNames, parentOrganization.OrgName];
        }
        else
        {
            if (organizationModel.ParentId != input.ParentId)
            {
                description += "删除父级机构";
            }

            organizationModel.ParentId = 0;
            organizationModel.ParentName = null;
            organizationModel.ParentIds = [0];
            organizationModel.ParentNames = [];
        }

        await _repository.UpdateAsync(organizationModel);

        // 更新所有子级
        List<OrganizationModel> childrenList = await _repository.Entities
            .Where(wh => SqlFunc.JsonArrayAny(wh.ParentIds, organizationModel.OrgId))
            .ToListAsync();

        void updateChildrenName(long parentId)
        {
            OrganizationModel parentModel = childrenList.Single(s => s.OrgId == parentId);
            foreach (OrganizationModel item in childrenList.Where(wh => wh.ParentId == parentId))
            {
                item.ParentIds = [.. parentModel.ParentIds, parentModel.OrgId];
                item.ParentNames = [.. parentModel.ParentNames, parentModel.OrgName];
                updateChildrenName(item.OrgId);
            }
        }

        // 处理顶层
        foreach (OrganizationModel item in childrenList.Where(wh => wh.ParentId == organizationModel.OrgId))
        {
            item.ParentName = organizationModel.OrgName;
            item.ParentIds = [.. organizationModel.ParentIds, organizationModel.OrgId];
            item.ParentNames = [.. organizationModel.ParentNames, organizationModel.OrgName];

            updateChildrenName(item.OrgId);
        }

        await _repository.Updateable(childrenList)
            .UpdateColumns(e => new {e.ParentName, e.ParentIds, e.ParentNames})
            .ExecuteCommandAsync();

        await _repository.Updateable<DepartmentModel>()
            .SetColumns(_ => new DepartmentModel {OrgName = organizationModel.OrgName})
            .Where(wh => wh.OrgId == organizationModel.OrgId)
            .ExecuteCommandAsync();

        await _repository.Updateable<EmployeeOrgModel>()
            .SetColumns(_ => new EmployeeOrgModel
            {
                OrgName = organizationModel.OrgName,
                OrgNames = new List<string>(organizationModel.ParentNames) {organizationModel.OrgName}
            })
            .Where(wh => wh.OrgId == organizationModel.OrgId)
            .ExecuteCommandAsync();

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "编辑机构",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = organizationModel.OrgId,
            BizNo = null,
            Description = description
        });
    }

    /// <summary>
    /// 删除机构
    /// </summary>
    [HttpPost]
    [ApiInfo("删除机构", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Department.Delete)]
    public async Task DeleteOrganization(OrganizationIdInput input)
    {
        if (await _repository.AnyAsync(a => a.ParentId == input.OrgId))
        {
            throw new UserFriendlyException("机构存在子机构，无法删除！");
        }

        // 检查是否有部门关联
        if (await _repository.Queryable<DepartmentModel>().AnyAsync(a => a.OrgId == input.OrgId))
        {
            throw new UserFriendlyException("机构存在部门关联，无法删除！");
        }

        // 检查是否有员工关联
        if (await _repository.Queryable<EmployeeOrgModel>().AnyAsync(a => a.OrgId == input.OrgId))
        {
            throw new UserFriendlyException("机构存在员工关联，无法删除！");
        }

        OrganizationModel organizationModel = await _repository.SingleOrDefaultAsync(input.OrgId);
        if (organizationModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(organizationModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "删除机构",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = organizationModel.OrgId,
            BizNo = null,
            Description = $"删除机构：{organizationModel.OrgName}"
        });
    }
}
