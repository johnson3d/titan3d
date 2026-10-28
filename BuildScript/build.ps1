#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',

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

. "$PSScriptRoot\common\common.ps1"

Assert-WindowsHost

$repoRoot = Get-RepoRoot
$dotnet = Find-DotNet
$nuget = Find-NuGet
$msbuild = Find-Vs2022MsBuild
Import-VsDevEnvironment $msbuild
#$llvmBin = Find-VsLlvmBin $msbuild
#Add-PathEntry $llvmBin

$configs = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
$toolConfig = if ($Configuration -eq 'Release') { 'Release' } else { 'Debug' }

# 两种生成工具独立编译自身源码；完成自举后才按配置执行生成。
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
# DataCopyer.All 依赖反射阶段生成的 Copyer 与插件引用清单，由 TitanRunDataCopyerStage 在其他插件之后构建。
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
        [string]$BuildConfiguration
    )

    $projectPath = Test-RepoFile $Project
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
    )

    Invoke-External $dotnet $args $repoRoot
}

function Get-GeneratorTool {
    param([Parameter(Mandatory = $true)][string]$Name)

    $tool = Join-Path $repoRoot ('binaries\Tools\net8.0\' + $Name + '.exe')
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
    Write-Host "== Build CppWeavingTools ($toolConfig) =="
    Invoke-DotNetBuild $cppWeavingProject $toolConfig
    Write-Host "== Build CSharpCodeTools ($toolConfig) =="
    Invoke-DotNetBuild $csharpCodeToolsProject $toolConfig
}

# 同一平台输出根逐配置串行消费，避免 All 先发布 Release 却编译 Debug。
foreach ($config in $configs) {
    if (-not $SkipCodeGen) {
        Write-Host ''
        Write-Host "== Generate NativeBinder code ($config) =="
        Invoke-CppWeaving $config
        Write-Host "== Generate RPC/AutoSync/Macross code ($config) =="
        Invoke-CSharpCodeTools $config
    }

    if ($CodeGenOnly) {
        continue
    }

    if (-not $SkipNative) {
        Write-Host ''
        Write-Host "== Build LeanCLR Windows x64 static libraries ($config) =="
        & "$PSScriptRoot\leanclr\build-leanclr.ps1" -Configuration $config

        Write-Host "== Build Core.Window x64 ($config) =="
        Invoke-External $msbuild @(
            (Test-RepoFile 'Core.Window\Core.Window.vcxproj'),
            '/m',
            '/nr:false',
            '/restore',
            '/t:Build',
            ('/p:Configuration=' + $config),
            ('/p:SolutionDir=' + $repoRoot + '\'),
            ('/p:TitanRoot=' + $repoRoot + '\'),
            '/p:TitanPlatform=Windows',
            '/p:SkipTitanCodeGen=true',
            '/p:Platform=x64'
        ) $repoRoot
        Copy-NativeRuntimeDependencies $config
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

if ($CodeGenOnly) {
    Write-Host ''
    if ($SkipCodeGen) { Write-Host 'Code generation skipped.' }
    else { Write-Host 'Code generation completed.' }
    return
}

Write-Host ''
Write-Host 'Windows build completed.'
