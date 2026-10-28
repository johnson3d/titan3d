#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$scriptPath = Join-Path $repoRoot 'BuildScript\check\Check-NativeLongTypes.ps1'
$tempRoot = Join-Path $repoRoot ("cache\NativeLongTypes-{0}-{1}" -f $PID, (Get-Random))
$hostExe = (Get-Process -Id $PID).Path

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Write-Fixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $path = Join-Path $tempRoot $RelativePath
    New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent) | Out-Null
    Set-Content -LiteralPath $path -Value $Content -NoNewline -Encoding UTF8
}

function Invoke-Guard([string]$Root = $tempRoot) {
    $output = & $hostExe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Root $Root 2>&1 | Out-String
    return @{ ExitCode = $LASTEXITCODE; Output = $output }
}

try {
    Write-Fixture 'NativeCode/Bad.h' "typedef unsigned long ULONG;`nlong a;`nunsigned long b;`n"
    Write-Fixture 'NativeCode/Good.h' ((@(
        'long long a;'
        'unsigned long long b;'
        'long double c;'
        '// long in comment'
        '/* long'
        '   unsigned long */'
        'const char* s = "unsigned long";'
        "char q = 'l';"
        'long e; // lp64-ok: fixture'
        'int LONG_VALUE = 0;'
    ) -join "`n") + "`n")
    Write-Fixture 'NativeCode/ImGui/third.h' "long x;`n"
    Write-Fixture 'codegen/Windows/NativeBinder/Foo.cpp2cs.cpp' "extern `"C`" unsigned long F();`n"
    Write-Fixture 'codegen/Mac/NativeBinder/Foo.cpp2cs.cpp' "extern `"C`" unsigned long F();`n"
    Write-Fixture 'NativeCode/AfterComment.h' "/* a`n   b */`nULONG z;`n"

    $r = Invoke-Guard
    Assert-True ($r.ExitCode -eq 1) "violations must exit 1, got $($r.ExitCode)`n$($r.Output)"
    foreach ($expected in @('NativeCode/Bad.h:1:', 'NativeCode/Bad.h:2:', 'NativeCode/Bad.h:3:', 'codegen/Windows/NativeBinder/Foo.cpp2cs.cpp:1:', 'codegen/Mac/NativeBinder/Foo.cpp2cs.cpp:1:', 'NativeCode/AfterComment.h:3:')) {
        Assert-True ($r.Output.Contains($expected)) "missing violation $expected`n$($r.Output)"
    }
    Assert-True (-not $r.Output.Contains('Good.h')) "false positive in Good.h`n$($r.Output)"
    Assert-True (-not $r.Output.Contains('ImGui')) "allow-listed ImGui reported`n$($r.Output)"

    Remove-Item -LiteralPath (Join-Path $tempRoot 'NativeCode/Bad.h') -Force
    Remove-Item -LiteralPath (Join-Path $tempRoot 'codegen/Windows/NativeBinder/Foo.cpp2cs.cpp') -Force
    Remove-Item -LiteralPath (Join-Path $tempRoot 'codegen/Mac/NativeBinder/Foo.cpp2cs.cpp') -Force
    Remove-Item -LiteralPath (Join-Path $tempRoot 'NativeCode/AfterComment.h') -Force
    $r = Invoke-Guard
    Assert-True ($r.ExitCode -eq 0) "clean tree must exit 0, got $($r.ExitCode)`n$($r.Output)"
    Assert-True ($r.Output.Contains('[lp64] OK')) "missing OK banner`n$($r.Output)"

    # 相对 Root 必须先归一化，否则相对路径截取错位、allowlist 失效而报 ImGui
    Push-Location $tempRoot
    try {
        $r = Invoke-Guard '.'
    }
    finally {
        Pop-Location
    }
    Assert-True ($r.ExitCode -eq 0) "relative root must exit 0, got $($r.ExitCode)`n$($r.Output)"

    # 只命中 allowlist 的定向扫描不算错配
    $r = & $hostExe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Root $tempRoot -Paths 'NativeCode/ImGui' 2>&1 | Out-String
    Assert-True ($LASTEXITCODE -eq 0) "allow-listed-only scan must exit 0, got $LASTEXITCODE`n$r"

    # 错配 Root 导致一个文件都没扫到时不能静默放行
    $r = Invoke-Guard (Join-Path $tempRoot 'NativeCode')
    Assert-True ($r.ExitCode -eq 1) "empty scan must exit 1, got $($r.ExitCode)`n$($r.Output)"

    Write-Host 'NativeLongTypes tests passed.'
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
# 预期失败的 fixture 会留下 LASTEXITCODE=1；全部断言通过后显式返回成功。
exit 0
