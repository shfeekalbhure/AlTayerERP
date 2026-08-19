# اختبارات قبول الحزمة — الإعدادات والبيانات المرجعية والترقيم

## المتطلب المسبق

1. نفّذ ترقية قاعدة البيانات الآمنة: `Database/2026-07-24_secure_numbering_counter_scope.sql`.
2. شغّل API وسجّل الدخول بمدير نظام يملك صلاحيات `GeneralSettings` و`NumberingSettings`.
3. تأكد من وجود إعداد نشط لنوع `RECEIPT_VOUCHER`، مثلاً `Prefix = RCV` و`Digits_Count = 6` و`Reset_Type = BRANCHYEAR`.

## حالات إعدادات النظام

| الحالة | الإجراء | النتيجة المتوقعة |
|---|---|---|
| SET-01 | حفظ `AUTO_POST_VOUCHERS=false` على نطاق SYSTEM | ينشأ سجل واحد فقط ويظهر في سجل التدقيق. |
| SET-02 | حفظ القيمة نفسها على نطاق COMPANY ثم BRANCH ثم FISCAL_YEAR | `GET SystemSettings/Resolve/AUTO_POST_VOUCHERS` يعيد نطاق السنة ثم الفرع ثم الشركة ثم النظام حسب التوفر. |
| SET-03 | محاولة مستخدم بلا `GeneralSettings.Edit` حفظ إعداد | API يعيد 403 ولا يتغير الجدول. |
| SET-04 | حذف إعداد | يتحول إلى `Is_Active=false` ولا يحذف الصف. |

## حالات البيانات المرجعية

| الحالة | الإجراء | النتيجة المتوقعة |
|---|---|---|
| REF-01 | إضافة طريقة `CHEQUE` مع `Requires_Reference_Date=true` و`Requires_Reference=false` | API يعيد 400. |
| REF-02 | إضافة كود طريقة سداد مكرر | API يعيد 409. |
| REF-03 | محاولة تعطيل حالة `POSTED` أو تغيير كودها | API يعيد 400. |
| REF-04 | إنشاء/تعديل طريقة سداد أو نوع سند أو حالة | يسجل `CREATE` أو `UPDATE` في `audit_logs`. |
| REF-05 | بدء API على قاعدة فيها بيانات | يضاف فقط المفقود من أنواع السندات والحالات ولا يعدل القيم الموجودة. |

## اختبار الترقيم المتزامن

> هذا الاختبار يحجز أرقاماً فعلية ولا يعيدها. نفذه على بيانات اختبار أو إعداد نوع مستند تجريبي.

1. أنشئ إعداداً فعالاً لنوع `TEST_CONCURRENT` مع `Prefix=TEST` و`Digits_Count=6` و`Reset_Type=BRANCHYEAR`.
2. أرسل 100 طلب متزامن من جلسة واحدة أو جلسات صحيحة إلى `POST /api/NumberingSettings/Reserve`:

```json
{ "document_Type": "TEST_CONCURRENT" }
```

3. تحقق من الآتي:

| المعيار | النتيجة المقبولة |
|---|---|
| عدد الاستجابات الناجحة | 100 |
| عدد `Document_Number` المميزة | 100 |
| تسلسل `Serial_Number` | بلا تكرار وبفجوات صفرية ضمن الدفعة عند عدم وجود حجوزات أخرى |
| صف العداد | صف واحد فقط للنطاق في `numbering_counters` |
| سجل التدقيق | 100 سجل `NUMBER_RESERVED` |
| الإلغاء/حذف المسودة | لا ينقص `Last_Number` ولا يعاد الرقم المحجوز |

## الاختبار الأمني

- إرسال `companyId` أو `branchId` أو `year` في جسم حجز الرقم لا يغير نطاق الحجز؛ API يأخذ القيم من `ServerSession`.
- طلب `GET` لا يحجز رقماً؛ الحجز يتم فقط بعملية `POST Reserve`.
- مستخدم بلا صلاحية `NumberingSettings.Add` يستقبل 403.
