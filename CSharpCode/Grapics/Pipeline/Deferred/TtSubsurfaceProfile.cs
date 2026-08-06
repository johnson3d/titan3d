using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    /// <summary>
    /// GPU-side subsurface profile data. Must match HLSL FSubsurfaceProfile exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct FSubsurfaceProfile
    {
        public Vector3 ScatterColor;
        public float ScatterRadius;
        public Vector3 FalloffColor;
        public float SubsurfaceOpacity;

        public static FSubsurfaceProfile DefaultSkin => new FSubsurfaceProfile
        {
            ScatterColor = new Vector3(0.48f, 0.25f, 0.14f),
            ScatterRadius = 1.0f,
            FalloffColor = new Vector3(1.0f, 0.37f, 0.3f),
            SubsurfaceOpacity = 1.0f,
        };
    }

    #region Asset
    [Rtti.Meta("")]
    public class TtSubsurfaceProfileAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtSubsurfaceProfileData.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.SubsurfaceProfileManager.GetProfile(GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.SubsurfaceProfileManager.CreateProfile(GetAssetName());
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            cmdlist.AddText(in start, 0xFFFFFFFF, "SSS", null);
        }
        public override string GetAssetTypeName()
        {
            return "SSSProfile";
        }
    }

    [TtSubsurfaceProfileData.TtSubsurfaceProfileImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/SubsurfaceProfile")]
    [Editor.TtAssetEditor(EditorType = typeof(TtSubsurfaceProfileEditor))]
    public class TtSubsurfaceProfileData : IO.BaseSerializer, IO.IAsset, IO.ISerializer
    {
        public const string AssetExt = ".sssprofile";
        public string TypeExt { get => AssetExt; }

        public class TtSubsurfaceProfileImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = new TtSubsurfaceProfileData();
                PGAsset.Target = mAsset;
            }
            protected override bool CheckAsset()
            {
                return true;
            }
        }

        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            return new TtSubsurfaceProfileAMeta();
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            name.AMeta.ClearAssetFiles();
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("Profile", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }
            xnd.SaveXnd(name.Address);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static TtSubsurfaceProfileData LoadXnd(IO.TtXndNode node)
        {
            var result = new TtSubsurfaceProfileData();
            if (ReloadXnd(result, node) == false)
                return null;
            return result;
        }
        public static bool ReloadXnd(TtSubsurfaceProfileData profile, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("Profile");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(profile, null);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            return true;
        }

        [Rtti.Meta("")]
        [RName.PGRName(ReadOnly = true)]
        public RName AssetName { get; set; }
        #endregion

        [Rtti.Meta("")]
        [Category("Profile")]
        [TtColor3PickerEditor()]
        public Vector3 ScatterColor { get; set; } = new Vector3(0.48f, 0.25f, 0.14f);

        [Rtti.Meta("")]
        [Category("Profile")]
        public float ScatterRadius { get; set; } = 1.0f;

        [Rtti.Meta("")]
        [Category("Profile")]
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
        public Vector3 FalloffColor { get; set; } = new Vector3(1.0f, 0.37f, 0.3f);

        [Rtti.Meta("")]
        [Category("Profile")]
        public float SubsurfaceOpacity { get; set; } = 1.0f;

        public FSubsurfaceProfile ToGpuProfile()
        {
            return new FSubsurfaceProfile
            {
                ScatterColor = ScatterColor,
                ScatterRadius = ScatterRadius,
                FalloffColor = FalloffColor,
                SubsurfaceOpacity = SubsurfaceOpacity,
            };
        }
    }
    #endregion

    #region Editor
    public class TtSubsurfaceProfileEditor : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public TtSubsurfaceProfileData ProfileData;
        public EGui.Controls.PropertyGrid.TtPropertyGrid ProfilePropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();

        #region 统一Undo/Redo(开门)
        // 控制门就是是否new出历史栈: 需回退旧流程时把mEditorHistory改为null即可
        public bool EnableUndoRedo => EditorHistory != null;
        public EngineNS.Editor.Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        EngineNS.Editor.Infrastructure.TtEditorHistory mEditorHistory = new EngineNS.Editor.Infrastructure.TtEditorHistory();
        EngineNS.Editor.Infrastructure.TtEditorHistoryPanel mHistoryPanel = new EngineNS.Editor.Infrastructure.TtEditorHistoryPanel();
        #endregion

        ~TtSubsurfaceProfileEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            ProfileData = null;
            ProfilePropGrid.Target = null;
            ProfilePropGrid.HistoryHost = null;
            mEditorHistory?.Clear();
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await ProfilePropGrid.Initialize();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            ProfileData = await name.CreateAsset<TtSubsurfaceProfileData>();
            if (ProfileData == null)
                return false;

            ProfilePropGrid.Target = ProfileData;
            mEditorHistory?.Clear();
            ProfilePropGrid.HistoryHost = mEditorHistory;
            Visible = true;
            return true;
        }
        public void OnCloseEditor()
        {
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
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || ProfileData == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
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
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
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
                ProfileData.SaveAssetTo(ProfileData.AssetName);
                var unused = TtEngine.Instance.GfxDevice.SubsurfaceProfileManager.ReloadProfile(ProfileData.AssetName);
                mEditorHistory?.SetSavePoint();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {
                var unused = TtEngine.Instance.GfxDevice.SubsurfaceProfileManager.ReloadProfile(ProfileData.AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            // mEditorHistory为null时按钮/快捷键均为空操作
            EngineNS.Editor.Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
            EngineNS.Editor.Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Apply to GPU", in btSize))
            {
                var manager = TtEngine.Instance?.GfxDevice?.SubsurfaceProfileManager;
                if (manager != null && ProfileData?.AssetName != null)
                {
                    var gpuProfile = ProfileData.ToGpuProfile();
                    manager.RegisterProfile(ProfileData.AssetName, in gpuProfile, true);
                }
            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("ProfileProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ProfilePropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e) { }
        public string GetWindowsName()
        {
            return ProfileData.AssetName.Name;
        }
    }
    #endregion

    #region Manager
    /// <summary>
    /// Manages a table of subsurface scattering profiles (max 256).
    /// Uses TtCpu2GpuBuffer for standard CPU→GPU upload.
    /// Provides RName→Index mapping and asset load/save/reload lifecycle.
    /// </summary>
    public class TtSubsurfaceProfileManager : IDisposable
    {
        public const int MaxProfileCount = 256;

        private TtCpu2GpuBuffer<FSubsurfaceProfile> mGpuBuffer;
        private Dictionary<RName, int> mRNameToIndex = new Dictionary<RName, int>();
        public Dictionary<RName, TtSubsurfaceProfileData> ProfileAssets { get; } = new Dictionary<RName, TtSubsurfaceProfileData>();

        public NxRHI.TtSrView ProfileSRV => mGpuBuffer?.Srv;
        public int ProfileCount => mGpuBuffer?.DataArray.Count ?? 0;

        public TtSubsurfaceProfileManager()
        {
            mGpuBuffer = new TtCpu2GpuBuffer<FSubsurfaceProfile>();
            mGpuBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);

            // Index 0 = default skin profile
            var defaultProfile = FSubsurfaceProfile.DefaultSkin;
            mGpuBuffer.PushData(in defaultProfile);
        }

        public void Cleanup()
        {
            ProfileAssets.Clear();
            mRNameToIndex.Clear();
        }

        /// <summary>
        /// Register a profile with a named asset and return its index (0~255).
        /// If already registered, returns existing index and updates GPU data.
        /// </summary>
        public int RegisterProfile(RName assetName, in FSubsurfaceProfile profile, bool bFlushToGpu)
        {
            if (assetName != null && mRNameToIndex.TryGetValue(assetName, out int existingIndex))
            {
                SetProfile(existingIndex, in profile, bFlushToGpu);
                return existingIndex;
            }

            int newIndex = AddProfile(in profile);
            if (assetName != null)
                mRNameToIndex[assetName] = newIndex;
            return newIndex;
        }

        /// <summary>
        /// Get the profile index for a given RName. Returns 0 (default) if not found.
        /// </summary>
        public int GetIndexByRName(RName assetName)
        {
            if (assetName == null)
                return 0;
            if (mRNameToIndex.TryGetValue(assetName, out int index))
                return index;
            return 0;
        }

        /// <summary>
        /// Add a profile and return its index (0~255).
        /// </summary>
        public int AddProfile(in FSubsurfaceProfile profile)
        {
            if (ProfileCount >= MaxProfileCount)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning,
                    "SubsurfaceProfileManager: max profile count reached (256)");
                return ProfileCount - 1;
            }
            return mGpuBuffer.PushData(in profile);
        }

        /// <summary>
        /// Update an existing profile by index.
        /// </summary>
        public void SetProfile(int index, in FSubsurfaceProfile profile, bool bFlushToGpu)
        {
            mGpuBuffer.UpdateData(index, in profile);
            if (bFlushToGpu)
            {
                FlushToGpu();
            }
        }

        public FSubsurfaceProfile GetProfileData(int index)
        {
            if (index < 0 || index >= ProfileCount)
                return mGpuBuffer.DataArray[0];
            return mGpuBuffer.DataArray[index];
        }

        /// <summary>
        /// Flush dirty profiles to GPU. Call once per frame before rendering.
        /// </summary>
        public void FlushToGpu()
        {
            mGpuBuffer.Flush2GPU((NxRHI.TtCommandList)null);
        }

        #region Asset Lifecycle
        public TtSubsurfaceProfileData GetProfileSync(RName rn)
        {
            if (rn == null)
                return null;

            if (ProfileAssets.TryGetValue(rn, out var existing))
                return existing;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd == null)
                    return null;

                var profileData = TtSubsurfaceProfileData.LoadXnd(xnd.RootNode);
                if (profileData == null)
                    return null;

                profileData.AssetName = rn;
                ProfileAssets[rn] = profileData;

                var gpuProfile = profileData.ToGpuProfile();
                RegisterProfile(rn, in gpuProfile, true);
                return profileData;
            }
        }

        public async Thread.Async.TtTask<TtSubsurfaceProfileData> GetProfile(RName rn)
        {
            if (rn == null)
                return null;

            if (ProfileAssets.TryGetValue(rn, out var existing))
                return existing;

            var result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd == null)
                        return null;

                    var profileData = TtSubsurfaceProfileData.LoadXnd(xnd.RootNode);
                    if (profileData == null)
                        return null;

                    profileData.AssetName = rn;
                    return profileData;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                ProfileAssets[rn] = result;
                var gpuProfile = result.ToGpuProfile();
                RegisterProfile(rn, in gpuProfile, true);
                return result;
            }
            return null;
        }

        public async Thread.Async.TtTask<TtSubsurfaceProfileData> CreateProfile(RName rn)
        {
            return await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd == null)
                        return null;

                    var profileData = TtSubsurfaceProfileData.LoadXnd(xnd.RootNode);
                    if (profileData == null)
                        return null;

                    profileData.AssetName = rn;
                    return profileData;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
        }

        public async Thread.Async.TtTask<bool> ReloadProfile(RName rn)
        {
            if (!ProfileAssets.TryGetValue(rn, out var existing))
                return true;

            var reloaded = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd == null)
                        return false;
                    return TtSubsurfaceProfileData.ReloadXnd(existing, xnd.RootNode);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (reloaded && mRNameToIndex.TryGetValue(rn, out int index))
            {
                var gpuProfile = existing.ToGpuProfile();
                SetProfile(index, in gpuProfile, true);
            }
            return reloaded;
        }
        #endregion

        public void Dispose()
        {
            mRNameToIndex.Clear();
            ProfileAssets.Clear();
            mGpuBuffer?.Dispose();
            mGpuBuffer = null;
        }
    }
    #endregion
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class TtGfxDevice
    {
        public Deferred.TtSubsurfaceProfileManager SubsurfaceProfileManager { get; } = new Deferred.TtSubsurfaceProfileManager();
    }
}
