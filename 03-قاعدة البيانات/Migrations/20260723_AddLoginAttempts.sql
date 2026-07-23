-- المرحلة الأولى / الرقابة الأمنية
-- Migration: 20260723_AddLoginAttempts.sql

CREATE TABLE login_attempts (
    Login_Attempt_ID BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    User_ID INT NULL,
    Company_ID VARCHAR(50) NULL,
    Branch_ID INT NULL,
    Login_Name VARCHAR(100) NULL,
    Is_Success TINYINT(1) NOT NULL,
    Failure_Reason VARCHAR(200) NULL,
    IP_Address VARCHAR(64) NULL,
    Device_ID VARCHAR(100) NULL,
    Attempted_At DATETIME(6) NOT NULL,
    PRIMARY KEY (Login_Attempt_ID),
    INDEX IX_LoginAttempts_LoginTime (Login_Name, Attempted_At),
    INDEX IX_LoginAttempts_CompanyTime (Company_ID, Attempted_At)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;