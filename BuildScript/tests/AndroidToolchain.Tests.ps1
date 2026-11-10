#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$toolsScript = Join-Path $repoRoot 'BuildScript\android\AndroidTools.ps1'
$tempRoot = Join-Path $repoRoot ("cache\AndroidToolchain-{0}-{1}" -f $PID, (Get-Random))
$script:failures = @()
# Skips must be printed at the end: when the NDK is absent the ELF gate tests never actually ran and must not count as passed.
$script:skips = @()

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Assert-ThrowsLike {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,

        [Parameter(Mandatory = $true)]
        [string[]]$Patterns,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    try {
        & $Action | Out-Null
    }
    catch {
        $text = $_.Exception.Message
        foreach ($pattern in $Patterns) {
            Assert-True ($text -match $pattern) "$Message (message '$text' does not match '$pattern')"
        }
        return
    }

    throw "$Message (no exception was thrown)"
}

function New-StubCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Output,

        [int]$ExitCode = 0
    )

    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    $lines = @('@echo off', "echo $Output", "exit /b $ExitCode")
    Set-Content -LiteralPath $Path -Value ($lines -join "`r`n") -Encoding ASCII
    return $Path
}

function New-FakeNdk {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$Revision
    )

    $ndk = Join-Path $Root "ndk\$Revision"
    New-Item -ItemType Directory -Force -Path $ndk | Out-Null
    Set-Content -LiteralPath (Join-Path $ndk 'source.properties') -Encoding ASCII -Value @(
        'Pkg.Desc = Android NDK'
        "Pkg.Revision = $Revision"
    )
    $llvmBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
    New-Item -ItemType Directory -Force -Path $llvmBin | Out-Null
    foreach ($tool in @('llvm-readelf.exe', 'llvm-nm.exe', 'llvm-strip.exe')) {
        Set-Content -LiteralPath (Join-Path $llvmBin $tool) -Encoding ASCII -Value ''
    }
    return $ndk
}

function New-FakeAndroidSdk {
    param([Parameter(Mandatory = $true)][string]$Root)

    New-Item -ItemType Directory -Force -Path (Join-Path $Root 'platforms\android-28') | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $Root 'platform-tools') | Out-Null
    Set-Content -LiteralPath (Join-Path $Root 'platform-tools\adb.exe') -Encoding ASCII -Value ''
    return $Root
}

function New-FakeJavaHome {
    param([Parameter(Mandatory = $true)][string]$Root)

    New-Item -ItemType Directory -Force -Path (Join-Path $Root 'bin') | Out-Null
    Set-Content -LiteralPath (Join-Path $Root 'bin\java.exe') -Encoding ASCII -Value ''
    return $Root
}

# Compile a minimal arm64 .so with the real NDK clang as the ELF gate fixture;
# no fabricated binaries, because the readelf/nm output is the input under test.
function New-ElfLibrary {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Source,
        [string[]]$ExtraArguments = @(),
        [string]$Target = 'aarch64-linux-android28'
    )

    $directory = Join-Path $tempRoot 'elf'
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $sourcePath = Join-Path $directory ($Name + '.cpp')
    Set-Content -LiteralPath $sourcePath -Encoding ASCII -Value $Source
    $library = Join-Path $directory ($Name + '.so')

    $arguments = @(('--target=' + $Target), '-shared', '-fPIC') + $ExtraArguments + @('-o', $library, $sourcePath)
    $result = Invoke-AndroidToolCapture -File $script:elfClang -Arguments $arguments
    Assert-True ($result.Code -eq 0) "clang could not build fixture '$Name' (exit $($result.Code)): $($result.Output)"
    return $library
}

# Write only the fields the user must fill; unfilled tools stay empty strings and the validation script never guesses paths.
# Omit is used to deliberately delete compile-contract fields, verifying they cannot be silently downgraded.
function Write-Config {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [hashtable]$Values = @{},

        [string[]]$Omit = @()
    )

    $all = [ordered]@{
        abi        = 'arm64-v8a'
        apiLevel   = 28
        generator  = 'Ninja'
        stl        = 'c++_static'
        ndk        = ''
        androidSdk = ''
        cmake      = ''
        ninja      = ''
        dotnet     = ''
        javaHome   = ''
        adb        = ''
    }

    foreach ($key in $Values.Keys) {
        $all[$key] = $Values[$key]
    }

    $config = [ordered]@{ schemaVersion = 1 }
    foreach ($key in $all.Keys) {
        if ($Omit -notcontains $key) {
            $config[$key] = $all[$key]
        }
    }
    $config['requirements'] = [ordered]@{
        ndkMajor            = 27
        minimumCMakeVersion = '3.24'
        dotnetSdkMajor      = 10
    }

    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    Set-Content -LiteralPath $Path -Encoding UTF8 -Value ($config | ConvertTo-Json -Depth 4)
    return $Path
}

# The exit-code contract can only be verified via a real subprocess; in-process calls cannot test parameter binding and the exit statement.
function Invoke-ChildPowerShell {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $exe = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    $outFile = Join-Path $tempRoot ('child-{0}.out' -f (Get-Random))
    $errFile = Join-Path $tempRoot ('child-{0}.err' -f (Get-Random))

    $process = Start-Process -FilePath $exe -ArgumentList (@('-NoProfile', '-ExecutionPolicy', 'Bypass') + $Arguments) `
        -RedirectStandardOutput $outFile -RedirectStandardError $errFile -NoNewWindow -Wait -PassThru

    # The subprocess's Write-Host output is written to the redirected file using the console OEM code page.
    $text = ''
    if (Test-Path -LiteralPath $outFile) { $text += (Get-Content -Raw -LiteralPath $outFile -Encoding Oem) }
    if (Test-Path -LiteralPath $errFile) { $text += (Get-Content -Raw -LiteralPath $errFile -Encoding Oem) }
    if ($null -eq $text) { $text = '' }

    return @{ Code = $process.ExitCode; Output = $text }
}

try {
    Assert-True (Test-Path -LiteralPath $toolsScript -PathType Leaf) "AndroidTools.ps1 was not found: $toolsScript"
    . $toolsScript

    $fixture = Join-Path $tempRoot 'fixture'
    $ndk27 = New-FakeNdk (Join-Path $fixture 'sdk27') '27.2.12479018'
    $ndk21 = New-FakeNdk (Join-Path $fixture 'sdk21') '21.4.7075529'
    $androidSdk = New-FakeAndroidSdk (Join-Path $fixture 'android-sdk')
    $javaHome = New-FakeJavaHome (Join-Path $fixture 'jdk')
    $cmakeGood = New-StubCommand (Join-Path $fixture 'tools\cmake-new.cmd') 'cmake version 4.4.0'
    $cmakeOld = New-StubCommand (Join-Path $fixture 'tools\cmake-old.cmd') 'cmake version 3.23.0'
    $ninja = New-StubCommand (Join-Path $fixture 'tools\ninja.cmd') '1.11.1'
    $dotnet10 = New-StubCommand (Join-Path $fixture 'tools\dotnet10.cmd') '10.0.403 [C:\Program Files\dotnet\sdk]'
    $dotnet8 = New-StubCommand (Join-Path $fixture 'tools\dotnet8.cmd') '8.0.300 [C:\Program Files\dotnet\sdk]'

    $nativeValues = @{
        ndk        = $ndk27
        cmake      = $cmakeGood
        ninja      = $ninja
        androidSdk = $androidSdk
        javaHome   = $javaHome
        adb        = (Join-Path $androidSdk 'platform-tools\adb.exe')
    }

    # 1. A missing config file must report the exact path and how to create it, never silently falling back to a guessed SDK directory.
    $missingConfig = Join-Path $tempRoot 'absent\android-toolchain.json'
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $missingConfig
    } @([regex]::Escape($missingConfig), 'AndroidToolchain\.example\.json') 'Missing config must name the expected path and the template'

    # 2. An explicitly configured dead path must be reported verbatim, never auto-swapped for another NDK on the machine.
    $badNdkValues = $nativeValues.Clone()
    $badNdkValues['ndk'] = Join-Path $fixture 'no-such-ndk'
    $badNdkConfig = Write-Config (Join-Path $tempRoot 'bad-ndk.json') $badNdkValues
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $badNdkConfig
    } @('(?i)ndk', [regex]::Escape('no-such-ndk')) 'A broken explicit NDK path must be reported verbatim'

    # 3. An NDK major-version mismatch must be rejected, with the r27 install command given.
    $oldNdkValues = $nativeValues.Clone()
    $oldNdkValues['ndk'] = $ndk21
    $oldNdkConfig = Write-Config (Join-Path $tempRoot 'old-ndk.json') $oldNdkValues
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $oldNdkConfig
    } @('(?i)r27', 'sdkmanager', '21\.4') 'An r21 NDK must be rejected with the r27 install command'

    # 4. NDK/CMake/Ninja alone must pass the Native stage: building Core.Android.so does not need dotnet, Android SDK, JDK, or adb.
    $nativeOnly = Write-Config (Join-Path $tempRoot 'native-only.json') @{ ndk = $ndk27; cmake = $cmakeGood; ninja = $ninja }
    $native = Find-AndroidTools $repoRoot -Stage Native -ConfigPath $nativeOnly
    Assert-True ($native.Ndk -eq $ndk27) "Native stage must return the configured NDK, got '$($native.Ndk)'"
    Assert-True ($native.CMake -eq $cmakeGood) "Native stage must return the configured CMake, got '$($native.CMake)'"
    Assert-True ($native.Ninja -eq $ninja) "Native stage must return the configured Ninja, got '$($native.Ninja)'"
    Assert-True ($null -eq $native.DotNet) "Native stage must not require dotnet, got '$($native.DotNet)'"
    Assert-True ($null -eq $native.Adb) "Native stage must not require adb, got '$($native.Adb)'"
    Assert-True ($native.LlvmBin -eq (Join-Path $ndk27 'toolchains\llvm\prebuilt\windows-x86_64\bin')) "LlvmBin must come from the NDK, got '$($native.LlvmBin)'"

    # 5. CMake below 3.24 must be rejected, reporting both the actual and required versions.
    $oldCMakeConfig = Write-Config (Join-Path $tempRoot 'old-cmake.json') @{ ndk = $ndk27; cmake = $cmakeOld; ninja = $ninja }
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $oldCMakeConfig
    } @('(?i)cmake', '3\.23', '3\.24') 'CMake 3.23 must be rejected with both versions in the message'

    # 6. Unconfigured tools must report the tool name and install guidance, not an empty reference.
    $noCMakeConfig = Write-Config (Join-Path $tempRoot 'no-cmake.json') @{ ndk = $ndk27; ninja = $ninja }
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $noCMakeConfig
    } @('(?i)cmake', 'android-toolchain\.json') 'An unset CMake must be named together with the config file'

    $noNinjaConfig = Write-Config (Join-Path $tempRoot 'no-ninja.json') @{ ndk = $ndk27; cmake = $cmakeGood }
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $noNinjaConfig
    } @('(?i)ninja', 'android-toolchain\.json') 'An unset Ninja must be named together with the config file'

    # 7. Only the managed stage requires dotnet/SDK/JDK; an insufficient SDK major version must also be rejected.
    $managedValues = $nativeValues.Clone()
    $managedValues['dotnet'] = $dotnet10
    $managedFull = Write-Config (Join-Path $tempRoot 'managed.json') $managedValues
    $managed = Find-AndroidTools $repoRoot -Stage Managed -ConfigPath $managedFull
    Assert-True ($managed.DotNet -eq $dotnet10) "Managed stage must return the configured dotnet, got '$($managed.DotNet)'"
    Assert-True ($managed.Sdk -eq $androidSdk) "Managed stage must return the configured Android SDK, got '$($managed.Sdk)'"
    Assert-True ($managed.JavaHome -eq $javaHome) "Managed stage must return the configured JDK, got '$($managed.JavaHome)'"

    # 7b. Per-item progress printing: every passed Find-AndroidTools check must write a [TITAN-CHECK] OK <name> line,
    #     from which the editor panel shows "which step was reached and what already passed on failure". adb must not appear in the Managed stage.
    $checkOut = ((Find-AndroidTools $repoRoot -Stage Managed -ConfigPath $managedFull 6>&1) | ForEach-Object { $_.ToString() }) -join "`n"
    foreach ($step in @('config', 'settings', 'ndk', 'llvm', 'cmake', 'ninja', 'dotnet', 'sdk', 'java')) {
        Assert-True ($checkOut -match ("\[TITAN-CHECK\] OK {0}\b" -f $step)) "Find-AndroidTools must print [TITAN-CHECK] OK $step, got: $checkOut"
    }
    Assert-True ($checkOut -notmatch '\[TITAN-CHECK\] OK adb') 'adb step must not print for the Managed stage'

    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Managed -ConfigPath $nativeOnly
    } @('(?i)dotnet') 'Managed stage must require dotnet'

    # The Managed stage depends on Native artifacts, so the missing native tool (ninja) must be reported first instead of jumping to dotnet.
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Managed -ConfigPath $noNinjaConfig
    } @('(?i)ninja', '(?i)managed') 'Managed stage must validate the native tools it depends on first'

    $oldDotnetValues = $nativeValues.Clone()
    $oldDotnetValues['dotnet'] = $dotnet8
    $dotnetTooOld = Write-Config (Join-Path $tempRoot 'dotnet8.json') $oldDotnetValues
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Managed -ConfigPath $dotnetTooOld
    } @('(?i)dotnet', '(?i)sdk', '10') 'A dotnet without SDK 10 must be rejected'

    # 8. Only the device stage requires adb.
    $device = Find-AndroidTools $repoRoot -Stage Device -ConfigPath $managedFull
    Assert-True ($device.Adb -eq (Join-Path $androidSdk 'platform-tools\adb.exe')) "Device stage must return adb, got '$($device.Adb)'"

    $noAdbValues = $managedValues.Clone()
    $noAdbValues['adb'] = ''
    $noAdb = Write-Config (Join-Path $tempRoot 'no-adb.json') $noAdbValues
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Device -ConfigPath $noAdb
    } @('(?i)adb') 'Device stage must require adb'

    # 9. ABI/API/STL in the config are compile contracts and must be returned to the caller verbatim.
    Assert-True ($native.Abi -eq 'arm64-v8a') "Abi must be forwarded, got '$($native.Abi)'"
    Assert-True ($native.ApiLevel -eq 28) "ApiLevel must be forwarded, got '$($native.ApiLevel)'"
    Assert-True ($native.Stl -eq 'c++_static') "Stl must be forwarded, got '$($native.Stl)'"

    # 10. Unknown stages must be rejected, so a typo cannot silently skip checks.
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Nativ -ConfigPath $nativeOnly
    } @('(?i)stage', '(?i)nativ') 'An unknown stage must be rejected and echo the value'

    # 11. The command-line entry can only return 0 (all tools present) or 2 (user must supply tools); no exception may leak to the caller.
    $checkScript = Join-Path $repoRoot 'BuildScript\check\check-android-toolchain.ps1'
    Assert-True (Test-Path -LiteralPath $checkScript -PathType Leaf) "check-android-toolchain.ps1 was not found: $checkScript"

    $okResult = Get-AndroidToolchainCheckResult -RepositoryRoot $repoRoot -Stage Native -ConfigPath $nativeOnly
    Assert-True ($okResult.Code -eq 0) "A complete Native config must report exit code 0, got '$($okResult.Code)'"
    foreach ($expected in @($ndk27, $cmakeGood, $ninja)) {
        Assert-True ($okResult.Message -like "*$expected*") "The success report must list '$expected', got: $($okResult.Message)"
    }

    $missingResult = Get-AndroidToolchainCheckResult -RepositoryRoot $repoRoot -Stage Native -ConfigPath $missingConfig
    Assert-True ($missingResult.Code -eq 2) "A missing config must report exit code 2, got '$($missingResult.Code)'"
    Assert-True ($missingResult.Message -like "*$missingConfig*") "The failure report must name the config path, got: $($missingResult.Message)"

    $brokenResult = Get-AndroidToolchainCheckResult -RepositoryRoot $repoRoot -Stage Managed -ConfigPath $nativeOnly
    Assert-True ($brokenResult.Code -eq 2) "An incomplete Managed config must report exit code 2, got '$($brokenResult.Code)'"
    Assert-True ($brokenResult.Message -match '(?i)dotnet') "The failure report must name the missing tool, got: $($brokenResult.Message)"

    # 12. The exit-code contract must be verified via a real subprocess: only 0 or 2; argument errors must not degrade to 1.
    $cliOk = Invoke-ChildPowerShell @('-File', $checkScript, '-Stage', 'Native', '-ConfigPath', $nativeOnly)
    Assert-True ($cliOk.Code -eq 0) "CLI with a complete config must exit 0, got '$($cliOk.Code)': $($cliOk.Output)"
    Assert-True ($cliOk.Output -like '*Android toolchain OK*') "CLI success output must report OK, got: $($cliOk.Output)"

    $cliMissing = Invoke-ChildPowerShell @('-File', $checkScript, '-Stage', 'Native', '-ConfigPath', $missingConfig)
    Assert-True ($cliMissing.Code -eq 2) "CLI with a missing config must exit 2, got '$($cliMissing.Code)': $($cliMissing.Output)"

    $cliBadStage = Invoke-ChildPowerShell @('-File', $checkScript, '-Stage', 'Bogus', '-ConfigPath', $nativeOnly)
    Assert-True ($cliBadStage.Code -eq 2) "CLI with an invalid -Stage must exit 2, not leak exit 1, got '$($cliBadStage.Code)': $($cliBadStage.Output)"
    Assert-True ($cliBadStage.Output -match '(?i)stage') "CLI must report the invalid stage, got: $($cliBadStage.Output)"

    # 13. Missing compile-contract fields must be rejected; [int]$null must not silently become apiLevel=0.
    $noApi = Write-Config (Join-Path $tempRoot 'no-api.json') @{ ndk = $ndk27; cmake = $cmakeGood; ninja = $ninja } @('apiLevel')
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $noApi
    } @('(?i)apiLevel', 'android-toolchain\.json') 'A missing apiLevel must be rejected, not silently become 0'

    $badApi = Write-Config (Join-Path $tempRoot 'bad-api.json') @{ ndk = $ndk27; cmake = $cmakeGood; ninja = $ninja; apiLevel = 'android-28' } @()
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $badApi
    } @('(?i)apiLevel', 'android-toolchain\.json') 'A non-numeric apiLevel must be rejected with a readable message'

    $noAbi = Write-Config (Join-Path $tempRoot 'no-abi.json') @{ ndk = $ndk27; cmake = $cmakeGood; ninja = $ninja } @('abi')
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $noAbi
    } @('(?i)abi', 'android-toolchain\.json') 'A missing abi must be rejected, not silently become an empty string'

    # 14. The Device stage adds adb on top of Managed: a config with only adb must never report green.
    $adbOnlyValues = @{ adb = (Join-Path $androidSdk 'platform-tools\adb.exe') }
    $adbOnly = Write-Config (Join-Path $tempRoot 'adb-only.json') $adbOnlyValues
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Device -ConfigPath $adbOnly
    } @('(?i)ndk') 'Device stage must not pass when everything except adb is missing'

    $deviceFull = $managedValues.Clone()
    $deviceFull['adb'] = Join-Path $androidSdk 'platform-tools\adb.exe'
    $deviceConfig = Write-Config (Join-Path $tempRoot 'device-full.json') $deviceFull
    $deviceResult = Find-AndroidTools $repoRoot -Stage Device -ConfigPath $deviceConfig
    Assert-True ($deviceResult.Adb -eq $deviceFull['adb']) "Device stage must return adb, got '$($deviceResult.Adb)'"
    Assert-True ($deviceResult.DotNet -eq $dotnet10) 'Device stage must keep the Managed tools it builds on'

    # 15. ninja must actually run; checking file existence alone is not enough.
    $badNinja = New-StubCommand (Join-Path $fixture 'tools\ninja-broken.cmd') 'ninja: broken' 1
    $badNinjaConfig = Write-Config (Join-Path $tempRoot 'bad-ninja.json') @{ ndk = $ndk27; cmake = $cmakeGood; ninja = $badNinja }
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $badNinjaConfig
    } @('(?i)ninja', 'android-toolchain\.json') 'A ninja that cannot report its version must be rejected with the config hint'

    # 16. Invalid JSON must name the file, not bubble up the raw ConvertFrom-Json error.
    $badJson = Join-Path $tempRoot 'broken.json'
    Set-Content -LiteralPath $badJson -Encoding ASCII -Value '{ this is not json'
    Assert-ThrowsLike {
        Find-AndroidTools $repoRoot -Stage Native -ConfigPath $badJson
    } @([regex]::Escape($badJson), '(?i)json') 'Invalid JSON must name the config file'

    # 17. A relative -ConfigPath is resolved against the repo root, not the caller's current directory.
    $relativeConfig = $nativeOnly.Substring($repoRoot.Length + 1)
    $relativeResult = Get-AndroidToolchainCheckResult -RepositoryRoot $repoRoot -Stage Native -ConfigPath $relativeConfig
    Assert-True ($relativeResult.Code -eq 0) "A repo-relative -ConfigPath must resolve against the repository root, got code '$($relativeResult.Code)': $($relativeResult.Message)"

    # 18. A drive root must not be TrimEnd'ed into a drive-relative path.
    $driveRoot = Resolve-AndroidToolPath $repoRoot 'E:\'
    Assert-True ($driveRoot -eq 'E:\') "A drive root must stay 'E:\', got '$driveRoot'"

    # 19. Command-line assembly must follow Windows parsing rules: quote only when necessary (otherwise cmd's /D /S /C switches break),
    # double trailing backslashes and escape inner quotes when quoting; double % whether quoted or not.
    $plainArgument = ConvertTo-AndroidBatchArgument 'plain'
    Assert-True ($plainArgument -eq 'plain') "A plain argument must stay bare so cmd switches keep working, got <$plainArgument>"
    $spacedArgument = ConvertTo-AndroidBatchArgument 'a b'
    Assert-True ($spacedArgument -eq '"a b"') "A spaced argument must stay one token, got <$spacedArgument>"
    $slashArgument = ConvertTo-AndroidBatchArgument 'C:\dir\'
    Assert-True ($slashArgument -eq '"C:\dir\\"') "A trailing backslash must be doubled before the closing quote, got <$slashArgument>"
    $quoteArgument = ConvertTo-AndroidBatchArgument 'say "hi"'
    Assert-True ($quoteArgument -eq '"say \"hi\""') "Inner quotes must be escaped, got <$quoteArgument>"
    $percentArgument = ConvertTo-AndroidBatchArgument '100%'
    Assert-True ($percentArgument -eq '100%%') "Percent must be doubled inside a batch file, got <$percentArgument>"
    $metaArgument = ConvertTo-AndroidBatchArgument '-DX(1)'
    Assert-True ($metaArgument -eq '"-DX(1)"') "cmd metacharacters must be quoted, got <$metaArgument>"

    # 20. The bounded executor must return the exit code and collect both stdout and stderr into Output.
    $captured = Invoke-AndroidProcess -File $env:ComSpec -Arguments @('/d', '/s', '/c', 'echo out-line& echo err-line 1>&2& exit /b 7') -TimeoutMs 20000
    Assert-True ($captured.Code -eq 7) "Invoke-AndroidProcess must return the child exit code, got '$($captured.Code)'"
    Assert-True ($captured.Output -match 'out-line') "Output must contain stdout, got: $($captured.Output)"
    Assert-True ($captured.Output -match 'err-line') "Output must contain stderr, got: $($captured.Output)"

    # 21. A timeout must throw and kill the whole subprocess tree; adb-like children must not be left in the background.
    $survivor = Join-Path $tempRoot 'survivor.txt'
    $sleepScript = "Start-Sleep -Seconds 6; Set-Content -LiteralPath '$survivor' -Value alive"
    $powerShellExe = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    Assert-ThrowsLike {
        Invoke-AndroidProcess -File $powerShellExe -Arguments @('-NoProfile', '-Command', $sleepScript) -TimeoutMs 1500
    } @('(?i)timeout', '(?i)seconds|ms') 'A hanging command must fail with a timeout message naming the budget'
    Start-Sleep -Seconds 7
    Assert-True (-not (Test-Path -LiteralPath $survivor)) 'The timed-out process tree must be killed, but the child kept running'

    # 22. A timeout budget <= 0 is a caller error and must be rejected immediately, not waited on forever.
    Assert-ThrowsLike {
        Invoke-AndroidProcess -File $env:ComSpec -Arguments @('/d', '/s', '/c', 'exit /b 0') -TimeoutMs 0
    } @('(?i)timeout') 'A non-positive timeout budget must be rejected'

    # 23. On failure Invoke-AndroidCommand must include the stage and exit code; on success it hands the output to the caller.
    Assert-ThrowsLike {
        Invoke-AndroidCommand -Stage 'Native' -File $env:ComSpec -Arguments @('/d', '/s', '/c', 'echo boom 1>&2& exit /b 7')
    } @('(?i)native', '7', '(?i)boom') 'A failing command must report the stage, the exit code and the captured output'

    $succeeded = Invoke-AndroidCommand -Stage 'Native' -File $env:ComSpec -Arguments @('/d', '/s', '/c', 'echo fine') -TimeoutMs 20000
    Assert-True ($succeeded.Code -eq 0) "A successful command must return code 0, got '$($succeeded.Code)'"
    Assert-True ($succeeded.Output -match 'fine') "A successful command must keep its output, got: $($succeeded.Output)"

    # 24. ELF gate: NEEDED whitelist, no exported operators, LOAD 16 KB alignment, and arm64 architecture.
    $elfTools = $null
    try { $elfTools = Find-AndroidTools $repoRoot -Stage Native } catch { $script:elfClang = '' }
    $script:elfClang = ''
    if ($null -ne $elfTools) {
        $clangCandidate = Join-Path $elfTools.LlvmBin 'clang.exe'
        if (Test-Path -LiteralPath $clangCandidate -PathType Leaf) { $script:elfClang = $clangCandidate }
    }

    if ($script:elfClang.Length -eq 0) {
        $script:skips += 'ELF gate cases 24a-24f: a real Android NDK clang.exe was not found (fill cache/config/android-toolchain.json).'
    }
    else {
        Assert-ThrowsLike {
            Test-AndroidNativeLibrary -Tools $elfTools -Library (Join-Path $tempRoot 'missing.so')
        } @('missing\.so') 'A missing native library must be reported with its path'

# NDK r27 only adds max-page-size=16384 when ANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES is enabled,
        # so the "compliant sample" must carry that link option explicitly; the default artifact is 4 KB.
        $plainLibrary = New-ElfLibrary 'libtitanplain' 'extern "C" int titan_probe_add(int a, int b) { return a + b; }' `
            @('-Wl,-z,max-page-size=16384')
        $plainResult = Test-AndroidNativeLibrary -Tools $elfTools -Library $plainLibrary
        Assert-True ($plainResult.Needed.Count -ge 1) "A valid arm64 library must report its NEEDED entries, got '$($plainResult.Needed -join ',')'"
        Assert-True ($plainResult.Machine -match '(?i)aarch64') "A valid library must report AArch64, got '$($plainResult.Machine)'"

        $fourKLibrary = New-ElfLibrary 'libtitan4k' 'extern "C" int titan_probe_mul(int a, int b) { return a * b; }' @('-Wl,-z,max-page-size=4096')
        Assert-ThrowsLike {
            Test-AndroidNativeLibrary -Tools $elfTools -Library $fourKLibrary
        } @('(?i)16 ?KB|align') 'A 4 KB LOAD alignment must be rejected by the 16 KB page requirement'

        # Except for the 4 KB alignment case, every other fixture carries the 16 KB link options, so they are caught by their dedicated rules (not by alignment).
        $operatorLibrary = New-ElfLibrary 'libtitanop' 'void* operator new(unsigned long) { return 0; }' `
            @('-Wl,-z,max-page-size=16384')
        Assert-ThrowsLike {
            Test-AndroidNativeLibrary -Tools $elfTools -Library $operatorLibrary
        } @('_Znwm') 'An exported operator new must be rejected by name'

        $dependency = New-ElfLibrary 'libtitandep' 'extern "C" int titan_dep_value() { return 3; }' `
            @('-Wl,-z,max-page-size=16384')
        $needyLibrary = New-ElfLibrary 'libtitanneedy' 'extern "C" int titan_dep_value(); extern "C" int titan_needy() { return titan_dep_value(); }' `
            @(('-L' + (Split-Path -Parent $dependency)), '-ltitandep', '-Wl,--no-as-needed', '-Wl,-z,max-page-size=16384')
        Assert-ThrowsLike {
            Test-AndroidNativeLibrary -Tools $elfTools -Library $needyLibrary
        } @('libtitandep') 'A NEEDED entry outside the allow list must be rejected by name'

        $x86Library = New-ElfLibrary 'libtitanx86' 'extern "C" int titan_probe_x86(void) { return 1; }' @() 'x86_64-linux-android28'
        Assert-ThrowsLike {
            Test-AndroidNativeLibrary -Tools $elfTools -Library $x86Library
        } @('(?i)aarch64') 'A non-arm64 library must be rejected with the expected architecture'
    }

    Write-Host 'AndroidToolchain tests passed.'
    if ($script:skips.Count -gt 0) {
        foreach ($skip in $script:skips) { Write-Host -ForegroundColor Yellow ('SKIPPED: ' + $skip) }
    }
}
catch {
    $script:failures += $_.Exception.Message
    Write-Host -ForegroundColor Red $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}

if ($script:failures.Count -gt 0) {
    exit 1
}
exit 0
