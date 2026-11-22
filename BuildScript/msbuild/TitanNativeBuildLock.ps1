param(
    [Parameter(Mandatory = $true)][ValidateSet('Acquire', 'Release')][string]$Action,
    [Parameter(Mandatory = $true)][string]$Path,
    [int]$TimeoutSeconds = 1800
)

# native Core.Window 的嵌套构建（TitanBuildNativeCoreForVS）在 VS 里可能被多个 MSBuild 节点同时请求：
# 顶层 Engine.Window 与作为 MainEditor ProjectReference 的那次构建各触发一遍。两个节点并发构建同一个
# vcxproj 会互抢 intermediate\cpp\Core.Window\x64\<Config>\Core.Window.tlog\CL.*.tlog，
# 表现为 warning MSB6008（强制重新生成所有源）与 error MSB6003（CL.exe 无法运行），随后整份工程被迫全量重编。
#
# 本脚本用独占创建（FileMode.CreateNew 原子操作）的锁文件把这段嵌套构建串行化：
# 拿到锁的节点执行构建，其他节点等锁释放后走增量（几乎全部 up-to-date），不会再并发写 tlog。
# Release 只删除自己持有的锁；持有者进程已退出的僵尸锁由下一位等待者清理。

function Get-NodeProcessId {
    # Exec 在 Windows 上经 cmd.exe 启动本脚本，锁的持有者取祖父进程（cmd 的父进程 = MSBuild 节点）。
    $self = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId=$PID" -ErrorAction SilentlyContinue
    if (-not $self) { return 0 }
    $owner = [int]$self.ParentProcessId
    $parent = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId=$owner" -ErrorAction SilentlyContinue
    if ($parent -and $parent.Name -ieq 'cmd.exe') { $owner = [int]$parent.ParentProcessId }
    return $owner
}

$ownerId = Get-NodeProcessId

if ($Action -eq 'Release') {
    # Release 只会由已拿到锁的那次构建调用（成功或 OnError 路径），直接删除即可；
    # 不校验 owner：Acquire 与 Release 是两个独立 Exec（各自的 cmd 不同），但都属于同一个 MSBuild 节点，
    # 而取消构建时脚本可能被直接终止、根本走不到这里。
    Remove-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
    exit 0
}

$directory = Split-Path -Parent $Path
if ($directory -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
$announced = $false
while ($true) {
    try {
        $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
        try {
            $bytes = [System.Text.Encoding]::UTF8.GetBytes("$ownerId`n$([DateTime]::UtcNow.ToString('o'))`n")
            $stream.Write($bytes, 0, $bytes.Length)
        }
        finally { $stream.Dispose() }
        exit 0
    }
    catch [System.IO.IOException] {
        $holder = 0
        try {
            $lines = [System.IO.File]::ReadAllLines($Path)
            if ($lines.Length -ge 1) { [void][int]::TryParse($lines[0].Trim(), [ref]$holder) }
        }
        catch { }

        # 持有者进程已退出（构建被取消/强杀）就清掉僵尸锁重试；存活则继续等。
        if ($holder -le 0 -or -not (Get-Process -Id $holder -ErrorAction SilentlyContinue)) {
            Remove-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
            continue
        }
        if (-not $announced) {
            Write-Host "NativeBuildLock: another MSBuild node ($holder) is building Core.Window, waiting..."
            $announced = $true
        }
        if ((Get-Date) -gt $deadline) {
            # 超时说明前一位构建已被取消或卡死（锁文件不会自动消失）：接管锁继续构建，
            # 否则取消一次生成就会让后续所有构建都卡在等待里。
            Write-Warning "NativeBuildLock: gave up waiting after $TimeoutSeconds s (holder node $holder), taking over '$Path'"
            Remove-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
            $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
            continue
        }
        Start-Sleep -Seconds 2
    }
}
