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
UPluginLoader是用来描述插件的
AssemblyEntry是用来描述DLL模块在UTypeDesc的信息的，通常处理类型和Meta信息
```C#
namespace EngineNS.Plugins.你的插件名
{
    public class UPluginLoader
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
        public class UGameServerAssemblyDesc : UAssemblyDesc
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
```XML
<?xml version="1.0" encoding="utf-8"?>
<Root Type="EngineNS.Bricks.AssemblyLoader.UPluginDescriptor@EngineCore">
  <LoadOnInit Type="System.Boolean@Unknown" Value="False" />
  <Platforms Type="System.Collections.Generic.List&lt;EngineNS.EPlatformType@EngineCore,&gt;@Unknown" Count="1">
    <e_0 Value="PLTF_Windows" />
  </Platforms>
  <Dependencies Type="System.Collections.Generic.List&lt;System.String@Unknown,&gt;@Unknown" Count="1">
    <e_0 Value="GameServer" />
  </Dependencies>
</Root>
```
Plugins目录下CopyPlugins.bat在修改*.plugin后目前需要手工执行，刷新到插件目录
## 3.Native Bricks开发
1.目前需要在Base/BaseHead.h里面根据平台添加类似 **#define HasModule_NextRHI** 
2.这个用来确保生成胶水代码参与编译构建，否则会发生C#调用C++找不到函数
## 4.RPC函数
1.为函数添加URpcMethod属性
2.最后一个参数必须为UCallContext context，可以从中取出Connect
```C#
		[URpcMethod(Index = RpcIndexStart + 2)]
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
- 派生IAutoSyncObject接口
- 为类添加[UAutoSync()]属性
- 添加属性，并用UAutoSync修饰
- set的实现调用FSyncHelper.SetValue，注意Index参数和属性的Index一致
- 示例
```C#
	[UAutoSync()]
    public partial class IAutoSyncObject_Test0 : IAutoSyncObject
    {
        int mA;
        [UAutoSync(Index = 0)]
        public int A 
        {
            get => mA;
            set
            {
                FSyncHelper.SetValue(this, 0, ref mA, in value);
            }
        }
        float mB;
        [UAutoSync(Index = 1)]
        public float B
        {
            get => mB;
            set
            {
                FSyncHelper.SetValue(this, 1, ref mB, in value);
            }
        }

        public void RealObjectUpdate2Server()
        {
            using (var writer = Bricks.Network.RPC.UMemWriter.CreateInstance())
            {
                var ar = new IO.AuxWriter<Bricks.Network.RPC.UMemWriter>(writer);
                FSyncHelper.BuildModify(this, ar);
            }
        }
        public unsafe void GhostObjectUpdateByServer(Bricks.Network.RPC.UMemWriter writer)
        {
            using (var reader = Bricks.Network.RPC.UMemReader.CreateInstance((byte*)writer.Ptr, writer.GetPosition()))
            {
                var ar = new IO.AuxReader<Bricks.Network.RPC.UMemReader>(reader, null);
                FSyncHelper.SyncValues(this, ar);
            }
        }
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
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        [ContextMenu("samplelevel2d", "Sample\\Level2D", UMaterialGraph.MaterialEditorKeyword)]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(Sample2DNode))]
        public static Vector4 Sample2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleArrayLevel2DNode))]
        public static Vector4 SampleArrayLevel2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleArray2DNode))]
        public static Vector4 SampleArray2D(Var.Texture2DArray texture, Var.SamplerState sampler, Vector2 uv, float arrayIndex, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        public static Vector3 GetTerrainDiffuse(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        public static Vector3 GetTerrainNormal(Vector2 uv, Graphics.Pipeline.Shader.UMaterial.PSInput input)
        {
            return Vector3.Zero;
        }
        [Rtti.Meta]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
	}
```

## 7.增加一个UMdfQueue处理顶点变换定制
- 参阅EngineNS.Graphics.Mesh.UMdfStaticMesh
- 核心是实现GetBaseBuilder函数提供MdfQueue的hlsl代码
- 通过继续派生本类的泛型版本来控制MdfQueu的shader permutation

## 8.增加一个UShadingEnv处理Shader总流程
- 参阅Graphics.Pipeline.Deferred.UOpaqueShading
- 构造器中提供定制好的CodeName
- 在BeginPermutaion和UpdatePermutation中添加PushPermutation来增加Permutaion
- 通过Permuation对象的SetValue来设置当前Permutation
- 记得重载GetNeedStreams来指定需要的VertexBuffer
- 顺道关注一下RenderGraphNode，通常他负责最终使用这些ShadingEnv
