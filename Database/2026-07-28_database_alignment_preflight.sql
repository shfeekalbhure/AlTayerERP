-- AlTayerERP
-- الفحص الشامل الأولي لمطابقة قاعدة البيانات مع المشروع
-- لا يحذف ولا يعدل بيانات تشغيلية.
-- ينشئ جدول نتائج تشخيصية فقط، ثم يسجل التعارضات المؤكدة.

USE altayer_erp_db;

CREATE TABLE IF NOT EXISTS database_alignment_findings
(
    Finding_ID BIGINT NOT NULL AUTO_INCREMENT,
    Check_Code VARCHAR(50) NOT NULL,
    Severity VARCHAR(20) NOT NULL,
    Object_Name VARCHAR(150) NOT NULL,
    Finding_Message VARCHAR(1000) NOT NULL,
    Finding_Count BIGINT NOT NULL DEFAULT 0,
    Detected_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Finding_ID),
    KEY IX_Database_Alignment_Findings_Code (Check_Code),
    KEY IX_Database_Alignment_Findings_Severity (Severity)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

TRUNCATE TABLE database_alignment_findings;

-- =========================================================
-- 1) الجداول المطلوبة بواسطة AppDbContext
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'REQ_TABLE_MISSING', 'CRITICAL', required.Table_Name,
       CONCAT('الجدول المطلوب بواسطة المشروع غير موجود: ', required.Table_Name), 1
FROM
(
    SELECT 'tenant_groups' Table_Name UNION ALL
    SELECT 'companies' UNION ALL
    SELECT 'tenant_branches' UNION ALL
    SELECT 'fiscal_years' UNION ALL
    SELECT 'users' UNION ALL
    SELECT 'roles' UNION ALL
    SELECT 'role_permissions' UNION ALL
    SELECT 'user_permissions' UNION ALL
    SELECT 'system_permissions' UNION ALL
    SELECT 'system_screens' UNION ALL
    SELECT 'login_attempts' UNION ALL
    SELECT 'refresh_tokens' UNION ALL
    SELECT 'system_settings' UNION ALL
    SELECT 'fiscal_periods' UNION ALL
    SELECT 'exchange_rates' UNION ALL
    SELECT 'numbering_settings' UNION ALL
    SELECT 'numbering_counters' UNION ALL
    SELECT 'financial_limits' UNION ALL
    SELECT 'financial_limit_movements' UNION ALL
    SELECT 'approval_requests' UNION ALL
    SELECT 'account_code_settings' UNION ALL
    SELECT 'chart_of_accounts' UNION ALL
    SELECT 'cost_centers' UNION ALL
    SELECT 'cash_boxes' UNION ALL
    SELECT 'currencies' UNION ALL
    SELECT 'bank_accounts' UNION ALL
    SELECT 'financial_voucher_headers' UNION ALL
    SELECT 'financial_voucher_details' UNION ALL
    SELECT 'journal_entry_headers' UNION ALL
    SELECT 'journal_entry_details' UNION ALL
    SELECT 'document_allocations' UNION ALL
    SELECT 'document_links' UNION ALL
    SELECT 'audit_logs' UNION ALL
    SELECT 'voucher_action_logs' UNION ALL
    SELECT 'voucher_types' UNION ALL
    SELECT 'voucher_statuses' UNION ALL
    SELECT 'payment_methods' UNION ALL
    SELECT 'parties' UNION ALL
    SELECT 'payment_requests' UNION ALL
    SELECT 'payment_request_lines' UNION ALL
    SELECT 'payment_request_attachments'
) required
LEFT JOIN information_schema.tables t
       ON t.table_schema = DATABASE()
      AND t.table_name = required.Table_Name
WHERE t.table_name IS NULL;

-- =========================================================
-- 2) جداول قديمة أو مكررة
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'LEGACY_TABLE', 'WARNING', t.table_name,
       CONCAT('جدول قديم أو مكرر يحتاج قرار عزل/ترحيل: ', t.table_name),
       COALESCE(ts.table_rows, 0)
FROM information_schema.tables t
LEFT JOIN information_schema.tables ts
       ON ts.table_schema = t.table_schema
      AND ts.table_name = t.table_name
WHERE t.table_schema = DATABASE()
  AND t.table_name IN
  (
      'branches',
      'voucher_headers',
      'voucher_details',
      'currency_exchange_rates'
  );

-- =========================================================
-- 3) اختلافات أنواع Company_ID
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'COMPANY_ID_TYPE',
       CASE WHEN character_maximum_length < 50 THEN 'HIGH' ELSE 'INFO' END,
       CONCAT(table_name, '.', column_name),
       CONCAT('نوع Company_ID الحالي: ', column_type,
              '، المطلوب المستهدف VARCHAR(50) بنفس الترميز.'),
       1
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND column_name = 'Company_ID'
  AND (data_type <> 'varchar' OR character_maximum_length <> 50);

-- =========================================================
-- 4) اختلافات أنواع Branch_ID في الجداول الحالية
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'BRANCH_ID_TYPE', 'HIGH', CONCAT(table_name, '.', column_name),
       CONCAT('نوع Branch_ID غير موحد: ', column_type,
              '؛ tenant_branches.Branch_ID هو المرجع INT.'), 1
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND column_name = 'Branch_ID'
  AND table_name IN
  (
      'financial_voucher_headers',
      'journal_entry_headers',
      'payment_requests',
      'fiscal_periods',
      'numbering_settings',
      'numbering_counters'
  )
  AND data_type NOT IN ('int', 'bigint');

-- قيم Branch_ID غير رقمية في السندات الحالية.
INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'INVALID_BRANCH_VALUE', 'CRITICAL', 'financial_voucher_headers.Branch_ID',
       'توجد قيم Branch_ID غير رقمية ولا يمكن تحويلها إلى INT.', COUNT(*)
FROM financial_voucher_headers
WHERE Branch_ID IS NOT NULL
  AND TRIM(Branch_ID) <> ''
  AND TRIM(Branch_ID) NOT REGEXP '^[0-9]+$'
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'ORPHAN_BRANCH', 'CRITICAL', 'financial_voucher_headers.Branch_ID',
       'توجد سندات تشير إلى فرع غير موجود في tenant_branches.', COUNT(*)
FROM financial_voucher_headers h
LEFT JOIN tenant_branches b
       ON b.Branch_ID = CAST(h.Branch_ID AS UNSIGNED)
WHERE h.Branch_ID IS NOT NULL
  AND TRIM(h.Branch_ID) REGEXP '^[0-9]+$'
  AND b.Branch_ID IS NULL
HAVING COUNT(*) > 0;

-- =========================================================
-- 5) فهرس أرقام السندات
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'VOUCHER_GLOBAL_UNIQUE', 'HIGH', s.index_name,
       'يوجد فهرس فريد عالمي على Voucher_No؛ المطلوب فهرس مركب حسب الفرع والسنة والنوع.', 1
FROM information_schema.statistics s
WHERE s.table_schema = DATABASE()
  AND s.table_name = 'financial_voucher_headers'
  AND s.non_unique = 0
GROUP BY s.index_name
HAVING GROUP_CONCAT(s.column_name ORDER BY s.seq_in_index) = 'Voucher_No';

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'VOUCHER_SCOPE_DUPLICATE', 'CRITICAL', 'financial_voucher_headers',
       'توجد أرقام سندات مكررة داخل نفس الفرع والسنة والنوع.', COUNT(*)
FROM
(
    SELECT Branch_ID, Fiscal_Year_ID, Voucher_Type_ID, Voucher_No
    FROM financial_voucher_headers
    GROUP BY Branch_ID, Fiscal_Year_ID, Voucher_Type_ID, Voucher_No
    HAVING COUNT(*) > 1
) d
HAVING COUNT(*) > 0;

-- =========================================================
-- 6) مطابقة bank_accounts مع الكيان الحالي
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'BANK_SCHEMA_MISMATCH', 'CRITICAL', 'bank_accounts',
       'جدول bank_accounts لا يطابق الكيان الحالي؛ أعمدة مطلوبة مفقودة أو المفتاح بنوع غير صحيح.',
       COUNT(*)
FROM
(
    SELECT 'Bank_Code' col UNION ALL
    SELECT 'Account_No' UNION ALL
    SELECT 'GL_Account' UNION ALL
    SELECT 'Branch_Name'
) req
LEFT JOIN information_schema.columns c
       ON c.table_schema = DATABASE()
      AND c.table_name = 'bank_accounts'
      AND c.column_name = req.col
WHERE c.column_name IS NULL
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'BANK_PK_TYPE', 'CRITICAL', 'bank_accounts.Bank_Account_ID',
       CONCAT('نوع المفتاح الحالي ', column_type, ' بينما المشروع يتوقع INT AUTO_INCREMENT.'), 1
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = 'bank_accounts'
  AND column_name = 'Bank_Account_ID'
  AND NOT (data_type = 'int' AND extra LIKE '%auto_increment%');

-- =========================================================
-- 7) الحسابات والقيم اليتيمة
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'ORPHAN_ACCOUNT_VOUCHER', 'CRITICAL', 'financial_voucher_details.Account_ID',
       'توجد تفاصيل سندات تشير إلى حساب غير موجود.', COUNT(*)
FROM financial_voucher_details d
LEFT JOIN chart_of_accounts a ON a.Account_ID = d.Account_ID
WHERE a.Account_ID IS NULL
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'ORPHAN_ACCOUNT_JOURNAL', 'CRITICAL', 'journal_entry_details.Account_ID',
       'توجد تفاصيل قيود تشير إلى حساب غير موجود.', COUNT(*)
FROM journal_entry_details d
LEFT JOIN chart_of_accounts a ON a.Account_ID = d.Account_ID
WHERE a.Account_ID IS NULL
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'INVALID_ACCOUNT_PARENT', 'CRITICAL', 'chart_of_accounts.Parent_Account_ID',
       'توجد حسابات تشير إلى حساب أب غير موجود.', COUNT(*)
FROM chart_of_accounts c
LEFT JOIN chart_of_accounts p ON p.Account_ID = c.Parent_Account_ID
WHERE c.Parent_Account_ID IS NOT NULL
  AND TRIM(c.Parent_Account_ID) <> ''
  AND p.Account_ID IS NULL
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'SUMMARY_POSTABLE', 'HIGH', 'chart_of_accounts',
       'توجد حسابات تجميعية معرفة كقابلة للترحيل.', COUNT(*)
FROM chart_of_accounts
WHERE Is_Summary_Account = 1 AND Is_Postable = 1
HAVING COUNT(*) > 0;

-- =========================================================
-- 8) العملات
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'CURRENCY_GLOBAL_UNIQUE', 'HIGH', s.index_name,
       'كود العملة فريد عالمياً بينما النظام متعدد الشركات؛ يلزم فهرس Company_ID + Currency_Code بعد معالجة مراجع Legacy.', 1
FROM information_schema.statistics s
WHERE s.table_schema = DATABASE()
  AND s.table_name = 'currencies'
  AND s.non_unique = 0
GROUP BY s.index_name
HAVING GROUP_CONCAT(s.column_name ORDER BY s.seq_in_index) = 'Currency_Code';

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'MULTIPLE_DEFAULT_CURRENCY', 'CRITICAL', 'currencies',
       'توجد أكثر من عملة افتراضية داخل شركة واحدة.', COUNT(*)
FROM
(
    SELECT Company_ID
    FROM currencies
    WHERE Is_Default = 1 AND Is_Active = 1
    GROUP BY Company_ID
    HAVING COUNT(*) > 1
) x
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'MULTIPLE_LOCAL_CURRENCY', 'CRITICAL', 'currencies',
       'توجد أكثر من عملة محلية داخل شركة واحدة.', COUNT(*)
FROM
(
    SELECT Company_ID
    FROM currencies
    WHERE Is_Local_Currency = 1 AND Is_Active = 1
    GROUP BY Company_ID
    HAVING COUNT(*) > 1
) x
HAVING COUNT(*) > 0;

-- =========================================================
-- 9) الفترات والسنوات
-- =========================================================

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'ORPHAN_FISCAL_YEAR_VOUCHER', 'CRITICAL', 'financial_voucher_headers.Fiscal_Year_ID',
       'توجد سندات مرتبطة بسنة مالية غير موجودة.', COUNT(*)
FROM financial_voucher_headers h
LEFT JOIN fiscal_years y ON y.Fiscal_Year_ID = h.Fiscal_Year_ID
WHERE y.Fiscal_Year_ID IS NULL
HAVING COUNT(*) > 0;

INSERT INTO database_alignment_findings
(Check_Code, Severity, Object_Name, Finding_Message, Finding_Count)
SELECT 'OVERLAPPING_PERIODS', 'HIGH', 'fiscal_periods',
       'توجد فترات مالية متداخلة لنفس الفرع والسنة.', COUNT(*)
FROM fiscal_periods p1
JOIN fiscal_periods p2
  ON p1.Fiscal_Period_ID < p2.Fiscal_Period_ID
 AND p1.Branch_ID = p2.Branch_ID
 AND p1.Fiscal_Year_ID = p2.Fiscal_Year_ID
 AND p1.Start_Date <= p2.End_Date
 AND p2.Start_Date <= p1.End_Date
HAVING COUNT(*) > 0;

-- =========================================================
-- 10) نتائج عامة
-- =========================================================

SELECT Severity, COUNT(*) AS Findings
FROM database_alignment_findings
GROUP BY Severity
ORDER BY FIELD(Severity, 'CRITICAL', 'HIGH', 'WARNING', 'INFO');

SELECT Finding_ID,
       Check_Code,
       Severity,
       Object_Name,
       Finding_Message,
       Finding_Count,
       Detected_At
FROM database_alignment_findings
ORDER BY FIELD(Severity, 'CRITICAL', 'HIGH', 'WARNING', 'INFO'), Finding_ID;
