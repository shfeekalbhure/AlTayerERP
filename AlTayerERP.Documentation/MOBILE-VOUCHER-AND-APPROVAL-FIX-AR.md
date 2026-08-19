# إصلاح مطابقة سند القبض وشاشة طلبات الاعتماد في الجوال

## نطاق التغيير

نُفذت هذه المرحلة على فرع `agent/unified-phase1-screens` في مشروع `AlTayerERP` فقط. لم تُجرَ أي تغييرات على قاعدة البيانات، ولم يُدمج أي Commit في `master`.

## مطابقة سند القبض

كان عقد الجوال يرسل جزءاً من عقد `CreateFinancialVoucherDto` الرسمي فقط. أضيفت الآن الحقول الرأسية الناقصة إلى `CreateMobileVoucherDto`، وأضيفت الحقول التفصيلية الناقصة إلى `CreateMobileVoucherLineDto`. كما تمت مطابقة عقد التحديث حتى لا تعود الفروقات عند تعديل السند.

| المجموعة | الحقول التي أصبحت مدعومة |
|---|---|
| رأس السند | `Reference_Date`, `Notes`, `Module_ID`, `Document_Type_ID`, `Document_ID`, `Source_Document_No`, `Allocations` |
| تفاصيل السند | `Project_ID`, `Reference_Type`, `Reference_No`, `Reference_Name`, `Reference_Date`, `Notes` |
| دورة الاعتماد | `Requires_Approval` لم يعد ثابتاً؛ يتحكم به مفتاح **يتطلب اعتماداً** في الشاشة، مع إبقاء القيمة الافتراضية مفعلة للحفاظ على السلوك السابق |

تم توسيع شاشة إنشاء السند لإدخال تاريخ المرجع وملاحظات الرأس، وحقول مرجع السطر واسم المرجع وتاريخ المرجع ومعرف المشروع وملاحظات السطر. عند عدم إدخال رقم المرجع يُرسل `Reference_Date` بقيمة `null` بدلاً من تاريخ افتراضي مضلل. وتُرسل حقول ربط المستندات (`Module_ID`, `Document_Type_ID`, `Document_ID`, `Source_Document_No`) بقيم `null` لأنها لا تملك عناصر إدخال في هذه الشاشة، بينما أصبحت بنية `Allocations` متوافقة مع عقد API وتظل فارغة عندما لا توجد تسوية بمستند سابق.

## إصلاح شاشة طلبات الاعتماد

كانت الشاشة تستخدم `PaymentRequestService` وتستعلم من `api/payment-requests` بحالات خاصة بطلبات الصرف مثل `PENDING_REVIEW` و`PENDING_APPROVAL`. أما طلبات الاعتماد العامة التي تنشأ عند حفظ سند يتطلب اعتماداً فتوجد في `Approval_Requests` وتُقرأ عبر `api/approval-requests`.

| العنصر | قبل الإصلاح | بعد الإصلاح |
|---|---|---|
| مصدر القائمة | `api/payment-requests` | `api/approval-requests` |
| نموذج البيانات | `PaymentRequestListItemDto` | `ApprovalRequestListItemDto` |
| الحالات | `DRAFT`, `PENDING_REVIEW`, `PENDING_APPROVAL` | `Pending`, `UnderReview`, `Approved`, `Rejected`, `Returned` |
| التفاصيل | صفحة تفاصيل طلب صرف | صفحة تفاصيل طلب اعتماد عامة |
| الإجراءات | إجراءات طلب الصرف | `/review`, `/approve`, `/reject`, `/return` الخاصة بالاعتماد العام |

أُضيفت `ApprovalRequestsService` وسُجلت في `MauiProgram`. كما عُدلت الصفحة الرئيسية لتمرير الخدمة الصحيحة، وأضيفت صفحة تفاصيل مستقلة تعرض بيانات طلب الاعتماد وتنفذ إجراءات المراجعة والاعتماد والرفض والإعادة. يطلب التطبيق سبباً عند الاعتماد أو الرفض أو الإعادة، بما يتوافق مع تحقق الخادم.

## خطأ ترقيم طلب الصرف PAYMENT_REQUEST

رسالة الصورة (`لا يوجد إعداد ترقيم نشط لنوع المستند: PAYMENT_REQUEST`) صادرة من `NumberGeneratorService` في API، بعد أن يفشل الخادم في العثور على صف في `numbering_settings` تكون قيمة `Document_Type` فيه `PAYMENT_REQUEST` وقيمة `Is_Active` فيه مساوية لـ `1`. شاشة الجوال تصل إلى المسار الصحيح، لكن إنشاء الطلب يتوقف قبل حفظه لأن الخادم يحجز الرقم أولاً.

يوجد في المستودع سكربت تهيئة آمن وقابل لإعادة التشغيل هو `Database/Scripts/20260724_add_payment_requests.sql`، ويحتوي على إدراج إعداد `PAYMENT_REQUEST` عند غيابه وإعادة تنشيطه عند وجوده. ظهور الرسالة يعني غالباً أن هذا الجزء من السكربت لم يُنفذ على قاعدة البيانات المتصلة بالخادم، أو أن الإعداد موجود لكنه غير نشط. لا ينبغي معالجة المشكلة بإضافة رقم ثابت داخل تطبيق الجوال؛ الرقم يجب أن يُحجز مركزياً من الخادم.

أضيف أيضاً الاستعلام الآمن `Database/Scripts/20260820_diagnose_payment_request_numbering.sql`. هذا الاستعلام يقرأ فقط ولا ينفذ أي `INSERT` أو `UPDATE` أو `DELETE`، ويصنف الحالة إلى `MISSING` أو `INACTIVE` أو `DUPLICATE_ACTIVE` أو `READY`. بعد التأكد من النتيجة، يُنفذ سكربت التهيئة الموجود مسبقاً على قاعدة البيانات المستهدفة وفق إجراءات تشغيل النظام، ثم يعاد اختبار إنشاء طلب صرف من الجوال.

## التحقق

تم تنفيذ فحص `git diff --check` دون أخطاء تنسيق أو مسافات زائدة. تعذر تنفيذ `dotnet build` و`dotnet test` في بيئة المراجعة الحالية لعدم توفر .NET SDK؛ لذلك يلزم تنفيذ البناء الفعلي على جهاز التطوير الذي يحتوي على .NET MAUI SDK، ثم اختبار سيناريو حفظ سند قبض يتطلب اعتماداً والتأكد من ظهوره في شاشة الاعتماد.

## ملفات التغيير الرئيسية

| الملف | الغرض |
|---|---|
| `AlTayerERP.Mobile.Office/DTOs/VoucherEntryDtos.cs` | مطابقة عقود الإنشاء والتحديث وحقول التخصيص |
| `AlTayerERP.Mobile.Office/NewVoucherPage.xaml` و`.xaml.cs` | إدخال وربط الحقول الجديدة والتحكم في الاعتماد |
| `AlTayerERP.Mobile.Office/DTOs/ApprovalRequestDtos.cs` | نموذج طلب الاعتماد العام |
| `AlTayerERP.Mobile.Office/Services/ApprovalRequestsService.cs` | الاتصال بمسار الاعتماد العام وتنفيذ قراراته |
| `AlTayerERP.Mobile.Office/ApprovalRequestsPage.xaml` و`.xaml.cs` | عرض الطلبات العامة المعلقة |
| `AlTayerERP.Mobile.Office/ApprovalRequestDetailsPage.xaml` و`.xaml.cs` | عرض التفاصيل وتنفيذ القرارات |
| `AlTayerERP.Mobile.Office/HomePage.xaml.cs` و`MauiProgram.cs` | تسجيل الخدمة وتمريرها إلى الشاشة |
| `Database/Scripts/20260820_diagnose_payment_request_numbering.sql` | تشخيص قراءة فقط لإعداد ترقيم طلب الصرف |
| `Database/Scripts/20260724_add_payment_requests.sql` | سكربت التهيئة الآمن لإعداد ترقيم `PAYMENT_REQUEST` |

## مراجع داخلية

1. عقد API الرسمي: `AlTayerERP.API/DTOs/Accounting/CreateFinancialVoucherDto.cs`.
2. تنفيذ حفظ سند القبض في سطح المكتب: `AlTayerERP.Desktop/Accounting/ReceiptVoucher/FrmReceiptVoucher.Save.cs`.
3. Controller طلبات الاعتماد العامة: `AlTayerERP.API/Controllers/ApprovalRequestsController.cs`.
4. كيان طلب الاعتماد: `AlTayerERP.Core/Entities/ApprovalRequest.cs`.
