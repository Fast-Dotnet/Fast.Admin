// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// Excel导入结果
/// </summary>
/// <typeparam name="T">导入数据类型</typeparam>
[SuppressSniffer]
public class ExcelImportResult<T> where T : class, new()
{
    /// <summary>
    /// 导入数据
    /// </summary>
    public List<T> Data { get; set; } = [];

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<ExcelImportError> Errors { get; set; } = [];

    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError => Errors?.Count > 0;

    /// <inheritdoc />
    public override string ToString()
    {
        if (Errors == null || Errors.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine,
            Errors.OrderBy(e => e.RowIndex).Select(e => $"第 {e.RowIndex} 行，{e.ErrorMessage}"));
    }

    /// <summary>
    /// 检查是否存在错误，如果有则抛出异常
    /// </summary>
    public void ThrowIfError()
    {
        if (!HasError)
        {
            return;
        }

        throw new UserFriendlyException(ToString());
    }
}
