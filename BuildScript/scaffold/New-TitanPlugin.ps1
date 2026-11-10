#requires -Version 5.1
# Create a new Titan plugin project: generates .plugin / shproj / projitems / Plugin.cs and per-platform csproj, and registers them in EngineAll.sln.
# Directory layout is Plugins\[Category]\<Folder>\<Name>.plugin; Category may be nested ('A/B' or 'A\B'); Folder defaults to Name.
# When launched with -File, array parameters arrive as a single "A,B" string; the script splits on commas uniformly.
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

. (Join-Path $PSScriptRoot '..\common\SolutionTools.ps1')

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

# Platform: All is mutually exclusive with specific platforms; output order is fixed to All / Windows / Android.
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
    $SolutionPath = Join-Path $PSScriptRoot '..\..\EngineAll.sln'
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

# Existing plugin: the plugin name comes from the .plugin file name; copies in build output directories do not count.
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

# Compute all sln changes in memory first, so any failure happens before writing files.
$lines = Read-SolutionLines $solutionFilePath
$pluginsFolderGuid = Get-SolutionFolderGuid $lines 'Plugins'
if (-not $pluginsFolderGuid) {
    throw "Solution does not contain a top-level 'Plugins' solution folder: $solutionFilePath"
}

$nested = @{}
$nestedOrder = @()
$parentGuid = $pluginsFolderGuid
# Reuse same-named solution folders per Category level; create missing levels and attach them to the parent.
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
# VS cannot distinguish same-named solution folders in the same level, so this must be rejected.
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
# VS keeps SharedMSBuildProjectFiles sorted by GUID ascending; insert the new line before the first line with a larger GUID.
$sortedImports = @($sharedImports | Sort-Object { Get-SharedImportGuid $_ })
$lines = Set-SolutionSectionLines $lines 'GlobalSection(SharedMSBuildProjectFiles) = preSolution' $sortedImports {
    param($NewLine, $ExistingLine)
    (Get-SharedImportGuid $NewLine) -lt (Get-SharedImportGuid $ExistingLine)
}

# Generate files.
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
    $platformImport = ''
    $engineReference = @"
    <Reference Include="$engineName">
      <HintPath>`$(TtTitanRoot)binaries\`$(_TitanTargetFramework)\$engineName.dll</HintPath>
      <Private>false</Private>
    </Reference>
"@
    if ($hostName -eq 'Android') {
        $engineName = 'Engine.Android'
        $platformSuffix = 'Android'
        $defineConstants = "`r`n    <DefineConstants>`$(DefineConstants);PAndroid</DefineConstants>"
        # Android engine output is isolated from desktop (same source as TitanEngineAssemblyPath in TitanManagedProject.props).
        $engineReference = $engineReference.Replace('binaries\$(_TitanTargetFramework)\Engine.Window.dll', 'binaries\android\$(_TitanTargetFramework)\Engine.Android.dll').Replace('Engine.Window', $engineName)
    }
    if ($hostName -eq 'All') {
        $parentPath = '../' * (3 + $categorySegments.Count)
        $platformImport = "  <Import Project=`"${parentPath}BuildScript/msbuild/TitanManagedProject.props`" />"
        $engineName = '`$(TitanEngineAssemblyName)'.Replace('`', '')
        $platformSuffix = '`$(TitanPluginPlatformSuffix)'.Replace('`', '')
        $engineReference = '    <ProjectReference Include="$(TitanEngineProject)" />'
    }
    Write-TextFile $hostProjects[$hostName] @"
<Project Sdk="Microsoft.NET.Sdk">
$platformImport

  <PropertyGroup>
    <!-- TFM follows dependencies.json (Directory.Build.props injects _TitanTargetFramework). -->
    <TargetFramework>`$(_TitanTargetFramework)</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>$defineConstants
    <BaseOutputPath>`$(TtTitanRoot)\binaries\Plugins</BaseOutputPath>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
  </PropertyGroup>

  <Import Project="..\$Name\$Name.projitems" Label="Shared" />

  <ItemGroup>
$engineReference
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

$depsJsonPath = Join-Path $repoRoot 'BuildScript\common\dependencies.json'
if ($hostNames -contains 'Android' -and (Test-Path -LiteralPath $depsJsonPath)) {
    $manifestTfm = ((Get-Content -Raw -LiteralPath $depsJsonPath | ConvertFrom-Json).dotnetSdk.targetFramework)
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot ('binaries\android\' + $manifestTfm + '\Engine.Android.dll')) -PathType Leaf)) {
        Write-Warning ('binaries\android\' + $manifestTfm + '\Engine.Android.dll was not found; build Module\Engine.Android before building the Android project.')
    }
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
