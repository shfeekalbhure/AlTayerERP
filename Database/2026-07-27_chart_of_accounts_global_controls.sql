-- AlTayerERP
-- ضوابط دليل الحسابات والتوجيه المالي المتقدم
-- قاعدة البيانات المعتمدة: altayer_erp_db فقط
-- آمن لإعادة التشغيل ولا يحذف بيانات أو جداول.

USE altayer_erp_db;

SET @db_name = DATABASE();

-- =========================================================
-- 1) تواريخ السريان وخرائط التقارير على الحساب
-- =========================================================

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Effective_From'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Effective_From DATE NULL AFTER Is_Active'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Effective_To'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Effective_To DATE NULL AFTER Effective_From'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Financial_Statement_Line_Code'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Financial_Statement_Line_Code VARCHAR(50) NULL AFTER Effective_To'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Cash_Flow_Category'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Cash_Flow_Category VARCHAR(20) NULL AFTER Financial_Statement_Line_Code'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Is_Intercompany'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Is_Intercompany TINYINT(1) NOT NULL DEFAULT 0 AFTER Cash_Flow_Category'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND column_name='Intercompany_Partner_Required'),
    'SELECT 1',
    'ALTER TABLE chart_of_accounts ADD COLUMN Intercompany_Partner_Required TINYINT(1) NOT NULL DEFAULT 0 AFTER Is_Intercompany'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- القيم القديمة تصبح سارية من تاريخ إنشائها إن أمكن.
UPDATE chart_of_accounts
SET Effective_From = COALESCE(Effective_From, DATE(Created_At))
WHERE Effective_From IS NULL;

-- =========================================================
-- 2) كتالوج بنود القوائم المالية
-- =========================================================

CREATE TABLE IF NOT EXISTS financial_statement_lines (
    Statement_Line_ID BIGINT NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Statement_Type VARCHAR(30) NOT NULL COMMENT 'BALANCE_SHEET, INCOME_STATEMENT, CASH_FLOW, EQUITY, OCI',
    Line_Code VARCHAR(50) NOT NULL,
    Line_Name_AR VARCHAR(200) NOT NULL,
    Line_Name_EN VARCHAR(200) NULL,
    Parent_Line_ID BIGINT NULL,
    Display_Order INT NOT NULL DEFAULT 0,
    Normal_Balance VARCHAR(10) NULL COMMENT 'Debit/Credit',
    Is_Subtotal TINYINT(1) NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_By VARCHAR(50) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_By VARCHAR(50) NULL,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Statement_Line_ID),
    UNIQUE KEY UX_Financial_Statement_Line (Company_ID, Statement_Type, Line_Code),
    KEY IX_Financial_Statement_Lines_Parent (Parent_Line_ID),
    CONSTRAINT FK_Financial_Statement_Lines_Parent
        FOREIGN KEY (Parent_Line_ID) REFERENCES financial_statement_lines(Statement_Line_ID)
        ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS account_statement_mappings (
    Account_Statement_Mapping_ID BIGINT NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Account_ID VARCHAR(50) NOT NULL,
    Statement_Line_ID BIGINT NOT NULL,
    Effective_From DATE NOT NULL,
    Effective_To DATE NULL,
    Mapping_Percentage DECIMAL(7,4) NOT NULL DEFAULT 100.0000,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_By VARCHAR(50) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_By VARCHAR(50) NULL,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Account_Statement_Mapping_ID),
    UNIQUE KEY UX_Account_Statement_Mapping (Company_ID, Account_ID, Statement_Line_ID, Effective_From),
    KEY IX_Account_Statement_Mappings_Account (Account_ID),
    KEY IX_Account_Statement_Mappings_Line (Statement_Line_ID),
    CONSTRAINT FK_Account_Statement_Mappings_Account
        FOREIGN KEY (Account_ID) REFERENCES chart_of_accounts(Account_ID)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Account_Statement_Mappings_Line
        FOREIGN KEY (Statement_Line_ID) REFERENCES financial_statement_lines(Statement_Line_ID)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CHK_Account_Statement_Mapping_Percentage
        CHECK (Mapping_Percentage > 0 AND Mapping_Percentage <= 100),
    CONSTRAINT CHK_Account_Statement_Mapping_Dates
        CHECK (Effective_To IS NULL OR Effective_To >= Effective_From)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- 3) سجل تغييرات بنية دليل الحسابات
-- =========================================================

CREATE TABLE IF NOT EXISTS chart_of_accounts_change_log (
    Change_ID BIGINT NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Account_ID VARCHAR(50) NOT NULL,
    Change_Type VARCHAR(30) NOT NULL COMMENT 'CREATE, UPDATE, MOVE, ACTIVATE, DEACTIVATE, MAP',
    Change_Reason VARCHAR(500) NULL,
    Old_Data_JSON JSON NULL,
    New_Data_JSON JSON NULL,
    Requested_By VARCHAR(50) NULL,
    Approved_By VARCHAR(50) NULL,
    Effective_Date DATE NOT NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Change_ID),
    KEY IX_COA_Change_Log_Account (Company_ID, Account_ID, Effective_Date),
    CONSTRAINT FK_COA_Change_Log_Account
        FOREIGN KEY (Account_ID) REFERENCES chart_of_accounts(Account_ID)
        ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- 4) فهارس الحماية والأداء
-- =========================================================

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.statistics WHERE table_schema=@db_name AND table_name='chart_of_accounts' AND index_name='IX_COA_Company_Active_Effective'),
    'SELECT 1',
    'CREATE INDEX IX_COA_Company_Active_Effective ON chart_of_accounts (Company_ID, Is_Active, Effective_From, Effective_To)'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- =========================================================
-- 5) مشغلات حماية السند قبل الحفظ/التعديل
-- تمنع استخدام حساب غير نشط أو تجميعي أو خارج السريان،
-- وتلزم مركز التكلفة والمشروع عندما يطلبهما الحساب.
-- =========================================================

DROP TRIGGER IF EXISTS trg_financial_voucher_details_account_policy_bi;
DELIMITER $$
CREATE TRIGGER trg_financial_voucher_details_account_policy_bi
BEFORE INSERT ON financial_voucher_details
FOR EACH ROW
BEGIN
    DECLARE v_company_id VARCHAR(50);
    DECLARE v_voucher_date DATE;
    DECLARE v_active TINYINT;
    DECLARE v_postable TINYINT;
    DECLARE v_summary TINYINT;
    DECLARE v_requires_cc TINYINT;
    DECLARE v_requires_project TINYINT;
    DECLARE v_effective_from DATE;
    DECLARE v_effective_to DATE;

    SELECT h.Company_ID, DATE(h.Voucher_Date)
      INTO v_company_id, v_voucher_date
      FROM financial_voucher_headers h
     WHERE h.Voucher_ID = NEW.Voucher_ID
     LIMIT 1;

    SELECT a.Is_Active, a.Is_Postable, a.Is_Summary_Account,
           a.Requires_CostCenter, a.Requires_Project,
           a.Effective_From, a.Effective_To
      INTO v_active, v_postable, v_summary,
           v_requires_cc, v_requires_project,
           v_effective_from, v_effective_to
      FROM chart_of_accounts a
     WHERE a.Account_ID = NEW.Account_ID
       AND a.Company_ID = v_company_id
     LIMIT 1;

    IF v_active IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب غير موجود ضمن شركة السند.';
    END IF;
    IF v_active = 0 OR v_postable = 0 OR v_summary = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب موقوف أو تجميعي أو غير قابل للترحيل.';
    END IF;
    IF v_effective_from IS NOT NULL AND v_voucher_date < v_effective_from THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'تاريخ السند يسبق تاريخ سريان الحساب.';
    END IF;
    IF v_effective_to IS NOT NULL AND v_voucher_date > v_effective_to THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'تاريخ السند يتجاوز تاريخ انتهاء الحساب.';
    END IF;
    IF v_requires_cc = 1 AND (NEW.Cost_Center_ID IS NULL OR TRIM(NEW.Cost_Center_ID) = '') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب المختار يتطلب مركز تكلفة.';
    END IF;
    IF v_requires_project = 1 AND (NEW.Project_ID IS NULL OR TRIM(NEW.Project_ID) = '') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب المختار يتطلب مشروعاً.';
    END IF;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS trg_financial_voucher_details_account_policy_bu;
DELIMITER $$
CREATE TRIGGER trg_financial_voucher_details_account_policy_bu
BEFORE UPDATE ON financial_voucher_details
FOR EACH ROW
BEGIN
    DECLARE v_company_id VARCHAR(50);
    DECLARE v_voucher_date DATE;
    DECLARE v_active TINYINT;
    DECLARE v_postable TINYINT;
    DECLARE v_summary TINYINT;
    DECLARE v_requires_cc TINYINT;
    DECLARE v_requires_project TINYINT;
    DECLARE v_effective_from DATE;
    DECLARE v_effective_to DATE;

    SELECT h.Company_ID, DATE(h.Voucher_Date)
      INTO v_company_id, v_voucher_date
      FROM financial_voucher_headers h
     WHERE h.Voucher_ID = NEW.Voucher_ID
     LIMIT 1;

    SELECT a.Is_Active, a.Is_Postable, a.Is_Summary_Account,
           a.Requires_CostCenter, a.Requires_Project,
           a.Effective_From, a.Effective_To
      INTO v_active, v_postable, v_summary,
           v_requires_cc, v_requires_project,
           v_effective_from, v_effective_to
      FROM chart_of_accounts a
     WHERE a.Account_ID = NEW.Account_ID
       AND a.Company_ID = v_company_id
     LIMIT 1;

    IF v_active IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب غير موجود ضمن شركة السند.';
    END IF;
    IF v_active = 0 OR v_postable = 0 OR v_summary = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب موقوف أو تجميعي أو غير قابل للترحيل.';
    END IF;
    IF v_effective_from IS NOT NULL AND v_voucher_date < v_effective_from THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'تاريخ السند يسبق تاريخ سريان الحساب.';
    END IF;
    IF v_effective_to IS NOT NULL AND v_voucher_date > v_effective_to THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'تاريخ السند يتجاوز تاريخ انتهاء الحساب.';
    END IF;
    IF v_requires_cc = 1 AND (NEW.Cost_Center_ID IS NULL OR TRIM(NEW.Cost_Center_ID) = '') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب المختار يتطلب مركز تكلفة.';
    END IF;
    IF v_requires_project = 1 AND (NEW.Project_ID IS NULL OR TRIM(NEW.Project_ID) = '') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'الحساب المختار يتطلب مشروعاً.';
    END IF;
END$$
DELIMITER ;

-- =========================================================
-- 6) قواعد اتساق التصنيف الأساسية
-- =========================================================

UPDATE chart_of_accounts
SET Normal_Balance = CASE
    WHEN Account_Type IN ('Asset','Expense') THEN 'Debit'
    WHEN Account_Type IN ('Liability','Equity','Revenue') THEN 'Credit'
    ELSE Normal_Balance
END
WHERE Normal_Balance IS NULL OR TRIM(Normal_Balance) = '';

-- لا يتم تعديل الحسابات التي لها طبيعة معتمدة مسبقاً؛ التحقق البرمجي يمنع التغيير بعد الحركة.

SELECT 'تم تطبيق ضوابط دليل الحسابات والتوجيه المالي المتقدم بنجاح.' AS Result;
