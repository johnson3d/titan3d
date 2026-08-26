using EngineNS.Animation.Asset;
using EngineNS.Animation.Montage;
using EngineNS.Animation.Notify;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Graphics.Mesh;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Editor.Forms
{
    /// <summary>
    /// Montage编辑器: 左侧属性 + 右侧预览 + 底部时间轴。
    /// 时间轴负责"时间"(段落位置/长度、Section位置、Notify打点), 明细参数走PropertyGrid。
    /// 预览直接驱动一个TtAnimMontageInstance, 与运行时走同一套推进与求值代码。
    /// </summary>
    public class TtAnimMontageEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }

        public TtAnimMontage Montage;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.TtPropertyGrid MontagePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid DetailPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        EGui.Controls.TtTimelineControl mTimeline = new EGui.Controls.TtTimelineControl();

        #region 统一Undo/Redo(开门)
        public bool EnableUndoRedo => EditorHistory != null;
        public Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        Infrastructure.TtEditorHistory mEditorHistory = new Infrastructure.TtEditorHistory();
        Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Infrastructure.TtEditorHistoryPanel();
        #endregion

        #region 预览
        GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        GamePlay.Scene.TtMeshNode PlaneMeshNode;
        TtAnimatableSkeletonPose mBindingPose = null;
        TtLocalSpaceRuntimePose mAnimatedPose = null;
        TtLocalSpaceRuntimePose mSlotPose = null;
        TtAnimMontageInstance mPreviewInstance = null;
        bool mPreviewDirty = false;
        bool mIsPlaying = false;
        bool mLoopPreview = true;
        bool mApplyRootMotionInPreview = false;
        DVector3 mPreviewStartPosition = DVector3.Zero;
        public float PlaneScale = 5.0f;
        #endregion 预览

        #region 时间轴编辑态
        const int SectionTrackOffset = 0;   // 轨道排列: [0]=Sections, [1]=Notifies, [2..]=Slots
        const int NotifyTrackOffset = 1;
        const int SlotTrackOffset = 2;
        object mSelectedObject = null;
        // 拖拽期间的旧值, 松手后一次性入历史
        object mDraggingObject = null;
        float mDragOldBegin = 0.0f;
        float mDragOldEnd = 0.0f;
        bool mPopupForItem = false;
        int mPopupTrackIndex = -1;
        float mPopupTime = 0.0f;
        bool mLoadingClips = false;
        #endregion 时间轴编辑态

        ~TtAnimMontageEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Montage = null;
            mPreviewInstance = null;
            mBindingPose = null;
            mAnimatedPose = null;
            mSlotPose = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            MontagePropGrid.Target = null;
            MontagePropGrid.HistoryHost = null;
            DetailPropGrid.Target = null;
            DetailPropGrid.HistoryHost = null;
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
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";

        public IRootForm GetRootForm()
        {
            return this;
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await MontagePropGrid.Initialize();
            await DetailPropGrid.Initialize();
            return true;
        }

        public void OnCloseEditor()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }

        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            Montage = await name.GetAsset<TtAnimMontage>();
            if (Montage == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"AnimMontage:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            MontagePropGrid.Target = Montage;
            mEditorHistory?.Clear();
            MontagePropGrid.HistoryHost = mEditorHistory;
            DetailPropGrid.HistoryHost = mEditorHistory;
            TtEngine.Instance.TickableManager.AddTickable(this);

            var previewMesh = Montage.PreviewMeshName;
            var ameta = AssetName.AMeta as TtAnimMontageAMeta;
            if (ameta != null && ameta.PreviewMeshName != null)
                previewMesh = ameta.PreviewMeshName;
            if (previewMesh != null)
            {
                var mesh = await previewMesh.GetAsset<TtMaterialMesh>();
                if (mesh != null)
                    await OnPreviewMeshChange(mesh);
            }
            return true;
        }

        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;
            await viewport.World.InitWorld();
            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var aabb = new BoundingBox(3, 3, 3);
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

        public async TtTask OnPreviewMeshChange(TtMaterialMesh materialMesh)
        {
            if (mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
                mCurrentMeshNode = null;
            }

            var meshData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData.MeshName = materialMesh.AssetName;
            meshData.MdfQueueType = Rtti.TtTypeDesc.TypeStr(typeof(TtMdfSkinMesh));
            meshData.AtomType = Rtti.TtTypeDesc.TypeStr(typeof(TtRenderMesh.TtAtom));
            var mesh = new TtRenderMesh();
            mesh.Initialize(materialMesh, Rtti.TtTypeDescGetter<TtMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.TtPlacement), mesh,
                DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            mCurrentMeshNode = meshNode;
            mPreviewStartPosition = meshNode.Placement.Position;

            var sklAsset = await materialMesh.GetSkeletonAsset();
            mBindingPose = sklAsset?.Skeleton.CreatePose() as TtAnimatableSkeletonPose;
            if (mBindingPose == null)
                return;

            mAnimatedPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(mBindingPose);
            mSlotPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(mBindingPose);
            meshNode.RuntimePose = mAnimatedPose;
            RebuildPreviewInstance();

            Montage.PreviewMeshName = materialMesh.AssetName;
            var ameta = AssetName.AMeta as TtAnimMontageAMeta;
            if (ameta != null)
            {
                ameta.PreviewMeshName = materialMesh.AssetName;
                ameta.SaveAMeta((IO.IAsset)null);
            }
        }

        void RebuildPreviewInstance()
        {
            mPreviewDirty = false;
            if (Montage == null || mBindingPose == null)
                return;

            float lastPosition = mPreviewInstance != null ? mPreviewInstance.Position : 0.0f;
            var instance = new TtAnimMontageInstance();
            if (!instance.Initialize(Montage, mBindingPose))
                return;

            // 预览不需要进出混合, 直接满权重, 由编辑器控制播放/暂停
            instance.Play(1.0f, 0.0f, null);
            instance.SetWeightForPreview(1.0f);
            instance.SetPosition(lastPosition);
            if (!mIsPlaying)
                instance.Pause();
            mPreviewInstance = instance;
        }
        #endregion IAssetEditor

        #region 绘制

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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.25f, ref leftId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.35f, ref bottomId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Timeline", mDockKeyClass), bottomId);
            ImGuiAPI.DockBuilderFinish(id);
        }

        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(1000, 700);
        public void OnDraw()
        {
            if (Visible == false || Montage == null)
                return;

            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
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
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
            DrawPreview();
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
                Montage.SaveAssetTo(Montage.AssetName);
                mEditorHistory?.SetSavePoint();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton(mIsPlaying ? "Pause" : "Play", in btSize))
            {
                mIsPlaying = !mIsPlaying;
                if (mPreviewInstance != null)
                {
                    if (mIsPlaying)
                        mPreviewInstance.Resume();
                    else
                        mPreviewInstance.Pause();
                }
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Stop", in btSize))
            {
                mIsPlaying = false;
                ResetPreviewToStart();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Rebuild", in btSize))
            {
                RebuildPreviewInstance();
            }
            ImGuiAPI.SameLine(0, -1);
            EGui.UIProxy.CheckBox.DrawCheckBox("Loop", ref mLoopPreview, false);
            ImGuiAPI.SameLine(0, -1);
            bool applyRootMotion = mApplyRootMotionInPreview;
            EGui.UIProxy.CheckBox.DrawCheckBox("RootMotion", ref applyRootMotion, false);
            if (applyRootMotion != mApplyRootMotionInPreview)
            {
                mApplyRootMotionInPreview = applyRootMotion;
                if (!applyRootMotion && mCurrentMeshNode != null)
                {
                    mCurrentMeshNode.Placement.Position = mPreviewStartPosition;
                    mCurrentMeshNode.Placement.Quat = Quaternion.Identity;
                }
            }
            ImGuiAPI.SameLine(0, -1);
            Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
            Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);
        }

        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("Montage", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    MontagePropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("Selected", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    if (mSelectedObject == null)
                        ImGuiAPI.Text("Select a segment / section / notify in timeline");
                    else
                        DetailPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        bool mPreviewShow = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref mPreviewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        bool mTimelineShow = true;
        protected unsafe void DrawTimeline()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Timeline", ref mTimelineShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ImGuiAPI.Text($"Duration: {Montage.Duration:0.###}s   Section: {(mPreviewInstance != null ? mPreviewInstance.CurrentSectionName : "-")}");
                BuildTimelineData();
                var size = new Vector2(-1, 0);
                mTimeline.OnDraw(in size);
                ProcessTimelineInteraction();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        #endregion 绘制

        #region 时间轴数据与交互

        void BuildTimelineData()
        {
            mTimeline.Duration = Math.Max(Montage.Duration, 0.001f);
            mTimeline.Tracks.Clear();
            mTimeline.Items.Clear();

            mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = "Sections" });
            mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = "Notifies" });
            for (int i = 0; i < Montage.SlotTracks.Count; ++i)
            {
                mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = Montage.SlotTracks[i].SlotName });
            }

            for (int i = 0; i < Montage.Sections.Count; ++i)
            {
                var section = Montage.Sections[i];
                mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                {
                    Id = $"section_{i}",
                    Label = section.Name,
                    TrackIndex = SectionTrackOffset,
                    Begin = section.StartTime,
                    End = section.StartTime,
                    Type = EGui.Controls.ETimelineItemType.Marker,
                    Color = 0xFF20A0FF,
                    UserData = section,
                });
            }

            for (int i = 0; i < Montage.Notifies.Count; ++i)
            {
                var notify = Montage.Notifies[i];
                if (notify == null)
                    continue;
                bool durative = notify.EndTime > notify.BeginTime;
                mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                {
                    Id = $"notify_{i}",
                    Label = notify.Name,
                    TrackIndex = NotifyTrackOffset,
                    Begin = notify.BeginTime * 0.001f,
                    End = notify.EndTime * 0.001f,
                    Type = durative ? EGui.Controls.ETimelineItemType.Range : EGui.Controls.ETimelineItemType.Marker,
                    Color = durative ? 0xFF30C030u : 0xFF30FF30u,
                    AllowResize = durative,
                    UserData = notify,
                });
            }

            for (int trackIndex = 0; trackIndex < Montage.SlotTracks.Count; ++trackIndex)
            {
                var track = Montage.SlotTracks[trackIndex];
                for (int i = 0; i < track.Segments.Count; ++i)
                {
                    var segment = track.Segments[i];
                    mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                    {
                        Id = $"segment_{trackIndex}_{i}",
                        Label = segment.ClipName != null ? segment.ClipName.PureName : "<None>",
                        TrackIndex = SlotTrackOffset + trackIndex,
                        Begin = segment.StartPos,
                        End = segment.EndPos,
                        Type = EGui.Controls.ETimelineItemType.Bar,
                        Color = 0xFFB07030,
                        UserData = segment,
                    });
                }
            }

            if (mPreviewInstance != null && mIsPlaying)
                mTimeline.PlayPosition = mPreviewInstance.Position;
        }

        void ProcessTimelineInteraction()
        {
            // 拖播放头 = 手动定位预览
            if (mTimeline.PlayPositionChanged && mPreviewInstance != null)
            {
                mIsPlaying = false;
                mPreviewInstance.Pause();
                mPreviewInstance.SetPosition(mTimeline.PlayPosition);
            }

            var clicked = mTimeline.ClickedItem;
            if (clicked != null)
            {
                mSelectedObject = clicked.UserData;
                DetailPropGrid.Target = mSelectedObject;
            }

            // 拖拽开始时记住旧值, 松手后作为一条历史入栈
            var changed = mTimeline.ChangedItem;
            if (changed != null && mDraggingObject != changed.UserData)
            {
                mDraggingObject = changed.UserData;
                ReadObjectTime(mDraggingObject, out mDragOldBegin, out mDragOldEnd);
            }
            if (changed != null)
            {
                WriteObjectTime(changed.UserData, changed.Begin, changed.End);
            }
            else if (mDraggingObject != null)
            {
                PushTimeChangeCommand(mDraggingObject, mDragOldBegin, mDragOldEnd);
                mDraggingObject = null;
            }

            // 右键菜单: OpenPopup与BeginPopup都不在PushID作用域内, 保证能弹出
            if (mTimeline.RightClickedItem != null)
            {
                mPopupForItem = true;
                mSelectedObject = mTimeline.RightClickedItem.UserData;
                DetailPropGrid.Target = mSelectedObject;
                mPopupTrackIndex = mTimeline.RightClickedItem.TrackIndex;
                ImGuiAPI.OpenPopup("MontageTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            else if (mTimeline.RightClickedTrack >= 0)
            {
                mPopupForItem = false;
                mPopupTrackIndex = mTimeline.RightClickedTrack;
                mPopupTime = mTimeline.RightClickedTime;
                ImGuiAPI.OpenPopup("MontageTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            DrawTimelineMenu();
        }

        void DrawTimelineMenu()
        {
            if (!ImGuiAPI.BeginPopup("MontageTimelineMenu", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                return;

            if (mPopupForItem)
            {
                if (ImGuiAPI.MenuItem("Remove", null, false, true))
                {
                    RemoveSelectedObject();
                }
            }
            else
            {
                if (mPopupTrackIndex == SectionTrackOffset)
                {
                    if (ImGuiAPI.MenuItem("Add Section Here", null, false, true))
                        AddSection(mPopupTime);
                }
                else if (mPopupTrackIndex == NotifyTrackOffset)
                {
                    if (ImGuiAPI.MenuItem("Add Transient Notify", null, false, true))
                        AddNotify(mPopupTime, false);
                    if (ImGuiAPI.MenuItem("Add Durative Notify", null, false, true))
                        AddNotify(mPopupTime, true);
                }
                else
                {
                    if (ImGuiAPI.MenuItem("Add Segment Here", null, false, true))
                        AddSegment(mPopupTrackIndex - SlotTrackOffset, mPopupTime);
                }
                if (ImGuiAPI.MenuItem("Add Slot Track", null, false, true))
                    AddSlotTrack();
            }
            ImGuiAPI.EndPopup();
        }

        static void ReadObjectTime(object target, out float begin, out float end)
        {
            begin = 0.0f;
            end = 0.0f;
            switch (target)
            {
                case TtMontageSegment segment:
                    begin = segment.StartPos;
                    end = segment.EndPos;
                    break;
                case TtMontageSection section:
                    begin = section.StartTime;
                    end = section.StartTime;
                    break;
                case IAnimNotify notify:
                    begin = notify.BeginTime * 0.001f;
                    end = notify.EndTime * 0.001f;
                    break;
            }
        }
        static void WriteObjectTime(object target, float begin, float end)
        {
            switch (target)
            {
                case TtMontageSegment segment:
                    segment.StartPos = begin;
                    break;
                case TtMontageSection section:
                    section.StartTime = begin;
                    break;
                case IAnimNotify notify:
                    notify.BeginTime = (Int64)(begin * 1000);
                    notify.EndTime = (Int64)(end * 1000);
                    break;
            }
        }
        void PushTimeChangeCommand(object target, float oldBegin, float oldEnd)
        {
            if (mEditorHistory == null || target == null)
                return;

            float newBegin;
            float newEnd;
            ReadObjectTime(target, out newBegin, out newEnd);
            if (newBegin == oldBegin && newEnd == oldEnd)
                return;

            var cmd = new Infrastructure.TtDelegateCommand("Move Timeline Item",
                () => WriteObjectTime(target, newBegin, newEnd),
                () => WriteObjectTime(target, oldBegin, oldEnd));
            cmd.Seal();
            mEditorHistory.PushCommand(cmd);
        }

        void AddSection(float time)
        {
            var section = new TtMontageSection();
            section.Name = $"Section{Montage.Sections.Count}";
            section.StartTime = time;
            ExecuteStructureCommand($"Add {section.Name}",
                () => Montage.Sections.Add(section),
                () => Montage.Sections.Remove(section));
            mSelectedObject = section;
            DetailPropGrid.Target = section;
        }
        void AddNotify(float time, bool durative)
        {
            IAnimNotify notify;
            if (durative)
            {
                var durativeNotify = new TtDurativeAnimNotify();
                durativeNotify.BeginTriggerTime = (Int64)(time * 1000);
                durativeNotify.EndTriggerTime = (Int64)((time + 0.2f) * 1000);
                notify = durativeNotify;
            }
            else
            {
                var transientNotify = new TtTransientAnimNotify();
                transientNotify.TriggerTime = (Int64)(time * 1000);
                notify = transientNotify;
            }
            notify.Name = $"Notify{Montage.Notifies.Count}";
            ExecuteStructureCommand($"Add {notify.Name}",
                () => Montage.Notifies.Add(notify),
                () => Montage.Notifies.Remove(notify));
            mSelectedObject = notify;
            DetailPropGrid.Target = notify;
        }
        void AddSegment(int slotIndex, float time)
        {
            if (slotIndex < 0 || slotIndex >= Montage.SlotTracks.Count)
                return;

            var track = Montage.SlotTracks[slotIndex];
            var segment = new TtMontageSegment();
            segment.StartPos = time;
            ExecuteStructureCommand("Add Segment",
                () => track.Segments.Add(segment),
                () => track.Segments.Remove(segment));
            mSelectedObject = segment;
            DetailPropGrid.Target = segment;
        }
        void AddSlotTrack()
        {
            var track = new TtMontageSlotTrack();
            track.SlotName = Montage.SlotTracks.Count == 0 ? "DefaultSlot" : $"Slot{Montage.SlotTracks.Count}";
            ExecuteStructureCommand($"Add {track.SlotName}",
                () => Montage.SlotTracks.Add(track),
                () => Montage.SlotTracks.Remove(track));
            mSelectedObject = track;
            DetailPropGrid.Target = track;
        }
        void RemoveSelectedObject()
        {
            var target = mSelectedObject;
            if (target == null)
                return;

            switch (target)
            {
                case TtMontageSegment segment:
                    {
                        var track = FindTrackOfSegment(segment);
                        if (track == null)
                            return;
                        int index = track.Segments.IndexOf(segment);
                        ExecuteStructureCommand("Remove Segment",
                            () => track.Segments.Remove(segment),
                            () => track.Segments.Insert(Math.Min(index, track.Segments.Count), segment));
                    }
                    break;
                case TtMontageSection section:
                    {
                        int index = Montage.Sections.IndexOf(section);
                        ExecuteStructureCommand("Remove Section",
                            () => Montage.Sections.Remove(section),
                            () => Montage.Sections.Insert(Math.Min(index, Montage.Sections.Count), section));
                    }
                    break;
                case IAnimNotify notify:
                    {
                        int index = Montage.Notifies.IndexOf(notify);
                        ExecuteStructureCommand("Remove Notify",
                            () => Montage.Notifies.Remove(notify),
                            () => Montage.Notifies.Insert(Math.Min(index, Montage.Notifies.Count), notify));
                    }
                    break;
            }
            mSelectedObject = null;
            DetailPropGrid.Target = null;
        }
        TtMontageSlotTrack FindTrackOfSegment(TtMontageSegment segment)
        {
            for (int i = 0; i < Montage.SlotTracks.Count; ++i)
            {
                if (Montage.SlotTracks[i].Segments.Contains(segment))
                    return Montage.SlotTracks[i];
            }
            return null;
        }
        /// <summary>
        /// 结构性修改(增删)统一走历史, 并标记预览需要重建(采样器绑定在实例初始化时建立)
        /// </summary>
        void ExecuteStructureCommand(string name, Action doAction, Action undoAction)
        {
            var cmd = new Infrastructure.TtDelegateCommand(name,
                () => { doAction(); mPreviewDirty = true; },
                () => { undoAction(); mPreviewDirty = true; });
            cmd.Seal();
            if (mEditorHistory != null)
                mEditorHistory.ExecuteCommand(cmd);
            else
                cmd.Do();
        }
        #endregion 时间轴数据与交互

        #region ITickable

        public void TickLogic(float ellapse)
        {
            TickPreview(ellapse);
            PreviewViewport.TickLogic(ellapse);
        }

        void TickPreview(float ellapse)
        {
            if (Montage == null)
                return;

            EnsureSegmentClipsLoaded();
            if (mPreviewDirty)
                RebuildPreviewInstance();
            if (mPreviewInstance == null || mAnimatedPose == null)
                return;

            if (mIsPlaying)
            {
                mPreviewInstance.Advance(ellapse, ERootMotionMode.FromMontagesOnly);
                mPreviewInstance.SetWeightForPreview(1.0f);
                if (!mPreviewInstance.IsActive)
                {
                    if (mLoopPreview)
                        ResetPreviewToStart();
                    else
                        mIsPlaying = false;
                }
                mTimeline.PlayPosition = mPreviewInstance.Position;

                if (mApplyRootMotionInPreview && mCurrentMeshNode != null)
                {
                    var rootMotion = mPreviewInstance.RootMotion;
                    if (rootMotion.HasRootMotion)
                    {
                        var placement = mCurrentMeshNode.Placement;
                        var worldDelta = TtRootMotionUtil.ConvertDeltaToWorldTranslation(rootMotion.Delta, placement.Quat);
                        var newQuat = rootMotion.Delta.Quat * placement.Quat;
                        newQuat.Normalize();
                        placement.Position = placement.Position + worldDelta;
                        placement.Quat = newQuat;
                    }
                }
            }

            // 依次求值每条Slot, 后面的轨道覆盖前面的(预览用简单叠加)
            for (int i = 0; i < Montage.SlotTracks.Count; ++i)
            {
                if (mPreviewInstance.EvaluateSlotPose(Montage.SlotTracks[i].SlotName, ref mSlotPose))
                    TtRuntimePoseUtility.CopyPose(ref mAnimatedPose, mSlotPose);
            }
        }

        void ResetPreviewToStart()
        {
            if (mPreviewInstance == null)
            {
                RebuildPreviewInstance();
                return;
            }
            mPreviewInstance.Play(1.0f, 0.0f, null);
            mPreviewInstance.SetWeightForPreview(1.0f);
            if (!mIsPlaying)
                mPreviewInstance.Pause();
            mTimeline.PlayPosition = mPreviewInstance.Position;
            if (mCurrentMeshNode != null && mApplyRootMotionInPreview)
            {
                mCurrentMeshNode.Placement.Position = mPreviewStartPosition;
                mCurrentMeshNode.Placement.Quat = Quaternion.Identity;
            }
        }

        /// <summary>
        /// PropertyGrid里改过ClipName的段落需要异步补载动画资产
        /// </summary>
        void EnsureSegmentClipsLoaded()
        {
            if (mLoadingClips)
                return;

            for (int i = 0; i < Montage.SlotTracks.Count; ++i)
            {
                var segments = Montage.SlotTracks[i].Segments;
                for (int j = 0; j < segments.Count; ++j)
                {
                    var segment = segments[j];
                    if (segment.ClipName == null)
                        continue;
                    if (segment.Clip != null && segment.Clip.AssetName == segment.ClipName)
                        continue;

                    mLoadingClips = true;
                    var target = segment;
                    System.Action exec = async () =>
                    {
                        target.Clip = await target.ClipName.GetAsset<TtAnimationClip>();
                        mPreviewDirty = true;
                        mLoadingClips = false;
                    };
                    exec();
                    return;
                }
            }
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
        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        public string GetWindowsName()
        {
            return Montage.AssetName.Name;
        }
        #endregion ITickable
    }
}

namespace EngineNS.Animation.Asset
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtAnimMontageEditor))]
    public partial class TtAnimMontage
    {
    }
}
