#requires -Version 5.1
# Titan MCP setup: verifies that the committed .mcp.json can actually run on this machine.
#
# The project .mcp.json is machine independent on purpose:
#   * command       : a name resolved from PATH (python / py), never an absolute interpreter path
#   * script args   : workspace-relative, combined with "cwd": "." (absolute paths and
#                     ${workspaceFolder}-style placeholders do NOT work in Qoder MCP config)
#   * per-machine   : interpreter paths, virtualenvs and extra pip modules belong in the
#                     user-level ~/.qoder/mcp.json override, which -WriteUserOverride generates
#
# Qoder only reads the project .mcp.json at IDE start, so restart the IDE after changing it.

[CmdletBinding()]
param(
    # Only report findings, never touch the machine.
    [switch]$CheckOnly,

    # Install the python packages listed in Plugins/AIGC/MCPServer/Python/requirements.txt.
    [switch]$InstallPythonPackages,

    # Merge absolute-path overrides for the servers declared in .mcp.json into
    # ~/.qoder/mcp.json. Existing entries that this repo does not own are preserved.
    [switch]$WriteUserOverride,

    # Python version required by renderdoc_mcp (py launcher selector, e.g. 3.13).
    [string]$RenderdocPythonVersion = '3.13'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\..\common\common.ps1"

Assert-WindowsHost

$repoRoot = Get-RepoRoot
$mcpJsonPath = Join-Path $repoRoot '.mcp.json'
$pythonRoot = Join-Path $repoRoot 'Plugins\AIGC\MCPServer\Python'
$requirementsPath = Join-Path $pythonRoot 'requirements.txt'
$bridgeScript = Join-Path $pythonRoot 'titan_mcp_server.py'
$userMcpJsonPath = Join-Path $env:USERPROFILE '.qoder\mcp.json'

$findings = New-Object System.Collections.Generic.List[string]
$notes = New-Object System.Collections.Generic.List[string]
function Add-Finding {
    param([string]$Message)
    $findings.Add($Message) | Out-Null
    Write-Host "  [!] $Message" -ForegroundColor Yellow
}

# Notes are advisory: they do not make the setup unusable and do not change the exit code.
function Add-Note {
    param([string]$Message)
    $notes.Add($Message) | Out-Null
    Write-Host "  [i] $Message" -ForegroundColor DarkGray
}

# PowerShell 5.1 converts native stderr into a terminating error while $ErrorActionPreference is Stop,
# and it strips embedded double quotes when invoking native executables. Every probe therefore goes
# through this helper: quote free program text, merged streams, exit code only.
function Invoke-Probe {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Arguments
    )

    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $rawOutput = & $FilePath @Arguments 2>&1
        $code = $LASTEXITCODE
    }
    catch {
        $rawOutput = @()
        $code = 1
    }
    finally {
        $ErrorActionPreference = $previous
    }

    $lines = @()
    foreach ($line in @($rawOutput)) {
        $lines += [string]$line
    }

    return [pscustomobject]@{ ExitCode = $code; Output = $lines }
}

function Test-PythonModule {
    param(
        [Parameter(Mandatory = $true)][string]$PythonCommand,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$PythonArguments,
        [Parameter(Mandatory = $true)][string]$ModuleName
    )

    $probe = 'import sys,importlib.util as u; sys.exit(0 if u.find_spec(sys.argv[1]) else 1)'
    $result = Invoke-Probe -FilePath $PythonCommand -Arguments (@($PythonArguments) + @('-c', $probe, $ModuleName))
    return ($result.ExitCode -eq 0)
}

function Get-PythonVersion {
    param(
        [Parameter(Mandatory = $true)][string]$PythonCommand,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$PythonArguments
    )

    $result = Invoke-Probe -FilePath $PythonCommand -Arguments (@($PythonArguments) + @('-V'))
    if ($result.ExitCode -ne 0 -or $result.Output.Count -eq 0) {
        return ''
    }
    return $result.Output[-1].Trim() -replace '^Python\s+', ''
}

Write-Host ''
Write-Host '=== Titan MCP setup ===' -ForegroundColor Cyan
Write-Host "Repo root : $repoRoot"
Write-Host "Mode      : $(if ($CheckOnly) { 'check only' } else { 'verify (+ optional install)' })"
Write-Host ''

# ---------------------------------------------------------------- 1. project .mcp.json
Write-Host '[1/5] Project .mcp.json'
if (-not (Test-Path -LiteralPath $mcpJsonPath)) {
    throw "Project MCP config was not found: $mcpJsonPath"
}

$mcpConfig = Get-Content -LiteralPath $mcpJsonPath -Raw | ConvertFrom-Json
$serverNames = @($mcpConfig.mcpServers.PSObject.Properties.Name)
if ($serverNames.Count -eq 0) {
    throw "No servers are declared in $mcpJsonPath"
}
Write-Host "  servers: $($serverNames -join ', ')"

# Guard against machine specific values sneaking back into the committed file.
foreach ($name in $serverNames) {
    $server = $mcpConfig.mcpServers.$name

    $commandName = [string]$server.command
    if ($commandName -match '^[A-Za-z]:' -or $commandName -match '^/') {
        Add-Finding "server '$name' uses an absolute command path ('$commandName'); move it to the user-level override."
    }
    if ($commandName -match '\$\{') {
        Add-Finding "server '$name' uses a placeholder ('$commandName'); Qoder does not expand these in MCP config."
    }

    $argv = @()
    if ($null -ne $server.args) {
        $argv = @($server.args)
    }
    foreach ($argument in $argv) {
        $argumentText = [string]$argument
        if ($argumentText -match '\$\{') {
            Add-Finding "server '$name' passes a placeholder argument ('$argumentText'); Qoder does not expand these in MCP config."
            continue
        }
        # Relative script arguments are resolved against "cwd", which must stay the workspace root.
        if ($argumentText -match '\.(py|js|cjs|mjs|ps1|cmd|bat)$' -and $argumentText -notmatch '^[A-Za-z]:' -and $argumentText -notmatch '^/') {
            $resolved = Join-Path $repoRoot $argumentText
            if (Test-Path -LiteralPath $resolved) {
                Write-Host "  ok: '$name' -> $argumentText"
            }
            else {
                Add-Finding "server '$name' references a missing script relative to the repo root: $argumentText"
            }
        }
    }
}

# ---------------------------------------------------------------- 2. titan bridge interpreter
Write-Host ''
Write-Host '[2/5] Titan bridge interpreter (PATH "python")'

$pathPython = Get-Command python -ErrorAction SilentlyContinue
$pythonArgs = @()
$bridgeOk = $false
if ($null -eq $pathPython) {
    Add-Finding 'python was not found on PATH; the committed .mcp.json expects a PATH-visible interpreter.'
}
else {
    $pythonVersion = Get-PythonVersion -PythonCommand $pathPython.Source -PythonArguments $pythonArgs
    Write-Host "  python: $($pathPython.Source) ($pythonVersion)"
}

if (-not (Test-Path -LiteralPath $bridgeScript)) {
    Add-Finding "bridge script is missing: $bridgeScript"
}
else {
    Write-Host "  bridge: $bridgeScript"

    # The bridge is a hand rolled stdio JSON-RPC server (argparse/json/socket only), so starting it
    # is the authoritative check: requirements.txt lists packages the bridge itself may not import.
    if ($null -ne $pathPython) {
        $bridgeProbe = Invoke-Probe -FilePath $pathPython.Source -Arguments (@($pythonArgs) + @($bridgeScript, '--help'))
        if ($bridgeProbe.ExitCode -eq 0) {
            Write-Host '  ok: bridge starts'
            $bridgeOk = $true
        }
        else {
            Add-Finding "bridge failed to start: $($bridgeProbe.Output -join ' ')"
        }
    }
}

# ---------------------------------------------------------------- 3. bridge dependencies
Write-Host ''
Write-Host '[3/5] Bridge python packages'

# requirements.txt is the single source of truth for the bridge dependencies.
$requiredModules = @()
if (Test-Path -LiteralPath $requirementsPath) {
    foreach ($line in Get-Content -LiteralPath $requirementsPath) {
        $entry = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($entry) -or $entry.StartsWith('#')) {
            continue
        }
        $moduleMatch = [regex]::Match($entry, '^([A-Za-z0-9_.\-]+)')
        if ($moduleMatch.Success) {
            # Package names use '-' while import names use '_'.
            $requiredModules += $moduleMatch.Groups[1].Value.Replace('-', '_')
        }
    }
}
else {
    Add-Finding "requirements file is missing: $requirementsPath"
}

$missingModules = @()
if ($null -ne $pathPython) {
    foreach ($module in $requiredModules) {
        if (Test-PythonModule -PythonCommand $pathPython.Source -PythonArguments $pythonArgs -ModuleName $module) {
            Write-Host "  ok: $module"
        }
        else {
            $missingModules += $module
            Add-Note "python package '$module' (declared in requirements.txt) is missing; run SetupMCP.bat -InstallPythonPackages to add it"
        }
    }
}

if ($missingModules.Count -gt 0 -and -not $CheckOnly -and $InstallPythonPackages) {
    if (-not (Test-Path -LiteralPath $requirementsPath)) {
        Add-Finding "requirements file is missing: $requirementsPath"
    }
    else {
        Write-Host ''
        Invoke-External -FilePath $pathPython.Source -Arguments (@($pythonArgs) + @('-m', 'pip', 'install', '-r', $requirementsPath))
        foreach ($module in $missingModules) {
            if (Test-PythonModule -PythonCommand $pathPython.Source -PythonArguments $pythonArgs -ModuleName $module) {
                Write-Host "  installed: $module"
            }
            else {
                Add-Note "python package '$module' is still missing after pip install"
            }
        }
    }
}

# ---------------------------------------------------------------- 4. renderdoc interpreter
Write-Host ''
Write-Host "[4/5] renderdoc interpreter (py -$RenderdocPythonVersion)"

$pyLauncher = Get-Command py -ErrorAction SilentlyContinue
$renderdocCommand = $null
$renderdocArgs = @()

if ($null -eq $pyLauncher) {
    Add-Finding 'the py launcher was not found; renderdoc cannot select its required python version on this machine.'
}
else {
    $probeResult = Invoke-Probe -FilePath $pyLauncher.Source -Arguments @("-$RenderdocPythonVersion", '-c', 'import sys; print(sys.executable)')
    if ($probeResult.ExitCode -ne 0 -or $probeResult.Output.Count -eq 0) {
        Add-Finding "python $RenderdocPythonVersion is not installed; run 'py -0' to list the available versions."
    }
    else {
        $renderdocCommand = $pyLauncher.Source
        $renderdocArgs = @("-$RenderdocPythonVersion")
        $renderdocPython = $probeResult.Output[-1].Trim()
        Write-Host "  python $RenderdocPythonVersion : $renderdocPython"

        # renderdoc-mcp bundles its own renderdoc.pyd under site-packages\renderdoc_mcp\lib,
        # so nothing points at the RenderDoc install. The real failure mode is the mcp SDK:
        # the server imports mcp.server.fastmcp, which mcp 2.x renamed away.
        $serverProbe = Invoke-Probe -FilePath $renderdocCommand -Arguments (@($renderdocArgs) + @('-c', 'import renderdoc_mcp.server'))
        if ($serverProbe.ExitCode -eq 0) {
            Write-Host '  ok: renderdoc_mcp server imports'
        }
        else {
            $reason = ($serverProbe.Output | Select-Object -Last 1)
            Add-Finding "renderdoc_mcp cannot start; install with 'py -$RenderdocPythonVersion -m pip install renderdoc-mcp \"mcp<2\"': $reason"
        }
    }
}

# ---------------------------------------------------------------- 5. user-level override
Write-Host ''
Write-Host '[5/5] User-level override (~/.qoder/mcp.json)'

if (-not $WriteUserOverride) {
    Write-Host '  skipped (pass -WriteUserOverride to merge absolute-path entries)'
}
elseif ($CheckOnly) {
    Write-Host '  skipped (-CheckOnly)'
}
else {
    if ($null -eq $pathPython) {
        Add-Finding 'cannot write the override without a PATH-visible python.'
    }
    else {
        $override = [ordered]@{ mcpServers = [ordered]@{} }

        # Preserve every entry this repo does not own so hand made overrides survive.
        if (Test-Path -LiteralPath $userMcpJsonPath) {
            try {
                $existing = Get-Content -LiteralPath $userMcpJsonPath -Raw | ConvertFrom-Json
                if ($null -ne $existing.mcpServers) {
                    foreach ($property in $existing.mcpServers.PSObject.Properties) {
                        $override.mcpServers[$property.Name] = $property.Value
                    }
                }
            }
            catch {
                throw "Existing user MCP config is not valid JSON, refusing to overwrite it: $userMcpJsonPath ($($_.Exception.Message))"
            }
        }

        # The project entry already works when a PATH visible interpreter can run the bridge;
        # writing an override in that case would mask later changes to the committed .mcp.json.
        if ($null -ne $pathPython -and (Test-Path -LiteralPath $bridgeScript)) {
            if ($bridgeOk) {
                Write-Host '  titan-engine-mcp: project entry works, no override needed'
            }
            else {
                $override.mcpServers['titan-engine-mcp'] = [ordered]@{
                    command = $pathPython.Source
                    args    = @($bridgeScript, '--port', '18118')
                    cwd     = $repoRoot
                }
                Write-Host '  titan-engine-mcp: override written (project entry did not start)'
            }
        }

        if ($null -ne $renderdocCommand) {
            $override.mcpServers['renderdoc'] = [ordered]@{
                command = $renderdocCommand
                args    = @($renderdocArgs) + @('-m', 'renderdoc_mcp')
            }
        }
        else {
            Write-Host '  renderdoc: interpreter unavailable, existing entry left untouched'
        }

        $overrideDirectory = Split-Path -Parent $userMcpJsonPath
        if (-not (Test-Path -LiteralPath $overrideDirectory)) {
            New-Item -ItemType Directory -Force -Path $overrideDirectory | Out-Null
        }
        $jsonText = $override | ConvertTo-Json -Depth 8
        # UTF-8 without BOM: Set-Content -Encoding UTF8 emits a BOM on PowerShell 5.1 and a
        # JSON parser that does not strip it (JSON.parse) fails on the very first character.
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($userMcpJsonPath, $jsonText + [Environment]::NewLine, $utf8NoBom)
        Write-Host "  written: $userMcpJsonPath"
        Write-Host "  entries: $($override.mcpServers.Keys -join ', ')"
    }
}

# ---------------------------------------------------------------- summary
Write-Host ''
Write-Host '=== Summary ===' -ForegroundColor Cyan
if ($findings.Count -eq 0) {
    Write-Host 'All checks passed.' -ForegroundColor Green
}
else {
    Write-Host "$($findings.Count) issue(s) need attention:" -ForegroundColor Yellow
    foreach ($finding in $findings) {
        Write-Host "  - $finding"
    }
}

if ($notes.Count -gt 0) {
    Write-Host ''
    Write-Host "$($notes.Count) note(s):"
    foreach ($note in $notes) {
        Write-Host "  - $note"
    }
}

Write-Host ''
Write-Host 'Reminder: Qoder loads the project .mcp.json only at IDE start. Restart the IDE, then'
Write-Host 'check Settings -> MCP Server. A connected titan-engine-mcp still reports'
Write-Host '"engine call failed ... connection refused" until the editor is running on port 18118.'
Write-Host ''

if ($findings.Count -gt 0) {
    exit 1
}
exit 0
