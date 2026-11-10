#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [ValidateSet('All', 'Native', 'Managed', 'Apk')][string]$Stage = 'All',

    # For tests to inject a fake repo root and fake toolchain; leave empty for normal use.
    [string]$RepositoryRoot = '',
    [string]$ToolchainConfig = ''
)

# Android build orchestration: Native -> Managed(S1) -> content staging -> APK.
# Each stage consumes and re-verifies the previous stage's artifacts; missing prerequisites fail fast instead of producing an empty-shell APK.
# This file must run under Windows PowerShell 5.1 ConstrainedLanguage.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\AndroidTools.ps1"

function Get-TitanRoot {
    param([string]$Path = '')

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).ProviderPath.TrimEnd('\')
    }
    return (Resolve-Path -LiteralPath $Path).ProviderPath.TrimEnd('\')
}

if ($env:OS -ne 'Windows_NT') {
    Write-Error 'Stage-1 build requires a Windows host' -ErrorAction Continue
    exit 2
}

$root = Get-TitanRoot $RepositoryRoot
# Sub-step process CWD is pinned to the repo root: otherwise dotnet cannot find global.json under TEMP and silently picks the newest SDK (including rc previews).
$script:AndroidProcessWorkingDirectory = $root
$config = $Configuration.ToLowerInvariant()
$nativeLibrary = Join-Path $root "binaries\android\$config\libCore.Android.so"
# Native intermediates always land in intermediate/cpp/<Project>/<Config> (in-source builds are rejected by the CMakeLists guard).
$nativeBuildDirectory = Join-Path $root "intermediate\cpp\Core.Android\$Configuration"
$mauiDirectory = Join-Path $root 'Module\AloneGame.Maui'
$engineProject = Join-Path $root 'Module\Engine.Android\Engine.Android.csproj'
$statePath = Join-Path $root 'codegen\Android\DataCopyer\.datacopyer.state.json'

# The Apk stage needs dotnet and the Android SDK, so the toolchain is validated for the Managed stage (Native + Managed).
try {
    $tools = Find-AndroidTools -RepositoryRoot $root -Stage 'Managed' -ConfigPath $ToolchainConfig
}
catch {
    Write-Error $_.Exception.Message -ErrorAction Continue
    exit 2
}

try {
    if ($Stage -in @('All', 'Native')) {
        # [TITAN-STAGE] machine-readable stage marker: the editor panel draws step progress from it.
        Write-Host '[TITAN-STAGE] Native'
        Enable-AndroidBashOnPath -Stage 'Native'
        Invoke-AndroidCommand 'RefreshSources' $tools.CMake @(
            '-DTITAN_CHECK=ON', '-P', (Join-Path $root 'Core.Android\RefreshSources.cmake'))
        Invoke-AndroidCommand 'GenerateCodegen' $tools.CMake @(
            "-DTITAN_NDK_ROOT=$($tools.Ndk)", "-DTITAN_CONFIGURATION=$Configuration",
            "-DTITAN_ANDROID_API_LEVEL=$($tools.ApiLevel)",
            '-P', (Join-Path $root 'Core.Android\GenerateCodegen.cmake'))
        Invoke-AndroidCommand 'Configure' $tools.CMake @(
            '-S', (Join-Path $root 'Core.Android'), '-B', $nativeBuildDirectory,
            '-G', $tools.Generator,
            "-DCMAKE_MAKE_PROGRAM=$($tools.Ninja)",
            "-DCMAKE_TOOLCHAIN_FILE=$($tools.Ndk)\build\cmake\android.toolchain.cmake",
            "-DANDROID_ABI=$($tools.Abi)", "-DANDROID_PLATFORM=$($tools.ApiLevel)", "-DANDROID_STL=$($tools.Stl)",
            '-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON',
            "-DCMAKE_BUILD_TYPE=$Configuration", "-DTITAN_CONFIGURATION=$Configuration")
        Invoke-AndroidCommand 'Native' $tools.CMake @(
            '--build', $nativeBuildDirectory, '--parallel')
        Test-AndroidNativeLibrary -Tools $tools -Library $nativeLibrary | Out-Null
    }

    if ($Stage -in @('All', 'Managed')) {
        Write-Host '[TITAN-STAGE] Managed'
        if (-not (Test-Path -LiteralPath $nativeLibrary -PathType Leaf)) {
            throw "Native artifact missing: $nativeLibrary; run -Stage Native first"
        }
        Invoke-AndroidCommand 'Engine.Android' $tools.DotNet @(
            'build', $engineProject, '-c', $Configuration,
            '-p:TitanPlatform=Android', '-p:SkipTitanCodeGen=true', '-p:SkipTitanDataCopyerGen=true')
        Invoke-AndroidCommand 'DataCopyer Android' $tools.DotNet @(
            'msbuild', $engineProject, '-t:TitanRunDataCopyerStage',
            '-p:TitanPlatform=Android', "-p:Configuration=$Configuration",
            '-p:SkipTitanCodeGen=true', '-p:SkipTitanDataCopyerGen=false')
    }

    if ($Stage -in @('All', 'Apk')) {
        if (-not (Test-Path -LiteralPath $nativeLibrary -PathType Leaf) -or
            -not (Test-Path -LiteralPath $statePath -PathType Leaf)) {
            throw "Apk is missing Native/Managed prerequisites: $nativeLibrary or $statePath; run -Stage All first"
        }
        $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
        if ([string]$state.Platform -ne 'Android' -or [string]$state.Configuration -ne $Configuration) {
            throw "S1 state configuration mismatch: Platform=$($state.Platform) Configuration=$($state.Configuration), expected Android/$Configuration; run -Stage Managed first"
        }

        # Intermediates always land in intermediate/android/content/<Configuration>; project directories no longer hold staged content.
        $stageOutput = Join-Path $root "intermediate\android\content\$Configuration"
        Write-Host '[TITAN-STAGE] Content'
        & (Join-Path $PSScriptRoot 'Stage-AndroidContent.ps1') -RepositoryRoot $root -OutputDirectory $stageOutput -Configuration $Configuration

        # Start-Process does not inherit Push-Location (the process CWD is unchanged), so the csproj must be an absolute path.
        # Under Debug the SDK defaults EmbedAssembliesIntoApk=False (FastDev mode) and the artifact cannot install standalone; embedding must be requested explicitly here.
        $mauiProject = Join-Path $mauiDirectory 'AloneGame.Maui.csproj'
        Write-Host '[TITAN-STAGE] Apk'
        Invoke-AndroidCommand 'Apk' $tools.DotNet @(
            'build', $mauiProject, '-f', 'net10.0-android', '-c', $Configuration,
            '-p:AndroidPackageFormats=apk', '-p:AndroidKeyStore=false', '-p:BuildProjectReferences=false',
            '-p:EmbedAssembliesIntoApk=true',
            "-p:AndroidSdkDirectory=$($tools.Sdk)", "-p:AndroidNdkDirectory=$($tools.Ndk)",
            "-p:JavaSdkDirectory=$($tools.JavaHome)")

        $apkDirectory = Join-Path $mauiDirectory "bin\$Configuration\net10.0-android"
        $signed = @()
        if (Test-Path -LiteralPath $apkDirectory) {
            $signed = @(Get-ChildItem -LiteralPath $apkDirectory -Filter '*-Signed.apk' -File -Recurse)
        }
        if ($signed.Count -ne 1) {
            throw "Expected exactly one signed APK, got $($signed.Count) ($apkDirectory)"
        }

        $resultDirectory = Join-Path $root 'cache\android_build'
        New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null
        Set-Content -LiteralPath (Join-Path $resultDirectory "${config}.apk-path.txt") -Value $signed[0].FullName -Encoding ASCII
        Write-Host "APK=$($signed[0].FullName)"
    }
}
catch {
    Write-Error $_.Exception.Message -ErrorAction Continue
    exit 1
}

exit 0
