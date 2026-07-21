-- المرحلة الأولى: الإعدادات العامة والمالية متعددة النطاق.
-- شغّل هذا الملف مرة واحدة على قاعدة بيانات AlTayerERP قبل استخدام شاشة "الإعدادات العامة والمالية".
-- النطاقات المعتمدة:
-- SYSTEM      : إعداد عام لكل النظام.
-- COMPANY     : إعداد خاص بالشركة الحالية.
-- BRANCH      : إعداد خاص بالفرع الحالي.
-- FISCAL_YEAR : إعداد خاص بالفرع والسنة المالية الحالية.

CREATE TABLE IF NOT EXISTS system_settings
(
    Setting_ID       INT NOT NULL AUTO_INCREMENT,
    Setting_Key      VARCHAR(100) NOT NULL,
    Setting_Name     VARCHAR(200) NOT NULL,
    Setting_Value    TEXT NOT NULL,
    Scope            VARCHAR(20) NOT NULL DEFAULT 'SYSTEM',
    Company_ID       VARCHAR(50) NOT NULL DEFAULT '',
    Branch_ID        INT NOT NULL DEFAULT 0,
    Fiscal_Year_ID   INT NOT NULL DEFAULT 0,
    Effective_Date   DATE NULL,
    Description      TEXT NOT NULL,
    Is_Active        TINYINT(1) NOT NULL DEFAULT 1,
    Created_At       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At       DATETIME NULL,
    PRIMARY KEY (Setting_ID),
    UNIQUE KEY UQ_system_settings_scope
        (Setting_Key, Scope, Company_ID, Branch_ID, Fiscal_Year_ID),
    KEY IX_system_settings_context
        (Company_ID, Branch_ID, Fiscal_Year_ID, Is_Active)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;

-- مثال اختياري لإعداد عام:
-- INSERT INTO system_settings
-- (Setting_Key, Setting_Name, Setting_Value, Scope, Company_ID, Branch_ID, Fiscal_Year_ID, Description, Is_Active)
-- VALUES
-- ('DEFAULT_CURRENCY', 'العملة الافتراضية', 'YER', 'SYSTEM', '', 0, 0, 'العملة المقترحة عند إنشاء مستند جديد.', 1);
