// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Buffers.Binary;
using System.Drawing;
using System.Text;
using KGySoft.Drawing.Imaging;
using SkiaSharp;

namespace Fast.File.Applications;

public partial class FileApplication
{
    /// <summary>
    /// 将指定静态图片帧或动画帧解码到调用方提供的位图缓冲区，仅接受完整解码成功的结果。
    /// </summary>
    /// <param name="codec">已打开的图片解码器，由调用方管理生命周期</param>
    /// <param name="image">接收像素的可写位图，尺寸及像素格式必须适用于当前解码器；解码时会覆盖或更新其内容</param>
    /// <param name="frameIndex">目标帧的从零开始索引，静态图片传入 0</param>
    /// <param name="priorFrame">缓冲区中可复用的已解码前帧索引；-1 表示没有可复用前帧，由解码器重建依赖</param>
    /// <remarks>
    /// 指定非负的 <paramref name="priorFrame"/> 时，<paramref name="image"/> 必须保留对应帧的有效像素。
    /// 本方法不执行 EXIF 方向变换，也不会释放传入的解码器或位图；部分解码成功仍视为失败。
    /// </remarks>
    /// <exception cref="UserFriendlyException">解码结果不是 <see cref="SKCodecResult.Success"/> 时抛出。</exception>
    private static void DecodeImageFrame(SKCodec codec, SKBitmap image, int frameIndex, int priorFrame)
    {
        // RowBytes 是像素缓冲区每行的实际字节跨度，不假定它一定等于宽度乘以 4。
        // 帧索引及可复用前帧索引告诉解码器如何还原当前帧，像素直接写入已有位图。
        SKCodecResult result = codec.GetPixels(image.Info, image.GetPixels(), image.RowBytes,
            new SKCodecOptions(frameIndex, priorFrame));
        if (result != SKCodecResult.Success)
        {
            // 部分解码成功也可能留下缺失像素，不能将不完整内容作为正常缩略图输出。
            throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
        }
    }

    /// <summary>
    /// 使用 Mitchell 三次采样，将源位图缩放到指定尺寸并生成独立的透明底位图。
    /// </summary>
    /// <param name="source">待缩放的源位图，由调用方管理生命周期</param>
    /// <param name="width">目标宽度，单位为像素，必须大于 0</param>
    /// <param name="height">目标高度，单位为像素，必须大于 0</param>
    /// <returns>采用 BGRA8888 像素格式及预乘透明度的新位图，由调用方负责释放</returns>
    /// <remarks>
    /// 直接按给定宽高重采样，是否保持比例及是否允许放大由调用方决定；不会修改或释放源位图。
    /// 内部画布自动释放，绘制异常时会回收尚未返回的目标位图。
    /// </remarks>
    private static SKBitmap ResizeImage(SKBitmap source, int width, int height)
    {
        var result = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        try
        {
            using var canvas = new SKCanvas(result);
            // 清空为透明底，避免未覆盖区域出现未初始化像素；目标矩形决定最终输出尺寸。
            canvas.Clear(SKColors.Transparent);
            // Mitchell 三次重采样兼顾缩小后的平滑度与细节，源图和目标图之间没有共享可写像素。
            canvas.DrawBitmap(source, new SKRect(0, 0, width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
            return result;
        }
        catch
        {
            // 只有成功返回才转交位图所有权，失败时仍由本方法回收。
            result.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 逐帧解码、合成并缩放 GIF，将保留帧延时及循环设置的缩略动画写入输出流。
    /// </summary>
    /// <param name="codec">原始 GIF 的解码器，提供帧像素、帧延时、帧处置方式及循环设置</param>
    /// <param name="image">与原图画布同尺寸的可复用帧缓冲区，逐帧解码时会修改其像素</param>
    /// <param name="output">从当前位置接收 GIF 数据的可写流，由调用方关闭</param>
    /// <param name="width">缩略动画的画布宽度，单位为像素</param>
    /// <param name="height">缩略动画的画布高度，单位为像素</param>
    /// <param name="cancellationToken">取消令牌，在开始处理每一帧时检查</param>
    /// <remarks>
    /// <para>
    /// SkiaSharp 负责像素解码及缩放，KGySoft 负责 GIF 编码；使用 Wu 调色板量化及 Floyd–Steinberg 抖动，颜色可能受 GIF 调色板限制影响。
    /// 原始毫秒帧延时转换为 GIF 的百分之一秒单位，不能作为下一帧基底的 RestorePrevious 帧由解码器重建依赖。
    /// </para>
    /// <para>
    /// 每帧按完整画布输出，并在显示后清空，避免透明区域残留上一帧内容。
    /// 内部编码器及临时像素资源自动释放，传入的解码器、帧缓冲区和输出流保持由调用方管理；取消或异常不回滚已写入数据。
    /// </para>
    /// </remarks>
    /// <exception cref="UserFriendlyException">任一帧未能完整解码时抛出。</exception>
    /// <exception cref="OperationCanceledException">处理帧前收到取消请求。</exception>
    /// <exception cref="IOException">写入 GIF 数据失败。</exception>
    private static void WriteGifThumbnail(SKCodec codec, SKBitmap image, Stream output, int width, int height,
        CancellationToken cancellationToken)
    {
        // SkiaSharp 不提供 GIF 编码器；KGySoft 仅负责将缩放后的像素编码回 GIF。
        using var encoder = new GifEncoder(output, new Size(width, height));
        // 不附加编码器自身的说明信息，输出内容只包含图片及播放所需的数据。
        encoder.AddMetaInfo = false;
        // SkiaSharp 用负数表示无限循环、0 表示不重复；编码器对应使用 0 和 null。
        encoder.RepeatCount = codec.RepetitionCount < 0 ? 0 :
            codec.RepetitionCount > 0 ? codec.RepetitionCount : null;

        SKCodecFrameInfo[] frames = codec.FrameInfo;
        // 某些静态 GIF 的 FrameCount 为 0，仍需按第 0 帧写出一张完整图片。
        for (int index = 0; index < Math.Max(1, codec.FrameCount); index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // RestorePrevious 帧不能作为下一帧的合成基底，交由解码器重新构建所需依赖帧。
            int priorFrame = index > 0 && frames[index - 1].DisposalMethod != SKCodecAnimationDisposalMethod.RestorePrevious
                ? index - 1
                : -1;
            DecodeImageFrame(codec, image, index, priorFrame);
            using SKBitmap thumbnail = ResizeImage(image, width, height);
            // 此处 Size 来自跨平台的 System.Drawing.Primitives，不依赖 GDI+ 或 System.Drawing.Common。
            using IReadWriteBitmapData pixels = BitmapDataFactory.CreateBitmapData(thumbnail.GetPixels(), new Size(width, height),
                thumbnail.RowBytes, KnownPixelFormat.Format32bppPArgb);
            // pixels 借用缩略图的预乘 BGRA 内存；GIF 使用调色板，将其转换为最多 256 色的索引像素。
            // Wu 算法选择调色板，Floyd–Steinberg 将量化误差扩散到邻近像素，减少明显色带。
            using IReadWriteBitmapData indexed = pixels.Clone(KnownPixelFormat.Format8bppIndexed, OptimizedPaletteQuantizer.Wu(),
                ErrorDiffusionDitherer.FloydSteinberg);
            // 解码器给出的延时单位是毫秒，GIF 文件中的延时单位是 10 毫秒，因此向下取整除以 10。
            int delay = frames.Length > index ? frames[index].Duration / 10 : 0;
            // 每帧已合成为完整画布；显示后清空，避免透明区域残留上一帧内容。
            encoder.AddImage(indexed, delay: delay, disposalMethod: GifGraphicDisposalMethod.RestoreToBackground);
        }
    }

    /// <summary>
    /// 将位图编码为使用 BITMAPV4HEADER 和 RGBA 位掩码的 32 位 BMP，保留透明度。
    /// </summary>
    /// <param name="image">已完成缩放的源位图，不会被修改或释放</param>
    /// <param name="output">从当前位置接收 BMP 文件头及像素数据的可写流，由调用方关闭</param>
    /// <remarks>
    /// 将像素转换为非预乘透明度的 BGRA8888 格式，以负高度表示自顶向下的像素行顺序。
    /// 内部像素副本、图像包装及二进制写入器自动释放；写入器不会关闭输出流。
    /// </remarks>
    /// <exception cref="UserFriendlyException">无法从源位图读取目标格式的像素时抛出。</exception>
    /// <exception cref="OverflowException">像素字节数或 BMP 文件总长度超出整数范围时抛出。</exception>
    /// <exception cref="IOException">写入 BMP 文件头或像素数据失败。</exception>
    private static void WriteBmpThumbnail(SKBitmap image, Stream output)
    {
        // SkiaSharp 不提供 BMP 编码器；写入带 alpha 位掩码的标准 BITMAPV4HEADER，保留透明度。
        // BMP 位掩码描述的是独立颜色值，先将画布使用的预乘颜色转换为非预乘 BGRA。
        var imageInfo = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var pixels = new SKBitmap(imageInfo);
        using var source = SKImage.FromBitmap(image);
        if (!source.ReadPixels(imageInfo, pixels.GetPixels(), pixels.RowBytes, 0, 0))
        {
            throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
        }

        // 文件由 14 字节 BITMAPFILEHEADER、108 字节 BITMAPV4HEADER 和像素区组成。
        const int PixelOffset = 14 + 108;
        // 按实际行跨度计算像素区长度，并用 checked 防止长度溢出。
        int pixelBytes = checked(pixels.RowBytes * pixels.Height);
        // BMP 的整数采用小端序；leaveOpen=true，释放写入器时仍保留调用方的输出流。
        using var writer = new BinaryWriter(output, Encoding.UTF8, true);
        writer.Write((ushort)0x4D42); // 小端字节为 42 4D，即 BMP 文件签名“BM”。
        writer.Write(checked(PixelOffset + pixelBytes)); // 包括文件头在内的总字节数。
        writer.Write(0); // 两个 2 字节保留字段合计 4 字节，均填 0。
        writer.Write(PixelOffset); // 从文件开头到像素区的字节偏移。

        writer.Write(108); // 当前 DIB 头的长度，即 BITMAPV4HEADER。
        writer.Write(pixels.Width);
        writer.Write(-pixels.Height); // 负高度表示像素从上向下存储，无需倒序写入各行。
        writer.Write((ushort)1); // 颜色平面数，BMP 固定为 1。
        writer.Write((ushort)32); // 每像素 32 位，即 BGRA 各占 8 位。
        writer.Write(3); // BI_BITFIELDS：使用下面的通道位掩码解释像素值。
        writer.Write(pixelBytes);
        writer.Write(3780); // 水平方向的每米像素数，约为 96 DPI。
        writer.Write(3780); // 垂直方向的每米像素数。
        writer.Write(0); // 调色板颜色数；32 位直接颜色不需要调色板。
        writer.Write(0); // 重要颜色数；0 表示全部颜色都重要。
        writer.Write(0x00FF0000u); // 红色通道位掩码。
        writer.Write(0x0000FF00u); // 绿色通道位掩码。
        writer.Write(0x000000FFu); // 蓝色通道位掩码。
        writer.Write(0xFF000000u); // Alpha 通道位掩码，保留透明度。
        writer.Write(0x73524742u); // LCS_sRGB：按 sRGB 色彩空间解释颜色。
        writer.Write(new byte[48]); // CIEXYZ 端点占 36 字节，三个 Gamma 字段占 12 字节；sRGB 模式下填 0。
        // 每行 BGRA 本身已满足 4 字节对齐，直接写出整个像素缓冲区。
        writer.Write(pixels.GetPixelSpan());
    }

    /// <summary>
    /// 写入 JPEG 起始标记及仅包含 EXIF 方向标签的 APP1 段，供后续拼接已编码的 JPEG 图像数据。
    /// </summary>
    /// <param name="output">接收 JPEG 文件头及 EXIF APP1 段的可写流，由调用方关闭</param>
    /// <param name="origin">从原始 JPEG 解码器读取的编码方向</param>
    /// <remarks>
    /// 本方法只写入前缀，不旋转或镜像像素，也不复制原图的其他 EXIF 元数据。
    /// 调用方随后应跳过已编码 JPEG 的前 2 字节起始标记，再续写剩余数据，避免重复写入 JPEG 起始标记。
    /// </remarks>
    /// <exception cref="IOException">写入 JPEG 文件头或 EXIF 方向数据失败。</exception>
    private static void WriteJpegOrientation(Stream output, SKEncodedOrigin origin)
    {
        // 保留原图的 EXIF 方向，避免手机照片的缩略图在浏览器中横置或镜像。
        // 像素仍按原始宽高缩放，与原有文件命名和尺寸规则一致。
        // 共 38 字节：JPEG 起始标记 2 + APP1 标记 2 + APP1 长度字段 2 + EXIF/TIFF 内容 32。
        // 新数组默认填 0，未显式赋值的高位、补齐字段和下一目录偏移均保持 0。
        byte[] header = new byte[38];
        // JPEG 使用 FF D8 开始，紧接 FF E1 标识承载 EXIF 的 APP1 段。
        header[0] = 0xFF;
        header[1] = 0xD8;
        header[2] = 0xFF;
        header[3] = 0xE1;
        header[5] = 34; // APP1 段长度为 34，包含长度字段自身的 2 字节，不包含 FF E1 标记。
        "Exif\0\0"u8.CopyTo(header.AsSpan(6));
        // TIFF 头从文件偏移 12 开始；“II”表示后续 TIFF 字段采用小端序。
        header[12] = (byte)'I';
        header[13] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(14), 42); // TIFF 固定标识。
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), 8); // 首个 IFD 相对 TIFF 头起点偏移 8 字节。
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), 1); // IFD 中只有一个标签项。
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(22), 0x0112); // Orientation 方向标签。
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(24), 3); // 标签值类型为 SHORT，即 16 位无符号整数。
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(26), 1); // 标签只包含一个方向值。
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(30), (ushort)origin); // 值可放入 4 字节值槽，直接内联保存。
        output.Write(header);
    }
}
