// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using System.Text;
using Fast.Center.Domain;
using Fast.UnifyResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Fast.Center.Service.App;

public partial class AppService
{
    /// <summary>
    /// 下载应用
    /// </summary>
    [HttpGet("/app/download/{appId:long}")]
    [ApiInfo("下载应用", HttpRequestActionEnum.Other)]
    [NonUnify]
    [AllowAnonymous]
    [DisabledRequestLog]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ApiDescriptionSettings(false)]
    public async Task<IActionResult> DownloadApp([Range(1, long.MaxValue)] long appId)
    {
        // 浏览器扫码不携带客户端身份头；使用独立客户端，避免匿名查询触发用户相关 AOP。
        using var db = new SqlSugarClient(SqlSugarContext.GetConnectionConfig(SqlSugarContext.ConnectionSettings));
        ApplicationModel application = await db.Queryable<ApplicationModel>().InSingleAsync(appId);
        if (application == null)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status404NotFound,
                ContentType = "text/html; charset=utf-8",
                Content = PageHtmlRender("应用不存在", null, [], null)
            };
        }

        List<ApplicationClientModel> clients = await db.Queryable<ApplicationClientModel>()
            .Where(a => a.AppId == appId && a.EnvironmentType == EnvironmentTypeEnum.Production)
            .ToListAsync();
        var appKeys = clients.Select(a => a.AppKey).ToList();
        List<ApplicationVersionModel> versions = appKeys.Count == 0
            ? []
            : await db.Queryable<ApplicationVersionModel>()
                .Where(a => appKeys.Contains(a.AppKey)
                            && a.Status == CommonStatusEnum.Enable
                            && a.PackageType == ApplicationPackageTypeEnum.FullPackage)
                .OrderByDescending(a => a.VersionCode)
                .ToListAsync();
        var options = new List<AppDownloadOption>();
        foreach ((AppEnvironmentEnum platform, string name) in new[]
                 {
                     (AppEnvironmentEnum.Android, "Android"), (AppEnvironmentEnum.IOS, "iOS"),
                     (AppEnvironmentEnum.HarmonyOS, "HarmonyOS"), (AppEnvironmentEnum.Windows, "Windows")
                 })
        {
            var matches = clients.Where(a => (a.AppType & platform) != 0).ToList();
            // 历史重复配置不随机取一条；维护端消除冲突后才开放此平台下载。
            if (matches.Count != 1)
            {
                continue;
            }

            ApplicationClientModel client = matches[0];
            if (platform == AppEnvironmentEnum.IOS)
            {
                if (!string.IsNullOrEmpty(client.AppStoreId) && client.AppStoreId.All(c => c is >= '0' and <= '9'))
                {
                    options.Add(new AppDownloadOption(name, $"https://apps.apple.com/app/id{client.AppStoreId}", null, null));
                }

                continue;
            }

            ApplicationVersionModel version = versions.FirstOrDefault(a => a.AppKey == client.AppKey && a.AppType == platform);
            if (version == null)
            {
                continue;
            }

            FileModel file = await db.Queryable<FileModel>().Where(a => a.FileLocation == version.FileUrl).SingleAsync();
            if (file == null || !IsDownloadPackage(platform, file.FileSuffix))
            {
                continue;
            }

            string url = $"{version.FileUrl}{(version.FileUrl.Contains('?') ? '&' : '?')}recordId={version.RecordId}"
                         + $"&appKey={Uri.EscapeDataString(client.AppKey)}";
            if (!IsHttpUrl(url))
            {
                continue;
            }

            options.Add(new AppDownloadOption(name, url, version.VersionName, version.UpdateContent, file.FileSizeKb,
                version.CreatedTime));
        }

        string userAgent = _httpContext.Request.Headers.UserAgent.ToString();
        bool isMobile =
            new[] {"iPhone", "iPad", "iPod", "Android", "HarmonyOS", "OpenHarmony"}.Any(value =>
                userAgent.Contains(value, StringComparison.OrdinalIgnoreCase));
        string qrCodeData = null;
        // 手机端不展示扫码入口，省去二维码生成与内嵌图片；未知设备保留桌面扫码能力。
        if (options.Count > 0 && !isMobile)
        {
            string pageUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}"
                             + $"{_httpContext.Request.PathBase}{_httpContext.Request.Path}";
            using SKBitmap qrCode = ImageUtil.GenQrCode(pageUrl, 280, 280);
            qrCodeData = $"data:image/png;base64,{ImageUtil.ConvertToBase64Image(qrCode)}";
        }

        return new ContentResult
        {
            ContentType = "text/html; charset=utf-8",
            Content = PageHtmlRender(application.AppName, application.LogoUrl, options, qrCodeData)
        };
    }

    /// <summary>
    /// 只允许 HTTP(S) 下载地址，防止持久化配置成为脚本入口。
    /// </summary>
    internal static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri uri)
               && uri.Scheme is "http" or "https"
               && string.IsNullOrEmpty(uri.UserInfo);
    }

    /// <summary>
    /// 对外下载渠道，不包含应用密钥等管理字段
    /// </summary>
    internal sealed record AppDownloadOption(
        string Platform,
        string Url,
        string Version,
        string UpdateContent,
        long? FileSizeKb = null,
        DateTime? PublishedAt = null);

    /// <summary>
    /// 下载页仅提供可首次安装的对应平台整包。
    /// </summary>
    private static bool IsDownloadPackage(AppEnvironmentEnum platform, string suffix)
    {
        return platform switch
        {
            AppEnvironmentEnum.Android => string.Equals(suffix, "apk", StringComparison.OrdinalIgnoreCase),
            AppEnvironmentEnum.HarmonyOS => string.Equals(suffix, "app", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(suffix, "hap", StringComparison.OrdinalIgnoreCase),
            AppEnvironmentEnum.Windows => string.Equals(suffix, "exe", StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(suffix, "msi", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// 将数据库内容编码为文本或属性值后输出，更新说明不支持 HTML。
    /// </summary>
    internal static string PageHtmlRender(string appName, string logoUrl, List<AppDownloadOption> options, string qrCode)
    {
        const string downloadIcon =
            """<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3v12m-5-5 5 5 5-5M5 16v5h14v-5"/></svg>""";
        const string arrowIcon =
            """<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M7 17 17 7M7 7h10v10"/></svg>""";
        const string phoneIcon =
            """<svg class="icon icon--harmony" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="6" y="2" width="12" height="20" rx="2"/><path d="M11 18h2"/></svg>""";
        const string androidIcon =
            """<svg class="icon icon--android platform-android" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="m6.4 6.4-2-3 .8-.6 2.1 3.1a10 10 0 0 1 9.4 0l2.1-3.1.8.6-2 3A9 9 0 0 1 21 13H3a9 9 0 0 1 3.4-6.6ZM3 14h18v6H3Z"/><circle cx="8" cy="10" r="1" fill="var(--icon-cutout, var(--surface))"/><circle cx="16" cy="10" r="1" fill="var(--icon-cutout, var(--surface))"/></svg>""";
        // Apple 轮廓来自 Simple Icons（CC0），内嵌以避免额外请求。
        const string appleIcon =
            """<svg class="icon icon--apple" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12.152 6.896c-.948 0-2.415-1.078-3.96-1.04-2.04.027-3.91 1.183-4.961 3.014-2.117 3.675-.546 9.103 1.519 12.09 1.013 1.454 2.208 3.09 3.792 3.039 1.52-.065 2.09-.987 3.935-.987 1.831 0 2.35.987 3.96.948 1.637-.026 2.676-1.48 3.676-2.948 1.156-1.688 1.636-3.325 1.662-3.415-.039-.013-3.182-1.221-3.22-4.857-.026-3.04 2.48-4.494 2.597-4.559-1.429-2.09-3.623-2.324-4.39-2.376-2-.156-3.675 1.09-4.61 1.09zM15.53 3.83c.843-1.012 1.4-2.427 1.245-3.83-1.207.052-2.662.805-3.532 1.818-.78.896-1.454 2.338-1.273 3.714 1.338.104 2.715-.688 3.559-1.701"/></svg>""";
        const string desktopIcon =
            """<svg class="icon icon--windows" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M2 3h9v8H2Zm11 0h9v8h-9ZM2 13h9v8H2Zm11 0h9v8h-9Z"/></svg>""";
        const string linkIcon =
            """<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m10 13 4-4m-5 7-1 1a4 4 0 0 1-6-6l4-4a4 4 0 0 1 6 0m0 10a4 4 0 0 0 6 0l4-4a4 4 0 0 0-6-6l-1 1"/></svg>""";
        var channels = new StringBuilder();
        var platformButtons = new StringBuilder();
        var updates = new StringBuilder();
        int availableCount = 0;
        foreach (AppDownloadOption option in options.Where(option => IsHttpUrl(option.Url)))
        {
            availableCount++;
            string platform = WebUtility.HtmlEncode(option.Platform);
            string platformGlyph = option.Platform switch
            {
                "Android" => androidIcon,
                "iOS" => appleIcon,
                "Windows" => desktopIcon,
                _ => phoneIcon
            };
            // 四种平台图标统一使用固定尺寸容器，避免 SVG 的画布留白影响文字对齐。
            string platformIcon = $"<span class=\"platform-icon\">{platformGlyph}</span>";
            string description = option.Version == null ? "通过 App Store 获取最新版本" :
                option.Version.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? option.Version : $"v{option.Version}";
            if (option.FileSizeKb.HasValue)
            {
                description += option.FileSizeKb.Value < 1024
                    ? $" · {option.FileSizeKb.Value} KB"
                    : $" · {option.FileSizeKb.Value / 1024d:0.#} MB";
            }

            if (option.PublishedAt.HasValue)
            {
                description += $" · {option.PublishedAt.Value:yyyy-MM-dd}";
            }

            string installHint = option.Platform switch
            {
                "iOS" => "将在 App Store 中查看并下载应用",
                "HarmonyOS" => "原生鸿蒙包请使用设备支持的安装方式",
                "Windows" => "下载后运行安装程序，按提示完成安装",
                _ => "下载后，请按照系统提示完成安装"
            };
            string actionText = option.Platform == "iOS" ? "前往 App Store" : $"下载 {platform} 版";
            channels.Append($"""
                             <section class="download-option" data-platform="{platform}">
                                 <div class="platform-row"><h2>{platformIcon}<span class="platform-title">{platform} 版</span></h2><span class="platform-badge">适用于当前设备</span></div>
                                 <p class="version">{WebUtility.HtmlEncode(description)}</p>
                                 <a class="download" href="{WebUtility.HtmlEncode(option.Url)}"><span class="download-platform">{platformIcon}</span><span class="download-text">{actionText}</span><span class="action-icon">{(option.Platform == "iOS" ? arrowIcon : downloadIcon)}</span></a>
                                 <p class="install-hint">{installHint}</p>
                             </section>
                             """);
            platformButtons.Append($"""
                                    <button class="platform-choice" type="button" data-platform="{platform}" aria-pressed="false"><span class="platform-name">{platformIcon}<span>{platform}{(option.Platform == "iOS" ? " · App Store" : "")}</span></span>{arrowIcon}</button>
                                    """);
            if (!string.IsNullOrWhiteSpace(option.UpdateContent))
            {
                // 逐行编码后生成列表，不将后台维护的更新说明作为 HTML 执行。
                string updateItems = string.Concat(option.UpdateContent
                    .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(line => $"<li>{WebUtility.HtmlEncode(line)}</li>"));
                updates.Append($"""
                                <section class="update-option" data-platform="{platform}">
                                    <h2>更新说明<span>{WebUtility.HtmlEncode(option.Version)}</span></h2>
                                    <ul class="update-content">{updateItems}</ul>
                                </section>
                                """);
            }
        }

        string safeName = WebUtility.HtmlEncode(appName);
        string logo = IsHttpUrl(logoUrl) ? $"<img class=\"logo\" src=\"{WebUtility.HtmlEncode(logoUrl)}\" alt=\"应用图标\">" : "";
        // UA 只用于推荐，始终保留手动选择平台的入口。
        string html = $$"""
                        <!doctype html>
                        <html lang="zh-CN">
                        <head>
                            <meta charset="utf-8">
                            <meta name="viewport" content="width=device-width, initial-scale=1, minimum-scale=1, maximum-scale=1, user-scalable=no, viewport-fit=cover">
                            <meta name="color-scheme" content="light dark">
                            <meta name="referrer" content="no-referrer">
                            <title>{{safeName}} · 应用下载</title>
                            <style>
                                :root { --bg:#f6f7fa; --surface:#fff; --text:#20252c; --secondary:#737b88; --border:#e9edf2; --accent:#409eff; --tint:#edf6ff; }
                                * { box-sizing:border-box; }
                                body { margin:0; background:var(--bg); color:var(--text); font:14px/1.6 -apple-system,BlinkMacSystemFont,"PingFang SC","Microsoft YaHei",sans-serif; }
                                body.guide-open { overflow:hidden; }
                                button,input { font:inherit; }
                                button { cursor:pointer; }
                                button,a,summary { -webkit-tap-highlight-color:transparent; -webkit-user-select:none; user-select:none; transition:opacity .15s; }
                                button:active,a:active,summary:active { opacity:.8; }
                                button:focus-visible,a:focus-visible,summary:focus-visible { outline:2px solid var(--accent); outline-offset:3px; }
                                p,h1,h2 { margin:0; }
                                [hidden] { display:none !important; }
                                .brand { background:var(--surface); border-bottom:1px solid var(--border); }
                                .brand-inner { display:flex; align-items:center; gap:7px; max-width:390px; margin:auto; padding:17px 22px; font-size:12px; color:var(--secondary); }
                                .brand-dot { width:7px; height:7px; border-radius:50%; background:var(--accent); flex-shrink:0; }
                                .brand-name { overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
                                .brand-label { margin-left:auto; white-space:nowrap; }
                                main { max-width:390px; margin:auto; padding:30px 22px calc(22px + env(safe-area-inset-bottom,0px)); }
                                .icon { display:block; width:20px; height:20px; flex-shrink:0; }
                                .platform-android { color:#3ddc84; }
                                .platform-icon { display:inline-flex; align-items:center; justify-content:center; flex:0 0 24px; width:24px; height:24px; line-height:0; }
                                .platform-icon .icon--android { transform:scale(1.12); }
                                .platform-icon .icon--apple { transform:scale(.86); }
                                .platform-icon .icon--windows { transform:scale(1.08); }
                                .platform-name { display:flex; align-items:center; gap:8px; min-width:0; line-height:24px; }
                                .platform-name > span:last-child { min-width:0; overflow-wrap:anywhere; }
                                .hero { text-align:center; padding-bottom:27px; }
                                .logo { display:block; width:74px; height:74px; object-fit:contain; border-radius:18px; margin:0 auto 14px; background:var(--surface); box-shadow:0 3px 16px #00000009; }
                                h1 { font-size:25px; font-weight:650; letter-spacing:1px; overflow-wrap:anywhere; }
                                .subtitle { margin-top:5px; color:var(--secondary); font-size:12px; }
                                .download-option,.qr-panel,.empty { padding:20px; border:1px solid var(--border); border-radius:18px; background:var(--surface); }
                                .download-option:not([hidden]) + .download-option:not([hidden]) { margin-top:12px; }
                                .platform-row { display:flex; align-items:center; justify-content:space-between; gap:8px; flex-wrap:wrap; }
                                .platform-row h2 { display:flex; align-items:center; gap:8px; min-height:24px; line-height:24px; font-size:14px; font-weight:600; }
                                .platform-title { display:block; line-height:24px; }
                                .platform-badge { padding:2px 7px; border-radius:5px; background:var(--tint); color:var(--accent); font-size:11px; }
                                .version { margin:11px 0 18px; color:var(--secondary); font-size:11px; overflow-wrap:anywhere; }
                                .download { display:grid; grid-template-columns:32px minmax(0,1fr) 32px; align-items:center; gap:8px; width:100%; min-height:46px; padding:7px 12px; border-radius:10px; background:var(--accent); color:#fff; text-decoration:none; font-weight:500; --icon-cutout:var(--accent); }
                                .download-platform,.action-icon { display:flex; align-items:center; justify-content:center; width:32px; height:32px; line-height:0; }
                                .download-platform .platform-android { color:#fff; }
                                .download-text { min-width:0; text-align:center; line-height:1.5; overflow-wrap:anywhere; }
                                .action-icon { border:1px solid #ffffff50; border-radius:8px; background:#ffffff18; }
                                .install-hint { margin-top:11px; color:var(--secondary); font-size:11px; text-align:center; }
                                .empty { text-align:center; }
                                .empty h2 { font-size:16px; font-weight:500; }
                                .empty p { margin-top:9px; color:var(--secondary); font-size:12px; }
                                .qr-panel { text-align:center; }
                                .download-option:not([hidden]) ~ .qr-panel { margin-top:16px; }
                                .qr-paper { width:192px; max-width:100%; padding:12px; border-radius:10px; background:#fff; margin:0 auto 16px; }
                                .qr-paper img { display:block; width:100%; height:auto; }
                                .qr-panel h2 { font-size:16px; font-weight:600; }
                                .qr-panel p { margin-top:7px; color:var(--secondary); font-size:11px; }
                                .other-platforms { margin-top:12px; }
                                .other-platforms summary { display:flex; align-items:center; justify-content:center; gap:6px; min-height:42px; background:transparent; color:var(--secondary); font-size:12px; list-style:none; cursor:pointer; }
                                .other-platforms summary::-webkit-details-marker { display:none; }
                                .platform-chevron { width:14px; height:14px; flex-shrink:0; transition:transform .2s; }
                                .other-platforms[open] .platform-chevron { transform:rotate(180deg); }
                                .platform-list { display:grid; grid-template-columns:1fr 1fr; gap:8px; padding-bottom:12px; }
                                .platform-choice { display:flex; align-items:center; justify-content:space-between; gap:6px; min-width:0; min-height:44px; padding:8px 12px; border:1px solid var(--border); border-radius:9px; background:var(--surface); color:var(--text); font-size:12px; }
                                .platform-choice[aria-pressed="true"] { border-color:var(--accent); color:var(--accent); }
                                .update-option { margin-top:20px; padding-top:21px; border-top:1px solid var(--border); }
                                .update-option h2 { display:flex; align-items:center; justify-content:space-between; gap:10px; font-size:13px; font-weight:600; }
                                .update-option h2 span { font-size:11px; font-weight:400; color:var(--secondary); }
                                .update-content { margin:12px 0 0; padding-left:18px; overflow-wrap:anywhere; color:var(--secondary); font-size:12px; line-height:1.9; }
                                .update-content li + li { margin-top:4px; }
                                .footer { padding-top:26px; text-align:center; }
                                .copy { display:inline-flex; align-items:center; justify-content:center; gap:6px; min-height:38px; padding:6px 12px; border:0; background:transparent; color:var(--accent); font-size:12px; }
                                .footer p { margin-top:4px; color:var(--secondary); font-size:11px; }
                                .copy-address { display:block; width:100%; margin:8px 0; padding:8px; border:1px solid var(--border); border-radius:6px; background:var(--surface); color:var(--text); }
                                .copy-status { min-height:18px; }
                                .copy-status:empty { display:none; }
                                .guide { position:fixed; inset:0; z-index:1000; overflow-y:auto; overscroll-behavior:contain; background:rgba(12,16,22,.91); color:#fff; }
                                .guide-sheet { position:relative; width:100%; max-width:390px; margin:0 auto; padding:28px 26px calc(24px + env(safe-area-inset-bottom,0px)); }
                                .guide-close { position:absolute; top:14px; left:12px; width:40px; height:40px; padding:0; border:0; border-radius:50%; background:transparent; color:#ffffff99; font-size:26px; }
                                .guide-direction { display:flex; align-items:flex-end; flex-direction:column; gap:2px; margin-top:4px; padding-bottom:16px; }
                                .guide-arrow { width:58px; height:72px; margin-right:1px; }
                                .guide-direction p { color:#ffffffd9; font-size:13px; }
                                .guide-direction p span { display:inline-block; padding:0 7px; font-size:21px; line-height:1; letter-spacing:1px; }
                                .guide-content { padding-top:9px; text-align:center; }
                                .guide-eyebrow { color:#9bcaff; font-size:11px; letter-spacing:2px; }
                                .guide-content h2 { margin-top:11px; font-size:23px; font-weight:600; letter-spacing:.5px; }
                                .guide-description { margin:13px 0 26px; color:#ffffff9c; font-size:12px; line-height:1.9; }
                                .guide-step { display:flex; align-items:center; gap:11px; margin:13px 0; font-size:13px; text-align:left; }
                                .guide-number { display:flex; align-items:center; justify-content:center; flex:0 0 23px; height:23px; border:1px solid #ffffff50; border-radius:50%; color:#ffffffb8; font-size:11px; }
                                .guide-dots { display:inline-block; padding:0 8px; margin:0 3px; border-radius:5px; background:#ffffff18; font-size:18px; line-height:21px; }
                                .guide-menu { display:flex; align-items:center; gap:10px; margin:14px 0 0 34px; padding:15px 14px; border-radius:12px; background:#fff; color:#253042; box-shadow:0 8px 28px #0003; font-size:13px; text-align:left; }
                                .guide-menu>span:first-child { color:var(--accent); font-size:21px; }
                                .guide-menu>span:last-child { margin-left:auto; color:#9ba1ab; }
                                .guide-after { margin:12px 0 26px; padding-right:10px; color:#ffffff7a; font-size:11px; text-align:right; }
                                .guide-dismiss { width:100%; min-height:44px; border:1px solid #ffffff50; border-radius:11px; background:#ffffff0d; color:#fff; font-size:13px; }
                                .guide-copy { display:inline-flex; align-items:center; justify-content:center; min-height:38px; margin-top:12px; padding:4px 8px; border:0; background:transparent; color:#9bcaff; font-size:12px; }
                                .guide-feedback { min-height:18px; color:#ffffffb3; font-size:11px; }
                                @media(prefers-color-scheme:dark) {
                                    :root { --bg:#141414; --surface:#202020; --text:#f2f3f5; --secondary:#a8acb4; --border:#343434; --tint:#14293d; }
                                }
                                @media(max-width:360px) {
                                    main { padding-left:16px; padding-right:16px; }
                                    .download-option { padding:16px; }
                                }
                            </style>
                        </head>
                        <body>
                            <header class="brand"><div class="brand-inner"><span class="brand-dot" aria-hidden="true"></span><span class="brand-name">{{safeName}}</span><span class="brand-label">应用下载</span></div></header>
                            <main>
                                <div class="hero">{{logo}}<h1>{{safeName}}</h1><p class="subtitle">官方应用下载</p></div>
                                {{channels}}
                                <section id="unavailable" class="empty" hidden><h2>当前设备暂无对应版本</h2><p>可以查看其他平台，或联系应用管理员。</p></section>
                                {{(availableCount == 0 ? "<section class=\"empty\"><h2>暂未提供下载</h2><p>应用安装包准备中，请稍后再试或联系管理员。</p></section>" : "")}}
                                {{(qrCode == null || availableCount == 0 ? "" : $"<section id=\"qr\" class=\"qr-panel\" hidden><div class=\"qr-paper\"><img src=\"{WebUtility.HtmlEncode(qrCode)}\" alt=\"下载页面二维码\"></div><h2>手机扫码下载</h2><p>使用手机扫描二维码，选择对应平台下载</p></section>")}}
                                {{(availableCount == 0 ? "" : $"<details id=\"other-platforms\" class=\"other-platforms\"><summary>其他平台下载<svg class=\"platform-chevron\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\"><path d=\"m6 9 6 6 6-6\"/></svg></summary><div class=\"platform-list\">{platformButtons}</div></details>")}}
                                {{updates}}
                                <footer class="footer">
                                    <button id="copy" class="copy" type="button">{{linkIcon}}复制下载地址</button>
                                    <input id="address" class="copy-address" readonly aria-label="下载地址" hidden>
                                    <p id="status" class="copy-status" role="status" aria-live="polite"></p>
                                    <p>将此页面分享给其他设备，即可下载安装</p>
                                </footer>
                            </main>
                            <div id="wechat-guide" class="guide" role="dialog" aria-modal="true" aria-labelledby="guide-title" hidden>
                                <div class="guide-sheet">
                                    <button id="guide-close" class="guide-close" type="button" aria-label="关闭打开方式引导">×</button>
                                    <div class="guide-direction"><svg class="guide-arrow" viewBox="0 0 58 72" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10 64h12c14 0 24-10 24-24V8M32 22 46 8l10 14"/></svg><p>点击微信右上角 <span>···</span></p></div>
                                    <div class="guide-content">
                                        <p class="guide-eyebrow">还差一步，即可下载</p>
                                        <h2 id="guide-title">请在浏览器中打开</h2>
                                        <p class="guide-description">微信内暂不支持直接下载安装包<br>按下面两步继续就好</p>
                                        <div class="guide-step"><span class="guide-number">1</span><p>点击右上角的 <span class="guide-dots">···</span> 菜单</p></div>
                                        <div class="guide-step"><span class="guide-number">2</span><p>选择「在浏览器中打开」</p></div>
                                        <div class="guide-menu" aria-label="菜单选项示意"><span aria-hidden="true">↗</span><span>在浏览器中打开</span><span aria-hidden="true">›</span></div>
                                        <p class="guide-after">打开后，再点击下载按钮</p>
                                        <button id="guide-dismiss" class="guide-dismiss" type="button">我知道了</button>
                                        <button id="guide-copy" class="guide-copy" type="button">复制链接，手动打开</button>
                                        <input id="guide-address" class="copy-address" readonly aria-label="下载地址" hidden>
                                        <p id="guide-status" class="guide-feedback" role="status" aria-live="polite"></p>
                                    </div>
                                </div>
                            </div>
                            <script>
                                const ua = navigator.userAgent;
                                const ios = /iPhone|iPad|iPod/i.test(ua) || (/Macintosh/i.test(ua) && navigator.maxTouchPoints > 1);
                                const harmony = /HarmonyOS|OpenHarmony/i.test(ua);
                                const android = !harmony && /Android/i.test(ua);
                                const mobile = ios || harmony || android;
                                const wechat = /MicroMessenger/i.test(ua);
                                const detectedPlatform = ios ? "iOS" : harmony ? "HarmonyOS" : android ? "Android" : /Windows/i.test(ua) ? "Windows" : "";
                                const options = Array.from(document.querySelectorAll(".download-option"));
                                const updates = Array.from(document.querySelectorAll(".update-option"));
                                const choices = Array.from(document.querySelectorAll(".platform-choice"));
                                const guide = document.getElementById("wechat-guide");
                                const main = document.querySelector("main");
                                const header = document.querySelector("header");
                                const dismiss = document.getElementById("guide-dismiss");
                                let previousFocus;

                                function selectPlatform(platform) {
                                    const selected = options.find(option => option.dataset.platform === platform);
                                    options.forEach(option => {
                                        option.hidden = option !== selected;
                                        option.querySelector(".platform-badge").textContent = platform === detectedPlatform ? "适用于当前设备" : "已选择平台";
                                    });
                                    updates.forEach(update => { update.hidden = !selected || update.dataset.platform !== platform; });
                                    choices.forEach(choice => choice.setAttribute("aria-pressed", String(choice.dataset.platform === platform)));
                                    document.getElementById("unavailable").hidden = Boolean(selected) || options.length === 0 || !mobile;
                                }

                                function showGuide() {
                                    previousFocus = document.activeElement;
                                    guide.hidden = false;
                                    document.body.classList.add("guide-open");
                                    main.inert = true;
                                    header.inert = true;
                                    dismiss.focus({ preventScroll:true });
                                }

                                function hideGuide() {
                                    guide.hidden = true;
                                    document.body.classList.remove("guide-open");
                                    main.inert = false;
                                    header.inert = false;
                                    if (previousFocus && previousFocus.isConnected) previousFocus.focus({ preventScroll:true });
                                }

                                async function copyAddress(status, input) {
                                    const address = location.href.split("?")[0].split("#")[0];
                                    try {
                                        if (navigator.clipboard && window.isSecureContext) {
                                            await navigator.clipboard.writeText(address);
                                        } else {
                                            input.hidden = false;
                                            input.value = address;
                                            input.focus();
                                            input.select();
                                            input.setSelectionRange(0, address.length);
                                            if (!document.execCommand("copy")) throw new Error("copy failed");
                                            input.hidden = true;
                                        }
                                        status.textContent = "下载地址已复制";
                                    } catch {
                                        input.hidden = false;
                                        input.value = address;
                                        status.textContent = "请长按或选中上方地址进行复制";
                                    }
                                }

                                selectPlatform(mobile ? detectedPlatform : "");
                                const qr = document.getElementById("qr");
                                if (qr) qr.hidden = mobile;
                                if (!mobile && !qr && options.length) {
                                    selectPlatform(options.some(option => option.dataset.platform === detectedPlatform)
                                        ? detectedPlatform : options[0].dataset.platform);
                                }
                                choices.forEach(choice => choice.addEventListener("click", () => {
                                    selectPlatform(choice.dataset.platform);
                                    document.getElementById("other-platforms").open = false;
                                    const selected = options.find(option => !option.hidden);
                                    selected.querySelector(".download").focus({ preventScroll:true });
                                }));
                                document.querySelectorAll(".download").forEach(link => link.addEventListener("click", event => {
                                    if (!wechat) return;
                                    event.preventDefault();
                                    showGuide();
                                }));
                                document.getElementById("guide-close").addEventListener("click", hideGuide);
                                dismiss.addEventListener("click", hideGuide);
                                guide.addEventListener("click", event => { if (event.target === guide) hideGuide(); });
                                guide.addEventListener("keydown", event => {
                                    if (event.key === "Escape") hideGuide();
                                    if (event.key !== "Tab") return;
                                    const controls = Array.from(guide.querySelectorAll("button,input")).filter(control => !control.hidden);
                                    const first = controls[0];
                                    const last = controls[controls.length - 1];
                                    if (event.shiftKey && document.activeElement === first) {
                                        event.preventDefault();
                                        last.focus();
                                    } else if (!event.shiftKey && document.activeElement === last) {
                                        event.preventDefault();
                                        first.focus();
                                    }
                                });
                                document.getElementById("copy").addEventListener("click", () => copyAddress(document.getElementById("status"), document.getElementById("address")));
                                document.getElementById("guide-copy").addEventListener("click", () => copyAddress(document.getElementById("guide-status"), document.getElementById("guide-address")));
                            </script>
                        </body>
                        </html>
                        """;
        // 只清理行首尾空白与空行，保留脚本换行，避免改变行注释和自动分号插入的语义。
        return string.Join('\n', html.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
}
