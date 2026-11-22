# Shared helpers for Windows build orchestration.
Set-StrictMode -Version Latest

function Get-RepoRoot {
    $root = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')
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
        throw 'dotnet was not found on PATH. Install the .NET SDK pinned in global.json.'
    }

    return $dotnet.Source
}

function Get-TitanTargetFramework {
    $depsJson = Join-Path (Get-RepoRoot) 'BuildScript\common\dependencies.json'
    $manifest = Get-Content -LiteralPath $depsJson -Raw | ConvertFrom-Json
    $tfm = $manifest.dotnetSdk.targetFramework
    if ([string]::IsNullOrWhiteSpace($tfm)) {
        throw "dotnetSdk.targetFramework is missing or empty in '$depsJson'."
    }

    return $tfm
}

function Get-JscfgNativeDll {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return ''
    }

    $text = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($text)) {
        return ''
    }

    # jscfg 是扁平 json，NativeDll 只在顶层出现一次，与 TitanNativeCore.props 用同一个正则。
    $match = [regex]::Match($text, '"NativeDll"\s*:\s*"([^"]*)"')
    if (-not $match.Success) {
        return ''
    }

    return $match.Groups[1].Value.Trim()
}

# native Core.Window 的编译配置由 jscfg 的 NativeDll 决定，规则与 BuildScript\msbuild\TitanNativeCore.props 一致：
#   显式覆盖 > cache/config/engine.jscfg > content/config/engineconfig_windows_<BaseConfig>.jscfg > release
# 这样命令行构建与 VS 内构建产出同一个 native 产物，不再跟随 -Configuration。
function Resolve-TitanNativeConfiguration {
    param(
        [string]$Override = '',
        [string]$BaseConfig = 'dx12'
    )

    $repoRoot = Get-RepoRoot
    $cacheJscfg = Join-Path $repoRoot 'cache\config\engine.jscfg'
    $baseJscfg = Join-Path $repoRoot ('content\config\engineconfig_windows_' + $BaseConfig + '.jscfg')

    $nativeDll = $Override.Trim()
    $origin = 'command line override'
    if ([string]::IsNullOrWhiteSpace($nativeDll)) {
        $nativeDll = Get-JscfgNativeDll $cacheJscfg
        $origin = 'cache\config\engine.jscfg'
    }
    if ([string]::IsNullOrWhiteSpace($nativeDll)) {
        $nativeDll = Get-JscfgNativeDll $baseJscfg
        $origin = 'content\config\engineconfig_windows_' + $BaseConfig + '.jscfg'
    }
    if ([string]::IsNullOrWhiteSpace($nativeDll)) {
        $nativeDll = 'release'
        $origin = 'default'
    }

    $nativeDll = $nativeDll.Trim().ToLowerInvariant()
    $configuration = 'Release'
    switch ($nativeDll) {
        'debug' { $configuration = 'Debug' }
        'release' { $configuration = 'Release' }
        'auto' {
            throw "NativeDll='auto' ($origin) cannot be resolved at build time: the runtime picks it from the command line. Pass -NativeDll Debug|Release or write a concrete value into cache/config/engine.jscfg."
        }
        default {
            throw "NativeDll='$nativeDll' ($origin) is not one of debug/release/auto."
        }
    }

    return [pscustomobject]@{
        Configuration = $configuration
        NativeDll     = $nativeDll
        Origin        = $origin
    }
}

function Find-CMake {
    $cmake = Get-Command cmake -ErrorAction SilentlyContinue
    if ($null -eq $cmake) {
        throw 'CMake was not found on PATH. Install CMake 3.15 or newer to build LeanCLR from source.'
    }

    return $cmake.Source
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

# VS 的可用版本区间由 dependencies.json 声明，避免 vswhere 硬编码与清单漂移。
function Get-TitanVsVersionRange {
    $depsJson = Join-Path (Get-RepoRoot) 'BuildScript\common\dependencies.json'
    $manifest = Get-Content -LiteralPath $depsJson -Raw | ConvertFrom-Json
    $versionRange = $manifest.visualStudio.msbuildVersionRange
    if ([string]::IsNullOrWhiteSpace($versionRange)) {
        throw "visualStudio.msbuildVersionRange is missing or empty in '$depsJson'."
    }

    return $versionRange
}

function Find-VsMsBuild {
    if ($env:MSBUILD_EXE -and (Test-Path -LiteralPath $env:MSBUILD_EXE)) {
        return (Resolve-Path -LiteralPath $env:MSBUILD_EXE).ProviderPath
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}
    if (-not $programFilesX86) {
        throw 'ProgramFiles(x86) is not set; cannot locate Visual Studio.'
    }

    $vswhere = Join-Path $programFilesX86 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw "vswhere.exe was not found at '$vswhere'. Install Visual Studio 2026 or Build Tools 2026."
    }

    $versionRange = Get-TitanVsVersionRange
    $commonArgs = @(
        '-latest',
        '-products', '*',
        '-version', $versionRange,
        '-find', 'MSBuild\**\Bin\MSBuild.exe'
    )

    $msbuild = & $vswhere @commonArgs '-requires' 'Microsoft.Component.MSBuild' '-requires' 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' | Select-Object -First 1
    if (-not $msbuild) {
        $msbuild = & $vswhere @commonArgs '-requires' 'Microsoft.Component.MSBuild' | Select-Object -First 1
    }

    if (-not $msbuild -or -not (Test-Path -LiteralPath $msbuild)) {
        throw "Visual Studio MSBuild was not found in range $versionRange. Install Visual Studio 2026 with MSBuild and Desktop development with C++."
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

# native PDB 是 MSF 7.00 格式。构建被强杀（Ctrl+C、VS 取消、进程崩溃）时可能留下写了一半的文件，
# 之后每次增量构建都会因为读不出符号而失败（MSB4166，或 MSB4018 + "No 'name' stream"）。
# MSF 的不变量是“文件长度 == BlockSize * NumBlocks”，据此可以直接判定文件是否被写坏。
function Test-NativePdbComplete {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    try {
        $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    }
    catch {
        # 被 VS / 调试器占着时读不到，不能据此判定文件损坏。
        return $true
    }

    try {
        $length = $stream.Length
        if ($length -lt 56) {
            return $false
        }

        $header = New-Object byte[] 56
        $read = 0
        while ($read -lt $header.Length) {
            $chunk = $stream.Read($header, $read, $header.Length - $read)
            if ($chunk -le 0) {
                break
            }
            $read += $chunk
        }
        if ($read -lt $header.Length) {
            return $false
        }

        # 非 MSF 头（例如 DebugType=portable 的 BSJB 容器）不在本函数的判定范围内，宁可放过。
        $magic = [System.Text.Encoding]::ASCII.GetString($header, 0, 32)
        if (-not $magic.StartsWith('Microsoft C/C++ MSF 7.00', [System.StringComparison]::Ordinal)) {
            return $true
        }

        $blockSize = [System.BitConverter]::ToUInt32($header, 32)
        $numBlocks = [System.BitConverter]::ToUInt32($header, 40)
        if ($blockSize -eq 0) {
            return $false
        }

        return ([uint64]$numBlocks * [uint64]$blockSize -eq [uint64]$length)
    }
    catch {
        # 读不出来就交给编译器自己报错，不在这里误删。
        return $true
    }
    finally {
        $stream.Dispose()
    }
}

# 删除被中断构建写坏的符号文件，返回修复的 PDB 数量。
# obj\<Config>\<TFM> 下的 PDB 全部是编译器产物，坏了只会让后续构建崩；重建即可恢复。
function Repair-CorruptSymbols {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$BuildConfiguration
    )

    $projectDirectory = Split-Path -Parent $ProjectPath
    $pattern = Join-Path $projectDirectory ('obj\' + $BuildConfiguration + '\*\*.pdb')
    $repaired = 0

    foreach ($pdb in @(Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue)) {
        if (Test-NativePdbComplete $pdb.FullName) {
            continue
        }

        # PDB 和同目录的中间程序集必须成对重出：只删 PDB 时编译器可能判定无改动而跳过编译，符号会一直缺着。
        $assembly = [System.IO.Path]::ChangeExtension($pdb.FullName, '.dll')
        try {
            Remove-Item -LiteralPath $pdb.FullName -Force
            if (Test-Path -LiteralPath $assembly) {
                Remove-Item -LiteralPath $assembly -Force
            }
            Write-Host "  Repaired corrupt symbols: $($pdb.FullName) (+ $(Split-Path -Leaf $assembly))"
            $repaired++
        }
        catch {
            Write-Warning "Could not repair '$($pdb.FullName)': $($_.Exception.Message)"
        }
    }

    return $repaired
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
