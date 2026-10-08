// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// Excel导入错误信息
/// </summary>
[SuppressSniffer]
public class ExcelImportError
{
    /// <summary>
    /// 行号
    /// </summary>
    /// <remarks>从 1 开始统计读取到的数据行，不含表头，也不是工作表中的绝对行号。</remarks>
    public int RowIndex { get; set; }

    /// <summary>
    /// 列名称
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 单元格值
    /// </summary>
    public object CellValue { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }
}
