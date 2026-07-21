-- قيم مرجعية أولية لإعدادات المرحلة الأولى حتى سند القبض.
-- آمن لإعادة التشغيل ولا يبدل القيم المعتمدة مسبقاً.

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'System.PasswordMinLength', 'الحد الأدنى لطول كلمة المرور', 'ADMIN', 'INTEGER', '8', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'System.PasswordMinLength'
);

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'ReceiptVoucher.RequiresApproval', 'سند القبض يتطلب اعتماداً', 'ACCOUNTING', 'BOOLEAN', 'true', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'ReceiptVoucher.RequiresApproval'
);

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'ReceiptVoucher.PreventCreatorApproval', 'منع منشئ سند القبض من اعتماده', 'ACCOUNTING', 'BOOLEAN', 'true', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'ReceiptVoucher.PreventCreatorApproval'
);

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'ReceiptVoucher.RequireUnpostReason', 'إلزام سبب عند فك ترحيل سند القبض', 'ACCOUNTING', 'BOOLEAN', 'true', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'ReceiptVoucher.RequireUnpostReason'
);

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'ReceiptVoucher.AllowForeignCurrency', 'السماح بالعملة الأجنبية في سند القبض', 'ACCOUNTING', 'BOOLEAN', 'true', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'ReceiptVoucher.AllowForeignCurrency'
);

INSERT INTO system_settings
    (Setting_Code, Setting_Name, Module_Name, Data_Type, Default_Value, Is_Sensitive, Is_Active, Created_At)
SELECT 'Accounting.RequireBalancedVoucher', 'منع حفظ السند غير المتوازن', 'ACCOUNTING', 'BOOLEAN', 'true', 1, 1, UTC_TIMESTAMP()
WHERE NOT EXISTS (
    SELECT 1 FROM system_settings WHERE Setting_Code = 'Accounting.RequireBalancedVoucher'
);