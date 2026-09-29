// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using Fast.Center.Domain;
using Fast.Center.Service.ClientUser.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fast.Center.Service.ClientUser;

/// <summary>
/// 客户端用户服务
/// </summary>
[ApiDescriptionSettings(ApiGroupConst.Center, Name = "clientUser")]
public class ClientUserService : IDynamicApplication
{
    private readonly IUser _user;
    private readonly ISqlSugarRepository<ClientUserModel> _repository;

    public ClientUserService(IUser user, ISqlSugarRepository<ClientUserModel> repository)
    {
        _user = user;
        _repository = repository;
    }

    /// <summary>
    /// 客户端用户选择器
    /// </summary>
    [HttpPost]
    [ApiInfo("客户端用户选择器", HttpRequestActionEnum.Paged)]
    public async Task<PagedResult<ElSelectorOutput<long>>> ClientUserSelector(PagedInput input)
    {
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);
        PagedResult<ClientUserModel> data = await _repository
            .Entities.Where(wh => wh.AppId == applicationModel.AppId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchValue),
                wh => wh.Mobile.Contains(input.SearchValue)
                      || wh.OpenId.Contains(input.SearchValue)
                      || wh.NickName.Contains(input.SearchValue))
            .OrderBy(ob => ob.Mobile)
            .Select(sl => new ClientUserModel
            {
                UserId = sl.UserId,
                Mobile = sl.Mobile,
                OpenId = sl.OpenId,
                NickName = sl.NickName,
                Avatar = sl.Avatar
            })
            .ToPagedListAsync(input);

        return data.ToPagedData(sl => new ElSelectorOutput<long>
        {
            Value = sl.UserId,
            Label = !string.IsNullOrWhiteSpace(sl.Mobile) ? sl.Mobile : sl.OpenId,
            Data = new {sl.NickName, sl.Avatar}
        });
    }

    /// <summary>
    /// 获取客户端用户分页列表
    /// </summary>
    [HttpPost]
    [ApiInfo("获取客户端用户分页列表", HttpRequestActionEnum.Paged)]
    [Permission(PermissionConst.ClientUser.Paged)]
    public async Task<PagedResult<QueryClientUserPagedOutput>> QueryClientUserPaged(QueryClientUserPagedInput input)
    {
        return await _repository
            .Entities.WhereIF(input.AppId != null, wh => wh.AppId == input.AppId)
            .WhereIF(input.UserType != null, wh => wh.UserType == input.UserType)
            .WhereIF(input.Sex != null, wh => wh.Sex == input.Sex)
            .OrderByIF(input.IsOrderBy, ob => ob.CreatedTime, OrderByType.Desc)
            .Select(sl => new QueryClientUserPagedOutput
            {
                UserId = sl.UserId,
                AppId = sl.AppId,
                UserType = sl.UserType,
                Mobile = sl.Mobile,
                OpenId = sl.OpenId,
                UnionId = sl.UnionId,
                NickName = sl.NickName,
                Avatar = sl.Avatar,
                Sex = sl.Sex,
                LastLoginDevice = sl.LastLoginDevice,
                LastLoginOS = sl.LastLoginOS,
                LastLoginBrowser = sl.LastLoginBrowser,
                LastLoginProvince = sl.LastLoginProvince,
                LastLoginCity = sl.LastLoginCity,
                LastLoginIp = sl.LastLoginIp,
                LastLoginTime = sl.LastLoginTime,
                MobileUpdateTime = sl.MobileUpdateTime,
                CreatedTime = sl.CreatedTime,
                UpdatedTime = sl.UpdatedTime,
                RowVersion = sl.RowVersion
            })
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取客户端用户详情
    /// </summary>
    [HttpGet]
    [ApiInfo("获取客户端用户详情", HttpRequestActionEnum.Query)]
    public async Task<QueryClientUserDetailOutput> QueryClientUserDetail()
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        // 获取当前客户端用户信息
        QueryClientUserDetailOutput result = await _repository
            .Entities.Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.UserId == _user.ClientUserId)
            .Select(sl => new QueryClientUserDetailOutput
            {
                UserId = sl.UserId,
                UserType = sl.UserType,
                Mobile = sl.Mobile,
                OpenId = sl.OpenId,
                UnionId = sl.UnionId,
                HasPassword = !string.IsNullOrWhiteSpace(sl.Password),
                NickName = sl.NickName,
                Avatar = sl.Avatar,
                Sex = sl.Sex,
                LastLoginDevice = sl.LastLoginDevice,
                LastLoginOS = sl.LastLoginOS,
                LastLoginBrowser = sl.LastLoginBrowser,
                LastLoginProvince = sl.LastLoginProvince,
                LastLoginCity = sl.LastLoginCity,
                LastLoginIp = sl.LastLoginIp,
                LastLoginTime = sl.LastLoginTime,
                CreatedTime = sl.CreatedTime,
                UpdatedTime = sl.UpdatedTime,
                MobileUpdateTime = sl.MobileUpdateTime,
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
    /// 编辑客户端用户
    /// </summary>
    [HttpPost]
    [ApiInfo("编辑客户端用户", HttpRequestActionEnum.Edit)]
    [Permission(PermissionConst.ClientService)]
    public async Task EditClientUser(EditClientUserInput input)
    {
        // 查询应用信息
        ApplicationOpenIdModel applicationModel = await ApplicationContext.GetApplication(GlobalContext.Origin);

        if (applicationModel.AppType != GlobalContext.DeviceType)
        {
            throw new UserFriendlyException("应用类型不匹配！");
        }

        ClientUserModel userModel = await _repository
            .Entities.Where(wh => wh.AppId == applicationModel.AppId)
            .Where(wh => wh.UserId == _user.ClientUserId)
            .SingleAsync();

        if (userModel == null)
        {
            throw new UserFriendlyException("数据不存在！");
        }

        if (!string.IsNullOrWhiteSpace(input.Mobile) && userModel.Mobile != input.Mobile)
        {
            // 检查手机号是否已存在账号
            if (await _repository.AnyAsync(a => a.Mobile == input.Mobile && a.UserId != userModel.UserId))
            {
                throw new UserFriendlyException("该手机号已被其他用户绑定，请更换手机号！");
            }

            userModel.Mobile = input.Mobile;
            userModel.MobileUpdateTime = DateTime.Now;
        }

        userModel.NickName = input.NickName;
        userModel.Avatar = input.Avatar;
        userModel.Sex = input.Sex;
        userModel.RowVersion = input.RowVersion;

        await _repository.UpdateAsync(userModel);

        // 刷新缓存
        await _user.RefreshClientUser(new RefreshClientUserDto
        {
            DeviceType = _user.DeviceType,
            AppNo = _user.AppNo,
            Mobile = userModel.Mobile,
            NickName = userModel.NickName,
            Avatar = userModel.Avatar,
            TenantNo = _user.TenantNo
        });
    }
}
