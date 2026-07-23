-- المرحلة الأولى: الفترات المالية.
-- شغّل هذا الملف مرة واحدة قبل استخدام شاشة "الفترات المالية".

CREATE TABLE IF NOT EXISTS fiscal_periods
(
    Fiscal_Period_ID INT NOT NULL AUTO_INCREMENT,
    Company_ID       VARCHAR(50) NOT NULL,
    Branch_ID        INT NOT NULL,
    Fiscal_Year_ID   INT NOT NULL,
    Period_Code      VARCHAR(50) NOT NULL,
    Period_Name      VARCHAR(150) NOT NULL,
    Start_Date       DATE NOT NULL,
    End_Date         DATE NOT NULL,
    Is_Closed        TINYINT(1) NOT NULL DEFAULT 0,
    Close_Date       DATE NULL,
    Close_Reason     VARCHAR(500) NULL,
    Is_Active        TINYINT(1) NOT NULL DEFAULT 1,
    Created_At       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At       DATETIME NULL,
    PRIMARY KEY (Fiscal_Period_ID),
    UNIQUE KEY UQ_fiscal_period_code
        (Company_ID, Branch_ID, Fiscal_Year_ID, Period_Code),
    KEY IX_fiscal_period_scope_dates
        (Company_ID, Branch_ID, Fiscal_Year_ID, Start_Date, End_Date)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;

-- التداخل بين الفترات يتم التحقق منه في API حتى يشمل الحالات التي لا تغطيها الفهارس.
