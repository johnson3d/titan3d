# CookManager 设计（编辑器批次化异步 cook）

## 目录

- [背景与目标](#背景与目标)
- [决策记录](#决策记录)
- [总体架构](#总体架构)
- [worker 生命周期模型](#worker-生命周期模型)
- [进程间通讯协议](#进程间通讯协议)
- [任务模型](#任务模型)
- [共享 cache 目录下的一致性规则](#共享-cache-目录下的一致性规则)
- [进度与编辑器 UI](#进度与编辑器-ui)
- [线程与数据流](#线程与数据流)
- [与既有链路的关系](#与既有链路的关系)
- [代码落点](#代码落点)
- [里程碑与验收](#里程碑与验收)
- [测试策略](#测试策略)
- [已知风险与未决项](#已知风险与未决项)

## 背景与目标

资产 cook 现在有两个入口：编辑器内直接调 `IAssetMeta.CookAsset`（阻塞主编辑器，cook 出错会拖垮编辑器），以及 `TitanCMD ExeCmd=Cook`（一次性批量，无进度反馈、无法与编辑器工作流衔接，见 [TitanCMD.md](TitanCMD.md)）。

随着资产量增长，目标是：**编辑器里所有 cook 需求统一提交给 CookManager，由它排队成批次，派发给 TitanCMD 子进程执行，通过进程间通讯回收任务状态，编辑器侧只显示进度条/错误列表**。

| 目标 | 落实手段 |
|---|---|
| cook 崩溃/卡死不影响主编辑器 | cook 全部在 TitanCMD 子进程内执行，编辑器只排队与收结果 |
| 批次化、可看进度、可取消、可重试 | Job 级粒度 + 行式 JSON IPC 协议 + 常驻 worker 池 |
| 以后其他电脑能参与 cook | 传输层抽象 `ITtCookTransport`；本机=子进程 stdin/stdout，远端=TCP，消息 schema 完全一致 |

必须保持的既有红线：

- `IAssetMeta.CookAsset(IAsset asset, params object[] args)` 只消费驱动传入的实例，结束时 `DisposeWithChildren`，**禁止 `SaveAssetTo` 回写源文件**（cook 只产缓存侧派生数据）。
- TitanCMD 命令层不含任何资产类型专有逻辑；新增资产类型只改引擎侧 `TtXxxAMeta`。
- 取消"后台在飞的重活"只能用协作式 token 轮询，**不得**对长任务调 `CancelTask()`（`Cancelling` 即 `IsCompleted`，会让池化的 `TtTaskData` 提前回池后被后台写污染）。
- 命名规范：类名带 `Tt` 前缀，文件名不带任何前缀。

## 决策记录

| 决策 | 选择 | 理由 |
|---|---|---|
| 通用 cook 驱动归属 | **下沉到引擎 `CSharpCode\Base\IO\`**，TitanCMD 变薄壳 | 编辑器需要同一套扫描/黑名单逻辑生成 job 列表，远端 worker 需要同一套执行体；避免两处重复实现漂移 |
| worker 生命周期 | **常驻 worker 池**（空闲优雅退出 + 批次边界重启） | 引擎启动很贵（完整 `InitEngine` + Tick 循环），每批起进程把时间全花在启动上；常驻才支持批次内动态追加任务与细粒度取消 |
| 多机产物存储 | **共享 cache 目录** | 只需解决原子写与租约锁，产物无需回传；远端机器可完全独立读写同一份缓存 |
| 单 worker 内并发度 | **1（严格串行）** | 一个 worker 内并行跑两个 job 会纠缠引擎全局态（RVT/材质/TPools）；并行度靠多进程获得 |
| worker 的 `PlayMode` | **保持 `EPlayMode.Editor`** | 今天 TitanCMD 就是这个行为。切 `EPlayMode.Cook` 会改变 `Texture.cs:3737`、`TerrainEdit.IsTerrainEditSupported` 等判定的分支，属独立审计项，不进本设计 |
| IPC 载体（v1） | **子进程 stdin/stdout 行协议** | 零新依赖、跨平台、天然只属于该子进程、进程退出即断链、无端口冲突；与仓库既有 AndroidBuild 进程会话范式一致 |

## 总体架构

```
编辑器进程 (MainEditor)                              worker 进程 (TitanCMD)
┌────────────────────────────────┐                 ┌──────────────────────────────────┐
│ TtCookManagerPanel (IRootForm) │                 │ Program: case "CookServer"       │
│   只读状态 / 发指令，不驱动状态机 │                 │   └ Command.TtCookServer (service)│
│              ▲                 │                 │        └ TtCookDriver.RunJob()   │
│ TtCookManager (TtModule)       │  行式 JSON IPC  │             ▲ 与 ExeCmd=Cook 同源 │
│   ├ 队列 / 批次 / 去重 / 互斥     │ ◄──────────────►│        TtCookContext(进度/取消)   │
│   ├ TtCookWorker × N（进程宿主）  │                 │ 引擎 Tick 驱动异步 bake           │
│   └ 状态机 / 台账 / 日志汇聚      │                 │ RHI_VirtualDevice：无窗口无 GPU   │
└────────────────────────────────┘                 └──────────────────────────────────┘
```

### 各角色职责

- **`TtCookManager : TtModule<TtEngine>`** —— 唯一的真相源。持有 job 队列、批次台账、worker 池；在 `TickModule`（引擎主循环）里排空 IPC 收件队列并驱动状态机。挂法与 `TtEngine.Instance.EditorInstance` 相同：作为 `TtEngine` partial 上的 public property，被 `TtModuleHost.GatherModules()` 反射收集，自动获得 Initialize/Tick/Cleanup。
  - `Initialize` 中判断 `host.GfxDevice.SlateApplication is TtMainEditorApplication` 才启用，否则整体休眠。cook 配置 `RHIType=RHI_VirtualDevice` 时 `TtGfxDevice.Initialize` 提前返回、根本不创建 `SlateApplication`，因此 TitanCMD 里该模块存在但不拉起 worker。
- **`TtCookManagerPanel : IRootForm`** —— 纯 UI，`OnDraw` 只读 `TtCookManager` 的状态并发送指令，**不调用任何 Poll**。这一点是刻意区别于现有 `TtAndroidBuildPanel` 的：该面板把 `mSession.Poll()` 写在 `OnDraw` 里，而 `TtRootFormManager.DrawRootForms()` 对 `Visible == false` 的表单直接跳过 `OnDraw`，所以面板一关、构建状态机就停摆。cook 是长任务，不能有这个行为。
- **`TtCookWorker`** —— 一条 worker 会话（本机=一个进程，远端=一条连接）。抽象 `ITtCookTransport` + `ITtCookProcessLauncher`/`ITtCookProcessHost`，沿用 `Editor\AndroidBuild\` 里为可单测而做的接口分离，测试注入 fake。
- **`TtCookDriver`（引擎侧，新增）** —— 从 `Module\TitanCMD\Command\CookAsset.cs` 原样下沉的通用驱动：类型枚举（反射 `IAssetMeta` 子类）、Game/Engine 双 root 目录扫描、黑名单过滤、`Asset=` 单资产定位、`GetAssetMeta → CreateAsset(world) → CookAsset(asset)` 三段执行。TitanCMD 的 `Cook` 与 `CookServer` 都调它。
- **`TtCookContext`（引擎侧，新增）** —— 单次 job 的执行上下文，经 `CookAsset(asset, args)` 的 `args` 传入（该签名本就带 `params object[]`，向后兼容）。成员：`IsCancellationRequested`/`RequestCancel()`、`ReportStage(string)`、`ReportProgress(float)`、`ReportArtifact(string)`、`GetArtifacts()`（返回相对仓库根的产物路径快照）。`TtNode.CookAsset()` 无参数，故 Driver 在调用 meta 前把 context 放进 **进程级 static ambient**（`TtCookContext.Current` + `PushAmbient` 的 `using` 作用域），供节点级实现上报。
  > 实现期修正：原计划用 `AsyncLocal`，但引擎 `TtTask` 的自定义 builder（`Thread\Async\Task.cs` 的 `AwaitOnCompleted`）是直接把 `stateMachine.MoveNext` 交给 awaiter、**不捕获 ExecutionContext**，续体在完成方线程上跑，`AsyncLocal` 会跨 await 静默丢失。改用与 `TtPgcGraph.ExecutionContext` 同形态的进程级字段（`Volatile.Read/Write`），并在替换非空 ambient 时打 Warning —— 前提"同进程同时只有一个在飞 job"正是本设计"单 worker 内严格串行"的决策。默认 sink 只写 `Profiler.Log`（`TtCookGategory`）；worker 装入协议 sink 输出 JSON 行。

## worker 生命周期模型

| 配置项 | 命令行参数（worker 侧） | 默认 | 语义 |
|---|---|---|---|
| `WorkerCount` | —（编辑器侧设置） | 1 | 并行 worker 数；单 worker 内 job 严格串行 |
| `IdleShutdownSec` | `WorkerIdleSec=` | 300 | 空闲超时后优雅退出（队列空且无在飞 job 才退），下次提交再拉起 |
| `MaxJobsPerWorker` | `WorkerMaxJobs=` | 200 | 达到即在**批次边界**优雅重启，防长跑导致的内存增长与状态漂移 |
| `JobTimeoutSec` | `WorkerJobTimeout=` | 按类型，Scene 1200 | 与 `TtTerrainNode.CookTerrainAsync(bakeTimeoutMs: 1200000)` 的既有超时对齐；判活依据是心跳而非墙钟 |
| `BeatSec` | `WorkerBeatSec=` | 10 | 心跳间隔；空闲期 `beat` 的 `elapsed` 是开机相对时间，在飞时是 job 相对时间 |
| `CrashRestartLimit` | —（编辑器侧设置） | 2 | 进程异常退出后自动重启次数上限；超限则该 worker 标 `Disabled` 并在面板报错 |
| `JobRetryLimit` | —（编辑器侧设置） | 1 | 单 job 因 worker 崩溃失败时的重新入队次数 |

worker 侧接入方式已核实可行：`TtCommandExecutionPolicy.IsServiceCommand` 增加 `"CookServer"` 后，`Module\TitanCMD\Program.cs` 的主循环不会走 one-shot 的 `PostQuitMessage`；`ExecuteCommand` 返回的长跑 async `TtTask<int>` 的续体照常由 `TtEngine.Tick()` 推进 —— 恰好满足"cook 期间必须持续 Tick 引擎（地形 bake 投递在 TPools，续体走 AsyncEvents 队列）"。

> 实现期修正：`IsServiceCommand == true` 的另一面是**命令返回后主循环也不会退出**（`Program.Run` 只在非 service 时 `PostQuitMessage`）。worker 是自己决定何时收摊的（idle/restart/manual/pipe/fatal），所以 `TtCookServer.ExecuteCommand` 在写完 `quit` 行后**必须自己调 `TtEngine.Instance.PostQuitMessage()`**，否则进程会空转到天荒地老。`Tests\Titan.Tests.Base\TitanCMD\CommandExecutionPolicyTests.cs` 已加 `[TestCase("CookServer", true)]` 与 `[TestCase("Cook", false)]` 锁住这条语义。

**实现红线**：读 stdin 必须放独立线程，把**原始行**投进 `ConcurrentQueue`，由 cook 循环排空解码。**不得**照抄现有 `ExtraCmd=` 的主线程 `Console.ReadLine()`（`Program.cs` 内那段会把引擎 Tick 卡死）；也不得把读线程挂到 `EventPoster` 专线上（会永久占掉 cook 自己要用的那条上下文线程）。

IPC 响应延迟上界约等于 worker 的轮询间隔（`PollIntervalMs=100`；cook 循环用 `Task.Delay` 让出而非阻塞，以便引擎 `Tick` 继续推进 bake 续体）。取消的实际生效时间取决于 job 内部检查点密度，不是 IPC 往返。

**退出时机分两档**（M1 的重投策略依赖这条，必须写清）：

| 触发 | 在飞 job | 已接收但未开工的队列 |
|---|---|---|
| `quit` 指令（`manual`） | 跑完 | **跑完**（语义是"不再接新活"，不是"作废已接的活"） |
| stdin 关闭（`pipe`） | 跑完 | 作废 |
| `WorkerMaxJobs=`（`restart`）/ 取消宽限期超时（`fatal`） | 跑完当前这个 | 作废 |

提交侧的统一规则因此很简单：**凡是没收到终态行（`job.done`/`job.error`）的 job 一律重投**（幂等，见下文原子写与 hash）。

> 实现期修掉的竞态：读线程在 `ReadLine()` 返回 `null` 时不能直接把退出原因置成 `pipe` ——
> 一次性灌进来的指令（含最后那条 `quit`）可能还在收件箱里，主循环会一个 job 都跑不上就退。
> 改为只报 `mStdinClosed` 标志，由主循环**先排空收件箱**再判退出条件；写 stdout 失败（对端读端挂了）
> 也归一按 `pipe` 处理，不让它炸成未观测异常把退出码搅乱。

## 进程间通讯协议

行式 JSON + 哨兵前缀，风格对齐仓库既有的 `[TITAN-STAGE]`（`Editor\AndroidBuild\TtAndroidBuildLogParser.cs`）：**非协议行原样当日志转发**，人读输出零丢失，协议解析失败不丢信息。

```
[TITAN-COOK]{"v":1,"t":"job.begin","id":7,"asset":"tutorials/terrain/terrain_scene.scene","type":"Scene"}
```

### 编辑器 → worker（写子进程 stdin）

| 消息 | 载荷 | 说明 |
|---|---|---|
| `add` | `id`, `asset`, `type`, `force` | 入队一个单资产 job；worker 侧队列满则回 `busy`。`asset` 必须是资产表键形式（相对 root、剥掉 `.ameta` 的那串，如 `utest/emptyscene.scene`），详见 `TitanCMD.md` 的“资产标识写法” |
| `cancel` | `id` | 置位该 job 的协作式取消标志；不在飞则出队 |
| `flush` | — | 批次边界，worker 回 `stats`（处理数/失败数/耗时） |
| `quit` | `reason` | 优雅退出（`idle` / `restart` / `manual`） |

### worker → 编辑器（写 stdout）

| 消息 | 载荷 | 说明 |
|---|---|---|
| `hello` | `proto`, `engineVer`, `cfgHash`, `root`, `types[]`, `pid` | 握手。**协议版本或引擎版本/配置 hash 不匹配 → 直接报错，不静默降级**（不同版本写同一 cache 是互相污染） |
| `ready` / `idle` | `queued` | 可接活 / 队列已空（启动后与每个 job 到终态后各发一行：还有活发 `ready`，真空长发 `idle`；在飞 job 由 `job.begin`~终态行 区间确定，不另发字段） |
| `beat` | `id`, `seq`, `elapsed` | 心跳，用于卡死判定 |
| `job.begin` | `id`, `asset`, `type` | — |
| `job.stage` | `id`, `stage`, `elapsed` | 阶段文本（如 terrain 的 `bake pending=3`） |
| `job.progress` | `id`, `fraction` | 子进度 0..1，CookAsset 实现逐步接入 |
| `job.done` | `id`, `elapsed`, `artifacts[]` | `artifacts` 一律为**相对仓库根**的路径 |
| `job.error` | `id`, `message`, `stack`, `kind` | `kind` ∈ `Asset` / `Cancel` / `Crash` / `Timeout` / `Conflict` |
| `log` | `tag`, `category`, `text` | 可选：结构化转发 worker 日志到编辑器 `Profiler.Log`，LogWatcher 直接可见 |
| `stats` | `done`, `failed`, `skipped`, `elapsed` | 对 `flush` 的应答 |

**job 的关闭条件**（提交侧状态机只认这个，不靠墙钟猜）：收到 `job.done` 或 `job.error` 即为终态；
收到 `busy` 表示本次 `add` 被拒，需重投；`cancel` 无论打在在飞 job 还是未开工 job 上，最终都会落到
一行 `job.error`（`kind=Cancel`，worker 侧 M0 已保证）；worker 进程挂了则按崩溃路径处理
（见生命周期模型）。`Conflict` 预留给 M3 的多机写冲突；`Crash` 由提交侧从进程退出码推断，worker 自身不发。

编解码放在 `CSharpCode\Base\IO\CookProtocol.cs`，纯静态、不依赖引擎初始化，便于脱离引擎单测。传输抽象：

```csharp
public interface ITtCookTransport : IDisposable
{
    bool IsAlive { get; }
    void WriteLine(string message);
    event Action<string> OnLine;      // 任意线程回调
    event Action<int> OnExit;         // 参数为退出码
}
```

v1 实现 `TtProcTransport`（`Process` + `RedirectStandardInput/Output/Error` + 独立读线程）；v3 加 `TtTcpTransport`（复用 `Bricks\TcpServer\TtTcpServer`/`TtTcpClient` 与 `TtNetPackageManager`），**消息 schema 一行不改**。

## 任务模型

- **Job = 单资产**（即现行 `Asset=` 语义）。`AssetType=` 批量在**编辑器侧**展开成 job 列表 —— 这样多机分片时远端不必自己扫目录，负载也能均分，且失败可精确到资产。
- **去重与互斥**：同一 asset key 同时只允许一个在飞 job；已在队列或已在飞的重复提交合并（面板显示 `merged` 计数）。
- **黑名单只在入队侧生效**（策略集中一处），worker 只接收具体资产路径。
- **跳过判定**：新增 `IAssetMeta.NeedsCook(IAsset)` virtual，默认返回 `true`（保守重烘）。已有可包装的先例：`TtTextureCookManager.IsCookValid`（AssetId + 源时间戳）。面板 `Force` 开关绕过它。
- **取消**：`cancel` 只置位 `TtCookContext.IsCancellationRequested`，cook 实现在资产/level 边界轮询后**正常返回** `Cancelled`；不使用 `task.CancelTask()`。地形侧 bake 的 QEM/重采样循环已有检查点，可直接对接。
- **前置校验（硬约束）**：cook 以**磁盘上的源文件**为唯一真相。入队时若资产在编辑器里是 dirty（存在未保存内存态），拒绝入队并在面板提示"先保存"。否则会 cook 出比编辑器更旧的数据，且失败是静默的。

## 共享 cache 目录下的一致性规则

多机读写同一 `cache/`（`ERootDir.Cache` + `ESystemDir.CookedAssets`）时，以下五条是必须的，其中前两条在本机多 worker 阶段就会踩到：

1. **临时文件名带机器+进程后缀**：`{final}.{machineId}-{pid}.tmp` → 再原子替换。两台机器同时写 `xxx.tmp` 会互相截断。由 `TtCookArtifactWriter` 统一封装，CookAsset 实现不得自己 `File.WriteAllBytes` 到最终路径。
2. **原子替换要容忍"谁先到"**：`File.Replace`/`Move` 在 SMB/NFS 上对同一目标可能返回 access-denied。规则：替换失败 → 重读目标，hash 与自己产物一致即视为成功（`AlreadyUpToDate`），不一致才报 `Conflict`。
3. **租约锁防重复劳动**：`cache/locks/{assetHash}.lock` 内容 `{machineId, pid, beginTime, heartbeatTime}`。分派前先查租约，心跳超时的陈旧锁可抢占。没有这条，两台机器会重复烘同一场景的 DAG（bake 是最贵的一类）。
4. **有效性判定改用内容 hash，不用 LastWriteTime**：现有 `IsCookValid` 比较源文件时间戳，共享目录 + 多机时钟漂移会让判定永远"过期"或永远"有效"。对齐既有做法：地形 overlay 的 `levellist.txt` 每行是 `{levelX}_{levelZ}#{SHA256}`。`NeedsCook` 的默认实现走 `TtCookArtifactWriter` 记录的 hash 台账，时间戳只作本机快路径。
5. **协议中的 `artifacts` 与 `root` 都用相对仓库根路径**，远端机器自行映射；`hello` 的版本/hash 不匹配为**硬拒**。

另外两条边界：

- **产物归属不变**：cook 只写 `cache/`（可重建派生物），绝不碰 `content/` 下 scene 目录旁的 overlay（`.thl`/`.tml` 是项目数据，归版本控制）。
- **编辑器已打开的资产**：编辑器自身（CDLOD 预览 / PIE）也会 bake 同一场景，跨进程锁不成立。v1 规则：CookManager 默认**跳过编辑器已打开的场景**，面板显著标注原因，并提供 `IncludeOpen` 勾选。

## 进度与编辑器 UI

面板入口：`Windows ▸ Cook Manager`（并加 `Tools ▸ Cook All`）。

- **总进度**：`Σ(done + fraction × weight) / Σweight`，样式沿用 AndroidBuild 的 `✓ / ▶ / · / ✗` + 文本进度条。
- **Worker 区**：每个 worker 显示 `pid`、状态、启动耗时、内存占用、当前 job、最近心跳时刻；按钮：启停、重启、并发数调整。
- **队列区**：`Pending / Running / Done / Failed / Cancelled / Skipped` 分列；Failed 行可展开错误详情、跳转"打开资产"、`Retry`；日志跳 LogWatcher（预置 `Cook` 类别过滤）。
- **状态栏**：编辑器主窗口一条轻量进度，有活跃 batch 时显示，不必打开面板也能看到进度。
- **历史台账**：批次记录（提交时间、job 数、失败数、耗时）落 `cache/config/cook_history.json`，便于回看"上一批为什么失败"。

## 线程与数据流

```
[IO 线程]  读 stdout → 解析 CookProtocol → 入 ConcurrentQueue
[主循环]   TtCookManager.TickModule 排空队列 → 更新 job/worker 状态 → 面板与状态栏只读
[主循环]   面板按钮 → CookManager 入队 → 投递给空闲 worker（写 stdin，QuickWrite 不阻塞）
[worker]   TtCookDriver 串行跑 job，引擎 Tick 推进 bake，心跳由 worker 侧循环发
```

- 跨线程写只有 IO 读线程 → 队列一处，符合引擎"后台重活、结果在主循环回收"的既有范式（对应 `TtTask.AddWaitTask` + `TaskCollector.Tick` 的语义，但这里用显式队列，因为消息是流式的而非单次结果）。
- 面板 `OnDraw` 不写任何状态，只发起命令式操作（入队/取消/重启）。
- 写 stdin 在 TickModule 中执行，但必须 `AutoFlush` 且不读回，避免管道满时卡主循环；必要时改投 IO 线程写。

## 与既有链路的关系

| 入口 | 处置 |
|---|---|
| `TitanCMD ExeCmd=Cook` | **保留**，供 CI / 命令行 / 脚本使用。内部改为调 `TtCookDriver`（纯重构，命令行语义与返回码不变） |
| `TitanCMD ExeCmd=CookServer` | **新增**，常驻 worker，service command |
| `ExeCmd=SaveAsLastest` | **不动**。注意 AndroidBuild 面板的 "Cook" 阶段实际跑的是 `ExeCmd=SaveAsLastest`（`TtAndroidBuildCommandLine.Cook` 生成，测试有断言），它会回写源文件，与 cook 红线相反 —— 下沉 Driver 时不要顺手改掉 |
| 编辑器内右键直接 in-process cook | 收口到 CookManager 提交（本机 worker）。不保留"worker 起不来就在编辑器进程内跑"的 fallback，维持单一语义；worker 启动失败就明确报错 |
| `TtAndroidBuildPanel` 的 CookFirst | 后续（M3 后）可改走 CookManager 以获得细进度与错误隔离；v1 不动 |

## 代码落点

| 文件 | 内容 | 状态 |
|---|---|---|
| `CSharpCode\Base\IO\CookProtocol.cs` | `TtCookMessage` 结构 + 行编解码（纯静态，可脱离引擎单测） | M0 |
| `CSharpCode\Base\IO\CookContext.cs` | `TtCookContext`、`ITtCookProgressSink`、`TtCookLogSink`、`TtCookProtocolSink` | M0 |
| `CSharpCode\Base\IO\CookDriver.cs` | `TtCookDriver`：类型枚举/扫描/黑名单（下沉自 TitanCMD）+ 单 job 执行 | M0 |
| `CSharpCode\Base\IO\IAssetMeta.cs` | `CookAsset` 契约注释补 `TtCookContext` 用法与禁止事项 | M0 |
| `CSharpCode\Base\IO\CookContext.cs` | `TtCookArtifactWriter`（原子写 + hash 台账） | M2 |
| `CSharpCode\Base\IO\IAssetMeta.cs` | 新增 `NeedsCook` virtual（默认 true） | M2 |
| `CSharpCode\Editor\Cook\CookManager.cs` | `TtCookManager : TtModule<TtEngine>`（队列/批次/状态机/`TickModule` 排空）+ `TtEngine` partial property | M1 |
| `CSharpCode\Editor\Cook\CookWorker.cs` | `TtCookWorker`、`ITtCookTransport`、`TtProcTransport`、`ITtCookProcessLauncher`/`Host` | M1 |
| `CSharpCode\Editor\Cook\CookJob.cs` | `TtCookJob` 状态枚举与台账结构 | M1 |
| `CSharpCode\Editor\Forms\CookManagerPanel.cs` | `TtCookManagerPanel : IRootForm` | M2 |
| `CSharpCode\Editor\MainEditorApplication.cs` | 菜单入口 + 面板创建与 Initialize（照 `mAndroidBuildPanel` 的三处接法） | M1/M2 |
| `Module\TitanCMD\Command\CookAsset.cs` | 改薄壳，调 `TtCookDriver` | M0 |
| `Module\TitanCMD\Command\CookServer.cs` | `TtCookServer : TtCookCommand`，常驻 worker 主循环 | M0 |
| `Module\TitanCMD\Program.cs` | `case "CookServer"` | M0 |
| `Module\TitanCMD\UCookSettings.cs` | `IsServiceCommand` 补 `"CookServer"`；`Param_` 常量 + `FindIntArgument` | M0 |
| `Tests\Titan.Tests.Base\Cook\*` | 协议编解码、上下文行为（已交付）；队列状态机（fake transport）、去重/取消/超时判定、原子写规则（待 M1/M2） | M0+ |
| `Documents\editor\CookManager.md`（本文）/ `TitanCMD.md` | 后者补 `CookServer` 命令条目 | M0 |

TitanCMD 工程是 SDK-style 且未关闭 `EnableDefaultCompileItems`，新增 `.cs` 自动参与编译，无需手工登记。
**但 `CSharpCode\**\*.projitems` 是共享工程且用显式 `<Compile Include>` 清单** —— 引擎侧新增文件必须往 `Base.projitems` 登记，否则静默不参与编译。

## 里程碑与验收

### M0 —— 协议与驱动下沉（不动 UI）

分两步降低回归面：

1. **纯重构**：把 `CookAsset.cs` 的扫描与执行逻辑**原样搬**进 `TtCookDriver`，TitanCMD 的 `Cook` 改为调用它。跑通即算完成。
2. **叠新功能**：`CookProtocol` / `TtCookContext` / 协议 sink / `Command\CookServer.cs`。

验收：手工脚本喂 3 个资产（含 1 个不存在、1 个故意失败）给 `ExeCmd=CookServer` 的 stdin，协议行齐全（`hello`/`job.begin`/`job.stage`/`job.done`/`job.error`/`beat`）；`ExeCmd=Cook` 的**日志文本与返回码**与重构前一致；macOS 冒烟脚本 `Core.Mac\smoke_titancmd.sh` 不受影响。

> 实现期修正（原条文字面不可达）："输出逐字节一致" 做不到。`Profiler.Log.WriteLine` 带 `[CallerFilePath]/[CallerLineNumber]`，`OnReportLog_WriteConsole` 会把 `源文件(行号,0):` 拼到每一行前面 —— 代码从 TitanCMD 搬到引擎就必然改前缀。另外单资产 cook 每个 job 会多出两条 stage 日志（`resolve meta` / `create asset`，走 `TtCookLogSink`）。文本主体、条数变化之外的顺序、以及返回码均保持不变；已核实仓库内无脚本/测试解析 `ExeCmd=Cook` 的 stdout，`smoke_titancmd.sh` 跑的是 `ExeCmd=MakeSln`，不受影响。

#### M0 实测记录（2026-11-09）

已跑通的部分（真实 `TitanCMD.exe ExeCmd=CookServer`，工作目录 `binaries`，`NativeDLL=debug`）：

- 引擎能正常起、正常收摊：`hello`→`ready`→…→`stats`→`quit`，退出码 0；`hello` 里 `root` 报 `d:/github/titanengine/content/`，`types` 含 `Scene`。
- 协议行齐全且与引擎人读日志混排不互于干扰；一次性灌入的 stdin（读完即关）不会抢在收件箱排空前退出。
- 不存在的资产得到 `job.error kind=Asset`；每 job 终态行后的 `ready`/`idle` 分档正确。

跑这一步探出的两个缺陷（已修，但需重新构建 Engine 后才能复验）：

1. **`TtCookMessage.IsVersionSupported` 被序列化进每一行** —— 它是本地判定用的计算属性，不在线上字段表里；已加 `[JsonIgnore]`。提交侧解析器必须对未知字段宽容，但 worker 不该往线上抹冗余。
2. **单资产定位判据错** —— 原用“主文件是否存在”，但 `Scene` 类资产的名字是个**目录**（`File.Exists` 恒 `false`），导致合法资产被误报。已改为以 `.ameta` 存在为权威判据，与批量扫描、`AssetMetaManager` 资产表键同源。

待复验项：真资产 `job.done`（含 `artifacts`）、`job.stage` 序列、terrain 长 job 的 `beat` 与取消、`ExeCmd=Cook Asset=` 单资产通路。

### M1 —— 编辑器队列与本机 worker

`TtCookManager` + `TtCookWorker`，`Tools ▸ Cook All` 提交、跑完、失败自动重启 worker。此阶段**可以先不做面板**，进度写引擎日志即可。

验收：提交 50 个 job；期间编辑器主循环帧率无可测下降；杀掉 worker 进程 → 在飞 job 标 `Crash`、自动重试、批次最终状态正确；关闭 Cook 面板后进度继续推进（这是与 AndroidBuild 面板行为的关键差异）。

### M2 —— 面板与体验

`TtCookManagerPanel`、ContentBrowser 右键 `Cook`、跳过编辑器打开资产、历史台账、terrain 子进度（`bake pending=N` → `fraction`）、`TtCookArtifactWriter` 原子写与 hash 台账。

验收：混选 dirty/clean 资产提交，dirty 被拒并提示；同一资产重复提交被合并；取消在飞 job 在检查点内退出且不产生半截产物。

### M3 —— 多机

`TtTcpTransport` + `hello` 硬握手 + `IWorkerSelector`（能力标签：平台、压缩格式）+ 租约锁。

前置条件（必须先解决，否则不要开工）：① 共享 cache 目录对所有机器可读写；② 所有机器引擎版本/配置 hash 一致；③ worker 地址与鉴权配置项。

## 测试策略

- `CookProtocol`：编解码往返、畸形行不抛且被当日志、协议版本不匹配被拒。
- `TtCookManager` 状态机：注入 fake `ITtCookTransport`（与 AndroidBuild 的 fake host 同思路），覆盖去重合并、per-asset 互斥、崩溃重试上限、空闲优雅退出、批次边界重启、取消传播。
- `TtCookArtifactWriter`：tmp 名唯一性、替换失败后的 hash 等价判定、陈旧租约抢占。
- 引擎工程不能用 `dotnet build` 单独编译（analyzer/source generator 会加载引擎程序集导致失败），协议与状态机测试尽量写成不依赖 `TtEngine.Instance` 的纯逻辑；确需引擎的用现有测试工程的既有装配方式。

## 已知风险与未决项

| 风险 | 现状与处置 |
|---|---|
| worker 长跑内存增长 | 引擎关闭路径本身脆弱（`FinalCleanup` 有已知遗留缺口），故用"批次边界重启"规避而非依赖单次退出干净 |
| `EPlayMode.Cook` 至今全仓无赋值 | 本设计不动 `PlayMode`；若要启用需先逐个审计所有 `PlayMode == Editor` 判定点 |
| 编辑器与 worker 同烘一场景 | v1 用"跳过编辑器已打开资产"回避；根治需要 cache 侧租约与 bake 幂等 hash，排 M2/M3 |
| 子进度覆盖率 | v1 只有 job 级进度 + stage 文本；`ReportSubProgress` 需各 `CookAsset` 实现逐个接入，terrain 先做 |
| 远端机器的 engineconfig | 共享 cache 不等于共享配置；`cfgHash` 只保证不同不会跑，不保证该跑的配置是什么 —— 需要一份"cook 专用配置"作为分发基线（未决） |
