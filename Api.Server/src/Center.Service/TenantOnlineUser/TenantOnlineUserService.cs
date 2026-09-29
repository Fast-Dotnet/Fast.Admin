// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Center.Domain;
using Fast.Center.Service.TenantOnlineUser.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Fast.Center.Service.TenantOnlineUser;

/// <summary>
/// 在线用户服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "tenantOnlineUser")]
public class TenantOnlineUserService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ICache<AuthCCL> _authCache;
    private readonly ISqlSugarRepository<TenantOnlineUserModel> _repository;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public TenantOnlineUserService(IUser user, ICache<AuthCCL> authCache, ISqlSugarRepository<TenantOnlineUserModel> repository,
        IHubContext<ChatHub, IChatClient> hubContext)
    {
        _user = user;
        _authCache = authCache;
        _repository = repository;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 获取在线用户分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取在线用户分页列表", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.TenantOnlineUser.Paged)]
    public async Task<PagedResult<TenantOnlineUserModel>> QueryTenantOnlineUserPaged(QueryTenantOnlineUserPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.DeviceType != null, wh => wh.DeviceType == input.DeviceType)
            .WhereIF(input.AccountId != null, wh => wh.AccountId == input.AccountId)
            .WhereIF(input.EmployeeId != null, wh => wh.EmployeeId == input.EmployeeId)
            .OrderByIF(input.IsOrderBy, ob => ob.IsOnline, OrderByType.Desc)
            .OrderByIF(input.IsOrderBy, ob => ob.LastLoginTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 强制下线
    /// </summary>
    [HttpPost]
    [ApiInfo("强制下线", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.TenantOnlineUser.ForceOffline)]
    public async Task ForceOffline(ForceOfflineInput input)
    {
        TenantOnlineUserModel onlineUser = await _repository
            .Entities.Where(wh => wh.ConnectionId == input.ConnectionId)
            .SingleAsync();
        if (onlineUser == null)
        {
            throw new UserFriendlyException("在线会话不存在或已下线！");
        }

        // 同一次登录可能建立多个 SignalR 连接，按会话统一下线，不影响关闭单点登录后的其他独立会话。
        List<TenantOnlineUserModel> onlineUsers = await _repository.Entities.Where(wh => wh.IsOnline)
            .WhereIF(!string.IsNullOrWhiteSpace(onlineUser.SessionId), wh => wh.SessionId == onlineUser.SessionId)
            .WhereIF(string.IsNullOrWhiteSpace(onlineUser.SessionId), wh => wh.ConnectionId == input.ConnectionId)
            .ToListAsync();
        if (onlineUsers.Count == 0)
        {
            throw new UserFriendlyException("在线会话不存在或已下线！");
        }

        DateTime offlineTime = DateTime.Now;
        var connectionIds = onlineUsers.Select(sl => sl.ConnectionId).Distinct().ToList();
        await _hubContext.Clients.Clients(connectionIds)
            .ForceOffline(new ForceOfflineOutput
            {
                IsAdmin = _user.IsSuperAdmin || _user.IsAdmin,
                NickName = _user.NickName,
                EmployeeNo = _user.EmployeeNo,
                OfflineTime = offlineTime
            });

        if (!string.IsNullOrWhiteSpace(onlineUser.SessionId))
        {
            string cacheKey = CacheConst.GetCacheKey(CacheConst.AuthUser, onlineUser.AppNo, _user.TenantNo, onlineUser.DeviceType,
                onlineUser.EmployeeNo, onlineUser.SessionId);
            await _authCache.DelAsync(cacheKey);
        }

        onlineUsers.ForEach(item =>
        {
            item.IsOnline = false;
            item.OfflineTime = offlineTime;
        });
        await _repository.Updateable(onlineUsers).ExecuteCommandAsync();
    }
}
