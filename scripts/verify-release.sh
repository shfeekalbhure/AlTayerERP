#!/usr/bin/env bash
set -euo pipefail

# Local / CI verification for Linux. It never connects to MySQL and it does not
# publish artifacts. Windows and MAUI device verification remain in the PowerShell script.
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

echo "[1/5] SDK information"
dotnet --info

echo "[2/5] Restore server tests and desktop cross-target assets"
dotnet restore AlTayerERP.Tests/AlTayerERP.Tests.csproj
dotnet restore AlTayerERP.Desktop/AlTayerERP.Desktop.csproj -p:EnableWindowsTargeting=true

echo "[3/5] API Release build with warnings as errors"
dotnet build AlTayerERP.API/AlTayerERP.API.csproj \
  -c Release --no-restore -warnaserror

echo "[4/5] Desktop cross-target Release build with warnings as errors"
dotnet build AlTayerERP.Desktop/AlTayerERP.Desktop.csproj \
  -c Release --no-restore -p:EnableWindowsTargeting=true -warnaserror

echo "[5/5] Server tests and delivery invariants"
dotnet test AlTayerERP.Tests/AlTayerERP.Tests.csproj \
  -c Release --no-restore
git diff --check
grep -q 'Idempotency-Key' AlTayerERP.API/Controllers/FinancialVoucherController.cs
grep -q 'Idempotency-Key' AlTayerERP.API/Controllers/PaymentRequestsController.cs
grep -q 'UQ_Idempotency_Record_Scope_Key' \
  AlTayerERP.Infrastructure/Migrations/20260820043000_AddIdempotencyRecords.cs

echo "PASS: local delivery verification completed."
echo "NEXT: run scripts/Verify-Release.ps1 on Windows and the MySQL/device acceptance checks in the runbook."
