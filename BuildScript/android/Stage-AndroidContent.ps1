#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$RepositoryRoot,
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [Parameter(Mandatory = $true)][ValidateSet('Debug', 'Release')][string]$Configuration
)

# 把 Android 运行期内容按 TitanContent.txt 的映射暂存到 intermediate\android\content\<Configuration>，
# 并生成 build-id manifest，供 TitanContentInstaller 判断是否需要重装。
# 中间产物一律不进 Module 工程目录，避免与项目源码/产物混在一起。
# ConstrainedLanguage 下不能用 [IO.Path]/[Text.Encoding]/[Security.Cryptography]：
# 路径手工规范化，哈希一律走 Get-FileHash。

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-NormalPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $text = $Path.Trim().Trim('"').Replace('/', '\')
    if ($text.Length -eq 0) { throw '路径不能为空' }

    $prefix = ''
    if ($text -match '^[A-Za-z]:') { $prefix = $text.Substring(0, 2); $text = $text.Substring(2) }
    elseif ($text.StartsWith('\\')) { throw "不支持 UNC 路径：$Path" }

    $segments = @()
    foreach ($segment in ($text -split '\\')) {
        if ($segment -eq '' -or $segment -eq '.') { continue }
        if ($segment -eq '..') {
            if ($segments.Count -eq 0) { throw "路径越界：$Path" }
            if ($segments.Count -eq 1) { $segments = @() } else { $segments = @($segments[0..($segments.Count - 2)]) }
            continue
        }
        $segments += $segment
    }

    if ($prefix.Length -eq 0) { throw "只接受绝对路径：$Path" }
    return $prefix + '\' + ($segments -join '\')
}

function Assert-AsciiText {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$What
    )

    # manifest 用 ASCII 落盘（ConstrainedLanguage 下拿不到 UTF8 无 BOM 编码器），
    # 所以非 ASCII 的内容路径必须显式报错，不能被悄悄替换成 '?'。
    if ($Text -match '[^\x20-\x7E]') { throw "$What 含非 ASCII 字符，Android 内容清单只支持 ASCII 路径：$Text" }
}

$root = ConvertTo-NormalPath $RepositoryRoot
$out = ConvertTo-NormalPath $OutputDirectory
$expectedOut = ConvertTo-NormalPath (Join-Path $root "intermediate\android\content\$Configuration")
if ($out -ine $expectedOut) {
    throw "只允许暂存到 intermediate\android\content\$Configuration，收到 '$out'"
}

$contentList = Join-Path $root 'Module\AloneGame.Maui\TitanContent.txt'
if (-not (Test-Path -LiteralPath $contentList -PathType Leaf)) { throw "缺少内容清单：$contentList" }

if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
New-Item -ItemType Directory -Force -Path $out | Out-Null

$allowedPrefixes = @('content/', 'enginecontent/', 'binaries/')
$map = @{}
foreach ($rawPattern in @(Get-Content -LiteralPath $contentList)) {
    $pattern = $rawPattern.Trim()
    if ($pattern.Length -eq 0 -or $pattern.StartsWith('#')) { continue }
    Assert-AsciiText $pattern '内容模式'
    if ($pattern.Contains('\') -or $pattern.Contains(':') -or $pattern.Split('/') -contains '..') {
        throw "内容模式不能包含反斜杠、盘符或 '..'：$pattern"
    }

    if ($pattern -eq 'binaries/Plugins/DataCopyer.plugin') {
        # 描述符在 Android 产物目录下没有子目录形式，必须按映射单独取。
        $source = Join-Path $root 'binaries\android\Plugins\DataCopyer.plugin'
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "缺少 $source" }
        $map[$pattern] = $source
        continue
    }

    if ($pattern.EndsWith('/**')) {
        $base = Join-Path $root ($pattern.Substring(0, $pattern.Length - 3) -replace '/', '\')
        if (-not (Test-Path -LiteralPath $base -PathType Container)) { throw "内容模式没有匹配目录：$pattern" }
        $files = @(Get-ChildItem -LiteralPath $base -File -Recurse)
    }
    elseif ($pattern.Contains('*')) {
        $files = @(Get-ChildItem -Path (Join-Path $root ($pattern -replace '/', '\')) -File -Recurse -ErrorAction SilentlyContinue)
    }
    else {
        $files = @(Get-Item -LiteralPath (Join-Path $root ($pattern -replace '/', '\')) -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer })
    }

    if ($files.Count -eq 0) { throw "内容模式没有匹配文件：$pattern" }
    Write-Host ("[TITAN-CONTENT] {0} - {1} file(s)" -f $pattern, $files.Count)
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($root.Length + 1).Replace('\', '/')
        Assert-AsciiText $relative '内容路径'
        $map[$relative] = $file.FullName
    }
}

# DataCopyer 的托管程序集不在内容模式里：它由 S1 阶段产出到 binaries/android 下。
$dataCopyerDll = 'binaries/Plugins/DataCopyer/DataCopyer.All.dll'
$dataCopyerSource = Join-Path $root 'binaries\android\Plugins\DataCopyer\DataCopyer.All.dll'
if (-not (Test-Path -LiteralPath $dataCopyerSource -PathType Leaf)) { throw "缺少 $dataCopyerSource；先运行 build-android.ps1 -Stage Managed" }
$map[$dataCopyerDll] = $dataCopyerSource
Write-Host "[TITAN-CONTENT] $dataCopyerDll - 1 file(s)"

$paths = @($map.Keys | Sort-Object)
foreach ($relative in $paths) {
    $prefix = ($relative -split '/')[0] + '/'
    if ($allowedPrefixes -notcontains $prefix) { throw "内容路径不在 content/enginecontent/binaries 之下：$relative" }
}

$fingerprintLines = @()
foreach ($relative in $paths) {
    $source = $map[$relative]
    $destination = Join-Path $out ($relative -replace '/', '\')
    New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
    $fingerprintLines += "$relative=$((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash)"
}

# build-id = 每个文件相对路径 + 内容哈希的 SHA256：内容变则必然换 id。
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
if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) { throw "缺少 S1 状态：$statePath；先运行 build-android.ps1 -Stage Managed" }
$state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
if ([string]$state.Platform -ne 'Android') { throw "S1 状态平台不是 Android：$($state.Platform)" }
Set-Content -LiteralPath (Join-Path $out 'datacopyer.hash') -Value ([string]$state.Hash) -Encoding Ascii

Write-Host "Staged $($paths.Count) file(s) into $out (build $buildId)"
