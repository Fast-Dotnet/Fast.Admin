-- Fast.Admin：Center 中心库 Entity 变更的 SQL Server 迁移脚本
-- 执行顺序 1（旧库完整升级）：1 Center 实体 -> 2 Admin 实体 -> 跳过 3 / 4 -> 5 Center 菜单 -> 5 输出的 AdminSql。
-- 已按最新 Model 初始化的数据库不执行 1 / 2，改为 3 Center 索引 -> 4 Admin 索引 -> 5 Center 菜单。
-- 本脚本仅生成供人工审核，没有连接或操作实际数据库。
-- 在维护窗口停止相关写入、备份数据库后执行；整个脚本应在同一连接内执行。
-- 选择实际 Center 中心库，新建查询窗口后执行本文件；不切换数据库，不需要选择库类型。
-- Admin 业务库请单独执行 2_20261016_EntityMigration.Admin.SqlServer.sql。
-- CenterLog / AdminLog 无须删除部门列，已有日志表和历史分表继续保留部门信息。
-- 按当前暂存 Model 与 Fast.SqlSugar 3.5.68 整理，兼容旧表及此前迁移过的表结构。
-- MessageTemplate 移除 AppId；ApplicationToken 移除 RowVersion，刷新及过期时间允许为空。
-- ClientUser 身份字段允许为空，通过过滤唯一索引仅约束已填写的值。
-- 本文件已包含相关索引更新，无须再执行 3_20261016_IndexUpdate.Center.SqlServer.sql。
-- 若表结构已与当前 Model 一致，仅需调整索引，执行 IndexUpdate 文件即可。
-- 仅适用于默认 dbo 架构；自定义表名、索引或外键应先人工核对。

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @TemplateTypeMap TABLE
(
    RecordId bigint NOT NULL PRIMARY KEY,
    TemplateType bigint NOT NULL
);

-- 旧 ApplicationTemplateTypeEnum 只有 None = 0，不能推断其业务含义。
-- 如旧模板存在 0 或其他未定义值，请按实际用途在这里填写记录级映射。
-- 新值：1 注册，2 登录，3 密码操作，4 手机号验证，5 注销账号，101 邮箱验证。
-- INSERT INTO @TemplateTypeMap (RecordId, TemplateType) VALUES (<实际记录Id>, <新类型>);
-- 未填写映射且仍存在未定义类型时，中心库迁移将回滚，并仅返回记录Id及类型。

IF @@TRANCOUNT <> 0
    THROW 51000, N'请在没有外部事务的连接中执行本脚本。', 1;

DECLARE @Sql nvarchar(max);
DECLARE @ObjectName nvarchar(517);
DECLARE @TableName sysname;
DECLARE @OldName sysname;
DECLARE @NewName sysname;
DECLARE @DefaultConstraint sysname;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Application', N'U') IS NULL
        OR OBJECT_ID(N'dbo.Tenant', N'U') IS NULL
        OR OBJECT_ID(N'dbo.DatabaseMain', N'U') IS NULL
        OR OBJECT_ID(N'dbo.ClientUser', N'U') IS NULL
        THROW 51000, N'当前数据库不符合 Center 库结构，请检查连接目标。', 1;

    -- 不自动合并或删除重复身份，避免改变用户归属。
    IF EXISTS
    (
        SELECT 1 FROM dbo.ClientUser WHERE OpenId IS NOT NULL AND OpenId <> N''
        GROUP BY AppId, OpenId HAVING COUNT_BIG(*) > 1
    )
        THROW 51000, N'同一应用下存在重复的非空 OpenId，请先人工处理重复用户。', 1;
    IF EXISTS
    (
        SELECT 1 FROM dbo.ClientUser WHERE Mobile IS NOT NULL AND Mobile <> ''
        GROUP BY AppId, Mobile HAVING COUNT_BIG(*) > 1
    )
        THROW 51000, N'同一应用下存在重复的非空手机号，请先人工处理重复用户。', 1;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ClientUser')
        AND name = N'IX_ClientUser_Identity')
        DROP INDEX IX_ClientUser_Identity ON dbo.ClientUser;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ClientUser')
        AND name = N'UX_ClientUser_Identity')
        DROP INDEX UX_ClientUser_Identity ON dbo.ClientUser;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ClientUser')
        AND name = N'UX_ClientUser_OpenId')
        DROP INDEX UX_ClientUser_OpenId ON dbo.ClientUser;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ClientUser')
        AND name = N'UX_ClientUser_Mobile')
        DROP INDEX UX_ClientUser_Mobile ON dbo.ClientUser;
    CREATE UNIQUE INDEX UX_ClientUser_OpenId ON dbo.ClientUser(AppId, OpenId)
        WHERE OpenId IS NOT NULL AND OpenId <> N'';
    CREATE UNIQUE INDEX UX_ClientUser_Mobile ON dbo.ClientUser(AppId, Mobile)
        WHERE Mobile IS NOT NULL AND Mobile <> '';

    -- 保留原表数据和主键，不能依赖 CodeFirst 自动识别重命名。
    DECLARE @TableRenames TABLE (OldName sysname, NewName sysname);
    INSERT INTO @TableRenames VALUES
        (N'ApplicationOpenId', N'ApplicationClient'),
        (N'ApplicationTemplateId', N'MessageTemplate');

    DECLARE tableRenameCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT OldName, NewName FROM @TableRenames;
    OPEN tableRenameCursor;
    FETCH NEXT FROM tableRenameCursor INTO @OldName, @NewName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF OBJECT_ID(N'dbo.' + QUOTENAME(@OldName), N'U') IS NOT NULL
        BEGIN
            IF OBJECT_ID(N'dbo.' + QUOTENAME(@NewName), N'U') IS NOT NULL
                THROW 51000, N'新旧表同时存在，请先人工核对并合并数据。', 1;
            SET @ObjectName = N'dbo.' + QUOTENAME(@OldName);
            EXEC sys.sp_rename @objname = @ObjectName, @newname = @NewName, @objtype = N'OBJECT';
        END
        ELSE IF OBJECT_ID(N'dbo.' + QUOTENAME(@NewName), N'U') IS NULL
            THROW 51000, N'缺少待迁移的应用客户端表或消息模板表。', 1;
        FETCH NEXT FROM tableRenameCursor INTO @OldName, @NewName;
    END;
    CLOSE tableRenameCursor;
    DEALLOCATE tableRenameCursor;

    DECLARE @ColumnRenames TABLE (TableName sysname, OldName sysname, NewName sysname);
    INSERT INTO @ColumnRenames VALUES
        (N'ApplicationClient', N'OpenId', N'AppKey'),
        (N'ApplicationClient', N'OpenSecret', N'AppSecret'),
        (N'MessageTemplate', N'OpenId', N'AppKey'),
        (N'MessageTemplate', N'TemplateId', N'TemplateCode'),
        (N'Complaint', N'OpenId', N'AppKey'),
        (N'PayRecord', N'AppOpenId', N'AppKey'),
        (N'RefundRecord', N'AppOpenId', N'AppKey');

    -- PayRecord / RefundRecord / ClientUser 的用户 OpenId 均不重命名。
    DECLARE columnRenameCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName, OldName, NewName FROM @ColumnRenames;
    OPEN columnRenameCursor;
    FETCH NEXT FROM columnRenameCursor INTO @TableName, @OldName, @NewName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ObjectName = N'dbo.' + QUOTENAME(@TableName);
        IF COL_LENGTH(@ObjectName, @OldName) IS NOT NULL
        BEGIN
            IF COL_LENGTH(@ObjectName, @NewName) IS NOT NULL
                THROW 51000, N'新旧字段同时存在，请先人工核对数据。', 1;
            SET @ObjectName = @ObjectName + N'.' + QUOTENAME(@OldName);
            EXEC sys.sp_rename @objname = @ObjectName, @newname = @NewName, @objtype = N'COLUMN';
        END
        ELSE IF COL_LENGTH(@ObjectName, @NewName) IS NULL
            THROW 51000, N'缺少待迁移的应用标识或模板字段。', 1;
        FETCH NEXT FROM columnRenameCursor INTO @TableName, @OldName, @NewName;
    END;
    CLOSE columnRenameCursor;
    DEALLOCATE columnRenameCursor;

    -- 移除旧名字的索引；兼容此前使用 IX 前缀的唯一索引。
    DECLARE @OldIndexes TABLE (TableName sysname, IndexName sysname);
    INSERT INTO @OldIndexes VALUES
        (N'ApplicationClient', N'UX_ApplicationOpenId_OpenId'),
        (N'ApplicationClient', N'IX_ApplicationOpenId_OpenId'),
        (N'ApplicationClient', N'UX_ApplicationClient_AppKey'),
        (N'ApplicationToken', N'UX_ApplicationToken_AppKey'),
        (N'MessageTemplate', N'IX_ApplicationTemplateId_OpenId'),
        (N'MessageTemplate', N'UX_ApplicationTemplateId_TemplateId'),
        (N'MessageTemplate', N'IX_ApplicationTemplateId_TemplateId'),
        (N'MessageTemplate', N'IX_MessageTemplate_AppKey'),
        (N'MessageTemplate', N'UX_MessageTemplate_TemplateCode'),
        (N'MessageTemplate', N'UX_MessageTemplate_TemplateType');
    DECLARE @IndexName sysname;
    DECLARE indexDropCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName, IndexName FROM @OldIndexes;
    OPEN indexDropCursor;
    FETCH NEXT FROM indexDropCursor INTO @TableName, @IndexName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ObjectName = N'dbo.' + QUOTENAME(@TableName);
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(@ObjectName) AND name = @IndexName)
        BEGIN
            SET @Sql = N'DROP INDEX ' + QUOTENAME(@IndexName) + N' ON ' + @ObjectName + N';';
            EXEC sys.sp_executesql @Sql;
        END;
        FETCH NEXT FROM indexDropCursor INTO @TableName, @IndexName;
    END;
    CLOSE indexDropCursor;
    DEALLOCATE indexDropCursor;

    -- 重命名后的语句独立编译，避免同一批次提前绑定旧列名。
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.MessageTemplate ALTER COLUMN AppKey nvarchar(50) NULL;
        IF COL_LENGTH(N''dbo.MessageTemplate'', N''RowVersion'') IS NULL
        BEGIN
            ALTER TABLE dbo.MessageTemplate ADD RowVersion bigint NOT NULL
                CONSTRAINT DF_MessageTemplate_RowVersion_Migration DEFAULT (1) WITH VALUES;
            ALTER TABLE dbo.MessageTemplate DROP CONSTRAINT DF_MessageTemplate_RowVersion_Migration;
        END;
        -- 同名索引也重建，保证最终列定义与当前 Model 一致。
        CREATE UNIQUE INDEX UX_ApplicationClient_AppKey ON dbo.ApplicationClient(AppKey);
    ';

    IF EXISTS (SELECT 1 FROM @TemplateTypeMap WHERE TemplateType NOT IN (1, 2, 3, 4, 5, 101))
        THROW 51000, N'模板类型映射只能使用当前枚举的 1、2、3、4、5、101。', 1;
    DECLARE @TemplateRecordId bigint;
    DECLARE @TemplateType bigint;
    DECLARE templateTypeCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT RecordId, TemplateType FROM @TemplateTypeMap;
    OPEN templateTypeCursor;
    FETCH NEXT FROM templateTypeCursor INTO @TemplateRecordId, @TemplateType;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC sys.sp_executesql N'
            UPDATE dbo.MessageTemplate SET TemplateType = @Type WHERE RecordId = @Id;
            IF @@ROWCOUNT <> 1 THROW 51000, N''模板类型映射包含不存在的记录Id。'', 1;
        ', N'@Type bigint, @Id bigint', @Type = @TemplateType, @Id = @TemplateRecordId;
        FETCH NEXT FROM templateTypeCursor INTO @TemplateRecordId, @TemplateType;
    END;
    CLOSE templateTypeCursor;
    DEALLOCATE templateTypeCursor;
    EXEC sys.sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.MessageTemplate WHERE TemplateType IS NULL OR TemplateType NOT IN (1, 2, 3, 4, 5, 101))
        BEGIN
            SELECT RecordId, TemplateType FROM dbo.MessageTemplate
                WHERE TemplateType IS NULL OR TemplateType NOT IN (1, 2, 3, 4, 5, 101);
            THROW 51000, N''仍存在未映射的旧模板类型，请填写脚本开头的记录级映射。'', 1;
        END;
        ALTER TABLE dbo.MessageTemplate ALTER COLUMN TemplateType bigint NOT NULL;
        -- 先完成旧类型映射，再校验新唯一键，避免旧 None = 0 记录提前冲突。
        CREATE UNIQUE INDEX UX_MessageTemplate_TemplateType
            ON dbo.MessageTemplate(AppKey, TemplateType);
    ';

    IF OBJECT_ID(N'dbo.ApplicationToken', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ApplicationToken
        (
            RecordId bigint NOT NULL CONSTRAINT PK_ApplicationToken PRIMARY KEY,
            AppKey nvarchar(50) NOT NULL,
            TokenType tinyint NOT NULL,
            TokenValue nvarchar(512) NOT NULL,
            RefreshTime datetime NULL,
            ExpireTime datetime NULL
        );
    END;
    -- 兼容此前已创建的凭据表；先移除旧版本列，再迁移凭据。
    IF COL_LENGTH(N'dbo.ApplicationToken', N'RowVersion') IS NOT NULL
    BEGIN
        SET @DefaultConstraint = NULL;
        SELECT @DefaultConstraint = dc.name
        FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.ApplicationToken') AND c.name = N'RowVersion';
        IF @DefaultConstraint IS NOT NULL
        BEGIN
            SET @Sql = N'ALTER TABLE dbo.ApplicationToken DROP CONSTRAINT ' + QUOTENAME(@DefaultConstraint) + N';';
            EXEC sys.sp_executesql @Sql;
        END;
        EXEC sys.sp_executesql N'ALTER TABLE dbo.ApplicationToken DROP COLUMN RowVersion;';
    END;

    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.ApplicationToken ALTER COLUMN RefreshTime datetime NULL;
        ALTER TABLE dbo.ApplicationToken ALTER COLUMN ExpireTime datetime NULL;
        CREATE UNIQUE INDEX UX_ApplicationToken_AppKey ON dbo.ApplicationToken(AppKey, TokenType);
    ';

    DECLARE @LegacyTokenColumnCount int =
    (
        SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ApplicationClient')
            AND name IN (N'WeChatAccessToken', N'WeChatAccessTokenExpiresIn', N'WeChatAccessTokenRefreshTime',
                N'WeChatJsApiTicket', N'WeChatJsApiTicketExpiresIn', N'WeChatJsApiTicketRefreshTime')
    );
    IF @LegacyTokenColumnCount NOT IN (0, 6)
        THROW 51000, N'旧凭据字段不完整，请先人工核对，不能直接删除。', 1;

    IF @LegacyTokenColumnCount = 6
        EXEC sys.sp_executesql N'
            IF EXISTS (SELECT 1 FROM dbo.ApplicationClient WHERE RecordId <= 0)
                THROW 51000, N''旧客户端记录Id必须为正数，才能使用本脚本的凭据主键映射。'', 1;
            DECLARE @LegacyTokens TABLE
            (
                RecordId bigint, AppKey nvarchar(50), TokenType tinyint,
                TokenValue nvarchar(512), RefreshTime datetime, ExpiresIn int
            );
            -- AccessToken 复用原客户端Id；JsApiTicket 使用其相反数。
            -- 负数主键仅用于这次迁移，与运行时生成的正数雪花Id区分。
            INSERT INTO @LegacyTokens
            SELECT RecordId, AppKey, 1, WeChatAccessToken, WeChatAccessTokenRefreshTime, WeChatAccessTokenExpiresIn
            FROM dbo.ApplicationClient WHERE NULLIF(LTRIM(RTRIM(WeChatAccessToken)), N'''') IS NOT NULL
            UNION ALL
            SELECT -RecordId, AppKey, 2, WeChatJsApiTicket, WeChatJsApiTicketRefreshTime, WeChatJsApiTicketExpiresIn
            FROM dbo.ApplicationClient WHERE NULLIF(LTRIM(RTRIM(WeChatJsApiTicket)), N'''') IS NOT NULL;
            IF EXISTS (SELECT 1 FROM @LegacyTokens WHERE RefreshTime IS NULL OR ExpiresIn IS NULL OR ExpiresIn <= 0)
                THROW 51000, N''旧凭据缺少有效刷新时间或有效期，请先修正或明确废弃这些旧值。'', 1;
            IF EXISTS
            (
                SELECT 1 FROM @LegacyTokens s JOIN dbo.ApplicationToken t ON t.RecordId = s.RecordId
                WHERE t.AppKey <> s.AppKey OR t.TokenType <> s.TokenType
            )
                THROW 51000, N''凭据迁移主键与已有记录冲突，请人工指定新的主键。'', 1;
            INSERT INTO dbo.ApplicationToken (RecordId, AppKey, TokenType, TokenValue, RefreshTime, ExpireTime)
            SELECT s.RecordId, s.AppKey, s.TokenType, s.TokenValue, s.RefreshTime,
                DATEADD(SECOND, s.ExpiresIn, s.RefreshTime)
            FROM @LegacyTokens s
            WHERE NOT EXISTS (SELECT 1 FROM dbo.ApplicationToken t WHERE t.AppKey = s.AppKey AND t.TokenType = s.TokenType);
            -- 已存在相同 AppKey + TokenType 的新表记录时，保留新表值，不用旧值覆盖。
            -- 凭据只在数据库内部迁移，脚本不查询输出凭据值或密钥。
        ';

    -- 明确列出本次基类变更影响的表，避免误删真实的业务部门字段。
    DECLARE @DepartmentTables TABLE (TableName sysname PRIMARY KEY);
    INSERT INTO @DepartmentTables VALUES
        (N'ApplicationClient'), (N'Application'), (N'Button'), (N'Config'),
        (N'DatabaseMain'), (N'DatabaseSlave'), (N'DictionaryItem'), (N'DictionaryType'),
        (N'Menu'), (N'Merchant'), (N'MessageSendRecord'), (N'MessageTemplate'),
        (N'Region'), (N'SysSerialRule'), (N'TableConfig'), (N'Tenant');

    DECLARE @DropColumns TABLE (TableName sysname, ColumnName sysname);
    INSERT INTO @DropColumns
        SELECT t.TableName, c.ColumnName FROM @DepartmentTables t
        CROSS JOIN (VALUES (N'DepartmentId'), (N'DepartmentName')) c(ColumnName);
    INSERT INTO @DropColumns VALUES
        (N'ApplicationClient', N'WebSocketUrl'),
        (N'ApplicationClient', N'WeChatAccessToken'),
        (N'ApplicationClient', N'WeChatAccessTokenExpiresIn'),
        (N'ApplicationClient', N'WeChatAccessTokenRefreshTime'),
        (N'ApplicationClient', N'WeChatJsApiTicket'),
        (N'ApplicationClient', N'WeChatJsApiTicketExpiresIn'),
        (N'ApplicationClient', N'WeChatJsApiTicketRefreshTime'),
        (N'MessageTemplate', N'AppId'),
        (N'Tenant', N'AllowDeleteData');

    -- 以下删除会清除旧列数据；业务开关 AllowDeleteData 也按暂存模型删除。
    -- 不删除 EmployeeOrg / Department / TenantUser / TenantOnlineUser 的业务部门字段。
    -- 非预期索引、外键等依赖不会自动删除；SQL Server 报错后整段迁移回滚。
    DECLARE @ColumnName sysname;
    DECLARE columnDropCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName, ColumnName FROM @DropColumns;
    OPEN columnDropCursor;
    FETCH NEXT FROM columnDropCursor INTO @TableName, @ColumnName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ObjectName = N'dbo.' + QUOTENAME(@TableName);
        IF COL_LENGTH(@ObjectName, @ColumnName) IS NOT NULL
        BEGIN
            SET @DefaultConstraint = NULL;
            SELECT @DefaultConstraint = dc.name
            FROM sys.default_constraints dc
            JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
            WHERE dc.parent_object_id = OBJECT_ID(@ObjectName) AND c.name = @ColumnName;
            IF @DefaultConstraint IS NOT NULL
            BEGIN
                SET @Sql = N'ALTER TABLE ' + @ObjectName + N' DROP CONSTRAINT ' + QUOTENAME(@DefaultConstraint) + N';';
                EXEC sys.sp_executesql @Sql;
            END;
            SET @Sql = N'ALTER TABLE ' + @ObjectName + N' DROP COLUMN ' + QUOTENAME(@ColumnName) + N';';
            EXEC sys.sp_executesql @Sql;
        END;
        FETCH NEXT FROM columnDropCursor INTO @TableName, @ColumnName;
    END;
    CLOSE columnDropCursor;
    DEALLOCATE columnDropCursor;

    -- 同步当前 Model 中新增或修改的表、字段说明。
    DECLARE @Descriptions TABLE (TableName sysname, ColumnName sysname NULL, Description nvarchar(4000));
    INSERT INTO @Descriptions VALUES
        (N'ApplicationClient', N'AppSecret', N'应用密钥'),
        (N'MessageTemplate', NULL, N'消息模板表'),
        (N'MessageTemplate', N'TemplateCode', N'模板Code'),
        (N'MessageTemplate', N'RowVersion', N'更新版本控制字段'),
        (N'ApplicationToken', NULL, N'应用凭据表'),
        (N'ApplicationToken', N'RecordId', N'记录Id'),
        (N'ApplicationToken', N'AppKey', N'应用标识'),
        (N'ApplicationToken', N'TokenType', N'凭据类型'),
        (N'ApplicationToken', N'TokenValue', N'凭据值'),
        (N'ApplicationToken', N'RefreshTime', N'刷新时间'),
        (N'ApplicationToken', N'ExpireTime', N'过期时间');

    DECLARE @Description nvarchar(4000);
    DECLARE @ColumnId int;
    DECLARE @Level2Type varchar(128);
    DECLARE descriptionCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName, ColumnName, Description FROM @Descriptions;
    OPEN descriptionCursor;
    FETCH NEXT FROM descriptionCursor INTO @TableName, @ColumnName, @Description;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ObjectName = N'dbo.' + QUOTENAME(@TableName);
        SET @ColumnId = CASE WHEN @ColumnName IS NULL THEN 0
            ELSE COLUMNPROPERTY(OBJECT_ID(@ObjectName), @ColumnName, 'ColumnId') END;
        IF @ColumnId IS NULL
            THROW 51000, N'缺少需要更新说明的字段，请先核对表结构。', 1;
        SET @Level2Type = CASE WHEN @ColumnName IS NULL THEN NULL ELSE 'COLUMN' END;

        IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 1
            AND major_id = OBJECT_ID(@ObjectName) AND minor_id = @ColumnId AND name = N'MS_Description')
            EXEC sys.sp_updateextendedproperty @name = N'MS_Description', @value = @Description,
                @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = @TableName,
                @level2type = @Level2Type, @level2name = @ColumnName;
        ELSE
            EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = @Description,
                @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = @TableName,
                @level2type = @Level2Type, @level2name = @ColumnName;

        FETCH NEXT FROM descriptionCursor INTO @TableName, @ColumnName, @Description;
    END;
    CLOSE descriptionCursor;
    DEALLOCATE descriptionCursor;

    COMMIT TRANSACTION;
    SELECT DB_NAME() AS DatabaseName, N'Center' AS DatabaseRole, N'迁移完成' AS Result;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- 已同步新增及修改的表、字段说明，主键约束名称和物理列顺序保持不变。
-- CreateTableFieldSort 仅影响新建表，无须重建旧表或调整日志分表列顺序。
-- 本脚本仅处理 Entity 相关变更，不迁移菜单、权限、缓存或其他运行时状态。
