// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Security.Cryptography;
using System.Text;
using Fast.Center.Domain;
using Fast.SqlSugar;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace Fast.Core;

/// <summary>
/// 文件上下文
/// </summary>
[SuppressSniffer]
public class FileContext
{
    private const int MediaAssetTokenNonceLength = 12;
    private const int MediaAssetTokenTagLength = 16;
    private const string MediaAssetTokenEncryptionPurpose = "Fast:MediaAssetToken";

    /// <summary>
    /// 图片
    /// </summary>
    public static readonly HashSet<string> Images = ["image/jpg", "image/jpeg", "image/png", "image/gif", "image/bmp"];

    /// <summary>
    /// 视频
    /// </summary>
    public static readonly HashSet<string> Videos =
        ["video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/x-ms-wmv", "video/webm", "video/ogg"];

    /// <summary>
    /// 音频
    /// </summary>
    public static readonly HashSet<string> Audios = ["audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4", "audio/flac"];

    /// <summary>
    /// 文本
    /// </summary>
    public static readonly HashSet<string> Texts =
    [
        "text/plain",
        "text/csv",
        "text/html",
        "text/markdown"
    ];

    /// <summary>
    /// 文档
    /// </summary>
    public static readonly HashSet<string> Documents =
    [
        // PDF
        "application/pdf",
        // Word
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        // Excel
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        // PowerPoint
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    ];

    /// <summary>
    /// 压缩包
    /// </summary>
    public static readonly HashSet<string> Archives =
        ["application/zip", "application/x-rar-compressed", "application/x-7z-compressed", "application/gzip"];

    /// <summary>
    /// 将配置或数据库中的跨平台路径转换为当前操作系统使用的本地路径
    /// </summary>
    /// <param name="rootPath">应用根目录</param>
    /// <param name="filePath">配置或数据库中的相对目录，兼容“/”和“\”</param>
    /// <param name="fileName">可选文件名</param>
    /// <returns>绝对配置保持原位置；相对配置基于程序根目录解析</returns>
    public static string GetLocalPath(string rootPath, string filePath, string fileName = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new UserFriendlyException("文件存储路径不能为空！");
        }

        rootPath = Path.GetFullPath(rootPath);
        string localPath = filePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        string fullPath = string.IsNullOrEmpty(fileName)
            ? Path.GetFullPath(Path.Combine(rootPath, localPath))
            : Path.GetFullPath(Path.Combine(rootPath, localPath, fileName));

        return fullPath;
    }

    /// <summary>
    /// 创建媒体资源临时访问 Token
    /// </summary>
    /// <param name="fileUrl">媒体文件地址</param>
    /// <param name="lifetimeMinutes">Token 有效期，单位：分钟，限制为 15～120 分钟</param>
    /// <returns>媒体资源临时访问地址</returns>
    public static async Task<string> CreateMediaAssetTicket(string fileUrl, double lifetimeMinutes)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new UserFriendlyException("文件地址不能为空！");
        }

        if (!double.IsFinite(lifetimeMinutes))
        {
            throw new UserFriendlyException("Token有效期不正确！");
        }

        // 限制媒体 Token 有效期，避免调用方传入异常值导致 Token 长期有效
        lifetimeMinutes = Math.Clamp(lifetimeMinutes, 15L, 120L);

        // 提取文件路径，仅支持完整的地址
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out Uri uri))
        {
            throw new UserFriendlyException("文件地址格式不正确！");
        }

        // 获取文件Id
        string fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath.TrimEnd('/'));
        if (!long.TryParse(fileName, out long fileId))
        {
            throw new UserFriendlyException("文件地址不受支持！");
        }

        ISqlSugarRepository<FileModel> repository = FastContext.GetService<ISqlSugarRepository<FileModel>>();
        bool fileExists = await repository.Entities.AnyAsync(wh =>
            wh.FileId == fileId && (wh.FileMimeType.StartsWith("audio/") || wh.FileMimeType.StartsWith("video/")));
        if (!fileExists)
        {
            throw new UserFriendlyException("文件不存在或存储地址不受支持！");
        }

        IUser _user = FastContext.GetService<IUser>();
        var payload = new MediaAssetTokenPayload
        {
            FileId = fileId,
            AppNo = _user.AppNo,
            TenantNo = _user.TenantNo,
            DeviceType = _user.DeviceType,
            EmployeeNo = _user.EmployeeNo,
            SessionId = _user.SessionId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes).ToUnixTimeSeconds()
        };
        string token = EncryptMediaAssetToken(payload);

        return $"{uri.GetLeftPart(UriPartial.Authority)}/file/media/{token}";
    }

    /// <summary>
    /// 验证并解析媒体资源访问 Token
    /// </summary>
    /// <param name="token">媒体资源访问 Token</param>
    /// <param name="payload">验证成功后的 Token 载荷</param>
    /// <returns>Token 加密认证、格式和有效期均有效时返回 <c>true</c></returns>
    public static bool TryValidateMediaAssetToken(string token, out MediaAssetTokenPayload payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(token) || token.Length > 2048)
        {
            return false;
        }

        try
        {
            byte[] tokenBytes = WebEncoders.Base64UrlDecode(token);
            if (tokenBytes.Length < MediaAssetTokenNonceLength + MediaAssetTokenTagLength)
            {
                return false;
            }

            int ciphertextLength = tokenBytes.Length - MediaAssetTokenNonceLength - MediaAssetTokenTagLength;
            Span<byte> nonce = tokenBytes.AsSpan(0, MediaAssetTokenNonceLength);
            Span<byte> ciphertext = tokenBytes.AsSpan(MediaAssetTokenNonceLength, ciphertextLength);
            Span<byte> tag = tokenBytes.AsSpan(tokenBytes.Length - MediaAssetTokenTagLength, MediaAssetTokenTagLength);
            byte[] plaintext = new byte[ciphertextLength];
            byte[] encryptionKey = GetMediaAssetEncryptionKey();
            try
            {
                using var aesGcm = new AesGcm(encryptionKey, MediaAssetTokenTagLength);
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                return TryDeserializeMediaAssetTokenPayload(plaintext, out payload);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
        catch (FormatException)
        {
            payload = null;
            return false;
        }
        catch (CryptographicException)
        {
            payload = null;
            return false;
        }
        catch (IOException)
        {
            payload = null;
            return false;
        }
    }

    /// <summary>
    /// 加密媒体资源访问 Token
    /// </summary>
    private static string EncryptMediaAssetToken(MediaAssetTokenPayload payload)
    {
        byte[] plaintext = SerializeMediaAssetTokenPayload(payload);
        byte[] encryptionKey = GetMediaAssetEncryptionKey();
        byte[] nonce = RandomNumberGenerator.GetBytes(MediaAssetTokenNonceLength);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[MediaAssetTokenTagLength];
        try
        {
            using var aesGcm = new AesGcm(encryptionKey, MediaAssetTokenTagLength);
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(encryptionKey);
            CryptographicOperations.ZeroMemory(plaintext);
        }

        // 格式：[12 字节随机 Nonce][密文][16 字节认证标签]
        byte[] tokenBytes = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, tokenBytes, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, tokenBytes, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, tokenBytes, nonce.Length + ciphertext.Length, tag.Length);
        return WebEncoders.Base64UrlEncode(tokenBytes);
    }

    /// <summary>
    /// 序列化媒体资源访问 Token 载荷
    /// </summary>
    private static byte[] SerializeMediaAssetTokenPayload(MediaAssetTokenPayload payload)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(payload.FileId);
        writer.Write(payload.ExpiresAt);
        writer.Write((long)payload.DeviceType);
        writer.Write(payload.AppNo);
        writer.Write(payload.TenantNo);
        writer.Write(payload.EmployeeNo);
        writer.Write(payload.SessionId);
        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>
    /// 反序列化并验证媒体资源访问 Token 载荷
    /// </summary>
    private static bool TryDeserializeMediaAssetTokenPayload(byte[] plaintext, out MediaAssetTokenPayload payload)
    {
        using var stream = new MemoryStream(plaintext, false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        payload = new MediaAssetTokenPayload
        {
            FileId = reader.ReadInt64(),
            ExpiresAt = reader.ReadInt64(),
            DeviceType = (AppEnvironmentEnum)reader.ReadInt64(),
            AppNo = reader.ReadString(),
            TenantNo = reader.ReadString(),
            EmployeeNo = reader.ReadString(),
            SessionId = reader.ReadString()
        };

        if (stream.Position != stream.Length
            || payload.FileId <= 0
            || string.IsNullOrWhiteSpace(payload.AppNo)
            || string.IsNullOrWhiteSpace(payload.TenantNo)
            || string.IsNullOrWhiteSpace(payload.EmployeeNo)
            || string.IsNullOrWhiteSpace(payload.SessionId)
            || payload.ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            payload = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取媒体资源访问 Token 加密密钥
    /// </summary>
    private static byte[] GetMediaAssetEncryptionKey()
    {
        IConfiguration configuration = FastContext.GetService<IConfiguration>();
        string issuerSigningKey = configuration["JWTSettings:IssuerSigningKey"];
        if (string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            throw new InvalidOperationException("JWT签名密钥未配置，无法签发或验证媒体资源访问 Token。");
        }

        return Convert.FromHexString(CryptoUtil.HMACSHA256Encrypt(MediaAssetTokenEncryptionPurpose, issuerSigningKey));
    }
}
