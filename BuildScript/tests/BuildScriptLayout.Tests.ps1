#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# BuildScript layout contract: files are grouped by function into subdirectories;
# only the two entry scripts (build.ps1 / test.ps1) may sit in the BuildScript root.
# Any project file, script or doc that still points at the old flat path fails here.

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$buildScriptRoot = Join-Path $repoRoot 'BuildScript'

$rootScriptAllowList = @('build.ps1', 'test.ps1')
$rootDirAllowList = @('android', 'check', 'cmake', 'common', 'EngineConfig', 'leanclr', 'msbuild', 'scaffold', 'tests')

# file name -> owning subdirectory (relative to BuildScript)
$grouped = [ordered]@{
    'common.ps1'                 = 'common'
    'SolutionTools.ps1'          = 'common'
    'dependencies.json'          = 'common'
    'deps.ps1'                   = 'check'
    'Check-NativeLongTypes.ps1'  = 'check'
    'check-android-toolchain.ps1' = 'check'
    'AndroidTools.ps1'           = 'android'
    'AndroidToolchain.example.json' = 'android'
    'build-android.ps1'          = 'android'
    'Stage-AndroidContent.ps1'   = 'android'
    'build-leanclr.ps1'          = 'leanclr'
    'LeanClrDeploy.targets'      = 'leanclr'
    'LeanClrPlugin.targets'      = 'leanclr'
    'LeanClrStub.targets'        = 'leanclr'
    'New-LeanClrStub.ps1'        = 'scaffold'
    'New-TitanPlugin.ps1'        = 'scaffold'
    'TitanPlatform.props'        = 'msbuild'
    'TitanManagedEngine.props'   = 'msbuild'
    'TitanManagedProject.props'  = 'msbuild'
    'TitanCodeGen.targets'       = 'msbuild'
    'TitanDataCopyer.targets'    = 'msbuild'
    'TitanSourceInputs.targets'  = 'msbuild'
}

$problems = @()

# 1. root must stay clean
$rootFiles = @(Get-ChildItem -LiteralPath $buildScriptRoot -File)
foreach ($file in $rootFiles) {
    if ($rootScriptAllowList -notcontains $file.Name) {
        $owner = $grouped[$file.Name]
        $hint = if ($owner) { "move it to BuildScript\$owner\" } else { 'group it into a functional subdirectory' }
        $problems += "unexpected file in BuildScript root: $($file.Name) ($hint)"
    }
}
foreach ($dir in @(Get-ChildItem -LiteralPath $buildScriptRoot -Directory)) {
    if ($rootDirAllowList -notcontains $dir.Name) {
        $problems += "unexpected directory in BuildScript root: $($dir.Name)"
    }
}

# 2. every grouped file lives at its new home and no longer at the old one
foreach ($name in $grouped.Keys) {
    $subdir = $grouped[$name]
    $newPath = Join-Path (Join-Path $buildScriptRoot $subdir) $name
    $oldPath = Join-Path $buildScriptRoot $name
    if (-not (Test-Path -LiteralPath $newPath -PathType Leaf)) {
        $problems += "missing relocated file: BuildScript\$subdir\$name"
    }
    if (Test-Path -LiteralPath $oldPath -PathType Leaf) {
        $problems += "stale file still in BuildScript root: $name"
    }
}

# 3. no reference may still use the flat location
$scannedExtensions = @('.ps1', '.targets', '.props', '.csproj', '.vcxproj', '.sln', '.bat', '.cmd', '.yml', '.json', '.cmake', '.md', '.cs')
$skippedDirs = @('3rd', 'cache', 'codegen', 'binaries', 'intermediate', 'packages', 'TestResults', '.git', '.vs', '.superpowers', '.qoder', 'docs', 'Documents', 'content', 'enginecontent', 'editorcontent', 'plugincontent', 'obj')
$scanFiles = @()
foreach ($entry in @(Get-ChildItem -LiteralPath $repoRoot -Force)) {
    if ($entry.PSIsContainer) {
        if ($skippedDirs -contains $entry.Name) { continue }
        foreach ($file in @(Get-ChildItem -LiteralPath $entry.FullName -Recurse -File -ErrorAction SilentlyContinue)) {
            if ($scannedExtensions -contains $file.Extension.ToLowerInvariant()) { $scanFiles += $file }
        }
    } elseif ($scannedExtensions -contains $entry.Extension.ToLowerInvariant()) {
        $scanFiles += $entry
    }
}

$selfPath = (Resolve-Path -LiteralPath $PSCommandPath).ProviderPath
foreach ($file in $scanFiles) {
    if ($file.FullName -eq $selfPath) { continue }
    $text = Get-Content -Raw -LiteralPath $file.FullName
    if ($null -eq $text) { continue }
    foreach ($name in $grouped.Keys) {
        # C# 用 Path.Combine(root, 'BuildScript', '<name>') 拼路径，不是字面量斜杠，所以单独识别
        $stale = $text.Contains("BuildScript\$name") -or $text.Contains("BuildScript/$name") -or $text.Contains('"BuildScript", "' + $name + '"')
        if ($stale) {
            $relative = $file.FullName.Substring($repoRoot.Length + 1)
            $problems += "stale reference to flat path '$name' in $relative (use BuildScript\$($grouped[$name])\$name)"
        }
    }
}

# 4. the relocated msbuild files must still resolve the repository root
#    (every '..' hop from BuildScript\msbuild now needs one more level)
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host '[BuildScriptLayout] SKIP msbuild root resolution (dotnet not found)'
}
else {
    $rootPath = $repoRoot + '\'
    $probes = @(
        @{ Project = 'Module\Engine.Window\Engine.Window.csproj'; Platform = ''
           Properties = @('TitanRepositoryRoot', 'TitanManagedRoot', 'TitanPlatform')
           Expected = @{ TitanRepositoryRoot = $rootPath; TitanManagedRoot = $rootPath } },
        @{ Project = 'Module\Engine.Android\Engine.Android.csproj'; Platform = 'Android'
           Properties = @('TitanRepositoryRoot', 'TitanPlatform')
           Expected = @{ TitanRepositoryRoot = $rootPath } },
        @{ Project = 'Plugins\Game\Survivor\Survivor.All\Survivor.All.csproj'; Platform = ''
           Properties = @('TtTitanRoot', '_TitanRoot', 'TitanEngineProject')
           Expected = @{ TtTitanRoot = $rootPath; _TitanRoot = $rootPath; TitanEngineProject = (Join-Path $repoRoot 'Module\Engine.Window\Engine.Window.csproj') } }
    )
    foreach ($probe in $probes) {
        $arguments = @('msbuild', (Join-Path $repoRoot $probe.Project), '-nologo')
        if ($probe.Platform) { $arguments += "-p:TitanPlatform=$($probe.Platform)" }
        foreach ($property in $probe.Properties) { $arguments += "-getProperty:$property" }
        # stdout only: msbuild writes its diagnostics to stderr and would corrupt the JSON
        $output = (& $dotnet.Source @arguments | Out-String)
        if ($LASTEXITCODE -ne 0) {
            $problems += "msbuild cannot evaluate $($probe.Project) with the relocated BuildScript\msbuild files"
            continue
        }
        $evaluated = ($output | ConvertFrom-Json).Properties
        foreach ($property in $probe.Expected.Keys) {
            $evaluatedProperty = $evaluated.PSObject.Properties[$property]
            $value = if ($evaluatedProperty) { [string]$evaluatedProperty.Value } else { '' }
            if (-not $value) {
                $problems += "$($probe.Project): $property was not set"
            }
            elseif ($value -ine $probe.Expected[$property]) {
                $problems += "$($probe.Project): $property resolved to '$value' instead of '$($probe.Expected[$property])'"
            }
        }
    }
}

# 5. relocated targets must still reach the repository-level siblings they reference
#    (BuildScript\leanclr sits one level deeper than the old flat location)
if ($dotnet) {
    $leanClrProbe = @{
        Project = 'Plugins\Game\Survivor\Survivor.LeanClrStub\Survivor.LeanClrStub.csproj'
        ExpectedContentDir = (Join-Path $repoRoot 'content\LeanCLR\Survivor') + '\'
    }
    $arguments = @('msbuild', (Join-Path $repoRoot $leanClrProbe.Project), '-nologo', '-getProperty:LeanClrContentDir', '-getItem:ProjectReference')
    $output = (& $dotnet.Source @arguments | Out-String)
    if ($LASTEXITCODE -ne 0) {
        $problems += "msbuild cannot evaluate $($leanClrProbe.Project) with the relocated BuildScript\leanclr targets"
    }
    else {
        $evaluated = ($output | ConvertFrom-Json)
        $contentDir = [string]$evaluated.Properties.LeanClrContentDir
        if ($contentDir -ine $leanClrProbe.ExpectedContentDir) {
            $problems += "BuildScript\leanclr\LeanClrDeploy.targets: LeanClrContentDir resolved to '$contentDir' instead of '$($leanClrProbe.ExpectedContentDir)'"
        }
        $references = @($evaluated.Items.ProjectReference | ForEach-Object { [string]$_.FullPath })
        foreach ($expected in @('Module\LeanClrEngineStub\LeanClrEngineStub.csproj', 'Plugins\Game\Survivor\Survivor.All\Survivor.All.csproj')) {
            $full = Join-Path $repoRoot $expected
            if ($references -notcontains $full) { $problems += "BuildScript\leanclr\LeanClrStub.targets: referenced project $expected was not resolved (stale '..' depth after relocation)" }
        }
    }
}

# 6. EngineAll.sln must mirror the BuildScript directory tree as nested solution folders,
#    otherwise Solution Explorer keeps showing every relocated file flat under "BuildScript".
$solutionPath = Join-Path $repoRoot 'EngineAll.sln'
$solutionLines = @(Get-Content -LiteralPath $solutionPath -Encoding UTF8)
$folderTypeGuid = '{2150E333-8FDC-42A3-9474-1A3956D46DE8}'
$solutionFolders = @()
$currentFolder = $null
foreach ($line in $solutionLines) {
    $projectType = if ($line -match '^Project\("(?<Type>\{[^}]+\})"\) = "(?<Name>[^"]+)", "[^"]*", "(?<Guid>\{[^}]+\})"$') { $Matches['Type'] } else { '' }
    if ($projectType -ieq $folderTypeGuid) {
        $currentFolder = @{ Name = $Matches['Name']; Guid = $Matches['Guid']; Items = @() }
        $solutionFolders += $currentFolder
        continue
    }
    if ($line -match '^Project\(') { $currentFolder = $null; continue }
    if ($null -eq $currentFolder) { continue }
    if ($line -match '^EndProject\s*$') { $currentFolder = $null; continue }
    if ($line -match '^\s+(?<Path>[^=\t]+?) = ') { $currentFolder.Items += $Matches['Path'].Trim() }
}
$folderParent = @{}
$inNestedSection = $false
foreach ($line in $solutionLines) {
    if ($line -match '^\s*GlobalSection\(NestedProjects\) = preSolution\s*$') { $inNestedSection = $true; continue }
    if ($inNestedSection -and $line -match '^\s*EndGlobalSection\s*$') { break }
    if ($inNestedSection -and $line -match '^\s*(?<Child>\{[^}]+\})\s*=\s*(?<Parent>\{[^}]+\})\s*$') { $folderParent[$Matches['Child']] = $Matches['Parent'] }
}
$buildScriptFolders = @($solutionFolders | Where-Object { $_.Name -ceq 'BuildScript' -and -not $folderParent.ContainsKey($_.Guid) })
if ($buildScriptFolders.Count -ne 1) {
    $problems += "EngineAll.sln: expected exactly one top-level 'BuildScript' solution folder, found $($buildScriptFolders.Count)"
}
else {
    $rootFolderGuid = $buildScriptFolders[0].Guid
    $itemsByGuid = @{}
    foreach ($folder in $solutionFolders) { $itemsByGuid[$folder.Guid] = @($folder.Items) }
    $buildScriptRoot = Join-Path $repoRoot 'BuildScript'
    foreach ($file in @(Get-ChildItem -LiteralPath $buildScriptRoot -Recurse -File)) {
        $relative = $file.FullName.Substring($repoRoot.Length + 1)
        $directoryParts = @($file.DirectoryName.Substring($buildScriptRoot.Length).Trim('\').Split('\')) | Where-Object { $_ }
        $ownerGuid = $rootFolderGuid
        $ownerName = 'BuildScript'
        foreach ($part in $directoryParts) {
            $sub = @($solutionFolders | Where-Object { $_.Name -ceq $part -and $folderParent.ContainsKey($_.Guid) -and $folderParent[$_.Guid] -ieq $ownerGuid })
            if ($sub.Count -ne 1) {
                $problems += "EngineAll.sln: missing solution folder $ownerName\$part for $relative"
                $ownerGuid = $null
                break
            }
            $ownerGuid = $sub[0].Guid
            $ownerName = "$ownerName\$part"
        }
        if (-not $ownerGuid) { continue }
        if ($itemsByGuid[$ownerGuid] -notcontains $relative) {
            $problems += "EngineAll.sln: $relative is not a solution item of folder $ownerName"
        }
    }
    foreach ($item in $itemsByGuid[$rootFolderGuid]) {
        if ($item.Substring('BuildScript\'.Length).Contains('\')) {
            $problems += "EngineAll.sln: $item must move into a nested BuildScript solution folder, not stay flat under BuildScript"
        }
    }
    foreach ($childFolder in @($solutionFolders | Where-Object { $folderParent.ContainsKey($_.Guid) -and $folderParent[$_.Guid] -ieq $rootFolderGuid })) {
        if (-not (Test-Path -LiteralPath (Join-Path $buildScriptRoot $childFolder.Name) -PathType Container)) {
            $problems += "EngineAll.sln: solution folder BuildScript\$($childFolder.Name) does not match any BuildScript subdirectory"
        }
    }
}

if ($problems.Count -gt 0) {
    throw ("BuildScript layout violations:`n  - " + ($problems -join "`n  - "))
}

Write-Host "[BuildScriptLayout] PASS: $($grouped.Count) files grouped into $((@($grouped.Values | Select-Object -Unique)).Count) directories, EngineAll.sln hierarchy mirrored, no stale references, msbuild root resolution intact"
