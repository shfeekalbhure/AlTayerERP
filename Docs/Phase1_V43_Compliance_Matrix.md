# مصفوفة مطابقة المرحلة الأولى — الكراسة الموحدة v4.3

هذه المصفوفة مرجع التنفيذ، لا تعتبر الشاشة مكتملة لمجرد ظهورها في القائمة.

| المتطلب | الحالة | الدليل/المسار | ملاحظة قبول |
|---|---|---|---|
| سياق الشركة والفرع والسنة | منفذ | `FinancialVoucherController` و`VoucherValidationService` | السياق يفرض من جلسة الخادم؛ وتتحقق الفترة المفتوحة. |
| Default Deny للمراجع الجغرافية | منفذ | `GeographicReferencesController` | View/Add/Edit/Delete وإعادة التفعيل محمية بتفويض الشاشة؛ التدقيق لكل دورة الحياة. |
| الدول/المحافظات/المدن RTL + BaseForm + شجرة | منفذ | `GeographicReferenceForms.cs`, `FrmMain.cs`, `SystemScreenCatalogSeeder.cs` | API فقط ولا حذف مادي. |
| سند القبض | جزئي | `FrmReceiptVoucher`، `FinancialVoucherController` | موحد على BaseForm ومحرك Backend؛ يحتاج اختبار قبول تشغيلي كامل. |
| سند الصرف | منفذ برمجياً / قيد قبول | `FrmPaymentVoucher.cs` | شاشة مستقلة BaseForm RTL، تفاصيل مدينة وصندوق/بنك دائن ودورة API ومرفقات. |
| القيد اليومي | جزئي | `FrmJournalVoucher.cs`، `VoucherValidationService.cs` | شاشة BaseForm مستقلة وسطور مدين/دائن ودورة API؛ يلزم اختبار قبول فعلي وتوسعة عرض القيد. |
| مراجعة/اعتماد/ترحيل/فك ترحيل | منفذ Backend / جزئي Desktop | `FinancialVoucherController`, `VoucherPostingService` | السبب إلزامي لفك الترحيل؛ الواجهة موفرة للقيد اليومي، واختبارها مطلوب. |
| عرض القيد والبحث بالرقم الكامل/الجزئي | جزئي | `FinancialVoucherController`, `FrmJournalVoucher.cs` | البحث محصور بسياق الجلسة؛ يلزم شاشة استعلام موحدة مكتملة. |
| مرفقات السندات | منفذ برمجياً / قيد قبول | `VoucherAttachmentsController.cs`, `FrmVoucherAttachments.cs` | تخزين ملفات خارج DB ضمن نطاق جلسة موثق وتدقيق وحذف منطقي. |
| طلبات الاعتماد والسقوف | جزئي | `FinancialGovernanceController.cs` | API صارم بلا تجاوز تلقائي لمدير النظام؛ شاشات الإدارة ما زالت مطلوبة. |
| ميزان المراجعة/الأستاذ العام | منفذ برمجياً / قيد قبول | `AccountingReportsController.cs`, `FrmFinancialReports.cs` | نطاق جلسة، حساب/تاريخ/عملة/مركز تكلفة، والأرصدة الافتتاحية والحركة والختامية. |
| Fail-Closed والتقارير | منفذ برمجياً / قيد قبول | `FrmMain.cs`, `Tests/FrmMain_Permission_FailClosed_And_Reports_Acceptance.md` | لا قائمة بديلة عند فشل API الصلاحيات، حتى لمدير النظام. |
| اختبارات القبول وقاعدة altayer_erp_db | قيد التحقق | GitHub Actions | لا توجد ترقية قاعدة مطلوبة في هذه الحزمة. |
