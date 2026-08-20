import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router } from "expo-router";
import { Alert, FlatList, Pressable, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, palette, PrimaryButton, StatusPill } from "@/components/erp-ui";
import { safeApiMessage } from "@/lib/altayer-api";
import type { PaymentRequest } from "@/lib/erp-types";
import { useErpStore } from "@/lib/erp-store";

const labels: Record<PaymentRequest["status"], string> = { DRAFT: "مسودة", PENDING_REVIEW: "بانتظار المراجعة", PENDING_APPROVAL: "بانتظار الاعتماد", APPROVED: "معتمد", REJECTED: "مرفوض", RETURNED: "معاد" };
const tones: Record<PaymentRequest["status"], "success" | "warning" | "error" | "info"> = { DRAFT: "info", PENDING_REVIEW: "warning", PENDING_APPROVAL: "warning", APPROVED: "success", REJECTED: "error", RETURNED: "info" };

function PaymentCard({ item }: { item: PaymentRequest }) {
  return <Card style={styles.card}><View style={styles.cardHeader}><StatusPill label={labels[item.status]} tone={tones[item.status]} /><View style={styles.cardTitleWrap}><Text style={styles.cardTitle}>{item.beneficiary}</Text><Text style={styles.cardNo}>{item.requestNo}</Text></View><View style={styles.cardIcon}><MaterialIcons name="receipt-long" size={21} color={palette.teal} /></View></View><Text style={styles.cardDescription}>{item.description}</Text><View style={styles.cardFooter}><Text style={styles.date}>{item.date} · {item.lineCount} أسطر</Text><Text style={styles.amount}>{item.amount.toLocaleString("en-US")} <Text style={styles.currency}>YER</Text></Text></View></Card>;
}

export default function PaymentsScreen() {
  const { payments, settings, refreshPayments, busy } = useErpStore();
  const refresh = () => void refreshPayments().catch((error) => Alert.alert("تعذر تحديث القائمة", safeApiMessage(error, "تعذر تحميل طلبات الصرف من الخادم.")));
  return <AppScreen><AppHeader title="طلبات الصرف" subtitle="الحفظ والإرسال ومتابعة الحالة" action={<View style={styles.headerActions}><Pressable onPress={refresh} disabled={busy} style={({ pressed }) => [styles.add, (pressed || busy) && styles.pressed]}><MaterialIcons name="sync" color="#FFFFFF" size={20} /></Pressable><Pressable onPress={() => router.push("/payment-request")} style={({ pressed }) => [styles.add, pressed && styles.pressed]}><MaterialIcons name="add" color="#FFFFFF" size={22} /></Pressable></View>} />
    <FlatList data={payments} keyExtractor={(item) => item.id} contentContainerStyle={styles.list} renderItem={({ item }) => <PaymentCard item={item} />} ListHeaderComponent={<><PrimaryButton disabled={busy} label={busy ? "جارٍ تحديث الطلبات…" : "تحديث من الخادم"} tone="outline" icon="sync" onPress={refresh} style={styles.refreshButton} /><View style={styles.infoBox}><MaterialIcons name="info-outline" size={18} color={palette.violet} /><Text style={styles.infoText}>يتم توليد رقم الطلب مركزياً في API. عند ظهور خطأ الترقيم، راجع مركز التشخيص بدلاً من تكرار الإرسال.</Text></View>{!settings.numberingReady ? <PrimaryButton label="فتح فحص الترقيم" tone="outline" icon="health-and-safety" onPress={() => router.push("/(tabs)/diagnostics")} style={styles.diagnosticButton} /> : null}</>} ListEmptyComponent={<View style={styles.empty}><Text style={styles.emptyTitle}>لا توجد طلبات صرف بعد</Text><PrimaryButton label="إنشاء طلب صرف" tone="teal" onPress={() => router.push("/payment-request")} /></View>} />
  </AppScreen>;
}

const styles = StyleSheet.create({
  list: { padding: 18, paddingBottom: 30, gap: 11 },
  headerActions: { flexDirection: "row", gap: 7 },
  add: { width: 38, height: 38, borderRadius: 19, backgroundColor: "#FFFFFF22", justifyContent: "center", alignItems: "center" },
  refreshButton: { marginBottom: 10 },
  infoBox: { padding: 12, borderRadius: 14, backgroundColor: "#F0ECFF", flexDirection: "row", alignItems: "flex-start", gap: 9, marginBottom: 10 },
  infoText: { flex: 1, color: "#463A87", textAlign: "right", writingDirection: "rtl", fontSize: 12, lineHeight: 18 },
  diagnosticButton: { marginBottom: 4 },
  card: { marginBottom: 1 },
  cardHeader: { flexDirection: "row", alignItems: "center", gap: 10 },
  cardIcon: { width: 39, height: 39, borderRadius: 12, backgroundColor: "#E6F8FB", alignItems: "center", justifyContent: "center" },
  cardTitleWrap: { flex: 1, alignItems: "flex-end" },
  cardTitle: { color: palette.text, fontSize: 15, fontWeight: "900", writingDirection: "rtl" },
  cardNo: { marginTop: 3, color: palette.muted, fontSize: 11 },
  cardDescription: { marginTop: 14, color: palette.muted, fontSize: 12, writingDirection: "rtl", textAlign: "right" },
  cardFooter: { marginTop: 15, paddingTop: 12, borderTopWidth: 1, borderTopColor: "#EDF0F5", flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  date: { color: palette.muted, fontSize: 11, writingDirection: "rtl" },
  amount: { color: palette.navy, fontWeight: "900", fontSize: 17 },
  currency: { fontSize: 11 },
  empty: { paddingVertical: 44, alignItems: "center", gap: 14 },
  emptyTitle: { color: palette.muted, writingDirection: "rtl" },
  pressed: { opacity: 0.75, transform: [{ scale: 0.97 }] },
});
