import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router } from "expo-router";
import { Alert, FlatList, Pressable, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, palette, StatusPill } from "@/components/erp-ui";
import { safeApiMessage } from "@/lib/altayer-api";
import type { ApprovalRequest } from "@/lib/erp-types";
import { useErpStore } from "@/lib/erp-store";

const labels = { PENDING: "بانتظار القرار", UNDER_REVIEW: "قيد المراجعة", APPROVED: "معتمد", REJECTED: "مرفوض", RETURNED: "معاد" };
const tones = { PENDING: "warning", UNDER_REVIEW: "info", APPROVED: "success", REJECTED: "error", RETURNED: "info" } as const;

function ApprovalCard({ item }: { item: ApprovalRequest }) {
  return <Pressable onPress={() => router.push(`/approval/${item.id}`)} style={({ pressed }) => pressed && styles.pressed}><Card style={styles.card}><View style={styles.row}><StatusPill label={labels[item.status]} tone={tones[item.status]} /><View style={styles.copy}><Text style={styles.title}>{item.title}</Text><Text style={styles.reference}>{item.reference} · {item.requester}</Text></View><View style={styles.icon}><MaterialIcons name={item.type === "PAYMENT_REQUEST" ? "receipt-long" : "request-quote"} size={20} color={palette.violet} /></View></View><View style={styles.footer}><Text style={styles.created}>{item.createdAt}</Text><Text style={styles.amount}>{item.amount.toLocaleString("en-US")} <Text style={styles.currency}>YER</Text></Text></View></Card></Pressable>;
}

export default function ApprovalsScreen() {
  const { approvals, refreshApprovals, busy } = useErpStore();
  const active = approvals.filter((item) => item.status === "PENDING" || item.status === "UNDER_REVIEW");
  const refresh = () => void refreshApprovals().catch((error) => Alert.alert("تعذر تحديث القائمة", safeApiMessage(error, "تعذر تحميل طلبات الاعتماد من الخادم.")));
  return <AppScreen><AppHeader title="طلبات الاعتماد" subtitle={`${active.length} طلبات تتطلب إجراء`} action={<Pressable disabled={busy} onPress={refresh} style={({ pressed }) => [styles.refresh, (pressed || busy) && styles.pressed]}><MaterialIcons name="sync" size={21} color="#FFFFFF" /></Pressable>} />
    <FlatList data={approvals} keyExtractor={(item) => item.id} contentContainerStyle={styles.list} renderItem={({ item }) => <ApprovalCard item={item} />} ListHeaderComponent={<View style={styles.note}><MaterialIcons name="fact-check" size={18} color={palette.violet} /><Text style={styles.noteText}>هذه القائمة مصدرها طلبات الاعتماد العامة، وليست قائمة طلبات الصرف وحدها.</Text></View>} ListEmptyComponent={<Text style={styles.empty}>لا توجد طلبات اعتماد حالياً.</Text>} />
  </AppScreen>;
}

const styles = StyleSheet.create({ list: { padding: 18, gap: 11, paddingBottom: 32 }, refresh: { width: 38, height: 38, borderRadius: 19, backgroundColor: "#FFFFFF22", alignItems: "center", justifyContent: "center" }, note: { padding: 12, borderRadius: 14, backgroundColor: "#F0ECFF", flexDirection: "row", gap: 9, alignItems: "flex-start", marginBottom: 1 }, noteText: { flex: 1, color: "#463A87", fontSize: 12, lineHeight: 18, textAlign: "right", writingDirection: "rtl" }, card: { marginBottom: 1 }, row: { flexDirection: "row", alignItems: "center", gap: 9 }, copy: { flex: 1, alignItems: "flex-end" }, icon: { width: 39, height: 39, borderRadius: 12, backgroundColor: "#F0ECFF", alignItems: "center", justifyContent: "center" }, title: { color: palette.text, fontSize: 15, fontWeight: "900", writingDirection: "rtl" }, reference: { color: palette.muted, fontSize: 11, marginTop: 4, writingDirection: "rtl" }, footer: { marginTop: 14, paddingTop: 12, borderTopColor: "#EDF0F5", borderTopWidth: 1, flexDirection: "row", justifyContent: "space-between" }, created: { color: palette.muted, fontSize: 11, writingDirection: "rtl" }, amount: { color: palette.navy, fontSize: 16, fontWeight: "900" }, currency: { fontSize: 10 }, empty: { color: palette.muted, textAlign: "center", paddingVertical: 45, writingDirection: "rtl" }, pressed: { opacity: 0.75, transform: [{ scale: 0.99 }] } });
