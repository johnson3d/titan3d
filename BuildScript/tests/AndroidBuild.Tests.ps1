#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Orchestration contract of build-android.ps1: stage order, stop on failure, and no redundant prerequisite work within a single stage.
# Fake cmake/ninja/dotnet tools record the invocation sequence; only the ELF gate uses real NDK artifacts.
# When the NDK is unavailable, the All-order case is skipped with an explicit SKIPPED report, never counting a skip as a pass.

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$buildScript = Join-Path $repoRoot 'BuildScript\android\build-android.ps1'
$stageScript = Join-Path $repoRoot 'BuildScript\android\Stage-AndroidContent.ps1'
$toolsScript = Join-Path $repoRoot 'BuildScript\android\AndroidTools.ps1'
$tempRoot = Join-Path $repoRoot ("cache\AndroidBuild-{0}-{1}" -f $PID, (Get-Random))

$script:failures = @()
$script:skips = @()

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) { throw $Message }
}

function Assert-ThrowsLike {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string[]]$Patterns,
        [Parameter(Mandatory = $true)][string]$Message
    )

    try { & $Action | Out-Null }
    catch {
        $text = $_.Exception.Message
        foreach ($pattern in $Patterns) {
            Assert-True ($text -match $pattern) "$Message (message '$text' does not match '$pattern')"
        }
        return
    }

    throw "$Message (no exception was thrown)"
}

function Invoke-ChildPowerShell {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $exe = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    $outFile = Join-Path $tempRoot ('child-{0}.out' -f (Get-Random))
    $errFile = Join-Path $tempRoot ('child-{0}.err' -f (Get-Random))
    $process = Start-Process -FilePath $exe -ArgumentList (@('-NoProfile', '-ExecutionPolicy', 'Bypass') + $Arguments) `
        -RedirectStandardOutput $outFile -RedirectStandardError $errFile -NoNewWindow -Wait -PassThru

    $text = ''
    if (Test-Path -LiteralPath $outFile) { $text += (Get-Content -Raw -LiteralPath $outFile -Encoding Oem) }
    if (Test-Path -LiteralPath $errFile) { $text += (Get-Content -Raw -LiteralPath $errFile -Encoding Oem) }
    if ($null -eq $text) { $text = '' }

    return @{ Code = $process.ExitCode; Output = $text }
}

function Write-Utf8Text {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    Set-Content -LiteralPath $Path -Value $Text -Encoding UTF8
}

function New-BatchFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string[]]$Lines
    )

    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    Set-Content -LiteralPath $Path -Value ($Lines -join "`r`n") -Encoding ASCII
}

function Write-ToolchainConfig {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][hashtable]$Values
    )

    $config = @{
        schemaVersion = 1
        abi           = 'arm64-v8a'
        apiLevel      = 28
        generator     = 'Ninja'
        stl           = 'c++_static'
        requirements  = @{ ndkMajor = 27; minimumCMakeVersion = '3.24'; dotnetSdkMajor = 10 }
    }
    foreach ($key in $Values.Keys) { $config[$key] = $Values[$key] }

    Write-Utf8Text $Path ($config | ConvertTo-Json -Depth 4)
    return $Path
}

# The fake tools only do two things: append invocations to events.txt (order is the orchestration contract), and satisfy probe output.
# Real tools are .exe files, which run.cmd can invoke without 'call' and return to the wrapper; .cmd cannot,
# so the fake tools write their exit code into exit.txt in the current directory (the wrapper's working directory).
function New-FakeToolchain {
    param([Parameter(Mandatory = $true)][string]$FakeRoot)

    $tools = Join-Path $FakeRoot 'tools'
    New-Item -ItemType Directory -Force -Path $tools | Out-Null
    Set-Content -LiteralPath (Join-Path $tools 'stub.apk') -Value 'apk' -Encoding ASCII

    New-BatchFile (Join-Path $tools 'cmake.cmd') @(
        '@echo off',
        'echo cmake %*>>"%~dp0..\events.txt"',
        'if /i "%~1"=="--version" goto :version',
        'if /i "%~1"=="--build" goto :native',
        'echo 0 > exit.txt',
        'exit /b 0',
        ':version',
        'echo cmake version 3.31.6',
        'exit /b 0',
        ':native',
        'if not exist "%~dp0..\fail-native.txt" goto :native-ok',
        'echo native boom 1>&2',
        'echo 1 > exit.txt',
        'exit /b 1',
        ':native-ok',
        'echo 0 > exit.txt',
        'exit /b 0'
    )
    New-BatchFile (Join-Path $tools 'ninja.cmd') @(
        '@echo off',
        'echo 1.11.1',
        'exit /b 0'
    )
    New-BatchFile (Join-Path $tools 'dotnet.cmd') @(
        '@echo off',
        'if /i "%~1"=="--list-sdks" goto :sdks',
        'echo %~2| findstr /i "AloneGame.Maui.csproj" >nul || goto :plain',
        'if exist "%~dp0..\repo\intermediate\android\content\Debug\manifest.txt" (echo apk-precheck=present>>"%~dp0..\events.txt") else (echo apk-precheck=missing>>"%~dp0..\events.txt")',
        'if not exist "%~dp0..\repo\Module\AloneGame.Maui\bin\Debug\net10.0-android" mkdir "%~dp0..\repo\Module\AloneGame.Maui\bin\Debug\net10.0-android">nul 2>&1',
        'if not exist "%~dp0..\repo\Module\AloneGame.Maui\bin\Release\net10.0-android" mkdir "%~dp0..\repo\Module\AloneGame.Maui\bin\Release\net10.0-android">nul 2>&1',
        'copy /y "%~dp0stub.apk" "%~dp0..\repo\Module\AloneGame.Maui\bin\Debug\net10.0-android\AloneGame.Maui-Signed.apk">nul',
        'copy /y "%~dp0stub.apk" "%~dp0..\repo\Module\AloneGame.Maui\bin\Release\net10.0-android\AloneGame.Maui-Signed.apk">nul',
        ':plain',
        'echo dotnet %*>>"%~dp0..\events.txt"',
        'echo 0 > exit.txt',
        'exit /b 0',
        ':sdks',
        'echo 10.0.403 [C:\Program Files\dotnet\sdk]',
        'exit /b 0'
    )

    return @{
        CMake  = (Join-Path $tools 'cmake.cmd')
        Ninja  = (Join-Path $tools 'ninja.cmd')
        DotNet = (Join-Path $tools 'dotnet.cmd')
        Tools  = $tools
    }
}

function New-FakeNdk {
    param([Parameter(Mandatory = $true)][string]$Root)

    $ndk = Join-Path $Root 'ndk'
    New-Item -ItemType Directory -Force -Path $ndk | Out-Null
    Set-Content -LiteralPath (Join-Path $ndk 'source.properties') -Encoding ASCII -Value @(
        'Pkg.Desc = Android NDK'
        'Pkg.Revision = 27.2.12479018'
    )
    $llvmBin = Join-Path $ndk 'toolchains\llvm\prebuilt\windows-x86_64\bin'
    New-Item -ItemType Directory -Force -Path $llvmBin | Out-Null
    foreach ($tool in @('llvm-readelf.exe', 'llvm-nm.exe')) {
        Set-Content -LiteralPath (Join-Path $llvmBin $tool) -Encoding ASCII -Value ''
    }
    return $ndk
}

function New-FakeRepository {
    param([Parameter(Mandatory = $true)][string]$FakeRoot)

    $repository = Join-Path $FakeRoot 'repo'
    foreach ($configuration in @('Debug', 'Release')) {
        New-Item -ItemType Directory -Force -Path (Join-Path $repository "Core.Android\build\$configuration") | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $repository "binaries\android\$configuration") | Out-Null
    }
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'codegen\Android\DataCopyer') | Out-Null
    Write-Utf8Text (Join-Path $repository 'codegen\Android\DataCopyer\.datacopyer.state.json') `
        '{"Platform":"Android","Configuration":"Debug","Hash":"HASH-Debug"}'
    Set-Content -LiteralPath (Join-Path $repository 'Core.Android\RefreshSources.cmake') -Encoding ASCII -Value '# fake'
    Set-Content -LiteralPath (Join-Path $repository 'Core.Android\GenerateCodegen.cmake') -Encoding ASCII -Value '# fake'
    $contentLines = @(
        'content/config/engineconfig_android_smoke.jscfg'
        'enginecontent/metadata/**'
        'content/metadata/**'
        'binaries/Plugins/DataCopyer.plugin'
    ) -join "`n"
    Write-Utf8Text (Join-Path $repository 'Module\AloneGame.Maui\TitanContent.txt') $contentLines
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'content\config') | Out-Null
    Set-Content -LiteralPath (Join-Path $repository 'content\config\engineconfig_android_smoke.jscfg') -Encoding ASCII -Value 'config'
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'enginecontent\metadata') | Out-Null
    Set-Content -LiteralPath (Join-Path $repository 'enginecontent\metadata\engine.meta') -Encoding ASCII -Value 'engine'
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'content\metadata') | Out-Null
    Set-Content -LiteralPath (Join-Path $repository 'content\metadata\game.meta') -Encoding ASCII -Value 'game'
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'binaries\android\Plugins\DataCopyer') | Out-Null
    Set-Content -LiteralPath (Join-Path $repository 'binaries\android\Plugins\DataCopyer.plugin') -Encoding ASCII -Value 'plugin'
    Set-Content -LiteralPath (Join-Path $repository 'binaries\android\Plugins\DataCopyer\DataCopyer.All.dll') -Encoding ASCII -Value 'dll'
    New-Item -ItemType Directory -Force -Path (Join-Path $repository 'Module\AloneGame.Maui') | Out-Null
    Set-Content -LiteralPath (Join-Path $repository 'Module\AloneGame.Maui\AloneGame.Maui.csproj') -Encoding ASCII -Value '<Project />'

    return $repository
}

function Get-Events {
    param([Parameter(Mandatory = $true)][string]$FakeRoot)

    $path = Join-Path $FakeRoot 'events.txt'
    if (-not (Test-Path -LiteralPath $path)) { return @() }
    return @(Get-Content -LiteralPath $path)
}

function Clear-Events {
    param([Parameter(Mandatory = $true)][string]$FakeRoot)

    $path = Join-Path $FakeRoot 'events.txt'
    if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force }
}

# Version probing and ninja only prove tool presence; they do not participate in orchestration order.
function Get-Stages {
    param([string[]]$Events = @())

    $stages = @()
    foreach ($line in $Events) {
        if ($line -match '^cmake .*--version') { continue }
        if ($line -match '^ninja ') { continue }
        if ($line -match '^dotnet .*--list-sdks') { continue }
        if ($line -match 'RefreshSources\.cmake') { $stages += 'Refresh' }
        elseif ($line -match 'GenerateCodegen\.cmake') { $stages += 'Generate' }
        elseif ($line -match '^cmake -S ') { $stages += 'Configure' }
        elseif ($line -match '^cmake --build ') { $stages += 'Native' }
        elseif ($line -match 'TitanRunDataCopyerStage') { $stages += 'DataCopyer' }
        elseif ($line -match 'Engine\.Android\.csproj') { $stages += 'Engine' }
        elseif ($line -match 'AloneGame\.Maui\.csproj') { $stages += 'Apk' }
        elseif ($line -match '^apk-precheck=') { $stages += 'ApkPrecheck' }
    }
    return $stages
}

try {
    Assert-True (Test-Path -LiteralPath $toolsScript -PathType Leaf) "AndroidTools.ps1 was not found: $toolsScript"
    Assert-True (Test-Path -LiteralPath $buildScript -PathType Leaf) "build-android.ps1 was not found: $buildScript"
    Assert-True (Test-Path -LiteralPath $stageScript -PathType Leaf) "Stage-AndroidContent.ps1 was not found: $stageScript"

    $fakeRoot = Join-Path $tempRoot 'fake'
    $repository = New-FakeRepository $fakeRoot
    $fakeTools = New-FakeToolchain $fakeRoot
    $ndk = New-FakeNdk $fakeRoot

    # The Native stage has the ELF gate, and a fake NDK cannot provide readelf/nm; only a real NDK can run it end to end.
    . $toolsScript
    $realTools = $null
    try { $realTools = Find-AndroidTools $repoRoot -Stage Native } catch { $realTools = $null }
    $elfClang = ''
    if ($null -ne $realTools) {
        $candidate = Join-Path $realTools.LlvmBin 'clang.exe'
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $elfClang = $candidate
            $ndk = $realTools.Ndk
        }
    }
    $androidSdk = Join-Path $fakeRoot 'android-sdk'
    New-Item -ItemType Directory -Force -Path (Join-Path $androidSdk 'platforms\android-28') | Out-Null
    $javaHome = Join-Path $fakeRoot 'jdk'
    New-Item -ItemType Directory -Force -Path (Join-Path $javaHome 'bin') | Out-Null
    Set-Content -LiteralPath (Join-Path $javaHome 'bin\java.exe') -Encoding ASCII -Value ''

    $configPath = Write-ToolchainConfig (Join-Path $fakeRoot 'toolchain.json') @{
        ndk        = $ndk
        cmake      = $fakeTools.CMake
        ninja      = $fakeTools.Ninja
        dotnet     = $fakeTools.DotNet
        androidSdk = $androidSdk
        javaHome   = $javaHome
    }

    $nativeLibrary = Join-Path $repository 'binaries\android\debug\libCore.Android.so'

    # 1. Missing tools: must exit 2 first (user-fixable), never fall through to build failure 1.
    $missingConfig = Join-Path $fakeRoot 'missing-toolchain.json'
    $missingResult = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $missingConfig, '-Configuration', 'Debug', '-Stage', 'All')
    Assert-True ($missingResult.Code -eq 2) "A missing toolchain config must exit 2, got '$($missingResult.Code)': $($missingResult.Output)"

    $noDotNet = Write-ToolchainConfig (Join-Path $fakeRoot 'no-dotnet.json') @{
        ndk = $ndk; cmake = $fakeTools.CMake; ninja = $fakeTools.Ninja; androidSdk = $androidSdk; javaHome = $javaHome
    }
    $noDotNetResult = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $noDotNet, '-Configuration', 'Debug', '-Stage', 'All')
    Assert-True ($noDotNetResult.Code -eq 2) "An unset dotnet must exit 2, got '$($noDotNetResult.Code)': $($noDotNetResult.Output)"
    Assert-True ($noDotNetResult.Output -match '(?i)dotnet') "The failure must name the missing tool, got: $($noDotNetResult.Output)"

    # 2. All: the eight events appear strictly as Refresh->Generate->Configure->Native->Engine->DataCopyer->Stage->Apk.
    #    The ELF gate follows Native, so this case needs real arm64 artifacts; skip explicitly when the NDK is absent.
    if ($elfClang.Length -eq 0) {
        $script:skips += 'All-stage ordering: a real Android NDK clang.exe was not found (fill cache/config/android-toolchain.json).'
    }
    else {
        $source = Join-Path $tempRoot 'probe.cpp'
        Set-Content -LiteralPath $source -Encoding ASCII -Value 'extern "C" int titan_probe(void) { return 1; }'
        foreach ($configuration in @('debug', 'release')) {
            $target = Join-Path $repository "binaries\android\$configuration\libCore.Android.so"
            & $elfClang ('--target=aarch64-linux-android28') '-shared' '-fPIC' '-Wl,-z,max-page-size=16384' '-o' $target $source
            Assert-True ($LASTEXITCODE -eq 0) "clang could not build the native fixture for '$configuration' (exit $LASTEXITCODE)"
        }

        Clear-Events $fakeRoot
        $all = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
            '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'All')
        Assert-True ($all.Code -eq 0) "Stage All must succeed, got '$($all.Code)': $($all.Output)"
        $stages = Get-Stages (Get-Events $fakeRoot)
        Assert-True (($stages -join ',') -eq 'Refresh,Generate,Configure,Native,Engine,DataCopyer,ApkPrecheck,Apk') `
            "Stage All must run every stage in order, got '$($stages -join ',')'"
        Assert-True (@(Get-Events $fakeRoot | Where-Object { $_ -match '^apk-precheck=present$' }).Count -eq 1) 'The staged content must exist before the APK build starts'
        # Under Debug the SDK does not embed assemblies by default (FastDev); the APK must request embedding explicitly or the artifact cannot install standalone.
        Assert-True (@(Get-Events $fakeRoot | Where-Object { $_ -match 'EmbedAssembliesIntoApk=true' }).Count -ge 1) `
            'The APK build must explicitly embed assemblies into the APK'

        # Native intermediates must land in intermediate/cpp/<Project>/<Config>, never written back into the Core.Android source directory.
        $configureEvent = @(Get-Events $fakeRoot | Where-Object { $_ -match '^cmake -S ' })[0]
        Assert-True ($configureEvent -match [regex]::Escape('intermediate\cpp\Core.Android\Debug')) `
            "The configure build dir must be intermediate\\cpp\\Core.Android\\Debug, got '$configureEvent'"
        Assert-True ($configureEvent -notmatch [regex]::Escape('Core.Android\build')) `
            "The configure build dir must stay out of the source tree, got '$configureEvent'"

        $apkPathFile = Join-Path $repository 'cache\android_build\debug.apk-path.txt'
        Assert-True (Test-Path -LiteralPath $apkPathFile -PathType Leaf) "Stage All must record the APK path in '$apkPathFile'"
        $apkPath = (Get-Content -Raw -LiteralPath $apkPathFile).Trim()
        Assert-True ($apkPath -like '*-Signed.apk') "The recorded APK path must point at the signed APK, got '$apkPath'"
        Assert-True (Test-Path -LiteralPath $apkPath -PathType Leaf) "The recorded APK must exist: '$apkPath'"

        # 3. Native failure: must stop at that stage; none of Engine/DataCopyer/Apk may run afterwards.
        Set-Content -LiteralPath (Join-Path $fakeRoot 'fail-native.txt') -Encoding ASCII -Value 'fail'
        Clear-Events $fakeRoot
        $nativeFailure = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
            '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'All')
        Assert-True ($nativeFailure.Code -eq 1) "A failing native build must exit 1, got '$($nativeFailure.Code)': $($nativeFailure.Output)"
        Assert-True ($nativeFailure.Output -match '(?i)native') "The failure must name the failing stage, got: $($nativeFailure.Output)"
        $failedStages = Get-Stages (Get-Events $fakeRoot)
        Assert-True (($failedStages -join ',') -eq 'Refresh,Generate,Configure,Native') `
            "A failing native build must not continue to Engine/DataCopyer/Apk, got '$($failedStages -join ',')'"
        Remove-Item -LiteralPath (Join-Path $fakeRoot 'fail-native.txt') -Force
    }

    # 4. Managed: managed layer only; CMake must not rerun.
    Clear-Events $fakeRoot
    Set-Content -LiteralPath $nativeLibrary -Encoding ASCII -Value 'not-an-elf'
    $managed = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'Managed')
    Assert-True ($managed.Code -eq 0) "Stage Managed must succeed, got '$($managed.Code)': $($managed.Output)"
    $managedStages = Get-Stages (Get-Events $fakeRoot)
    Assert-True (($managedStages -join ',') -eq 'Engine,DataCopyer') `
        "Stage Managed must only build the engine and the DataCopyer, got '$($managedStages -join ',')'"
    Assert-True (@(Get-Events $fakeRoot | Where-Object { $_ -match '^cmake ' -and $_ -notmatch '--version' }).Count -eq 0) `
        'Stage Managed must not configure or build native code'

    # 5. Managed with missing native prerequisite: must fail before running dotnet.
    Clear-Events $fakeRoot
    Remove-Item -LiteralPath $nativeLibrary -Force
    $managedWithoutNative = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'Managed')
    Assert-True ($managedWithoutNative.Code -eq 1) "Stage Managed without the native library must exit 1, got '$($managedWithoutNative.Code)'"
    Assert-True ($managedWithoutNative.Output -match '(?i)native') "The failure must name the missing native library, got: $($managedWithoutNative.Output)"
    Assert-True (@(Get-Stages (Get-Events $fakeRoot)).Count -eq 0) 'No tool must run when the native prerequisite is missing'

    # 6. Apk: no S1 rerun, just packaging and recording the APK path.
    Set-Content -LiteralPath $nativeLibrary -Encoding ASCII -Value 'not-an-elf'
    if (Test-Path -LiteralPath (Join-Path $repository 'Module\AloneGame.Maui\bin')) {
        Remove-Item -LiteralPath (Join-Path $repository 'Module\AloneGame.Maui\bin') -Recurse -Force
    }
    Clear-Events $fakeRoot
    $apk = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'Apk')
    Assert-True ($apk.Code -eq 0) "Stage Apk must succeed, got '$($apk.Code)': $($apk.Output)"
    $apkStages = Get-Stages (Get-Events $fakeRoot)
    Assert-True (($apkStages -join ',') -eq 'ApkPrecheck,Apk') `
        "Stage Apk must not rebuild the engine or the DataCopyer, got '$($apkStages -join ',')'"

    # 7. Apk without S1 state: must demand Managed first, not package from stale state.
    $statePath = Join-Path $repository 'codegen\Android\DataCopyer\.datacopyer.state.json'
    $savedState = Get-Content -Raw -LiteralPath $statePath
    Write-Utf8Text $statePath '{"Platform":"Android","Configuration":"Release","Hash":"HASH-Release"}'
    Clear-Events $fakeRoot
    $staleState = Invoke-ChildPowerShell @('-File', $buildScript, '-RepositoryRoot', $repository,
        '-ToolchainConfig', $configPath, '-Configuration', 'Debug', '-Stage', 'Apk')
    Assert-True ($staleState.Code -eq 1) "A Release state must not be packaged as Debug, got '$($staleState.Code)'"
    Assert-True ($staleState.Output -match '(?i)configuration') "The failure must complain about the configuration, got: $($staleState.Output)"
    Set-Content -LiteralPath $statePath -Value $savedState -Encoding UTF8

    # 8. Staging script: same input gives the same build-id, changed content gives a new build-id, out-of-root directories are refused, empty patterns are refused.
    $stageRoot = Join-Path $tempRoot 'stage'
    $stageOutput = Join-Path $tempRoot 'stage-out'
    Assert-ThrowsLike {
        & $stageScript -RepositoryRoot $stageRoot -OutputDirectory $stageOutput -Configuration Debug
    } @('intermediate\\android\\content') 'The staging script must refuse an output directory outside intermediate/android/content/<Configuration>'

    # The old location (Module/AloneGame.Maui/obj/titan-content) must be refused: intermediates only land under intermediate.
    Assert-ThrowsLike {
        & $stageScript -RepositoryRoot $repository -OutputDirectory (Join-Path $repository 'Module\AloneGame.Maui\obj\titan-content') -Configuration Debug
    } @('intermediate\\android\\content') 'The staging script must refuse the legacy AloneGame.Maui/obj/titan-content output directory'

    $allowedOutput = Join-Path $repository 'intermediate\android\content\Debug'
    & $stageScript -RepositoryRoot $repository -OutputDirectory $allowedOutput -Configuration Debug
    $firstManifest = (Get-Content -LiteralPath (Join-Path $allowedOutput 'manifest.txt')) -join "`n"
    $firstBuildId = (Get-Content -LiteralPath (Join-Path $allowedOutput 'manifest.txt'))[0]
    & $stageScript -RepositoryRoot $repository -OutputDirectory $allowedOutput -Configuration Debug
    $secondBuildId = (Get-Content -LiteralPath (Join-Path $allowedOutput 'manifest.txt'))[0]
    Assert-True ($firstBuildId -eq $secondBuildId) "Staging the same inputs twice must keep the build id, got '$firstBuildId' then '$secondBuildId'"
    Assert-True ($firstManifest -match 'binaries/Plugins/DataCopyer/DataCopyer.All\.dll') 'The staged manifest must list the Android DataCopyer DLL'
    Assert-True ($firstManifest -match 'binaries/Plugins/DataCopyer\.plugin') 'The staged manifest must list the Android plugin descriptor'
    Assert-True ($firstManifest -notmatch '(?i)Core\.Window|Engine\.Window') 'The staged manifest must not reference Windows artifacts'
    Assert-True (Test-Path -LiteralPath (Join-Path $allowedOutput 'datacopyer.hash')) 'The staged content must carry the DataCopyer hash'
    Assert-True (Test-Path -LiteralPath (Join-Path $allowedOutput 'content\config\engineconfig_android_smoke.jscfg')) 'The staged content must contain the engine config'
    Assert-True (Test-Path -LiteralPath (Join-Path $allowedOutput 'enginecontent\metadata\engine.meta')) 'The staged content must contain engine metadata'

    Add-Content -LiteralPath (Join-Path $repository 'binaries\android\Plugins\DataCopyer\DataCopyer.All.dll') -Value 'changed'
    & $stageScript -RepositoryRoot $repository -OutputDirectory $allowedOutput -Configuration Debug
    $changedBuildId = (Get-Content -LiteralPath (Join-Path $allowedOutput 'manifest.txt'))[0]
    Assert-True ($changedBuildId -ne $firstBuildId) "Changing a staged DLL must change the build id, got '$changedBuildId'"

    $emptyPatternRepo = Join-Path $tempRoot 'empty-pattern'
    New-Item -ItemType Directory -Force -Path (Join-Path $emptyPatternRepo 'Module\AloneGame.Maui') | Out-Null
    Write-Utf8Text (Join-Path $emptyPatternRepo 'Module\AloneGame.Maui\TitanContent.txt') 'content/absent/**'
    New-Item -ItemType Directory -Force -Path (Join-Path $emptyPatternRepo 'content') | Out-Null
    Assert-ThrowsLike {
        & $stageScript -RepositoryRoot $emptyPatternRepo -OutputDirectory (Join-Path $emptyPatternRepo 'intermediate\android\content\Debug') -Configuration Debug
    } @('content/absent') 'A content pattern with no match must fail with the pattern text'

    $traversalRepo = Join-Path $tempRoot 'traversal'
    New-Item -ItemType Directory -Force -Path (Join-Path $traversalRepo 'Module\AloneGame.Maui') | Out-Null
    Write-Utf8Text (Join-Path $traversalRepo 'Module\AloneGame.Maui\TitanContent.txt') 'content/../outside.txt'
    New-Item -ItemType Directory -Force -Path (Join-Path $traversalRepo 'content') | Out-Null
    Set-Content -LiteralPath (Join-Path $traversalRepo 'outside.txt') -Encoding ASCII -Value 'outside'
    Assert-ThrowsLike {
        & $stageScript -RepositoryRoot $traversalRepo -OutputDirectory (Join-Path $traversalRepo 'intermediate\android\content\Debug') -Configuration Debug
    } @('outside|content') 'A pattern escaping its content root must be rejected'

    # 9. MAUI reference evaluation: the engine TargetPath lands exactly in binaries/android/<dependencies.json TFM>, and the MAUI reference's
    #    GlobalPropertiesToRemove actually blocks RuntimeIdentifier/UseMaui/SingleProject propagation.
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        $script:skips += 'MAUI reference evaluation: dotnet was not found.'
    }
    else {
        $engineProject = Join-Path $repoRoot 'Module\Engine.Android\Engine.Android.csproj'
        $mauiProject = Join-Path $repoRoot 'Module\AloneGame.Maui\AloneGame.Maui.csproj'
        $titanTfm = ((Get-Content (Join-Path $repoRoot 'BuildScript\common\dependencies.json') -Raw | ConvertFrom-Json).dotnetSdk.targetFramework)
        $expectedTargetPath = Join-Path $repoRoot ('binaries\android\' + $titanTfm + '\Engine.Android.dll')
        $engineProperties = @('-nologo', $engineProject, '-p:TitanPlatform=Android', '-p:Configuration=Debug',
            '-p:SkipTitanCodeGen=true', '-p:SkipTitanDataCopyerGen=true', '-getProperty:TargetPath')
        $targetPath = (& dotnet msbuild @engineProperties 2>&1 | Select-Object -Last 1)
        $targetPath = ([string]$targetPath).Trim()
        Assert-True ($targetPath -ieq $expectedTargetPath) `
            "Engine.Android TargetPath must be '$expectedTargetPath', got '$targetPath'"

        # RID leakage would rewrite the engine into a <TFM>/android-arm64 subdirectory; the MAUI reference must strip these global properties explicitly.
        $leaked = ([string]((& dotnet msbuild @engineProperties '-p:RuntimeIdentifier=android-arm64' 2>&1) | Select-Object -Last 1)).Trim()
        Assert-True ($leaked -ne $targetPath) `
            'A leaked RuntimeIdentifier must change the engine TargetPath; otherwise GlobalPropertiesToRemove is untested'

        $reference = (& dotnet msbuild '-nologo' '-getItem:ProjectReference' $mauiProject '-p:TargetFramework=net10.0-android' 2>&1) -join "`n"
        Assert-True ($reference -match 'Engine\.Android\.csproj') 'The MAUI project must reference Engine.Android.csproj'
        Assert-True ($reference -match 'TitanPlatform=Android') 'The MAUI engine reference must declare TitanPlatform=Android'
        foreach ($removed in @('RuntimeIdentifier', 'RuntimeIdentifiers', 'UseMaui', 'SingleProject')) {
            Assert-True ($reference -match ('GlobalPropertiesToRemove[^\n]*' + $removed)) `
                "GlobalPropertiesToRemove must strip '$removed' from the engine build"
        }

        # iOS only joins the TFM on macOS; Windows evaluation must not show an iOS target or a second engine reference.
        $frameworks = ([string]((& dotnet msbuild '-nologo' '-getProperty:TargetFrameworks' $mauiProject 2>&1) | Select-Object -Last 1)).Trim()
        Assert-True ($frameworks -notmatch 'ios') "Windows evaluation must not add an iOS target, got '$frameworks'"
    }

    # 10. Android intermediate locations: staged content and the MAUI project obj (including assets/titan copied by the SDK)
    #     must both live under the repo intermediate, never written back into Module/AloneGame.Maui/obj.
    #     AndroidAssetDirectory is only assigned at targets execution time and unavailable during evaluation, so pin IntermediateOutputPath only
    #     (the SDK's AndroidAssetDirectory is $(IntermediateOutputPath)assets\); the real artifact location is verified by the APK build.
    $mauiProject = Join-Path $repoRoot 'Module\AloneGame.Maui\AloneGame.Maui.csproj'
    $layout = [string]((& dotnet msbuild '-nologo' $mauiProject '-p:TargetFramework=net10.0-android' '-p:Configuration=Debug' `
        '-getProperty:BaseIntermediateOutputPath,IntermediateOutputPath,TitanAndroidContentDirectory' 2>&1) -join "`n")
    $layoutPath = Join-Path $tempRoot 'maui-layout.json'
    Set-Content -LiteralPath $layoutPath -Value $layout -Encoding UTF8
    $props = (Get-Content -Raw -LiteralPath $layoutPath | ConvertFrom-Json).Properties
    $intermediateRoot = (Join-Path $repoRoot 'intermediate\android').ToLowerInvariant()
    $baseIntermediate = ([string]$props.BaseIntermediateOutputPath).Replace('/', '\').ToLowerInvariant()
    $intermediate = ([string]$props.IntermediateOutputPath).Replace('/', '\').ToLowerInvariant()
    $contentDirectory = ([string]$props.TitanAndroidContentDirectory).Replace('/', '\').ToLowerInvariant()

    Assert-True ($baseIntermediate.StartsWith($intermediateRoot)) `
        "BaseIntermediateOutputPath must live under intermediate\\android, got '$baseIntermediate'"
    Assert-True ($baseIntermediate -like '*\maui\obj\') `
        "BaseIntermediateOutputPath must be intermediate\\android\\maui\\obj\\, got '$baseIntermediate'"
    Assert-True ($intermediate -eq ($baseIntermediate + 'debug\net10.0-android\')) `
        "IntermediateOutputPath must be <BaseIntermediateOutputPath>Debug\\net10.0-android\\, got '$intermediate'"
    Assert-True ($contentDirectory -eq ((Join-Path $repoRoot 'intermediate\android\content\debug').ToLowerInvariant() + '\')) `
        "TitanAndroidContentDirectory must be intermediate\\android\\content\\Debug, got '$contentDirectory'"

    # 11. Stage markers: build-android.ps1 must emit machine-readable
    #     [TITAN-STAGE] lines at the start of the four stage blocks (the editor panel draws step progress from them),
    #     ordered Native -> Managed -> Content -> Apk.
    $buildText = (Get-Content -Raw -LiteralPath $buildScript)
    $markerOrder = @('Native', 'Managed', 'Content', 'Apk') | ForEach-Object { $buildText.IndexOf("[TITAN-STAGE] $_") }
    Assert-True (-not ($markerOrder -contains -1)) 'build-android.ps1 must emit [TITAN-STAGE] markers for Native/Managed/Content/Apk'
    $sorted = $markerOrder | Sort-Object
    Assert-True (($sorted -join ',') -eq ($markerOrder -join ',')) "[TITAN-STAGE] markers must appear in order Native,Managed,Content,Apk, got offsets $($markerOrder -join ',')"

    # 12. Sub-step and per-entry content transfer progress: Invoke-AndroidCommand prints [TITAN-STEP] before each call,
    #     Stage-AndroidContent prints [TITAN-CONTENT] after each manifest pattern match,
    #     and the panel log shows fine-grained asset-preparation/transfer progress from them.
    $toolsText = (Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'BuildScript\android\AndroidTools.ps1'))
    Assert-True ($toolsText -match '\[TITAN-STEP\]') 'Invoke-AndroidCommand must print a [TITAN-STEP] line before running the command'
    $contentScriptText = (Get-Content -Raw -LiteralPath $stageScript)
    Assert-True ($contentScriptText -match '\[TITAN-CONTENT\]') 'Stage-AndroidContent.ps1 must print [TITAN-CONTENT] lines per content pattern'

    Write-Host 'AndroidBuild tests passed.'
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

if ($script:failures.Count -gt 0) { exit 1 }
exit 0
