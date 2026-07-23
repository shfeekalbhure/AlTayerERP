-- ============================================================================
-- AlTayerERP | ترقية آمنة: حقول تدقيق وحالة الفروع Tenant_Branches
-- التاريخ: 2026-07-24
-- قاعدة البيانات المستهدفة: altayer_erp_db فقط
-- الضمان: لا تحذف هذه الترقية جدولاً أو سجلاً أو عموداً قائماً.
-- المتطلب: MySQL 8.0 (يدعم ADD COLUMN IF NOT EXISTS).
-- ============================================================================

USE altayer_erp_db;

-- حقول التدقيق التي يتطلبها كيان TenantBranch ومسارات الإدارة.
-- جميعها قابلة للتطبيق أكثر من مرة دون فقد بيانات.
ALTER TABLE tenant_branches
    ADD COLUMN IF NOT EXISTS Created_By INT NULL,
    ADD COLUMN IF NOT EXISTS Updated_By INT NULL,
    ADD COLUMN IF NOT EXISTS Edit_Count INT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Stopped_By INT NULL,
    ADD COLUMN IF NOT EXISTS Stopped_At DATETIME NULL,
    ADD COLUMN IF NOT EXISTS Stopped_Reason VARCHAR(500) NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_By INT NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_At DATETIME NULL,
    ADD COLUMN IF NOT EXISTS Reactivate_Reason VARCHAR(500) NULL;

-- تحقق بعد الترقية: يجب أن يعيد الحقول التسعة.
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'altayer_erp_db'
  AND TABLE_NAME = 'tenant_branches'
  AND COLUMN_NAME IN (
      'Created_By', 'Updated_By', 'Edit_Count',
      'Stopped_By', 'Stopped_At', 'Stopped_Reason',
      'Reactivated_By', 'Reactivated_At', 'Reactivate_Reason'
  )
ORDER BY COLUMN_NAME;

-- الفهارس تؤجل لسكربت منفصل بعد فحص تكرار Branch_Code؛ لا تنفذ ضمن إصلاح الدخول.
