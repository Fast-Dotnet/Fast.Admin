// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;

namespace Fast.Center.Service.ApplicationVersion.Dto;

/// <summary>
/// 查询应用版本详情输出
/// </summary>
public class QueryApplicationVersionDetailOutput : PagedOutput
{
    /// <summary>
    /// 记录Id
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [SugarSearchValue]
    public string AppName { get; set; }

    /// <summary>
    /// 应用标识
    /// </summary>
    [SugarSearchValue]
    public string AppKey { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    public AppEnvironmentEnum AppType { get; set; }

    /// <summary>
    /// 环境类型
    /// </summary>
    public EnvironmentTypeEnum EnvironmentType { get; set; }

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
    /// 状态
    /// </summary>
    public CommonStatusEnum Status { get; set; }

    /// <summary>
    /// 包文件地址
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// 最低兼容原生版本号
    /// </summary>
    public int? MinNativeVersionCode { get; set; }

    /// <summary>
    /// 最高兼容原生版本号
    /// </summary>
    public int? MaxNativeVersionCode { get; set; }

    /// <summary>
    /// 更新说明
    /// </summary>
    public string UpdateContent { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
