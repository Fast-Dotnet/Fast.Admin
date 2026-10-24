// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.ApplicationVersion.Dto;

/// <summary>
/// 添加应用版本输入
/// </summary>
public class AddApplicationVersionInput
{
    /// <summary>
    /// 应用标识
    /// </summary>
    [StringRequired(ErrorMessage = "应用标识不能为空")]
    public string AppKey { get; set; }

    /// <summary>
    /// 包类型
    /// </summary>
    [EnumRequired(ErrorMessage = "包类型不能为空")]
    public ApplicationPackageTypeEnum PackageType { get; set; }

    /// <summary>
    /// 版本名称
    /// </summary>
    [StringRequired(ErrorMessage = "版本名称不能为空")]
    public string VersionName { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [IntRequired(ErrorMessage = "版本号不能为空")]
    [Range(1, int.MaxValue)]
    public int VersionCode { get; set; }

    /// <summary>
    /// 包文件地址
    /// </summary>
    [StringRequired(ErrorMessage = "包文件地址不能为空")]
    public string FileUrl { get; set; }

    /// <summary>
    /// 最低兼容原生版本号
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? MinNativeVersionCode { get; set; }

    /// <summary>
    /// 最高兼容原生版本号
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? MaxNativeVersionCode { get; set; }

    /// <summary>
    /// 更新说明
    /// </summary>
    public string UpdateContent { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    [Required(ErrorMessage = "是否强制更新不能为空")]
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
