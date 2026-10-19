#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',

    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\common.ps1"

Assert-WindowsHost

$repoRoot = Get-RepoRoot
$cmake = Find-CMake
$msbuild = Find-Vs2022MsBuild
Import-VsDevEnvironment $msbuild

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

    Write-Host "== Build LeanCLR Windows x64 $config =="
    Invoke-External $cmake @(
        '-S', $runtimeRoot,
        '-B', $buildDirectory,
        '-G', 'Visual Studio 17 2022',
        '-A', 'x64',
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
