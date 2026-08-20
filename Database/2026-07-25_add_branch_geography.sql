USE altayer_erp_db;

-- ترقية آمنة لإضافة المراجع الجغرافية للفروع دون حذف أو إعادة إنشاء الجدول.
SET @schema_name = DATABASE();

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND column_name='Country_ID'),
    'SELECT 1',
    'ALTER TABLE Tenant_Branches ADD COLUMN Country_ID INT NULL AFTER Parent_Branch_ID'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND column_name='Governorate_ID'),
    'SELECT 1',
    'ALTER TABLE Tenant_Branches ADD COLUMN Governorate_ID INT NULL AFTER Country_ID'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND column_name='City_ID'),
    'SELECT 1',
    'ALTER TABLE Tenant_Branches ADD COLUMN City_ID INT NULL AFTER Governorate_ID'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.statistics WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND index_name='IX_Tenant_Branches_Country'),
    'SELECT 1',
    'CREATE INDEX IX_Tenant_Branches_Country ON Tenant_Branches(Country_ID)'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.statistics WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND index_name='IX_Tenant_Branches_Governorate'),
    'SELECT 1',
    'CREATE INDEX IX_Tenant_Branches_Governorate ON Tenant_Branches(Governorate_ID)'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.statistics WHERE table_schema=@schema_name AND table_name='Tenant_Branches' AND index_name='IX_Tenant_Branches_City'),
    'SELECT 1',
    'CREATE INDEX IX_Tenant_Branches_City ON Tenant_Branches(City_ID)'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
