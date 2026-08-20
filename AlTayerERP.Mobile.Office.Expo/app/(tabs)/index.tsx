import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router } from "expo-router";
import { useEffect } from "react";
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, Metric, palette, SectionTitle, StatusPill } from "@/components/erp-ui";
import { useErpStore } from "@/lib/erp-store";

function QuickAction({ icon, label, caption, color, onPress }: { icon: keyof typeof MaterialIcons.glyphMap; label: string; caption: string; color: string; onPress: () => void }) {
  return <Pressable onPress={onPress} style={({ pressed }) => [styles.quickAction, pressed && styles.pressed]}><View style={[styles.quickIcon, { backgroundColor: `${color}16` }]}><MaterialIcons name={icon} color={color} size={24} /></View><View style={styles.quickCopy}><Text style={styles.quickLabel}>{label}</Text><Text style={styles.quickCaption}>{caption}</Text></View><MaterialIcons name="arrow-back-ios" size={16} color="#9CA7B6" /></Pressable>;
}

export default function OfficeScreen() {
  const { hydrated, signedIn, companyName, settings, payments, approvals, vouchers } = useErpStore();
  useEffect(() => { if (hydrated && !signedIn) router.replace("/login"); }, [hydrated, signedIn]);
  if (!hydrated || !signedIn) return <AppScreen><View style={styles.loading}><ActivityIndicator color={palette.teal} /></View></AppScreen>;

  const pendingApprovals = approvals.filter((item) => item.status === "PENDING" || item.status === "UNDER_REVIEW").length;
  const pendingPayments = payments.filter((item) => item.status === "PENDING_REVIEW").length;
  return (
    <AppScreen>
      <AppHeader title="مكتب الطائر" subtitle={companyName} action={<StatusPill label={settings.state === "ready" ? "متصل" : "غير متصل"} tone={settings.state === "ready" ? "success" : "error"} />} />
      <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
        <View style={styles.welcomeRow}><View><Text style={styles.welcomeTitle}>مرحباً بك</Text><Text style={styles.welcomeSub}>تابع العمليات والاعتمادات من مكان واحد.</Text></View><View style={styles.avatar}><Text style={styles.avatarText}>م</Text></View></View>

        <View style={styles.metrics}><Metric icon="fact-check" label="بانتظار الاعتماد" value={String(pendingApprovals)} tone="violet" /><Metric icon="receipt-long" label="طلبات الصرف" value={String(pendingPayments)} tone="teal" /><Metric icon="account-balance-wallet" label="سندات اليوم" value={String(vouchers.length)} tone="navy" /></View>

        <SectionTitle title="إجراءات سريعة" />
        <View style={styles.quickGrid}>
          <QuickAction icon="add-card" label="طلب صرف جديد" caption="حفظ مسودة أو إرسال" color={palette.teal} onPress={() => router.push("/payment-request")} />
          <QuickAction icon="request-quote" label="سند قبض جديد" caption="مرجع وملاحظات واعتماد" color={palette.navy} onPress={() => router.push("/receipt-voucher")} />
        </View>

        <SectionTitle title="الحالة التشغيلية" action="التشخيص" onPress={() => router.push("/(tabs)/diagnostics")} />
        <Card>
          <View style={styles.healthRow}><StatusPill label={settings.state === "ready" ? "API جاهز" : "يتطلب متابعة"} tone={settings.state === "ready" ? "success" : "warning"} /><View style={styles.healthCopy}><Text style={styles.healthTitle}>فحص الاتصال والخادم</Text><Text style={styles.healthDetail}>واجهة الجوال تستخدم HTTP إلى API فقط، ثم يتعامل API مع MySQL.</Text></View><MaterialIcons name="cloud-done" size={25} color={settings.state === "ready" ? palette.success : palette.warning} /></View>
          {!settings.numberingReady ? <View style={styles.warningBox}><MaterialIcons name="warning-amber" color={palette.warning} size={19} /><Text style={styles.warningText}>يلزم التحقق من إعداد ترقيم <Text style={styles.code}>PAYMENT_REQUEST</Text> قبل إرسال طلبات الصرف الفعلية.</Text></View> : null}
        </Card>
      </ScrollView>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  content: { padding: 18, paddingBottom: 30 },
  loading: { flex: 1, alignItems: "center", justifyContent: "center" },
  welcomeRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between", marginBottom: 22 },
  welcomeTitle: { color: palette.text, fontSize: 24, fontWeight: "900", textAlign: "right", writingDirection: "rtl" },
  welcomeSub: { marginTop: 4, color: palette.muted, fontSize: 13, textAlign: "right", writingDirection: "rtl" },
  avatar: { width: 46, height: 46, borderRadius: 16, backgroundColor: palette.navy, alignItems: "center", justifyContent: "center" },
  avatarText: { color: "#FFFFFF", fontSize: 18, fontWeight: "900" },
  metrics: { flexDirection: "row", justifyContent: "space-between" },
  quickGrid: { gap: 10 },
  quickAction: { minHeight: 75, padding: 13, borderRadius: 17, backgroundColor: "#FFFFFF", borderWidth: 1, borderColor: palette.line, flexDirection: "row", alignItems: "center", gap: 12 },
  quickIcon: { width: 46, height: 46, borderRadius: 14, alignItems: "center", justifyContent: "center" },
  quickCopy: { flex: 1, alignItems: "flex-end" },
  quickLabel: { color: palette.text, fontSize: 15, fontWeight: "900", writingDirection: "rtl" },
  quickCaption: { color: palette.muted, fontSize: 11, marginTop: 4, writingDirection: "rtl" },
  healthRow: { flexDirection: "row", alignItems: "center", gap: 10 },
  healthCopy: { flex: 1, alignItems: "flex-end" },
  healthTitle: { color: palette.text, fontWeight: "900", fontSize: 14, writingDirection: "rtl" },
  healthDetail: { marginTop: 4, color: palette.muted, fontSize: 11, lineHeight: 17, textAlign: "right", writingDirection: "rtl" },
  warningBox: { marginTop: 15, padding: 11, backgroundColor: "#FFF8E8", borderRadius: 12, flexDirection: "row", gap: 8, alignItems: "flex-start" },
  warningText: { flex: 1, color: "#785100", fontSize: 12, lineHeight: 18, textAlign: "right", writingDirection: "rtl" },
  code: { fontWeight: "900", writingDirection: "ltr" },
  pressed: { opacity: 0.75, transform: [{ scale: 0.99 }] },
});
