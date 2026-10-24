// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using Fast.Center.Domain;
using Fast.Core;
using Fast.Runtime;
using Fast.Shared;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using Yitter.IdGenerator;

namespace Fast.File.Applications;

/// <summary>
/// 上传文件
/// </summary>
public partial class FileApplication
{
    /// <summary>
    /// 上传Logo
    /// </summary>
    [HttpPost]
    [ApiInfo("上传Logo", HttpRequestActionEnum.Upload)]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<string> UploadLogo(IFormFile file)
    {
        return await LocalUploadFile(file, _uploadFileSettingsOptions.Logo);
    }

    /// <summary>
    /// 上传头像
    /// </summary>
    [HttpPost]
    [ApiInfo("上传头像", HttpRequestActionEnum.Upload)]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<string> UploadAvatar(IFormFile file)
    {
        return await LocalUploadFile(file, _uploadFileSettingsOptions.Avatar);
    }

    /// <summary>
    /// 上传证件照
    /// </summary>
    [HttpPost]
    [ApiInfo("上传证件照", HttpRequestActionEnum.Upload)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<string> UploadIdPhoto(IFormFile file)
    {
        return await LocalUploadFile(file, _uploadFileSettingsOptions.IdPhoto);
    }

    /// <summary>
    /// 上传安装包
    /// </summary>
    [HttpPost]
    [ApiInfo("上传安装包", HttpRequestActionEnum.Upload)]
    [PlatformOnly]
    [RequestSizeLimit(513L * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 512L * 1024 * 1024)]
    public async Task<string> UploadPackage(IFormFile file)
    {
        return await LocalUploadFile(file, _uploadFileSettingsOptions.Package);
    }

    /// <summary>
    /// 上传富文本
    /// </summary>
    [HttpPost]
    [ApiInfo("上传富文本", HttpRequestActionEnum.Upload)]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<string> UploadEditor(IFormFile file)
    {
        return await LocalUploadFile(file, _uploadFileSettingsOptions.Editor);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost]
    [ApiInfo("上传文件", HttpRequestActionEnum.Upload)]
    [RequestSizeLimit(101 * 1024 * 1024)]
    public async Task<string> UploadFile(IFormFile file)
    {
        return await LocalUploadFile(file);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns>文件访问地址</returns>
    private async Task<string> LocalUploadFile(IFormFile file, UploadFileInfoSettings fileInfoSettings = null)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("上传文件不能为空！");
        }

        fileInfoSettings ??= _uploadFileSettingsOptions.Default;

        DateTime dateTime = DateTime.Now;

        // 文件大小
        long fileSizeKb = (file.Length + 1023L) / 1024L;
        if (fileInfoSettings.MaxSize > 0 && fileSizeKb > fileInfoSettings.MaxSize)
        {
            throw new UserFriendlyException($"文件大小超出限制，最大允许{fileInfoSettings.MaxSize / 1024}MB。");
        }

        // 浏览器可能提交带 Windows 或 Unix 路径的文件名，存储前只保留最后一段名称
        string fileOriginName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (fileOriginName.Length > 255)
        {
            throw new UserFriendlyException("文件名称不能超过255个字符！");
        }

        string fileSuffix = Path.GetExtension(fileOriginName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(fileSuffix))
        {
            throw new UserFriendlyException("文件没有有效后缀名!");
        }

        if (fileSuffix.Length > 17)
        {
            throw new UserFriendlyException("文件后缀名过长！");
        }

        string normalizedContentType = file.ContentType.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedContentType))
        {
            throw new UserFriendlyException("文件内容类型不能为空！");
        }

        if (fileInfoSettings.ContentType?.Any() == true)
        {
            if (!fileInfoSettings.ContentType.Contains(normalizedContentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException($"文件类型不支持，当前类型：{normalizedContentType}");
            }
        }

        // 根据当前已支持的 MIME 类型校验文件后缀，防止客户端声明的类型与文件名不一致
        bool isFileExtensionCompatible = normalizedContentType switch
        {
            "image/jpg" or "image/jpeg" => fileSuffix is ".jpg" or ".jpeg",
            "image/png" => fileSuffix == ".png",
            "image/gif" => fileSuffix == ".gif",
            "image/bmp" => fileSuffix is ".bmp" or ".dib",
            "video/mp4" => fileSuffix is ".mp4" or ".m4v",
            "video/mpeg" => fileSuffix is ".mpeg" or ".mpg" or ".mpe",
            "video/quicktime" => fileSuffix is ".mov" or ".qt",
            "video/x-msvideo" => fileSuffix == ".avi",
            "video/x-ms-wmv" => fileSuffix == ".wmv",
            "video/webm" => fileSuffix == ".webm",
            "video/ogg" => fileSuffix is ".ogv" or ".ogg",
            "audio/mpeg" => fileSuffix is ".mp3" or ".mpga",
            "audio/wav" => fileSuffix == ".wav",
            "audio/ogg" => fileSuffix is ".ogg" or ".oga" or ".opus",
            "audio/mp4" => fileSuffix is ".m4a" or ".mp4",
            "audio/flac" => fileSuffix == ".flac",
            "text/plain" => fileSuffix == ".txt",
            "text/csv" => fileSuffix == ".csv",
            "text/html" => fileSuffix is ".html" or ".htm",
            "text/markdown" => fileSuffix is ".md" or ".markdown",
            "application/pdf" => fileSuffix == ".pdf",
            "application/msword" => fileSuffix == ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => fileSuffix == ".docx",
            "application/vnd.ms-excel" => fileSuffix == ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => fileSuffix == ".xlsx",
            "application/vnd.ms-powerpoint" => fileSuffix == ".ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => fileSuffix == ".pptx",
            "application/zip" => fileSuffix == ".zip",
            "application/x-rar-compressed" => fileSuffix == ".rar",
            "application/x-7z-compressed" => fileSuffix == ".7z",
            "application/gzip" => fileSuffix is ".gz" or ".gzip" or ".tgz",
            // Android 安装包
            "application/vnd.android.package-archive" => fileSuffix == ".apk",
            // Windows EXE 安装包
            "application/vnd.microsoft.portable-executable" => fileSuffix == ".exe",
            // 通用二进制应用包
            "application/octet-stream" => fileSuffix is ".ipa" or ".wgt" or ".app" or ".hap" or ".exe" or ".msi",
            _ => false
        };
        if (!isFileExtensionCompatible)
        {
            throw new UserFriendlyException("文件后缀与声明的文件类型不一致！");
        }

        if (FileContext.Images.Contains(normalizedContentType))
        {
            try
            {
                await using Stream formatStream = file.OpenReadStream();
                IImageFormat imageFormat = await Image.DetectFormatAsync(formatStream);
                string expectedMimeType = normalizedContentType == "image/jpg" ? "image/jpeg" : normalizedContentType;
                if (imageFormat == null
                    || !string.Equals(imageFormat.DefaultMimeType, expectedMimeType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UserFriendlyException("图片内容与声明的文件类型不一致！");
                }

                await using Stream identifyStream = file.OpenReadStream();
                ImageInfo imageInfo = await Image.IdentifyAsync(identifyStream);
                if (imageInfo == null || (long)imageInfo.Width * imageInfo.Height > MaxImagePixels)
                {
                    throw new UserFriendlyException("图片像素尺寸超出限制！");
                }
            }
            catch (UnknownImageFormatException)
            {
                throw new UserFriendlyException("图片内容与声明的文件类型不一致！");
            }
            catch (InvalidImageContentException)
            {
                throw new UserFriendlyException("图片内容已损坏或格式不受支持！");
            }
        }

        if (FileContext.Packages.Contains(normalizedContentType)
            && fileSuffix is ".apk" or ".app" or ".hap" or ".ipa" or ".wgt" or ".exe" or ".msi")
        {
            const int MaxArchiveEntries = 50000;
            const int MaxMetadataLength = 1024 * 1024;

            string packageName = fileSuffix switch
            {
                ".apk" => "安卓 APK",
                ".app" => "鸿蒙 APP",
                ".hap" => "鸿蒙 HAP",
                ".ipa" => "苹果 IPA",
                ".wgt" => "热更新 WGT",
                ".exe" => "Windows EXE",
                ".msi" => "Windows MSI",
                _ => fileSuffix[1..].ToUpperInvariant()
            };

            // 记录实际解析的配置文件，确保 JSON 异常能够准确说明出错位置。
            string jsonFileName = null;
            CancellationToken cancellationToken = _httpContext.RequestAborted;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using Stream packageStream = file.OpenReadStream();

                if (fileSuffix is ".exe" or ".msi")
                {
                    // EXE 和 MSI 需要按文件偏移读取，不能交给 ZipArchive 校验。
                    // 不支持定位属于文件流实现的问题，不应误报为安装包损坏。
                    if (!packageStream.CanSeek)
                    {
                        throw new NotSupportedException("应用包校验需要支持定位的文件流。");
                    }

                    long packageLength = packageStream.Length;

                    if (packageLength == 0)
                    {
                        throw new UserFriendlyException($"{packageName} 文件为空！");
                    }

                    if (fileSuffix == ".exe")
                    {
                        // PEReader 只解析文件结构，不加载或执行上传的程序。
                        // 不要求存在 .NET 元数据，以兼容原生 EXE 和自解压安装程序。
                        using var peReader = new PEReader(packageStream, PEStreamOptions.LeaveOpen);

                        PEHeaders headers = peReader.PEHeaders;
                        PEHeader peHeader = headers.PEHeader;
                        Characteristics characteristics = headers.CoffHeader.Characteristics;

                        // 必须具有 PE 可选头和可执行标志，同时排除改名为 EXE 的 DLL。
                        if (peHeader is null
                            || (characteristics & Characteristics.ExecutableImage) == 0
                            || (characteristics & Characteristics.Dll) != 0)
                        {
                            throw new UserFriendlyException($"{packageName} 不是有效的 PE 可执行文件！");
                        }

                        // Windows 加载器最多支持 96 个节。
                        if (headers.SectionHeaders.Length is 0 or > 96)
                        {
                            throw new UserFriendlyException($"{packageName} 的 PE 节数量无效！");
                        }

                        if (peHeader.SizeOfHeaders <= 0 || peHeader.SizeOfHeaders > packageLength)
                        {
                            throw new UserFriendlyException($"{packageName} 的 PE 头长度无效！");
                        }

                        // PE 头可以解析，不代表各节的数据完整；继续检查原始数据范围。
                        // 允许最后一个节之后存在附加数据，以兼容自解压安装程序。
                        foreach (SectionHeader section in headers.SectionHeaders)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (section.SizeOfRawData < 0)
                            {
                                throw new UserFriendlyException($"{packageName} 的 PE 节数据长度无效！");
                            }

                            // 未初始化数据节可以不占用文件空间。
                            if (section.SizeOfRawData == 0)
                            {
                                continue;
                            }

                            // 转为 long 后计算，避免恶意偏移或长度导致整数溢出。
                            long sectionEnd = (long)section.PointerToRawData + section.SizeOfRawData;

                            if (section.PointerToRawData < peHeader.SizeOfHeaders || sectionEnd > packageLength)
                            {
                                throw new UserFriendlyException($"{packageName} 的 PE 节数据越界或文件不完整！");
                            }
                        }
                    }
                    else
                    {
                        // MSI 使用 OLE/CFB 复合文件格式，固定头部为 512 字节。
                        // ReadExactlyAsync 会处理短读，数据不足时抛出 EndOfStreamException。
                        byte[] header = new byte[512];
                        await packageStream.ReadExactlyAsync(header.AsMemory(), cancellationToken);

                        byte[] signature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

                        if (!header.AsSpan(0, signature.Length).SequenceEqual(signature))
                        {
                            throw new UserFriendlyException($"{packageName} 缺少有效的 CFB 文件头！");
                        }

                        ushort majorVersion = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(26, 2));
                        ushort byteOrder = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(28, 2));
                        ushort sectorShift = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(30, 2));
                        ushort miniSectorShift = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(32, 2));

                        // CFB V3 使用 512 字节扇区，V4 使用 4096 字节扇区；
                        // 两者均采用小端序，迷你扇区固定为 64 字节。
                        bool isValidSectorLayout = (majorVersion, sectorShift) is (3, 9) or (4, 12);

                        if (!isValidSectorLayout || byteOrder != 0xFFFE || miniSectorShift != 6)
                        {
                            throw new UserFriendlyException($"{packageName} 的 CFB 头部参数无效！");
                        }

                        // 文件头中的 CLSID 是保留字段，应为零；它不是 MSI 的类型标识。
                        if (new Guid(header.AsSpan(8, 16)) != Guid.Empty)
                        {
                            throw new UserFriendlyException($"{packageName} 的 CFB 头部保留字段无效！");
                        }

                        int sectorSize = 1 << sectorShift;
                        uint directorySector = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(48, 4));

                        // 文件头占据一个扇区，扇区编号从文件头之后开始计算。
                        // 定位前验证整个目录扇区都在文件范围内。
                        long directoryOffset = ((long)directorySector + 1) * sectorSize;

                        if (directorySector >= 0xFFFFFFFA || directoryOffset > packageLength - sectorSize)
                        {
                            throw new UserFriendlyException($"{packageName} 的 CFB 目录位置无效或文件不完整！");
                        }

                        packageStream.Seek(directoryOffset, SeekOrigin.Begin);

                        // 第一个目录扇区的第一个目录项固定为根存储，目录项长度为 128 字节。
                        byte[] rootEntry = new byte[128];
                        await packageStream.ReadExactlyAsync(rootEntry.AsMemory(), cancellationToken);

                        if (rootEntry[66] != 0x05)
                        {
                            throw new UserFriendlyException($"{packageName} 缺少有效的 CFB 根存储！");
                        }

                        // Word、Excel 等旧格式也使用 CFB，不能仅检查文件头签名。
                        // 根存储的 CLSID 用于区分 MSI 数据库与其他复合文件。
                        var rootClassId = new Guid(rootEntry.AsSpan(80, 16));
                        var msiClassId = new Guid("000C1084-0000-0000-C000-000000000046");

                        if (rootClassId != msiClassId)
                        {
                            throw new UserFriendlyException($"{packageName} 不是 MSI 安装数据库！");
                        }
                    }
                }
                else
                {
                    // APK、APP、HAP、IPA、WGT 均按 ZIP 归档读取。
                    // LeaveOpen 将底层文件流的释放交给外层 await using。
                    await using var archive = new ZipArchive(packageStream, ZipArchiveMode.Read, true);

                    if (archive.Entries.Count == 0)
                    {
                        throw new UserFriendlyException($"{packageName} 包为空！");
                    }

                    if (archive.Entries.Count > MaxArchiveEntries)
                    {
                        throw new UserFriendlyException($"{packageName} 包内条目数量不能超过 {MaxArchiveEntries}！");
                    }

                    // 在规范化路径分隔符后检查绝对路径、父目录跳转和重复条目。
                    // 此处只读取归档，不向服务器文件系统解压。
                    var names = new HashSet<string>(StringComparer.Ordinal);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string name = entry.FullName.Replace('\\', '/');

                        if (string.IsNullOrEmpty(name)
                            || name.StartsWith('/')
                            || name.Contains(':')
                            || name.Split('/').Contains("..")
                            || !names.Add(name))
                        {
                            throw new UserFriendlyException($"{packageName} 包包含无效路径或重复条目！");
                        }
                    }

                    if (fileSuffix == ".apk")
                    {
                        // APK 的 AndroidManifest.xml 通常为二进制 XML，
                        // 此处仅检查根目录文件是否存在且非空，不按文本 XML 解析。
                        if (archive.GetEntry("AndroidManifest.xml") is not {Length: > 0})
                        {
                            throw new UserFriendlyException($"{packageName} 缺少有效的 AndroidManifest.xml！");
                        }
                    }
                    else if (fileSuffix == ".app")
                    {
                        // 鸿蒙 APP 是发布包，包含 pack.info 和至少一个 HAP，
                        // 同时允许包含 HSP；此处不递归解析内部应用模块。
                        jsonFileName = "pack.info";

                        if (!archive.Entries.Any(entry =>
                                entry.Length > 0 && entry.FullName.EndsWith(".hap", StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new UserFriendlyException($"{packageName} 中未包含 HAP 应用包！");
                        }
                    }
                    else if (fileSuffix == ".hap")
                    {
                        // Stage 模型使用 module.json，FA 模型使用 config.json。
                        // module.json 存在但损坏时直接拒绝，不回退到 config.json。
                        if (archive.GetEntry("module.json") is not null)
                        {
                            jsonFileName = "module.json";
                        }
                        else if (archive.GetEntry("config.json") is not null)
                        {
                            jsonFileName = "config.json";
                        }
                        else
                        {
                            throw new UserFriendlyException($"{packageName} 缺少 module.json 或 config.json！");
                        }
                    }
                    else if (fileSuffix == ".ipa")
                    {
                        // 主应用配置必须位于 Payload/<应用名>.app/Info.plist。
                        // 限定路径层级，避免将插件或扩展中的 Info.plist 误认为主应用配置。
                        // plist 可以是 XML 或二进制格式，此处只检查存在性和非空。
                        if (!archive.Entries.Any(entry =>
                                entry.Length > 0
                                && entry.FullName.StartsWith("Payload/", StringComparison.Ordinal)
                                && entry.FullName.EndsWith(".app/Info.plist", StringComparison.Ordinal)
                                && entry.FullName.Split('/').Length == 3))
                        {
                            throw new UserFriendlyException($"{packageName} 缺少主应用的 Info.plist！");
                        }
                    }
                    else if (fileSuffix == ".wgt")
                    {
                        jsonFileName = "manifest.json";
                    }

                    // APP、HAP 和 WGT 的配置均为 JSON，共用长度与格式校验。
                    if (jsonFileName is not null)
                    {
                        ZipArchiveEntry metadata = archive.GetEntry(jsonFileName);

                        if (metadata is not {Length: > 0})
                        {
                            throw new UserFriendlyException($"{packageName} 缺少有效的 {jsonFileName}！");
                        }

                        // 使用解压后的条目长度限制配置大小，避免解析过大的配置文件。
                        if (metadata.Length > MaxMetadataLength)
                        {
                            throw new UserFriendlyException($"{packageName} 的 {jsonFileName} 大小不能超过 1 MB！");
                        }

                        await using Stream metadataStream = await metadata.OpenAsync(cancellationToken);

                        using JsonDocument document = await JsonDocument.ParseAsync(
                            metadataStream, cancellationToken: cancellationToken);

                        // JSON 语法正确并不代表配置结构正确，根节点还必须是对象。
                        if (document.RootElement.ValueKind != JsonValueKind.Object)
                        {
                            throw new UserFriendlyException($"{packageName} 的 {jsonFileName} 根节点必须是 JSON 对象！");
                        }
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (JsonException exception)
            {
                // 使用实际包类型和配置文件名，避免将鸿蒙配置错误提示成 WGT 错误。
                throw new UserFriendlyException($"{packageName} 的 {jsonFileName} 不是有效的 JSON 文件！", exception);
            }
            catch (BadImageFormatException exception) when (fileSuffix == ".exe")
            {
                // PEReader 无法解析 PE 结构时抛出此异常。
                throw new UserFriendlyException($"{packageName} 的 PE 文件结构无效或已损坏！", exception);
            }
            catch (InvalidDataException exception) when (fileSuffix is ".apk" or ".app" or ".hap" or ".ipa" or ".wgt")
            {
                // ZIP 目录解析或条目解压失败，不仅限于文件头格式错误。
                throw new UserFriendlyException($"{packageName} 不是有效的 ZIP 归档，或包内压缩数据已损坏！", exception);
            }
            catch (EndOfStreamException exception)
            {
                // 读取必要结构时提前结束，通常表示文件被截断。
                throw new UserFriendlyException($"{packageName} 文件不完整，无法读取必要的结构信息！", exception);
            }
        }

        // 计算文件哈希
        await using Stream stream = file.OpenReadStream();
        byte[] hashBytes = await SHA256.HashDataAsync(stream, _httpContext.RequestAborted);
        string fileHash = Convert.ToHexStringLower(hashBytes);

        // 判断是否存在重复文件
        FileModel existFileModel = await _repository.SingleOrDefaultAsync(s => s.FileHash == fileHash);
        if (existFileModel != null)
        {
            if (!System.IO.File.Exists(
                    FileContext.GetLocalPath(_rootPath, existFileModel.FilePath, existFileModel.FileObjectName)))
            {
                throw new UserFriendlyException("相同文件的存储记录存在，但物理文件已丢失，请联系管理员处理！");
            }

            return existFileModel.FileLocation;
        }

        long fileId = YitIdHelper.NextId();
        // 本地文件名称
        string fileObjectName = $"{fileId}{fileSuffix}";

        // 本地文件路径
        string filePath = fileInfoSettings.Path;

        if (!string.IsNullOrWhiteSpace(_user?.TenantNo))
        {
            filePath = Path.Combine(filePath, _user.TenantNo);
        }

        // 判断是否启用类型文件夹
        if (fileInfoSettings.UseTypeFolder)
        {
            if (FileContext.Images.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "image");
            }
            else if (FileContext.Videos.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "video");
            }
            else if (FileContext.Audios.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "audio");
            }
            else if (FileContext.Texts.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "text");
            }
            else if (FileContext.Documents.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "document");
            }
            else if (FileContext.Archives.Contains(normalizedContentType))
            {
                filePath = Path.Combine(filePath, "archive");
            }
            else if (FileContext.Packages.Contains(normalizedContentType))
            {
                string _fileSuffix = fileSuffix.TrimStart('.');
                filePath = Path.Combine(filePath, _fileSuffix is "exe" or "msi" ? "msi" : _fileSuffix);
            }
            else
            {
                filePath = Path.Combine(filePath, "other");
            }
        }

        // 默认使用年份和月份拆分文件夹
        filePath = Path.Combine(filePath, dateTime.ToString("yyyy/MM"));

        // 数据库存储统一使用“/”，保证路径记录可以在 Windows、Linux 和 macOS 之间迁移
        filePath = filePath.Replace('\\', '/');

        // 获取文件访问地址
        string publicDomain = _uploadFileSettingsOptions.PublicDomain;
        if (string.IsNullOrWhiteSpace(publicDomain))
        {
            publicDomain = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}";
        }

        var fileInfoModel = new FileModel
        {
            FileId = fileId,
            FileObjectName = fileObjectName,
            FileOriginName = fileOriginName,
            FileSuffix = fileSuffix.TrimStart('.'),
            FileMimeType = normalizedContentType,
            FileSizeKb = fileSizeKb,
            FilePath = filePath,
            FileLocation = string.IsNullOrWhiteSpace(fileInfoSettings.BaseUrl)
                ? $"{publicDomain}/file/{fileObjectName}"
                : $"{publicDomain}/file{fileInfoSettings.BaseUrl}/{fileId}",
            FileHash = fileHash
        };
        // 获取设备信息
        UserAgentInfo userAgentInfo = _httpContext.RequestUserAgentInfo();
        // 获取万网信息
        WanNetIPInfo wanNetIpInfo = await _httpContext.RemoteIpv4InfoAsync();
        fileInfoModel.UploadDevice = userAgentInfo.Device;
        fileInfoModel.UploadOS = userAgentInfo.OS;
        fileInfoModel.UploadBrowser = userAgentInfo.Browser;
        fileInfoModel.UploadProvince = wanNetIpInfo.Province;
        fileInfoModel.UploadCity = wanNetIpInfo.City;
        fileInfoModel.UploadIp = wanNetIpInfo.Ip;
        fileInfoModel.CreatedUserId = _user?.EmployeeId;
        fileInfoModel.CreatedUserName = _user?.NickName;
        fileInfoModel.CreatedTime = dateTime;

        // 本地存储
        string localFilePath = FileContext.GetLocalPath(_rootPath, filePath);
        Directory.CreateDirectory(localFilePath);
        string localFullPath = Path.Combine(localFilePath, fileObjectName);
        await using (var fileStream = new FileStream(localFullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await file.CopyToAsync(fileStream, _httpContext.RequestAborted);
        }

        // 判断是否为图片
        if (FileContext.Images.Contains(normalizedContentType))
        {
            // 异步读取原始图片
            using Image image = await Image.LoadAsync(localFullPath);

            foreach (KeyValuePair<string, int> item in ImageSizes)
            {
                int width = Math.Min(item.Value, image.Width);
                // 按原图比例计算高度
                float ratio = (float)width / image.Width;
                int height = Math.Max(1, (int)(image.Height * ratio));

                // 创建图片副本并调整大小
                using Image clone = image.Clone(ctx => ctx.Resize(width, height));
                // 拼接缩略图文件名
                string thumbnailName = $"{fileId}@{item.Key}{fileSuffix}";
                string thumbnailPath = Path.Combine(localFilePath, thumbnailName);

                // 保存图片到本地，格式自动根据后缀判断
                await clone.SaveAsync(thumbnailPath);
            }
        }

        // 物理文件及缩略图全部写入成功后再保存记录，失败时只会产生冗余文件，不会产生无效数据库记录
        await _repository.InsertAsync(fileInfoModel);

        return fileInfoModel.FileLocation;
    }
}
