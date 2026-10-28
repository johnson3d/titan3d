#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [ValidateSet('All', 'Native', 'Managed', 'Apk')][string]$Stage = 'All',

    # 供测试注入假仓库根与假工具链；正常调用留空即可。
    [string]$RepositoryRoot = '',
    [string]$ToolchainConfig = ''
)

# Android 构建编排：Native -> Managed(S1) -> 内容暂存 -> APK。
# 每个阶段都消费上一阶段的产物并复核，缺前置时直接失败，不产出空壳 APK。
# 本文件必须能在 Windows PowerShell 5.1 的 ConstrainedLanguage 下运行。

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
    Write-Error '一期构建要求 Windows 主机' -ErrorAction Continue
    exit 2
}

$root = Get-TitanRoot $RepositoryRoot
$config = $Configuration.ToLowerInvariant()
$nativeLibrary = Join-Path $root "binaries\android\$config\libCore.Android.so"
# 原生中间产物统一落在 intermediate/cpp/<Project>/<Config>（源码内构建会被 CMakeLists 守卫拒绝）。
$nativeBuildDirectory = Join-Path $root "intermediate\cpp\Core.Android\$Configuration"
$mauiDirectory = Join-Path $root 'Module\AloneGame.Maui'
$engineProject = Join-Path $root 'Module\Engine.Android\Engine.Android.csproj'
$statePath = Join-Path $root 'codegen\Android\DataCopyer\.datacopyer.state.json'

# Apk 阶段要用 dotnet 与 Android SDK，所以工具链按 Managed 阶段（Native + Managed）校验。
try {
    $tools = Find-AndroidTools -RepositoryRoot $root -Stage 'Managed' -ConfigPath $ToolchainConfig
}
catch {
    Write-Error $_.Exception.Message -ErrorAction Continue
    exit 2
}

try {
    if ($Stage -in @('All', 'Native')) {
        # [TITAN-STAGE] 机器可读阶段标记：编辑器面板依此画步骤进度条。
        Write-Host '[TITAN-STAGE] Native'
        Invoke-AndroidCommand 'RefreshSources' $tools.CMake @(
            '-DTITAN_CHECK=ON', '-P', (Join-Path $root 'Core.Android\RefreshSources.cmake'))
        Invoke-AndroidCommand 'GenerateCodegen' $tools.CMake @(
            "-DTITAN_NDK_ROOT=$($tools.Ndk)", "-DTITAN_CONFIGURATION=$Configuration",
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
            throw "Native 产物缺失：$nativeLibrary；先运行 -Stage Native"
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
            throw "Apk 缺少 Native/Managed 前置：$nativeLibrary 或 $statePath；先运行 -Stage All"
        }
        $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
        if ([string]$state.Platform -ne 'Android' -or [string]$state.Configuration -ne $Configuration) {
            throw "S1 状态配置不匹配：Platform=$($state.Platform) Configuration=$($state.Configuration)，期望 Android/$Configuration；先运行 -Stage Managed"
        }

        # 中间产物统一落在 intermediate/android/content/<Configuration>；工程目录不再承载暂存内容。
        $stageOutput = Join-Path $root "intermediate\android\content\$Configuration"
        Write-Host '[TITAN-STAGE] Content'
        & (Join-Path $PSScriptRoot 'Stage-AndroidContent.ps1') -RepositoryRoot $root -OutputDirectory $stageOutput -Configuration $Configuration

        # Start-Process 不继承 Push-Location（进程 CWD 不变），所以 csproj 必须用绝对路径。
        # Debug 下 SDK 默认 EmbedAssembliesIntoApk=False（FastDev 模式），产物不能独立安装；这里必须显式嵌入。
        $mauiProject = Join-Path $mauiDirectory 'AloneGame.Maui.csproj'
        Write-Host '[TITAN-STAGE] Apk'
        Invoke-AndroidCommand 'Apk' $tools.DotNet @(
            'build', $mauiProject, '-f', 'net10.0-android', '-c', $Configuration,
            '-p:AndroidPackageFormats=apk', '-p:AndroidKeyStore=false', '-p:BuildProjectReferences=false',
            '-p:EmbedAssembliesIntoApk=true',
            "-p:AndroidSdkDirectory=$($tools.Sdk)", "-p:AndroidNdkDirectory=$($tools.Ndk)")

        $apkDirectory = Join-Path $mauiDirectory "bin\$Configuration\net10.0-android"
        $signed = @()
        if (Test-Path -LiteralPath $apkDirectory) {
            $signed = @(Get-ChildItem -LiteralPath $apkDirectory -Filter '*-Signed.apk' -File -Recurse)
        }
        if ($signed.Count -ne 1) {
            throw "预期唯一 Signed APK，实际 $($signed.Count) 个（$apkDirectory）"
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
