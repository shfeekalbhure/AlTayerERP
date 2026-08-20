import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";

import { ErpProvider } from "@/lib/erp-store";

export default function RootLayout() {
  return (
    <ErpProvider>
      <StatusBar style="light" />
      <Stack screenOptions={{ headerShown: false, animation: "slide_from_left" }}>
        <Stack.Screen name="(tabs)" />
        <Stack.Screen name="login" />
        <Stack.Screen name="payment-request" />
        <Stack.Screen name="receipt-voucher" />
        <Stack.Screen name="approval/[id]" />
        <Stack.Screen name="connection" />
      </Stack>
    </ErpProvider>
  );
}
