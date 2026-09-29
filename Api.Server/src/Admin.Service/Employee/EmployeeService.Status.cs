// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Text.RegularExpressions;
using Fast.Admin.Domain;
using Fast.Admin.Service.Employee.Dto;
using Fast.AdminLog.Domain;
using Fast.Center.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace Fast.Admin.Service.Employee;

public partial class EmployeeService
{
    /// <summary>
    /// 职员更改状态
    /// </summary>
    [HttpPost]
    [ApiInfo("职员更改状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task ChangeStatus(ChangeStatusInput input)
    {
        if (input.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止直接修改为离职状态！");
        }

        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.ResignDate != null)
        {
            employeeModel.ResignDate = null;
            employeeModel.ResignReason = null;
        }

        employeeModel.Status = input.Status;
        employeeModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(employeeModel);

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "更改职员状态",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，状态 -> {employeeModel.Status.GetDescription()}"
        });
    }

    /// <summary>
    /// 职员离职
    /// </summary>
    [HttpPost]
    [ApiInfo("职员离职", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task EmployeeResigned(EmployeeResignedInput input)
    {
        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.EmployeeId == _user.EmployeeId)
        {
            throw new UserFriendlyException("禁止将当前登录职员设为离职！");
        }

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("该职员已离职，请勿重复操作！");
        }

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            TenantUserModel tenantUserModel = await _centerRepository
                .Queryable<TenantUserModel>()
                .InSingleAsync(employeeModel.EmployeeId);
            if (tenantUserModel != null)
            {
                tenantUserModel.Status = CommonStatusEnum.Disable;
                await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();
            }

            employeeModel.Status = EmployeeStatusEnum.Resigned;
            employeeModel.ResignDate = input.ResignDate;
            employeeModel.ResignReason = input.ResignReason;
            employeeModel.RowVersion = input.RowVersion;

            await _repository.UpdateAsync(employeeModel);

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        await _user.RevokeEmployee(_user.TenantNo, employeeModel.EmployeeNo);
        await ForceEmployeeOffline(employeeModel.EmployeeId, "职员已离职");

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "职员离职",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，离职 -> {employeeModel.ResignDate:yyyy-MM-dd HH:mm:ss}"
        });
    }

    /// <summary>
    /// 绑定登录账号
    /// </summary>
    [HttpPost]
    [ApiInfo("绑定登录账号", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Edit)]
    public async Task BindLoginAccount(BindLoginAccountInput input)
    {
        if (!new Regex(RegexConst.Mobile).IsMatch(input.Mobile))
        {
            throw new UserFriendlyException("手机号码不正确！");
        }

        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止为已离职的职员绑定登录账号！");
        }

        if (await _centerRepository.Queryable<TenantUserModel>().AnyAsync(a => a.EmployeeId == employeeModel.EmployeeId))
        {
            throw new UserFriendlyException("已存在登录账号！");
        }

        EmployeeOrgModel employeeOrgModel = await _repository.Queryable<EmployeeOrgModel>()
            .SingleAsync(s => s.EmployeeId == employeeModel.EmployeeId && s.IsPrimary);

        if (string.IsNullOrWhiteSpace(employeeModel.Email))
        {
            employeeModel.Email = input.Email;
        }

        employeeModel.RowVersion = input.RowVersion;

        // 开启事务
        await _repository.Ado.BeginTranAsync();
        await _centerRepository.Ado.BeginTranAsync();
        try
        {
            AccountModel accountModel = await _centerRepository.Queryable<AccountModel>()
                .Where(wh => wh.Mobile == input.Mobile)
                .SingleAsync();
            if (accountModel == null)
            {
                if (await _centerRepository.Queryable<AccountModel>().AnyAsync(a => a.Email == input.Email))
                {
                    throw new UserFriendlyException("邮箱已存在账号信息！");
                }

                string passwordHash = CryptoUtil.HashPasswordPBKDF2SHA256(CommonConst.Default.Password);
                long accountId = YitIdHelper.NextId();
                accountModel = new AccountModel
                {
                    AccountId = accountId,
                    AccountKey = NumberUtil.IdToCodeByLong(accountId),
                    Mobile = input.Mobile,
                    Email = input.Email,
                    Password = passwordHash,
                    Status = CommonStatusEnum.Enable,
                    NickName = employeeModel.EmployeeName,
                    Avatar = employeeModel.IdPhoto
                };
                await _centerRepository.Insertable(accountModel).ExecuteCommandAsync();

                #region PasswordRecordModel

                // 初始化密码记录表
                await _centerRepository.Insertable(new List<PasswordRecordModel>
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

            var tenantUserModel = new TenantUserModel
            {
                EmployeeId = employeeModel.EmployeeId,
                UserKey = NumberUtil.IdToCodeByLong(employeeModel.EmployeeId),
                AccountId = accountModel.AccountId,
                EmployeeNo = employeeModel.EmployeeNo,
                EmployeeName = employeeModel.EmployeeName,
                IdPhoto = employeeModel.IdPhoto,
                DepartmentId = employeeOrgModel?.DepartmentId,
                DepartmentName = employeeOrgModel?.DepartmentName,
                UserType = UserTypeEnum.None,
                Status = CommonStatusEnum.Enable
            };
            await _centerRepository.Insertable(tenantUserModel).ExecuteCommandAsync();

            await _repository.UpdateAsync(employeeModel);

            // 提交事务
            await _repository.Ado.CommitTranAsync();
            await _centerRepository.Ado.CommitTranAsync();
        }
        catch
        {
            // 回滚事务
            await _repository.Ado.RollbackTranAsync();
            await _centerRepository.Ado.RollbackTranAsync();
            throw;
        }

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "职员绑定登录账号",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，手机：{input.Mobile}，邮箱：{input.Email}"
        });
    }

    /// <summary>
    /// 更改登录状态
    /// </summary>
    [HttpPost]
    [ApiInfo("更改登录状态", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.Employee.Status)]
    public async Task ChangeLoginStatus(EmployeeIdInput input)
    {
        EmployeeModel employeeModel = await GetEmployeeWithinDataScope(input.EmployeeId);

        if (employeeModel.RowVersion != input.RowVersion)
        {
            throw new UserFriendlyException("职员信息已发生变化，请刷新后重试！");
        }

        if (employeeModel.Status == EmployeeStatusEnum.Resigned)
        {
            throw new UserFriendlyException("禁止操作已离职的职员！");
        }

        TenantUserModel tenantUserModel = await _centerRepository
            .Queryable<TenantUserModel>()
            .InSingleAsync(employeeModel.EmployeeId);
        if (tenantUserModel == null)
        {
            throw new UserFriendlyException("未绑定登录账号！");
        }

        if (_user.AccountId == tenantUserModel.AccountId)
        {
            throw new UserFriendlyException("禁止更改当前登录账号状态！");
        }

        if (input.AccountStatus != null && input.AccountStatus != tenantUserModel.Status)
        {
            throw new UserFriendlyException("登录账号状态已发生变化，请刷新后重试！");
        }

        tenantUserModel.Status = tenantUserModel.Status switch
        {
            CommonStatusEnum.Enable => CommonStatusEnum.Disable,
            CommonStatusEnum.Disable => CommonStatusEnum.Enable,
            _ => tenantUserModel.Status
        };

        await _centerRepository.Updateable(tenantUserModel).ExecuteCommandAsync();

        if (tenantUserModel.Status == CommonStatusEnum.Disable)
        {
            await _user.RevokeEmployee(_user.TenantNo, employeeModel.EmployeeNo);
            await ForceEmployeeOffline(employeeModel.EmployeeId, "账号已被禁用");
        }

        // 操作日志
        await LogContext.OperateLog(new OperateLogDto
        {
            Title = "更改职员登录状态",
            OperateType = OperateLogTypeEnum.Organization,
            BizId = employeeModel.EmployeeId,
            BizNo = employeeModel.EmployeeNo,
            Description = $"职员：{employeeModel.EmployeeName}，{tenantUserModel.Status.GetDescription()}登录账号"
        });
    }

    /// <summary>
    /// 强制下线职员的全部在线会话
    /// </summary>
    private async Task ForceEmployeeOffline(long employeeId, string message)
    {
        List<string> connectionIds = await _centerRepository.Queryable<TenantOnlineUserModel>()
            .Where(wh => wh.IsOnline)
            .Where(wh => wh.TenantId == _user.TenantId)
            .Where(wh => wh.EmployeeId == employeeId)
            .Select(sl => sl.ConnectionId)
            .ToListAsync();
        if (connectionIds.Count == 0)
        {
            return;
        }

        await _hubContext.Clients.Clients(connectionIds)
            .ForceOffline(new ForceOfflineOutput
            {
                IsAdmin = _user.IsSuperAdmin || _user.IsAdmin,
                NickName = _user.NickName,
                EmployeeNo = _user.EmployeeNo,
                OfflineTime = DateTime.Now,
                Message = message
            });
    }
}
