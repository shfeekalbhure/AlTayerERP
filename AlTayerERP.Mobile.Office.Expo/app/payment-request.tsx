import { router } from "expo-router";
import { useState } from "react";
import { Alert, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, Field, palette, PrimaryButton, SectionTitle, StatusPill } from "@/components/erp-ui";
import { isPaymentNumberingError, safeApiMessage } from "@/lib/altayer-api";
import { useErpStore } from "@/lib/erp-store";

export default function PaymentRequestScreen() {
  const { createPayment, busy, settings } = useErpStore();
  const [beneficiary, setBeneficiary] = useState("");
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [accountId, setAccountId] = useState("");
  const [currencyId, setCurrencyId] = useState("");
  const [paymentMethodId, setPaymentMethodId] = useState("");
  const [partyId, setPartyId] = useState("");
  const [costCenterId, setCostCenterId] = useState("");
  const [referenceNo, setReferenceNo] = useState("");

  const save = (send: boolean) => {
    const numericAmount = Number(amount.replace(/,/g, ""));
    const parsedCurrencyId = Number(currencyId);
    const parsedPaymentMethodId = Number(paymentMethodId);
    if (!beneficiary.trim() || !numericAmount || !description.trim() || !accountId.trim() || !Number.isInteger(parsedCurrencyId) || parsedCurrencyId <= 0 || !Number.isInteger(parsedPaymentMethodId) || parsedPaymentMethodId <= 0) {
      Alert.alert("بيانات غير مكتملة", "أدخل المستفيد والمبلغ والوصف ومعرف الحساب ومعرف العملة وطريقة الدفع قبل الحفظ.");
      return;
    }
    void (async () => {
      try {
        const requestNo = await createPayment({ beneficiary: beneficiary.trim(), amount: numericAmount, description: description.trim(), accountId: accountId.trim(), currencyId: parsedCurrencyId, paymentMethodId: parsedPaymentMethodId, partyId: partyId.trim() || undefined, costCenterId: costCenterId.trim() || undefined, referenceNo: referenceNo.trim() || undefined }, send);
        Alert.alert(send ? "تم إرسال الطلب" : "تم حفظ المسودة", `رقم المرجع: ${requestNo}`, [{ text: "عودة للقائمة", onPress: () => router.back() }]);
      } catch (error) {
        const message = safeApiMessage(error, "تعذر حفظ طلب الصرف.");
        Alert.alert("تعذر حفظ طلب الصرف", message, isPaymentNumberingError(error) ? [{ text: "فتح التشخيص", onPress: () => router.replace("/(tabs)/diagnostics") }, { text: "حسنًا", style: "cancel" }] : undefined);
      }
    })();
  };

  return <AppScreen><AppHeader title="طلب صرف جديد" subtitle="استكمال رأس الطلب والأسطر" back onBack={() => router.back()} />
    <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
      <Card><View style={styles.statusRow}><StatusPill label={settings.numberingReady ? "الترقيم جاهز" : "فحص الترقيم مطلوب"} tone={settings.numberingReady ? "success" : "warning"} /><Text style={styles.statusText}>نوع المستند: PAYMENT_REQUEST</Text></View></Card>
      <SectionTitle title="بيانات الطلب" caption="سيُولد الرقم مركزياً من API عند الحفظ." />
      <Card><Field label="المستفيد" value={beneficiary} onChangeText={setBeneficiary} placeholder="اسم المستفيد" /><Field label="المبلغ المحلي" value={amount} onChangeText={setAmount} keyboardType="numeric" placeholder="0" /><Field label="وصف الطلب" value={description} onChangeText={setDescription} multiline placeholder="سبب الصرف أو وصف العملية" /></Card>
      <SectionTitle title="تفاصيل السطر المحاسبي" caption="استعمل المعرّفات المعتمدة في بيانات الشركة والفرع الحاليين." />
      <Card><Field label="معرف الحساب" value={accountId} onChangeText={setAccountId} placeholder="مثال: 12101" /><Field label="معرف العملة" value={currencyId} onChangeText={setCurrencyId} keyboardType="numeric" placeholder="مثال: 1" /><Field label="معرف طريقة الدفع" value={paymentMethodId} onChangeText={setPaymentMethodId} keyboardType="numeric" placeholder="مثال: 1" /><Field label="معرف الطرف (اختياري)" value={partyId} onChangeText={setPartyId} placeholder="معرف العميل أو المورد" /><Field label="معرف مركز التكلفة (اختياري)" value={costCenterId} onChangeText={setCostCenterId} placeholder="مركز التكلفة" /><Field label="رقم المرجع (اختياري)" value={referenceNo} onChangeText={setReferenceNo} placeholder="مرجع فاتورة أو رحلة" /><View style={styles.linePreview}><Text style={styles.previewTitle}>معاينة السطر المحاسبي</Text><Text style={styles.previewValue}>{amount || "0"} · {accountId || "الحساب غير محدد"}</Text></View></Card>
      <View style={styles.actions}><PrimaryButton disabled={busy} label={busy ? "يتم الحفظ…" : "حفظ مسودة"} tone="outline" icon="save" onPress={() => save(false)} style={styles.half} /><PrimaryButton disabled={busy} label={busy ? "يتم الإرسال…" : "حفظ وإرسال"} tone="teal" icon="send" onPress={() => save(true)} style={styles.half} /></View>
      <Text style={styles.footnote}>لا ينشئ التطبيق أرقاماً محلية. عند فشل الترقيم يعرض رد الخادم كما هو ويمنع تكرار الإرسال التلقائي.</Text>
    </ScrollView>
  </AppScreen>;
}

const styles = StyleSheet.create({ content: { padding: 18, paddingBottom: 34 }, statusRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" }, statusText: { color: palette.muted, fontSize: 11, writingDirection: "ltr" }, linePreview: { padding: 12, borderRadius: 12, backgroundColor: "#F6F9FC", alignItems: "flex-end" }, previewTitle: { color: palette.muted, fontSize: 11, writingDirection: "rtl" }, previewValue: { marginTop: 5, color: palette.navy, fontSize: 13, fontWeight: "800", writingDirection: "rtl" }, actions: { marginTop: 22, flexDirection: "row", gap: 10 }, half: { flex: 1 }, footnote: { marginTop: 14, color: palette.muted, fontSize: 11, lineHeight: 18, textAlign: "right", writingDirection: "rtl" } });
