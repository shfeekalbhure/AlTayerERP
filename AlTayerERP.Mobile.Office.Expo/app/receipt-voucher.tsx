import { router } from "expo-router";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Alert, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, Field, palette, PrimaryButton, SectionTitle, StatusPill } from "@/components/erp-ui";
import { safeApiMessage } from "@/lib/altayer-api";
import { useErpStore } from "@/lib/erp-store";

export default function ReceiptVoucherScreen() {
  const { createVoucher, loadVoucherReferences, voucherReferences, busy } = useErpStore();
  const [party, setParty] = useState("");
  const [partyId, setPartyId] = useState("");
  const [amount, setAmount] = useState("");
  const [reference, setReference] = useState("");
  const [costCenterId, setCostCenterId] = useState("");
  const [notes, setNotes] = useState("");
  const [cashAccountId, setCashAccountId] = useState("");
  const [revenueAccountId, setRevenueAccountId] = useState("");
  const [currencyId, setCurrencyId] = useState("");
  const [exchangeRate, setExchangeRate] = useState("1");
  const [paymentMethodId, setPaymentMethodId] = useState("");
  const [requiresApproval, setRequiresApproval] = useState(true);

  const defaultsLabel = useMemo(() => voucherReferences ? `المصادر ${voucherReferences.sources.length} · الحسابات ${voucherReferences.accounts.length} · العملات ${voucherReferences.currencies.length}` : "لم تُحمّل المراجع بعد", [voucherReferences]);
  const applyReferences = useCallback(async () => {
    try {
      const refs = await loadVoucherReferences();
      const defaultCurrency = refs.currencies.find((item) => item.isDefault) || refs.currencies[0];
      const defaultParty = refs.parties[0];
      const defaultCash = refs.sources[0] || refs.accounts[0];
      const defaultRevenue = refs.accounts.find((item) => item.id !== defaultCash?.id) || refs.accounts[0];
      const defaultMethod = refs.paymentMethods[0];
      const defaultCostCenter = refs.costCenters[0];
      if (defaultParty) { setPartyId(defaultParty.id); setParty(defaultParty.label); }
      if (defaultCash) setCashAccountId(defaultCash.id);
      if (defaultRevenue) setRevenueAccountId(defaultRevenue.id);
      if (defaultCurrency) { setCurrencyId(String(defaultCurrency.id)); setExchangeRate(String(defaultCurrency.exchangeRate)); }
      if (defaultMethod) setPaymentMethodId(String(defaultMethod.id));
      if (defaultCostCenter) setCostCenterId(defaultCostCenter.id);
      Alert.alert("تم تحميل المراجع", "تم اختيار القيم الافتراضية المتاحة. راجع المعرّفات قبل الحفظ.");
    } catch (error) { Alert.alert("تعذر تحميل المراجع", safeApiMessage(error, "تحقق من الجلسة والصلاحيات ثم أعد المحاولة.")); }
  }, [loadVoucherReferences]);

  useEffect(() => { void applyReferences(); }, [applyReferences]);

  const save = async () => {
    const numericAmount = Number(amount.replace(/,/g, ""));
    const numericCurrency = Number(currencyId);
    const numericRate = Number(exchangeRate);
    if (!voucherReferences || !voucherReferences.voucherTypeId || !voucherReferences.draftStatusId || !party.trim() || !numericAmount || !cashAccountId.trim() || !revenueAccountId.trim() || !numericCurrency) {
      Alert.alert("بيانات غير مكتملة", "حمّل المراجع ثم أدخل الطرف والمبلغ ومعرفي الحساب والعملة قبل الحفظ.");
      return;
    }
    try {
      const voucherNo = await createVoucher({ party, partyId: partyId || undefined, amount: numericAmount, reference, notes, requiresApproval, voucherTypeId: voucherReferences.voucherTypeId, voucherStatusId: voucherReferences.draftStatusId, cashAccountId, revenueAccountId, currencyId: numericCurrency, exchangeRate: numericRate || 1, paymentMethodId: paymentMethodId ? Number(paymentMethodId) : undefined, costCenterId: costCenterId || undefined });
      Alert.alert("تم حفظ سند القبض", requiresApproval ? `تم إنشاء ${voucherNo} وإرساله لمسار الاعتماد.` : `تم حفظ ${voucherNo} بنجاح.`, [{ text: "عودة", onPress: () => router.back() }]);
    } catch (error) { Alert.alert("تعذر حفظ سند القبض", safeApiMessage(error, "راجع رسالة API في مركز التشخيص.")); }
  };

  return <AppScreen><AppHeader title="سند قبض جديد" subtitle="مراجع API وحفظ مالي فعلي" back onBack={() => router.back()} />
    <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled"><Card><View style={styles.topRow}><StatusPill label={requiresApproval ? "يتطلب اعتماداً" : "لا يتطلب اعتماداً"} tone={requiresApproval ? "warning" : "info"} /><Text style={styles.typeText}>RECEIPT_VOUCHER</Text></View><Text style={styles.referenceSummary}>{defaultsLabel}</Text><PrimaryButton label={busy ? "جاري تحميل المراجع…" : "تحميل المراجع من API"} tone="outline" icon="sync" disabled={busy} onPress={applyReferences} style={styles.referenceButton} /></Card>
      <SectionTitle title="بيانات الرأس" /><Card><Field label="الطرف" value={party} onChangeText={setParty} placeholder="اسم العميل أو الجهة" /><Field label="معرف الطرف (Party_ID)" value={partyId} onChangeText={setPartyId} placeholder="يُملأ من المراجع" /><Field label="المبلغ المحلي" value={amount} onChangeText={setAmount} keyboardType="numeric" placeholder="0" /><Field label="الملاحظات" value={notes} onChangeText={setNotes} multiline placeholder="ملاحظات السند" /></Card>
      <SectionTitle title="المرجع والتفاصيل" caption="قيم مراجع مطابقة لعقد API الرسمي." /><Card><Field label="رقم المرجع" value={reference} onChangeText={setReference} placeholder="رقم المستند أو الرحلة" /><Field label="حساب النقد (Cash_Account_ID)" value={cashAccountId} onChangeText={setCashAccountId} placeholder="معرف حساب المصدر" /><Field label="حساب الإيراد (Account_ID)" value={revenueAccountId} onChangeText={setRevenueAccountId} placeholder="معرف حساب سطر القيد" /><Field label="العملة (Currency_ID)" value={currencyId} onChangeText={setCurrencyId} keyboardType="numeric" placeholder="معرف العملة" /><Field label="سعر الصرف" value={exchangeRate} onChangeText={setExchangeRate} keyboardType="numeric" placeholder="1" /><Field label="طريقة الدفع (Payment_Method_ID)" value={paymentMethodId} onChangeText={setPaymentMethodId} keyboardType="numeric" placeholder="اختياري" /><Field label="مركز التكلفة (Cost_Center_ID)" value={costCenterId} onChangeText={setCostCenterId} placeholder="اختياري" /><View style={styles.detailRow}><View style={styles.detailCopy}><Text style={styles.detailTitle}>طلب اعتماد</Text><Text style={styles.detailText}>ينشئ طلباً عاماً في قائمة الاعتماد عند حفظ السند.</Text></View><Pressable onPress={() => setRequiresApproval((current) => !current)} style={({ pressed }) => [styles.toggle, requiresApproval && styles.toggleActive, pressed && styles.pressed]}><View style={[styles.knob, requiresApproval && styles.knobActive]} /></Pressable></View></Card>
      <PrimaryButton label={busy ? "جاري حفظ السند…" : "حفظ سند القبض"} tone="navy" icon="save" disabled={busy} onPress={save} style={styles.save} />
    </ScrollView>
  </AppScreen>;
}

const styles = StyleSheet.create({ content: { padding: 18, paddingBottom: 34 }, topRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center" }, typeText: { color: palette.muted, fontSize: 11 }, referenceSummary: { marginTop: 12, color: palette.muted, fontSize: 11, writingDirection: "rtl", textAlign: "right" }, referenceButton: { marginTop: 12, minHeight: 43 }, detailRow: { paddingTop: 3, flexDirection: "row", alignItems: "center", justifyContent: "space-between" }, detailCopy: { flex: 1, alignItems: "flex-end", paddingLeft: 12 }, detailTitle: { color: palette.text, fontSize: 14, fontWeight: "800", writingDirection: "rtl" }, detailText: { marginTop: 4, color: palette.muted, fontSize: 11, textAlign: "right", writingDirection: "rtl", lineHeight: 16 }, toggle: { width: 48, height: 29, borderRadius: 20, padding: 3, backgroundColor: "#CCD5E1", justifyContent: "center" }, toggleActive: { backgroundColor: palette.teal }, knob: { height: 23, width: 23, borderRadius: 14, backgroundColor: "#FFFFFF", alignSelf: "flex-end" }, knobActive: { alignSelf: "flex-start" }, save: { marginTop: 22 }, pressed: { opacity: 0.78, transform: [{ scale: 0.98 }] } });
