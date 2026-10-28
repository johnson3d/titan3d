#requires -Version 5.1
<#
.SYNOPSIS
    LP64 守护：跨平台原生代码禁止 long / unsigned long / ULONG 系列类型。
.DESCRIPTION
    Windows(LLP64) 上 long 是 4 字节，macOS/iOS/Android(LP64) 上是 8 字节；结构体字段、胶水返回值一旦用到就会两端不一致。
    扫描前剥离注释与字符串/字符字面量；long long、long double 不算。
    确需保留的行（CRT/系统 API 签名）在行尾写 '// lp64-ok: <原因>'；整文件/整目录例外写进 $AllowList 并注明原因。
    已知局限：字符串里出现 '/*' 会导致之后到 '*/' 的代码被当成注释（只会漏报不会误报）；原始字符串 R"(...)" 不识别。
    脚本需在 ConstrainedLanguage 下可运行，不调用非核心 .NET 类型。
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File BuildScript/check/Check-NativeLongTypes.ps1
#>
param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath,
    [string[]]$Paths = @('NativeCode', 'Core.Window', 'Core.Mac', 'codegen/Windows/NativeBinder', 'codegen/Mac/NativeBinder')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# -Root 必须是仓库根（-Paths 相对它）；归一化后才能正确截取相对路径
$Root = (Resolve-Path -LiteralPath $Root).ProviderPath.TrimEnd('\', '/')

# 前缀匹配（相对 $Root，正斜杠，不区分大小写）
$AllowList = [ordered]@{
    'NativeCode/ImGui/'                              = '第三方 ImGui/implot/stb，按上游维护'
    'NativeCode/Platform/Windows/'                   = 'ICorProfilerCallback 等 Windows SDK COM 签名（ULONG），只在 Windows 编译'
    'NativeCode/NextRHI/Dx12/'                       = 'D3D12 后端与 d3dx12.h，只在 Windows 编译'
    'NativeCode/Bricks/TcpClient/TcpClient.cpp'      = 'Winsock ioctlsocket 的 u_long 参数'
    'NativeCode/Bricks/DestinyPX/NxMath/NxUtility.h' = 'Int128 对全部内建整数类型的转换重载，不参与跨边界布局'
    'Core.Mac/build/'                                = '构建产物（含规范化后的胶水副本）'
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
    # 块注释换成同样数量的换行，保持行号不变
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
    Write-Output ("[lp64] {0} violation(s) in {1} scanned file(s). 改用 int/UINT/INT64 等定长类型；CRT/系统 API 签名在行尾加 '// lp64-ok: <原因>'." -f $violations.Count, $scanned)
    exit 1
}
if ($candidates -eq 0) {
    Write-Output "[lp64] no source files found under $Root; check -Root/-Paths"
    exit 1
}
Write-Output "[lp64] OK (scanned $scanned files)"
exit 0
