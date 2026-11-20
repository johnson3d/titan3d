#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',

    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\..\common\common.ps1"

Assert-WindowsHost

$repoRoot = Get-RepoRoot
$cmake = Find-CMake
$msbuild = Find-VsMsBuild
Import-VsDevEnvironment $msbuild

# 显式钉住 MSVC 工具集：CMake 的 "Visual Studio 18 2026" 生成器默认仍选 v143，
# 不传 -T 就得靠额外安装 VS 2022 工具集才编得过。
$vcToolset = 'v145'
$cmakeGenerator = 'Visual Studio 18 2026'

$runtimeRoot = Test-RepoFile '3rd\native\LeanCLR\runtime\CMakeLists.txt' | Split-Path -Parent
$buildRoot = Join-Path $repoRoot 'intermediate\leanclr\windows\x64'
$outputRoot = Join-Path $repoRoot '3rd\native\LeanCLR\lib\windows\x64'
$configurations = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }

function Test-LeanClrOutputIsCurrent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputLibrary
    )

    if (-not (Test-Path -LiteralPath $OutputLibrary)) {
        return $false
    }

    $outputTime = (Get-Item -LiteralPath $OutputLibrary).LastWriteTimeUtc
    $newerSource = Get-ChildItem -LiteralPath $runtimeRoot -Recurse -File |
        Where-Object { $_.LastWriteTimeUtc -gt $outputTime } |
        Select-Object -First 1
    return $null -eq $newerSource
}

foreach ($config in $configurations) {
    $gcDebug = if ($config -eq 'Debug') { 'ON' } else { 'OFF' }
    $msvcRuntime = if ($config -eq 'Debug') { 'MultiThreadedDebug' } else { 'MultiThreaded' }
    $buildDirectory = Join-Path $buildRoot $config
    $outputDirectory = Join-Path $outputRoot $config
    $outputLibrary = Join-Path $outputDirectory 'leanclr.lib'

    if (-not $Force -and (Test-LeanClrOutputIsCurrent $outputLibrary)) {
        Write-Host "LeanCLR $config library is current: $outputLibrary"
        continue
    }

    # 生成器/工具集变更后旧缓存会让 cmake 直接报错或静默沿用旧工具集，这里自动清理。
    $cmakeCache = Join-Path $buildDirectory 'CMakeCache.txt'
    if (Test-Path -LiteralPath $cmakeCache) {
        $cachedGenerator = Select-String -LiteralPath $cmakeCache -Pattern '^CMAKE_GENERATOR:INTERNAL=(.+)$' |
            Select-Object -First 1
        $cachedToolset = Select-String -LiteralPath $cmakeCache -Pattern '^CMAKE_GENERATOR_TOOLSET:INTERNAL=(.+)$' |
            Select-Object -First 1
        $generator = if ($cachedGenerator) { $cachedGenerator.Matches[0].Groups[1].Value.Trim() } else { '' }
        $toolset = if ($cachedToolset) { $cachedToolset.Matches[0].Groups[1].Value.Trim() } else { '' }
        if ($generator -ne $cmakeGenerator -or $toolset -ne $vcToolset) {
            Write-Host "Removing stale CMake cache (generator='$generator', toolset='$toolset'): $buildDirectory"
            Remove-Item -LiteralPath $buildDirectory -Recurse -Force
        }
    }

    Write-Host "== Build LeanCLR Windows x64 $config =="
    Invoke-External $cmake @(
        '-S', $runtimeRoot,
        '-B', $buildDirectory,
        '-G', $cmakeGenerator,
        '-A', 'x64',
        '-T', $vcToolset,
        ('-DCMAKE_MSVC_RUNTIME_LIBRARY=' + $msvcRuntime),
        ('-DLEANCLR_GC_DEBUG=' + $gcDebug),
        '-DBUILD_SHARED_LEANCLR=OFF'
    ) $repoRoot
    Invoke-External $cmake @(
        '--build', $buildDirectory,
        '--config', $config,
        '--target', 'leanclr',
        '--parallel'
    ) $repoRoot

    $builtLibrary = Join-Path $buildDirectory ($config + '\leanclr.lib')
    if (-not (Test-Path -LiteralPath $builtLibrary)) {
        throw "LeanCLR $config build did not produce '$builtLibrary'."
    }

    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
    Copy-Item -LiteralPath $builtLibrary -Destination $outputLibrary -Force
    Write-Host "LeanCLR $config library written to $outputLibrary"
}
