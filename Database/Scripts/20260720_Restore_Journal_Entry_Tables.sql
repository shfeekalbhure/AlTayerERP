-- استعادة جداول القيود المحاسبية التي يعتمد عليها الترحيل وإلغاء الترحيل.
-- الملف آمن لإعادة التشغيل: لا يعيد إنشاء الجدول إذا كان موجودًا.

CREATE TABLE IF NOT EXISTS `journal_entry_headers` (
    `Journal_Entry_ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Entry_No` VARCHAR(50) NOT NULL,
    `Entry_Type` TINYINT UNSIGNED NOT NULL,
    `Entry_Status_ID` INT NOT NULL,
    `Branch_ID` VARCHAR(50) NOT NULL,
    `Fiscal_Year_ID` INT NULL,
    `Entry_Date` DATETIME(6) NOT NULL,
    `Transaction_Date` DATETIME(6) NOT NULL,
    `Source_System` VARCHAR(30) NOT NULL,
    `Is_System_Generated` TINYINT(1) NOT NULL,
    `Source_Voucher_ID` BIGINT NULL,
    `Source_Document_Type` VARCHAR(50) NULL,
    `Source_Document_No` VARCHAR(100) NULL,
    `Description` VARCHAR(500) NULL,
    `Notes` VARCHAR(1000) NULL,
    `Total_Debit` DECIMAL(18,2) NOT NULL,
    `Total_Credit` DECIMAL(18,2) NOT NULL,
    `Is_Posted` TINYINT(1) NOT NULL,
    `Posted_By` VARCHAR(50) NULL,
    `Posted_At` DATETIME(6) NULL,
    `Is_Reversal` TINYINT(1) NOT NULL,
    `Original_Journal_Entry_ID` BIGINT NULL,
    `Is_Reversed` TINYINT(1) NOT NULL,
    `Reversal_Journal_Entry_ID` BIGINT NULL,
    `Reversal_Reason` VARCHAR(500) NULL,
    `Reversed_By` VARCHAR(50) NULL,
    `Reversed_At` DATETIME(6) NULL,
    `Is_Cancelled` TINYINT(1) NOT NULL,
    `Cancellation_Reason` VARCHAR(500) NULL,
    `Cancelled_By` VARCHAR(50) NULL,
    `Cancelled_At` DATETIME(6) NULL,
    `Is_Active` TINYINT(1) NOT NULL,
    `Created_By` VARCHAR(50) NULL,
    `Created_At` DATETIME(6) NOT NULL,
    `Updated_By` VARCHAR(50) NULL,
    `Updated_At` DATETIME(6) NULL,
    PRIMARY KEY (`Journal_Entry_ID`),
    UNIQUE KEY `UQ_Journal_Entry_No` (`Entry_No`),
    KEY `IX_Journal_Entry_Date` (`Entry_Date`),
    KEY `IX_Journal_Entry_Source_Voucher` (`Source_Voucher_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `journal_entry_details` (
    `Journal_Entry_Detail_ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Journal_Entry_ID` BIGINT NOT NULL,
    `Line_No` INT NOT NULL,
    `Account_ID` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `Cost_Center_ID` VARCHAR(50) NULL,
    `Project_ID` VARCHAR(50) NULL,
    `Currency_ID` INT NOT NULL,
    `Exchange_Rate` DECIMAL(18,6) NOT NULL,
    `Foreign_Amount` DECIMAL(18,2) NOT NULL,
    `Local_Amount` DECIMAL(18,2) NOT NULL,
    `Debit_Amount` DECIMAL(18,2) NOT NULL,
    `Credit_Amount` DECIMAL(18,2) NOT NULL,
    `Reference_Type` VARCHAR(50) NULL,
    `Reference_No` VARCHAR(100) NULL,
    `Reference_Name` VARCHAR(250) NULL,
    `Reference_Date` DATETIME(6) NULL,
    `Source_Voucher_Detail_ID` BIGINT NULL,
    `Line_Type` TINYINT UNSIGNED NOT NULL,
    `Notes` VARCHAR(500) NULL,
    `Created_By` VARCHAR(50) NULL,
    `Created_At` DATETIME(6) NOT NULL,
    `Updated_By` VARCHAR(50) NULL,
    `Updated_At` DATETIME(6) NULL,
    PRIMARY KEY (`Journal_Entry_Detail_ID`),
    UNIQUE KEY `UQ_Journal_Entry_Detail_Line` (`Journal_Entry_ID`, `Line_No`),
    KEY `IX_Journal_Entry_Details_Header` (`Journal_Entry_ID`),
    CONSTRAINT `FK_journal_entry_details_journal_entry_headers_Journal_Entry_ID`
        FOREIGN KEY (`Journal_Entry_ID`)
        REFERENCES `journal_entry_headers` (`Journal_Entry_ID`)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- لأن الجداول المحذوفة لا يمكنها الاحتفاظ بالقيود القديمة، نعيد أي سند
-- ما زال يشير إلى قيد مفقود إلى حالة غير مرحل حتى يمكن ترحيله من جديد.
UPDATE `financial_voucher_headers` AS h
LEFT JOIN `journal_entry_headers` AS j
    ON j.`Journal_Entry_ID` = h.`Journal_Entry_ID`
SET
    h.`Is_Posted` = 0,
    h.`Journal_Entry_ID` = NULL,
    h.`Posted_By_User_ID` = NULL,
    h.`Posted_At` = NULL
WHERE h.`Journal_Entry_ID` IS NOT NULL
  AND j.`Journal_Entry_ID` IS NULL;

SELECT
    (SELECT COUNT(*) FROM `journal_entry_headers`) AS Journal_Header_Count,
    (SELECT COUNT(*) FROM `journal_entry_details`) AS Journal_Detail_Count;
