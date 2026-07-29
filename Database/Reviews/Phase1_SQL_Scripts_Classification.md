# تصنيف سكربتات SQL — المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** تدقيق فقط؛ لم يُنفذ أو يُنقل أو يُحذف أي ملف.

## 1. العدد النهائي وطريقة الإثبات

العدد النهائي لجميع ملفات `.sql` داخل `Database` وما تحته هو **28 ملفًا**.

تم تثبيت العدد من:

1. ملفات SQL الموجودة في مرجع `master` قبل PR #11.
2. القائمة الكاملة لجميع الملفات المتغيرة في PR #11، وليس نتائج البحث الجزئية.
3. إزالة التكرار بالمسار الكامل.
4. فتح الملفات المؤثرة وقراءة محتواها، خصوصًا الجغرافيا، توسعة الفروع، القيود، طلبات الصرف، الأمن، المحاذاة ودليل الحسابات.

## 2. التصنيف النهائي

| # | المسار الكامل | الغرض والجداول/الأعمدة | USE القديمة | CREATE TABLE | ALTER TABLE | Seed/DML | قابل للإعادة | التصنيف النهائي |
|---:|---|---|:---:|:---:|:---:|:---:|:---:|---|
| 1 | `Database/2026-07-21_phase1_bank_accounts.sql` | إنشاء `bank_accounts` ومفاتيحه وفهارسه | لا | نعم | لا | لا | نعم | مخطط أساسي جزئي |
| 2 | `Database/2026-07-21_phase1_exchange_rates.sql` | إنشاء `exchange_rates` | لا | نعم | لا | لا | نعم | مخطط أساسي جزئي |
| 3 | `Database/2026-07-21_phase1_fiscal_periods.sql` | إنشاء `fiscal_periods` | لا | نعم | لا | لا | نعم | مخطط أساسي جزئي |
| 4 | `Database/2026-07-21_phase1_system_settings.sql` | إنشاء `system_settings` | لا | نعم | لا | محتمل/مرجعي | نعم | مخطط أساسي جزئي + Seed منفصل مطلوب |
| 5 | `Database/2026-07-21_phase1_setup_compatibility.sql` | إنشاء الجداول الأربعة السابقة عند غيابها وإضافة `Sort_Order` إلى مراجع السندات | لا | نعم | نعم | لا | جزئيًا | توافق مؤقت، مكرر |
| 6 | `Database/2026-07-23_expand_tenant_groups.sql` | توسعة `tenant_groups`: الكود، الاسم المختصر، الأب، الشركة الرئيسية، الموقع، الإدارة، الظهور، الترتيب والتدقيق؛ تعبئة بيانات قديمة | لا | لا | نعم | نعم | جزئيًا | ترقية قديمة + إصلاح بيانات |
| 7 | `Database/2026-07-23_geographic_references.sql` | إنشاء `countries`, `governorates`, `cities` والعلاقات؛ Seed اليمن | لا | نعم | لا | نعم | نعم | مخطط SQL-only + Seed |
| 8 | `Database/2026-07-24_add_branch_audit.sql` | إضافة حقول الإنشاء والتعديل والإيقاف وإعادة التفعيل إلى `tenant_branches` | نعم | لا | نعم | لا | نعم | توافق مؤقت/ترقية قديمة |
| 9 | `Database/2026-07-24_add_company_audit.sql` | إضافة `Created_By`, `Updated_By`, `Edit_Count` وفهارس إلى `companies` | لا | لا | نعم | لا | نعم | ترقية قديمة |
| 10 | `Database/2026-07-24_add_login_security.sql` | إضافة حقول قفل الدخول إلى `users` وإنشاء `login_attempts`, `refresh_tokens` بإجراء مؤقت | نعم | نعم | نعم | لا | نعم | مخطط جزئي + توافق مؤقت |
| 11 | `Database/2026-07-24_add_record_status_audit.sql` | إضافة حقول الإيقاف وإعادة التفعيل إلى `tenant_groups`, `companies` | لا | لا | نعم | لا | نعم | ترقية قديمة |
| 12 | `Database/2026-07-24_add_tenant_group_audit.sql` | إضافة `Created_By`, `Updated_By`, `Edit_Count` وفهارس إلى `tenant_groups` | لا | لا | نعم | لا | نعم | ترقية قديمة |
| 13 | `Database/2026-07-24_secure_numbering_counter_scope.sql` | تطبيع نطاق `numbering_counters` وإنشاء Unique مركب بعد فحص التكرار | نعم | لا | لا مباشر؛ ينشئ فهرسًا | نعم | مشروط | إصلاح بيانات/قيد تاريخي |
| 14 | `Database/2026-07-25_add_branch_geography.sql` | إضافة `Country_ID`, `Governorate_ID`, `City_ID` وفهارس إلى الفروع | نعم | لا | نعم | لا | نعم | توافق مؤقت/ترقية قديمة |
| 15 | `Database/2026-07-26_branch_types_compatibility.sql` | إنشاء `branch_types` وSeed الأنواع | نعم | نعم | لا | نعم | نعم | مخطط SQL-only + Seed + توافق |
| 16 | `Database/2026-07-26_expand_audit_log_actions.sql` | استبدال Check الخاص بـ`audit_logs.Action_Type` | لا | لا | نعم | لا | نعم | إصلاح قيد تاريخي |
| 17 | `Database/2026-07-26_tenant_group_display_settings.sql` | إضافة `Is_Default`, `Show_In_Tree` وتنظيف الافتراضي والظهور | نعم | لا | نعم عبر SQL ديناميكي | نعم | نعم | ترقية قديمة + إصلاح بيانات |
| 18 | `Database/2026-07-27_chart_of_accounts_global_controls.sql` | تنظيف حسابات قديمة وإنشاء Triggerين لحماية تفاصيل السند | نعم | لا | لا | نعم | نعم | منطق قاعدة/Triggers؛ يحتاج قرار معماري |
| 19 | `Database/2026-07-28_account_categories.sql` | إنشاء `account_categories` وSeed تصنيفات لكل شركة | نعم | نعم | لا | نعم | نعم | مخطط SQL-only + Seed |
| 20 | `Database/2026-07-28_chart_of_accounts_control_accounts.sql` | إضافة حقول الحساب الرقابي وفهرس وتنظيف البيانات | نعم | لا | نعم ديناميكي | نعم | نعم | ترقية قديمة + إصلاح بيانات |
| 21 | `Database/2026-07-28_chart_of_accounts_final_verification.sql` | فحوص قراءة فقط لدليل الحسابات والتصنيفات | نعم | لا | لا | لا | نعم | تحقق/تشخيص، ليس DDL |
| 22 | `Database/2026-07-28_database_alignment_phase_a.sql` | مواءمة `bank_accounts` وفهرس رقم السند؛ قد يعيد تسمية جدول ويحذف فهرسًا | نعم | نعم | نعم | محتمل | مشروط ويتوقف عند الخطر | إصلاح/ترحيل تاريخي عالي المخاطر |
| 23 | `Database/2026-07-28_database_alignment_preflight.sql` | إنشاء `database_alignment_findings` ثم `TRUNCATE` وتعبئة نتائج Drift | نعم | نعم | لا | نعم | نعم لكن كتابي | تشخيص كتابي؛ لا يدخل Baseline |
| 24 | `Database/Scripts/20260719_Add_Amount_To_Financial_Voucher_Headers.sql` | إضافة `Amount` إلى رأس السند وتعبئة السجلات القديمة | لا | لا | نعم | نعم | لا | ترقية قديمة غير idempotent |
| 25 | `Database/Scripts/20260719_Complete_Receipt_Voucher_Workflow.sql` | إضافة حقول الاستلام والمراجعة إلى رأس السند وتعبئة الأسماء | لا | لا | نعم ديناميكي | نعم | نعم | ترقية قديمة + إصلاح بيانات |
| 26 | `Database/Scripts/20260720_Fix_Voucher_Review_Action_Constraint.sql` | إعادة إنشاء Check لحركات السند | لا | لا | نعم | لا | نعم | إصلاح قيد تاريخي |
| 27 | `Database/Scripts/20260720_Restore_Journal_Entry_Tables.sql` | إنشاء/استعادة جدولي القيود وتعديل السندات ذات القيد المفقود | لا | نعم | لا | نعم | جزئيًا | إصلاح/استعادة؛ مكرر مع Migration |
| 28 | `Database/Scripts/20260724_add_payment_requests.sql` | إنشاء `payment_requests`, `payment_request_lines`, `payment_request_attachments` وSeed ترقيم `PAYMENT_REQUEST` | لا | نعم | لا | نعم | نعم | مخطط SQL-only + Seed؛ مكرر مع SchemaInitializer |

## 3. الملاحظات الحاكمة

- **14 ملفًا** يحتوي `USE altayer_erp_db` أو يستهدف القاعدة القديمة صراحةً/ضمنيًا؛ لا يجوز تشغيلها على القاعدة النظيفة.
- توجد تعريفات مكررة مؤكدة:
  - `journal_entry_headers/details`: Migration + Restore SQL.
  - `payment_requests/*`: SQL + `PaymentRequestSchemaInitializer`.
  - جداول الإعدادات الأربعة: ملفات منفصلة + `phase1_setup_compatibility.sql`.
  - `bank_accounts`: ملف مستقل + ملف التوافق + Alignment Phase A.
- توجد ملفات كتابية تحمل اسم فحص، مثل `database_alignment_preflight.sql`؛ فهي تنشئ جدولًا وتنفذ `TRUNCATE/INSERT` وليست قراءة فقط.
- توجد Triggers فعلية في سكربت دليل الحسابات، ولا توجد Views أو Functions دائمة مثبتة. توجد Procedures مؤقتة تُنشأ ثم تُحذف داخل عدة سكربتات.

## 4. قرار Baseline المقترح

1. لا يدخل أي سكربت تاريخي مباشرة في Baseline.
2. تُستخرج النتيجة النهائية المطلوبة من كل ملف إلى EF/Fluent API.
3. تفصل Seed Data عن DDL.
4. تنقل الملفات لاحقًا إلى `Database/Legacy` فقط بعد اعتماد المالك، دون حذفها.
5. تصبح Migration المعتمدة المصدر الوحيد للمخطط بعد المرحلة الثانية.

## 5. التأكيد

لم يُنفذ أي SQL، ولم يُعدل أو يُحذف أو يُنقل أي سكربت، ولم تُمس القاعدة القديمة.