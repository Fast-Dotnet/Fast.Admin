// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Fast.Core;

/// <summary>
/// <see cref="ITenant"/> 默认实现
/// </summary>
/// <remarks>
/// <para>不代表操作人已经登录或具有管理权限</para>
/// <para>作用域注册，保证当前请求管道中是唯一的，并且只会加载一次</para>
/// </remarks>
public sealed class Tenant : ITenant, IScopedDependency
{
    /// <summary>
    /// 线程锁
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 授权用户信息
    /// </summary>
    private readonly IUser _user;

    /// <summary>
    /// 请求上下文
    /// </summary>
    private readonly HttpContext _httpContext;

    /// <summary>
    /// 是否存在租户信息
    /// </summary>
    private bool _hasTenantInfo { get; set; }

    /// <summary>
    /// 是否存在用户租户信息
    /// </summary>
    private bool _hasUserTenant => _user.TenantId > 0 && !string.IsNullOrWhiteSpace(_user.TenantNo);

    /// <summary>
    /// 当前作用域的业务租户
    /// </summary>
    public Tenant(IUser user, IHttpContextAccessor httpContextAccessor)
    {
        _user = user;
        _httpContext = httpContextAccessor.HttpContext;
    }

    private long? _tenantId;
    private string _tenantNo;
    private string _tenantName;
    private string _tenantCode;
    private bool _isSystemTenant;

    /// <inheritdoc />
    public long? TenantId
    {
        get
        {
            ResolveRequired();
            return _tenantId;
        }
    }

    /// <inheritdoc />
    public string TenantNo
    {
        get
        {
            ResolveRequired();
            return _tenantNo;
        }
    }

    /// <inheritdoc />
    public string TenantName
    {
        get
        {
            ResolveRequired();
            return _tenantName;
        }
    }

    /// <inheritdoc />
    public string TenantCode
    {
        get
        {
            ResolveRequired();
            return _tenantCode;
        }
    }

    /// <inheritdoc />
    public bool IsSystemTenant
    {
        get
        {
            ResolveRequired();
            return _isSystemTenant;
        }
    }

    /// <summary>
    /// 在首次访问租户业务库前解析并固定当前租户
    /// </summary>
    /// <returns>本作用域唯一的租户结果</returns>
    /// <remarks>
    /// 有效用户上下文优先；没有用户时，仅显式匿名端点可使用服务端应用绑定的租户。
    /// 没有租户时明确失败，不回退系统租户。此方法不会填充或更改 IUser。
    /// </remarks>
    private void ResolveRequired()
    {
        if (_hasTenantInfo)
        {
            return;
        }

        lock (_lock)
        {
            // 存在授权用户信息，直接从授权用户信息中获取
            if (_hasUserTenant)
            {
                _tenantId = _user.TenantId;
                _tenantNo = _user.TenantNo;
                _tenantName = _user.TenantName;
                _tenantCode = _user.TenantCode;
                _isSystemTenant = _user.IsSystemTenant;
                _hasTenantInfo = true;
                return;
            }

            // 独立客户端不加载 AOP，避免记录写入再次触发 SQL 审计
            using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
            var data = db.Queryable<ApplicationOpenIdModel>()
                .InnerJoin<ApplicationModel>((t1, t2) => t1.AppId == t2.AppId)
                .LeftJoin<TenantModel>((t1, t2, t3) => t2.TenantId == t3.TenantId)
                .Where(t1 => t1.OpenId == GlobalContext.Origin)
                .Select((t1, t2, t3) => new
                {
                    t1.AppType,
                    ApplicationEdition = t2.Edition,
                    t2.TenantId,
                    t3.TenantNo,
                    t3.TenantName,
                    t3.TenantCode,
                    t3.TenantType,
                    Status = (CommonStatusEnum?)t3.Status,
                    Edition = (EditionEnum?)t3.Edition
                })
                .Single();

            // 请求已取消时终止后续处理
            _httpContext.RequestAborted.ThrowIfCancellationRequested();

            // 找不到租户直接返回为空
            if (data == null)
            {
                _hasTenantInfo = true;
                return;
            }

            if (data.AppType != GlobalContext.DeviceType)
            {
                throw new UserFriendlyException("未知的客户端信息！", HttpStatusCode.Forbidden);
            }

            if (data.TenantId == null || data.TenantId <= 0)
            {
                throw new UserFriendlyException("未知的应用信息！");
            }

            if (data.Status != CommonStatusEnum.Enable)
            {
                throw new UserFriendlyException("未知的应用信息！");
            }

            if (data.Edition < data.ApplicationEdition)
            {
                throw new UserFriendlyException("未知的应用信息！");
            }

            _tenantId = data.TenantId.Value;
            _tenantNo = data.TenantNo;
            _tenantName = data.TenantName;
            _tenantCode = data.TenantCode;
            _isSystemTenant = data.TenantType == TenantTypeEnum.System;
            _hasTenantInfo = true;
        }
    }
}
