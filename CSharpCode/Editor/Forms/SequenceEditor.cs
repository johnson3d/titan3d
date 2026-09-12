using EngineNS.Graphics.Mesh;
using EngineNS.Sequencer;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.Forms
{
    /// <summary>
    /// 序列编辑器: 左侧属性 + 绑定面板, 右侧预览视口, 底部时间轴。
    ///
    /// 与 Montage 编辑器最大的差别是"被编辑的对象在场景里": 时间轴上拖播放头会直接调
    /// TtSequencePlayer.SetPosition 改预览世界里真实节点的 Placement。因为播放器求值是
    /// 无状态的 (同一 tick 反复求值结果一致), scrub 与顺序播放走的是同一条代码路径。
    ///
    /// 编辑器持有自己的预览世界而不是去操作主场景: 序列资产可以在没有任何场景打开的情况下
    /// 被编辑, 且不会把编辑期的属性写入用户正在编辑的场景。关闭面板时 RestorePreAnimatedState
    /// 把被驱动过的属性恢复回进入前的值。
    ///
    /// 时间的单位在这里有一次换算: 序列内部一律是 Int64 tick, 而 TtTimelineControl 全程是
    /// float 秒。换算集中在 TickToSeconds / SecondsToTick 两个方法里, 别在其他地方直接算。
    /// </summary>
    public class TtSequenceEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }

        public Sequencer.Asset.TtSequence Sequence;
        public Editor.TtPreviewViewport PreviewViewport = new Editor.TtPreviewViewport();
        public EGui.Controls.PropertyGrid.TtPropertyGrid SequencePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        public EGui.Controls.PropertyGrid.TtPropertyGrid DetailPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        /// <summary>
        /// “Set Value &amp; Key” 弹窗里的属性面板。Target 摆的是预览世界的节点, 靠
        /// FilterString 收窄到目标那一条属性上。
        ///
        /// 存在的理由: 打属性关键帧读的是节点当前值 (见 KeyProperty), 所以节点上得先有值
        /// 才打得出点。没有这个入口的话, 像 BehaviorName 这种引用类型属性在本编辑器里就是
        /// 死循环 —— 没地方赋值, 于是永远是 null, 于是永远打不出关键帧。
        ///
        /// 故意不挂 HistoryHost: mEditorHistory 里装的是序列资产的改动, 而这里改的是预览
        /// 世界的节点。混在一条历史里, Ctrl+Z 会交替回退两种东西; 更要紧的是那些命令持有
        /// 节点引用, 编辑器一关预览世界就销毁了, 命令会变成悬空引用。改动本身也不会存进
        /// 预览场景资产 —— 值一旦打成关键帧就由序列保管了。
        /// </summary>
        public EGui.Controls.PropertyGrid.TtPropertyGrid KeyValuePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();
        EGui.Controls.TtTimelineControl mTimeline = new EGui.Controls.TtTimelineControl();
        TtSequencePlayer mPlayer = new TtSequencePlayer();

        #region 统一Undo/Redo(开门)
        public bool EnableUndoRedo => EditorHistory != null;
        public Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        Infrastructure.TtEditorHistory mEditorHistory = new Infrastructure.TtEditorHistory();
        Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Infrastructure.TtEditorHistoryPanel();
        #endregion

        #region 编辑态
        /// <summary>
        /// 播放头位置, 是任意 tick 而不吸附到整帧: 吸附会让拖动变成一档一档跳
        /// (30fps + TickResolution 24000 时一档 800 tick, 屏幕上约 10 像素)。求值对任意
        /// tick 都是连续插值的, 所以 scrub 没有吸附的理由 —— 只有打点时才需要 (见 SnapTick)。
        /// </summary>
        long mCurrentTick = 0;
        object mSelectedObject = null;
        Guid mSelectedBindingId = Guid.Empty;
        GamePlay.Scene.TtNode mSelectedSceneNode = null;
        public float PlaneScale = 5.0f;

        // 拖拽期间只改数据, 松手后把"拖之前的快照"作为一条历史入栈
        Sequencer.Asset.TtSequenceSection mDragSection = null;
        object mDragItemData = null;
        FSectionSnapshot mDragBefore = null;

        // 每帧重建时间轴用的临时容器, 复用避免每帧分配
        List<long> mKeyTickBuffer = new List<long>();
        List<ISequenceChannel> mChannelBuffer = new List<ISequenceChannel>();

        bool mPopupForItem = false;
        int mPopupTrackIndex = -1;
        float mPopupTime = 0.0f;

        /// <summary>
        /// "加属性轨道"弹窗的候选属性。在打开弹窗那一刻按目标节点的实际类型现取,
        /// 不长期缓存: 节点类型在编辑期可能因 C# 热重载而换成新的 Type 对象。
        /// </summary>
        List<System.Reflection.PropertyInfo> mPropertyCandidates = new List<System.Reflection.PropertyInfo>();
        #endregion 编辑态

        ~TtSequenceEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Sequence = null;
            mDragSection = null;
            mDragItemData = null;
            mDragBefore = null;
            mSelectedObject = null;
            mSelectedSceneNode = null;
            CoreSDK.DisposeObject(ref PreviewViewport);
            SequencePropGrid.Target = null;
            SequencePropGrid.HistoryHost = null;
            DetailPropGrid.Target = null;
            DetailPropGrid.HistoryHost = null;
            KeyValuePropGrid.Target = null;
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
            await SequencePropGrid.Initialize();
            await DetailPropGrid.Initialize();
            await KeyValuePropGrid.Initialize();
            return true;
        }

        public void OnCloseEditor()
        {
            // 必须恢复原值: 否则预览世界虽然一起销毁了, 但播放器写过的属性如果指向别处
            // (以后支持绑主场景节点) 就会永久停在播放头那一帧的姿态上
            mPlayer.RestorePreAnimatedState();
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }

        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            Sequence = await name.GetAsset<Sequencer.Asset.TtSequence>();
            if (Sequence == null)
                return false;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"Sequence:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);
            // 在视口里点节点才是用户的第一反应, Bindings 里那份文字列表只是备选入口。
            // 不接这个回调的话, 点中了节点 (视口里已经高亮了) 但属性面板不跟着变。
            PreviewViewport.OnNodeSelectedChanged = SelectSceneNode;

            SequencePropGrid.Target = Sequence;
            mEditorHistory?.Clear();
            SequencePropGrid.HistoryHost = mEditorHistory;
            DetailPropGrid.HistoryHost = mEditorHistory;
            TtEngine.Instance.TickableManager.AddTickable(this);

            Sequence.ExpandPlaybackRangeToContent();
            if (Sequence.PlaybackEndTick <= Sequence.PlaybackStartTick)
            {
                // 新建出来的序列播放范围是空的, 时间轴会退化成零长度没法操作。给一个 5 秒的
                // 缺省范围, 它是 Playback 分类下的公开属性, 用户随时能改。
                Sequence.PlaybackEndTick = Sequence.PlaybackStartTick + Sequence.TickResolution.FromSeconds(5.0);
            }
            mPlayer.Initialize(Sequence, PreviewViewport.World?.Root);
            mPlayer.RestoreStateOnStop = true;
            mCurrentTick = Sequence.PlaybackStartTick;
            return true;
        }

        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;
            await viewport.World.InitWorld();
            await LoadPreviewScene(viewport);
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

        /// <summary>
        /// 把 Sequence.PreviewSceneName 指的场景换成预览世界的根。要在 CreateStudioEnvironment
        /// 之前调: studio 环境与坐标轴都是挂到 World.Root 下的, 之后再换根会把它们丢掉。
        ///
        /// 用换根而不是把场景挂成子节点: 绑定的 RelativePath 是相对场景根算的, 挂成子节点
        /// 会让真实路径多出一层场景名, 存好的绑定就全对不上了。
        /// </summary>
        async Thread.Async.TtTask LoadPreviewScene(Graphics.Pipeline.TtViewportSlate viewport)
        {
            if (Sequence?.PreviewSceneName == null)
                return;

            var scene = await Sequence.PreviewSceneName.GetAsset<GamePlay.Scene.TtScene>(viewport.World);
            if (scene == null)
            {
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: 预览场景 {Sequence.PreviewSceneName} 加载失败, 绑定将无法解析");
                return;
            }
            scene.NodeName = Sequence.PreviewSceneName.PureName;
            // Axis 原本挂在 InitWorld 建的那个根下, 不重挂就会随旧根一起从世界里消失
            var previewViewport = viewport as Editor.TtPreviewViewport;
            if (previewViewport?.Axis?.RootNode != null)
                previewViewport.Axis.RootNode.Parent = scene;
            viewport.World.Root = scene;
        }
        #endregion IAssetEditor

        #region 时间换算
        float TickToSeconds(long tick)
        {
            return (float)Sequence.TickResolution.AsSeconds(tick);
        }
        long SecondsToTick(float seconds)
        {
            return Sequence.TickResolution.FromSeconds(seconds);
        }
        /// <summary>
        /// 吸附到 DisplayRate 的整帧。凡是会变成关键帧时刻的 tick 都要过这里, 否则关键帧
        /// 会落在帧间。反过来, 只用于播放头的 tick 不要过这里 —— 那会把 scrub 变成跳档。
        /// </summary>
        long SnapTick(long tick)
        {
            return Sequence.TickResolution.SnapTo(tick, Sequence.DisplayRate);
        }
        #endregion 时间换算

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
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Bindings", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Timeline", mDockKeyClass), bottomId);
            ImGuiAPI.DockBuilderFinish(id);
        }

        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(1000, 700);
        public void OnDraw()
        {
            if (Visible == false || Sequence == null)
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
            DrawBindings();
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
                Sequence.SaveAssetTo(Sequence.AssetName);
                mEditorHistory?.SetSavePoint();
            }
            ImGuiAPI.SameLine(0, -1);
            bool playing = mPlayer.State == ESequencePlaybackState.Playing;
            if (EGui.UIProxy.CustomButton.ToolButton(playing ? "Pause" : "Play", in btSize))
            {
                if (playing)
                    mPlayer.Pause();
                else
                    mPlayer.Play();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Stop", in btSize))
            {
                // Stop 会走快照层把属性恢复成进入序列前的值, 播放头回到起点
                mPlayer.Stop();
                mCurrentTick = Sequence.PlaybackStartTick;
                mTimeline.PlayPosition = TickToSeconds(mCurrentTick);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Key", in btSize))
            {
                KeySelectedOrAll();
            }
            ImGuiAPI.SameLine(0, -1);
            bool loop = mPlayer.LoopMode == ESequenceLoopMode.Loop;
            EGui.UIProxy.CheckBox.DrawCheckBox("Loop", ref loop, false);
            mPlayer.LoopMode = loop ? ESequenceLoopMode.Loop : ESequenceLoopMode.Once;
            ImGuiAPI.SameLine(0, -1);
            bool frameLocked = mPlayer.EvaluationType == ESequenceEvaluationType.FrameLocked;
            EGui.UIProxy.CheckBox.DrawCheckBox("FrameLocked", ref frameLocked, false);
            mPlayer.EvaluationType = frameLocked ? ESequenceEvaluationType.FrameLocked : ESequenceEvaluationType.WithSubFrames;
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
                if (ImGuiAPI.CollapsingHeader("Sequence", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    SequencePropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("Selected", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    if (mSelectedObject == null)
                        ImGuiAPI.Text("Select a binding / section in timeline");
                    else
                        DetailPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        bool mBindingsShow = true;
        protected unsafe void DrawBindings()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Bindings", ref mBindingsShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                ImGuiAPI.Text("Drag assets into the preview to add nodes");
                if (ImGuiAPI.CollapsingHeader("Scene Nodes", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    var root = PreviewViewport.World?.Root;
                    if (root != null)
                    {
                        // 预览场景重载过之后节点换了一批, 旧的选中节点已经不在世界里了 ——
                        // 不清的话属性面板会让你去改一个脱离了场景的节点, 改了也没人看得见。
                        // 用 RootNode 而不是 root.Children.Contains: 视口里点选得到的可能是任意
                        // 深度的子节点, 只查直接子节点会把它们当成已移除的节点当场清掉。
                        // 节点从场景里抽走时 Parent 会置 null, 那时它的 RootNode 就是自己。
                        if (mSelectedSceneNode != null && ReferenceEquals(mSelectedSceneNode.RootNode, root) == false)
                            SelectSceneNode(null);
                        for (int i = 0; i < root.Children.Count; ++i)
                        {
                            var node = root.Children[i];
                            bool selected = ReferenceEquals(node, mSelectedSceneNode);
                            // Selectable 的命中框横跨整行 (ItemSize 只按文字宽推进光标, 但 bb 一直延到
                            // WorkRect 右边), 会盖住后面 SameLine 的按钮。它先提交, 鼠标按下时抢到
                            // ActiveId, 后面那个按钮就永远不会被判定为 hovered -> 点了没反应。
                            ImGuiAPI.SetNextItemAllowOverlap();
                            if (ImGuiAPI.Selectable($"{node.NodeName}##scenenode{i}", selected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                            {
                                SelectSceneNode(node);
                            }
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.SmallButton($"+##addbinding{i}"))
                            {
                                AddBindingForNode(node);
                            }
                        }
                    }
                }
                if (ImGuiAPI.CollapsingHeader("Bindings", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    for (int i = 0; i < Sequence.Bindings.Count; ++i)
                    {
                        var binding = Sequence.Bindings[i];
                        if (binding == null)
                            continue;
                        // 解析不到时明确标出来, 不静默丢弃: 序列可能引用了这份预览世界里没有的节点
                        bool resolved = mPlayer.Resolver.Resolve(binding) != null;
                        var label = resolved ? binding.DisplayName : $"{binding.DisplayName} <Unresolved>";
                        bool selected = binding.BindingId == mSelectedBindingId;
                        ImGuiAPI.SetNextItemAllowOverlap();
                        if (ImGuiAPI.Selectable($"{label}##binding{i}", selected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            SelectObject(binding);
                        }
                        ImGuiAPI.SameLine(0, -1);
                        if (ImGuiAPI.SmallButton($"+##addtrack{i}"))
                        {
                            RefreshPropertyCandidates(binding);
                            ImGuiAPI.OpenPopup($"AddPropertyTrack{i}", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            ImGuiAPI.SetTooltip("Add property track");
                        ImGuiAPI.SameLine(0, -1);
                        if (ImGuiAPI.SmallButton($"x##delbinding{i}"))
                        {
                            RemoveBinding(binding);
                            break;
                        }
                        // OpenPopup 与 BeginPopup 必须在同一 ID 作用域, 所以弹窗就在这里展开
                        if (ImGuiAPI.BeginPopup($"AddPropertyTrack{i}", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                        {
                            DrawPropertyCandidates(binding);
                            ImGuiAPI.EndPopup();
                        }
                        // 轨道列表: 属性轨除了时间轴上那一行, 没有其他地方能看见自己绑的是哪条属性
                        bool trackRemoved = false;
                        for (int ti = 0; ti < binding.Tracks.Count; ++ti)
                        {
                            var track = binding.Tracks[ti];
                            if (track == null)
                                continue;
                            var trackLabel = track.Muted ? $"    {track.DisplayName} (muted)" : $"    {track.DisplayName}";
                            ImGuiAPI.SetNextItemAllowOverlap();
                            if (ImGuiAPI.Selectable($"{trackLabel}##track{i}_{ti}", ReferenceEquals(track, mSelectedObject),
                                ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                            {
                                SelectObject(track);
                            }
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.SmallButton($"x##deltrack{i}_{ti}"))
                            {
                                RemoveTrack(binding, track);
                                trackRemoved = true;
                                break;
                            }
                        }
                        if (trackRemoved)
                            break;
                    }
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

                if (ImGuiAPI.BeginDragDropTarget())
                {
                    var payload = ImGuiAPI.AcceptDragDropPayload("ContentBrowserAssetDragDrop", ImGuiDragDropFlags_.ImGuiDragDropFlags_None);
                    if (payload != null)
                    {
                        var handle = GCHandle.FromIntPtr((IntPtr)(payload->Data));
                        var dragData = (EGui.Controls.TtContentBrowser.DragDropData)handle.Target;
                        for (int i = 0; i < dragData.Metas.Length; i++)
                        {
                            dragData.Metas[i].OnDragTo(PreviewViewport).AddWaitTask();
                        }
                        // 新节点进场景后旧的解析缓存里可能还留着 null, 不失效的话新节点绑不上
                        mPlayer.Resolver.Invalidate();
                    }
                    ImGuiAPI.EndDragDropTarget();
                }
            }
            PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        bool mTimelineShow = true;
        protected unsafe void DrawTimeline()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Timeline", ref mTimelineShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var frameTicks = Sequence.TickResolution.TicksPerFrame(Sequence.DisplayRate);
                var frameIndex = frameTicks > 0 ? mCurrentTick / frameTicks : 0;
                ImGuiAPI.Text($"Tick: {mCurrentTick}   Frame: {frameIndex} @ {Sequence.DisplayRate}   Range: [{Sequence.PlaybackStartTick}, {Sequence.PlaybackEndTick}]");
                BuildTimelineData();
                var size = new Vector2(-1, 0);
                mTimeline.OnDraw(in size);
                ProcessTimelineInteraction();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        #endregion 绘制

        #region 时间轴数据与交互

        /// <summary>
        /// 时间轴上一行属于哪条轨道。
        ///
        /// 不能再用"行号 / 2 = 绑定下标"算: 一个绑定现在可以挂 Transform 轨 + 任意多条
        /// 属性轨, 占多少行取决于它自己。每帧重建时顺手把行→(绑定, 轨道) 的对应存下来,
        /// 右键菜单靠它判断用户点的是哪条轨。Track 为 null 表示这是一个还没有任何轨道
        /// 的绑定占位行。
        /// </summary>
        class FRowInfo
        {
            public Sequencer.Asset.TtSequenceBinding Binding;
            public Sequencer.Asset.TtSequenceTrack Track;
        }
        List<FRowInfo> mRows = new List<FRowInfo>();

        /// <summary>关键帧点在时间轴上的句柄。一个点代表该 Section 在这个 tick 上所有通道的关键帧。</summary>
        class FKeyHandle
        {
            public Sequencer.Asset.TtSequenceSection Section;
            public long Tick;
        }
        List<FKeyHandle> mKeyHandles = new List<FKeyHandle>();

        /// <summary>一条轨道占两行: 上面画 Section 条, 下面画关键帧点。返回 Section 条那一行的行号。</summary>
        int AddRowPair(Sequencer.Asset.TtSequenceBinding binding, Sequencer.Asset.TtSequenceTrack track, string header)
        {
            int sectionRow = mRows.Count;
            mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = header });
            mRows.Add(new FRowInfo() { Binding = binding, Track = track });
            mTimeline.Tracks.Add(new EGui.Controls.TtTimelineTrack() { Label = "      Keys" });
            mRows.Add(new FRowInfo() { Binding = binding, Track = track });
            return sectionRow;
        }

        void BuildTimelineData()
        {
            mTimeline.Duration = Math.Max(TickToSeconds(Sequence.PlaybackEndTick), 0.001f);
            mTimeline.SnapInterval = (float)Sequence.TickResolution.AsSeconds(Sequence.TickResolution.TicksPerFrame(Sequence.DisplayRate));
            mTimeline.Tracks.Clear();
            mTimeline.Items.Clear();
            mKeyHandles.Clear();
            mRows.Clear();

            for (int bi = 0; bi < Sequence.Bindings.Count; ++bi)
            {
                var binding = Sequence.Bindings[bi];
                if (binding == null)
                    continue;
                bool resolved = mPlayer.Resolver.Resolve(binding) != null;
                var bindingLabel = resolved ? binding.DisplayName : $"{binding.DisplayName}?";

                int emitted = 0;
                for (int ti = 0; ti < binding.Tracks.Count; ++ti)
                {
                    var track = binding.Tracks[ti];
                    if (track == null)
                        continue;
                    // 绑定名只抬在第一条轨上, 其余轨缩进 —— 时间轴控件没有分组行的概念,
                    // 每行都写一遍绑定名会把轨道名挤到看不见。
                    var header = emitted == 0 ? $"{bindingLabel} / {track.DisplayName}" : $"    {track.DisplayName}";
                    int sectionRow = AddRowPair(binding, track, header);
                    int keyRow = sectionRow + 1;
                    ++emitted;

                    for (int si = 0; si < track.Sections.Count; ++si)
                    {
                        var section = track.Sections[si];
                        if (section == null)
                            continue;
                        // 先收关键帧: Section 条要拿最早/最晚那两个 tick 当缩放下限。范围被拖得
                        // 盖不住关键帧的话, 露在外面的那几个就既不驱动节点 (Section 在那段时间
                        // 不生效) 又照旧画在 Keys 行上。
                        GatherKeyTicks(section, mKeyTickBuffer);
                        float coverBegin = float.NaN;
                        float coverEnd = float.NaN;
                        if (mKeyTickBuffer.Count > 0)
                        {
                            coverBegin = TickToSeconds(mKeyTickBuffer[0]);
                            coverEnd = TickToSeconds(mKeyTickBuffer[mKeyTickBuffer.Count - 1]);
                        }
                        mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                        {
                            Id = $"section_{bi}_{ti}_{si}",
                            Label = track.Muted ? $"(muted) {track.DisplayName}" : track.DisplayName,
                            TrackIndex = sectionRow,
                            Begin = TickToSeconds(section.StartTick),
                            End = TickToSeconds(section.EndTick),
                            Type = EGui.Controls.ETimelineItemType.Bar,
                            Color = resolved ? 0xFFB07030u : 0xFF606060u,
                            AllowResize = true,
                            MustCoverBegin = coverBegin,
                            MustCoverEnd = coverEnd,
                            UserData = section,
                        });

                        for (int ki = 0; ki < mKeyTickBuffer.Count; ++ki)
                        {
                            var handle = new FKeyHandle() { Section = section, Tick = mKeyTickBuffer[ki] };
                            mKeyHandles.Add(handle);
                            var seconds = TickToSeconds(handle.Tick);
                            mTimeline.Items.Add(new EGui.Controls.TtTimelineItem()
                            {
                                Id = $"key_{bi}_{ti}_{si}_{handle.Tick}",
                                Label = "",
                                TrackIndex = keyRow,
                                Begin = seconds,
                                End = seconds,
                                Type = EGui.Controls.ETimelineItemType.Marker,
                                Color = 0xFF20A0FF,
                                UserData = handle,
                            });
                        }
                    }
                }
                // 一条轨道都没有的绑定也要占两行: 否则它在时间轴上完全看不见, 右键打点
                // 的入口也就没了 —— 而刚拖进来的绑定就是这个状态。
                if (emitted == 0)
                    AddRowPair(binding, null, $"{bindingLabel} / <no track>");
            }

            mTimeline.PlayPosition = TickToSeconds(mCurrentTick);
        }

        void ProcessTimelineInteraction()
        {
            // 拖播放头 = scrub。求值无状态, 所以这里和顺序播放走的是同一个 SetPosition。
            // 这里故意不 SnapTick: 播放头吸附到整帧的话, 拖动就是一档一档跳而不是跟着鼠标走。
            if (mTimeline.PlayPositionChanged)
            {
                mPlayer.Pause();
                SetPlayHead(SecondsToTick(mTimeline.PlayPosition));
            }

            var clicked = mTimeline.ClickedItem;
            if (clicked != null)
                SelectObject(clicked.UserData);

            var changed = mTimeline.ChangedItem;
            if (changed != null)
            {
                if (mDragItemData != changed.UserData)
                {
                    // 拖拽刚开始: 记下这一段的完整快照, 松手时作为一条历史的 undo 目标
                    mDragItemData = changed.UserData;
                    mDragSection = SectionOfItemData(changed.UserData);
                    mDragBefore = mDragSection != null ? FSectionSnapshot.Capture(mDragSection) : null;
                }
                ApplyItemDrag(changed);
            }
            // 结束信号必须用控件的 ItemDragFinished。曾经这里用的是 "ChangedItem == null",
            // 而那个字段的含义是“本帧值变了” —— 鼠标按住不动的帧它就是 null, 于是一次拖拽
            // 被切成几十条 "Edit Sequence Item", 要按同数量的 Ctrl+Z 才能退回去。
            if (mTimeline.ItemDragFinished && mDragItemData != null)
            {
                PushSectionCommand("Edit Sequence Item", mDragSection, mDragBefore);
                mDragItemData = null;
                mDragSection = null;
                mDragBefore = null;
            }

            // OpenPopup 与 BeginPopup 必须在同一 PushID 作用域, 都放在控件外面
            if (mTimeline.RightClickedItem != null)
            {
                mPopupForItem = true;
                SelectObject(mTimeline.RightClickedItem.UserData);
                mPopupTrackIndex = mTimeline.RightClickedItem.TrackIndex;
                ImGuiAPI.OpenPopup("SequenceTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            else if (mTimeline.RightClickedTrack >= 0)
            {
                mPopupForItem = false;
                mPopupTrackIndex = mTimeline.RightClickedTrack;
                mPopupTime = mTimeline.RightClickedTime;
                ImGuiAPI.OpenPopup("SequenceTimelineMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            DrawTimelineMenu();
            // 必须在 DrawTimelineMenu 之后、且不在任何 PushID 里: 弹窗是从右键菜单里请求
            // 打开的, 而 OpenPopup 得在菜单那层 popup 之外调 (见 DrawSetValueAndKeyDialog)。
            DrawSetValueAndKeyDialog();
        }

        void DrawTimelineMenu()
        {
            if (!ImGuiAPI.BeginPopup("SequenceTimelineMenu", ImGuiWindowFlags_.ImGuiWindowFlags_None))
                return;

            var binding = BindingOfRow(mPopupTrackIndex);
            var track = TrackOfRow(mPopupTrackIndex);
            if (mPopupForItem)
            {
                if (mSelectedObject is FKeyHandle keyHandle)
                {
                    // 改已有关键帧的值走的就是打点那条路: 通道的 AddKey 在同一 tick 上是
                    // 覆盖而不是插入 (见 TtNameChannel.AddKey), 所以“改值”不需要另一套 API。
                    // 用 handle.Tick 而不是右键位置换算出来的时刻: 后者吸附到整帧之后未必
                    // 正好落在关键帧那一 tick 上, 差一个 tick 就变成在旁边新插一个点了。
                    if (track is Sequencer.Asset.TtPropertyTrack keyDlgTrack &&
                        ImGuiAPI.MenuItem($"Set Key Value ({TickToSeconds(keyHandle.Tick):0.00}s)...", null, false, true))
                    {
                        OpenSetValueAndKeyDialog(binding, keyDlgTrack, keyHandle.Tick, true);
                    }
                    if (ImGuiAPI.MenuItem("Remove Key", null, false, true))
                        RemoveKeyAt(keyHandle);
                }
                else if (mSelectedObject is Sequencer.Asset.TtSequenceSection section)
                {
                    if (ImGuiAPI.MenuItem("Remove Section", null, false, true))
                        RemoveSection(binding, section);
                }
            }
            else if (binding == null)
            {
                ImGuiAPI.Text("Add a binding from the Bindings panel");
            }
            else
            {
                // 菜单项按右键那一行所属的轨道给: 一个绑定现在可能挂着 Transform 轨和好几条
                // 属性轨, 一律给 "Key Transform" 会打到用户没在看的那条轨上。
                var label = track != null ? track.DisplayName : "Transform";
                if (ImGuiAPI.MenuItem($"Key {label} At PlayHead", null, false, true))
                    KeyRow(binding, track);
                if (ImGuiAPI.MenuItem($"Key {label} Here", null, false, true))
                {
                    SetPlayHead(SnapTick(SecondsToTick(mPopupTime)));
                    KeyRow(binding, track);
                }
                if (track != null && ImGuiAPI.MenuItem($"Remove Track {track.DisplayName}", null, false, true))
                    RemoveTrack(binding, track);
                // 只对属性轨给: Transform 轨是一次写七条通道的复合轨, 没有“那一条属性”可设。
                // 弹窗而不是常驻面板: 打点读的是节点当前值, 而引用类型属性 (RName 之类)
                // 没设过就是 null、一个关键帧都打不出。在打点的上下文里只摆这一条属性,
                // 比让人去一个几十条属性的面板里翻找直接得多。
                if (track is Sequencer.Asset.TtPropertyTrack dlgTrack &&
                    ImGuiAPI.MenuItem($"Set Value & Key {dlgTrack.DisplayName}...", null, false, true))
                {
                    OpenSetValueAndKeyDialog(binding, dlgTrack, SnapTick(SecondsToTick(mPopupTime)));
                }
                // 多 Section 本身不是错, 但“每个 Section 只装一个关键帧”就是 —— 那种形状在时间轴上
                // 和正常的多关键帧长得一模一样, 只有拖播放头时才发现值是阶梯的。菜单项只在真的
                // 有多个 Section 时出现, 不给一个永远点不动的灰项。
                if (track != null && track.Sections.Count > 1 &&
                    ImGuiAPI.MenuItem($"Merge {track.Sections.Count} Sections", null, false, true))
                {
                    MergeSections(track);
                }
                if (ImGuiAPI.MenuItem("Remove Binding", null, false, true))
                    RemoveBinding(binding);
            }
            ImGuiAPI.EndPopup();
        }

        // “Set Value & Key” 弹窗的上下文。右键菜单一关, mPopupTrackIndex / mPopupTime 就可能
        // 被下一次右键改掉, 所以开窗时把这几样快照下来。
        Sequencer.Asset.TtSequenceBinding mKeyDlgBinding = null;
        Sequencer.Asset.TtPropertyTrack mKeyDlgTrack = null;
        long mKeyDlgTick = 0;
        bool mKeyDlgOpen = false;
        bool mKeyDlgRequestOpen = false;
        /// <summary>
        /// true = 从关键帧右键进来改它的值, false = 在一个空位置新打点。
        /// 两者底层是同一条路 (同 tick 打点即覆盖), 区分只为了按钮和标题能说准话。
        /// </summary>
        bool mKeyDlgEditExisting = false;
        const string mKeyDlgId = "Set Value & Key##SequenceSetValueAndKey";

        void OpenSetValueAndKeyDialog(Sequencer.Asset.TtSequenceBinding binding,
            Sequencer.Asset.TtPropertyTrack track, long tick, bool editExisting = false)
        {
            mKeyDlgBinding = binding;
            mKeyDlgTrack = track;
            mKeyDlgTick = tick;
            mKeyDlgEditExisting = editExisting;
            // 改已有关键帧时先把播放头挪到它身上: 求值会把该 tick 的值写回节点, 于是属性
            // 面板里摆的就是这个关键帧当前的值。不这么做的话面板上显示的是节点此刻恰好是
            // 什么 (可能是另一个关键帧求值的结果), 一按确认就把这个关键帧改错了。
            if (editExisting)
                SetPlayHead(tick);
            // 只在开窗这一下子设 Target / FilterString, 不每帧设 —— FilterString 背后就是
            // 搜索框的文本, 每帧覆盖会把用户自己改的搜索词抹掉。
            KeyValuePropGrid.Target = binding != null ? mPlayer.Resolver.Resolve(binding) : null;
            KeyValuePropGrid.FilterString = PropertyNameOfTrack(track);
            mKeyDlgRequestOpen = true;
        }
        /// <summary>
        /// 拿轨道对应的属性名, 用来把属性面板筛到只剩这一条。
        /// 不用 track.DisplayName: 它是 [Rtti.Meta] 可改的字段, 用户改过之后就筛不到了。
        /// 反射访问器的 PropertyId 是 "Reflect:属性名"; 显式访问器 (如 Node.Position) 没这个
        /// 前缀, 那时只能退回 DisplayName。
        /// </summary>
        static string PropertyNameOfTrack(Sequencer.Asset.TtPropertyTrack track)
        {
            if (track == null)
                return "";
            var id = track.PropertyId;
            var prefix = TtReflectedPropertyAccessor.IdPrefix;
            if (string.IsNullOrEmpty(id) == false && id.StartsWith(prefix))
                return id.Substring(prefix.Length);
            return track.DisplayName != null ? track.DisplayName : "";
        }
        /// <summary>
        /// 设值并打点。PropertyGrid 直接摆节点本体, 靠 FilterString 收窄到目标属性;
        /// 不做成“只包一条属性的代理对象”是因为那样属性上的 attribute (RName 的资产
        /// 筛选器等) 全会丢, 编辑控件就不对了。
        /// </summary>
        unsafe void DrawSetValueAndKeyDialog()
        {
            // OpenPopup 必须在右键菜单那个 BeginPopup/EndPopup 之外调: 从一个 popup 里开
            // 另一个 popup, 模态屏障装不到根层, 会出现点击穿透到背景、且被当成非模态
            // popup 点外即关 (见 TtKawaiiCurveEditorAttribute 里记的同一个坑)。
            if (mKeyDlgRequestOpen)
            {
                ImGuiAPI.OpenPopup(mKeyDlgId, ImGuiPopupFlags_.ImGuiPopupFlags_None);
                mKeyDlgOpen = true;
                mKeyDlgRequestOpen = false;
            }
            if (mKeyDlgOpen == false)
                return;

            var dlgSize = new Vector2(560, 320);
            ImGuiAPI.SetNextWindowSize(in dlgSize, ImGuiCond_.ImGuiCond_Appearing);
            // BeginPopupModal 和 BeginPopup 一样: 只有返回 true 时才配 EndPopup。
            // ref mKeyDlgOpen: 标题栏的 X 会把它置 false。
            bool wasOpen = mKeyDlgOpen;
            if (ImGuiAPI.BeginPopupModal(mKeyDlgId, ref mKeyDlgOpen, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var node = mKeyDlgBinding != null ? mPlayer.Resolver.Resolve(mKeyDlgBinding) : null;
                if (node == null || mKeyDlgTrack == null)
                {
                    // 绑定在预览世界里解析不到节点, 就没有值可读也没有值可设
                    ImGuiAPI.TextDisabled("This binding does not resolve to a node in the preview world");
                }
                else
                {
                    if (mKeyDlgEditExisting)
                        ImGuiAPI.Text($"{node.NodeName}  .  {mKeyDlgTrack.DisplayName}   -  existing key at {TickToSeconds(mKeyDlgTick):0.00}s");
                    else
                        ImGuiAPI.Text($"{node.NodeName}  .  {mKeyDlgTrack.DisplayName}");
                    ImGuiAPI.Separator();
                    // 底部给按钮行留出一行高
                    var pgSize = new Vector2(0, -ImGuiAPI.GetFrameHeightWithSpacing() - 4);
                    if (ImGuiAPI.BeginChild("SetValueKeyPG", in pgSize,
                        ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                    {
                        KeyValuePropGrid.OnDraw(true, false, false);
                    }
                    ImGuiAPI.EndChild();

                    var btnSize = new Vector2(170, 0);
                    if (mKeyDlgEditExisting)
                    {
                        // 改已有关键帧时只给一个按钮: 开窗时已经把播放头挪到这个关键帧上了,
                        // “Here” 和 “At PlayHead” 是同一个 tick, 摆两个一模一样的按钮只会让人犯疑。
                        if (ImGuiAPI.Button($"Apply To Key ({TickToSeconds(mKeyDlgTick):0.00}s)", in btnSize))
                        {
                            ApplyValueToExistingKey();
                            CloseSetValueAndKeyDialog();
                        }
                        ImGuiAPI.SameLine(0, -1);
                    }
                    else
                    {
                        if (ImGuiAPI.Button($"Key Here ({TickToSeconds(mKeyDlgTick):0.00}s)", in btnSize))
                        {
                            // 跟菜单里的 "Key XXX Here" 一个语义: 先把播放头移到右键位置再打
                            SetPlayHead(mKeyDlgTick);
                            KeyProperty(mKeyDlgBinding, mKeyDlgTrack);
                            CloseSetValueAndKeyDialog();
                        }
                        ImGuiAPI.SameLine(0, -1);
                        if (ImGuiAPI.Button("Key At PlayHead", in btnSize))
                        {
                            KeyProperty(mKeyDlgBinding, mKeyDlgTrack);
                            CloseSetValueAndKeyDialog();
                        }
                        ImGuiAPI.SameLine(0, -1);
                    }
                    // 不叫 Cancel: 属性值是当场写进节点的, 关窗收不回来
                    if (ImGuiAPI.Button("Close", in btnSize))
                        CloseSetValueAndKeyDialog();
                }
                ImGuiAPI.EndPopup();
            }
            // 点标题栏的 X: ImGuiAPI 直接把 mKeyDlgOpen 改成了 false, 上面那些 Close 按钮
            // 一个都没走到, 得在这里补上收尾
            if (wasOpen && mKeyDlgOpen == false)
                ClearKeyDlgContext();
        }
        /// <summary>
        /// 把面板上改好的值写回原来那个关键帧。靠的是通道 AddKey 在同一 tick 上覆盖而不是
        /// 插入, 所以前提是打点的 tick 与关键帧的 tick 严格相等。
        ///
        /// KeyProperty 打点用的是 SnapTick(mCurrentTick), 中间隔着 SetPlayHead 里的 ClampTick
        /// 和一次 SnapTick —— 万一关键帧落在播放范围外或非整帧上, 实际写入的 tick 就会
        /// 偏掉, 那就不是“改值”而是“在旁边新插一个点”了。宁可不写并说明原因。
        /// </summary>
        void ApplyValueToExistingKey()
        {
            SetPlayHead(mKeyDlgTick);
            var actual = SnapTick(mCurrentTick);
            if (actual != mKeyDlgTick)
            {
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: 关键帧在 {mKeyDlgTick} tick, 但播放头只能落到 {actual} tick (超出播放范围或不在整帧上), 没有改这个关键帧的值");
                return;
            }
            KeyProperty(mKeyDlgBinding, mKeyDlgTrack);
        }
        void CloseSetValueAndKeyDialog()
        {
            // 只有在弹窗内部调用时才成立: CloseCurrentPopup 关的是当前这一层 popup
            ImGuiAPI.CloseCurrentPopup();
            ClearKeyDlgContext();
        }
        /// <summary>
        /// 松掉弹窗抱着的引用。单独一个方法是因为标题栏的 X 是 ImGui 自己把 mKeyDlgOpen
        /// 置 false 的, 走不到 CloseSetValueAndKeyDialog —— 不清的话 PG 会一直攥着预览世界
        /// 的节点, 预览世界重载之后就成了悬空引用。
        /// </summary>
        void ClearKeyDlgContext()
        {
            mKeyDlgOpen = false;
            mKeyDlgBinding = null;
            mKeyDlgTrack = null;
            mKeyDlgEditExisting = false;
            KeyValuePropGrid.Target = null;
        }
        Sequencer.Asset.TtSequenceBinding BindingOfRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= mRows.Count)
                return null;
            return mRows[rowIndex].Binding;
        }
        Sequencer.Asset.TtSequenceTrack TrackOfRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= mRows.Count)
                return null;
            return mRows[rowIndex].Track;
        }
        static Sequencer.Asset.TtSequenceSection SectionOfItemData(object data)
        {
            if (data is Sequencer.Asset.TtSequenceSection section)
                return section;
            if (data is FKeyHandle handle)
                return handle.Section;
            return null;
        }

        /// <summary>把控件写回来的秒数落到数据上。Section 整体移动要连带平移关键帧。</summary>
        void ApplyItemDrag(EGui.Controls.TtTimelineItem item)
        {
            if (item.UserData is Sequencer.Asset.TtSequenceSection section)
            {
                var newStart = SnapTick(SecondsToTick(item.Begin));
                var newEnd = SnapTick(SecondsToTick(item.End));
                var deltaStart = newStart - section.StartTick;
                var deltaEnd = newEnd - section.EndTick;
                section.StartTick = newStart;
                section.EndTick = newEnd;
                // 两端位移相同即整体拖动; 关键帧存的是绝对 tick, 不跟着平移就会留在原地
                if (deltaStart == deltaEnd && deltaStart != 0)
                    section.ShiftKeys(deltaStart);
                OnSequenceDataChanged();
            }
            else if (item.UserData is FKeyHandle handle)
            {
                var newTick = SnapTick(SecondsToTick(item.Begin));
                if (newTick == handle.Tick)
                    return;
                MoveKeysAt(handle.Section, handle.Tick, newTick);
                handle.Tick = newTick;
                OnSequenceDataChanged();
            }
        }
        #endregion 时间轴数据与交互

        #region 绑定与打点

        /// <summary>
        /// 记下预览世界里当前选中的节点。只用来给 Scene Nodes 列表里那一行上高亮,
        /// 让你看得出视口里选的是哪个; 改属性值走的是右键菜单里的 Set Value &amp; Key 弹窗。
        /// 和 SelectObject 分开两个方法: 一个选的是预览世界的节点, 一个选的是序列资产里的数据。
        /// </summary>
        void SelectSceneNode(GamePlay.Scene.TtNode node)
        {
            mSelectedSceneNode = node;
        }

        void SelectObject(object target)
        {
            mSelectedObject = target;
            if (target is Sequencer.Asset.TtSequenceBinding binding)
            {
                mSelectedBindingId = binding.BindingId;
                DetailPropGrid.Target = binding;
            }
            else if (target is FKeyHandle)
            {
                // 关键帧点本身没有可编辑属性面板, 展示它所属的 Section
                DetailPropGrid.Target = SectionOfItemData(target);
            }
            else
            {
                DetailPropGrid.Target = target;
            }
        }

        /// <summary>
        /// 把预览世界里的一个节点加进序列。绑定同时存两套定位信息: 目标节点 Guid
        /// (解析时优先, 改名与挪父节点都不断链) 与 (参照节点 Guid + 相对路径) 回退路
        /// (轻量节点没有 Guid, 只能靠它；同时也让同一条序列能复用到别的场景实例上)。
        /// </summary>
        void AddBindingForNode(GamePlay.Scene.TtNode node)
        {
            if (node == null)
                return;

            Guid parentNodeId;
            string relativePath;
            if (TtSequenceBindingResolver.MakeBindingPath(node, null, out parentNodeId, out relativePath) == false)
                return;

            // 这里不用 Sequence.GetOrCreateBinding: 它会直接把绑定加进资产, 绕过历史,
            // 撤销时就没有对应的 Do 可以重放了
            var existing = FindExistingBinding(node.NodeId, in parentNodeId, relativePath);
            if (existing != null)
            {
                SelectObject(existing);
                return;
            }

            var binding = new Sequencer.Asset.TtSequenceBinding()
            {
                TargetNodeId = node.NodeId,
                ParentNodeId = parentNodeId,
                RelativePath = relativePath,
                DisplayName = node.NodeName,
            };
            ExecuteStructureCommand($"Add Binding {node.NodeName}",
                () => Sequence.Bindings.Add(binding),
                () => Sequence.Bindings.Remove(binding));
            SelectObject(binding);
        }
        /// <summary>
        /// 同一个节点只该有一条绑定, 否则两条绑定上的轨道会写同一个目标。去重先比
        /// Guid 再比路径: 节点改过名后路径已经对不上了, 光比路径会把它当成新节点。
        /// </summary>
        Sequencer.Asset.TtSequenceBinding FindExistingBinding(in Guid targetNodeId, in Guid parentNodeId, string relativePath)
        {
            var byNode = Sequence.FindBindingByNode(in targetNodeId);
            if (byNode != null)
                return byNode;
            var path = relativePath != null ? relativePath : "";
            for (int i = 0; i < Sequence.Bindings.Count; ++i)
            {
                var binding = Sequence.Bindings[i];
                if (binding != null && binding.ParentNodeId == parentNodeId && binding.RelativePath == path)
                    return binding;
            }
            return null;
        }
        void RemoveBinding(Sequencer.Asset.TtSequenceBinding binding)
        {
            if (binding == null)
                return;
            int index = Sequence.Bindings.IndexOf(binding);
            if (index < 0)
                return;
            ExecuteStructureCommand($"Remove Binding {binding.DisplayName}",
                () => Sequence.Bindings.Remove(binding),
                () => Sequence.Bindings.Insert(Math.Min(index, Sequence.Bindings.Count), binding));
            if (mSelectedBindingId == binding.BindingId)
            {
                mSelectedBindingId = Guid.Empty;
                mSelectedObject = null;
                DetailPropGrid.Target = null;
            }
        }
        void RemoveSection(Sequencer.Asset.TtSequenceBinding binding, Sequencer.Asset.TtSequenceSection section)
        {
            if (binding == null || section == null)
                return;
            // 不能按 FindTrack("Transform") 拿轨道: 属性轨的 Section 也走这个入口, 而属性轨
            // 的 TrackTypeName 全是 "Property" 且一个绑定上可能有多条。按持有关系反查最直接。
            Sequencer.Asset.TtSequenceTrack owner = null;
            for (int i = 0; i < binding.Tracks.Count; ++i)
            {
                if (binding.Tracks[i] != null && binding.Tracks[i].Sections.Contains(section))
                {
                    owner = binding.Tracks[i];
                    break;
                }
            }
            if (owner == null)
                return;
            int index = owner.Sections.IndexOf(section);
            ExecuteStructureCommand("Remove Section",
                () => owner.RemoveSection(section),
                () => owner.Sections.Insert(Math.Min(index, owner.Sections.Count), section));
            mSelectedObject = null;
            DetailPropGrid.Target = null;
        }
        /// <summary>
        /// 把一条轨道的多个 Section 并成一个。修的是“每个 Section 只装一个关键帧”这种形状:
        /// 单关键帧的通道求值只能是常量, 拖播放头时值就在 Section 边界上阶梯式突变。
        ///
        /// Undo 要同时回滚两件事: 宿主 Section 的关键帧与范围, 以及被移除的那几个 Section
        /// 本身 —— 所以不能用 PushSectionCommand (它只管得住一个 Section 的内容)。
        /// </summary>
        void MergeSections(Sequencer.Asset.TtSequenceTrack track)
        {
            if (track == null || track.Sections.Count < 2)
                return;
            // 先把被移除的 Section 连原下标记下来: 合并之后就无处去问它们原来在哪了。
            var removed = new List<Sequencer.Asset.TtSequenceSection>();
            for (int i = 1; i < track.Sections.Count; ++i)
                removed.Add(track.Sections[i]);

            var host = track.Sections[0];
            var before = FSectionSnapshot.Capture(host);

            int skipped;
            track.MergeSectionsIntoFirst(Sequence.TickResolution, out skipped);
            var after = FSectionSnapshot.Capture(host);
            if (skipped > 0)
            {
                // 搬不动的通道意味着它的关键帧跟着被删的 Section 一起没了。宁可吵一句也不要
                // 默默丢数据 —— 用户还能 Ctrl+Z 回去。
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: Merge Sections 有 {skipped} 条通道搬不动, 它们的关键帧已丢弃");
            }

            var cmd = new Infrastructure.TtDelegateCommand("Merge Sections",
                () =>
                {
                    for (int i = removed.Count - 1; i >= 0; --i)
                        track.RemoveSection(removed[i]);
                    after.ApplyTo(host);
                    OnSequenceDataChanged();
                },
                () =>
                {
                    before.ApplyTo(host);
                    // 按原先的前后顺序接回去。它们原本就是从下标 1 开始连续排列的。
                    for (int i = 0; i < removed.Count; ++i)
                        track.Sections.Add(removed[i]);
                    OnSequenceDataChanged();
                });
            cmd.Seal();
            OnSequenceDataChanged();
            // 改动已经落在数据上了, 用 PushCommand 而不是 ExecuteCommand
            mEditorHistory?.PushCommand(cmd);
            mSelectedObject = null;
            DetailPropGrid.Target = null;
            // 重求一次, 否则节点还停在合并前那一帧的值上
            SetPlayHead(mCurrentTick);
        }
        /// <summary>
        /// 现取一次目标节点上能被序列驱动的属性。判据完全交给 CanAnimate —— 列出一条
        /// 点了没反应的属性比不列出来更糟。
        /// </summary>
        void RefreshPropertyCandidates(Sequencer.Asset.TtSequenceBinding binding)
        {
            mPropertyCandidates.Clear();
            var node = mPlayer.Resolver.Resolve(binding);
            if (node == null)
                return;
            TtReflectedPropertyAccessor.GatherAnimatableProperties(node.GetType(), mPropertyCandidates);
            // 反射给的是声明顺序 (还混着继承层次), 界面上按名字排才找得到
            mPropertyCandidates.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }
        void DrawPropertyCandidates(Sequencer.Asset.TtSequenceBinding binding)
        {
            // Transform 先列, 且不受下面那个“没有可驱动属性”的早退影响: 它不是反射枚举出来的
            // 属性, 而是一条一次写 Position/Scale/Rotation 的复合轨 (七条通道), 所以既不在
            // mPropertyCandidates 里, 也不依赖绑定能不能解析到节点。
            //
            // 以前它只能靠按 Key 时 GetOrCreateTransformTrack 隐式建出来 —— 删了之后想重建,
            // 在这个菜单里找不到任何入口。它是最常用的轨道, 不能只有隐式入口。
            bool transformExist = binding.FindTrack("Transform") != null;
            if (ImGuiAPI.MenuItem("Transform  :  Position / Scale / Rotation", null, transformExist, transformExist == false))
                AddTransformTrack(binding);
            ImGuiAPI.Separator();

            if (mPropertyCandidates.Count == 0)
            {
                // 绑定解析不到节点时也走这里 —— 不知道目标类型就列不出属性
                ImGuiAPI.Text("No animatable property (needs [Rtti.Meta] + public get/set + a value adapter)");
                return;
            }
            for (int i = 0; i < mPropertyCandidates.Count; ++i)
            {
                var prop = mPropertyCandidates[i];
                var propertyId = TtReflectedPropertyAccessor.IdPrefix + prop.Name;
                // 已经有轨道的属性画成打勾 + 禁用: 一条属性只允许一条轨 (理由见
                // TtSequenceBinding.GetOrCreatePropertyTrack), 但仍然要让用户看得见它已经在了
                bool exist = binding.FindPropertyTrack(propertyId) != null;
                if (ImGuiAPI.MenuItem($"{prop.Name}  :  {prop.PropertyType.Name}", null, exist, exist == false))
                    AddPropertyTrack(binding, prop);
            }
        }
        /// <summary>
        /// 把一条属性加成轨道。只有过了 CanAnimate 四道关的属性才会被列出来 (见
        /// TtReflectedPropertyAccessor), 所以这里拿不到适配器只可能是热重载后的赶巧,
        /// 直接不建 —— 建一条没适配器的轨道存盘后永远不会动。
        /// </summary>
        void AddPropertyTrack(Sequencer.Asset.TtSequenceBinding binding, System.Reflection.PropertyInfo prop)
        {
            if (binding == null || prop == null)
                return;
            var adapter = TtSequenceValueAdapters.FindByValueType(prop.PropertyType);
            if (adapter == null)
                return;
            var propertyId = TtReflectedPropertyAccessor.IdPrefix + prop.Name;
            if (binding.FindPropertyTrack(propertyId) != null)
                return;

            // 不用 binding.GetOrCreatePropertyTrack: 它会直接把轨道加进资产, 绕过历史 (与
            // AddBindingForNode 不用 GetOrCreateBinding 同理)
            var track = new Sequencer.Asset.TtPropertyTrack()
            {
                PropertyId = propertyId,
                AdapterId = adapter.AdapterId,
                DisplayName = prop.Name,
            };
            ExecuteStructureCommand($"Add Track {prop.Name}",
                () => binding.Tracks.Add(track),
                () => binding.Tracks.Remove(track));
            SelectObject(track);
        }
        /// <summary>
        /// 加一条 Transform 轨。不用 binding.GetOrCreateTransformTrack: 它会直接把轨道加进资产,
        /// 绕过历史 (与 AddPropertyTrack 同理)。
        /// </summary>
        void AddTransformTrack(Sequencer.Asset.TtSequenceBinding binding)
        {
            if (binding == null)
                return;
            // 一个绑定上只允许一条 Transform 轨 (同 GetOrCreateTransformTrack 的约定)
            if (binding.FindTrack("Transform") != null)
                return;

            var track = new Sequencer.Asset.TtTransformTrack();
            ExecuteStructureCommand("Add Track Transform",
                () => binding.Tracks.Add(track),
                () => binding.Tracks.Remove(track));
            SelectObject(track);
        }
        void RemoveTrack(Sequencer.Asset.TtSequenceBinding binding, Sequencer.Asset.TtSequenceTrack track)
        {
            if (binding == null || track == null)
                return;
            int index = binding.Tracks.IndexOf(track);
            if (index < 0)
                return;
            ExecuteStructureCommand($"Remove Track {track.DisplayName}",
                () => binding.Tracks.Remove(track),
                () => binding.Tracks.Insert(Math.Min(index, binding.Tracks.Count), track));
            if (ReferenceEquals(mSelectedObject, track))
            {
                mSelectedObject = null;
                DetailPropGrid.Target = null;
            }
        }

        void KeySelectedOrAll()
        {
            var binding = Sequence.FindBinding(mSelectedBindingId);
            if (binding != null)
            {
                KeyAllTracks(binding);
                return;
            }
            for (int i = 0; i < Sequence.Bindings.Count; ++i)
                KeyAllTracks(Sequence.Bindings[i]);
        }
        /// <summary>
        /// 给一个绑定上已有的每条轨道都打一组关键帧。
        ///
        /// 一条轨道都没有时才建 Transform 轨: 保留"刚拖进来的绑定按 Key 就出关键帧"
        /// 的手感; 但已经有属性轨时不能凭空再补一条 Transform 轨 —— 用户只想 K 那条属性。
        /// </summary>
        void KeyAllTracks(Sequencer.Asset.TtSequenceBinding binding)
        {
            if (binding == null)
                return;
            bool keyedAny = false;
            for (int i = 0; i < binding.Tracks.Count; ++i)
            {
                if (binding.Tracks[i] is Sequencer.Asset.TtTransformTrack)
                {
                    KeyTransform(binding);
                    keyedAny = true;
                }
                else if (binding.Tracks[i] is Sequencer.Asset.TtPropertyTrack propertyTrack)
                {
                    KeyProperty(binding, propertyTrack);
                    keyedAny = true;
                }
            }
            if (keyedAny == false)
                KeyTransform(binding);
        }
        /// <summary>给时间轴上某一行所属的轨道打点。track 为 null (绑定占位行) 时建 Transform 轨。</summary>
        void KeyRow(Sequencer.Asset.TtSequenceBinding binding, Sequencer.Asset.TtSequenceTrack track)
        {
            if (track is Sequencer.Asset.TtPropertyTrack propertyTrack)
                KeyProperty(binding, propertyTrack);
            else
                KeyTransform(binding);
        }

        /// <summary>
        /// 在播放头处按当前节点的 Placement 打一组 Transform 关键帧。
        /// 轨道 / Section 不存在时按需创建, 这些创建也一起进同一条 Undo 里, 否则撤销后会
        /// 留下一条空轨道。
        /// </summary>
        void KeyTransform(Sequencer.Asset.TtSequenceBinding binding)
        {
            if (binding == null)
                return;
            var node = mPlayer.Resolver.Resolve(binding);
            if (node == null || node.Placement == null)
                return;

            // 播放头本身是任意 tick (scrub 不吸附), 但关键帧必须落在整帧上
            var tick = SnapTick(mCurrentTick);
            bool trackCreated = binding.FindTrack("Transform") == null;
            var track = binding.GetOrCreateTransformTrack();
            // GetOrCreateSectionAt 常返回已有的 Section (本次打点会把它的范围扩过来), 那时 undo 不能
            // 把它删掉 —— 只能看 Sections 是不是真的多出了一个。
            var sectionCountBefore = track.Sections.Count;
            var section = track.GetOrCreateSectionAt(tick);
            bool sectionCreated = track.Sections.Count > sectionCountBefore;

            var before = FSectionSnapshot.Capture(section);
            WriteKeys(section, node, tick);
            var after = FSectionSnapshot.Capture(section);
            OnSequenceDataChanged();

            var cmd = new Infrastructure.TtDelegateCommand("Key Transform",
                () =>
                {
                    if (trackCreated && binding.Tracks.Contains(track) == false)
                        binding.Tracks.Add(track);
                    if (sectionCreated && track.Sections.Contains(section) == false)
                        track.Sections.Add(section);
                    after.ApplyTo(section);
                    OnSequenceDataChanged();
                },
                () =>
                {
                    before.ApplyTo(section);
                    if (sectionCreated)
                        track.RemoveSection(section);
                    if (trackCreated)
                        binding.Tracks.Remove(track);
                    OnSequenceDataChanged();
                });
            cmd.Seal();
            // 改动已经落在数据上了, 用 PushCommand 而不是 ExecuteCommand
            mEditorHistory?.PushCommand(cmd);
        }
        void WriteKeys(Sequencer.Asset.TtTransformSection section, GamePlay.Scene.TtNode node, long tick)
        {
            if (tick > section.EndTick)
                section.EndTick = tick;
            if (tick < section.StartTick)
                section.StartTick = tick;

            var position = node.Placement.Position;
            var scale = node.Placement.Scale;
            AddScalarKey(section.PositionX, tick, position.X);
            AddScalarKey(section.PositionY, tick, position.Y);
            AddScalarKey(section.PositionZ, tick, position.Z);
            AddScalarKey(section.ScaleX, tick, scale.X);
            AddScalarKey(section.ScaleY, tick, scale.Y);
            AddScalarKey(section.ScaleZ, tick, scale.Z);
            section.Rotation.AddKey(tick, node.Placement.Quat);
        }
        void AddScalarKey(TtScalarChannel channel, long tick, double value)
        {
            var index = channel.AddKey(tick, value);
            // 只算自己会让左右邻居的切线停在旧值上, 曲线会出现看不出原因的拐折
            channel.AutoComputeTangentsAround(index, Sequence.TickResolution);
        }

        /// <summary>
        /// 在播放头处按目标属性的当前值打一组关键帧。与 KeyTransform 的差别只在"值从哪来":
        /// 这里走访问器读属性, 拆到哪几条通道上由适配器决定。
        ///
        /// 不像 KeyTransform 那样会按需建轨道: 属性轨是用户在 Bindings 面板上显式加的,
        /// 能走到这里就说明轨道已经存在。
        /// </summary>
        void KeyProperty(Sequencer.Asset.TtSequenceBinding binding, Sequencer.Asset.TtPropertyTrack track)
        {
            if (binding == null || track == null)
                return;
            var node = mPlayer.Resolver.Resolve(binding);
            if (node == null)
                return;
            var accessor = TtEngine.Instance.SequencerModule.PropertyRegistry.Find(track.PropertyId);
            if (accessor == null)
            {
                // 轨道的 PropertyId 指向了一个现在没注册的访问器 (比如老资产里引用了已删掉的
                // 内置访问器)。必须吐一句: 点了菜单什么都不发生而不给理由, 用户无从得知。
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: 轨道 {track.DisplayName} 的属性 {track.PropertyId} 找不到访问器, 没打上关键帧");
                return;
            }
            // 属性当前是空引用 (没设资产的 RName), 或者节点类型上根本没这条属性 —— 两种
            // 情况 Read 都返回 null。打一个值是 null 的关键帧没意义 (求值时会被当成"不覆盖
            // 当前值"), 直接不打。
            var value = accessor.Read(node);
            if (value == null)
            {
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: {node.NodeName} 的 {track.DisplayName} 当前没有值 (空引用, 或这个节点类型上没这条属性), 打不出关键帧。先在 Bindings 面板的 Scene Nodes 里选中节点, 把这条属性设上值再打点");
                return;
            }

            // 同 KeyTransform: 播放头是任意 tick, 打点前吸附到整帧
            var tick = SnapTick(mCurrentTick);
            // GetOrCreateSectionAt 常返回已有的 Section, 那时 undo 不能把它删掉 (同 KeyTransform)
            var sectionCountBefore = track.Sections.Count;
            var section = track.GetOrCreateSectionAt(tick);
            bool sectionCreated = track.Sections.Count > sectionCountBefore;

            var before = FSectionSnapshot.Capture(section);
            if (tick > section.EndTick)
                section.EndTick = tick;
            if (tick < section.StartTick)
                section.StartTick = tick;
            var resolution = Sequence.TickResolution;
            if (section.AddKeyFromValue(tick, value, in resolution) == false)
            {
                // 适配器没找到 (轨道的 AdapterId 指向了已删掉的适配器): 一个关键帧都没写进去,
                // 把刚刚可能新建的 Section 和改过的范围一并收回去, 不给历史里留一条空命令
                before.ApplyTo(section);
                if (sectionCreated)
                    track.RemoveSection(section);
                Profiler.Log.WriteLine<Sequencer.TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequenceEditor: 轨道 {track.DisplayName} 的适配器 {track.AdapterId} 找不到, 没打上关键帧");
                return;
            }
            var after = FSectionSnapshot.Capture(section);
            OnSequenceDataChanged();

            var cmd = new Infrastructure.TtDelegateCommand($"Key {track.DisplayName}",
                () =>
                {
                    if (sectionCreated && track.Sections.Contains(section) == false)
                        track.Sections.Add(section);
                    after.ApplyTo(section);
                    OnSequenceDataChanged();
                },
                () =>
                {
                    before.ApplyTo(section);
                    if (sectionCreated)
                        track.RemoveSection(section);
                    OnSequenceDataChanged();
                });
            cmd.Seal();
            mEditorHistory?.PushCommand(cmd);
        }

        void RemoveKeyAt(FKeyHandle handle)
        {
            if (handle == null || handle.Section == null)
                return;
            var before = FSectionSnapshot.Capture(handle.Section);
            mChannelBuffer.Clear();
            handle.Section.GatherChannels(mChannelBuffer);
            for (int i = 0; i < mChannelBuffer.Count; ++i)
            {
                var channel = mChannelBuffer[i];
                int index = IndexOfKeyTime(channel, handle.Tick);
                if (index >= 0)
                    channel.RemoveKey(index);
            }
            RecomputeTangents(handle.Section);
            OnSequenceDataChanged();
            PushSectionCommand("Remove Key", handle.Section, before);
            mSelectedObject = null;
            DetailPropGrid.Target = null;
        }
        void MoveKeysAt(Sequencer.Asset.TtSequenceSection section, long oldTick, long newTick)
        {
            mChannelBuffer.Clear();
            section.GatherChannels(mChannelBuffer);
            for (int i = 0; i < mChannelBuffer.Count; ++i)
            {
                var channel = mChannelBuffer[i];
                int index = IndexOfKeyTime(channel, oldTick);
                if (index >= 0)
                    channel.SetKeyTime(index, newTick);
            }
            RecomputeTangents(section);
        }
        /// <summary>
        /// 重算本 Section 所有通道的 Auto 切线。没有切线概念的通道 (四元数 / 资产引用)
        /// 把 AutoComputeAllTangents 实现成了空操作, 所以这里不需要按通道类型分支。
        /// </summary>
        void RecomputeTangents(Sequencer.Asset.TtSequenceSection section)
        {
            var resolution = Sequence.TickResolution;
            mChannelBuffer.Clear();
            section.GatherChannels(mChannelBuffer);
            for (int i = 0; i < mChannelBuffer.Count; ++i)
                mChannelBuffer[i].AutoComputeAllTangents(in resolution);
        }
        static int IndexOfKeyTime(ISequenceChannel channel, long tick)
        {
            for (int i = 0; i < channel.KeyCount; ++i)
            {
                if (channel.GetKeyTime(i) == tick)
                    return i;
            }
            return -1;
        }
        /// <summary>取本 Section 所有通道关键帧时刻的并集, 升序</summary>
        void GatherKeyTicks(Sequencer.Asset.TtSequenceSection section, List<long> result)
        {
            result.Clear();
            mChannelBuffer.Clear();
            section.GatherChannels(mChannelBuffer);
            for (int i = 0; i < mChannelBuffer.Count; ++i)
            {
                var channel = mChannelBuffer[i];
                for (int k = 0; k < channel.KeyCount; ++k)
                {
                    var tick = channel.GetKeyTime(k);
                    if (result.Contains(tick) == false)
                        result.Add(tick);
                }
            }
            result.Sort();
        }

        /// <summary>
        /// 序列播放器。对外公开主要是为了诊断 (MCP 工具): 拿它的 Resolver 能把绑定解成
        /// 场景节点, 从而检查求值结果真的落到了节点上。不要拿它去改播放状态 —— 播放头
        /// 还得同步给时间轴, 那是 ScrubTo 的责任。
        /// </summary>
        public TtSequencePlayer Player { get => mPlayer; }
        /// <summary>当前播放头位置 (任意 tick, 未吸附到整帧)</summary>
        public long CurrentTick { get => mCurrentTick; }
        /// <summary>
        /// 把播放头挪到 tick 并立即求值, 返回实际生效的 tick (会被夹到播放范围内)。
        ///
        /// 走的就是拖动播放头那条路径, 所以外部用它复现 scrub 的结果是可信的 —— 这也是
        /// 它存在的理由: 另开一条求值路径去诊断, 验出来的结果就不代表界面上的行为了。
        /// </summary>
        public long ScrubTo(long tick)
        {
            SetPlayHead(tick);
            return mCurrentTick;
        }

        void SetPlayHead(long tick)
        {
            mCurrentTick = Sequence.ClampTick(tick);
            mPlayer.SetPosition(mCurrentTick);
            mTimeline.PlayPosition = TickToSeconds(mCurrentTick);
        }
        /// <summary>数据变了之后重新求值, 让视口当帧就跟上 (而不是等下一帧 Tick)</summary>
        void OnSequenceDataChanged()
        {
            Sequence.ExpandPlaybackRangeToContent();
            mPlayer.SetPosition(mCurrentTick);
        }
        #endregion 绑定与打点

        #region Undo

        /// <summary>
        /// 一个 Section 的完整时间数据快照。打点 / 拖关键帧 / 拖 Section 都是
        /// "改一堆通道里的若干个数"，逐个记差量既啰嗦又容易漏, 直接整段快照最省心 —— Section
        /// 规模下 (几十个关键帧) 拷贝成本可以忽略。
        ///
        /// 不认识任何具体的 Section 类型: 通道从 GatherChannels 拿, 每条通道的关键帧交给
        /// 通道自己 CaptureKeys / RestoreKeys。加一种新通道类型 (或新的 Section 子类) 时
        /// 这里一行都不用改。
        /// </summary>
        class FSectionSnapshot
        {
            long mStartTick;
            long mEndTick;
            // 直接持有通道引用: 通道对象在 Section 生命周期内不会被换掉 (只换里面的关键帧
            // 表), 而回写时再问 Section 要一遍的话, 一旦通道数量变了 (属性轨的 EnsureChannels
            // 会补通道) 就会按下标错位。
            List<ISequenceChannel> mChannels = new List<ISequenceChannel>();
            List<object> mKeys = new List<object>();

            public static FSectionSnapshot Capture(Sequencer.Asset.TtSequenceSection section)
            {
                var result = new FSectionSnapshot();
                if (section == null)
                    return result;
                result.mStartTick = section.StartTick;
                result.mEndTick = section.EndTick;
                section.GatherChannels(result.mChannels);
                for (int i = 0; i < result.mChannels.Count; ++i)
                    result.mKeys.Add(result.mChannels[i].CaptureKeys());
                return result;
            }
            /// <summary>回写。通道自己负责不把快照里的表直接交出去 (快照要能被 Undo/Redo 反复用)</summary>
            public void ApplyTo(Sequencer.Asset.TtSequenceSection section)
            {
                if (section == null)
                    return;
                section.StartTick = mStartTick;
                section.EndTick = mEndTick;
                for (int i = 0; i < mChannels.Count; ++i)
                    mChannels[i].RestoreKeys(mKeys[i]);
            }
            /// <summary>
            /// section 当前的数据是不是和本快照完全一致。用来判断一次编辑要不要入历史。
            ///
            /// 比"再抓一份快照然后两份对比"少一次拷贝, 也少一层"哪份是当前值"的含义
            /// 模糊 —— 通道的 KeysEqualTo 比的本来就是"我现在 vs 你那份快照"。
            /// </summary>
            public bool MatchesCurrent(Sequencer.Asset.TtSequenceSection section)
            {
                if (section == null)
                    return false;
                if (section.StartTick != mStartTick || section.EndTick != mEndTick)
                    return false;
                var channels = new List<ISequenceChannel>();
                section.GatherChannels(channels);
                if (channels.Count != mChannels.Count)
                    return false;
                for (int i = 0; i < mChannels.Count; ++i)
                {
                    // 通道对象换了就不能再按下标对比, 一律当成"变了"让这次编辑入历史
                    if (ReferenceEquals(channels[i], mChannels[i]) == false)
                        return false;
                    if (mChannels[i].KeysEqualTo(mKeys[i]) == false)
                        return false;
                }
                return true;
            }
        }

        /// <summary>改动已经落在 section 上了, 用 before 快照合成一条历史</summary>
        void PushSectionCommand(string name, Sequencer.Asset.TtSequenceSection section, FSectionSnapshot before)
        {
            if (mEditorHistory == null || section == null || before == null)
                return;
            if (before.MatchesCurrent(section))
                return;
            var after = FSectionSnapshot.Capture(section);

            var cmd = new Infrastructure.TtDelegateCommand(name,
                () => { after.ApplyTo(section); OnSequenceDataChanged(); },
                () => { before.ApplyTo(section); OnSequenceDataChanged(); });
            cmd.Seal();
            mEditorHistory.PushCommand(cmd);
        }
        /// <summary>结构性增删统一走历史, 并让解析缓存失效 (绑定列表变了)</summary>
        void ExecuteStructureCommand(string name, Action doAction, Action undoAction)
        {
            var cmd = new Infrastructure.TtDelegateCommand(name,
                () => { doAction(); mPlayer.Resolver.Invalidate(); OnSequenceDataChanged(); },
                () => { undoAction(); mPlayer.Resolver.Invalidate(); OnSequenceDataChanged(); });
            cmd.Seal();
            if (mEditorHistory != null)
                mEditorHistory.ExecuteCommand(cmd);
            else
                cmd.Do();
        }
        #endregion Undo

        #region ITickable

        /// <summary>上一帧预览世界根下的子节点数, 用来发现用户拖进/删掉了节点 (见 TickLogic)</summary>
        int mLastPreviewChildCount = -1;

        public void TickLogic(float ellapse)
        {
            if (Sequence != null)
            {
                // 预览世界可能在编辑器打开之后才建好, SetScene 相同实例会直接返回, 不必判重
                var previewRoot = PreviewViewport.World?.Root;
                mPlayer.Resolver.SetScene(previewRoot);
                // 解析结果连同"解析失败的 null"一起按 BindingId 缓存着, 所以用户往预览世界里补
                // 拖节点之后必须清缓存, 否则绑定会一直停在 <Unresolved> —— 预览世界不落盘,
                // 重开编辑器后重新拖同名节点是恢复绑定的正常手段。节点数变化是最便宜的判据。
                var previewChildCount = previewRoot != null ? previewRoot.Children.Count : 0;
                if (previewChildCount != mLastPreviewChildCount)
                {
                    mLastPreviewChildCount = previewChildCount;
                    mPlayer.Resolver.Invalidate();
                }
                if (mPlayer.State == ESequencePlaybackState.Playing)
                {
                    // TickLogic 的 ellapse 是毫秒 (跟 SkeletonEditor / PhyScene / CCamera 一致), 而
                    // 播放器推进的单位是秒。直传的话一帧就冲过整条序列, LoopMode.Once 下立即
                    // 置 Stopped —— 表现为点 Play 没任何反应。
                    mPlayer.Update(ellapse * 0.001f);
                    mCurrentTick = mPlayer.PositionTick;
                    mTimeline.PlayPosition = TickToSeconds(mCurrentTick);
                }
            }
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
        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        public string GetWindowsName()
        {
            return Sequence.AssetName.Name;
        }
        #endregion ITickable
    }
}

namespace EngineNS.Sequencer.Asset
{
    [Editor.TtAssetEditor(EditorType = typeof(Editor.Forms.TtSequenceEditor))]
    public partial class TtSequence
    {
    }
}
