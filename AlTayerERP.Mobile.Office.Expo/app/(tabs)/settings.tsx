import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { router } from "expo-router";
import { Alert, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { AppHeader, AppScreen, Card, palette, PrimaryButton, SectionTitle, StatusPill } from "@/components/erp-ui";
import { useErpStore } from "@/lib/erp-store";

function SettingRow({ icon, title, detail, onPress }: { icon: keyof typeof MaterialIcons.glyphMap; title: string; detail: string; onPress?: () => void }) { return <Pressable disabled={!onPress} onPress={onPress} style={({ pressed }) => [styles.row, pressed && onPress && styles.pressed]}><MaterialIcons name="chevron-left" size={21} color="#9BA6B5" /><View style={styles.copy}><Text style={styles.rowTitle}>{title}</Text><Text style={styles.rowDetail}>{detail}</Text></View><View style={styles.icon}><MaterialIcons name={icon} size={20} color={palette.teal} /></View></Pressable>; }

export default function SettingsScreen() {
  const { companyName, settings, signOut } = useErpStore();
  const logout = () => Alert.alert("تسجيل الخروج", "سيتم إغلاق الجلسة المحلية فقط.", [{ text: "إلغاء", style: "cancel" }, { text: "تسجيل الخروج", style: "destructive", onPress: () => { signOut(); router.replace("/login"); } }]);
  return <AppScreen><AppHeader title="الإعدادات" subtitle="الجلسة والاتصال والشفافية التشغيلية" />
    <ScrollView contentContainerStyle={styles.content}><Card><View style={styles.profile}><View style={styles.profileCopy}><Text style={styles.company}>{companyName}</Text><Text style={styles.role}>مشرف المكتب · جلسة محلية</Text></View><View style={styles.avatar}><Text style={styles.avatarText}>م</Text></View></View></Card>
      <SectionTitle title="التشغيل" /><Card style={styles.group}><SettingRow icon="settings-ethernet" title="إعداد الاتصال" detail={`${settings.mode} · ${settings.host}:${settings.port}`} onPress={() => router.push("/connection")} /><View style={styles.divider} /><SettingRow icon="health-and-safety" title="مركز التشخيص" detail="فحص الصحة والترقيم ورسائل API" onPress={() => router.push("/(tabs)/diagnostics")} /></Card>
      <SectionTitle title="الحوكمة" /><Card style={styles.governance}><View style={styles.govRow}><StatusPill label="بدون اتصال MySQL مباشر" tone="success" /><Text style={styles.govText}>الجوال يتعامل مع API فقط</Text></View><View style={styles.govRow}><StatusPill label="لا أسرار في الواجهة" tone="success" /><Text style={styles.govText}>بيانات اتصال الخادم ليست Connection String</Text></View><View style={styles.govRow}><StatusPill label="master محمي" tone="info" /><Text style={styles.govText}>لا دمج تلقائي ولا تعديل قاعدة إنتاجية</Text></View></Card>
      <PrimaryButton label="تسجيل الخروج" tone="outline" icon="logout" onPress={logout} style={styles.logout} />
      <Text style={styles.version}>AlTayerERP Mobile Office · واجهة استرشادية محلية · v1.0</Text>
    </ScrollView>
  </AppScreen>;
}

const styles = StyleSheet.create({ content: { padding: 18, paddingBottom: 34 }, profile: { flexDirection: "row", alignItems: "center", gap: 12 }, profileCopy: { flex: 1, alignItems: "flex-end" }, company: { color: palette.text, fontSize: 16, fontWeight: "900", textAlign: "right", writingDirection: "rtl" }, role: { color: palette.muted, fontSize: 11, marginTop: 4, writingDirection: "rtl" }, avatar: { width: 45, height: 45, borderRadius: 15, backgroundColor: palette.navy, alignItems: "center", justifyContent: "center" }, avatarText: { color: "#FFFFFF", fontSize: 18, fontWeight: "900" }, group: { paddingVertical: 3 }, row: { paddingVertical: 14, flexDirection: "row", alignItems: "center", gap: 11 }, copy: { flex: 1, alignItems: "flex-end" }, rowTitle: { color: palette.text, fontSize: 14, fontWeight: "800", writingDirection: "rtl" }, rowDetail: { marginTop: 4, color: palette.muted, fontSize: 11, textAlign: "right", writingDirection: "rtl" }, icon: { width: 38, height: 38, borderRadius: 12, backgroundColor: "#E6F8FB", alignItems: "center", justifyContent: "center" }, divider: { height: 1, backgroundColor: "#EDF0F5" }, governance: { gap: 13 }, govRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between", gap: 10 }, govText: { flex: 1, color: palette.text, fontSize: 12, textAlign: "right", writingDirection: "rtl" }, logout: { marginTop: 24 }, version: { marginTop: 18, color: palette.muted, fontSize: 11, textAlign: "center", writingDirection: "rtl" }, pressed: { opacity: 0.75, transform: [{ scale: 0.99 }] } });
