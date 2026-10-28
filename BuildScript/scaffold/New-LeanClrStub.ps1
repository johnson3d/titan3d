#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$HostProject,

    [string]$SolutionPath = '',

    [switch]$Force,

    [switch]$Build
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '..\common\SolutionTools.ps1')

$hostProjectPath = Get-NormalizedFullPath $HostProject
if (-not (Test-Path -LiteralPath $hostProjectPath -PathType Leaf)) {
    throw "LeanCLR host project was not found: $hostProjectPath"
}
if ($hostProjectPath -notmatch '(?i)\.csproj$') {
    throw "LeanCLR host project must be a .csproj file: $hostProjectPath"
}

if (-not $SolutionPath) {
    $repoRoot = Get-NormalizedFullPath (Join-Path $PSScriptRoot '..\..')
    $solutionFilePath = Join-Path $repoRoot 'EngineAll.sln'
}
else {
    $solutionFilePath = Get-NormalizedFullPath $SolutionPath
    $repoRoot = Get-NormalizedFullPath (Split-Path -Parent $solutionFilePath)
}
if (-not (Test-Path -LiteralPath $solutionFilePath -PathType Leaf)) {
    throw "Solution was not found: $solutionFilePath"
}

$pluginsRoot = Get-NormalizedFullPath (Join-Path $repoRoot 'Plugins')
$pluginsPrefix = $pluginsRoot + '\'
if ($hostProjectPath -notlike ($pluginsPrefix + '*')) {
    throw "LeanCLR host project must be located under '$pluginsRoot': $hostProjectPath"
}

[xml]$hostXml = Get-Content -Raw -LiteralPath $hostProjectPath
$targetFrameworks = @()
foreach ($propertyGroup in @($hostXml.Project.PropertyGroup)) {
    if ($propertyGroup.PSObject.Properties.Name -contains 'TargetFramework') {
        $targetFrameworks += ([string]$propertyGroup.TargetFramework -split ';')
    }
    if ($propertyGroup.PSObject.Properties.Name -contains 'TargetFrameworks') {
        $targetFrameworks += ([string]$propertyGroup.TargetFrameworks -split ';')
    }
}
if (-not ($targetFrameworks -contains 'net8.0')) {
    throw "LeanCLR host project must target net8.0: $hostProjectPath"
}

$allowUnsafe = $false
foreach ($propertyGroup in @($hostXml.Project.PropertyGroup)) {
    if ($propertyGroup.PSObject.Properties.Name -contains 'AllowUnsafeBlocks' -and
        [string]$propertyGroup.AllowUnsafeBlocks -ieq 'true') {
        $allowUnsafe = $true
    }
}
if (-not $allowUnsafe) {
    throw "LeanCLR host project must set <AllowUnsafeBlocks>true</AllowUnsafeBlocks>: $hostProjectPath"
}

$hostDirectory = Split-Path -Parent $hostProjectPath
$pluginRoot = $hostDirectory
$pluginManifest = $null
while ($pluginRoot -like ($pluginsPrefix + '*') -or $pluginRoot -ieq $pluginsRoot) {
    $manifests = @(Get-ChildItem -LiteralPath $pluginRoot -Filter '*.plugin' -File)
    if ($manifests.Count -gt 1) {
        throw "Multiple plugin manifests were found in '$pluginRoot'."
    }
    if ($manifests.Count -eq 1) {
        $pluginManifest = $manifests[0]
        break
    }
    if ($pluginRoot -ieq $pluginsRoot) {
        break
    }
    $pluginRoot = Split-Path -Parent $pluginRoot
}
if ($null -eq $pluginManifest) {
    throw "No .plugin manifest was found above LeanCLR host project: $hostProjectPath"
}

$pluginName = $pluginManifest.Name -replace '\.plugin$', ''
$hostProjectName = (Split-Path -Leaf $hostProjectPath) -replace '\.csproj$', ''
$stubProjectName = "$pluginName.LeanClrStub"
$stubDirectory = Join-Path $pluginRoot $stubProjectName
$stubProjectPath = Join-Path $stubDirectory "$stubProjectName.csproj"
$leanClrPluginTargets = Join-Path $repoRoot 'BuildScript\leanclr\LeanClrPlugin.targets'
$leanClrStubTargets = Join-Path $repoRoot 'BuildScript\leanclr\LeanClrStub.targets'
foreach ($requiredFile in @($leanClrPluginTargets, $leanClrStubTargets)) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Required LeanCLR targets file was not found: $requiredFile"
    }
}

$hasPluginImport = $false
if ($hostXml.Project.PSObject.Properties.Name -contains 'Import') {
    $hasPluginImport = @($hostXml.Project.Import | Where-Object {
        ([string]$_.Project) -match '(^|[\\/])LeanClrPlugin\.targets$'
    }).Count -gt 0
}
if (-not $hasPluginImport) {
    $hostContent = Get-Content -Raw -LiteralPath $hostProjectPath
    $pluginTargetsRelativePath = Get-RelativeFilePath $hostDirectory $leanClrPluginTargets
    $importLine = "  <Import Project=`"$pluginTargetsRelativePath`" />"
    if ($hostContent -notmatch '(?m)^\s*</Project>\s*$') {
        throw "Host project does not contain a closing </Project> element: $hostProjectPath"
    }
    $hostContent = $hostContent -replace '(?m)^\s*</Project>\s*$', ($importLine + "`r`n`r`n</Project>")
    Set-Content -LiteralPath $hostProjectPath -Value $hostContent -Encoding UTF8 -NoNewline
    Write-Host "Added LeanClrPlugin.targets import: $hostProjectPath"
}
else {
    Write-Host "LeanClrPlugin.targets import already exists: $hostProjectPath"
}

if ((Test-Path -LiteralPath $stubProjectPath -PathType Leaf) -and -not $Force) {
    Write-Host "LeanCLR stub project already exists (use -Force to overwrite): $stubProjectPath"
}
else {
    New-Item -ItemType Directory -Force -Path $stubDirectory | Out-Null
    $hostProjectRelativePath = Get-RelativeFilePath $stubDirectory $hostProjectPath
    $stubTargetsRelativePath = Get-RelativeFilePath $stubDirectory $leanClrStubTargets
    $stubContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- 编译 $hostProjectName 生成的 VM 桩源码，产出 $hostProjectName.LeanClrStub.dll，部署到 content\LeanCLR\$pluginName\ 由引擎自动扫描注册 -->
    <LeanClrHostProject>$hostProjectRelativePath</LeanClrHostProject>
    <!-- VM 桩只消费宿主生成的 LeanCLR 源码，不拥有 RPC/Property/Macross 输出。 -->
    <TitanConsumeSourceOutputs>false</TitanConsumeSourceOutputs>
    <!-- Plugins\Directory.Build.targets 的 PreBuild 会清空 TtOutputBasePath；与 $hostProjectName 共用目录会删掉插件 dll -->
    <TtOutputBasePath>`$(TtTitanRoot)binaries\LeanClrStub\`$(TtPluginName)\</TtOutputBasePath>
    <OutDir>`$(TtOutputBasePath)</OutDir>
  </PropertyGroup>
  <Import Project="$stubTargetsRelativePath" />
</Project>
"@
    Set-Content -LiteralPath $stubProjectPath -Value $stubContent -Encoding UTF8
    Write-Host "Generated LeanCLR stub project: $stubProjectPath"
}

$solutionDirectory = Split-Path -Parent $solutionFilePath
$solutionContent = Get-Content -Raw -LiteralPath $solutionFilePath
$hostProjectGuid = Get-SolutionProjectGuid $solutionContent $solutionDirectory $hostProjectPath
if (-not $hostProjectGuid) {
    throw "Host project is not registered in solution '$solutionFilePath': $hostProjectPath"
}
$parentFolderGuid = Get-ProjectSolutionFolderGuid $solutionContent $hostProjectGuid
$stubProjectGuid = Get-SolutionProjectGuid $solutionContent $solutionDirectory $stubProjectPath
if (-not $stubProjectGuid) {
    $dotnetCommand = Get-Command dotnet -ErrorAction Stop
    & $dotnetCommand.Source sln $solutionFilePath add $stubProjectPath --in-root
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet sln failed to add LeanCLR stub project (exit $LASTEXITCODE): $stubProjectPath"
    }
    $solutionContent = Get-Content -Raw -LiteralPath $solutionFilePath
    $stubProjectGuid = Get-SolutionProjectGuid $solutionContent $solutionDirectory $stubProjectPath
    if (-not $stubProjectGuid) {
        throw "LeanCLR stub project was added but could not be located in solution: $stubProjectPath"
    }
    Write-Host "Added LeanCLR stub project to solution: $solutionFilePath"
}
else {
    Write-Host "LeanCLR stub project already exists in solution: $solutionFilePath"
}

if ($parentFolderGuid) {
    Set-ProjectSolutionFolder $solutionFilePath $stubProjectGuid $parentFolderGuid
    Write-Host "Placed LeanCLR stub project in host solution folder: $parentFolderGuid"
}

if ($Build) {
    $dotnetCommand = Get-Command dotnet -ErrorAction Stop
    & $dotnetCommand.Source build $stubProjectPath --configuration Debug "/p:SolutionDir=$repoRoot\" "/p:TitanRoot=$repoRoot\"
    if ($LASTEXITCODE -ne 0) {
        throw "LeanCLR stub project build failed (exit $LASTEXITCODE): $stubProjectPath"
    }
}

Write-Host ''
Write-Host 'LeanCLR stub scaffold completed.'
Write-Host "  Host: $hostProjectPath"
Write-Host "  Stub: $stubProjectPath"
Write-Host "  Solution: $solutionFilePath"
