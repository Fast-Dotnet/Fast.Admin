// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Fast.Core;

/// <summary>
/// 解密微信用户信息
/// </summary>
/// <remarks>解密数据返回</remarks>
public class DecryptWeChatUserInfo
{
    /// <summary>
    /// 微信昵称
    /// </summary>
    [JsonPropertyName("nickName")]
    [JsonProperty("nickName")]
    public string NickName { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [JsonPropertyName("gender")]
    [JsonProperty("gender")]
    public GenderEnum Gender { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    [JsonPropertyName("country")]
    [JsonProperty("country")]
    public string Country { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    [JsonPropertyName("province")]
    [JsonProperty("province")]
    public string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonPropertyName("city")]
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    [JsonPropertyName("language")]
    [JsonProperty("language")]
    public string Language { get; set; }
}
