// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using SqlSugar;
using Yitter.IdGenerator;

namespace Fast.Core;

/// <summary>
/// 配置种子数据
/// </summary>
internal static class ConfigSeedData
{
    /// <summary>
    /// 配置种子数据
    /// </summary>
    public static async Task SystemConfigSeedData(ISqlSugarClient db, DateTime dateTime)
    {
        await db.Insertable(new List<ConfigModel>
            {
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SingleTenantWhenAutoLogin,
                    ConfigName = "单租户自动登录",
                    ConfigValue = "True",
                    Remark = "True：打开（如果只有一个租户，则默认当前租户自动登录）；False：关闭；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SingleLogin,
                    ConfigName = "单点登录",
                    ConfigValue = "True",
                    Remark = "True：打开（多次登录只会保留最后一次登录有效）；False：关闭；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.LoginCaptchaOpen,
                    ConfigName = "登录验证码开关",
                    ConfigValue = "True",
                    Remark = "True：打开；False：关闭；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.LoginIdentityVerificationOpen,
                    ConfigName = "登录后身份验证开关",
                    ConfigValue = "False",
                    Remark = "True：打开；False：关闭；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailSmtp,
                    ConfigName = "邮件服务器地址",
                    ConfigValue = "smtp.qq.com",
                    Remark = "QQ：smtp.qq.com，网易：smtp.qq.com；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailPort,
                    ConfigName = "邮件服务器端口",
                    ConfigValue = "465",
                    Remark = "常规端口：25，加密端口：465/994；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailEmail,
                    ConfigName = "发件邮箱",
                    ConfigValue = "",
                    Remark = "发送系统邮件的邮箱地址；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailAuthCode,
                    ConfigName = "邮件授权码",
                    ConfigValue = "",
                    Remark = "发件邮箱的SMTP授权码；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailDisplayName,
                    ConfigName = "发件人名称",
                    ConfigValue = "FastDotnet",
                    Remark = "系统邮件显示的发件人名称；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.MailReceiveEmails,
                    ConfigName = "默认收件邮箱",
                    ConfigValue = "[]",
                    Remark = "默认收件邮箱列表，配置值使用JSON数组格式；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SmsAccessKeyId,
                    ConfigName = "阿里云短信AccessKeyId",
                    ConfigValue = "",
                    Remark = "",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SmsAccessKeySecret,
                    ConfigName = "阿里云短信AccessKey密钥",
                    ConfigValue = "",
                    Remark = "",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SmsSignName,
                    ConfigName = "阿里云短信签名",
                    ConfigValue = "",
                    Remark = "",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.SmsVerificationTemplateCode,
                    ConfigName = "阿里云短信验证码模板Code",
                    ConfigValue = "",
                    Remark = "阿里云审核通过的验证码短信模板编码，模板变量为Code；",
                    CreatedTime = dateTime
                },
                new()
                {
                    ConfigId = YitIdHelper.NextId(),
                    ConfigCode = ConfigConst.GaoDeMapKey,
                    ConfigName = "高德地图Key",
                    ConfigValue = "",
                    Remark = null,
                    CreatedTime = dateTime
                }
            })
            .ExecuteCommandAsync();
    }
}
