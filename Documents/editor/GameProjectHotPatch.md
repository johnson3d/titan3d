# GameProject Macross HotPatch 使用指南

GameProject Macross HotPatch 是 TitanEngine 面向已发布版本的“救命热修”机制。它允许在不重新发布完整客户端的情况下，用 LeanCLR 执行新的 Macross 方法体。

该机制适合修复范围明确、能够通过替换现有方法逻辑解决的问题。它不是通用的程序集热替换系统，也不能改变已发布版本的类型布局或公开接口。

> 当前实现对应：
>
> - 发版镜像与 baseline：`Module/GameBuilder/Program.cs`、`CSharpCode/Bricks/CodeBuilder/MacrossNode/MacrossEditor.cs`
> - 补丁工程：`Module/GameProject.Patch/`
> - 补丁构建：`CSharpCode/Bricks/AssemblyLoader/MacrossPatchBuilder.cs`
> - 运行时加载：`CSharpCode/Bricks/AssemblyLoader/MacrossPatchLoader.cs`
> - 端到端示例：`Tests/Titan.Tests.LeanCLR*`

---

## 1. 适用范围

HotPatch 可以用于：

- 修正已发布 Macross 类中现有方法的逻辑。
- 修改方法返回值、条件判断、数值计算和现有对象状态操作。
- 在补丁方法中调用镜像已经暴露的其他宿主方法。
- 处理当前编组层支持的基础值、`string`、枚举、可直接复制的结构体和已镜像宿主对象。

HotPatch 不能用于：

- 新增宿主类型、字段或需要宿主识别的新方法。
- 修改已有方法的名称、参数、返回类型或参数顺序。
- 修复未进入发版 baseline 的方法。
- 替换原生 C++、Shader、资源或普通非 Macross 逻辑。
- 绕过 GameProject MVID、镜像哈希或方法签名校验。

如果修复需要改变接口或数据布局，应制作新的完整版本。

---

## 2. 大致原理

完整链路如下：

```text
发版 GameProject 源码
    │
    │ BuildGame.bat / GameBuilder（enableMirror: true）
    ▼
GameProject.dll
GameProject.LeanClrMirror.dll
GameProject.baseline.json
    │
    │ 程序员针对 baseline 编写 GameProject.Patch
    ▼
GameProject.Patch.dll
    │
    │ TitanCMD ExeCmd=macross-patch
    ▼
patch.json + GameProject.Patch.dll (+ PDB)
    │
    │ 部署到 cache/macrosspatch，重启进程
    ▼
Stage A：VM 初始化前校验文件、镜像并注册补丁程序集
Stage B：GameProject 加载后校验 MVID、方法签名并激活
    │
    ▼
宿主方法入口桩按 MethodId 转发到 LeanCLR 补丁方法
```

### 2.1 发版时生成的三类产物

`GameBuilder` 使用 `enableMirror: true` 编译 GameProject。`TtMacrossMirrorGenerator` 扫描同时满足 `[TtMacross]` 与 `[TtLeanClr]` 要求的类型，并生成：

1. 宿主 thunk 注册表，编入 `GameProject.dll`。
2. LeanCLR 镜像桩，最终编译为 `content/LeanCLR/GameProject.LeanClrMirror.dll`。
3. 补丁 baseline，保存为 `content/LeanCLR/GameProject.baseline.json`。

baseline 固定记录：

- `GameProject.dll` 的 MVID。
- 镜像程序集名称及 SHA256。
- 每个可热修方法的完整签名和 MethodId。

MethodId 是完整签名

```text
类全名::方法名(参数类型1,参数类型2,...)
```

按 UTF-16 code unit 计算的 FNV-1a 64 位哈希。方法名称或参数发生任何变化都会得到不同的 MethodId。

### 2.2 入口桩

可热修 Macross 方法生成的 C# 方法体首部包含类似逻辑：

```csharp
if (EngineNS.Macross.TtMacrossPatch.Active &&
    EngineNS.Macross.TtMacrossPatch.TryInvoke(methodId, this, out TResult result, arg0))
{
    return result;
}
```

没有补丁、补丁未命中、调用线程不符合要求或该方法已经熔断时，入口桩继续执行原始方法体。

### 2.3 两阶段加载

- **Stage A**：LeanCLR VM 初始化前执行。读取 `patch.json`，验证补丁包文件 SHA256、镜像程序集名称与哈希、程序集名称冲突，然后注册 `GameProject.Patch.dll`。
- **Stage B**：GameProject 加载后、Rtti 更新前执行。验证 GameProject MVID、MethodId、宿主类型、参数和返回类型，建立补丁类型映射并激活补丁。

任一阶段失败都会拒绝整包，游戏继续使用原始逻辑，不会只激活其中一部分方法。

---

## 3. 发版前准备

HotPatch 只能基于已发布版本的原始产物制作。每次正式发版必须执行以下操作。

### 3.1 构建 GameProject 镜像和 baseline

在仓库根目录执行：

```bat
BuildGame.bat
```

`BuildGame.bat` 调用 `GameBuilder.exe`，并以 `enableMirror: true` 编译 GameProject。

> 编辑器内普通的“编译 GameProject”当前使用默认参数 `enableMirror: false`，不会生成可用于正式补丁的镜像和 baseline。制作发版基线应使用 `BuildGame.bat`/`GameBuilder`。

构建后至少检查以下文件：

```text
binaries/net8.0/GameProject.dll
content/LeanCLR/GameProject.LeanClrMirror.dll
content/LeanCLR/GameProject.baseline.json
content/LeanCLR/Titan.LeanClr.EngineStub.dll
```

其中镜像程序集需要随游戏版本部署到该版本的 `LeanCLR` 内容目录，否则运行时 Stage A 会拒绝补丁。

### 3.2 归档基线

为每个正式版本单独归档以下文件，禁止被后续开发构建覆盖：

```text
GameProject.dll
GameProject.LeanClrMirror.dll
GameProject.baseline.json
```

建议按版本号或构建号归档，例如：

```text
PatchBaselines/
  1.2.3-1042/
    GameProject.dll
    GameProject.LeanClrMirror.dll
    GameProject.baseline.json
```

制作补丁时必须使用目标客户端对应的 baseline。使用其他版本的 baseline 会因 MVID 或镜像哈希不一致而被拒绝。

---

## 4. 制作一次热修

### 步骤 1：确认问题能由方法体修改解决

先确认：

- 目标方法存在于目标版本的 `GameProject.baseline.json`。
- 修复不要求新增字段、修改类型布局或改变方法签名。
- 目标逻辑可以只使用镜像中已有的类型和成员实现。

如果 baseline 中没有该方法，这次热修无法补入，只能发布新版本。

### 步骤 2：在补丁工程中编写补丁类

仓库已提供补丁工程：

```text
Module/GameProject.Patch/GameProject.Patch.csproj
```

该工程目标框架为 `netstandard2.0`，程序集名称固定为 `GameProject.Patch`，并引用：

- `content/LeanCLR/GameProject.LeanClrMirror.dll`
- `content/LeanCLR/Titan.LeanClr.EngineStub.dll`

在 `Module/GameProject.Patch/` 下新建 `.cs` 文件。补丁类必须：

1. 与宿主类型位于相同 namespace。
2. 类名为宿主类名加 `__Patch` 后缀。
3. 继承镜像中的宿主类型。
4. 用 `public new` 声明与 baseline 完全相同的方法。

示例：

```csharp
namespace NS_tutorials.helloword
{
    public class helloword__Patch : global::NS_tutorials.helloword
    {
        public new System.Boolean Foo(System.Int32 a)
        {
            // 修复后的逻辑
            return a >= 0;
        }
    }
}
```

不要复制或修改原宿主类；补丁程序集只包含 `__Patch` 类型。

### 步骤 3：遵守方法限制

当前补丁入口和镜像生成链路的主要限制如下：

- 按实例方法使用；补丁方法不能是 `static`。
- 参数最多 8 个。
- 不支持嵌套宿主类、泛型宿主类和泛型方法。
- 不支持 `ref`、`out`、`in`、`params` 参数和 ref 返回值。
- 不支持 Macross async 方法、unsafe 方法和 RawBody/Host 方法。
- 补丁类中不能声明同名重载；LeanCLR 当前按方法名解析补丁方法。
- 参数和返回类型必须与 baseline/宿主方法完全一致。
- 参与本次补丁的 `__Patch` 类中，每个声明的方法都必须被列入 `patch.json`；同一补丁类存在未登记方法时整包拒绝。

可参考实际覆盖用例：

```text
Tests/Titan.Tests.LeanCLR.Patch/PatchableActorPatch.cs
Tests/Titan.Tests.LeanCLR.HostGame/PatchableActor.cs
```

### 步骤 4：编译补丁工程

例如编译 Release：

```powershell
dotnet build Module/GameProject.Patch/GameProject.Patch.csproj -c Release
```

必须先成功生成：

```text
Module/GameProject.Patch/bin/Release/netstandard2.0/GameProject.Patch.dll
```

当前 `TtMacrossPatchBuilder` 不会代替用户执行 `dotnet build`，只会查找已编译 DLL。查找顺序是 `bin/Debug/*`，然后是 `bin/Release/*`；如果两处都有产物，要先清理旧的 Debug 产物，避免误打包旧 DLL。

### 步骤 5：校验并打包

在仓库根目录执行：

```powershell
binaries/net8.0/TitanCMD.exe `
  ExeCmd=macross-patch `
  PatchProject=Module/GameProject.Patch `
  BaselinePath=content/LeanCLR/GameProject.baseline.json `
  OutputDir=cache/macrosspatch
```

参数说明：

| 参数 | 说明 |
|---|---|
| `PatchProject` | 补丁工程目录，工具从其 `bin/Debug/*` 或 `bin/Release/*` 查找 `GameProject.Patch.dll` |
| `BaselinePath` | 目标发布版本的 `GameProject.baseline.json` |
| `OutputDir` | 补丁包输出目录；本地运行时默认读取 `cache/macrosspatch/` |

成功时会输出类似信息：

```text
[INFO] Patch package written to cache/macrosspatch (1 methods).
```

输出目录包含：

```text
cache/macrosspatch/
  patch.json
  GameProject.Patch.dll
  GameProject.Patch.pdb       # 存在 PDB 时一并输出
```

构建器会验证每个补丁方法是否存在于 baseline，并重新计算 MethodId。新方法或签名变化会报 `requires a new release`。

### 步骤 6：测试补丁包

至少验证：

1. 无补丁包时原版本行为不变。
2. 放入补丁包并重启后，日志出现 `Macross patch activated`。
3. 修复场景行为正确。
4. 未补丁方法仍执行原逻辑。
5. 删除补丁包并重启后能够回退到原逻辑。
6. 非法包、旧 baseline 或文件被修改时，日志出现 `Macross patch rejected`，且游戏仍可执行原逻辑。

端到端测试实现位于：

```text
Tests/Titan.Tests.LeanCLR/MacrossPatchProbe.cs
```

它覆盖文件哈希、镜像哈希、MVID、MethodId、签名、返回类型、未登记方法、重载、线程回退和异常熔断等场景。

### 步骤 7：部署并重启

把同一次构建产生的 `patch.json`、`GameProject.Patch.dll` 和清单列出的其他文件完整部署到目标游戏的：

```text
cache/macrosspatch/
```

补丁程序集必须在 LeanCLR VM 初始化前注册，因此替换补丁包后需要重启进程。不要在进程运行期间只替换其中一个文件。

---

## 5. patch.json 内容

`patch.json` 由工具生成，不应手工编辑。结构示意：

```json
{
  "BaselineId": {
    "GameMvid": "00000000-0000-0000-0000-000000000000",
    "MirrorAssembly": "GameProject.LeanClrMirror",
    "MirrorSha256": "..."
  },
  "PatchAssembly": "GameProject.Patch",
  "Methods": [
    {
      "MethodId": "0x0000000000000000",
      "Class": "NS_tutorials.helloword",
      "Method": "Foo",
      "Signature": "NS_tutorials.helloword::Foo(System.Int32)"
    }
  ],
  "Files": [
    {
      "Name": "GameProject.Patch.dll",
      "Sha256": "..."
    }
  ]
}
```

运行时要求 `Files[].Name` 只能是文件名，不能包含路径，并会逐个验证 SHA256。

---

## 6. 失败回退与异常熔断

### 6.1 整包拒绝

Stage A 或 Stage B 任一校验失败时：

- 整个补丁包不会激活。
- `TtMacrossPatch.Active` 保持关闭。
- GameProject 继续执行原始逻辑。
- 失败原因写入日志，格式包含 `Macross patch rejected`。

### 6.2 非 VM 线程调用

补丁方法只在 LeanCLR VM 所属逻辑线程执行。其他线程调用同一宿主方法时会跳过补丁、执行原始逻辑，并为该方法记录一次警告。

### 6.3 单方法异常熔断

补丁方法执行失败时会抛出 `TtMacrossPatchException`，并将该 MethodId 标记为 Broken。当前进程内该方法的后续调用回退到原始逻辑，其他补丁方法不受影响。

---

## 7. 回滚补丁

回滚不需要重新发布 GameProject：

1. 停止目标进程。
2. 移走或替换 `cache/macrosspatch/` 下的整个补丁包。
3. 重新启动进程。
4. 验证日志中没有旧补丁的激活记录，并确认原始逻辑恢复。

不要只删除 DLL 而保留引用它的 `patch.json`；这会产生明确的整包拒绝日志，但不是干净的回滚状态。

---

## 8. 安全注意事项

- `patch.json` 中的 SHA256 用于完整性和版本一致性校验，不等同于发布者身份认证。
- 加载器提供 `ITtMacrossPatchVerifier` 和可选 `patch.sig` 扩展点，但当前引擎默认使用 `TtMacrossPatchAcceptAllVerifier`，不会强制验证签名。
- 正式发布系统应在分发链路中控制补丁目录写权限，并在接入可信签名验证后再把 `patch.sig` 当作安全凭据。
- baseline、补丁 DLL 与 `patch.json` 必须来自同一次受控构建，不能混用。

---

## 9. 常见问题

### 找不到 `GameProject.LeanClrMirror.dll`

先执行 `BuildGame.bat`，并检查 `content/LeanCLR/Titan.LeanClr.EngineStub.dll` 是否准备完成。普通编辑器编译不会生成正式镜像。

### `GameProject.Patch.dll not found`

先执行 `dotnet build Module/GameProject.Patch/GameProject.Patch.csproj`。打包器不会自动编译补丁工程。

### `requires a new release`

补丁方法不在目标 baseline 中，或者方法签名已经变化。不能通过手工修改 `patch.json` 绕过，必须重新发版。

### `GameProject MVID ... does not match baseline`

使用了错误版本的 baseline，或者目标客户端的 `GameProject.dll` 不是制作补丁时对应的版本。

### `mirror assembly ... does not match the patch baseline`

客户端部署的镜像 DLL 与 baseline 不匹配，或文件已被修改。恢复目标版本原始镜像。

### `is overloaded; patch methods are resolved by name`

同一个 `__Patch` 类中存在同名方法。当前补丁方法按名称解析，必须消除重载；如果宿主 baseline 本身需要补其中一个重载，应通过完整发版调整可热修接口。

### 补丁已部署但没有激活

依次检查：

1. 补丁是否位于运行实例实际使用的 `cache/macrosspatch/`。
2. 进程是否在部署后重启。
3. `content/LeanCLR/` 是否包含 EngineStub 和目标镜像，LeanCLR runtime 是否启动。
4. 日志中是否存在 `Macross patch rejected` 及具体原因。
5. GameProject、镜像、baseline 和补丁包是否属于同一发布版本。
