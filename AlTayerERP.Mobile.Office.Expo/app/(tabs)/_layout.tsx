import MaterialIcons from "@expo/vector-icons/MaterialIcons";
import { Tabs } from "expo-router";
import { Platform } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { palette } from "@/components/erp-ui";

const tabIcons: Record<string, keyof typeof MaterialIcons.glyphMap> = {
  index: "space-dashboard",
  payments: "receipt-long",
  approvals: "fact-check",
  diagnostics: "health-and-safety",
  settings: "settings",
};

export default function TabLayout() {
  const insets = useSafeAreaInsets();
  const bottomInset = Platform.OS === "web" ? 10 : Math.max(insets.bottom, 8);
  return (
    <Tabs screenOptions={({ route }) => ({
      headerShown: false,
      tabBarActiveTintColor: palette.teal,
      tabBarInactiveTintColor: "#8995A6",
      tabBarStyle: { backgroundColor: "#FFFFFF", borderTopColor: "#DFE5EE", height: 60 + bottomInset, paddingBottom: bottomInset, paddingTop: 6 },
      tabBarLabelStyle: { fontSize: 10, fontWeight: "700" },
      tabBarIcon: ({ color, focused }) => <MaterialIcons name={tabIcons[route.name]} size={focused ? 25 : 23} color={color} />,
    })}>
      <Tabs.Screen name="index" options={{ title: "المكتب" }} />
      <Tabs.Screen name="payments" options={{ title: "الصرف" }} />
      <Tabs.Screen name="approvals" options={{ title: "الاعتماد" }} />
      <Tabs.Screen name="diagnostics" options={{ title: "التشخيص" }} />
      <Tabs.Screen name="settings" options={{ title: "الإعدادات" }} />
    </Tabs>
  );
}
