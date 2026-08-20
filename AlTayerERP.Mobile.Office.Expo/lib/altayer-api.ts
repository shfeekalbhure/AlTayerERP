import type { ApprovalRequest, PaymentRequest, VoucherDraft } from "@/lib/erp-types";

export type ApiSession = {
  userId: number;
  fullName: string;
  companyId: string;
  branchId: number;
  yearId: number;
  sessionId: string;
  accessToken: string;
  refreshToken?: string;
  accessTokenExpiresAt?: string;
};

export type CompanyOption = { id: string; name: string };
export type LoginOption = { id: number; name: string; isDefault: boolean };
export type LoginOptions = { branches: LoginOption[]; years: LoginOption[] };
export type PaymentCreateInput = {
  beneficiary: string; amount: number; description: string; accountId: string; currencyId: number; paymentMethodId: number; partyId?: string; costCenterId?: string; referenceNo?: string;
};
export type VoucherCreateInput = {
  party: string; partyId?: string; amount: number; reference: string; notes: string; requiresApproval: boolean; voucherTypeId: number; voucherStatusId: number; cashAccountId: string; revenueAccountId: string; currencyId: number; exchangeRate: number; paymentMethodId?: number; costCenterId?: string;
};
export type VoucherReferenceData = {
  sources: { id: string; label: string }[];
  accounts: { id: string; label: string }[];
  currencies: { id: number; label: string; exchangeRate: number; isDefault: boolean }[];
  parties: { id: string; label: string }[];
  paymentMethods: { id: number; label: string }[];
  costCenters: { id: string; label: string }[];
  voucherTypeId: number;
  draftStatusId: number;
};

export type ApiFieldErrors = Record<string, string[]>;

const businessErrorMessages: Record<string, string> = {
  NUMBERING_NOT_CONFIGURED: "لا يوجد إعداد ترقيم نشط للعملية. يلزم مراجعة إعداد PAYMENT_REQUEST على خادم AlTayerERP.",
  PAYMENT_REQUEST_NUMBERING_NOT_CONFIGURED: "لا يوجد إعداد ترقيم نشط لطلبات الصرف. اطلب من مسؤول النظام تشغيل استعلام التشخيص الآمن ثم تهيئة الترقيم على قاعدة بيانات API.",
  SELF_APPROVAL_DENIED: "لا تسمح سياسة الاعتماد باعتماد منشئ الطلب لطلبه نفسه.",
  CONCURRENCY_CONFLICT: "تغيرت بيانات الطلب لدى الخادم. أعد تحميل القائمة قبل اتخاذ قرار جديد.",
  APPROVAL_STATE_INVALID: "لا تتوافق حالة طلب الاعتماد الحالية مع هذا الإجراء. أعد تحميل القائمة.",
  PERMISSION_DENIED: "ليس لديك صلاحية لتنفيذ هذا الإجراء.",
  SCOPE_DENIED: "الطلب خارج نطاق الشركة أو الفرع أو السنة المالية المسموح بها.",
  STATE_TRANSITION_INVALID: "لا يمكن نقل العملية إلى هذه الحالة في وضعها الحالي.",
  IDEMPOTENCY_CONFLICT: "تمت معالجة عملية مطابقة سابقاً؛ أعد تحميل القائمة للتحقق من النتيجة قبل المحاولة مجدداً.",
};

export class AlTayerApiError extends Error {
  constructor(message: string, public status?: number, public endpoint?: string, public code?: string, public fieldErrors?: ApiFieldErrors, public correlationId?: string) { super(message); }
}

const timeoutMs = 20_000;
const value = (source: Record<string, unknown>, ...keys: string[]) => keys.map((key) => source[key]).find((item) => item !== undefined && item !== null);
const stringValue = (source: Record<string, unknown>, ...keys: string[]) => String(value(source, ...keys) ?? "");
const numberValue = (source: Record<string, unknown>, ...keys: string[]) => Number(value(source, ...keys) ?? 0);

export function formatApiBase(host: string, port: string) {
  const normalizedHost = host.trim().replace(/^https?:\/\//i, "").replace(/\/$/, "");
  const normalizedPort = port.trim();
  if (!normalizedHost) throw new AlTayerApiError("عنوان خادم API مطلوب.");
  return `http://${normalizedHost}${normalizedPort ? `:${normalizedPort}` : ""}`;
}

export function safeApiMessage(error: unknown, fallback = "تعذر إتمام العملية مع الخادم.") {
  if (error instanceof AlTayerApiError) {
    const firstFieldError = Object.values(error.fieldErrors || {}).flat().find(Boolean);
    const message = firstFieldError || (error.code ? businessErrorMessages[error.code.toUpperCase()] : undefined) || error.message || fallback;
    return error.correlationId ? `${message} (مرجع المتابعة: ${error.correlationId})` : message;
  }
  if (error instanceof Error && error.name === "AbortError") return "انتهت مهلة الاتصال بالخادم. تحقق من الشبكة والمنفذ 5021.";
  if (error instanceof TypeError) return "تعذر الوصول إلى الخادم. تحقق من عنوان الشبكة وجدار الحماية.";
  return error instanceof Error && error.message ? error.message : fallback;
}

export function isPaymentNumberingError(error: unknown) {
  if (!(error instanceof AlTayerApiError)) return false;
  const fingerprint = `${error.code || ""} ${error.message || ""}`.toUpperCase();
  return fingerprint.includes("PAYMENT_REQUEST") || fingerprint.includes("NUMBERING") || fingerprint.includes("ترقيم");
}

export function createIdempotencyKey(scope: string) {
  return `${scope}-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 10)}`;
}

function parseFieldErrors(payload: Record<string, unknown>): ApiFieldErrors | undefined {
  const raw = value(payload, "fieldErrors", "FieldErrors", "errors", "Errors");
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return undefined;
  const entries = Object.entries(raw as Record<string, unknown>)
    .map(([key, messages]) => [key, Array.isArray(messages) ? messages.map(String).filter(Boolean) : [String(messages)].filter(Boolean)] as const)
    .filter(([, messages]) => messages.length);
  return entries.length ? Object.fromEntries(entries) : undefined;
}

async function request<T>(baseUrl: string, endpoint: string, options: RequestInit & { token?: string } = {}): Promise<T> {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const response = await fetch(`${baseUrl.replace(/\/$/, "")}/${endpoint.replace(/^\//, "")}`, {
      ...options,
      signal: controller.signal,
      headers: {
        Accept: "application/json",
        ...(options.body ? { "Content-Type": "application/json" } : {}),
        ...(options.token ? { Authorization: `Bearer ${options.token}` } : {}),
        ...options.headers,
      },
    });
    const raw = await response.text();
    let payload: unknown = raw;
    try { payload = raw ? JSON.parse(raw) : null; } catch { /* Use safe text below. */ }
    if (!response.ok) {
      const object = payload && typeof payload === "object" ? payload as Record<string, unknown> : {};
      const message = stringValue(object, "message", "detail", "title") || (response.status === 401 ? "انتهت جلسة الدخول. سجل الدخول من جديد." : fallbackMessage(response.status));
      throw new AlTayerApiError(message, response.status, endpoint, stringValue(object, "code", "Code", "errorCode", "ErrorCode") || undefined, parseFieldErrors(object), stringValue(object, "correlationId", "CorrelationId", "traceId", "TraceId") || undefined);
    }
    return payload as T;
  } finally {
    clearTimeout(timeout);
  }
}

function fallbackMessage(status: number) {
  if (status === 403) return "ليس لديك صلاحية لتنفيذ هذه العملية.";
  if (status === 404) return "المورد المطلوب غير موجود على الخادم.";
  if (status === 409) return "تغيرت حالة العملية ولا يمكن تنفيذ الإجراء الحالي.";
  if (status >= 500) return "حدث خطأ في الخادم. لم يتم عرض التفاصيل التقنية.";
  return "تعذر إتمام الطلب. تحقق من البيانات ثم أعد المحاولة.";
}

export async function checkHealth(baseUrl: string) {
  const health = await request<Record<string, unknown>>(baseUrl, "api/health");
  const api = stringValue(health, "api").toLowerCase();
  const database = stringValue(health, "database").toLowerCase();
  return { ready: Boolean(value(health, "isReady")) || (api === "ready" && database === "ready"), api, database };
}

export async function getCompanies(baseUrl: string): Promise<CompanyOption[]> {
  const rows = await request<Record<string, unknown>[]>(baseUrl, "api/Auth/LoginCompanies");
  return rows.map((row) => ({ id: stringValue(row, "company_ID", "Company_ID"), name: stringValue(row, "company_Name_AR", "Company_Name_AR", "company_Name", "Company_Name") })).filter((item) => item.id && item.name);
}

export async function getLoginOptions(baseUrl: string, companyId: string, loginName: string, password: string): Promise<LoginOptions> {
  const data = await request<Record<string, unknown>>(baseUrl, "api/Auth/LoginOptions", { method: "POST", body: JSON.stringify({ Company_ID: companyId, Login_Name: loginName, Password: password, Device_ID: "altayer-expo" }) });
  const mapOptions = (input: unknown) => Array.isArray(input) ? input.map((raw) => {
    const row = raw as Record<string, unknown>;
    return { id: numberValue(row, "branch_ID", "Branch_ID", "year_ID", "Year_ID"), name: stringValue(row, "branch_Name", "Branch_Name", "year_Name", "Year_Name"), isDefault: Boolean(value(row, "is_Default", "Is_Default")) };
  }).filter((item) => item.id > 0) : [];
  return { branches: mapOptions(value(data, "branches", "Branches")), years: mapOptions(value(data, "years", "Years")) };
}

export async function login(baseUrl: string, input: { companyId: string; loginName: string; password: string; branchId: number; yearId: number }): Promise<ApiSession> {
  const row = await request<Record<string, unknown>>(baseUrl, "api/Auth/Login", { method: "POST", body: JSON.stringify({ Company_ID: input.companyId, Login_Name: input.loginName, Password: input.password, Branch_ID: input.branchId, Year_ID: input.yearId, User_ID: 0, Device_ID: "altayer-expo" }) });
  const accessToken = stringValue(row, "access_Token", "Access_Token", "accessToken");
  if (!accessToken) throw new AlTayerApiError("لم يعُد الخادم برمز جلسة صالح.");
  return { userId: numberValue(row, "user_ID", "User_ID", "userId"), fullName: stringValue(row, "full_Name", "Full_Name", "fullName"), companyId: stringValue(row, "company_ID", "Company_ID", "companyId"), branchId: numberValue(row, "branch_ID", "Branch_ID", "branchId"), yearId: numberValue(row, "year_ID", "Year_ID", "yearId"), sessionId: stringValue(row, "session_ID", "Session_ID", "sessionId"), accessToken, refreshToken: stringValue(row, "refresh_Token", "Refresh_Token", "refreshToken") || undefined, accessTokenExpiresAt: stringValue(row, "access_Token_Expires_At", "Access_Token_Expires_At", "accessTokenExpiresAt") || undefined };
}

export async function verifySession(baseUrl: string, session: ApiSession) {
  await request<unknown>(baseUrl, "api/Auth/CurrentSession", { token: session.accessToken });
}

export async function logout(baseUrl: string, session: ApiSession) { await request<unknown>(baseUrl, "api/Auth/Logout", { method: "POST", token: session.accessToken }); }

export async function listPayments(baseUrl: string, session: ApiSession): Promise<PaymentRequest[]> {
  const rows = await request<Record<string, unknown>[]>(baseUrl, "api/payment-requests", { token: session.accessToken });
  return rows.map((row) => ({ id: stringValue(row, "payment_Request_ID", "Payment_Request_ID", "paymentRequestId"), requestNo: stringValue(row, "request_No", "Request_No", "requestNo"), beneficiary: stringValue(row, "beneficiary_Name", "Beneficiary_Name", "beneficiaryName"), amount: numberValue(row, "approved_Local_Total", "Approved_Local_Total", "local_Total", "Local_Total"), date: stringValue(row, "request_Date", "Request_Date", "requestDate"), status: stringValue(row, "status", "Status") as PaymentRequest["status"], description: stringValue(row, "description", "Description"), lineCount: Array.isArray(value(row, "details", "Details")) ? (value(row, "details", "Details") as unknown[]).length : 1 }));
}

export async function createPayment(baseUrl: string, session: ApiSession, input: PaymentCreateInput) {
  const now = new Date().toISOString();
  const payload = { Request_Date: now, Beneficiary_Name: input.beneficiary, Party_ID: input.partyId || null, Payment_Method_ID: input.paymentMethodId, Header_Reference_No: input.referenceNo || null, Description: input.description, Lines: [{ Account_ID: input.accountId, Cost_Center_ID: input.costCenterId || null, Currency_ID: input.currencyId, Exchange_Rate: 1, Foreign_Amount: 0, Local_Amount: input.amount, Reference_No: input.referenceNo || null, Description: input.description }] };
  const row = await request<Record<string, unknown>>(baseUrl, "api/payment-requests", { method: "POST", token: session.accessToken, body: JSON.stringify(payload), headers: { "Idempotency-Key": createIdempotencyKey("payment-request") } });
  return { id: stringValue(row, "payment_Request_ID", "Payment_Request_ID", "paymentRequestId"), requestNo: stringValue(row, "request_No", "Request_No", "requestNo") };
}

export async function submitPayment(baseUrl: string, session: ApiSession, id: string) { await request<unknown>(baseUrl, `api/payment-requests/${id}/submit`, { method: "POST", token: session.accessToken }); }

export async function listApprovals(baseUrl: string, session: ApiSession): Promise<ApprovalRequest[]> {
  const rows = await request<Record<string, unknown>[]>(baseUrl, "api/approval-requests", { token: session.accessToken });
  return rows.map((row) => ({ id: stringValue(row, "approval_ID", "Approval_ID", "approvalId"), reference: stringValue(row, "reference_ID", "Reference_ID", "referenceId"), title: stringValue(row, "request_Type", "Request_Type", "entity_Type", "Entity_Type") || "طلب اعتماد", requester: stringValue(row, "requested_By", "Requested_By", "requestedBy"), amount: numberValue(row, "amount", "Amount"), createdAt: stringValue(row, "requested_At", "Requested_At", "requestedAt"), status: stringValue(row, "status", "Status") as ApprovalRequest["status"], reason: stringValue(row, "reason", "Reason"), type: stringValue(row, "reference_Type", "Reference_Type") === "PAYMENT_REQUEST" ? "PAYMENT_REQUEST" : "RECEIPT_VOUCHER" }));
}

export async function decideApproval(baseUrl: string, session: ApiSession, id: string, action: "review" | "approve" | "reject" | "return", reason?: string) {
  return request<unknown>(baseUrl, `api/approval-requests/${id}/${action}`, { method: "POST", token: session.accessToken, body: JSON.stringify({ Reason: reason || null }) });
}

export async function createReceiptVoucher(baseUrl: string, session: ApiSession, input: VoucherCreateInput) {
  const now = new Date().toISOString();
  const payload = { Voucher_Type_ID: input.voucherTypeId, Voucher_Status_ID: input.voucherStatusId, Branch_ID: String(session.branchId), Fiscal_Year_ID: session.yearId, Voucher_Date: now, Transaction_Date: now, Cash_Account_ID: input.cashAccountId, Party_ID: input.partyId || null, Received_From_Name: input.party, Payment_Method_ID: input.paymentMethodId || null, Currency_ID: input.currencyId, Exchange_Rate: input.exchangeRate, Amount: input.amount, Foreign_Total: 0, Local_Total: input.amount, Reference_No: input.reference || null, Reference_Date: now, Against_Text: input.notes || null, Description: input.notes || null, Notes: input.notes || null, Requires_Approval: input.requiresApproval, Details: [{ Line_No: 1, Account_ID: input.cashAccountId, Description: input.notes || null, Currency_ID: input.currencyId, Exchange_Rate: input.exchangeRate, Foreign_Amount: 0, Local_Amount: input.amount, Debit_Amount: input.amount, Credit_Amount: 0, Line_Type: 1 }, { Line_No: 2, Account_ID: input.revenueAccountId, Description: input.notes || null, Cost_Center_ID: input.costCenterId || null, Reference_No: input.reference || null, Currency_ID: input.currencyId, Exchange_Rate: input.exchangeRate, Foreign_Amount: 0, Local_Amount: input.amount, Debit_Amount: 0, Credit_Amount: input.amount, Line_Type: 2 }] };
  const row = await request<Record<string, unknown>>(baseUrl, "api/FinancialVoucher", { method: "POST", token: session.accessToken, body: JSON.stringify(payload), headers: { "Idempotency-Key": createIdempotencyKey("receipt-voucher") } });
  if (value(row, "success", "Success") === false) throw new AlTayerApiError(stringValue(row, "message", "Message") || "تعذر حفظ السند.");
  return { id: stringValue(row, "voucher_ID", "Voucher_ID", "voucherId"), voucherNo: stringValue(row, "voucher_No", "Voucher_No", "voucherNo") };
}

export async function getVoucherReferences(baseUrl: string, session: ApiSession): Promise<VoucherReferenceData> {
  const row = await request<Record<string, unknown>>(baseUrl, "api/mobile/voucher-entry-references?type=RECEIPT", { token: session.accessToken });
  const mapLookup = (source: unknown) => Array.isArray(source) ? source.map((raw) => {
    const item = raw as Record<string, unknown>;
    return { id: stringValue(item, "id", "Id", "accountId", "AccountId"), label: stringValue(item, "displayName", "DisplayName", "name", "Name") };
  }).filter((item) => item.id && item.label) : [];
  const mapCurrency = (source: unknown) => Array.isArray(source) ? source.map((raw) => {
    const item = raw as Record<string, unknown>;
    return { id: numberValue(item, "id", "Id"), label: stringValue(item, "displayName", "DisplayName"), exchangeRate: numberValue(item, "exchangeRate", "ExchangeRate") || 1, isDefault: Boolean(value(item, "isDefault", "IsDefault")) };
  }).filter((item) => item.id > 0) : [];
  const mapMethod = (source: unknown) => Array.isArray(source) ? source.map((raw) => {
    const item = raw as Record<string, unknown>;
    return { id: numberValue(item, "id", "Id"), label: stringValue(item, "displayName", "DisplayName") };
  }).filter((item) => item.id > 0) : [];
  const sourceRows = Array.isArray(value(row, "sources", "Sources")) ? (value(row, "sources", "Sources") as Record<string, unknown>[]).map((item) => ({ id: stringValue(item, "accountId", "AccountId"), label: stringValue(item, "displayName", "DisplayName") })).filter((item) => item.id && item.label) : [];
  const type = value(row, "voucherType", "VoucherType") as Record<string, unknown> | undefined;
  const status = value(row, "draftStatus", "DraftStatus") as Record<string, unknown> | undefined;
  return { sources: sourceRows, accounts: mapLookup(value(row, "accounts", "Accounts")), currencies: mapCurrency(value(row, "currencies", "Currencies")), parties: mapLookup(value(row, "parties", "Parties")), paymentMethods: mapMethod(value(row, "paymentMethods", "PaymentMethods")), costCenters: mapLookup(value(row, "costCenters", "CostCenters")), voucherTypeId: type ? numberValue(type, "id", "Id") : 0, draftStatusId: status ? numberValue(status, "id", "Id") : 0 };
}
