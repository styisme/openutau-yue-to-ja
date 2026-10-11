<#
.SYNOPSIS
    Fetch third-party build dependencies for YueToJapanesePhonemizer.

.DESCRIPTION
    The plugin project references five third-party DLLs that are not checked
    into Git (see .gitignore). This script downloads them:

      1. OpenUtau-win-x64.zip from the official stakira/OpenUtau GitHub release
         -> OpenUtau.Core.dll, OpenUtau.Plugin.Builtin.dll,
            WanaKanaNet.dll, Serilog.dll, YamlDotNet.dll (if present in the zip)
      2. Any DLL still missing is fetched from NuGet (nupkg = zip) as fallback.

    When every DLL already exists in the destination the script exits
    immediately, which makes it safe to run on every build/CI invocation.

.PARAMETER Destination
    Directory that receives the DLLs (the src project directory by default).

.PARAMETER OpenUtauTag
    Optional OpenUtau release tag to pin, e.g. "0.1.572.3-alpha".
    Empty = latest release that contains OpenUtau-win-x64.zip (prereleases included).

.EXAMPLE
    .\fetch-deps.ps1
    .\fetch-deps.ps1 -OpenUtauTag "0.1.572.3-alpha"
#>

param(
    [string]$Destination = "",
    [string]$OpenUtauTag = ""
)

$ErrorActionPreference = "Stop"

$RequiredDlls = @(
    "OpenUtau.Core.dll",
    "OpenUtau.Plugin.Builtin.dll",
    "WanaKanaNet.dll",
    "Serilog.dll",
    "YamlDotNet.dll"
)

# NuGet fallback (package id -> file name inside the nupkg)
$NugetFallback = @{
    "Serilog.dll"      = "serilog"
    "YamlDotNet.dll"   = "yamldotnet"
    "WanaKanaNet.dll"  = "wanakana-net"
}

function Write-Info { param($msg) Write-Host "[INFO]  $msg" -ForegroundColor Cyan }
function Write-Ok   { param($msg) Write-Host "[OK]    $msg" -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host "[WARN]  $msg" -ForegroundColor Yellow }
function Write-Err  { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

if (-not $Destination) {
    $Destination = (Resolve-Path (Join-Path $PSScriptRoot "..\src\YueToJapanesePhonemizer")).Path
}
New-Item -ItemType Directory -Path $Destination -Force | Out-Null

# ---- Fast path: everything already present (CI cache hit / second run) ----
$missing = @($RequiredDlls | Where-Object { -not (Test-Path (Join-Path $Destination $_)) })
if ($missing.Count -eq 0) {
    Write-Ok "All dependencies already present in $Destination"
    exit 0
}
Write-Info "Missing: $($missing -join ', ')"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$githubHeaders = @{ "User-Agent" = "openutau-yue-to-ja-fetch-deps" }

# ---- Step 1: OpenUtau release zip ----
try {
    if ($OpenUtauTag) {
        Write-Info "Resolving release tag $OpenUtauTag ..."
        $release = Invoke-RestMethod -Headers $githubHeaders `
            -Uri "https://api.github.com/repos/stakira/OpenUtau/releases/tags/$OpenUtauTag"
    } else {
        Write-Info "Resolving latest OpenUtau release ..."
        $releases = Invoke-RestMethod -Headers $githubHeaders `
            -Uri "https://api.github.com/repos/stakira/OpenUtau/releases?per_page=10"
        $release = $releases | Where-Object {
            $_.assets | Where-Object { $_.name -eq "OpenUtau-win-x64.zip" }
        } | Select-Object -First 1
    }

    $asset = $null
    if ($release) {
        $asset = $release.assets | Where-Object { $_.name -eq "OpenUtau-win-x64.zip" }
    }

    if (-not $asset) {
        throw "No release with asset 'OpenUtau-win-x64.zip' found"
    }

    Write-Info "Release: $($release.tag_name)"
    $zipPath = Join-Path $env:TEMP "OpenUtau-win-x64-$($release.tag_name).zip"

    if (-not (Test-Path $zipPath)) {
        Write-Info "Downloading $($asset.browser_download_url) ..."
        Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -UseBasicParsing
    } else {
        Write-Info "Using cached zip: $zipPath"
    }

    Write-Info "Extracting DLLs from zip ..."
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        foreach ($dll in $missing) {
            $entry = $zip.Entries | Where-Object {
                $_.Name -eq $dll -and -not $_.FullName.EndsWith("/")
            } | Select-Object -First 1
            if ($entry) {
                $target = Join-Path $Destination $dll
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
                Write-Ok "Extracted $dll ($([math]::Round($entry.Length/1KB,1)) KB)"
            } else {
                Write-Warn "$dll not found in zip, will try NuGet fallback"
            }
        }
    } finally {
        $zip.Dispose()
    }
} catch {
    Write-Warn "OpenUtau release fetch failed: $($_.Exception.Message)"
    Write-Warn "Will try NuGet for everything missing."
}

# ---- Step 2: NuGet fallback for whatever is still missing ----
$stillMissing = @($RequiredDlls | Where-Object { -not (Test-Path (Join-Path $Destination $_)) })

foreach ($dll in $stillMissing) {
    if (-not $NugetFallback.ContainsKey($dll)) {
        Write-Err "No NuGet fallback known for $dll - copy it manually into $Destination"
        exit 1
    }
    $pkgId = $NugetFallback[$dll]

    try {
        Write-Info "NuGet: resolving latest stable version of $pkgId ..."
        $index = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$pkgId/index.json"
        $version = $index.versions | Where-Object { $_ -notmatch "-" } | Select-Object -Last 1
        if (-not $version) { $version = $index.versions | Select-Object -Last 1 }

        $nupkg = Join-Path $env:TEMP "$pkgId-$version.nupkg"
        if (-not (Test-Path $nupkg)) {
            Write-Info "Downloading $pkgId $version ..."
            Invoke-WebRequest -Uri "https://api.nuget.org/v3-flatcontainer/$pkgId/$version/$pkgId.$version.nupkg" `
                -OutFile $nupkg -UseBasicParsing
        }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
        try {
            # Prefer lib/<tfm>/<dll>; skip ref/ and resources
            $entry = $zip.Entries |
                Where-Object { $_.Name -eq $dll -and $_.FullName -like "lib/*" -and $_.FullName -notlike "*resources*" } |
                Sort-Object { if ($_.FullName -match "net[0-9]") { 0 } else { 1 } } |
                Select-Object -First 1
            if (-not $entry) {
                $entry = $zip.Entries | Where-Object { $_.Name -eq $dll } | Select-Object -First 1
            }
            if (-not $entry) { throw "entry $dll not found in nupkg" }

            $target = Join-Path $Destination $dll
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
            Write-Ok "NuGet $pkgId $version -> $dll"
        } finally {
            $zip.Dispose()
        }
    } catch {
        Write-Err "NuGet fallback failed for $dll : $($_.Exception.Message)"
        exit 1
    }
}

# ---- Final check ----
$final = @($RequiredDlls | Where-Object { -not (Test-Path (Join-Path $Destination $_)) })
if ($final.Count -gt 0) {
    Write-Err "Still missing: $($final -join ', ')"
    exit 1
}
Write-Ok "All dependencies ready in $Destination"
