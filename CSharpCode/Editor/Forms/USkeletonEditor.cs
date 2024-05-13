using Assimp;
using EngineNS.Animation.Asset;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class USkeletonEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public Animation.Asset.TtAnimationClip AnimationClip;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid AnimationClipPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~USkeletonEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            AnimationClip = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            AnimationClipPropGrid.Target = null;
        }
        #region IAssetEditor
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public IRootForm GetRootForm()
        {
            return this;
        }

        public async Task<bool> Initialize()
        {
            await AnimationClipPropGrid.Initialize();
            return true;
        }

        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public void OnDraw()
        {
            if(Visible == false || AnimationClip == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EGui.UIProxy.DockProxy.BeginMainForm(AnimationClip.AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawLeft();
            DrawRight();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                AnimationClip.SaveAssetTo(AnimationClip.AssetName);
                var unused = UEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(AnimationClip.AssetName);

                //USnapshot.Save(AnimationClip.AssetName, AnimationClip.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {

            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    AnimationClipPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        EngineNS.GamePlay.Scene.UMeshNode mCurrentMeshNode;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.UMeshNode PlaneMeshNode;
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);


            var aabb = new BoundingBox(3,3,3);
            float radius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter() + new Vector3(0, 1, 0);
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(ref sphere);

            {
                var box = Graphics.Mesh.UMeshDataProvider.MakeBox(-5, -0.1001f, -5, 10, 0.1f, 10).ToMesh();
                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(UEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(box, tMaterials,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PlaneMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                PlaneMeshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                PlaneMeshNode.NodeData.Name = "Plane";
                PlaneMeshNode.IsAcceptShadow = true;
                PlaneMeshNode.IsCastShadow = false;
            }

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        TtAnimationClipPreview AnimationClipPreview = null;
        public async Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            AnimationClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(name);
            if (AnimationClip == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);
            AnimationClipPreview = new TtAnimationClipPreview();
            AnimationClipPreview.AnimationClipEditor = this;
            AnimationClipPreview.AnimationClip = AnimationClip;
            AnimationClipPropGrid.Target = AnimationClipPreview;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public async Task OnPreviewMeshChange(UMeshPrimitives meshPrimitives)
        {
            if(mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
            }
            var mtl = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(UEngine.Instance.Config.MeshPrimitiveEditorConfig.MaterialName);
            var materials = new Graphics.Pipeline.Shader.UMaterial[meshPrimitives.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mtl;
            }

            var meshData = new EngineNS.GamePlay.Scene.UMeshNode.UMeshNodeData();
            meshData.MeshName = meshPrimitives.AssetName;
            meshData.MdfQueueType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
            meshData.AtomType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMesh.TtAtom));
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(meshPrimitives, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.UPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            
            mCurrentMeshNode = meshNode;

            var sapnd = new EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.TtSkeletonAnimPlayNodeData();
            sapnd.Name = "PlayAnim";
            sapnd.AnimatinName = AssetName;
            await EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.AddSkeletonAnimPlayNode(PreviewViewport.World, mCurrentMeshNode, sapnd, 
                            EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));
        }

        class TtAnimationClipPreview
        {
            [Browsable(false)]
            public USkeletonEditor AnimationClipEditor = null;
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
            private RName mPreivewMeshName;
            public RName PreivewMesh
            {
                get
                {
                    return mPreivewMeshName;
                }
                set
                {
                    if (AssetState == IO.EAssetState.Loading)
                        return;
                    mPreivewMeshName = value;
                    AssetState = IO.EAssetState.Loading;
                    System.Action exec = async () =>
                    {
                        var Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value);
                        if (Mesh.mCoreObject.IsValidPointer == false)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await AnimationClipEditor.OnPreviewMeshChange(Mesh);
                    };
                    exec();
                }
            }

            public TtAnimationClip AnimationClip { get; set; } = new TtAnimationClip();
        }

        #endregion IAssetEditor

        #region ITickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }

        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USkeletonEditor))]
    public partial class TtSkeletonAsset
    {

    }
}