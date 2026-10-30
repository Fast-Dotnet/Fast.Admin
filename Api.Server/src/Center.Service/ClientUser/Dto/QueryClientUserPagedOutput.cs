// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.ClientUser.Dto;

/// <summary>
/// 获取客户端用户分页列表输出
/// </summary>
public class QueryClientUserPagedOutput : PagedOutput
{
    /// <summary>
    /// 客户端用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 手机
    /// </summary>
    [SugarSearchValue]
    public string Mobile { get; set; }

    /// <summary>
    /// 唯一用户标识
    /// </summary>
    [SugarSearchValue]
    public string OpenId { get; set; }

    /// <summary>
    /// 统一用户标识
    /// </summary>
    [SugarSearchValue]
    public string UnionId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarSearchValue]
    public string NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderEnum Sex { get; set; }

    /// <summary>
    /// 最后登录设备
    /// </summary>
    public string LastLoginDevice { get; set; }

    /// <summary>
    /// 最后登录操作系统（版本）
    /// </summary>
    public string LastLoginOS { get; set; }

    /// <summary>
    /// 最后登录浏览器（版本）
    /// </summary>
    public string LastLoginBrowser { get; set; }

    /// <summary>
    /// 最后登录省份
    /// </summary>
    public string LastLoginProvince { get; set; }

    /// <summary>
    /// 最后登录城市
    /// </summary>
    public string LastLoginCity { get; set; }

    /// <summary>
    /// 最后登录Ip
    /// </summary>
    public string LastLoginIp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 手机号更新时间
    /// </summary>
    public DateTime? MobileUpdateTime { get; set; }

    /// <summary>创建时间</summary>
    [SugarSearchTime]
    public override DateTime? CreatedTime { get; set; }
}
