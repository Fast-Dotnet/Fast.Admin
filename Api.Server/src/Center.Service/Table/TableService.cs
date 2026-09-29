// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Cache;
using Fast.Center.Domain;

namespace Fast.Center.Service.Table;

/// <summary>
/// 表格服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "table")]
public partial class TableService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ICache<CenterCCL> _centerCache;
    private readonly ISqlSugarRepository<TableConfigModel> _tableRepository;
    private readonly ISqlSugarRepository<TableColumnConfigModel> _columnRepository;
    private readonly ISqlSugarRepository<TableColumnConfigCacheModel> _columnCacheRepository;

    public TableService(IUser user, ICache<CenterCCL> center, ISqlSugarRepository<TableConfigModel> tableRepository,
        ISqlSugarRepository<TableColumnConfigModel> columnRepository,
        ISqlSugarRepository<TableColumnConfigCacheModel> columnCacheRepository)
    {
        _user = user;
        _centerCache = center;
        _tableRepository = tableRepository;
        _columnRepository = columnRepository;
        _columnCacheRepository = columnCacheRepository;
    }
}
