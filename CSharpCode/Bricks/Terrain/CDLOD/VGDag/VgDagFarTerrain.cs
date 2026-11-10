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
    /// cull pass(compute) 做 LOD 选择 + 视锥剔除, 可见 cluster 压缩写入 DrawClusters
    /// 并以 InterlockedAdd 生成 DrawIndexedInstancedIndirect 参数。
    /// 组织方式与 TtSWEWaterNode / TtSceneCapture 同款: new TtRenderPolicy() +
    /// 独立 TtRCmdQueue, 在主 policy Tick 内驱动、与主图 cmdlist 解耦。
    /// 双缓冲乒乓: prepare 写写侧, 主图远景 mesh(gbuffer opaque)读上一帧读侧,
    /// 主图对"本帧正在被绘制的数据"零写竞争。
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

            public TtVgDagPrepareNode()
            {
                Name = "VgDagPrepareNode";
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
        /// <summary>原始 cluster 表(cull 端读; 单份, bake 后不变)。</summary>
        public TtCpu2GpuBuffer<FVgVisCluster> VisClusters = new TtCpu2GpuBuffer<FVgVisCluster>();
        /// <summary>乒乓: cull 输出的压缩可见 cluster(容量 = cluster 总数上界)。</summary>
        public TtGpuBuffer<FVgVisCluster>[] ClustersBuffer = new TtGpuBuffer<FVgVisCluster>[2];
        /// <summary>乒乓: DrawIndexedInstancedIndirect 参数(5 uint)。</summary>
        public TtGpuBuffer<uint>[] ArgsBuffer = new TtGpuBuffer<uint>[2];

        public TtGpuBuffer<FVgVisCluster> WriteClusters => ClustersBuffer[mWriteIndex];
        public TtGpuBuffer<uint> WriteArgs => ArgsBuffer[mWriteIndex];
        /// <summary>主图绘制读的"上一帧"产物。</summary>
        public TtGpuBuffer<FVgVisCluster> ReadClusters => ClustersBuffer[mWriteIndex ^ 1];
        public TtGpuBuffer<uint> ReadArgs => ArgsBuffer[mWriteIndex ^ 1];
        private int mWriteIndex = 0;

        public uint ClusterCount => mVisParams.mVis.Y > 0 ? (uint)mVisParams.mVis.Y : 0;
        public bool HasAsset => ClusterCount > 0;
        #endregion

        /// <summary>LOD 选择阈值(QEM 代价, 量纲 texel²)。</summary>
        public float ErrorThreshold = 1600.0f;

        private TtRenderPolicy mPreparePolicy;
        private TtRCmdQueue mCmdQueue;
        private TtVgDagPrepareNode mPrepareNode;
        private bool mDataDirty = false;

        public void Dispose()
        {
            CoreSDK.DisposeObject(ref CBVisParams);
            CoreSDK.DisposeObject(ref VisVertices);
            CoreSDK.DisposeObject(ref VisClusters);
            CoreSDK.DisposeObject(ref ClustersBuffer[0]);
            CoreSDK.DisposeObject(ref ClustersBuffer[1]);
            CoreSDK.DisposeObject(ref ArgsBuffer[0]);
            CoreSDK.DisposeObject(ref ArgsBuffer[1]);
            CoreSDK.DisposeObject(ref mPreparePolicy);
            CoreSDK.DisposeObject(ref mCmdQueue);
            mPrepareNode = null;
        }

        /// <summary>
        /// 上传一份 bake 好的 DAG 资产。origin 为高度场原点(须已转局部坐标, 见
        /// CodingGuidelines §1.5), texelWorldSize 为单个高度场格点的世界尺寸。
        /// </summary>
        public unsafe void UploadDagAsset(TtVgDagAsset asset, Vector3 origin, float texelWorldSize)
        {
            if (asset == null || asset.Clusters == null || asset.Clusters.Length == 0)
                return;

            var clusters = asset.Clusters;
            int clusterCount = clusters.Length;

            // 父链映射: 供 shader 做可见性判定(自身误差 <= 阈值 && 父误差 > 阈值)
            var parentOf = new int[clusterCount];
            for (int i = 0; i < clusterCount; i++)
                parentOf[i] = -1;
            for (int i = 0; i < clusterCount; i++)
            {
                var c = clusters[i];
                if (c.IsLeaf)
                    continue;
                for (int k = 0; k < 4; k++)
                {
                    int child = c.FirstChild + k;
                    if (child >= 0 && child < clusterCount)
                        parentOf[child] = i;
                }
            }

            var visClusters = new FVgVisCluster[clusterCount];
            for (int i = 0; i < clusterCount; i++)
            {
                var c = clusters[i];
                ref var g = ref visClusters[i];
                g.mMinTexelX = (uint)c.MinTexelX;
                g.mMinTexelZ = (uint)c.MinTexelZ;
                g.mExtentTexel = (uint)c.ExtentTexel;
                g.mLevel = (uint)c.Level;
                g.mBaseVertex = (uint)c.BaseVertex;
                g.mIndexStart = (uint)c.IndexStart;
                g.mIndexCount = (uint)c.IndexCount;
                g.mMaxError = c.MaxError;
                g.mParent = parentOf[i];
                g.mParentMaxError = parentOf[i] >= 0 ? clusters[parentOf[i]].MaxError : 0.0f;
                g.mHeightfieldSize = (uint)asset.HeightfieldSize;
                // 显式层 cluster 的顶点数可由索引数反推: 规则 grid 三角数 = (side-1)^2 * 2
                g.mVertCount = 0;
                if (c.IndexCount > 0)
                {
                    int quads = (int)System.Math.Sqrt(c.IndexCount / 6.0);
                    g.mVertCount = (uint)(quads + 1);
                }
            }

            var visVerts = new FVgVisVertex[asset.Vertices.Length];
            for (int i = 0; i < asset.Vertices.Length; i++)
            {
                visVerts[i].mTexelX = asset.Vertices[i].TexelX;
                visVerts[i].mTexelZ = asset.Vertices[i].TexelZ;
                visVerts[i].mY = asset.Vertices[i].Y;
            }

            VisClusters.SetSize(clusterCount);
            fixed (FVgVisCluster* pC = &visClusters[0])
            {
                VisClusters.UpdateData(0, pC, clusterCount * sizeof(FVgVisCluster));
            }
            VisVertices.SetSize(visVerts.Length);
            fixed (FVgVisVertex* pV = &visVerts[0])
            {
                VisVertices.UpdateData(0, pV, visVerts.Length * sizeof(FVgVisVertex));
            }

            mVisParams.mOriginAndTexel = new Vector4(origin.X, origin.Y, origin.Z, texelWorldSize);
            mVisParams.mVis = new Vector4(ErrorThreshold, clusterCount, asset.HeightMax, 0);

            for (int i = 0; i < 2; i++)
            {
                CoreSDK.DisposeObject(ref ClustersBuffer[i]);
                ClustersBuffer[i] = new TtGpuBuffer<FVgVisCluster>();
                ClustersBuffer[i].SetSize((uint)clusterCount, (void*)0,
                    EBufferType.BFT_SRV | EBufferType.BFT_UAV);
                CoreSDK.DisposeObject(ref ArgsBuffer[i]);
                ArgsBuffer[i] = new TtGpuBuffer<uint>();
                // 初始全 0(InstanceCount=0): 首帧 ExecuteIndirect 执行 0 个 draw, 天然安全
                ArgsBuffer[i].SetSize(5, (void*)0,
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

            mPreparePolicy = new TtRenderPolicy();
            mCmdQueue = new TtRCmdQueue();
            mPreparePolicy.CmdQueue = mCmdQueue;

            mPrepareNode = new TtVgDagPrepareNode();
            mPrepareNode.Host = this;
            mPrepareNode.InitNodePins();
            mPrepareNode.Name = "VgDagPrepare";

            var endingNode = new TtAssitRootNode();
            endingNode.InitNodePins();
            endingNode.Name = "VgDagPrepareEnding";

            mPreparePolicy.RegRenderNode2("VgDagPrepare", mPrepareNode);
            mPreparePolicy.RegRenderNode2("VgDagPrepareEnding", endingNode);

            bool hasInputError = false;
            mPreparePolicy.BuildGraph(ref hasInputError);

            foreach (var kvp in mPreparePolicy.GraphNodes)
            {
                await kvp.Value.Initialize(mPreparePolicy, kvp.Value.Name);
            }
        }

        /// <summary>
        /// 每帧驱动一次(渲染线程, 主 policy Tick 早期调用; 须先 InitializeAsync 完成):
        /// 同步主相机 -> 写侧 clear+cull -> 提交独立命令队列 -> 乒乓交换。
        /// 写侧变成本帧读侧, 主图 gbuffer 里远景 mesh 读到的是刚完成的剔除结果
        /// (若后续把驱动点移到更早/独立线程, 读侧天然退回"上一帧"语义, 无需改动)。
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
                var vp = mainPolicy.DefaultCamera.TryGetGpuViewProjection(out var gpuVP)
                    ? gpuVP : mainPolicy.DefaultCamera.GetViewProjection();
                vp.Transpose();
                mVisParams.mViewProj = vp;
                mVisParams.mVis.X = ErrorThreshold;
            }

            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "VgDagPrepare.FlushBuffers"))
                {
                    VisClusters.Flush2GPU(tsCmd.CmdList);
                    VisVertices.Flush2GPU(tsCmd.CmdList);
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
