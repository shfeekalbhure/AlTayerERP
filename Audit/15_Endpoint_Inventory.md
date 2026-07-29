# Endpoint Inventory — نطاق المرحلة الأولى

| Controller | Method/Route | Permission | DTO/Validation | Transaction/Pagination/Tenant | 401/403/409 | الحالة |
|---|---|---|---|---|---|---|
| PaymentRequestsController | GET/POST/PUT `/api/payment-requests` | Screen `PaymentRequest` | PaymentRequestDto + Validate | scope شركة/فرع/سنة؛ list Take(500) | completion يعالج 403/409 | جزئي على integration؛ أحدث في completion |
| PaymentRequestsController | POST `/{id}/review|approve|return|reject` | Approve/Unapprove | ReasonDto | scope؛ بلا pagination | SOD في completion فقط | يحتاج نقل/UAT |
| PaymentRequestsController | POST `/{id}/create-payment-voucher` | PaymentVoucher Add في completion | CreateVoucherDto | Serializable + Payment_Voucher_ID | 403/409 عربي في completion | يحتاج UAT |
| GeographicReferencesController | GET/POST countries/governorates/cities | ScreenAuthorization | DTO داخلي | raw DML؛ لا pagination | 401 middleware | يعمل فقط مع Baseline/EF موحد |
| BranchesController | CRUD `/api/Branches` | middleware + screen checks | CreateBranchDto | company scope؛ city geography | Conflict validation | يحتاج clean DB UAT |
| MobilePaymentRequestReferencesController | GET references | Authorize | response DTOs | session scope | 401/403 | يحتاج UAT |
| MobilePaymentVouchersController | GET/details | Authorize | mobile DTO | session scope | 401/403/409 client | يحتاج UAT |

المخزون الكامل للـControllers موجود في الكود (53 Controller)؛ هذا الجدول يركز endpoints التي تتأثر مباشرة بقرار P0. غير المذكور يحتاج جرد آلي قبل إصدار عام.
