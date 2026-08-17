-- المرحلة الأولى | بيانات الإيقاف وإعادة التفعيل للسجلات المرجعية.
-- السبب إلزامي في API؛ لا تنفذ هذه التعديلات من الواجهة.
ALTER TABLE tenant_groups
    ADD COLUMN IF NOT EXISTS Stopped_By INT NULL,
    ADD COLUMN IF NOT EXISTS Stopped_At DATETIME(6) NULL,
    ADD COLUMN IF NOT EXISTS Stopped_Reason VARCHAR(500) NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_By INT NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_At DATETIME(6) NULL,
    ADD COLUMN IF NOT EXISTS Reactivate_Reason VARCHAR(500) NULL;

ALTER TABLE companies
    ADD COLUMN IF NOT EXISTS Stopped_By INT NULL,
    ADD COLUMN IF NOT EXISTS Stopped_At DATETIME(6) NULL,
    ADD COLUMN IF NOT EXISTS Stopped_Reason VARCHAR(500) NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_By INT NULL,
    ADD COLUMN IF NOT EXISTS Reactivated_At DATETIME(6) NULL,
    ADD COLUMN IF NOT EXISTS Reactivate_Reason VARCHAR(500) NULL;
