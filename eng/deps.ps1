#requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$Restore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\common.ps1"

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

    $expanded = [Environment]::ExpandEnvironmentVariables($Path) -replace '/', '\'
    if ([System.IO.Path]::IsPathRooted($expanded)) {
        return $expanded
    }

    return Join-Path $BasePath $expanded
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

$repoRoot = Get-RepoRoot
$manifestPath = Join-Path $PSScriptRoot 'dependencies.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$missing = @()

Write-Host 'Titan dependency check'
Write-Host "  Manifest: $manifestPath"

$dotnet = Find-DotNet
$dotnetVersion = & $dotnet --version
Write-Host "  dotnet:   $dotnetVersion ($dotnet)"

$msbuild = Find-Vs2022MsBuild
Import-VsDevEnvironment $msbuild
Write-Host "  MSBuild:  $msbuild"

try {
    $llvmBin = Find-VsLlvmBin $msbuild
    Add-PathEntry $llvmBin
    Write-Host "  libclang: $llvmBin"
}
catch {
    Add-Missing $_.Exception.Message
}

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
