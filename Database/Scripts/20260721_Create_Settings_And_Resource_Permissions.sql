-- محرك الإعدادات الهرمية والصلاحيات الدقيقة: الحقول والأزرار.
-- لا ينشئ أي صلاحية فعلية للمستخدمين؛ يبقى المنع الافتراضي هو السياسة.

CREATE TABLE IF NOT EXISTS system_settings (
    Setting_ID BIGINT NOT NULL AUTO_INCREMENT,
    Setting_Code VARCHAR(150) NOT NULL,
    Setting_Name VARCHAR(200) NOT NULL,
    Module_Name VARCHAR(100) NULL,
    Data_Type VARCHAR(30) NOT NULL,
    Default_Value TEXT NULL,
    Is_Sensitive TINYINT(1) NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_At DATETIME(6) NOT NULL,
    PRIMARY KEY (Setting_ID),
    UNIQUE KEY UQ_System_Settings_Code (Setting_Code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS setting_scope_values (
    Setting_Scope_Value_ID BIGINT NOT NULL AUTO_INCREMENT,
    Setting_ID BIGINT NOT NULL,
    Scope_Type VARCHAR(30) NOT NULL,
    Scope_ID VARCHAR(100) NOT NULL,
    Value TEXT NULL,
    Effective_From DATETIME(6) NULL,
    Effective_To DATETIME(6) NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Created_By VARCHAR(100) NOT NULL,
    Created_At DATETIME(6) NOT NULL,
    Change_Reason VARCHAR(500) NULL,
    PRIMARY KEY (Setting_Scope_Value_ID),
    KEY IX_Setting_Scope_Value_Active (Setting_ID, Is_Active),
    KEY IX_Setting_Scope_Value_Resolution (Setting_ID, Scope_Type, Scope_ID, Is_Active),
    CONSTRAINT FK_Setting_Scope_Values_Settings
        FOREIGN KEY (Setting_ID) REFERENCES system_settings (Setting_ID)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS system_screen_fields (
    Screen_Field_ID BIGINT NOT NULL AUTO_INCREMENT,
    Screen_ID INT NOT NULL,
    Field_Code VARCHAR(150) NOT NULL,
    Field_Name VARCHAR(200) NOT NULL,
    Is_Sensitive TINYINT(1) NOT NULL DEFAULT 0,
    Default_Required TINYINT(1) NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Sort_Order INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Screen_Field_ID),
    UNIQUE KEY UQ_System_Screen_Field (Screen_ID, Field_Code),
    CONSTRAINT FK_Screen_Fields_Screens
        FOREIGN KEY (Screen_ID) REFERENCES system_screens (Screen_ID)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS system_screen_actions (
    Screen_Action_ID BIGINT NOT NULL AUTO_INCREMENT,
    Screen_ID INT NOT NULL,
    Action_Code VARCHAR(100) NOT NULL,
    Action_Name VARCHAR(200) NOT NULL,
    Is_Sensitive TINYINT(1) NOT NULL DEFAULT 0,
    Requires_Reason TINYINT(1) NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Sort_Order INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Screen_Action_ID),
    UNIQUE KEY UQ_System_Screen_Action (Screen_ID, Action_Code),
    CONSTRAINT FK_Screen_Actions_Screens
        FOREIGN KEY (Screen_ID) REFERENCES system_screens (Screen_ID)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS role_resource_permissions (
    Role_Resource_Permission_ID BIGINT NOT NULL AUTO_INCREMENT,
    Role_ID INT NOT NULL,
    Resource_Type VARCHAR(30) NOT NULL,
    Resource_Code VARCHAR(150) NOT NULL,
    Permission_Code VARCHAR(30) NOT NULL,
    Effect TINYINT(1) NOT NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (Role_Resource_Permission_ID),
    UNIQUE KEY UQ_Role_Resource_Permission
        (Role_ID, Resource_Type, Resource_Code, Permission_Code),
    KEY IX_Role_Resource_Permission_Lookup
        (Role_ID, Resource_Type, Resource_Code, Is_Active),
    CONSTRAINT FK_Role_Resource_Permissions_Roles
        FOREIGN KEY (Role_ID) REFERENCES roles (Role_ID)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS user_resource_permissions (
    User_Resource_Permission_ID BIGINT NOT NULL AUTO_INCREMENT,
    User_ID INT NOT NULL,
    Resource_Type VARCHAR(30) NOT NULL,
    Resource_Code VARCHAR(150) NOT NULL,
    Permission_Code VARCHAR(30) NOT NULL,
    Effect TINYINT(1) NOT NULL,
    Effective_To DATETIME(6) NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (User_Resource_Permission_ID),
    UNIQUE KEY UQ_User_Resource_Permission
        (User_ID, Resource_Type, Resource_Code, Permission_Code),
    KEY IX_User_Resource_Permission_Lookup
        (User_ID, Resource_Type, Resource_Code, Is_Active),
    CONSTRAINT FK_User_Resource_Permissions_Users
        FOREIGN KEY (User_ID) REFERENCES users (User_ID)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- كتالوج حقول سند القبض، دون ربط صلاحيات تلقائية.
INSERT INTO system_screen_fields
    (Screen_ID, Field_Code, Field_Name, Is_Sensitive, Default_Required, Is_Active, Sort_Order)
SELECT s.Screen_ID, v.Field_Code, v.Field_Name, v.Is_Sensitive, v.Default_Required, 1, v.Sort_Order
FROM system_screens s
JOIN (
    SELECT 'VoucherDate' AS Field_Code, 'تاريخ السند' AS Field_Name, 0 AS Is_Sensitive, 1 AS Default_Required, 10 AS Sort_Order
    UNION ALL SELECT 'CashAccount', 'الصندوق أو البنك', 1, 1, 20
    UNION ALL SELECT 'Currency', 'العملة', 0, 1, 30
    UNION ALL SELECT 'ExchangeRate', 'سعر الصرف', 1, 1, 40
    UNION ALL SELECT 'Amount', 'مبلغ السند', 1, 1, 50
    UNION ALL SELECT 'AgainstText', 'استلمنا من', 0, 1, 60
    UNION ALL SELECT 'DetailsGrid', 'تفاصيل التوزيع المحاسبي', 1, 1, 70
) v
WHERE s.Screen_Code = 'ReceiptVoucher'
  AND NOT EXISTS (
      SELECT 1
      FROM system_screen_fields f
      WHERE f.Screen_ID = s.Screen_ID AND f.Field_Code = v.Field_Code
  );

-- كتالوج أزرار وإجراءات سند القبض؛ السجلات لا تمنح صلاحية وحدها.
INSERT INTO system_screen_actions
    (Screen_ID, Action_Code, Action_Name, Is_Sensitive, Requires_Reason, Is_Active, Sort_Order)
SELECT s.Screen_ID, v.Action_Code, v.Action_Name, v.Is_Sensitive, v.Requires_Reason, 1, v.Sort_Order
FROM system_screens s
JOIN (
    SELECT 'NEW' AS Action_Code, 'جديد' AS Action_Name, 0 AS Is_Sensitive, 0 AS Requires_Reason, 10 AS Sort_Order
    UNION ALL SELECT 'SAVE', 'حفظ', 1, 0, 20
    UNION ALL SELECT 'EDIT', 'تعديل', 1, 0, 30
    UNION ALL SELECT 'DELETE', 'حذف', 1, 1, 40
    UNION ALL SELECT 'REVIEW', 'تمت المراجعة', 1, 0, 50
    UNION ALL SELECT 'RETURN_CORRECTION', 'إعادة للتصحيح', 1, 1, 55
    UNION ALL SELECT 'APPROVE', 'اعتماد', 1, 0, 60
    UNION ALL SELECT 'CANCEL_APPROVAL', 'إلغاء الاعتماد', 1, 1, 70
    UNION ALL SELECT 'POST', 'ترحيل', 1, 0, 80
    UNION ALL SELECT 'UNPOST', 'فك الترحيل', 1, 1, 90
    UNION ALL SELECT 'PRINT', 'طباعة', 0, 0, 100
    UNION ALL SELECT 'VIEW_JOURNAL', 'استعراض القيد', 0, 0, 110
) v
WHERE s.Screen_Code = 'ReceiptVoucher'
  AND NOT EXISTS (
      SELECT 1
      FROM system_screen_actions a
      WHERE a.Screen_ID = s.Screen_ID AND a.Action_Code = v.Action_Code
  );
