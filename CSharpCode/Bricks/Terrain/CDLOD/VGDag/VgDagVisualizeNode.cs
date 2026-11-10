using EngineNS.Bricks.Terrain.CDLOD.VGDag;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// cluster DAG 调试可视化的 GPU 数据(经 TtShaderDefine 反射注入 HLSL, 字段剥 m 前缀)。
    /// 只覆盖显式顶点池的 cluster(Level >= 1); 叶子(Level 0)是隐式网格, 其覆盖区域
    /// 由父层 cluster 的几何完全覆盖, 调试视图无需单独绘制。
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVgVisVertex")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FVgVisVertex
    {
        public uint mTexelX;
        public uint mTexelZ;
        public float mY;
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVgVisCluster")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVgVisCluster
    {
        public uint mMinTexelX;
        public uint mMinTexelZ;
        public uint mExtentTexel;
        public uint mLevel;
        public uint mBaseVertex;
        public uint mIndexStart;
        public uint mIndexCount;
        public float mMaxError;
        public int mParent;              // -1 = 根
        public float mParentMaxError;    // 根填 0(恒 <= 阈值即可见)
        public uint mHeightfieldSize;
        /// <summary>显式层 cluster 顶点数(规则 grid, 可由索引数反推); 叶子为 0。</summary>
        public uint mVertCount;
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVgVisParams")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVgVisParams
    {
        public Matrix mViewProj;             // cbuffer 列主序约定, CPU 端转置后上传
        public Vector4 mRTSize;              // xy = 尺寸
        public Vector4 mOriginAndTexel;      // xyz = 高度场原点(局部坐标), w = 每格点世界尺寸
        public Vector4 mVis;                 // x = LOD 误差阈值, y = cluster 总数
    }

    /// <summary>
    /// 地形 cluster DAG 调试可视化节点(M2 第一片):
    /// 把 TtVgClusterBaker 产出的 TtVgDagAsset 上传 GPU, compute 软光栅绘制
    /// "误差阈值 LOD 选择"后可见的显式层 cluster, 按 Level 着色。
    /// 与 Quark 的 QuarkDAGVisualizeNode 同套路(独立 UAV RT + atomic depth),
    /// 可在 RGD 编辑器挂在任意位置预览; 后续 indirect gbuffer 绘制通路基于
    /// 本节点的 cull/LOD 选择逻辑展开。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("VGDagVisualize", "Terrain\\VGDagVisualize", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtVgDagVisualizeNode : TAuxRenderGraphNode<TtVgDagVisualizeNode>
    {
        public TtRenderGraphPin VisRTPinOut = TtRenderGraphPin.CreateOutput("VisRT", true,
            EPixelFormat.PXF_R8G8B8A8_UNORM, EBufferType.BFT_UAV | EBufferType.BFT_SRV);
        public TtRenderGraphPin VisDepthPinOut = TtRenderGraphPin.CreateOutput("VisDepth", true,
            EPixelFormat.PXF_R32_UINT, EBufferType.BFT_UAV);

        /// <summary>编辑器调试模式: 直接指定输出 UAV, 不依赖 RenderGraph 装配。</summary>
        public NxRHI.TtUaView OverrideOutputUav;
        public NxRHI.TtUaView OverrideDepthUav;

        public FVgVisParams mVisParams;
        public TtCbView CBVisParams;

        public TtCpu2GpuBuffer<FVgVisVertex> VisVertices = new TtCpu2GpuBuffer<FVgVisVertex>();
        public TtCpu2GpuBuffer<uint> VisIndices = new TtCpu2GpuBuffer<uint>();
        public TtCpu2GpuBuffer<FVgVisCluster> VisClusters = new TtCpu2GpuBuffer<FVgVisCluster>();

        private TtVgVisClearShading mClearShading;
        private TtComputeDraw mClearDrawcall;
        private TtVgVisRasterShading mRasterShading;
        private TtComputeDraw mRasterDrawcall;

        private bool mDataDirty = false;
        public uint ClusterCount => mVisParams.mVis.Y > 0 ? (uint)mVisParams.mVis.Y : 0;

        /// <summary>LOD 选择阈值(QEM 代价, 量纲 texel²); 0 = 只画最粗层。</summary>
        public float ErrorThreshold = 1600.0f;

        public TtVgDagVisualizeNode()
        {
            Name = "VgDagVisualizeNode";
        }

        public override void InitNodePins()
        {
            AddOutput(VisRTPinOut);
            AddOutput(VisDepthPinOut);
            base.InitNodePins();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
            mVisParams.mRTSize = new Vector4(x, y, 0, 0);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CoreSDK.DisposeObject(ref mClearDrawcall);
            mClearDrawcall = rc.CreateComputeDraw();
            mClearShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVgVisClearShading>();

            CoreSDK.DisposeObject(ref mRasterDrawcall);
            mRasterDrawcall = rc.CreateComputeDraw();
            mRasterShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVgVisRasterShading>();

            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);
            VisClusters.Initialize(EBufferType.BFT_SRV);
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mClearDrawcall);
            CoreSDK.DisposeObject(ref mRasterDrawcall);
            CoreSDK.DisposeObject(ref CBVisParams);
            base.Dispose();
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
            VisIndices.SetSize(asset.Indices.Length);
            fixed (uint* pI = &asset.Indices[0])
            {
                VisIndices.UpdateData(0, pI, asset.Indices.Length * sizeof(uint));
            }

            mVisParams.mOriginAndTexel = new Vector4(origin.X, origin.Y, origin.Z, texelWorldSize);
            mVisParams.mVis = new Vector4(ErrorThreshold, clusterCount, 0, 0);
            mDataDirty = true;
        }

        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            uint clusterCount = ClusterCount;
            if (clusterCount == 0)
                return;

            // camera(局部坐标 VP; cbuffer 列主序, CPU 端转置)
            var vp = policy.DefaultCamera.GetViewProjection();
            vp.Transpose();
            mVisParams.mViewProj = vp;
            mVisParams.mVis = new Vector4(ErrorThreshold, clusterCount, 0, 0);

            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "VgDagVis.FlushBuffers"))
                {
                    VisClusters.Flush2GPU(tsCmd.CmdList);
                    VisVertices.Flush2GPU(tsCmd.CmdList);
                    VisIndices.Flush2GPU(tsCmd.CmdList);
                }
                mDataDirty = false;
            }

            var cmd = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "VgDagVisualize"))
            {
                uint rtW = (uint)mVisParams.mRTSize.X;
                uint rtH = (uint)mVisParams.mRTSize.Y;

                mClearShading.SetDrawcallDispatch(this, policy, mClearDrawcall, rtW, rtH, 1, true);
                cmd.PushGpuDraw(mClearDrawcall);

                mRasterShading.SetDrawcallDispatch(this, policy, mRasterDrawcall, clusterCount, 1, 1, true);
                cmd.PushGpuDraw(mRasterDrawcall);

                cmd.FlushDraws();
            }

            if (policy != null)
                policy.CommitCommandList(cmd, "VgDagVisualize");
            else
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmd, "VgDagVisualize", EQueueType.QU_Compute);
        }
    }

    #region Shading Environments

    public class TtVgVisClearShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtVgVisClearShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Terrain/VGDag/VgDagVisualize.compute", RName.ERNameType.Engine);
            MainName = "CS_ClearVisTexture";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtVgDagVisualizeNode;
            if (node == null)
                return;

            var uav = node.OverrideOutputUav ?? node.GetAttachBuffer(node.VisRTPinOut)?.Uav;
            if (uav != null)
                drawcall.BindUav("VisColorTexture", uav);
            var depthUav = node.OverrideDepthUav ?? node.GetAttachBuffer(node.VisDepthPinOut)?.Uav;
            if (depthUav != null)
                drawcall.BindUav("VisDepthTexture", depthUav);
        }
    }

    public class TtVgVisRasterShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);

        public TtVgVisRasterShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Terrain/VGDag/VgDagVisualize.compute", RName.ERNameType.Engine);
            MainName = "CS_VisualizeClusters";
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtVgDagVisualizeNode;
            if (node == null)
                return;

            drawcall.BindSrv("VisClusterBuffer", node.VisClusters.Srv);
            drawcall.BindSrv("VisVertexBuffer", node.VisVertices.Srv);
            drawcall.BindSrv("VisIndexBuffer", node.VisIndices.Srv);
            var uav = node.OverrideOutputUav ?? node.GetAttachBuffer(node.VisRTPinOut)?.Uav;
            if (uav != null)
                drawcall.BindUav("VisColorTexture", uav);
            var depthUav = node.OverrideDepthUav ?? node.GetAttachBuffer(node.VisDepthPinOut)?.Uav;
            if (depthUav != null)
                drawcall.BindUav("VisDepthTexture", depthUav);

            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbVgVisParams");
            if (binder.IsValidPointer)
            {
                if (node.CBVisParams == null)
                {
                    // §1.1: 创建即全字段 SetValue + MarkDirty + FlushDirty
                    node.CBVisParams = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    node.CBVisParams.SetValue("VgVisParams", in node.mVisParams);
                    node.CBVisParams.MarkDirty();
                    node.CBVisParams.FlushDirty();
                }
                drawcall.BindCBV(binder, node.CBVisParams);
            }
        }
    }

    #endregion
}
