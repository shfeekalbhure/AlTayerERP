-- AlTayerERP 4.2 - ترقية آمنة للعدادات المركزية
-- قاعدة الهدف الوحيدة: altayer_erp_db
-- لا يحذف هذا الملف بيانات ولا يعيد ضبط Last_Number ولا يعيد استخدام أي رقم.
USE altayer_erp_db;

DELIMITER $$

DROP PROCEDURE IF EXISTS Ensure_Numbering_Counter_Scope_Index $$
CREATE PROCEDURE Ensure_Numbering_Counter_Scope_Index()
BEGIN
    -- نفحص التكرارات كما ستصبح بعد تطبيع NULL إلى ''/0 قبل أي كتابة.
    IF EXISTS
    (
        SELECT 1
        FROM numbering_counters
        GROUP BY
            Document_Type,
            COALESCE(Company_ID, ''),
            COALESCE(Branch_ID, 0),
            COALESCE(Year_Value, 0)
        HAVING COUNT(*) > 1
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'لم ينشأ فهرس UQ_Numbering_Counter_Scope: توجد عدادات متكررة. راجع التقرير أولاً ولا تحذف أي سجل.';
    END IF;

    -- توحيد مفتاح النطاق للقيم غير المستخدمة حتى يعمل UNIQUE في MySQL.
    UPDATE numbering_counters SET Company_ID = '' WHERE Company_ID IS NULL;
    UPDATE numbering_counters SET Branch_ID = 0 WHERE Branch_ID IS NULL;
    UPDATE numbering_counters SET Year_Value = 0 WHERE Year_Value IS NULL;

    IF NOT EXISTS
    (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'numbering_counters'
          AND index_name = 'UQ_Numbering_Counter_Scope'
    ) THEN
        CREATE UNIQUE INDEX UQ_Numbering_Counter_Scope
            ON numbering_counters (Document_Type, Company_ID, Branch_ID, Year_Value);
    END IF;
END $$

CALL Ensure_Numbering_Counter_Scope_Index() $$
DROP PROCEDURE Ensure_Numbering_Counter_Scope_Index $$

DELIMITER ;

-- تقرير قراءة فقط بعد التنفيذ: يجب ألا يعيد أي صف.
SELECT
    Document_Type,
    Company_ID,
    Branch_ID,
    Year_Value,
    COUNT(*) AS Duplicate_Count
FROM numbering_counters
GROUP BY Document_Type, Company_ID, Branch_ID, Year_Value
HAVING COUNT(*) > 1;
