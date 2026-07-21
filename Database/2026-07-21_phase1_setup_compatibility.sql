-- توافق المرحلة الأولى مع قاعدة بيانات AlTayerERP الحالية.
-- شغّل هذا الملف مرة واحدة على قاعدة البيانات نفسها التي تتصل بها خدمة API.
-- آمن لإعادة التشغيل: ينشئ الجداول المفقودة ويضيف فقط الأعمدة الناقصة.
-- بعد التنفيذ أعد تشغيل API ثم حدّث شاشة سطح المكتب (F5).

CREATE TABLE IF NOT EXISTS system_settings
(
    Setting_ID INT NOT NULL AUTO_INCREMENT,
    Setting_Key VARCHAR(100) NOT NULL,
    Setting_Name VARCHAR(200) NOT NULL,
    Setting_Value TEXT NOT NULL,
    Scope VARCHAR(20) NOT NULL DEFAULT 'SYSTEM',
    Company_ID VARCHAR(50) NOT NULL DEFAULT '',
    Branch_ID INT NOT NULL DEFAULT 0,
    Fiscal_Year_ID INT NOT NULL DEFAULT 0,
    Effective_Date DATE NULL,
    Description TEXT NOT NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Setting_ID),
    UNIQUE KEY UQ_system_settings_scope
        (Setting_Key, Scope, Company_ID, Branch_ID, Fiscal_Year_ID),
    KEY IX_system_settings_context
        (Company_ID, Branch_ID, Fiscal_Year_ID, Is_Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS fiscal_periods
(
    Fiscal_Period_ID INT NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Branch_ID INT NOT NULL,
    Fiscal_Year_ID INT NOT NULL,
    Period_Code VARCHAR(50) NOT NULL,
    Period_Name VARCHAR(150) NOT NULL,
    Start_Date DATE NOT NULL,
    End_Date DATE NOT NULL,
    Is_Closed TINYINT(1) NOT NULL DEFAULT 0,
    Close_Date DATE NULL,
    Close_Reason VARCHAR(500) NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Fiscal_Period_ID),
    UNIQUE KEY UQ_fiscal_period_code
        (Company_ID, Branch_ID, Fiscal_Year_ID, Period_Code),
    KEY IX_fiscal_period_scope_dates
        (Company_ID, Branch_ID, Fiscal_Year_ID, Start_Date, End_Date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS exchange_rates
(
    Exchange_Rate_ID INT NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Currency_Code VARCHAR(20) NOT NULL,
    Rate_Date DATE NOT NULL,
    Exchange_Rate DECIMAL(18,6) NOT NULL,
    Min_Rate DECIMAL(18,6) NULL,
    Max_Rate DECIMAL(18,6) NULL,
    Is_Default TINYINT(1) NOT NULL DEFAULT 0,
    Notes VARCHAR(500) NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Exchange_Rate_ID),
    UNIQUE KEY UQ_exchange_rates_company_currency_date
        (Company_ID, Currency_Code, Rate_Date),
    KEY IX_exchange_rates_company_currency_active
        (Company_ID, Currency_Code, Is_Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- الجداول الثلاثة التالية موجودة في معظم النسخ القديمة، لكن عمود الترتيب
-- كان مفقوداً فيها؛ غيابه يسبب خطأ API 500 عند فتح الشاشات المرجعية.
ALTER TABLE payment_methods ADD COLUMN IF NOT EXISTS Sort_Order INT NOT NULL DEFAULT 0;
ALTER TABLE voucher_types ADD COLUMN IF NOT EXISTS Sort_Order INT NOT NULL DEFAULT 0;
ALTER TABLE voucher_statuses ADD COLUMN IF NOT EXISTS Sort_Order INT NOT NULL DEFAULT 0;

-- تحقق مختصر بعد التنفيذ:
-- SELECT TABLE_NAME FROM information_schema.TABLES
-- WHERE TABLE_SCHEMA = DATABASE()
--   AND TABLE_NAME IN ('system_settings','fiscal_periods','exchange_rates');
