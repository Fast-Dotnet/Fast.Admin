// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Merchant.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.Merchant;

/// <summary>
/// 商户号服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "merchant")]
[PlatformOnly]
public class MerchantService : IDynamicApplication
{
    private readonly ISqlSugarRepository<MerchantModel> _repository;

    public MerchantService(ISqlSugarRepository<MerchantModel> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 商户号选择器
    /// </summary>
    [HttpGet]
    [ApiInfo("商户号选择器", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Merchant.Paged)]
    public async Task<List<ElSelectorOutput<long>>> MerchantSelector(PaymentChannelEnum? merchantType)
    {
        var data = await _repository.Entities.WhereIF(merchantType != null, wh => wh.MerchantType == merchantType)
            .OrderBy(ob => ob.MerchantNo)
            .Select(sl => new {sl.MerchantId, sl.MerchantName, sl.MerchantNo, sl.MerchantType})
            .ToListAsync();

        return data.Select(sl =>
                new ElSelectorOutput<long>
                {
                    Value = sl.MerchantId, Label = sl.MerchantNo, Data = new {sl.MerchantName, sl.MerchantType}
                })
            .ToList();
    }

    /// <summary>
    /// 获取商户号分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取商户号分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Merchant.Paged)]
    public async Task<PagedResult<QueryMerchantPagedOutput>> QueryMerchantPaged(QueryMerchantPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.MerchantType != null, wh => wh.MerchantType == input.MerchantType)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryMerchantPagedOutput
            {
                MerchantId = sl.MerchantId,
                MerchantType = sl.MerchantType,
                MerchantName = sl.MerchantName,
                MerchantNo = sl.MerchantNo,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取商户号详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取商户号详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Merchant.Detail)]
    public async Task<QueryMerchantDetailOutput> QueryMerchantDetail([Required(ErrorMessage = "商户号Id不能为空")] long? merchantId)
    {
        QueryMerchantDetailOutput result = await _repository.Entities.Where(wh => wh.MerchantId == merchantId)
            .Select(sl => new QueryMerchantDetailOutput
            {
                MerchantId = sl.MerchantId,
                MerchantType = sl.MerchantType,
                MerchantName = sl.MerchantName,
                MerchantNo = sl.MerchantNo,
                MerchantSecret = sl.MerchantSecret,
                PublicSerialNum = sl.PublicSerialNum,
                PublicKey = sl.PublicKey,
                CertSerialNum = sl.CertSerialNum,
                Cert = sl.Cert,
                CertPrivateKey = sl.CertPrivateKey,
                Remark = sl.Remark,
                DepartmentName = sl.DepartmentName,
                CreatedUserName = sl.CreatedUserName,
                CreatedTime = sl.CreatedTime,
                UpdatedUserName = sl.UpdatedUserName,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加商户号
    /// </summary>
    [HttpPost]
    [ApiInfo("添加商户号", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Merchant.Add)]
    public async Task AddMerchant(AddMerchantInput input)
    {
        if (await _repository.AnyAsync(a => a.MerchantNo == input.MerchantNo))
        {
            throw new UserFriendlyException("商户号重复！");
        }

        var merchantModel = new MerchantModel
        {
            MerchantType = input.MerchantType,
            MerchantName = input.MerchantName,
            MerchantNo = input.MerchantNo,
            MerchantSecret = input.MerchantSecret,
            PublicSerialNum = input.PublicSerialNum,
            PublicKey = input.PublicKey,
            CertSerialNum = input.CertSerialNum,
            Cert = input.Cert,
            CertPrivateKey = input.CertPrivateKey,
            Remark = input.Remark
        };

        await _repository.InsertAsync(merchantModel);
    }

    /// <summary>
    /// 编辑商户号
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑商户号", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Merchant.Edit)]
    public async Task EditMerchant(EditMerchantInput input)
    {
        if (await _repository.AnyAsync(a => a.MerchantNo == input.MerchantNo && a.MerchantId != input.MerchantId))
        {
            throw new UserFriendlyException("商户号重复！");
        }

        MerchantModel merchantModel = await _repository.SingleOrDefaultAsync(s => s.MerchantId == input.MerchantId);
        if (merchantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        merchantModel.MerchantType = input.MerchantType;
        merchantModel.MerchantName = input.MerchantName;
        merchantModel.MerchantNo = input.MerchantNo;
        merchantModel.MerchantSecret = input.MerchantSecret;
        merchantModel.PublicSerialNum = input.PublicSerialNum;
        merchantModel.PublicKey = input.PublicKey;
        merchantModel.CertSerialNum = input.CertSerialNum;
        merchantModel.Cert = input.Cert;
        merchantModel.CertPrivateKey = input.CertPrivateKey;
        merchantModel.Remark = input.Remark;
        merchantModel.RowVersion = input.RowVersion;

        await _repository.Ado.UseTranAsync(async () =>
        {
            await _repository.UpdateAsync(merchantModel);
            await _repository.Updateable<ApplicationOpenIdModel>()
                .SetColumns(_ => new ApplicationOpenIdModel {WeChatMerchantNo = merchantModel.MerchantNo})
                .Where(wh => wh.WeChatMerchantId == merchantModel.MerchantId)
                .ExecuteCommandAsync();
            await _repository.Updateable<ApplicationOpenIdModel>()
                .SetColumns(_ => new ApplicationOpenIdModel {AlipayMerchantNo = merchantModel.MerchantNo})
                .Where(wh => wh.AlipayMerchantId == merchantModel.MerchantId)
                .ExecuteCommandAsync();
        }, ex => throw ex);

        // 删除缓存
        await MerchantContext.DeleteMerchant(merchantModel.MerchantNo);
    }

    /// <summary>
    /// 删除商户号
    /// </summary>
    [HttpPost]
    [ApiInfo("删除商户号", HttpRequestActionEnum.Delete)]
    [Permission(PermissionConst.Merchant.Delete)]
    public async Task DeleteMerchant(MerchantIdInput input)
    {
        if (await _repository.Queryable<ApplicationOpenIdModel>()
                .AnyAsync(a => a.WeChatMerchantId == input.MerchantId || a.AlipayMerchantId == input.MerchantId))
        {
            throw new UserFriendlyException("商户号存在绑定应用，无法删除！");
        }

        MerchantModel merchantModel = await _repository.SingleOrDefaultAsync(s => s.MerchantId == input.MerchantId);
        if (merchantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        await _repository.DeleteAsync(merchantModel);
        // 删除缓存
        await MerchantContext.DeleteMerchant(merchantModel.MerchantNo);
    }
}
