// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Buffers.Binary;
using SkiaSharp;

namespace Fast.File.Applications;

public partial class FileApplication
{
    // 当前 SkiaSharp 原生 PNG 解码器只读取默认图像。这里处理 APNG 容器和帧合成，
    // 各帧的像素解码、缩放和 PNG 压缩仍由 SkiaSharp 完成。
    /// <summary>
    /// 源 PNG 文件中一段数据的位置及长度，用于延迟读取压缩像素或共享数据块。
    /// </summary>
    /// <param name="Offset">相对源文件起始位置的绝对字节偏移</param>
    /// <param name="Length">待读取的字节数</param>
    /// <remarks>
    /// 在共享前缀中表示包含长度、类型、数据及 CRC 的完整块；在帧数据中表示 IDAT 有效载荷或去掉序列号的 fdAT 有效载荷。
    /// 只记录位置，不保存压缩数据副本，因此后续写出期间源文件必须保持不变。
    /// </remarks>
    private sealed record PngDataSegment(long Offset, int Length);

    /// <summary>
    /// PNG 默认图像或 APNG 动画帧的控制信息及压缩像素数据位置。
    /// </summary>
    /// <param name="Control">完整的 26 字节 fcTL 有效载荷；为 null 时表示不参与动画播放的独立默认图像</param>
    /// <param name="Data">按源文件顺序排列的压缩像素数据段，不包含 fdAT 序列号</param>
    private sealed record PngAnimationFrame(byte[] Control, List<PngDataSegment> Data);

    /// <summary>
    /// 用于重新解码和缩放 APNG 的画布头部、播放设置及帧索引。
    /// </summary>
    /// <param name="Header">原始 IHDR 的 13 字节有效载荷，包含画布尺寸、位深、颜色类型及压缩等设置</param>
    /// <param name="Plays">acTL 中的播放次数，0 表示无限循环</param>
    /// <param name="Prefix">像素数据之前需要复用的共享块，例如调色板及透明度信息，包含完整块字节</param>
    /// <param name="Frames">按源文件顺序排列的图像帧，可能包含不参与动画播放的独立默认图像</param>
    private sealed record PngAnimation(byte[] Header, uint Plays, List<PngDataSegment> Prefix, List<PngAnimationFrame> Frames);

    /// <summary>
    /// 扫描 PNG 数据块，提取 APNG 动画控制信息及各帧压缩数据位置，并检查基本结构和帧边界。
    /// </summary>
    /// <param name="path">已经通过基础 PNG 格式识别的本地图片路径</param>
    /// <returns>包含画布信息、循环次数及帧数据索引的动画描述；未发现动画控制信息时返回 null</returns>
    /// <remarks>
    /// <para>
    /// 解析 IHDR、acTL、fcTL、IDAT 和 fdAT，保留动画帧数量、帧尺寸、偏移、延时、混合及处置方式。
    /// 默认静态图像不参与动画时，将其单独记录为控制信息为空的首个图像帧。
    /// </para>
    /// <para>
    /// 本方法假定输入已识别为 PNG，直接跳过 8 字节签名；不校验源块 CRC 或 APNG 序列号，也不解码像素，不能替代完整的文件有效性校验。
    /// 压缩数据仅记录源文件偏移，返回后源文件应保持可读且内容不变；内部文件流在返回或异常时释放。
    /// </para>
    /// </remarks>
    /// <exception cref="UserFriendlyException">数据块长度、帧尺寸或位置无效，或动画缺少结束块、帧数据及声明数量对应的动画帧时抛出。</exception>
    /// <exception cref="IOException">打开或读取源文件失败，或读取过程中遇到意外的文件结束。</exception>
    private static PngAnimation ReadPngAnimation(string path)
    {
        using FileStream stream = System.IO.File.OpenRead(path);
        // 上游已经识别 PNG 格式，跳过固定的 8 字节文件签名，从第一个数据块开始扫描。
        stream.Position = 8;
        // Header 保存 IHDR 的 13 字节数据；动画不存在时 frameCount 保持 0。
        byte[] header = null;
        uint frameCount = 0;
        uint plays = 0;
        var prefix = new List<PngDataSegment>();
        var frames = new List<PngAnimationFrame>();
        // current 收集当前图像帧的数据段；前缀块只收集到首个 IDAT 出现之前。
        PngAnimationFrame current = null;
        bool hasImageData = false;
        bool hasEnd = false;
        // 每个块的头部包含 4 字节大端长度和 4 字节 ASCII 类型；acTL 数据固定为 8 字节。
        Span<byte> chunkHeader = stackalloc byte[8];
        Span<byte> animationControl = stackalloc byte[8];
        while (stream.Position < stream.Length)
        {
            // 即使数据为空，一个 PNG 块也至少需要“长度 4 + 类型 4 + CRC 4”共 12 字节。
            if (stream.Length - stream.Position < 12)
            {
                throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
            }

            stream.ReadExactly(chunkHeader);
            int length = BinaryPrimitives.ReadInt32BigEndian(chunkHeader);
            // 读取块头后，剩余内容必须容得下声明的数据和尾部 CRC，避免按错误长度读取越界。
            if (length < 0 || length > stream.Length - stream.Position - 4)
            {
                throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
            }

            // offset 指向有效载荷开头；后续只保存偏移，等实际解码该帧时再读取数据。
            long offset = stream.Position;
            ReadOnlySpan<byte> type = chunkHeader[4..];
            if (type.SequenceEqual("IHDR"u8) && length == 13)
            {
                // IHDR 的前 8 字节是画布宽高，后 5 字节描述位深、颜色类型、压缩、过滤和隔行方式。
                header = new byte[13];
                stream.ReadExactly(header);
            }
            else if (type.SequenceEqual("acTL"u8) && length == 8)
            {
                // acTL 声明动画帧数及播放次数；播放次数为 0 表示无限循环，动画帧数必须大于 0。
                stream.ReadExactly(animationControl);
                frameCount = BinaryPrimitives.ReadUInt32BigEndian(animationControl);
                plays = BinaryPrimitives.ReadUInt32BigEndian(animationControl[4..]);
                if (frameCount == 0)
                {
                    throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
                }
            }
            else if (type.SequenceEqual("fcTL"u8) && length == 26)
            {
                // fcTL：序号[0..3]、宽[4..7]、高[8..11]、X[12..15]、Y[16..19]、延时[20..23]。
                // 最后两字节分别是处置方式[24]和混合方式[25]；尺寸和坐标均以原图画布为基准。
                byte[] control = new byte[26];
                stream.ReadExactly(control);
                int width = BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(4));
                int height = BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(8));
                int x = BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(12));
                int y = BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(16));
                // 局部帧必须落在画布内；处置只允许 0/1/2，混合只允许 0/1。
                // 坐标加尺寸时先提升为 long，防止整数溢出掩盖越界。
                if (header == null
                    || width <= 0
                    || height <= 0
                    || x < 0
                    || y < 0
                    || (long)x + width > BinaryPrimitives.ReadInt32BigEndian(header)
                    || (long)y + height > BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(4))
                    || control[24] > 2
                    || control[25] > 1)
                {
                    throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
                }

                // 每个 fcTL 开始一个新的动画帧，后续 IDAT 或 fdAT 都归入这个帧的数据列表。
                current = new PngAnimationFrame(control, []);
                frames.Add(current);
            }
            else if (type.SequenceEqual("IDAT"u8))
            {
                if (frameCount == 0)
                {
                    // APNG 的 acTL 应在首个 IDAT 之前出现；未声明动画时交回静态 PNG 流程。
                    return null;
                }

                hasImageData = true;
                if (current == null)
                {
                    // 默认静态图可不参与动画；保留它作为独立的 PNG 默认图像。
                    current = new PngAnimationFrame(null, []);
                    frames.Add(current);
                }

                // IDAT 本身不含动画序号，可直接把完整有效载荷作为当前帧的压缩像素数据。
                current.Data.Add(new PngDataSegment(offset, length));
            }
            else if (type.SequenceEqual("fdAT"u8) && current?.Control != null && length >= 4)
            {
                // fdAT 前 4 字节是 APNG 序号，剩余字节才是压缩数据；重建普通 PNG 帧时必须去掉序号。
                current.Data.Add(new PngDataSegment(offset + 4, length - 4));
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                // IEND 表示文件数据块结束，动画完整性检查必须确认曾读到该结束块。
                hasEnd = true;
                break;
            }
            else if (!hasImageData && !type.SequenceEqual("acTL"u8) && !type.SequenceEqual("fcTL"u8))
            {
                // 调色板、透明度等前置块是解码各帧所需的共享上下文。
                // offset-8 回到块头，length+12 覆盖整个块，回读时可连同原始 CRC 一起复制。
                prefix.Add(new PngDataSegment(offset - 8, length + 12));
            }

            // 各分支可能只读取了部分数据，统一定位到“有效载荷末尾 + 4 字节 CRC”，继续扫描下一个块。
            stream.Position = offset + length + 4;
        }

        if (frameCount == 0)
        {
            // 没有动画控制信息的文件按普通 PNG 处理，本方法不为其创建动画描述。
            return null;
        }

        // acTL 只统计带 fcTL 的动画帧，不统计独立默认图像；每个记录的图像帧都必须有像素数据。
        if (!hasEnd || frames.Count(frame => frame.Control != null) != frameCount || frames.Any(frame => frame.Data.Count == 0))
        {
            throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
        }

        return new PngAnimation(header, plays, prefix, frames);
    }

    /// <summary>
    /// 按原始 APNG 的混合及处置规则合成各帧，缩放后重新编码为保留播放设置的 APNG 缩略图。
    /// </summary>
    /// <param name="sourcePath">生成动画索引时使用的同一本地 PNG 文件路径，文件内容须保持不变</param>
    /// <param name="animation">由 <see cref="ReadPngAnimation"/> 提取的画布头部、循环次数及帧数据索引</param>
    /// <param name="image">与原图画布尺寸一致的可写帧缓冲区，方法开始时清空，并在帧合成期间反复修改</param>
    /// <param name="output">从当前位置接收完整 APNG 数据的可写流，由调用方关闭</param>
    /// <param name="width">缩略动画的画布宽度，单位为像素</param>
    /// <param name="height">缩略动画的画布高度，单位为像素</param>
    /// <param name="cancellationToken">取消令牌，在开始处理每个默认图像或动画帧时检查</param>
    /// <remarks>
    /// <para>
    /// 从源文件回读每帧的压缩数据，重建临时 PNG 交由 SkiaSharp 解码，再按原始 Source/Over 混合及 None/Background/Previous 处置规则合成。
    /// 缩放后的动画帧均覆盖完整输出画布，使用 Source 混合方式，并重新生成控制块、序列号及 CRC，保留原始帧延时和循环次数。
    /// </para>
    /// <para>
    /// 首个默认图像若不参与动画，仍作为独立默认图像输出，不计入 acTL 的动画帧数。
    /// 内部源文件流、画布、解码器及临时位图自动释放，传入的帧缓冲区和输出流不由本方法释放；取消或异常不会回滚已写入数据。
    /// </para>
    /// </remarks>
    /// <exception cref="UserFriendlyException">重建的帧无法识别或不能完整解码时抛出。</exception>
    /// <exception cref="OperationCanceledException">处理默认图像或动画帧前收到取消请求。</exception>
    /// <exception cref="IOException">读取源数据或写入 APNG 缩略图失败，或源数据被截断。</exception>
    private static void WritePngAnimationThumbnail(string sourcePath, PngAnimation animation, SKBitmap image, Stream output,
        int width, int height, CancellationToken cancellationToken)
    {
        using FileStream source = System.IO.File.OpenRead(sourcePath);
        using var canvas = new SKCanvas(image);
        using var paint = new SKPaint();
        // image 是原图大小的合成画布；每生成一个尺寸都重新从透明底开始，不能沿用上一次处理的末帧。
        canvas.Clear(SKColors.Transparent);
        // fcTL 和 fdAT 共用一个从 0 开始递增的序号；独立默认图像不计入动画帧数。
        uint sequence = 0;
        uint frameCount = (uint)animation.Frames.Count(frame => frame.Control != null);
        for (int index = 0; index < animation.Frames.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PngAnimationFrame frame = animation.Frames[index];
            byte[] control = frame.Control;
            // 带 fcTL 的帧可能只是画布上的局部矩形；独立默认图像则使用完整原图尺寸且从左上角开始。
            int frameWidth = control == null ? image.Width : BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(4));
            int frameHeight = control == null ? image.Height : BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(8));
            int x = control == null ? 0 : BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(12));
            int y = control == null ? 0 : BinaryPrimitives.ReadInt32BigEndian(control.AsSpan(16));
            // 将当前帧包装成临时普通 PNG，让 SkiaSharp 解码像素，无需自行实现 PNG 解压及过滤算法。
            using var frameStream = new MemoryStream();
            // 固定 PNG 签名：89 50 4E 47 0D 0A 1A 0A。
            frameStream.Write([137, 80, 78, 71, 13, 10, 26, 10]);
            // 克隆 IHDR 后仅改帧尺寸，沿用原图位深和颜色类型；不能修改多个帧共享的原始 Header。
            byte[] header = (byte[])animation.Header.Clone();
            BinaryPrimitives.WriteInt32BigEndian(header, frameWidth);
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), frameHeight);
            WritePngChunk(frameStream, "IHDR"u8, header);
            // 调色板 PNG 的压缩像素依赖 PLTE/tRNS 等共享块，先将这些完整块复制到临时文件。
            foreach (PngDataSegment segment in animation.Prefix)
            {
                source.Position = segment.Offset;
                byte[] bytes = new byte[segment.Length];
                source.ReadExactly(bytes);
                frameStream.Write(bytes);
            }

            foreach (PngDataSegment segment in frame.Data)
            {
                source.Position = segment.Offset;
                byte[] bytes = new byte[segment.Length];
                source.ReadExactly(bytes);
                // 输入索引已去掉 fdAT 序号，所以所有帧的数据都可以按普通 PNG 的 IDAT 写入。
                WritePngChunk(frameStream, "IDAT"u8, bytes);
            }

            WritePngChunk(frameStream, "IEND"u8, []);
            // 临时 PNG 已写完，回到起点供解码器读取；patch 只保存这一帧局部矩形的像素。
            frameStream.Position = 0;
            using SKCodec codec = SKCodec.Create(frameStream) ?? throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
            using var patch = new SKBitmap(new SKImageInfo(frameWidth, frameHeight, SKColorType.Bgra8888, SKAlphaType.Premul));
            DecodeImageFrame(codec, patch, 0, -1);
            // 处置方式 2=Previous：播放后要恢复绘制前的整张画布，所以必须在叠加当前帧前保存快照。
            using SKBitmap previous = control != null && control[24] == 2 ? image.Copy() : null;
            // 混合方式 0=Source，连透明像素也覆盖；1=Over，按 Alpha 叠加在已有画布上。
            paint.BlendMode = control == null || control[25] == 0 ? SKBlendMode.Src : SKBlendMode.SrcOver;
            // 此处只按原坐标合成，不缩放局部帧；整个画布合成完成后再统一缩小，避免帧位置取整不一致。
            canvas.DrawBitmap(patch, x, y, new SKSamplingOptions(SKFilterMode.Nearest), paint);
            using SKBitmap thumbnail = ResizeImage(image, width, height);
            using SKData png = thumbnail.Encode(SKEncodedImageFormat.Png, 100);
            ReadOnlySpan<byte> encoded = png.AsSpan();
            if (index == 0)
            {
                // 输出文件的签名、IHDR 和 acTL 只写一次；IHDR 来自缩放后的 PNG，宽高及颜色信息与输出像素一致。
                output.Write(encoded[..8]);
                // 签名占 8 字节，IHDR 块的长度和类型再占 8 字节，因此其 13 字节数据从偏移 16 开始。
                WritePngChunk(output, "IHDR"u8, encoded.Slice(16, 13));
                byte[] animationControl = new byte[8];
                BinaryPrimitives.WriteUInt32BigEndian(animationControl, frameCount);
                BinaryPrimitives.WriteUInt32BigEndian(animationControl.AsSpan(4), animation.Plays);
                WritePngChunk(output, "acTL"u8, animationControl);
            }

            if (control != null)
            {
                // 新数组默认填 0：帧偏移为 (0,0)，处置为 None，混合为 Source；每帧直接替换整个输出画布。
                byte[] outputControl = new byte[26];
                BinaryPrimitives.WriteUInt32BigEndian(outputControl, sequence++);
                BinaryPrimitives.WriteInt32BigEndian(outputControl.AsSpan(4), width);
                BinaryPrimitives.WriteInt32BigEndian(outputControl.AsSpan(8), height);
                // 原始延时由 2 字节分子和 2 字节分母构成，直接复制，避免换算后改变动画速度。
                control.AsSpan(20, 4).CopyTo(outputControl.AsSpan(20));
                // 输出完整合成帧，用 Source 替换画布，保留透明像素及原始帧延时。
                WritePngChunk(output, "fcTL"u8, outputControl);
            }

            // 每张缩放后的普通 PNG 只提取 IDAT 压缩数据，文件级头尾和动画控制块由本方法统一组织。
            for (int offset = 8; offset < encoded.Length;)
            {
                int length = BinaryPrimitives.ReadInt32BigEndian(encoded[offset..]);
                if (encoded.Slice(offset + 4, 4).SequenceEqual("IDAT"u8))
                {
                    ReadOnlySpan<byte> data = encoded.Slice(offset + 8, length);
                    if (index == 0)
                    {
                        // 文件的首张图像使用 IDAT；它可以是动画首帧，也可以是不参与动画的默认静态图。
                        WritePngChunk(output, "IDAT"u8, data);
                    }
                    else
                    {
                        // 后续动画帧改写为 fdAT：先加入新的 4 字节序号，再追加原始压缩像素数据。
                        byte[] frameData = new byte[length + 4];
                        BinaryPrimitives.WriteUInt32BigEndian(frameData, sequence++);
                        data.CopyTo(frameData.AsSpan(4));
                        WritePngChunk(output, "fdAT"u8, frameData);
                    }
                }

                offset += length + 12; // 跳过长度、类型、有效载荷和 CRC，定位下一个块。
            }

            // 输出当前帧后，再按原始处置规则准备下一帧所需的画布。
            // 独立默认图像不参与动画背景；Background(1) 只清空当前局部矩形，保留画布其他区域。
            if (control == null || control[24] == 1)
            {
                paint.BlendMode = SKBlendMode.Clear;
                canvas.DrawRect(new SKRect(x, y, x + frameWidth, y + frameHeight), paint);
            }
            else if (previous != null)
            {
                // Previous(2) 恢复叠加前的画布；None(0) 不进入任何分支，保留当前画布供下一帧继续使用。
                paint.BlendMode = SKBlendMode.Src;
                canvas.DrawBitmap(previous, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest), paint);
            }
        }

        // 所有图像帧共用一个文件结束块，不能把每张临时 PNG 的 IEND 原样插入动画中间。
        WritePngChunk(output, "IEND"u8, []);
    }

    /// <summary>
    /// 按 PNG 块格式依次写入大端数据长度、块类型、数据内容及重新计算的 CRC-32。
    /// </summary>
    /// <param name="output">从当前位置接收数据块的可写流，由调用方关闭</param>
    /// <param name="type">长度为 4 字节的 ASCII 块类型，例如 IHDR、IDAT 或 IEND</param>
    /// <param name="data">数据块的有效载荷，不包含长度、块类型或 CRC；允许为空</param>
    /// <remarks>
    /// CRC 仅覆盖块类型和有效载荷，不包含长度字段；空数据块仍写入类型及 CRC。
    /// 本方法不写 PNG 文件签名，不校验块类型长度、数据块顺序或 APNG 序列号，调用方须保证这些条件正确。
    /// </remarks>
    /// <exception cref="IOException">写入数据块任一部分失败。</exception>
    private static void WritePngChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        // PNG 块布局：4 字节大端数据长度 + 4 字节类型 + N 字节数据 + 4 字节 CRC。
        Span<byte> value = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(value, data.Length);
        output.Write(value);
        output.Write(type);
        output.Write(data);
        // CRC 初始值为全 1，仅累计类型和数据；长度字段不参与校验。
        uint crc = uint.MaxValue;
        foreach (byte item in type)
        {
            crc = UpdatePngCrc(crc, item);
        }

        foreach (byte item in data)
        {
            crc = UpdatePngCrc(crc, item);
        }

        // 最终校验值取反后按大端序写入；即使是空数据的 IEND，也必须对块类型计算 CRC。
        BinaryPrimitives.WriteUInt32BigEndian(value, ~crc);
        output.Write(value);
    }

    /// <summary>
    /// 使用反射多项式 0xEDB88320，将一个字节并入 PNG 的滚动 CRC-32 校验值。
    /// </summary>
    /// <param name="crc">尚未最终取反的中间校验值，处理首字节前使用 uint.MaxValue 初始化</param>
    /// <param name="value">依次来自块类型或有效载荷的下一个字节</param>
    /// <returns>合入当前字节后、尚未最终取反的 CRC 中间值</returns>
    /// <remarks>调用方应在块类型和全部数据字节处理完毕后按位取反，再以大端格式写入 PNG 的 CRC 字段。</remarks>
    private static uint UpdatePngCrc(uint crc, byte value)
    {
        // PNG 使用从低位开始处理的反射 CRC-32，先将新字节异或到滚动校验值的低 8 位。
        crc ^= value;
        // 逐位右移；移出位为 1 时与反射多项式异或，为 0 时只右移。每个输入字节处理 8 次。
        for (int bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }

        return crc;
    }
}
