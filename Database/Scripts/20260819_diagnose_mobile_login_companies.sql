-- تشخيص ظهور الشركات في شاشة دخول Mobile.Office
-- هذا الملف للقراءة فقط ولا ينفذ أي تعديل تلقائي.

SELECT
    c.Company_ID,
    c.Company_Name_AR,
    COALESCE(c.Is_Active, 0) AS Company_Is_Active,
    c.Group_ID,
    g.Group_Code,
    g.Group_Name_AR,
    COALESCE(g.Is_Active, 0) AS Group_Is_Active,
    COALESCE(g.Show_In_Login, 0) AS Group_Show_In_Login,
    CASE
        WHEN c.Company_ID IS NULL THEN 'لا يوجد سجل شركة'
        WHEN COALESCE(c.Is_Active, 0) = 0 THEN 'الشركة موقوفة'
        WHEN g.Group_ID IS NULL THEN 'الشركة غير مربوطة بمجموعة تجارية'
        WHEN COALESCE(g.Is_Active, 0) = 0 THEN 'المجموعة التجارية موقوفة'
        WHEN COALESCE(g.Show_In_Login, 0) = 0 THEN 'المجموعة غير مفعلة للظهور عند الدخول'
        ELSE 'تظهر في api/Auth/LoginCompanies'
    END AS Login_Visibility_Status
FROM companies c
LEFT JOIN tenant_groups g ON g.Group_ID = c.Group_ID
ORDER BY
    CASE
        WHEN c.Company_ID IS NULL THEN 0
        WHEN COALESCE(c.Is_Active, 0) = 0 THEN 1
        WHEN g.Group_ID IS NULL THEN 2
        WHEN COALESCE(g.Is_Active, 0) = 0 THEN 3
        WHEN COALESCE(g.Show_In_Login, 0) = 0 THEN 4
        ELSE 5
    END,
    c.Company_Name_AR;

-- القاعدة الفعلية التي يجب أن يحققها السجل حتى يظهر:
-- companies.Is_Active = 1
-- tenant_groups.Is_Active = 1
-- tenant_groups.Show_In_Login = 1
-- companies.Group_ID = tenant_groups.Group_ID

-- لا تنفذ التعديل التالي إلا بعد اعتماد مسؤول النظام للشركة والمجموعة المقصودة:
-- UPDATE tenant_groups
-- SET Show_In_Login = 1,
--     Updated_At = UTC_TIMESTAMP()
-- WHERE Group_ID = '<GROUP_ID>';

-- بعد أي تعديل، أعد تشغيل الاستعلام أعلاه ثم أعد تحميل شاشة الجوال.
-- لا تغيّر ConnectionString أو بيانات الشركات من تطبيق العميل.
