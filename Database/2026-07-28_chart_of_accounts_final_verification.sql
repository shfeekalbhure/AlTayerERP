-- ============================================================
-- AlTayerERP - الفحص النهائي لدليل الحسابات وتصنيفات الحسابات
-- القاعدة المعتمدة: altayer_erp_db فقط
-- هذا الملف لا يعدل البيانات؛ يعرض المخالفات المطلوب معالجتها فقط.
-- ============================================================

USE altayer_erp_db;

-- 1) التحقق من وجود الجداول الأساسية.
SELECT
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = DATABASE() AND table_name = 'chart_of_accounts'
    ) THEN 'OK' ELSE 'MISSING' END AS chart_of_accounts_table,
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = DATABASE() AND table_name = 'account_categories'
    ) THEN 'OK' ELSE 'MISSING' END AS account_categories_table;

-- 2) التحقق من أعمدة الحسابات الرقابية.
SELECT required_column,
       CASE WHEN EXISTS (
           SELECT 1 FROM information_schema.columns
           WHERE table_schema = DATABASE()
             AND table_name = 'chart_of_accounts'
             AND column_name = required_column
       ) THEN 'OK' ELSE 'MISSING' END AS column_status
FROM (
    SELECT 'Is_Control_Account' AS required_column
    UNION ALL SELECT 'Control_Account_Type'
    UNION ALL SELECT 'Account_Category'
    UNION ALL SELECT 'Allow_ManualEntry'
    UNION ALL SELECT 'Is_Postable'
    UNION ALL SELECT 'Is_Summary_Account'
) required_columns;

-- 3) شركات لا تملك تصنيفات حسابات فعالة.
SELECT c.Company_ID, c.Company_Name_AR
FROM companies c
LEFT JOIN account_categories ac
       ON ac.Company_ID = c.Company_ID AND ac.Is_Active = 1
GROUP BY c.Company_ID, c.Company_Name_AR
HAVING COUNT(ac.Category_ID) = 0;

-- 4) أنواع الحسابات التي لا تملك أي تصنيف فعال داخل شركة.
SELECT c.Company_ID, required_types.Account_Type
FROM companies c
CROSS JOIN (
    SELECT 'Asset' Account_Type
    UNION ALL SELECT 'Liability'
    UNION ALL SELECT 'Equity'
    UNION ALL SELECT 'Revenue'
    UNION ALL SELECT 'Expense'
) required_types
LEFT JOIN account_categories ac
       ON ac.Company_ID = c.Company_ID
      AND ac.Account_Type = required_types.Account_Type
      AND ac.Is_Active = 1
GROUP BY c.Company_ID, required_types.Account_Type
HAVING COUNT(ac.Category_ID) = 0;

-- 5) حسابات بتصنيف غير موجود أو غير فعال أو لا يطابق نوع الحساب.
SELECT coa.Account_ID,
       coa.Company_ID,
       coa.Account_Code,
       coa.Account_Name_AR,
       coa.Account_Type,
       coa.Account_Category,
       CASE
           WHEN ac.Category_ID IS NULL THEN 'CATEGORY_NOT_FOUND'
           WHEN ac.Is_Active = 0 THEN 'CATEGORY_INACTIVE'
           WHEN ac.Account_Type <> coa.Account_Type THEN 'CATEGORY_TYPE_MISMATCH'
           ELSE 'OK'
       END AS validation_status
FROM chart_of_accounts coa
LEFT JOIN account_categories ac
       ON ac.Company_ID = coa.Company_ID
      AND ac.Category_Code = coa.Account_Category
WHERE ac.Category_ID IS NULL
   OR ac.Is_Active = 0
   OR ac.Account_Type <> coa.Account_Type;

-- 6) حسابات رئيسية أو تجميعية قابلة للترحيل بالخطأ.
SELECT Account_ID, Company_ID, Account_Code, Account_Name_AR,
       Parent_Account_ID, Is_Postable, Is_Summary_Account
FROM chart_of_accounts
WHERE (Parent_Account_ID IS NULL OR Parent_Account_ID = '' OR Is_Summary_Account = 1)
  AND Is_Postable = 1;

-- 7) حسابات فرعية نهائية غير قابلة للترحيل رغم عدم وجود أبناء.
SELECT coa.Account_ID, coa.Company_ID, coa.Account_Code, coa.Account_Name_AR
FROM chart_of_accounts coa
WHERE coa.Is_Active = 1
  AND coa.Parent_Account_ID IS NOT NULL
  AND coa.Parent_Account_ID <> ''
  AND coa.Is_Postable = 0
  AND coa.Is_Summary_Account = 0
  AND NOT EXISTS (
      SELECT 1 FROM chart_of_accounts child
      WHERE child.Company_ID = coa.Company_ID
        AND child.Parent_Account_ID = coa.Account_ID
  );

-- 8) حسابات رقابية تسمح بالقيد اليدوي أو ليست نهائية.
SELECT Account_ID, Company_ID, Account_Code, Account_Name_AR,
       Is_Control_Account, Control_Account_Type,
       Allow_ManualEntry, Is_Postable, Is_Summary_Account
FROM chart_of_accounts
WHERE Is_Control_Account = 1
  AND (
      Allow_ManualEntry = 1
      OR Is_Postable = 0
      OR Is_Summary_Account = 1
      OR Control_Account_Type IS NULL
      OR Control_Account_Type NOT IN ('Customer', 'Vendor', 'Employee', 'Other')
  );

-- 9) عدم تطابق الحسابات الرقابية مع نوع الحساب المحاسبي.
SELECT Account_ID, Company_ID, Account_Code, Account_Name_AR,
       Account_Type, Control_Account_Type
FROM chart_of_accounts
WHERE Is_Control_Account = 1
  AND (
      (Control_Account_Type = 'Customer' AND Account_Type <> 'Asset')
      OR (Control_Account_Type = 'Vendor' AND Account_Type <> 'Liability')
  );

-- 10) حسابات ذات طبيعة رصيد لا تطابق نوع الحساب.
SELECT Account_ID, Company_ID, Account_Code, Account_Name_AR,
       Account_Type, Normal_Balance
FROM chart_of_accounts
WHERE (Account_Type IN ('Asset', 'Expense') AND Normal_Balance <> 'Debit')
   OR (Account_Type IN ('Liability', 'Equity', 'Revenue') AND Normal_Balance <> 'Credit');

-- 11) أكواد تصنيفات مكررة داخل الشركة، ويجب أن تكون النتيجة فارغة.
SELECT Company_ID, Category_Code, COUNT(*) AS duplicate_count
FROM account_categories
GROUP BY Company_ID, Category_Code
HAVING COUNT(*) > 1;

-- 12) ملخص الجاهزية لكل شركة.
SELECT
    c.Company_ID,
    c.Company_Name_AR,
    COUNT(DISTINCT coa.Account_ID) AS accounts_count,
    COUNT(DISTINCT ac.Category_ID) AS active_categories_count,
    SUM(CASE WHEN coa.Is_Control_Account = 1 THEN 1 ELSE 0 END) AS control_accounts_count
FROM companies c
LEFT JOIN chart_of_accounts coa ON coa.Company_ID = c.Company_ID
LEFT JOIN account_categories ac ON ac.Company_ID = c.Company_ID AND ac.Is_Active = 1
GROUP BY c.Company_ID, c.Company_Name_AR
ORDER BY c.Company_ID;
