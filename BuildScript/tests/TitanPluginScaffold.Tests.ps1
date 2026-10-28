#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$scriptPath = Join-Path $repoRoot 'BuildScript\scaffold\New-TitanPlugin.ps1'
$tempRoot = Join-Path $repoRoot ("cache\TitanPluginScaffold-{0}-{1}" -f $PID, (Get-Random))

$folderType = '{2150E333-8FDC-42A3-9474-1A3956D46DE8}'
$sharedType = '{D954291E-2A0B-460D-934E-DC6B0785DB48}'
$csharpType = '{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}'
$pluginsFolderGuid = '{E5CE3791-9C8E-4476-8528-B81DFB5FDDF5}'
$decoyPluginsFolderGuid = '{AAAAAAAA-0000-0000-0000-000000000002}'
$gameCoreFolderGuid = '{AAAAAAAA-0000-0000-0000-000000000003}'

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

function Get-FirstBytes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($PSVersionTable.PSVersion.Major -ge 6) {
        return @(Get-Content -LiteralPath $Path -AsByteStream -TotalCount 3)
    }
    return @(Get-Content -LiteralPath $Path -Encoding Byte -TotalCount 3)
}

function Get-SlnProject {
    # 按名称和类型查找 sln 中的 Project 条目；返回 @{ Path; Guid } 列表。
    param(
        [Parameter(Mandatory = $true)]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [string]$TypeGuid,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    foreach ($line in ($Content -split '\r?\n')) {
        if ($line -match '^Project\("(?<Type>\{[^}]+\})"\) = "(?<Name>[^"]+)", "(?<Path>[^"]+)", "(?<Guid>\{[^}]+\})"$' -and
            $Matches['Type'] -ieq $TypeGuid -and $Matches['Name'] -ceq $Name) {
            @{ Path = $Matches['Path']; Guid = $Matches['Guid'] }
        }
    }
}

function Get-SlnSectionLines {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [string]$Header
    )

    $inside = $false
    foreach ($line in ($Content -split '\r?\n')) {
        if ($line.Trim() -ceq $Header) {
            $inside = $true
            continue
        }
        if ($inside) {
            if ($line.Trim() -ceq 'EndGlobalSection') {
                break
            }
            $line.Trim()
        }
    }
}

function Get-SlnParent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [string]$Guid
    )

    foreach ($line in @(Get-SlnSectionLines $Content 'GlobalSection(NestedProjects) = preSolution')) {
        if ($line -match '^(?<Child>\{[^}]+\}) = (?<Parent>\{[^}]+\})$' -and $Matches['Child'] -ieq $Guid) {
            return $Matches['Parent']
        }
    }
    return $null
}

function Get-RepositorySnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $files = @(Get-ChildItem -LiteralPath (Join-Path $Root 'Plugins') -Recurse -Force | ForEach-Object { $_.FullName })
    $sln = Get-Content -Raw -LiteralPath (Join-Path $Root 'EngineAll.sln')
    return (($files -join "`n") + "`n----`n" + $sln)
}

function New-FakeRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $inventoryRoot = Join-Path $Root 'Plugins\GameCore\Inventory'
    $ghostCopyRoot = Join-Path $Root 'Plugins\Tools\Ghost\Ghost.All\binaries\Plugins'
    $buildScriptDirectory = Join-Path $Root 'BuildScript\leanclr'
    New-Item -ItemType Directory -Force -Path (Join-Path $inventoryRoot 'Inventory.All'), $ghostCopyRoot, $buildScriptDirectory | Out-Null

    Set-Content -LiteralPath (Join-Path $inventoryRoot 'Inventory.plugin') -Value '{}' -Encoding UTF8
    # 编译输出里的 .plugin 拷贝不算已有插件。
    Set-Content -LiteralPath (Join-Path $ghostCopyRoot 'GhostCopy.plugin') -Value '{}' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $buildScriptDirectory 'LeanClrPlugin.targets') -Value '<Project />' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $buildScriptDirectory 'LeanClrStub.targets') -Value '<Project />' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $inventoryRoot 'Inventory.All\Inventory.All.csproj') -Value '<Project Sdk="Microsoft.NET.Sdk" />' -Encoding UTF8

    $solutionPath = Join-Path $Root 'EngineAll.sln'
    $solutionContent = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("$folderType") = "Bricks", "Bricks", "{AAAAAAAA-0000-0000-0000-000000000001}"
EndProject
Project("$folderType") = "Plugins", "Plugins", "$decoyPluginsFolderGuid"
EndProject
Project("$folderType") = "Plugins", "Plugins", "$pluginsFolderGuid"
EndProject
Project("$folderType") = "GameCore", "GameCore", "$gameCoreFolderGuid"
EndProject
Project("$folderType") = "Phantom", "Phantom", "{AAAAAAAA-0000-0000-0000-000000000006}"
EndProject
Project("$folderType") = "Inventory", "Inventory", "{AAAAAAAA-0000-0000-0000-000000000004}"
	ProjectSection(SolutionItems) = preProject
		Plugins\GameCore\Inventory\Inventory.plugin = Plugins\GameCore\Inventory\Inventory.plugin
	EndProjectSection
EndProject
Project("$csharpType") = "Inventory.All", "Plugins\GameCore\Inventory\Inventory.All\Inventory.All.csproj", "{AAAAAAAA-0000-0000-0000-000000000005}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Debug|x64 = Debug|x64
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{AAAAAAAA-0000-0000-0000-000000000005}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{AAAAAAAA-0000-0000-0000-000000000005}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{AAAAAAAA-0000-0000-0000-000000000005}.Debug|x64.ActiveCfg = Debug|Any CPU
		{AAAAAAAA-0000-0000-0000-000000000005}.Debug|x64.Build.0 = Debug|Any CPU
		{AAAAAAAA-0000-0000-0000-000000000005}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{AAAAAAAA-0000-0000-0000-000000000005}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		$decoyPluginsFolderGuid = {AAAAAAAA-0000-0000-0000-000000000001}
		$gameCoreFolderGuid = $pluginsFolderGuid
		{AAAAAAAA-0000-0000-0000-000000000004} = $gameCoreFolderGuid
		{AAAAAAAA-0000-0000-0000-000000000006} = $gameCoreFolderGuid
		{AAAAAAAA-0000-0000-0000-000000000005} = {AAAAAAAA-0000-0000-0000-000000000004}
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {AAAAAAAA-0000-0000-0000-0000000000FF}
	EndGlobalSection
EndGlobal
"@
    Set-Content -LiteralPath $solutionPath -Value $solutionContent -Encoding UTF8
    return $solutionPath
}

function Assert-PluginRegistered {
    # 校验插件文件夹、shproj、各平台 csproj 在 sln 中的条目、嵌套、配置与共享导入；返回插件文件夹 GUID。
    param(
        [Parameter(Mandatory = $true)]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$RelativeRoot,

        [Parameter(Mandatory = $true)]
        [string]$ParentGuid,

        [Parameter(Mandatory = $true)]
        [string[]]$Hosts,

        [Parameter(Mandatory = $true)]
        [string]$SharedGuid,

        # 插件目录名（sln 中插件文件夹同名），缺省等于插件名。
        [string]$FolderName = ''
    )

    if (-not $FolderName) {
        $FolderName = $Name
    }
    $folders = @(Get-SlnProject $Content $folderType $FolderName)
    Assert-True ($folders.Count -eq 1) "Solution must contain exactly one '$FolderName' plugin folder (actual: $($folders.Count))."
    $folderGuid = $folders[0].Guid
    Assert-True ((Get-SlnParent $Content $folderGuid) -ieq $ParentGuid) "Plugin folder '$FolderName' must be nested under $ParentGuid."
    $manifestItem = "$RelativeRoot\$Name.plugin = $RelativeRoot\$Name.plugin"
    Assert-True (@(($Content -split '\r?\n') | Where-Object { $_.Trim() -ceq $manifestItem }).Count -eq 1) "Plugin folder '$Name' must list its .plugin as a solution item."

    $shared = @(Get-SlnProject $Content $sharedType $Name)
    Assert-True ($shared.Count -eq 1) "Solution must contain exactly one '$Name' shared project."
    Assert-True ($shared[0].Path -ceq "$RelativeRoot\$Name\$Name.shproj") "Shared project path is wrong: $($shared[0].Path)"
    Assert-True ($shared[0].Guid -ieq "{$SharedGuid}") 'Solution shared project GUID must match the shproj ProjectGuid.'
    Assert-True ($shared[0].Guid -ceq $shared[0].Guid.ToUpperInvariant()) 'Solution GUIDs must be upper case.'
    Assert-True ((Get-SlnParent $Content $shared[0].Guid) -ieq $folderGuid) "Shared project '$Name' must be nested under its plugin folder."

    $configLines = @(Get-SlnSectionLines $Content 'GlobalSection(ProjectConfigurationPlatforms) = postSolution')
    # dotnet sln add（LeanClr 桩工程）会向 sln 补充 x86/x64 配置，按当前配置数校验。
    $solutionConfigCount = @(Get-SlnSectionLines $Content 'GlobalSection(SolutionConfigurationPlatforms) = preSolution' | Where-Object { $_ }).Count
    $sharedImports = @(Get-SlnSectionLines $Content 'GlobalSection(SharedMSBuildProjectFiles) = preSolution')
    $projitems = "$RelativeRoot\$Name\$Name.projitems"
    Assert-True (@($sharedImports | Where-Object { $_ -ceq "$projitems*{$($SharedGuid.ToLowerInvariant())}*SharedItemsImports = 13" }).Count -eq 1) "Shared project '$Name' must be registered in SharedMSBuildProjectFiles."
    Assert-True (@($configLines | Where-Object { $_ -like "$($shared[0].Guid).*" }).Count -eq 0) 'Shared projects must not get configuration mappings.'

    foreach ($hostName in $Hosts) {
        $projectName = "$Name.$hostName"
        $projects = @(Get-SlnProject $Content $csharpType $projectName)
        Assert-True ($projects.Count -eq 1) "Solution must contain exactly one '$projectName' project."
        $guid = $projects[0].Guid
        Assert-True ($projects[0].Path -ceq "$RelativeRoot\$projectName\$projectName.csproj") "Project path is wrong: $($projects[0].Path)"
        Assert-True ((Get-SlnParent $Content $guid) -ieq $folderGuid) "Project '$projectName' must be nested under its plugin folder."
        $mapped = @($configLines | Where-Object { $_ -like "$guid.*" })
        Assert-True ($mapped.Count -eq 2 * $solutionConfigCount) "Project '$projectName' must have ActiveCfg + Build.0 for each of $solutionConfigCount solution configurations (actual: $($mapped.Count))."
        Assert-True (@($mapped | Where-Object { $_ -ceq "$guid.Release|Any CPU.Build.0 = Release|Any CPU" }).Count -eq 1) 'Release solution configuration must map to Release|Any CPU.'
        Assert-True (@($mapped | Where-Object { $_ -ceq "$guid.Debug|x64.ActiveCfg = Debug|Any CPU" }).Count -eq 1) 'Debug|x64 solution configuration must map to Debug|Any CPU.'
        Assert-True (@($sharedImports | Where-Object { $_ -ceq "$projitems*$($guid.ToLowerInvariant())*SharedItemsImports = 5" }).Count -eq 1) "Project '$projectName' must import its shared project in SharedMSBuildProjectFiles."
    }
    return $folderGuid
}

function Assert-PluginFiles {
    # 校验生成的文件；返回 shproj GUID。
    param(
        [Parameter(Mandatory = $true)]
        [string]$PluginRoot,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string[]]$Platforms,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Dependencies,

        [Parameter(Mandatory = $true)]
        [bool]$LoadOnInit
    )

    $manifest = Get-Content -Raw -LiteralPath (Join-Path $PluginRoot "$Name.plugin") | ConvertFrom-Json
    Assert-True ($manifest.Enable -eq $true) "$Name.plugin must be enabled."
    Assert-True ($manifest.LoadOnInit -eq $LoadOnInit) "$Name.plugin LoadOnInit must be $LoadOnInit."
    Assert-True ((@($manifest.Platforms) -join ',') -ceq ($Platforms -join ',')) "$Name.plugin Platforms must be '$($Platforms -join ',')' (actual: '$(@($manifest.Platforms) -join ',')')."
    Assert-True ((@($manifest.Dependencies) -join ',') -ceq ($Dependencies -join ',')) "$Name.plugin Dependencies must be '$($Dependencies -join ',')'."

    $sharedDirectory = Join-Path $PluginRoot $Name
    [xml]$shproj = Get-Content -Raw -LiteralPath (Join-Path $sharedDirectory "$Name.shproj")
    [xml]$projitems = Get-Content -Raw -LiteralPath (Join-Path $sharedDirectory "$Name.projitems")
    $sharedGuid = [string]$shproj.Project.PropertyGroup[0].ProjectGuid
    Assert-True ($sharedGuid -match '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$') "shproj ProjectGuid must be a lower case GUID: $sharedGuid"
    Assert-True (([string]$projitems.Project.PropertyGroup[0].SharedGUID) -ceq $sharedGuid) 'projitems SharedGUID must equal shproj ProjectGuid.'
    $projitemsText = Get-Content -Raw -LiteralPath (Join-Path $sharedDirectory "$Name.projitems")
    Assert-True ($projitemsText -like '*<Compile Include="$(MSBuildThisFileDirectory)Plugin.cs" />*') 'projitems must compile Plugin.cs.'
    Assert-True ($projitemsText -like "*<Import_RootNamespace>EngineNS.Plugins.$Name</Import_RootNamespace>*") 'projitems must set the plugin root namespace.'

    $pluginCode = Get-Content -Raw -LiteralPath (Join-Path $sharedDirectory 'Plugin.cs')
    Assert-True ($pluginCode -like "*namespace EngineNS.Plugins.$Name*") 'Plugin.cs must use the plugin namespace.'
    Assert-True ($pluginCode -like '*: EngineNS.Bricks.AssemblyLoader.IPlugin*') 'Plugin.cs must implement AssemblyLoader.IPlugin.'
    Assert-True ($pluginCode -like '*`[EngineNS.Bricks.AssemblyLoader.TtPlugin`]*') 'Plugin.cs must mark the loader with TtPlugin.'
    Assert-True ($pluginCode -like "*get => `"$Name`"*") 'Plugin.cs AssemblyDesc must return the plugin name.'

    $hostNames = @()
    foreach ($platform in $Platforms) {
        switch ($platform) {
            'PLTF_ALL' { $hostNames += 'All' }
            'PLTF_Windows' { $hostNames += 'Window' }
            'PLTF_Android' { $hostNames += 'Android' }
        }
    }
    foreach ($hostName in $hostNames) {
        $projectPath = Join-Path $PluginRoot "$Name.$hostName\$Name.$hostName.csproj"
        Assert-True (Test-Path -LiteralPath $projectPath -PathType Leaf) "Project was not generated: $projectPath"
        $text = Get-Content -Raw -LiteralPath $projectPath
        [xml]$xml = $text
        Assert-True ($null -ne $xml.Project) "$Name.$hostName.csproj must be valid XML."
        Assert-True ($text -like '*<AllowUnsafeBlocks>true</AllowUnsafeBlocks>*') "$Name.$hostName.csproj must allow unsafe blocks."
        Assert-True ($text -like '*<TargetFramework>net8.0</TargetFramework>*') "$Name.$hostName.csproj must target net8.0."
        Assert-True ($text -like "*<Import Project=`"..\$Name\$Name.projitems`" Label=`"Shared`" />*") "$Name.$hostName.csproj must import the shared project."
        Assert-True ($text -like "*Exclude=`"`$(TtTitanRoot)binaries\Plugins\$Name\*.dll*") "$Name.$hostName.csproj must not reference its own output."
        if ($hostName -eq 'Android') {
            Assert-True ($text -like '*binaries\android\net8.0\Engine.Android.dll*') 'Android project must reference the isolated Android engine output.'
            Assert-True ($text -like '*binaries\Plugins\*\*.Android.dll*') 'Android project must reference other Android plugins.'
            Assert-True ($text -like '*<DefineConstants>$(DefineConstants);PAndroid</DefineConstants>*') 'Android project must define PAndroid.'
            Assert-True ($text -notlike '*Engine.Window.dll</HintPath>*') 'Android project must not reference Engine.Window.dll.'
        }
        elseif ($hostName -eq 'All') {
            # All 宿主通过 TitanManagedProject.props 的平台映射引用引擎工程，不写死单平台 dll。
            Assert-True ($text -like '*<ProjectReference Include="$(TitanEngineProject)" />*') 'All project must reference the engine through $(TitanEngineProject).'
            Assert-True ($text -like '*Import Project=*TitanManagedProject.props*') 'All project must import TitanManagedProject.props.'
            Assert-True ($text -like '*binaries\Plugins\*\*.All.dll*') 'All project must reference other All plugins.'
        }
        else {
            Assert-True ($text -like '*binaries\net8.0\Engine.Window.dll*') "$hostName project must reference Engine.Window.dll."
            Assert-True ($text -like '*binaries\Plugins\*\*.Window.dll*') "$hostName project must reference Window plugins."
        }
    }
    return $sharedGuid
}

function Assert-Rejected {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [hashtable]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedMessage,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $before = Get-RepositorySnapshot $Root
    $message = ''
    try {
        & $scriptPath @Arguments | Out-Null
    }
    catch {
        $message = $_.Exception.Message
    }
    Assert-True ($message -match $ExpectedMessage) "$Description must be rejected with '$ExpectedMessage' (actual: '$message')."
    Assert-True ((Get-RepositorySnapshot $Root) -ceq $before) "$Description must not change the repository."
}

try {
    Assert-True (Test-Path -LiteralPath $scriptPath -PathType Leaf) "Titan plugin scaffold script was not found: $scriptPath"

    $root = Join-Path $tempRoot 'repo'
    $solutionPath = New-FakeRepository $root
    New-Item -ItemType Directory -Force -Path (Join-Path $root 'Plugins\Game\Occupied') | Out-Null

    # 错误输入：校验全部发生在写盘前，失败时不留下任何改动。
    Assert-Rejected $root @{ Name = 'Inventory'; SolutionPath = $solutionPath } 'already exists' 'Duplicate plugin name'
    Assert-Rejected $root @{ Name = 'inventory'; Category = 'Game'; SolutionPath = $solutionPath } 'already exists' 'Case-insensitive duplicate plugin name'
    Assert-Rejected $root @{ Name = 'Occupied'; Category = 'Game'; SolutionPath = $solutionPath } 'already exists' 'Existing target directory'
    Assert-Rejected $root @{ Name = 'Foo.Bar'; SolutionPath = $solutionPath } 'Name' 'Dotted plugin name'
    Assert-Rejected $root @{ Name = '1abc'; SolutionPath = $solutionPath } 'Name' 'Plugin name starting with a digit'
    Assert-Rejected $root @{ Name = 'Foo'; Category = 'Game/1bad'; SolutionPath = $solutionPath } 'Invalid Category' 'Nested category with an invalid segment'
    Assert-Rejected $root @{ Name = 'Foo'; Category = 'Game//Sub'; SolutionPath = $solutionPath } 'Invalid Category' 'Nested category with an empty segment'
    Assert-Rejected $root @{ Name = 'Foo'; Category = '..\Game'; SolutionPath = $solutionPath } 'Invalid Category' 'Category escaping the Plugins directory'
    Assert-Rejected $root @{ Name = 'Foo'; Folder = 'a.b'; SolutionPath = $solutionPath } 'Invalid Folder' 'Dotted folder name'
    Assert-Rejected $root @{ Name = 'Fresh'; Category = 'Game'; Folder = 'Occupied'; SolutionPath = $solutionPath } 'already exists' 'Custom folder pointing at an existing directory'
    Assert-Rejected $root @{ Name = 'Phantom'; Category = 'GameCore'; SolutionPath = $solutionPath } 'Solution folder .* already exists' 'Solution folder that already exists without a directory'
    Assert-Rejected $root @{ Name = 'Foo'; Platforms = @('All', 'Windows'); SolutionPath = $solutionPath } 'All' 'All combined with another platform'
    Assert-Rejected $root @{ Name = 'Foo'; Platforms = @('iOS'); SolutionPath = $solutionPath } 'iOS' 'Unsupported platform'
    Assert-Rejected $root @{ Name = 'Foo'; Dependencies = @('Missing'); SolutionPath = $solutionPath } 'Missing' 'Unknown dependency'
    Assert-Rejected $root @{ Name = 'Foo'; Dependencies = @('GhostCopy'); SolutionPath = $solutionPath } 'GhostCopy' 'Dependency that only exists as a build output copy'
    Assert-Rejected $root @{ Name = 'Foo'; Platforms = @('Android'); LeanClr = $true; SolutionPath = $solutionPath } 'LeanClr' 'LeanCLR without an All or Windows host'

    # 默认 All + 新建 Category：Game 文件夹必须挂在顶层 Plugins 下，而不是 Bricks 下的同名文件夹。
    & $scriptPath -Name Foo -Category Game -Dependencies Inventory -SolutionPath $solutionPath | Out-Null
    $fooRoot = Join-Path $root 'Plugins\Game\Foo'
    $fooShared = Assert-PluginFiles $fooRoot 'Foo' @('PLTF_ALL') @('Inventory') $true
    $content = Get-Content -Raw -LiteralPath $solutionPath
    $gameFolders = @(Get-SlnProject $content $folderType 'Game')
    Assert-True ($gameFolders.Count -eq 1) 'Solution must contain exactly one Game category folder.'
    Assert-True ((Get-SlnParent $content $gameFolders[0].Guid) -ieq $pluginsFolderGuid) 'New category folder must be nested under the top-level Plugins folder.'
    Assert-PluginRegistered $content 'Foo' 'Plugins\Game\Foo' $gameFolders[0].Guid @('All') $fooShared | Out-Null
    $bom = Get-FirstBytes $solutionPath
    Assert-True ($bom.Count -eq 3 -and $bom[0] -eq 0xEF -and $bom[1] -eq 0xBB -and $bom[2] -eq 0xBF) 'Solution must keep its UTF-8 BOM.'
    Assert-Rejected $root @{ Name = 'foo'; SolutionPath = $solutionPath } 'already exists' 'Name of a freshly created plugin'

    # 已有 Category 复用 + NoLoadOnInit + 空依赖。
    & $scriptPath -Name Baz -Category GameCore -NoLoadOnInit -SolutionPath $solutionPath | Out-Null
    $bazShared = Assert-PluginFiles (Join-Path $root 'Plugins\GameCore\Baz') 'Baz' @('PLTF_ALL') @() $false
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-True (@(Get-SlnProject $content $folderType 'GameCore').Count -eq 1) 'Existing category folder must be reused.'
    Assert-PluginRegistered $content 'Baz' 'Plugins\GameCore\Baz' $gameCoreFolderGuid @('All') $bazShared | Out-Null

    # 无 Category：插件文件夹直接挂在顶层 Plugins 下。
    & $scriptPath -Name Qux -SolutionPath $solutionPath | Out-Null
    $quxShared = Assert-PluginFiles (Join-Path $root 'Plugins\Qux') 'Qux' @('PLTF_ALL') @() $true
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-PluginRegistered $content 'Qux' 'Plugins\Qux' $pluginsFolderGuid @('All') $quxShared | Out-Null

    # 以 -File 启动（BrickManager 的调用方式）：逗号分隔的数组参数会作为单个字符串传入。
    $hostExe = 'powershell'
    if ($PSVersionTable.PSEdition -eq 'Core') {
        $hostExe = 'pwsh'
    }
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Name Bar -Category Game -Platforms 'Windows,Android' -Dependencies 'Inventory,Foo' -SolutionPath $solutionPath | Out-Null
    Assert-True ($LASTEXITCODE -eq 0) "Scaffold script started with -File must succeed (exit $LASTEXITCODE)."
    $barShared = Assert-PluginFiles (Join-Path $root 'Plugins\Game\Bar') 'Bar' @('PLTF_Windows', 'PLTF_Android') @('Inventory', 'Foo') $true
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $root 'Plugins\Game\Bar\Bar.All'))) 'Platform-specific plugin must not generate an All project.'
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-True (@(Get-SlnProject $content $folderType 'Game').Count -eq 1) 'Category created by an earlier run must be reused.'
    Assert-PluginRegistered $content 'Bar' 'Plugins\Game\Bar' $gameFolders[0].Guid @('Window', 'Android') $barShared | Out-Null

    # 多次插入后 SharedMSBuildProjectFiles 仍按 GUID 升序，且段只有一个。
    $sharedImports = @(Get-SlnSectionLines $content 'GlobalSection(SharedMSBuildProjectFiles) = preSolution')
    $importGuids = @($sharedImports | ForEach-Object { ($_ -split '\*')[1] })
    Assert-True ($importGuids.Count -eq 9) "SharedMSBuildProjectFiles must contain 9 lines (actual: $($importGuids.Count))."
    Assert-True (($importGuids -join ',') -ceq (@($importGuids | Sort-Object) -join ',')) 'SharedMSBuildProjectFiles lines must be sorted by GUID.'
    Assert-True (@(($content -split '\r?\n') | Where-Object { $_.Trim() -ceq 'GlobalSection(SharedMSBuildProjectFiles) = preSolution' }).Count -eq 1) 'Solution must contain exactly one SharedMSBuildProjectFiles section.'
    Assert-True (@(($content -split '\r?\n') | Where-Object { $_.Trim() -ceq 'GlobalSection(NestedProjects) = preSolution' }).Count -eq 1) 'Solution must contain exactly one NestedProjects section.'
    $lines = @($content -split '\r?\n')
    $extensibilityIndex = -1
    $sharedIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -ceq 'GlobalSection(ExtensibilityGlobals) = postSolution') { $extensibilityIndex = $i }
        if ($lines[$i].Trim() -ceq 'GlobalSection(SharedMSBuildProjectFiles) = preSolution') { $sharedIndex = $i }
    }
    Assert-True ($sharedIndex -gt $extensibilityIndex -and $extensibilityIndex -ge 0) 'New SharedMSBuildProjectFiles section must follow ExtensibilityGlobals like Visual Studio writes it.'

    # LeanClr：为 All 宿主补导入并生成桩工程。
    & $scriptPath -Name Lean -LeanClr -SolutionPath $solutionPath | Out-Null
    $leanRoot = Join-Path $root 'Plugins\Lean'
    Assert-True (Test-Path -LiteralPath (Join-Path $leanRoot 'Lean.LeanClrStub\Lean.LeanClrStub.csproj') -PathType Leaf) 'LeanClr switch must generate the stub project.'
    Assert-True ((Get-Content -Raw -LiteralPath (Join-Path $leanRoot 'Lean.All\Lean.All.csproj')) -like '*LeanClrPlugin.targets*') 'LeanClr switch must import LeanClrPlugin.targets into the host project.'
    $content = Get-Content -Raw -LiteralPath $solutionPath
    $leanFolder = @(Get-SlnProject $content $folderType 'Lean')
    $stub = @(Get-SlnProject $content $csharpType 'Lean.LeanClrStub')
    Assert-True ($stub.Count -eq 1) 'LeanClr stub must be registered in the solution once.'
    Assert-True ((Get-SlnParent $content $stub[0].Guid) -ieq $leanFolder[0].Guid) 'LeanClr stub must be nested under the plugin folder.'

    # 多级 Category（经 -File 传入带 '/' 的单个字符串）：已有层复用，缺失层逐级新建。
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Name Deep -Category 'GameCore/Tools/Editor' -SolutionPath $solutionPath | Out-Null
    Assert-True ($LASTEXITCODE -eq 0) "Nested category run must succeed (exit $LASTEXITCODE)."
    $deepShared = Assert-PluginFiles (Join-Path $root 'Plugins\GameCore\Tools\Editor\Deep') 'Deep' @('PLTF_ALL') @() $true
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-True (@(Get-SlnProject $content $folderType 'GameCore').Count -eq 1) 'Nested category must reuse the existing first level.'
    $toolsFolders = @(Get-SlnProject $content $folderType 'Tools')
    $editorFolders = @(Get-SlnProject $content $folderType 'Editor')
    Assert-True ($toolsFolders.Count -eq 1 -and $editorFolders.Count -eq 1) 'Missing nested category levels must be created once.'
    Assert-True ((Get-SlnParent $content $toolsFolders[0].Guid) -ieq $gameCoreFolderGuid) 'Second category level must be nested under the first.'
    Assert-True ((Get-SlnParent $content $editorFolders[0].Guid) -ieq $toolsFolders[0].Guid) 'Third category level must be nested under the second.'
    Assert-PluginRegistered $content 'Deep' 'Plugins\GameCore\Tools\Editor\Deep' $editorFolders[0].Guid @('All') $deepShared | Out-Null

    # 反斜杠写法复用上一步新建的中间层。
    & $scriptPath -Name Deep2 -Category 'GameCore\Tools' -SolutionPath $solutionPath | Out-Null
    $deep2Shared = Assert-PluginFiles (Join-Path $root 'Plugins\GameCore\Tools\Deep2') 'Deep2' @('PLTF_ALL') @() $true
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-True (@(Get-SlnProject $content $folderType 'Tools').Count -eq 1) 'Nested category level created by an earlier run must be reused.'
    Assert-PluginRegistered $content 'Deep2' 'Plugins\GameCore\Tools\Deep2' $toolsFolders[0].Guid @('All') $deep2Shared | Out-Null

    # 目录名与插件名不同（同 Plugins\GameServer\Gate\GateServer.plugin）：工程仍按插件名命名，sln 文件夹按目录名。
    & $scriptPath -Name GateServer -Category Game -Folder Gate -SolutionPath $solutionPath | Out-Null
    $gateRoot = Join-Path $root 'Plugins\Game\Gate'
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $root 'Plugins\Game\GateServer'))) 'Custom folder must replace the default plugin directory.'
    $gateShared = Assert-PluginFiles $gateRoot 'GateServer' @('PLTF_ALL') @() $true
    $content = Get-Content -Raw -LiteralPath $solutionPath
    Assert-True (@(Get-SlnProject $content $folderType 'GateServer').Count -eq 0) 'Custom folder must not create a solution folder named after the plugin.'
    Assert-PluginRegistered $content 'GateServer' 'Plugins\Game\Gate' $gameFolders[0].Guid @('All') $gateShared 'Gate' | Out-Null
    Assert-Rejected $root @{ Name = 'GateServer2'; Category = 'Game'; Folder = 'Gate'; SolutionPath = $solutionPath } 'already exists' 'Custom folder reused by a second plugin'

    Write-Host 'Titan plugin scaffold tests passed.'
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
