-- تشخيص فقط: لا ينفذ INSERT أو UPDATE أو DELETE.
-- الغرض: تحديد سبب رسالة عدم وجود إعداد ترقيم نشط لطلبات الصرف.

SELECT
    Numbering_ID,
    Document_Type,
    Prefix,
    Digits_Count,
    Reset_Type,
    Last_Number,
    Use_Company,
    Use_Branch,
    Use_Year,
    Is_Active
FROM numbering_settings
WHERE UPPER(TRIM(Document_Type)) = 'PAYMENT_REQUEST';

-- نتيجة التحقق المتوقعة: صف واحد على الأقل بقيمة Is_Active = 1.
SELECT
    CASE
        WHEN COUNT(*) = 0 THEN 'MISSING'
        WHEN SUM(CASE WHEN Is_Active = 1 THEN 1 ELSE 0 END) = 0 THEN 'INACTIVE'
        WHEN SUM(CASE WHEN Is_Active = 1 THEN 1 ELSE 0 END) > 1 THEN 'DUPLICATE_ACTIVE'
        ELSE 'READY'
    END AS Numbering_Status,
    COUNT(*) AS Matching_Rows,
    SUM(CASE WHEN Is_Active = 1 THEN 1 ELSE 0 END) AS Active_Rows
FROM numbering_settings
WHERE UPPER(TRIM(Document_Type)) = 'PAYMENT_REQUEST';

-- فحص تقريبي للأسماء التي قد تكون محفوظة بصيغة غير صحيحة.
SELECT Numbering_ID, Document_Type, Is_Active
FROM numbering_settings
WHERE UPPER(Document_Type) LIKE '%PAYMENT%'
   OR UPPER(Document_Type) LIKE '%REQUEST%';
