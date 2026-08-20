-- جزء 32: رمز التزامن التفاؤلي لطلبات الصرف.
-- آمن لإعادة التشغيل ولا يحذف أية بيانات. نفّذه على نسخة اختبار أولاً ثم في نافذة صيانة الإنتاج.

ALTER TABLE payment_requests
    ADD COLUMN IF NOT EXISTS RowVersion CHAR(36) NOT NULL
    DEFAULT '00000000-0000-0000-0000-000000000000'
    COMMENT 'Optimistic concurrency token for payment request updates';

-- لا تبقَ أي طلبات قائمة بالرمز الافتراضي المشترك.
UPDATE payment_requests
SET RowVersion = UUID()
WHERE RowVersion IS NULL
   OR RowVersion = '00000000-0000-0000-0000-000000000000';

-- فحص قبول ميداني: يجب أن يكون عدد الرموز الافتراضية صفراً.
SELECT COUNT(*) AS Default_RowVersion_Count
FROM payment_requests
WHERE RowVersion = '00000000-0000-0000-0000-000000000000';

SHOW COLUMNS FROM payment_requests LIKE 'RowVersion';
