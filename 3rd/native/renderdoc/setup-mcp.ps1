#requires -Version 5.1
<#
.SYNOPSIS
搭建 Windows x64 RenderDoc MCP 独立环境。
.DESCRIPTION
优先复用 x64 Python 3.13；缺少时通过 winget 为当前用户安装。
环境位于仓库 cache\renderdoc-mcp-venv，不安装或替换系统 RenderDoc。
固定 renderdoc-mcp 0.2.7 与 mcp 1.30.0，验证 pyd 加载及 MCP 握手。
默认不修改 .mcp.json；使用 -ConfigureMcp 才合并项目配置并备份原文件。
无需激活虚拟环境，也不依赖 RENDERDOC_MODULE_PATH 或当前工作目录。
.PARAMETER PythonPath
指定已有的 x64 Python 3.13 python.exe；错误版本会直接报错。
.PARAMETER SkipPythonInstall
禁止自动安装 Python；适合预装 Python 或限制联网安装的机器。
.PARAMETER CheckOnly
只检查 Python 及输出目标路径，不安装、不创建环境、不修改配置。
.PARAMETER ConfigureMcp
环境验证通过后，更新仓库 .mcp.json 的 renderdoc 项，保留其他服务。
.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1 -ConfigureMcp
.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1 -PythonPath 'C:\Tools\Python313\python.exe' -SkipPythonInstall
.NOTES
-ExecutionPolicy Bypass 只作用于本次启动的进程，不修改系统持久执行策略。
#>
[CmdletBinding()]
param(
    [string]$PythonPath = '',
    [switch]$SkipPythonInstall,
    [switch]$CheckOnly,
    [switch]$ConfigureMcp
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# 不使用非核心 .NET 方法，兼容 Windows PowerShell 5.1 ConstrainedLanguage。
$pythonProbe = @'
import sys, sysconfig
if sys.version_info[:2] != (3, 13) or sysconfig.get_platform() != 'win-amd64':
    raise SystemExit(1)
print(sys.executable)
'@

function Find-CompatiblePython {
    param([string]$ExplicitPath = '')

    if ($ExplicitPath) {
        $candidates = @(@{ Path = $ExplicitPath; Arguments = @() })
    }
    else {
        $candidates = @()
        foreach ($launcher in @(Get-Command py.exe -All -CommandType Application -ErrorAction SilentlyContinue)) {
            $candidates += @{ Path = $launcher.Source; Arguments = @('-3.13') }
        }
        # 注册表支持自定义安装目录，不假定所有机器都采用默认目录。
        foreach ($key in @(
            'HKCU:\Software\Python\PythonCore\3.13\InstallPath',
            'HKCU:\Software\Python\PythonCore\3.13-64\InstallPath',
            'HKLM:\Software\Python\PythonCore\3.13\InstallPath',
            'HKLM:\Software\Python\PythonCore\3.13-64\InstallPath'
        )) {
            if (Test-Path -LiteralPath $key) {
                $exe = Get-ItemPropertyValue -LiteralPath $key -Name ExecutablePath -ErrorAction SilentlyContinue
                if (-not $exe) {
                    $directory = Get-ItemPropertyValue -LiteralPath $key -Name '(default)' -ErrorAction SilentlyContinue
                    if ($directory) {
                        $exe = Join-Path $directory 'python.exe'
                    }
                }
                if ($exe) {
                    $candidates += @{ Path = $exe; Arguments = @() }
                }
            }
        }
        foreach ($directory in @($env:LOCALAPPDATA, $env:ProgramFiles)) {
            if ($directory) {
                $relative = if ($directory -eq $env:LOCALAPPDATA) { 'Programs\Python\Python313\python.exe' } else { 'Python313\python.exe' }
                $candidates += @{ Path = (Join-Path $directory $relative); Arguments = @() }
            }
        }
        # 跳过 Windows Store 占位程序后，继续检查 PATH 中其他 Python。
        foreach ($command in @(Get-Command python.exe -All -CommandType Application -ErrorAction SilentlyContinue)) {
            if ($command.Source -notmatch '\\WindowsApps\\') {
                $candidates += @{ Path = $command.Source; Arguments = @() }
            }
        }
    }

    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate.Path -PathType Leaf)) {
            continue
        }
        try {
            $arguments = @($candidate.Arguments) + @('-I', '-c', $pythonProbe)
            # WinPS 5.1 的 stderr 重定向会生成错误记录，警告不能成为误装 Python 的理由。
            $savedPreference = $ErrorActionPreference
            $savedAutomaticInstall = $env:PYTHON_MANAGER_AUTOMATIC_INSTALL
            try {
                $ErrorActionPreference = 'Continue'
                # 新版 py 在没有运行时时会自动安装，探测阶段必须禁止此行为。
                $env:PYTHON_MANAGER_AUTOMATIC_INSTALL = 'false'
                $output = @(& $candidate.Path @arguments 2>$null)
                $code = $LASTEXITCODE
            }
            finally {
                $ErrorActionPreference = $savedPreference
                $env:PYTHON_MANAGER_AUTOMATIC_INSTALL = $savedAutomaticInstall
            }
            if ($code -eq 0 -and $output.Count -gt 0) {
                $resolved = [string]($output | Select-Object -Last 1)
                if (Test-Path -LiteralPath $resolved -PathType Leaf) {
                    return (Resolve-Path -LiteralPath $resolved).ProviderPath
                }
            }
        }
        catch {
            Write-Verbose "忽略不可用的 Python：$($candidate.Path)"
        }
    }
    return ''
}

function Invoke-SetupCommand {
    param([string]$FilePath, [string[]]$Arguments)

    & $FilePath @Arguments
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        throw "外部命令失败，退出码 $code：$FilePath"
    }
}

$verifyEnvironment = @'
import asyncio, importlib.metadata as metadata, pathlib, sys, sysconfig
if sys.version_info[:2] != (3, 13) or sysconfig.get_platform() != 'win-amd64':
    raise RuntimeError('Expected Windows x64 Python 3.13')
if sys.prefix == sys.base_prefix:
    raise RuntimeError('Expected an isolated virtual environment')
for name, expected in [('renderdoc-mcp', '0.2.7'), ('mcp', '1.30.0')]:
    distribution = metadata.distribution(name)
    actual = distribution.version
    if actual != expected:
        raise RuntimeError(f'{name}: expected {expected}, got {actual}')
    if not pathlib.Path(distribution.locate_file('')).resolve().is_relative_to(pathlib.Path(sys.prefix).resolve()):
        raise RuntimeError(f'{name} must be installed inside this virtual environment')
    print(f'{name}: {actual}', flush=True)
import renderdoc_mcp, renderdoc
package_path = pathlib.Path(renderdoc_mcp.__file__).resolve()
if not package_path.is_relative_to(pathlib.Path(sys.prefix).resolve()):
    raise RuntimeError('renderdoc-mcp must be installed inside this virtual environment')
expected_module = package_path.parent / 'lib' / 'renderdoc.pyd'
if pathlib.Path(renderdoc.__file__).resolve() != expected_module.resolve():
    raise RuntimeError('renderdoc.pyd was not loaded from the bundled lib directory')
print(f'renderdoc.pyd: {renderdoc.__file__}', flush=True)
print(f'RenderDoc: {renderdoc.GetVersionString()}', flush=True)
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
async def verify_server():
    parameters = StdioServerParameters(command=sys.executable, args=['-I', '-m', 'renderdoc_mcp'])
    async with stdio_client(parameters) as (reader, writer):
        async with ClientSession(reader, writer) as session:
            await session.initialize()
            result = await session.list_tools()
            if not result.tools:
                raise RuntimeError('MCP server returned no tools')
            print(f'MCP initialize / tools/list: OK ({len(result.tools)} tools)', flush=True)
asyncio.run(asyncio.wait_for(verify_server(), timeout=30))
'@

$updateConfig = @'
import json, os, pathlib, sys, tempfile, time
path = pathlib.Path(sys.argv[1])
python = pathlib.Path(sys.argv[2]).resolve().as_posix()
def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f'Duplicate JSON key: {key}')
        result[key] = value
    return result
original = path.read_bytes() if path.exists() else None
data = json.loads(original.decode('utf-8-sig'), object_pairs_hook=unique_object) if original is not None else {}
if not isinstance(data, dict):
    raise ValueError('MCP configuration root must be an object')
servers = data.setdefault('mcpServers', {})
if not isinstance(servers, dict):
    raise ValueError('mcpServers must be an object')
server = servers.setdefault('renderdoc', {})
if not isinstance(server, dict):
    raise ValueError('renderdoc server must be an object')
if 'url' in server or server.get('type', 'stdio') != 'stdio':
    raise ValueError('Existing renderdoc server is not a local stdio server; configuration left unchanged')
before = json.dumps(data, ensure_ascii=False)
server['command'] = python
server['args'] = ['-I', '-m', 'renderdoc_mcp']
if json.dumps(data, ensure_ascii=False) == before and original is not None:
    print(f'MCP configuration already up to date: {path}', flush=True)
    raise SystemExit(0)
replacement = (json.dumps(data, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
temporary = None
try:
    with tempfile.NamedTemporaryFile(dir=path.parent, prefix='.mcp.', suffix='.tmp', delete=False) as stream:
        temporary = pathlib.Path(stream.name)
        stream.write(replacement)
    current = path.read_bytes() if path.exists() else None
    if current != original:
        raise RuntimeError('MCP configuration changed concurrently; refusing to overwrite it')
    if original is not None:
        backup_dir = path.parent / 'cache' / 'renderdoc-mcp-backups'
        backup_dir.mkdir(parents=True, exist_ok=True)
        backup = backup_dir / f'mcp.{time.time_ns()}.json'
        backup.write_bytes(original)
        print(f'MCP configuration backup: {backup}', flush=True)
    current = path.read_bytes() if path.exists() else None
    if current != original:
        raise RuntimeError('MCP configuration changed during backup; refusing to overwrite it')
    os.replace(temporary, path)
    print(f'MCP configuration updated: {path}', flush=True)
finally:
    if temporary is not None and temporary.exists():
        temporary.unlink()
'@

try {
    if ($env:OS -ne 'Windows_NT') {
        throw '本脚本仅支持 Windows x64 Python 环境。'
    }
    $root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).ProviderPath
    $environmentDirectory = Join-Path $root 'cache\renderdoc-mcp-venv'
    $environmentPython = Join-Path $environmentDirectory 'Scripts\python.exe'
    $configPath = Join-Path $root '.mcp.json'
    $python = Find-CompatiblePython -ExplicitPath $PythonPath
    if (-not $python) {
        if ($PythonPath) {
            throw "指定程序不是可用的 x64 Python 3.13：$PythonPath"
        }
        if ($SkipPythonInstall -or $CheckOnly) {
            throw '未找到 x64 Python 3.13。请安装后重试，或使用 -PythonPath 指定其 python.exe。'
        }
        $winget = Get-Command winget.exe -CommandType Application -ErrorAction SilentlyContinue
        $wingetPath = if ($null -ne $winget) { $winget.Source } else { '' }
        if (-not $wingetPath -and $env:LOCALAPPDATA) {
            $alias = Join-Path $env:LOCALAPPDATA 'Microsoft\WindowsApps\winget.exe'
            if (Test-Path -LiteralPath $alias -PathType Leaf) {
                $wingetPath = $alias
            }
        }
        if (-not $wingetPath) {
            throw '缺少 Python 3.13 和 winget。请安装 Windows 应用安装程序，或从 python.org 安装 x64 Python 3.13 后重试。'
        }
        Write-Host '未找到 Python 3.13，使用 winget 为当前用户安装 x64 版本（需要联网）。'
        Invoke-SetupCommand $wingetPath @(
            'install', '--id', 'Python.Python.3.13', '--exact', '--source', 'winget',
            '--architecture', 'x64', '--scope', 'user', '--silent',
            '--accept-package-agreements', '--accept-source-agreements', '--disable-interactivity'
        )
        # 安装后重新查注册表，不依赖当前进程尚未刷新的 PATH。
        $python = Find-CompatiblePython
        if (-not $python) {
            throw 'Python 安装命令已结束，但未找到 x64 Python 3.13。请用 -PythonPath 指定实际安装路径后重试。'
        }
    }

    Write-Host "仓库：$root"
    Write-Host "基础 Python：$python"
    Write-Host "独立环境：$environmentDirectory"
    if ($CheckOnly) {
        Write-Host '预检查通过；未安装依赖、未创建环境、未修改 MCP 配置。'
        exit 0
    }

    if (Test-Path -LiteralPath $environmentDirectory) {
        if (-not (Test-Path -LiteralPath (Join-Path $environmentDirectory 'pyvenv.cfg') -PathType Leaf) -or
            -not (Test-Path -LiteralPath $environmentPython -PathType Leaf)) {
            throw "已有目录不是完整虚拟环境，停止以免覆盖其内容：$environmentDirectory"
        }
        $compatible = Find-CompatiblePython -ExplicitPath $environmentPython
        if (-not $compatible) {
            throw "已有虚拟环境不可用或版本不符，请人工检查后重试：$environmentDirectory"
        }
        Write-Host '复用已有虚拟环境。'
    }
    else {
        New-Item -ItemType Directory -Path (Join-Path $root 'cache') -Force | Out-Null
        Write-Host '创建独立虚拟环境。'
        Invoke-SetupCommand $python @('-I', '-m', 'venv', $environmentDirectory)
    }

    Write-Host '安装固定依赖：renderdoc-mcp 0.2.7、mcp 1.30.0（首次运行需要联网）。'
    Invoke-SetupCommand $environmentPython @(
        '-I', '-m', 'pip', 'install', '--disable-pip-version-check', '--no-input',
        'renderdoc-mcp==0.2.7', 'mcp[cli]==1.30.0'
    )
    Invoke-SetupCommand $environmentPython @('-I', '-m', 'pip', 'check')
    Write-Host '验证虚拟环境、包内 pyd 及 MCP initialize / tools/list（30 秒超时）。'
    Invoke-SetupCommand $environmentPython @('-I', '-c', $verifyEnvironment)

    if ($ConfigureMcp) {
        Invoke-SetupCommand $environmentPython @('-I', '-c', $updateConfig, $configPath, $environmentPython)
        Write-Host '请在 Qoder 的 MCP 设置中确认服务已加载；环境验证不代表 IDE 已刷新。'
    }
    else {
        Write-Host '环境已就绪，项目 .mcp.json 未修改。可再次运行并添加 -ConfigureMcp 自动配置。'
        $fragment = @{ renderdoc = @{ command = ($environmentPython -replace '\\', '/'); args = @('-I', '-m', 'renderdoc_mcp') } }
        Write-Host ($fragment | ConvertTo-Json -Depth 4)
    }
    Write-Host 'RenderDoc MCP 环境搭建完成；抓帧重放兼容性仍需用实际 .rdc 验证。'
    exit 0
}
catch {
    Write-Error "RenderDoc MCP 环境搭建失败：$($_.Exception.Message)" -ErrorAction Continue
    exit 1
}
