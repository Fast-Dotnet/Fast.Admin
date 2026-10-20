// Copyright © 2018-Present 小方
// SPDX-License-Identifier: Apache-2.0
// 
// 本文件依据 Apache License 2.0 授权，完整条款见仓库根目录 LICENSE。
// 本软件按“原样”提供，相关免责声明及责任限制以许可证及适用法律为准。
// 版权来源、合法使用与二次开发责任说明见仓库根目录 README.md。

namespace Fast.Core;

/// <summary>
/// 权限常量
/// </summary>
/// <remarks>所有权限都在这里</remarks>
[SuppressSniffer]
public static partial class PermissionConst
{
    /// <summary>客户端服务</summary>
    public const string ClientService = "ClientService";

    /// <summary>接口列表</summary>
    public const string ApiPaged = "Api:Paged";

    /// <summary>Swagger</summary>
    public const string ApiSwagger = "Api:Swagger";

    /// <summary>Knife4j</summary>
    public const string ApiKnife4j = "Api:Knife4j";

    /// <summary>配置</summary>
    public static class Config
    {
        /// <summary>配置列表</summary>
        public const string Paged = "Config:Paged";

        /// <summary>配置详情</summary>
        public const string Detail = "Config:Detail";

        /// <summary>配置编辑</summary>
        public const string Edit = "Config:Edit";
    }

    /// <summary>菜单</summary>
    public static class Menu
    {
        /// <summary>菜单列表</summary>
        public const string Paged = "Menu:Paged";

        /// <summary>菜单详情</summary>
        public const string Detail = "Menu:Detail";

        /// <summary>菜单新增</summary>
        public const string Add = "Menu:Add";

        /// <summary>菜单编辑</summary>
        public const string Edit = "Menu:Edit";

        /// <summary>菜单删除</summary>
        public const string Delete = "Menu:Delete";

        /// <summary>菜单状态更改</summary>
        public const string Status = "Menu:Status";
    }

    /// <summary>系统序号</summary>
    public static class SysSerial
    {
        /// <summary>系统序号列表</summary>
        public const string Paged = "SysSerial:Paged";

        /// <summary>系统序号详情</summary>
        public const string Detail = "SysSerial:Detail";

        /// <summary>系统序号新增</summary>
        public const string Add = "SysSerial:Add";

        /// <summary>系统序号编辑</summary>
        public const string Edit = "SysSerial:Edit";
    }

    /// <summary>字典</summary>
    public static class Dictionary
    {
        /// <summary>字典列表</summary>
        public const string Paged = "Dictionary:Paged";

        /// <summary>字典详情</summary>
        public const string Detail = "Dictionary:Detail";

        /// <summary>字典新增</summary>
        public const string Add = "Dictionary:Add";

        /// <summary>字典编辑</summary>
        public const string Edit = "Dictionary:Edit";

        /// <summary>字典删除</summary>
        public const string Delete = "Dictionary:Delete";
    }

    /// <summary>表格</summary>
    public static class Table
    {
        /// <summary>表格列表</summary>
        public const string Paged = "Table:Paged";

        /// <summary>表格详情</summary>
        public const string Detail = "Table:Detail";

        /// <summary>表格新增</summary>
        public const string Add = "Table:Add";

        /// <summary>表格编辑</summary>
        public const string Edit = "Table:Edit";

        /// <summary>表格删除</summary>
        public const string Delete = "Table:Delete";
    }

    /// <summary>调度程序</summary>
    public static class Scheduler
    {
        /// <summary>调度程序列表</summary>
        public const string Paged = "Scheduler:Paged";

        /// <summary>调度程序详情</summary>
        public const string Detail = "Scheduler:Detail";

        /// <summary>调度程序新增</summary>
        public const string Add = "Scheduler:Add";

        /// <summary>调度程序编辑</summary>
        public const string Edit = "Scheduler:Edit";

        /// <summary>调度程序删除</summary>
        public const string Delete = "Scheduler:Delete";

        /// <summary>调度程序启动</summary>
        public const string Start = "Scheduler:Start";

        /// <summary>调度程序暂停</summary>
        public const string Stop = "Scheduler:Stop";

        /// <summary>调度程序执行作业</summary>
        public const string Trigger = "Scheduler:Trigger";

        /// <summary>调度程序恢复作业</summary>
        public const string ResumeJob = "Scheduler:ResumeJob";

        /// <summary>调度程序暂停作业</summary>
        public const string StopJob = "Scheduler:StopJob";
    }

    /// <summary>密码记录列表</summary>
    public const string PasswordRecordPaged = "PasswordRecord:Paged";

    /// <summary>消息发送记录</summary>
    public static class MessageSendRecord
    {
        /// <summary>消息发送记录列表</summary>
        public const string Paged = "MessageSendRecord:Paged";

        /// <summary>消息发送记录详情</summary>
        public const string Detail = "MessageSendRecord:Detail";
    }

    /// <summary>异常日志列表</summary>
    public const string ExceptionLogPaged = "ExceptionLog:Paged";

    /// <summary>SQL 异常日志列表</summary>
    public const string SqlExceptionLogPaged = "SqlExceptionLog:Paged";

    /// <summary>SQL 超时日志列表</summary>
    public const string SqlTimeoutLogPaged = "SqlTimeoutLog:Paged";

    /// <summary>SQL 执行日志列表</summary>
    public const string SqlExecutionLogPaged = "SqlExecutionLog:Paged";

    /// <summary>SQL 差异日志列表</summary>
    public const string SqlDiffLogPaged = "SqlDiffLog:Paged";


    /// <summary>系统监控</summary>
    public const string SystemMonitor = "System:Monitor";

    /// <summary>文件列表</summary>
    public const string FilePaged = "File:Paged";

    /// <summary>租户</summary>
    public static class Tenant
    {
        /// <summary>租户列表</summary>
        public const string Paged = "Tenant:Paged";

        /// <summary>租户详情</summary>
        public const string Detail = "Tenant:Detail";

        /// <summary>租户新增</summary>
        public const string Add = "Tenant:Add";

        /// <summary>租户编辑</summary>
        public const string Edit = "Tenant:Edit";

        /// <summary>租户状态更改</summary>
        public const string Status = "Tenant:Status";
    }

    /// <summary>数据库</summary>
    public static class Database
    {
        /// <summary>数据库列表</summary>
        public const string Paged = "Database:Paged";

        /// <summary>数据库详情</summary>
        public const string Detail = "Database:Detail";

        /// <summary>数据库新增</summary>
        public const string Add = "Database:Add";

        /// <summary>数据库编辑</summary>
        public const string Edit = "Database:Edit";

        /// <summary>数据库删除</summary>
        public const string Delete = "Database:Delete";
    }

    /// <summary>应用</summary>
    public static class App
    {
        /// <summary>应用列表</summary>
        public const string Paged = "App:Paged";

        /// <summary>应用详情</summary>
        public const string Detail = "App:Detail";

        /// <summary>应用新增</summary>
        public const string Add = "App:Add";

        /// <summary>应用编辑</summary>
        public const string Edit = "App:Edit";

        /// <summary>应用删除</summary>
        public const string Delete = "App:Delete";

        /// <summary>应用标识详情</summary>
        public const string ClientDetail = "AppClient:Detail";

        /// <summary>应用标识新增</summary>
        public const string ClientAdd = "AppClient:Add";

        /// <summary>应用标识编辑</summary>
        public const string ClientEdit = "AppClient:Edit";

        /// <summary>应用标识删除</summary>
        public const string ClientDelete = "AppClient:Delete";
    }

    /// <summary>应用凭据</summary>
    public static class AppToken
    {
        /// <summary>应用凭据列表</summary>
        public const string Paged = "AppToken:Paged";

        /// <summary>应用凭据新增</summary>
        public const string Add = "AppToken:Add";

        /// <summary>应用凭据删除</summary>
        public const string Delete = "AppToken:Delete";

        /// <summary>应用凭据刷新</summary>
        public const string Refresh = "AppToken:Refresh";
    }

    /// <summary>消息模板</summary>
    public static class MessageTemplate
    {
        /// <summary>消息模板列表</summary>
        public const string Paged = "MessageTemplate:Paged";

        /// <summary>消息模板详情</summary>
        public const string Detail = "MessageTemplate:Detail";

        /// <summary>消息模板新增</summary>
        public const string Add = "MessageTemplate:Add";

        /// <summary>消息模板编辑</summary>
        public const string Edit = "MessageTemplate:Edit";

        /// <summary>消息模板删除</summary>
        public const string Delete = "MessageTemplate:Delete";
    }

    /// <summary>账号</summary>
    public static class Account
    {
        /// <summary>账号列表</summary>
        public const string Paged = "Account:Paged";

        /// <summary>账号详情</summary>
        public const string Detail = "Account:Detail";

        /// <summary>账号解除锁定</summary>
        public const string Unlock = "Account:Unlock";

        /// <summary>账号重置密码</summary>
        public const string ResetPassword = "Account:ResetPassword";

        /// <summary>账号状态更改</summary>
        public const string Status = "Account:Status";
    }

    /// <summary>序号</summary>
    public static class Serial
    {
        /// <summary>序号列表</summary>
        public const string Paged = "Serial:Paged";

        /// <summary>序号详情</summary>
        public const string Detail = "Serial:Detail";

        /// <summary>序号新增</summary>
        public const string Add = "Serial:Add";

        /// <summary>序号编辑</summary>
        public const string Edit = "Serial:Edit";
    }

    /// <summary>职位</summary>
    public static class Position
    {
        /// <summary>职位列表</summary>
        public const string Paged = "Position:Paged";

        /// <summary>职位详情</summary>
        public const string Detail = "Position:Detail";

        /// <summary>职位新增</summary>
        public const string Add = "Position:Add";

        /// <summary>职位编辑</summary>
        public const string Edit = "Position:Edit";

        /// <summary>职位删除</summary>
        public const string Delete = "Position:Delete";
    }

    /// <summary>职级</summary>
    public static class JobLevel
    {
        /// <summary>职级列表</summary>
        public const string Paged = "JobLevel:Paged";

        /// <summary>职级详情</summary>
        public const string Detail = "JobLevel:Detail";

        /// <summary>职级新增</summary>
        public const string Add = "JobLevel:Add";

        /// <summary>职级编辑</summary>
        public const string Edit = "JobLevel:Edit";

        /// <summary>职级删除</summary>
        public const string Delete = "JobLevel:Delete";
    }

    /// <summary>角色</summary>
    public static class Role
    {
        /// <summary>角色列表</summary>
        public const string Paged = "Role:Paged";

        /// <summary>角色详情</summary>
        public const string Detail = "Role:Detail";

        /// <summary>角色新增</summary>
        public const string Add = "Role:Add";

        /// <summary>角色编辑</summary>
        public const string Edit = "Role:Edit";

        /// <summary>角色删除</summary>
        public const string Delete = "Role:Delete";
    }

    /// <summary>部门</summary>
    public static class Department
    {
        /// <summary>部门列表</summary>
        public const string Paged = "Department:Paged";

        /// <summary>部门详情</summary>
        public const string Detail = "Department:Detail";

        /// <summary>部门新增</summary>
        public const string Add = "Department:Add";

        /// <summary>部门编辑</summary>
        public const string Edit = "Department:Edit";

        /// <summary>部门删除</summary>
        public const string Delete = "Department:Delete";
    }

    /// <summary>职员</summary>
    public static class Employee
    {
        /// <summary>职员列表</summary>
        public const string Paged = "Employee:Paged";

        /// <summary>职员详情</summary>
        public const string Detail = "Employee:Detail";

        /// <summary>职员新增</summary>
        public const string Add = "Employee:Add";

        /// <summary>职员编辑</summary>
        public const string Edit = "Employee:Edit";

        /// <summary>职员状态更改</summary>
        public const string Status = "Employee:Status";
    }

    /// <summary>在线用户</summary>
    public static class TenantOnlineUser
    {
        /// <summary>在线用户列表</summary>
        public const string Paged = "TenantOnlineUser:Paged";

        /// <summary>强制下线</summary>
        public const string ForceOffline = "TenantOnlineUser:ForceOffline";
    }

    /// <summary>商户号</summary>
    public static class Merchant
    {
        /// <summary>商户号列表</summary>
        public const string Paged = "Merchant:Paged";

        /// <summary>商户号详情</summary>
        public const string Detail = "Merchant:Detail";

        /// <summary>商户号新增</summary>
        public const string Add = "Merchant:Add";

        /// <summary>商户号编辑</summary>
        public const string Edit = "Merchant:Edit";

        /// <summary>商户号删除</summary>
        public const string Delete = "Merchant:Delete";
    }

    /// <summary>支付记录列表</summary>
    public const string PayRecordPaged = "PayRecord:Paged";

    /// <summary>退款记录列表</summary>
    public const string RefundRecordPaged = "RefundRecord:Paged";

    /// <summary>客户端用户</summary>
    public static class ClientUser
    {
        /// <summary>客户端用户列表</summary>
        public const string Paged = "ClientUser:Paged";
    }

    /// <summary>投诉</summary>
    public static class Complaint
    {
        /// <summary>投诉工单列表</summary>
        public const string Paged = "Complaint:Paged";

        /// <summary>投诉工单详情</summary>
        public const string Detail = "Complaint:Detail";

        /// <summary>处理投诉工单</summary>
        public const string Handle = "Complaint:Handle";

        /// <summary>用户投诉列表</summary>
        public const string TenantPaged = "Complaint:TenantPaged";

        /// <summary>用户投诉详情</summary>
        public const string TenantDetail = "Complaint:TenantDetail";

        /// <summary>处理用户投诉</summary>
        public const string TenantHandle = "Complaint:TenantHandle";
    }

    /// <summary>访问日志列表</summary>
    public const string VisitLogPaged = "VisitLog:Paged";

    /// <summary>操作日志列表</summary>
    public const string OperateLogPaged = "OperateLog:Paged";

    /// <summary>请求日志列表</summary>
    public const string RequestLogPaged = "RequestLog:Paged";
}
