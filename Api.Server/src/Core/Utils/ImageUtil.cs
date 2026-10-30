// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Runtime.InteropServices;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace Fast.Core;

/// <summary>
/// 图片工具类
/// </summary>
[SuppressSniffer]
public static class ImageUtil
{
    /// <summary>
    /// 默认字体
    /// </summary>
    /// <remarks>使用随宿主发布的字体，避免不同操作系统的系统字体差异影响图片布局</remarks>
    private static SKTypeface DefaultFont =>
        SKTypeface.FromFile(Path.Combine(Path.Combine(AppContext.BaseDirectory, "Assets/Fonts"), "NotoSansSC-Regular.ttf"));

    /// <summary>
    /// 加粗字体
    /// </summary>
    /// <remarks>使用随宿主发布的字体，避免不同操作系统的系统字体差异影响图片布局</remarks>
    private static SKTypeface BoldFont =>
        SKTypeface.FromFile(Path.Combine(Path.Combine(AppContext.BaseDirectory, "Assets/Fonts"), "NotoSansSC-Medium.ttf"));

    /// <summary>
    /// 默认行高
    /// </summary>
    private static int DefaultLineHeight => 20;

    /// <summary>
    /// 生成图片
    /// </summary>
    /// <param name="drawAction">绘制回调，参数为画布、字体、行高、宽度和高度；画布与字体由本方法释放，仅在回调期间有效</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="fontSize">文字大小</param>
    /// <param name="lineHeight">行高</param>
    /// <returns>独立的图像实例，由调用方负责释放</returns>
    public static SKBitmap GenImage(Action<SKCanvas, SKFont, int, int, int> drawAction, int width = 450, int height = 750,
        float fontSize = 16f, int? lineHeight = null)
    {
        lineHeight ??= DefaultLineHeight;

        var image = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));

        try
        {
            // 固定字体
            using var font = new SKFont(DefaultFont, fontSize);
            font.Edging = SKFontEdging.Antialias;
            font.Subpixel = true;

            using var canvas = new SKCanvas(image);

            // 设置图片背景颜色，白色
            canvas.Clear(SKColors.White);

            // 绘制调试边框
            float borderWidth = 1F;
            using var borderPaint = new SKPaint();
            borderPaint.Color = SKColors.Red;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.IsAntialias = true;
            // 描边向路径两侧扩展，内缩半个线宽，避免边框被画布边缘裁切。
            float borderInset = borderWidth / 2;
            canvas.DrawRect(new SKRect(borderInset, borderInset, width - borderInset, height - borderInset), borderPaint);

            drawAction?.Invoke(canvas, font, lineHeight.Value, width, height);

            return image;
        }
        catch
        {
            // 成功返回时位图由调用方接管；绘制失败时由本方法回收，避免泄漏。
            image.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 将图片转换为 Base64 编码
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>不带 Data URL 前缀的 PNG Base64 字符串</returns>
    /// <remarks>不会释放传入图像，图像的生命周期仍由调用方管理。</remarks>
    public static string ConvertToBase64Image(SKBitmap image)
    {
        // 将图片转换为 Base64 字符串
        using SKData imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        return Convert.ToBase64String(imageData.AsSpan());
    }

    /// <summary>
    /// 获取对应文本字体的文本宽度
    /// </summary>
    /// <param name="content">需要测量的文本</param>
    /// <param name="font">实际渲染使用的字体</param>
    /// <returns>文本渲染宽度，单位为像素</returns>
    public static float GetFontWidthByContent(string content, SKFont font)
    {
        // 获取要渲染的文字宽度
        return font.MeasureText(content);
    }

    /// <summary>
    /// 生成 Code_128 条形码
    /// </summary>
    /// <param name="content">条码内容</param>
    /// <param name="width">条码宽度</param>
    /// <param name="height">条码高度</param>
    /// <param name="dWidth">底图宽度</param>
    /// <param name="showContent">显示文字</param>
    /// <param name="fontSize">文字大小</param>
    /// <param name="drawAction">绘制回调，参数为画布、字体、行高、宽度和高度；画布与字体由本方法释放，仅在回调期间有效</param>
    /// <returns>Code 128 条形码图像，由调用方负责释放</returns>
    public static SKBitmap GenBarCode_128(string content, int width = 200, int height = 200, int dWidth = 300,
        bool showContent = true, float fontSize = 16f, Action<SKCanvas, SKFont, int, int, int> drawAction = null)
    {
        return GenQrOrBarCode(BarcodeFormat.CODE_128, content, width, height, dWidth, showContent, fontSize, drawAction);
    }

    /// <summary>
    /// 生成 QR Code 二维码
    /// </summary>
    /// <param name="content">二维码内容</param>
    /// <param name="width">二维码宽度</param>
    /// <param name="height">二维码高度</param>
    /// <param name="drawAction">绘制回调，参数为画布、字体、行高、宽度和高度；画布与字体由本方法释放，仅在回调期间有效</param>
    /// <returns>QR Code 图像，由调用方负责释放</returns>
    public static SKBitmap GenQrCode(string content, int width = 200, int height = 200,
        Action<SKCanvas, SKFont, int, int, int> drawAction = null)
    {
        return GenQrOrBarCode(BarcodeFormat.QR_CODE, content, width, height, width, false, 16f, drawAction);
    }

    /// <summary>
    /// 生成条码或二维码
    /// </summary>
    /// <param name="barcodeFormat">条形码或二维码格式</param>
    /// <param name="content">条形码或二维码的编码内容</param>
    /// <param name="width">条形码或二维码图案的目标宽度，单位为像素</param>
    /// <param name="height">条形码或二维码图案的目标高度，单位为像素</param>
    /// <param name="dWidth">包含条形码或二维码的底图宽度，单位为像素</param>
    /// <param name="showContent">是否在条形码或二维码下方显示编码内容</param>
    /// <param name="fontSize">编码内容文字的字号，单位为像素</param>
    /// <param name="drawAction">附加绘制回调，参数依次为画布、字体、行高及输出图片的宽度和高度</param>
    /// <returns>生成的条码或二维码，由调用方负责释放</returns>
    private static SKBitmap GenQrOrBarCode(BarcodeFormat barcodeFormat, string content, int width = 100, int height = 100,
        int dWidth = 100, bool showContent = true, float fontSize = 16f,
        Action<SKCanvas, SKFont, int, int, int> drawAction = null)
    {
        var writer = new BarcodeWriter<PixelData>
        {
            // 使用 ITF 格式，不能被现在常用的支付宝，微信扫出来
            // 如果想生成可识别的，可以使用 CODE_128 格式
            Format = barcodeFormat,
            Options = new EncodingOptions
            {
                // 条形码宽度
                Width = width,
                // 条形码高度
                Height = height,
                // 条形码前后间距
                Margin = 0
            },
            Renderer = new PixelDataRenderer
            {
                Foreground = new PixelDataRenderer.Color(unchecked((int)0xFF000000)),
                Background = new PixelDataRenderer.Color(unchecked((int)0xFFFFFFFF))
            }
        };

        // 写入内容
        PixelData pixelData = writer.Write(content);

        // 文字字体
        var textFont = new SKFont(BoldFont, fontSize);
        textFont.Edging = SKFontEdging.Antialias;
        textFont.Subpixel = true;

        int imageHeight = showContent ? height + (int)fontSize + 5 : height;

        // ZXing 的像素缓冲区采用 BGRA 顺序，直接复制可避免逐像素转换。
        var barcodeImage = new SKBitmap(
            new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        Marshal.Copy(pixelData.Pixels, 0, barcodeImage.GetPixels(), pixelData.Pixels.Length);

        SKBitmap skBitmap = GenImage((image, _, lineHeight, imageWidth, _) =>
        {
            // 绘制条形码
            image.DrawBitmap(barcodeImage, (imageWidth - pixelData.Width) / 2F, 0, new SKSamplingOptions(SKFilterMode.Nearest));

            // 判断是否显示详情文本
            if (showContent)
            {
                // 文字Y轴位置
                int textY = pixelData.Height;

                // 判断如果是条形码，则 -4 二维码则 -10
                switch (barcodeFormat)
                {
                    case BarcodeFormat.ITF:
                    case BarcodeFormat.CODE_39:
                    case BarcodeFormat.CODE_128:
                        //textY -= 5;
                        break;
                    case BarcodeFormat.QR_CODE:
                        textY -= 5;
                        break;
                }

                // 将文字写入底图中，文字居中显示
                using var paint = new SKPaint();
                paint.Color = SKColors.Black;
                paint.IsAntialias = true;
                // SkiaSharp 使用基线，需根据字体度量换算。
                image.DrawText(content, (imageWidth - GetFontWidthByContent(content, textFont)) / 2F,
                    textY - textFont.Metrics.Ascent, SKTextAlign.Left, textFont, paint);
            }

            // 其余操作
            drawAction?.Invoke(image, textFont, lineHeight, imageWidth, imageHeight);
        }, dWidth, imageHeight, fontSize);

        return skBitmap;
    }
}
