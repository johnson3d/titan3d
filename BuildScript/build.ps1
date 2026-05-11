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

. "$PSScriptRoot\common.ps1"

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

$generatorToolProjects = @(
    'Module\CppWeavingTools\CppWeavingTools.csproj',
    'Module\CSharpCodeTools\CSharpCodeTools.csproj'
)

$analyzerProjects = @(
    'Module\CompilingGenerator\CompilingGenerator.csproj',
    'Module\TtEngineLints\TtEngineLints.csproj'
)

$additionalToolProjects = @(
    'Module\CSharpCompiler\CSharpCompiler.csproj',
    'Module\GameBuilder\GameBuilder.csproj',
    'Module\TitanCMD\TitanCMD.csproj'
)

$managedProjects = @(
    'Module\Engine.Window\Engine.Window.csproj',
    'Module\MainEditor\MainEditor.csproj'
)

$pluginProjects = @(
    'Plugins\GameCore\Inventory\Inventory.All\Inventory.All.csproj',
    'Plugins\Game\Survivor\Survivor.All\Survivor.All.csproj',
    'Plugins\GameItems\GameItems.All\GameItems.All.csproj',
    'Plugins\GameTasks\GameTasks.All\GameTasks.All.csproj',
    'Plugins\RpcCaller\RpcCaller.Window\RpcCaller.Window.csproj',
    'Plugins\GameServer\GameServer.Window\GameServer.Window.csproj',
    'Plugins\GameServer\Root\RootServer.Window\RootServer.Window.csproj',
    'Plugins\GameServer\Login\LoginServer.Window\LoginServer.Window.csproj',
    'Plugins\GameServer\Gate\GateServer.Window\GateServer.Window.csproj',
    'Plugins\GameServer\Level\LevelServer.Window\LevelServer.Window.csproj',
    'Plugins\GameServer\ClientRobot\ClientRobot.Window\ClientRobot.Window.csproj',
    'Plugins\AIGC\MCPServer\MCPServer.All\MCPServer.All.csproj',
    'Plugins\AIGC\TencentAIGC\TencentAIGC.All\TencentAIGC.All.csproj',
    'Plugins\SourceGit\SourceGit.Window\SourceGit.Window.csproj',
    'Plugins\VisualStudioPlugin\VisualStudioPlugin.Window\VisualStudioPlugin.Window.csproj',
    'Plugins\DataCopyer\DataCopyer.All\DataCopyer.All.csproj'
)

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
        '--no-restore'
    )

    Invoke-External $dotnet $args $repoRoot
}

function Invoke-CodeGeneration {
    $toolsDir = Join-Path $repoRoot 'binaries\Tools\net8.0'
    $cppWeaving = Join-Path $toolsDir 'CppWeavingTools.exe'
    $csharpTools = Join-Path $toolsDir 'CSharpCodeTools.exe'

    if (-not (Test-Path -LiteralPath $cppWeaving)) {
        throw "CppWeavingTools was not built at '$cppWeaving'."
    }

    if (-not (Test-Path -LiteralPath $csharpTools)) {
        throw "CSharpCodeTools was not built at '$csharpTools'."
    }

    $nativeBinderDir = Join-Path $repoRoot 'codegen\NativeBinder\'
    Invoke-External $cppWeaving @(
        ('vcxproj=' + (Test-RepoFile 'Core.Window\Core.Window.vcxproj')),
        ('CppOut=' + $nativeBinderDir),
        ('CsOut=' + $nativeBinderDir),
        'ModuleNC=EngineNS.CoreSDK.CoreModule',
        ('Pch=' + (Test-RepoFile 'Core.Window\pch.h')),
        ('TargetCppPOD=' + (Join-Path $nativeBinderDir 'PODStructDefine.h'))
    ) $repoRoot

    Invoke-External $csharpTools @(
        (Test-RepoFile 'Rpc_Engine.txt'),
        'mode=Rpc+AutoSync+Macross'
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
        & "$PSScriptRoot\deps.ps1"
    }
    else {
        & "$PSScriptRoot\deps.ps1" -Restore
    }
}

if (-not $SkipTools) {
    Write-Host ''
    Write-Host "== Build generator tools ($toolConfig) =="
    foreach ($project in $generatorToolProjects) {
        Invoke-DotNetBuild $project $toolConfig
    }
}

if (-not $SkipCodeGen) {
    Write-Host ''
    Write-Host '== Generate NativeBinder/RPC/AutoSync/Macross code =='
    Invoke-CodeGeneration
}

if ($CodeGenOnly) {
    Write-Host ''
    if ($SkipCodeGen) {
        Write-Host 'Code generation skipped.'
    }
    else {
        Write-Host 'Code generation completed.'
    }
    return
}

if (-not $SkipNative) {
    Write-Host ''
    Write-Host '== Build Core.Window x64 =='
    foreach ($config in $configs) {
        Invoke-External $msbuild @(
            (Test-RepoFile 'Core.Window\Core.Window.vcxproj'),
            '/m',
            '/nr:false',
            '/restore',
            '/t:Build',
            ('/p:Configuration=' + $config),
            ('/p:SolutionDir=' + $repoRoot + '\'),
            '/p:Platform=x64'
        ) $repoRoot
        Copy-NativeRuntimeDependencies $config
    }
}

if (-not $SkipManaged) {
    if (-not $SkipTools) {
        Write-Host ''
        Write-Host '== Build analyzer/tool support projects =='
        foreach ($config in $configs) {
            foreach ($project in $analyzerProjects) {
                Invoke-DotNetBuild $project $config
            }
        }
    }

    Write-Host ''
    Write-Host '== Build Engine.Window and MainEditor =='
    foreach ($config in $configs) {
        foreach ($project in $managedProjects) {
            Invoke-DotNetBuild $project $config
        }
    }

    if (-not $SkipTools) {
        Write-Host ''
        Write-Host '== Build tool projects =='
        foreach ($config in $configs) {
            foreach ($project in $additionalToolProjects) {
                Invoke-DotNetBuild $project $config
            }
        }
    }

    if (-not $SkipPlugins) {
        Write-Host ''
        Write-Host '== Build Windows plugin projects =='
        foreach ($config in $configs) {
            foreach ($project in $pluginProjects) {
                Invoke-DotNetBuild $project $config
            }
        }
    }
}

Write-Host ''
Write-Host 'Windows build completed.'
