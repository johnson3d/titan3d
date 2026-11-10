#requires -Version 5.1

# Android toolchain locator: only reads the local paths the user filled in the config file; never auto-downloads or guesses versions.
# This file must run under Windows PowerShell 5.1 ConstrainedLanguage:
# it must not use non-core types such as [IO.Path], Diagnostics.Process, Text.Encoding, Cryptography.

Set-StrictMode -Version Latest

$script:AndroidToolchainConfigRelativePath = 'cache\config\android-toolchain.json'
$script:AndroidToolchainExampleRelativePath = 'BuildScript\android\AndroidToolchain.example.json'
# Default CWD of child processes spawned by Invoke-AndroidProcess: the host script pins it to the repo root (see comments inside Invoke-AndroidProcess).
$script:AndroidProcessWorkingDirectory = ''

# Static contract of the ELF gate: real devices only guarantee these system libraries, and 16 KB page alignment is a load requirement on Android 15.
$script:AndroidAllowedNeededLibraries = @('libc.so', 'libm.so', 'libdl.so', 'liblog.so', 'libandroid.so', 'libvulkan.so')
$script:AndroidForbiddenExports = @(
    '_Znwm', '_Znam', '_ZdlPv', '_ZdlPvm', '_ZdaPv', '_ZdaPvm',
    '_ZnwmSt11align_val_t', '_ZnamSt11align_val_t', '_ZdlPvSt11align_val_t', '_ZdlPvmSt11align_val_t',
    '_ZdaPvSt11align_val_t', '_ZdaPvmSt11align_val_t'
)
$script:AndroidRequiredLoadAlignment = 16384

function Get-AndroidToolchainConfigPath {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$ConfigPath = ''
    )

    if (-not [string]::IsNullOrWhiteSpace($ConfigPath)) {
        return (Resolve-AndroidToolPath -RepositoryRoot $RepositoryRoot -Path $ConfigPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($env:TITAN_ANDROID_TOOLCHAIN_CONFIG)) {
        return (Resolve-AndroidToolPath -RepositoryRoot $RepositoryRoot -Path $env:TITAN_ANDROID_TOOLCHAIN_CONFIG)
    }

    return Join-Path $RepositoryRoot $script:AndroidToolchainConfigRelativePath
}

function Get-AndroidToolchainConfigHint {
    param([Parameter(Mandatory = $true)][string]$ConfigPath)

    return "Edit '$ConfigPath'. Local config: '$($script:AndroidToolchainConfigRelativePath)' (copy the template '$($script:AndroidToolchainExampleRelativePath)')."
}

function Get-AndroidConfigProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $Object -or -not ($Object.PSObject.Properties.Name -contains $Name)) {
        return $null
    }

    return $Object.$Name
}

# Relative paths are always resolved against the repo root (same rule for config files and tool directories), never following the caller's current directory.
# Environment variables are not expanded; drive-relative paths like 'C:foo' are rejected outright because they implicitly hit the current directory.
function Resolve-AndroidToolPath {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $expanded = $Path.Trim()
    if ($expanded.Length -eq 0) {
        return $expanded
    }

    if ($expanded -match '^[A-Za-z]:$') {
        return $expanded + '\'
    }

    $isAbsolute = ($expanded.Length -ge 2 -and $expanded[1] -eq ':') -or
        ($expanded.Length -ge 2 -and $expanded[0] -eq '\' -and $expanded[1] -eq '\')

    if ($isAbsolute) {
        if ($expanded -match '^[A-Za-z]:[^\\/]') {
            throw "Path '$Path' is a drive-relative path, which is not supported. Use an absolute path like 'C:\dir' or a path relative to the repository root."
        }
        if ($expanded.Length -le 3) {
            return $expanded
        }
        return $expanded.TrimEnd('\')
    }

    return (Join-Path $RepositoryRoot $expanded).TrimEnd('\')
}

# Output is read via Start-Process + temp files: under ConstrainedLanguage we avoid Process objects,
# and we do not depend on the caller PowerShell's stdout redirection. .cmd/.bat wrappers are executed via cmd.exe.
function Invoke-AndroidToolCapture {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [string[]]$Arguments = @()
    )

    if ([string]::IsNullOrWhiteSpace($env:TEMP)) {
        throw "Cannot run '$File' because TEMP is not set; the tool output cannot be captured."
    }

    $stamp = "titan-android-{0}-{1}" -f $PID, (Get-Random)
    $outFile = Join-Path $env:TEMP ($stamp + '.out')
    $errFile = Join-Path $env:TEMP ($stamp + '.err')

    $extension = ''
    if ($File.Length -ge 4) {
        $extension = $File.Substring($File.Length - 4).ToLowerInvariant()
    }
    $isWrapper = ($extension -eq '.cmd' -or $extension -eq '.bat')

    $commandFile = $File
    $commandArguments = $Arguments
    if ($isWrapper) {
        # cmd needs /s combined with a fully paired quoted command line: ""<path>" "<args>"".
        # Only then do paths containing spaces, (x86), or & avoid being split or truncated.
        $commandFile = $env:ComSpec
        $inner = '""' + $File + '"'
        foreach ($argument in $Arguments) {
            $inner += ' "' + $argument + '"'
        }
        $inner += '"'
        $commandArguments = @('/d', '/s', '/c', $inner)
    }

    $process = $null
    try {
        # Must use -Wait -PassThru: ConstrainedLanguage forbids WaitForExit/Refresh,
        # and without -Wait $process.ExitCode reads back as empty. The cost is no internal timeout;
        # if a tool hangs it must be killed manually (the error message reminds the user to rerun by hand).
        try {
            $process = Start-Process -FilePath $commandFile -ArgumentList $commandArguments `
                -RedirectStandardOutput $outFile -RedirectStandardError $errFile `
                -NoNewWindow -Wait -PassThru
        }
        catch {
            throw "Failed to start '$File': $($_.Exception.Message)"
        }

        $code = $process.ExitCode
        # Console program output is decoded with the OEM code page, otherwise errors containing non-ASCII text become '???'.
        $text = ''
        if (Test-Path -LiteralPath $outFile) {
            $text = (Get-Content -Raw -LiteralPath $outFile -Encoding Oem)
        }
        if (Test-Path -LiteralPath $errFile) {
            $text += (Get-Content -Raw -LiteralPath $errFile -Encoding Oem)
        }
        if ($null -eq $text) {
            $text = ''
        }
        return @{ Code = $code; Output = $text }
    }
    finally {
        if (Test-Path -LiteralPath $outFile) { Remove-Item -LiteralPath $outFile -Force }
        if (Test-Path -LiteralPath $errFile) { Remove-Item -LiteralPath $errFile -Force }
    }
}

# Convert arguments into a form that is safe inside a batch file.
# Only quote when necessary: once cmd's own switches (/d /s /c) are wrapped in quotes they are no longer recognized.
# Empirically '"exe" "/d" "/s" "/c" "..."' fails outright with "The filename, directory name, or volume label syntax is incorrect".
# When quoting is required, escape per CRT rules (double the backslashes before a quote, double trailing backslashes); %
# must be doubled whether quoted or not, otherwise the batch treats it as a variable to expand.
function ConvertTo-AndroidBatchArgument {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)

    $text = [regex]::Replace($Value, '%', '%%')
    if ($text.Length -eq 0) {
        return '""'
    }

    # Spaces, quotes, cmd metacharacters and trailing backslashes all require quoting; everything else stays bare.
    if ($text -notmatch '[\s"&|<>^()]' -and $text -notmatch '\\$') {
        return $text
    }

    $text = [regex]::Replace($text, '(\\*)"', '$1$1\"')
    $text = [regex]::Replace($text, '(\\+)$', '$1$1')
    return '"' + $text + '"'
}

# Run an external command with a bounded duration. ConstrainedLanguage forbids New-Object Diagnostics.Process and WaitForExit,
# so a .cmd wrapper writes stdout/stderr/exit code into temp files, and we poll HasExited to implement the timeout.
# Do not use Wait-Process and then read ExitCode: empirically $process.ExitCode is still empty that way.
function Invoke-AndroidProcess {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [string[]]$Arguments = @(),
        [int]$TimeoutMs = 1800000
    )

    if ($TimeoutMs -le 0) {
        throw "Refusing to run '$File': the timeout budget is $TimeoutMs ms. Pass a positive -TimeoutMs."
    }
    if (-not (Test-Path -LiteralPath $File -PathType Leaf)) {
        throw "Cannot run '$File': the file does not exist."
    }
    if ([string]::IsNullOrWhiteSpace($env:TEMP)) {
        throw "Cannot run '$File' because TEMP is not set; the command output cannot be captured."
    }

    $directory = Join-Path $env:TEMP ("titan-android-proc-{0}-{1}" -f $PID, (Get-Random))
    New-Item -ItemType Directory -Force -Path $directory | Out-Null

    $wrapper = Join-Path $directory 'run.cmd'
    # Output files must use absolute paths: after the child CWD is pinned to the repo root, relative redirects would write there.
    $stdoutPath = Join-Path $directory 'stdout.txt'
    $stderrPath = Join-Path $directory 'stderr.txt'
    $exitPath = Join-Path $directory 'exit.txt'
    $line = (ConvertTo-AndroidBatchArgument $File) + ' '
    foreach ($argument in $Arguments) {
        $line += (ConvertTo-AndroidBatchArgument $argument) + ' '
    }
    # Parentheses are mandatory: in PowerShell comma binds tighter than '+', so the line would split into two array elements.
    $lines = @(
        '@echo off',
        ($line + '1>' + (ConvertTo-AndroidBatchArgument $stdoutPath) + ' 2>' + (ConvertTo-AndroidBatchArgument $stderrPath)),
        # There must be a space between '%ERRORLEVEL%' and '>', otherwise the file is written empty.
        ('echo %ERRORLEVEL% > ' + (ConvertTo-AndroidBatchArgument $exitPath))
    )
    # Must be Default (ANSI) encoding: ASCII would turn non-ASCII usernames into '??' and break the path.
    Set-Content -LiteralPath $wrapper -Value ($lines -join "`r`n") -Encoding Default

    # Default working directory of the child process: the host script (e.g. build-android.ps1) pins it to the repo root.
    # It must not stay in TEMP: dotnet picks an SDK by walking up from the process CWD looking for global.json; in TEMP none is found
    # and it silently selects the newest SDK (e.g. an rc preview), causing version drift and errors like missing MSBuild.exe inside the SDK.
    $cwd = $script:AndroidProcessWorkingDirectory
    if ([string]::IsNullOrWhiteSpace($cwd) -or -not (Test-Path -LiteralPath $cwd -PathType Container)) {
        $cwd = (Get-Location).ProviderPath
    }

    $process = $null
    try {
        try {
            $process = Start-Process -FilePath $wrapper -WorkingDirectory $cwd -NoNewWindow -PassThru
        }
        catch {
            throw "Failed to start '$File': $($_.Exception.Message)"
        }

        $remaining = $TimeoutMs
        while (-not $process.HasExited) {
            if ($remaining -le 0) {
                # A timeout must kill the whole tree: otherwise grandchildren like adb stay in the background and block the next build.
                & taskkill.exe /T /F /PID $process.Id 2>&1 | Out-Null
                throw "Command '$File' exceeded the timeout budget of $TimeoutMs ms; the process tree was terminated."
            }
            $slice = 200
            if ($remaining -lt 2000) { $slice = 50 }
            Start-Sleep -Milliseconds $slice
            $remaining -= $slice
        }

        $codeText = ''
        $codePath = $exitPath
        if (Test-Path -LiteralPath $codePath -PathType Leaf) {
            $raw = Get-Content -Raw -LiteralPath $codePath -Encoding Oem
            if ($null -ne $raw) { $codeText = $raw.Trim() }
        }
        if ($codeText -notmatch '^-?[0-9]+$') {
            throw "Command '$File' finished but did not report an exit code (wrapper '$wrapper'). Check that '$File' can be started from cmd.exe."
        }

        # Console program output is decoded with the OEM code page, otherwise errors containing non-ASCII text become '???'.
        $text = ''
        foreach ($name in @($stdoutPath, $stderrPath)) {
            $path = $name
            if (Test-Path -LiteralPath $path -PathType Leaf) {
                $part = Get-Content -Raw -LiteralPath $path -Encoding Oem
                if ($null -ne $part) { $text += $part }
            }
        }

        return @{ Code = [int]$codeText; Output = $text }
    }
    finally {
        if (Test-Path -LiteralPath $directory) {
            Remove-Item -LiteralPath $directory -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

# TitanCodegenGen.cmake wraps long tasks (NuGet restore, tool compilation, etc.) with 'bash progress-run.sh'.
# On Windows Git Bash is often installed but not on PATH (especially for processes launched by the editor);
# here we locate bash via PATH then common Git install locations and prepend its directory to PATH, erroring with install guidance if not found.
function Enable-AndroidBashOnPath {
    param([string]$Stage = 'Native')

    $resolved = Get-Command bash -ErrorAction SilentlyContinue
    if ($null -ne $resolved) {
        Write-Host "[TITAN-CHECK] OK bash - $($resolved.Source)"
        return
    }
    $candidates = @(
        (Join-Path $env:ProgramFiles 'Git\bin'),
        (Join-Path $env:ProgramFiles 'Git\usr\bin')
    )
    if (-not [string]::IsNullOrWhiteSpace(${env:ProgramFiles(x86)})) {
        $candidates += (Join-Path ${env:ProgramFiles(x86)} 'Git\bin')
    }
    if (-not [string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
        $candidates += (Join-Path $env:LOCALAPPDATA 'Programs\Git\bin')
    }
    foreach ($dir in $candidates) {
        if (Test-Path -LiteralPath (Join-Path $dir 'bash.exe') -PathType Leaf) {
            $env:PATH = "$dir;$env:PATH"
            Write-Host "[TITAN-CHECK] OK bash - $dir\bash.exe (not on original PATH, prepended)"
            return
        }
    }
    throw "[$Stage] bash not found: TitanCodegen needs bash to run the progress wrapper script. Install Git for Windows (https://git-scm.com/download/win) or add Git\bin to PATH, then retry."
}

# Default entry point for external commands: on failure, throw with the stage, exit code and output tail together,
# so the caller can tell which stage failed without digging through logs.
function Invoke-AndroidCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Stage,
        [Parameter(Mandatory = $true)][string]$File,
        [string[]]$Arguments = @(),
        [int]$TimeoutMs = 1800000
    )

    # Print a machine-readable progress line before each sub-step (the panel log shows fine-grained progress and where it stalled on failure).
    $displayArgs = if ($Arguments.Count -gt 0) { $Arguments[0] } else { '' }
    Write-Host ("[TITAN-STEP] {0} - {1} {2}" -f $Stage, (Split-Path $File -Leaf), $displayArgs)
    try {
        $result = Invoke-AndroidProcess -File $File -Arguments $Arguments -TimeoutMs $TimeoutMs
    }
    catch {
        throw "[$Stage] $($_.Exception.Message)"
    }

    if ($result.Code -ne 0) {
        $output = $result.Output.Trim()
        if ($output.Length -gt 4000) {
            $output = $output.Substring($output.Length - 4000)
        }
        throw "[$Stage] Command failed (exit $($result.Code)): `"$File`" $($Arguments -join ' ')`n$output"
    }

    return $result
}

# ELF gate: the artifact must satisfy architecture, 16 KB page alignment, the NEEDED whitelist, and no exported C++ operator,
# otherwise it either fails to install on a real device or fights other .so files over symbols.
function Test-AndroidNativeLibrary {
    param(
        [Parameter(Mandatory = $true)][hashtable]$Tools,
        [Parameter(Mandatory = $true)][string]$Library
    )

    if (-not (Test-Path -LiteralPath $Library -PathType Leaf)) {
        throw "Android native library was not produced: '$Library'. The native build step did not emit the expected .so."
    }

    $readelf = Join-Path $Tools.LlvmBin 'llvm-readelf.exe'
    $nm = Join-Path $Tools.LlvmBin 'llvm-nm.exe'
    foreach ($tool in @($readelf, $nm)) {
        if (-not (Test-Path -LiteralPath $tool -PathType Leaf)) {
            throw "Cannot gate '$Library' because the NDK tool '$tool' is missing. Reinstall the configured NDK."
        }
    }

    $header = Invoke-AndroidProcess -File $readelf -Arguments @('-h', $Library) -TimeoutMs 120000
    if ($header.Code -ne 0) {
        throw "llvm-readelf -h failed on '$Library' (exit $($header.Code)): $($header.Output.Trim())"
    }
    $machine = ''
    foreach ($line in ($header.Output -split "`n")) {
        if ($line -match '^\s*Machine:\s*(.+?)\s*$') { $machine = $Matches[1] }
    }
    if ($machine.Length -eq 0) {
        throw "Could not read the ELF machine type from '$Library'; the file is not a valid ELF object."
    }
    if ($machine -notmatch 'AArch64') {
        throw "'$Library' is built for '$machine', but the Android arm64 pipeline expects AArch64. Rebuild with --target=aarch64-linux-android<apiLevel>."
    }

    $programHeaders = Invoke-AndroidProcess -File $readelf -Arguments @('-W', '-l', $Library) -TimeoutMs 120000
    if ($programHeaders.Code -ne 0) {
        throw "llvm-readelf -l failed on '$Library' (exit $($programHeaders.Code)): $($programHeaders.Output.Trim())"
    }
    $loadAlignments = @()
    foreach ($line in ($programHeaders.Output -split "`n")) {
        if ($line -match '^\s*LOAD\s') {
            $parts = @($line.Trim() -split '\s+')
            $loadAlignments += $parts[$parts.Count - 1]
        }
    }
    if ($loadAlignments.Count -eq 0) {
        throw "No LOAD segment was found in '$Library'; the file is not a loadable shared object."
    }
    foreach ($alignment in $loadAlignments) {
        $value = 0
        try { $value = [int]$alignment }
        catch { throw "'$Library' has an unreadable LOAD segment alignment '$alignment'." }
        if ($value -lt $script:AndroidRequiredLoadAlignment) {
            throw "'$Library' has a LOAD segment aligned to $value bytes, but 16 KB page devices need 16 KB alignment. Build with -Wl,-z,max-page-size=16384 (CMake: set ANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON)."
        }
    }

    $dynamic = Invoke-AndroidProcess -File $readelf -Arguments @('-d', $Library) -TimeoutMs 120000
    if ($dynamic.Code -ne 0) {
        throw "llvm-readelf -d failed on '$Library' (exit $($dynamic.Code)): $($dynamic.Output.Trim())"
    }
    $needed = @()
    foreach ($line in ($dynamic.Output -split "`n")) {
        if ($line -match '\(NEEDED\)\s+Shared library: \[(.+?)\]') { $needed += $Matches[1] }
    }
    foreach ($entry in $needed) {
        if ($script:AndroidAllowedNeededLibraries -notcontains $entry) {
            throw "'$Library' depends on '$entry', which is not an allowed Android system library (allowed: $($script:AndroidAllowedNeededLibraries -join ', ')). Link it statically or drop the dependency."
        }
    }

    $symbols = Invoke-AndroidProcess -File $nm -Arguments @('-D', '--defined-only', $Library) -TimeoutMs 120000
    if ($symbols.Code -ne 0) {
        throw "llvm-nm -D --defined-only failed on '$Library' (exit $($symbols.Code)): $($symbols.Output.Trim())"
    }
    $exported = @{}
    foreach ($line in ($symbols.Output -split "`n")) {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0) { continue }
        $parts = @($trimmed -split '\s+')
        if ($parts.Count -ge 3) { $exported[$parts[$parts.Count - 1]] = $true }
    }
    foreach ($forbidden in $script:AndroidForbiddenExports) {
        if ($exported.ContainsKey($forbidden)) {
            throw "'$Library' exports '$forbidden', a C++ operator that c++_static must own. Do not define a global operator new/delete in engine code."
        }
    }

    return @{
        Library   = $Library
        Machine   = $machine
        Needed    = $needed
        LoadAlign = $loadAlignments
    }
}

function Get-AndroidNdkRevision {
    param([Parameter(Mandatory = $true)][string]$Ndk)

    $properties = Join-Path $Ndk 'source.properties'
    if (-not (Test-Path -LiteralPath $properties -PathType Leaf)) {
        return $null
    }

    foreach ($line in (Get-Content -LiteralPath $properties -Encoding UTF8)) {
        if ($line -match '^\s*Pkg\.Revision\s*=\s*([0-9][0-9.]*)\s*$') {
            return $Matches[1]
        }
    }

    return $null
}

function Get-AndroidCmakeVersion {
    param(
        [Parameter(Mandatory = $true)][string]$CMake,
        [string]$Hint = ''
    )

    $result = Invoke-AndroidToolCapture -File $CMake -Arguments @('--version')
    if ($result.Code -ne 0) {
        throw "CMake at '$CMake' failed to report its version (exit $($result.Code)). Run '$CMake --version' manually. $Hint"
    }
    if ($result.Output -notmatch 'cmake version ([0-9]+\.[0-9]+(?:\.[0-9]+)?)') {
        throw "CMake at '$CMake' did not print a recognizable version: $($result.Output.Trim()) $Hint"
    }

    return $Matches[1]
}

function Assert-AndroidToolchainValue {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [string]$Value,
        [Parameter(Mandatory = $true)][string]$ConfigPath,
        [Parameter(Mandatory = $true)][string]$InstallHint,
        [Parameter(Mandatory = $true)][string]$Stage
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "[$Stage] Android toolchain entry '$Name' is empty. $InstallHint $(Get-AndroidToolchainConfigHint $ConfigPath)"
    }
}

# Integer fields inside requirements: empty strings must not be treated as 0 (otherwise you get misleading errors like 'r0').
function Get-AndroidRequiredInt {
    param(
        [Parameter(Mandatory = $true)]$Requirements,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][int]$Default,
        [Parameter(Mandatory = $true)][string]$ConfigPath,
        [string]$InstallHint = ''
    )

    $raw = Get-AndroidConfigProperty $Requirements $Name
    if ($null -eq $raw) {
        return $Default
    }

    $text = ([string]$raw).Trim()
    if ($text.Length -eq 0) {
        return $Default
    }

    # Do not use [int]::TryParse ([ref] cannot create types under ConstrainedLanguage).
    if ($text -notmatch '^[0-9]+$' -or [int]$text -le 0) {
        throw "Android toolchain config 'requirements.$Name' must be a positive integer, got '$raw'. $InstallHint $(Get-AndroidToolchainConfigHint $ConfigPath)"
    }

    return [int]$text
}

# Read and validate the toolchain config per stage, returning a located result with relative/absolute paths unified.
# Stage: Native = build libCore.Android.so; Managed = build the managed layer and APK; Device = adb operations on a real device.
function Find-AndroidTools {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$Stage = 'Native',
        [string]$ConfigPath = ''
    )

    $known = @('Native', 'Managed', 'Device')
    if ($known -notcontains $Stage) {
        throw "Unknown Android toolchain stage '$Stage'. Expected one of: $($known -join ', ')."
    }

    $resolvedConfig = Get-AndroidToolchainConfigPath -RepositoryRoot $RepositoryRoot -ConfigPath $ConfigPath
    if (-not (Test-Path -LiteralPath $resolvedConfig -PathType Leaf)) {
        throw "Android toolchain config was not found: '$resolvedConfig'. Copy the template '$($script:AndroidToolchainExampleRelativePath)' to '$($script:AndroidToolchainConfigRelativePath)' and fill in the tool directories."
    }

    $rawConfig = Get-Content -Raw -LiteralPath $resolvedConfig -Encoding UTF8
    try {
        $config = $rawConfig | ConvertFrom-Json
    }
    catch {
        throw "Android toolchain config '$resolvedConfig' is not valid JSON: $($_.Exception.Message) $(Get-AndroidToolchainConfigHint $resolvedConfig)"
    }

    $schemaVersion = Get-AndroidConfigProperty $config 'schemaVersion'
    if ($null -ne $schemaVersion -and [int]$schemaVersion -ne 1) {
        throw "Android toolchain config '$resolvedConfig' has schemaVersion $schemaVersion; this tool understands version 1. $(Get-AndroidToolchainConfigHint $resolvedConfig)"
    }

    $requirements = Get-AndroidConfigProperty $config 'requirements'

    # Per-item progress printing: each passed check writes a [TITAN-CHECK] line so the user can see what already passed on failure.
    $checkStep = {
        param([string]$Name, [string]$Detail)
        Write-Host ("[TITAN-CHECK] OK {0} - {1}" -f $Name, $Detail)
    }

    $ndkMajor = 27
    $minimumCMake = '3.24'
    $dotnetMajor = 10
    if ($null -ne $requirements) {
        $ndkMajor = Get-AndroidRequiredInt -Requirements $requirements -Name 'ndkMajor' -Default $ndkMajor `
            -ConfigPath $resolvedConfig -InstallHint "Use NDK r$ndkMajor unless you have a specific reason to switch."
        $dotnetMajor = Get-AndroidRequiredInt -Requirements $requirements -Name 'dotnetSdkMajor' -Default $dotnetMajor `
            -ConfigPath $resolvedConfig -InstallHint "Use the .NET $dotnetMajor SDK unless you have a specific reason to switch."
        $value = Get-AndroidConfigProperty $requirements 'minimumCMakeVersion'
        if (-not [string]::IsNullOrWhiteSpace($value)) { $minimumCMake = [string]$value }
    }

    $hint = Get-AndroidToolchainConfigHint $resolvedConfig

    # abi/apiLevel/stl/generator are compile contracts; missing values must not silently become empty strings or 0.
    $abi = [string](Get-AndroidConfigProperty $config 'abi')
    Assert-AndroidToolchainValue -Name 'abi' -Value $abi -ConfigPath $resolvedConfig -Stage $Stage `
        -InstallHint "Set 'abi' to the target ABI, for example 'arm64-v8a'."

    $stl = [string](Get-AndroidConfigProperty $config 'stl')
    Assert-AndroidToolchainValue -Name 'stl' -Value $stl -ConfigPath $resolvedConfig -Stage $Stage `
        -InstallHint "Set 'stl' to the STL flavor, for example 'c++_static'."

    $generator = [string](Get-AndroidConfigProperty $config 'generator')
    Assert-AndroidToolchainValue -Name 'generator' -Value $generator -ConfigPath $resolvedConfig -Stage $Stage `
        -InstallHint "Set 'generator' to the CMake generator, for example 'Ninja'."

    $apiRaw = Get-AndroidConfigProperty $config 'apiLevel'
    $apiText = ''
    if ($null -ne $apiRaw) { $apiText = ([string]$apiRaw).Trim() }
    if ($apiText -notmatch '^[0-9]+$' -or [int]$apiText -le 0) {
        throw "[$Stage] Android toolchain entry 'apiLevel' must be a positive integer such as 28, got '$apiRaw'. $hint"
    }
    $apiLevel = [int]$apiText
    # apiLevel is configurable but must not go below the engine runtime floor (same contract as TITAN_ANDROID_MIN_API_LEVEL in Core.Android/CMakeLists).
    $minApiLevel = 28
    if ($apiLevel -lt $minApiLevel) {
        throw "[$Stage] Android toolchain entry 'apiLevel' is $apiLevel, but the engine requires >= $minApiLevel. Raise it in $resolvedConfig."
    }

    & $checkStep 'config' $resolvedConfig
    & $checkStep 'settings' ("abi=$abi apiLevel=$apiLevel stl=$stl generator=$generator")

    $result = @{
        Stage      = $Stage
        ConfigPath = $resolvedConfig
        Abi        = $abi
        ApiLevel   = $apiLevel
        Stl        = $stl
        Generator  = $generator
        Sdk        = $null
        Ndk        = $null
        CMake      = $null
        Ninja      = $null
        DotNet     = $null
        JavaHome   = $null
        LlvmBin    = $null
        Adb        = $null
    }

    # Every stage builds on the native toolchain; Device adds adb on top of Managed, so a config with only adb can never report green.
    $wantsNative = $true
    $wantsManaged = ($Stage -eq 'Managed' -or $Stage -eq 'Device')

    if ($wantsNative) {
        $ndkInstallHint = "Download Android NDK r$ndkMajor and unpack it, or run: sdkmanager --install `"ndk;$ndkMajor.x`" (the r$ndkMajor LTS build is 27.2.12479018)."
        $ndkValue = [string](Get-AndroidConfigProperty $config 'ndk')
        Assert-AndroidToolchainValue -Name 'ndk' -Value $ndkValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint $ndkInstallHint
        $ndk = Resolve-AndroidToolPath $RepositoryRoot $ndkValue
        if (-not (Test-Path -LiteralPath $ndk -PathType Container)) {
            throw "[$Stage] Configured NDK directory does not exist: '$ndk'. $hint Nothing else was substituted for it."
        }

        $revision = Get-AndroidNdkRevision $ndk
        if ($null -eq $revision) {
            throw "[$Stage] '$ndk' is not an Android NDK (missing or unreadable source.properties). Expected NDK r$ndkMajor. $hint"
        }
        $revisionMajor = ($revision -split '\.')[0]
        if ([int]$revisionMajor -ne $ndkMajor) {
            throw "[$Stage] NDK revision $revision is not the required r$ndkMajor. $ndkInstallHint $hint"
        }
        & $checkStep 'ndk' ("$ndk (r$revision)")

        $llvmBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
        if (-not (Test-Path -LiteralPath $llvmBin -PathType Container)) {
            throw "[$Stage] NDK at '$ndk' has no LLVM toolchain at '$llvmBin'. Reinstall NDK r$ndkMajor. $hint"
        }
        foreach ($tool in @('llvm-readelf.exe', 'llvm-nm.exe')) {
            if (-not (Test-Path -LiteralPath (Join-Path $llvmBin $tool) -PathType Leaf)) {
                throw "[$Stage] NDK LLVM tool is missing: '$(Join-Path $llvmBin $tool)'. Reinstall NDK r$ndkMajor. $hint"
            }
        }
        & $checkStep 'llvm' $llvmBin

        $cmakeValue = [string](Get-AndroidConfigProperty $config 'cmake')
        Assert-AndroidToolchainValue -Name 'cmake' -Value $cmakeValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint "Download CMake $minimumCMake or newer and set 'cmake' to its cmake.exe."
        $cmake = Resolve-AndroidToolPath $RepositoryRoot $cmakeValue
        if (-not (Test-Path -LiteralPath $cmake -PathType Leaf)) {
            throw "[$Stage] Configured CMake does not exist: '$cmake'. $hint"
        }

        $cmakeVersion = Get-AndroidCmakeVersion -CMake $cmake -Hint $hint
        if ([version]$cmakeVersion -lt [version]$minimumCMake) {
            throw "[$Stage] CMake $cmakeVersion at '$cmake' is older than the required $minimumCMake. Install a newer CMake and update 'cmake'. $hint"
        }
        & $checkStep 'cmake' ("$cmake ($cmakeVersion)")

        $ninjaValue = [string](Get-AndroidConfigProperty $config 'ninja')
        Assert-AndroidToolchainValue -Name 'ninja' -Value $ninjaValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint "Download Ninja and set 'ninja' to its ninja.exe."
        $ninja = Resolve-AndroidToolPath $RepositoryRoot $ninjaValue
        if (-not (Test-Path -LiteralPath $ninja -PathType Leaf)) {
            throw "[$Stage] Configured Ninja does not exist: '$ninja'. $hint"
        }

        $ninjaVersion = Invoke-AndroidToolCapture -File $ninja -Arguments @('--version')
        if ($ninjaVersion.Code -ne 0 -or [string]::IsNullOrWhiteSpace($ninjaVersion.Output)) {
            throw "[$Stage] Ninja at '$ninja' failed to report its version (exit $($ninjaVersion.Code)). Run '$ninja --version' manually. $hint"
        }
        & $checkStep 'ninja' ("$ninja ($($ninjaVersion.Output.Trim()))")

        $result.Ndk = $ndk
        $result.LlvmBin = $llvmBin
        $result.CMake = $cmake
        $result.Ninja = $ninja
    }

    if ($wantsManaged) {
        $dotnetValue = [string](Get-AndroidConfigProperty $config 'dotnet')
        Assert-AndroidToolchainValue -Name 'dotnet' -Value $dotnetValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint "Download the .NET $dotnetMajor SDK and set 'dotnet' to its dotnet.exe."
        $dotnet = Resolve-AndroidToolPath $RepositoryRoot $dotnetValue
        if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
            throw "[$Stage] Configured dotnet does not exist: '$dotnet'. $hint"
        }

        $sdks = Invoke-AndroidToolCapture -File $dotnet -Arguments @('--list-sdks')
        if ($sdks.Code -ne 0) {
            throw "[$Stage] 'dotnet --list-sdks' failed with exit $($sdks.Code). Run '$dotnet --list-sdks' manually. $hint"
        }
        $hasMajor = $false
        foreach ($line in ($sdks.Output -split "`n")) {
            if ($line -match ('^\s*' + $dotnetMajor + '\.')) {
                $hasMajor = $true
            }
        }
        if (-not $hasMajor) {
            throw "[$Stage] dotnet at '$dotnet' does not provide the required SDK major ${dotnetMajor}: $($sdks.Output.Trim()) $hint"
        }
        & $checkStep 'dotnet' $dotnet

        $sdkValue = [string](Get-AndroidConfigProperty $config 'androidSdk')
        Assert-AndroidToolchainValue -Name 'androidSdk' -Value $sdkValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint 'Download the Android SDK command-line tools plus a platform and set androidSdk to the SDK root.'
        $androidSdk = Resolve-AndroidToolPath $RepositoryRoot $sdkValue
        if (-not (Test-Path -LiteralPath $androidSdk -PathType Container)) {
            throw "[$Stage] Configured Android SDK directory does not exist: '$androidSdk'. $hint"
        }
        & $checkStep 'sdk' $androidSdk

        $javaValue = [string](Get-AndroidConfigProperty $config 'javaHome')
        Assert-AndroidToolchainValue -Name 'javaHome' -Value $javaValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint 'Install a JDK 17 or newer and set javaHome to its root directory.'
        $javaHome = Resolve-AndroidToolPath $RepositoryRoot $javaValue
        if (-not (Test-Path -LiteralPath (Join-Path $javaHome 'bin\java.exe') -PathType Leaf)) {
            throw "[$Stage] Configured javaHome has no bin\java.exe: '$javaHome'. $hint"
        }
        & $checkStep 'java' $javaHome

        $result.DotNet = $dotnet
        $result.Sdk = $androidSdk
        $result.JavaHome = $javaHome
    }

    if ($Stage -eq 'Device') {
        $adbValue = [string](Get-AndroidConfigProperty $config 'adb')
        Assert-AndroidToolchainValue -Name 'adb' -Value $adbValue -ConfigPath $resolvedConfig -Stage $Stage `
            -InstallHint 'Install Android platform-tools and set adb to its adb.exe.'
        $adb = Resolve-AndroidToolPath $RepositoryRoot $adbValue
        if (-not (Test-Path -LiteralPath $adb -PathType Leaf)) {
            throw "[$Stage] Configured adb does not exist: '$adb'. $hint"
        }
        & $checkStep 'adb' $adb
        $result.Adb = $adb
    }

    return $result
}

# Command-line facing result: never throws; returns Code = 0 (all tools present) / 2 (user must supply tools) plus directly printable text.
function Get-AndroidToolchainCheckResult {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$Stage = 'Native',
        [string]$ConfigPath = ''
    )

    try {
        $tools = Find-AndroidTools -RepositoryRoot $RepositoryRoot -Stage $Stage -ConfigPath $ConfigPath
    }
    catch {
        return @{ Code = 2; Message = $_.Exception.Message }
    }

    $lines = @(
        "Android toolchain OK for stage '$($tools.Stage)'.",
        "  abi=$($tools.Abi)  apiLevel=$($tools.ApiLevel)  stl=$($tools.Stl)  generator=$($tools.Generator)",
        "  config: $($tools.ConfigPath)"
    )
    if ($null -ne $tools.Ndk) { $lines += "  ndk:    $($tools.Ndk)" }
    if ($null -ne $tools.LlvmBin) { $lines += "  llvm:   $($tools.LlvmBin)" }
    if ($null -ne $tools.CMake) { $lines += "  cmake:  $($tools.CMake)" }
    if ($null -ne $tools.Ninja) { $lines += "  ninja:  $($tools.Ninja)" }
    if ($null -ne $tools.DotNet) { $lines += "  dotnet: $($tools.DotNet)" }
    if ($null -ne $tools.Sdk) { $lines += "  sdk:    $($tools.Sdk)" }
    if ($null -ne $tools.JavaHome) { $lines += "  java:   $($tools.JavaHome)" }
    if ($null -ne $tools.Adb) { $lines += "  adb:    $($tools.Adb)" }

    return @{ Code = 0; Message = ($lines -join "`n"); Tools = $tools }
}
