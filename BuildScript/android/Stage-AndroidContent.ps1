#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$RepositoryRoot,
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [Parameter(Mandatory = $true)][ValidateSet('Debug', 'Release')][string]$Configuration
)

# Stage Android runtime content into intermediate\android\content\<Configuration> according to the TitanContent.txt mapping,
# and generate a build-id manifest for TitanContentInstaller to decide whether reinstallation is needed.
# Intermediates never enter the Module project directories, to keep them separate from project sources and build outputs.
# Under ConstrainedLanguage [IO.Path]/[Text.Encoding]/[Security.Cryptography] are unavailable:
# normalize paths manually and always use Get-FileHash for hashing.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-NormalPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $text = $Path.Trim().Trim('"').Replace('/', '\')
    if ($text.Length -eq 0) { throw 'Path must not be empty' }

    $prefix = ''
    if ($text -match '^[A-Za-z]:') { $prefix = $text.Substring(0, 2); $text = $text.Substring(2) }
    elseif ($text.StartsWith('\\')) { throw "UNC paths are not supported: $Path" }

    $segments = @()
    foreach ($segment in ($text -split '\\')) {
        if ($segment -eq '' -or $segment -eq '.') { continue }
        if ($segment -eq '..') {
            if ($segments.Count -eq 0) { throw "Path escapes the root: $Path" }
            if ($segments.Count -eq 1) { $segments = @() } else { $segments = @($segments[0..($segments.Count - 2)]) }
            continue
        }
        $segments += $segment
    }

    if ($prefix.Length -eq 0) { throw "Only absolute paths are accepted: $Path" }
    return $prefix + '\' + ($segments -join '\')
}

function Assert-AsciiText {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$What
    )

    # The manifest is written as ASCII (no UTF8-no-BOM encoder available under ConstrainedLanguage),
    # so non-ASCII content paths must be rejected explicitly instead of being silently replaced with '?'.
    if ($Text -match '[^\x20-\x7E]') { throw "$What contains non-ASCII characters; the Android content manifest only supports ASCII paths: $Text" }
}

$root = ConvertTo-NormalPath $RepositoryRoot
$out = ConvertTo-NormalPath $OutputDirectory
$expectedOut = ConvertTo-NormalPath (Join-Path $root "intermediate\android\content\$Configuration")
if ($out -ine $expectedOut) {
    throw "Staging is only allowed into intermediate\android\content\$Configuration, got '$out'"
}

$contentList = Join-Path $root 'Module\AloneGame.Maui\TitanContent.txt'
if (-not (Test-Path -LiteralPath $contentList -PathType Leaf)) { throw "Missing content manifest: $contentList" }

if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
New-Item -ItemType Directory -Force -Path $out | Out-Null

$allowedPrefixes = @('content/', 'enginecontent/', 'binaries/')
$map = @{}
foreach ($rawPattern in @(Get-Content -LiteralPath $contentList)) {
    $pattern = $rawPattern.Trim()
    if ($pattern.Length -eq 0 -or $pattern.StartsWith('#')) { continue }
    Assert-AsciiText $pattern 'content pattern'
    if ($pattern.Contains('\') -or $pattern.Contains(':') -or $pattern.Split('/') -contains '..') {
        throw "Content pattern must not contain backslashes, drive letters, or '..': $pattern"
    }

    if ($pattern -eq 'binaries/Plugins/DataCopyer.plugin') {
        # The descriptor has no subdirectory form under the Android output directory; it must be resolved per mapping.
        $source = Join-Path $root 'binaries\android\Plugins\DataCopyer.plugin'
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing $source" }
        $map[$pattern] = $source
        continue
    }

    if ($pattern.EndsWith('/**')) {
        $base = Join-Path $root ($pattern.Substring(0, $pattern.Length - 3) -replace '/', '\')
        if (-not (Test-Path -LiteralPath $base -PathType Container)) { throw "Content pattern matched no directory: $pattern" }
        $files = @(Get-ChildItem -LiteralPath $base -File -Recurse)
    }
    elseif ($pattern.Contains('*')) {
        $files = @(Get-ChildItem -Path (Join-Path $root ($pattern -replace '/', '\')) -File -Recurse -ErrorAction SilentlyContinue)
    }
    else {
        $files = @(Get-Item -LiteralPath (Join-Path $root ($pattern -replace '/', '\')) -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer })
    }

    if ($files.Count -eq 0) { throw "Content pattern matched no files: $pattern" }
    Write-Host ("[TITAN-CONTENT] {0} - {1} file(s)" -f $pattern, $files.Count)
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($root.Length + 1).Replace('\', '/')
        Assert-AsciiText $relative 'content path'
        $map[$relative] = $file.FullName
    }
}

# The DataCopyer managed assembly is not in the content patterns: it is produced by the S1 stage into binaries/android.
$dataCopyerDll = 'binaries/Plugins/DataCopyer/DataCopyer.All.dll'
$dataCopyerSource = Join-Path $root 'binaries\android\Plugins\DataCopyer\DataCopyer.All.dll'
if (-not (Test-Path -LiteralPath $dataCopyerSource -PathType Leaf)) { throw "Missing $dataCopyerSource; run build-android.ps1 -Stage Managed first" }
$map[$dataCopyerDll] = $dataCopyerSource
Write-Host "[TITAN-CONTENT] $dataCopyerDll - 1 file(s)"

$paths = @($map.Keys | Sort-Object)
foreach ($relative in $paths) {
    $prefix = ($relative -split '/')[0] + '/'
    if ($allowedPrefixes -notcontains $prefix) { throw "Content path is not under content/enginecontent/binaries: $relative" }
}

$fingerprintLines = @()
foreach ($relative in $paths) {
    $source = $map[$relative]
    $destination = Join-Path $out ($relative -replace '/', '\')
    New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
    $fingerprintLines += "$relative=$((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash)"
}

# build-id = SHA256 over each file's relative path + content hash: any content change necessarily changes the id.
$fingerprintFile = Join-Path $env:TEMP ("titan-content-{0}-{1}.txt" -f $PID, (Get-Random))
try {
    Set-Content -LiteralPath $fingerprintFile -Value ($fingerprintLines -join "`n") -Encoding Ascii
    $buildId = (Get-FileHash -LiteralPath $fingerprintFile -Algorithm SHA256).Hash.ToLowerInvariant()
}
finally {
    if (Test-Path -LiteralPath $fingerprintFile) { Remove-Item -LiteralPath $fingerprintFile -Force }
}

Set-Content -LiteralPath (Join-Path $out 'manifest.txt') -Value (@($buildId) + $paths) -Encoding Ascii

$statePath = Join-Path $root 'codegen\Android\DataCopyer\.datacopyer.state.json'
if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) { throw "Missing S1 state: $statePath; run build-android.ps1 -Stage Managed first" }
$state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
if ([string]$state.Platform -ne 'Android') { throw "S1 state platform is not Android: $($state.Platform)" }
Set-Content -LiteralPath (Join-Path $out 'datacopyer.hash') -Value ([string]$state.Hash) -Encoding Ascii

Write-Host "Staged $($paths.Count) file(s) into $out (build $buildId)"
