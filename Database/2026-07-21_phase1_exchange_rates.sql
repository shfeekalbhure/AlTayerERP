-- المرحلة الأولى: سجل أسعار الصرف.
-- شغّل هذا الملف مرة واحدة قبل استخدام شاشة "أسعار الصرف".

CREATE TABLE IF NOT EXISTS exchange_rates
(
    Exchange_Rate_ID INT NOT NULL AUTO_INCREMENT,
    Company_ID       VARCHAR(50) NOT NULL,
    Currency_Code    VARCHAR(20) NOT NULL,
    Rate_Date        DATE NOT NULL,
    Exchange_Rate    DECIMAL(18,6) NOT NULL,
    Min_Rate         DECIMAL(18,6) NULL,
    Max_Rate         DECIMAL(18,6) NULL,
    Is_Default       TINYINT(1) NOT NULL DEFAULT 0,
    Notes            VARCHAR(500) NULL,
    Is_Active        TINYINT(1) NOT NULL DEFAULT 1,
    Created_At       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At       DATETIME NULL,
    PRIMARY KEY (Exchange_Rate_ID),
    UNIQUE KEY UQ_exchange_rates_company_currency_date
        (Company_ID, Currency_Code, Rate_Date),
    KEY IX_exchange_rates_company_currency_active
        (Company_ID, Currency_Code, Is_Active)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;

-- يتم منع سعر العملة المحلية غير المساوي لـ 1 والتحقق من الحدود في API.
