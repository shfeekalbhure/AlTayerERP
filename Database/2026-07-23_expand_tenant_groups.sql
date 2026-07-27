-- توسعة جدول المجموعات التجارية وفق قرار التصميم ADR-UI-P1-001
-- خذ نسخة احتياطية قبل التنفيذ. الملف آمن لإعادة التشغيل في MySQL 8.

ALTER TABLE tenant_groups
    ADD COLUMN IF NOT EXISTS Group_Code VARCHAR(30) NULL AFTER Group_ID,
    ADD COLUMN IF NOT EXISTS Short_Name VARCHAR(100) NULL AFTER Group_Name_EN,
    ADD COLUMN IF NOT EXISTS Parent_Group_ID VARCHAR(36) NULL AFTER Short_Name,
    ADD COLUMN IF NOT EXISTS Main_Company_ID VARCHAR(50) NULL AFTER Parent_Group_ID,
    ADD COLUMN IF NOT EXISTS Default_Currency_Code VARCHAR(10) NULL AFTER Main_Company_ID,
    ADD COLUMN IF NOT EXISTS Country_Name VARCHAR(100) NULL AFTER Default_Currency_Code,
    ADD COLUMN IF NOT EXISTS City_Name VARCHAR(100) NULL AFTER Country_Name,
    ADD COLUMN IF NOT EXISTS Short_Address VARCHAR(300) NULL AFTER City_Name,
    ADD COLUMN IF NOT EXISTS Phone VARCHAR(50) NULL AFTER Short_Address,
    ADD COLUMN IF NOT EXISTS Email VARCHAR(150) NULL AFTER Phone,
    ADD COLUMN IF NOT EXISTS Manager_Name VARCHAR(150) NULL AFTER Email,
    ADD COLUMN IF NOT EXISTS Show_In_Login TINYINT(1) NOT NULL DEFAULT 1 AFTER Manager_Name,
    ADD COLUMN IF NOT EXISTS Sort_Order INT NOT NULL DEFAULT 0 AFTER Show_In_Login,
    ADD COLUMN IF NOT EXISTS Notes VARCHAR(500) NULL AFTER Sort_Order,
    ADD COLUMN IF NOT EXISTS Updated_At DATETIME NULL AFTER Created_At;

UPDATE tenant_groups tg
JOIN (
    SELECT Group_ID,
           CONCAT('BG', LPAD(ROW_NUMBER() OVER (ORDER BY Created_At, Group_ID), 3, '0')) AS Generated_Code
    FROM tenant_groups
) numbered ON numbered.Group_ID = tg.Group_ID
SET tg.Group_Code = numbered.Generated_Code
WHERE tg.Group_Code IS NULL OR TRIM(tg.Group_Code) = '';

UPDATE tenant_groups
SET Short_Name = Group_Name_AR
WHERE Short_Name IS NULL OR TRIM(Short_Name) = '';

ALTER TABLE tenant_groups
    MODIFY COLUMN Group_Code VARCHAR(30) NOT NULL,
    MODIFY COLUMN Short_Name VARCHAR(100) NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS UX_Tenant_Groups_Group_Code ON tenant_groups (Group_Code);
CREATE INDEX IF NOT EXISTS IX_Tenant_Groups_Parent ON tenant_groups (Parent_Group_ID);
CREATE INDEX IF NOT EXISTS IX_Tenant_Groups_Login_Order ON tenant_groups (Show_In_Login, Is_Active, Sort_Order);
