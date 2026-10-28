# EngineAll.sln 文本操作的公共函数，由 New-LeanClrStub.ps1 / New-TitanPlugin.ps1 dot-source 使用。
# 只用 PowerShell 原生命令与字符串方法，保证在 ConstrainedLanguage 下可用。

$script:SolutionFolderTypeGuid = '{2150E333-8FDC-42A3-9474-1A3956D46DE8}'
$script:SharedProjectTypeGuid = '{D954291E-2A0B-460D-934E-DC6B0785DB48}'
$script:CSharpProjectTypeGuid = '{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}'

function Get-NormalizedFullPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return ((Resolve-Path -LiteralPath $Path).ProviderPath -replace '[\\/]+$', '')
}

function Get-RelativeFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FromDirectory,

        [Parameter(Mandatory = $true)]
        [string]$ToPath
    )

    Push-Location -LiteralPath $FromDirectory
    try {
        $relativePath = Resolve-Path -LiteralPath $ToPath -Relative
    }
    finally {
        Pop-Location
    }

    return (($relativePath -replace '/', '\') -replace '^\.\\', '')
}

function Read-SolutionLines {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SolutionPath
    )

    return @(Get-Content -LiteralPath $SolutionPath -Encoding UTF8)
}

function Write-SolutionLines {
    # 内容未变时不写盘；写盘时保留 UTF-8 BOM（VS 生成的 sln 带 BOM，pwsh 的 UTF8 默认不带）。
    param(
        [Parameter(Mandatory = $true)]
        [string]$SolutionPath,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    $current = Read-SolutionLines $SolutionPath
    if (($current -join "`n") -ceq ($Lines -join "`n")) {
        return $false
    }
    $encoding = 'UTF8'
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $encoding = 'utf8BOM'
    }
    Set-Content -LiteralPath $SolutionPath -Value $Lines -Encoding $encoding
    return $true
}

function Get-SolutionProjects {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    $projectPattern = '^Project\("(?<Type>\{[^}]+\})"\) = "(?<Name>[^"]+)", "(?<Path>[^"]+)", "(?<Guid>\{[^}]+\})"$'
    foreach ($line in $Lines) {
        if ($line -match $projectPattern) {
            @{
                TypeGuid = $Matches['Type']
                Name = $Matches['Name']
                Path = $Matches['Path']
                Guid = $Matches['Guid']
            }
        }
    }
}

function Get-SolutionProjectGuid {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SolutionContent,

        [Parameter(Mandatory = $true)]
        [string]$SolutionDirectory,

        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    $expectedPath = $ProjectPath -replace '/', '\'
    if (Test-Path -LiteralPath $expectedPath) {
        $expectedPath = Get-NormalizedFullPath $expectedPath
    }
    foreach ($project in @(Get-SolutionProjects ($SolutionContent -split '\r?\n'))) {
        if ($project.TypeGuid -ieq $script:SolutionFolderTypeGuid) {
            continue
        }
        $candidatePath = Join-Path $SolutionDirectory ($project.Path -replace '/', '\')
        if (Test-Path -LiteralPath $candidatePath) {
            $candidatePath = Get-NormalizedFullPath $candidatePath
        }
        if ($candidatePath -ieq $expectedPath) {
            return $project.Guid
        }
    }

    return $null
}

function Get-ProjectSolutionFolderGuid {
    # 只在 NestedProjects 段内查找：同一 GUID 也会出现在 ProjectDependencies 里。
    param(
        [Parameter(Mandatory = $true)]
        [string]$SolutionContent,

        [Parameter(Mandatory = $true)]
        [string]$ProjectGuid
    )

    $insideNestedProjects = $false
    foreach ($line in ($SolutionContent -split '\r?\n')) {
        if ($line -match '^\s*GlobalSection\(NestedProjects\) = preSolution\s*$') {
            $insideNestedProjects = $true
            continue
        }
        if ($insideNestedProjects -and $line -match '^\s*EndGlobalSection\s*$') {
            break
        }
        if ($insideNestedProjects -and
            $line -match '^\s*(?<Project>\{[^}]+\})\s*=\s*(?<Parent>\{[^}]+\})\s*$' -and
            $Matches['Project'] -ieq $ProjectGuid) {
            return $Matches['Parent']
        }
    }
    return $null
}

function Get-SolutionFolderGuid {
    # 按名称和父文件夹查找解决方案文件夹；ParentGuid 为空表示查找顶层文件夹。
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [string]$ParentGuid = ''
    )

    $content = $Lines -join "`n"
    foreach ($project in @(Get-SolutionProjects $Lines)) {
        if ($project.TypeGuid -ine $script:SolutionFolderTypeGuid -or $project.Name -ine $Name) {
            continue
        }
        $parent = Get-ProjectSolutionFolderGuid $content $project.Guid
        if ((-not $ParentGuid -and -not $parent) -or ($ParentGuid -and $parent -ieq $ParentGuid)) {
            return $project.Guid
        }
    }
    return $null
}

function Add-SolutionProjectEntry {
    # 在 Global 之前插入一个 Project 条目，可带 SolutionItems。
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$TypeGuid,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Guid,

        [string[]]$SolutionItems = @()
    )

    $entry = @("Project(`"$TypeGuid`") = `"$Name`", `"$Path`", `"$Guid`"")
    if ($SolutionItems.Count -gt 0) {
        $entry += "`tProjectSection(SolutionItems) = preProject"
        foreach ($item in $SolutionItems) {
            $entry += "`t`t$item = $item"
        }
        $entry += "`tEndProjectSection"
    }
    $entry += 'EndProject'

    $inserted = $false
    $output = foreach ($line in $Lines) {
        if (-not $inserted -and $line -match '^Global\s*$') {
            $entry
            $inserted = $true
        }
        $line
    }
    if (-not $inserted) {
        throw 'Solution does not contain a Global section.'
    }
    return , @($output)
}

function Set-SolutionSectionLines {
    # 在指定 GlobalSection 末尾追加行；段不存在时在 EndGlobal 之前新建。
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$SectionHeader,

        [Parameter(Mandatory = $true)]
        [string[]]$NewLines,

        [scriptblock]$InsertBefore = $null
    )

    $hasSection = @($Lines | Where-Object { $_.Trim() -ceq $SectionHeader }).Count -gt 0
    if (-not $hasSection) {
        $output = foreach ($line in $Lines) {
            if ($line -match '^EndGlobal\s*$') {
                "`t$SectionHeader"
                foreach ($newLine in $NewLines) { "`t`t$newLine" }
                "`tEndGlobalSection"
            }
            $line
        }
        return , @($output)
    }

    $pending = @($NewLines)
    $inside = $false
    $output = foreach ($line in $Lines) {
        if ($line.Trim() -ceq $SectionHeader) {
            $inside = $true
            $line
            continue
        }
        if ($inside) {
            if ($line -match '^\s*EndGlobalSection\s*$') {
                foreach ($newLine in $pending) { "`t`t$newLine" }
                $pending = @()
                $inside = $false
            }
            elseif ($InsertBefore) {
                $remaining = @()
                foreach ($newLine in $pending) {
                    if (& $InsertBefore $newLine $line.Trim()) {
                        "`t`t$newLine"
                    }
                    else {
                        $remaining += $newLine
                    }
                }
                $pending = $remaining
            }
        }
        $line
    }
    return , @($output)
}

function Set-SolutionNestedProjects {
    # 批量设置 NestedProjects 映射：已存在的就地改父级，新的追加在段尾。
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [hashtable]$Mappings,

        [Parameter(Mandatory = $true)]
        [string[]]$Order
    )

    $remaining = @($Order)
    $inside = $false
    $hasSection = $false
    $output = foreach ($line in $Lines) {
        if ($line -match '^\s*GlobalSection\(NestedProjects\) = preSolution\s*$') {
            $inside = $true
            $hasSection = $true
            $line
            continue
        }
        if ($inside) {
            if ($line -match '^\s*EndGlobalSection\s*$') {
                foreach ($child in $remaining) { "`t`t$child = $($Mappings[$child])" }
                $remaining = @()
                $inside = $false
            }
            elseif ($line -match '^\s*(?<Project>\{[^}]+\})\s*=\s*(?<Parent>\{[^}]+\})\s*$') {
                $child = $Matches['Project']
                $key = @($Mappings.Keys | Where-Object { $_ -ieq $child })
                if ($key.Count -gt 0) {
                    "`t`t$child = $($Mappings[$key[0]])"
                    $remaining = @($remaining | Where-Object { $_ -ine $child })
                    continue
                }
            }
        }
        $line
    }

    if (-not $hasSection) {
        $section = @("`tGlobalSection(NestedProjects) = preSolution")
        foreach ($child in $Order) { $section += "`t`t$child = $($Mappings[$child])" }
        $section += "`tEndGlobalSection"
        $inserted = $false
        $output = foreach ($line in $output) {
            if (-not $inserted -and ($line -match '^\s*GlobalSection\(ExtensibilityGlobals\) = postSolution\s*$' -or $line -match '^EndGlobal\s*$')) {
                $section
                $inserted = $true
            }
            $line
        }
    }
    return , @($output)
}

function Set-ProjectSolutionFolder {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SolutionPath,

        [Parameter(Mandatory = $true)]
        [string]$ProjectGuid,

        [Parameter(Mandatory = $true)]
        [string]$ParentGuid
    )

    $lines = Read-SolutionLines $SolutionPath
    $lines = Set-SolutionNestedProjects $lines @{ $ProjectGuid = $ParentGuid } @($ProjectGuid)
    Write-SolutionLines $SolutionPath $lines | Out-Null
}

function Get-SolutionProjectConfigurationLines {
    # 按 SolutionConfigurationPlatforms 生成 SDK 工程的配置映射：Release* -> Release|Any CPU，其余 -> Debug|Any CPU。
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$ProjectGuid
    )

    $inside = $false
    foreach ($line in $Lines) {
        if ($line -match '^\s*GlobalSection\(SolutionConfigurationPlatforms\) = preSolution\s*$') {
            $inside = $true
            continue
        }
        if (-not $inside) {
            continue
        }
        if ($line -match '^\s*EndGlobalSection\s*$') {
            break
        }
        if ($line -match '^\s*(?<Config>[^|=]+)\|(?<Platform>[^=]+?)\s*=') {
            $solutionConfig = "$($Matches['Config'])|$($Matches['Platform'])"
            $projectConfig = 'Debug|Any CPU'
            if ($Matches['Config'] -like 'Release*') {
                $projectConfig = 'Release|Any CPU'
            }
            "$ProjectGuid.$solutionConfig.ActiveCfg = $projectConfig"
            "$ProjectGuid.$solutionConfig.Build.0 = $projectConfig"
        }
    }
}
