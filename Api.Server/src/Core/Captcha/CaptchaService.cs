// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CSRedis;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Fast.Core;

/// <summary>
/// <see cref="ICaptchaService"/> 默认实现
/// </summary>
public class CaptchaService : ICaptchaService, ISingletonDependency
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache _cache;

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化验证码服务
    /// </summary>
    public CaptchaService(ICache cache, ILogger<IMailService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 验证码缓存Dto
    /// </summary>
    private class VerificationCodeCacheDto
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientIdentity { get; set; }
    }

    /// <inheritdoc />
    public async Task<(string captchaKey, string captchaImage)> GetImageCaptcha()
    {
        string captchaKey = Guid.NewGuid().ToString("N");

        // 生成验证码
        const string codeCharacters = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        var dto = new VerificationCodeCacheDto
        {
            VerificationCode = new string(Enumerable
                .Range(0, 4)
                .Select(_ => codeCharacters[RandomNumberGenerator.GetInt32(codeCharacters.Length)])
                .ToArray()),
            ClientIdentity = GlobalContext.ClientIdentity
        };

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.ImageCaptcha, captchaKey);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        // 背景颜色
        var backgroundColor = Color.ParseHex("EEF4FF");
        // 背景纹理颜色
        Color[] backgroundPatternColors =
        [
            Color.FromRgba(147, 197, 253, 40),
            Color.FromRgba(191, 219, 254, 55),
            Color.FromRgba(165, 180, 252, 35),
            Color.FromRgba(219, 234, 254, 60)
        ];
        // 验证码文字颜色
        Color[] textColors =
        [
            Color.ParseHex("1E3A8A"),
            Color.ParseHex("1D4ED8"),
            Color.ParseHex("3730A3"),
            Color.ParseHex("312E81"),
            Color.ParseHex("1E40AF")
        ];
        // 干扰线颜色
        Color[] lineColors =
        [
            Color.FromRgba(59, 130, 246, 120),
            Color.FromRgba(99, 102, 241, 105),
            Color.FromRgba(96, 165, 250, 115),
            Color.FromRgba(129, 140, 248, 95)
        ];
        // 噪点颜色
        Color[] noiseColors =
        [
            Color.FromRgba(59, 130, 246, 125),
            Color.FromRgba(96, 165, 250, 115),
            Color.FromRgba(99, 102, 241, 100),
            Color.FromRgba(30, 64, 175, 85)
        ];
        // 生成图片验证码
        using Image<Rgba32> image = ImageUtil.GenImage((canvas, font, _, _, width, height) =>
        {
            // 背景、纹理和后置干扰
            canvas.Mutate(context =>
            {
                // 设置背景
                context.BackgroundColor(backgroundColor);
                // 背景随机纹理
                for (int index = 0; index < 18; index++)
                {
                    Color color = backgroundPatternColors[RandomNumberGenerator.GetInt32(backgroundPatternColors.Length)];
                    int radius = RandomNumberGenerator.GetInt32(4, 15);
                    context.Fill(color,
                        new EllipsePolygon(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                            radius));
                }

                // 背景噪点
                for (int index = 0; index < 45; index++)
                {
                    Color color = noiseColors[RandomNumberGenerator.GetInt32(noiseColors.Length)];
                    int radius = RandomNumberGenerator.GetInt32(1, 3);
                    context.Fill(color,
                        new EllipsePolygon(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                            radius));
                }

                // 字符后面的直线干扰
                for (int index = 0; index < 6; index++)
                {
                    Color color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                    float thickness = RandomNumberGenerator.GetInt32(8, 16) / 10F;
                    context.DrawLine(color, thickness,
                        new PointF(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height)),
                        new PointF(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height)));
                }

                // 字符后面的Bezier曲线
                for (int index = 0; index < 2; index++)
                {
                    Color color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                    context.DrawBeziers(color, RandomNumberGenerator.GetInt32(10, 18) / 10F,
                        new PointF(0, RandomNumberGenerator.GetInt32(height)),
                        new PointF(RandomNumberGenerator.GetInt32(width / 4, width / 2), RandomNumberGenerator.GetInt32(height)),
                        new PointF(RandomNumberGenerator.GetInt32(width / 2, width * 3 / 4),
                            RandomNumberGenerator.GetInt32(height)), new PointF(width, RandomNumberGenerator.GetInt32(height)));
                }

                // 每个字符单独绘制、旋转后合成到主画布
                for (int index = 0; index < dto.VerificationCode.Length; index++)
                {
                    // 提前保存当前字符，避免Lambda捕获循环变量
                    char character = dto.VerificationCode[index];
                    Color textColor = textColors[RandomNumberGenerator.GetInt32(textColors.Length)];
                    int fontSize = RandomNumberGenerator.GetInt32(25, 31);
                    var characterFont = new Font(font, fontSize);
                    // 创建单字符透明画布
                    using var characterImage = new Image<Rgba32>(42, 48, new Rgba32(0, 0, 0, 0));
                    characterImage.Mutate(characterContext =>
                    {
                        characterContext.DrawText(character.ToString(), characterFont, textColor,
                            new PointF(RandomNumberGenerator.GetInt32(5, 9), RandomNumberGenerator.GetInt32(2, 7)));
                    });
                    // 单字符随机旋转 -18° ~ 18°
                    int angle = RandomNumberGenerator.GetInt32(-18, 19);
                    characterImage.Mutate(characterContext => { characterContext.Rotate(angle); });
                    // 根据旋转后尺寸修正坐标，避免整体向右下偏移
                    int x = 1 + index * 31 + RandomNumberGenerator.GetInt32(-2, 4) - (characterImage.Width - 42) / 2;
                    int y = -2 + RandomNumberGenerator.GetInt32(-2, 5) - (characterImage.Height - 48) / 2;
                    // 直接绘制，不再通过Lambda捕获characterImage
                    context.DrawImage(characterImage, new Point(x, y), 1F);
                }

                // 绘制字符前面的干扰层
                for (int index = 0; index < 5; index++)
                {
                    Color color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                    float thickness = RandomNumberGenerator.GetInt32(7, 14) / 10F;
                    context.DrawLine(color, thickness,
                        new PointF(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height)),
                        new PointF(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height)));
                }

                // 穿过字符的Bezier曲线
                for (int index = 0; index < 2; index++)
                {
                    Color color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                    context.DrawBeziers(color, RandomNumberGenerator.GetInt32(10, 17) / 10F,
                        new PointF(0, RandomNumberGenerator.GetInt32(8, height - 8)),
                        new PointF(RandomNumberGenerator.GetInt32(width / 5, width / 2), RandomNumberGenerator.GetInt32(height)),
                        new PointF(RandomNumberGenerator.GetInt32(width / 2, width * 4 / 5),
                            RandomNumberGenerator.GetInt32(height)),
                        new PointF(width, RandomNumberGenerator.GetInt32(8, height - 8)));
                }

                // 前景噪点，让部分噪点直接覆盖字符
                for (int index = 0; index < 40; index++)
                {
                    Color color = noiseColors[RandomNumberGenerator.GetInt32(noiseColors.Length)];
                    int radius = RandomNumberGenerator.GetInt32(1, 3);
                    context.Fill(color,
                        new EllipsePolygon(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                            radius));
                }
            });
        }, 132, 46, 28);

        return (captchaKey, $"data:image/png;base64,{await ImageUtil.ConvertToBase64Image(image)}");
    }

    /// <inheritdoc />
    public async Task VerifyImageCaptcha(string captchaKey, string verificationCode)
    {
        if (!Guid.TryParseExact(captchaKey, "N", out _)
            || string.IsNullOrWhiteSpace(verificationCode)
            || !Regex.IsMatch(verificationCode, RegexConst.ImageCaptchaCode))
        {
            throw new UserFriendlyException("请输入图片验证码！");
        }

        verificationCode = verificationCode.Trim();

        // 获取缓存Key
        string cacheKey = CacheConst.GetCacheKey(CacheConst.ImageCaptcha, captchaKey);
        using CSRedisClientLock codeLock = _cache.Client.TryLock($"{cacheKey}:Lock", 10);
        if (codeLock == null)
        {
            throw new UserFriendlyException("操作过于频繁，请稍后重试！");
        }

        VerificationCodeCacheDto dto = await _cache.GetAsync<VerificationCodeCacheDto>(cacheKey);
        if (dto == null || dto.ClientIdentity != GlobalContext.ClientIdentity)
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }

        // 图片验证码验证一次后立即失效
        await _cache.DelAsync(cacheKey);

        // 忽略大小写
        if (!string.Equals(dto.VerificationCode, verificationCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("验证码无效或已过期！");
        }
    }
}
