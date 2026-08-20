import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { PropsWithChildren } from "react";
import { Pressable, StyleProp, StyleSheet, Text, TextInput, TextStyle, View, ViewStyle } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { ScreenContainer } from "@/components/screen-container";

export const palette = {
  navy: "#173A5E",
  teal: "#0E859B",
  violet: "#5B2BE0",
  canvas: "#F5F7FB",
  surface: "#FFFFFF",
  text: "#172B4D",
  muted: "#6E7787",
  line: "#DFE5EE",
  success: "#168449",
  warning: "#C77700",
  error: "#C1291F",
  paleTeal: "#E6F8FB",
  paleWarning: "#FFF6DF",
  paleError: "#FFF0EE",
};

export function AppScreen({ children, style }: PropsWithChildren<{ style?: StyleProp<ViewStyle> }>) {
  return (
    <ScreenContainer containerClassName="bg-background" style={style}>
      <SafeAreaView style={styles.screen} edges={["top", "left", "right"]}>{children}</SafeAreaView>
    </ScreenContainer>
  );
}

export function AppHeader({ title, subtitle, back, onBack, action }: {
  title: string;
  subtitle?: string;
  back?: boolean;
  onBack?: () => void;
  action?: React.ReactNode;
}) {
  return (
    <View style={styles.header}>
      <View style={styles.headerAction}>{action}</View>
      <View style={styles.headerCopy}>
        <Text style={styles.headerTitle}>{title}</Text>
        {subtitle ? <Text style={styles.headerSubtitle}>{subtitle}</Text> : null}
      </View>
      {back ? (
        <Pressable onPress={onBack} hitSlop={12} style={({ pressed }) => [styles.backButton, pressed && styles.pressed]}>
          <MaterialIcons color="#FFFFFF" name="arrow-forward" size={24} />
        </Pressable>
      ) : <View style={styles.backPlaceholder} />}
    </View>
  );
}

export function Card({ children, style }: PropsWithChildren<{ style?: StyleProp<ViewStyle> }>) {
  return <View style={[styles.card, style]}>{children}</View>;
}

export function SectionTitle({ title, caption, action, onPress }: { title: string; caption?: string; action?: string; onPress?: () => void }) {
  return (
    <View style={styles.sectionRow}>
      {action ? <Pressable onPress={onPress} style={({ pressed }) => [styles.textAction, pressed && styles.pressed]}><Text style={styles.textActionLabel}>{action}</Text></Pressable> : <View />}
      <View>
        <Text style={styles.sectionTitle}>{title}</Text>
        {caption ? <Text style={styles.sectionCaption}>{caption}</Text> : null}
      </View>
    </View>
  );
}

export function StatusPill({ label, tone = "info" }: { label: string; tone?: "success" | "warning" | "error" | "info" }) {
  const colors = tone === "success" ? [palette.success, "#E8F7EE"] : tone === "warning" ? [palette.warning, palette.paleWarning] : tone === "error" ? [palette.error, palette.paleError] : [palette.violet, "#F0ECFF"];
  return <View style={[styles.pill, { backgroundColor: colors[1] }]}><Text style={[styles.pillText, { color: colors[0] }]}>{label}</Text></View>;
}

export function PrimaryButton({ label, onPress, icon, tone = "navy", disabled, style }: {
  label: string; onPress: () => void; icon?: keyof typeof MaterialIcons.glyphMap; tone?: "navy" | "teal" | "outline" | "danger"; disabled?: boolean; style?: StyleProp<ViewStyle>;
}) {
  const variants = {
    navy: { backgroundColor: palette.navy, color: "#FFFFFF" },
    teal: { backgroundColor: palette.teal, color: "#FFFFFF" },
    danger: { backgroundColor: palette.error, color: "#FFFFFF" },
    outline: { backgroundColor: palette.surface, color: palette.navy, borderWidth: 1, borderColor: palette.line },
  };
  const variant = variants[tone];
  return (
    <Pressable disabled={disabled} onPress={onPress} style={({ pressed }) => [styles.primaryButton, variant, disabled && styles.disabled, pressed && !disabled && styles.pressed, style]}>
      {icon ? <MaterialIcons color={variant.color} name={icon} size={19} /> : null}
      <Text style={[styles.primaryButtonLabel, { color: variant.color }]}>{label}</Text>
    </Pressable>
  );
}

export function Field({ label, value, onChangeText, placeholder, keyboardType = "default", multiline = false }: {
  label: string; value: string; onChangeText: (value: string) => void; placeholder?: string; keyboardType?: "default" | "numeric"; multiline?: boolean;
}) {
  return (
    <View style={styles.fieldWrap}>
      <Text style={styles.fieldLabel}>{label}</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor="#9AA4B2"
        keyboardType={keyboardType}
        multiline={multiline}
        textAlign="right"
        style={[styles.field, multiline && styles.fieldMultiline]}
      />
    </View>
  );
}

export function Metric({ icon, label, value, tone = "navy" }: { icon: keyof typeof MaterialIcons.glyphMap; label: string; value: string; tone?: "navy" | "teal" | "violet" }) {
  const color = tone === "teal" ? palette.teal : tone === "violet" ? palette.violet : palette.navy;
  return (
    <View style={styles.metric}>
      <View style={[styles.metricIcon, { backgroundColor: `${color}14` }]}><MaterialIcons name={icon} size={19} color={color} /></View>
      <Text style={styles.metricValue}>{value}</Text>
      <Text style={styles.metricLabel}>{label}</Text>
    </View>
  );
}

export const sharedText: StyleProp<TextStyle> = { writingDirection: "rtl", textAlign: "right" };

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: palette.canvas },
  header: { minHeight: 78, paddingHorizontal: 20, backgroundColor: palette.navy, flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  headerCopy: { flex: 1, alignItems: "center" },
  headerTitle: { color: "#FFFFFF", fontSize: 20, fontWeight: "800", writingDirection: "rtl" },
  headerSubtitle: { marginTop: 3, color: "#C9DBEA", fontSize: 12, writingDirection: "rtl" },
  headerAction: { width: 40, alignItems: "flex-start" },
  backButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: "#FFFFFF20", alignItems: "center", justifyContent: "center" },
  backPlaceholder: { width: 40 },
  card: { backgroundColor: palette.surface, borderRadius: 18, borderWidth: 1, borderColor: palette.line, padding: 16, shadowColor: "#15395B", shadowOpacity: 0.05, shadowRadius: 10, elevation: 1 },
  sectionRow: { marginTop: 22, marginBottom: 11, flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  sectionTitle: { color: palette.text, fontSize: 18, fontWeight: "800", textAlign: "right", writingDirection: "rtl" },
  sectionCaption: { marginTop: 3, color: palette.muted, fontSize: 12, textAlign: "right", writingDirection: "rtl" },
  textAction: { paddingVertical: 5, paddingHorizontal: 2 },
  textActionLabel: { color: palette.teal, fontSize: 13, fontWeight: "800", writingDirection: "rtl" },
  pill: { alignSelf: "flex-start", borderRadius: 99, paddingVertical: 5, paddingHorizontal: 10 },
  pillText: { fontWeight: "800", fontSize: 11, writingDirection: "rtl" },
  primaryButton: { minHeight: 50, paddingHorizontal: 16, gap: 8, borderRadius: 14, flexDirection: "row", alignItems: "center", justifyContent: "center" },
  primaryButtonLabel: { fontSize: 15, fontWeight: "800", writingDirection: "rtl" },
  disabled: { opacity: 0.45 },
  pressed: { opacity: 0.78, transform: [{ scale: 0.98 }] },
  fieldWrap: { marginBottom: 14 },
  fieldLabel: { marginBottom: 7, color: palette.text, fontSize: 13, fontWeight: "700", textAlign: "right", writingDirection: "rtl" },
  field: { minHeight: 48, paddingHorizontal: 13, borderRadius: 12, borderWidth: 1, borderColor: palette.line, backgroundColor: "#FBFCFE", color: palette.text, fontSize: 15, writingDirection: "rtl" },
  fieldMultiline: { minHeight: 94, paddingTop: 12, textAlignVertical: "top" },
  metric: { width: "31.5%", borderRadius: 15, padding: 12, backgroundColor: palette.surface, borderWidth: 1, borderColor: palette.line },
  metricIcon: { width: 31, height: 31, borderRadius: 10, alignItems: "center", justifyContent: "center", alignSelf: "flex-end" },
  metricValue: { marginTop: 12, color: palette.text, fontSize: 18, fontWeight: "900", textAlign: "right" },
  metricLabel: { marginTop: 3, color: palette.muted, fontSize: 11, textAlign: "right", writingDirection: "rtl" },
});
