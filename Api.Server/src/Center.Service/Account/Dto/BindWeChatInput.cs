// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Center.Service.Account.Dto;

/// <summary>
/// 绑定微信输入
/// </summary>
public class BindWeChatInput : UpdateVersionInput
{
    /// <summary>
    /// wx.login 获取到的Code
    /// </summary>
    [StringRequired(ErrorMessage = "微信Code不能为空")]
    public string WeChatCode { get; set; }

    /// <summary>
    /// 加密算法的初始向量
    /// </summary>
    [StringRequired(ErrorMessage = "初始向量不能为空")]
    public string IV { get; set; }

    /// <summary>
    /// 包括敏感数据在内的完整用户信息的加密数据
    /// </summary>
    [StringRequired(ErrorMessage = "加密数据不能为空")]
    public string EncryptedData { get; set; }
}
