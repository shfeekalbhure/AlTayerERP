-- استكمال شاشة سند القبض: اسم المستلم منه ودورة المراجعة الرقابية.
-- الملف آمن لإعادة التشغيل؛ كل عمود يضاف فقط إذا كان غير موجود.

SET @ddl = IF(
    EXISTS(
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'financial_voucher_headers'
          AND COLUMN_NAME = 'Received_From_Name'
    ),
    'SELECT 1',
    'ALTER TABLE financial_voucher_headers ADD COLUMN `Received_From_Name` VARCHAR(200) NULL AFTER `Party_ID`'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @ddl = IF(
    EXISTS(
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'financial_voucher_headers'
          AND COLUMN_NAME = 'Review_Status'
    ),
    'SELECT 1',
    'ALTER TABLE financial_voucher_headers ADD COLUMN `Review_Status` TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER `Approval_Status`'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @ddl = IF(
    EXISTS(
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'financial_voucher_headers'
          AND COLUMN_NAME = 'Reviewed_By_User_ID'
    ),
    'SELECT 1',
    'ALTER TABLE financial_voucher_headers ADD COLUMN `Reviewed_By_User_ID` VARCHAR(50) NULL AFTER `Review_Status`'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @ddl = IF(
    EXISTS(
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'financial_voucher_headers'
          AND COLUMN_NAME = 'Reviewed_At'
    ),
    'SELECT 1',
    'ALTER TABLE financial_voucher_headers ADD COLUMN `Reviewed_At` DATETIME NULL AFTER `Reviewed_By_User_ID`'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @ddl = IF(
    EXISTS(
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'financial_voucher_headers'
          AND COLUMN_NAME = 'Review_Notes'
    ),
    'SELECT 1',
    'ALTER TABLE financial_voucher_headers ADD COLUMN `Review_Notes` VARCHAR(500) NULL AFTER `Reviewed_At`'
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- تعبئة أسماء الأطراف في السندات القديمة التي كانت تحفظ Party_ID فقط.
-- السندات القديمة التي لم تحفظ Party_ID لا يمكن استرجاع الاسم لها تلقائيًا.
UPDATE financial_voucher_headers AS h
LEFT JOIN parties AS p ON p.Party_ID = h.Party_ID
SET h.Received_From_Name = p.Party_Name_AR
WHERE h.Voucher_ID > 0
  AND h.Party_ID IS NOT NULL
  AND (h.Received_From_Name IS NULL OR h.Received_From_Name = '');

-- فحص الأعمدة بعد التنفيذ.
SELECT
    Voucher_ID,
    Voucher_No,
    Party_ID,
    Received_From_Name,
    Review_Status,
    Reviewed_By_User_ID,
    Reviewed_At,
    Review_Notes
FROM financial_voucher_headers
ORDER BY Voucher_ID DESC
LIMIT 20;
