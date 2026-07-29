# مراجعة قاعدة البيانات وEF Core

## ما هو منفذ

PR #16 يتضمن Baseline_Phase1، Collation `utf8mb4_unicode_ci`، جداول جغرافية و`branch_types`، وقيودًا وفهارس متعددة. هذا دليل تصميم جيد، لكنه ليس دليلاً على أن PR #11 يعمل فوق القاعدة النظيفة.

## فجوة التكامل

رأس PR #11 يحوي Migration قديمة واحدة `20260716223222_AddJournalEntryTables`، بينما `GeographicReferencesController` و`BranchReferenceLookupsController` يستخدمان مباشرة جداول `countries` و`governorates` و`cities` و`branch_types`. هذه الجداول ليست DbSet/Entities في `AppDbContext` لهذا الرأس، وإنما نشأت تاريخيًا من SQL يدوي.

| الحالة | العنصر |
|---|---|
| موجود في Baseline فقط | `countries`, `governorates`, `cities`, `branch_types` |
| موجود في SQL تاريخي فقط على PR #11 | السكربتات الجغرافية و`branch_types_compatibility.sql` |
| غير قابل للتحقق | تطبيق Baseline فعليًا على `altayer_erp_db_clean`؛ لم ينفذ التدقيق أي Migration |

## ملاحظات

- `PaymentRequestSchemaInitializer` ينفذ DDL خامًا لكنه لا يظهر مسجلاً أو مستدعى في `Program.cs`. وجود مسار بديل للـDDL خطر حوكمة حتى لو كان غير مستخدم.
- توجد مفاتيح معرفات نصية ورقمية معًا، مثال `FinancialVoucherHeader.Branch_ID` نصي مقابل `TenantBranch.Branch_ID` صحيح. Baseline يحسن جزءًا من العلاقات، لكن يلزم اختبار تكامل فعلي بعد توحيد الفرع.

العيوب: AT-001، AT-002، AT-009.
