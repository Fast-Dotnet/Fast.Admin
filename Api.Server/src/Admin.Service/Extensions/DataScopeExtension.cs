// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Fast.Admin.Domain;

namespace Fast.Admin.Service;

/// <summary>
/// <see cref="ISugarQueryable{T}"/> 数据权限扩展方法
/// </summary>
public static class DataScopeExtension
{
    /// <summary>
    /// 数据权限过滤
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="queryable">待应用数据范围的查询对象</param>
    /// <param name="departmentIdFieldSelector">部门Id过滤字段</param>
    /// <param name="userIdFieldSelector">用户Id过滤字段</param>
    /// <param name="menuCode">菜单编码</param>
    /// <param name="allowPublicData">允许公开数据</param>
    /// <returns>应用数据权限过滤后的查询对象</returns>
    public static ISugarQueryable<TEntity> DataScope<TEntity>(this ISugarQueryable<TEntity> queryable,
        Expression<Func<TEntity, long?>> departmentIdFieldSelector = null,
        Expression<Func<TEntity, long?>> userIdFieldSelector = null, string menuCode = null, bool allowPublicData = true)
        where TEntity : class, new()
    {
        IUser _user = FastContext.GetService<IUser>();

        // 超级管理员直接跳过
        if (_user.IsSuperAdmin || _user.IsAdmin)
        {
            return queryable;
        }

        // 菜单权限检测
        if (!string.IsNullOrWhiteSpace(menuCode) && !_user.MenuCodeList.Contains(menuCode))
        {
            throw new UserFriendlyException("无权限操作！", HttpStatusCode.Forbidden);
        }

        // 管理员跳过数据权限检测
        if (_user.IsAdmin)
        {
            return queryable;
        }

        // 全部数据
        if (_user.DataScopeType == DataScopeTypeEnum.All)
        {
            return queryable;
        }

        // 多个自定义部门角色取部门Id并集，供其他数据范围分支合并使用
        var departmentIds = (_user.DataScopeDepartmentIdList ?? []).Distinct().ToList();

        Type entityType = typeof(TEntity);
        if (departmentIdFieldSelector == null)
        {
            // 未指定部门字段时，按实体约定使用 DepartmentId
            PropertyInfo property = entityType.GetProperty(nameof(IBaseEntity.DepartmentId),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                throw new NullReferenceException($"【{nameof(IBaseEntity.DepartmentId)}】不存在类型中！");
            }

            ParameterExpression parameter = Expression.Parameter(entityType);
            MemberExpression memberExpression = Expression.Property(parameter, property);
            UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(long?));
            departmentIdFieldSelector = Expression.Lambda<Func<TEntity, long?>>(unaryExpression, parameter);
        }

        if (userIdFieldSelector == null)
        {
            // 未指定用户字段时，按实体约定使用 CreatedUserId
            PropertyInfo property = entityType.GetProperty(nameof(IBaseEntity.CreatedUserId),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                throw new NullReferenceException($"【{nameof(IBaseEntity.CreatedUserId)}】不存在类型中！");
            }

            ParameterExpression parameter = Expression.Parameter(entityType);
            MemberExpression memberExpression = Expression.Property(parameter, property);
            UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(long?));
            userIdFieldSelector = Expression.Lambda<Func<TEntity, long?>>(unaryExpression, parameter);
        }

        // 职员Id
        long employeeId = _user.EmployeeId;
        // 部门Id
        long departmentId = _user.DepartmentId ?? 0;

        // 仅本人数据
        if (_user.DataScopeType == DataScopeTypeEnum.Self)
        {
            // 本人数据条件
            Expression<Func<TEntity, bool>> expression = BuildEqualExpression(userIdFieldSelector, employeeId);
            // 自定义部门补充条件
            Expression<Func<TEntity, bool>> departmentExpression =
                BuildContainsExpression(departmentIdFieldSelector, departmentIds);
            // 本人范围与各角色配置的自定义部门范围取并集
            return queryable.Where(Expressionable.Create<TEntity>().Or(expression).Or(departmentExpression).ToExpression());
        }

        // 本部门数据
        if (_user.DataScopeType == DataScopeTypeEnum.Dept)
        {
            // 当前部门条件
            Expression<Func<TEntity, bool>> expression = BuildEqualExpression(departmentIdFieldSelector, departmentId);
            // 自定义部门补充条件
            Expression<Func<TEntity, bool>> departmentExpression =
                BuildContainsExpression(departmentIdFieldSelector, departmentIds);
            // 当前部门范围与各角色配置的自定义部门范围取并集
            return queryable.Where(Expressionable.Create<TEntity>().Or(expression).Or(departmentExpression).ToExpression());
        }

        // 本机构及以下数据
        if (_user.DataScopeType == DataScopeTypeEnum.OrgWithChild)
        {
            // 公开部门、当前职员主机构下部门及自定义部门共同组成可访问部门范围
            ISugarQueryable<DepartmentModel> dataScopeQueryable = queryable.Context.Queryable<DepartmentModel>()
                .Where(Expressionable.Create<DepartmentModel>()
                    .OrIF(allowPublicData, e => e.DataPublic)
                    .Or(wh => wh.OrgId
                              == SqlFunc.Subqueryable<EmployeeOrgModel>()
                                  // 主部门
                                  .Where(e => e.EmployeeId == employeeId && e.IsPrimary)
                                  .Where(e => e.OrgId == wh.OrgId)
                                  .Select(e => e.OrgId))
                    .Or(wh => departmentIds.Contains(wh.DepartmentId))
                    .ToExpression())
                .Select(sl => new DepartmentModel {DepartmentId = sl.DepartmentId});

            return BuildInnerJoin(queryable, departmentIdFieldSelector, dataScopeQueryable);
        }

        // 本部门及以下数据
        if (_user.DataScopeType == DataScopeTypeEnum.DeptWithChild)
        {
            // 公开部门、当前部门及其子部门、自定义部门共同组成可访问部门范围
            ISugarQueryable<DepartmentModel> dataScopeQueryable = queryable.Context.Queryable<DepartmentModel>()
                .Where(Expressionable.Create<DepartmentModel>()
                    .OrIF(allowPublicData, e => e.DataPublic)
                    .Or(wh => wh.DepartmentId == departmentId)
                    .Or(wh => SqlFunc.JsonArrayAny(wh.ParentIds, departmentId))
                    .Or(wh => departmentIds.Contains(wh.DepartmentId))
                    .ToExpression())
                .Select(sl => new DepartmentModel {DepartmentId = sl.DepartmentId});

            return BuildInnerJoin(queryable, departmentIdFieldSelector, dataScopeQueryable);
        }

        // 自定义部门数据
        if (_user.DataScopeType == DataScopeTypeEnum.CustomDept)
        {
            Expression<Func<TEntity, bool>> expression = BuildContainsExpression(departmentIdFieldSelector, departmentIds);

            return queryable.Where(expression);
        }

        // 未配置或未知的数据范围一律不返回数据，避免错误配置扩大权限
        return queryable.Where(_ => false);
    }

    /// <summary>
    /// 构造字段等于指定Id的条件
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildEqualExpression<TEntity>(Expression<Func<TEntity, long?>> fieldSelector,
        long? value)
    {
        ParameterExpression parameter = fieldSelector.Parameters[0];
        Expression operand = fieldSelector.Body is UnaryExpression unary ? unary.Operand : fieldSelector.Body;

        BinaryExpression equal =
            Expression.Equal(Expression.Convert(operand, typeof(long?)), Expression.Constant(value, typeof(long?)));

        return Expression.Lambda<Func<TEntity, bool>>(equal, parameter);
    }

    /// <summary>
    /// 构造字段 IN 指定Id集合的条件
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity>(
        Expression<Func<TEntity, long?>> fieldSelector, IEnumerable<long> ids)
    {
        ParameterExpression parameter = fieldSelector.Parameters[0];
        Expression operand = fieldSelector.Body is UnaryExpression unary ? unary.Operand : fieldSelector.Body;

        var nullableIds = ids.Select(id => (long?)id).ToList();

        MethodCallExpression contains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(long?)],
            Expression.Constant(nullableIds), Expression.Convert(operand, typeof(long?)));

        return Expression.Lambda<Func<TEntity, bool>>(contains, parameter);
    }

    /// <summary>
    /// 构建 InnerJoin 表达式
    /// </summary>
    private static ISugarQueryable<TEntity> BuildInnerJoin<TEntity>(ISugarQueryable<TEntity> queryable,
        Expression<Func<TEntity, long?>> departmentIdFieldSelector, ISugarQueryable<DepartmentModel> dataScopeQueryable)
        where TEntity : class, new()
    {
        // 获取业务实体的部门Id字段表达式
        ParameterExpression leftParameter = departmentIdFieldSelector.Parameters[0];
        Expression leftUnaryOperand = departmentIdFieldSelector.Body is UnaryExpression leftUnary
            ? leftUnary.Operand
            : departmentIdFieldSelector.Body;

        // 构造可访问部门查询的 DepartmentId 字段表达式
        ParameterExpression rightParameter = Expression.Parameter(typeof(DepartmentModel), "tDS");
        MemberExpression rightProperty = Expression.Property(rightParameter, nameof(DepartmentModel.DepartmentId));

        // 通过部门Id相等条件关联业务数据与可访问部门范围
        BinaryExpression equal = Expression.Equal(Expression.Convert(leftUnaryOperand, typeof(long?)),
            Expression.Convert(rightProperty, typeof(long?)));
        var joinLambda = Expression.Lambda<Func<TEntity, DepartmentModel, bool>>(equal, leftParameter, rightParameter);

        return queryable.InnerJoin(dataScopeQueryable, joinLambda);
    }
}
