-- أساس كتالوج الشاشات للمرحلة الأولى حتى سند القبض.
-- آمن لإعادة التشغيل: لا يغير السجل الموجود عند وجود Screen_Code نفسه.

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'Companies', 'الشركات', 'ADMIN', 1, 10, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'Companies');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'Branches', 'الفروع', 'ADMIN', 1, 20, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'Branches');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'FiscalYears', 'السنوات المالية', 'ADMIN', 1, 30, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'FiscalYears');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'Users', 'المستخدمون والصلاحيات', 'ADMIN', 1, 40, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'Users');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'Roles', 'الأدوار', 'ADMIN', 1, 50, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'Roles');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'NumberingSettings', 'إعدادات الترقيم', 'ADMIN', 1, 60, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'NumberingSettings');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'ChartOfAccounts', 'الدليل المحاسبي', 'ACCOUNTING', 1, 110, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'ChartOfAccounts');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'Currencies', 'العملات', 'ACCOUNTING', 1, 120, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'Currencies');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'CostCenters', 'مراكز التكلفة', 'ACCOUNTING', 1, 130, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'CostCenters');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'CashBoxes', 'الصناديق', 'ACCOUNTING', 1, 140, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'CashBoxes');

INSERT INTO system_screens
    (Screen_Code, Screen_Name, Module_Name, Is_Active, Sort_Order, Created_At)
SELECT 'ReceiptVoucher', 'سند القبض', 'ACCOUNTING', 1, 150, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM system_screens WHERE Screen_Code = 'ReceiptVoucher');

-- بعد تشغيل الملف: يمنح مدير النظام رؤية كاملة عبر Is_System_Admin.
-- أما الأدوار العادية فتمنح من شاشة الأدوار والصلاحيات حسب العمل الفعلي.
