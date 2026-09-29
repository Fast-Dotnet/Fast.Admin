// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.Tenant.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Yitter.IdGenerator;

namespace Fast.Center.Service.Tenant;

/// <summary>
/// 租户服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "tenant")]
public class TenantService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<TenantModel> _repository;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public TenantService(IUser user, ISqlSugarRepository<TenantModel> repository, IHubContext<ChatHub, IChatClient> hubContext)
    {
        _user = user;
        _repository = repository;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 租户选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("租户选择器", HttpRequestActionEnum.Query)]
    public async Task<PagedResult<ElSelectorOutput<long>>> TenantSelector(PagedInput input)
    {
        TenantModel tenantModel = await TenantContext.GetTenant(_user.TenantNo);

        PagedResult<TenantModel> data = await _repository.Entities
            .WhereIF(tenantModel.TenantType == TenantTypeEnum.Common, wh => wh.TenantId == _user.TenantId)
            .OrderBy(ob => ob.TenantName)
            .Select(sl => new TenantModel
            {
                TenantId = sl.TenantId,
                TenantName = sl.TenantName,
                TenantNo = sl.TenantNo,
                TenantCode = sl.TenantCode,
                ShortName = sl.ShortName,
                Edition = sl.Edition,
                LogoUrl = sl.LogoUrl
            })
            .ToPagedListAsync(input);

        return data.ToPagedData(sl => new ElSelectorOutput<long>
        {
            Value = sl.TenantId,
            Label = sl.TenantName,
            Data = new
            {
                sl.TenantNo,
                sl.TenantCode,
                sl.ShortName,
                sl.Edition,
                sl.LogoUrl
            }
        });
    }

    /// <summary>
    /// 获取租户分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取租户分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.Tenant.Paged)]
    [PlatformOnly]
    public async Task<PagedResult<QueryTenantPagedOutput>> QueryTenantPaged(QueryTenantPagedInput input)
    {
        return await _repository.Entities.WhereIF(input.Status != null, wh => wh.Status == input.Status)
            .WhereIF(input.Edition != null, wh => (wh.Edition & input.Edition) != 0)
            .WhereIF(!string.IsNullOrWhiteSpace(input.AdminMobile), wh => wh.AdminMobile.Contains(input.AdminMobile))
            .WhereIF(!string.IsNullOrWhiteSpace(input.AdminEmail), wh => wh.AdminEmail.Contains(input.AdminEmail))
            .WhereIF(input.TenantType != null, wh => wh.TenantType == input.TenantType)
            .OrderByDescending(ob => ob.CreatedTime)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryTenantPagedOutput
            {
                TenantId = sl.TenantId,
                TenantNo = sl.TenantNo,
                TenantCode = sl.TenantCode,
                Status = sl.Status,
                TenantName = sl.TenantName,
                ShortName = sl.ShortName,
                SpellName = sl.SpellName,
                Edition = sl.Edition,
                AdminAccountId = sl.AdminAccountId,
                AdminName = sl.AdminName,
                AdminMobile = sl.AdminMobile,
                AdminEmail = sl.AdminEmail,
                AdminPhone = sl.AdminPhone,
                RobotName = sl.RobotName,
                TenantType = sl.TenantType,
                LogoUrl = sl.LogoUrl,
                AllowDeleteData = sl.AllowDeleteData,
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
    /// 获取租户详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取租户详情", HttpRequestActionEnum.Query)]
    [Permission(PermissionConst.Tenant.Detail)]
    [PlatformOnly]
    public async Task<QueryTenantDetailOutput> QueryTenantDetail([Required(ErrorMessage = "租户Id不能为空")] long? tenantId)
    {
        QueryTenantDetailOutput result = await _repository.Queryable<TenantModel>()
            .LeftJoin<AccountModel>((t1, t2) => t1.AdminAccountId == t2.AccountId)
            .Where(t1 => t1.TenantId == tenantId)
            .Select((t1, t2) => new QueryTenantDetailOutput
            {
                TenantId = t1.TenantId,
                TenantNo = t1.TenantNo,
                TenantCode = t1.TenantCode,
                Status = t1.Status,
                TenantName = t1.TenantName,
                ShortName = t1.ShortName,
                SpellName = t1.SpellName,
                Edition = t1.Edition,
                AdminAccountId = t1.AdminAccountId,
                AdminName = t1.AdminName,
                AdminMobile = t1.AdminMobile,
                AdminEmail = t1.AdminEmail,
                AdminPhone = t1.AdminPhone,
                RobotName = t1.RobotName,
                TenantType = t1.TenantType,
                LogoUrl = t1.LogoUrl,
                AllowDeleteData = t1.AllowDeleteData,
                DepartmentName = t1.DepartmentName,
                CreatedUserName = t1.CreatedUserName,
                CreatedTime = t1.CreatedTime,
                UpdatedUserName = t1.UpdatedUserName,
                UpdatedTime = t1.UpdatedTime,
                RowVersion = t1.RowVersion,
                FirstLoginDevice = t2.FirstLoginDevice,
                FirstLoginOS = t2.FirstLoginOS,
                FirstLoginBrowser = t2.FirstLoginBrowser,
                FirstLoginProvince = t2.FirstLoginProvince,
                FirstLoginCity = t2.FirstLoginCity,
                FirstLoginIp = t2.FirstLoginIp,
                FirstLoginTime = t2.FirstLoginTime,
                LastLoginDevice = t2.LastLoginDevice,
                LastLoginOS = t2.LastLoginOS,
                LastLoginBrowser = t2.LastLoginBrowser,
                LastLoginProvince = t2.LastLoginProvince,
                LastLoginCity = t2.LastLoginCity,
                LastLoginIp = t2.LastLoginIp,
                LastLoginTime = t2.LastLoginTime,
                PasswordErrorTime = t2.PasswordErrorTime,
                LockStartTime = t2.LockStartTime,
                LockEndTime = t2.LockEndTime
            })
            .SingleAsync();

        if (result == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        return result;
    }

    /// <summary>
    /// 添加租户
    /// </summary>
    [HttpPost]
    [ApiInfo("添加租户", HttpRequestActionEnum.Add)]
    [Permission(PermissionConst.Tenant.Add)]
    [PlatformOnly]
    public async Task AddTenant(AddTenantInput input)
    {
        if (await _repository.AnyAsync(a => a.TenantCode == input.TenantCode))
        {
            throw new UserFriendlyException("租户编码重复！");
        }

        if (await _repository.AnyAsync(a => a.TenantName == input.TenantName))
        {
            throw new UserFriendlyException("租户名称重复！");
        }

        var tenantModel = new TenantModel
        {
            TenantId = YitIdHelper.NextId(),
            TenantCode = input.TenantCode,
            TenantName = input.TenantName,
            Status = CommonStatusEnum.Enable,
            ShortName = input.ShortName,
            SpellName = input.SpellName,
            Edition = input.Edition,
            AdminName = input.AdminName,
            AdminMobile = input.AdminMobile,
            AdminEmail = input.AdminEmail,
            AdminPhone = input.AdminPhone,
            RobotName = input.RobotName,
            TenantType = TenantTypeEnum.Common,
            LogoUrl = input.LogoUrl,
            AllowDeleteData = true
        };

        await _repository.Ado.UseTranAsync(async () =>
        {
            tenantModel.TenantNo = SysSerialContext.GenTenantNo(_repository);
            await _repository.InsertAsync(tenantModel);
        }, ex => throw ex);

        // 删除缓存
        await TenantContext.DeleteTenant(tenantModel.TenantNo);
    }

    /// <summary>
    /// 编辑租户
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑租户", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Tenant.Edit)]
    [PlatformOnly]
    public async Task EditTenant(EditTenantInput input)
    {
        if (await _repository.AnyAsync(a => a.TenantCode == input.TenantCode && a.TenantId != input.TenantId))
        {
            throw new UserFriendlyException("租户编码重复！");
        }

        if (await _repository.AnyAsync(a => a.TenantName == input.TenantName && a.TenantId != input.TenantId))
        {
            throw new UserFriendlyException("租户名称重复！");
        }

        TenantModel tenantModel = await _repository.SingleOrDefaultAsync(input.TenantId);
        if (tenantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        tenantModel.TenantCode = input.TenantCode;
        tenantModel.Status = input.Status;
        tenantModel.TenantName = input.TenantName;
        tenantModel.ShortName = input.ShortName;
        tenantModel.SpellName = input.SpellName;
        tenantModel.Edition = input.Edition;
        tenantModel.AdminName = input.AdminName;
        tenantModel.AdminEmail = input.AdminEmail;
        tenantModel.AdminPhone = input.AdminPhone;
        tenantModel.LogoUrl = input.LogoUrl;
        tenantModel.RowVersion = input.RowVersion;

        await _repository.Ado.UseTranAsync(async () =>
        {
            // 只有手机号码不同才更换管理员
            if (tenantModel.AdminMobile != input.AdminMobile)
            {
                if (tenantModel.AdminAccountId != 0)
                {
                    AccountModel accountModel = await _repository.Queryable<AccountModel>()
                        .Where(wh => wh.Mobile == input.AdminMobile)
                        .SingleAsync();
                    if (accountModel == null)
                    {
                        string passwordHash = CryptoUtil.HashPasswordPBKDF2SHA256(CommonConst.Default.Password);
                        long accountId = YitIdHelper.NextId();
                        accountModel = new AccountModel
                        {
                            AccountId = accountId,
                            AccountKey = NumberUtil.IdToCodeByLong(accountId),
                            Mobile = input.AdminMobile,
                            Email = input.AdminEmail,
                            Password = passwordHash,
                            NickName = input.AdminName,
                            Avatar = tenantModel.LogoUrl,
                            Status = CommonStatusEnum.Enable
                        };
                        accountModel = await _repository.Insertable(accountModel).ExecuteReturnEntityAsync();

                        #region PasswordRecordModel

                        // 初始化密码记录表
                        await _repository.Insertable(new List<PasswordRecordModel>
                            {
                                new()
                                {
                                    AccountId = accountModel.AccountId,
                                    OperationType = PasswordOperationTypeEnum.Create,
                                    Type = PasswordTypeEnum.PBKDF2_SHA256,
                                    Password = passwordHash
                                }
                            })
                            .ExecuteCommandAsync();

                        #endregion
                    }

                    TenantUserModel tenantUserModel = await _repository.Queryable<TenantUserModel>()
                        .Where(wh => wh.AccountId == tenantModel.AdminAccountId)
                        .SingleAsync();
                    tenantUserModel.AccountId = accountModel.AccountId;
                    await _repository.Updateable(tenantUserModel).ExecuteCommandAsync();

                    // 回填管理员账号Id
                    tenantModel.AdminAccountId = accountModel.AccountId;
                }

                tenantModel.AdminMobile = input.AdminMobile;
            }

            if (tenantModel.RobotName != input.RobotName)
            {
                TenantUserModel tenantUserModel = await _repository.Queryable<TenantUserModel>()
                    .Where(wh => wh.UserType == UserTypeEnum.Robot)
                    .SingleAsync();
                if (tenantUserModel != null)
                {
                    tenantUserModel.EmployeeName = input.RobotName;
                    await _repository.Updateable(tenantUserModel).ExecuteCommandAsync();
                }

                tenantModel.RobotName = input.RobotName;
            }

            await _repository.UpdateAsync(tenantModel);
        }, ex => throw ex);

        if (tenantModel.Status == CommonStatusEnum.Disable)
        {
            // 撤销租户全部会话
            await _user.RevokeTenant(tenantModel.TenantNo);
        }

        // 删除缓存
        await TenantContext.DeleteTenant(tenantModel.TenantNo);
    }

    /// <summary>
    /// 租户更改状态
    /// </summary>
    [HttpPost]
    [ApiInfo("租户更改状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Tenant.Status)]
    [PlatformOnly]
    public async Task ChangeStatus(TenantIdInput input)
    {
        TenantModel tenantModel = await _repository.SingleOrDefaultAsync(input.TenantId);
        if (tenantModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (tenantModel.TenantType == TenantTypeEnum.System)
        {
            throw new UserFriendlyException("禁止修改系统租户状态！");
        }

        // 更改状态
        tenantModel.Status = tenantModel.Status switch
        {
            CommonStatusEnum.Enable => CommonStatusEnum.Disable,
            CommonStatusEnum.Disable => CommonStatusEnum.Enable,
            _ => tenantModel.Status
        };
        tenantModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(tenantModel);

        if (tenantModel.Status == CommonStatusEnum.Disable)
        {
            await _user.RevokeTenant(tenantModel.TenantNo);

            // 强制下线当前租户所有在线用户
            List<string> connectionIdList = await _repository.Queryable<TenantOnlineUserModel>()
                .ClearFilter<IBaseTEntity>()
                .Where(wh => wh.IsOnline)
                .Where(wh => wh.TenantId == tenantModel.TenantId)
                .Select(sl => sl.ConnectionId)
                .ToListAsync();

            await _hubContext.Clients.Clients(connectionIdList)
                .ForceOffline(new ForceOfflineOutput
                {
                    IsAdmin = true,
                    NickName = "系统操作",
                    EmployeeNo = "无",
                    OfflineTime = DateTime.Now,
                    Message = "当前租户已被禁用"
                });
        }

        // 删除缓存
        await TenantContext.DeleteTenant(tenantModel.TenantNo);
    }
}
