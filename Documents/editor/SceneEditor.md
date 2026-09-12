# SceneEditor 场景编辑器使用指南

`SceneEditor` 是 TitanEngine 的核心场景搭建工具，用于打开 / 编辑 / 保存 `.scene` 资产，配合 Outliner、属性面板、Content Browser 与 Camera/RenderPolicy 设置完成场景的可视化编辑。

> 对应代码：
> - 编辑器主体：`CSharpCode/Editor/Forms/SceneEditor.cs`
> - 视口与交互模式：`CSharpCode/Editor/Forms/TtSceneEditorInteractiveMode.cs`、`CSharpCode/Editor/PreviewViewport.cs`、`CSharpCode/ImGui/Slate/TtWorldViewportInteractiveMode.cs`
> - 视口面板栏 / Axis / Camera 弹窗：`CSharpCode/Grapics/Pipeline/ViewportSlate.cs`
> - 节点拾取与坐标轴：`CSharpCode/GamePlay/Axis.cs`、`CSharpCode/Grapics/Pipeline/HitproxyManager.cs`
> - 地形高度编辑：`CSharpCode/Bricks/Terrain/CDLOD/TerrainEditorInteractiveMode.cs`、`TerrainEdit.cs`、`TerrainHeightCommand.cs`、`TerrainHeightOverlay.cs`

---

## 1. 总览

![SceneEditor 主界面](SceneEditor/MainUI.png)

打开任意 `.scene` 资产（在 Content Browser 双击，或从 PIE 工具栏新建）后，编辑器界面分区如下：

| 区域 | 位置 | 主要职责 |
|---|---|---|
| **主菜单栏** | 窗口顶端 | 工程级 / 编辑器级菜单（File / Windows / View / Illumination 等） |
| **工具栏** | Tab 标题之下，横排按钮 | 当前 .scene 的常用操作：Save / Reload / Undo / Redo / Mode 切换 / 节点过滤 |
| **Preview 视口** | 中间最大区域 | 3D 场景预览 + Axis 拖动 + 鼠标 / 键盘相机控制 |
| **视口面板栏** | Preview 视口左上角 | Axis 模式 (Move/Rot/Scale)、AABB/球面、相机弹窗、SnapMove/Rot/Scale、RenderPolicy |
| **Outliner** | 右上 | 场景节点树，单击 / Ctrl+单击选中节点 |
| **Terrain Brush** | 右上（与 Outliner 同一 tab 组） | 地形笔刷参数（高度 / 材质双通道）；**仅在地形雕刻模式下出现**，详见 [§5](#5-地形编辑terrain-brush) |
| **属性面板（三个 Tab）** | Outliner 下方 | `Editor Settings` / `NodeDetails` / `SceneDetails` |
| **Content Browser** | 窗口下方 | 资产浏览，双击 .scene 切换打开 |
| **Profilers** | 最底部 tab 区 | CpuProfiler / GpuProfiler / MemProfiler / ClrProfiler / LogWatcher |

> 视口左下角的 **xyz 小坐标轴** + `fps=xx.xx` 用于直观判断当前视图朝向和帧率。

---

## 2. 工具栏（顶部一排按钮）

工具栏紧跟在 Tab 标题（例如 `tutorials/gi/cornell/cornell.scene`）下面，从左到右：

| 按钮 | 说明 |
|---|---|
| **Open Macross** | 打开 / 切换该场景关联的 Macross 蓝图脚本 |
| **Save** | 保存当前 .scene 到磁盘（写回 AssetName 路径） |
| **Snapshot** | 截取当前预览图作为资产缩略图 |
| **Reload** | 丢弃未保存改动，从磁盘重新加载 .scene |
| **Undo / Redo** | 编辑撤销 / 重做（基于命令栈） |
| **Mode：下拉框** | 当前视口的 `TtInteractiveMode`，默认 `TtSceneEditorInteractiveMode`（含 Ctrl+Drag 复制），可切到基类 `TtWorldViewportInteractiveMode`（仅基础相机操作）或 `TtTerrainEditorInteractiveMode`（地形高度笔刷，见 [§5](#5-地形高度编辑terrain-brush)） |
| **GameObject** | 节点种类过滤（开关） |
| **LightDebug / PhyxDebug / UtilityDebug / NavMesh** | 各类 Debug 节点过滤（开关） |

> Mode 下拉框列出所有已通过 `InteractiveModeManager.RegisterMode` 注册的 Mode；新增 Mode 时只需在 `TtSceneEditorViewport` 静态构造函数里注册一次，下拉框自动出现。

---

## 3. Preview 视口

视口是编辑器的核心区域，3D 场景在这里被实时渲染。视口左上角有一排小图标（面板栏），主体响应鼠标 / 键盘 / 滚轮事件。

### 3.1 视口面板栏（左上小图标，从左到右）

| 图标 | 含义 | 说明 |
|---|---|---|
| ▶️ | **Pick 模式** | 切回普通选择，不显示 Axis |
| ✥ | **Move 模式** | 显示移动坐标轴（红 X / 绿 Y / 蓝 Z 箭头），拖动平移选中节点 |
| 🔄 | **Rotate 模式** | 显示旋转环，拖动绕轴旋转 |
| 🟦 | **Scale 模式** | 显示缩放手柄，拖动缩放选中节点 |
| 📏 | **Edge 模式** | 边吸附编辑（用于点 / 边 / 面级别操作） |
| 🟫 / ⚪ | **AABB / Sphere 切换** | 选中节点高亮包围体显示形式：立方体或球 |
| **C** | **Camera 设置弹窗** | 弹出 KeyMove / WheelMove / RotSpeed / ZNear / ZFar / X / Y / Z 等参数 |
| **🔘 (RPolicy)** | **RenderPolicy 切换** | 弹窗选择当前视口的 RenderPolicy（Deferred / Forward / 自定义...） |
| `SM 0.100` | **Snap Move** | 移动吸附步长（米）；拖 Move 轴时按此值整数倍吸附 |
| `SR 10.000` | **Snap Rotate** | 旋转吸附步长（度） |
| `SS 0.100` | **Snap Scale** | 缩放吸附步长（倍率） |

> 控件由 `TtWorldViewportSlate.OnDrawViewportUI` 绘制，与 `Axis.SetType / Axis.MoveStep / RotStep / ScaleStep` 直接绑定。

### 3.2 视口左下信息

- **xyz 小坐标轴**：实时反映相机朝向（红=X, 绿=Y, 蓝=Z）
- **fps=xx.xx**：当前预览的 FPS

### 3.3 鼠标 / 键盘交互

视口默认行为来自 `TtSceneEditorInteractiveMode`（继承 `TtWorldViewportInteractiveMode`）：

| 输入 | 行为 |
|---|---|
| **左键单击空白** | 取消选中 |
| **左键单击节点** | 选中该节点（走 Hitproxy 拾取） |
| **Ctrl + 左键单击节点** | 切换该节点的多选状态 |
| **左键拖 Axis** | 平移 / 旋转 / 缩放选中节点（取决于当前 Axis 模式） |
| **Ctrl + 左键拖 Move 轴** | **复制并拖动**：同步克隆选中节点（NodeName 自动加 `_Copy`，分配新 HitProxy ProxyId），选择切换到副本 |
| **Alt + 左键拖** | 围绕 LookAt 点 **轨道旋转** 相机 |
| **中键拖 / X1 拖** | **平移** 相机（沿屏幕平面） |
| **滚轮** | 沿视线 **推进 / 后退** 相机，LookAt 同步推进（`CameralWheelMoveWithLookAt = true`） |
| **W / A / S / D** | **飞行式** 相机移动 |
| **F** | 把当前选中节点的包围球 **Frame** 到视野中央 |
| **Delete** | 删除选中节点 |

> Mode 下拉框切到 `TtWorldViewportInteractiveMode` 时，Ctrl+Drag 复制功能失效，其它操作不变。适合需要"纯净相机操作"的场景。
>
> 切到 `TtTerrainEditorInteractiveMode` 后，**左键改为落笔雕刻地形**（不再选中节点、不再拖 Axis），相机操作（Alt+左键 / 中键 / 滚轮 / WASD）保持不变。详见 [§5](#5-地形高度编辑terrain-brush)。

---

## 4. 右侧面板

### 4.1 Outliner（右上）

显示当前 .scene 的节点树，例如：

```
EmptyName       <- 根节点 (Scene)
├─ red          <- 选中态高亮蓝色背景
├─ GridLine
└─ red_Copy     <- Ctrl+Drag 复制出的节点
```

- **单击节点名**：选中节点（与视口 hitproxy 拾取双向同步）
- **Ctrl + 单击**：多选 / 反选
- **左侧 ▶ 折叠箭头**：展开 / 折叠子节点
- **拖拽**：改变父子层级（将节点拖到目标父节点上）

### 4.2 属性面板三 Tab（Outliner 下方）

| Tab | 显示对象 | 典型用途 |
|---|---|---|
| **Editor Settings** | 编辑器自身的属性 (`EditorPropGrid.Target`) | `IsReadOnly` 锁编辑、查看 `AssetName`、切换 `RenderPolicy` |
| **NodeDetails** | 当前选中节点 (`NodeInspector.Target`) | 编辑 Position / Rotation / Scale / 材质 / 自定义脚本字段 |
| **SceneDetails** | 当前 .scene 资产本身 (`ScenePropGrid.Target = Scene`) | 编辑场景级属性（后处理、雾、GI 设置等） |

#### Editor Settings 关键字段

- **IsReadOnly**：勾选后禁用所有改动，用于"看图不动手"
- **AssetName**：当前 .scene 的资产路径（只读 chip）
- **RenderPolicy**：当前视口使用的 `TtRenderPolicy` 类型（例如 `EngineNS.Graphics.Pipeline.TtDeferred`）

> 多选时 `NodeDetails` 显示 PropertyGrid 的多对象交集编辑模式，共同字段可批量改。

### 4.3 Camera Settings（C 按钮弹出）

| 字段 | 说明 |
|---|---|
| **KeyMove** | WASD 键的相机移动速度（米/秒） |
| **WheelMove** | 滚轮单次推进的距离 |
| **RotSpeed** | Alt+左键旋转的角速度 |
| **ZNear / ZFar** | 视锥的近 / 远裁剪面，影响深度精度 |
| **X / Y / Z** | 相机当前世界坐标（可直接输入跳转） |

---

## 5. 地形编辑（Terrain Brush）

对 CDLOD 地形（`TtTerrainNode`）做视口内的笔刷编辑，分**两个通道**：

| 通道 | 写入对象 | 落盘 | 说明 |
|---|---|---|---|
| **Height** | `SourceHeightMap`（float 高度） | `.thl` 稀疏 **delta** | 雕高低起伏，见 [§5.4](#54-四种笔刷)~[§5.7](#57-保存与持久化thl-覆盖层) |
| **Material** | `SourceMaterialIdMap`（byte 材质 ID） | `.tml` 稀疏 **绝对值** | 刷地表材质，见 [§5.8](#58-材质-id-通道) |

两个通道**共用同一套拾取、圆环、Radius 与 Falloff**（视口里就一个圆环，分开存只会让手感跟不上），其余参数各自独立。编辑结果都以**相对 PGC 基底的稀疏覆盖层**形式保存，不修改 PGC 图表、不污染 `.trlvl` 基底缓存。

### 5.1 前提条件

两个通道各有一个独立的可编辑判定：

| 通道 | 判定 | 需同时满足 |
|---|---|---|
| Height | `UTerrainLevelData.IsEditable` | **PlayMode == Editor**（CPU 侧 `SourceHeightMap` 约 4MB / 已加载 level，只在编辑器态常驻，避免运行时内存回归）+ **level 已流式加载**（`SourceHeightMap.SuperPixels` 与 `HeightMap` 都在，卸载后只剩空壳） |
| Material | `UTerrainLevelData.IsMaterialIdEditable` | 同上两条 + **PGC 图表里有 `MatIdMapping` 节点**（没有则地形根本没有材质 ID 图，`MaterialIdArray` 为空） |

不满足时左键落笔会静默无反应（不报错、不卡）。用 MCP `terrain_get_info` 可以直接看到 `PlayMode` / `sourcePixelsAlive` / `materialIdPixelsAlive` / `materialCount` 定位原因。

### 5.2 进入 / 退出雕刻模式

- **进入**：工具栏 `Mode:` 下拉 → 选 `TtTerrainEditorInteractiveMode`
- **退出**：Terrain Brush 面板左上的 **Stop Sculpting** 按钮（切回默认的 `TtSceneEditorInteractiveMode`），或从 Mode 下拉里选其它模式

雕刻模式下只有**左键**被接管：落笔期间完全不走基类事件，因此既不会触发 hitproxy 选中，也不会被 TtAxis 抢走拖动。其余事件（Alt+左键转视角、中键平移、滚轮推进、WASD）原样交回基类。

> 高度通道进入模式时会对碰到的 level 开 `BeginEditSession`（临时拉开高度纹理的编码 headroom，保证拖动中的局部上传始终有效），离开模式时逐个 `EndEditSession` 收回真实范围并重算。会话期间高度编码精度略低是预期行为。**材质通道没有会话这一层**（ID 图是 R8 原值，没有 min/max 编码基准可冻结）。

### 5.3 Terrain Brush 面板

面板停靠在右上、与 Outliner 同一 tab 组，**仅当前交互模式是地形笔刷时才绘制**，切进模式的那一帧会自动抢焦点（不需要手动点 tab）。切出模式后 tab 直接消失。

顶部的 **Channel** 下拉切通道，下方控件随之整组替换：

| 控件 | 取值范围 | 说明 |
|---|---|---|
| **Stop Sculpting** | — | 切回默认选择模式；右侧提示当前可用操作 |
| **Channel** | Height / Material | 编辑通道。**切通道等于抬手**：半段拖动会先被封口入 undo 栈 |
| **Tool** | Raise / Lower / Smooth / Flatten | 仅 Height 通道。笔刷类型，见 §5.4 |
| **Material** | 材质列表 | 仅 Material 通道。`[下标] 漫反射贴图名`，单击选中；列表为空时提示"没有 MatIdMapping 节点" |
| **Radius** | 1 ~ 200 | **两通道共用**。笔刷半径，**世界单位（米）**，不是 texel 数 |
| **Strength** | Raise/Lower: 0 ~ 10<br>Smooth/Flatten: 0 ~ 1 | 仅 Height 通道。两种语义量级完全不同，**分开存**，切工具不会把对方的值带过来（hover 有 tooltip 说明当前语义） |
| **Falloff** | 0 ~ 1 | **两通道共用**。Height 下是边缘软化比例（0 = 硬边，1 = 从圆心就开始衰减的反向 smoothstep）；Material 下控制散点抖动带宽度，见 §5.8 |
| **Target** + **Pick** | -500 ~ 500 | 仅 Height 通道的 Flatten 时出现。Pick 把鼠标当前所指位置的高度填进 Target |
| 状态行 | — | Height/Raise/Lower 下提示"按住 Shift 反向落笔"；Material 下提示"按住 Shift 擦除回基底"；按下时切成进行中的描述 |
| `Cursor: (x, y, z)` | — | 当前鼠标所指地表的世界坐标，未命中时显示 `--` |

### 5.4 四种笔刷

| Tool | 公式（每个 texel） | 说明 |
|---|---|---|
| **Raise** | `h += Strength * weight` | 抬高。Strength 是高度增量（米） |
| **Lower** | `h -= Strength * weight` | 下沉。同样用**正** Strength |
| **Smooth** | `h = Lerp(h, avg, Clamp(Strength * weight, 0, 1))` | `avg` 取自**落笔前的快照**，不是边扫边改的当前值——否则平滑结果会沿扫描方向漂移 |
| **Flatten** | `h = Lerp(h, TargetHeight, Clamp(Strength * weight, 0, 1))` | 拉平到 Target，配 Pick 按钮取基准高度很好用 |

`weight` 由 `Falloff` 控制：圆心为 1，向外按反向 smoothstep 衰减到 0。

**Shift 反向**：Strength 滑条只给正值（负的 "Raise 强度" 读起来很反直觉，而 Smooth/Flatten 的 Strength 是 0~1 权重，负值无语义），所以**按住 Shift 落笔即 Raise↔Lower 互换**（地形类笔刷的通行约定）。Smooth / Flatten 本身无"方向"可言，不反。

> 反向标记在**鼠标按下的那一瞬间锁定**，不跟随拖动中 Shift 的按下 / 抬起——否则一笔里会出现一半抬一半压的鬼影。

### 5.5 笔刷圆环

视口内会画一个贴合地形起伏的圆环（沿圆周 48 段重新采高度），**颜色就是"落笔会发生什么"的预告**，不看面板也能判断：

| 颜色 | 含义 |
|---|---|
| **黄色** | Height 通道本笔会抬高（Raise / Smooth / Flatten） |
| **青色** | Height 通道本笔会下沉（Lower，含 Shift 反向后的 Raise） |
| **洋红** | Material 通道本笔会刷上选中材质 |
| **白色** | Material 通道本笔会擦除回基底材质（Shift） |

颜色在**没落笔时就跟随实时 Shift 状态**。相机背面的圆周采样点会自动断线，不会拉出横贯屏幕的假线段。

拾取走的是 `SourceHeightMap` 上的 **ray marching + 二分细化**（步长一个 texel，8 次二分到亚 texel），不依赖物理引擎，所以即使抬手前物理还没重建也能精确拾到最新地表。

### 5.6 Undo / Redo

- **粒度：一次拖动 = 一步**。拖动中每帧会落好几笔，逐笔入栈会让 Ctrl+Z 变成一次只退一个像素点的噩梦
- 命令名为 `Terrain Raise` / `Terrain Lower` / `Terrain Smooth` / `Terrain Flatten`，直接进 **History 面板** 与工具栏 Undo/Redo（与节点编辑共用同一个历史栈），Shift 反向的一笔会写成实际生效的 `Terrain Lower`
- Undo/Redo 走 `SetHeightBlock`，内部会连带 GPU 局部上传 + 法线重算 + patch AABB + physx heightfield 重建，所以回退后画面与物理是一致的
- **切通道等于抬手**：切 Channel 之前会先把半段拖动封口入栈，不会丢掉那段编辑

> **一条命令只描述一个 level 的一块矩形**。一次拖动如果跨了 level 边界，会自动拆成多条命令，此时需要按多次 Ctrl+Z 才能完全回退。
>
> level 被流式卸载后的旧命令会静默跳过（`IsEditable == false`）：重新加载时会从覆盖层重建，结果与命令的目标状态一致。

### 5.7 保存与持久化（.thl 覆盖层）

所有写入 `SourceHeightMap` 的路径都会经过 `AccumulateOverlayDelta` 累积一份**相对 PGC 基底的 delta**。点工具栏 **Save** 时，`TtTerrainNode.OnSaveNodeExtraData` 把它们落到 scene 目录下（不进资产系统、没有 `.ameta`）：

```
xxx.scene/
  terrainheight/
    {TerrainNodeId}/
      levellist.txt        # 每行一条 {levelX}_{levelZ}#{SHA256}
      {levelX}_{levelZ}.thl  # TtXndHolder(Desc + Delta)
```

- **按需懒加载**：level 构建时先查 `levellist.txt` 判断这个 level 到底有没有 delta，有才去读 `.thl`。100×100 个 level 全读一遍是不可接受的
- **内容哈希去重**：内容未变且文件仍在时跳过写盘，反复 Save 不会无意义刷盘
- **未碰过的 level 不会丢**：盘上已存的 level（包括本次没加载进内存的）hash 会原样带回 `levellist.txt`
- **叠加时机**：`UTerrainLevelData.ApplyHeightOverlayIfAny` 在 `CreateFromBuffer` 建立任何派生数据**之前**把 delta 叠到高度副本上并同步重算法线。它返回的是**副本**：原地叠加会把 delta 烤进 `.trlvl` 基底缓存，下次加载再叠一次 → 高度翻倍
- 覆盖层在**运行时也生效**（叠加发生在 level 构建期，与能不能编辑无关），所以 PIE / 打包后看到的地形就是编辑完的地形

### 5.8 材质 ID 通道

把 Channel 切到 **Material** 后，笔刷写的是**地表材质 ID 图**（`SourceMaterialIdMap`）而不是高度。

#### 5.8.1 材质 ID 是什么

地形的材质图是一张 `R8G8B8A8` 纹理，但**只有 R 通道有意义，值就是 `MaterialIdArray` 的整数下标**（由 PGC 图表的 `MatIdMapping` 节点产出）。shader 侧（`TerrainCDLOD.cginc` 的 `GetTerrainDiffuse` / `GetTerrainNormal`）取当前 texel 与 +X / +Y / +XY 三个邻居的 ID，各采一次纹理数组后按小数部分双线性 lerp **颜色**——所以笔刷只需要写整数 ID，不需要 weight 纹理，也没有改任何 shader。

#### 5.8.2 选材质

面板列出 `MaterialIdArray` 的全部条目，形如 `[2] grass_01`（下标 + 漫反射贴图的纯名），单击选中。地形没配 `MatIdMapping` 节点时列表为空，面板会直接提示，此时落笔无效。

#### 5.8.3 软边为什么是散点（抖动覆盖）

ID 是整数下标，**两个材质之间没有"中间值"可插值**。所以 `Falloff` 算出的权重被当成**该 texel 写不写的概率**：

- 圆心附近权重 1 → 必写
- 边缘权重 0.3 → 约 30% 的 texel 被写，其余保留原材质 → 形成散点过渡带
- 这些散点再经 shader 的 4-tap 颜色混合，视觉上就是一条自然的过渡边

抖动哈希**只依赖 texel 坐标**（不带随机数状态），所以反复刷同一处结果稳定，不会越刷越糊。`Falloff` 越大，散点带越宽。

#### 5.8.4 Shift 擦除

材质通道**只有一个动作**（刷），没有 Tool 下拉，也没有 Strength（整数下标不存在"刷多少"）。**按住 Shift 落笔 = 擦除**：把 texel 写回 PGC 基底 ID（首次擦除时会从 `.trlvl` 基底重建一份 `mBaseMaterialIdMap`）。与高度侧一致，反向标记在**按下瞬间锁定**，不跟随拖动中的 Shift 变化。

#### 5.8.5 Undo / Redo

- 粒度同样是**一次拖动 = 一步**，与高度共用同一个历史栈
- 命令名为 `Terrain Paint {贴图名}` / `Terrain Erase Material`
- Undo/Redo 走 `SetMaterialIdBlock`，只做局部上传 + RVT 标脏——**没有法线重算、没有物理重建**（ID 图不参与碰撞），所以抬手不会像高度那样卡一下
- 跨 level 边界同样会拆成多条命令

#### 5.8.6 保存与持久化（.tml 覆盖层）

与高度侧同构，但**存绝对 ID 而不是 delta**（ID 没有可加性）：

```
xxx.scene/
  terrainmaterialid/
    {TerrainNodeId}/
      levellist.txt        # 每行一条 {levelX}_{levelZ}#{SHA256}
      {levelX}_{levelZ}.tml  # TtXndHolder(Desc + Ids + Mask)
```

`Mask` 是逐 texel 的"这个点被手绘过吗"位图——没有它就区分不了"刷成了 0 号材质"和"没刷过"。按需懒加载、内容哈希去重、未加载 level 的 hash 原样带回，规则与 `.thl` 完全一致；`ApplyMaterialIdOverlayIfAny` 同样返回**副本**（传进来的 ID 缓冲随后会被写进 `.trlvl` 基底缓存，原地改会把手绘烤进基底）。

#### 5.8.7 MCP 工具

| 工具 | 用途 |
|---|---|
| `terrain_list_materials` | 列出 `MaterialIdArray`（下标 / 漫反射 / 法线 / TransitionRange），拿到可用的 `materialId` 范围 |
| `terrain_paint_material` | 代码态刷一笔，`radius` 是**世界单位**；返回落笔前后的中心 ID 与影响包围盒 |
| `terrain_dump_material_region` | 导出一块区域的 ID **直方图 + 降采样网格**（ID 是分类量，min/max/mean 没有意义） |

#### 5.8.8 限制

- **不影响植被 / 草**：`Plants` / `Grasses` 是在 level 构建期按基底 ID 撒的，手绘材质**不会**重新生成它们
- **`TransitionRange` 等材质参数不由笔刷改**：笔刷只写 ID，材质本身的参数仍在 PGC / 节点属性里配
- **换了 PGC 图表后手绘仍在**：覆盖层是按 texel 坐标存的绝对 ID，与基底无关。若新图表的 `MaterialIdArray` 顺序变了，旧的手绘 ID 会指向不同材质
- 其余（只作用于圆心所在 level、节点 Id 决定目录名）与高度侧相同，见 §5.10

### 5.9 与 MCP 工具联动

`titan-engine-mcp` 提供了一组地形工具，与面板操作共用同一套数据层，适合自动化验证：

| 工具 | 用途 |
|---|---|
| `terrain_get_info` | 查地形节点 / level 状态（`PlayMode`、`sourcePixelsAlive`、`materialIdPixelsAlive`、`materialCount`、`brushHint` 等），排查"笔刷不生效"的第一站 |
| `terrain_sample_height` | 采单点高度，**分开返回** CPU 侧高度与物理高度，能区分"数据没改"与"物理没重建" |
| `terrain_apply_brush` | 代码态落笔（高度），`radius` 是**世界单位** |
| `terrain_dump_height_region` | 导出一块区域的高度，交叉验证笔刷效果 |
| `terrain_edit_session` | 手动开 / 关编辑会话（仅高度通道有会话） |
| `terrain_list_materials` / `terrain_paint_material` / `terrain_dump_material_region` | 材质通道三件套，见 §5.8.7 |

> MCP 路径**不进 undo 栈**（没有 `HistoryHost`）。用 MCP 改完的地形无法用 Ctrl+Z 回退，只能靠 Reload 或反向再刷一遍。

### 5.10 限制与注意

- **笔刷只作用于圆心所在的 level**：跃过 level 边界的那部分不会被刷到，沿接缝雕刻会看到硬边（接缝融合属后续任务）
- **抬手时会有一下卡顿**（仅高度通道）：拖动中只做 GPU 局部上传，physx heightfield 到抬手才**整层重建**（控开销的有意取舍）
- **编辑器不经切模式直接关闭时**，已开的 `EditSession` 不会走 `EndEditSession`（下次加载从覆盖层重建，不影响数据正确性）
- **只有高度与材质 ID 两个通道**：水高等其它通道目前不保留 CPU 副本
- **节点 Id 变了会找不到覆盖层**：`.thl` / `.tml` 的目录名就是 `TerrainNodeId`，重建地形节点前先把目录名对上

---

## 6. 底部 Content Browser

整个窗口下半部分嵌入 Content Browser（与独立的 ContentBrowser 编辑器同源）：

- **左侧目录树**：工程下所有资产文件夹（`Game > tutorials > gi > cornell` 等）
- **中间面包屑**：当前路径
- **右侧资产卡片**：缩略图 + 名称，双击 `.scene` 切到该场景，双击 `.uminstance` / `.material` 等会打开对应编辑器
- **Search Assets**：资产模糊搜索

详见 [ContentBrowser 文档](ContentBrowser.md)。

---

## 7. 主菜单（窗口顶部）

| 菜单 | 项 | 作用 |
|---|---|---|
| **File** | New / Open / Save / Save As / Exit | 工程级文件操作 |
| **Edit** | Undo / Redo / Preferences | 全局编辑操作 |
| **Windows** | 各面板的显示 / 隐藏（Outliner、NodeDetails、Terrain Brush、Content Browser、Profilers ...） | 重置 / 自定义工作区布局 |
| **View** | DisableShadow / DisableAO / DisableHDR / DisablePointLight | 临时关闭某些渲染特性，便于排查问题 |
| **Illumination** | VoxelDebugger / ResetVoxels | GI / 体素相关调试入口（具体行为依 RenderPolicy） |

---

## 8. 快捷键速查

| 分类 | 快捷键 | 功能 |
|---|---|---|
| 选择 | **左键单击** | 选中节点 |
| 选择 | **Ctrl + 左键单击** | 多选切换 |
| 选择 | **空白处单击** | 取消选中 |
| 编辑 | **左键拖 Move/Rot/Scale 轴** | 平移 / 旋转 / 缩放 |
| 编辑 | **Ctrl + 左键拖 Move 轴** | 复制并拖动（新节点 `_Copy` / `_Copy1` / `_Copy2` ...） |
| 编辑 | **Delete** | 删除选中节点 |
| 编辑 | **Ctrl + Z / Ctrl + Y** | Undo / Redo |
| 地形 | **左键拖** | 雕刻模式下落笔（不会选中节点 / 拖 Axis） |
| 地形 | **Shift + 左键拖** | Height 通道：反向落笔（Raise↔Lower）；Material 通道：擦除回基底材质。两者都在**按下瞬间锁定** |
| 地形 | **Alt + 左键拖** | 仍然是相机轨道旋转，不会落笔 |
| 视图 | **Alt + 左键拖** | 轨道旋转相机 |
| 视图 | **中键拖** | 平移相机 |
| 视图 | **滚轮** | 沿视线推进 / 后退（LookAt 同步） |
| 视图 | **W / A / S / D** | 飞行式移动 |
| 视图 | **F** | Frame 选中节点到视野 |
| 文件 | **Ctrl + S** | Save |
| 调试 | **F11** | RenderDoc 抓帧（捕获当前帧供 GPU 调试分析） |

---

## 9. 高级用法

### 9.1 Ctrl+Drag 节点复制

- **触发条件**：当前 Axis 在 Move 模式 + 至少有一个选中节点 + 鼠标 hover 在某条 Move 轴上 + 按住 Ctrl 后按下左键
- **复制规则**：
  - 通过 `TtNode.CloneNode` 同步克隆（递归子节点）
  - 新节点挂在源节点的 **同一 Parent** 下
  - `NodeName` 自动加 `_Copy` 后缀（重名时追加 `_Copy1` / `_Copy2` ...），反复复制不会出现 `red_Copy_Copy_Copy`
  - 自动分配 **新 HitProxy ProxyId**，原节点的拾取不受影响
- **多选支持**：选了 N 个节点会一次性复制 N 份，选中状态切到全部副本，Axis 直接拖整体

> 实现见 `TtSceneEditorInteractiveMode.OnEvent / TryStartClonedDrag / MakeUniqueNodeName`。

### 9.2 切换交互模式（Mode 下拉框）

工具栏 Mode 下拉框列出当前视口可用的所有 `TtInteractiveMode`：

- `TtSceneEditorInteractiveMode`（默认）：SceneEditor 专用，含 Ctrl+Drag 复制
- `TtWorldViewportInteractiveMode`：基础相机 + Axis，不含复制
- `TtTerrainEditorInteractiveMode`：地形笔刷（高度 / 材质双通道），左键落笔（见 [§5](#5-地形编辑terrain-brush)）

> 添加自定义 Mode：实现一个继承 `TtWorldViewportInteractiveMode` 的类，在 `TtSceneEditorViewport` 静态构造函数里 `InteractiveModeManager.RegisterMode<TtMyMode>()`，启动后会自动出现在下拉框。

### 9.3 RenderPolicy 切换

- 工具栏面板的 **RPolicy** 按钮 / `Editor Settings` 的 `RenderPolicy` 字段，都可以切换当前视口使用的 RenderPolicy 类型（Deferred / Forward / 自定义）
- 切换后视口重建 RenderGraph，适合对比同一场景在不同管线下的表现差异

### 9.4 Snap 吸附（SM / SR / SS）

- `SM`：Move 拖动按此值整数倍 snap（默认 0.1m）
- `SR`：Rotate 拖动按此值整数倍 snap（默认 10°）
- `SS`：Scale 拖动按此值整数倍 snap（默认 0.1）
- 调小可获得连续操作，调大方便对齐网格

---

## 10. 常见问题 / 故障排查

| 问题 | 可能原因 | 解决 |
|---|---|---|
| **场景全紫色**（如截图所示） | 材质没找到 / Shader 编译失败 | 看 LogWatcher 报错，检查 .scene 引用的 .uminstance / .material 是否丢失 |
| **滚轮推不动相机** | LookAt 点没同步推进，推到 LookAt 处就停 | 确认 `CameralWheelMoveWithLookAt = true`（SceneEditor 默认开启） |
| **Ctrl+Drag 复制后点不中原节点** | HitProxy 共用了 ProxyId | 已通过 `cloned.HitproxyType = cloned.HitproxyType` 自赋值触发重新分配，确保使用最新版 `TtSceneEditorInteractiveMode` |
| **复制出的节点在 Outliner 都同名** | 历史版本未自动加后缀 | 已通过 `MakeUniqueNodeName` 自动追加 `_Copy[N]`，确保使用最新版 |
| **Mode 下拉框是空的** | 没有调用 `InteractiveModeManager.RegisterMode` | 检查 `TtSceneEditorViewport` 的静态构造函数 |
| **Save 不生效** | `IsReadOnly` 勾选了 | 在 `Editor Settings` 里取消勾选 |
| **F 键 Frame 不到位** | 节点 BoundVolume 为空或不正确 | 检查节点的 BoundVolume 计算，多见于自定义节点忘了实现包围体 |
| **切到地形雕刻模式但左键没反应** | 场景里没地形节点 / level 还没流式加载 / 当前不是 Editor PlayMode；**材质通道额外需要 PGC 有 `MatIdMapping` 节点** | 先把相机飞到地形上方等 level 加载；用 MCP `terrain_get_info` 看 `PlayMode` / `sourcePixelsAlive`（对应 `IsEditable`）/ `materialIdPixelsAlive` / `materialCount` |
| **找不到 Terrain Brush 面板** | 它只在地形雕刻模式下绘制 | 先从工具栏 `Mode:` 切到 `TtTerrainEditorInteractiveMode`；若曾用 X 关掉面板，从 Windows 菜单重新勾上 |
| **新面板飘成独立浮窗** | `cache/imgui.ini` 里存的旧布局优先于 DockBuilder 默认值 | 手动拖到 Outliner 标签栏停靠一次，之后会持久化 |
| **Strength 调不到负，地形压不下去** | 设计如此（负的抬高强度反直觉，Smooth/Flatten 的 Strength 是 0~1 权重） | 按住 **Shift** 反向落笔，或直接把 Tool 切到 Lower |
| **Ctrl+Z 只退了一半** | 本次拖动跨了 level 边界，一条命令只描述一个 level | 继续按 Ctrl+Z，每个 level 一步 |
| **抬手瞬间卡一下** | 抬手时重建整层 physx heightfield | 预期行为；拖动中只做 GPU 局部上传，不会卡 |
| **Save 后重开高度没了** | 覆盖层没落盘，或地形节点 Id 变了 | 查 `xxx.scene/terrainheight/{NodeId}/levellist.txt` 是否存在且目录名与当前节点 Id 一致 |
| **重开后地形高度翻倍** | delta 被烤进了 `.trlvl` 基底缓存，加载时又叠了一次 | 已由 `ApplyHeightOverlayIfAny` 返回副本修正；若遇到历史缓存，清掉 `cache` 里对应 `.trlvl` 重生成 |
| **刷完材质边缘有噪点** | 设计如此：ID 是整数下标，无法插值，软边靠**抖动覆盖**实现（见 §5.8.3） | 把 `Falloff` 调小得到硬且整齐的边，或调大把噪点带拉宽、让 shader 的 4-tap 混合把它揉开 |
| **刷了材质但草木没跟着变** | `Plants` / `Grasses` 在 level 构建期按**基底** ID 撒完就不再动了 | 预期行为。需要植被跟随手绘材质则得改 PGC 图表重生 level |
| **换了 PGC 后手绘材质还在 / 材质错了** | `.tml` 存的是按 texel 坐标的**绝对 ID**，与基底无关；新图表的 `MaterialIdArray` 顺序一变，旧 ID 就指向另一个材质 | 要么保持材质列表顺序稳定，要么删掉 `xxx.scene/terrainmaterialid/{NodeId}/` 重刷 |

---

## 11. 相关文档

- [ContentBrowser 资产浏览器](ContentBrowser.md)
- [MaterialEditor 材质编辑器](MaterialEditor.md)
- [MaterialedMeshEditor 带材质 Mesh 编辑器](MaterialedMeshEditor.md)
- [PIEController Play-In-Editor 控制器](PIEController.md)
- [RenderPolicyEditor 渲染管线编辑器](RendPolicyEditor.md)
- [GpuProfiler GPU 性能分析](GpuProfiler.md) / [CpuProfiler](CpuProfiler.md)
- [LogWatcher 日志窗口](LogWatcher.md)

---

> 本文档对应代码版本：见仓库当前 HEAD。UI 截图来自 `tutorials/gi/cornell/cornell.scene`。如新增了视口控件 / 交互模式 / 菜单项，请同步更新本文档对应章节。
