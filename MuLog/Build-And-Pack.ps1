# Build-And-Pack.ps1 - CON DIRECTORY CONFIGURABILE
param(
    [string]$OutputDir = "",  # Se vuoto, usa default del csproj
    [switch]$IncrementVersion
)

Write-Host "=== MU-LOG PACKAGE BUILDER ===" -ForegroundColor Cyan

# 1. DETERMINA DIRECTORY OUTPUT
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    # Usa default dal csproj
    $OutputDir = "$PSScriptRoot\..\NuGetPackages"
    Write-Host "Usando directory di default: $OutputDir" -ForegroundColor Yellow
} else {
    # Usa directory specificata
    Write-Host "Usando directory specificata: $OutputDir" -ForegroundColor Green
}

# 2. FILE PER LA VERSIONE
$versionFile = "$PSScriptRoot\build-version.json"
$csprojPath = "$PSScriptRoot\MuLog.csproj"

# 3. LEGGI/CREA VERSIONE
if (Test-Path $versionFile) {
    $versionData = Get-Content $versionFile | ConvertFrom-Json
    $currentVersion = $versionData.Version
} else {
    $currentVersion = "1.0.0"
}

# 4. INCREMENTA SE RICHIESTO
if ($IncrementVersion) {
    $parts = $currentVersion -split '\.'
    $patch = [int]$parts[2] + 1
    $newVersion = "$($parts[0]).$($parts[1]).$patch"
} else {
    $newVersion = $currentVersion
}

Write-Host "Versione: $newVersion" -ForegroundColor Green

# 5. SALVA NUOVA VERSIONE
$buildCount = 0
if (Test-Path $versionFile) {
    $versionData = Get-Content $versionFile | ConvertFrom-Json
    $buildCount = $versionData.BuildCount + 1
}

$newVersionData = @{
    Version = $newVersion
    BuildCount = $buildCount
    LastBuild = [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss")
}
$newVersionData | ConvertTo-Json | Set-Content $versionFile

# 6. AGGIORNA CSPROJ
$csprojContent = Get-Content $csprojPath -Raw
$csprojContent = $csprojContent -replace '<Version>.*?</Version>', "<Version>$newVersion</Version>"
Set-Content $csprojPath $csprojContent

# 7. PULIZIA E BUILD
Write-Host "`nCompilazione in corso..." -ForegroundColor Green
dotnet clean --configuration Release
dotnet build --configuration Release

# 8. CREA PACCHETTO PASSANDO LA DIRECTORY
Write-Host "Creazione pacchetto NuGet..." -ForegroundColor Green
dotnet pack `
    --configuration Release `
    --output $OutputDir `
    --no-build `
    -p:PackageOutputPath="$OutputDir" `
    --include-symbols

# 9. MOSTRA RISULTATI
Write-Host "`n=== COMPLETATO ===" -ForegroundColor Cyan
$packageDir = "$OutputDir\MuLog\$newVersion"
if (Test-Path $packageDir) {
    Write-Host "Pacchetto creato in: $packageDir" -ForegroundColor Green
    Get-ChildItem -Path $packageDir | ForEach-Object {
        Write-Host "  ✓ $($_.Name)" -ForegroundColor Gray
    }
}