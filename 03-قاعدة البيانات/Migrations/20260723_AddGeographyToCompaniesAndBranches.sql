-- المرحلة الأولى / البيانات الجغرافية للشركات والفروع
-- Migration: 20260723_AddGeographyToCompaniesAndBranches.sql
-- MySQL 8.0+
-- نفّذ هذه الهجرة بعد 20260723_AddGeographyFoundation.sql.
-- الحقول تقبل NULL مؤقتاً حتى لا تتعطل بيانات الشركات والفروع القائمة.
-- بعد استكمال تنظيف البيانات، تُحوّل Country_ID إلى NOT NULL حسب سياسة الإطلاق.

START TRANSACTION;

ALTER TABLE companies
    ADD COLUMN Country_ID BIGINT UNSIGNED NULL AFTER Address,
    ADD COLUMN Governorate_ID BIGINT UNSIGNED NULL AFTER Country_ID,
    ADD COLUMN City_ID BIGINT UNSIGNED NULL AFTER Governorate_ID,
    ADD COLUMN Postal_Code VARCHAR(20) NULL AFTER City_ID,
    ADD INDEX IX_Companies_Country (Country_ID),
    ADD INDEX IX_Companies_Governorate (Governorate_ID),
    ADD INDEX IX_Companies_City (City_ID),
    ADD CONSTRAINT FK_Companies_Country
        FOREIGN KEY (Country_ID) REFERENCES countries(Country_ID) ON DELETE RESTRICT,
    ADD CONSTRAINT FK_Companies_Governorate
        FOREIGN KEY (Governorate_ID) REFERENCES governorates(Governorate_ID) ON DELETE RESTRICT,
    ADD CONSTRAINT FK_Companies_City
        FOREIGN KEY (City_ID) REFERENCES cities(City_ID) ON DELETE RESTRICT;

ALTER TABLE tenant_branches
    ADD COLUMN Country_ID BIGINT UNSIGNED NULL AFTER Address,
    ADD COLUMN Governorate_ID BIGINT UNSIGNED NULL AFTER Country_ID,
    ADD COLUMN City_ID BIGINT UNSIGNED NULL AFTER Governorate_ID,
    ADD COLUMN Postal_Code VARCHAR(20) NULL AFTER City_ID,
    ADD INDEX IX_TenantBranches_Country (Country_ID),
    ADD INDEX IX_TenantBranches_Governorate (Governorate_ID),
    ADD INDEX IX_TenantBranches_City (City_ID),
    ADD CONSTRAINT FK_TenantBranches_Country
        FOREIGN KEY (Country_ID) REFERENCES countries(Country_ID) ON DELETE RESTRICT,
    ADD CONSTRAINT FK_TenantBranches_Governorate
        FOREIGN KEY (Governorate_ID) REFERENCES governorates(Governorate_ID) ON DELETE RESTRICT,
    ADD CONSTRAINT FK_TenantBranches_City
        FOREIGN KEY (City_ID) REFERENCES cities(City_ID) ON DELETE RESTRICT;

COMMIT;

-- تحقق تشغيلي بعد الهجرة:
-- لا تسمح الخدمة بحفظ Governorate_ID أو City_ID لا يتبعان Country_ID المختارة.
-- لا تستخدم ON DELETE CASCADE على البيانات المرجعية الجغرافية.