using EngineNS.Animation.Asset;
using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtAnimationBlendSpaceEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtBlendSpace2D BlendSpace;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid AnimationPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~TtAnimationBlendSpaceEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            BlendSpace = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            AnimationPropGrid.Target = null;
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

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await AnimationPropGrid.Initialize();
            return true;
        }

        public void OnCloseEditor()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public void OnDraw()
        {
            if(Visible == false || BlendSpace == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
            DrawRight();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                BlendSpace.SaveAssetTo(BlendSpace.AssetName);
                var unused = TtEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(BlendSpace.AssetName);

                //USnapshot.Save(AnimationClip.AssetName, AnimationClip.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
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
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    AnimationPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        protected async System.Threading.Tasks.Task<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var aabb = new BoundingBox(3,3,3);
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector() + new DVector3(0, 1, 0);
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);

            {
                var PlaneMesh = new Graphics.Mesh.TtMesh();
                var tMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
                tMaterials[0] = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(TtEngine.Instance.Config.MeshPrimitiveEditorConfig.PlaneMaterialName);
                PlaneMesh.Initialize(Graphics.Mesh.TtMeshDataProvider.MakePlane(10, 10).ToMesh(), tMaterials,
                    Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                PlaneMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), PlaneMesh, new DVector3(0, -0.0001f, 0), Vector3.One, Quaternion.Identity);
                PlaneMeshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
                PlaneMeshNode.NodeData.Name = "Plane";
                PlaneMeshNode.IsAcceptShadow = true;
                PlaneMeshNode.IsCastShadow = false;
            }

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;

            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        TtAnimationBlendSpacePreview AnimationPreview = null;
        public async Thread.Async.TtTask<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            BlendSpace = await TtEngine.Instance.AnimationModule.BlendSpaceClipManager.GetAnimation(name);
            if (BlendSpace == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.SimpleRPolicyName, 0, 1);
            AnimationPreview = new TtAnimationBlendSpacePreview();
            AnimationPreview.Editor = this;
            AnimationPreview.Animation = BlendSpace;
            AnimationPropGrid.Target = AnimationPreview;
            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public async TtTask OnPreviewMeshChange(TtMeshPrimitives meshPrimitives)
        {
            if(mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
            }
            var mtl = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(TtEngine.Instance.Config.MeshPrimitiveEditorConfig.MaterialName);
            var materials = new Graphics.Pipeline.Shader.TtMaterial[meshPrimitives.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mtl;
            }

            var meshData = new EngineNS.GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData.MeshName = meshPrimitives.AssetName;
            meshData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
            meshData.AtomType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMesh.TtAtom));
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(meshPrimitives, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.TtPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            
            mCurrentMeshNode = meshNode;

            //await EngineNS.Animation.SceneNode.TtAnimStateMachinePlayNode.Add(PreviewViewport.World, mCurrentMeshNode, new GamePlay.Scene.UNodeData(),
            //                EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));

            var sapnd = new EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.TtSkeletonAnimPlayNodeData();
            sapnd.Name = "PlayAnim";
            sapnd.AnimatinName = AssetName;
            await EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.AddSkeletonAnimPlayNode(PreviewViewport.World, mCurrentMeshNode, sapnd,
                            EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtIdentityPlacement));
        }
        [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        class TtAnimationBlendSpacePreview
        {
            [Browsable(false)]
            public TtAnimationBlendSpaceEditor Editor = null;
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
            [Browsable(false)]
            private RName mPreivewMeshName;
            [Category("Option")]
            [RName.PGRName(FilterExts = TtMeshPrimitives.AssetExt)]
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
                        var Mesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value);
                        if (Mesh.mCoreObject.IsValidPointer == false)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await Editor.OnPreviewMeshChange(Mesh);
                    };
                    exec();
                }
            }
            [Category("Option")]
            public TtBlendSpace2D Animation { get; set; } = new();
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

        public string GetWindowsName()
        {
            return BlendSpace.AssetName.Name;
        }
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset.BlendSpace
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.TtAnimationBlendSpaceEditor))]
    public partial class TtBlendSpace2D
    {

    }
}