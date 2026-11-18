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
    /// 远景 cluster DAG 的 GPU 数据准备系统(独立迷你 RenderPolicy, 不进主 RDG):
    /// cull pass(compute) 做组一致 LOD 选择(VgDagLod.cginc) + 视锥剔除, 可见 cluster
    /// 下标压缩写入 DrawClusters 并以 InterlockedAdd 生成 DrawIndexedInstancedIndirect 参数。
    /// 组织方式与 TtSWEWaterNode / TtSceneCapture 同款: new TtRenderPolicy() +
    /// 独立 TtRCmdQueue, 在主 policy Tick 内驱动、与主图 cmdlist 解耦。
    /// 双缓冲乒乓: prepare 写写侧, 每次 TickPrepare 末尾交换, 主图远景 mesh(gbuffer
    /// opaque)读交换后的读侧; cull 永远不写主图正在读的那一份。
    /// </summary>
    [Rtti.Meta("")]
    public class TtVgDagFarTerrain : IDisposable
    {
        #region prepare 节点(迷你 policy 图内)
        public class TtVgDagClearShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(1, 1, 1);

            public TtVgDagClearShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/Terrain/VGDag/VgDagCull.compute", RName.ERNameType.Engine);
                MainName = "CS_ClearDrawArgs";
                this.UpdatePermutation().AddWaitTask();
            }

            public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtVgDagPrepareNode;
                if (node == null)
                    return;
                drawcall.BindUav("DrawArgs", node.Host.WriteArgs.Uav);
            }
        }

        public class TtVgDagCullShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

            public TtVgDagCullShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/Terrain/VGDag/VgDagCull.compute", RName.ERNameType.Engine);
                MainName = "CS_CullClusters";
                this.UpdatePermutation().AddWaitTask();
            }

            public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtVgDagPrepareNode;
                if (node == null)
                    return;

                var host = node.Host;
                drawcall.BindSrv("VisClusterBuffer", host.VisClusters.Srv);
                drawcall.BindSrv("VisGroupBuffer", host.VisGroups.Srv);
                drawcall.BindUav("DrawClusters", host.WriteClusters.Uav);
                drawcall.BindUav("DrawArgs", host.WriteArgs.Uav);

                var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVgVisParams");
                if (binder.IsValidPointer)
                {
                    if (host.CBVisParams == null)
                    {
                        // §1.1: 创建即全字段 SetValue + MarkDirty + FlushDirty
                        host.CBVisParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                        host.CBVisParams.SetValue("VgVisParams", in host.mVisParams);
                        host.CBVisParams.MarkDirty();
                        host.CBVisParams.FlushDirty();
                    }
                    else
                    {
                        host.CBVisParams.SetValue("VgVisParams", in host.mVisParams);
                    }
                    drawcall.BindCBV(binder, host.CBVisParams);
                }
            }
        }

        public class TtVgDagPrepareNode : TAuxRenderGraphNode<TtVgDagPrepareNode>
        {
            /// <summary>宿主系统, 持有乒乓 buffer 与参数。</summary>
            public TtVgDagFarTerrain Host;

            private TtVgDagClearShading mClearShading;
            private TtComputeDraw mClearDrawcall;
            private TtVgDagCullShading mCullShading;
            private TtComputeDraw mCullDrawcall;

            /// <summary>
            /// 排序用途的输出 pin(不携带资源): 迷你图靠 RootNode 回溯决定节点是否 Tick,
            /// 必须经它连到 ending 节点(GpuCullingNode.GpuCullOut 同款)。
            /// </summary>
            public TtRenderGraphPin PreparedPinOut = TtRenderGraphPin.CreateOutput("Prepared", false, EPixelFormat.PXF_UNKNOWN, EBufferType.BFT_NONE);

            public TtVgDagPrepareNode()
            {
                Name = "VgDagPrepareNode";
            }

            public override void InitNodePins()
            {
                AddOutput(PreparedPinOut);
                base.InitNodePins();
            }

            public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
            {
                await base.Initialize(policy, debugName);
                var rc = TtEngine.Instance.GfxDevice.RenderContext;

                CoreSDK.DisposeObject(ref mClearDrawcall);
                mClearDrawcall = rc.CreateComputeDraw();
                mClearShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVgDagClearShading>();

                CoreSDK.DisposeObject(ref mCullDrawcall);
                mCullDrawcall = rc.CreateComputeDraw();
                mCullShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVgDagCullShading>();
            }

            public override void Dispose()
            {
                CoreSDK.DisposeObject(ref mClearDrawcall);
                CoreSDK.DisposeObject(ref mCullDrawcall);
                base.Dispose();
            }

            public override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
            {
                if (Host == null)
                    return;

                uint clusterCount = Host.ClusterCount;
                if (clusterCount == 0)
                    return;

                var cmd = TtCommandList.GetCmdList();
                using (new TtCmdListScope(cmd, "VgDagPrepare.Cull"))
                {
                    mClearShading.SetDrawcallDispatch(this, policy, mClearDrawcall, 1, 1, 1, true);
                    cmd.PushGpuDraw(mClearDrawcall);

                    mCullShading.SetDrawcallDispatch(this, policy, mCullDrawcall, clusterCount, 1, 1, true);
                    cmd.PushGpuDraw(mCullDrawcall);

                    cmd.FlushDraws();
                }
                policy.CommitCommandList(cmd);
            }
        }
        #endregion

        #region shader 数据
        public FVgVisParams mVisParams;
        public TtCbView CBVisParams;

        /// <summary>bake 顶点池(SRV, draw 端 VS 拉取; 单份, bake 后不变)。</summary>
        public TtCpu2GpuBuffer<FVgVisVertex> VisVertices = new TtCpu2GpuBuffer<FVgVisVertex>();
        /// <summary>cluster 局部索引(相对 BaseVertex, draw 端 VS 拉取)。</summary>
        public TtCpu2GpuBuffer<uint> VisIndices = new TtCpu2GpuBuffer<uint>();
        /// <summary>原始 cluster 表(cull 与 draw 端共读; 单份, bake 后不变)。</summary>
        public TtCpu2GpuBuffer<FVgVisCluster> VisClusters = new TtCpu2GpuBuffer<FVgVisCluster>();
        /// <summary>group 表(cull 端 LOD 选择)。</summary>
        public TtCpu2GpuBuffer<FVgVisGroup> VisGroups = new TtCpu2GpuBuffer<FVgVisGroup>();
        /// <summary>乒乓: cull 输出的压缩可见 cluster 下标(容量 = cluster 总数)。</summary>
        public TtGpuBuffer<uint>[] ClustersBuffer = new TtGpuBuffer<uint>[2];
        /// <summary>乒乓: DrawIndexedInstancedIndirect 参数(5 uint)。</summary>
        public TtGpuBuffer<uint>[] ArgsBuffer = new TtGpuBuffer<uint>[2];

        public TtGpuBuffer<uint> WriteClusters => ClustersBuffer[mWriteIndex];
        public TtGpuBuffer<uint> WriteArgs => ArgsBuffer[mWriteIndex];
        /// <summary>主图绘制读的"上一帧"产物。</summary>
        public TtGpuBuffer<uint> ReadClusters => ClustersBuffer[mWriteIndex ^ 1];
        public TtGpuBuffer<uint> ReadArgs => ArgsBuffer[mWriteIndex ^ 1];
        private int mWriteIndex = 0;

        public uint ClusterCount => mVisParams.mVis.Y > 0 ? (uint)mVisParams.mVis.Y : 0;
        public bool HasAsset => ClusterCount > 0;
        #endregion

        /// <summary>LOD 选择阈值: 屏幕空间竖直误差(像素)。</summary>
        public float PixelErrorThreshold = 1.0f;
        /// <summary>主视口像素高度(像素误差换算用), 由绘制节点 OnResize 同步。</summary>
        public float ViewportHeight = 1080.0f;

        private TtRenderPolicy mPreparePolicy;
        private TtRCmdQueue mCmdQueue;
        private TtVgDagPrepareNode mPrepareNode;
        private bool mDataDirty = false;

        public void Dispose()
        {
            // 先拆图(节点持有的 drawcall 可能引用下面的 buffer), 再释放 buffer
            CoreSDK.DisposeObject(ref mPreparePolicy);
            mPrepareNode = null;
            CoreSDK.DisposeObject(ref CBVisParams);
            CoreSDK.DisposeObject(ref VisVertices);
            CoreSDK.DisposeObject(ref VisIndices);
            CoreSDK.DisposeObject(ref VisClusters);
            CoreSDK.DisposeObject(ref VisGroups);
            CoreSDK.DisposeObject(ref ClustersBuffer[0]);
            CoreSDK.DisposeObject(ref ClustersBuffer[1]);
            CoreSDK.DisposeObject(ref ArgsBuffer[0]);
            CoreSDK.DisposeObject(ref ArgsBuffer[1]);
            CoreSDK.DisposeObject(ref mCmdQueue);
        }

        /// <summary>
        /// 上传一份 bake 好的 DAG 资产。origin 为高度场原点(须已转局部坐标, 见
        /// CodingGuidelines §1.5), texelWorldSize 为单个高度场格点的世界尺寸。
        /// </summary>
        public unsafe void UploadDagAsset(TtVgDagAsset asset, Vector3 origin, float texelWorldSize)
        {
            if (VisVertices == null)
                return; // 已 Dispose
            if (!TtVgDagGpuData.Build(asset, out var visClusters, out var visGroups, out var visVerts, out var visIndices))
                return;
            int clusterCount = visClusters.Length;

            // Initialize 会释放旧 GPU buffer 并重建 CPU 数组, 重复上传安全
            VisClusters.Initialize(EBufferType.BFT_SRV);
            VisGroups.Initialize(EBufferType.BFT_SRV);
            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);

            VisClusters.SetSize(clusterCount);
            fixed (FVgVisCluster* pC = &visClusters[0])
            {
                VisClusters.UpdateData(0, pC, clusterCount * sizeof(FVgVisCluster));
            }
            VisGroups.SetSize(visGroups.Length);
            fixed (FVgVisGroup* pG = &visGroups[0])
            {
                VisGroups.UpdateData(0, pG, visGroups.Length * sizeof(FVgVisGroup));
            }
            VisVertices.SetSize(visVerts.Length);
            fixed (FVgVisVertex* pV = &visVerts[0])
            {
                VisVertices.UpdateData(0, pV, visVerts.Length * sizeof(FVgVisVertex));
            }
            VisIndices.SetSize(visIndices.Length);
            fixed (uint* pI = &visIndices[0])
            {
                VisIndices.UpdateData(0, pI, visIndices.Length * sizeof(uint));
            }

            mVisParams.mOriginAndTexel = new Vector4(origin.X, origin.Y, origin.Z, texelWorldSize);
            // z/w 在 TickPrepare 里按相机重算; 先填非 0 值避免除零
            mVisParams.mVis = new Vector4(PixelErrorThreshold, clusterCount, 1, 1);

            // 显式初值全 0(InstanceCount=0): 首帧 ExecuteIndirect 执行 0 个 draw, 天然安全
            uint* zeroArgs = stackalloc uint[5] { 0, 0, 0, 0, 0 };
            for (int i = 0; i < 2; i++)
            {
                CoreSDK.DisposeObject(ref ClustersBuffer[i]);
                ClustersBuffer[i] = new TtGpuBuffer<uint>();
                ClustersBuffer[i].SetSize((uint)clusterCount, (void*)0,
                    EBufferType.BFT_SRV | EBufferType.BFT_UAV);
                CoreSDK.DisposeObject(ref ArgsBuffer[i]);
                ArgsBuffer[i] = new TtGpuBuffer<uint>();
                ArgsBuffer[i].SetSize(5, zeroArgs,
                    EBufferType.BFT_UAV | EBufferType.BFT_IndirectArgs);
            }
            mWriteIndex = 0;

            mDataDirty = true;
        }

        /// <summary>
        /// 构建独立迷你 RenderPolicy(不含 ending 之外任何主图概念),
        /// SWEWaterNode.InitSWEPolicy 同款: 必须有 RootNode 且 cull 节点
        /// 经 linker 连到 ending, 否则 BuildGraph 回溯不到、Tick 被跳过。
        /// 须在渲染前的 Initialize 阶段 await 完成(渲染线程禁 fire-and-forget)。
        /// </summary>
        public async Thread.Async.TtTask InitializeAsync()
        {
            if (mPreparePolicy != null)
                return;

            BuildPrepareGraph(out var hasInputError);
            if (hasInputError)
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "VgDagFarTerrain: prepare graph has unlinked inputs");
            mCmdQueue = new TtRCmdQueue();
            mPreparePolicy.CmdQueue = mCmdQueue;

            foreach (var kvp in mPreparePolicy.GraphNodes)
            {
                await kvp.Value.Initialize(mPreparePolicy, kvp.Value.Name);
            }
        }

        /// <summary>
        /// 只建图不建 GPU 资源(不创建命令队列、不 Initialize 节点), 单测可直接验证
        /// prepare 节点能从 RootNode 回溯到(IsUsed)、NodeLayers 已建立。
        /// </summary>
        internal TtRenderPolicy BuildPrepareGraph(out bool hasInputError)
        {
            mPreparePolicy = new TtRenderPolicy();

            mPrepareNode = new TtVgDagPrepareNode();
            mPrepareNode.Host = this;
            mPrepareNode.InitNodePins();
            mPrepareNode.Name = "VgDagPrepare";

            var endingNode = new TtAssitRootNode();
            endingNode.InitNodePins();
            endingNode.Name = "VgDagPrepareEnding";

            mPreparePolicy.RegRenderNode2("VgDagPrepare", mPrepareNode);
            mPreparePolicy.RegRenderNode2("VgDagPrepareEnding", endingNode);
            mPreparePolicy.AddLinker(mPrepareNode.PreparedPinOut, endingNode.SrcPinIn);
            mPreparePolicy.RootNode = endingNode;

            hasInputError = false;
            mPreparePolicy.BuildGraph(ref hasInputError);
            return mPreparePolicy;
        }

        internal TtVgDagPrepareNode PrepareNode => mPrepareNode;

        /// <summary>
        /// 每帧驱动一次(渲染线程, 主 policy Tick 早期调用; 须先 InitializeAsync 完成):
        /// 同步主相机 -> 写侧 clear+cull -> 提交独立命令队列 -> 乒乓交换。
        /// 写侧变成本帧读侧, 主图 gbuffer 里远景 mesh 读到的是刚提交的剔除结果。
        /// 注意: 迷你 policy 走独立 TtRCmdQueue, 与主图之间目前没有显式 fence,
        /// 依赖两者最终提交到同一硬件队列的提交顺序(未经 GPU 运行验证)。
        /// </summary>
        public void TickPrepare(TtRenderPolicy mainPolicy)
        {
            if (HasAsset == false || mPreparePolicy == null)
                return;

            // 同步相机(局部坐标 VP; cbuffer 列主序, CPU 端转置)
            // 用 GPU 状态镜像的 VP (与最近一次 cbPerCamera 写入逐字段一致) ——
            // GPU 剔影用什么相机状态, 绘制 cbPerCamera 就是同一份状态; 读实时相机
            // 会与渲染错位, 大幅快转时屏幕边缘错误裁剪。从未上传过 cb 时退回实时相机。
            if (mainPolicy != null)
            {
                var camera = mainPolicy.DefaultCamera;
                var vp = camera.TryGetGpuViewProjection(out var gpuVP)
                    ? gpuVP : camera.GetViewProjection();
                vp.Transpose();
                mVisParams.mViewProj = vp;
                mVisParams.mVis = TtVgDagGpuData.MakeLodParams(camera, ViewportHeight, PixelErrorThreshold, ClusterCount);
            }

            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "VgDagPrepare.FlushBuffers"))
                {
                    VisClusters.Flush2GPU(tsCmd.CmdList);
                    VisGroups.Flush2GPU(tsCmd.CmdList);
                    VisVertices.Flush2GPU(tsCmd.CmdList);
                    VisIndices.Flush2GPU(tsCmd.CmdList);
                }
                mDataDirty = false;
            }

            mPreparePolicy.BeginTick(null);
            mPreparePolicy.Tick(null, null);
            mPreparePolicy.EndTick(null);
            mPreparePolicy.ExecuteCmdQueue(false);

            // 乒乓交换: 刚写完的一侧交给绘制, 上一帧读侧交还给 cull 覆写
            mWriteIndex ^= 1;
        }
    }
}
