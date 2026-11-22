#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',

    # native Core.Window 的编译配置由 jscfg 的 NativeDll 决定（与 VS 内构建同一规则）；
    # 显式传入等价于 VS 的 /p:TitanNativeDll=debug|release。
    [string]$NativeDll = '',

    [switch]$SkipRestore,
    [switch]$SkipCodeGen,
    [switch]$SkipNative,
    [switch]$SkipManaged,
    [switch]$SkipPlugins,
    [switch]$SkipTools,
    [switch]$SkipDependencyCheck,
    [switch]$CodeGenOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Keep all build output in English so .NET localized text never garbles on non-UTF-8 consoles.
$env:DOTNET_CLI_UI_LANGUAGE = 'en'

. "$PSScriptRoot\common\common.ps1"

Assert-WindowsHost

$repoRoot = Get-RepoRoot
$dotnet = Find-DotNet
$nuget = Find-NuGet
$msbuild = Find-VsMsBuild
Import-VsDevEnvironment $msbuild
#$llvmBin = Find-VsLlvmBin $msbuild
#Add-PathEntry $llvmBin

# Manifests are already isolated under owner+configuration subdirectories, so the order of the two All passes no longer affects manifest validity.
$configs = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
$toolConfig = if ($Configuration -eq 'Release') { 'Release' } else { 'Debug' }

# Core.Window 不再跟随 -Configuration：按 jscfg 的 NativeDll 决定编 Debug 还是 Release，
# 与 VS 内 TitanNativeCore.props 的解析规则完全一致（cache/config/engine.jscfg 覆盖优先）。
$native = Resolve-TitanNativeConfiguration -Override $NativeDll
$nativeConfig = $native.Configuration
$nativeBuilt = $false

# The two generator tools compile their own sources independently; generation per configuration only starts after bootstrap succeeds.
$cppWeavingProject = 'Module\CppWeavingTools\CppWeavingTools.csproj'
$csharpCodeToolsProject = 'Module\CSharpCodeTools\CSharpCodeTools.csproj'

$analyzerProjects = @(
    'Module\CompilingGenerator\CompilingGenerator.csproj',
    'Module\TtEngineLints\TtEngineLints.csproj'
)

$additionalToolProjects = @(
    'Module\CSharpCompiler\CSharpCompiler.csproj',
    'Module\GameBuilder\GameBuilder.csproj',
    'Module\TitanCMD\TitanCMD.csproj',
    'Module\SlateApp\LocalizationTool\LocalizationTool.csproj'
)

$managedProjects = @(
    'Module\Engine.Window\Engine.Window.csproj',
    'Module\MainEditor\MainEditor.csproj'
)

$pluginProjects = @()
$pluginsRoot = Join-Path $repoRoot 'Plugins'
foreach ($pluginFile in (Get-ChildItem -Path $pluginsRoot -Filter '*.plugin' -Recurse)) {
    $pluginDir = $pluginFile.DirectoryName
    $csprojFiles = Get-ChildItem -Path $pluginDir -Filter '*.csproj' -Recurse |
        Where-Object { $_.FullName -notmatch '\\obj\\' }
    foreach ($csproj in $csprojFiles) {
        $relativePath = $csproj.FullName.Substring($repoRoot.Length).TrimStart('\')
        $pluginProjects += $relativePath
    }
}
$pluginProjects = $pluginProjects | Select-Object -Unique
# DataCopyer.All depends on the Copyer and plugin reference manifests produced by the reflection stage, and is built by TitanRunDataCopyerStage after the other plugins.
$pluginProjects = @($pluginProjects | Where-Object { $_ -notmatch '(^|\\)DataCopyer\.All\.csproj$' })

function Invoke-DotNetRestore {
    param([Parameter(Mandatory = $true)][string]$Project)

    if (-not $SkipRestore) {
        Invoke-External $dotnet @(
            'restore',
            (Test-RepoFile $Project),
            ('/p:SolutionDir=' + $repoRoot + '\'),
            ('/p:TitanRoot=' + $repoRoot + '\')
        ) $repoRoot
    }
}

function Invoke-DotNetBuild {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Project,

        [Parameter(Mandatory = $true)]
        [string]$BuildConfiguration,

        [string[]]$ExtraProperties = @()
    )

    $projectPath = Test-RepoFile $Project
    # 上一次构建被强杀时可能留下写了一半的 PDB，之后每次增量构建都会在读符号时失败，先自检一次。
    $null = Repair-CorruptSymbols $projectPath $BuildConfiguration
    Invoke-DotNetRestore $Project

    $args = @(
        'build',
        $projectPath,
        '-c',
        $BuildConfiguration,
        ('/p:SolutionDir=' + $repoRoot + '\'),
        ('/p:TitanRoot=' + $repoRoot + '\'),
        '/p:TitanPlatform=Windows',
        '/p:SkipTitanCodeGen=true',
        '--no-restore'
    ) + $ExtraProperties

    Invoke-External $dotnet $args $repoRoot
}

function Get-GeneratorTool {
    param([Parameter(Mandatory = $true)][string]$Name)

    $tool = Join-Path $repoRoot ('binaries\Tools\' + (Get-TitanTargetFramework) + '\' + $Name + '.exe')
    if (-not (Test-Path -LiteralPath $tool)) {
        throw "$Name was not built at '$tool'."
    }
    return $tool
}

function Invoke-CppWeaving {
    param([Parameter(Mandatory = $true)][string]$BuildConfiguration)

    $cppWeaving = Get-GeneratorTool 'CppWeavingTools'
    $nativeBinderDir = Join-Path $repoRoot 'codegen\Windows\NativeBinder'
    Invoke-External $cppWeaving @(
        ('vcxproj=' + (Test-RepoFile 'Core.Window\Core.Window.vcxproj')),
        ('TitanRoot=' + $repoRoot),
        'Platform=Windows',
        ('Configuration=' + $BuildConfiguration),
        ('CppOut=' + $nativeBinderDir),
        ('CsOut=' + $nativeBinderDir),
        'ModuleNC=EngineNS.CoreSDK.CoreModule',
        ('Pch=' + (Test-RepoFile 'Core.Window\pch.h')),
        ('TargetCppPOD=' + (Join-Path $nativeBinderDir 'PODStructDefine.h'))
    ) $repoRoot
}

function Invoke-CSharpCodeTools {
    param([Parameter(Mandatory = $true)][string]$BuildConfiguration)

    $csharpTools = Get-GeneratorTool 'CSharpCodeTools'
    Invoke-External $csharpTools @(
        (Test-RepoFile 'Rpc_Engine.txt'),
        'mode=Rpc+AutoSync+Macross',
        ('TitanRoot=' + $repoRoot),
        'Platform=Windows',
        ('Configuration=' + $BuildConfiguration),
        ('csproj=' + (Test-RepoFile 'Module\Engine.Window\Engine.Window.csproj'))
    ) $repoRoot
}

function Copy-NativeRuntimeDependencies {
    param([Parameter(Mandatory = $true)][string]$BuildConfiguration)

    $nativeOut = Join-Path $repoRoot ('binaries\' + $BuildConfiguration.ToLowerInvariant())
    if (-not (Test-Path -LiteralPath $nativeOut)) {
        New-Item -ItemType Directory -Force -Path $nativeOut | Out-Null
    }

    $physxConfig = $BuildConfiguration.ToLowerInvariant()
    $gmpBin = if ($BuildConfiguration -eq 'Debug') { 'debug\bin' } else { 'bin' }
    $runtimeFiles = @(
        "3rd\native\CGAL\auxiliary\gmp\$gmpBin\gmp-10.dll",
        "3rd\native\CGAL\auxiliary\gmp\$gmpBin\gmpxx-4.dll",
        "3rd\native\CGAL\auxiliary\gmp\$gmpBin\mpfr-6.dll",
        "3rd\native\PhysX5\bin\win.x86_64.vc143.mt\$physxConfig\PhysX_64.dll",
        "3rd\native\PhysX5\bin\win.x86_64.vc143.mt\$physxConfig\PhysXCommon_64.dll",
        "3rd\native\PhysX5\bin\win.x86_64.vc143.mt\$physxConfig\PhysXCooking_64.dll",
        "3rd\native\PhysX5\bin\win.x86_64.vc143.mt\$physxConfig\PhysXFoundation_64.dll",
        "3rd\native\PhysX5\bin\win.x86_64.vc143.mt\$physxConfig\PVDRuntime_64.dll"
    )

    foreach ($runtimeFile in $runtimeFiles) {
        Copy-Item -LiteralPath (Test-RepoFile $runtimeFile) -Destination $nativeOut -Force
    }

    $openMpDll = if ($BuildConfiguration -eq 'Debug') { 'VCOMP140D.DLL' } else { 'VCOMP140.DLL' }
    $vsRoot = Split-Path (Split-Path (Split-Path (Split-Path $msbuild -Parent) -Parent) -Parent) -Parent
    $redistRoot = Join-Path $vsRoot 'VC\Redist\MSVC'
    $openMpRuntime = Get-ChildItem -Path $redistRoot -Recurse -Filter $openMpDll -ErrorAction SilentlyContinue |
        Where-Object {
            $_.FullName -match '\\x64\\' -and
            $_.FullName -notmatch '\\onecore\\' -and
            (($_.FullName -match '\\debug_nonredist\\') -eq ($BuildConfiguration -eq 'Debug'))
        } |
        Sort-Object FullName -Descending |
        Select-Object -First 1
    if ($null -eq $openMpRuntime) {
        throw "Could not find Visual C++ OpenMP runtime '$openMpDll' under '$redistRoot'."
    }
    Copy-Item -LiteralPath $openMpRuntime.FullName -Destination $nativeOut -Force
}

Write-Host "Titan Windows build"
Write-Host "  Repo:          $repoRoot"
Write-Host "  MSBuild:       $msbuild"
Write-Host "  dotnet:        $dotnet"
Write-Host "  NuGet:         $nuget"
#Write-Host "  LLVM/libclang: $llvmBin"
Write-Host "  Configuration: $($configs -join ', ')"
Write-Host "  NativeDll:     $($native.NativeDll) ($($native.Origin)) -> Core.Window $nativeConfig"
if ($Configuration -ne 'All' -and $Configuration -ne $nativeConfig) {
    Write-Host "  Note:          managed projects build $Configuration, native & codegen build $nativeConfig (jscfg wins, same as Visual Studio)"
}

if (-not $SkipDependencyCheck) {
    Write-Host ''
    Write-Host '== Check external dependencies =='
    if ($SkipRestore) {
        & "$PSScriptRoot\check\deps.ps1"
    }
    else {
        & "$PSScriptRoot\check\deps.ps1" -Restore
    }
}

if (-not $SkipTools) {
    Write-Host ''
    # 工具工程自身的构建输出在 bin/，必须显式部署到 binaries/Tools 才能被 Get-GeneratorTool 找到；
    # 两者分离后，解决方案重新生成不会去覆盖正在运行的生成工具实例。
    $toolDeployProperty = '/p:TitanToolDeployDir=' + $repoRoot + '\binaries\Tools'
    Write-Host "== Build CppWeavingTools ($toolConfig) =="
    Invoke-DotNetBuild $cppWeavingProject $toolConfig -ExtraProperties @($toolDeployProperty)
    Write-Host "== Build CSharpCodeTools ($toolConfig) =="
    Invoke-DotNetBuild $csharpCodeToolsProject $toolConfig -ExtraProperties @($toolDeployProperty)
}

# 代码生成与 native 编译同源，由 jscfg 的 NativeDll 决定配置（本脚本解析出的 $nativeConfig 与
# TitanCodeGen.targets 的 _TitanCodeGenConfiguration 读的是同一份 jscfg），与 -Configuration 无关，
# 因此只在循环外跑一次；-Configuration All 时不再重复生成两遍。
if (-not $SkipCodeGen) {
    Write-Host ''
    # NativeBinder 必须按真正参与编译的 native 配置解析 Core.Window 的宏，
    # CSharpCodeTools 也用同一份配置求值源码，否则命令行与 VS 内的生成物会互相覆盖。
    Write-Host "== Generate NativeBinder code ($nativeConfig) =="
    Invoke-CppWeaving $nativeConfig
    Write-Host "== Generate RPC/AutoSync/Macross code ($nativeConfig) =="
    Invoke-CSharpCodeTools $nativeConfig
}

if ($CodeGenOnly) {
    Write-Host ''
    if ($SkipCodeGen) { Write-Host 'Code generation skipped.' }
    else { Write-Host 'Code generation completed.' }
    return
}

# The same platform output root is consumed serially per configuration, so All publishing Release first does not race with the Debug build.
foreach ($config in $configs) {

    # native 只有一份配置（由 jscfg 决定），-Configuration All 时也只编一次。
    if (-not $SkipNative -and -not $nativeBuilt) {
        Write-Host ''
        Write-Host "== Build LeanCLR Windows x64 static libraries ($nativeConfig) =="
        & "$PSScriptRoot\leanclr\build-leanclr.ps1" -Configuration $nativeConfig

        Write-Host "== Build Core.Window x64 ($nativeConfig, NativeDll=$($native.NativeDll)) =="
        Invoke-External $msbuild @(
            (Test-RepoFile 'Core.Window\Core.Window.vcxproj'),
            '/m',
            '/nr:false',
            '/restore',
            '/t:Build',
            ('/p:Configuration=' + $nativeConfig),
            ('/p:SolutionDir=' + $repoRoot + '\'),
            ('/p:TitanRoot=' + $repoRoot + '\'),
            '/p:TitanPlatform=Windows',
            '/p:SkipTitanCodeGen=true',
            '/p:Platform=x64',
            # Release 开 LTCG，32 位宿主的 HostX86 链接器地址空间不够会报 C1002。
            '/p:PreferredToolArchitecture=x64'
        ) $repoRoot
        Copy-NativeRuntimeDependencies $nativeConfig
        $nativeBuilt = $true
    }

    if (-not $SkipManaged) {
        if (-not $SkipTools) {
            Write-Host ''
            Write-Host "== Build analyzer/tool support projects ($config) =="
            foreach ($project in $analyzerProjects) {
                Invoke-DotNetBuild $project $config
            }
        }

        Write-Host ''
        Write-Host "== Build Engine.Window and MainEditor ($config) =="
        foreach ($project in $managedProjects) {
            Invoke-DotNetBuild $project $config
        }

        if (-not $SkipTools) {
            Write-Host ''
            Write-Host "== Build tool projects ($config) =="
            foreach ($project in $additionalToolProjects) {
                Invoke-DotNetBuild $project $config
            }
        }

        if (-not $SkipPlugins) {
            Write-Host ''
            Write-Host "== Build Windows plugin projects ($config) =="
            foreach ($project in $pluginProjects) {
                Invoke-DotNetBuild $project $config
            }

            Write-Host ''
            Write-Host "== Generate DataCopyer from built Engine.Window ($config) =="
            Invoke-External $dotnet @(
                'msbuild',
                (Test-RepoFile 'Module\Engine.Window\Engine.Window.csproj'),
                '-nologo',
                '-t:TitanRunDataCopyerStage',
                ('-p:Configuration=' + $config),
                ('/p:SolutionDir=' + $repoRoot + '\'),
                ('/p:TitanRoot=' + $repoRoot + '\'),
                '/p:TitanPlatform=Windows'
            ) $repoRoot
        }
    }
}

Write-Host ''
Write-Host 'Windows build completed.'
