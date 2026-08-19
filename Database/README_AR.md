# دليل قاعدة البيانات — AlTayerERP

## الحالة الحالية

يحتوي المستودع حاليًا على مسارين متكاملين لكن غير قابلين للاستبدال تلقائيًا:

| المسار | الاستخدام | ملاحظة الاعتماد |
|---|---|---|
| **EF Core Migrations** | تغييرات المخطط التي أُنشئت من نموذج `AppDbContext` | توجد Migration حالية لجداول القيود المحاسبية فقط، ولا تمثل وحدها إنشاء قاعدة جديدة كاملة |
| **SQL اليدوي** | تأسيس أو معالجة جداول مرحلة أولى وإصلاحات توافقية | بعض الملفات قابلة لإعادة التشغيل، وبعضها تغييرات مرة واحدة ويحتاج فحصًا قبليًا |

لا ينفذ API الترحيلات تلقائيًا عند بدء التشغيل. هذا مقصود حاليًا لتجنب تعديل قاعدة الإنتاج دون موافقة صريحة، ولأن خط الأساس الحالي يجمع بين مخطط موجود وملفات SQL مرحلية وMigration جزئية.

## ترتيب التنفيذ المقترح لبيئة اختبار جديدة

قبل التنفيذ يجب إنشاء نسخة احتياطية وتحديد إصدار MySQL المتوافق مع المشروع. لا تستخدم بيانات اعتماد `root` للتطبيق، ولا تشغل الأوامر على الإنتاج قبل اختبارها على نسخة مطابقة.

يُتبع الترتيب الآتي:

1. تطبيق خط الأساس المؤسسي المعتمد في وثائق قاعدة البيانات الموجودة في المستودع.
2. تشغيل `Database/2026-07-21_phase1_setup_compatibility.sql` عند الحاجة فقط. هذا الملف ينشئ مجموعة من جداول التوافق، ومنها `system_settings` و`fiscal_periods` و`exchange_rates` و`bank_accounts`.
3. عدم تشغيل الملفات المنفصلة للجداول الأربعة السابقة بعد نجاح ملف التوافق، إلا بعد التحقق من عدم وجود الجداول أو أعمدة مطلوبة غير مطبقة. سبب ذلك أن تعريفاتها مكررة في المسارين.
4. تشغيل `Database/Scripts/20260719_Add_Amount_To_Financial_Voucher_Headers.sql` و`Database/Scripts/20260719_Complete_Receipt_Voucher_Workflow.sql` وفق سجل الإصدار، مع حفظ نتيجة كل فحص.
5. تشغيل `Database/Scripts/20260720_Fix_Voucher_Review_Action_Constraint.sql` لإصلاح قيد سجل إجراءات السند عند الحاجة.
6. تشغيل `Database/Scripts/20260720_Restore_Journal_Entry_Tables.sql` إذا كانت جداول القيود غير موجودة أو كانت قاعدة البيانات مستعادة من نسخة فقدت هذه الجداول. الملف يعيد أيضًا السندات التي تشير إلى قيود مفقودة إلى حالة غير مرحل؛ لذلك يجب مراجعة عدد السجلات الناتج قبل اعتماد العملية.
7. تطبيق EF Core Migrations المعتمدة بعد التأكد من أن جدول `__EFMigrationsHistory` يعكس حالة القاعدة. لا تُشغّل Migration `AddJournalEntryTables` إذا كانت الجداول موجودة مسبقًا خارج سجل EF إلا بعد مراجعة حالة المخطط.

## التحقق بعد الترحيل

ينبغي تسجيل نتائج الاستعلامات الآتية في محضر النشر:

```sql
SELECT TABLE_NAME
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN (
    'financial_voucher_headers',
    'financial_voucher_details',
    'journal_entry_headers',
    'journal_entry_details',
    'system_settings',
    'fiscal_periods',
    'exchange_rates',
    'bank_accounts'
  )
ORDER BY TABLE_NAME;

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;

SELECT COUNT(*) AS MissingJournalReferences
FROM financial_voucher_headers h
LEFT JOIN journal_entry_headers j
  ON j.Journal_Entry_ID = h.Journal_Entry_ID
WHERE h.Journal_Entry_ID IS NOT NULL
  AND j.Journal_Entry_ID IS NULL;
```

يجب أن تكون قيمة `MissingJournalReferences` مساوية للصفر بعد اكتمال الإصلاحات، ما لم يكن هناك محضر استثناء معتمد. كما يجب مقارنة تعريف الأعمدة والفهارس في قاعدة الاختبار مع `AppDbContextModelSnapshot.cs` قبل نشر أي تغيير محاسبي.

## سياسة الترحيلات وطلبات الدمج

لا يُدمج أي Pull Request يضيف SQL جديدًا أو يعدل `AppDbContext` دون توضيح: اسم الإصدار، ترتيب التنفيذ، قابلية إعادة التشغيل، خطة التراجع، ونتيجة الاختبار على قاعدة نظيفة وقاعدة تحتوي بيانات.

طلبات الدمج المرحلية القديمة مثل حزمة **Phase1 clean database** كبيرة ومتشعبة، وبعضها يعتمد على فروع أخرى. لذلك لا تُدمج تلقائيًا ضمن خط الأساس الأمني الحالي. يجب أولًا اختيار فرع مرجعي واحد، ثم إعادة بناء Migrations النهائية من ذلك الفرع، وإغلاق الطلبات المتداخلة أو تحويلها إلى طلبات صغيرة قابلة للمراجعة.

## قرار مؤقت آمن

حتى اكتمال توحيد المخطط، يبقى التطبيق على سياسة **الترحيل اليدوي الموثق مع مراجعة مسبقة**. يمنع ذلك التشغيل التلقائي غير المقصود، لكنه يتطلب من مسؤول الإصدار تنفيذ الخطوات وتسجيل نتائجها. بعد توحيد خط الأساس يمكن اعتماد مسار EF Core واحد وإضافة فحص CI يمنع أي Migration غير موثقة أو SQL مكرر جديدًا.
