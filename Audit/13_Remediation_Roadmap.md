# خارطة المعالجة — محدثة

| المرحلة | التنفيذ | شرط الإغلاق |
|---|---|---|
| P0 | اجعل `agent/mobile-payment-request-clean-db-integration` رأسًا موحدًا حقيقيًا: Baseline `a026…` + نقل انتقائي لإصلاحات الشاشات وcompletion، دون SQL/Migration قديمة | SHA جديد، Build API/Desktop/Android، قاعدة اختبار نظيفة، UAT المراجع |
| P1 | نقل AT-008، HTTPS/configuration، sessions وrate limiting، اختبارات محاسبية | فصل 3 مستخدمين، 401/403/409، سند صرف واحد |
| P2 | Pagination، ProblemDetails، AT-011، CI integration DB | TRX/UAT Desktop/Mobile |
| P3 | النقل والشحن والتذاكر ثم Driver/Customer | سيناريو بوليصة كامل/offline/GPS |
| P4/P5 | observability/release ثم UX/accessibility | release قابل للرجوع وفحص UI |

لا يكفي وجود الفرع؛ وضعه الحالي مطابق لـBaseline ولا يحمل الإصلاحات اللاحقة.
