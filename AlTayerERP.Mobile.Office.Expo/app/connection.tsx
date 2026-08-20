import { router } from "expo-router";
import { useState } from "react";
import { Alert, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, Field, palette, PrimaryButton, SectionTitle, StatusPill } from "@/components/erp-ui";
import { safeApiMessage } from "@/lib/altayer-api";
import { useErpStore } from "@/lib/erp-store";
import { isConnectionConfigurationValid } from "@/lib/erp-rules";

const modes = [{ value: "AUTO" as const, title: "تلقائي", detail: "استخدم العنوان المحفوظ بعد فحصه" }, { value: "WIFI" as const, title: "Wi‑Fi", detail: "عنوان جهاز الخادم على الشبكة المحلية" }, { value: "USB" as const, title: "USB", detail: "127.0.0.1 عبر إعداد DEBUG" }];

export default function ConnectionScreen() {
  const { settings, saveSettings, testConnection, busy } = useErpStore();
  const [mode, setMode] = useState(settings.mode); const [host, setHost] = useState(settings.host); const [port, setPort] = useState(settings.port);
  const save = () => {
    const valid = isConnectionConfigurationValid(host, port);
    saveSettings({ mode, host: host.trim(), port: port.trim(), state: valid ? "warning" : "offline" });
    Alert.alert(valid ? "تم حفظ الإعداد" : "الإعداد غير مكتمل", valid ? "يمكنك الآن اختبار الاتصال من هذه الشاشة أو مركز التشخيص." : "أدخل عنوان خادم ومنفذاً رقمياً بين 10 و99999.");
  };
  const test = () => {
    if (!isConnectionConfigurationValid(host, port)) {
      Alert.alert("الإعداد غير مكتمل", "أدخل عنوان خادم ومنفذاً رقمياً بين 10 و99999 قبل الاختبار.");
      return;
    }
    saveSettings({ mode, host: host.trim(), port: port.trim(), state: "warning" });
    void testConnection()
      .then(() => Alert.alert("نجح اختبار الاتصال", "استجاب مسار صحة الخادم. راجع مركز التشخيص لحالة API وقاعدة البيانات."))
      .catch((error) => Alert.alert("فشل اختبار الاتصال", safeApiMessage(error, "تعذر الوصول إلى خادم AlTayerERP. تحقق من العنوان والمنفذ وجدار الحماية.")));
  };
  return <AppScreen><AppHeader title="إعداد الاتصال" subtitle="Mobile → HTTP → API → MySQL" back onBack={() => router.back()} />
    <ScrollView contentContainerStyle={styles.content}><Card><View style={styles.status}><StatusPill label={settings.state === "ready" ? "آخر فحص ناجح" : "غير مؤكد"} tone={settings.state === "ready" ? "success" : "warning"} /><Text style={styles.statusText}>المنفذ الافتراضي: 5021</Text></View></Card>
      <SectionTitle title="وضع الاتصال" /><View style={styles.modes}>{modes.map((item) => { const active = item.value === mode; return <Pressable key={item.value} onPress={() => setMode(item.value)} style={({ pressed }) => [styles.mode, active && styles.modeActive, pressed && styles.pressed]}><View style={styles.modeCopy}><Text style={[styles.modeTitle, active && styles.modeTitleActive]}>{item.title}</Text><Text style={styles.modeDetail}>{item.detail}</Text></View><View style={[styles.radio, active && styles.radioActive]}>{active ? <View style={styles.radioInner} /> : null}</View></Pressable>; })}</View>
      <SectionTitle title="عنوان الخادم" caption="لا تحفظ كلمات مرور أو Connection Strings في هذا الحقل." /><Card><Field label="العنوان أو IP" value={host} onChangeText={setHost} placeholder="مثال: 172.16.5.82" /><Field label="المنفذ" value={port} onChangeText={setPort} keyboardType="numeric" placeholder="5021" /></Card>
      <View style={styles.actions}><PrimaryButton disabled={busy} label="حفظ الإعداد" tone="outline" icon="save" onPress={save} style={styles.half} /><PrimaryButton disabled={busy} label={busy ? "جارٍ الاختبار…" : "اختبار الاتصال"} tone="teal" icon="sync" onPress={test} style={styles.half} /></View>
      <Text style={styles.hint}>عند فشل `/api/health` لا تعرض الواجهة قائمة شركات فارغة؛ افتح مركز التشخيص لمعرفة هل السبب الشبكة أو API أو قاعدة البيانات.</Text>
    </ScrollView>
  </AppScreen>;
}

const styles = StyleSheet.create({ content: { padding: 18, paddingBottom: 34 }, status: { flexDirection: "row", justifyContent: "space-between", alignItems: "center" }, statusText: { color: palette.muted, fontSize: 11, writingDirection: "rtl" }, modes: { gap: 9 }, mode: { minHeight: 67, padding: 13, borderRadius: 15, borderWidth: 1, borderColor: palette.line, backgroundColor: "#FFFFFF", flexDirection: "row", alignItems: "center", gap: 11 }, modeActive: { borderColor: palette.teal, backgroundColor: "#F2FCFD" }, modeCopy: { flex: 1, alignItems: "flex-end" }, modeTitle: { color: palette.text, fontSize: 14, fontWeight: "800", writingDirection: "rtl" }, modeTitleActive: { color: palette.teal }, modeDetail: { marginTop: 4, color: palette.muted, fontSize: 11, textAlign: "right", writingDirection: "rtl" }, radio: { width: 21, height: 21, borderRadius: 12, borderWidth: 2, borderColor: "#A7B2C1", alignItems: "center", justifyContent: "center" }, radioActive: { borderColor: palette.teal }, radioInner: { width: 11, height: 11, borderRadius: 6, backgroundColor: palette.teal }, actions: { marginTop: 22, flexDirection: "row", gap: 10 }, half: { flex: 1 }, hint: { marginTop: 15, color: palette.muted, fontSize: 11, textAlign: "right", writingDirection: "rtl", lineHeight: 18 }, pressed: { opacity: 0.75, transform: [{ scale: 0.99 }] } });
