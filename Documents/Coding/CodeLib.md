# 常用代码
## 1.性能分析
```C#
[ThreadStatic]
private static Profiler.TimeScope mScopeChildren;
private static Profiler.TimeScope ScopeChildren
{
    get
    {
        if (mScopeChildren == null)
            mScopeChildren = new Profiler.TimeScope(typeof(TtWorld), nameof(GatherVisibleMeshes) + ".Children");
        return mScopeChildren;
    }
}
using (new Profiler.TimeScopeHelper(ScopeChildren))
{
	//do sth
}
```
## 2.插件开发
1.确保插件dll包含如下实现
TtPluginLoader是用来描述插件的
AssemblyEntry是用来描述DLL模块在TtTypeDesc的信息的，通常处理类型和Meta信息
```C#
namespace EngineNS.Plugins.你的插件名
{
    public class TtPluginLoader
    {
        public static UGameItemPlugin? mPluginObject = new UGameItemPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
}

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGameServerAssemblyDesc : TtAssemblyDesc
        {
            public UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:你的插件名 AssemblyDesc Created");
            }
            ~UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:你的插件名 AssemblyDesc Destroyed");
            }
            public override string Name { get => "你的插件名"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGameServerAssemblyDesc AssmblyDesc = new UGameServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

```
2.插件编译结果路径
binaries\Plugins\Debug\net7.0
3.在插件目录还要添加插件同名.plugin文件，内容大致如下：
```JSON
{
  "Enable": true,
  "LoadOnInit": true,
  "Platforms": [
    "PLTF_Windows"
  ],
  "Dependencies": [
    "Survivor"
  ]
}
```
Plugins目录下CopyPlugins.bat在修改*.plugin后目前需要手工执行，刷新到插件目录
## 3.Native Bricks开发
1.目前需要在Base/BaseHead.h里面根据平台添加类似 **#define HasModule_NextRHI** 
2.这个用来确保生成胶水代码参与编译构建，否则会发生C#调用C++找不到函数
## 4.RPC函数
1.为函数添加TtRpcMethod属性
2.最后一个参数必须为UCallContext context，可以从中取出Connect
```C#
		[TtRpcMethod(Index = RpcIndexStart + 2)]
		public async System.Threading.Tasks.Task<Bricks.Network.FNetworkPoint> SelectGateway(string user, Guid sessionId, UCallContext context)
		{
			ServerCommon.UServerBase slt = null;
			long Payload = long.MaxValue;
            foreach (var i in GateServers)
			{
				if (i == null)
					continue;
				var tmp = i.ClientManager.FindClient(sessionId);
                if (tmp == null)
                {
					slt = i;
					break;
				}
                if (i.Payload < Payload)
                {
                    Payload = i.Payload;
					slt = i;
                }
            }
			if (slt == null)
			{
				return null;
			}
			var ok = await GateServer.UGateServer.WaitSession(sessionId, user, 0, slt.Connect);
			if (ok == false)
                return null;
            return slt.ListenPoint;
		}
```
## 5.自动同步对象
- 所有IRpcHost派生类都可以作为自动同步对象
- 利用TtEngine.Instance.RpcModule.RpcManager.RpcPropertyDataManager.RegisterHost(host)注册一个自动同步对象
- 添加属性，并用TtRpcProperty修饰
- TtRpcPropertyDataManager在Tick函数中会自动同步这些属性
- 示例
```C#
	[TtRpcClassAttribute(RunTarget = ERunTarget.None, Executer = EExecuter.PropertyData, CallerInClass = true)]
    public partial class TtRpcPropertyDataManager : AuxRpcHost<TtRpcPropertyDataManager>
    {
        [TtRpcProperty]
        public int TestSync1 { get; set; } = 1;
    }
```
## 6.增加一个材质编辑器可调用函数节点
- 在HLSLMethod增加一个静态函数
- - 参数必须为Vector2/3/4和系统变量SamplerState、Texture2D
- 增加Rtti.Meta属性修饰
- 如果使用UserCallNode属性，可以自定义材质编辑器中的Node
- 使用ContextMenu修饰在菜单中出现的位置
- 下面是示例
 ```C#
	public partial class HLSLMethod
    {
        [Rtti.Meta("")]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("samplelevel2d", "Sample\\Level2D", UMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta("")]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta("")]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta("")]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta("")]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta("")]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta("")]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
	}
```

## 7.增加一个 Mesh Modifier / TtMdfQueue 定制顶点变换

> 强制约束见 `CodingGuidelines.md` §7 (执行顺序 / vsOut 写入 / 蒙皮类必须派生 /
> per-SubMesh 状态 / BindSRV 大小写)。下面只给可直接照抄的骨架。

一个完整的顶点变换特性包含三个件:

| 件 | 位置 | 职责 |
|---|---|---|
| `.cginc` | `enginecontent/Shaders/Modifier/Xxx.cginc` | VS 端的 `DoXxxModifierVS` |
| modifier | `CSharpCode/Grapics/Mesh/Modifier/XxxModifier.cs` | 声明顶点流 / 绑定 per-instance 资源 |
| MdfQueue | `CSharpCode/Grapics/Mesh/MdfXxxMesh.cs` | 组合 modifier, 供资产与节点选型 |

### 7.1 modifier (参照 `Modifier/MorphModifier.cs`)

```C#
public class TtXxxModifier : Pipeline.Shader.IMeshModifier
{
    // 注意: 无需 native 对应物。既有 modifier 都继承 AuxPtrType<IXxxModifier>, 但 C# 侧
    // 从不访问 modifier 的 mCoreObject, 纯 C# 实现完全可行。
    public string ModifierNameVS { get => "DoXxxModifierVS"; }   // 与 cginc 里函数名一致
    public string ModifierNamePS { get => null; }
    public RName SourceName => RName.GetRName("shaders/modifier/Xxx.cginc", RName.ERNameType.Engine);

    public NxRHI.EVertexStreamType[] GetNeedStreams()
        => new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position };
    public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs() => null;
    public unsafe NxRHI.FShaderCode* GetHLSLCode(string inc, string oriInc) => (NxRHI.FShaderCode*)0;
    public string GetUniqueText() => "";

    public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
    {
        // 按 SubMesh 建 per-instance 状态 (CodingGuidelines §7.5)
        // materialMesh.SubMeshes[i].Mesh 是那个 SubMesh 的 TtMeshPrimitives
    }
    public void OnBuildDrawCall(...) { }

    public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue,
        NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall,
        Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
    {
        var state = mStates[atom.SubMesh.MeshIndex];        // 选对应那份
        state.Buffer.Flush2GPU(cmd);                        // 首帧在这里创建 Srv, 必须先于 Bind
        drawcall.BindSRV("XxxBuffer", state.Buffer.Srv);    // TtGraphicDraw 是 BindSRV (§7.6)
    }
    public void Dispose() { /* 释放自建 buffer */ }
}
```

per-instance 的逐顶点 buffer 用 `Graphics.Pipeline.TtCpu2GpuBuffer<T>` (见 §1.5.1 与
`CodingGuidelines.md` §1.6), `Initialize(BFT_SRV)` + `SetSize(vertexCount)` 一次定长,
之后不重建, 绑定的 `Srv` 就不会被后续 flush 失效。

### 7.2 cginc (参照 `Modifier/MorphModifier.cginc`)

```hlsl
#ifndef _XxxModifier_cginc_
#define _XxxModifier_cginc_

#include "../Inc/GlobalDefine.cginc"      // 用到 [TtShaderDefine] 注入类型时必需 (§3.3)

StructuredBuffer<FXxxData> XxxBuffer;     // 名字跟 C# 端 BindSRV 的字符串一致

void DoXxxModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
    FXxxData d = XxxBuffer[vert.vVertexID];   // vVertexID 无条件可用, 不需额外顶点流
    vert.vPosition.xyz  += d.Offset;          // 给后续 modifier
    vsOut.vPosition.xyz  = vert.vPosition.xyz; // 自己也得写 (§7.1, 否则可能静默无效)
}
#endif
```

### 7.3 MdfQueue (参照 `MdfMorphMesh.cs`)

```C#
// 不涉及蒙皮: 直接用泛型组合, T0 先于 T1 执行
[Rtti.Meta("")]
public class TtMdfXxxMesh : TtMdfQueue2<Mesh.Modifier.TtXxxModifier, Mesh.Modifier.TtStaticModifier> { }

// 涉及蒙皮: 必须派生自 TtMdfSkinMesh (§7.4), 否则骨骼动画会整体失效
[Rtti.Meta("")]
public class TtMdfSkinXxxMesh : TtMdfSkinMesh
{
    public TtMdfSkinXxxMesh()
    {
        Modifiers.Insert(0, new Mesh.Modifier.TtXxxModifier());  // 插到 skin 之前
        UpdateShaderCode();                                      // 必须重新生成 (§7.2)
    }
    public override void CopyFrom(TtMdfQueueBase mdf)
    {
        base.CopyFrom(mdf);   // 基类搬 PerSkinMeshCBuffer
        // 再搬自己的运行时状态, 否则切 MdfQueue 类型时状态丢失
    }
}
```

❗ 三个新建 `.cs` 都要登记到 `CSharpCode/Grapics/Mesh/Mesh.projitems`
(不是 `Grapics.projitems`, 该文件不存在; 规则见 `CodingGuidelines.md` §8)。

选型提示: 资产/节点侧通过 `TtMeshNode.TtMeshNodeData.MdfQueueType` 指定类型字符串;
编辑器预览可参照 `MeshPrimitiveEditor.Initialize_PreviewMaterialInstance` 里根据
"有无骨骼 / 有无 morph" 四路选 MdfQueue 的写法。

## 8.增加一个TtShadingEnv处理Shader总流程
- 参阅Graphics.Pipeline.Deferred.TtOpaqueShading
- 构造器中提供定制好的CodeName
- 在BeginPermutaion和UpdatePermutation中添加PushPermutation来增加Permutaion
- 通过Permuation对象的SetValue来设置当前Permutation
- 记得重载GetNeedStreams来指定需要的VertexBuffer
- 顺道关注一下RenderGraphNode，通常他负责最终使用这些ShadingEnv

## 9.加载一个资产
- 所有资产通过RName来唯一标识，而通过代码构造RName的唯一途径是RName.GetRName
- 绝大多数RName是通过序列化产生的，在RName属性上添加[Rtti.Meta("")]就可保证序列化
- 泛型RName的GetAsset<T>()负责从RName加载资产，T是资产类型
- RName由路径和类型组成，常见的类型有
  - Engine：引擎自带的资产，通常在EngineContent目录下
  - Game：游戏项目的资产，通常在Game/Content目录下
  - Cloud：云端资产，在本地会有缓存
```C#
var textureName = RName.GetRName("texture/checkboard.txpic", RName.ERNameType.Engine);
var texture = await textureName.GetAsset<NxRHI.TtSrView>();//异步加载
var texture1 = textureName.GetAsset<NxRHI.TtSrView>().GetResultUntilCompleted();//同步加载
```

## 10.增加一个Config配置文件
- 定义一个IO.IConfig接口的类
- 对该类添加[IO.TtConfig(Path="你的配置文件路径")]属性
- 将需要配置的属性增加[Rtti.Meta("")]属性
- 通过TtEngine.Instance.ConfigManager.GetConfig<TtCloudConfig>().CloudAssetUrlBase类似方法访问配置
```C#
    [IO.TtConfig(Path = "cloud.jscfg")]
    public class TtCloudConfig : IO.IConfig
    {
        [Rtti.Meta("")]
        public string CloudAssetUrlBase { get; set; } = "http://localhost:7000";
        [Rtti.Meta("")]
        public string WebApiUrlBase { get; set; } = "http://localhost:7000";
    }
```

## 11.让 C# struct / enum 自动生成对应的 HLSL 定义
- 给 C# `struct` 或 `enum` 加 `[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FXxx")]`,
  引擎在编译 shader 时会反射这个类型, 自动产生同名的 HLSL `struct FXxx { ... }` /
  `static const uint EXxx_Value = N;` 注入到对应 .cginc 的 define 段
- 这样 C# 端和 HLSL 端**只有一份字段声明**, 改 C# 字段时 HLSL 自动同步, 杜绝
  "字段对不齐 → cbuffer 偏移错位 → GPU 读垃圾" 这一类问题
- `TtShaderDefineAttribute` 可配置字段 (定义在 `CSharpCode/Editor/ShaderCompiler/ShaderCode.cs:15`):
  - `ShaderName`: 生成到 HLSL 的 struct/enum 名。**强制规约**: C# 端 struct 名
    必须以 `F` 开头、enum 名必须以 `E` 开头, `ShaderName` **保持和 C# 端完全
    一致** (例如 `FGpuBvhNode` / `EParticleFlags`); 不允许出现裸名 `GpuBvhNode`
    这种, 否则一眼分不清是 class 还是 struct, 而且和引擎现有 `FShaderBinder` /
    `FFenceDesc` / `FMeshlet` / `FAdvShadowNodeData` / `FSubResourceFootPrint`
    等命名风格不统一。详见 CodingGuidelines.md §3.1
  - `Flags`: `EShaderDefine.HasGet | HasSet`, 控制是否生成 getter/setter
  - `Condition`: 条件宏 (例如某个 permutation 关闭时不生成)
  - `Semantic` / `Binder`: VS input / cbuffer binder 时用
  - `Order`: 字段排序优先级
- HLSL 端**不要再手写**同名 struct, 直接 `#include` 对应 cginc 后用即可
- 如果要让 C# struct 的内存布局 (size / alignment / 字段 offset) 严格对齐 GPU,
  可叠加 `[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, Pack = N)]`:
  - cbuffer 用的 struct 必须 `Pack = 16` (HLSL cbuffer 16 字节边界对齐)
  - structured buffer 一般 `Pack = 4` 即可, 大多数情况下不写也没问题, 引擎按字段
    自然对齐生成

```C#
// 示例 1: structured buffer 的 struct (无 [StructLayout] 也可以工作)
// 范例: CSharpCode/Bricks/AdvanceShadow/QNode.cs:11
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowNodeData")]
public struct FAdvShadowNodeData
{
    public Matrix mShadowMatrix;
    public int mChildIndex00;
    public int mChildIndex01;
    public int mChildIndex10;
    public int mChildIndex11;
    public int mNodeType;
    public int mPageIndex;
    public float mZNear;
    public float mZFar;
}

// 示例 2: cbuffer 用的 struct, 必须 Pack = 16
// 范例: CSharpCode/Bricks/AdvanceShadow/AdvanceShadowShading.cs:15
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowLayerData")]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)] // align 16 for cbv
public struct FAdvShadowLayerData
{
    public Vector2i mLayerStartAndSide;
    public Vector2 mLayerGridSize;
}

// 示例 3: enum 同样适用, HLSL 端会得到一组对应的 static const 常量
// 实证范例: CSharpCode/Bricks/Particle/Emitter.cs:10 (EParticleFlags)
//          CSharpCode/Bricks/Particle/Emitter.cs:93 (EParticleEmitterStyles)
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EParticleFlags")]
public enum EParticleFlags : uint
{
    Alive = 0,
    Dead = 1,
}

// 示例 4: 字段级 [TtShaderDefine] 可以为 HLSL 端字段单独命名
// 实证范例: CSharpCode/Bricks/GpuDriven/Cluster.cs:282 (FMeshlet)
[EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FMeshlet")]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
public struct FMeshlet
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VertexOffset")]
    public uint VertexOffset;
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "TriangleOffset")]
    public uint TriangleOffset;
}
```

#### 配套规约 (硬性约束)
- **C# 端命名必须 `F` 前缀 (struct) / `E` 前缀 (enum)**, `ShaderName` 与 C# 端
  名字保持完全一致。详见 CodingGuidelines.md §3.1。反例:
  `[TtShaderDefine(ShaderName = "GpuBvhNode")] public struct GpuBvhNode` —— 没
  前缀, 不允许; 正例: `[TtShaderDefine(ShaderName = "FGpuBvhNode")] public
  struct FGpuBvhNode`
- **`[TtShaderDefine]` struct 内的字段名必须以 `m` 开头** (例如 `mBoxMin` /
  `mHitProxyId`), 引擎在 `CSharpCode/Editor/ShaderCompiler/ShaderCode.cs:384`
  里 `Debug.Assert(i.Name[0] == 'm')`, 编译期会**剥掉首字母 `m`** 生成 HLSL 端
  字段名 (`mBoxMin` -> `BoxMin`); 不带 `m` 前缀会触发断言、Debug 构建直接挂掉。
  例外: 字段自带 `[TtShaderDefine(ShaderName = "...")]` 标注时, C# 字段名可
  任意 (引擎用 attribute 里的 `ShaderName` 而不是字段名), 例如 `FMeshlet` 的
  `VertexOffset` / `TriangleOffset`。详见 CodingGuidelines.md §3.2
- **任何引用 `[TtShaderDefine]` 自动生成 struct / enum 的 shader, 都必须在
  顶部 `#include "../../Inc/GlobalDefine.cginc"`** (相对路径按文件深度调整)。
  这是引擎反射注入 ENGINE_PREPROCESSORTS_INC 块的唯一入口, 不 include 等于
  这些 struct 在 shader 编译单元里**完全不存在**, 报错形式是 `error X3000:
  syntax error: unexpected token 'FXxx'` + 一连串看似无关的下游 X3004
  (`undeclared identifier`)。即使**不用任何 cbuffer / 内置全局**, 只要 shader
  里出现了 `[TtShaderDefine]` 标注的类型 (无论作为变量类型、StructuredBuffer
  模板参数还是函数参数), 就必须 include。`.compute` 和 `.cginc` **两端都要
  include** (即使 .cginc 已 include, .compute 入口文件偶尔会被反射 pass 单独
  预处理, 双重 include 由 `#ifndef` 守卫去重)。详见 CodingGuidelines.md §3.3
- HLSL 端**不要再写**和 C# 同名的 `struct` / `enum` / `static const` 定义,
  否则会和引擎自动生成的版本产生重复定义错误
- 改 C# 字段顺序 / 类型时, **必须** 重新触发 shader 编译 (改一下对应 .compute /
  .cginc 文件保存触发, 或重启编辑器), 让自动生成的 HLSL 同步更新; 仅改 C# 不
  动 shader 文件, 引擎可能复用旧的编译缓存
- HLSL 内置类型映射: `Vector2/3/4 → float2/3/4`, `Vector2i/3i/4i → int2/3/4`,
  `Vector2ui → uint2`, `Matrix → float4x4`, `int → int`, `uint → uint`,
  `float → float`. 不在这个清单里的类型 (例如 `DVector3` / `Color4f`) 不要直接放
  进 `[TtShaderDefine]` 标注的 struct, 引擎不一定能识别
- 引擎的 HLSL 反射在 `CSharpCode/Editor/ShaderCompiler/ShaderCode.cs:260+` 和
  `CSharpCode/Bricks/CodeBuilder/Backends/HLSLBackend.cs:465+`, 排查问题时直接
  跳到这两处看反射逻辑

## 12.提交 GPU 命令的两条路径: `TtRenderPolicy.QueueCmd` vs `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd`

引擎对外暴露两个 `QueueCmd` 入口, **签名一致 (6 参)**, 但执行时机和归属队列不同。
选错会出现 "命令晚一帧执行" / "命令脱离 RenderGraph 时序" / "Profiler 抓不到归属
pass" 这一类现象。

### 12.1 `TtRenderPolicy.QueueCmd` —— RenderGraph 内的"按 policy 归并"路径

签名 (定义见 `CSharpCode/Grapics/Pipeline/RenderPolicy.cs:458`):

```csharp
public void QueueCmd(NxRHI.FRenderCmd cmd, string name, object tag = null,
                     NxRHI.EQueueType qType = NxRHI.EQueueType.QU_Default,
                     NxRHI.ERCmdType type = NxRHI.ERCmdType.Cmd,
                     bool bImm = false)
{
    if (CmdQueue != null)
    {
        // ← 入 policy 自己的子队列, type / bImm 透传到底层 TtRCmdQueue
        CmdQueue.QueueCmd(cmd, name, tag, qType, type, bImm);
    }
    else
    {
        // 透传到全局 RenderQueue
        TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd(cmd, name, tag, qType, type, bImm);
    }
}
```

> 新增的两个可选参 `type` / `bImm` 语义和底层 `TtRCmdQueue.QueueCmd` 完全一致,
> 见 §12.2 与 §12.4 的说明。绝大多数业务场景保持默认即可 (`type = Cmd`,
> `bImm = false`), 只有"帧末哨兵 / 启动期阻塞执行"等少数场景才需要显式传入,
> 典型用例见 `CSharpCode/Base/Thread/ThreadLogic.cs:59` (`ERCmdType.FrameEnd`)
> 和 `CSharpCode/Bricks/Procedure/Node/GpuShading/GpuFetch.cs:56` (`bImm: true`)。

行为分两支:

- **`CmdQueue != null`** (常见情况, RenderGraph 正在 tick): 命令进入 **policy 私有的
  子 CmdQueue**, **不会立刻** 提交到 GPU; 而是在该 policy `OnRenderGraphCompleted`
  后由 `CmdQueue.FlushExecute(tsCmd.CmdList)` 一次性 record 到一条 `FTransientCmd`
  上批量提交。这意味着:
  - 该命令和当前帧的所有 RenderGraph drawcall **共享同一条 commandlist** (节省
    submit 开销, 也便于 RenderDoc 抓帧时归属到本帧)
  - 该命令的 GPU 端时序 = **当前帧 RenderGraph 之后**, 不会插队到 RenderGraph 中间
  - `tag` 会随命令一起被 CmdQueue 持有, 用于 GPU 完成后的回调 (`OnExecuted` 等)
- **`CmdQueue == null`** (policy 还没初始化 / 或显式不开子队列): 等价于直接调
  `RenderQueue.QueueCmd`, 见下条。

适用场景:

- 在 **RenderGraph 节点** 内或在 policy 内部需要追加一条 GPU 命令, 想让它跟随本帧
  的渲染时序一起提交 (例如 readback copy 在所有 drawcall 写完之后才执行 / RT 拷
  贝到 system memory / GPU buffer 之间的 dispatch-time copy)。
- 命令需要绑定到具体的 policy / 视口 (因为不同视口可能用不同的 CmdQueue, 命令
  混在一起会乱)。

### 12.2 `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd` —— 全局渲染线程立即执行路径

签名 (定义见 `CSharpCode/NxRHI/CmdQueue.cs` `TtRCmdQueue.QueueCmd`):

```csharp
public void QueueCmd(FRenderCmd cmd, string name, object tag = null,
                     NxRHI.EQueueType qType = EQueueType.QU_Default,
                     ERCmdType type = ERCmdType.Cmd,
                     bool bImm = false);
```

各参数含义:

- `cmd` / `name` / `tag` / `qType`: 同 `policy.QueueCmd`, 见 §12.1。
- `type`: 命令在 `RenderQueue.Cmds` 队列里的**分类标签**, 取值见 `ERCmdType` 枚举:
  - `Cmd` (默认): 普通单条命令, 被 `TickRender` 循环消费后继续处理下一条。
  - `Cmdlist`: `QueueCmdlist` / `QueueFence` 内部使用, 业务代码不要手动传。
  - `FrameEnd`: **帧末哨兵**, `TickRenderImpl` 消费到这一条会 `break` 跳出本帧消费循环。
    典型用例: `CSharpCode/Base/Thread/ThreadLogic.cs:59` 在逻辑线程 `Tick` 末尾投递一条
    `#TickLogicEnd#` FrameEnd 命令, 让渲染线程能识别"本帧逻辑侧已结束"。
- `bImm`: 是否**绕过 `EMultiRenderMode.Queue` 异步队列模式, 立即在当前线程执行**。
  - `false` (默认): 走 `ProcCmd` 的常规路径, 在 `Queue` 多线程渲染模式下 enqueue 到
    `Cmds`, 由渲染线程的 `TickRender` 消费; 其他模式下立即同步执行。
  - `true`: 无论当前是什么 `MultiRenderMode`, 都**立即在调用线程同步执行** (`FrameEnd`
    类型除外, 它永远 enqueue 作为帧末哨兵)。用于启动期资源上传 / 引擎关闭前 flush
    等"不能等下一帧"的场景。典型用例: `CSharpCode/Bricks/Procedure/Node/GpuShading/
    GpuFetch.cs:56` 在 fence signal 后用 `bImm: true` 立即执行, 保证 readback
    数据在函数返回前拿到。

行为: 命令进入 **引擎全局 `RenderQueue`** (单一队列, 跨所有 policy / 视口), 在
**渲染线程的下一个 tick 立刻被消费**, record 到一条全局 transient cmdlist 后
submit 到 GPU。**不依赖任何 policy 的生命周期**。

适用场景:

- 命令和具体 policy 没关系 —— 比如全局资源上传 / 全局 readback / shader cache
  warm-up / TTextureManager 的资源流式调度。
- 调用方根本不在 RenderGraph 上下文内 —— 比如 `Tools` / `Editor` 命令、worker
  thread 临时投递、热重载触发的资源刷新。
- 当前没有任何 `TtRenderPolicy` 实例可用 (例如启动期, RenderGraph 还没建)。

### 12.3 选型决策表

| 你的场景 | 用哪条 |
|---|---|
| 在某个 `RenderGraph` 节点的 `Tick` 里追加一条 readback / copy / dispatch | `policy.QueueCmd` |
| 在 `TtRenderPolicy` 子类的回调里追加命令 | `policy.QueueCmd` |
| 引擎启动期 / 编辑器 Tools / 热重载时的全局资源更新 | `RenderQueue.QueueCmd` |
| worker thread / async 任务里临时要扔一条 GPU 命令, 跟视口无关 | `RenderQueue.QueueCmd` |
| 已经在渲染线程上 (例如 `ITickable.TickRender`), 就是想立刻执行 | `RenderQueue.QueueCmd` |
| 不确定 —— 但你能拿到 `policy` 引用 | `policy.QueueCmd` (它内部会在 `CmdQueue == null` 时自动降级到全局路径, 兼容性更好) |
| **❌ 永远不要用** `TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList` | 它绕开引擎的 `RenderQueue`, 失去 CmdQueue 归并 / Profiler hook / policy flush 时序保证, 详见 §12.4 |

> 提交 `TtCommandList` (cmdlist 级) 用 `RenderQueue.QueueCmdlist(cmd, name, qType)`,
> 提交 `FRenderCmd` (单条 cmd 级) 用 `RenderQueue.QueueCmd(cmd, name, tag, qType)`。
> 两者都是引擎管理的"正道"入口。

### 12.4 反直觉点 (容易踩的坑)

- **`policy.QueueCmd` 不等于 "立即执行"**: 命令要等 policy 当前帧 RenderGraph
  完整跑完后才 flush。如果你在 `policy.QueueCmd` 之后**同步** `FetchGpuData` /
  `fence.Wait`, 大概率拿不到数据 (本仓库已踩过, 见 `NxRHI/Buffer.cs::TtBuffer::
  AsyncFetchGpuData` 的演化注释 + `CodingGuidelines.md §1.5.3.1`)。
- **绝不要直接调 `RenderContext.GpuQueue.ExecuteCommandList` / `GpuQueue.QueueCmdlist`
  之外的任何底层 GpuQueue 提交 API**。`GpuQueue` 是 native 层 `IGpuQueue` 的直接
  暴露, 走它绕开了引擎的:
  ① `CmdQueue` 归并机制 (CmdQueue 会把多个 policy / 节点产生的 cmdlist 合到一条
  transient cmdlist 里减少 submit 开销);
  ② Profiler / RenderDoc 标签链 (`name` / `tag` 参数走 RenderQueue 才会被 hook);
  ③ policy `OnRenderGraphCompleted` 时序保证 (走 GpuQueue 直接立即提交, 顺序和
  RenderGraph 主流交错混乱)。
  正确做法: cmdlist 级走 `TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist
  (cmd, name, qType)`, 单条 cmd 级走 `RenderQueue.QueueCmd(cmd, name, tag,
  qType)` 或对应的 `policy.QueueCmd` / `policy.CommitCommandList`。
  历史已知违规残留 (待修): `Bricks/Procedure/Node/GpuShading/GpuFetch.cs:51`。
  详见 CodingGuidelines.md §1.7。
- **`RenderQueue.QueueCmd` 跨 policy 共享单队列**: 高频投递会和 RenderGraph 主流
  抢 submit 带宽, 大批量命令优先走 policy 路径让其归并到一条 cmdlist。
- **`CmdQueue == null` 时 `policy.QueueCmd` 行为 = `RenderQueue.QueueCmd`**: 不要
  以为 "我用了 policy 路径就一定走子队列", 这取决于 policy 的初始化阶段。需要
  确定时机时直接看 `policy.CmdQueue` 是不是 null。
- **`tag` 参数语义不同**: 子 CmdQueue 路径的 `tag` 会被 CmdQueue 强引用直到
  flush 完成 (用于 `OnExecuted` 回调匹配); 全局 RenderQueue 路径的 `tag` 仅作
  RenderDoc / Profiler 标签, 不持有引用。
- **`type` / `bImm` 默认值不要轻易改**: 这两个新增可选参看起来"通用", 但实际业务
  含义很窄:
  - `type = ERCmdType.Cmd` 是业务代码**唯一**应该使用的取值。`Cmdlist` 是
    `QueueCmdlist` / `QueueFence` 内部专用 (带 `ExecuteCommandList` / `IncreaseSignal`
    的特殊 `Cmd` lambda), 手动传会把一条裸 `FRenderCmd` 伪装成 cmdlist, 统计和
    `Flush` 分支都会错乱。`FrameEnd` 仅用于**逻辑线程每帧投一次的帧末哨兵**
    (参考 `ThreadLogic.cs:59` 的 `#TickLogicEnd#`), 任何其他位置投 `FrameEnd`
    都会让 `TickRenderImpl` 提前 break, 丢弃队列里本帧剩余命令。
  - `bImm = true` 会绕过 `EMultiRenderMode.Queue` 的异步排队, 直接在**调用线程**
    同步执行 cmd lambda。调用线程不是渲染线程时, lambda 里任何 `TtCommandList` /
    `GpuQueue` 操作都会踩多线程雷; 即使调用线程是渲染线程, 也会打乱 `Cmds` 的
    FIFO 顺序, 让后面已排队但还未消费的命令相对于当前命令"晚执行"。只有在
    "启动期单线程 / 引擎 shutdown flush / 外层已持有 fence 保证时序" 这类场景
    才考虑用, 参考 `GpuFetch.cs:56` (fence signal 之后立即 `bImm: true` 收尾)。
- **同步 readback 想要"显式 GPU 完成屏障"用 `RenderQueue.QueueFence`,
  不要手写 `rc.GpuQueue.IncreaseSignal`**。前者是引擎为"绑在 RenderQueue 提交序列
  上"专门提供的 fence 入口, 时序与 `RenderQueue.QueueCmdlist` 强一致; 后者是底层
  `IGpuQueue` API, signal 时机和 cmdlist 实际 submit 时机不一定对齐, wait 出来
  语义含糊。注: `FetchGpuData` 内部已经含 flush+wait, 一般情况下 `QueueCmdlist
  + 立刻 FetchGpuData` 就能正常工作 (见 `Bricks/GpuDriven/Cluster.cs:489+509`
  范例); 但当业务需要"先 fence wait 再走自己的同步 / 跨队列同步逻辑"时, 就必须
  用 `QueueFence`。详见 CodingGuidelines.md §1.7 末尾。

  ```csharp
  TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(
      mCmdList, "GpuBvh.RayCast", EQueueType.QU_Compute);
  var fence = TtEngine.Instance.GfxDevice.RenderQueue.QueueFence(
      null, "GpuBvh.RayCast", EQueueType.QU_Compute, true);
  fence.Wait(1);                                          // ✓ 显式屏障, 走 RenderQueue
  mHitBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject); // 现在 GPU 一定写完了
  ```

## 13.给属性加一个"收藏夹"下拉 (跨编辑器共享字符串路径)

场景: 在 A 编辑器里选中了某个东西 (骨骼 / 插槽 / 节点路径 / 资源子项...), 想在 B 编辑器的
Details 里用下拉直接选到它, 而不是在"全量候选项"里翻 (典型反例: DMC 的骨骼 picker 会
把工程里所有 `.skt` × 所有骨骼铺成一棵树)。

机制两个文件, 位于 `CSharpCode/Editor/Infrastructure/Favorites/`:

| 类 | 职责 |
|---|---|
| `TtEditorFavoritePaths` | channel(字符串通道) → 有序去重的路径列表; `Get/Add/Remove/Clear/Contains/Find`, 最近添加排最前, 默认容量 32; 纯静态内存, **只活本次会话** |
| `TtPGFavoritePathAttribute` | PG 自定义编辑器: 在 Details 行内画出 `[+]` 加入收藏夹 / `[v]` 从收藏夹挑选 (下拉里每条右侧 `x` 移除) |

设计前提: 机制本身**与类型无关**, 只存字符串; 语义由同一 channel 的生产端与消费端约定。

### 13.1 三步接入

**① 定义通道**。直接用字符串常量; 如果希望"属性上不写 Channel 也能自动映射", 再给属性类型登记一个默认值:

```csharp
// 通道常量建议放在 TtEditorFavoritePaths 里集中管理 (已有: ChannelBone = "Bone")
TtEditorFavoritePaths.RegisterTypeChannel(typeof(TtSocketRef), "Socket");   // 可选
```

**② 生产端** (能"选中一个东西"的编辑器): 暴露一个**只读 `string` 属性**给出规范路径, 只开 Add:

```csharp
[Category("General")]
[ReadOnly(true)]
[Editor.Infrastructure.TtPGFavoritePath(
    Channel = Editor.Infrastructure.TtEditorFavoritePaths.ChannelBone, AllowPick = false)]
public string BonePath =>
    Editor.Infrastructure.TtEditorFavoritePaths.MakeBonePath(mSkeletonAssetName, Desc?.Name);
```

**③ 消费端** (要引用它的地方): 可写 `string` 属性, 只开 Pick:

```csharp
[Editor.Infrastructure.TtPGFavoritePath(
    Channel = Editor.Infrastructure.TtEditorFavoritePaths.ChannelBone, AllowAdd = false)]
public string TargetBonePath { get; set; }
```

两端都不需要知道对方存在, 也不需要任何初始化/注册时机 —— channel 就是契约。

### 13.2 消费端不是 `string` 类型怎么接

很多老属性存的是结构体 (例如 `LimbIndexInSkeleton { Name, Index, Skeleton }`), 通用 attribute 挂不上去。
做法: 在它自己的 combo 顶部多画一段 `Favorites`, 把路径解析回自己的值类型 —— 参考
`CSharpCode/Bricks/Animation/AnimUtil.cs` 的 `TtSkeletonBoneIndexPickerEditorAttribute`:

```csharp
var favorites = Editor.Infrastructure.TtEditorFavoritePaths.Get(
    Editor.Infrastructure.TtEditorFavoritePaths.ChannelBone);
if (favorites.Count > 0)
{
    ImGuiAPI.TextDisabled("Favorites");
    for (int fi = 0; fi < favorites.Count; fi++)
    {
        if (!TryResolveFavoritePath(favorites[fi].Path, out var favVal))   // 自定义解析
            continue;
        bool favSel = ...;
        if (ImGuiAPI.Selectable($"...##fav{fi}", ref favSel, ..., in Vector2.Zero))
        {
            comboNewVal = favVal;
            comboChanged = true;
        }
    }
    ImGuiAPI.Separator();
}
```

解析函数里用 `TryParseBonePath` 拆出"资产名 + 条目名", 再**按名字到当前数据里重新查 index**,
不要相信路径里的任何索引式信息 (原因见 §13.3)。

### 13.3 路径格式约定

- 格式统一为 **`资产名:条目名`**, 并在 `TtEditorFavoritePaths` 里**成对**提供
  `MakeXxxPath` / `TryParseXxxPath` (已有 `MakeBonePath` / `TryParseBonePath`), 不要让业务
  代码自己拼字符串。
- **绝不把 index / 运行时 id / 指针 编进路径**。骨骼 index 会随骨架重导入漂移, 路径只能放
  "重开工程也稳定"的名字; index 由消费端现场解析。
- 同一容器内名字必须唯一才能用这个格式。骨骼成立 (因为 `TtSkinSkeleton.HashDic` 以 NameHash
  为键), 其他域接入前先确认这一点, 否则要改成层级路径并同步这节。
- `TtFavoritePathEntry.Display` 只影响下拉里的显示文字, 判重与写回一律用 `Path`。

### 13.4 UI 实现上的坑 (照抄 `TtPGFavoritePathAttribute`)

- **`ImGuiAPI` 没绑 `BeginDisabled` / `EndDisabled`** (只有 `TextDisabled`)。按钮置灰的写法是
  "按钮照画 + `&& canDo` 守卫 + tooltip 说明为何不能点", 与 `InstanceMeshNode.cs` 的
  `Button(...) && !disabled` 一致。
- **`OpenPopup` 与 `BeginPopup` 必须在同一个 `PushID` 作用域内**, 否则 popup id 算不上,
  表现为"点了没反应"。
- **PG 表格单元格里要自己算宽度**, 可用宽 = `GetColumnWidth(TableGetColumnIndex())` 减掉
  `按钮数 * (GetFrameHeight() + 间距)` 与 `StyleConfig.Instance.PGCellPadding.X`, 给文本框
  `SetNextItemWidth` 前预留最小宽。
- **只读 (getter-only) 属性上的编辑器必须保证 `OnDraw` 永不返回 true**, 否则 PG 会去
  `SetValue` 报错 —— 所以生产端一定要写 `AllowPick = false`。
- 遍历收藏列表时若在循环体内 `Remove`, 必须立即 `break`。
- 值本身用只读 `InputText` 展示, **不提供手敲路径**的入口 (手敲必错且无校验)。

### 13.5 参考位置

- 机制本体: `CSharpCode/Editor/Infrastructure/Favorites/TtEditorFavoritePaths.cs`,
  `.../TtPGFavoritePathAttribute.cs`
- 生产端范例: `CSharpCode/Editor/Forms/SkeletonTreePanel.cs` 的 `TtBoneEditProxy.BonePath`
  (骨架资产名由 `TtSkeletonEditor` 通过 `SkeletonTreePanel.SkeletonAssetName` 注入)
- 非 string 消费端范例: `CSharpCode/Bricks/Animation/AnimUtil.cs` 的
  `TtSkeletonBoneIndexPickerEditorAttribute` (combo 顶部 Favorites 分段 + `TryResolveFavoritePath`)
- 强制约束见 `CodingGuidelines.md` §5

❗ 新增的 `.cs` 文件要登记到对应 `.projitems` (本机制在 `CSharpCode/Editor/Editor.projitems`),
projitems 是显式文件列表, 不做通配。

## 这是没用的LaTex测试，请忽略
$$\sum_{i=0}^{^9}{\left(\frac{a_i}{b_i}\right)}$$