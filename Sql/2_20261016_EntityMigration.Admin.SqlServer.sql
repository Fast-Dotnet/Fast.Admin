-- Fast.Admin：Admin 业务库 Entity 变更的 SQL Server 迁移脚本
-- 执行顺序 2（旧库完整升级）：先执行 1 Center 实体，再对各 Admin 库执行本文件，跳过 3 / 4，最后执行 5 Center 菜单。
-- 已按最新 Model 初始化的数据库不执行 1 / 2，改为 3 Center 索引 -> 4 Admin 索引 -> 5 Center 菜单。
-- 选择实际 Admin 业务库，新建查询窗口后执行本文件；每个独立租户业务库分别执行。
-- 在维护窗口停止相关写入、备份数据库后执行；整个脚本应在同一连接内执行。
-- 重建机构内部门名称唯一索引，并删除 5 张表随 BaseEntity 移除的通用部门审计字段。
-- Employee / Department 改为继承 BaseEntity 后，已有审计列继续保留，无须重建。
-- Department / EmployeeOrg 的业务部门字段保留；AdminLog 日志库无须执行本文件。
-- PagedOutput 删除 DepartmentName 仅影响 DTO，不对应额外的数据库字段变更。
-- 本文件已包含部门名称索引更新，无须再执行 4_20261016_IndexUpdate.Admin.SqlServer.sql。
-- 若表结构已与当前 Model 一致，仅需调整索引，执行 IndexUpdate 文件即可。
-- 仅适用于默认 dbo 架构；非预期索引、外键等依赖不会自动删除。
-- 本脚本仅生成供人工审核，没有连接或操作实际数据库。

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF @@TRANCOUNT <> 0
    THROW 51000, N'请在没有外部事务的连接中执行本脚本。', 1;

IF OBJECT_ID(N'dbo.Department', N'U') IS NULL
    OR OBJECT_ID(N'dbo.EmployeeOrg', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Role', N'U') IS NULL
    THROW 51000, N'当前数据库不符合 Admin 业务库结构，请检查连接目标。', 1;

DECLARE @DepartmentTables TABLE (TableName sysname PRIMARY KEY);
INSERT INTO @DepartmentTables VALUES
    (N'JobLevel'), (N'Organization'), (N'Position'), (N'Role'), (N'SerialRule');

DECLARE @Sql nvarchar(max);
DECLARE @ObjectName nvarchar(517);
DECLARE @TableName sysname;
DECLARE @ColumnName sysname;
DECLARE @DefaultConstraint sysname;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1 FROM dbo.Department
        GROUP BY OrgId, DepartmentName HAVING COUNT_BIG(*) > 1
    )
        THROW 51000, N'同一机构下存在重复部门名称，请先人工处理重复数据。', 1;

    -- 索引名称沿用实体配置，更新索引列；兼容旧 IX 前缀及已执行过的迁移。
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Department')
        AND name = N'IX_Department_DepartmentName')
        DROP INDEX IX_Department_DepartmentName ON dbo.Department;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Department')
        AND name = N'UX_Department_DepartmentName')
        DROP INDEX UX_Department_DepartmentName ON dbo.Department;
    CREATE UNIQUE INDEX UX_Department_DepartmentName ON dbo.Department(OrgId, DepartmentName);

    DECLARE columnDropCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT t.TableName, c.ColumnName FROM @DepartmentTables t
        CROSS JOIN (VALUES (N'DepartmentId'), (N'DepartmentName')) c(ColumnName);
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

    COMMIT TRANSACTION;
    SELECT DB_NAME() AS DatabaseName, N'Admin' AS DatabaseRole, N'迁移完成' AS Result;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
