using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.GamePlay.TtWorld;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Capture", "SceneCapture", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(USceneCapture.USceneCaptureData), DefaultNamePrefix = "Capture")]
    public partial class USceneCapture : TtSceneActorNode, ITickable, IRootForm
    {
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
        public class USceneCaptureData : TtNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
            public RName RPolicyName { get; set; }
            [Rtti.Meta]
            public Vector2 TargetSize { get; set; } = new Vector2(256, 256);
            [Rtti.Meta]
            public ECaptureMode CaptureMode { get; set; } = ECaptureMode.Normal;
            [Rtti.Meta]
            public List<Guid> ShowActors { get; set; }
            [Rtti.Meta]
            public List<Guid> ExcludeActors { get; set; }
        }
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy { get; set; }
        public Editor.Controller.EditorCameraController CameraController = new Editor.Controller.EditorCameraController();
        public GamePlay.TtWorld CaptureWorld { get; set; }
        GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var nd = GetNodeData<USceneCaptureData>();
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

            RenderPolicy = policy;
            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            CaptureWorld = world;

            UpdateCamera();

            TtEngine.Instance.TickableManager.AddTickable(this);

            mVisParameter.OnVisitNode = this.OnVisitNode;
            return true;
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
                        var actor = node as TtSceneActorNode;
                        if (actor == null)
                            return false;
                        var data = GetNodeData<USceneCaptureData>();
                        return data.ShowActors.Contains(actor.ActorId);
                    }
                case ECaptureMode.ExcludeNodes:
                    {
                        var actor = node as TtSceneActorNode;
                        if (actor == null)
                            return false;
                        var data = GetNodeData<USceneCaptureData>();
                        return !data.ExcludeActors.Contains(actor.ActorId);
                    }
                default:
                    break;
            }
            
            return true;
        }
        public ECaptureMode CaptureMode
        {
            get => GetNodeData<USceneCaptureData>().CaptureMode;
            set
            {
                GetNodeData<USceneCaptureData>().CaptureMode = value;
            }
        }

        public Vector2 TargetSize
        {
            get => GetNodeData<USceneCaptureData>().TargetSize;
            set
            {
                GetNodeData<USceneCaptureData>().TargetSize = value;
                RenderPolicy.OnResize(value.X, value.Y);
            }
        }

        void UpdateCamera()
        {
            if (RenderPolicy == null)
                return;
            ref var eyePos = ref this.Placement.AbsTransform.mPosition;
            var dir = this.Placement.AbsTransform.TransformVector3NoScale(in Vector3.Forward);
            dir.Normalize();
            var lookAt = eyePos + dir * 100.0f;
            RenderPolicy.DefaultCamera.mCoreObject.LookAtLH(in eyePos, in lookAt, in Vector3.Up);
        }
        protected override void OnAbsTransformChanged()
        {
            UpdateCamera();
        }
        bool IsCaptureVisible = false;
        public void TickLogic(float ellapse)
        {
            var absAABB = DBoundingBox.TransformNoScale(in AABB, in Placement.AbsTransform);
            var type = CameraController.Camera.WhichContainTypeFast(CaptureWorld, in absAABB, false);

            if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
            {
                IsCaptureVisible = false;
                return;
            }
            IsCaptureVisible = true;

            mVisParameter.World = CaptureWorld;
            mVisParameter.CullCamera = RenderPolicy.DefaultCamera;
            CaptureWorld.GatherVisibleMeshes(mVisParameter);

            RenderPolicy?.BeginTickLogic(CaptureWorld);
            RenderPolicy?.TickLogic(CaptureWorld, null);
            RenderPolicy?.EndTickLogic(CaptureWorld);
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
                RenderPolicy?.TickSync();
        }

        #region DebugUI
        bool mVisible;
        public bool Visible 
        {
            get => mVisible;
            set
            {
                mVisible = value;
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
            if (Visible == false || RenderPolicy == null)
                return;

            ImGuiAPI.SetNextWindowSize(GetNodeData<USceneCaptureData>().TargetSize, ImGuiCond_.ImGuiCond_FirstUseEver);
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
                    drawlist.AddImage((ulong)RenderPolicy.GetFinalShowRSV().GetTextureHandle(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                }
                ImGuiAPI.EndChild();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        #endregion
    }
}
