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
    public async Task<PagedResult<QueryMessageTemplatePagedOutput>> QueryMessageTemplatePaged(PagedInput input)
    {
        return await _repository.Entities.LeftJoin<ApplicationClientModel>((t1, t2) => t1.AppKey == t2.AppKey)
            .LeftJoin<ApplicationModel>((t1, t2, t3) => t2.AppId == t3.AppId)
            .SelectMergeTable((t1, t2, t3) => new QueryMessageTemplatePagedOutput
            {
                RecordId = t1.RecordId,
                AppName = t3.AppName,
                AppKey = t1.AppKey,
                AppType = t2.AppType,
                EnvironmentType = t2.EnvironmentType,
                TemplateType = t1.TemplateType,
                TemplateCode = t1.TemplateCode,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
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
        QueryMessageTemplateDetailOutput result = await _repository.Entities
            .LeftJoin<ApplicationClientModel>((t1, t2) => t1.AppKey == t2.AppKey)
            .Where(t1 => t1.RecordId == recordId)
            .Select((t1, t2) => new QueryMessageTemplateDetailOutput
            {
                RecordId = t1.RecordId,
                AppKey = t1.AppKey,
                AppType = t2.AppType,
                EnvironmentType = t2.EnvironmentType,
                TemplateType = t1.TemplateType,
                TemplateCode = t1.TemplateCode,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion
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
        if (await _repository.AnyAsync(a => a.AppKey == input.AppKey && a.TemplateType == input.TemplateType))
        {
            throw new UserFriendlyException("模板类型重复！");
        }

        if (!string.IsNullOrWhiteSpace(input.AppKey)
            && !await _repository.Queryable<ApplicationClientModel>().AnyAsync(a => a.AppKey == input.AppKey))
        {
            throw new UserFriendlyException("数据不存在！");
        }

        var messageTemplateModel = new MessageTemplateModel
        {
            AppKey = input.AppKey, TemplateType = input.TemplateType, TemplateCode = input.TemplateCode
        };

        await _repository.InsertAsync(messageTemplateModel);
        // 删除缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.AppKey);
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
                a.AppKey == input.AppKey && a.TemplateType == input.TemplateType && a.RecordId != input.RecordId))
        {
            throw new UserFriendlyException("模板类型重复！");
        }

        MessageTemplateModel messageTemplateModel = await _repository.SingleOrDefaultAsync(input.RecordId);
        if (messageTemplateModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (!string.IsNullOrWhiteSpace(input.AppKey)
            && !await _repository.Queryable<ApplicationClientModel>().AnyAsync(a => a.AppKey == input.AppKey))
        {
            throw new UserFriendlyException("数据不存在！");
        }

        // 删除旧的缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.AppKey);

        messageTemplateModel.AppKey = input.AppKey;
        messageTemplateModel.TemplateType = input.TemplateType;
        messageTemplateModel.TemplateCode = input.TemplateCode;
        messageTemplateModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(messageTemplateModel);
        // 删除缓存
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.AppKey);
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
        await MessageTemplateContext.DeleteMessageTemplate(messageTemplateModel.TemplateType, messageTemplateModel.AppKey);
    }
}
