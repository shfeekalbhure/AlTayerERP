-- =============================================================
-- المرحلة الأولى | حقول التدقيق للمجموعات التجارية
-- القرار: ADR-UI-020
-- ينفذ بعد 2026-07-23_expand_tenant_groups.sql مرة واحدة.
-- =============================================================

ALTER TABLE tenant_groups
    ADD COLUMN IF NOT EXISTS Created_By INT NULL COMMENT 'المستخدم الذي أنشأ السجل من جلسة الخادم',
    ADD COLUMN IF NOT EXISTS Updated_By INT NULL COMMENT 'آخر مستخدم عدل السجل من جلسة الخادم',
    ADD COLUMN IF NOT EXISTS Edit_Count INT NOT NULL DEFAULT 0 COMMENT 'عدد التعديلات الناجحة';

CREATE INDEX IF NOT EXISTS IX_Tenant_Groups_Created_By ON tenant_groups (Created_By);
CREATE INDEX IF NOT EXISTS IX_Tenant_Groups_Updated_By ON tenant_groups (Updated_By);
