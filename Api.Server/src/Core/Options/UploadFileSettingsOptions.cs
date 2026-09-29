// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 上传文件配置选项
/// </summary>
public class UploadFileSettingsOptions : IPostConfigure
{
    /// <summary>
    /// 公网域名，未配置时使用当前请求地址
    /// </summary>
    public string PublicDomain { get; set; }

    /// <summary>
    /// Logo
    /// </summary>
    public UploadFileInfoSettings Logo { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public UploadFileInfoSettings Avatar { get; set; }

    /// <summary>
    /// 证件照
    /// </summary>
    public UploadFileInfoSettings IdPhoto { get; set; }

    /// <summary>
    /// 富文本
    /// </summary>
    public UploadFileInfoSettings Editor { get; set; }

    /// <summary>
    /// 默认
    /// </summary>
    public UploadFileInfoSettings Default { get; set; }

    /// <inheritdoc />
    public void PostConfigure()
    {
        if (string.IsNullOrWhiteSpace(PublicDomain))
        {
            PublicDomain = null;
        }
        else
        {
            PublicDomain = PublicDomain.Trim().TrimEnd('/');
        }

        Logo ??= new UploadFileInfoSettings
        {
            Path = "Upload/Logo",
            MaxSize = 2048,
            ContentType =
            [
                "image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"
            ]
        };
        Avatar ??= new UploadFileInfoSettings
        {
            Path = "Upload/Avatar",
            MaxSize = 2048,
            ContentType =
            [
                "image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"
            ]
        };
        IdPhoto ??= new UploadFileInfoSettings
        {
            Path = "Upload/IdPhoto",
            MaxSize = 5120,
            ContentType =
            [
                "image/jpg", "image/jpeg", "image/png"
            ]
        };
        Editor ??= new UploadFileInfoSettings
        {
            Path = "Upload/Editor",
            MaxSize = 10240,
            UseDateFolder = true,
            ContentType =
            [ // 图片类
                "image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp",
                // 视频类
                "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/x-ms-wmv", "video/webm",
                "video/ogg"
            ]
        };
        Default ??= new UploadFileInfoSettings
        {
            Path = "Upload/Default",
            MaxSize = 102400,
            UseTypeFolder = true,
            UseDateFolder = true,
            ContentType =
            [
                // 图片类
                "image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp",
                // 视频类
                "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/x-ms-wmv", "video/webm",
                "video/ogg",
                // 音频类
                "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4", "audio/flac",
                // 文本类
                "text/plain",
                "text/csv",
                "text/html",
                "text/markdown",
                // PDF
                "application/pdf",
                // Word
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                // Excel
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                // PowerPoint
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                // 压缩包
                "application/zip", "application/x-rar-compressed", "application/x-7z-compressed", "application/gzip"
            ]
        };
    }
}

/// <summary>
/// 上传文件信息配置
/// </summary>
public class UploadFileInfoSettings
{
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 最大大小，单位kb
    /// </summary>
    public long MaxSize { get; set; }

    /// <summary>
    /// 使用类型文件夹
    /// </summary>
    public bool UseTypeFolder { get; set; }

    /// <summary>
    /// 使用日期文件夹
    /// </summary>
    public bool UseDateFolder { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string[] ContentType { get; set; }
}
