// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.ApplicationClient.Dto;

/// <summary>
/// 添加应用标识输入
/// </summary>
public class AddApplicationClientInput
{
    /// <summary>
    /// 应用标识
    /// </summary>
    [StringRequired(ErrorMessage = "应用标识不能为空")]
    public string AppKey { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    [LongRequired(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    [EnumRequired(ErrorMessage = "应用类型不能为空")]
    public AppEnvironmentEnum AppType { get; set; }

    /// <summary>
    /// 应用密钥
    /// </summary>
    public string AppSecret { get; set; }

    /// <summary>
    /// 环境类型
    /// </summary>
    [EnumRequired(ErrorMessage = "环境类型不能为空")]
    public EnvironmentTypeEnum EnvironmentType { get; set; }

    /// <summary>
    /// 登录组件
    /// </summary>
    public string LoginComponent { get; set; }

    /// <summary>
    /// 请求超时时间（毫秒）
    /// </summary>
    [IntRequired(ErrorMessage = "请求超时时间不能为空")]
    public int RequestTimeout { get; set; }

    /// <summary>
    /// 请求加密
    /// </summary>
    [Required(ErrorMessage = "请求加密不能为空")]
    public bool RequestEncipher { get; set; }

    /// <summary>
    /// 微信商户号Id
    /// </summary>
    public long? WeChatMerchantId { get; set; }

    /// <summary>
    /// 微信商户号
    /// </summary>
    public string WeChatMerchantNo { get; set; }

    /// <summary>
    /// 支付宝商户号Id
    /// </summary>
    public long? AlipayMerchantId { get; set; }

    /// <summary>
    /// 支付宝商户号
    /// </summary>
    public string AlipayMerchantNo { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
