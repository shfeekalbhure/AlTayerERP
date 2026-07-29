# Entity / DbSet / Migration Matrix — عناصر P0

| Entity | DbSet/Table | PK/FK/Index | Nullability/Collation | Migration coverage | المستهلك |
|---|---|---|---|---|---|
| Country | `Countries` / `countries` | PK Country_ID؛ ISO/code unique | Baseline `utf8mb4_unicode_ci` | Baseline_Phase1 | API geographic/Desktop branches |
| Governorate | `Governorates` / `governorates` | FK Country؛ code/name unique per country | Baseline collation | Baseline_Phase1 | API/Desktop |
| City | `Cities` / `cities` | FK Country/Governorate؛ code/name unique | Baseline collation | Baseline_Phase1 | API/Desktop |
| BranchType | `Branch_Types` / `branch_types` | code unique; active/sort index | Baseline collation | Baseline_Phase1 | API/Desktop |
| PaymentRequest | `Payment_Requests` / `payment_requests` | unique company/branch/year/request no؛ FK voucher | required scope ids | Baseline_Phase1 | API/Mobile |
| PaymentRequestLine | `Payment_Request_Lines` | FK request; unique request/line | monetary precision | Baseline_Phase1 | API/Mobile |
| FinancialVoucherHeader | `Financial_Voucher_Headers` | PK/FK/indexes | mixed legacy identifiers | Baseline_Phase1 | API/Desktop/Mobile |

الـNullability والـFK التفصيلية يجب إثباتها من migration عند تطبيق قاعدة اختبار فقط؛ هذا التدقيق لم يطبقها.
