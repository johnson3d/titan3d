using EngineNS.Animation.Asset;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Editor.Forms
{
    /// <summary>
    /// TtPoseAsset 编辑器。结构对标 TtAnimationClipEditor(预览视口 + PropertyGrid + dock 布局):
    ///   - 左侧: 资产属性 + pose 列表 + 从 AnimationClip 导入 pose 的控件;
    ///   - 右侧: 骨架预览(选中某个 pose 时把它应用到预览骨架上)。
    /// pose 的逐骨骼微调走 PropertyGrid(选中 pose 后编辑其 Transforms), 不做 3D gizmo。
    /// </summary>
    public class TtPoseAssetEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }

        public Animation.Asset.TtPoseAsset PoseAsset;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        /// <summary>
        /// 单一属性面板: 同时承载"预览模型 + 导入设置"。
        /// 不用多个 TtPropertyGrid 堆在同一面板里 —— 它们内部的表格 id 会互相干扰,
        /// 导致后面的控件/折叠头画不出来。
        /// </summary>
        public EGui.Controls.PropertyGrid.TtPropertyGrid PropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid PosePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        TtPoseAssetPreview mPreview = null;

        #region 统一Undo/Redo(开门)
        public bool EnableUndoRedo => EditorHistory != null;
        public Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        Infrastructure.TtEditorHistory mEditorHistory = new Infrastructure.TtEditorHistory();
        Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Infrastructure.TtEditorHistoryPanel();
        #endregion

        ~TtPoseAssetEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            PoseAsset = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            PropGrid.Target = null;
            PropGrid.HistoryHost = null;
            PosePropGrid.Target = null;
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
            await PropGrid.Initialize();
            await PosePropGrid.Initialize();
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.3f, ref leftId, ref rightId);

            // 属性与 pose 列表各占一个面板(同一 dock 区的两个 tab), 避免互抢剩余空间
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Poses", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }

        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(900, 600);
        public void OnDraw()
        {
            if (Visible == false || PoseAsset == null)
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
            DrawPoses();
            DrawRight();
            if (mEditorHistory != null)
                mHistoryPanel.OnDraw(in mDockKeyClass, "History", mEditorHistory);
        }

        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                PoseAsset.SaveAssetTo(PoseAsset.AssetName);
                mEditorHistory?.SetSavePoint();
            }
            ImGuiAPI.SameLine(0, -1);
            Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
            Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);
        }

        // 选中的 pose 名
        string mSelectedPoseName = null;
        // 重命名输入框的缓冲与它当前对应的 pose
        string mRenameBuffer = null;
        string mRenameTargetName = null;

        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                // 整个面板就交给 PG(它会占满剩余空间)。导入动作已做成 PG 内的按钮属性,
                // pose 列表去了独立的 Poses 面板, 两者不再互抢空间。
                PropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        bool mPosesShow = true;
        protected unsafe void DrawPoses()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Poses", ref mPosesShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ImGuiAPI.Text($"Poses ({PoseAsset.PoseCount})   Bones: {PoseAsset.BoneCount}");
                ImGuiAPI.Separator();

                // 列表占上半部分, 下半给选中 pose 的逐骨骼属性
                var listSize = new Vector2(0, 160);
                ImGuiAPI.BeginChild("PoseList", in listSize,
                    ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None);
                DrawPoseList();
                ImGuiAPI.EndChild();

                if (mSelectedPoseName != null)
                {
                    ImGuiAPI.Separator();
                    DrawSelectedPoseRename();
                    ImGuiAPI.PushID("SelectedPosePG");
                    PosePropGrid.OnDraw(true, false, false);
                    ImGuiAPI.PopID();
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        /// <summary> 供 PG 上的导入按钮回调。 </summary>
        internal void DoImportPose()
        {
            DoImportPoseInternal();
        }

        void DoImportPoseInternal()
        {
            if (mPreview == null || mPreview.Clip == null)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PoseAsset",
                    "Import failed: Clip 未选择");
                return;
            }
            if (mBindPose == null)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PoseAsset",
                    "Import failed: 预览骨架未就绪, 请先设置 PreviewMesh");
                return;
            }

            var clipName = mPreview.Clip;
            float time = mPreview.Time;
            string desiredName = string.IsNullOrEmpty(mPreview.PoseName) ? null : mPreview.PoseName;
            System.Action exec = async () =>
            {
                var clip = await clipName.GetAsset<TtAnimationClip>();
                if (clip == null)
                    return;
                var imported = Animation.Asset.TtPoseAssetUtil.ImportPoseFromClip(PoseAsset, clip, mBindPose, time, desiredName);
                if (imported != null)
                    SelectPose(imported.Name);
                else
                    Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PoseAsset",
                        "Import failed: 从 clip 采样 pose 失败(骨架不匹配或 clip 无数据)");
            };
            exec();
        }

        unsafe void DrawPoseList()
        {
            int removeIndex = -1;
            const float RemoveButtonWidth = 26.0f;

            for (int i = 0; i < PoseAsset.Poses.Count; ++i)
            {
                var pose = PoseAsset.Poses[i];
                ImGuiAPI.PushID(i);

                // Selectable 必须显式限宽: 它默认(size.x=0)会占满整行, 命中区会盖住
                // 后面 SameLine 画的删除按钮, 导致按钮永远点不到(只触发选中)。
                float avail = ImGuiAPI.GetContentRegionAvail().X;
                float selWidth = avail - RemoveButtonWidth - 8.0f;
                if (selWidth < 40.0f)
                    selWidth = 40.0f;
                var selSize = new Vector2(selWidth, 0);

                bool selected = (pose.Name == mSelectedPoseName);
                if (ImGuiAPI.Selectable(pose.Name, selected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in selSize))
                    SelectPose(pose.Name);
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    EGui.Controls.CtrlUtility.DrawHelper(pose.Name);

                ImGuiAPI.SameLine(0, 4);
                var xSize = new Vector2(RemoveButtonWidth, 0);
                if (EGui.UIProxy.CustomButton.ToolButton("X", in xSize))
                    removeIndex = i;
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    EGui.Controls.CtrlUtility.DrawHelper($"删除 pose '{pose.Name}'");

                ImGuiAPI.PopID();
            }

            if (removeIndex >= 0)
            {
                var name = PoseAsset.Poses[removeIndex].Name;
                PoseAsset.RemovePose(name);
                if (mSelectedPoseName == name)
                {
                    // 删掉的正是当前预览的, 改选剩下的第一个(没了就取消选择)
                    SelectPose(PoseAsset.PoseCount > 0 ? PoseAsset.Poses[0].Name : null);
                }
            }
        }

        /// <summary>
        /// 重命名当前选中的 pose。走 TtPoseAsset.RenamePose 以便做重名检查 ——
        /// PoseDriver 的 target 是按名字引用 pose 的, 出现同名会让引用变得不确定。
        /// </summary>
        unsafe void DrawSelectedPoseRename()
        {
            if (mSelectedPoseName == null)
                return;

            if (mRenameBuffer == null || mRenameTargetName != mSelectedPoseName)
            {
                mRenameBuffer = mSelectedPoseName;
                mRenameTargetName = mSelectedPoseName;
            }

            ImGuiAPI.Text("Name");
            ImGuiAPI.SameLine(0, 6);
            ImGuiAPI.SetNextItemWidth(180);
            ImGuiAPI.InputText("##PoseRename", ref mRenameBuffer);
            ImGuiAPI.SameLine(0, 6);
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Rename", in btSize))
            {
                var newName = mRenameBuffer;
                if (!string.IsNullOrEmpty(newName) && newName != mSelectedPoseName)
                {
                    if (PoseAsset.RenamePose(mSelectedPoseName, newName))
                    {
                        SelectPose(newName);
                    }
                    else
                    {
                        Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PoseAsset",
                            $"重命名失败: '{newName}' 已存在或非法");
                        mRenameBuffer = mSelectedPoseName;
                    }
                }
            }
        }

        void SelectPose(string poseName)
        {
            mSelectedPoseName = poseName;
            var pose = (poseName != null) ? PoseAsset.FindPose(poseName) : null;
            PosePropGrid.Target = pose;

            // 把选中的 pose 写到绑定在 meshNode 上的 RuntimePose, 预览才会变
            ApplySelectedPoseToPreview();
        }

        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                // 在视口上方提示当前预览的是哪个 pose(多 pose 资产里很容易搞混)
                if (mSelectedPoseName != null)
                    ImGuiAPI.Text($"Previewing Pose: {mSelectedPoseName}");
                else if (PoseAsset.PoseCount == 0)
                    ImGuiAPI.Text("No pose yet - import one from a clip");
                else
                    ImGuiAPI.Text("No pose selected (bind pose)");

                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        public void OnEvent(in Bricks.Input.Event e)
        {
        }

        #endregion IAssetEditor

        #region 预览场景

        EngineNS.GamePlay.Scene.TtMeshNode mCurrentMeshNode;
        EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose mBindPose;
        EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose mPreviewRuntimePose;
        public float PlaneScale = 5.0f;

        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport,
            TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
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
            await PreviewViewport.CreateStudioEnvironment(aabb, PlaneScale, planeMaterialName);
            return true;
        }

        public async TtTask OnPreviewMeshChange(TtMaterialMesh materialMesh)
        {
            if (mCurrentMeshNode != null)
                mCurrentMeshNode.Parent = null;

            var meshData = new EngineNS.GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData.MeshName = materialMesh.AssetName;
            meshData.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMdfSkinMesh));
            meshData.AtomType = EngineNS.Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtRenderMesh.TtAtom));
            var mesh = new Graphics.Mesh.TtRenderMesh();
            mesh.Initialize(materialMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfSkinMesh>.TypeDesc);

            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData,
                typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;
            mCurrentMeshNode = meshNode;

            // 用预览 mesh 的骨架建立一份可动画 pose, 供从 clip 采样
            mBindPose = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(meshNode);

            // 预览用的 RuntimePose 必须是"绑到 meshNode 上的那一份"(BindRuntimeSkeletonPoseToNode
            // 会设 meshNode.RuntimePose), 否则写一份游离的 pose 对渲染没任何影响。
            mPreviewRuntimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(meshNode);

            // 资产还没确定骨骼顺序时, 以预览骨架的骨骼顺序初始化
            if (PoseAsset.BoneCount == 0)
            {
                var orderSource = (mPreviewRuntimePose != null)
                    ? (EngineNS.Animation.SkeletonAnimation.Runtime.Pose.IRuntimePose)mPreviewRuntimePose
                    : ((mBindPose != null) ? EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(mBindPose) : null);
                if (orderSource != null)
                    PoseAsset.SetBoneNames(Animation.Asset.TtPoseAssetUtil.ExtractBoneNames(orderSource));
            }

            // 已选中的 pose 在模型重建后需重新应用一次;
            // 没选中但资产里有 pose 时, 默认预览 0 号 pose(比绑定姿势更有参考价值)
            if (mSelectedPoseName == null && PoseAsset.PoseCount > 0)
                SelectPose(PoseAsset.Poses[0].Name);
            else if (mSelectedPoseName != null)
                ApplySelectedPoseToPreview();
        }

        void ApplySelectedPoseToPreview()
        {
            if (mSelectedPoseName == null || mPreviewRuntimePose == null)
                return;
            Animation.Asset.TtPoseAssetUtil.ApplyPoseToRuntimePose(PoseAsset, mSelectedPoseName, mPreviewRuntimePose);

            // 必须重新赋值一次: 渲染实际消费的是 MeshNode.MeshSpaceRuntimePose, 而它只在
            // MeshNode.RuntimePose 的 setter 里重算(ConvetToMeshSpaceRuntimePose)。只改 local pose
            // 的 Transforms 内容不会触发 setter, 骨骼矩阵就一直停在绑定姿势上。
            if (mCurrentMeshNode != null)
                mCurrentMeshNode.RuntimePose = mPreviewRuntimePose;
        }

        #endregion 预览场景

        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";

        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            PoseAsset = await name.GetAsset<Animation.Asset.TtPoseAsset>();
            if (PoseAsset == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"PoseAsset:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            mPreview = new TtPoseAssetPreview();
            mPreview.Editor = this;
            PropGrid.Target = mPreview;
            mEditorHistory?.Clear();
            PropGrid.HistoryHost = mEditorHistory;
            TtEngine.Instance.TickableManager.AddTickable(this);

            // 载入预览模型: 走 mPreview.PreviewMesh 的 setter, 以便复用同一条加载路径
            var ameta = AssetName.AMeta as Animation.Asset.TtPoseAssetAMeta;
            RName previewMesh = (ameta != null && ameta.PreviewMeshName != null) ? ameta.PreviewMeshName : PoseAsset.PreviewMeshName;
            if (previewMesh != null)
                mPreview.PreviewMesh = previewMesh;
            return true;
        }

        #region ITickable
        public void TickLogic(float ellapse)
        {
            // 每帧重新应用一次选中的 pose: 这样在属性面板里逐骨骼微调时能实时看到效果。
            // 成本是一次 pose 写入 + 一次 mesh space 转换, 对编辑器而言可忽略。
            ApplySelectedPoseToPreview();
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
            return PoseAsset.AssetName.Name;
        }
        #endregion ITickable

        // 属性面板的宝装对象: 预览模型 + 导入设置。
        // PreviewMesh 必须走这个包装类而不是直接把资产给 PG —— 需要在 setter 里异步
        // 加载模型并重建预览骨架(与 AnimationClipEditor 的 TtAnimationClipPreview 同思路)。
        class TtPoseAssetPreview
        {
            [System.ComponentModel.Browsable(false)]
            public TtPoseAssetEditor Editor = null;
            [System.ComponentModel.Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;

            RName mPreviewMeshName;
            [System.ComponentModel.Category("Preview")]
            [System.ComponentModel.DisplayName("PreviewMesh")]
            [RName.PGRName(FilterExts = TtMaterialMesh.AssetExt)]
            public RName PreviewMesh
            {
                get { return mPreviewMeshName; }
                set
                {
                    if (AssetState == IO.EAssetState.Loading)
                        return;
                    mPreviewMeshName = value;
                    if (value == null)
                        return;
                    AssetState = IO.EAssetState.Loading;
                    System.Action exec = async () =>
                    {
                        var mesh = await value.GetAsset<Graphics.Mesh.TtMaterialMesh>();
                        if (mesh == null)
                        {
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                        await Editor.OnPreviewMeshChange(mesh);
                        Editor.PoseAsset.PreviewMeshName = value;
                        var ameta = Editor.AssetName.AMeta as Animation.Asset.TtPoseAssetAMeta;
                        if (ameta != null)
                        {
                            ameta.PreviewMeshName = value;
                            ameta.SaveAMeta((IO.IAsset)null);
                        }
                    };
                    exec();
                }
            }

            /// <summary>
            /// 取当前资产的 ameta。导入设置(Clip/Time)存在 ameta 而不是资产本体 ——
            /// 它们是纯编辑期数据, 这样 .poseasset 保持干净, cook 时只处理 ameta。
            /// SaveAssetTo 内部会调 ameta.SaveAMeta, 所以点 Save 就会一并落盘。
            /// </summary>
            Animation.Asset.TtPoseAssetAMeta GetAMeta()
            {
                return (Editor != null && Editor.AssetName != null)
                    ? Editor.AssetName.AMeta as Animation.Asset.TtPoseAssetAMeta
                    : null;
            }

            [System.ComponentModel.Category("Import")]
            [System.ComponentModel.DisplayName("Clip")]
            [RName.PGRName(FilterExts = TtAnimationClip.AssetExt)]
            public RName Clip
            {
                get { var m = GetAMeta(); return (m != null) ? m.ImportClipName : null; }
                set { var m = GetAMeta(); if (m != null) m.ImportClipName = value; }
            }

            [System.ComponentModel.Category("Import")]
            [System.ComponentModel.DisplayName("Time(s)")]
            public float Time
            {
                get { var m = GetAMeta(); return (m != null) ? m.ImportTime : 0.0f; }
                set { var m = GetAMeta(); if (m != null) m.ImportTime = value; }
            }

            [System.ComponentModel.Category("Import")]
            [System.ComponentModel.DisplayName("Pose Name(optional)")]
            public string PoseName { get; set; } = "";

            /// <summary>
            /// 展示为一个按钮的"属性": 值不用, 只为了把导入动作放进属性面板里。
            /// 放在 PG 内而不是外面, 是因为 PG 会占满剩余空间, 在它之后画的控件看不见。
            /// </summary>
            [System.ComponentModel.Category("Import")]
            [System.ComponentModel.DisplayName("Action")]
            [TtPGButtonEditor(ButtonText = "Import Pose From Clip", MethodName = "DoImport",
                Tooltip = "从上面选定的 Clip 在指定时刻采样一个 pose 并加入资产")]
            public int ImportAction { get; set; } = 0;

            // 由 TtPGButtonEditor 通过方法名反射调用
            void DoImport()
            {
                Editor?.DoImportPose();
            }
        }
    }
}

namespace EngineNS.Animation.Asset
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtPoseAssetEditor))]
    public partial class TtPoseAsset
    {
    }
}
