#requires -Version 5.1

# 校验 Android 工具链配置。退出码只有两种：0 = 该阶段工具齐全；2 = 需要用户补齐配置或工具。
# 本脚本不下载、不安装、不猜测路径，只读 BuildScript\android\AndroidToolchain.example.json 的副本。
# 参数错误、配置损坏、依赖脚本缺失同样返回 2，不把异常泄漏给调用方。

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
