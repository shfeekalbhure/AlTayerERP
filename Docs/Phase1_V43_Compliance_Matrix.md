# مصفوفة مطابقة المرحلة الأولى — الكراسة الموحدة v4.3

هذه المصفوفة مرجع التنفيذ، لا تعتبر الشاشة مكتملة لمجرد ظهورها في القائمة.

| المتطلب | الحالة | الدليل/المسار | ملاحظة قبول |
|---|---|---|---|
| سياق الشركة والفرع والسنة | منفذ | `FinancialVoucherController` و`VoucherValidationService` | السياق يفرض من جلسة الخادم؛ وتتحقق الفترة المفتوحة. |
| Default Deny للمراجع الجغرافية | منفذ | `GeographicReferencesController` | View/Add/Edit/Delete وإعادة التفعيل محمية بتفويض الشاشة؛ التدقيق لكل دورة الحياة. |
| الدول/المحافظات/المدن RTL + BaseForm + شجرة | منفذ | `GeographicReferenceForms.cs`, `FrmMain.cs`, `SystemScreenCatalogSeeder.cs` | API فقط ولا حذف مادي. |
| سند القبض | جزئي | `FrmReceiptVoucher`، `FinancialVoucherController` | موحد على BaseForm ومحرك Backend؛ يحتاج اختبار قبول تشغيلي كامل. |
| سند الصرف | جزئي | `FrmPaymentVoucher`، `FrmReceiptVoucher.Save.cs` | نوع PAYMENT مستقل وصرف يعكس طرفي المدين/الدائن؛ يحتاج فصل عرضي إضافي واختبار قبول. |
| القيد اليومي | جزئي | `FrmJournalVoucher.cs`، `VoucherValidationService.cs` | شاشة BaseForm مستقلة وسطور مدين/دائن ودورة API؛ يلزم اختبار قبول فعلي وتوسعة عرض القيد. |
| مراجعة/اعتماد/ترحيل/فك ترحيل | منفذ Backend / جزئي Desktop | `FinancialVoucherController`, `VoucherPostingService` | السبب إلزامي لفك الترحيل؛ الواجهة موفرة للقيد اليومي، واختبارها مطلوب. |
| عرض القيد والبحث بالرقم الكامل/الجزئي | جزئي | `FinancialVoucherController`, `FrmJournalVoucher.cs` | البحث محصور بسياق الجلسة؛ يلزم شاشة استعلام موحدة مكتملة. |
| المرفقات/طلبات الاعتماد/السقوف | غير مكتمل | — | الحزمة التالية. |
| ميزان المراجعة/الأستاذ العام | غير مكتمل | — | الحزمة التالية. |
| اختبارات القبول وقاعدة altayer_erp_db | قيد التحقق | GitHub Actions | لا توجد ترقية قاعدة مطلوبة في هذه الحزمة. |
