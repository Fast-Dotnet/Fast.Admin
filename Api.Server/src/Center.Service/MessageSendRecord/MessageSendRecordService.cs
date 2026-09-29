// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.MessageSendRecord.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.MessageSendRecord;

/// <summary>
/// 消息发送记录服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "messageSendRecord")]
[PlatformOnly]
public class MessageSendRecordService : IDynamicApplication
{
    private readonly ISqlSugarRepository<MessageSendRecordModel> _repository;

    public MessageSendRecordService(ISqlSugarRepository<MessageSendRecordModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取消息发送记录分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取消息发送记录分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.MessageSendRecord.Paged)]
    public async Task<PagedResult<QueryMessageSendRecordPagedOutput>> QueryMessageSendRecordPaged(
        QueryMessageSendRecordPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.Channel != null, wh => wh.Channel == input.Channel)
            .WhereIF(input.IsSuccess != null, wh => wh.IsSuccess == input.IsSuccess)
            .Select(sl => new QueryMessageSendRecordPagedOutput
            {
                RecordId = sl.RecordId,
                Channel = sl.Channel,
                Receiver = sl.Receiver,
                Title = sl.Title,
                IsSuccess = sl.IsSuccess,
                Device = sl.Device,
                OS = sl.OS,
                Browser = sl.Browser,
                Province = sl.Province,
                City = sl.City,
                Ip = sl.Ip,
                CreatedTime = sl.CreatedTime
            })
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取消息发送记录详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取消息发送记录详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.MessageSendRecord.Detail)]
    public async Task<QueryMessageSendRecordDetailOutput> QueryMessageSendRecordDetail(
        [Required(ErrorMessage = "记录Id不能为空")] long? recordId)
    {
        QueryMessageSendRecordDetailOutput result = await _repository.Entities.Where(wh => wh.RecordId == recordId)
            .Select(sl => new QueryMessageSendRecordDetailOutput
            {
                RecordId = sl.RecordId,
                Channel = sl.Channel,
                Receiver = sl.Receiver,
                Title = sl.Title,
                RecordValue = sl.RecordValue,
                IsSuccess = sl.IsSuccess,
                Device = sl.Device,
                OS = sl.OS,
                Browser = sl.Browser,
                Province = sl.Province,
                City = sl.City,
                Ip = sl.Ip,
                CreatedTime = sl.CreatedTime
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }
}
