// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.AdminLog.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;

namespace Fast.Admin.Service;

/// <summary>
/// 日志上下文
/// </summary>
[SuppressSniffer]
public class LogContext
{
    /// <summary>
    /// 添加操作日志，并等待日志持久化完成
    /// </summary>
    public static async Task OperateLog(OperateLogDto logDto)
    {
        HttpContext httpContext = FastContext.HttpContext;
        IUser _user = httpContext.RequestServices.GetRequiredService<IUser>();

        // 组装数据
        var operateLogModel = new OperateLogModel
        {
            RecordId = YitIdHelper.NextId(),
            EmployeeNo = _user.EmployeeNo,
            Mobile = _user.Mobile,
            Title = logDto.Title?.GetNVarcharMaxLen(50, true),
            OperateType = logDto.OperateType,
            BizId = logDto.BizId,
            BizNo = logDto.BizNo,
            Description = logDto.Description?.GetNVarcharMaxLen(500, true),
            DepartmentId = _user.DepartmentId,
            DepartmentName = _user.DepartmentName,
            CreatedUserId = _user.EmployeeId,
            CreatedUserName = _user.EmployeeName,
            CreatedTime = DateTime.Now
        };
        operateLogModel.RecordCreate(httpContext);

        // 获取 AdminLog 库的连接字符串配置
        ISqlSugarEntityService sqlSugarEntityService = httpContext.RequestServices.GetRequiredService<ISqlSugarEntityService>();
        ConnectionSettingsOptions connectionSetting =
            await sqlSugarEntityService.GetConnectionSetting(_user.TenantId, _user.TenantNo, DatabaseTypeEnum.AdminLog);
        ConnectionConfig connectionConfig = SqlSugarContext.GetConnectionConfig(connectionSetting);

        // 独立客户端不加载 AOP，避免操作日志写入再次触发 SQL 审计；返回业务响应前等待写入完成
        using var db = new SqlSugarClient(connectionConfig);
        await db.Insertable(operateLogModel).SplitTable().ExecuteCommandAsync();
    }
}

/// <summary>
/// 操作日志上下文数据
/// </summary>
public class OperateLogDto
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public OperateLogTypeEnum OperateType { get; set; }

    /// <summary>
    /// 业务Id
    /// </summary>
    public long? BizId { get; set; }

    /// <summary>
    /// 业务编码
    /// </summary>
    public string BizNo { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}
