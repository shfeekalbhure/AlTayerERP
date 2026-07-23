-- AlTayerERP | المرحلة الأولى | استكمال شاشة المجموعات التجارية
-- ينفذ مرة واحدة قبل تشغيل API بعد هذا التحديث. لا يحذف أي مجموعة أو شركة.

ALTER TABLE tenant_groups
    ADD COLUMN IF NOT EXISTS Group_Code VARCHAR(40) NULL AFTER Group_ID,
    ADD COLUMN IF NOT EXISTS Notes VARCHAR(500) NULL AFTER Group_Name_EN,
    ADD COLUMN IF NOT EXISTS Updated_At DATETIME NULL AFTER Created_At;

-- يمنح السجلات السابقة أرقام عرض مستقرة؛ لا يغير Group_ID ولا علاقات الشركات.
UPDATE tenant_groups
SET Group_Code = CONCAT('GRP-LEGACY-', LEFT(REPLACE(Group_ID, '-', ''), 8))
WHERE Group_Code IS NULL OR TRIM(Group_Code) = '';

CREATE UNIQUE INDEX IF NOT EXISTS UQ_tenant_groups_group_code ON tenant_groups (Group_Code);

-- سياسة الترقيم التي تستخدمها API للمجموعات الجديدة.
INSERT INTO numbering_settings
    (Document_Type, Prefix, Digits_Count, Reset_Type, Last_Number, Use_Company, Use_Branch, Use_Year, Is_Active)
SELECT 'BUSINESS_GROUP', 'GRP', 4, 'NONE', 0, 0, 0, 0, 1
WHERE NOT EXISTS (
    SELECT 1 FROM numbering_settings WHERE UPPER(TRIM(Document_Type)) = 'BUSINESS_GROUP'
);
