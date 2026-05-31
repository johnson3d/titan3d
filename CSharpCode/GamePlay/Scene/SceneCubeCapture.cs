using EngineNS.Bricks.WorldSimulator;
using EngineNS.Editor.Forms;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static EngineNS.GamePlay.TtAxis;

namespace EngineNS.GamePlay.Scene
{
    // -------------------------------------------------------------------------
    // TtSceneCubeCapture
    //   场景中的"动态 CubeMap 拍摄节点". 用法类似 TtSceneCapture, 但输出
    //   是一张 6 面的 TextureCube, 适合做水面反射 / 局部环境探针 / 动态 IBL
    //   的素材源.
    //
    // 设计要点:
    //   1. 1 个 RenderPolicy 实例 + 1 个相机, 6 面循环 LookAt 切相机, 每面跑
    //      一次 RP, 完成后用 TtCopyDraw 把 RP 的 final RT (sub=0) 拷到
    //      cube 的第 face 面 (sub=face). 零侵入 RP, 仅多 6 次拷贝.
    //   2. Volume = OBB (节点自身 Placement.AbsTransform + VolumeExtent
    //      半边长). 默认只把 Volume 内的 TtVisual 纳入拍摄, 同时 OnlyShowNodes
    //      列表里的节点强制参与 (不受 Volume 限制).
    //   3. CaptureInterval 单字段语义: >0 按秒间隔; =0 每帧拍; <0 仅手动.
    //   4. 编辑器预览 (IRootForm) 暂不在本期实现, 留空 OnDraw 方便后续接入.
    //
    // -------------------------------------------------------------------------
    [Bricks.CodeBuilder.ContextMenu("CubeCapture", "Graphics\\CubeCapture", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSceneCubeCapture.TtSceneCubeCaptureData), DefaultNamePrefix = "CubeCapture")]
    [Rtti.Meta("")]
    public partial class TtSceneCubeCapture : TtSceneActorNode
    {
        public enum ECaptureMode
        {
            Normal,             // 拍 Volume 内所有 TtVisual + OnlyShowNodes 列表
            OnlyShowNodes,      // 只拍 OnlyShowNodes 列表里的节点 (忽略 Volume)
            ExcludeNodes,       // 拍 Volume 内除 ExcludeActors 之外的所有 TtVisual
        }

        [Rtti.Meta("")]
        public class TtSceneCubeCaptureData : TtNodeData
        {
            // 用哪个 RenderPolicy. 推荐指定一个轻量 RP (无 TAA / SSR / Bloom),
            // 否则 6 面 × 完整后效会很贵.
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
            public RName RPolicyName { get; set; }

            // 单面分辨率 (cube 强制方形). 256~512 适合水面反射.
            [Rtti.Meta("")]
            public uint CubeFaceSize { get; set; } = 256;

            [Rtti.Meta("")]
            public ECaptureMode CaptureMode { get; set; } = ECaptureMode.Normal;

            [Rtti.Meta("")]
            public List<Guid> ShowActors { get; set; } = new List<Guid>();

            [Rtti.Meta("")]
            public List<Guid> ExcludeActors { get; set; } = new List<Guid>();

            // 拍摄间隔 (秒). >0 按秒间隔; =0 每帧拍; <0 仅手动 CaptureNow().
            [Rtti.Meta("")]
            public float CaptureInterval { get; set; } = float.MaxValue;

            // Volume OBB 的半边长 (节点局部空间). 节点的 RefAABB 就是这个 OBB
            // 的 AABB 表示 (用于 scene cull / pick).
            [Rtti.Meta("")]
            public Vector3 VolumeExtent { get; set; } = new Vector3(50, 50, 50);

            // 是否仅拍 Volume 内的 TtVisual. false = Volume 范围被忽略,
            // 拍 world 内的所有 TtVisual (相当于全场 cube capture).
            [Rtti.Meta("")]
            public bool CaptureOnlyVisualsInVolume { get; set; } = true;

            [Rtti.Meta("")]
            public float NearPlane { get; set; } = 0.1f;

            [Rtti.Meta("")]
            public float FarPlane { get; set; } = 1000.0f;

            // 注意: 不再暴露 CaptureHDR. cube format 必须等于 RP final RT 的真实
            // 格式 (D3D12 CopyTextureRegion 是物理字节拷贝, bpp 不一致直接崩
            // #874 COPYTEXTUREREGION_FORMATMISMATCH), 由 CubeWorldRenderer 在
            // 第一次拍摄时从 RootNode.ColorAttachement.Format 反查决定. 用户能
            // 控制的 HDR/LDR 应该通过换一个 HDR 输出的 RP (RPolicyName) 而不是
            // 在 cube 这一层硬转格式.
        }

        // ---------- OnlyShowNodes (运行期解析后的节点引用) ----------
        public List<TtNode> OnlyShowNodes { get; } = new List<TtNode>();

        [Rtti.Meta("")]
        public void AddOnlyShowNode(TtNode node)
        {
            if (node != null && OnlyShowNodes.Contains(node) == false)
                OnlyShowNodes.Add(node);
        }
        [Rtti.Meta("")]
        public void RemoveOnlyShowNode(TtNode node)
        {
            if (node != null)
                OnlyShowNodes.Remove(node);
        }
        [Rtti.Meta("")]
        public void ClearOnlyShowNodes() => OnlyShowNodes.Clear();

        // ---------- 6 面 cube 渲染封装 ----------
        // 用普通字段而不是 { get; } 只读属性, 因为 CoreSDK.DisposeObject 签名是
        // ref T, 必须能传 ref 进去把字段置空.
        TtCubeWorldRenderer mCubeRenderer = new TtCubeWorldRenderer();
        public TtCubeWorldRenderer CubeRenderer => mCubeRenderer;

        // 给下游 (例如水面反射) 直接消费的 cube SRV. CubeRenderer 内部维护.
        public TtSrView CubeSrv => CubeRenderer.CubeSrv;
        public bool IsReady => CubeRenderer.IsReady;
        public Vector3 CaptureCenterF
        {
            get
            {
                return this.Placement.AbsTransform.mPosition.ToSingleVector3();
            }
        }

        // ---------- 间隔节流 ----------
        float mAccumulatedTime = 0;
        bool mManualTriggerPending = false;

        // 立即在下一次 TickLogic 触发一次 cube 拍摄 (忽略 interval).
        // 暴露给 Macross / 编辑器 / 任意 C# 调用方手动触发.
        [Category("Capture")]
        public bool CaptureNow
        {
            get
            {
                return false;
            }
            set
            {
                mManualTriggerPending = true;
            }
        }

        // 与 CaptureNow 相同, 但额外让 CubeRenderer 在这次拍摄时打开 RenderDoc
        // 抓帧 (一次性, 抓完自动关). 用于 detail 面板里"我现在要 debug 这次
        // cube 拍摄的 GPU 行为" 场景 — 点一下按钮就在 RenderDoc UI 拿到
        // 包含 6 面渲染 + 6 次 face copy 的完整 capture.
        [Category("Capture")]
        public bool CaptureWithRenderDoc
        {
            get
            {
                return false;
            }
            set
            {
                mManualTriggerPending = true;
                if (CubeRenderer != null)
                    CubeRenderer.CaptureRenderDocNextFrame = true;
            }
        }

        Editor.Forms.TtTextureViewer mTextureViewer = null;
        [Category("Capture")]
        public bool OpenInTextureViewer
        {
            get
            {
                if (mTextureViewer != null && mTextureViewer.Visible == false)
                    mTextureViewer = null;
                return mTextureViewer != null;
            }
            set
            {
                if (CubeSrv == null)
                    return;
                if (mTextureViewer == null)
                {
                    mTextureViewer = new Editor.Forms.TtTextureViewer();
                }
                CubeSrv.AssetName = RName.GetRName($"@SceneCubeCapture:{this.NodeId}@", RName.ERNameType.Transient);
                Editor.TtAssetEditorManager.TryOpenEditor(mTextureViewer,
                    CubeSrv.AssetName, 
                    CubeSrv, false).AddWaitTask();
            }
        }
        #region DebugMesh
        Graphics.Mesh.TtRenderMesh mDebugMesh;
        public Graphics.Mesh.TtRenderMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(
                        RName.GetRName("mesh/utility/sm_cinecam.ums", RName.ERNameType.Engine)).GetResultUntilCompleted();
                    if (cookedMesh == null)
                        return null;
                    var mesh2 = new Graphics.Mesh.TtRenderMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, 
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok1)
                    {
                        mesh2.IsAcceptShadow = false;
                        mDebugMesh = mesh2;

                        mDebugMesh.HostNode = this;

                        BoundVolume.LocalAABB = mDebugMesh.MaterialMesh.AABB;

                        this.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();
                    }
                }
                return mDebugMesh;
            }
        }
        public override bool HashVisual => true;
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if ((rp.CullFilters & TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) == 0)
            {
                return;
            }
            if (DebugMesh != null)
                rp.AddVisibleMesh(DebugMesh);
        }
        public override void GetHitProxyDrawMesh(List<TtRenderMesh> meshes)
        {
            base.GetHitProxyDrawMesh(meshes);
            if (DebugMesh != null)
                meshes.Add(DebugMesh);
        }
        public override void OnHitProxyChanged()
        {
            if (mDebugMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mDebugMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                mDebugMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mDebugMesh.SetHitproxy(in value);
            }
            else
            {
                mDebugMesh.IsDrawHitproxy = false;
            }
        }
        #endregion
        // ---------- 初始化 ----------
        protected override async Thread.Async.TtTask<bool> InitializeNode(
            TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var nd = GetNodeData<TtSceneCubeCaptureData>();
            if (nd.RPolicyName == null)
                nd.RPolicyName = TtEngine.Instance.Config.MainRPolicyName;

            // 每个 face 必须从独立加载的 asset 创建 RenderPolicy.
            // CreateRenderPolicy 内部 RegRenderNode 会把 PolicyGraph 里的节点实例
            // 的 RenderGraph 引用设为当前 RP; 如果 6 个 RP 共用同一个 rpAsset,
            // 它们共享同一批节点对象, 最后一个 RP 会覆盖前面所有 RP 的节点引用,
            // 导致 Tick 时 this != j.RenderGraph 断言失败.
            var policies = new TtRenderPolicy[TtCubeWorldRenderer.kFaceCount];
            for (int f = 0; f < TtCubeWorldRenderer.kFaceCount; f++)
            {
                var policy = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(nd.RPolicyName, null);
                if (policy == null)
                    continue;
                await policy.Initialize(null);
                policies[f] = policy;
            }

            CubeRenderer.Initialize(world, policies, nd.CubeFaceSize);

            // 解析 ShowActors -> OnlyShowNodes (运行期节点引用)
            this.OnlyShowNodes.Clear();
            if (nd.ShowActors != null)
            {
                foreach (var id in nd.ShowActors)
                {
                    var n = world.Root.FindNode(id, true);
                    if (n != null)
                        this.OnlyShowNodes.Add(n);
                }
            }

            UpdateCubeCameras();

            // 为每个 face RP 的 CpuCullingNode 注入 culling 钩子.
            for (int fi = 0; fi < TtCubeWorldRenderer.kFaceCount; fi++)
            {
                var faceRP = CubeRenderer.RenderPolicies?[fi];
                if (faceRP == null)
                    continue;
                var cullNode = faceRP.FindFirstNode<TtCpuCullingNode>();
                if (cullNode == null)
                    continue;
                var visParam = cullNode.VisParameter;
                visParam.IsGatherVisibleMeshes = this.OnVisitNode;
                int capturedFaceIndex = fi; // lambda 捕获
                cullNode.UserTickLogic = (TtWorld w, TtRenderPolicy p, TtCommandList cmd, bool clear) =>
                {
                    visParam.World = CubeRenderer.CaptureWorld;
                    visParam.CullCamera = p.DefaultCamera;
                    //visParam.DontFrustumCull = true;
                    if (CaptureMode == ECaptureMode.OnlyShowNodes)
                    {
                        visParam.ClearVisibles();
                        foreach (var n in OnlyShowNodes)
                        {
                            if (n != null)
                                n.OnGatherVisibleMeshes(visParam);
                        }
                    }
                    else
                    {
                        visParam.OnVisitNode = (node, parameter) =>
                        {
                            if (node is TtAxisNode)
                                return false;
                            if (node.Parent is TtAxisNode)
                                return false;
                            if (node is TtMeshNode meshNode)
                            {
                                if (meshNode.Tag is TtAxis.TtAxisData)
                                    return false;
                            }
                            return true;
                        };
                        CubeRenderer.CaptureWorld.GatherVisibleMeshes(visParam);
                    }
                };
            }

            // 注意: 节点 tick 走 TtNode.OnTickLogic 路径 (由父节点统一驱动,
            // 同帧同线程, 不需要 ITickable + AddTickable). 见本文件下方
            // OnTickLogic(TtNodeTickParameters args) override.
            return true;
        }

        protected override void OnBeforeSaveNodeData()
        {
            var nd = GetNodeData<TtSceneCubeCaptureData>();
            nd.ShowActors.Clear();
            foreach (var n in OnlyShowNodes)
            {
                if (n != null)
                    nd.ShowActors.Add(n.NodeId);
            }
        }

        public override void Dispose()
        {
            // 先释放 CubeRenderer (它只解引用 RP, 不 dispose).
            var policies = mCubeRenderer?.RenderPolicies;
            CoreSDK.DisposeObject(ref mCubeRenderer);

            // 再释放 6 个 RP (上层创建, 上层负责释放).
            if (policies != null)
            {
                for (int i = 0; i < policies.Length; i++)
                    policies[i]?.Dispose();
            }

            base.Dispose();
        }

        // ---------- Volume / Visual 过滤 ----------
        // visit 时被 CpuCullingNode 调用一次 / 节点. 规则:
        //   - OnlyShowNodes 模式: 拦截在外层 GatherVisibleMeshes(node) 调用,
        //     这里直接 return true.
        //   - ExcludeNodes 模式: 排除 ExcludeActors 列表里的 actor.
        //   - Normal 模式: 必须是 TtVisual; 若开 CaptureOnlyVisualsInVolume,
        //     还要落在 OBB Volume 内.
        bool OnVisitNode(TtNode node, TtWorld.TtVisParameter arg)
        {
            switch (CaptureMode)
            {
                case ECaptureMode.OnlyShowNodes:
                    return true;

                case ECaptureMode.ExcludeNodes:
                    {
                        var actor = node as TtSceneActorNode;
                        if (actor == null)
                            return false;
                        var nd = GetNodeData<TtSceneCubeCaptureData>();
                        if (nd.ExcludeActors != null && nd.ExcludeActors.Contains(actor.NodeId))
                            return false;
                        return PassVolumeAndVisualFilter(actor);
                    }

                case ECaptureMode.Normal:
                default:
                    {
                        var actor = node as TtSceneActorNode;
                        if (actor == null)
                            return false;
                        return PassVolumeAndVisualFilter(actor);
                    }
            }
        }

        bool PassVolumeAndVisualFilter(TtSceneActorNode actor)
        {
            var nd = GetNodeData<TtSceneCubeCaptureData>();

            // OnlyShowNodes 列表里的节点强制参与 (不受 Volume / TtVisual 类型限制).
            if (OnlyShowNodes.Contains(actor))
                return true;

            // 必须是 TtVisual (光源 / 体积雾 / 标记节点等不参与 cube 拍摄).
            if (actor is TtVisual == false)
                return false;

            if (nd.CaptureOnlyVisualsInVolume == false)
                return true;

            // Volume = 节点自身 Placement.AbsTransform + VolumeExtent 半边长 OBB.
            // 简化判定: 把 actor 的 RefAABB 变换到世界 AABB, 与 capture 节点的世界
            // OBB AABB 求相交. 这是保守且足够便宜的近似 (真 OBB-OBB 相交对于
            // visit 频率太重). 漏 cull 一些 OBB 边缘的 actor 在 cube capture
            // 场景下完全可接受 — 反射本身不是像素精确的.
            var actorAABB = DBoundingBox.TransformNoScale(in actor.RefAABB, in actor.Placement.AbsTransform);
            // DBoundingBox.Intersects 仅有静态方法签名: static bool Intersects(in DBoundingBox, in DBoundingBox).
            return DBoundingBox.Intersects(in mVolumeAABB, in actorAABB);
        }

        // 缓存的 Volume 世界 AABB (每次 TickLogic 在拍摄前刷新).
        DBoundingBox mVolumeAABB;
        void UpdateVolumeAABB()
        {
            var nd = GetNodeData<TtSceneCubeCaptureData>();
            // 节点局部空间的 OBB 用 ±VolumeExtent 表示, 转世界 AABB.
            var localAABB = new DBoundingBox(
                new DVector3(-nd.VolumeExtent.X, -nd.VolumeExtent.Y, -nd.VolumeExtent.Z),
                new DVector3( nd.VolumeExtent.X,  nd.VolumeExtent.Y,  nd.VolumeExtent.Z));
            mVolumeAABB = DBoundingBox.TransformNoScale(in localAABB, in Placement.AbsTransform);
        }

        // ---------- 6 面相机 ----------
        void UpdateCubeCameras()
        {
            if (CubeRenderer == null)
                return;
            var nd = GetNodeData<TtSceneCubeCaptureData>();
            CubeRenderer.UpdateFaceCameras(
                in this.Placement.AbsTransform.mPosition,
                nd.CubeFaceSize, nd.NearPlane, nd.FarPlane);
        }

        protected override void OnAbsTransformChanged()
        {
            UpdateCubeCameras();
            if (mDebugMesh != null)
                mDebugMesh.DirectSetWorldMatrix(Placement.AbsTransform.ToMatrixNoScale(this.GetWorld().CameraOffset));
        }

        // ---------- Tick: 间隔节流 + 触发 6 面拍摄 ----------
        // 走 TtNode.OnTickLogic 标准路径 (同 NebulaNode.cs:95-105 的模板):
        //   - 由父节点 scene tick 链统一驱动, 同帧同线程
        //   - args.Policy 拿到当前主 RP (这里我们其实用的是 cube 自己的内嵌 RP,
        //     不依赖 args.Policy, 留作未来需要时取用)
        //   - 时间增量取 TtEngine.Instance.ElapsedSecond (NebulaNode 同款做法)
        //   - 返回 true 表示继续 tick 子节点
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (CubeRenderer == null || CubeRenderer.RenderPolicies == null)
                return true;

            var nd = GetNodeData<TtSceneCubeCaptureData>();
            var ellapse = TtEngine.Instance.ElapsedSecond;

            // CaptureInterval 语义:
            //   <0  仅手动 (CaptureNow 触发)
            //   =0  每帧拍
            //   >0  按秒间隔
            bool shouldCapture = false;
            if (mManualTriggerPending)
            {
                shouldCapture = true;
                mManualTriggerPending = false;
            }
            else if (nd.CaptureInterval == 0)
            {
                shouldCapture = true;
            }
            else if (nd.CaptureInterval > 0)
            {
                mAccumulatedTime += ellapse;
                if (mAccumulatedTime >= nd.CaptureInterval)
                {
                    shouldCapture = true;
                    mAccumulatedTime = 0;
                }
            }

            if (!shouldCapture)
                return true;

            // 拍摄前: 刷新 Volume AABB + 6 面相机 (eyePos 可能跟随 actor 移动).
            UpdateVolumeAABB();
            UpdateCubeCameras();

            TtEngine.Instance.ThreadRender.QueueRenderAction("CubeRenderer.CaptureCubeFaces", static (in Thread.TtThreadRender.FRenderAction RAct) =>
            {
                var This = (RAct.Arg as TtSceneCubeCapture);
                This.UpdateCubeCameras();
                This.CubeRenderer.CaptureCubeFaces(TtEngine.Instance.ElapsedSecond);
            }, this);

            System.Threading.AutoResetEvent mRenderFinishedEvent = new System.Threading.AutoResetEvent(false);
            TtEngine.Instance.ThreadRender.WaitFinishRenderAction(mRenderFinishedEvent);

            return true;
        }

        // ---------- 编辑器属性 ----------
        [Category("Capture")]
        public ECaptureMode CaptureMode
        {
            get => GetNodeData<TtSceneCubeCaptureData>().CaptureMode;
            set => GetNodeData<TtSceneCubeCaptureData>().CaptureMode = value;
        }

        [Category("Capture")]
        public uint CubeFaceSize
        {
            get => GetNodeData<TtSceneCubeCaptureData>().CubeFaceSize;
            set
            {
                var nd = GetNodeData<TtSceneCubeCaptureData>();
                if (nd.CubeFaceSize == value)
                    return;
                nd.CubeFaceSize = value;
                CubeRenderer?.OnResize(value);
                UpdateCubeCameras();
            }
        }

        [Category("Capture")]
        public float CaptureInterval
        {
            get => GetNodeData<TtSceneCubeCaptureData>().CaptureInterval;
            set => GetNodeData<TtSceneCubeCaptureData>().CaptureInterval = value;
        }

        [Category("Capture")]
        public Vector3 VolumeExtent
        {
            get => GetNodeData<TtSceneCubeCaptureData>().VolumeExtent;
            set => GetNodeData<TtSceneCubeCaptureData>().VolumeExtent = value;
        }

        [Category("Capture")]
        public bool CaptureOnlyVisualsInVolume
        {
            get => GetNodeData<TtSceneCubeCaptureData>().CaptureOnlyVisualsInVolume;
            set => GetNodeData<TtSceneCubeCaptureData>().CaptureOnlyVisualsInVolume = value;
        }
    }
}

#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtSceneCubeCapture
	{
		public unsafe void macross_AddOnlyShowNode (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode node) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			AddOnlyShowNode(node);
		}
		public unsafe void macross_RemoveOnlyShowNode (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode node) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			RemoveOnlyShowNode(node);
		}
		public unsafe void macross_ClearOnlyShowNodes (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			ClearOnlyShowNodes();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross