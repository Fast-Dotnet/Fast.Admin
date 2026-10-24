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
    /// 安装包
    /// </summary>
    public UploadFileInfoSettings Package { get; set; }

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
        Package ??= new UploadFileInfoSettings
        {
            Path = "Upload/ApplicationPackage",
            MaxSize = 512 * 1024,
            BaseUrl = "/package",
            UseTypeFolder = true,
            ContentType =
            [
                // Windows 安装包
                "application/vnd.microsoft.portable-executable",
                // Android 安装包
                "application/vnd.android.package-archive",
                // IPA、WGT 和以通用二进制类型上传的 Windows 安装包
                "application/octet-stream"
            ]
        };
        Editor ??= new UploadFileInfoSettings
        {
            Path = "Upload/Editor",
            MaxSize = 10240,
            UseTypeFolder = true,
            ContentType =
            [
                // 图片类
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
    /// 单个文件允许的最大大小，单位为 KB（1024 字节）
    /// </summary>
    public long MaxSize { get; set; }

    /// <summary>
    /// 拼接在 /file 后的基础地址，必须已 / 开头
    /// 如果存在地址，比如存在对应地址的接口
    /// 存在地址的情况下，实际的文件 Url 会以文件Id结尾，不带后缀
    /// </summary>
    /// <example>
    /// BaseUrl = "/images"，最终路径为 "/file/images"
    /// </example>
    public string BaseUrl { get; set; }

    /// <summary>
    /// 是否按文件类型创建存储子目录
    /// </summary>
    public bool UseTypeFolder { get; set; }

    /// <summary>
    /// 允许上传的 MIME 类型列表
    /// </summary>
    public string[] ContentType { get; set; }
}
