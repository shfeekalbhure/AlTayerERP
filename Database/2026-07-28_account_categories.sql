-- ============================================================
-- AlTayerERP - جدول تصنيفات دليل الحسابات
-- القاعدة المستهدفة: altayer_erp_db فقط
-- آمن للتكرار ولا يحذف أي بيانات
-- ============================================================

USE altayer_erp_db;

CREATE TABLE IF NOT EXISTS account_categories (
    Category_ID          VARCHAR(50)  NOT NULL,
    Company_ID           VARCHAR(50)  NOT NULL,
    Category_Code        VARCHAR(50)  NOT NULL,
    Category_Name_AR     VARCHAR(150) NOT NULL,
    Category_Name_EN     VARCHAR(150) NULL,
    Account_Type         VARCHAR(30)  NOT NULL,
    Normal_Balance       VARCHAR(10)  NOT NULL,
    Is_System            TINYINT(1)   NOT NULL DEFAULT 0,
    Is_Active            TINYINT(1)   NOT NULL DEFAULT 1,
    Sort_Order           INT          NOT NULL DEFAULT 0,
    Created_At           DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Created_By           VARCHAR(100) NULL,
    Updated_At           DATETIME(6)  NULL,
    Updated_By           VARCHAR(100) NULL,
    PRIMARY KEY (Category_ID),
    UNIQUE KEY UQ_AccountCategories_Company_Code (Company_ID, Category_Code),
    KEY IX_AccountCategories_Company_Type_Active (Company_ID, Account_Type, Is_Active, Sort_Order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- إدراج التصنيفات النظامية لكل شركة موجودة، دون تكرار.
INSERT INTO account_categories
(Category_ID, Company_ID, Category_Code, Category_Name_AR, Category_Name_EN,
 Account_Type, Normal_Balance, Is_System, Is_Active, Sort_Order, Created_At, Created_By)
SELECT UUID(), c.Company_ID, s.Category_Code, s.Category_Name_AR, s.Category_Name_EN,
       s.Account_Type, s.Normal_Balance, 1, 1, s.Sort_Order, UTC_TIMESTAMP(6), 'SYSTEM'
FROM companies c
CROSS JOIN (
    SELECT 'Cash' Category_Code, 'نقدية وصناديق' Category_Name_AR, 'Cash' Category_Name_EN, 'Asset' Account_Type, 'Debit' Normal_Balance, 10 Sort_Order
    UNION ALL SELECT 'Bank', 'بنوك', 'Bank', 'Asset', 'Debit', 20
    UNION ALL SELECT 'Receivable', 'ذمم مدينة', 'Receivable', 'Asset', 'Debit', 30
    UNION ALL SELECT 'Customer', 'عملاء', 'Customer', 'Asset', 'Debit', 40
    UNION ALL SELECT 'Inventory', 'مخزون', 'Inventory', 'Asset', 'Debit', 50
    UNION ALL SELECT 'FixedAsset', 'أصول ثابتة', 'Fixed Asset', 'Asset', 'Debit', 60
    UNION ALL SELECT 'OtherAsset', 'أصول أخرى', 'Other Asset', 'Asset', 'Debit', 90
    UNION ALL SELECT 'Payable', 'ذمم دائنة', 'Payable', 'Liability', 'Credit', 110
    UNION ALL SELECT 'Vendor', 'موردون', 'Vendor', 'Liability', 'Credit', 120
    UNION ALL SELECT 'Loan', 'قروض', 'Loan', 'Liability', 'Credit', 130
    UNION ALL SELECT 'TaxPayable', 'ضرائب مستحقة', 'Tax Payable', 'Liability', 'Credit', 140
    UNION ALL SELECT 'OtherLiability', 'خصوم أخرى', 'Other Liability', 'Liability', 'Credit', 190
    UNION ALL SELECT 'Capital', 'رأس المال', 'Capital', 'Equity', 'Credit', 210
    UNION ALL SELECT 'Reserve', 'احتياطيات', 'Reserve', 'Equity', 'Credit', 220
    UNION ALL SELECT 'RetainedEarnings', 'أرباح محتجزة', 'Retained Earnings', 'Equity', 'Credit', 230
    UNION ALL SELECT 'OtherEquity', 'حقوق ملكية أخرى', 'Other Equity', 'Equity', 'Credit', 290
    UNION ALL SELECT 'TransportRevenue', 'إيرادات نقل', 'Transport Revenue', 'Revenue', 'Credit', 310
    UNION ALL SELECT 'ShippingRevenue', 'إيرادات شحن', 'Shipping Revenue', 'Revenue', 'Credit', 320
    UNION ALL SELECT 'TicketRevenue', 'إيرادات تذاكر', 'Ticket Revenue', 'Revenue', 'Credit', 330
    UNION ALL SELECT 'ServiceRevenue', 'إيرادات خدمات', 'Service Revenue', 'Revenue', 'Credit', 340
    UNION ALL SELECT 'OtherRevenue', 'إيرادات أخرى', 'Other Revenue', 'Revenue', 'Credit', 390
    UNION ALL SELECT 'FuelExpense', 'مصروف وقود', 'Fuel Expense', 'Expense', 'Debit', 410
    UNION ALL SELECT 'SalaryExpense', 'مصروف رواتب', 'Salary Expense', 'Expense', 'Debit', 420
    UNION ALL SELECT 'MaintenanceExpense', 'مصروف صيانة', 'Maintenance Expense', 'Expense', 'Debit', 430
    UNION ALL SELECT 'RentExpense', 'مصروف إيجارات', 'Rent Expense', 'Expense', 'Debit', 440
    UNION ALL SELECT 'OperatingExpense', 'مصروفات تشغيلية', 'Operating Expense', 'Expense', 'Debit', 450
    UNION ALL SELECT 'AdministrativeExpense', 'مصروفات إدارية', 'Administrative Expense', 'Expense', 'Debit', 460
    UNION ALL SELECT 'FinanceCost', 'تكاليف تمويل', 'Finance Cost', 'Expense', 'Debit', 470
    UNION ALL SELECT 'DepreciationExpense', 'مصروف إهلاك', 'Depreciation Expense', 'Expense', 'Debit', 480
    UNION ALL SELECT 'OtherExpense', 'مصروفات أخرى', 'Other Expense', 'Expense', 'Debit', 490
) s
WHERE NOT EXISTS (
    SELECT 1
    FROM account_categories ac
    WHERE ac.Company_ID = c.Company_ID
      AND ac.Category_Code = s.Category_Code
);

SELECT Company_ID, Account_Type, COUNT(*) AS Categories_Count
FROM account_categories
GROUP BY Company_ID, Account_Type
ORDER BY Company_ID, Account_Type;
