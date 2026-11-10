#requires -Version 5.1
<#
.SYNOPSIS
    LP64 guard: cross-platform native code must not use long / unsigned long / ULONG style types.
.DESCRIPTION
    On Windows (LLP64) long is 4 bytes; on macOS/iOS/Android (LP64) it is 8 bytes; using it in struct fields or glue return values makes the two sides disagree.
    Comments and string/char literals are stripped before scanning; long long and long double do not count.
    Lines that must keep it (CRT/system API signatures) end with '// lp64-ok: <reason>'; whole-file/directory exceptions go into $AllowList with a reason.
    Known limitation: a '/*' inside a string makes everything up to '*/' be treated as a comment (only false negatives, never false positives); raw strings R"(...)" are not recognized.
    The script must run under ConstrainedLanguage without calling non-core .NET types.
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File BuildScript/check/Check-NativeLongTypes.ps1
#>
param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath,
    [string[]]$Paths = @('NativeCode', 'Core.Window', 'Core.Mac', 'codegen/Windows/NativeBinder', 'codegen/Mac/NativeBinder')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# -Root must be the repo root (-Paths are relative to it); normalize before extracting relative paths
$Root = (Resolve-Path -LiteralPath $Root).ProviderPath.TrimEnd('\', '/')

# Prefix match (relative to $Root, forward slashes, case-insensitive)
$AllowList = [ordered]@{
    'NativeCode/ImGui/'                              = 'Third-party ImGui/implot/stb, maintained upstream'
    'NativeCode/Platform/Windows/'                   = 'Windows SDK COM signatures such as ICorProfilerCallback (ULONG); only compiled on Windows'
    'NativeCode/NextRHI/Dx12/'                       = 'D3D12 backend and d3dx12.h; only compiled on Windows'
    'NativeCode/Bricks/TcpClient/TcpClient.cpp'      = 'u_long parameter of Winsock ioctlsocket'
    'NativeCode/Bricks/DestinyPX/NxMath/NxUtility.h' = 'Int128 conversion overloads for all built-in integer types; not part of cross-boundary layout'
    'Core.Mac/build/'                                = 'Build outputs (including the normalized glue copies)'
}
$Extensions = @('.h', '.hpp', '.hh', '.inl', '.c', '.cc', '.cpp', '.cxx', '.m', '.mm')
$TypePattern = '\b(?:ULONG|LPULONG|LPCULONG|ulong)\b|(?<!\blong\s+)\blong\b(?!\s+(?:long|double)\b)'

function Get-RelativePath([string]$FullName) {
    return $FullName.Substring($Root.Length).TrimStart('\', '/').Replace('\', '/')
}

function Test-Allowed([string]$RelativePath) {
    foreach ($prefix in $AllowList.Keys) {
        if ($RelativePath.ToLowerInvariant().StartsWith($prefix.ToLowerInvariant())) {
            return $true
        }
    }
    return $false
}

function Remove-BlockComments([string]$Text) {
    # Replace block comments with the same number of newlines to keep line numbers stable
    $parts = @()
    $last = 0
    foreach ($m in [regex]::Matches($Text, '(?s)/\*.*?\*/')) {
        $parts += $Text.Substring($last, $m.Index - $last)
        $parts += ($m.Value -replace '[^\n]', '')
        $last = $m.Index + $m.Length
    }
    $parts += $Text.Substring($last)
    return ($parts -join '')
}

$violations = @()
$candidates = 0
$scanned = 0
foreach ($p in $Paths) {
    $dir = Join-Path $Root $p
    if (-not (Test-Path -LiteralPath $dir)) {
        Write-Output "[lp64] skip missing $p"
        continue
    }
    foreach ($file in Get-ChildItem -LiteralPath $dir -Recurse -File) {
        if ($Extensions -notcontains $file.Extension.ToLowerInvariant()) {
            continue
        }
        $candidates++
        $rel = Get-RelativePath $file.FullName
        if (Test-Allowed $rel) {
            continue
        }
        $scanned++
        $text = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        if ($null -eq $text) {
            continue
        }
        $text = $text.Replace("`r`n", "`n")
        $original = $text.Split("`n")
        $code = (Remove-BlockComments $text).Split("`n")
        for ($i = 0; $i -lt $code.Length; $i++) {
            if ($original[$i].Contains('lp64-ok')) {
                continue
            }
            $line = $code[$i] -replace '"(?:\\.|[^"\\])*"', '""' -replace "'(?:\\.|[^'\\])+'", "''"
            $line = $line -replace '//.*$', ''
            if ($line -cmatch $TypePattern) {
                $violations += ('{0}:{1}: {2}' -f $rel, ($i + 1), $original[$i].Trim())
            }
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Output $_ }
    Write-Output ("[lp64] {0} violation(s) in {1} scanned file(s). Use fixed-width types such as int/UINT/INT64; append '// lp64-ok: <reason>' at end of line for CRT/system API signatures." -f $violations.Count, $scanned)
    exit 1
}
if ($candidates -eq 0) {
    Write-Output "[lp64] no source files found under $Root; check -Root/-Paths"
    exit 1
}
Write-Output "[lp64] OK (scanned $scanned files)"
exit 0
