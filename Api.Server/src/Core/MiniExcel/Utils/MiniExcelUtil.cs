// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Fast.Core;

/// <summary>
/// Excel工具类
/// </summary>
/// <remarks>基于 <see cref="MiniExcel"/> 封装的 Excel 导入导出工具，支持Dto特性驱动</remarks>
[SuppressSniffer]
public static class MiniExcelUtil
{
    /// <summary>
    /// 属性信息缓存
    /// </summary>
    /// <remarks>按Dto类型缓存属性元信息（特性、类型标记等），避免每次导入导出都重复反射解析</remarks>
    private static readonly ConcurrentDictionary<Type, List<ExcelPropertyInfo>> _propertyInfoCache = new();

    /// <summary>
    /// 枚举映射缓存
    /// </summary>
    /// <remarks>按枚举类型缓存 值↔描述 的双向映射，避免每次循环都重复遍历枚举字段和 Description 特性</remarks>
    private static readonly ConcurrentDictionary<Type, ExcelEnumMapping> _enumMappingCache = new();

    #region 导出

    /// <summary>
    /// 导出 Excel 数据到内存流
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <returns>导出内容的内存流</returns>
    public static MemoryStream ExportExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX) where T : class, new()
    {
        // 从缓存获取属性元信息（含列名、排序、类型标记等）
        List<ExcelPropertyInfo> propertyInfos = GetPropertyInfos<T>();

        // 根据属性元信息将Dto数据转换为 Dictionary 列表（适配 MiniExcel 的动态列导出）
        List<Dictionary<string, object>> exportData = ConvertExportData(data, propertyInfos);

        // 构建 MiniExcel 的动态列配置（列名、宽度、格式等）
        var config = new OpenXmlConfiguration();
        List<DynamicExcelColumn> dynamicColumns = BuildDynamicColumns(propertyInfos);
        if (dynamicColumns.Count > 0)
        {
            config.DynamicColumns = dynamicColumns.ToArray();
        }

        // 当数据为空时，使用 DataTable 确保仍然导出表头（MiniExcel 对空字典列表不会生成表头）
        object dataToExport = GetExportDataObject(exportData, propertyInfos);

        // 使用 MiniExcel 写入 Excel 到流
        var memoryStream = new MemoryStream();
        memoryStream.SaveAs(dataToExport, sheetName: sheetName, excelType: excelType, configuration: config);

        // 重置流位置到开头，以便调用方可以直接读取
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <summary>
    /// 导出 Excel 数据到内存流
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="sheetName">Sheet名称</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导出内容的内存流</returns>
    public static async Task<MemoryStream> ExportExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX, CancellationToken cancellationToken = default) where T : class, new()
    {
        // 从缓存获取属性元信息（含列名、排序、类型标记等）
        List<ExcelPropertyInfo> propertyInfos = GetPropertyInfos<T>();

        // 根据属性元信息将Dto数据转换为 Dictionary 列表（适配 MiniExcel 的动态列导出）
        List<Dictionary<string, object>> exportData = ConvertExportData(data, propertyInfos);

        // 构建 MiniExcel 的动态列配置（列名、宽度、格式等）
        var config = new OpenXmlConfiguration();
        List<DynamicExcelColumn> dynamicColumns = BuildDynamicColumns(propertyInfos);
        if (dynamicColumns.Count > 0)
        {
            config.DynamicColumns = dynamicColumns.ToArray();
        }

        // 当数据为空时，使用 DataTable 确保仍然导出表头（MiniExcel 对空字典列表不会生成表头）
        object dataToExport = GetExportDataObject(exportData, propertyInfos);

        // 使用 MiniExcel 写入 Excel 到流
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(dataToExport, sheetName: sheetName, excelType: excelType, configuration: config,
            cancellationToken: cancellationToken);

        // 重置流位置到开头，以便调用方可以直接读取
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <summary>
    /// 导出 Excel 文件并返回下载流
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <returns>导出文件响应</returns>
    public static FileStreamResult ExportExcelResult<T>(IEnumerable<T> data, string fileName, string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX) where T : class, new()
    {
        MemoryStream memoryStream = ExportExcel(data, sheetName, excelType);
        return new FileStreamResult(memoryStream, "application/octet-stream") {FileDownloadName = fileName};
    }

    /// <summary>
    /// 导出 Excel 文件并返回下载流
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="sheetName">Sheet名称</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导出文件响应</returns>
    public static async Task<FileStreamResult> ExportExcelResultAsync<T>(IEnumerable<T> data, string fileName,
        string sheetName = "Sheet1", ExcelType excelType = ExcelType.XLSX, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        MemoryStream memoryStream = await ExportExcelAsync(data, sheetName, excelType, cancellationToken);
        return new FileStreamResult(memoryStream, "application/octet-stream") {FileDownloadName = fileName};
    }

    /// <summary>
    /// 导出 Excel 数据到文件
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    public static void ExportToFile<T>(string filePath, IEnumerable<T> data, string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX) where T : class, new()
    {
        // 从缓存获取属性元信息
        List<ExcelPropertyInfo> propertyInfos = GetPropertyInfos<T>();

        // 转换数据为 MiniExcel 可写入的字典格式
        List<Dictionary<string, object>> exportData = ConvertExportData(data, propertyInfos);

        // 构建 MiniExcel 的动态列配置（列名、宽度、格式等）
        var config = new OpenXmlConfiguration();
        List<DynamicExcelColumn> dynamicColumns = BuildDynamicColumns(propertyInfos);
        if (dynamicColumns.Count > 0)
        {
            config.DynamicColumns = dynamicColumns.ToArray();
        }

        // 当数据为空时，使用 DataTable 确保仍然导出表头（MiniExcel 对空字典列表不会生成表头）
        object dataToExport = GetExportDataObject(exportData, propertyInfos);

        // 使用 MiniExcel 写入文件
        MiniExcel.SaveAs(filePath, dataToExport, sheetName: sheetName, excelType: excelType, configuration: config);
    }

    /// <summary>
    /// 导出 Excel 数据到文件
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">数据集合</param>
    /// <param name="sheetName">Sheet名称</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    public static async Task ExportToFileAsync<T>(string filePath, IEnumerable<T> data, string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX, CancellationToken cancellationToken = default) where T : class, new()
    {
        // 从缓存获取属性元信息
        List<ExcelPropertyInfo> propertyInfos = GetPropertyInfos<T>();

        // 转换数据为 MiniExcel 可写入的字典格式
        List<Dictionary<string, object>> exportData = ConvertExportData(data, propertyInfos);

        // 构建 MiniExcel 的动态列配置（列名、宽度、格式等）
        var config = new OpenXmlConfiguration();
        List<DynamicExcelColumn> dynamicColumns = BuildDynamicColumns(propertyInfos);
        if (dynamicColumns.Count > 0)
        {
            config.DynamicColumns = dynamicColumns.ToArray();
        }

        // 当数据为空时，使用 DataTable 确保仍然导出表头（MiniExcel 对空字典列表不会生成表头）
        object dataToExport = GetExportDataObject(exportData, propertyInfos);

        // 使用 MiniExcel 写入文件
        await MiniExcel.SaveAsAsync(filePath, dataToExport, sheetName: sheetName, excelType: excelType, configuration: config,
            cancellationToken: cancellationToken);
    }

    #endregion

    #region 导入

    /// <summary>
    /// 从流导入 Excel 数据
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="stream">Excel文件流</param>
    /// <param name="sheetName">Sheet名称，为空时读取第一个Sheet</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="startCell">起始单元格，如 "A1"、"B2"，默认从 A1 开始读取</param>
    /// <returns>导入结果</returns>
    public static ExcelImportResult<T> ImportExcel<T>(Stream stream, string sheetName = null,
        ExcelType excelType = ExcelType.XLSX, string startCell = "A1") where T : class, new()
    {
        // 使用 MiniExcel 读取 Excel 数据（启用 useHeaderRow 以列头名称作为 Key）
        var rows = stream.Query(sheetName: sheetName, useHeaderRow: true, excelType: excelType, startCell: startCell)
            .Cast<IDictionary<string, object>>()
            .ToList();

        // 将原始行数据解析为强类型Dto列表，并进行验证
        return ParseImportData<T>(rows);
    }

    /// <summary>
    /// 从流导入 Excel 数据
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="stream">Excel文件流</param>
    /// <param name="sheetName">Sheet名称，为空时读取第一个Sheet</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="startCell">起始单元格，如 "A1"、"B2"，默认从 A1 开始读取</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导入结果</returns>
    public static async Task<ExcelImportResult<T>> ImportExcelAsync<T>(Stream stream, string sheetName = null,
        ExcelType excelType = ExcelType.XLSX, string startCell = "A1", CancellationToken cancellationToken = default)
        where T : class, new()
    {
        // 使用 MiniExcel 读取 Excel 数据（启用 useHeaderRow 以列头名称作为 Key）
        IEnumerable<dynamic> rows =
            await stream.QueryAsync(true, sheetName, excelType, startCell, cancellationToken: cancellationToken);

        // 将动态行数据转换为字典列表
        var rowList = rows.Cast<IDictionary<string, object>>().ToList();

        // 将原始行数据解析为强类型Dto列表，并进行验证
        return ParseImportData<T>(rowList);
    }

    /// <summary>
    /// 从文件导入 Excel 数据
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="sheetName">Sheet名称，为空时读取第一个Sheet</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="startCell">起始单元格，如 "A1"、"B2"，默认从 A1 开始读取</param>
    /// <returns>导入结果</returns>
    public static ExcelImportResult<T> ImportExcel<T>(string filePath, string sheetName = null,
        ExcelType excelType = ExcelType.XLSX, string startCell = "A1") where T : class, new()
    {
        // 使用 MiniExcel 从文件读取数据
        var rows = MiniExcel.Query(filePath, sheetName: sheetName, useHeaderRow: true, excelType: excelType, startCell: startCell)
            .Cast<IDictionary<string, object>>()
            .ToList();

        // 将原始行数据解析为强类型Dto列表，并进行验证
        return ParseImportData<T>(rows);
    }

    /// <summary>
    /// 从文件导入 Excel 数据
    /// </summary>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="sheetName">Sheet名称，为空时读取第一个Sheet</param>
    /// <param name="excelType">Excel类型</param>
    /// <param name="startCell">起始单元格，如 "A1"、"B2"，默认从 A1 开始读取</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导入结果</returns>
    public static async Task<ExcelImportResult<T>> ImportExcelAsync<T>(string filePath, string sheetName = null,
        ExcelType excelType = ExcelType.XLSX, string startCell = "A1", CancellationToken cancellationToken = default)
        where T : class, new()
    {
        // 使用 MiniExcel 异步从文件读取数据
        IEnumerable<dynamic> rows = await MiniExcel.QueryAsync(filePath, true, sheetName, excelType, startCell,
            cancellationToken: cancellationToken);

        // 将动态行数据转换为字典列表
        var rowList = rows.Cast<IDictionary<string, object>>().ToList();

        // 将原始行数据解析为强类型Dto列表，并进行验证
        return ParseImportData<T>(rowList);
    }

    #endregion

    #region 属性元信息解析与缓存

    /// <summary>
    /// 获取Dto类型的属性元信息列表（带缓存）
    /// </summary>
    /// <remarks>
    /// 首次调用时通过反射解析属性特性和类型信息，后续调用直接从缓存读取
    /// 缓存内容包括：列名、排序、类型标记（<see cref="ExcelPropertyInfo.IsBool"/>、<see cref="ExcelPropertyInfo.IsEnum"/>、<see cref="ExcelPropertyInfo.IsDateTime"/> 等）、验证规则、预编译正则等
    /// </remarks>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <returns>Dto类型的属性元信息列表（带缓存）</returns>
    private static List<ExcelPropertyInfo> GetPropertyInfos<T>() where T : class, new()
    {
        return _propertyInfoCache.GetOrAdd(typeof(T), type =>
        {
            // 获取所有属性
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<ExcelPropertyInfo>();

            foreach (PropertyInfo prop in properties)
            {
                ExcelColumnAttribute columnAttr = prop.GetCustomAttribute<ExcelColumnAttribute>();

                // 跳过没有标记 [ExcelColumn] 特性的属性（未标记的属性默认忽略，不参与导入导出）
                if (columnAttr == null)
                {
                    continue;
                }

                // 跳过标记了 Ignore = true 的属性
                if (columnAttr.Ignore)
                {
                    continue;
                }

                // 解析属性类型信息
                Type propertyType = prop.PropertyType;
                Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                // 构建属性元信息，一次性计算所有类型标记
                var info = new ExcelPropertyInfo
                {
                    // 基础信息
                    Property = prop,
                    ColumnAttribute = columnAttr,
                    ColumnName = columnAttr?.Name ?? prop.Name,
                    Order = columnAttr?.Order ?? int.MaxValue,

                    // 验证特性
                    RequiredAttribute = prop.GetCustomAttribute<ExcelRequiredAttribute>(),
                    RegexAttributes = prop.GetCustomAttributes<ExcelRegexAttribute>().ToList(),

                    // 缓存的类型信息（避免每行数据都重复判断类型）
                    PropertyType = propertyType,
                    UnderlyingType = underlyingType,
                    IsNullable = Nullable.GetUnderlyingType(propertyType) != null,
                    IsBool = underlyingType == typeof(bool),
                    IsEnum = underlyingType.IsEnum,
                    IsDateTime = underlyingType == typeof(DateTime),
                    IsDateTimeOffset = underlyingType == typeof(DateTimeOffset),
                    IsGuid = underlyingType == typeof(Guid),
                    IsValueTypeCollection = CheckIsValueTypeCollection(propertyType),
                    IsComplexCollection = CheckIsComplexCollection(propertyType)
                };

                // 如果是值类型集合，缓存元素类型（避免每次 GetGenericArguments / GetElementType）
                if (info.IsValueTypeCollection)
                {
                    if (propertyType.IsArray)
                    {
                        info.CollectionElementType = propertyType.GetElementType();
                    }
                    else if (propertyType.IsGenericType)
                    {
                        info.CollectionElementType = propertyType.GetGenericArguments()[0];
                    }
                }

                // 如果是枚举类型，预先构建枚举映射缓存
                if (info.IsEnum)
                {
                    GetOrBuildEnumMapping(underlyingType);
                }

                // 预编译正则表达式（避免每行数据都重新编译正则）
                foreach (ExcelRegexAttribute regexAttr in info.RegexAttributes)
                {
                    info.CompiledRegexPatterns.Add((new Regex(regexAttr.Pattern, RegexOptions.Compiled), regexAttr.ErrorMessage));
                }

                result.Add(info);
            }

            // 按 Order 排序，Order 相同则按列名排序
            return result.OrderBy(p => p.Order).ThenBy(p => p.ColumnName).ToList();
        });
    }

    /// <summary>
    /// 获取或构建枚举映射（带缓存）
    /// </summary>
    /// <remarks>
    /// 构建枚举类型的双向映射
    /// <para>- 导出方向：枚举值 → Description 文本</para>
    /// <para>- 导入方向：Description/名称/数值字符串 → 枚举值</para>
    /// </remarks>
    /// <returns>枚举值映射</returns>
    private static ExcelEnumMapping GetOrBuildEnumMapping(Type enumType)
    {
        return _enumMappingCache.GetOrAdd(enumType, type =>
        {
            var mapping = new ExcelEnumMapping();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                object value = field.GetValue(null);
                string name = field.Name;

                // 获取 [Description] 特性的描述文本，如果没有则使用枚举名称
                DescriptionAttribute descAttr = field.GetCustomAttribute<DescriptionAttribute>();
                string description = descAttr?.Description ?? name;

                // 导出映射：枚举值 → 描述文本
                mapping.ValueToDescription[value] = description;

                /*
                 * 导入映射：描述文本 → 枚举值（忽略大小写）
                 * 注意：当多个枚举值共享相同 Description 时，TryAdd 只保留第一个映射，
                 * 这是预期行为（导入时同名描述只能映射到一个值）
                 */
                mapping.TextToValue.TryAdd(description, value);

                // 导入映射：枚举名称 → 枚举值
                mapping.TextToValue.TryAdd(name, value);

                // 导入映射：数值字符串 → 枚举值（如 "1" → SomeEnum.Value1）
                object numericValue = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
                mapping.TextToValue.TryAdd(numericValue.ToString(), value);
            }

            return mapping;
        });
    }

    #endregion

    #region 导出数据转换

    /// <summary>
    /// 构建 MiniExcel 的动态列配置
    /// </summary>
    /// <remarks>根据属性元信息生成列名、列序、列宽、格式等配置</remarks>
    /// <returns>构建的 MiniExcel 动态列配置</returns>
    private static List<DynamicExcelColumn> BuildDynamicColumns(List<ExcelPropertyInfo> propertyInfos)
    {
        var columns = new List<DynamicExcelColumn>();

        for (int i = 0; i < propertyInfos.Count; i++)
        {
            ExcelPropertyInfo info = propertyInfos[i];

            // 创建动态列：设置列名和索引
            var column = new DynamicExcelColumn(info.ColumnName) {Index = i, Width = info.ColumnAttribute?.Width ?? 10};

            // 仅当标记了 ExcelColumn 特性时才处理格式：优先使用显式指定的格式，否则按属性类型设置默认格式
            if (info.ColumnAttribute != null)
            {
                column.Format = !string.IsNullOrEmpty(info.ColumnAttribute.Format)
                    ? info.ColumnAttribute.Format
                    : GetDefaultFormat(info);
            }

            columns.Add(column);
        }

        return columns;
    }

    /// <summary>
    /// 获取导出数据对象
    /// </summary>
    /// <remarks>
    /// 当导出数据不为空时直接返回字典列表
    /// 当导出数据为空时，使用 <see cref="DataTable"/> 构建仅含列定义的空表，确保 MiniExcel 仍然输出表头行
    /// </remarks>
    /// <param name="exportData">转换后的导出数据</param>
    /// <param name="propertyInfos">属性元信息列表</param>
    /// <returns>可传递给 <see cref="MiniExcel"/> 的 <c>SaveAs</c> 方法的数据对象</returns>
    private static object GetExportDataObject(List<Dictionary<string, object>> exportData, List<ExcelPropertyInfo> propertyInfos)
    {
        if (exportData.Count > 0)
        {
            return exportData;
        }

        // 当数据为空时，构建仅含列定义的 DataTable，确保导出文件包含表头
        var table = new DataTable();
        foreach (ExcelPropertyInfo info in propertyInfos)
        {
            table.Columns.Add(info.ColumnName);
        }

        return table;
    }

    /// <summary>
    /// 根据属性类型获取默认的 Excel 格式化字符串
    /// </summary>
    /// <remarks>
    /// 当 <see cref="ExcelColumnAttribute.Format"/> 未指定时，根据属性类型返回默认格式
    /// <para>- <see cref="DateTime"/> / <see cref="DateTimeOffset"/> → yyyy-MM-dd HH:mm:ss</para>
    /// <para>- <see langword="decimal"/> / <see langword="double"/> / <see langword="float"/> → 0.00</para>
    /// </remarks>
    /// <returns>默认格式字符串；无需指定格式时为 <see langword="null"/></returns>
    private static string GetDefaultFormat(ExcelPropertyInfo info)
    {
        if (info.IsDateTime || info.IsDateTimeOffset)
        {
            return "yyyy-MM-dd HH:mm:ss";
        }

        Type type = info.UnderlyingType;
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return "0.00";
        }

        return null;
    }

    /// <summary>
    /// 将Dto数据集合转换为 MiniExcel 可写入的字典列表
    /// </summary>
    /// <remarks>遍历每条数据的每个属性，根据类型标记进行值转换（枚举→描述、Bool→是/否、集合→分隔字符串等）</remarks>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <returns>将Dto数据集合转换为 MiniExcel 可写入的字典列表</returns>
    private static List<Dictionary<string, object>> ConvertExportData<T>(IEnumerable<T> data,
        List<ExcelPropertyInfo> propertyInfos)
    {
        var result = new List<Dictionary<string, object>>();

        foreach (T item in data)
        {
            var dict = new Dictionary<string, object>();

            foreach (ExcelPropertyInfo info in propertyInfos)
            {
                // 读取属性值并转换为导出格式
                object value = info.Property.GetValue(item);
                dict[info.ColumnName] = ConvertExportValue(value, info);
            }

            result.Add(dict);
        }

        return result;
    }

    /// <summary>
    /// 将单个属性值转换为导出格式
    /// </summary>
    /// <remarks>
    /// 根据预缓存的类型标记进行快速分支判断（无需每次重新检测类型）
    /// <para>- <see cref="ExcelColumnAttribute.IsJson"/> → JSON 序列化</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsBool"/> → 是/否文本</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsEnum"/> → <see cref="DescriptionAttribute"/> 描述文本</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsDateTime"/> / <see cref="ExcelPropertyInfo.IsDateTimeOffset"/> → 格式化字符串</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsValueTypeCollection"/> → 分隔符连接</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsComplexCollection"/> → JSON 序列化</para>
    /// </remarks>
    /// <param name="value">属性原始值</param>
    /// <param name="info">属性元信息</param>
    /// <returns>将单个属性值转换为导出格式</returns>
    private static object ConvertExportValue(object value, ExcelPropertyInfo info)
    {
        if (value == null)
        {
            return null;
        }

        ExcelColumnAttribute columnAttr = info.ColumnAttribute;

        // JSON 序列化：标记了 IsJson 的属性直接序列化为 JSON 字符串
        if (columnAttr?.IsJson == true)
        {
            return value.ToJsonString();
        }

        // Bool 类型：转换为配置的 TrueText/FalseText（默认 "是"/"否"）
        if (info.IsBool)
        {
            bool boolValue = (bool)value;
            string trueText = columnAttr?.TrueText ?? "是";
            string falseText = columnAttr?.FalseText ?? "否";
            return boolValue ? trueText : falseText;
        }

        // 枚举类型：从缓存的映射表中查找 Description 描述文本
        if (info.IsEnum)
        {
            ExcelEnumMapping mapping = GetOrBuildEnumMapping(info.UnderlyingType);
            return mapping.ValueToDescription.TryGetValue(value, out string desc) ? desc : value.ToString();
        }

        // DateTime 类型：按 Format 格式化输出
        if (info.IsDateTime)
        {
            var dateValue = (DateTime)value;
            if (!string.IsNullOrEmpty(columnAttr?.Format))
            {
                return dateValue.ToString(columnAttr.Format);
            }

            return dateValue;
        }

        // DateTimeOffset 类型：按 Format 格式化输出
        if (info.IsDateTimeOffset)
        {
            var dateValue = (DateTimeOffset)value;
            if (!string.IsNullOrEmpty(columnAttr?.Format))
            {
                return dateValue.ToString(columnAttr.Format);
            }

            return dateValue;
        }

        // 值类型集合（如 List<int>、List<string>）：使用分隔符连接为字符串
        if (info.IsValueTypeCollection)
        {
            string separator = columnAttr?.Separator ?? ",";
            return JoinCollection(value, separator);
        }

        // 复杂对象集合（非值类型集合）：默认 JSON 序列化
        if (info.IsComplexCollection)
        {
            return value.ToJsonString();
        }

        // 其他类型直接返回原值
        return value;
    }

    #endregion

    #region 导入数据解析

    /// <summary>
    /// 将 Excel 原始行数据解析为强类型Dto列表
    /// </summary>
    /// <remarks>
    /// 处理流程
    /// <para>1. 构建列名→属性的映射关系</para>
    /// <para>2. 逐行遍历，对每个属性执行：查找列值 → 必填验证 → 正则验证 → 类型转换 → 赋值</para>
    /// <para>3. 收集所有验证错误和转换错误</para>
    /// </remarks>
    /// <typeparam name="T">数据模型类型</typeparam>
    /// <param name="rows">从 MiniExcel 读取的原始行数据</param>
    /// <returns>导入结果</returns>
    private static ExcelImportResult<T> ParseImportData<T>(List<IDictionary<string, object>> rows) where T : class, new()
    {
        var result = new ExcelImportResult<T>();

        // 从缓存获取属性元信息
        List<ExcelPropertyInfo> propertyInfos = GetPropertyInfos<T>();

        // 逐行解析数据
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            IDictionary<string, object> row = rows[rowIndex];
            var item = new T();
            var rowErrors = new List<ExcelImportError>();
            // 行号从 1 开始（不含表头行）
            int rowNumber = rowIndex + 1;

            // 遍历每个属性，尝试从行数据中提取并转换值
            foreach (ExcelPropertyInfo info in propertyInfos)
            {
                // --- 第一步：从行数据中查找对应列的值 ---
                bool columnFound = false;

                // 优先按 ExcelColumn.Name 匹配列
                if (row.TryGetValue(info.ColumnName, out object cellValue))
                {
                    columnFound = true;
                }
                // 其次按属性名匹配列
                else if (info.ColumnName != info.Property.Name && row.TryGetValue(info.Property.Name, out cellValue))
                {
                    columnFound = true;
                }

                // --- 第二步：必填验证 ---
                if (info.RequiredAttribute != null)
                {
                    if (!columnFound || cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString()))
                    {
                        rowErrors.Add(new ExcelImportError
                        {
                            RowIndex = rowNumber,
                            ColumnName = info.ColumnName,
                            PropertyName = info.Property.Name,
                            CellValue = cellValue,
                            ErrorMessage = string.IsNullOrEmpty(info.RequiredAttribute.ErrorMessage)
                                ? $"{info.ColumnName} 不能为空"
                                : info.RequiredAttribute.ErrorMessage
                        });
                        // 必填字段为空时跳过后续验证和赋值
                        continue;
                    }
                }

                // 列不存在或值为空时跳过（非必填字段）
                if (!columnFound || cellValue == null)
                {
                    continue;
                }

                string cellString = cellValue.ToString()?.Trim();

                /*
                 * --- 第三步：正则验证（使用预编译的 Regex 对象） ---
                 * 注意：仅对非空值执行正则验证。空值的存在性应由 ExcelRequired 特性控制，
                 * 正则验证仅校验"有值时格式是否正确"，不负责判断"是否必须有值"
                 */
                if (!string.IsNullOrEmpty(cellString))
                {
                    bool hasRegexError = false;
                    foreach ((Regex compiledRegex, string errorMessage) in info.CompiledRegexPatterns)
                    {
                        if (!compiledRegex.IsMatch(cellString))
                        {
                            rowErrors.Add(new ExcelImportError
                            {
                                RowIndex = rowNumber,
                                ColumnName = info.ColumnName,
                                PropertyName = info.Property.Name,
                                CellValue = cellValue,
                                ErrorMessage = string.IsNullOrEmpty(errorMessage) ? $"{info.ColumnName} 格式不正确" : errorMessage
                            });
                            hasRegexError = true;
                        }
                    }

                    // 正则验证失败时跳过类型转换和赋值
                    if (hasRegexError)
                    {
                        continue;
                    }
                }

                // --- 第四步：类型转换并赋值 ---
                try
                {
                    object convertedValue = ConvertImportValue(cellValue, info);
                    if (convertedValue != null)
                    {
                        info.Property.SetValue(item, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    rowErrors.Add(new ExcelImportError
                    {
                        RowIndex = rowNumber,
                        ColumnName = info.ColumnName,
                        PropertyName = info.Property.Name,
                        CellValue = cellValue,
                        ErrorMessage = $"{info.ColumnName} 数据转换失败：{ex.Message}"
                    });
                }
            }

            // 无论是否有错误，都将解析结果加入（由调用方决定如何处理错误行）
            result.Data.Add(item);
            result.Errors.AddRange(rowErrors);
        }

        return result;
    }

    /// <summary>
    /// 将单元格值转换为属性对应的目标类型
    /// </summary>
    /// <remarks>
    /// 根据预缓存的类型标记进行快速分支判断（无需每次重新检测类型）
    /// <para>- <see cref="ExcelColumnAttribute.IsJson"/> → JSON 反序列化</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsBool"/> → 是/否/<see langword="true"/>/<see langword="false"/>/1/0 文本解析</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsEnum"/> → 从缓存的映射表中查找（支持描述、名称、数值）</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsDateTime"/> / <see cref="ExcelPropertyInfo.IsDateTimeOffset"/> → 日期解析（支持 OLE 日期格式）</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsValueTypeCollection"/> → 按分隔符拆分为集合</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsComplexCollection"/> → JSON 反序列化</para>
    /// <para>- <see cref="ExcelPropertyInfo.IsGuid"/> → <see cref="Guid"/> 解析</para>
    /// <para>- 其他 → <see cref="Convert.ChangeType(object, Type)"/> 基础类型转换</para>
    /// </remarks>
    /// <param name="cellValue">单元格原始值</param>
    /// <param name="info">属性元信息</param>
    /// <returns>将单元格值转换为属性对应的目标类型</returns>
    private static object ConvertImportValue(object cellValue, ExcelPropertyInfo info)
    {
        if (cellValue == null)
        {
            return null;
        }

        ExcelColumnAttribute columnAttr = info.ColumnAttribute;
        string cellString = cellValue.ToString()?.Trim();

        // 空字符串处理：可空类型返回 null，值类型返回默认值
        if (string.IsNullOrEmpty(cellString))
        {
            return info.IsNullable ? null : GetDefaultValue(info.UnderlyingType);
        }

        // JSON 反序列化：标记了 IsJson 的属性从 JSON 字符串反序列化
        if (columnAttr?.IsJson == true)
        {
            return cellString.ToObject(info.PropertyType);
        }

        // Bool 类型：支持 是/否、true/false、1/0 多种格式
        if (info.IsBool)
        {
            string trueText = columnAttr?.TrueText ?? "是";
            string falseText = columnAttr?.FalseText ?? "否";

            if (string.Equals(cellString, trueText, StringComparison.OrdinalIgnoreCase)
                || string.Equals(cellString, "true", StringComparison.OrdinalIgnoreCase)
                || cellString == "1")
            {
                return true;
            }

            if (string.Equals(cellString, falseText, StringComparison.OrdinalIgnoreCase)
                || string.Equals(cellString, "false", StringComparison.OrdinalIgnoreCase)
                || cellString == "0")
            {
                return false;
            }

            throw new InvalidCastException($"无法将 \"{cellString}\" 转换为布尔值，期望值为 \"{trueText}\" 或 \"{falseText}\"");
        }

        // 枚举类型：从缓存的映射表中快速查找（支持描述文本、枚举名称、数值字符串）
        if (info.IsEnum)
        {
            ExcelEnumMapping mapping = GetOrBuildEnumMapping(info.UnderlyingType);

            // 从缓存映射中查找（已包含 Description、Name、数值字符串的映射）
            if (mapping.TextToValue.TryGetValue(cellString, out object enumValue))
            {
                return enumValue;
            }

            // 回退：尝试 Enum.TryParse（处理映射中未覆盖的情况）
            if (Enum.TryParse(info.UnderlyingType, cellString, true, out object parsedEnum))
            {
                return parsedEnum;
            }

            throw new InvalidCastException($"无法将 \"{cellString}\" 转换为枚举类型 {info.UnderlyingType.Name}");
        }

        // DateTime 类型：支持原生 DateTime、OLE 日期（double）、格式化字符串
        if (info.IsDateTime)
        {
            if (cellValue is DateTime dtValue)
            {
                return dtValue;
            }

            // Excel 内部存储日期为 OLE Automation 日期（double 类型）
            if (cellValue is double oleDate)
            {
                return DateTime.FromOADate(oleDate);
            }

            // 如果指定了格式化字符串，使用精确解析
            if (!string.IsNullOrEmpty(columnAttr?.Format))
            {
                return DateTime.ParseExact(cellString, columnAttr.Format, System.Globalization.CultureInfo.InvariantCulture);
            }

            return DateTime.Parse(cellString);
        }

        // DateTimeOffset 类型：支持原生 DateTimeOffset、DateTime、OLE 日期
        if (info.IsDateTimeOffset)
        {
            if (cellValue is DateTimeOffset dtoValue)
            {
                return dtoValue;
            }

            if (cellValue is DateTime dtValue2)
            {
                return new DateTimeOffset(dtValue2);
            }

            if (cellValue is double oleDate2)
            {
                return new DateTimeOffset(DateTime.FromOADate(oleDate2));
            }

            return DateTimeOffset.Parse(cellString);
        }

        // 值类型集合（如 List<int>、List<string>）：按分隔符拆分并转换每个元素
        if (info.IsValueTypeCollection)
        {
            string separator = columnAttr?.Separator ?? ",";
            return ParseValueTypeCollection(cellString, info.PropertyType, info.CollectionElementType, separator);
        }

        // 复杂对象集合（非值类型）：JSON 反序列化
        if (info.IsComplexCollection)
        {
            return cellString.ToObject(info.PropertyType);
        }

        // Guid 类型：解析 Guid 字符串
        if (info.IsGuid)
        {
            return Guid.Parse(cellString);
        }

        // 其他基础类型：使用 Convert.ChangeType 进行通用转换
        return Convert.ChangeType(cellValue, info.UnderlyingType);
    }

    #endregion

    #region 类型判断辅助

    /// <summary>
    /// 判断类型是否为值类型集合
    /// </summary>
    /// <remarks>如 <c>List&lt;int&gt;</c>、<c>List&lt;string&gt;</c>、<c>int[]</c>、<c>string[]</c> 等</remarks>
    /// <param name="type">待检查的类型</param>
    /// <returns>类型是否为受支持的值类型集合</returns>
    private static bool CheckIsValueTypeCollection(Type type)
    {
        // 数组类型：检查元素类型是否为值类型或常用简单类型
        if (type.IsArray)
        {
            Type arrayElementType = type.GetElementType();
            return arrayElementType != null
                   && (arrayElementType.IsPrimitive
                       || arrayElementType == typeof(string)
                       || arrayElementType == typeof(decimal)
                       || arrayElementType == typeof(Guid)
                       || arrayElementType == typeof(DateTime));
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        // 检查是否为常见的集合泛型定义
        Type genericDef = type.GetGenericTypeDefinition();
        if (genericDef != typeof(List<>)
            && genericDef != typeof(IList<>)
            && genericDef != typeof(IEnumerable<>)
            && genericDef != typeof(ICollection<>))
        {
            return false;
        }

        // 检查元素类型是否为值类型或常用简单类型
        Type elementType = type.GetGenericArguments()[0];
        return elementType.IsPrimitive
               || elementType == typeof(string)
               || elementType == typeof(decimal)
               || elementType == typeof(Guid)
               || elementType == typeof(DateTime);
    }

    /// <summary>
    /// 判断类型是否为复杂对象集合
    /// </summary>
    /// <remarks>实现了 <see cref="IEnumerable"/> 的非值类型集合（数组、<see cref="List{T}"/> 等）</remarks>
    /// <param name="type">待检查的类型</param>
    /// <returns>类型是否为复杂对象集合</returns>
    private static bool CheckIsComplexCollection(Type type)
    {
        // 值类型集合已在 CheckIsValueTypeCollection 中处理，此处排除
        if (CheckIsValueTypeCollection(type))
        {
            return false;
        }

        // 数组类型且非值类型数组视为复杂集合
        if (type.IsArray)
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        // 实现了 IEnumerable 且不是字符串
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region 值转换辅助

    /// <summary>
    /// 将集合转换为分隔符连接的字符串
    /// </summary>
    /// <remarks>用于导出时将 <c>List&lt;int&gt;</c> 等值类型集合转为 "1,2,3" 格式的字符串</remarks>
    /// <returns>将集合转换为分隔符连接的字符串</returns>
    private static string JoinCollection(object value, string separator)
    {
        if (value is not IEnumerable enumerable)
        {
            return value?.ToString();
        }

        var items = new List<string>();
        foreach (object item in enumerable)
        {
            items.Add(item?.ToString() ?? string.Empty);
        }

        return string.Join(separator, items);
    }

    /// <summary>
    /// 将分隔符字符串解析为值类型集合
    /// </summary>
    /// <remarks>用于导入时将 "1,2,3" 格式的字符串转为 <c>List&lt;int&gt;</c>、<c>int[]</c> 等值类型集合</remarks>
    /// <param name="text">分隔符连接的字符串</param>
    /// <param name="collectionType">目标集合类型</param>
    /// <param name="elementType">集合元素类型（从缓存获取，避免重复调用 <see cref="Type.GetGenericArguments()"/>）</param>
    /// <param name="separator">分隔符</param>
    /// <returns>将分隔符字符串解析为值类型集合</returns>
    private static object ParseValueTypeCollection(string text, Type collectionType, Type elementType, string separator)
    {
        /*
         * 防御性编程：正常情况下 elementType 在 ExcelPropertyInfo 初始化时已缓存，
         * 此处兜底处理仅用于方法被独立调用时的安全保障
         */
        elementType ??= collectionType.IsArray ? collectionType.GetElementType() : collectionType.GetGenericArguments()[0];

        // 按分隔符拆分并去除空项
        string[] parts = text.Split([separator], StringSplitOptions.RemoveEmptyEntries);

        // 数组类型：直接创建对应类型的数组
        if (collectionType.IsArray)
        {
            var convertedParts = new List<object>(parts.Length);
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    convertedParts.Add(Convert.ChangeType(trimmed, elementType));
                }
            }

            var array = Array.CreateInstance(elementType, convertedParts.Count);
            for (int i = 0; i < convertedParts.Count; i++)
            {
                array.SetValue(convertedParts[i], i);
            }

            return array;
        }

        // 泛型集合类型：创建对应类型的 List<T> 实例
        Type listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType);

        // 逐项转换并添加到列表
        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                list?.Add(Convert.ChangeType(trimmed, elementType));
            }
        }

        return list;
    }

    /// <summary>
    /// 获取类型的默认值
    /// <remarks>值类型返回 <c>default(T)</c>，引用类型返回 <see langword="null"/></remarks>
    /// </summary>
    /// <returns>类型的默认值；值类型返回 <see langword="default"/>，引用类型返回 <see langword="null"/></returns>
    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    #endregion
}
