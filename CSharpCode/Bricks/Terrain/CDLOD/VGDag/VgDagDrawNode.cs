using EngineNS.Bricks.Terrain.CDLOD.VGDag;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// 远景 cluster DAG 绘制节点(主 RDG 图内的 mesh 宿主):
    /// GPU cull/压缩在独立迷你 RenderPolicy 里(TtVgDagFarTerrain, 与主图解耦),
    /// 本节点只负责把 patch mesh 挂进 world, 走正规 scene 采集链进 gbuffer
    /// opaque layer(一次延迟渲染)。绘制为单次 DrawIndexedInstancedIndirect,
    /// instance = 可见 cluster(VgDagDrawModifier.cginc), 读 TtVgDagFarTerrain
    /// 的乒乓读侧 buffer。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("VGDagDraw", "Terrain\\VGDagDraw", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtVgDagDrawNode : TAuxRenderGraphNode<TtVgDagDrawNode>
    {
        #region MDF 三件套
        /// <summary>
        /// 远景绘制 modifier: 绑定 cull 产物 + indirect args。
        /// indirect 路径与 VoxelDebugger/TtParticleMdfQueue 同款
        /// (TtGraphicDraw.Commit 检测到 IndirectDrawArgsView 时走 ExecuteIndirect)。
        /// </summary>
        public class TtVgDagDrawModifier : Graphics.Pipeline.Shader.IMeshModifier
        {
            public string ModifierNameVS { get => "DoVgDagDrawModifierVS"; }
            public string ModifierNamePS { get => null; }
            public RName SourceName
            {
                get => RName.GetRName("shaders/Modifier/VgDagDrawModifier.cginc", RName.ERNameType.Engine);
            }
            public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
            {
                return (NxRHI.FShaderCode*)0;
            }
            public string GetUniqueText()
            {
                return "";
            }
            public NxRHI.EVertexStreamType[] GetNeedStreams()
            {
                return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, };
            }
            public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
            {
                return null;
            }
            public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
            {
            }
            public void OnBuildDrawCall(Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall, Graphics.Mesh.TtRenderMesh.TtAtom atom)
            {
            }
            public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
            {
                var vgQueue = mdfQueue as TtMdfVgDagDrawMesh;
                var node = vgQueue?.MdfDatas as TtVgDagDrawNode;
                var host = node?.FarTerrain;
                if (host == null || host.HasAsset == false)
                    return;

                drawcall.BindSRV("VgDrawClusters", host.ReadClusters.Srv);
                drawcall.BindSRV("VgClusterBuffer", host.VisClusters.Srv);
                drawcall.BindSRV("VgVertexBuffer", host.VisVertices.Srv);
                drawcall.BindSRV("VgIndexBuffer", host.VisIndices.Srv);
                var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVgVisParams");
                if (binder.IsValidPointer)
                {
                    if (node.CBVisParams == null)
                    {
                        // §1.1: 创建即全字段 SetValue + MarkDirty + FlushDirty
                        node.CBVisParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                        node.CBVisParams.SetValue("VgVisParams", in host.mVisParams);
                        node.CBVisParams.MarkDirty();
                        node.CBVisParams.FlushDirty();
                    }
                    else
                    {
                        node.CBVisParams.SetValue("VgVisParams", in host.mVisParams);
                    }
                    drawcall.BindCBV(binder, node.CBVisParams);
                }
                // arg 初始全 0(InstanceCount=0), 首帧天然执行 0 个 draw
                drawcall.BindIndirectDrawArgsView(host.ReadArgs.Iav, 0);
            }
            public void Dispose()
            {
            }
        }

        [Rtti.Meta("")]
        public class TtMdfVgDagDrawMesh : Graphics.Pipeline.Shader.TtMdfQueue1<TtVgDagDrawModifier>
        {
        }
        #endregion

        /// <summary>
        /// 远景准备系统(独立迷你 policy + 乒乓 cull)。由地形系统注入;
        /// 缺省惰性自建以便单独预览。
        /// </summary>
        [Rtti.Meta("")]
        public TtVgDagFarTerrain FarTerrain
        {
            get
            {
                if (mFarTerrain == null)
                    mFarTerrain = new TtVgDagFarTerrain();
                return mFarTerrain;
            }
            set => mFarTerrain = value;
        }
        private TtVgDagFarTerrain mFarTerrain;

        public TtCbView CBVisParams;

        /// <summary>LOD 选择阈值: 屏幕空间竖直误差(像素), 转发给 FarTerrain。</summary>
        [Rtti.Meta("")]
        public float PixelErrorThreshold
        {
            get => FarTerrain.PixelErrorThreshold;
            set => FarTerrain.PixelErrorThreshold = value;
        }

        /// <summary>patch mesh 材质(默认无光调试材质; 最终应换成地形材质走完整延迟光照)。</summary>
        [Rtti.Meta("")]
        public RName MaterialName = RName.GetRName("material/dft_unlit.material", RName.ERNameType.Engine);

        /// <summary>
        /// 排序用途的输入 pin(不消费数据): RenderGraph 靠 RootNode 回溯决定节点是否
        /// Tick, 无 pin 节点会被跳过(SWEWaterNode 同坑); 在 RGD 编辑器把它连到
        /// gbuffer 链最前端, 保证每帧先驱动 prepare 再绘制。
        /// </summary>
        public TtRenderGraphPin OrderPinIn = TtRenderGraphPin.CreateInput("Order", EBufferType.BFT_SRV);

        private Graphics.Mesh.TtRenderMesh mDrawMesh;
        private GamePlay.Scene.TtMeshNode mDrawMeshNode;
        private bool mMaterialLoading = false;
        /// <summary>绘制网格建立失败(材质缺失等): 不再每帧重试, 改 MaterialName 后复位。</summary>
        private bool mDrawMeshFailed = false;
        private RName mFailedMaterialName;
        private bool mDisposed = false;

        public TtVgDagDrawNode()
        {
            Name = "VgDagDrawNode";
        }

        public override void InitNodePins()
        {
            AddInput(OrderPinIn);
            OrderPinIn.IsAllowInputNull = true;
            base.InitNodePins();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
            FarTerrain.ViewportHeight = y;
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            // 独立迷你 policy 的构建在初始化阶段完成(渲染线程禁 fire-and-forget)
            await FarTerrain.InitializeAsync();
        }

        public override void Dispose()
        {
            mDisposed = true;
            CoreSDK.DisposeObject(ref CBVisParams);
            // 先把绘制节点从 world 摘掉并断开 modifier → 节点的反向引用, 再释放网格
            DetachDrawMesh();
            mFarTerrain?.Dispose();
            mFarTerrain = null;
            base.Dispose();
        }

        /// <summary>
        /// 建立 patch 绘制网格并挂到 world: MaxClusterIndexCount 个顶点 + 顺序索引,
        /// 使 vVertexID 恰为实例内索引序号; 几何是 dummy, modifier VS 按 cluster
        /// 局部索引重定位到顶点池。
        /// </summary>
        private static unsafe Graphics.Mesh.TtMeshDataProvider MakePatchMesh()
        {
            var meshBuilder = new Graphics.Mesh.TtMeshDataProvider();
            meshBuilder.AssetName = RName.GetRName("@VgDagPatch", RName.ERNameType.Transient);
            var builder = meshBuilder.mCoreObject;
            builder.Init((uint)(1 << (int)NxRHI.EVertexStreamType.VST_Position), true, 1);
            const int count = TtVgDagGpuData.MaxClusterIndexCount;
            for (int i = 0; i < count; i++)
            {
                // 各顶点位置互不相同, 避免任何按位置的顶点合并
                var pos = new Vector3((float)i, 0, 0);
                builder.AddVertex(in pos, in Vector3.Zero, in Vector2.Zero, 0xFFFFFFFF);
            }
            for (uint t = 0; t < count / 3; t++)
                builder.AddTriangle(t * 3, t * 3 + 1, t * 3 + 2);

            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (uint)(count / 3);
            builder.PushAtomLOD(0, in dpDesc);
            return meshBuilder;
        }

        private void DetachDrawMesh()
        {
            if (mDrawMeshNode != null)
            {
                mDrawMeshNode.Parent = null;
                mDrawMeshNode = null;
            }
            if (mDrawMesh != null && mDrawMesh.MdfQueue != null)
                mDrawMesh.MdfQueue.MdfDatas = null;
            CoreSDK.DisposeObject(ref mDrawMesh);
        }

        private async Thread.Async.TtTask EnsureDrawMesh(GamePlay.TtWorld world)
        {
            if (mDrawMesh != null || MaterialName == null || mMaterialLoading)
                return;
            if (mDrawMeshFailed && mFailedMaterialName == MaterialName)
                return;
            mDrawMeshFailed = false;
            mMaterialLoading = true;

            var material = await MaterialName.GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
            if (material == null || mDisposed)
            {
                MarkDrawMeshFailed(material == null);
                return;
            }

            // patch 容量必须与 VgDagLod.cginc 的 VG_CLUSTER_MAX_INDEX 一致
            var patch = MakePatchMesh();
            mDrawMesh = new Graphics.Mesh.TtRenderMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = material;
            if (false == mDrawMesh.Initialize(patch.ToMesh(), materials,
                Rtti.TtTypeDescGetter<TtMdfVgDagDrawMesh>.TypeDesc))
            {
                CoreSDK.DisposeObject(ref mDrawMesh);
                MarkDrawMeshFailed(true);
                return;
            }
            mDrawMesh.MdfQueue.MdfDatas = this;

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(world, world.Root,
                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement),
                mDrawMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            if (meshNode == null)
            {
                DetachDrawMesh();
                MarkDrawMeshFailed(true);
                return;
            }
            meshNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
            meshNode.NodeData.Name = "VGDag_FarDrawMesh";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            mDrawMeshNode = meshNode;
            mMaterialLoading = false;
            // await 期间节点已被 Dispose: 立刻摘掉, 不留孤儿节点在 world 里
            if (mDisposed)
                DetachDrawMesh();
        }

        private void MarkDrawMeshFailed(bool failed)
        {
            mMaterialLoading = false;
            if (failed)
            {
                mDrawMeshFailed = true;
                mFailedMaterialName = MaterialName;
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning,
                    $"VgDagDrawNode: build far draw mesh failed (material={MaterialName}), will not retry until MaterialName changes");
            }
        }

        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (FarTerrain.HasAsset == false)
                return;

            // 驱动独立 prepare policy(clear+cull, 独立 TtRCmdQueue, 与主图 cmdlist 解耦),
            // 之后乒乓交换; 本帧 gbuffer 里远景 mesh 读刚完成的剔除结果
            FarTerrain.TickPrepare(policy);

            // 绘制网格惰性建立(需 world 与材质就绪); 之后由 scene 采集链走 gbuffer
            // (异步但有 mMaterialLoading 防重入 + 失败不重试; 完成前本节点仅做 prepare 不出图)
            if (world != null && mDrawMeshNode == null && !mDisposed)
            {
                var task = EnsureDrawMesh(world);
            }
        }
    }
}
