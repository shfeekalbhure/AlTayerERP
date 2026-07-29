# Desktop Screen Matrix — P0

| الشاشة/المسار | تحميل/حفظ/تعديل | نقر/بحث/إيقاف | طباعة/صلاحية | حالة الاختبار والعيوب |
|---|---|---|---|---|
| BranchForm.cs | موجود ثابتًا؛ يحتاج DB clean | selectors جغرافية على screens branch فقط | صلاحيات API | UAT مطلوب؛ AT-001 |
| FrmCountries/FrmGovernorates/FrmCities | API موجود | selection/filter موجود | print/status API | UAT مطلوب على Baseline |
| FrmPaymentRequest.cs | موجود | تفاصيل/دورة عبر API | Screen permission | يحتاج نقل completion ثم UAT AT-008 |
| FrmPaymentVoucher.cs → FrmReceiptVoucher | وراثة النوع PAYMENT | حسب شاشة السند | workflow | AT-011 مؤكد نص موروث؛ print UAT |
| FrmSessions/FrmAuditLogs | load موجود | refresh/paging جزئي | Authorize | يحتاج UAT AT-006 |

«موجود» لا يساوي مختبر؛ لم يشغّل التدقيق WinForms.
