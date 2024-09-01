# ���ô���
## 1.���ܷ���
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
## 2.�������
1.ȷ�����dll��������ʵ��
UPluginLoader���������������
AssemblyEntry����������DLLģ����UTypeDesc����Ϣ�ģ�ͨ���������ͺ�Meta��Ϣ
```C#
namespace EngineNS.Plugins.��Ĳ����
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
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:��Ĳ���� AssemblyDesc Created");
            }
            ~UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:��Ĳ���� AssemblyDesc Destroyed");
            }
            public override string Name { get => "��Ĳ����"; }
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
2.���������·��
binaries\Plugins\Debug\net7.0
3.�ڲ��Ŀ¼��Ҫ��Ӳ��ͬ��.plugin�ļ������ݴ������£�
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
PluginsĿ¼��CopyPlugins.bat���޸�*.plugin��Ŀǰ��Ҫ�ֹ�ִ�У�ˢ�µ����Ŀ¼
## 3.Native Bricks����
1.Ŀǰ��Ҫ��Base/BaseHead.h�������ƽ̨������� **#define HasModule_NextRHI** 
2.�������ȷ�����ɽ�ˮ���������빹��������ᷢ��C#����C++�Ҳ�������
## 4.RPC����
1.Ϊ�������URpcMethod����
2.���һ����������ΪUCallContext context�����Դ���ȡ��Connect
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
## 5.�Զ�ͬ������
- ����IAutoSyncObject�ӿ�
- Ϊ�����[UAutoSync()]����
- ������ԣ�����UAutoSync����
- set��ʵ�ֵ���FSyncHelper.SetValue��ע��Index���������Ե�Indexһ��
- ʾ��
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
## 6.����һ�����ʱ༭���ɵ��ú����ڵ�
- ��HLSLMethod����һ����̬����
- - ��������ΪVector2/3/4��ϵͳ����SamplerState��Texture2D
- ����Rtti.Meta��������
- ���ʹ��UserCallNode���ԣ������Զ�����ʱ༭���е�Node
- ʹ��ContextMenu�����ڲ˵��г��ֵ�λ��
- ������ʾ��
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

## 7.����һ��UMdfQueue������任����
- ����EngineNS.Graphics.Mesh.UMdfStaticMesh
- ������ʵ��GetBaseBuilder�����ṩMdfQueue��hlsl����
- ͨ��������������ķ��Ͱ汾������MdfQueu��shader permutation

## 8.����һ��UShadingEnv����Shader������
- ����Graphics.Pipeline.Deferred.UOpaqueShading
- ���������ṩ���ƺõ�CodeName
- ��BeginPermutaion��UpdatePermutation�����PushPermutation������Permutaion
- ͨ��Permuation�����SetValue�����õ�ǰPermutation
- �ǵ�����GetNeedStreams��ָ����Ҫ��VertexBuffer
- ˳����עһ��RenderGraphNode��ͨ������������ʹ����ЩShadingEnv
