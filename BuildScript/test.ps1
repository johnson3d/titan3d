#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('L0', 'L1', 'L2', 'L3', 'All')]
    [string]$Suite = 'L1',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$Filter = '',

    [string]$ResultsDirectory = '',

    [ValidateRange(1, 1440)]
    [int]$TimeoutMinutes = 15,

    [switch]$List
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\common\common.ps1"

function Resolve-RepoPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $expanded = $Path -replace '/', '\'
    $isAbsolute = ($expanded.Length -ge 2 -and $expanded[1] -eq ':') -or
        ($expanded.Length -ge 2 -and $expanded[0] -eq '\' -and $expanded[1] -eq '\')
    if ($isAbsolute) {
        return $expanded
    }

    return Join-Path $repoRoot $expanded
}

function Get-SuiteProjects {
    param(
        [Parameter(Mandatory = $true)]
        $SuiteDefinition
    )

    if (-not ($SuiteDefinition.PSObject.Properties.Name -contains 'projects')) {
        throw "Suite '$($SuiteDefinition.name)' does not declare projects."
    }

    return @($SuiteDefinition.projects)
}

function Get-ReportPath {
    param(
        [Parameter(Mandatory = $true)]
        $SuiteDefinition,

        [Parameter(Mandatory = $true)]
        [string]$Project,

        [Parameter(Mandatory = $true)]
        [int]$ProjectCount
    )

    $reportName = [string]$SuiteDefinition.name
    if ($ProjectCount -gt 1) {
        $projectName = (Split-Path -Leaf $Project) -replace '\.[^.]+$', ''
        $reportName = "$reportName-$projectName"
    }

    $reportName = $reportName -replace '[^A-Za-z0-9._-]', '_'
    return Join-Path $resultsPath ($reportName + '.xml')
}

function Get-JUnitTestCount {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ReportPath
    )

    [xml]$report = Get-Content -Raw -LiteralPath $ReportPath
    if ($report.PSObject.Properties.Name -contains 'testsuite') {
        return [int]$report.testsuite.tests
    }

    $testCount = 0
    if ($report.PSObject.Properties.Name -contains 'testsuites') {
        foreach ($testSuite in @($report.testsuites.testsuite)) {
            $testCount += [int]$testSuite.tests
        }
    }
    return $testCount
}

function Invoke-TestProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [int]$ProcessTimeoutMinutes,

        [string]$WorkingDirectory = ''
    )

    if ([string]::IsNullOrWhiteSpace($WorkingDirectory)) {
        $WorkingDirectory = $repoRoot
    }

    $display = @($FilePath) + $Arguments
    Write-Host ''
    Write-Host ">> $($display -join ' ')"

    $job = Start-Job -ScriptBlock {
        param($CommandPath, $CommandArguments, $WorkingDirectory)

        Set-Location -LiteralPath $WorkingDirectory
        Write-Output "__TITAN_TEST_HOST_PID__=$PID"
        & $CommandPath @CommandArguments 2>&1
        Write-Output "__TITAN_TEST_EXIT_CODE__=$LASTEXITCODE"
    } -ArgumentList $FilePath, $Arguments, $WorkingDirectory

    $completedJob = Wait-Job -Job $job -Timeout ($ProcessTimeoutMinutes * 60)
    if ($null -eq $completedJob) {
        $hostProcessId = $null
        $output = @(Receive-Job -Job $job -Keep -ErrorAction Continue)
        foreach ($item in $output) {
            if ($item -is [string] -and $item -match '^__TITAN_TEST_HOST_PID__=(\d+)$') {
                $hostProcessId = [int]$matches[1]
            }
            else {
                Write-Host $item
            }
        }

        if ($null -ne $hostProcessId) {
            $taskKill = Join-Path $env:SystemRoot 'System32\taskkill.exe'
            & $taskKill /PID $hostProcessId /T /F | Out-Null
        }
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force
        Write-Host -ForegroundColor Red "Test process timed out after $ProcessTimeoutMinutes minute(s): $($display -join ' ')"
        return 124
    }

    $exitCode = $null
    $output = @(Receive-Job -Job $job -ErrorAction Continue)
    Remove-Job -Job $job -Force
    foreach ($item in $output) {
        if ($item -is [string] -and $item -match '^__TITAN_TEST_HOST_PID__=\d+$') {
            continue
        }
        if ($item -is [string] -and $item -match '^__TITAN_TEST_EXIT_CODE__=(-?\d+)$') {
            $exitCode = [int]$matches[1]
        }
        else {
            Write-Host $item
        }
    }

    if ($null -eq $exitCode) {
        Write-Host -ForegroundColor Red 'Test process ended without reporting an exit code.'
        return 125
    }

    return $exitCode
}

try {
    Assert-WindowsHost

    $repoRoot = Get-RepoRoot
    $manifestPath = Test-RepoFile 'content\config\quality-manifest.json'
    $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
    if (-not ($manifest.PSObject.Properties.Name -contains 'suites')) {
        throw "Quality manifest does not declare suites: $manifestPath"
    }

    $selectedSuites = @(
        if ($Suite -eq 'All') {
            $manifest.suites
        }
        else {
            $manifest.suites | Where-Object { $_.level -eq $Suite }
        }
    )

    if ($selectedSuites.Count -eq 0) {
        throw "No suites matched level '$Suite' in $manifestPath"
    }

    Write-Host 'Titan quality tests'
    Write-Host "  Manifest:      $manifestPath"
    Write-Host "  Suite:         $Suite"
    Write-Host "  Configuration: $Configuration"

    if ($List) {
        foreach ($suiteDefinition in $selectedSuites) {
            $capabilities = if (@($suiteDefinition.capabilities).Count -eq 0) { 'none' } else { @($suiteDefinition.capabilities) -join ', ' }
            Write-Host ''
            Write-Host "[$($suiteDefinition.name)] level=$($suiteDefinition.level) capabilities=$capabilities"
            foreach ($project in (Get-SuiteProjects $suiteDefinition)) {
                Write-Host "  $project"
            }
        }
        exit 0
    }

    $dotnet = Find-DotNet
    $resultsPath = if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
        Join-Path $repoRoot 'TestResults'
    }
    else {
        Resolve-RepoPath $ResultsDirectory
    }
    New-Item -ItemType Directory -Force -Path $resultsPath | Out-Null
    Write-Host "  Results:       $resultsPath"
    Write-Host "  Timeout:       $TimeoutMinutes minute(s) per project"

    $failed = @()
    foreach ($suiteDefinition in $selectedSuites) {
        $projects = @(Get-SuiteProjects $suiteDefinition)
        if ($projects.Count -eq 0) {
            throw "Suite '$($suiteDefinition.name)' does not contain any projects."
        }

        if ($suiteDefinition.level -eq 'L1') {
            foreach ($project in $projects) {
                $projectPath = Test-RepoFile (($project -replace '/', '\'))
                $reportPath = Get-ReportPath $suiteDefinition $project $projects.Count
                if (Test-Path -LiteralPath $reportPath) {
                    Remove-Item -LiteralPath $reportPath -Force
                }

                $arguments = @(
                    'test',
                    $projectPath,
                    '--configuration', $Configuration,
                    '--logger', ("junit;LogFilePath=$reportPath"),
                    '--results-directory', $resultsPath,
                    ('/p:SolutionDir=' + $repoRoot + '\'),
                    ('/p:TitanRoot=' + $repoRoot + '\')
                )
                if (-not [string]::IsNullOrWhiteSpace($Filter)) {
                    $arguments += @('--filter', $Filter)
                }

                $exitCode = Invoke-TestProcess $dotnet $arguments $TimeoutMinutes
                if ($exitCode -ne 0) {
                    $failed += "$($suiteDefinition.name):$project (exit $exitCode)"
                    continue
                }

                if (-not (Test-Path -LiteralPath $reportPath)) {
                    $failed += "$($suiteDefinition.name):$project (JUnit report missing)"
                    continue
                }
                if ((Get-JUnitTestCount $reportPath) -eq 0) {
                    $failed += "$($suiteDefinition.name):$project (no tests matched)"
                }
            }
            continue
        }

        if ($suiteDefinition.level -eq 'L2' -or $suiteDefinition.level -eq 'L3') {
            $buildFailed = $false
            foreach ($project in $projects) {
                $projectPath = Test-RepoFile (($project -replace '/', '\'))
                $buildArguments = @(
                    'build',
                    $projectPath,
                    '--configuration', $Configuration,
                    ('/p:SolutionDir=' + $repoRoot + '\'),
                    ('/p:TitanRoot=' + $repoRoot + '\')
                )
                $buildExitCode = Invoke-TestProcess $dotnet $buildArguments $TimeoutMinutes
                if ($buildExitCode -ne 0) {
                    $failed += "$($suiteDefinition.name):$project (build exit $buildExitCode)"
                    $buildFailed = $true
                }
            }
            if ($buildFailed) {
                continue
            }

            $suiteName = ([string]$suiteDefinition.name) -replace '[^A-Za-z0-9._-]', '_'
            $suiteResultsPath = Join-Path $resultsPath $suiteName
            New-Item -ItemType Directory -Force -Path $suiteResultsPath | Out-Null
            $reportPath = Join-Path $suiteResultsPath 'engine-tests.xml'
            if (Test-Path -LiteralPath $reportPath) {
                Remove-Item -LiteralPath $reportPath -Force
            }

            $titanCmd = Test-RepoFile 'binaries\net8.0\TitanCMD.exe'
            $engineConfig = Test-RepoFile 'content\config\engineconfig_windows_test.jscfg'
            $nativeConfiguration = if ($Configuration -eq 'Debug') { 'debug' } else { 'release' }
            $arguments = @(
                'ExeCmd=RunTests',
                ("CookCfg=$engineConfig"),
                ("ResultsDir=$suiteResultsPath"),
                ("NativeDLL=$nativeConfiguration")
            )
            if (-not [string]::IsNullOrWhiteSpace($Filter)) {
                $arguments += "Filter=$Filter"
            }

            $titanWorkingDirectory = Test-RepoFile 'binaries'
            $exitCode = Invoke-TestProcess $titanCmd $arguments $TimeoutMinutes $titanWorkingDirectory
            if ($exitCode -ne 0) {
                $failed += "$($suiteDefinition.name) (exit $exitCode)"
                continue
            }
            if (-not (Test-Path -LiteralPath $reportPath)) {
                $failed += "$($suiteDefinition.name) (JUnit report missing)"
                continue
            }
            if ((Get-JUnitTestCount $reportPath) -eq 0) {
                $failed += "$($suiteDefinition.name) (no tests matched)"
            }
            continue
        }

        $failed += "$($suiteDefinition.name) (unsupported level $($suiteDefinition.level))"
    }

    if ($failed.Count -gt 0) {
        Write-Host ''
        Write-Host -ForegroundColor Red "Quality tests failed: $($failed -join '; ')"
        exit 1
    }

    Write-Host ''
    Write-Host 'Quality tests completed successfully.'
    exit 0
}
catch {
    Write-Host -ForegroundColor Red $_.Exception.Message
    exit 2
}
