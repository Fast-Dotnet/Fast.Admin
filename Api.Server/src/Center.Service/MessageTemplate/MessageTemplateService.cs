// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.MessageTemplate.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.MessageTemplate;

/// <summary>
/// 消息模板服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "messageTemplate")]
[PlatformOnly]
public class MessageTemplateService : IDynamicApplication
{
    private readonly ISqlSugarRepository<MessageTemplateModel> _repository;

    public MessageTemplateService(ISqlSugarRepository<MessageTemplateModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取消息模板分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取消息模板分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.MessageTemplate.Paged)]
    public async Task<PagedResult<QueryMessageTemplatePagedOutput>> QueryMessageTemplatePaged(
        QueryMessageTemplatePagedInput input)
    {
        return await _repository.Entities.WhereIF(input.TenantId != null, wh => wh.TenantId == input.TenantId)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryMessageTemplatePagedOutput
            {
                RecordId = sl.RecordId,
                TemplateType = sl.TemplateType,
                TemplateCode = sl.TemplateCode,
                TenantName = sl.TenantName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取消息模板详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取消息模板详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.MessageTemplate.Detail)]
    public async Task<QueryMessageTemplateDetailOutput> QueryMessageTemplateDetail(
        [Required(ErrorMessage = "记录Id不能为空")] long? recordId)
    {
        QueryMessageTemplateDetailOutput result = await _repository.Entities.Where(wh => wh.RecordId == recordId)
            .Select(sl => new QueryMessageTemplateDetailOutput
            {
                RecordId = sl.RecordId,
                TemplateType = sl.TemplateType,
                TemplateCode = sl.TemplateCode,
                TenantId = sl.TenantId,
                TenantName = sl.TenantName,
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
    /// 添加消息模板
    /// </summary>
    [HttpPost]
    [ApiInfo("添加消息模板", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.MessageTemplate.Add)]
    public async Task AddMessageTemplate(AddMessageTemplateInput input)
    {
        if (await _repository.AnyAsync(a => a.TenantId == input.TenantId && a.TemplateType == input.TemplateType))
        {
            throw new UserFriendlyException("模板类型重复！");
        }

        var messageTemplateModel = new MessageTemplateModel
        {
            TemplateType = input.TemplateType,
            TemplateCode = input.TemplateCode,
            TenantId = input.TenantId,
            TenantName = input.TenantName
        };

        await _repository.InsertAsync(messageTemplateModel);
        // 删除缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.TenantId);
    }

    /// <summary>
    /// 编辑消息模板
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑消息模板", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.MessageTemplate.Edit)]
    public async Task EditMessageTemplate(EditMessageTemplateInput input)
    {
        if (await _repository.AnyAsync(a =>
                a.TenantId == input.TenantId && a.TemplateType == input.TemplateType && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("模板类型重复！");
        }

        MessageTemplateModel messageTemplateModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (messageTemplateModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 删除旧的缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.TenantId);

        messageTemplateModel.TemplateType = input.TemplateType;
        messageTemplateModel.TemplateCode = input.TemplateCode;
        messageTemplateModel.TenantId = input.TenantId;
        messageTemplateModel.TenantName = input.TenantName;
        messageTemplateModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(messageTemplateModel);
        // 删除缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.TenantId);
    }

    /// <summary>
    /// 删除消息模板
    /// </summary>
    [HttpPost]
    [ApiInfo("删除消息模板", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.MessageTemplate.Delete)]
    public async Task DeleteMessageTemplate(DeleteMessageTemplateInput input)
    {
        MessageTemplateModel messageTemplateModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (messageTemplateModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(messageTemplateModel);
        // 删除缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.TenantId);
    }
}
