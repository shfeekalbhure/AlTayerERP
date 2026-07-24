-- AlTayerERP 4.2 | ترقية آمنة لحزمة الجلسة الآمنة وتسجيل محاولات الدخول.
-- الهدف: قاعدة altayer_erp_db الحالية فقط. لا تحذف هذا الملف بيانات أو جداول،
-- ولا ينشئ فهرس UQ_Tenant_Branches_Company_Code المؤجل إلى أن تعالج التكرارات.
USE altayer_erp_db;

-- حقول قفل الحساب والتدقيق الأمني؛ جميعها يكتبها الـ API فقط.
ALTER TABLE users
    ADD COLUMN IF NOT EXISTS Failed_Login_Count INT NOT NULL DEFAULT 0 AFTER Updated_At,
    ADD COLUMN IF NOT EXISTS Last_Failed_Login_At DATETIME(6) NULL AFTER Failed_Login_Count,
    ADD COLUMN IF NOT EXISTS Locked_Until DATETIME(6) NULL AFTER Last_Failed_Login_At,
    ADD COLUMN IF NOT EXISTS Last_Login_At DATETIME(6) NULL AFTER Locked_Until,
    ADD COLUMN IF NOT EXISTS Last_Login_IP VARCHAR(64) NULL AFTER Last_Login_At;

-- سجل مستقل للمحاولات الناجحة والفاشلة؛ لا يحتوي Password أو Password_Hash.
CREATE TABLE IF NOT EXISTS login_attempts
(
    Login_Attempt_ID BIGINT NOT NULL AUTO_INCREMENT,
    User_ID INT NULL,
    Login_Name VARCHAR(100) NOT NULL,
    Company_ID VARCHAR(50) NULL,
    Branch_ID INT NULL,
    Fiscal_Year_ID INT NULL,
    Attempted_At DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Is_Success TINYINT(1) NOT NULL,
    Failure_Reason VARCHAR(100) NULL,
    IP_Address VARCHAR(64) NULL,
    User_Agent VARCHAR(512) NULL,
    Device_ID VARCHAR(128) NULL,
    Session_ID VARCHAR(64) NULL,
    Lockout_Until DATETIME(6) NULL,
    PRIMARY KEY (Login_Attempt_ID),
    INDEX IX_Login_Attempts_Login_At (Login_Name, Attempted_At),
    INDEX IX_Login_Attempts_User_At (User_ID, Attempted_At),
    INDEX IX_Login_Attempts_Session (Session_ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- يخزن Token_Hash فقط؛ لا يمكن استخراج Refresh Token الأصلي من هذا الجدول.
CREATE TABLE IF NOT EXISTS refresh_tokens
(
    Refresh_Token_ID BIGINT NOT NULL AUTO_INCREMENT,
    Token_Hash CHAR(64) NOT NULL,
    Session_ID VARCHAR(64) NOT NULL,
    User_ID INT NOT NULL,
    Company_ID VARCHAR(50) NOT NULL,
    Branch_ID INT NOT NULL,
    Fiscal_Year_ID INT NOT NULL,
    Device_ID VARCHAR(128) NOT NULL,
    Created_At DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Expires_At DATETIME(6) NOT NULL,
    Revoked_At DATETIME(6) NULL,
    Replaced_By_Hash CHAR(64) NULL,
    Revoked_Reason VARCHAR(100) NULL,
    PRIMARY KEY (Refresh_Token_ID),
    UNIQUE KEY UQ_Refresh_Tokens_Hash (Token_Hash),
    INDEX IX_Refresh_Tokens_User_Expiry (User_ID, Expires_At),
    INDEX IX_Refresh_Tokens_Session (Session_ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- تحقق بعد التنفيذ: يعرض الحقول والجداول والفهارس فقط ولا يغير أي بيانات.
SHOW COLUMNS FROM users WHERE Field IN
    ('Failed_Login_Count','Last_Failed_Login_At','Locked_Until','Last_Login_At','Last_Login_IP');
SHOW INDEX FROM login_attempts;
SHOW INDEX FROM refresh_tokens;
