-- AlTayerERP
-- التحديث المصحح المعتمد لحماية دليل الحسابات والسندات المالية
-- مبني على النسخة الحالية من altayer_erp_db
-- لا ينشئ قاعدة جديدة، ولا يحذف بيانات أو جداول.
-- الأعمدة والجداول الأساسية موجودة مسبقاً؛ هذا الملف يضيف الحماية الناقصة فقط.

USE altayer_erp_db;

-- =========================================================
-- 1) تعبئة تاريخ سريان الحسابات القديمة بأمان
-- =========================================================

SET SQL_SAFE_UPDATES = 0;

UPDATE chart_of_accounts
SET Effective_From = COALESCE(Effective_From, DATE(Created_At))
WHERE Effective_From IS NULL;

-- توحيد القيم العربية القديمة لطبيعة الحساب مع القيم البرمجية المعتمدة.
UPDATE chart_of_accounts
SET Normal_Balance = CASE
    WHEN TRIM(Normal_Balance) = 'مدين' THEN 'Debit'
    WHEN TRIM(Normal_Balance) = 'دائن' THEN 'Credit'
    WHEN Normal_Balance IS NULL OR TRIM(Normal_Balance) = '' THEN
        CASE
            WHEN Account_Type IN ('Asset', 'Expense') THEN 'Debit'
            WHEN Account_Type IN ('Liability', 'Equity', 'Revenue') THEN 'Credit'
            ELSE Normal_Balance
        END
    ELSE Normal_Balance
END
WHERE Normal_Balance IS NULL
   OR TRIM(Normal_Balance) IN ('', 'مدين', 'دائن');

SET SQL_SAFE_UPDATES = 1;

-- =========================================================
-- 2) Trigger حماية عند إضافة سطر سند مالي
-- الشركة تؤخذ من:
-- financial_voucher_headers.Branch_ID -> tenant_branches.Branch_ID -> Company_ID
-- =========================================================

DROP TRIGGER IF EXISTS trg_financial_voucher_details_account_policy_bi;

DELIMITER $$

CREATE TRIGGER trg_financial_voucher_details_account_policy_bi
BEFORE INSERT ON financial_voucher_details
FOR EACH ROW
BEGIN
    DECLARE v_company_id VARCHAR(50) DEFAULT NULL;
    DECLARE v_voucher_date DATE DEFAULT NULL;
    DECLARE v_account_found INT DEFAULT 0;
    DECLARE v_active TINYINT DEFAULT 0;
    DECLARE v_postable TINYINT DEFAULT 0;
    DECLARE v_summary TINYINT DEFAULT 0;
    DECLARE v_requires_cc TINYINT DEFAULT 0;
    DECLARE v_requires_project TINYINT DEFAULT 0;
    DECLARE v_effective_from DATE DEFAULT NULL;
    DECLARE v_effective_to DATE DEFAULT NULL;

    SELECT tb.Company_ID, DATE(h.Voucher_Date)
      INTO v_company_id, v_voucher_date
      FROM financial_voucher_headers h
      INNER JOIN tenant_branches tb
              ON tb.Branch_ID = CAST(h.Branch_ID AS UNSIGNED)
     WHERE h.Voucher_ID = NEW.Voucher_ID
     LIMIT 1;

    IF v_company_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تعذر تحديد شركة السند من الفرع المحدد.';
    END IF;

    SELECT COUNT(*),
           COALESCE(MAX(a.Is_Active), 0),
           COALESCE(MAX(a.Is_Postable), 0),
           COALESCE(MAX(a.Is_Summary_Account), 0),
           COALESCE(MAX(a.Requires_CostCenter), 0),
           COALESCE(MAX(a.Requires_Project), 0),
           MAX(a.Effective_From),
           MAX(a.Effective_To)
      INTO v_account_found,
           v_active,
           v_postable,
           v_summary,
           v_requires_cc,
           v_requires_project,
           v_effective_from,
           v_effective_to
      FROM chart_of_accounts a
     WHERE a.Account_ID = NEW.Account_ID
       AND a.Company_ID = v_company_id;

    IF v_account_found = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب غير موجود ضمن شركة السند.';
    END IF;

    IF v_active = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار موقوف.';
    END IF;

    IF v_postable = 0 OR v_summary = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار تجميعي أو غير قابل للترحيل.';
    END IF;

    IF v_effective_from IS NOT NULL AND v_voucher_date < v_effective_from THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تاريخ السند يسبق تاريخ سريان الحساب.';
    END IF;

    IF v_effective_to IS NOT NULL AND v_voucher_date > v_effective_to THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تاريخ السند يتجاوز تاريخ انتهاء الحساب.';
    END IF;

    IF v_requires_cc = 1
       AND (NEW.Cost_Center_ID IS NULL OR TRIM(NEW.Cost_Center_ID) = '') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار يتطلب مركز تكلفة.';
    END IF;

    IF v_requires_project = 1
       AND (NEW.Project_ID IS NULL OR TRIM(NEW.Project_ID) = '') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار يتطلب مشروعاً.';
    END IF;
END$$

DELIMITER ;

-- =========================================================
-- 3) Trigger الحماية عند تعديل سطر سند مالي
-- =========================================================

DROP TRIGGER IF EXISTS trg_financial_voucher_details_account_policy_bu;

DELIMITER $$

CREATE TRIGGER trg_financial_voucher_details_account_policy_bu
BEFORE UPDATE ON financial_voucher_details
FOR EACH ROW
BEGIN
    DECLARE v_company_id VARCHAR(50) DEFAULT NULL;
    DECLARE v_voucher_date DATE DEFAULT NULL;
    DECLARE v_account_found INT DEFAULT 0;
    DECLARE v_active TINYINT DEFAULT 0;
    DECLARE v_postable TINYINT DEFAULT 0;
    DECLARE v_summary TINYINT DEFAULT 0;
    DECLARE v_requires_cc TINYINT DEFAULT 0;
    DECLARE v_requires_project TINYINT DEFAULT 0;
    DECLARE v_effective_from DATE DEFAULT NULL;
    DECLARE v_effective_to DATE DEFAULT NULL;

    SELECT tb.Company_ID, DATE(h.Voucher_Date)
      INTO v_company_id, v_voucher_date
      FROM financial_voucher_headers h
      INNER JOIN tenant_branches tb
              ON tb.Branch_ID = CAST(h.Branch_ID AS UNSIGNED)
     WHERE h.Voucher_ID = NEW.Voucher_ID
     LIMIT 1;

    IF v_company_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تعذر تحديد شركة السند من الفرع المحدد.';
    END IF;

    SELECT COUNT(*),
           COALESCE(MAX(a.Is_Active), 0),
           COALESCE(MAX(a.Is_Postable), 0),
           COALESCE(MAX(a.Is_Summary_Account), 0),
           COALESCE(MAX(a.Requires_CostCenter), 0),
           COALESCE(MAX(a.Requires_Project), 0),
           MAX(a.Effective_From),
           MAX(a.Effective_To)
      INTO v_account_found,
           v_active,
           v_postable,
           v_summary,
           v_requires_cc,
           v_requires_project,
           v_effective_from,
           v_effective_to
      FROM chart_of_accounts a
     WHERE a.Account_ID = NEW.Account_ID
       AND a.Company_ID = v_company_id;

    IF v_account_found = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب غير موجود ضمن شركة السند.';
    END IF;

    IF v_active = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار موقوف.';
    END IF;

    IF v_postable = 0 OR v_summary = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار تجميعي أو غير قابل للترحيل.';
    END IF;

    IF v_effective_from IS NOT NULL AND v_voucher_date < v_effective_from THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تاريخ السند يسبق تاريخ سريان الحساب.';
    END IF;

    IF v_effective_to IS NOT NULL AND v_voucher_date > v_effective_to THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'تاريخ السند يتجاوز تاريخ انتهاء الحساب.';
    END IF;

    IF v_requires_cc = 1
       AND (NEW.Cost_Center_ID IS NULL OR TRIM(NEW.Cost_Center_ID) = '') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار يتطلب مركز تكلفة.';
    END IF;

    IF v_requires_project = 1
       AND (NEW.Project_ID IS NULL OR TRIM(NEW.Project_ID) = '') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'الحساب المختار يتطلب مشروعاً.';
    END IF;
END$$

DELIMITER ;

-- =========================================================
-- 4) التحقق النهائي
-- =========================================================

SELECT TRIGGER_NAME, EVENT_MANIPULATION, EVENT_OBJECT_TABLE
FROM information_schema.TRIGGERS
WHERE TRIGGER_SCHEMA = DATABASE()
  AND TRIGGER_NAME IN (
      'trg_financial_voucher_details_account_policy_bi',
      'trg_financial_voucher_details_account_policy_bu'
  )
ORDER BY TRIGGER_NAME;

SELECT 'تم تطبيق حماية دليل الحسابات والسندات المالية بنجاح.' AS Result;
