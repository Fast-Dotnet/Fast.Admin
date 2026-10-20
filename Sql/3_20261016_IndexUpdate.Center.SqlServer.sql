-- Fast.Admin：Center 中心库索引更新
-- 执行顺序 3（已按最新 Model 初始化的库）：跳过 1 / 2，执行 3 Center 索引 -> 4 Admin 索引 -> 5 Center 菜单。
-- 旧库完整升级走 1 -> 2 -> 5，已经执行 1 时跳过本文件。
-- 在实际中心库的新查询窗口执行；默认表和字段已按当前 Model 完成创建、改名。
-- 1_20261016_EntityMigration.Center.SqlServer.sql 已包含这些索引；执行过完整迁移后无须重复执行本文件。
-- 先删除已知旧索引，再按当前定义重建；同名索引已存在也会更新。
-- 仅更新索引，不创建表、不调整字段、不合并或删除数据。
-- 若已有数据违反新唯一约束，创建索引将报错，整个事务回滚；应先人工处理重复数据。
-- 在维护窗口停止相关写入并备份后执行；本文件未连接或操作实际数据库。

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

IF @@TRANCOUNT <> 0
    THROW 51000, N'请在没有外部事务的连接中执行本脚本。', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- ApplicationClientModel：应用标识唯一，清理改名前的索引名称。
    DROP INDEX IF EXISTS IX_ApplicationOpenId_OpenId ON dbo.ApplicationClient;
    DROP INDEX IF EXISTS UX_ApplicationOpenId_OpenId ON dbo.ApplicationClient;
    DROP INDEX IF EXISTS UX_ApplicationClient_AppKey ON dbo.ApplicationClient;
    CREATE UNIQUE INDEX UX_ApplicationClient_AppKey ON dbo.ApplicationClient(AppKey);

    -- ApplicationTokenModel：同一应用每种凭据类型仅一条。
    DROP INDEX IF EXISTS UX_ApplicationToken_AppKey ON dbo.ApplicationToken;
    CREATE UNIQUE INDEX UX_ApplicationToken_AppKey ON dbo.ApplicationToken(AppKey, TokenType);

    -- MessageTemplateModel：同一应用标识、同一模板用途仅一条。
    DROP INDEX IF EXISTS IX_ApplicationTemplateId_OpenId ON dbo.MessageTemplate;
    DROP INDEX IF EXISTS IX_ApplicationTemplateId_TemplateId ON dbo.MessageTemplate;
    DROP INDEX IF EXISTS UX_ApplicationTemplateId_TemplateId ON dbo.MessageTemplate;
    DROP INDEX IF EXISTS IX_MessageTemplate_AppKey ON dbo.MessageTemplate;
    DROP INDEX IF EXISTS UX_MessageTemplate_TemplateCode ON dbo.MessageTemplate;
    DROP INDEX IF EXISTS UX_MessageTemplate_TemplateType ON dbo.MessageTemplate;
    CREATE UNIQUE INDEX UX_MessageTemplate_TemplateType ON dbo.MessageTemplate(AppKey, TemplateType);

    -- ClientUserModel：拆分原三列唯一键；仅约束已填写的身份值。
    DROP INDEX IF EXISTS IX_ClientUser_Identity ON dbo.ClientUser;
    DROP INDEX IF EXISTS UX_ClientUser_Identity ON dbo.ClientUser;
    DROP INDEX IF EXISTS UX_ClientUser_OpenId ON dbo.ClientUser;
    DROP INDEX IF EXISTS UX_ClientUser_Mobile ON dbo.ClientUser;
    CREATE UNIQUE INDEX UX_ClientUser_OpenId ON dbo.ClientUser(AppId, OpenId)
        WHERE OpenId IS NOT NULL AND OpenId <> N'';
    CREATE UNIQUE INDEX UX_ClientUser_Mobile ON dbo.ClientUser(AppId, Mobile)
        WHERE Mobile IS NOT NULL AND Mobile <> '';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
