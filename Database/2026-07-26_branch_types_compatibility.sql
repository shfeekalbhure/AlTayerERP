-- المرحلة الأولى | توافق أنواع الفروع
-- قاعدة البيانات المعتمدة فقط: altayer_erp_db
-- آمن لإعادة التشغيل: لا يحذف بيانات أو جداول أو مفاتيح قائمة.

USE altayer_erp_db;

CREATE TABLE IF NOT EXISTS branch_types
(
    Branch_Type_ID INT NOT NULL AUTO_INCREMENT,
    Branch_Type_Code VARCHAR(30) NOT NULL,
    Branch_Type_Name_AR VARCHAR(100) NOT NULL,
    Branch_Type_Name_EN VARCHAR(100) NULL,
    Sort_Order INT NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Branch_Type_ID),
    UNIQUE KEY UQ_branch_types_code (Branch_Type_Code),
    KEY IX_branch_types_active_sort (Is_Active, Sort_Order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO branch_types
    (Branch_Type_Code, Branch_Type_Name_AR, Branch_Type_Name_EN, Sort_Order, Is_Active)
VALUES
    ('MAIN', 'رئيسي', 'Main', 10, 1),
    ('OPERATING', 'تشغيلي', 'Operating', 20, 1),
    ('DISTRIBUTION', 'نقطة توزيع', 'Distribution Point', 30, 1),
    ('WAREHOUSE', 'مستودع', 'Warehouse', 40, 1)
ON DUPLICATE KEY UPDATE
    Branch_Type_Name_AR = VALUES(Branch_Type_Name_AR),
    Branch_Type_Name_EN = VALUES(Branch_Type_Name_EN),
    Sort_Order = VALUES(Sort_Order),
    Is_Active = VALUES(Is_Active),
    Updated_At = UTC_TIMESTAMP();

-- تحقق بعد التنفيذ:
-- SELECT Branch_Type_ID, Branch_Type_Code, Branch_Type_Name_AR, Is_Active
-- FROM branch_types ORDER BY Sort_Order, Branch_Type_Name_AR;
