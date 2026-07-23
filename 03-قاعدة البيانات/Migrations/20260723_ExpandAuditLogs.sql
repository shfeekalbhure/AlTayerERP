-- المرحلة الأولى / توسيع سجل التدقيق المركزي
-- Migration: 20260723_ExpandAuditLogs.sql
-- ينفذ مرة واحدة على قاعدة تحتوي جدول audit_logs الحالي.

ALTER TABLE audit_logs
    ADD COLUMN Company_ID VARCHAR(50) NULL AFTER User_ID,
    ADD COLUMN Screen_Code VARCHAR(100) NULL AFTER Branch_ID,
    ADD COLUMN Result_Status VARCHAR(40) NOT NULL DEFAULT 'SUCCESS' AFTER Screen_Code,
    ADD COLUMN Correlation_ID VARCHAR(100) NULL AFTER Result_Status,
    ADD COLUMN Is_Offline TINYINT(1) NOT NULL DEFAULT 0 AFTER Correlation_ID,
    ADD COLUMN Sync_Batch_ID VARCHAR(100) NULL AFTER Is_Offline,
    ADD INDEX IX_AuditLogs_ActionAt (Action_At),
    ADD INDEX IX_AuditLogs_TableRecord (Table_Name, Record_ID),
    ADD INDEX IX_AuditLogs_UserActionAt (User_ID, Action_At);