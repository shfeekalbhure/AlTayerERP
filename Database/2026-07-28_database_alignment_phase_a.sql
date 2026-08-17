-- AlTayerERP
-- المرحلة A من مطابقة قاعدة البيانات مع المشروع
-- قاعدة البيانات: altayer_erp_db فقط
-- لا يحذف بيانات. يتوقف تلقائياً عند وجود حالة غير آمنة.

USE altayer_erp_db;

DROP PROCEDURE IF EXISTS sp_align_database_phase_a;
DELIMITER $$

CREATE PROCEDURE sp_align_database_phase_a()
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_count BIGINT DEFAULT 0;
    DECLARE v_old_bank_schema INT DEFAULT 0;
    DECLARE v_new_bank_schema INT DEFAULT 0;
    DECLARE v_duplicate_voucher_scope BIGINT DEFAULT 0;

    -- =====================================================
    -- A-1: مطابقة bank_accounts مع كيان BankAccount الحالي
    -- =====================================================

    SELECT COUNT(*) INTO v_exists
    FROM information_schema.tables
    WHERE table_schema = DATABASE()
      AND table_name = 'bank_accounts';

    IF v_exists = 1 THEN
        SELECT COUNT(*) INTO v_old_bank_schema
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'bank_accounts'
          AND column_name IN ('Account_Number', 'Account_ID');

        SELECT COUNT(*) INTO v_new_bank_schema
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'bank_accounts'
          AND column_name IN ('Account_No', 'Bank_Code', 'GL_Account', 'Branch_Name');

        IF v_old_bank_schema > 0 AND v_new_bank_schema = 0 THEN
            SELECT COUNT(*) INTO v_count FROM bank_accounts;

            IF v_count > 0 THEN
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'توقف آمن: جدول bank_accounts القديم يحتوي بيانات. يلزم ترحيل مخصص قبل تغييره.';
            END IF;

            SELECT COUNT(*) INTO v_exists
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
              AND table_name = 'bank_accounts_legacy';

            IF v_exists = 0 THEN
                RENAME TABLE bank_accounts TO bank_accounts_legacy;
            ELSE
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'توقف آمن: يوجد bank_accounts_legacy مسبقاً. راجع الجدولين قبل المتابعة.';
            END IF;
        END IF;
    END IF;

    CREATE TABLE IF NOT EXISTS bank_accounts
    (
        Bank_Account_ID INT NOT NULL AUTO_INCREMENT,
        Company_ID VARCHAR(50) NOT NULL,
        Bank_Code VARCHAR(50) NOT NULL DEFAULT '',
        Bank_Name_AR VARCHAR(200) NOT NULL,
        Bank_Name_EN VARCHAR(200) NULL,
        Account_No VARCHAR(100) NOT NULL,
        IBAN VARCHAR(64) NULL,
        Currency_Code VARCHAR(20) NOT NULL,
        GL_Account VARCHAR(100) NULL,
        Branch_Name VARCHAR(200) NULL,
        Is_Active TINYINT(1) NOT NULL DEFAULT 1,
        Notes TEXT NULL,
        Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        Updated_At DATETIME NULL,
        PRIMARY KEY (Bank_Account_ID),
        CONSTRAINT UQ_bank_accounts_company_account UNIQUE (Company_ID, Account_No),
        INDEX IX_bank_accounts_company_name (Company_ID, Bank_Name_AR),
        INDEX IX_bank_accounts_company_active (Company_ID, Is_Active)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    -- =====================================================
    -- A-2: تصحيح نطاق رقم السند
    -- الكود يعتمد الفرع + السنة + النوع + الرقم.
    -- =====================================================

    SELECT COUNT(*) INTO v_duplicate_voucher_scope
    FROM
    (
        SELECT Branch_ID, Fiscal_Year_ID, Voucher_Type_ID, Voucher_No
        FROM financial_voucher_headers
        GROUP BY Branch_ID, Fiscal_Year_ID, Voucher_Type_ID, Voucher_No
        HAVING COUNT(*) > 1
    ) duplicates;

    IF v_duplicate_voucher_scope > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'توقف آمن: توجد أرقام سندات مكررة داخل نفس الفرع والسنة والنوع.';
    END IF;

    SELECT COUNT(*) INTO v_exists
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = 'financial_voucher_headers'
      AND index_name = 'UQ_Financial_Voucher_Branch_Year_Type_No';

    IF v_exists = 0 THEN
        ALTER TABLE financial_voucher_headers
            ADD UNIQUE KEY UQ_Financial_Voucher_Branch_Year_Type_No
            (Branch_ID, Fiscal_Year_ID, Voucher_Type_ID, Voucher_No);
    END IF;

    SELECT COUNT(*) INTO v_exists
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = 'financial_voucher_headers'
      AND index_name = 'UQ_Financial_Voucher_No';

    IF v_exists > 0 THEN
        ALTER TABLE financial_voucher_headers
            DROP INDEX UQ_Financial_Voucher_No;
    END IF;
END$$

DELIMITER ;

CALL sp_align_database_phase_a();
DROP PROCEDURE IF EXISTS sp_align_database_phase_a;

-- =========================================================
-- التحقق النهائي
-- =========================================================

SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = 'bank_accounts'
  AND column_name IN
      ('Bank_Account_ID','Company_ID','Bank_Code','Account_No','GL_Account','Branch_Name')
ORDER BY ORDINAL_POSITION;

SELECT INDEX_NAME, GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX) AS Index_Columns
FROM information_schema.statistics
WHERE table_schema = DATABASE()
  AND table_name = 'financial_voucher_headers'
  AND INDEX_NAME IN
      ('UQ_Financial_Voucher_No','UQ_Financial_Voucher_Branch_Year_Type_No')
GROUP BY INDEX_NAME;

SELECT 'تم تنفيذ المرحلة A من مطابقة قاعدة البيانات بنجاح.' AS Result;
