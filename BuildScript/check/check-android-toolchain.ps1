#requires -Version 5.1

# Validate the Android toolchain config. Only two exit codes exist: 0 = all tools for this stage are present; 2 = the user must supply config or tools.
# This script never downloads, installs, or guesses paths; it only reads a copy of BuildScript\android\AndroidToolchain.example.json.
# Argument errors, corrupted config, and missing dependency scripts also return 2; no exceptions leak to the caller.

[CmdletBinding()]
param(
    [string]$Stage = 'Native',

    [string]$ConfigPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$exitCode = 2
try {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
    $toolsScript = Join-Path $PSScriptRoot '..\android\AndroidTools.ps1'
    if (-not (Test-Path -LiteralPath $toolsScript -PathType Leaf)) {
        throw "AndroidTools.ps1 was not found next to this script: '$toolsScript'."
    }
    . $toolsScript

    $result = Get-AndroidToolchainCheckResult -RepositoryRoot $repoRoot -Stage $Stage -ConfigPath $ConfigPath
    $exitCode = $result.Code
    if ($exitCode -eq 0) {
        Write-Host $result.Message
    }
    else {
        Write-Host -ForegroundColor Yellow $result.Message
    }
}
catch {
    Write-Host -ForegroundColor Yellow $_.Exception.Message
    $exitCode = 2
}

exit $exitCode
