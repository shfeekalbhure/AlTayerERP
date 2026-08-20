-- المرحلة الأولى | تدقيق الشركات ومنع الاعتماد على قيم العميل.
ALTER TABLE companies
    ADD COLUMN IF NOT EXISTS Created_By INT NULL COMMENT 'منشئ السجل من جلسة الخادم',
    ADD COLUMN IF NOT EXISTS Updated_By INT NULL COMMENT 'آخر معدل من جلسة الخادم',
    ADD COLUMN IF NOT EXISTS Edit_Count INT NOT NULL DEFAULT 0 COMMENT 'عداد تعديلات الخادم';

CREATE INDEX IF NOT EXISTS IX_Companies_Group_Active ON companies (Group_ID, Is_Active);
CREATE INDEX IF NOT EXISTS IX_Companies_Created_By ON companies (Created_By);
