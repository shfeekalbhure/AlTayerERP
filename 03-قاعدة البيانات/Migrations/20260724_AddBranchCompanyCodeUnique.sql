-- المرحلة الأولى / الهيكل المؤسسي
-- Migration: 20260724_AddBranchCompanyCodeUnique.sql
-- الغرض: منع تكرار Branch_Code (رمز الفرع) داخل الشركة نفسها.

ALTER TABLE tenant_branches
    ADD CONSTRAINT UQ_TenantBranches_CompanyBranchCode
        UNIQUE (Company_ID, Branch_Code);

-- قبل التنفيذ التشغيلي، تحقق من البيانات التاريخية:
-- SELECT Company_ID, Branch_Code, COUNT(*)
-- FROM tenant_branches
-- GROUP BY Company_ID, Branch_Code
-- HAVING COUNT(*) > 1;