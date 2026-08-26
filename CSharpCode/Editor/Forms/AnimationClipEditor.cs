using Assimp;
using EngineNS.Animation.Asset;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtAnimationClipEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public Animation.Asset.TtAnimationClip AnimationClip;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.TtPropertyGrid AnimationClipPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid NotifyPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        EGui.Controls.TtTimelineControl mTimeline = new EGui.Controls.TtTimelineControl();

        #region Notify时间轴与RootMotion预览
        Animation.SceneNode.TtSkeletonAnimPlayNode mAnimPlayNode = null;
        Animation.Notify.IAnimNotify mSelectedNotify = null;
        Animation.Notify.IAnimNotify mDraggingNotify = null;
        long mDragOldBeginMS = 0;
        long mDragOldEndMS = 0;
        bool mPopupForNotify = false;
        float mPopupTime = 0.0f;
        bool mApplyRootMotionInPreview = false;
        DVector3 mPreviewStartPosition = DVector3.Zero;
        #endregion Notify时间轴与RootMotion预览

        #region 统一Undo/Redo(开门)
        // 控制门就是是否new出历史栈: 需回退旧流程时把mEditorHistory改为null即可
        public bool EnableUndoRedo => EditorHistory != null;
        public Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        Infrastructure.TtEditorHistory mEditorHistory = new Infrastructure.TtEditorHistory();
        Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Infrastructure.TtEditorHistoryPanel();
        #endregion

        ~TtAnimationClipEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            AnimationClip = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            AnimationClipPropGrid.Target = null;
            AnimationClipPropGrid.HistoryHost = null;
            mEditorHistory?.Clear();
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
            await AnimationClipPropGrid.Initialize();
            await NotifyPropGrid.Initialize();
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
            uint bottomId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.3f, ref bottomId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Timeline", mDockKeyClass), bottomId);
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
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_None );
            if (result)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
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
            DrawTimeline();
            if (mEditorHistory != null)
            {
                mHistoryPanel.OnDraw(in mDockKeyClass, "History", mEditorHistory);
            }
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                AnimationClip.SaveAssetTo(AnimationClip.AssetName);
                TtEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(AnimationClip.AssetName).AddWaitTask();
                mEditorHistory?.SetSavePoint();

                //USnapshot.Save(AnimationClip.AssetName, AnimationClip.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            // 开启后预览角色会被动画的根骨骼位移带着跑(需要clip已勾EnableRootMotion)
            bool applyRootMotion = mApplyRootMotionInPreview;
            EGui.UIProxy.CheckBox.DrawCheckBox("RootMotion", ref applyRootMotion, false);
            if (applyRootMotion != mApplyRootMotionInPreview)
            {
                mApplyRootMotionInPreview = applyRootMotion;
                if (mAnimPlayNode != null)
                {
                    mAnimPlayNode.RootMotionMode = applyRootMotion
                        ? Animation.RootMotion.ERootMotionMode.FromEverything
                        : Animation.RootMotion.ERootMotionMode.Ignore;
                }
                if (!applyRootMotion && mCurrentMeshNode != null)
                {
                    mCurrentMeshNode.Placement.Position = mPreviewStartPosition;
                    mCurrentMeshNode.Placement.Quat = Quaternion.Identity;
                }
            }
            ImGuiAPI.SameLine(0, -1);
            // mEditorHistory为null时按钮/快捷键均为空操作, 与旧行为一致
            Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
            Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    AnimationClipPropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("Notify", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    if (mSelectedNotify == null)
                        ImGuiAPI.Text("Select a notify in timeline");
                    else
                        NotifyPropGrid.OnDraw(true, false, false);
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

        #region Notify时间轴

        bool mTimelineShow = true;
        protected unsafe void DrawTimeline()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Timeline", ref mTimelineShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ImGuiAPI.Text($"Duration: {AnimationClip.Duration:0.###}s");
                BuildTimelineData();
                var size = new Vector2(-1, 0);
                mTimeline.OnDraw(in size);
                ProcessTimelineInteraction();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        void BuildTimelineData()
        {
            mTimeline.Duration = Math.Max(AnimationClip.Duration, 0.001f);
            mTimeline.Tracks.Clear();
            mTimeline.Items.Clear();
            mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = "Notifies" });

            for (int i = 0; i < AnimationClip.Notifies.Count; ++i)
            {
                var notify = AnimationClip.Notifies[i];
                if (notify == null)
                    continue;
                bool durative = notify.EndTime > notify.BeginTime;
                mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                {
                    Id = $"notify_{i}",
                    Label = notify.Name,
                    TrackIndex = 0,
                    Begin = notify.BeginTime * 0.001f,
                    End = notify.EndTime * 0.001f,
                    Type = durative ? EGui.Controls.ETimelineItemType.Range : EGui.Controls.ETimelineItemType.Marker,
                    Color = durative ? 0xFF30C030u : 0xFF30FF30u,
                    AllowResize = durative,
                    UserData = notify,
                });
            }

            if (mAnimPlayNode?.Player != null)
                mTimeline.PlayPosition = mAnimPlayNode.Player.Time;
        }

        void ProcessTimelineInteraction()
        {
            if (mTimeline.PlayPositionChanged && mAnimPlayNode?.Player != null)
            {
                // 拖播放头相当于直接定位动画时间
                mAnimPlayNode.Player.Time = mTimeline.PlayPosition;
            }

            var clicked = mTimeline.ClickedItem;
            if (clicked != null)
            {
                mSelectedNotify = clicked.UserData as Animation.Notify.IAnimNotify;
                NotifyPropGrid.Target = mSelectedNotify;
            }

            var changed = mTimeline.ChangedItem;
            var changedNotify = changed?.UserData as Animation.Notify.IAnimNotify;
            if (changedNotify != null && mDraggingNotify != changedNotify)
            {
                mDraggingNotify = changedNotify;
                mDragOldBeginMS = changedNotify.BeginTime;
                mDragOldEndMS = changedNotify.EndTime;
            }
            if (changedNotify != null)
            {
                changedNotify.BeginTime = (long)(changed.Begin * 1000);
                changedNotify.EndTime = (long)(changed.End * 1000);
            }
            else if (mDraggingNotify != null)
            {
                PushNotifyTimeCommand(mDraggingNotify, mDragOldBeginMS, mDragOldEndMS);
                mDraggingNotify = null;
            }

            if (mTimeline.RightClickedItem != null)
            {
                mPopupForNotify = true;
                mSelectedNotify = mTimeline.RightClickedItem.UserData as Animation.Notify.IAnimNotify;
                NotifyPropGrid.Target = mSelectedNotify;
                ImGuiAPI.OpenPopup("ClipTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            else if (mTimeline.RightClickedTrack >= 0)
            {
                mPopupForNotify = false;
                mPopupTime = mTimeline.RightClickedTime;
                ImGuiAPI.OpenPopup("ClipTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            DrawTimelineMenu();
        }

        void DrawTimelineMenu()
        {
            if (!ImGuiAPI.BeginPopup("ClipTimelineMenu", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                return;

            if (mPopupForNotify)
            {
                if (ImGuiAPI.MenuItem("Remove Notify", null, false, true) && mSelectedNotify != null)
                {
                    var notify = mSelectedNotify;
                    int index = AnimationClip.Notifies.IndexOf(notify);
                    ExecuteNotifyCommand("Remove Notify",
                        () => AnimationClip.Notifies.Remove(notify),
                        () => AnimationClip.Notifies.Insert(Math.Min(index, AnimationClip.Notifies.Count), notify));
                    mSelectedNotify = null;
                    NotifyPropGrid.Target = null;
                }
            }
            else
            {
                if (ImGuiAPI.MenuItem("Add Transient Notify", null, false, true))
                    AddNotify(mPopupTime, false);
                if (ImGuiAPI.MenuItem("Add Durative Notify", null, false, true))
                    AddNotify(mPopupTime, true);
            }
            ImGuiAPI.EndPopup();
        }

        void AddNotify(float time, bool durative)
        {
            Animation.Notify.IAnimNotify notify;
            if (durative)
            {
                var durativeNotify = new Animation.Notify.TtDurativeAnimNotify();
                durativeNotify.BeginTriggerTime = (long)(time * 1000);
                durativeNotify.EndTriggerTime = (long)((time + 0.2f) * 1000);
                notify = durativeNotify;
            }
            else
            {
                var transientNotify = new Animation.Notify.TtTransientAnimNotify();
                transientNotify.TriggerTime = (long)(time * 1000);
                notify = transientNotify;
            }
            notify.Name = $"Notify{AnimationClip.Notifies.Count}";
            ExecuteNotifyCommand($"Add {notify.Name}",
                () => AnimationClip.Notifies.Add(notify),
                () => AnimationClip.Notifies.Remove(notify));
            mSelectedNotify = notify;
            NotifyPropGrid.Target = notify;
        }

        void PushNotifyTimeCommand(Animation.Notify.IAnimNotify notify, long oldBeginMS, long oldEndMS)
        {
            if (mEditorHistory == null || notify == null)
                return;
            long newBeginMS = notify.BeginTime;
            long newEndMS = notify.EndTime;
            if (newBeginMS == oldBeginMS && newEndMS == oldEndMS)
                return;

            var cmd = new Infrastructure.TtDelegateCommand("Move Notify",
                () => { notify.BeginTime = newBeginMS; notify.EndTime = newEndMS; },
                () => { notify.BeginTime = oldBeginMS; notify.EndTime = oldEndMS; });
            cmd.Seal();
            mEditorHistory.PushCommand(cmd);
        }

        void ExecuteNotifyCommand(string name, Action doAction, Action undoAction)
        {
            var cmd = new Infrastructure.TtDelegateCommand(name, doAction, undoAction);
            cmd.Seal();
            if (mEditorHistory != null)
                mEditorHistory.ExecuteCommand(cmd);
            else
                cmd.Do();
        }
        #endregion Notify时间轴
        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }
        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        public float PlaneScale = 5.0f;
        EngineNS.GamePlay.Scene.TtMeshNode PlaneMeshNode;
        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
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

            var planeMaterialName = TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().PlaneMaterialName;
            var studioContext = await PreviewViewport.CreateStudioEnvironment(aabb, PlaneScale, planeMaterialName);
            PlaneMeshNode = studioContext?.FloorNode;

            return true;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        TtAnimationClipPreview AnimationClipPreview = null;
        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            AnimationClip = await name.GetAsset<Animation.Asset.TtAnimationClip>();
            if (AnimationClip == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialMesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);
            AnimationClipPreview = new TtAnimationClipPreview();
            AnimationClipPreview.AnimationClipEditor = this;
            AnimationClipPreview.AnimationClip = AnimationClip;
            AnimationClipPropGrid.Target = AnimationClipPreview;
            mEditorHistory?.Clear();
            AnimationClipPropGrid.HistoryHost = mEditorHistory;
            TtEngine.Instance.TickableManager.AddTickable(this);

            var ameta = AssetName.AMeta as Animation.Asset.TtAnimationClipAMeta;
            if (ameta != null && ameta.PreviewMeshName != null)
                AnimationClipPreview.PreivewMesh = ameta.PreviewMeshName;
            else
                AnimationClipPreview.PreivewMesh = AnimationClip.PreviewMeshName;
            return true;
        }
        public async TtTask OnPreviewMeshChange(TtMaterialMesh materialMesh)
        {
            if(mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
            }

            var meshData = new EngineNS.GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData.MeshName = materialMesh.AssetName;
            meshData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMdfSkinMesh));
            meshData.AtomType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtRenderMesh.TtAtom));
            var mesh = new Graphics.Mesh.TtRenderMesh();
            mesh.Initialize(materialMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.TtPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            
            mCurrentMeshNode = meshNode;
            mPreviewStartPosition = meshNode.Placement.Position;

            //await EngineNS.Animation.SceneNode.TtAnimStateMachinePlayNode.Add(PreviewViewport.World, mCurrentMeshNode, new GamePlay.Scene.UNodeData(),
            //                EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));

            var sapnd = new EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.TtSkeletonAnimPlayNodeData();
            sapnd.Name = "PlayAnim";
            sapnd.AnimatinName = AssetName;
            sapnd.RootMotionMode = mApplyRootMotionInPreview
                ? Animation.RootMotion.ERootMotionMode.FromEverything
                : Animation.RootMotion.ERootMotionMode.Ignore;
            mAnimPlayNode = await EngineNS.Animation.SceneNode.TtSkeletonAnimPlayNode.AddSkeletonAnimPlayNode(PreviewViewport.World, mCurrentMeshNode, sapnd,
                            EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtIdentityPlacement));
        }

        class TtAnimationClipPreview
        {
            [Browsable(false)]
            public TtAnimationClipEditor AnimationClipEditor = null;
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
            private RName mPreivewMeshName;
            [RName.PGRName(FilterExts = TtMaterialMesh.AssetExt)]
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
                    if (value==null)
                        return;
                    AssetState = IO.EAssetState.Loading;
                    System.Action exec = async () =>
                    {
                        var Mesh = await value.GetAsset<Graphics.Mesh.TtMaterialMesh>();
                        if (Mesh == null)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await AnimationClipEditor.OnPreviewMeshChange(Mesh);
                        AnimationClipEditor.AnimationClip.PreviewMeshName = value;
                        var ameta = AnimationClipEditor.AssetName.AMeta as Animation.Asset.TtAnimationClipAMeta;
                        if (ameta != null)
                        {
                            ameta.PreviewMeshName = value;
                            ameta.SaveAMeta((IO.IAsset)null);
                        }
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
            TickRootMotionPreview();
            PreviewViewport.TickLogic(ellapse);
        }

        /// <summary>
        /// 预览世界里没有Movement节点, 编辑器直接消费动画节点产出的RootMotion并推动预览角色
        /// </summary>
        void TickRootMotionPreview()
        {
            if (mAnimPlayNode == null || mCurrentMeshNode == null)
                return;

            FTransform delta;
            if (!mAnimPlayNode.ConsumeRootMotion(out delta))
                return;
            if (!mApplyRootMotionInPreview)
                return;

            var placement = mCurrentMeshNode.Placement;
            var worldDelta = Animation.RootMotion.TtRootMotionUtil.ConvertDeltaToWorldTranslation(in delta, placement.Quat);
            var newQuat = delta.Quat * placement.Quat;
            newQuat.Normalize();
            placement.Position = placement.Position + worldDelta;
            placement.Quat = newQuat;
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
            return AnimationClip.AssetName.Name;
        }
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtAnimationClipEditor))]
    public partial class TtAnimationClip
    {

    }
}
