// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.App.Dto;

/// <summary>
/// 检查应用更新包输出
/// </summary>
public class CheckAppUpdatePackageOutput
{
    /// <summary>
    /// 版本记录Id
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 包类型
    /// </summary>
    public ApplicationPackageTypeEnum PackageType { get; set; }

    /// <summary>
    /// 版本名称
    /// </summary>
    public string VersionName { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public int VersionCode { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// 更新说明
    /// </summary>
    public string UpdateContent { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小，单位为 KB（向上取整）
    /// </summary>
    public long FileSizeKb { get; set; }

    /// <summary>
    /// 文件 SHA-256，小写十六进制
    /// </summary>
    public string FileSha256 { get; set; }

    /// <summary>
    /// 下载地址，支持 GET 和 Range
    /// </summary>
    public string DownloadUrl { get; set; }
}
