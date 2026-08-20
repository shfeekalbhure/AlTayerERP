$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Windows verification. This script never applies a database migration, posts a
# document, or publishes an application. Run it from PowerShell at repository root.
$RepositoryRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepositoryRoot

Write-Host '[1/6] SDK and workload information'
dotnet --info
dotnet workload list

Write-Host '[2/6] Restore solution'
dotnet restore .\AlTayerERP.sln

Write-Host '[3/6] Build API with warnings as errors'
dotnet build .\AlTayerERP.API\AlTayerERP.API.csproj -c Release --no-restore -warnaserror

Write-Host '[4/6] Build Desktop with warnings as errors'
dotnet build .\AlTayerERP.Desktop\AlTayerERP.Desktop.csproj -c Release --no-restore -warnaserror

Write-Host '[5/6] Build MAUI Android with warnings as errors'
dotnet build .\AlTayerERP.Mobile.Office\AlTayerERP.Mobile.Office.csproj -c Release --no-restore -f net10.0-android -warnaserror

Write-Host '[6/6] Run server tests'
dotnet test .\AlTayerERP.Tests\AlTayerERP.Tests.csproj -c Release --no-restore

Write-Host 'PASS: Windows code verification completed. Proceed to the MySQL and device acceptance steps in the runbook.'
