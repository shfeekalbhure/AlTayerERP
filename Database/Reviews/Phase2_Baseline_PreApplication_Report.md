# تقرير Baseline قبل التطبيق — المرحلة الثانية

**الحالة:** مولدة ومفحوصة دون إنشاء قاعدة أو تنفيذ SQL على MySQL.  
**اسم Baseline:** `Baseline_Phase1`  
**Character Set:** `utf8mb4`  
**Collation:** `utf8mb4_unicode_ci`

## النتائج العددية

- عدد الجداول الوظيفية: **0**.
- عدد علاقات Foreign Key في السكربت: **81**.
- عدد أوامر إنشاء الفهارس: **115**.
- عدد ظهور القيود/الفهارس الفريدة: **43**.
- عدد Check Constraints: **17**.
- عدد ملفات Migration/Snapshot النشطة: **3**.

## الجداول التي تنشئها Baseline



## فحص الأمان

- لا يوجد `altayer_erp_db`.
- لا يوجد `USE` لقاعدة ثابتة.
- لا يوجد `DROP DATABASE`.
- لا يوجد `DROP TABLE` في سكربت الإنشاء من الصفر.
- لا يوجد `TRUNCATE TABLE`.
- لا توجد كلمات مرور أو Connection String.
- لا توجد بيانات شركة أو فرع أو مستخدم تجريبية.

## بيانات Seed المسموحة

- أنواع الفروع.
- أنواع السندات.
- حالات السندات.
- طرق السداد.
- حالات الاعتماد.
- أنواع المستندات والترقيم.
- الصلاحيات الأساسية.
- كتالوج الشاشات الأساسي.

## مصادر المخطط

- `AppDbContext` و`Phase1ModelConfiguration`.
- Migration `Baseline_Phase1` المولدة بواسطة `dotnet ef`.
- ملفات SQL التاريخية غير مستخدمة في إنشاء Baseline.
- `PaymentRequestSchemaInitializer` لا ينفذ DDL.

## القيود

لم يتم تشغيل `Update-Database`، ولم تُنشأ `altayer_erp_db_clean`، ولم يتغير أي Connection String أو قاعدة بيانات.
