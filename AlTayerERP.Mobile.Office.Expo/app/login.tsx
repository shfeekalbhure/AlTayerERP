import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router } from "expo-router";
import { useEffect, useState } from "react";
import { ActivityIndicator, Alert, Image, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppScreen, Card, Field, palette, PrimaryButton, StatusPill } from "@/components/erp-ui";
import { useErpStore } from "@/lib/erp-store";
import type { CompanyOption } from "@/lib/altayer-api";

const appIcon = require("../assets/images/icon.png");

export default function LoginScreen() {
  const { hydrated, signedIn, settings, signIn, companies, loadCompanies, busy } = useErpStore();
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [selectedCompany, setSelectedCompany] = useState<CompanyOption | null>(null);

  useEffect(() => {
    if (hydrated && signedIn) router.replace("/");
  }, [hydrated, signedIn]);

  useEffect(() => {
    if (!hydrated) return;
    void loadCompanies().catch(() => undefined);
  }, [hydrated, loadCompanies]);

  useEffect(() => {
    if (!selectedCompany && companies[0]) setSelectedCompany(companies[0]);
  }, [companies, selectedCompany]);

  if (!hydrated) {
    return <AppScreen><View style={styles.loading}><ActivityIndicator color={palette.teal} size="large" /><Text style={styles.loadingText}>يتم تجهيز جلسة المكتب…</Text></View></AppScreen>;
  }

  const ready = settings.state === "ready";
  return (
    <AppScreen>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.brandBlock}>
          <View style={styles.logoWrap}>
            <Image source={appIcon} style={styles.logo} />
          </View>
          <Text style={styles.title}>AlTayerERP</Text>
          <Text style={styles.subtitle}>مكتبك المالي المتنقل</Text>
        </View>

        <Card>
          <View style={styles.connectionRow}>
            <StatusPill label={ready ? "الخادم جاهز" : "يلزم فحص الاتصال"} tone={ready ? "success" : "warning"} />
            <View style={styles.connectionCopy}>
              <Text style={styles.connectionTitle}>اتصال API</Text>
              <Text style={styles.connectionDetail}>{settings.host}:{settings.port} · آخر فحص {settings.lastCheckedAt}</Text>
            </View>
            <MaterialIcons name={ready ? "cloud-done" : "cloud-off"} size={24} color={ready ? palette.success : palette.warning} />
          </View>
          {!ready ? <Pressable onPress={() => router.push("/connection")} style={({ pressed }) => [styles.linkRow, pressed && styles.pressed]}><Text style={styles.linkText}>فتح إعداد الاتصال</Text><MaterialIcons name="arrow-back" size={18} color={palette.teal} /></Pressable> : null}
        </Card>

        <Text style={styles.formHeading}>تسجيل الدخول</Text>
        <Field label="اسم المستخدم" value={userName} onChangeText={setUserName} placeholder="أدخل اسم المستخدم" />
        <Field label="كلمة المرور" value={password} onChangeText={setPassword} placeholder="أدخل كلمة المرور" />

        <Text style={styles.companyLabel}>الشركة</Text>
        <View style={styles.companyList}>
          {companies.map((company) => {
            const active = company.id === selectedCompany?.id;
            return <Pressable key={company.id} onPress={() => setSelectedCompany(company)} style={({ pressed }) => [styles.companyOption, active && styles.companyOptionActive, pressed && styles.pressed]}><MaterialIcons name={active ? "radio-button-checked" : "radio-button-unchecked"} size={20} color={active ? palette.teal : "#97A2B2"} /><Text style={[styles.companyText, active && styles.companyTextActive]}>{company.name}</Text></Pressable>;
          })}
          {!companies.length ? <Pressable onPress={() => void loadCompanies().catch((error) => Alert.alert("تعذر تحميل الشركات", error instanceof Error ? error.message : "تحقق من الاتصال ثم أعد المحاولة."))} style={({ pressed }) => [styles.reloadCompanies, pressed && styles.pressed]}><MaterialIcons name="refresh" size={18} color={palette.teal} /><Text style={styles.reloadLabel}>{busy ? "يتم التحميل…" : "إعادة تحميل الشركات من الخادم"}</Text></Pressable> : null}
        </View>

        <PrimaryButton disabled={busy} label={busy ? "يتم التحقق…" : "دخول إلى المكتب"} icon="login" tone="navy" onPress={() => { void (async () => { if (!selectedCompany || !userName.trim() || !password) { Alert.alert("بيانات الدخول غير مكتملة", "اختر الشركة وأدخل اسم المستخدم وكلمة المرور."); return; } try { await signIn({ company: selectedCompany, loginName: userName.trim(), password }); router.replace("/"); } catch (error) { Alert.alert("تعذر تسجيل الدخول", error instanceof Error ? error.message : "تحقق من البيانات والاتصال ثم أعد المحاولة."); } })(); }} />
        <Text style={styles.disclaimer}>يُرسل التطبيق بيانات الدخول إلى API عبر الشبكة فقط، ويحفظ رمز الجلسة في التخزين الآمن للجهاز. لا يتصل مباشرة بقاعدة البيانات.</Text>
      </ScrollView>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  content: { padding: 22, paddingTop: 28, paddingBottom: 40 },
  loading: { flex: 1, justifyContent: "center", alignItems: "center", gap: 14 },
  loadingText: { color: palette.muted, writingDirection: "rtl" },
  brandBlock: { alignItems: "center", marginBottom: 28 },
  logoWrap: { width: 82, height: 82, overflow: "hidden", borderRadius: 24, shadowColor: palette.navy, shadowOpacity: 0.18, shadowRadius: 15, elevation: 5 },
  logo: { width: "100%", height: "100%" },
  title: { marginTop: 14, color: palette.navy, fontSize: 28, fontWeight: "900" },
  subtitle: { marginTop: 4, color: palette.muted, fontSize: 14, writingDirection: "rtl" },
  connectionRow: { flexDirection: "row", alignItems: "center", gap: 10 },
  connectionCopy: { flex: 1, alignItems: "flex-end" },
  connectionTitle: { color: palette.text, fontSize: 14, fontWeight: "800", writingDirection: "rtl" },
  connectionDetail: { marginTop: 3, color: palette.muted, fontSize: 11, writingDirection: "rtl" },
  linkRow: { marginTop: 14, paddingTop: 13, borderTopWidth: 1, borderTopColor: palette.line, flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  linkText: { color: palette.teal, fontWeight: "800", writingDirection: "rtl" },
  formHeading: { marginTop: 27, marginBottom: 16, color: palette.text, fontSize: 19, fontWeight: "900", textAlign: "right", writingDirection: "rtl" },
  companyLabel: { color: palette.text, fontSize: 13, fontWeight: "700", textAlign: "right", writingDirection: "rtl", marginBottom: 7 },
  companyList: { gap: 8, marginBottom: 22 },
  companyOption: { minHeight: 50, paddingHorizontal: 13, borderRadius: 12, borderWidth: 1, borderColor: palette.line, backgroundColor: "#FFFFFF", flexDirection: "row", alignItems: "center", justifyContent: "flex-end", gap: 10 },
  companyOptionActive: { borderColor: palette.teal, backgroundColor: "#F2FCFD" },
  companyText: { color: palette.text, fontSize: 14, writingDirection: "rtl" },
  companyTextActive: { fontWeight: "800" },
  reloadCompanies: { minHeight: 50, borderRadius: 12, borderWidth: 1, borderStyle: "dashed", borderColor: palette.teal, backgroundColor: "#F2FCFD", justifyContent: "center", alignItems: "center", flexDirection: "row", gap: 7 },
  reloadLabel: { color: palette.teal, fontWeight: "800", writingDirection: "rtl" },
  disclaimer: { marginTop: 16, color: palette.muted, fontSize: 11, lineHeight: 18, textAlign: "center", writingDirection: "rtl" },
  pressed: { opacity: 0.75, transform: [{ scale: 0.99 }] },
});
