# Common functions for text manipulation of EngineAll.sln, dot-sourced by New-LeanClrStub.ps1 / New-TitanPlugin.ps1.
# Uses only native PowerShell commands and string methods, guaranteeing availability under ConstrainedLanguage.

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
    # Do not write when content is unchanged; when writing, keep the UTF-8 BOM (VS-generated sln files carry a BOM; pwsh's UTF8 default does not).
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
    # Search only within the NestedProjects section: the same GUID also appears in ProjectDependencies.
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
    # Find a solution folder by name and parent folder; an empty ParentGuid means a top-level folder.
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
    # Insert a Project entry before Global, optionally with SolutionItems.
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
    # Append lines at the end of the given GlobalSection; create it before EndGlobal if absent.
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
    # Bulk-set NestedProjects mappings: reparent existing ones in place, append new ones at the section tail.
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
    # Generate the SDK project's configuration mapping from SolutionConfigurationPlatforms: Release* -> Release|Any CPU, everything else -> Debug|Any CPU.
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
