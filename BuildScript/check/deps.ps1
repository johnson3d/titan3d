#requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$Restore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\..\common\common.ps1"

function Resolve-RepoDependencyPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return Join-Path $repoRoot ($Path -replace '/', '\')
}

function Resolve-DependencyPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [string]$BasePath = $repoRoot
    )

    # Simple forward-slash replacement
    $expanded = $Path -replace '/', '\'
    
    # Manually decide whether it is an absolute path (Windows: starts with a drive letter or \\)
    $isAbsolute = ($expanded.Length -ge 2 -and $expanded[1] -eq ':') -or 
                  ($expanded.Length -ge 2 -and $expanded[0] -eq '\' -and $expanded[1] -eq '\')
    
    if ($isAbsolute) {
        return $expanded
    }
    
    # Manually combine the path
    $BasePath = $BasePath.TrimEnd('\')
    $expanded = $expanded.TrimStart('\')
    return "$BasePath\$expanded"
}

function Get-ManifestValues {
    param(
        [Parameter(Mandatory = $true)]
        $Object,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($null -eq $Object -or -not ($Object.PSObject.Properties.Name -contains $Name)) {
        return @()
    }

    $value = $Object.$Name
    if ($null -eq $value) {
        return @()
    }

    if ($value -is [System.Array]) {
        return @($value)
    }

    return @($value)
}

function Add-Missing {
    param([Parameter(Mandatory = $true)][string]$Message)

    $script:missing += $Message
}

function Get-MSBuildVersion {
    param([Parameter(Mandatory = $true)][string]$MSBuildPath)

    $line = @(& $MSBuildPath '-version' '-nologo') |
        Where-Object { $_ -match '^\d+\.\d+\.\d+' } |
        Select-Object -First 1
    if (-not $line) {
        return $null
    }

    return ([string]$line).Trim()
}

$repoRoot = Get-RepoRoot
$manifestPath = Join-Path $PSScriptRoot '..\common\dependencies.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$missing = @()

Write-Host 'Titan dependency check'
Write-Host "  Manifest: $manifestPath"

$dotnet = Find-DotNet
$dotnetVersion = & $dotnet --version
Write-Host "  dotnet:   $dotnetVersion ($dotnet)"

# global.json 与 dependencies.json 的 dotnetSdk 段是同一事实的两处镜像（见 TitanFramework.props 注释）。
# 只改一侧会让 VS 与 dotnet CLI 解析到不同 SDK，因此这里强制保持一致。
$globalJsonPath = Join-Path $repoRoot 'global.json'
if ((Test-Path -LiteralPath $globalJsonPath) -and ($manifest.PSObject.Properties.Name -contains 'dotnetSdk')) {
    $globalSdk = (Get-Content -Raw -LiteralPath $globalJsonPath | ConvertFrom-Json).sdk
    $globalVersion = if ($globalSdk.PSObject.Properties.Name -contains 'version') { [string]$globalSdk.version } else { '' }
    $manifestVersion = if ($manifest.dotnetSdk.PSObject.Properties.Name -contains 'version') { [string]$manifest.dotnetSdk.version } else { '' }
    if ($globalVersion -ne $manifestVersion) {
        Add-Missing "global.json sdk.version '$globalVersion' does not match dependencies.json dotnetSdk.version '$manifestVersion'."
    }

    # latestPatch is the effective default when rollForward is omitted on both sides.
    $globalRollForward = if ($globalSdk.PSObject.Properties.Name -contains 'rollForward') { [string]$globalSdk.rollForward } else { 'latestPatch' }
    $manifestRollForward = if ($manifest.dotnetSdk.PSObject.Properties.Name -contains 'rollForward') { [string]$manifest.dotnetSdk.rollForward } else { 'latestPatch' }
    if ($globalRollForward -ne $manifestRollForward) {
        Add-Missing "global.json sdk.rollForward '$globalRollForward' does not match dependencies.json dotnetSdk.rollForward '$manifestRollForward'."
    }
}

$msbuild = Find-VsMsBuild
Import-VsDevEnvironment $msbuild
$msbuildVersion = Get-MSBuildVersion $msbuild
Write-Host "  MSBuild:  $msbuild ($msbuildVersion)"

# VS 的 MSBuild 必须满足已解析 SDK 的最低要求（sdk\<ver>\minimumMSBuildVersion）。
# 不满足时 VS 只会在加载工程时抛 MSB4236「找不到指定的 SDK Microsoft.NET.Sdk」，而 dotnet CLI
# 走 SDK 自带的 MSBuild 一切正常；两套工具链由此分叉，是极难定位的一类故障，故在此提前拦住。
# 注意：不能靠降级 SDK 波段来满足这条，SDK 波段还绑着编译器自带的 Roslyn 版本（见下面的检查）。
if ($msbuildVersion) {
    $resolvedSdkDir = Join-Path (Split-Path -Parent $dotnet) ('sdk\' + $dotnetVersion)
    $minMsBuildFile = Join-Path $resolvedSdkDir 'minimumMSBuildVersion'
    if (Test-Path -LiteralPath $minMsBuildFile) {
        $minMsBuild = (Get-Content -Raw -LiteralPath $minMsBuildFile).Trim()
        try {
            if ([version]$msbuildVersion -lt [version]$minMsBuild) {
                Add-Missing (
                    "Visual Studio MSBuild $msbuildVersion is older than $minMsBuild, which .NET SDK $dotnetVersion requires. " +
                    "Install a Visual Studio release that bundles MSBuild $minMsBuild (VS 2026 provides MSBuild 18.x). " +
                    "Do not downgrade the SDK feature band to fit an older Visual Studio: " +
                    "that also downgrades the compiler's Roslyn and silently disables the source generators (CS9057). " +
                    "dotnet CLI builds are unaffected because they use the MSBuild bundled inside the SDK."
                )
            }
        }
        catch {
            Add-Missing "Could not compare MSBuild version '$msbuildVersion' with '$minMsBuild' required by .NET SDK $dotnetVersion."
        }
    }
}

# 编译器自带的 Roslyn 必须不低于仓库引用的 Microsoft.CodeAnalysis（Directory.Packages.props）。
# 低于时 Source Generator/Analyzer 会被 CS9057 拒绝加载，而该诊断只是一条 warning：
# 表现为所有生成代码（如 *_bind.g.cs）凭空消失，随后在 TtProgress/TtContainer 等处炸出
# CS0535/CS0115。定位成本极高，故在此提前拦住。
$packagesProps = Join-Path $repoRoot 'Directory.Packages.props'
$sdkRoslynDll = Join-Path (Join-Path (Split-Path -Parent $dotnet) ('sdk\' + $dotnetVersion)) 'Roslyn\bincore\Microsoft.CodeAnalysis.dll'
if ((Test-Path -LiteralPath $packagesProps) -and (Test-Path -LiteralPath $sdkRoslynDll)) {
    $codeAnalysisVersion = $null
    foreach ($line in (Get-Content -LiteralPath $packagesProps)) {
        if ($line -match 'Include\s*=\s*"Microsoft\.CodeAnalysis\.CSharp"\s+Version\s*=\s*"([^"]+)"') {
            $codeAnalysisVersion = $Matches[1]
            break
        }
    }

    $sdkRoslynVersion = $null
    try {
        $sdkRoslynVersion = [System.Reflection.AssemblyName]::GetAssemblyName($sdkRoslynDll).Version
    }
    catch {
        Add-Missing "Could not read the Roslyn version bundled with .NET SDK $dotnetVersion ($sdkRoslynDll)."
    }

    if ($codeAnalysisVersion -and $sdkRoslynVersion) {
        Write-Host "  Roslyn:   $sdkRoslynVersion (repo references Microsoft.CodeAnalysis.CSharp $codeAnalysisVersion)"
        if ($sdkRoslynVersion -lt [version]$codeAnalysisVersion) {
            Add-Missing (
                ".NET SDK $dotnetVersion ships Roslyn $sdkRoslynVersion, which is older than $codeAnalysisVersion required by " +
                "Directory.Packages.props. Source generators and analyzers will be rejected with CS9057 (a warning only), " +
                "so generated code (e.g. *_bind.g.cs) is missing and the build fails with CS0535/CS0115. " +
                "Use the SDK feature band that ships Roslyn >= $codeAnalysisVersion (10.0.4xx), i.e. keep " +
                "global.json rollForward=latestFeature."
            )
        }
    }
}

<#try {
    $llvmBin = Find-VsLlvmBin $msbuild
    Add-PathEntry $llvmBin
    Write-Host "  libclang: $llvmBin"
}
catch {
    Add-Missing $_.Exception.Message
}#>

$nuget = Find-NuGet
Write-Host "  NuGet:    $nuget"

foreach ($package in (Get-ManifestValues $manifest 'nativeNuGetPackages')) {
    $packagesConfig = Resolve-RepoDependencyPath $package.packagesConfig
    if (-not (Test-Path -LiteralPath $packagesConfig)) {
        Add-Missing "Missing packages.config: $packagesConfig"
        continue
    }

    [xml]$packagesXml = Get-Content -Raw -LiteralPath $packagesConfig
    $entry = $packagesXml.packages.package | Where-Object { $_.id -eq $package.id } | Select-Object -First 1
    if ($null -eq $entry) {
        Add-Missing "Missing package '$($package.id)' in $packagesConfig"
    }
    elseif ($entry.version -ne $package.version) {
        Add-Missing "Package '$($package.id)' has version '$($entry.version)' in packages.config, expected '$($package.version)'."
    }

    if ($Restore) {
        Invoke-External $nuget @(
            'restore',
            $packagesConfig,
            '-PackagesDirectory',
            (Join-Path $repoRoot 'packages'),
            '-NonInteractive'
        ) $repoRoot
    }

    foreach ($path in (Get-ManifestValues $package 'requiredPaths')) {
        $requiredPath = Resolve-RepoDependencyPath $path
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            Add-Missing "Missing native NuGet output for '$($package.id)': $requiredPath"
        }
    }
}

$centralPackagesPath = Join-Path $repoRoot 'Directory.Packages.props'
$centralPackageVersions = @{}
if (Test-Path -LiteralPath $centralPackagesPath) {
    [xml]$centralPackagesXml = Get-Content -Raw -LiteralPath $centralPackagesPath
    foreach ($packageVersion in $centralPackagesXml.Project.ItemGroup.PackageVersion) {
        if ($packageVersion.Include) {
            $centralPackageVersions[[string]$packageVersion.Include] = [string]$packageVersion.Version
        }
    }
}

foreach ($package in (Get-ManifestValues $manifest 'managedNuGetPackages')) {
    Write-Host "  NuGet:    $($package.id) $($package.version)"

    if (-not $centralPackageVersions.ContainsKey([string]$package.id)) {
        Add-Missing "Missing PackageVersion '$($package.id)' in $centralPackagesPath"
    }
    elseif ($centralPackageVersions[[string]$package.id] -ne [string]$package.version) {
        Add-Missing "PackageVersion '$($package.id)' is '$($centralPackageVersions[[string]$package.id])', expected '$($package.version)'."
    }

    if ($Restore) {
        foreach ($project in (Get-ManifestValues $package 'restoreProjects')) {
            Invoke-External $dotnet @(
                'restore',
                (Resolve-RepoDependencyPath $project),
                ('/p:SolutionDir=' + $repoRoot + '\'),
                ('/p:TitanRoot=' + $repoRoot + '\')
            ) $repoRoot
        }
    }

    $packageRoot = $null
    if ($package.PSObject.Properties.Name -contains 'packageRoot') {
        $packageRoot = Resolve-DependencyPath $package.packageRoot
    }
    else {
        $packageRoot = Join-Path $env:USERPROFILE ('.nuget\packages\' + ([string]$package.id).ToLowerInvariant() + '\' + [string]$package.version)
    }

    if (-not (Test-Path -LiteralPath $packageRoot)) {
        Add-Missing "Missing NuGet package '$($package.id)' $($package.version): $packageRoot"
        continue
    }

    foreach ($path in (Get-ManifestValues $package 'requiredPaths')) {
        $fullPath = Resolve-DependencyPath $path $packageRoot
        if (-not (Test-Path -LiteralPath $fullPath)) {
            Add-Missing "Missing NuGet package file '$($package.id)': $fullPath"
        }
    }
}

if ($manifest.PSObject.Properties.Name -contains 'windowsSdk') {
    $sdkRoot = $env:WindowsSdkDir
    if (-not $sdkRoot -or -not (Test-Path -LiteralPath $sdkRoot)) {
        $sdkRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\"
    }

    if (-not $sdkRoot -or -not (Test-Path -LiteralPath $sdkRoot)) {
        Add-Missing 'Windows 10/11 SDK was not found. Install the Windows SDK with Visual Studio 2022.'
    }
    else {
        $sdkRoot = (Resolve-Path -LiteralPath $sdkRoot).ProviderPath
        $sdkVersion = $env:WindowsSDKVersion
        if ($sdkVersion) {
            $sdkVersion = $sdkVersion.TrimEnd('\')
        }
        else {
            $includeRoot = Join-Path $sdkRoot 'Include'
            $sdkVersion = Get-ChildItem -LiteralPath $includeRoot -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -match '^10\.' } |
                Sort-Object Name -Descending |
                Select-Object -First 1 -ExpandProperty Name
        }

        if (-not $sdkVersion) {
            Add-Missing "Could not determine Windows SDK version under $sdkRoot."
        }
        else {
            Write-Host "  WinSDK:   $sdkVersion ($sdkRoot)"
            if ($manifest.windowsSdk.PSObject.Properties.Name -contains 'minimumVersion') {
                try {
                    if ([version]$sdkVersion -lt [version]$manifest.windowsSdk.minimumVersion) {
                        Add-Missing "Windows SDK version '$sdkVersion' is older than required '$($manifest.windowsSdk.minimumVersion)'."
                    }
                }
                catch {
                    Add-Missing "Windows SDK version '$sdkVersion' could not be parsed."
                }
            }

            foreach ($path in (Get-ManifestValues $manifest.windowsSdk 'requiredIncludeFiles')) {
                $fullPath = Join-Path $sdkRoot (Join-Path "Include\$sdkVersion" ($path -replace '/', '\'))
                if (-not (Test-Path -LiteralPath $fullPath)) {
                    Add-Missing "Missing Windows SDK include: $fullPath"
                }
            }

            foreach ($path in (Get-ManifestValues $manifest.windowsSdk 'requiredLibFiles')) {
                $fullPath = Join-Path $sdkRoot (Join-Path "Lib\$sdkVersion" ($path -replace '/', '\'))
                if (-not (Test-Path -LiteralPath $fullPath)) {
                    Add-Missing "Missing Windows SDK lib: $fullPath"
                }
            }
        }
    }
}

foreach ($group in (Get-ManifestValues $manifest 'nativeDependencyGroups')) {
    $version = ''
    if ($group.PSObject.Properties.Name -contains 'version' -and -not [string]::IsNullOrWhiteSpace([string]$group.version)) {
        $version = " $($group.version)"
    }
    Write-Host "  native:   $($group.id)$version"

    foreach ($tool in (Get-ManifestValues $group 'requiredTools')) {
        if ($tool -eq 'cmake') {
            try {
                $cmake = Find-CMake
                Write-Host "  CMake:    $cmake"
            }
            catch {
                Add-Missing $_.Exception.Message
            }
        }
        else {
            Add-Missing "Unsupported required build tool '$tool' for native dependency '$($group.id)'."
        }
    }

    foreach ($path in (Get-ManifestValues $group 'requiredPaths')) {
        $fullPath = Resolve-RepoDependencyPath $path
        if (-not (Test-Path -LiteralPath $fullPath)) {
            Add-Missing "Missing native dependency '$($group.id)': $fullPath"
        }
    }

    foreach ($path in (Get-ManifestValues $group 'requiredDirs')) {
        $fullPath = Resolve-RepoDependencyPath $path
        if (-not (Test-Path -LiteralPath $fullPath -PathType Container)) {
            Add-Missing "Missing native dependency directory '$($group.id)': $fullPath"
        }
    }

    foreach ($anyOf in (Get-ManifestValues $group 'requiredAnyOf')) {
        $found = $false
        $checkedPaths = @()
        foreach ($path in (Get-ManifestValues $anyOf 'paths')) {
            $fullPath = Resolve-DependencyPath $path
            $checkedPaths += $fullPath
            if (Test-Path -LiteralPath $fullPath) {
                $found = $true
                break
            }
        }

        if (-not $found) {
            $description = if ($anyOf.PSObject.Properties.Name -contains 'description') { [string]$anyOf.description } else { 'one required path' }
            Add-Missing "Missing native dependency '$($group.id)' ($description). Checked: $($checkedPaths -join '; ')"
        }
    }
}

foreach ($path in (Get-ManifestValues $manifest 'vendoredNativePaths')) {
    $fullPath = Resolve-RepoDependencyPath $path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        Add-Missing "Missing vendored native dependency path: $fullPath"
    }
}

foreach ($path in (Get-ManifestValues $manifest 'vendoredCSharpPaths')) {
    $fullPath = Resolve-RepoDependencyPath $path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        Add-Missing "Missing vendored C# dependency path: $fullPath"
    }
}

if ($missing.Count -gt 0) {
    Write-Host ''
    Write-Host 'Missing dependencies:'
    foreach ($item in $missing) {
        Write-Host "  - $item"
    }
    throw "Dependency check failed with $($missing.Count) missing item(s)."
}

Write-Host 'Dependency check completed.'
