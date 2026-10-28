#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# 引擎配置布局契约：
#   1) 所有启动配置都放在 content\config 下，content 根不允许残留 .jscfg
#   2) 主配置统一命名为 engineconfig_<平台>_<功能>.jscfg
#   3) 代码、脚本、工程文件与文档里不得再引用搬迁前的旧路径

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$contentRoot = Join-Path $repoRoot 'content'
$configRoot = Join-Path $contentRoot 'config'

# 搬迁前（content 根）-> 搬迁后（相对 content 的路径）
$moved = [ordered]@{
    'engineconfigdx11.jscfg'          = 'config\engineconfig_windows_dx11.jscfg'
    'engineconfigdx12.jscfg'          = 'config\engineconfig_windows_dx12.jscfg'
    'engineconfigvk.jscfg'            = 'config\engineconfig_windows_vk.jscfg'
    'engineconfigmac.jscfg'           = 'config\engineconfig_mac_vk.jscfg'
    'engineconfigforcook.jscfg'       = 'config\engineconfig_windows_cook.jscfg'
    'engineconfigforrootserver.jscfg' = 'config\engineconfig_windows_rootserver.jscfg'
    'alonegame.jscfg'                 = 'config\engineconfig_windows_alonegame.jscfg'
    'appbrickmanager.jscfg'           = 'config\engineconfig_windows_brickmanager.jscfg'
    'applocalization.jscfg'           = 'config\engineconfig_windows_localization.jscfg'
    'apptracer.jscfg'                 = 'config\engineconfig_windows_tracer.jscfg'
    'slateapp.jscfg'                  = 'config\engineconfig_windows_slateapp.jscfg'
}

# 已经在 content\config 下、但文件名不符合 <平台>_<功能> 规则的配置
$renamedInPlace = [ordered]@{
    'engineconfigfortest.jscfg' = 'engineconfig_windows_test.jscfg'
}

$problems = @()

# 1. content 根不再允许出现任何 .jscfg
foreach ($file in @(Get-ChildItem -LiteralPath $contentRoot -File -Filter '*.jscfg')) {
    $problems += "content\$($file.Name) must move into content\config (engine configs are grouped there)"
}

# 2. 搬迁目标必须存在，旧名不得留下
foreach ($old in $moved.Keys) {
    if (-not (Test-Path -LiteralPath (Join-Path $contentRoot $moved[$old]) -PathType Leaf)) {
        $problems += "content\$old was not moved to content\$($moved[$old])"
    }
}
foreach ($old in $renamedInPlace.Keys) {
    if (Test-Path -LiteralPath (Join-Path $configRoot $old) -PathType Leaf) {
        $problems += "content\config\$old must be renamed to $($renamedInPlace[$old])"
    }
    if (-not (Test-Path -LiteralPath (Join-Path $configRoot $renamedInPlace[$old]) -PathType Leaf)) {
        $problems += "content\config\$($renamedInPlace[$old]) is missing"
    }
}

# 3. content\config 下的主配置必须遵守 engineconfig_<平台>_<功能>.jscfg
$namingPattern = '^engineconfig_[a-z0-9]+_[a-z0-9]+\.jscfg$'
foreach ($file in @(Get-ChildItem -LiteralPath $configRoot -File -Filter 'engineconfig*.jscfg')) {
    if ($file.Name -cnotmatch $namingPattern) {
        $problems += "content\config\$($file.Name) must follow engineconfig_<platform>_<feature>.jscfg"
    }
}

# 4. 旧路径不得再被引用（扫描源码/工程/脚本/文档，跳过产物目录与本测试自身）
$scanRoots = @(
    'CSharpCode', 'Module', 'Tests', 'BuildScript', 'Documents', 'Core.Android', 'Core.Mac',
    'Core.Window', 'Core.iOS', 'NativeCode', 'Plugins', '.vscode', '.github'
)
$scanExtensions = @(
    '*.cs', '*.csproj', '*.props', '*.targets', '*.ps1', '*.bat', '*.sln', '*.json', '*.md',
    '*.yml', '*.cmake', '*.sh', '*.jscfg', '*.txt', '*.vcxproj', '*.shproj', '*.projitems'
)
$excludePath = '\\(bin|obj|binaries|intermediate|cache|codegen|3rd|TestResults|\.git|\.qoder|\.superpowers|node_modules)\\'

$staleNames = @($moved.Keys) + @($renamedInPlace.Keys)
$stalePattern = ($staleNames | ForEach-Object { '(?<![A-Za-z0-9_])' + $_.Replace('.', '\.') }) -join '|'

$scanFiles = @()
foreach ($dir in $scanRoots) {
    $full = Join-Path $repoRoot $dir
    if (-not (Test-Path -LiteralPath $full -PathType Container)) { continue }
    foreach ($file in @(Get-ChildItem -LiteralPath $full -Recurse -File -Include $scanExtensions -ErrorAction SilentlyContinue)) {
        if ($file.FullName -match $excludePath) { continue }
        $scanFiles += $file
    }
}
foreach ($file in @(Get-ChildItem -LiteralPath $repoRoot -File -Include $scanExtensions -ErrorAction SilentlyContinue)) {
    $scanFiles += $file
}

foreach ($file in $scanFiles) {
    if ($file.FullName -ieq $PSCommandPath) { continue }
    foreach ($hit in @(Select-String -LiteralPath $file.FullName -Pattern $stalePattern -CaseSensitive:$false -ErrorAction SilentlyContinue)) {
        $relative = $file.FullName.Substring($repoRoot.Length + 1)
        $problems += "${relative}:$($hit.LineNumber) still references '$($hit.Matches[0].Value)' (stale engine config path)"
    }
}

# 5. 未显式传 config 时的默认回退路径也必须在 content\config 下
$engineCs = Join-Path $repoRoot 'CSharpCode\Base\Engine.cs'
$engineText = Get-Content -Raw -LiteralPath $engineCs
if ($engineText -notmatch 'config[/\\]engineconfig_windows_dx11\.jscfg') {
    $problems += 'CSharpCode\Base\Engine.cs: default engine config fallback must point at config/engineconfig_windows_dx11.jscfg'
}

if ($problems.Count -gt 0) {
    throw ("Engine config layout problems:`n - " + ($problems -join "`n - "))
}

Write-Host '[EngineConfigLayout] PASS: engine configs live in content\config with engineconfig_<platform>_<feature> naming and no stale references remain'
