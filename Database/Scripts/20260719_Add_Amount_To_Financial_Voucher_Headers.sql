-- إضافة المبلغ الأصلي المدخل إلى رأس السند المالي.
ALTER TABLE financial_voucher_headers
    ADD COLUMN Amount DECIMAL(18,2) NOT NULL DEFAULT 0.00
    AFTER Exchange_Rate;

-- تعبئة السندات القديمة بقيمتها الأصلية المتاحة.
UPDATE financial_voucher_headers
SET Amount = CASE
    WHEN Foreign_Total > 0 THEN Foreign_Total
    ELSE Local_Total
END
WHERE Amount = 0;
