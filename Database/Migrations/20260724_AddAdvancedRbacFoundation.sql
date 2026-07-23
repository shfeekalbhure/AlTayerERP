-- =============================================================
-- المرحلة الأولى | نموذج RBAC المتقدم ونطاق الوصول المالي
-- الملف: 20260724_AddAdvancedRbacFoundation.sql
-- ملاحظة: ينفذ مرة واحدة بعد الجداول الأساسية users / roles / companies /
-- tenant_branches / system_permissions. لا ينشئ قيوداً محاسبية ولا Triggers.
-- =============================================================

CREATE TABLE IF NOT EXISTS user_roles (
    User_Role_ID BIGINT NOT NULL AUTO_INCREMENT COMMENT 'المعرف الداخلي للربط',
    User_ID INT NOT NULL COMMENT 'المستخدم',
    Role_ID INT NOT NULL COMMENT 'الدور',
    Effective_From DATETIME(6) NOT NULL COMMENT 'بداية السريان',
    Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة الربط',
    Granted_By INT NULL COMMENT 'من منح الدور',
    Reason VARCHAR(500) NULL COMMENT 'سبب المنح أو الإيقاف',
    PRIMARY KEY (User_Role_ID),
    CONSTRAINT UQ_User_Roles_User_Role_Start UNIQUE (User_ID, Role_ID, Effective_From),
    INDEX IX_User_Roles_Active (User_ID, Is_Active, Effective_From, Effective_To),
    CONSTRAINT FK_User_Roles_User FOREIGN KEY (User_ID) REFERENCES users(User_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_User_Roles_Role FOREIGN KEY (Role_ID) REFERENCES roles(Role_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='ربط المستخدمين بالأدوار متعددة السريان';

-- ترحيل الدور القديم للمستخدم إلى النموذج الجديد دون تكرار.
INSERT IGNORE INTO user_roles
    (User_ID, Role_ID, Effective_From, Is_Active, Reason)
SELECT User_ID, Role_ID, UTC_TIMESTAMP(6), Is_Active, 'Legacy role migration'
FROM users
WHERE Role_ID IS NOT NULL AND Role_ID > 0;

CREATE TABLE IF NOT EXISTS user_permission_overrides (
    User_Permission_Override_ID BIGINT NOT NULL AUTO_INCREMENT COMMENT 'معرف الاستثناء',
    User_ID INT NOT NULL COMMENT 'المستخدم',
    System_Permission_ID INT NOT NULL COMMENT 'الصلاحية القياسية',
    Effect VARCHAR(10) NOT NULL DEFAULT 'Deny' COMMENT 'Allow أو Deny',
    Effective_From DATETIME(6) NOT NULL COMMENT 'بداية السريان',
    Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة الاستثناء',
    Reason VARCHAR(500) NOT NULL COMMENT 'سبب الاستثناء',
    Created_By INT NULL COMMENT 'من أنشأ السجل',
    Created_At DATETIME(6) NOT NULL COMMENT 'وقت الإنشاء',
    PRIMARY KEY (User_Permission_Override_ID),
    CONSTRAINT UQ_User_Permission_Override UNIQUE (User_ID, System_Permission_ID, Effective_From),
    INDEX IX_User_Permission_Override_Active (User_ID, Is_Active, Effective_From, Effective_To),
    CONSTRAINT CK_User_Permission_Override_Effect CHECK (Effect IN ('Allow','Deny')),
    CONSTRAINT FK_User_Permission_Override_User FOREIGN KEY (User_ID) REFERENCES users(User_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_User_Permission_Override_Permission FOREIGN KEY (System_Permission_ID) REFERENCES system_permissions(Permission_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='استثناءات صلاحيات المستخدم';

CREATE TABLE IF NOT EXISTS company_access (
    Company_Access_ID BIGINT NOT NULL AUTO_INCREMENT COMMENT 'معرف نطاق الشركة',
    User_ID INT NOT NULL COMMENT 'المستخدم',
    Company_ID VARCHAR(50) NOT NULL COMMENT 'رمز الشركة',
    Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة النطاق',
    Effective_From DATETIME(6) NOT NULL COMMENT 'بداية السريان',
    Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    PRIMARY KEY (Company_Access_ID),
    CONSTRAINT UQ_Company_Access UNIQUE (User_ID, Company_ID),
    INDEX IX_Company_Access_Active (User_ID, Is_Active, Effective_From, Effective_To),
    CONSTRAINT FK_Company_Access_User FOREIGN KEY (User_ID) REFERENCES users(User_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_Company_Access_Company FOREIGN KEY (Company_ID) REFERENCES companies(Company_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='نطاق الشركات المسموح للمستخدم';

CREATE TABLE IF NOT EXISTS branch_access (
    Branch_Access_ID BIGINT NOT NULL AUTO_INCREMENT COMMENT 'معرف نطاق الفرع',
    User_ID INT NOT NULL COMMENT 'المستخدم',
    Company_ID VARCHAR(50) NOT NULL COMMENT 'رمز الشركة',
    Branch_ID INT NOT NULL COMMENT 'الفرع',
    Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة النطاق',
    Effective_From DATETIME(6) NOT NULL COMMENT 'بداية السريان',
    Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    PRIMARY KEY (Branch_Access_ID),
    CONSTRAINT UQ_Branch_Access UNIQUE (User_ID, Company_ID, Branch_ID),
    INDEX IX_Branch_Access_Active (User_ID, Is_Active, Effective_From, Effective_To),
    CONSTRAINT FK_Branch_Access_User FOREIGN KEY (User_ID) REFERENCES users(User_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_Branch_Access_Company FOREIGN KEY (Company_ID) REFERENCES companies(Company_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_Branch_Access_Branch FOREIGN KEY (Branch_ID) REFERENCES tenant_branches(Branch_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='نطاق الفروع المسموح للمستخدم';

CREATE TABLE IF NOT EXISTS approval_limits (
    Approval_Limit_ID BIGINT NOT NULL AUTO_INCREMENT COMMENT 'معرف الحد',
    User_ID INT NULL COMMENT 'المستخدم، أو NULL عند تخصيصه للدور',
    Role_ID INT NULL COMMENT 'الدور، أو NULL عند تخصيصه للمستخدم',
    Company_ID VARCHAR(50) NULL COMMENT 'الشركة، أو NULL لكل الشركات المسموح بها',
    Branch_ID INT NULL COMMENT 'الفرع، أو NULL لكل الفروع المسموح بها',
    Voucher_Type_Code VARCHAR(50) NULL COMMENT 'نوع المستند',
    Currency_ID INT NULL COMMENT 'عملة الحد',
    Max_Amount_Local DECIMAL(18,6) NOT NULL COMMENT 'أقصى مبلغ محلي',
    Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة الحد',
    Effective_From DATETIME(6) NOT NULL COMMENT 'بداية السريان',
    Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    PRIMARY KEY (Approval_Limit_ID),
    INDEX IX_Approval_Limits_Lookup (User_ID, Role_ID, Company_ID, Branch_ID, Voucher_Type_Code),
    CONSTRAINT CK_Approval_Limits_Owner CHECK ((User_ID IS NOT NULL) OR (Role_ID IS NOT NULL)),
    CONSTRAINT CK_Approval_Limits_Amount CHECK (Max_Amount_Local >= 0),
    CONSTRAINT FK_Approval_Limits_User FOREIGN KEY (User_ID) REFERENCES users(User_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_Approval_Limits_Role FOREIGN KEY (Role_ID) REFERENCES roles(Role_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='حدود الاعتماد المالية';


-- توسعة جدول role_permissions القائم لتطبيق كتالوج الصلاحيات الدقيق.
-- Screen_ID والحقول القديمة تبقى متوافقة إلى أن تنقل واجهة الإدارة بياناتها.
ALTER TABLE role_permissions
    ADD COLUMN IF NOT EXISTS System_Permission_ID INT NULL COMMENT 'الصلاحية القياسية',
    ADD COLUMN IF NOT EXISTS Effect VARCHAR(10) NOT NULL DEFAULT 'Allow' COMMENT 'Allow أو Deny',
    ADD COLUMN IF NOT EXISTS Grant_Descendants TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'توريث صريح',
    ADD COLUMN IF NOT EXISTS Effective_From DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'بداية السريان',
    ADD COLUMN IF NOT EXISTS Effective_To DATETIME(6) NULL COMMENT 'نهاية السريان',
    ADD COLUMN IF NOT EXISTS Is_Active TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'حالة السجل';

ALTER TABLE role_permissions
    ADD CONSTRAINT FK_Role_Permissions_System_Permission
    FOREIGN KEY (System_Permission_ID) REFERENCES system_permissions(Permission_ID)
    ON DELETE RESTRICT;

CREATE INDEX IX_Role_Permissions_Rbac_Active
    ON role_permissions (Role_ID, Is_Active, Effective_From, Effective_To);


-- تفرد كتالوج التراخيص والشاشات، ويجب معالجة أي تكرار تاريخي قبل التنفيذ.
CREATE UNIQUE INDEX UQ_System_Permissions_Code ON system_permissions (Permission_Code);
CREATE UNIQUE INDEX UQ_System_Screens_Code ON system_screens (Screen_Code);
