-- ============================================================
-- AlTayerERP - ترقية دليل الحسابات لدعم الحسابات الرقابية
-- القاعدة المستهدفة: altayer_erp_db فقط
-- آمنة للتكرار ولا تحذف أي بيانات
-- ============================================================

USE altayer_erp_db;

SET @schema_name = DATABASE();

-- إضافة Is_Control_Account عند عدم وجوده
SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'chart_of_accounts'
          AND column_name = 'Is_Control_Account'
    ),
    'SELECT ''Is_Control_Account already exists'' AS Result',
    'ALTER TABLE chart_of_accounts ADD COLUMN Is_Control_Account TINYINT(1) NOT NULL DEFAULT 0 AFTER Multi_Currency'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- إضافة Control_Account_Type عند عدم وجوده
SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'chart_of_accounts'
          AND column_name = 'Control_Account_Type'
    ),
    'SELECT ''Control_Account_Type already exists'' AS Result',
    'ALTER TABLE chart_of_accounts ADD COLUMN Control_Account_Type VARCHAR(30) NULL AFTER Is_Control_Account'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- فهرس مساعد للاستعلام عن الحسابات الرقابية حسب الشركة والنوع
SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = @schema_name
          AND table_name = 'chart_of_accounts'
          AND index_name = 'IX_COA_Company_ControlType'
    ),
    'SELECT ''IX_COA_Company_ControlType already exists'' AS Result',
    'CREATE INDEX IX_COA_Company_ControlType ON chart_of_accounts (Company_ID, Is_Control_Account, Control_Account_Type)'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- تنظيف القيم غير المتسقة دون حذف أي سجل
UPDATE chart_of_accounts
SET Control_Account_Type = NULL
WHERE Is_Control_Account = 0
  AND Control_Account_Type IS NOT NULL;

-- الحساب الرقابي لا يسمح بالقيد اليدوي المباشر
UPDATE chart_of_accounts
SET Allow_ManualEntry = 0
WHERE Is_Control_Account = 1
  AND Allow_ManualEntry <> 0;

-- تقرير تحقق نهائي
SELECT
    COUNT(*) AS Total_Accounts,
    SUM(CASE WHEN Is_Control_Account = 1 THEN 1 ELSE 0 END) AS Control_Accounts,
    SUM(CASE WHEN Is_Control_Account = 1 AND Allow_ManualEntry = 1 THEN 1 ELSE 0 END) AS Invalid_Manual_Control_Accounts,
    SUM(CASE WHEN Is_Control_Account = 1 AND (Control_Account_Type IS NULL OR TRIM(Control_Account_Type) = '') THEN 1 ELSE 0 END) AS Missing_Control_Type
FROM chart_of_accounts;
