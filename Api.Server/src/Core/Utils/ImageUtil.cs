// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
    private static readonly FontFamily DefaultFont;

    /// <summary>
    /// 加粗字体
    /// </summary>
    private static readonly FontFamily BoldFont;

    /// <summary>
    /// 默认行高
    /// </summary>
    private static int DefaultLineHeight => 20;

    static ImageUtil()
    {
        // 使用随宿主发布的字体，避免不同操作系统的系统字体差异影响图片布局
        string basePath = Path.Combine(AppContext.BaseDirectory, "Assets/Fonts");
        var fontCollection = new FontCollection();
        DefaultFont = fontCollection.Add(Path.Combine(basePath, "NotoSansSC-Regular.ttf"));
        BoldFont = fontCollection.Add(Path.Combine(basePath, "NotoSansSC-Medium.ttf"));
    }

    /// <summary>
    /// 生成图片
    /// </summary>
    /// <param name="drawAction">操作 drawAction(image, font, color, lineHeight, width, height)</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="fontSize">文字大小</param>
    /// <param name="lineHeight">行高</param>
    /// <returns>生成的图片</returns>
    public static Image<Rgba32> GenImage(Action<Image<Rgba32>, Font, Color, int, int, int> drawAction, int width = 450,
        int height = 750, float fontSize = 16f, int? lineHeight = null)
    {
        lineHeight ??= DefaultLineHeight;

        using var image = new Image<Rgba32>(width, height);

        // 设置图片背景颜色，白色
        var backgroundColor = new Rgba32(255, 255, 255);
        image.Mutate(x => x.BackgroundColor(backgroundColor));

        // 固定字体
        var font = new Font(DefaultFont, fontSize);

        // 固定颜色
        Color color = Color.Black;

        // 绘制调试边框
        //var borderWidth = 1F;
        //var borderColor = Color.Red;
        //image.Mutate(ctx =>
        //{
        //    // 绘制上边框
        //    ctx.DrawLine(borderColor, borderWidth, new PointF(0, 0), new PointF(width, 0));
        //    // 绘制下边框
        //    ctx.DrawLine(borderColor, borderWidth, new PointF(0, height - 1), new PointF(width, height - 1));
        //    // 绘制左边框
        //    ctx.DrawLine(borderColor, borderWidth, new PointF(0, 0), new PointF(0, height));
        //    // 绘制右边框
        //    ctx.DrawLine(borderColor, borderWidth, new PointF(width - 1, 0), new PointF(width - 1, width));
        //});

        drawAction?.Invoke(image, font, color, lineHeight.Value, width, height);

        // 返回独立副本，避免 using 释放画布后调用方持有失效图像
        return image.Clone();
    }

    /// <summary>
    /// 将图片转换为 Base64 编码
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>PNG 图像的 Base64 编码字符串</returns>
    public static async Task<string> ConvertToBase64Image(Image image)
    {
        // 将图片转换为 Base64 字符串
        using var memoryStream = new MemoryStream();
        await image.SaveAsync(memoryStream, new PngEncoder());
        byte[] imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    /// <summary>
    /// 将图片转换为 Base64 编码
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>PNG 图像的 Base64 编码字符串</returns>
    public static string ConvertToBase64ImageSync(Image image)
    {
        // 将图片转换为 Base64 字符串
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, new PngEncoder());
        byte[] imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    /// <summary>
    /// 获取对应文本字体的文本宽度
    /// </summary>
    /// <returns>对应文本字体的文本宽度</returns>
    public static float GetFontWidthByContent(string content, Font font)
    {
        // 获取要渲染的文字宽度
        return TextMeasurer.MeasureSize(content, new TextOptions(font)).Width;
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
    /// <param name="drawAction">操作 drawAction(image, font, color, lineHeight, width, height)</param>
    /// <returns>生成的 Code 128 条形码</returns>
    public static Image GenBarCode_128(string content, int width = 200, int height = 200, int dWidth = 300,
        bool showContent = true, float fontSize = 16f, Action<Image<Rgba32>, Font, Color, int, int, int> drawAction = null)
    {
        return GenQrOrBarCode(BarcodeFormat.CODE_128, content, width, height, dWidth, showContent, fontSize, drawAction);
    }

    /// <summary>
    /// 生成 QrCode 二维码
    /// </summary>
    /// <param name="content">二维码内容</param>
    /// <param name="width">二维码宽度</param>
    /// <param name="height">二维码高度</param>
    /// <param name="drawAction">操作 drawAction(image, font, color, lineHeight, width, height)</param>
    /// <returns>生成的 QR Code 二维码</returns>
    public static Image GenQrCode(string content, int width = 200, int height = 200,
        Action<Image<Rgba32>, Font, Color, int, int, int> drawAction = null)
    {
        return GenQrOrBarCode(BarcodeFormat.QR_CODE, content, width, height, width, false, 16f, drawAction);
    }

    /// <summary>
    /// 生成条码或二维码
    /// </summary>
    /// <returns>生成的条码或二维码</returns>
    private static Image GenQrOrBarCode(BarcodeFormat barcodeFormat, string content, int width = 100, int height = 100,
        int dWidth = 100, bool showContent = true, float fontSize = 16f,
        Action<Image<Rgba32>, Font, Color, int, int, int> drawAction = null)
    {
        var barcodeWriterPixelData = new BarcodeWriter<PixelData>
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
        PixelData pixelData = barcodeWriterPixelData.Write(content);

        // 文字字体
        var textFont = new Font(BoldFont, fontSize);

        // 固定颜色
        Color color = Color.Black;

        // 判断是否显示详情文本
        if (showContent)
        {
            // 条形码的高度 + 字体大小 + 5 像素
            int dHeight = height + (int)fontSize + 5;

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

            // 写入详情文本，居中
            using Image<Rgba32> loadPixelData = GenImage((image, _, _, _, _, _) =>
            {
                // 获取条形码图片
                var barcodeImage = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

                // 绘制条形码
                image.Mutate(ctx =>
                {
                    // 绘制条形码
                    ctx.DrawImage(barcodeImage, new Point((dWidth - pixelData.Width) / 2, 0), 1f);

                    // 将文字写入底图中，文字居中显示
                    ctx.DrawText(content, textFont, color,
                        new PointF((dWidth - GetFontWidthByContent(content, textFont)) / 2, textY));
                });

                // 其余操作
                drawAction?.Invoke(image, textFont, color, DefaultLineHeight, dWidth, dHeight);
            }, dWidth, dHeight, fontSize);

            // 这里克隆一份，避免被提前释放
            return loadPixelData.Clone();
        }
        else
        {
            using Image<Rgba32> loadPixelData = GenImage((image, _, _, _, _, _) =>
            {
                // 获取条形码图片
                var barcodeImage = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

                // 绘制条形码
                image.Mutate(ctx =>
                {
                    // 绘制条形码
                    ctx.DrawImage(barcodeImage, new Point((dWidth - pixelData.Width) / 2, 0), 1f);
                });

                // 其余操作
                drawAction?.Invoke(image, textFont, color, DefaultLineHeight, dWidth, height);
            }, dWidth, height, fontSize);

            // 这里克隆一份，避免被提前释放
            return loadPixelData.Clone();
        }
    }
}
