-- AlTayerERP
-- إضافة إعدادات المجموعة الافتراضية والظهور في شجرة النظام.
-- قاعدة البيانات المعتمدة: altayer_erp_db فقط.
-- ترقية آمنة بلا حذف بيانات.

USE altayer_erp_db;

SET @schema_name = DATABASE();

SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'tenant_groups'
          AND column_name = 'Is_Default'
    ),
    'SELECT 1',
    'ALTER TABLE tenant_groups ADD COLUMN Is_Default TINYINT(1) NOT NULL DEFAULT 0 AFTER Group_Name_EN'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'tenant_groups'
          AND column_name = 'Show_In_Tree'
    ),
    'SELECT 1',
    'ALTER TABLE tenant_groups ADD COLUMN Show_In_Tree TINYINT(1) NOT NULL DEFAULT 1 AFTER Show_In_Login'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- يمنع بقاء أكثر من مجموعة افتراضية بعد الترقية.
SET @default_group_id = (
    SELECT Group_ID
    FROM tenant_groups
    WHERE Is_Default = 1
    ORDER BY Created_At, Group_ID
    LIMIT 1
);

UPDATE tenant_groups
SET Is_Default = 0
WHERE Is_Default = 1
  AND (@default_group_id IS NULL OR Group_ID <> @default_group_id);

-- إذا لم توجد مجموعة افتراضية، تعتمد أول مجموعة نشطة فقط.
SET @default_group_id = COALESCE(
    @default_group_id,
    (
        SELECT Group_ID
        FROM tenant_groups
        WHERE Is_Active = 1
        ORDER BY Created_At, Group_ID
        LIMIT 1
    )
);

UPDATE tenant_groups
SET Is_Default = CASE WHEN Group_ID = @default_group_id THEN 1 ELSE 0 END
WHERE @default_group_id IS NOT NULL;

-- المجموعات الموقوفة لا تظهر في شاشة اختيار الشركة أو شجرة النظام.
UPDATE tenant_groups
SET Show_In_Login = 0,
    Show_In_Tree = 0,
    Is_Default = 0
WHERE Is_Active = 0;
