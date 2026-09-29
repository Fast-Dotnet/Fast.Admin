// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.JwtBearer;
using Fast.UnifyResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.Core;

/// <summary>
/// JWT 验证提供器
/// </summary>
public class JwtBearerHandle : IJwtBearerHandle
{
    /// <inheritdoc />
    public async Task<bool> AuthorizeHandle(AuthorizationHandlerContext context, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        if (httpContext.User.Identity?.IsAuthenticated != true)
            return false;

        // 从 AccessToken 中读取 Data
        string data = httpContext.User.FindFirst("Data")?.Value;
        if (string.IsNullOrWhiteSpace(data))
            return false;

        Dictionary<string, string> payload;
        try
        {
            payload = data.Base64ToString().ToObject<Dictionary<string, string>>();
        }
        catch
        {
            return false;
        }

        if (payload == null)
            return false;

        if (!payload.TryGetValue(nameof(AuthUserInfo.DeviceType), out string deviceTypeValue)
            || !Enum.TryParse(deviceTypeValue, true, out AppEnvironmentEnum deviceType)
            || !payload.TryGetValue(nameof(AuthUserInfo.SessionId), out string sessionId)
            || !payload.TryGetValue(nameof(AuthUserInfo.AppNo), out string appNo)
            || !payload.TryGetValue(nameof(AuthUserInfo.TenantNo), out string tenantNo)
            || !payload.TryGetValue(nameof(AuthUserInfo.EmployeeNo), out string employeeNo))
            return false;

        if (string.IsNullOrWhiteSpace(sessionId)
            || string.IsNullOrWhiteSpace(appNo)
            || string.IsNullOrWhiteSpace(tenantNo)
            || string.IsNullOrWhiteSpace(employeeNo))
            return false;

        // 请求已取消时终止后续处理
        httpContext.RequestAborted.ThrowIfCancellationRequested();

        // 获取 IUser，当前请求生命周期，只会解析一次
        IUser _user = httpContext.RequestServices.GetService<IUser>();
        // 获取授权用户信息
        AuthUserInfo authUserInfo = await _user.GetAuthUserInfo(deviceType, appNo, tenantNo, employeeNo, sessionId);
        if (authUserInfo == null)
            return false;

        if (authUserInfo.TenantId <= 0
            || authUserInfo.SessionId != sessionId
            || authUserInfo.AppNo != appNo
            || authUserInfo.TenantNo != tenantNo
            || authUserInfo.EmployeeNo != employeeNo
            || authUserInfo.DeviceType != deviceType)
            return false;


        try
        {
            // 判断设备信息是否和缓存中的一致
            if (GlobalContext.DeviceId != authUserInfo.DeviceId || GlobalContext.DeviceType != authUserInfo.DeviceType)
                return false;
        }
        catch (UserFriendlyException)
        {
            return false;
        }

        // 请求已取消时终止后续处理
        httpContext.RequestAborted.ThrowIfCancellationRequested();

        // 设置授权用户
        _user.SetAuthUser(authUserInfo);
        return true;
    }

    /// <inheritdoc />
    public async Task<object> AuthorizeFailHandle(AuthorizationHandlerContext context, HttpContext httpContext,
        Exception exception)
    {
        return await Task.FromResult(UnifyContext.GetRestfulResult(StatusCodes.Status401Unauthorized, false, null, "401 未经授权",
            httpContext));
    }

    /// <inheritdoc />
    public async Task<bool> PermissionHandle(AuthorizationHandlerContext context, IAuthorizationRequirement requirement,
        HttpContext httpContext)
    {
        // 请求已取消时终止后续处理
        httpContext.RequestAborted.ThrowIfCancellationRequested();

        // 获取 IUser
        IUser _user = httpContext.RequestServices.GetService<IUser>();

        // 超级管理员有所有的权限
        if (_user.IsSuperAdmin)
            return true;

        // 获取权限标识
        PermissionAttribute permissionAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<PermissionAttribute>();

        if (permissionAttribute?.TagList == null || permissionAttribute.TagList.Count == 0)
            return true;

        // 输出权限标识
        httpContext.Response.Headers.TryAdd("Auth-Permission", string.Join(",", permissionAttribute.TagList));

        if (_user.ButtonCodeList == null || _user.ButtonCodeList.Count == 0)
            return false;

        // 满足一个即可
        if (_user.ButtonCodeList.Intersect(permissionAttribute.TagList).Any())
            return true;

        return await Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<object> PermissionFailHandle(AuthorizationHandlerContext context, IAuthorizationRequirement requirement,
        HttpContext httpContext, Exception exception)
    {
        return await Task.FromResult<object>(null);
    }
}
