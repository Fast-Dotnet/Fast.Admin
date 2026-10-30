// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CSRedis;
using SkiaSharp;

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
    /// 初始化验证码服务
    /// </summary>
    public CaptchaService(ICache cache)
    {
        _cache = cache;
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
        var backgroundColor = SKColor.Parse("EEF4FF");
        // 背景纹理颜色
        SKColor[] backgroundPatternColors =
        [
            new(147, 197, 253, 40),
            new(191, 219, 254, 55),
            new(165, 180, 252, 35),
            new(219, 234, 254, 60)
        ];
        // 验证码文字颜色
        SKColor[] textColors =
        [
            SKColor.Parse("1E3A8A"),
            SKColor.Parse("1D4ED8"),
            SKColor.Parse("3730A3"),
            SKColor.Parse("312E81"),
            SKColor.Parse("1E40AF")
        ];
        // 干扰线颜色
        SKColor[] lineColors =
        [
            new(59, 130, 246, 120),
            new(99, 102, 241, 105),
            new(96, 165, 250, 115),
            new(129, 140, 248, 95)
        ];
        // 噪点颜色
        SKColor[] noiseColors =
        [
            new(59, 130, 246, 125),
            new(96, 165, 250, 115),
            new(99, 102, 241, 100),
            new(30, 64, 175, 85)
        ];
        // 保留背景、字符旋转及前后两层干扰，使用同一组随机范围和图片尺寸
        using SKBitmap image = ImageUtil.GenImage((canvas, font, _, width, height) =>
        {
            // 设置背景
            using var fillPaint = new SKPaint();
            fillPaint.IsAntialias = true;
            using var strokePaint = new SKPaint();
            strokePaint.IsAntialias = true;
            strokePaint.Style = SKPaintStyle.Stroke;
            canvas.Clear(backgroundColor);

            // 背景随机纹理
            for (int index = 0; index < 18; index++)
            {
                fillPaint.Color = backgroundPatternColors[RandomNumberGenerator.GetInt32(backgroundPatternColors.Length)];
                int radius = RandomNumberGenerator.GetInt32(4, 15);
                canvas.DrawCircle(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height), radius,
                    fillPaint);
            }

            // 背景噪点
            for (int index = 0; index < 45; index++)
            {
                fillPaint.Color = noiseColors[RandomNumberGenerator.GetInt32(noiseColors.Length)];
                int radius = RandomNumberGenerator.GetInt32(1, 3);
                canvas.DrawCircle(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height), radius,
                    fillPaint);
            }

            // 字符后面的直线干扰
            for (int index = 0; index < 6; index++)
            {
                strokePaint.Color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                strokePaint.StrokeWidth = RandomNumberGenerator.GetInt32(8, 16) / 10F;
                canvas.DrawLine(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                    RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height), strokePaint);
            }

            // 字符后面的贝塞尔曲线
            for (int index = 0; index < 2; index++)
            {
                strokePaint.Color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                strokePaint.StrokeWidth = RandomNumberGenerator.GetInt32(10, 18) / 10F;
                using var pathBuilder = new SKPathBuilder();
                pathBuilder.MoveTo(0, RandomNumberGenerator.GetInt32(height));
                pathBuilder.CubicTo(RandomNumberGenerator.GetInt32(width / 4, width / 2), RandomNumberGenerator.GetInt32(height),
                    RandomNumberGenerator.GetInt32(width / 2, width * 3 / 4), RandomNumberGenerator.GetInt32(height), width,
                    RandomNumberGenerator.GetInt32(height));
                using SKPath path = pathBuilder.Detach();
                canvas.DrawPath(path, strokePaint);
            }

            // 围绕原有 42 × 48 字符画布的中心旋转，避免旋转后文字整体向右下偏移。
            for (int index = 0; index < dto.VerificationCode.Length; index++)
            {
                fillPaint.Color = textColors[RandomNumberGenerator.GetInt32(textColors.Length)];
                using var characterFont = new SKFont(font.Typeface, RandomNumberGenerator.GetInt32(25, 31));
                characterFont.Edging = SKFontEdging.Antialias;
                characterFont.Subpixel = true;
                int textX = RandomNumberGenerator.GetInt32(5, 9);
                int textY = RandomNumberGenerator.GetInt32(2, 7);
                int angle = RandomNumberGenerator.GetInt32(-18, 19);
                int x = 1 + index * 31 + RandomNumberGenerator.GetInt32(-2, 4);
                int y = -2 + RandomNumberGenerator.GetInt32(-2, 5);
                canvas.Save();
                canvas.Translate(x + 21, y + 24);
                canvas.RotateDegrees(angle);
                canvas.DrawText(dto.VerificationCode[index].ToString(), textX - 21, textY - 24 - characterFont.Metrics.Ascent,
                    SKTextAlign.Left, characterFont, fillPaint);
                canvas.Restore();
            }

            // 绘制字符前面的干扰层
            for (int index = 0; index < 5; index++)
            {
                strokePaint.Color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                strokePaint.StrokeWidth = RandomNumberGenerator.GetInt32(7, 14) / 10F;
                canvas.DrawLine(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height),
                    RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height), strokePaint);
            }

            // 穿过字符的贝塞尔曲线
            for (int index = 0; index < 2; index++)
            {
                strokePaint.Color = lineColors[RandomNumberGenerator.GetInt32(lineColors.Length)];
                strokePaint.StrokeWidth = RandomNumberGenerator.GetInt32(10, 17) / 10F;
                using var pathBuilder = new SKPathBuilder();
                pathBuilder.MoveTo(0, RandomNumberGenerator.GetInt32(8, height - 8));
                pathBuilder.CubicTo(RandomNumberGenerator.GetInt32(width / 5, width / 2), RandomNumberGenerator.GetInt32(height),
                    RandomNumberGenerator.GetInt32(width / 2, width * 4 / 5), RandomNumberGenerator.GetInt32(height), width,
                    RandomNumberGenerator.GetInt32(8, height - 8));
                using SKPath path = pathBuilder.Detach();
                canvas.DrawPath(path, strokePaint);
            }

            // 前景噪点，让部分噪点直接覆盖字符
            for (int index = 0; index < 40; index++)
            {
                fillPaint.Color = noiseColors[RandomNumberGenerator.GetInt32(noiseColors.Length)];
                int radius = RandomNumberGenerator.GetInt32(1, 3);
                canvas.DrawCircle(RandomNumberGenerator.GetInt32(width), RandomNumberGenerator.GetInt32(height), radius,
                    fillPaint);
            }
        }, 132, 46, 28);

        return (captchaKey, $"data:image/png;base64,{ImageUtil.ConvertToBase64Image(image)}");
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
