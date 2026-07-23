-- المرحلة الأولى: جدول الحسابات البنكية التشغيلية
-- شغّل هذا الملف مرة واحدة على قاعدة بيانات AlTayerERP قبل استخدام شاشة البنوك.

CREATE TABLE IF NOT EXISTS bank_accounts (
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
    INDEX IX_bank_accounts_company_name (Company_ID, Bank_Name_AR)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
