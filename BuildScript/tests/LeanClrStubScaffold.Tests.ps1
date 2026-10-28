#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$scriptPath = Join-Path $repoRoot 'BuildScript\scaffold\New-LeanClrStub.ps1'
$survivorStubPath = Join-Path $repoRoot 'Plugins\Game\Survivor\Survivor.LeanClrStub\Survivor.LeanClrStub.csproj'
$tempRoot = Join-Path $repoRoot ("cache\LeanClrStubScaffold-{0}-{1}" -f $PID, (Get-Random))

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

function Assert-ContainsOnce {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [string]$Expected,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    $count = @(($Content -split '\r?\n') | Where-Object { $_ -like ('*' + $Expected + '*') }).Count
    Assert-True ($count -eq 1) "$Message (actual count: $count)"
}

function Normalize-XmlText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    # 显式按 UTF-8 读取：生成物带 BOM、入库模板不带，默认编码会因 BOM 差异读出不同的中文
    $content = Get-Content -Raw -Encoding UTF8 -LiteralPath $Path
    return (($content -replace "`r`n", "`n") -replace '^\s+|\s+$', '')
}

function New-FakeRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$PluginName,

        [string]$TargetFramework = 'net8.0'
    )

    $pluginRoot = Join-Path $Root "Plugins\Game\$PluginName"
    $hostDirectory = Join-Path $pluginRoot "$PluginName.All"
    $buildScriptDirectory = Join-Path $Root 'BuildScript\leanclr'
    $mainEditorDirectory = Join-Path $Root 'Module\MainEditor'
    New-Item -ItemType Directory -Force -Path $hostDirectory, $buildScriptDirectory, $mainEditorDirectory | Out-Null

    Set-Content -LiteralPath (Join-Path $pluginRoot "$PluginName.plugin") -Value '{}' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $buildScriptDirectory 'LeanClrPlugin.targets') -Value '<Project />' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $buildScriptDirectory 'LeanClrStub.targets') -Value '<Project />' -Encoding UTF8

    $hostProjectPath = Join-Path $hostDirectory "$PluginName.All.csproj"
    $hostContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$TargetFramework</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
</Project>
"@
    Set-Content -LiteralPath $hostProjectPath -Value $hostContent -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $mainEditorDirectory 'MainEditor.csproj') -Value $hostContent -Encoding UTF8

    $solutionPath = Join-Path $Root 'EngineAll.sln'
    $solutionContent = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Game", "Game", "{11111111-1111-1111-1111-111111111111}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$PluginName.All", "Plugins\Game\$PluginName\$PluginName.All\$PluginName.All.csproj", "{22222222-2222-2222-2222-222222222222}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MainEditor", "Module\MainEditor\MainEditor.csproj", "{33333333-3333-3333-3333-333333333333}"
	ProjectSection(ProjectDependencies) = postProject
		{22222222-2222-2222-2222-222222222222} = {22222222-2222-2222-2222-222222222222}
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{22222222-2222-2222-2222-222222222222}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{22222222-2222-2222-2222-222222222222}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{22222222-2222-2222-2222-222222222222} = {11111111-1111-1111-1111-111111111111}
	EndGlobalSection
EndGlobal
"@
    Set-Content -LiteralPath $solutionPath -Value $solutionContent -Encoding UTF8

    return @{
        HostProject = $hostProjectPath
        Solution = $solutionPath
        PluginRoot = $pluginRoot
    }
}

try {
    Assert-True (Test-Path -LiteralPath $scriptPath -PathType Leaf) "LeanCLR stub scaffold script was not found: $scriptPath"
    Assert-True (Test-Path -LiteralPath $survivorStubPath -PathType Leaf) "Survivor LeanCLR stub project was not found: $survivorStubPath"

    # 标准件比对：相同目录深度和插件名必须生成与 Survivor 现有桩工程等价的 XML。
    $survivorRoot = Join-Path $tempRoot 'survivor-template'
    $survivor = New-FakeRepository $survivorRoot 'Survivor'
    & $scriptPath -HostProject $survivor.HostProject -SolutionPath $survivor.Solution
    $generatedSurvivorStub = Join-Path $survivor.PluginRoot 'Survivor.LeanClrStub\Survivor.LeanClrStub.csproj'
    Assert-True (Test-Path -LiteralPath $generatedSurvivorStub -PathType Leaf) 'Survivor template stub project was not generated.'
    Assert-True ((Normalize-XmlText $generatedSurvivorStub) -eq (Normalize-XmlText $survivorStubPath)) 'Generated Survivor stub project differs from the checked-in standard template.'

    # 新插件：补宿主导入、生成桩工程、加入相同 solution folder。
    $sampleRoot = Join-Path $tempRoot 'sample-plugin'
    $sample = New-FakeRepository $sampleRoot 'SampleGame'
    & $scriptPath -HostProject $sample.HostProject -SolutionPath $sample.Solution

    $stubPath = Join-Path $sample.PluginRoot 'SampleGame.LeanClrStub\SampleGame.LeanClrStub.csproj'
    Assert-True (Test-Path -LiteralPath $stubPath -PathType Leaf) 'New plugin stub project was not generated.'

    $hostContent = Get-Content -Raw -LiteralPath $sample.HostProject
    Assert-ContainsOnce $hostContent '<Import Project="..\..\..\..\BuildScript\leanclr\LeanClrPlugin.targets" />' 'Host project must import LeanClrPlugin.targets exactly once.'

    $stubContent = Get-Content -Raw -LiteralPath $stubPath
    Assert-True ($stubContent -like '*<LeanClrHostProject>..\SampleGame.All\SampleGame.All.csproj</LeanClrHostProject>*') 'Stub project must reference its host project.'
    Assert-True ($stubContent -like '*<Import Project="..\..\..\..\BuildScript\leanclr\LeanClrStub.targets" />*') 'Stub project must import LeanClrStub.targets.'

    $solutionContent = Get-Content -Raw -LiteralPath $sample.Solution
    Assert-ContainsOnce $solutionContent 'SampleGame.LeanClrStub.csproj' 'Solution must contain the generated stub project exactly once.'
    $stubGuid = $null
    foreach ($line in ($solutionContent -split '\r?\n')) {
        if ($line -match '^Project\("\{[^}]+\}"\) = "SampleGame\.LeanClrStub", ".+?", "(?<Guid>\{[^}]+\})"$') {
            $stubGuid = $Matches['Guid']
        }
    }
    Assert-True ($null -ne $stubGuid) 'Generated stub project entry was not found in the solution.'
    Assert-ContainsOnce $solutionContent ("$stubGuid = {11111111-1111-1111-1111-111111111111}") 'Generated stub project must share the host solution folder.'
    Assert-ContainsOnce $solutionContent 'GlobalSection(NestedProjects)' 'Solution must contain exactly one NestedProjects section.'
    Assert-ContainsOnce $solutionContent '{22222222-2222-2222-2222-222222222222} = {11111111-1111-1111-1111-111111111111}' 'Existing host solution-folder mapping must be preserved.'

    # 幂等：第二次运行不重复导入、项目或 solution 映射，也不覆盖手工修改。
    Add-Content -LiteralPath $stubPath -Value '<!-- preserve without Force -->' -Encoding UTF8
    $solutionBeforeRerun = Get-Content -Raw -LiteralPath $sample.Solution
    & $scriptPath -HostProject $sample.HostProject -SolutionPath $sample.Solution
    $hostContent = Get-Content -Raw -LiteralPath $sample.HostProject
    $solutionContent = Get-Content -Raw -LiteralPath $sample.Solution
    $stubContent = Get-Content -Raw -LiteralPath $stubPath
    Assert-True ($solutionContent -ceq $solutionBeforeRerun) 'Idempotent run must leave the solution text unchanged.'
    Assert-ContainsOnce $hostContent 'LeanClrPlugin.targets' 'Idempotent run duplicated the host import.'
    Assert-ContainsOnce $solutionContent 'SampleGame.LeanClrStub.csproj' 'Idempotent run duplicated the solution project.'
    Assert-True ($stubContent -like '*preserve without Force*') 'Idempotent run unexpectedly overwrote the existing stub project.'

    # Force：恢复规范模板，但仍不得重复宿主导入和 solution 项目。
    & $scriptPath -HostProject $sample.HostProject -SolutionPath $sample.Solution -Force
    $hostContent = Get-Content -Raw -LiteralPath $sample.HostProject
    $solutionContent = Get-Content -Raw -LiteralPath $sample.Solution
    $stubContent = Get-Content -Raw -LiteralPath $stubPath
    Assert-True ($stubContent -notlike '*preserve without Force*') 'Force did not overwrite the existing stub project.'
    Assert-ContainsOnce $hostContent 'LeanClrPlugin.targets' 'Force run duplicated the host import.'
    Assert-ContainsOnce $solutionContent 'SampleGame.LeanClrStub.csproj' 'Force run duplicated the solution project.'

    # 错误输入：非 net8 宿主必须被拒绝。
    $invalidRoot = Join-Path $tempRoot 'invalid-framework'
    $invalid = New-FakeRepository $invalidRoot 'LegacyGame' 'net7.0'
    $rejected = $false
    try {
        & $scriptPath -HostProject $invalid.HostProject -SolutionPath $invalid.Solution
    }
    catch {
        $rejected = $_.Exception.Message -match 'net8\.0'
    }
    Assert-True $rejected 'A host project that does not target net8.0 must be rejected.'

    Write-Host 'LeanCLR stub scaffold tests passed.'
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
