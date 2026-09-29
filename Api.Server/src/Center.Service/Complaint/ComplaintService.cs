// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Complaint.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Complaint;

/// <summary>
/// 投诉服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "Complaint")]
public class ComplaintService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<ComplaintModel> _repository;

    public ComplaintService(IUser user, ISqlSugarRepository<ComplaintModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 获取投诉工单分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取投诉工单分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Complaint.Paged)]
    public async Task<PagedResult<QueryComplaintPagedOutput>> QueryComplaintPaged(QueryComplaintPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.ComplaintType != null, wh => wh.ComplaintType == input.ComplaintType)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryComplaintPagedOutput
            {
                ComplaintId = sl.ComplaintId,
                AppName = sl.AppName,
                OpenId = sl.OpenId,
                NickName = sl.NickName,
                ComplaintType = sl.ComplaintType,
                Mobile = sl.Mobile,
                ContactPhone = sl.ContactPhone,
                ContactEmail = sl.ContactEmail,
                Description = sl.Description,
                AttachmentImages = sl.AttachmentImages,
                HandleTime = sl.HandleTime,
                HandleDescription = sl.HandleDescription,
                Remark = sl.Remark,
                CreatedTime = sl.CreatedTime,
                TenantName = sl.TenantName,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取用户投诉分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取用户投诉分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Complaint.TenantPaged)]
    public async Task<PagedResult<QueryComplaintPagedOutput>> QueryTenantComplaintPaged(QueryComplaintPagedInput input)
    {
        return await _repository.Entities.Where(wh => wh.TenantId == _user.TenantId)
            .WhereIF(input.ComplaintType != null, wh => wh.ComplaintType == input.ComplaintType)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryComplaintPagedOutput
            {
                ComplaintId = sl.ComplaintId,
                AppName = sl.AppName,
                OpenId = sl.OpenId,
                NickName = sl.NickName,
                ComplaintType = sl.ComplaintType,
                Mobile = sl.Mobile,
                ContactPhone = sl.ContactPhone,
                ContactEmail = sl.ContactEmail,
                Description = sl.Description,
                AttachmentImages = sl.AttachmentImages,
                HandleTime = sl.HandleTime,
                HandleDescription = sl.HandleDescription,
                Remark = sl.Remark,
                CreatedTime = sl.CreatedTime,
                TenantName = sl.TenantName,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取投诉详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取投诉详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Complaint.Detail, PermissionConst.Complaint.TenantDetail)]
    public async Task<QueryComplaintPagedOutput> QueryComplaintDetail([Required(ErrorMessage = "投诉Id不能为空")] long? complaintId)
    {
        QueryComplaintPagedOutput result = await _repository.Entities.Where(wh => wh.ComplaintId == complaintId)
            .Select(sl => new QueryComplaintPagedOutput
            {
                ComplaintId = sl.ComplaintId,
                AppName = sl.AppName,
                OpenId = sl.OpenId,
                NickName = sl.NickName,
                ComplaintType = sl.ComplaintType,
                Mobile = sl.Mobile,
                ContactPhone = sl.ContactPhone,
                ContactEmail = sl.ContactEmail,
                Description = sl.Description,
                AttachmentImages = sl.AttachmentImages,
                HandleTime = sl.HandleTime,
                HandleDescription = sl.HandleDescription,
                Remark = sl.Remark,
                CreatedTime = sl.CreatedTime,
                TenantName = sl.TenantName,
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
    /// 添加投诉
    /// </summary>
    [HttpPost]
    [ApiInfo("添加投诉", HttpRequestActionEnum.Add)]
    public async Task AddComplaint(AddComplaintInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        var complaintModel = new ComplaintModel
        {
            AppId = applicationModel.AppId,
            AppName = applicationModel.Application.AppName,
            OpenId = applicationModel.OpenId,
            UserId = _user.EmployeeId,
            NickName = _user.NickName,
            ComplaintType = input.ComplaintType,
            Mobile = _user.Mobile,
            ContactPhone = input.ContactPhone,
            ContactEmail = input.ContactEmail,
            Description = input.Description,
            AttachmentImages = input.AttachmentImages,
            TenantId = applicationModel.Application.TenantId,
            TenantName = applicationModel.Application.TenantName
        };
        await _repository.InsertAsync(complaintModel);
    }

    /// <summary>
    /// 处理投诉
    /// </summary>
    [HttpPost]
    [ApiInfo("处理投诉", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Complaint.Handle, PermissionConst.Complaint.TenantHandle)]
    public async Task HandleComplaint(HandleComplaintInput input)
    {
        ComplaintModel complaintModel = await _repository.SingleOrDefaultAsync(input.ComplaintId);
        if (complaintModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        complaintModel.HandleTime = DateTime.Now;
        complaintModel.HandleDescription = input.HandleDescription;
        complaintModel.Remark = input.Remark;
        complaintModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(complaintModel);
    }
}
