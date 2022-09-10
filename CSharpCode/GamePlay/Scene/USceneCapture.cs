using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Capture", "SceneCapture", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(USceneCapture.USceneCaptureData), DefaultNamePrefix = "Capture")]
    public partial class USceneCapture : USceneActorNode, ITickable, IRootForm
    {
        public class USceneCaptureData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.URenderPolicyAsset.AssetExt)]
            public RName RPolicyName { get; set; }
            [Rtti.Meta]
            public Vector2 TargetSize { get; set; } = new Vector2(256, 256);
        }
        public Graphics.Pipeline.URenderPolicy RenderPolicy { get; set; }
        public Editor.Controller.EditorCameraController CameraController = new Editor.Controller.EditorCameraController();
        public GamePlay.UWorld CaptureWorld { get; set; }
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var nd = GetNodeData<USceneCaptureData>();
            if (nd.RPolicyName == null)
            {
                nd.RPolicyName = UEngine.Instance.Config.MainRPolicyName;
            }
            Graphics.Pipeline.URenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(nd.RPolicyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy();
            }
            await policy.Initialize(null);
            RenderPolicy = policy;
            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            RenderPolicy.OnResize(nd.TargetSize.X, nd.TargetSize.Y);
            CaptureWorld = world;

            UpdateCamera();

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
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
        public void TickLogic(int ellapse)
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
            mVisParameter.VisibleMeshes = RenderPolicy.VisibleMeshes;
            mVisParameter.VisibleNodes = RenderPolicy.VisibleNodes;
            mVisParameter.CullCamera = RenderPolicy.DefaultCamera;
            CaptureWorld.GatherVisibleMeshes(mVisParameter);

            RenderPolicy?.BeginTickLogic(CaptureWorld);
            RenderPolicy?.TickLogic(CaptureWorld);
            RenderPolicy?.EndTickLogic(CaptureWorld);
        }
        public void TickRender(int ellapse)
        {
            
        }
        public void TickSync(int ellapse)
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
                    UEngine.RootFormManager.RegRootForm(this);
                else
                    UEngine.RootFormManager.UnregRootForm(this);
            }
        }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; }
        public async Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
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
            if (ImGuiAPI.Begin($"Capture:{this.NodeName}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.BeginChild("FinalTexture", in Vector2.MinusOne, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var pos = ImGuiAPI.GetWindowPos();
                    var drawlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                    var uv1 = new Vector2(0, 0);
                    var uv2 = new Vector2(1, 1);
                    var min1 = ImGuiAPI.GetWindowContentRegionMin();
                    var max1 = ImGuiAPI.GetWindowContentRegionMax();

                    min1 = min1 + pos;
                    max1 = max1 + pos;
                    drawlist.AddImage(RenderPolicy.GetFinalShowRSV().GetTextureHandle().ToPointer(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                }
                ImGuiAPI.EndChild();
            }
            ImGuiAPI.End();
        }
        #endregion
    }
}
