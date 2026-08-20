import AsyncStorage from "@react-native-async-storage/async-storage";
import * as SecureStore from "expo-secure-store";
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";

import {
  AlTayerApiError,
  checkHealth,
  createPayment as apiCreatePayment,
  createReceiptVoucher,
  decideApproval,
  formatApiBase,
  getCompanies,
  getLoginOptions,
  getVoucherReferences,
  isPaymentNumberingError,
  listApprovals,
  listPayments,
  login as apiLogin,
  logout as apiLogout,
  safeApiMessage,
  submitPayment,
  verifySession,
  type ApiSession,
  type CompanyOption,
  type PaymentCreateInput,
  type VoucherCreateInput,
  type VoucherReferenceData,
} from "@/lib/altayer-api";
import type { ApprovalRequest, ApprovalStatus, ConnectionSettings, DiagnosticEvent, PaymentRequest, VoucherDraft } from "@/lib/erp-types";

const PREFERENCES_KEY = "altayer-mobile-office-preferences-v2";
const SESSION_KEY = "altayer-mobile-office-api-session-v1";

const defaultSettings: ConnectionSettings = {
  mode: "WIFI",
  host: "172.16.5.82",
  port: "5021",
  state: "warning",
  numberingReady: false,
  lastCheckedAt: "لم يتم الفحص",
};

type LoginCredentials = { company: CompanyOption; loginName: string; password: string };

type ErpStore = {
  hydrated: boolean;
  signedIn: boolean;
  busy: boolean;
  companyName: string;
  companies: CompanyOption[];
  settings: ConnectionSettings;
  payments: PaymentRequest[];
  approvals: ApprovalRequest[];
  vouchers: VoucherDraft[];
  voucherReferences: VoucherReferenceData | null;
  diagnostics: DiagnosticEvent[];
  loadCompanies: () => Promise<CompanyOption[]>;
  signIn: (credentials: LoginCredentials) => Promise<void>;
  signOut: () => Promise<void>;
  saveSettings: (patch: Partial<ConnectionSettings>) => void;
  testConnection: () => Promise<void>;
  refreshPayments: () => Promise<void>;
  createPayment: (input: PaymentCreateInput, submit: boolean) => Promise<string>;
  createVoucher: (input: VoucherCreateInput) => Promise<string>;
  loadVoucherReferences: () => Promise<VoucherReferenceData>;
  refreshApprovals: () => Promise<void>;
  updateApproval: (id: string, status: ApprovalStatus, note?: string) => Promise<void>;
  addDiagnostic: (event: Omit<DiagnosticEvent, "id" | "timestamp">) => void;
};

const ErpContext = createContext<ErpStore | null>(null);

function nowLabel() { return new Intl.DateTimeFormat("ar-YE", { hour: "2-digit", minute: "2-digit" }).format(new Date()); }
function event(title: string, detail: string, type: DiagnosticEvent["type"]): DiagnosticEvent { return { id: `diagnostic-${Date.now()}-${Math.random().toString(16).slice(2)}`, title, detail, type, timestamp: "الآن" }; }
function formatDate(value: string) { if (!value) return ""; const date = new Date(value); return Number.isNaN(date.valueOf()) ? value : new Intl.DateTimeFormat("ar-YE", { dateStyle: "medium" }).format(date); }

export function ErpProvider({ children }: PropsWithChildren) {
  const [hydrated, setHydrated] = useState(false);
  const [busy, setBusy] = useState(false);
  const [session, setSession] = useState<ApiSession | null>(null);
  const [companyName, setCompanyName] = useState("");
  const [companies, setCompanies] = useState<CompanyOption[]>([]);
  const [settings, setSettings] = useState(defaultSettings);
  const [payments, setPayments] = useState<PaymentRequest[]>([]);
  const [approvals, setApprovals] = useState<ApprovalRequest[]>([]);
  const [vouchers, setVouchers] = useState<VoucherDraft[]>([]);
  const [voucherReferences, setVoucherReferences] = useState<VoucherReferenceData | null>(null);
  const [diagnostics, setDiagnostics] = useState<DiagnosticEvent[]>([event("لم يبدأ اختبار الخادم", "اضغط اختبار الاتصال للتحقق من /api/health على عنوان الخادم الحالي.", "info")]);

  const addDiagnostic = useCallback((input: Omit<DiagnosticEvent, "id" | "timestamp">) => {
    setDiagnostics((current) => [event(input.title, input.detail, input.type), ...current].slice(0, 12));
  }, []);

  const baseUrl = useCallback(() => formatApiBase(settings.host, settings.port), [settings.host, settings.port]);
  const requireSession = useCallback(() => { if (!session) throw new AlTayerApiError("لا توجد جلسة دخول محفوظة. سجل الدخول من جديد.", 401); return session; }, [session]);

  useEffect(() => {
    void (async () => {
      try {
        const [rawPreferences, rawSession] = await Promise.all([AsyncStorage.getItem(PREFERENCES_KEY), SecureStore.getItemAsync(SESSION_KEY)]);
        let restoredSettings = defaultSettings;
        if (rawPreferences) {
          const preferences = JSON.parse(rawPreferences) as { settings?: ConnectionSettings; companyName?: string };
          if (preferences.settings) restoredSettings = preferences.settings;
          if (preferences.companyName) setCompanyName(preferences.companyName);
          setSettings(restoredSettings);
        }
        if (rawSession) {
          const storedSession = JSON.parse(rawSession) as ApiSession;
          try {
            await verifySession(formatApiBase(restoredSettings.host, restoredSettings.port), storedSession);
            setSession(storedSession);
          } catch (error) {
            await SecureStore.deleteItemAsync(SESSION_KEY);
            setDiagnostics((current) => [event("تحتاج الجلسة إلى دخول جديد", safeApiMessage(error, "تعذر التحقق من الجلسة المحفوظة."), "warning"), ...current]);
          }
        }
      } catch {
        setDiagnostics((current) => [event("تعذر استعادة إعدادات التطبيق", "يمكنك إدخال عنوان الخادم وتسجيل الدخول من جديد.", "warning"), ...current]);
      } finally { setHydrated(true); }
    })();
  }, []);

  useEffect(() => {
    if (!hydrated) return;
    void AsyncStorage.setItem(PREFERENCES_KEY, JSON.stringify({ settings, companyName }));
  }, [companyName, hydrated, settings]);

  const testConnection = useCallback(async () => {
    setBusy(true);
    try {
      const health = await checkHealth(baseUrl());
      const nextState: ConnectionSettings["state"] = health.ready ? "ready" : "warning";
      setSettings((current) => ({ ...current, state: nextState, lastCheckedAt: nowLabel() }));
      addDiagnostic({ title: health.ready ? "الخادم وقاعدة البيانات جاهزان" : "الخادم يعمل لكن حالة الصحة غير مكتملة", detail: `api=${health.api || "غير معروف"} · database=${health.database || "غير معروف"}`, type: health.ready ? "success" : "warning" });
    } catch (error) {
      setSettings((current) => ({ ...current, state: "offline", lastCheckedAt: nowLabel() }));
      addDiagnostic({ title: "فشل اختبار الاتصال", detail: safeApiMessage(error, "تعذر الوصول إلى /api/health."), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl]);

  const loadCompanies = useCallback(async () => {
    setBusy(true);
    try {
      const rows = await getCompanies(baseUrl());
      setCompanies(rows);
      addDiagnostic({ title: "تم تحميل الشركات", detail: `تم استلام ${rows.length} شركة من api/Auth/LoginCompanies.`, type: rows.length ? "success" : "warning" });
      return rows;
    } catch (error) {
      addDiagnostic({ title: "تعذر تحميل قائمة الشركات", detail: safeApiMessage(error, "تحقق من /api/health وعنوان API."), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl]);

  const refreshPayments = useCallback(async () => {
    const currentSession = requireSession();
    setBusy(true);
    try {
      const rows = await listPayments(baseUrl(), currentSession);
      setPayments(rows.map((item) => ({ ...item, date: formatDate(item.date) })));
    } catch (error) {
      addDiagnostic({ title: "تعذر تحميل طلبات الصرف", detail: safeApiMessage(error), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, requireSession]);

  const refreshApprovals = useCallback(async () => {
    const currentSession = requireSession();
    setBusy(true);
    try {
      const rows = await listApprovals(baseUrl(), currentSession);
      setApprovals(rows.map((item) => ({ ...item, createdAt: formatDate(item.createdAt) })));
    } catch (error) {
      addDiagnostic({ title: "تعذر تحميل طلبات الاعتماد", detail: safeApiMessage(error), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, requireSession]);

  const signIn = useCallback(async (credentials: LoginCredentials) => {
    setBusy(true);
    try {
      const base = baseUrl();
      const options = await getLoginOptions(base, credentials.company.id, credentials.loginName, credentials.password);
      const branch = options.branches.find((item) => item.isDefault) || options.branches[0];
      const year = options.years.find((item) => item.isDefault) || options.years[0];
      if (!branch || !year) throw new AlTayerApiError("لم يُرجع الخادم فرعاً أو سنة مالية متاحة للمستخدم.");
      const nextSession = await apiLogin(base, { companyId: credentials.company.id, loginName: credentials.loginName, password: credentials.password, branchId: branch.id, yearId: year.id });
      await SecureStore.setItemAsync(SESSION_KEY, JSON.stringify(nextSession));
      setSession(nextSession);
      setCompanyName(credentials.company.name);
      setSettings((current) => ({ ...current, state: "ready", lastCheckedAt: nowLabel() }));
      addDiagnostic({ title: "تم تسجيل الدخول", detail: `الفرع: ${branch.name} · السنة: ${year.name}`, type: "success" });
      const [paymentResult, approvalResult] = await Promise.allSettled([listPayments(base, nextSession), listApprovals(base, nextSession)]);
      if (paymentResult.status === "fulfilled") setPayments(paymentResult.value.map((item) => ({ ...item, date: formatDate(item.date) })));
      if (approvalResult.status === "fulfilled") setApprovals(approvalResult.value.map((item) => ({ ...item, createdAt: formatDate(item.createdAt) })));
      if (paymentResult.status === "rejected" || approvalResult.status === "rejected") addDiagnostic({ title: "تم الدخول مع تعذر تحميل بعض القوائم", detail: "افتح مركز التشخيص لمعرفة الاستجابة التي تحتاج متابعة.", type: "warning" });
    } catch (error) {
      addDiagnostic({ title: "فشل تسجيل الدخول", detail: safeApiMessage(error, "تحقق من بيانات الدخول ثم أعد المحاولة."), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl]);

  const signOut = useCallback(async () => {
    const currentSession = session;
    try { if (currentSession) await apiLogout(baseUrl(), currentSession); } catch (error) { addDiagnostic({ title: "تم إنهاء الجلسة محلياً", detail: safeApiMessage(error, "تعذر إخطار الخادم بإنهاء الجلسة."), type: "warning" }); }
    await SecureStore.deleteItemAsync(SESSION_KEY);
    setSession(null); setPayments([]); setApprovals([]); setVouchers([]);
  }, [addDiagnostic, baseUrl, session]);

  const createPayment = useCallback(async (input: PaymentCreateInput, submit: boolean) => {
    const currentSession = requireSession();
    setBusy(true);
    try {
      const created = await apiCreatePayment(baseUrl(), currentSession, input);
      if (submit) await submitPayment(baseUrl(), currentSession, created.id);
      setSettings((current) => ({ ...current, numberingReady: true }));
      addDiagnostic({ title: submit ? "تم إرسال طلب الصرف" : "تم حفظ طلب الصرف", detail: `مرجع الطلب: ${created.requestNo}`, type: "success" });
      await refreshPayments();
      return created.requestNo;
    } catch (error) {
      const message = safeApiMessage(error, "تعذر حفظ طلب الصرف.");
      if (isPaymentNumberingError(error)) setSettings((current) => ({ ...current, numberingReady: false }));
      addDiagnostic({ title: "تعذر حفظ طلب الصرف", detail: message, type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, refreshPayments, requireSession]);

  const createVoucher = useCallback(async (input: VoucherCreateInput) => {
    const currentSession = requireSession();
    setBusy(true);
    try {
      const created = await createReceiptVoucher(baseUrl(), currentSession, input);
      const local: VoucherDraft = { id: created.id, voucherNo: created.voucherNo, party: input.party, amount: input.amount, date: formatDate(new Date().toISOString()), notes: input.notes, requiresApproval: input.requiresApproval, status: input.requiresApproval ? "PENDING_REVIEW" : "DRAFT" };
      setVouchers((current) => [local, ...current]);
      addDiagnostic({ title: "تم حفظ سند القبض", detail: `رقم السند: ${created.voucherNo}`, type: "success" });
      if (input.requiresApproval) await refreshApprovals();
      return created.voucherNo;
    } catch (error) {
      addDiagnostic({ title: "تعذر حفظ سند القبض", detail: safeApiMessage(error, "تحقق من الحقول المرجعية ثم أعد المحاولة."), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, refreshApprovals, requireSession]);

  const loadVoucherReferences = useCallback(async () => {
    const currentSession = requireSession();
    setBusy(true);
    try {
      const references = await getVoucherReferences(baseUrl(), currentSession);
      setVoucherReferences(references);
      addDiagnostic({ title: "تم تحميل مراجع سند القبض", detail: `مصادر: ${references.sources.length} · حسابات: ${references.accounts.length} · عملات: ${references.currencies.length}`, type: references.sources.length && references.accounts.length ? "success" : "warning" });
      return references;
    } catch (error) {
      addDiagnostic({ title: "تعذر تحميل مراجع سند القبض", detail: safeApiMessage(error, "تحقق من جلسة المستخدم والصلاحيات."), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, requireSession]);

  const updateApproval = useCallback(async (id: string, status: ApprovalStatus, note?: string) => {
    const action = status === "UNDER_REVIEW" ? "review" : status === "APPROVED" ? "approve" : status === "REJECTED" ? "reject" : "return";
    const currentSession = requireSession();
    setBusy(true);
    try {
      await decideApproval(baseUrl(), currentSession, id, action, note);
      await refreshApprovals();
      addDiagnostic({ title: "تم تحديث قرار الاعتماد", detail: `تمت مزامنة قرار ${status} مع الخادم.`, type: "success" });
    } catch (error) {
      addDiagnostic({ title: "تعذر تحديث قرار الاعتماد", detail: safeApiMessage(error), type: "error" });
      throw error;
    } finally { setBusy(false); }
  }, [addDiagnostic, baseUrl, refreshApprovals, requireSession]);

  const value = useMemo<ErpStore>(() => ({
    hydrated, signedIn: Boolean(session), busy, companyName, companies, settings, payments, approvals, vouchers, voucherReferences, diagnostics,
    loadCompanies, signIn, signOut, saveSettings: (patch) => setSettings((current) => ({ ...current, ...patch })), testConnection, refreshPayments, createPayment, createVoucher, loadVoucherReferences, refreshApprovals, updateApproval, addDiagnostic,
  }), [addDiagnostic, approvals, busy, companies, companyName, createPayment, createVoucher, diagnostics, hydrated, loadCompanies, loadVoucherReferences, payments, refreshApprovals, refreshPayments, session, settings, signIn, signOut, testConnection, updateApproval, voucherReferences, vouchers]);

  return <ErpContext.Provider value={value}>{children}</ErpContext.Provider>;
}

export function useErpStore() {
  const context = useContext(ErpContext);
  if (!context) throw new Error("useErpStore must be used within ErpProvider");
  return context;
}
