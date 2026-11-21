// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Net;
using System.Text;
using Fast.Center.Domain;
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

        string pageUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}"
                         + $"{_httpContext.Request.PathBase}{_httpContext.Request.Path}";
        using SKBitmap qrCode = ImageUtil.GenQrCode(pageUrl, 280, 280);
        return new ContentResult
        {
            ContentType = "text/html; charset=utf-8",
            Content = PageHtmlRender(application.AppName, application.LogoUrl, options,
                $"data:image/png;base64,{ImageUtil.ConvertToBase64Image(qrCode)}")
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
        var channels = new StringBuilder();
        foreach (AppDownloadOption option in options)
        {
            string platform = WebUtility.HtmlEncode(option.Platform);
            string description = option.Version == null ? "通过 App Store 获取最新版本" : $"版本 {option.Version}";
            if (option.FileSizeKb.HasValue)
            {
                description += $" · {option.FileSizeKb.Value / 1024d:0.#} MB";
            }

            if (option.PublishedAt.HasValue)
            {
                description += $" · {option.PublishedAt.Value:yyyy-MM-dd}";
            }

            channels.Append($"""
                             <section class="channel" data-platform="{platform}">
                                 <h2>{platform}</h2>
                                 <p class="muted">{WebUtility.HtmlEncode(description)}</p>
                                 <a class="download" href="{WebUtility.HtmlEncode(option.Url)}" rel="noopener noreferrer">{(option.Platform == "iOS" ? "前往 App Store" : $"下载 {platform} 版")}</a>
                                 {(option.Platform == "HarmonyOS" ? "<p class=\"muted\">鸿蒙原生包需使用对应设备支持的安装方式。</p>" : "")}
                                 {(string.IsNullOrWhiteSpace(option.UpdateContent) ? "" : $"<details><summary>更新说明</summary><p class=\"notes\">{WebUtility.HtmlEncode(option.UpdateContent)}</p></details>")}
                             </section>
                             """);
        }

        string logo = IsHttpUrl(logoUrl)
            ? $"<img class=\"logo\" src=\"{WebUtility.HtmlEncode(logoUrl)}\" alt=\"应用图标\" referrerpolicy=\"no-referrer\">"
            : "";
        return $$"""
                 <!doctype html>
                 <html lang="zh-CN">
                 <head>
                     <meta charset="utf-8">
                     <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
                     <meta name="color-scheme" content="light dark">
                     <meta name="referrer" content="no-referrer">
                     <title>{{WebUtility.HtmlEncode(appName)}} · 应用下载</title>
                     <style>
                         * { box-sizing: border-box; }
                         body { margin: 0; padding: 32px 20px; background: #f5f7fa; color: #1f2937; font: 14px/1.6 system-ui, sans-serif; }
                         main { max-width: 440px; margin: auto; text-align: center; }
                         h1 { margin: 16px 0 8px; font-size: 24px; }
                         h2 { margin: 0 0 6px; font-size: 18px; }
                         .logo { width: 80px; height: 80px; border-radius: 18px; object-fit: contain; }
                         .muted { color: #667085; }
                         .channel { margin-top: 16px; padding: 20px; border: 1px solid #e5e7eb; border-radius: 16px; background: #fff; }
                         .channel.recommended { border-color: #409eff; }
                         .channel.recommended h2::after { content: " · 推荐"; color: #409eff; font-size: 12px; }
                         .download, button { display: block; width: 100%; padding: 12px; border: 0; border-radius: 10px; font: inherit; cursor: pointer; }
                         .download { background: #1677ff; color: #fff; text-decoration: none; }
                         button { margin-top: 20px; background: #e8edf5; color: inherit; }
                         .notes { text-align: left; white-space: pre-wrap; overflow-wrap: anywhere; }
                         details { margin-top: 16px; }
                         summary { cursor: pointer; }
                         .qr { width: 220px; height: 220px; margin-top: 12px; padding: 12px; background: #fff; }
                         .hint { padding: 12px; border-radius: 10px; background: #fff2d6; color: #7a4b00; }
                         #address { width: 100%; margin-top: 10px; padding: 8px; border: 1px solid #d0d5dd; border-radius: 6px; background: transparent; color: inherit; font: inherit; }
                         [hidden] { display: none !important; }
                         @media (prefers-color-scheme: dark) {
                             body { background: #151515; color: #ededed; }
                             .channel { background: #202020; border-color: #414141; }
                             .muted { color: #b6b6b6; }
                             button { background: #303030; }
                         }
                     </style>
                 </head>
                 <body>
                     <main>
                         {{logo}}
                         <h1>{{WebUtility.HtmlEncode(appName)}}</h1>
                         <p class="muted">请选择适合设备的下载方式</p>
                         <p id="wechat" class="hint" hidden>如无法下载或跳转，请点击右上角菜单，选择在浏览器中打开，再继续操作。</p>
                         <p id="unavailable" class="muted" hidden>当前设备暂无对应下载渠道，可以查看下方其他平台。</p>
                         {{channels}}
                         {{(options.Count == 0 ? "<p class=\"hint\">暂未提供可用下载，请联系应用管理员。</p>" : "")}}
                         {{(qrCode == null ? "" : $"<div id=\"qr\"><img class=\"qr\" src=\"{WebUtility.HtmlEncode(qrCode)}\" alt=\"下载页面二维码\"><p class=\"muted\">使用手机扫描二维码下载</p></div>")}}
                         <button id="copy" type="button">复制下载地址</button>
                         <input id="address" readonly aria-label="下载地址" hidden>
                         <p id="status" class="muted" role="status"></p>
                         <p class="muted">下载安装包后，请按照系统提示完成安装。</p>
                     </main>
                     <script>
                         const ua = navigator.userAgent;
                         const ios = /iPhone|iPad|iPod/i.test(ua) || (/Macintosh/i.test(ua) && navigator.maxTouchPoints > 1);
                         const harmony = /HarmonyOS|OpenHarmony/i.test(ua);
                         const android = !harmony && /Android/i.test(ua);
                         const platform = ios ? "iOS" : harmony ? "HarmonyOS" : android ? "Android" : /Windows/i.test(ua) ? "Windows" : "";
                         const recommended = Array.from(document.querySelectorAll(".channel")).find(item => item.dataset.platform === platform);
                         if (recommended) {
                             recommended.classList.add("recommended");
                             document.querySelector(".channel").before(recommended);
                         } else if (platform) {
                             document.getElementById("unavailable").hidden = false;
                         }
                         document.getElementById("wechat").hidden = !/MicroMessenger/i.test(ua);
                         const qr = document.getElementById("qr");
                         if (qr) qr.hidden = ios || harmony || android;
                         document.getElementById("copy").addEventListener("click", async () => {
                             const address = location.href.split("?")[0].split("#")[0];
                             const status = document.getElementById("status");
                             try {
                                 if (navigator.clipboard && window.isSecureContext) await navigator.clipboard.writeText(address);
                                 else {
                                     const input = document.getElementById("address");
                                     input.hidden = false;
                                     input.value = address;
                                     input.select();
                                     input.setSelectionRange(0, address.length);
                                     if (!document.execCommand("copy")) throw new Error("copy failed");
                                     input.hidden = true;
                                 }
                                 status.textContent = "下载地址已复制";
                             } catch {
                                 const input = document.getElementById("address");
                                 input.hidden = false;
                                 input.value = address;
                                 status.textContent = "请长按或选中上方地址进行复制";
                             }
                         });
                     </script>
                 </body>
                 </html>
                 """;
    }
}
