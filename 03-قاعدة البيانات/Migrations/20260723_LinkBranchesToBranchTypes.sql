-- المرحلة الأولى / إلزام مرجع نوع الفرع
-- Migration: 20260723_LinkBranchesToBranchTypes.sql
-- نفّذ بعد 20260723_AddBranchTypesAndWorkCenters.sql.

START TRANSACTION;

ALTER TABLE tenant_branches
    ADD COLUMN Branch_Type_ID INT UNSIGNED NULL AFTER Branch_Type,
    ADD INDEX IX_TenantBranches_BranchType (Branch_Type_ID),
    ADD CONSTRAINT FK_TenantBranches_BranchType
        FOREIGN KEY (Branch_Type_ID) REFERENCES branch_types(Branch_Type_ID) ON DELETE RESTRICT;

COMMIT;

-- خطة ترحيل لازمة قبل جعل الحقل NOT NULL:
-- 1) أضف أنواع الفروع المرجعية.
-- 2) حدّث كل فرع قائم بقيمة Branch_Type_ID الصحيحة.
-- 3) تحقق: SELECT * FROM tenant_branches WHERE Branch_Type_ID IS NULL;
-- 4) في إصدار لاحق: ALTER TABLE tenant_branches MODIFY Branch_Type_ID INT UNSIGNED NOT NULL;