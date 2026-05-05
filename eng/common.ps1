# Shared helpers for Windows build orchestration.
Set-StrictMode -Version Latest

function Get-RepoRoot {
    $root = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')
    return $root.ProviderPath
}

function Join-RepoPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return Join-Path (Get-RepoRoot) $Path
}

function Assert-WindowsHost {
    $isWindowsVariable = Get-Variable -Name IsWindows -ErrorAction SilentlyContinue
    if ($null -ne $isWindowsVariable -and -not $isWindowsVariable.Value) {
        throw 'This build entry point is Windows-only.'
    }
}

function Find-DotNet {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        throw 'dotnet was not found on PATH. Install the .NET 8 SDK.'
    }

    return $dotnet.Source
}

function Find-NuGet {
    $nuget = Get-Command nuget -ErrorAction SilentlyContinue
    if ($null -ne $nuget) {
        return $nuget.Source
    }

    $toolsRoot = Join-Path $env:LOCALAPPDATA 'TitanBuildTools'
    $nugetPath = Join-Path $toolsRoot 'nuget.exe'
    if (-not (Test-Path -LiteralPath $nugetPath)) {
        New-Item -ItemType Directory -Force -Path $toolsRoot | Out-Null
        $nugetUri = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
        Write-Host "Downloading NuGet CLI to $nugetPath"
        Invoke-WebRequest -Uri $nugetUri -OutFile $nugetPath
    }

    return (Resolve-Path -LiteralPath $nugetPath).ProviderPath
}

function Find-Vs2022MsBuild {
    if ($env:MSBUILD_EXE -and (Test-Path -LiteralPath $env:MSBUILD_EXE)) {
        return (Resolve-Path -LiteralPath $env:MSBUILD_EXE).ProviderPath
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}
    if (-not $programFilesX86) {
        throw 'ProgramFiles(x86) is not set; cannot locate Visual Studio.'
    }

    $vswhere = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw "vswhere.exe was not found at '$vswhere'. Install Visual Studio 2022 or Build Tools 2022."
    }

    $commonArgs = @(
        '-latest',
        '-products', '*',
        '-version', '[17.0,18.0)',
        '-find', 'MSBuild\**\Bin\MSBuild.exe'
    )

    $msbuild = & $vswhere @commonArgs '-requires' 'Microsoft.Component.MSBuild' '-requires' 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' | Select-Object -First 1
    if (-not $msbuild) {
        $msbuild = & $vswhere @commonArgs '-requires' 'Microsoft.Component.MSBuild' | Select-Object -First 1
    }

    if (-not $msbuild -or -not (Test-Path -LiteralPath $msbuild)) {
        throw 'Visual Studio 2022 MSBuild was not found. Install VS 2022 with MSBuild and Desktop development with C++.'
    }

    return (Resolve-Path -LiteralPath $msbuild).ProviderPath
}

function Get-VsInstallRootFromMsBuild {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath
    )

    $msbuildBin = Split-Path -Parent $MSBuildPath
    $msbuildCurrent = Split-Path -Parent $msbuildBin
    $msbuildRoot = Split-Path -Parent $msbuildCurrent
    return Split-Path -Parent $msbuildRoot
}

function Import-VsDevEnvironment {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath
    )

    $vsInstallRoot = Get-VsInstallRootFromMsBuild $MSBuildPath
    $vsDevCmd = Join-Path $vsInstallRoot 'Common7\Tools\VsDevCmd.bat'
    if (-not (Test-Path -LiteralPath $vsDevCmd)) {
        throw "VsDevCmd.bat was not found at '$vsDevCmd'."
    }

    $environment = & cmd.exe /d /s /c "`"$vsDevCmd`" -no_logo -arch=x64 -host_arch=x64 && set"
    foreach ($line in $environment) {
        if ($line -match '^(.*?)=(.*)$') {
            Set-Item -Path ('Env:' + $matches[1]) -Value $matches[2]
        }
    }
}

function Find-VsLlvmBin {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath
    )

    $vsInstallRoot = Get-VsInstallRootFromMsBuild $MSBuildPath
    $candidates = @(
        (Join-Path $vsInstallRoot 'VC\Tools\Llvm\x64\bin'),
        (Join-Path $vsInstallRoot 'VC\Tools\Llvm\bin')
    )

    foreach ($candidate in $candidates) {
        $libclang = Join-Path $candidate 'libclang.dll'
        if (Test-Path -LiteralPath $libclang) {
            return (Resolve-Path -LiteralPath $candidate).ProviderPath
        }
    }

    throw "VS LLVM libclang.dll was not found under '$vsInstallRoot\VC\Tools\Llvm'. Install the Visual Studio LLVM/Clang tools component."
}

function Add-PathEntry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolved = (Resolve-Path -LiteralPath $Path).ProviderPath
    $entries = $env:PATH -split ';'
    if ($entries -notcontains $resolved) {
        $env:PATH = $resolved + ';' + $env:PATH
    }
}

function Invoke-External {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [string[]]$Arguments = @(),

        [string]$WorkingDirectory = (Get-RepoRoot)
    )

    $display = @($FilePath) + $Arguments
    Write-Host ''
    Write-Host ">> $($display -join ' ')"

    Push-Location -LiteralPath $WorkingDirectory
    try {
        & $FilePath @Arguments
        $exitCode = $LASTEXITCODE
        if ($null -ne $exitCode -and $exitCode -ne 0) {
            throw "Command failed with exit code $exitCode`: $($display -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

function Test-RepoFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $fullPath = Join-RepoPath $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "Required file was not found: $fullPath"
    }

    return $fullPath
}
