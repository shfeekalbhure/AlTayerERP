-- المرحلة الأولى / الإدارة العامة
-- Migration: 20260723_AddBranchTypesAndWorkCenters.sql
-- MySQL 8.0+ - نفّذ بعد هجرات الهيكل المؤسسي والبيانات الجغرافية.

START TRANSACTION;

CREATE TABLE branch_types (
    Branch_Type_ID INT UNSIGNED NOT NULL AUTO_INCREMENT,
    Branch_Type_Code VARCHAR(30) NOT NULL,
    Branch_Type_Name_AR VARCHAR(100) NOT NULL,
    Branch_Type_Name_EN VARCHAR(100) NULL,
    Allows_Financial_Operations TINYINT(1) NOT NULL DEFAULT 1,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Sort_Order INT NOT NULL DEFAULT 0,
    Created_At DATETIME(6) NOT NULL,
    Updated_At DATETIME(6) NULL,
    PRIMARY KEY (Branch_Type_ID),
    CONSTRAINT UQ_BranchTypes_Code UNIQUE (Branch_Type_Code),
    CONSTRAINT UQ_BranchTypes_NameAR UNIQUE (Branch_Type_Name_AR)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE work_centers (
    Work_Center_ID BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    Company_ID VARCHAR(50) NOT NULL,
    Branch_ID INT NULL,
    Work_Center_Code VARCHAR(30) NOT NULL,
    Work_Center_Name_AR VARCHAR(150) NOT NULL,
    Work_Center_Name_EN VARCHAR(150) NULL,
    Work_Center_Type VARCHAR(30) NOT NULL,
    Parent_Work_Center_ID BIGINT UNSIGNED NULL,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Sort_Order INT NOT NULL DEFAULT 0,
    Created_At DATETIME(6) NOT NULL,
    Created_By INT NULL,
    Updated_At DATETIME(6) NULL,
    Updated_By INT NULL,
    PRIMARY KEY (Work_Center_ID),
    CONSTRAINT UQ_WorkCenters_CompanyCode UNIQUE (Company_ID, Work_Center_Code),
    INDEX IX_WorkCenters_CompanyBranch (Company_ID, Branch_ID),
    INDEX IX_WorkCenters_Parent (Parent_Work_Center_ID),
    CONSTRAINT FK_WorkCenters_Company FOREIGN KEY (Company_ID) REFERENCES companies(Company_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_WorkCenters_Branch FOREIGN KEY (Branch_ID) REFERENCES tenant_branches(Branch_ID) ON DELETE RESTRICT,
    CONSTRAINT FK_WorkCenters_Parent FOREIGN KEY (Parent_Work_Center_ID) REFERENCES work_centers(Work_Center_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

COMMIT;

-- ملاحظة انتقالية: لا يُستبدل عمود Tenant_Branches.Branch_Type النصي بهذه الهجرة.
-- ستُضاف Branch_Type_ID بخطة ترحيل منفصلة بعد تصنيف بيانات الفروع القائمة.