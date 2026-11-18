using EngineNS.Bricks.Terrain.CDLOD.VGDag;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// cluster DAG 的 GPU 数据(经 TtShaderDefine 反射注入 HLSL, 字段剥 m 前缀)。
    /// 布局与 TtVgDagAsset 一一对应: 顶点池 + 每 cluster 局部 ushort 索引(GPU 侧扩成 uint)
    /// + group 表; LOD 选择逻辑见 VgDagLod.cginc。
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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FVgVisCluster
    {
        public uint mBaseVertex;
        public uint mIndexStart;
        public uint mIndexCount;
        public uint mMipLevel;
        public int mGeneratingGroup;     // -1 = 叶子
        public int mParentGroup;         // -1 = 根
        public float mMinY;
        public float mMaxY;
        public uint mMinTexelX;
        public uint mMinTexelZ;
        public uint mMaxTexelX;
        public uint mMaxTexelZ;
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVgVisGroup")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FVgVisGroup
    {
        public float mParentLODError;    // 高度单位, 沿 DAG 单调
        public float mMinY;
        public float mMaxY;
        public uint mMipLevel;
        public uint mMinTexelX;
        public uint mMinTexelZ;
        public uint mMaxTexelX;
        public uint mMaxTexelZ;
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVgVisParams")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVgVisParams
    {
        public Matrix mViewProj;             // cbuffer 列主序约定, CPU 端转置后上传
        public Vector4 mRTSize;              // xy = 尺寸
        public Vector4 mOriginAndTexel;      // xyz = 高度场原点(局部坐标), w = 每格点世界尺寸
        /// <summary>
        /// x = 像素误差阈值 τ, y = cluster 总数,
        /// z = screenScale(P.M22 * 视口高 / 2), w = 近平面距离(透视) / -1(正交)。
        /// </summary>
        public Vector4 mVis;
    }

    /// <summary>TtVgDagAsset → GPU 数组的转换与 LOD 参数计算(可视化节点与远景绘制共用)。</summary>
    public static class TtVgDagGpuData
    {
        /// <summary>
        /// 每 cluster 的最大索引数 = TtQuarkTerrainBaker.ClusterSize(128 三角) * 3;
        /// 必须与 VgDagLod.cginc 的 VG_CLUSTER_MAX_INDEX 一致(indirect 每实例索引数)。
        /// </summary>
        public const int MaxClusterIndexCount = 384;

        public static bool Build(TtVgDagAsset asset, out FVgVisCluster[] clusters, out FVgVisGroup[] groups,
            out FVgVisVertex[] vertices, out uint[] indices)
        {
            clusters = null;
            groups = null;
            vertices = null;
            indices = null;
            if (asset == null || asset.Clusters == null || asset.Clusters.Length == 0 ||
                asset.Vertices == null || asset.Indices == null || asset.Groups == null)
                return false;

            clusters = new FVgVisCluster[asset.Clusters.Length];
            for (int i = 0; i < clusters.Length; i++)
            {
                var c = asset.Clusters[i];
                if (c.IndexCount > MaxClusterIndexCount)
                {
                    // 超出 patch 容量会被截断成空洞, 宁可整份拒绝
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning,
                        $"VGDag: cluster {i} has {c.IndexCount} indices > {MaxClusterIndexCount}, upload rejected");
                    clusters = null;
                    return false;
                }
                ref var g = ref clusters[i];
                g.mBaseVertex = (uint)c.BaseVertex;
                g.mIndexStart = (uint)c.IndexStart;
                g.mIndexCount = (uint)c.IndexCount;
                g.mMipLevel = (uint)c.MipLevel;
                g.mGeneratingGroup = c.GeneratingGroup;
                g.mParentGroup = c.ParentGroup;
                g.mMinY = c.MinY;
                g.mMaxY = c.MaxY;
                g.mMinTexelX = c.MinTexelX;
                g.mMinTexelZ = c.MinTexelZ;
                g.mMaxTexelX = c.MaxTexelX;
                g.mMaxTexelZ = c.MaxTexelZ;
            }

            // 空数组无法建 SRV, 补一个永不被引用的占位(无 group 意味着单根叶子)
            groups = new FVgVisGroup[System.Math.Max(1, asset.Groups.Length)];
            for (int i = 0; i < asset.Groups.Length; i++)
            {
                var s = asset.Groups[i];
                ref var g = ref groups[i];
                g.mParentLODError = s.ParentLODError;
                g.mMinY = s.MinY;
                g.mMaxY = s.MaxY;
                g.mMipLevel = (uint)s.MipLevel;
                g.mMinTexelX = s.MinTexelX;
                g.mMinTexelZ = s.MinTexelZ;
                g.mMaxTexelX = s.MaxTexelX;
                g.mMaxTexelZ = s.MaxTexelZ;
            }

            vertices = new FVgVisVertex[asset.Vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].mTexelX = asset.Vertices[i].TexelX;
                vertices[i].mTexelZ = asset.Vertices[i].TexelZ;
                vertices[i].mY = asset.Vertices[i].Y;
            }

            indices = new uint[asset.Indices.Length];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = asset.Indices[i];
            return true;
        }

        /// <summary>
        /// 由相机与视口高度算 mVis。投影误差 = ParentLODError * screenScale / max(minClipW, near),
        /// 正交时分母恒 1 (w = -1 作标记)。
        /// </summary>
        public static Vector4 MakeLodParams(Graphics.Pipeline.TtCamera camera, float viewportHeight,
            float pixelErrorThreshold, uint clusterCount)
        {
            var proj = camera.GetProjectionMatrix();
            float screenScale = System.Math.Abs(proj.M22) * System.Math.Max(viewportHeight, 1.0f) * 0.5f;
            float near = camera.IsOrtho ? -1.0f : System.Math.Max(camera.ZNear, 1e-4f);
            return new Vector4(pixelErrorThreshold, clusterCount, screenScale, near);
        }
    }

    /// <summary>
    /// 地形 cluster DAG 调试可视化节点:
    /// 把 TtQuarkTerrainBaker 产出的 TtVgDagAsset 上传 GPU, compute 软光栅绘制
    /// 组一致 LOD 选择(屏幕空间像素误差阈值)后的 cluster cut, 按 MipLevel 着色。
    /// 与 Quark 的 QuarkDAGVisualizeNode 同套路(独立 UAV RT + atomic depth),
    /// 可在 RGD 编辑器挂在任意位置预览。
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
        public TtCpu2GpuBuffer<FVgVisGroup> VisGroups = new TtCpu2GpuBuffer<FVgVisGroup>();

        private TtVgVisClearShading mClearShading;
        private TtComputeDraw mClearDrawcall;
        private TtVgVisRasterShading mRasterShading;
        private TtComputeDraw mRasterDrawcall;

        private bool mDataDirty = false;
        public uint ClusterCount => mVisParams.mVis.Y > 0 ? (uint)mVisParams.mVis.Y : 0;

        /// <summary>LOD 选择阈值: 屏幕空间竖直误差(像素); 越小越精细, 负值 = 只画叶子。</summary>
        public float PixelErrorThreshold = 1.0f;

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

            // Dispose 后可能被重新 Initialize
            VisVertices ??= new TtCpu2GpuBuffer<FVgVisVertex>();
            VisIndices ??= new TtCpu2GpuBuffer<uint>();
            VisClusters ??= new TtCpu2GpuBuffer<FVgVisCluster>();
            VisGroups ??= new TtCpu2GpuBuffer<FVgVisGroup>();
            VisVertices.Initialize(EBufferType.BFT_SRV);
            VisIndices.Initialize(EBufferType.BFT_SRV);
            VisClusters.Initialize(EBufferType.BFT_SRV);
            VisGroups.Initialize(EBufferType.BFT_SRV);
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mClearDrawcall);
            CoreSDK.DisposeObject(ref mRasterDrawcall);
            CoreSDK.DisposeObject(ref CBVisParams);
            CoreSDK.DisposeObject(ref VisVertices);
            CoreSDK.DisposeObject(ref VisIndices);
            CoreSDK.DisposeObject(ref VisClusters);
            CoreSDK.DisposeObject(ref VisGroups);
            base.Dispose();
        }

        /// <summary>
        /// 上传一份 bake 好的 DAG 资产。origin 为高度场原点(须已转局部坐标, 见
        /// CodingGuidelines §1.5), texelWorldSize 为单个高度场格点的世界尺寸。
        /// </summary>
        public unsafe void UploadDagAsset(TtVgDagAsset asset, Vector3 origin, float texelWorldSize)
        {
            if (!TtVgDagGpuData.Build(asset, out var visClusters, out var visGroups, out var visVerts, out var visIndices))
                return;
            int clusterCount = visClusters.Length;

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
            mVisParams.mVis = new Vector4(PixelErrorThreshold, clusterCount, 1, 1);
            mDataDirty = true;
        }

        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            uint clusterCount = ClusterCount;
            if (clusterCount == 0)
                return;
            // OnResize 之前 RT 尺寸为 0: 像素误差换算退化(全选叶子)且没有输出目标, 整帧跳过
            if (mVisParams.mRTSize.X < 1 || mVisParams.mRTSize.Y < 1)
                return;

            // camera(局部坐标 VP; cbuffer 列主序, CPU 端转置)
            var camera = policy.DefaultCamera;
            var vp = camera.GetViewProjection();
            vp.Transpose();
            mVisParams.mViewProj = vp;
            mVisParams.mVis = TtVgDagGpuData.MakeLodParams(camera, mVisParams.mRTSize.Y, PixelErrorThreshold, clusterCount);

            if (mDataDirty)
            {
                using (var tsCmd = new FTransientCmd(EQueueType.QU_Default, "VgDagVis.FlushBuffers"))
                {
                    VisClusters.Flush2GPU(tsCmd.CmdList);
                    VisGroups.Flush2GPU(tsCmd.CmdList);
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
            drawcall.BindSrv("VisGroupBuffer", node.VisGroups.Srv);
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
                else
                {
                    node.CBVisParams.SetValue("VgVisParams", in node.mVisParams);
                }
                drawcall.BindCBV(binder, node.CBVisParams);
            }
        }
    }

    #endregion
}
