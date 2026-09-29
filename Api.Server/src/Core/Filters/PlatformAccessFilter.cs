// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.Core;

/// <summary>
/// 平台控制面访问边界过滤器
/// </summary>
public sealed class PlatformAccessFilter : IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 获取 PlatformOnly 特性，Controller 和 Action 同时存在时优先使用 Action
        if (!context.ActionDescriptor.EndpointMetadata.OfType<PlatformOnlyAttribute>().Any())
        {
            return;
        }

        HttpContext httpContext = context.HttpContext;
        IUser _user = httpContext.RequestServices.GetService<IUser>();

        if (_user.IsSuperAdmin)
        {
            return;
        }

        if (!_user.IsSystemTenant)
        {
            throw new UserFriendlyException("该接口仅允许系统租户访问！", HttpStatusCode.Forbidden);
        }

        await Task.CompletedTask;
    }
}
