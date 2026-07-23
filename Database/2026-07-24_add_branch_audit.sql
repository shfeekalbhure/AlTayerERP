-- ============================================================================
-- AlTayerERP | تدقيق وحالة الفروع Tenant_Branches
-- التاريخ: 2026-07-24
-- الغرض: تطبيق الإيقاف وإعادة التفعيل دون حذف، مع تدقيق مصدره الخادم.
-- ملاحظة: تطبق مرة واحدة ضمن EF Core Migration/نشر قاعدة البيانات المعتمد.
-- ============================================================================

ALTER TABLE tenant_branches
    ADD COLUMN Created_By INT NULL AFTER Created_Date,
    ADD COLUMN Updated_By INT NULL AFTER Updated_Date,
    ADD COLUMN Edit_Count INT NOT NULL DEFAULT 0 AFTER Updated_By,
    ADD COLUMN Stopped_By INT NULL AFTER Edit_Count,
    ADD COLUMN Stopped_At DATETIME NULL AFTER Stopped_By,
    ADD COLUMN Stopped_Reason VARCHAR(500) NULL AFTER Stopped_At,
    ADD COLUMN Reactivated_By INT NULL AFTER Stopped_Reason,
    ADD COLUMN Reactivated_At DATETIME NULL AFTER Reactivated_By,
    ADD COLUMN Reactivate_Reason VARCHAR(500) NULL AFTER Reactivated_At;

-- يمنع تكرار كود الفرع داخل الشركة، ويخدم البحث حسب الحالة والتبعية الشجرية.
CREATE UNIQUE INDEX UQ_Tenant_Branches_Company_Code
    ON tenant_branches (Company_ID, Branch_Code);

CREATE INDEX IX_Tenant_Branches_Company_Active
    ON tenant_branches (Company_ID, Is_Active);

CREATE INDEX IX_Tenant_Branches_Parent_Active
    ON tenant_branches (Parent_Branch_ID, Is_Active);
