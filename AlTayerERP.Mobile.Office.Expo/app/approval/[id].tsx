import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router, useLocalSearchParams } from "expo-router";
import { useState } from "react";
import { Alert, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, Field, palette, PrimaryButton, SectionTitle, StatusPill } from "@/components/erp-ui";
import { safeApiMessage } from "@/lib/altayer-api";
import { useErpStore } from "@/lib/erp-store";

export default function ApprovalDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { approvals, updateApproval, busy } = useErpStore();
  const [note, setNote] = useState("");
  const request = approvals.find((item) => item.id === id);
  if (!request) return <AppScreen><AppHeader title="طلب غير موجود" back onBack={() => router.back()} /><View style={styles.notFound}><Text style={styles.notFoundText}>تعذر العثور على طلب الاعتماد.</Text></View></AppScreen>;
  const resolve = async (status: "UNDER_REVIEW" | "APPROVED" | "REJECTED" | "RETURNED") => {
    if ((status === "REJECTED" || status === "RETURNED") && !note.trim()) { Alert.alert("سبب القرار مطلوب", "أدخل سبب الرفض أو الإعادة قبل الحفظ."); return; }
    try {
      await updateApproval(request.id, status, note.trim() || undefined);
      Alert.alert("تم تحديث الطلب", "تمت مزامنة القرار مع خادم AlTayerERP.", [{ text: "عودة للقائمة", onPress: () => router.back() }]);
    } catch (error) {
      Alert.alert("تعذر تحديث الطلب", safeApiMessage(error, "تعذر تسجيل قرار الاعتماد. أعد تحميل القائمة ثم حاول من جديد."));
    }
  };
  const tone = request.status === "APPROVED" ? "success" : request.status === "REJECTED" ? "error" : request.status === "PENDING" ? "warning" : "info";
  const canDecide = request.status === "PENDING" || request.status === "UNDER_REVIEW";
  return <AppScreen><AppHeader title="تفاصيل الاعتماد" subtitle={request.reference} back onBack={() => router.back()} />
    <ScrollView contentContainerStyle={styles.content}><Card><View style={styles.titleRow}><StatusPill label={request.status} tone={tone} /><View style={styles.copy}><Text style={styles.title}>{request.title}</Text><Text style={styles.requester}>مقدم الطلب: {request.requester}</Text></View><View style={styles.icon}><MaterialIcons name="fact-check" size={22} color={palette.violet} /></View></View><View style={styles.amountBox}><Text style={styles.amountLabel}>المبلغ المطلوب</Text><Text style={styles.amount}>{request.amount.toLocaleString("en-US")} <Text style={styles.currency}>YER</Text></Text></View></Card>
      <SectionTitle title="سبب الطلب" /><Card><Text style={styles.reason}>{request.reason}</Text></Card>
      <SectionTitle title="قرار المراجع" caption="يُلزم سبب عند الرفض أو الإعادة." /><Card><Field label="ملاحظات القرار" value={note} onChangeText={setNote} multiline placeholder="أدخل سبب القرار أو ملاحظات المراجعة" /></Card>
      <View style={styles.firstAction}><PrimaryButton disabled={busy || !canDecide} label={busy ? "جارٍ الحفظ…" : "بدء المراجعة"} tone="outline" icon="visibility" onPress={() => void resolve("UNDER_REVIEW")} /></View><View style={styles.actions}><PrimaryButton disabled={busy || !canDecide} label="إعادة للمرسل" tone="outline" icon="undo" onPress={() => void resolve("RETURNED")} style={styles.half} /><PrimaryButton disabled={busy || !canDecide} label="رفض" tone="danger" icon="close" onPress={() => void resolve("REJECTED")} style={styles.half} /></View><PrimaryButton disabled={busy || !canDecide} label="اعتماد الطلب" tone="teal" icon="check" onPress={() => void resolve("APPROVED")} style={styles.approve} />
    </ScrollView>
  </AppScreen>;
}

const styles = StyleSheet.create({ content: { padding: 18, paddingBottom: 34 }, titleRow: { flexDirection: "row", alignItems: "center", gap: 10 }, copy: { flex: 1, alignItems: "flex-end" }, icon: { width: 42, height: 42, borderRadius: 13, backgroundColor: "#F0ECFF", alignItems: "center", justifyContent: "center" }, title: { color: palette.text, fontSize: 16, fontWeight: "900", writingDirection: "rtl" }, requester: { color: palette.muted, fontSize: 11, marginTop: 4, writingDirection: "rtl" }, amountBox: { marginTop: 17, paddingTop: 14, borderTopWidth: 1, borderTopColor: "#EDF0F5", alignItems: "flex-end" }, amountLabel: { color: palette.muted, fontSize: 11, writingDirection: "rtl" }, amount: { marginTop: 4, color: palette.navy, fontWeight: "900", fontSize: 23 }, currency: { fontSize: 12 }, reason: { color: palette.text, lineHeight: 22, fontSize: 14, textAlign: "right", writingDirection: "rtl" }, firstAction: { marginTop: 20 }, actions: { marginTop: 10, flexDirection: "row", gap: 10 }, half: { flex: 1 }, approve: { marginTop: 10 }, notFound: { flex: 1, alignItems: "center", justifyContent: "center" }, notFoundText: { color: palette.muted, writingDirection: "rtl" } });
