-- AlTayerERP | المرحلة الأولى | بيانات مرجعية لأنواع الفروع
-- ينشئ جدولاً جديداً فقط ولا يغير أو يحذف أي مفتاح قائم.

CREATE TABLE IF NOT EXISTS branch_types (
    Branch_Type_ID INT NOT NULL AUTO_INCREMENT,
    Branch_Type_Code VARCHAR(30) NOT NULL,
    Branch_Type_Name_AR VARCHAR(150) NOT NULL,
    Branch_Type_Name_EN VARCHAR(150) NULL,
    Sort_Order INT NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Notes VARCHAR(500) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Branch_Type_ID),
    CONSTRAINT UQ_branch_types_code UNIQUE (Branch_Type_Code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- بيانات بداية قابلة للتعديل من شاشة «إعدادات أنواع الفروع».
INSERT INTO branch_types (Branch_Type_Code, Branch_Type_Name_AR, Branch_Type_Name_EN, Sort_Order, Is_Active)
VALUES
    ('MAIN', 'فرع رئيسي', 'Main Branch', 10, 1),
    ('OPERATING', 'فرع تشغيلي', 'Operating Branch', 20, 1),
    ('OFFICE', 'مكتب', 'Office', 30, 1),
    ('AGENCY', 'وكالة', 'Agency', 40, 1),
    ('WAREHOUSE', 'مستودع', 'Warehouse', 50, 1),
    ('SERVICE_POINT', 'نقطة خدمة', 'Service Point', 60, 1)
ON DUPLICATE KEY UPDATE
    Branch_Type_Name_AR = VALUES(Branch_Type_Name_AR),
    Branch_Type_Name_EN = VALUES(Branch_Type_Name_EN),
    Sort_Order = VALUES(Sort_Order);

-- توافق آمن للبيانات القديمة التي كانت تحفظ الاسم العربي داخل Tenant_Branches.Branch_Type.
UPDATE tenant_branches
SET Branch_Type = CASE Branch_Type
    WHEN 'رئيسي' THEN 'MAIN'
    WHEN 'فرعي' THEN 'OPERATING'
    WHEN 'نقطة توزيع' THEN 'SERVICE_POINT'
    WHEN 'مستودع' THEN 'WAREHOUSE'
    ELSE Branch_Type
END
WHERE Branch_ID > 0
  AND Branch_Type IN ('رئيسي', 'فرعي', 'نقطة توزيع', 'مستودع');

-- سجل الشاشتين داخل كتالوج الصلاحيات؛ يمنح مدير النظام الوصول الكامل تلقائياً.
INSERT INTO system_screens (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'BusinessGroups', 'المجموعات التجارية', 'الإدارة العامة', 1, 10, NOW()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'BusinessGroups');

INSERT INTO system_screens (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'BranchTypes', 'إعدادات أنواع الفروع', 'الإدارة العامة', 1, 30, NOW()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'BranchTypes');
