// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Center.Domain;
using Fast.Center.Service.App.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.App;

/// <summary>
/// App
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "app")]
public class AppService : IDynamicApplication
{
    private readonly ICache<CenterCCL> _centerCache;
    private readonly ISqlSugarRepository<ApplicationModel> _repository;

    public AppService(ICache<CenterCCL> centerCache, ISqlSugarRepository<ApplicationModel> repository)
    {
        _centerCache = centerCache;
        _repository = repository;
    }

    /// <summary>
    /// 获取浏览器重定向
    /// </summary>
    [HttpGet("/queryBrowserRedirect")]
    [ApiInfo("获取浏览器重定向", HttpRequestActionEnum.Query)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ApiDescriptionSettings(false)]
    public async Task<QueryBrowserRedirectOutput> QueryBrowserRedirect([Required] [StringLength(200)] string path)
    {
        AppEnvironmentEnum deviceType = GlobalContext.DeviceType;
        if (deviceType is not (AppEnvironmentEnum.Web or AppEnvironmentEnum.H5))
        {
            return null;
        }

        string origin = GlobalContext.Origin;
        ApplicationClientModel applicationClientModel = await ApplicationContext.GetApplication(origin);
        if (!applicationClientModel.AppType.HasFlag(deviceType))
        {
            return null;
        }

        string cacheKey = CacheConst.GetCacheKey(CacheConst.Center.BrowserRedirect, origin, deviceType.ToString(), path);

        QueryBrowserRedirectOutput result = await _centerCache.GetAndSetAsync(cacheKey, TimeSpan.FromHours(2), async () =>
        {
            bool fromWeb = deviceType == AppEnvironmentEnum.Web;
            AppEnvironmentEnum targetType = fromWeb ? AppEnvironmentEnum.H5 : AppEnvironmentEnum.Web;

            string host = await _repository.Queryable<ApplicationClientModel>()
                .Where(wh => wh.AppId == applicationClientModel.AppId)
                .Where(wh => wh.EnvironmentType == applicationClientModel.EnvironmentType)
                .Where(wh => wh.AppKey != applicationClientModel.AppKey)
                .Where(wh => (wh.AppType & targetType) != 0)
                .OrderBy(ob => ob.RecordId)
                .Select(sl => sl.AppKey)
                .FirstAsync();

            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

            // 首页和登录页独立于业务菜单。
            string targetPath = fromWeb
                ? path switch
                {
                    "/" or "/dashboard" => "/pages/tabBar/workbench/index",
                    "/login" => "/pages/login/index",
                    _ => null
                }
                : path switch
                {
                    "/"
                        or "/pages/launcher/index"
                        or "/pages/tabBar/home/index"
                        or "/pages/tabBar/workbench/index"
                        or "/pages/tabBar/my/index" => "/dashboard",
                    "/pages/login/index" => "/login",
                    _ => null
                };

            if (targetPath == null)
            {
                MenuModel menu = await _repository.Queryable<MenuModel>()
                    .Where(wh => wh.AppId == applicationClientModel.AppId)
                    .Where(wh => wh.Status == CommonStatusEnum.Enable)
                    .Where(wh => wh.MenuType == MenuTypeEnum.Menu)
                    .Where(wh => wh.HasWeb && wh.HasMobile)
                    .WhereIF(fromWeb, wh => wh.WebRouter == path)
                    .WhereIF(!fromWeb, wh => wh.MobileRouter == path)
                    .OrderBy(ob => ob.MenuId)
                    .FirstAsync();

                targetPath = fromWeb ? menu?.MobileRouter : menu?.WebRouter;
            }

            return new QueryBrowserRedirectOutput {Host = host, Path = targetPath};
        });

        if (result == null || string.IsNullOrWhiteSpace(result.Host) || string.IsNullOrWhiteSpace(result.Path))
        {
            return null;
        }

        return result;
    }

    /// <summary>
    /// Launch
    /// </summary>
    [HttpPost("/launch")]
    [ApiInfo("Launch", HttpRequestActionEnum.Auth)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseEncipher]
    public async Task<LaunchOutput> Launch()
    {
        ApplicationClientModel applicationClientModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
        if (applicationClientModel == null || !applicationClientModel.AppType.HasFlag(GlobalContext.DeviceType))
        {
            throw new UserFriendlyException("非法访问！");
        }

        return new LaunchOutput
        {
            Edition = applicationClientModel.Application.Edition,
            AppNo = applicationClientModel.Application.AppNo,
            AppName = applicationClientModel.Application.AppName,
            LogoUrl = applicationClientModel.Application.LogoUrl,
            ThemeColor = applicationClientModel.Application.ThemeColor,
            AppType = applicationClientModel.AppType,
            EnvironmentType = applicationClientModel.EnvironmentType,
            LoginComponent = applicationClientModel.LoginComponent,
            RequestTimeout = applicationClientModel.RequestTimeout,
            RequestEncipher = applicationClientModel.RequestEncipher,
            TenantName = applicationClientModel.Application.TenantName
        };
    }

    /// <summary>
    /// 检查应用更新
    /// </summary>
    [HttpPost]
    [ApiInfo("检查应用更新", HttpRequestActionEnum.Query)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<CheckAppUpdateOutput> CheckAppUpdate(CheckAppUpdateInput input)
    {
        if (GlobalContext.DeviceType is not (AppEnvironmentEnum.Android
            or AppEnvironmentEnum.HarmonyOS
            or AppEnvironmentEnum.IOS
            or AppEnvironmentEnum.Windows))
        {
            throw new UserFriendlyException("非法的应用客户端！");
        }

        string appKey = GlobalContext.Origin;

        // 独立客户端不加载 AOP，避免写日志时再次产生 SQL 日志
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        ApplicationClientModel applicationClientModel = await ApplicationContext.GetApplication(appKey);
        if (applicationClientModel == null || !applicationClientModel.AppType.HasFlag(GlobalContext.DeviceType))
        {
            throw new UserFriendlyException("非法的应用客户端！");
        }

        // 获取所有版本号
        List<ApplicationVersionModel> applicationVersionList = await db.Queryable<ApplicationVersionModel>()
            .Where(wh => wh.AppKey == appKey && wh.Status == CommonStatusEnum.Enable && wh.VersionCode >= input.NativeVersionCode)
            .Where(wh => (wh.AppType & GlobalContext.DeviceType) != 0)
            .ToListAsync();

        // 选择已启用且兼容的最高版本；目标版本相同时优先 WGT。
        int currentVersionCode = Math.Max(input.NativeVersionCode, input.ResourceVersionCode ?? input.NativeVersionCode);
        ApplicationVersionModel applicationVersionModel = applicationVersionList.Where(wh => wh.PackageType switch
            {
                ApplicationPackageTypeEnum.FullPackage => wh.VersionCode > currentVersionCode,
                ApplicationPackageTypeEnum.Wgt => wh.VersionCode > currentVersionCode
                                                  && wh.MinNativeVersionCode.HasValue
                                                  && wh.MaxNativeVersionCode.HasValue
                                                  && input.NativeVersionCode >= wh.MinNativeVersionCode.Value
                                                  && input.NativeVersionCode <= wh.MaxNativeVersionCode.Value,
                _ => false
            })
            .OrderByDescending(ob => ob.VersionCode)
            .ThenByDescending(ob => ob.PackageType == ApplicationPackageTypeEnum.Wgt)
            .FirstOrDefault();

        if (applicationVersionModel == null)
        {
            // 无最新包
            return new CheckAppUpdateOutput {HasUpdate = false};
        }

        FileModel fileModel = await db.Queryable<FileModel>()
            .Where(wh => wh.FileLocation == applicationVersionModel.FileUrl)
            .SingleAsync();
        if (fileModel == null)
        {
            throw new UserFriendlyException("文件不存在或存储地址不受支持！");
        }

        return new CheckAppUpdateOutput
        {
            HasUpdate = true,
            Package = new CheckAppUpdatePackageOutput
            {
                RecordId = applicationVersionModel.RecordId,
                PackageType = applicationVersionModel.PackageType,
                VersionName = applicationVersionModel.VersionName,
                VersionCode = applicationVersionModel.VersionCode,
                IsForceUpdate = applicationVersionModel.IsForceUpdate,
                UpdateContent = applicationVersionModel.UpdateContent,
                FileName = fileModel.FileOriginName,
                FileSizeKb = fileModel.FileSizeKb,
                FileSha256 = fileModel.FileHash,
                DownloadUrl =
                    $"{applicationVersionModel.FileUrl}?recordId={applicationVersionModel.RecordId}&appKey={Uri.EscapeDataString(applicationVersionModel.AppKey)}"
            }
        };
    }

    /// <summary>
    /// 下载应用
    /// </summary>
    [HttpGet("/app/download/{appId:long}")]
    [ApiInfo("下载应用", HttpRequestActionEnum.Other)]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ApiDescriptionSettings(false)]
    public async Task DownloadApp([Range(1, long.MaxValue)] long appId)
    {
    }
}
