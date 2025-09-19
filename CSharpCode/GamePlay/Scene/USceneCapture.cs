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
            RenderPolicy?.BeginTickLogic(CaptureWorld);
            RenderPolicy?.TickLogic(CaptureWorld, null);
            RenderPolicy?.EndTickLogic(CaptureWorld);
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
            RenderPolicy.CmdQueue.Execute(new NxRHI.ICommandList());
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Capture", "SceneCapture", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSceneCapture.TtSceneCaptureData), DefaultNamePrefix = "Capture")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneCapture@EngineCore", "EngineNS.GamePlay.Scene.USceneCapture" })]
    public partial class TtSceneCapture : TtSceneActorNode, ITickable, IRootForm
    {
        public override void Dispose()
        {
            Visible = false;
            base.Dispose();
        }
        public int GetTickOrder()
        {
            return 0;
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
        public TtWorldRenderer WorldRenderer { get; } = new TtWorldRenderer();
        protected override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var nd = GetNodeData<TtSceneCaptureData>();
            if (nd.RPolicyName == null)
            {
                nd.RPolicyName = TtEngine.Instance.Config.SimpleRPolicyName;
            }
            Graphics.Pipeline.TtRenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.LoadAsset(nd.RPolicyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy(null);
            }
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

            TtEngine.Instance.TickableManager.AddTickable(this);

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
            WorldRenderer.RenderPolicy.DefaultCamera.mCoreObject.LookAtLH(in eyePos, in lookAt, in Vector3.Up);
        }
        protected override void OnAbsTransformChanged()
        {
            UpdateCamera();
        }
        bool IsCaptureVisible = false;
        public void TickLogic(float ellapse)
        {
            var absAABB = DBoundingBox.TransformNoScale(in RefAABB, in Placement.AbsTransform);
            var type = WorldRenderer.CameraController.Camera.WhichContainTypeFast(WorldRenderer.CaptureWorld, in absAABB, false);

            if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
            {
                IsCaptureVisible = false;
                return;
            }
            IsCaptureVisible = true;

            WorldRenderer.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            if (IsCaptureVisible)
                WorldRenderer.RenderPolicy?.TickSync();
        }

        #region DebugUI
        bool mShowDebugger;
        [Category("Option")]
        public bool Visible 
        {
            get => mShowDebugger;
            set
            {
                mShowDebugger = value;
                if (value)
                    TtEngine.RootFormManager.RegRootForm(this);
                else
                    TtEngine.RootFormManager.UnregRootForm(this);
            }
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup()
        {

        }
        public unsafe void OnDraw()
        {
            if (Visible == false || WorldRenderer.RenderPolicy == null)
                return;

            ImGuiAPI.SetNextWindowSize(GetNodeData<TtSceneCaptureData>().TargetSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm($"Capture:{this.NodeName}", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.BeginChild("FinalTexture", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var pos = ImGuiAPI.GetWindowPos();
                    var drawlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                    var uv1 = new Vector2(0, 0);
                    var uv2 = new Vector2(1, 1);
                    var min1 = ImGuiAPI.GetWindowContentRegionMin();
                    var max1 = ImGuiAPI.GetWindowContentRegionMax();

                    min1 = min1 + pos;
                    max1 = max1 + pos;
                    ImTextureRef imTextureRef = new ImTextureRef();
                    imTextureRef.m__TexID = (ulong)WorldRenderer.RenderPolicy.GetFinalShowRSV().GetTextureHandle();
                    drawlist.AddImage(imTextureRef, in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                }
                ImGuiAPI.EndChild();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        #endregion
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtSceneCapture
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_AddOnlyShowNode_467596569 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtSceneCapture->void AddOnlyShowNode(TtNode node)");
		public unsafe void macross_AddOnlyShowNode (string nodeName, TtNode node) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":node", node);
				}
			}
			AddOnlyShowNode(node);
			macross_break_AddOnlyShowNode_467596569.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RemoveOnlyShowNode_467596569 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtSceneCapture->void RemoveOnlyShowNode(TtNode node)");
		public unsafe void macross_RemoveOnlyShowNode (string nodeName, TtNode node) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":node", node);
				}
			}
			RemoveOnlyShowNode(node);
			macross_break_RemoveOnlyShowNode_467596569.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ClearOnlyShowNodes_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtSceneCapture->void ClearOnlyShowNodes()");
		public unsafe void macross_ClearOnlyShowNodes (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			ClearOnlyShowNodes();
			macross_break_ClearOnlyShowNodes_2609910045.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross