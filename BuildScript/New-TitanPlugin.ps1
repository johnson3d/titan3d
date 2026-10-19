#requires -Version 5.1
# 新建 Titan 插件工程：生成 .plugin / shproj / projitems / Plugin.cs 与各平台 csproj，并登记到 EngineAll.sln。
# 目录形态为 Plugins\[Category]\<Folder>\<Name>.plugin；Category 可多级（'A/B' 或 'A\B'），Folder 缺省等于 Name。
# 用 -File 启动时数组参数会以 "A,B" 单个字符串传入，脚本内统一按逗号拆分。
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Name,

    [string]$Category = '',

    [string]$Folder = '',

    [string[]]$Platforms = @('All'),

    [string[]]$Dependencies = @(),

    [switch]$NoLoadOnInit,

    [switch]$LeanClr,

    [switch]$Build,

    [string]$SolutionPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'SolutionTools.ps1')

function Split-ListArgument {
    param(
        [AllowNull()]
        [AllowEmptyCollection()]
        [string[]]$Values
    )

    foreach ($value in @($Values)) {
        foreach ($item in ([string]$value -split ',')) {
            if ($item.Trim()) {
                $item.Trim()
            }
        }
    }
}

function Write-TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Content,

        [switch]$NoBom
    )

    $encoding = 'UTF8'
    if ($NoBom) {
        $encoding = 'ASCII'
    }
    elseif ($PSVersionTable.PSVersion.Major -ge 6) {
        $encoding = 'utf8BOM'
    }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    Set-Content -LiteralPath $Path -Value $Content -Encoding $encoding
}

function Get-SharedImportGuid {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Line
    )

    return ($Line -split '\*')[1]
}

$identifierPattern = '^[A-Za-z_][A-Za-z0-9_]*$'
if ($Name -notmatch $identifierPattern) {
    throw "Invalid plugin Name '$Name': use letters, digits and '_' only, not starting with a digit."
}
if ($Folder -and $Folder -notmatch $identifierPattern) {
    throw "Invalid Folder '$Folder': use a single folder name made of letters, digits and '_'."
}
if (-not $Folder) {
    $Folder = $Name
}
$categorySegments = @()
if ($Category) {
    $categorySegments = @($Category -split '[\\/]')
    foreach ($segment in $categorySegments) {
        if ($segment -notmatch $identifierPattern) {
            throw "Invalid Category '$Category': each level must be a folder name made of letters, digits and '_', separated by '/' or '\'."
        }
    }
}

# 平台：All 与具体平台互斥；输出顺序固定为 All / Windows / Android。
$platformTable = [ordered]@{
    All = @{ Descriptor = 'PLTF_ALL'; Host = 'All' }
    Windows = @{ Descriptor = 'PLTF_Windows'; Host = 'Window' }
    Android = @{ Descriptor = 'PLTF_Android'; Host = 'Android' }
}
$requestedPlatforms = @(Split-ListArgument $Platforms)
if ($requestedPlatforms.Count -eq 0) {
    $requestedPlatforms = @('All')
}
foreach ($platform in $requestedPlatforms) {
    if (@($platformTable.Keys | Where-Object { $_ -ieq $platform }).Count -eq 0) {
        throw "Unsupported platform '$platform'. Supported platforms: $($platformTable.Keys -join ', ')."
    }
}
$selectedPlatforms = @($platformTable.Keys | Where-Object { $key = $_; @($requestedPlatforms | Where-Object { $_ -ieq $key }).Count -gt 0 })
if ($selectedPlatforms -contains 'All' -and $selectedPlatforms.Count -gt 1) {
    throw "Platform 'All' cannot be combined with other platforms."
}
$hostNames = @($selectedPlatforms | ForEach-Object { $platformTable[$_].Host })
$descriptorPlatforms = @($selectedPlatforms | ForEach-Object { $platformTable[$_].Descriptor })
if ($LeanClr -and -not ($hostNames -contains 'All' -or $hostNames -contains 'Window')) {
    throw '-LeanClr requires an All or Windows project to host the LeanCLR stub.'
}

if (-not $SolutionPath) {
    $SolutionPath = Join-Path $PSScriptRoot '..\EngineAll.sln'
}
if (-not (Test-Path -LiteralPath $SolutionPath -PathType Leaf)) {
    throw "Solution was not found: $SolutionPath"
}
$solutionFilePath = Get-NormalizedFullPath $SolutionPath
$repoRoot = Split-Path -Parent $solutionFilePath
$pluginsRoot = Join-Path $repoRoot 'Plugins'
if (-not (Test-Path -LiteralPath $pluginsRoot -PathType Container)) {
    throw "Plugins directory was not found: $pluginsRoot"
}
$pluginsRoot = Get-NormalizedFullPath $pluginsRoot

# 已有插件：插件名取 .plugin 文件名；编译输出目录里的拷贝不算。
$existingPlugins = @{}
foreach ($file in @(Get-ChildItem -LiteralPath $pluginsRoot -Recurse -File -Filter '*.plugin')) {
    $relative = $file.FullName.Substring($pluginsRoot.Length)
    if ($relative -match '(?i)\\(binaries|intermediate|bin|obj)\\') {
        continue
    }
    $existingPlugins[$file.BaseName.ToLowerInvariant()] = @{ Name = $file.BaseName; Path = $file.FullName }
}
if ($existingPlugins.ContainsKey($Name.ToLowerInvariant())) {
    throw "Plugin '$Name' already exists: $($existingPlugins[$Name.ToLowerInvariant()].Path)"
}

$relativeRoot = 'Plugins'
foreach ($segment in $categorySegments) {
    $relativeRoot += "\$segment"
}
$relativeRoot += "\$Folder"
$pluginRoot = Join-Path $repoRoot $relativeRoot
if (Test-Path -LiteralPath $pluginRoot) {
    throw "Target directory already exists: $pluginRoot"
}

$dependencyNames = @()
foreach ($dependency in @(Split-ListArgument $Dependencies)) {
    $key = $dependency.ToLowerInvariant()
    if (-not $existingPlugins.ContainsKey($key)) {
        throw "Dependency '$dependency' was not found under $pluginsRoot."
    }
    if ($dependencyNames -notcontains $existingPlugins[$key].Name) {
        $dependencyNames += $existingPlugins[$key].Name
    }
}

# 先在内存里算好 sln 改动，任何失败都发生在写文件之前。
$lines = Read-SolutionLines $solutionFilePath
$pluginsFolderGuid = Get-SolutionFolderGuid $lines 'Plugins'
if (-not $pluginsFolderGuid) {
    throw "Solution does not contain a top-level 'Plugins' solution folder: $solutionFilePath"
}

$nested = @{}
$nestedOrder = @()
$parentGuid = $pluginsFolderGuid
# Category 逐级复用同名 solution folder，缺失的层级新建并挂到上一级。
foreach ($segment in $categorySegments) {
    $categoryGuid = Get-SolutionFolderGuid $lines $segment $parentGuid
    if (-not $categoryGuid) {
        $categoryGuid = '{' + (New-Guid).Guid.ToUpperInvariant() + '}'
        $lines = Add-SolutionProjectEntry $lines $script:SolutionFolderTypeGuid $segment $segment $categoryGuid
        $nested[$categoryGuid] = $parentGuid
        $nestedOrder += $categoryGuid
    }
    $parentGuid = $categoryGuid
}
# 同级已有同名 solution folder 时 VS 无法区分，必须拒绝。
if (Get-SolutionFolderGuid $lines $Folder $parentGuid) {
    throw "Solution folder '$Folder' already exists under '$(@('Plugins') + $categorySegments -join '/')' in $solutionFilePath."
}

$pluginFolderGuid = '{' + (New-Guid).Guid.ToUpperInvariant() + '}'
$manifestItem = "$relativeRoot\$Name.plugin"
$lines = Add-SolutionProjectEntry $lines $script:SolutionFolderTypeGuid $Folder $Folder $pluginFolderGuid @($manifestItem)
$nested[$pluginFolderGuid] = $parentGuid
$nestedOrder += $pluginFolderGuid

$sharedGuid = (New-Guid).Guid.ToLowerInvariant()
$sharedSolutionGuid = '{' + $sharedGuid.ToUpperInvariant() + '}'
$projitemsRelativePath = "$relativeRoot\$Name\$Name.projitems"
$lines = Add-SolutionProjectEntry $lines $script:SharedProjectTypeGuid $Name "$relativeRoot\$Name\$Name.shproj" $sharedSolutionGuid
$nested[$sharedSolutionGuid] = $pluginFolderGuid
$nestedOrder += $sharedSolutionGuid
$sharedImports = @("$projitemsRelativePath*{$sharedGuid}*SharedItemsImports = 13")

$configLines = @()
$hostProjects = [ordered]@{}
foreach ($hostName in $hostNames) {
    $projectName = "$Name.$hostName"
    $projectGuid = '{' + (New-Guid).Guid.ToUpperInvariant() + '}'
    $lines = Add-SolutionProjectEntry $lines $script:CSharpProjectTypeGuid $projectName "$relativeRoot\$projectName\$projectName.csproj" $projectGuid
    $nested[$projectGuid] = $pluginFolderGuid
    $nestedOrder += $projectGuid
    $configLines += @(Get-SolutionProjectConfigurationLines $lines $projectGuid)
    $sharedImports += "$projitemsRelativePath*$($projectGuid.ToLowerInvariant())*SharedItemsImports = 5"
    $hostProjects[$hostName] = Join-Path $pluginRoot "$projectName\$projectName.csproj"
}

if ($configLines.Count -gt 0) {
    $lines = Set-SolutionSectionLines $lines 'GlobalSection(ProjectConfigurationPlatforms) = postSolution' $configLines
}
$lines = Set-SolutionNestedProjects $lines $nested $nestedOrder
# VS 按 GUID 升序维护 SharedMSBuildProjectFiles，新行插到第一个 GUID 更大的行之前。
$sortedImports = @($sharedImports | Sort-Object { Get-SharedImportGuid $_ })
$lines = Set-SolutionSectionLines $lines 'GlobalSection(SharedMSBuildProjectFiles) = preSolution' $sortedImports {
    param($NewLine, $ExistingLine)
    (Get-SharedImportGuid $NewLine) -lt (Get-SharedImportGuid $ExistingLine)
}

# 生成文件。
$loadOnInit = 'true'
if ($NoLoadOnInit) {
    $loadOnInit = 'false'
}
$platformJson = (@($descriptorPlatforms | ForEach-Object { "    `"$_`"" }) -join ",`r`n")
$dependencyJson = '[]'
if ($dependencyNames.Count -gt 0) {
    $dependencyJson = "[`r`n" + (@($dependencyNames | ForEach-Object { "    `"$_`"" }) -join ",`r`n") + "`r`n  ]"
}
Write-TextFile (Join-Path $pluginRoot "$Name.plugin") -NoBom @"
{
  "Enable": true,
  "LoadOnInit": $loadOnInit,
  "Platforms": [
$platformJson
  ],
  "Dependencies": $dependencyJson
}
"@

$sharedDirectory = Join-Path $pluginRoot $Name
Write-TextFile (Join-Path $sharedDirectory "$Name.shproj") @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup Label="Globals">
    <ProjectGuid>$sharedGuid</ProjectGuid>
    <MinimumVisualStudioVersion>14.0</MinimumVisualStudioVersion>
  </PropertyGroup>
  <Import Project="`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <Import Project="`$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v`$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.Default.props" />
  <Import Project="`$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v`$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.props" />
  <PropertyGroup />
  <Import Project="$Name.projitems" Label="Shared" />
  <Import Project="`$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v`$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.CSharp.targets" />
</Project>
"@

Write-TextFile (Join-Path $sharedDirectory "$Name.projitems") @"
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <MSBuildAllProjects Condition="'`$(MSBuildVersion)' == '' Or '`$(MSBuildVersion)' &lt; '16.0'">`$(MSBuildAllProjects);`$(MSBuildThisFileFullPath)</MSBuildAllProjects>
    <HasSharedItems>true</HasSharedItems>
    <SharedGUID>$sharedGuid</SharedGUID>
  </PropertyGroup>
  <PropertyGroup Label="Configuration">
    <Import_RootNamespace>EngineNS.Plugins.$Name</Import_RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="`$(MSBuildThisFileDirectory)Plugin.cs" />
  </ItemGroup>
</Project>
"@

Write-TextFile (Join-Path $sharedDirectory 'Plugin.cs') @"
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class Tt$($Name)AssemblyDesc : TtAssemblyDesc
        {
            public Tt$($Name)AssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:$Name AssemblyDesc Created");
            }
            ~Tt$($Name)AssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:$Name AssemblyDesc Destroyed");
            }
            public override string Name { get => "$Name"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static Tt$($Name)AssemblyDesc AssmblyDesc = new Tt$($Name)AssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.$Name
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static Tt$($Name)Plugin mPluginObject = new Tt$($Name)Plugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class Tt$($Name)Plugin : EngineNS.Bricks.AssemblyLoader.IPlugin
    {
        public void OnLoadedPlugin()
        {
        }
        public void OnUnloadPlugin()
        {
        }
    }
}
"@

foreach ($hostName in $hostProjects.Keys) {
    $engineName = 'Engine.Window'
    $platformSuffix = 'Window'
    $defineConstants = ''
    if ($hostName -eq 'Android') {
        $engineName = 'Engine.Android'
        $platformSuffix = 'Android'
        $defineConstants = "`r`n    <DefineConstants>`$(DefineConstants);PAndroid</DefineConstants>"
    }
    Write-TextFile $hostProjects[$hostName] @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>$defineConstants
    <BaseOutputPath>`$(TtTitanRoot)\binaries\Plugins</BaseOutputPath>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
  </PropertyGroup>

  <Import Project="..\$Name\$Name.projitems" Label="Shared" />

  <ItemGroup>
    <Reference Include="$engineName">
      <HintPath>`$(TtTitanRoot)binaries\net8.0\$engineName.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="`$(TtTitanRoot)binaries\Plugins\*\*.All.dll" Exclude="`$(TtTitanRoot)binaries\Plugins\$Name\*.dll">
      <Private>false</Private>
    </Reference>
    <Reference Include="`$(TtTitanRoot)binaries\Plugins\*\*.$platformSuffix.dll" Exclude="`$(TtTitanRoot)binaries\Plugins\$Name\*.dll;`$(TtTitanRoot)binaries\Plugins\*\$engineName.dll">
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
"@
}

Write-SolutionLines $solutionFilePath $lines | Out-Null
Write-Host "Created plugin '$Name': $pluginRoot"
foreach ($hostName in $hostProjects.Keys) {
    Write-Host "  Project: $($hostProjects[$hostName])"
}
Write-Host "Registered in solution: $solutionFilePath"

if ($LeanClr) {
    $leanClrHost = $hostProjects['All']
    if (-not $leanClrHost) {
        $leanClrHost = $hostProjects['Window']
    }
    & (Join-Path $PSScriptRoot 'New-LeanClrStub.ps1') -HostProject $leanClrHost -SolutionPath $solutionFilePath
}

if ($hostNames -contains 'Android' -and -not (Test-Path -LiteralPath (Join-Path $repoRoot 'binaries\net8.0\Engine.Android.dll') -PathType Leaf)) {
    Write-Warning 'binaries\net8.0\Engine.Android.dll was not found; build Module\Engine.Android before building the Android project.'
}

if ($Build) {
    $dotnetCommand = Get-Command dotnet -ErrorAction Stop
    foreach ($hostName in $hostProjects.Keys) {
        & $dotnetCommand.Source build $hostProjects[$hostName] --configuration Debug "/p:SolutionDir=$repoRoot\" "/p:TitanRoot=$repoRoot\"
        if ($LASTEXITCODE -ne 0) {
            throw "Plugin project build failed (exit $LASTEXITCODE): $($hostProjects[$hostName])"
        }
    }
}

Write-Host ''
Write-Host "Next: add `"$Name`" to the Plugins list of the jscfg that should load it, then reload EngineAll.sln in Visual Studio."
