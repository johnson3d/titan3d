# TitanCMD 命令行工具

`TitanCMD.exe` 是 TitanEngine 的离线命令行入口（工程位于 `Module/TitanCMD`，产物输出到 `binaries`），
用于在不开编辑器 GUI 的情况下执行资产批处理、烘焙、测试、服务进程启动等操作。

## 目录

- [通用参数](#通用参数)
- [命令生命周期](#命令生命周期)
- [命令一览](#命令一览)
- [命令详情](#命令详情)
  - [SaveAsLastest](#saveaslastest)
  - [Cook](#cook)
  - [RunTests](#runtests)
  - [MakeSln](#makesln)
  - [LoadPlugin / StartRobot / StartRootServer / StartLoginServer / StartLevelServer / StartGateServer](#loadplugin--startrobot--startrootserver--startloginserver--startlevelserver--startgateserver)
  - [StartDS](#startds)
  - [macross-patch](#macross-patch)
- [扩展新命令](#扩展新命令)
- [注意事项](#注意事项)

## 通用参数

所有命令共用的命令行参数（`Key=Value` 形式，顺序无关）：

| 参数 | 必填 | 说明 |
|---|---|---|
| `CookCfg=` | 否 | 引擎配置文件（`.jscfg`）路径。为空时使用默认配置；路径写了但文件不存在会直接报错退出（返回 1） |
| `ExeCmd=` | 是 | 要执行的命令名，见[命令一览](#命令一览)。未知命令打印 `Unknown ExeCmd` 并返回 2 |
| `ExtraCmd=` | 否 | 指定一个数字 N，程序启动时会从标准输入再读取 N 行，逐行追加到参数列表中（用于命令行长度受限或脚本拼接场景） |
| `NativeDLL=` | 否 | native 库配置：`debug` 或 `release`（默认 `release`）。若未在命令行指定，会读取 `cache/config/engine.jscfg` 中缓存的 `NativeDll` 值 |

典型调用（工作目录必须为 `binaries` 或 `binaries/net10.0`，程序以当前目录推导引擎根路径）：

```bat
TitanCMD.exe CookCfg=..\..\content\config\engineconfig_windows_alonegame.jscfg ExeCmd=Cook AssetType=Scene
```

## 命令生命周期

命令是否常驻由 `TtCommandExecutionPolicy.IsServiceCommand` 决定（见 `UCookSettings.cs`）：

- **一次性命令（one-shot）**：执行完自动向引擎投递退出消息，进程结束。返回码 = 命令的返回值。
- **服务命令（service）**：执行完后进程保持运行（引擎主循环继续 Tick），直到外部终止。
  当前服务命令：`StartDS`、`LoadPlugin`、`StartRobot`、`StartRootServer`、`StartLoginServer`、
  `StartLevelServer`、`StartGateServer`、`CookServer`。

> `CookServer` 是个例外：它虽然属于 service 命令（为了让引擎主循环在等活期间继续 Tick，地形 bake
> 的续体链靠 `Tick` 推进），但它的存活期就是那个 job 循环本身。因此它**自己**在收摊前调
> `TtEngine.Instance.PostQuitMessage()` 退出（见 `Command\CookServer.cs`），否则光靠
> `IsServiceCommand` 返回 true 会让进程永远空转。

## 命令一览

| 命令 | 类型 | 用途 |
|---|---|---|
| `SaveAsLastest` | one-shot | 按资产类型批量重新加载并保存资产（升级资产格式到最新） |
| `Cook` | one-shot | 按资产类型批量 cook，或 cook 单个指定资产（产出缓存侧派生数据） |
| `CookServer` | service | 常驻 cook worker，从 stdin 收 `[TITAN-COOK]` 指令逐 job 干活并向 stdout 回报进度（供编辑器 CookManager 派活） |
| `RunTests` | one-shot | 运行引擎单元测试并输出 JUnit 报告 |
| `MakeSln` | one-shot | 占位命令（直接返回 0） |
| `LoadPlugin` | service | 加载 `SourceGit` 插件 |
| `StartRobot` | service | 加载 `ClientRobot` 插件 |
| `StartRootServer` | service | 加载 `RootServer` 插件 |
| `StartLoginServer` | service | 加载 `LoginServer` 插件 |
| `StartLevelServer` | service | 加载 `LevelServer` 插件 |
| `StartGateServer` | service | 加载 `GateServer` 插件 |
| `StartDS` | service | 启动 Dedicated Server（仅 `PWindow` 编译配置，Windows 桌面） |
| `macross-patch` | one-shot | 编译/校验/打包 Macross 热更补丁 |

## 命令详情

### SaveAsLastest

按资产类型批量加载资产并 `SaveAssetTo` 回写源文件，用于把存量资产升级到当前引擎格式。

```
ExeCmd=SaveAsLastest [AssetType=Scene+Mesh+Material ...] [Blacklist=<黑名单文件>]
```

| 参数 | 说明 |
|---|---|
| `AssetType=` | 资产类型列表，`+` 分隔（如 `Scene+Mesh`）。缺省时执行全部已注册类型 |
| `Blacklist=` | 黑名单文件路径（每行一个相对路径，`#` 开头为注释）。未指定时尝试读 DynConfig 的 `Blacklist` 配置 |

已专门实现的类型：`Texture`、`UVAnim`、`Mesh`、`MeshPrimitive`、`Material`、`MaterialInst`、
`Scene`、`Prefab`、`AnimClip`、`UI`、`Macross`；其余类型走 `ProcAssets` 通用逻辑
（对每个 `.meta` 文件 `GetAsset` 后 `SaveAssetTo`）。

> 注意：`AssetType=` 的值可能被 `cache/config/engine.jscfg` 中的 `CookAssetType` 覆盖
> （启动时若发现参数列表里有 `AssetType=` 项，会整体替换为配置值）。

### Cook

按资产类型批量 cook，或 cook 单个指定资产。与 `SaveAsLastest` 的本质区别：**cook 不回写资产源文件**，
只产出缓存侧派生数据（如场景内地形节点的 `.trlvl` 缓存与 VGDag 烘焙产物 `.trvgdag`）。

```
:: 批量：按类型 cook
ExeCmd=Cook AssetType=Scene[+Texture...] [Blacklist=<黑名单文件>]

:: 单个：指定资产相对路径（优先于 AssetType，可带扩展名，自动在 Game/Engine root 下定位）
ExeCmd=Cook Asset=<相对路径>
```

| 参数 | 说明 |
|---|---|
| `AssetType=` | 资产类型列表，`+` 分隔。必须显式指定（缺省报错） |
| `Asset=` | 单资产标识，形式见下方“资产标识写法”。给了此参数则跳过类型扫描，只 cook 该资产 |
| `Blacklist=` | 同 `SaveAsLastest` |

执行链路（类型无关的统一两段式）：

```
遍历资产（按类型批量扫 Game/Engine root，或按 Asset= 定位单个）
  → AssetMetaManager.GetAssetMeta(rn)     // 取该资产的 meta
  → await ameta.CreateAsset(world)        // 引擎物化（加载/实例化）
  → await ameta.CookAsset(asset, ctx)     // 引擎内实现具体 cook 逻辑（ctx = IO.TtCookContext）
```

各资产类型的 cook 内容全部实现于引擎侧对应 `TtXxxAMeta.CookAsset` 重写，命令层不含任何类型专有流程；
新增类型只需在引擎侧实现 `CookAsset`，本命令零改动。当前已实现：

- **Scene**（`TtSceneAMeta.CookAsset`）：取物化好的场景 → `IterateNodes` 收集全树节点快照 →
  逐节点调用 `TtNode.CookAsset()` 虚方法。默认实现直接返回 true；
  `TtTerrainNode` 重写后执行完整地形 cook：
  - PGC 程序化节点图 → 高度场 → `.trlvl` level 缓存
  - 每个 level 的 cluster DAG QEM 烘焙 → `.trvgdag` 落盘（缓存 hash 命中则跳过，重复 cook 幂等）
  - 等待期间每 10 秒输出一条进度心跳日志，避免无输出难以判断是否存活

返回码：0 全部成功；1 有失败（未知类型、`GetAssetMeta`/`CreateAsset`/`CookAsset` 失败）。

资产标识写法（`Asset=` 与 `CookServer` 的 `add.asset` 同一个形式）：相对 Game/Engine root、
**剥掉 `.ameta` 后剩下的那串**，例如 `utest/emptyscene.scene`、`tutorials/terrain/terrain_scene.scene`。
这不是随手选的：`AssetMetaManager` 的资产表由扫 `*.ameta` 建立，键就是该形式（见
`IAssetMeta.LoadAMeta`），批量分支也是用同一算法得到资产路径的，所以两条路必须认同一份身份。
判据也不能退化成“主文件是否存在”—— `Scene` 这类资产的名字其实是个**目录**
（目录里放 `nodes/`、`class_graph.dat` 与同名主文件），`FileExists` 走 `File.Exists`，对目录恒为 `false`。
驱动侧会额外兼容“主文件写法”（`utest/x.scene/x.scene` → 归一到父目录），但从资源管理器直接复制
得到的往往是这种冗长形式，提交侧应统一发规范形式。

实现主体在引擎侧 `CSharpCode\Base\IO\CookDriver.cs`（`TtCookDriver`），本命令只是参数解析的薄壳；
`CookServer` 与编辑器侧 CookManager 走的都是同一个 Driver，行为同源。

### CookServer

常驻 cook worker。启动后不退出，从 **stdin 逐行**收 `[TITAN-COOK]{json}` 指令，按 job 串行调
`TtCookDriver`，结果与进度以协议行写回 **stdout**。设计见 `Documents\editor\CookManager.md`。

```
TitanCMD.exe CookCfg=..\..\content\config\<cook 配置>.jscfg ExeCmd=CookServer [WorkerIdleSec=300 ...]
```

| 参数 | 默认 | 说明 |
|---|---|---|
| `WorkerIdleSec=` | 300 | 空闲（队列空且无在飞 job）多久后优雅退出；`<=0` 禁用 |
| `WorkerMaxJobs=` | 200 | 本进程跑多少个 job 后在批次边界主动退出，由提交侧重新拉起；`<=0` 不限 |
| `WorkerJobTimeout=` | 1200 | 单 job 超时秒数（与地形 bake 的 `bakeTimeoutMs=1200000` 对齐）；`<=0` 不限 |
| `WorkerBeatSec=` | 10 | 心跳间隔秒数 |
| `CookCfg=` | — | 只用于算 `hello` 里的 `cfgHash`（配置内容 RIPEMD160），供提交侧拒绝不同配置共写 cache |

输入指令：`add`（`id`/`asset`/`type`/`force`）、`cancel`（`id`）、`flush`、`quit`（`reason`）。
输出消息：`hello`、`ready`、`idle`、`beat`、`job.begin`、`job.stage`、`job.progress`、`job.done`、
`job.error`、`stats`、`busy`、`quit`。完整字段表在 `CSharpCode\Base\IO\CookProtocol.cs`。

行为要点：

- **非协议行不算错误**：stdin 上不带哨兵的行会被当日志原文转写到 stderr；stdout 上引擎的人读日志与
  协议行混排，由提交侧按 `[TITAN-COOK]` 前缀分流。
- **退出时机分两档**：收到 `quit` 指令 = 把已接收的队列跑完再退（本意是"不再接新活"）；
  stdin 被关闭（提交侧进程没了）或达到 `WorkerMaxJobs=` = 在飞 job 跑完后立即退，
  **队列里未开工的 job 作废**。两种情形下提交侧都需按"没收到终态行的 job 重投"处理。
- **同一资产已在队/在飞则合并**，不重复跑；队列超 `512` 条则对该 `add` 回 `busy`。
- **取消是协作式的**：`cancel` 只置 `TtCookContext.RequestCancel()`，不使用 `CancelTask()`；
  超时后先置取消位，再给 `60s` 收尾宽限期，仍不收尾则判定引擎态不可信，以退出码 `1` 退场。
- **黑名单不参与**：黑名单是入队侧策略，worker 只接具体资产路径。

返回码：0 = 优雅退出（`idle` / `restart` / `manual` / `pipe`），1 = 致命错误（取消后 job 仍不收尾）。

手工冒烟（PowerShell，无需编辑器）。**工作目录必须是 `binaries`**：命令靠
`Directory.GetCurrentDirectory()` 拼 `SetDllDirectoryA(cwd/<NativeDLL>)` 定位 native 集，
在 `binaries\net10.0` 下直接跑会得到 `DllNotFoundException: Unable to load DLL 'Core.Window.dll'`：

```powershell
cd d:\GitHub\titanengine\binaries
$c = '[TITAN-COOK]'
$a = ($c + '{"v":1,"t":"add","id":1,"asset":"utest/emptyscene.scene","type":"Scene"}'),
     ($c + '{"v":1,"t":"add","id":2,"asset":"utest/nope/nope.scene","type":"Scene"}'),
     ($c + '{"v":1,"t":"flush"}'),
     ($c + '{"v":1,"t":"quit"}')
$a | .\net10.0\TitanCMD.exe "CookCfg=..\content\config\engineconfig_windows_cook.jscfg" `
    ExeCmd=CookServer NativeDLL=debug
```

`NativeDLL=` 选 `binaries\debug` 或 `binaries\release` 下的 native 集，应与当前托管产物同步选。
协议行混在引擎人读日志里，用前缀筛：`(Select-String -Path out.txt -Pattern "TITAN-COOK").Line`。

实测输出（2026-11-09，job 1 因当时驱动定位判据错而失败，序列本身完整）：

```
hello  proto=1 cfgHash=58_3E_… root=d:/github/titanengine/content/ types=[… "Scene" …] pid=70756
ready  queued=0
job.begin id=1 …
job.error id=1 kind=Asset elapsed=0.0059 …
ready  queued=1
job.begin id=2 asset=utest/nope/nope.scene
job.error id=2 kind=Asset message="asset not found in Game/Engine root: …" elapsed=0.0002
idle   queued=0
stats  done=0 failed=2 skipped=0 elapsed=0.0078
quit   reason=manual
```

期望：一个 `hello`（引擎就绪才发，紧随一行 `ready` 表示可接活），每 job 一行 `job.begin` 加若干
`job.stage`，最后得到 `job.done` / `job.error`（不存在的资产应为 `kind=Asset`）、一行 `stats`、一行 `quit`，
退出码 0。

### RunTests

运行引擎单元测试（`EngineNS.UnitTest.TtTestRunner`），输出 JUnit XML 报告。

```
ExeCmd=RunTests [Filter=<过滤串>] [ResultsDir=<输出目录>]
```

| 参数 | 说明 |
|---|---|
| `Filter=` | 测试过滤器（缺省空串，跑全部） |
| `ResultsDir=` | 报告输出目录（缺省 `TestResults`），报告为 `engine-tests.xml` |

返回码：0 无失败；1 存在失败用例。

### MakeSln

占位命令，直接返回 0，不做任何操作。

### LoadPlugin / StartRobot / StartRootServer / StartLoginServer / StartLevelServer / StartGateServer

服务型命令：加载对应插件后进程常驻（引擎主循环继续运行），用于启动网络服务端/机器人等长驻进程。

```
ExeCmd=LoadPlugin          :: 加载 SourceGit 插件
ExeCmd=StartRobot          :: 加载 ClientRobot 插件
ExeCmd=StartRootServer     :: 加载 RootServer 插件
ExeCmd=StartLoginServer    :: 加载 LoginServer 插件
ExeCmd=StartLevelServer    :: 加载 LevelServer 插件
ExeCmd=StartGateServer     :: 加载 GateServer 插件
```

### StartDS

启动 Dedicated Server（专用服务器），仅 Windows 桌面（`PWindow` 编译配置）可用；
非 Windows 平台执行会打印不支持提示并返回 -1。

```
ExeCmd=StartDS DS_Port=<端口>
```

| 参数 | 说明 |
|---|---|
| `DS_Port=` | 监听端口（uint16） |

服务型命令：启动成功后进程常驻。

### macross-patch

编译/校验/打包 Macross 热更补丁（`TtMacrossPatchBuilder`）。

```
ExeCmd=macross-patch PatchProject=<目录> BaselinePath=<基线文件> OutputDir=<输出目录>
```

| 参数 | 必填 | 说明 |
|---|---|---|
| `PatchProject=` | 是 | 补丁工程目录 |
| `BaselinePath=` | 是 | 基线（老版本）数据文件 |
| `OutputDir=` | 是 | 补丁输出目录 |

缺失参数时打印用法并返回 1；构建诊断信息逐行输出到控制台。返回码：0 成功；1 失败。

## 扩展新命令

1. 在 `Module/TitanCMD/Command/` 下新建处理器类，继承 `TtCookCommand`（或直接实现
   `async TtTask<int> ExecuteCommand(string[] args)`），文件名不加 `Tt`/`U` 等前缀，类名统一 `Tt` 前缀。
2. 在 `Module/TitanCMD/Program.cs` 的 `ExecuteCommandAsync` switch 中补一个 `case "命令名":`，
   否则参数再正确也无人处理（未知命令返回 2）。
3. 若命令需要进程常驻，在 `TtCommandExecutionPolicy.IsServiceCommand` 中登记；否则默认 one-shot。
4. 参数解析复用基类工具：`FindArgument(args, "Key=")`（取单值）、`GetArguments(args, "Key=")`（取 `+` 分隔列表）、
   `Param_Types`/`Param_Blacklist`/`Param_Asset` 常量。
5. 本文件（`Documents/editor/TitanCMD.md`）补一行命令说明。

> `TitanCMD.csproj` 是 SDK-style 工程且未关闭默认文件收集，新增 `.cs` 文件自动参与编译，无需手工登记。

## 注意事项

- **工作目录**：程序以 `Directory.GetCurrentDirectory()` 推导引擎根与 `cache/config` 路径，
  通常在 `binaries`（或 `binaries/net10.0`）目录下执行；若在多级目录中，`CookCfg=` 请给相对该目录的正确路径。
- **引擎初始化完整**：TitanCMD 会完整启动引擎（含 GfxDevice），资产 cook 过程中材质/RVT 等 GPU 资源
  会照常创建 —— 保真但比纯 CPU 处理慢。
- **`AssetType=` 可能被配置覆盖**：见上文 `engine.jscfg` 的 `CookAssetType`。
- **不要用 `dotnet build` 直接编译引擎工程**：analyzer/source generator 加载引擎程序集会报
  Fatal error，构建请走仓库既定的构建脚本/VS。
