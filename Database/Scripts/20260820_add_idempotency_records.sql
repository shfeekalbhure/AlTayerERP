-- AlTayerERP — ترحيل حماية إعادة الإرسال للسندات المالية
-- الغرض: يمنع إنشاء سند ثانٍ عندما يعيد العميل إرسال Idempotency-Key نفسه.
-- لا يُنفذ تلقائياً ولا يغيّر بيانات قائمة. ينفذ فقط ضمن نافذة ترحيل مع نسخة احتياطية واختبار استرجاع.

CREATE TABLE IF NOT EXISTS `idempotency_records` (
    `Idempotency_Record_ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Operation` VARCHAR(80) NOT NULL,
    `Idempotency_Key` VARCHAR(100) NOT NULL,
    `Company_ID` VARCHAR(50) NOT NULL,
    `Branch_ID` VARCHAR(50) NOT NULL,
    `Fiscal_Year_ID` INT NOT NULL,
    `User_ID` VARCHAR(50) NOT NULL,
    `Request_Fingerprint` CHAR(64) NOT NULL,
    `Status` TINYINT UNSIGNED NOT NULL,
    `Resource_ID` BIGINT NULL,
    `Resource_No` VARCHAR(100) NULL,
    `Created_At` DATETIME(6) NOT NULL,
    `Completed_At` DATETIME(6) NULL,
    PRIMARY KEY (`Idempotency_Record_ID`),
    UNIQUE KEY `UQ_Idempotency_Record_Scope_Key`
        (`Operation`, `Idempotency_Key`, `Company_ID`, `Branch_ID`, `Fiscal_Year_ID`, `User_ID`),
    KEY `IX_Idempotency_Record_Status_Created` (`Status`, `Created_At`)
) CHARACTER SET utf8mb4;

-- تحقق بعد التنفيذ، ولا تنفذ هذا الملف على الإنتاج قبل مراجعة DBA:
-- SHOW CREATE TABLE `idempotency_records`;
