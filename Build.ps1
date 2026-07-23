Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Join-Path $Root "src"
$Icon = Join-Path $Root "assets\app.ico"
$Manifest = Join-Path $Root "app.manifest"

$Release = Join-Path $Root "Release"
$Output = Join-Path $Release "Forza Horizon Luna Open Source.exe"

$SQLiteManaged = Join-Path $Root "tools\packages\Stub.System.Data.SQLite.Core.NetFramework\lib\net46\System.Data.SQLite.dll"
$SQLiteInteropX64 = Join-Path $Root "tools\packages\Stub.System.Data.SQLite.Core.NetFramework\build\net46\x64\SQLite.Interop.dll"
$WebView2Core = Join-Path $Root "tools\packages\Microsoft.Web.WebView2\lib\net462\Microsoft.Web.WebView2.Core.dll"
$WebView2WinForms = Join-Path $Root "tools\packages\Microsoft.Web.WebView2\lib\net462\Microsoft.Web.WebView2.WinForms.dll"
$WebView2LoaderX64 = Join-Path $Root "tools\packages\Microsoft.Web.WebView2\runtimes\win-x64\native\WebView2Loader.dll"

$CompilerCandidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)
$Compiler = $CompilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $Compiler) {
    throw "Could not find the .NET Framework C# compiler (csc.exe). Install the .NET Framework 4.x developer tools."
}

foreach ($Required in @($Icon, $Manifest, $SQLiteManaged, $SQLiteInteropX64, $WebView2Core, $WebView2WinForms, $WebView2LoaderX64)) {
    if (-not (Test-Path -LiteralPath $Required)) {
        throw "Missing required build input: $Required"
    }
}

$SidebarIconDir = Join-Path $Root "assets\sidebar"
$SidebarIconNames = @(
    "Connection.png", "features (1).png", "Autoshow.png", "database (1).png",
    "self-driving.png", "service_846355.png", "photo-mode.png", "role-play.png",
    "car-maintenance.png", "teleport (1).png", "consoel.png", "file.png",
    "Tutorial 2.png", "bug.png", "setting.png", "donate.png", "Credit.png"
)
$PlatformIconDir = Join-Path $Root "assets\platform"
$PlatformIconItems = @(
    @{ FileName = "steam-icon.png"; ResourceName = "steam-icon.png" },
    @{ FileName = "xbox-icon.png"; ResourceName = "xbox-icon.png" }
)
$PremadeTeleportLists = Join-Path $Root "assets\teleport_lists\premade"
$PremadeDrivingPresets = Join-Path $Root "assets\driving_presets\premade"
$PremadeTeleportItems = @(
    @{ FileName = "All XP Boards V1.1- Ken.txt"; ResourceName = "premade-teleport-all-xp-boards-ken.txt" },
    @{ FileName = "All Mascots - Ken.txt"; ResourceName = "premade-teleport-all-mascots-ken.txt" },
    @{ FileName = "All Houses - Ken.txt"; ResourceName = "premade-teleport-all-houses-ken.txt" },
    @{ FileName = "All Photo List V1.1- Ken.txt"; ResourceName = "premade-teleport-all-photo-list-ken.txt" },
    @{ FileName = "Discover Japan - Ken.txt"; ResourceName = "premade-teleport-discover-japan-ken.txt" },
    @{ FileName = "All Legends XP Boards - Patch.txt"; ResourceName = "premade-teleport-all-legends-xp-boards-patch.txt" },
    @{ FileName = "All Barn Finds - Patch.txt"; ResourceName = "premade-teleport-all-barn-finds-patch.txt" },
    @{ FileName = "All Treasure Cars - Patch.txt"; ResourceName = "premade-teleport-all-treasure-cars-patch.txt" }
)
$PremadeDrivingPresetItems = @(
    @{ FileName = "Driving Editor - Girp and Stability - By Ken.txt"; ResourceName = "premade-driving-grip-stability-ken.txt" },
    @{ FileName = "Driving Editor - Instant Launch.txt"; ResourceName = "premade-driving-instant-launch.txt" },
    @{ FileName = "Driving Editor Preset - Chaos Mode Preset - By Ken.txt"; ResourceName = "premade-driving-chaos-preset-ken.txt" },
    @{ FileName = "Driving Editor - Chaos Mode Preset - By Ken.txt"; ResourceName = "premade-driving-chaos-mode-ken.txt" }
)
$CreditAssetsDir = Join-Path $Root "assets\credits"
$CreditAssetNames = @(
    "Shina Profile Picture.png", "forza mods profile.gif", "Defualt.png",
    "Matkhl Profile Picture.png", "Ariza Profile Picture.png",
    "Ken Profile Picture.png", "Patchy Profile Picture.png", "Merika Profile Picture.png"
)

$ResourceArgs = @(
    "/resource:$Icon,app.ico",
    "/resource:$(Join-Path $Root 'assets\app-icon.png'),app-icon.png",
    "/resource:$(Join-Path $Root 'assets\luna-logo.png'),luna-logo.png",
    "/resource:$(Join-Path $Root 'assets\luna-banner.jpeg'),luna-banner.jpeg",
    "/resource:$(Join-Path $Root 'assets\gas-station.png'),gas-station.png",
    "/resource:$(Join-Path $Root 'assets\browser.png'),browser.png",
    "/resource:$(Join-Path $Root 'assets\discord.png'),discord.png",
    "/resource:$(Join-Path $Root 'assets\sidebar-logo.png'),sidebar-logo.png",
    "/resource:$(Join-Path $Root 'assets\kofi-logo.png'),kofi-logo.png",
    "/resource:$(Join-Path $Root 'assets\crypto-usdt.png'),crypto-usdt.png",
    "/resource:$(Join-Path $Root 'assets\theme\moon.png'),theme-moon.png",
    "/resource:$(Join-Path $Root 'assets\theme\sun.png'),theme-sun.png",
    "/resource:$(Join-Path $Root 'assets\teleport-location-saved.mp3'),teleport-location-saved.mp3",
    "/resource:$WebView2Core,bundled-webview2-core.dll",
    "/resource:$WebView2WinForms,bundled-webview2-winforms.dll",
    "/resource:$WebView2LoaderX64,bundled-webview2-loader.dll"
)
foreach ($SidebarIconName in $SidebarIconNames) {
    $ResourceArgs += "/resource:$(Join-Path $SidebarIconDir $SidebarIconName),$SidebarIconName"
}
foreach ($PlatformIconItem in $PlatformIconItems) {
    $ResourceArgs += "/resource:$(Join-Path $PlatformIconDir $PlatformIconItem.FileName),$($PlatformIconItem.ResourceName)"
}
foreach ($PremadeTeleportItem in $PremadeTeleportItems) {
    $ResourceArgs += "/resource:$(Join-Path $PremadeTeleportLists $PremadeTeleportItem.FileName),$($PremadeTeleportItem.ResourceName)"
}
foreach ($PremadeDrivingPresetItem in $PremadeDrivingPresetItems) {
    $ResourceArgs += "/resource:$(Join-Path $PremadeDrivingPresets $PremadeDrivingPresetItem.FileName),$($PremadeDrivingPresetItem.ResourceName)"
}
foreach ($CreditAssetName in $CreditAssetNames) {
    $ResourceArgs += "/resource:$(Join-Path $CreditAssetsDir $CreditAssetName),$CreditAssetName"
}

$Sources = Get-ChildItem -LiteralPath $SourceDir -Filter "*.cs" -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName }
if (-not $Sources) {
    throw "No C# source files found in src."
}

if (Test-Path -LiteralPath $Release) {
    Remove-Item -LiteralPath $Release -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $Release | Out-Null

$CompilerArgs = @(
    "/nologo", "/target:winexe", "/platform:x64", "/optimize+", "/debug-",
    "/win32icon:$Icon", "/win32manifest:$Manifest"
)
$CompilerArgs += $ResourceArgs
$CompilerArgs += @(
    "/out:$Output",
    "/reference:System.dll",
    "/reference:System.Core.dll",
    "/reference:System.Data.dll",
    "/reference:System.Drawing.dll",
    "/reference:$SQLiteManaged",
    "/reference:$WebView2Core",
    "/reference:$WebView2WinForms",
    "/reference:System.IO.Compression.dll",
    "/reference:System.IO.Compression.FileSystem.dll",
    "/reference:System.Windows.Forms.dll"
)
$CompilerArgs += $Sources

& $Compiler @CompilerArgs
if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE."
}

Copy-Item -LiteralPath $SQLiteManaged -Destination (Join-Path $Release "System.Data.SQLite.dll") -Force
$ReleaseX64 = Join-Path $Release "x64"
New-Item -ItemType Directory -Force -Path $ReleaseX64 | Out-Null
Copy-Item -LiteralPath $SQLiteInteropX64 -Destination (Join-Path $ReleaseX64 "SQLite.Interop.dll") -Force
Copy-Item -LiteralPath $WebView2Core -Destination (Join-Path $Release "Microsoft.Web.WebView2.Core.dll") -Force
Copy-Item -LiteralPath $WebView2WinForms -Destination (Join-Path $Release "Microsoft.Web.WebView2.WinForms.dll") -Force
Copy-Item -LiteralPath $WebView2LoaderX64 -Destination (Join-Path $Release "WebView2Loader.dll") -Force

$ReleaseCredits = Join-Path $Release "credits"
New-Item -ItemType Directory -Force -Path $ReleaseCredits | Out-Null
foreach ($CreditAssetName in $CreditAssetNames) {
    Copy-Item -LiteralPath (Join-Path $CreditAssetsDir $CreditAssetName) -Destination (Join-Path $ReleaseCredits $CreditAssetName) -Force
}

$ReleasePremadeTeleport = Join-Path $Release "teleport_lists\premade"
New-Item -ItemType Directory -Force -Path $ReleasePremadeTeleport | Out-Null
Get-ChildItem -LiteralPath $PremadeTeleportLists -Filter "*.txt" -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $ReleasePremadeTeleport -Force
}
$ReleasePremadeDriving = Join-Path $Release "driving_presets\premade"
New-Item -ItemType Directory -Force -Path $ReleasePremadeDriving | Out-Null
Get-ChildItem -LiteralPath $PremadeDrivingPresets -Filter "*.txt" -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $ReleasePremadeDriving -Force
}

Write-Host "Built: $Output"
