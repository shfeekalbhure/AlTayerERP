-- إصلاح هيكل تاريخ قيم الإعدادات.
-- ينفذ مرة واحدة فقط على قاعدة تم إنشاؤها بالإصدار السابق من السكربت.
-- بعده يمكن حفظ أكثر من نسخة لنفس الإعداد والنطاق مع بقاء نسخة فعالة واحدة.

DROP INDEX UQ_Setting_Scope_Value ON setting_scope_values;

CREATE INDEX IX_Setting_Scope_Value_Resolution
    ON setting_scope_values (Setting_ID, Scope_Type, Scope_ID, Is_Active);