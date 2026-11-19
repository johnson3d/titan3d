# Cook 调度状态机纯逻辑测试 (假通道, 不起子进程)。
# 被测源码直接编进测试 exe; Engine.Window.dll 只提供协议类型, 取自 binaries\net10.0 (需先构建过一次引擎)。
# 不登记进 EngineAll.sln: 本工程没有 ProjectReference, 进 sln 后干净构建可能先于 Engine.Window 编译而失败。
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repo = Resolve-Path (Join-Path $here '..\..')
$bin = Join-Path $repo 'binaries\net10.0'
if ((Test-Path (Join-Path $bin 'Engine.Window.dll')) -eq $false) {
    Write-Output "missing $bin\Engine.Window.dll, build the engine first"
    exit 2
}

& dotnet build (Join-Path $here 'Titan.Tests.Cook.csproj') -nologo -v:q -p:BuildProjectReferences=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$out = Join-Path $here 'bin\Debug\net10.0'
# Engine.Window 的运行时依赖不随 Reference 复制, 缺了会在加载协议类型时 FileNotFoundException
Copy-Item (Join-Path $bin '*.dll') $out -Force
& (Join-Path $out 'Titan.Tests.Cook.exe')
exit $LASTEXITCODE
