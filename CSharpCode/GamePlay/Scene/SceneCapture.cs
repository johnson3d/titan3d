using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.GamePlay.TtWorld;
using static EngineNS.GamePlay.TtAxis;
using EngineNS.Graphics.Mesh;

namespace EngineNS.GamePlay.Scene
{
    public class TtWorldRenderer
    {
        public GamePlay.TtWorld CaptureWorld { get; set; }
        public Editor.Controller.EditorCameraController CameraController = new Editor.Controller.EditorCameraController();
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get; set; }
        public virtual bool Initialize(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            CaptureWorld = world;

            RenderPolicy = policy;
            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            return true;
        }
        public virtual void TickLogic(float ellapse)
        {
            RenderPolicy?.BeginTick(CaptureWorld);
            RenderPolicy?.Tick(CaptureWorld, null);
            RenderPolicy?.EndTick(CaptureWorld);
        }
    }
    public class TtWorldImmRenderer : TtWorldRenderer
    {
        public override bool Initialize(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.Initialize(world, policy);
            RenderPolicy.CmdQueue = new NxRHI.TtRCmdQueue();
            return true;
        }
        public override void TickLogic(float ellapse)
        {
            base.TickLogic(ellapse);
            RenderPolicy.CmdQueue.FlushExecute(true);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Capture", "Graphics\\SceneCapture", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSceneCapture.TtSceneCaptureData), DefaultNamePrefix = "Capture")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneCapture@EngineCore", "EngineNS.GamePlay.Scene.USceneCapture" })]
    public partial class TtSceneCapture : TtSceneActorNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mFrustumMesh);
            var rp = WorldRenderer?.RenderPolicy;
            if (rp != null)
            {
                WorldRenderer.RenderPolicy = null;
                rp.Dispose();
            }
            base.Dispose();
        }
        public enum ECaptureMode
        {
            Normal,
            OnlyShowNodes,
            ExcludeNodes,
        }
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneCapture.USceneCaptureData@EngineCore", "EngineNS.GamePlay.Scene.USceneCapture.USceneCaptureData" })]
        public class TtSceneCaptureData : TtNodeData
        {
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
            public RName RPolicyName { get; set; }
            [Rtti.Meta("")]
            public Vector2 TargetSize { get; set; } = new Vector2(256, 256);
            [Rtti.Meta("")]
            public ECaptureMode CaptureMode { get; set; } = ECaptureMode.Normal;
            [Rtti.Meta("")]
            public List<Guid> ShowActors { get; set; }
            [Rtti.Meta("")]
            public List<Guid> ExcludeActors { get; set; }
            [Rtti.Meta("")]
            public float CaptureInterval { get; set; } = float.MaxValue;
        }

        #region OnlyShowNodes
        public List<TtNode> OnlyShowNodes { get; } = new List<TtNode>();
        [Rtti.Meta("")]
        public void AddOnlyShowNode(TtNode node)
        {
            if (OnlyShowNodes.Contains(node) == false)
            {
                OnlyShowNodes.Add(node);
            }
        }
        [Rtti.Meta("")]
        public void RemoveOnlyShowNode(TtNode node)
        {
            if (OnlyShowNodes.Contains(node))
            {
                OnlyShowNodes.Remove(node);
            }
        }
        [Rtti.Meta("")]
        public void ClearOnlyShowNodes()
        {
            OnlyShowNodes.Clear();
        }
        #endregion
        public TtWorldImmRenderer WorldRenderer { get; } = new TtWorldImmRenderer();
        protected override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var nd = GetNodeData<TtSceneCaptureData>();
            if (nd.RPolicyName == null)
            {
                nd.RPolicyName = TtEngine.Instance.Config.MainRPolicyName;
            }
            var policy = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(nd.RPolicyName, null);
            if (policy == null)
                return false;
            await policy.Initialize(null);
            policy.OnResize(nd.TargetSize.X, nd.TargetSize.Y);

            WorldRenderer.Initialize(world, policy);
            
            this.OnlyShowNodes.Clear();
            if (nd.ShowActors != null && nd.ShowActors.Count > 0)
            {
                foreach (var i in nd.ShowActors)
                {
                    var node = world.Root.FindNode(i, true);
                    if (node != null)
                    {
                        this.OnlyShowNodes.Add(node);
                    }
                }
            }

            UpdateCamera();
            RebuildFrustumMesh();

            var cullNode = WorldRenderer.RenderPolicy.FindFirstNode<TtCpuCullingNode>();
            GamePlay.TtWorld.TtVisParameter mVisParameter = cullNode.VisParameter;
            mVisParameter.IsGatherVisibleMeshes = this.OnVisitNode;
            cullNode.UserTickLogic = (GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear) =>
            {
                mVisParameter.World = WorldRenderer.CaptureWorld;
                mVisParameter.CullCamera = WorldRenderer.RenderPolicy.DefaultCamera;
                if (CaptureMode == ECaptureMode.OnlyShowNodes)
                {
                    mVisParameter.ClearVisibles();
                    foreach (var i in OnlyShowNodes)
                    {
                        WorldRenderer.CaptureWorld.GatherVisibleMeshes(mVisParameter, i);
                    }
                }
                else
                {
                    mVisParameter.OnVisitNode = (node, parameter) =>
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
                    WorldRenderer.CaptureWorld.GatherVisibleMeshes(mVisParameter);
                }
            };

            return true;
        }
        protected override void OnBeforeSaveNodeData()
        {
            var nd = GetNodeData<TtSceneCaptureData>();
            nd.ShowActors.Clear();
            foreach (var i in OnlyShowNodes)
            {
                var node = WorldRenderer.CaptureWorld.Root.FindNode(i.NodeId, true);
                if (node != null)
                {
                    nd.ShowActors.Add(i.NodeId);
                }
            }
        }
        bool OnVisitNode(Scene.TtNode node, TtVisParameter arg)
        {
            switch (CaptureMode)
            {
                case ECaptureMode.Normal:
                    {
                        return true;
                    }
                case ECaptureMode.OnlyShowNodes:
                    {
                        //var actor = node as TtSceneActorNode;
                        //if (actor == null)
                        //    return false;
                        //var data = GetNodeData<TtSceneCaptureData>();
                        //return data.ShowActors.Contains(actor.NodeId);
                        return true;
                    }
                case ECaptureMode.ExcludeNodes:
                    {
                        var actor = node as TtSceneActorNode;
                        if (actor == null)
                            return false;
                        var data = GetNodeData<TtSceneCaptureData>();
                        return !data.ExcludeActors.Contains(actor.NodeId);
                    }
                default:
                    break;
            }
            
            return true;
        }
        [Category("Option")]
        [Rtti.Meta("")]
        public ECaptureMode CaptureMode
        {
            get => GetNodeData<TtSceneCaptureData>().CaptureMode;
            set
            {
                GetNodeData<TtSceneCaptureData>().CaptureMode = value;
            }
        }
        [Category("Option")]
        public Vector2 TargetSize
        {
            get => GetNodeData<TtSceneCaptureData>().TargetSize;
            set
            {
                GetNodeData<TtSceneCaptureData>().TargetSize = value;
                WorldRenderer.RenderPolicy.OnResize(value.X, value.Y);
            }
        }

        void UpdateCamera()
        {
            if (WorldRenderer.RenderPolicy == null)
                return;
            ref var eyePos = ref this.Placement.AbsTransform.mPosition;
            var dir = this.Placement.AbsTransform.TransformVector3NoScale(in Vector3.Forward);
            dir.Normalize();
            var lookAt = eyePos + dir * 100.0f;
            WorldRenderer.RenderPolicy.DefaultCamera.LookAtLH(in eyePos, in lookAt, in Vector3.Up);
        }
        protected override void OnAbsTransformChanged()
        {
            UpdateCamera();
            var world = this.GetWorld();
            if (world != null)
            {
                var mat = Placement.AbsTransform.ToMatrixNoScale(world.CameraOffset);
                if (mDebugMesh != null)
                    mDebugMesh.DirectSetWorldMatrix(mat);
                if (mFrustumMesh != null)
                    mFrustumMesh.DirectSetWorldMatrix(mat);
            }
        }
        // ---------- 间隔节流 ----------
        float mAccumulatedTime = 0;
        bool mManualTriggerPending = false;

        [Category("Capture")]
        public bool CaptureNow
        {
            get => false;
            set => mManualTriggerPending = true;
        }

        [Category("Capture")]
        public bool CaptureWithRenderDoc
        {
            get => false;
            set
            {
                mManualTriggerPending = true;
                // SceneCapture 使用单 RP, 直接在 CmdQueue 上抓帧.
                var queue = WorldRenderer?.RenderPolicy?.CmdQueue;
                if (queue != null)
                    queue.CaptureRenderDocFrame = true;
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
                var srv = WorldRenderer?.RenderPolicy?.GetFinalShowRSV();
                if (srv == null)
                    return;
                if (mTextureViewer == null)
                    mTextureViewer = new Editor.Forms.TtTextureViewer();
                srv.AssetName = RName.GetRName($"@SceneCapture:{this.NodeId}@", RName.ERNameType.Transient);
                Editor.TtAssetEditorManager.TryOpenEditor(mTextureViewer,
                    srv.AssetName,
                    srv, false).AddWaitTask();
            }
        }

        [Category("Capture")]
        public float CaptureInterval
        {
            get => GetNodeData<TtSceneCaptureData>().CaptureInterval;
            set => GetNodeData<TtSceneCaptureData>().CaptureInterval = value;
        }

        // ---------- DebugMesh (摄像机图标) ----------
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
                    var mesh = new Graphics.Mesh.TtRenderMesh();
                    if (mesh.Initialize(cookedMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc))
                    {
                        mesh.IsAcceptShadow = false;
                        mDebugMesh = mesh;
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

        // ---------- FrustumMesh (平截头体线框) ----------
        Graphics.Mesh.TtRenderMesh mFrustumMesh;
        void RebuildFrustumMesh()
        {
            var camera = WorldRenderer?.RenderPolicy?.DefaultCamera;
            if (camera == null)
                return;

            var frustumProvider = Graphics.Mesh.TtMeshDataProvider.MakeFrustum(
                camera.Fov, camera.Aspect, camera.ZNear, camera.ZFar, 0xFFFFFF00);
            var frustumPrimitive = frustumProvider.ToMesh();

            var mtl = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = mtl;

            var mesh = new Graphics.Mesh.TtRenderMesh();
            if (mesh.Initialize(frustumPrimitive, materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc))
            {
                mesh.IsAcceptShadow = false;
                mesh.IsDrawHitproxy = false;
                CoreSDK.DisposeObject(ref mFrustumMesh);
                mFrustumMesh = mesh;

                if (this.GetWorld() != null)
                    mFrustumMesh.DirectSetWorldMatrix(Placement.AbsTransform.ToMatrixNoScale(this.GetWorld().CameraOffset));
            }
        }

        public override bool HashVisual => true;
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if ((rp.CullFilters & TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) == 0)
                return;
            if (DebugMesh != null)
                rp.AddVisibleMesh(DebugMesh);
            if (mFrustumMesh != null)
                rp.AddVisibleMesh(mFrustumMesh);
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            base.GetHitProxyDrawMesh(meshes);
            if (mDebugMesh != null)
                meshes.Add(mDebugMesh);
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
        // ---------- Tick ----------
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (WorldRenderer == null || WorldRenderer.RenderPolicy == null)
                return true;

            var nd = GetNodeData<TtSceneCaptureData>();
            var ellapse = TtEngine.Instance.ElapsedSecond;

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

            UpdateCamera();

            TtEngine.Instance.ThreadRender.QueueRenderAction("SceneCapture.Render", static (in Thread.TtThreadRender.FRenderAction RAct) =>
            {
                var This = (RAct.Arg as TtSceneCapture);
                This.WorldRenderer.TickLogic(TtEngine.Instance.ElapsedSecond);
            }, this);

            var renderFinished = new System.Threading.AutoResetEvent(false);
            TtEngine.Instance.ThreadRender.WaitFinishRenderAction(renderFinished);

            WorldRenderer.RenderPolicy?.TickSync();
            return true;
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtSceneCapture
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