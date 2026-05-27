using EngineNS.IO;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public partial class TtContentBrowser : IRootForm, EGui.IPanel
    {
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        uint mDockId = uint.MaxValue;
        public uint DockId { get => mDockId; set => mDockId = value; }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.SearchBarProxy mSearchBar;

        public string Name = "";
        public bool CreateNewAssets = true;
        public bool AutoGenerateSnapshots = false;
        //bool mViewDirty = true;
        string mExtNames = null;
        public string ExtNames 
        {
            get => mExtNames;
            set
            {
                if (mExtNames == value)
                    return;
                mExtNames = value;
                mExtNameArray = mExtNames.Split(',');
                mFolderView.ExtNameArray = mExtNameArray;
                mFolderView.ClearDirectoryShowCache();
                InvalidateAssetScan();
            }
        }
        string[] mExtNameArray;

        Rtti.TtTypeDesc mMacrossBase = null;
        public Rtti.TtTypeDesc MacrossBase
        {
            get => mMacrossBase;
            set
            {
                mMacrossBase = value;
                mFolderView.MacrossBase = mMacrossBase;
                InvalidateAssetScan();
            }
        }
        string mShaderType = null;
        public string ShaderType
        {
            get => mShaderType;
            set
            {
                mShaderType = value;
                mFolderView.ShaderType = mShaderType;
                InvalidateAssetScan();
            }
        }
        public string FilterText = "";
        public static IO.IAssetMeta GlobalSelectedAsset = null;
        static RName mGlobalFocusAsset = null;
        public static RName GlobalFocusAsset
        {
            get => mGlobalFocusAsset;
            set
            {
                if(value != null)
                {
                    if(RName.IsExist(value))
                        mGlobalFocusAsset = value;
                }
                else
                    mGlobalFocusAsset = null;
            }
        }
        public List<IO.IAssetMeta> SelectedAssets = new List<IO.IAssetMeta>();
        public Action<IO.IAssetMeta> ItemSelectedAction = null;
        public float ItemScale = 1.0f;

        public static string FilterImgName = "uestyle/content/filter.srv";

        public void Dispose()
        {
            CancelAssetScan(true);
            GlobalSelectedAsset = null;
            SelectedAssets.Clear();
            mSearchBar?.Cleanup();
            mSearchBar = null;
        }

        TtMenuItem mContextMenu;
        string mContextMenuFilterStr = "";
        int mSelectQuickMenuIdx = 0;
        int mCurrentQuickMenuIdx = 0;
        bool mContextMenuFilterFocused = false;
        bool mContextMenuOpenCheck = false;
        bool mOldContextMenuOpenCheck = false;
        bool mShiftSelection = false;

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            if(mContextMenu == null)
            {
                mContextMenu = new TtMenuItem()
                {
                    Text = "Content context menu",
                };

                OnTypeChanged();
            }

            mSearchBar = new UIProxy.SearchBarProxy();
            await mSearchBar.Initialize();
            mSearchBar.InfoText = "Search Assets";
            //Rtti.UTypeDescManager.Instance.OnTypeChanged += OnTypeChanged;

            await mFolderView.Initialize();
            await mSelectFolderView.Initialize();
            InitializeFilterMenu();

            if (TtEngine.Instance.UIProxyManager[FilterImgName] == null)
                TtEngine.Instance.UIProxyManager[FilterImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(FilterImgName, RName.ERNameType.Engine));

            return true;
        }
        Task SureBarTask = null;
        internal void SureSearchBar()
        {
            if (SureBarTask!=null && SureBarTask.IsCompleted==false)
            {
                return;
            }
            if (mSearchBar == null)
            {
                if (SureBarTask == null)
                {
                    mSearchBar = new UIProxy.SearchBarProxy();
                    SureBarTask = mSearchBar.Initialize();
                    mSearchBar.InfoText = "Search Assets";
                }
                else if(SureBarTask.IsCompleted)
                {
                    SureBarTask = null;
                }
            }
        }

        public void OnTypeChanged()
        {
            mContextMenu.SubMenuItems.Clear();

            // New Asset menu
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach(var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(IO.AssetCreateMenuAttribute), false);
                    if (atts.Length == 0)
                        continue;
                    var assetExtField = Rtti.TtTypeDesc.GetField(typeDesc.SystemType, "AssetExt");
                    var parentMenu = mContextMenu;
                    var att = atts[0] as IO.AssetCreateMenuAttribute;
                    var splits = att.MenuName.Split('/');
                    for(var menuIdx = 0; menuIdx < splits.Length; menuIdx++)
                    {
                        var menuStr = splits[menuIdx];
                        if (menuIdx < splits.Length - 1)
                            parentMenu = mContextMenu.AddMenuItem(menuStr, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuStr, menuStr, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    EnqueueAssetImporter(TtEngine.Instance.AssetMetaManager.ImportAsset(mFolderView.CurrentDir, typeDesc, (string)assetExtField.GetValue(null)), ""); 
                                });
                        }
                    }
                }
            }
        }
        bool CheckExtNameValid(in string name)
        {
            if (mExtNameArray != null && mExtNameArray.Length > 0)
            {
                var ext = IO.TtFileManager.GetExtName(name);
                bool find = false;
                for (int extIdx = 0; extIdx < mExtNameArray.Length; extIdx++)
                {
                    if (string.Equals(ext, mExtNameArray[extIdx], StringComparison.OrdinalIgnoreCase))
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                    return false;
            }
            return true;
        }

        bool CheckAssetMetaConstraint(in string name, IO.IAssetMeta ameta)
        {
            if (mExtNameArray != null && mExtNameArray.Length > 0)
            {
                var ext = IO.TtFileManager.GetExtName(name);
                if (MacrossBase != null && ext == Bricks.CodeBuilder.TtMacross.AssetExt)
                {
                    var ameta1 = ameta as Bricks.CodeBuilder.TtMacrossAMeta;
                    if (ameta1 == null || ameta1.BaseType == null)
                        return false;
                    if (!ameta1.BaseType.IsSubclassOf(MacrossBase) && ameta1.BaseType != MacrossBase)
                    {
                        return false;
                    }
                }
                else if (ShaderType != null && ext == Graphics.Pipeline.Shader.TtShaderAsset.AssetExt)
                {
                    var ameta1 = ameta as Graphics.Pipeline.Shader.TtShaderAssetAMeta;
                    if (ameta1 == null)
                        return false;

                    if (ameta1.ShaderType != ShaderType)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        bool CheckExtValid(in string name, RName dir)
        {
            if (!CheckExtNameValid(in name))
                return false;

            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType));
            return CheckAssetMetaConstraint(in name, ameta);
        }

        int mSortedAssetsColumn = 0;
        struct ViewAssetsData
        {
            public IAssetMeta Meta;
            public string File;
            public string PathName;
            public string Type
            {
                get
                {
                    if (Meta == null)
                        return "Invalid";
                    return Meta.GetAssetTypeName();
                }
            }
            public string Name
            {
                get
                {
                    if (Meta == null)
                        return "Invalid";
                    return IO.TtFileManager.GetPureName(Meta.GetAssetName().Name);
                }
            }
        }
        List<ViewAssetsData> mViewAssetsDatas = new List<ViewAssetsData>();
        List<ViewAssetsData> mViewMetadataDatas = new List<ViewAssetsData>();

        const int AssetPageSize = 256;
        const int AssetScanProcessMaxItemsPerFrame = 24;
        const double AssetScanProcessBudgetMilliseconds = 2.0;
        int mAssetPageIndex = 0;

        class AssetScanResult
        {
            public string Error;
        }

        class AssetScanState
        {
            public RName Dir;
            public string DirName;
            public string DirAddress;
            public RName.ERNameType DirType;
            public bool WithChildFolders;
            public int Version;
            public Task<AssetScanResult> ScanTask;
            public bool ScanResultApplied;
            public ConcurrentQueue<ViewAssetsData> PendingAssets = new ConcurrentQueue<ViewAssetsData>();
            public ConcurrentQueue<string> PendingMetadataFiles = new ConcurrentQueue<string>();
            public int EnumeratedAssetCount;
            public int EnumeratedMetadataCount;
            public int ProcessedAssetCount;
            public int ProcessedMetadataCount;
            public bool IsComplete;
            public string Error;
        }

        AssetScanState mAssetScanState;
        CancellationTokenSource mAssetScanCancellation;
        int mAssetScanVersion = 0;
        volatile bool mAssetScanDirty = false;

        void InvalidateAssetScan()
        {
            mAssetScanDirty = true;
        }

        void CancelAssetScan(bool dispose)
        {
            var cancellation = mAssetScanCancellation;
            mAssetScanCancellation = null;
            if (cancellation != null)
            {
                cancellation.Cancel();
                if (dispose)
                    cancellation.Dispose();
            }
        }

        void ResetAssetScan()
        {
            CancelAssetScan(false);
            mAssetScanState = null;
            mViewAssetsDatas.Clear();
            mViewMetadataDatas.Clear();
            mAssetScanDirty = false;
            mAssetPageIndex = 0;
        }

        static bool IsSameAssetName(RName left, RName right)
        {
            if (left == null || right == null)
                return false;

            return left.RNameType == right.RNameType &&
                   string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
        }

        public void NotifyAssetDeleted(IO.IAssetMeta ameta, RName assetName)
        {
            var deletedAssets = new List<IO.IAssetMeta>();
            var deletedNames = new List<RName>();
            if (assetName == null && ameta != null)
                assetName = ameta.GetAssetName();
            if (ameta != null)
                deletedAssets.Add(ameta);
            if (assetName != null)
                deletedNames.Add(assetName);
            NotifyAssetsDeleted(deletedAssets, deletedNames);
        }

        public void NotifyAssetsDeleted(IReadOnlyList<IO.IAssetMeta> deletedAssets, IReadOnlyList<RName> deletedNames)
        {
            if (deletedAssets != null)
            {
                for (int i = 0; i < deletedAssets.Count; i++)
                {
                    if (deletedAssets[i] != null)
                    {
                        deletedAssets[i].IsSelected = false;
                        TtEngine.Instance.AssetMetaManager.RemoveAMeta(deletedAssets[i]);
                    }
                }
            }
            if (deletedNames != null)
            {
                for (int i = 0; i < deletedNames.Count; i++)
                {
                    TtEngine.Instance.AssetMetaManager.RemoveAMeta(deletedNames[i]);
                }
            }

            SelectedAssets.RemoveAll(i => i == null || IsDeletedAsset(i, deletedAssets, deletedNames));
            if (IsDeletedAsset(GlobalSelectedAsset, deletedAssets, deletedNames))
                GlobalSelectedAsset = null;
            if (IsDeletedAssetName(GlobalFocusAsset, deletedNames))
                GlobalFocusAsset = null;

            mFolderView.ClearDirectoryShowCache();
            ResetAssetScan();
        }

        static bool IsDeletedAsset(IO.IAssetMeta asset, IReadOnlyList<IO.IAssetMeta> deletedAssets, IReadOnlyList<RName> deletedNames)
        {
            if (asset == null)
                return false;

            if (deletedAssets != null)
            {
                for (int i = 0; i < deletedAssets.Count; i++)
                {
                    if (asset == deletedAssets[i])
                        return true;
                }
            }
            return IsDeletedAssetName(asset.GetAssetName(), deletedNames);
        }

        static bool IsDeletedAssetName(RName assetName, IReadOnlyList<RName> deletedNames)
        {
            if (assetName == null || deletedNames == null)
                return false;

            for (int i = 0; i < deletedNames.Count; i++)
            {
                if (IsSameAssetName(assetName, deletedNames[i]))
                    return true;
            }
            return false;
        }

        bool IsSameAssetScan(AssetScanState state, RName dir)
        {
            if (state == null || dir == null)
                return false;

            return state.DirName == dir.Name &&
                   state.DirAddress == dir.Address &&
                   state.DirType == dir.RNameType &&
                   state.WithChildFolders == mWithChildFolders;
        }

        AssetScanState EnsureAssetScan(RName dir)
        {
            if (dir == null)
                return null;

            if (!mAssetScanDirty && IsSameAssetScan(mAssetScanState, dir))
                return mAssetScanState;

            CancelAssetScan(false);
            mViewAssetsDatas.Clear();
            mViewMetadataDatas.Clear();
            mAssetScanDirty = false;

            var cancellation = new CancellationTokenSource();
            mAssetScanCancellation = cancellation;
            var token = cancellation.Token;
            var version = ++mAssetScanVersion;
            var dirAddress = dir.Address;
            var withChildFolders = mWithChildFolders;

            var state = new AssetScanState()
            {
                Dir = dir,
                DirName = dir.Name,
                DirAddress = dirAddress,
                DirType = dir.RNameType,
                WithChildFolders = withChildFolders,
                Version = version,
            };
            state.ScanTask = Task.Run(() => EnumerateAssetFiles(state, token), token);
            QueueIndexedAssets(state);
            mAssetScanState = state;
            return state;
        }

        void QueueIndexedAssets(AssetScanState state)
        {
            foreach (var assetMeta in TtEngine.Instance.AssetMetaManager.RNameAssets)
            {
                var assetName = assetMeta.Key;
                var ameta = assetMeta.Value;
                if (assetName == null || ameta == null)
                    continue;
                if (!IO.TtFileManager.FileExists(assetName.Address + IO.IAssetMeta.MetaExt))
                    continue;
                if (!IsAssetInDirectory(state, assetName, out var relativeName))
                    continue;
                if (!CheckExtNameValid(in relativeName))
                    continue;
                if (!CheckAssetMetaConstraint(in relativeName, ameta))
                    continue;

                state.PendingAssets.Enqueue(new ViewAssetsData()
                {
                    Meta = ameta,
                    File = assetName.Address,
                    PathName = relativeName,
                });
                state.EnumeratedAssetCount++;
            }
        }

        static bool IsAssetInDirectory(AssetScanState state, RName assetName, out string relativeName)
        {
            relativeName = null;
            if (assetName.RNameType != state.DirType)
                return false;

            var name = assetName.Name;
            var dirName = state.DirName ?? string.Empty;
            if (string.IsNullOrEmpty(name) ||
                !name.StartsWith(dirName, StringComparison.OrdinalIgnoreCase))
                return false;

            relativeName = name.Substring(dirName.Length);
            if (string.IsNullOrEmpty(relativeName))
                return false;

            if (!state.WithChildFolders && relativeName.IndexOfAny(new[] { '/', '\\' }) >= 0)
                return false;

            return true;
        }

        static AssetScanResult EnumerateAssetFiles(AssetScanState state, CancellationToken token)
        {
            var result = new AssetScanResult();
            var dirAddress = state.DirAddress;
            if (string.IsNullOrEmpty(dirAddress) || !System.IO.Directory.Exists(dirAddress))
                return result;

            try
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(dirAddress, "*.metadata", System.IO.SearchOption.TopDirectoryOnly))
                {
                    token.ThrowIfCancellationRequested();
                    state.PendingMetadataFiles.Enqueue(file);
                    Interlocked.Increment(ref state.EnumeratedMetadataCount);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result.Error = string.IsNullOrEmpty(result.Error) ? ex.Message : result.Error + "; " + ex.Message;
            }

            return result;
        }

        void UpdateAssetScanState(RName dir)
        {
            var state = EnsureAssetScan(dir);
            if (state == null || state.Version != mAssetScanVersion)
                return;

            if (!state.ScanResultApplied && state.ScanTask.IsCompleted)
            {
                state.ScanResultApplied = true;
                if (state.ScanTask.IsCanceled)
                {
                    state.IsComplete = true;
                    return;
                }
                if (state.ScanTask.IsFaulted)
                {
                    state.Error = state.ScanTask.Exception?.GetBaseException().Message;
                    state.IsComplete = true;
                    return;
                }

                var result = state.ScanTask.Result;
                state.Error = result.Error;
            }

            if (state.IsComplete)
                return;

            var budget = AssetScanProcessMaxItemsPerFrame;
            var startTime = Stopwatch.GetTimestamp();
            while (budget > 0 && state.PendingAssets.TryDequeue(out var assetData))
            {
                mViewAssetsDatas.Add(assetData);
                state.ProcessedAssetCount++;
                budget--;
                if (IsAssetScanTimeBudgetExceeded(startTime))
                    break;
            }
            while (budget > 0 && state.PendingMetadataFiles.TryDequeue(out var metadataFile))
            {
                ProcessMetadataScanFile(metadataFile);
                state.ProcessedMetadataCount++;
                budget--;
                if (IsAssetScanTimeBudgetExceeded(startTime))
                    break;
            }

            if (state.ScanResultApplied &&
                state.PendingAssets.IsEmpty &&
                state.PendingMetadataFiles.IsEmpty)
            {
                state.IsComplete = true;
                ApplyAssetSort();
            }
        }

        static bool IsAssetScanTimeBudgetExceeded(long startTime)
        {
            return (Stopwatch.GetTimestamp() - startTime) * 1000.0 / Stopwatch.Frequency >= AssetScanProcessBudgetMilliseconds;
        }

        void ProcessAssetScanFile(AssetScanState state, string metaFile)
        {
            if (string.IsNullOrEmpty(metaFile) || metaFile.Length <= IO.IAssetMeta.MetaExt.Length)
                return;

            var file = metaFile.Substring(0, metaFile.Length - IO.IAssetMeta.MetaExt.Length);
            var name = IO.TtFileManager.GetRelativePath(state.DirAddress, file);
            if (!CheckExtNameValid(in name))
                return;

            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(state.DirName + name, state.DirType));
            if (ameta == null)
                return;
            if (!CheckAssetMetaConstraint(in name, ameta))
                return;

            var data = new ViewAssetsData()
            {
                Meta = ameta,
                File = file,
                PathName = name,
            };
            mViewAssetsDatas.Add(data);
        }

        void ProcessMetadataScanFile(string metaFile)
        {
            if (string.IsNullOrEmpty(metaFile))
                return;

            var rootType = TtEngine.Instance.FileManager.GetRootDirType(metaFile);
            var root = TtEngine.Instance.FileManager.GetRoot(rootType);
            if (string.IsNullOrEmpty(root))
                return;

            var rPath = IO.TtFileManager.GetRelativePath(root, metaFile);
            RName assetName = null;
            if (rootType == IO.TtFileManager.ERootDir.Game)
                assetName = RName.GetRName(rPath, RName.ERNameType.Game);
            else if (rootType == IO.TtFileManager.ERootDir.Engine)
                assetName = RName.GetRName(rPath, RName.ERNameType.Engine);
            else if (rootType == IO.TtFileManager.ERootDir.Cloud)
                assetName = RName.GetRName(rPath, RName.ERNameType.Cloud);
            else
                return;

            var ameta = new Rtti.TtMetaVersionMeta();
            ameta.TypeStr = Rtti.TtTypeDescGetter<Rtti.TtMetaVersion>.TypeDesc.TypeString;
            ameta.SetAssetName(assetName);
            mViewMetadataDatas.Add(new ViewAssetsData()
            {
                Meta = ameta,
                File = metaFile,
                PathName = rPath,
            });
        }

        bool IsAssetVisible(in ViewAssetsData data)
        {
            if (data.Meta == null)
                return false;

            var filterName = IO.TtFileManager.GetPureName(data.PathName);
            if (!string.IsNullOrEmpty(FilterText) &&
                filterName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            if (mActiveFiltersCount > 0)
            {
                var assetTypeName = data.Meta.GetAssetTypeName();
                if (!mFilterMenus.TryGetValue(assetTypeName, out var filterProxy))
                    return false;

                var filterMenu = filterProxy as UIProxy.MenuItemProxy;
                if (filterMenu == null || !filterMenu.Selected)
                    return false;
            }
            return true;
        }

        bool IsMetadataVisible(in ViewAssetsData data)
        {
            if (data.Meta == null)
                return false;

            var filterName = IO.TtFileManager.GetPureName(data.PathName);
            if (!string.IsNullOrEmpty(FilterText) &&
                filterName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            return true;
        }

        void ApplyAssetSort()
        {
            if (mSortedAssetsColumn == 1)
            {
                mViewAssetsDatas.Sort((a, b) => string.Compare(a.Type, b.Type, StringComparison.OrdinalIgnoreCase));
            }
            else if (mSortedAssetsColumn == 2)
            {
                mViewAssetsDatas.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            }
        }

        void DrawAssetScanStatus()
        {
            var state = mAssetScanState;
            if (state == null)
                return;

            if (!string.IsNullOrEmpty(state.Error))
            {
                DrawBlockStatusChip("Asset scan warning: " + state.Error, UIProxy.StyleConfig.Instance.WarningStringColor);
            }
            else if (!state.ScanResultApplied)
            {
                var processed = state.ProcessedAssetCount + state.ProcessedMetadataCount;
                var enumerated = state.EnumeratedAssetCount + state.EnumeratedMetadataCount;
                DrawBlockStatusChip($"Scanning assets {processed}/{enumerated}+", UIProxy.StyleConfig.Instance.LinkStringColor);
            }
            else if (!state.IsComplete)
            {
                var current = state.ProcessedAssetCount + state.ProcessedMetadataCount;
                var total = state.EnumeratedAssetCount + state.EnumeratedMetadataCount;
                DrawBlockStatusChip($"Loading assets {current}/{total}", UIProxy.StyleConfig.Instance.LinkStringColor);
            }
        }

        void DrawBlockStatusChip(string text, uint textColor)
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            DrawInlineChip(drawList, text, textColor, UIProxy.StyleConfig.Instance.ContentBrowserStatusBg);
            ImGuiAPI.Separator();
        }

        void DrawInlineChip(in ImDrawList drawList, string text, uint textColor, uint bgColor)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var style = UIProxy.StyleConfig.Instance;
            var textSize = ImGuiAPI.CalcTextSize(text, false, -1);
            var padding = style.ContentBrowserChipPadding;
            var start = ImGuiAPI.GetCursorScreenPos();
            var size = textSize + padding * 2.0f;
            var end = start + size;
            drawList.AddRectFilled(in start, in end, bgColor, 4.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            drawList.AddRect(in start, in end, style.ContentBrowserStatusBorder, 4.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);
            var textPos = start + padding;
            drawList.AddText(in textPos, textColor, text, null);
            ImGuiAPI.Dummy(in size);
        }

        void DrawAssetPager(int shownCount, bool hasNext)
        {
            if (mAssetPageIndex <= 0 && !hasNext)
                return;

            if (mAssetPageIndex > 0)
            {
                if (ImGuiAPI.Button("Prev##ContentBrowserAssetPage", in Vector2.Zero))
                    mAssetPageIndex--;
            }
            else
            {
                ImGuiAPI.Text("Prev");
            }
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.Text($"Page {mAssetPageIndex + 1}  {shownCount}/{AssetPageSize}");
            ImGuiAPI.SameLine(0, -1);
            if (hasNext)
            {
                if (ImGuiAPI.Button("Next##ContentBrowserAssetPage", in Vector2.Zero))
                    mAssetPageIndex++;
            }
            else
            {
                ImGuiAPI.Text("Next");
            }
        }

        bool ShouldDrawPagedAsset(bool isVisible, ref int visibleIndex, ref int shownCount, ref bool hasNext)
        {
            if (!isVisible)
                return false;

            var pageStart = mAssetPageIndex * AssetPageSize;
            if (visibleIndex < pageStart)
            {
                visibleIndex++;
                return false;
            }

            if (shownCount >= AssetPageSize)
            {
                hasNext = true;
                return false;
            }

            visibleIndex++;
            shownCount++;
            return true;
        }

        public unsafe void DrawFileWithColumn(RName dir, in Vector2 size)
        {
            if(ImGuiAPI.BeginChild("ColumnTT", Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                UpdateAssetScanState(dir);
                DrawAssetScanStatus();
                int pagedVisibleIndex = 0;
                int pagedShownCount = 0;
                bool pagedHasNext = false;

                Vector2 tableSize = Vector2.Zero;
                var tableFlags = ImGuiTableFlags_.ImGuiTableFlags_BordersInner |
                    ImGuiTableFlags_.ImGuiTableFlags_BordersOuterH |
                    ImGuiTableFlags_.ImGuiTableFlags_RowBg |
                    ImGuiTableFlags_.ImGuiTableFlags_Resizable |
                    ImGuiTableFlags_.ImGuiTableFlags_Reorderable |
                    ImGuiTableFlags_.ImGuiTableFlags_SizingStretchProp |
                    ImGuiTableFlags_.ImGuiTableFlags_ScrollY;
                if(ImGuiAPI.BeginTable("AssetColumns", 3, tableFlags, in tableSize, 0.0f))
                {
                    ImGuiAPI.TableSetupScrollFreeze(0, 1);
                    ImGuiAPI.TableSetupColumn("Icon", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_WidthFixed, 28, 0);
                    ImGuiAPI.TableSetupColumn("Type", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);
                    ImGuiAPI.TableSetupColumn("Name", ImGuiTableColumnFlags_.ImGuiTableColumnFlags_None, 0, 0);

                    ImGuiAPI.TableHeadersRow();
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.Selectable("Icon", mSortedAssetsColumn == 0, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, Vector2.Zero);
                    ImGuiAPI.TableSetColumnIndex(1);
                    if(ImGuiAPI.Selectable("Type", mSortedAssetsColumn == 1, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, Vector2.Zero))
                    {
                        mSortedAssetsColumn = 1;
                        ApplyAssetSort();
                    }
                    ImGuiAPI.TableSetColumnIndex(2);
                    if (ImGuiAPI.Selectable("Name", mSortedAssetsColumn == 2, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, Vector2.Zero))
                    {
                        mSortedAssetsColumn = 2;
                        ApplyAssetSort();
                    }

                    Vector2 tableMin = Vector2.Zero; 
                    Vector2 tableMax = Vector2.Zero;
                    ImGuiAPI.GetTableWorkRect(ref tableMin, ref tableMax);
                    var cmdList = ImGuiAPI.GetWindowDrawList();
                    int visibleIndex = 0;
                    for(int i=0; i<mViewAssetsDatas.Count; i++)
                    {
                        var data = mViewAssetsDatas[i];
                        var ameta = data.Meta;
                        using (var idHolder = new ImguiIDHolder(data.File))
                        {
                            if (!ShouldDrawPagedAsset(IsAssetVisible(in data), ref pagedVisibleIndex, ref pagedShownCount, ref pagedHasNext))
                                continue;

                            GlobalFocusAssetProcess(ameta);

                            ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0.0f);
                            ImGuiAPI.TableNextColumn();


                            ImGuiAPI.TableNextColumn();
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.TVHeader);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.TVHeaderActive);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered);
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, ameta.GetBorderColor().ToR8G8B8A8());
                            var selectItemResult = ImGuiAPI.Selectable(data.Type, ameta.IsSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_SpanAllColumns, Vector2.Zero);
                            ImGuiAPI.PopStyleColor(4);
                            AssetItemOperation(ameta, visibleIndex++);
                            if (ImGuiAPI.IsItemVisible())
                            {
                                DragDropOperation(ameta, ImGuiAPI.GetItemRectSize(), 1.0f);
                                //{
                                //    ameta.IsSelected = !ameta.IsSelected;
                                //}
                            }
                            ImGuiAPI.TableNextColumn();
                            ImGuiAPI.Text(data.Name);

                            ImGuiAPI.TableSetColumnIndex(0);
                            float startY = 0.0f, endY = 0.0f;
                            ImGuiAPI.GetTableRowStartY(ref startY);
                            ImGuiAPI.GetTableRowEndY(ref endY);
                            var snapStart = new Vector2(tableMin.X, startY);
                            var snapEnd = new Vector2(tableMin.X + (endY - startY), startY + (endY - startY));
                            ameta.OnDrawSnapshotForContentBrowser(cmdList, ref snapStart, ref snapEnd, AutoGenerateSnapshots);
                        }
                    }

                    ImGuiAPI.EndTable();
                }
                DrawAssetPager(pagedShownCount, pagedHasNext);
            }
            ImGuiAPI.EndChild();
        }
        public unsafe void DrawFiles(RName dir, in Vector2 size)
        {
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var itemSize = new Vector2(100, 150);

            var imViewPort = ImGuiAPI.GetWindowViewport();
            var dpiScale = imViewPort->DpiScale;
            itemSize *= dpiScale;

            CreateNewAssets = true;

            //var cldPos = ImGuiAPI.GetWindowPos();
            //var cldMin = ImGuiAPI.GetWindowContentRegionMin();
            //var cldMax = ImGuiAPI.GetWindowContentRegionMax();
            //cldMin += cldPos;
            //cldMax += cldPos;
            //////cldMin.Y += 30;
            //cmdlist.PushClipRect(in cldMin, in cldMax, true);
            var style = ImGuiAPI.GetStyle();
            var width = ImGuiAPI.GetWindowContentRegionWidth();
            UpdateAssetScanState(dir);

            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, new Vector2(8, 8));
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, 0x00000000);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, 0x00000000);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, 0x00000000);
            DrawAssetScanStatus();

            float curPos = 0;
            int drawIndex = 0;
            int pagedVisibleIndex = 0;
            int pagedShownCount = 0;
            bool pagedHasNext = false;
            for (int i = 0; i < mViewAssetsDatas.Count; i++)
            {
                var data = mViewAssetsDatas[i];
                if (!ShouldDrawPagedAsset(IsAssetVisible(in data), ref pagedVisibleIndex, ref pagedShownCount, ref pagedHasNext))
                    continue;

                DrawItem(in cmdlist, data.Meta.Icon, data.Meta, in itemSize, drawIndex++, ItemScale);
                curPos += itemSize.X + style->ItemSpacing.X;
                if (curPos + itemSize.X < width)
                {
                    ImGuiAPI.SameLine(0, style->ItemSpacing.X);
                }
                else
                {
                    curPos = 0;
                }
            }

            for (int i = 0; i < mViewMetadataDatas.Count; i++)
            {
                var data = mViewMetadataDatas[i];
                if (!ShouldDrawPagedAsset(IsMetadataVisible(in data), ref pagedVisibleIndex, ref pagedShownCount, ref pagedHasNext))
                    continue;

                DrawItem(in cmdlist, data.Meta.Icon, data.Meta, in itemSize, drawIndex++, ItemScale);
                curPos += itemSize.X + style->ItemSpacing.X;
                if (curPos + itemSize.X < width)
                {
                    ImGuiAPI.SameLine(0, style->ItemSpacing.X);
                }
                else
                {
                    curPos = 0;
                }
            }
            DrawAssetPager(pagedShownCount, pagedHasNext);
            ImGuiAPI.PopStyleVar(1);
            ImGuiAPI.PopStyleColor(3);
            //cmdlist.PopClipRect();


            ////////////////////////////////////////////////////////////
            //drawList.AddRect(ref min, ref max, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
            ////////////////////////////////////////////////////////////

            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, UIProxy.StyleConfig.Instance.PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, UIProxy.StyleConfig.Instance.PopupItemSpacing);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, UIProxy.StyleConfig.Instance.BorderColor);
            if (CreateNewAssets && ImGuiAPI.BeginPopupContextWindow("##ContentFilesMenuWindow", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                var popMenuWidth = ImGuiAPI.GetWindowContentRegionWidth();
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mContextMenuFilterFocused, in drawList, "search items", ref mContextMenuFilterStr, popMenuWidth);
                var wsize = new Vector2(200, 400);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PopupColor);
                if (ImGuiAPI.BeginChild("GraphContextMenu", in wsize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
                {
                    for (var childIdx = 0; childIdx < mContextMenu.SubMenuItems.Count; childIdx++)
                    {
                        TtMenuItem.Draw(mContextMenu.SubMenuItems[childIdx], this, this, mContextMenuFilterStr, in drawList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, null);
                    }
                }
                ImGuiAPI.EndChild();
                ImGuiAPI.PopStyleColor(1);
                ImGuiAPI.EndPopup();

                mContextMenuOpenCheck = true;
            }
            else
            {
                mContextMenuOpenCheck = false;
            }
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(2);
            if(mContextMenuOpenCheck && !mOldContextMenuOpenCheck)
            {
                mContextMenuFilterStr = "";
            }
            mOldContextMenuOpenCheck = mContextMenuOpenCheck;
        }
        public struct DragDropData
        {
            public IO.IAssetMeta[] Metas;
        }

        public static bool IsInDragDropMode = false;
        int FirstClickIndex = -1;
        int LastClickIndex = -1;
        RName LastDir;
        private void GlobalFocusAssetProcess(IO.IAssetMeta ameta)
        {
            if (GlobalFocusAsset != null && GlobalFocusAsset == ameta.GetAssetName())
            {
                ImGuiAPI.SetScrollHereX(0.5f);
                ImGuiAPI.SetScrollHereY(0.5f);
                GlobalSelectedAsset = ameta;
                for (var i = 0; i < SelectedAssets.Count; i++)
                {
                    SelectedAssets[i].IsSelected = false;
                }
                SelectedAssets.Clear();
                SelectedAssets.Add(ameta);
                ameta.IsSelected = true;
                GlobalFocusAsset = null;
            }
        }
        private void AssetItemOperation(IO.IAssetMeta ameta, int index)
        {
            //if(ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_DockHierarchy))
            {
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    ameta.DrawTooltip();
                    //CtrlUtility.DrawHelper(ameta.GetAssetName().Name, ameta.Description);
                    //if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 8))
                    //{
                    //    if (ItemDragging.GetCurItem() == null)
                    //    {
                    //        var curItem = new UItemDragging.UItem();
                    //        curItem.Tag = ameta;
                    //        curItem.AMeta = ameta;
                    //        curItem.Size = sz;
                    //        curItem.Browser = this;
                    //        ItemDragging.SetCurItem(curItem, () =>
                    //        {
                    //            curItem.AMeta.OnDragTo(TtEngine.Instance.ViewportSlateManager.GetPressedViewport());
                    //            return;
                    //        });
                    //    }
                    //}
                    if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        var type = Rtti.TtTypeDesc.TypeOf(ameta.TypeStr).SystemType;
                        if (type != null)
                        {
                            var attrs = type.GetCustomAttributes(typeof(Editor.UAssetEditorAttribute), false);
                            if (attrs.Length > 0)
                            {
                                var editorAttr = attrs[0] as Editor.UAssetEditorAttribute;
                                Editor.TtAssetEditorManager.TryOpenEditor(editorAttr.EditorType, ameta.GetAssetName(), null).AddWaitTask();
                            }
                        }

                        GlobalSelectedAsset = ameta;
                        for (var i = 0; i < SelectedAssets.Count; i++)
                        {
                            SelectedAssets[i].IsSelected = false;
                        }
                        SelectedAssets.Clear();
                        ameta.IsSelected = true;
                        SelectedAssets.Add(ameta);
                        // todo: multi select
                        ItemSelectedAction?.Invoke(ameta);
                    }
                    //}
                    else if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    //if(ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false))
                    //if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        //if(ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_ReservedForModCtrl))
                        if (TtEngine.Instance.InputSystem.IsCtrlKeyDown())
                        {
                            ameta.IsSelected = !ameta.IsSelected;
                            if (ameta.IsSelected)
                            {
                                GlobalSelectedAsset = ameta;
                                SelectedAssets.Add(ameta);
                                ItemSelectedAction?.Invoke(ameta);
                            }
                            else
                            {
                                SelectedAssets.Remove(ameta);
                            }
                        }
                        //else if(ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_ReservedForModShift))
                        else if (TtEngine.Instance.InputSystem.IsShiftKeyDown())
                        {
                            if (FirstClickIndex < 0)
                                FirstClickIndex = 0;
                            LastClickIndex = index;
                        }
                        else
                        {
                            GlobalSelectedAsset = ameta;
                            for (var i = 0; i < SelectedAssets.Count; i++)
                            {
                                SelectedAssets[i].IsSelected = false;
                            }
                            SelectedAssets.Clear();
                            SelectedAssets.Add(ameta);
                            ameta.IsSelected = true;
                            FirstClickIndex = index;
                            LastClickIndex = index;
                            ItemSelectedAction?.Invoke(ameta);
                        }
                    }
                }

                if (mShiftSelection)
                {
                    var min = Math.Min(FirstClickIndex, LastClickIndex);
                    var max = Math.Max(FirstClickIndex, LastClickIndex);
                    if (index >= min && index <= max)
                    {
                        ameta.IsSelected = true;
                        SelectedAssets.Add(ameta);
                    }
                }
            }
        }
        private unsafe void DragDropOperation(IO.IAssetMeta ameta, in Vector2 sz, float scale)
        {
            if (ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_SourceNoDisableHover))
            {
                IsInDragDropMode = true;
                if (!ameta.IsSelected)
                {
                    for (int i = 0; i < SelectedAssets.Count; i++)
                    {
                        SelectedAssets[i].IsSelected = false;
                    }
                    SelectedAssets.Clear();
                    ameta.IsSelected = true;
                    SelectedAssets.Add(ameta);
                }
                var data = new DragDropData();
                data.Metas = new IO.IAssetMeta[SelectedAssets.Count];
                SelectedAssets.CopyTo(data.Metas, 0);
                var handle = GCHandle.Alloc(data);
                ImGuiAPI.SetDragDropPayload("ContentBrowserAssetDragDrop", GCHandle.ToIntPtr(handle).ToPointer(), (uint)Marshal.SizeOf<DragDropData>(), ImGuiCond_.ImGuiCond_None);

                int drawCount = 0;
                for (int i = 0; i < SelectedAssets.Count; i++)
                {
                    if (SelectedAssets[i].CanDrawOnDragging())
                        drawCount++;
                }
                var dragDropCmdlist = ImGuiAPI.GetWindowDrawList();
                var offsetOri = new Vector2(16, 16);
                var offsetDelta = 1.0f;
                if (drawCount > 0)
                {
                    float decreaseDelta = 0.15f;
                    Vector2 offset = Vector2.Zero;
                    int totalCount = 0;
                    for (int i = 0; i < SelectedAssets.Count && offsetDelta > 0; i++)
                    {
                        if (i != 0)
                            offset += offsetOri * offsetDelta;
                        offsetDelta -= decreaseDelta;
                        totalCount = i + 1;
                    }
                    var winSize = sz + offset + new Vector2(0, 30);
                    if (ImGuiAPI.BeginChild("ContentBrowserDrag", in winSize, ImGuiChildFlags_.ImGuiChildFlags_None,
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground))
                    {
                        for (int i = totalCount - 1; i >= 0; i--)
                        {
                            if (i != (totalCount - 1))
                                offset -= offsetOri * offsetDelta;
                            if (!SelectedAssets[i].CanDrawOnDragging())
                            {
                                continue;
                            }
                            offsetDelta += decreaseDelta;
                            SelectedAssets[i].OnDraw(in dragDropCmdlist, offset, in sz, this, scale);
                        }
                        var posY = ImGuiAPI.GetCursorPosY();
                        ImGuiAPI.SetCursorPosY(posY + winSize.Y - 20);
                        ImGuiAPI.Text(SelectedAssets.Count + " items");
                    }
                    ImGuiAPI.EndChild();
                }

                ImGuiAPI.EndDragDropSource();
            }
        }
        private unsafe void DrawItem(in ImDrawList cmdlist, TtUVAnim icon, IO.IAssetMeta ameta, in Vector2 sz, int index, float scale)
        {
            ImGuiAPI.PushID($"##{ameta.GetAssetName().Name}");
            bool isSelected = false;
            ImGuiAPI.Selectable("", ref isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz);
            var itemMin = ImGuiAPI.GetItemRectMin();
            var itemMax = ImGuiAPI.GetItemRectMax();
            var isHovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            DrawAssetTileBackground(in cmdlist, in itemMin, in itemMax, ameta.IsSelected, isHovered);
            GlobalFocusAssetProcess(ameta);
            AssetItemOperation(ameta, index);
            if (ImGuiAPI.IsItemVisible())
            {
                ameta.ShowIconTime = TtEngine.Instance.CurrentTickCountUS;
                ameta.OnDraw(in cmdlist, in sz, this, scale);

                DragDropOperation(ameta, in sz, scale);
            }
            ImGuiAPI.PopID();
        }

        void DrawAssetTileBackground(in ImDrawList cmdlist, in Vector2 itemMin, in Vector2 itemMax, bool selected, bool hovered)
        {
            var style = UIProxy.StyleConfig.Instance;
            var bgColor = style.ContentBrowserAssetTileBg;
            var borderColor = style.ContentBrowserAssetTileBorder;
            var borderThickness = 1.0f;
            if (selected)
            {
                bgColor = style.ContentBrowserAssetTileSelectedBg;
                borderColor = style.ContentBrowserAssetTileSelectedBorder;
                borderThickness = 1.5f;
            }
            else if (hovered)
            {
                bgColor = style.ContentBrowserAssetTileHoveredBg;
                borderColor = style.BorderActiveColor;
            }

            var rounding = style.ContentBrowserTileRounding;
            cmdlist.AddRectFilled(in itemMin, in itemMax, bgColor, rounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            cmdlist.AddRect(in itemMin, in itemMax, borderColor, rounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, borderThickness);
            if (hovered || selected)
            {
                var accentEnd = new Vector2(itemMax.X, itemMin.Y + 2.0f);
                cmdlist.AddRectFilled(in itemMin, in accentEnd, selected ? style.AccentHoveredColor : style.BorderActiveColor, rounding, ImDrawFlags_.ImDrawFlags_RoundCornersTop);
            }
        }

        enum EViewType
        {
            Tiles,
            Columns,
        }
        EViewType mViewType = EViewType.Tiles;

        Dictionary<string, UIProxy.IUIProxyBase> mFilterMenus = new Dictionary<string, UIProxy.IUIProxyBase>();
        int mActiveFiltersCount = 0;
        bool mWithChildFolders = false;
        void InitializeFilterMenu()
        {
            mFilterMenus.Clear();
            mFilterMenus["##View Type"] = new UIProxy.NamedMenuSeparator()
            {
                Name = "View Type"
            };
            mFilterMenus["##Tiles"] = new UIProxy.MenuItemProxy()
            {
                MenuName = "Tiles",
                Selected = (mViewType == EViewType.Tiles),
                Action = (item, data) =>
                {
                    mViewType = EViewType.Tiles;
                    ((UIProxy.MenuItemProxy)mFilterMenus["##Columns"]).Selected = false;
                    item.Selected = true;
                }
            };
            mFilterMenus["##Columns"] = new UIProxy.MenuItemProxy()
            {
                MenuName = "Columns",
                Selected = (mViewType == EViewType.Columns),
                Action = (item, data) =>
                {
                    mViewType = EViewType.Columns;
                    ((UIProxy.MenuItemProxy)mFilterMenus["##Tiles"]).Selected = false;
                    item.Selected = true;
                }
            };
            mFilterMenus["##filter separate"] = new UIProxy.NamedMenuSeparator()
            {
                Name = "Filter"
            };
            mFilterMenus["##null"] = new UIProxy.MenuItemProxy()
            {
                MenuName = "Clear Filters",
                Action = (item, data)=>
                {
                    foreach(var menuItem in mFilterMenus)
                    {
                        if (menuItem.Key.Contains("##"))
                            continue;
                        var menu = menuItem.Value as EGui.UIProxy.MenuItemProxy;
                        if(menu != null)
                            menu.Selected = false;
                    }
                    mActiveFiltersCount = 0;
                    mAssetPageIndex = 0;
                }
            };
            mFilterMenus["##show child"] = new UIProxy.MenuItemProxy()
            {
                MenuName = "With child folders",
                Action = (item, data)=>
                {
                    mWithChildFolders = !mWithChildFolders;
                    item.Selected = mWithChildFolders;
                    InvalidateAssetScan();
                }
            };
            mFilterMenus["##sep0"] = new UIProxy.NamedMenuSeparator()
            {
                Name = "Asset Types",
            };
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach(var typeDesc in service.Types.Values)
                {
                    if(typeDesc.IsSubclassOf(typeof(IO.IAssetMeta)))
                    {
                        var inst = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as IO.IAssetMeta;
                        var name = inst.GetAssetTypeName();
                        var menu = new UIProxy.MenuItemProxy()
                        {
                            MenuName = name,
                            Action = (item, data) =>
                            {
                                item.Selected = !item.Selected;
                                if (item.Selected)
                                    mActiveFiltersCount++;
                                else
                                    mActiveFiltersCount--;
                                mAssetPageIndex = 0;
                            },
                        };
                        mFilterMenus[name] = menu;
                    }
                }
            }
        }
        void DrawFilterMenu()
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, UIProxy.StyleConfig.Instance.PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, UIProxy.StyleConfig.Instance.PopupItemSpacing);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, UIProxy.StyleConfig.Instance.BorderColor);
            if (ImGuiAPI.BeginPopup("AssetFilterMenus", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                if(mFilterMenus != null)
                {
                    var menuData = new Support.TtAnyPointer();
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    foreach(var menu in mFilterMenus.Values)
                    {
                        menu.OnDraw(in drawList, menuData);
                    }
                }
                ImGuiAPI.EndPopup();
            }
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(2);
        }
        bool mIsPreFolderMouseDown = false;
        bool mIsPreFolderMouseHover = false;
        bool mIsNextFolderMouseDown = false;
        bool mIsNextFolderMouseHover = false;
        bool mIsFilterMouseDown = false;
        bool mIsFilterMouseHover = false;
        bool[] mIsDirMouseDown = new bool[256];
        bool[] mIsDirMouseHover = new bool[256];
        bool[] mIsDirSplitMouseDown = new bool[256];
        bool[] mIsDirSplitMouseHover = new bool[256];
        void DrawToolbar(in ImDrawList cmd)
        {
            EGui.UIProxy.Toolbar.BeginToolbar(cmd);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(cmd,
                ref mIsPreFolderMouseDown,
                ref mIsPreFolderMouseHover,
                TtEngine.Instance.UIProxyManager[FolderView.PreFolderImgName] as EGui.UIProxy.ImageProxy,
                "", mFolderView.IsPreFolderDisable))
            {
                mFolderView.CurrentDirHistoryIdx--;
                if (mFolderView.CurrentDirHistoryIdx <= 0)
                {
                    mFolderView.IsPreFolderDisable = true;
                    mFolderView.CurrentDirHistoryIdx = 0;
                }
                mFolderView.CurrentDir = mFolderView.DirHistory[mFolderView.CurrentDirHistoryIdx];
                if (mFolderView.CurrentDirHistoryIdx < mFolderView.DirHistory.Count - 1)
                    mFolderView.IsNextFolderDisable = false;
            }
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(cmd,
                ref mIsNextFolderMouseDown,
                ref mIsNextFolderMouseHover,
                TtEngine.Instance.UIProxyManager[FolderView.NextFolderImgName] as EGui.UIProxy.ImageProxy,
                "", mFolderView.IsNextFolderDisable))
            {
                mFolderView.CurrentDirHistoryIdx++;
                if (mFolderView.CurrentDirHistoryIdx >= mFolderView.DirHistory.Count - 1)
                {
                    mFolderView.CurrentDirHistoryIdx = mFolderView.DirHistory.Count - 1;
                    mFolderView.IsNextFolderDisable = true;
                }
                mFolderView.CurrentDir = mFolderView.DirHistory[mFolderView.CurrentDirHistoryIdx];
                if (mFolderView.CurrentDirHistoryIdx > 0)
                    mFolderView.IsPreFolderDisable = false;
            }
            if (mFolderView.CurrentDir != null)
            {
                if(LastDir != mFolderView.CurrentDir)
                {
                    FirstClickIndex = -1;
                    LastClickIndex = -1;
                    LastDir = mFolderView.CurrentDir;
                }
                var root = RName.GetRName("", mFolderView.CurrentDir.RNameType);
                var dirName = IO.TtFileManager.GetRelativePath(root.Address, mFolderView.CurrentDir.Address).TrimEnd('/');
                dirName = mFolderView.CurrentDir.RNameType + "/" + dirName;
                var dirSplits = dirName.Split('/');
                for (int i = 0; i < dirSplits.Length; i++)
                {
                    if (i >= mIsDirMouseDown.Length)
                        break;
                    if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(cmd,
                        ref mIsDirMouseDown[i], ref mIsDirMouseHover[i], null, dirSplits[i]))
                    {
                        if (i != dirSplits.Length - 1)
                        {
                            if (i == 0)
                                mFolderView.PushHistory(root);
                            else
                            {
                                string tempDir = "";
                                for (int j = 0; j <= i - 1; j++)
                                    tempDir += dirSplits[j + 1] + "/";
                                tempDir.TrimEnd('/');
                                mFolderView.PushHistory(RName.GetRName(tempDir, mFolderView.CurrentDir.RNameType));
                            }
                        }
                    }
                    if (i < dirSplits.Length - 1)
                    {
                        if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(cmd,
                            ref mIsDirSplitMouseDown[i], ref mIsDirSplitMouseHover[i], null, ">"))
                        {

                        }
                    }
                }
            }
            DrawToolbarSummary(cmd);
            EGui.UIProxy.Toolbar.EndToolbar();
        }

        void DrawToolbarSummary(in ImDrawList cmd)
        {
            var loadedCount = mViewAssetsDatas.Count + mViewMetadataDatas.Count;
            var selectedCount = SelectedAssets.Count;
            ImGuiAPI.SameLine(0, 12);
            DrawInlineChip(cmd, $"{loadedCount} assets", UIProxy.StyleConfig.Instance.TextDisableColor, UIProxy.StyleConfig.Instance.ContentBrowserStatusBg);
            if (selectedCount > 0)
            {
                ImGuiAPI.SameLine(0, 6);
                DrawInlineChip(cmd, $"{selectedCount} selected", UIProxy.StyleConfig.Instance.AccentHoveredColor, UIProxy.StyleConfig.Instance.ContentBrowserAssetTileSelectedBg);
            }
            if (mActiveFiltersCount > 0)
            {
                ImGuiAPI.SameLine(0, 6);
                DrawInlineChip(cmd, $"{mActiveFiltersCount} filters", UIProxy.StyleConfig.Instance.WarningStringColor, UIProxy.StyleConfig.Instance.ContentBrowserStatusBg);
            }
            if (mWithChildFolders)
            {
                ImGuiAPI.SameLine(0, 6);
                DrawInlineChip(cmd, "recursive", UIProxy.StyleConfig.Instance.LinkStringColor, UIProxy.StyleConfig.Instance.ContentBrowserStatusBg);
            }
        }

        FolderView mFolderView = new FolderView();
        public UItemDragging ItemDragging = new UItemDragging();
        Vector2 RightSize;
        struct stAssetImporter
        {
            public IO.IAssetCreateAttribute Creater;
            public string FileName;
        }
        Queue<stAssetImporter> mAssetImporterQueue = new Queue<stAssetImporter>();
        public void EnqueueAssetImporter(IO.IAssetCreateAttribute creater, string fileName)
        {
            var importer = new stAssetImporter()
            {
                Creater = creater,
                FileName = fileName,
            };
            mAssetImporterQueue.Enqueue(importer);
        }
        void TryHandleDroppedSourceFiles()
        {
            if (!TtEngine.Instance.InputSystem.IsDropFiles)
                return;
            if (mFolderView.CurrentDir == null)
                return;

            List<Rtti.TtTypeDesc> importers = new List<Rtti.TtTypeDesc>();
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var attrs = typeDesc.GetCustomAttributes(typeof(IO.IAssetCreateAttribute), true);
                    if (attrs.Length > 0)
                    {
                        //var importer = attrs[0] as IO.IAssetCreateAttribute;
                        //importers.Add(importer);
                        importers.Add(typeDesc);
                    }
                }
            }

            var acceptedCount = 0;
            var rejectedFiles = new List<string>();
            for (int i = 0; i < TtEngine.Instance.InputSystem.DropFiles.Count; i++)
            {
                var file = TtEngine.Instance.InputSystem.DropFiles[i];
                var fileExt = IO.TtFileManager.GetExtName(file);
                var matched = false;
                for (int j = 0; j < importers.Count; j++)
                {
                    try
                    {
                        var attrs = importers[j].GetCustomAttributes(typeof(IO.IAssetCreateAttribute), true);
                        if (((IO.IAssetCreateAttribute)(attrs[0])).IsAssetSource(fileExt))
                        {
                            var assetExtField = Rtti.TtTypeDesc.GetField(importers[j].SystemType, "AssetExt");
                            if (assetExtField != null)
                            {
                                var creater = TtEngine.Instance.AssetMetaManager.ImportAsset(mFolderView.CurrentDir, importers[j], (string)assetExtField.GetValue(null));
                                if (creater != null)
                                {
                                    EnqueueAssetImporter(creater, file);
                                    acceptedCount++;
                                    matched = true;
                                }
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
                if (!matched)
                {
                    rejectedFiles.Add(file);
                }
            }
            if (acceptedCount > 0)
            {
                SetImportStatusMessage($"Queued {acceptedCount} dropped file(s) for import.");
            }
            else if (rejectedFiles.Count > 0)
            {
                SetImportStatusMessage($"No importer found for dropped file(s): {string.Join(", ", rejectedFiles)}", true);
            }
            TtEngine.Instance.InputSystem.ClearFilesDrop();
        }
        IO.IAssetCreateAttribute mAssetImporter;
        public string CurrentImporterFile;
        string mDropImportMessage = "";
        Vector4 mDropImportMessageColor = new Vector4(1, 1, 0, 1);
        public bool DrawInWindow = true;
        RName mCurrentDir;

        public void SetImportStatusMessage(string message, bool isError = false, bool isSuccess = false)
        {
            mDropImportMessage = message ?? "";
            var style = UIProxy.StyleConfig.Instance;
            if (isError)
                mDropImportMessageColor = ToColor4(style.ErrorStringColor);
            else if (isSuccess)
                mDropImportMessageColor = ToColor4(style.PassStringColor);
            else
                mDropImportMessageColor = ToColor4(style.WarningStringColor);
        }

        static Vector4 ToColor4(uint abgr)
        {
            const float inv = 1.0f / 255.0f;
            return new Vector4(
                (abgr & 0xFF) * inv,
                ((abgr >> 8) & 0xFF) * inv,
                ((abgr >> 16) & 0xFF) * inv,
                ((abgr >> 24) & 0xFF) * inv);
        }

        bool IsAssetOprating = false;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            //            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var name = Name;
            if (string.IsNullOrEmpty(name))
                name = "ContentBrowser";
            bool draw = true;
            if (DrawInWindow)
            {
                ImGuiAPI.SetNextWindowSize(new Vector2(800, 300), ImGuiCond_.ImGuiCond_FirstUseEver);
                draw = EGui.UIProxy.DockProxy.BeginMainForm(name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar);
            }
            else
            {
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, UIProxy.StyleConfig.Instance.PanelFramePadding);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, UIProxy.StyleConfig.Instance.PanelBackground);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            }
            if (draw)
            {
                if(mCurrentDir != mFolderView.CurrentDir)
                {
                    ResetAssetScan();
                    mCurrentDir = mFolderView.CurrentDir;
                }

                var cmd = ImGuiAPI.GetWindowDrawList();
                var style = ImGuiAPI.GetStyle();
                DrawToolbar(cmd);

                //                if (ImGuiAPI.IsWindowDocked())
                //                {
                //                    DockId = ImGuiAPI.GetWindowDockID();
                //                }
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Separator, UIProxy.StyleConfig.Instance.BorderColor);
                ImGuiAPI.Columns(2, null, true);

                if (mFolderView.ContentSize.X <= 0)
                {
                    var cltSize = ImGuiAPI.GetWindowContentRegionWidth();
                    ImGuiAPI.SetColumnWidth(0, ((float)cltSize) * 0.3f);
                }

                //bool open = true;
                if(GlobalFocusAsset != null)
                {
                    mFolderView.CurrentDir = GlobalFocusAsset.GetDirectoryRName();
                }
                mFolderView.Draw(in Vector2.MinusOne);
                ImGuiAPI.NextColumn();

                if (mFolderView.CurrentDir != null)
                {
                    //var cmdlist = ImGuiAPI.GetWindowDrawList();
                    if (mSearchBar != null)
                    {
                        mSearchBar.Width = ImGuiAPI.GetColumnWidth(1) - (style->WindowPadding.X) * 2 - 24;
                        if (mSearchBar.OnDraw(in cmd, in Support.TtAnyPointer.Default))
                        {
                            if (FilterText != mSearchBar.SearchText)
                                mAssetPageIndex = 0;
                            FilterText = mSearchBar.SearchText;
                        }
                    }
                    else
                    {
                        SureSearchBar();
                    }
                    var frameHeight = ImGuiAPI.GetFrameHeight() + style->FramePadding.Y;
                    if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(cmd,
                        ref mIsFilterMouseDown,
                        ref mIsFilterMouseHover,
                        TtEngine.Instance.UIProxyManager[FilterImgName] as EGui.UIProxy.ImageProxy,
                        "", false, frameHeight))
                    {
                        ImGuiAPI.OpenPopup("AssetFilterMenus", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                    }
                    if(mActiveFiltersCount > 0)
                    {
                        var pos = ImGuiAPI.GetItemRectMin() + new Vector2(16, 20);
                        cmd.AddCircleFilled(pos, 5, 0xff0000ff, 16);
                    }
                    DrawFilterMenu();

                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, new Vector2(8, 8));
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ChildRounding, 3.0f);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.ContentBrowserAssetPaneBg);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, UIProxy.StyleConfig.Instance.BorderColor);
                    if (ImGuiAPI.BeginChild("RightWindow", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                    {
                        var min = ImGuiAPI.GetWindowContentRegionMin();
                        var max = ImGuiAPI.GetWindowContentRegionMax();
                        RightSize = max - min;

                        TryHandleDroppedSourceFiles();

                        if (!string.IsNullOrEmpty(mDropImportMessage))
                        {
                            var clr = ImGuiAPI.ColorConvertFloat4ToU32(in mDropImportMessageColor);
                            DrawBlockStatusChip(mDropImportMessage, clr);
                        }

                        if (FirstClickIndex != LastClickIndex)
                        {
                            for (int i = 0; i < SelectedAssets.Count; i++)
                            {
                                SelectedAssets[i].IsSelected = false;
                            }
                            SelectedAssets.Clear();
                            mShiftSelection = true;
                        }

                        switch (mViewType)
                        {
                            case EViewType.Tiles:
                                DrawFiles(mFolderView.CurrentDir, in RightSize);
                                break;
                            case EViewType.Columns:
                                DrawFileWithColumn(mFolderView.CurrentDir, in RightSize);
                                break;
                        }

                        if (mShiftSelection)
                        {
                            LastClickIndex = FirstClickIndex;
                            mShiftSelection = false;
                        }
                    }
                    ImGuiAPI.EndChild();
                    ImGuiAPI.PopStyleColor(2);
                    ImGuiAPI.PopStyleVar(2);
                }
                    
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
                ImGuiAPI.PopStyleColor(1);
                DrawAssetOperationDialog();
            }
            if (DrawInWindow)
                EGui.UIProxy.DockProxy.EndMainForm(draw);
            else
            {
                ImGuiAPI.PopStyleVar(1);
                ImGuiAPI.PopStyleColor(2);
            }

            if (mAssetImporter != null)
            {
                if(mAssetImporter.OnDraw(this))
                {
                    ResetAssetScan();
                    mAssetImporter = null;
                    CurrentImporterFile = "";
                }
            }
            else if(mAssetImporterQueue.Count > 0)
            {
                var importer = mAssetImporterQueue.Dequeue();
                mAssetImporter = importer.Creater;
                CurrentImporterFile = importer.FileName;
                SetImportStatusMessage($"Importing {IO.TtFileManager.GetPureName(importer.FileName)}...");
            }

        }

        public enum EAssetOperationType
        {
            None,
            Delete,
            MoveTo,
            CopyTo,
            Rename,
            PackTo,
        }
        EAssetOperationType mOperationType = EAssetOperationType.None;
        IAssetMeta mOperationAsset = null;
        readonly List<IAssetMeta> mOperationAssets = new List<IAssetMeta>();
        FolderView mSelectFolderView = new FolderView();
        float mSelectFolderOKButtonHeight = 20;
        public void OperationAsset(IAssetMeta sourceAsset, EAssetOperationType opType)
        {
            mOperationAsset = sourceAsset;
            mOperationType = opType;
            mOperationAssets.Clear();
            if (mOperationAsset == null)
                return;

            NewName = mOperationAsset.GetAssetName().PureName;
            if (mOperationType == EAssetOperationType.Delete)
            {
                CollectDeleteOperationAssets(mOperationAsset);
            }
        }

        void CollectDeleteOperationAssets(IAssetMeta sourceAsset)
        {
            if (sourceAsset == null)
                return;

            var sourceSelected = IsAssetSelected(sourceAsset);
            if (sourceSelected)
            {
                for (int i = 0; i < SelectedAssets.Count; i++)
                {
                    AddOperationAsset(SelectedAssets[i]);
                }
            }
            AddOperationAsset(sourceAsset);
        }

        bool IsAssetSelected(IAssetMeta asset)
        {
            if (asset == null)
                return false;
            if (asset.IsSelected)
                return true;

            var assetName = asset.GetAssetName();
            for (int i = 0; i < SelectedAssets.Count; i++)
            {
                var selectedAsset = SelectedAssets[i];
                if (selectedAsset == asset || IsSameAssetName(selectedAsset?.GetAssetName(), assetName))
                    return true;
            }
            return false;
        }

        void AddOperationAsset(IAssetMeta asset)
        {
            if (asset == null)
                return;

            var assetName = asset.GetAssetName();
            if (assetName == null)
                return;

            for (int i = 0; i < mOperationAssets.Count; i++)
            {
                var existAsset = mOperationAssets[i];
                if (existAsset == asset || IsSameAssetName(existAsset?.GetAssetName(), assetName))
                    return;
            }
            mOperationAssets.Add(asset);
        }

        string NewName;
        unsafe void DrawAssetOperationDialog()
        {
            if (mOperationAsset == null || mOperationType == EAssetOperationType.None)
                return;

            var sourceName = mOperationAsset.GetAssetName();
            if (mOperationType == EAssetOperationType.Delete)
            {
                DrawDeleteAssetDialog(sourceName);
                return;
            }

            string keyName = "AssetOperation";
            switch (mOperationType)
            {
                case EAssetOperationType.MoveTo:
                    keyName = $"Move {sourceName.Name} To";
                    break;
                case EAssetOperationType.CopyTo:
                    keyName = $"Copy {sourceName.Name} To";
                    break;
                case EAssetOperationType.PackTo:
                    keyName = $"Copy {sourceName.Name} To";
                    break;
            }

            ImGuiAPI.OpenPopup(keyName, ImGuiPopupFlags_.ImGuiPopupFlags_None | ImGuiPopupFlags_.ImGuiPopupFlags_NoOpenOverExistingPopup);

            var pos = ImGuiAPI.GetWindowPos();
            var min = ImGuiAPI.GetWindowContentRegionMin();
            var max = ImGuiAPI.GetWindowContentRegionMax();
            var pivot = new Vector2(0.5f, 0.5f);
            ImGuiAPI.SetNextWindowPos((min + max) * 0.5f + pos, ImGuiCond_.ImGuiCond_Appearing, in pivot);
            ImGuiAPI.SetNextWindowSize(new Vector2(300, 500), ImGuiCond_.ImGuiCond_Appearing);
            UIProxy.StyleConfig.Instance.PushPopupStyle();
            if(ImGuiAPI.BeginPopupModal(keyName, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                UIProxy.StyleConfig.Instance.PushPanelChildStyle(UIProxy.StyleConfig.Instance.PanelBackground);
                if(ImGuiAPI.BeginChild(keyName + "CWin", in Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
                {
                    var winSize = new Vector2(ImGuiAPI.GetWindowWidth(), ImGuiAPI.GetWindowHeight() - mSelectFolderOKButtonHeight);
                    if (mOperationType == EAssetOperationType.MoveTo ||
                        mOperationType == EAssetOperationType.CopyTo ||
                        mOperationType == EAssetOperationType.PackTo)
                    {
                        mSelectFolderView.Draw(in winSize);
                    }
                    else if(mOperationType == EAssetOperationType.Rename)
                    {
                        if (ImGuiAPI.InputText("NewName", ref NewName))
                        {

                        }
                    }

                    if (ImGuiAPI.Button("OK", in Vector2.Zero))
                    {
                        try
                        {
                            if (IsAssetOprating == false)
                            {
                                switch(mOperationType)
                                {
                                    case EAssetOperationType.MoveTo:
                                        {
                                            var name = mSelectFolderView.CurrentDir.Name + sourceName.PureName + sourceName.ExtName;
                                            IsAssetOprating = true;
                                            mOperationAsset.MoveTo(name, mSelectFolderView.CurrentDir.RNameType).AddWaitTask((task)=>
                                            {
                                                IsAssetOprating = false;
                                                InvalidateAssetScan();
                                            });
                                        }
                                        break;
                                    case EAssetOperationType.CopyTo:
                                        {
                                            var name = mSelectFolderView.CurrentDir.Name + sourceName.PureName + sourceName.ExtName;
                                            IsAssetOprating = true;
                                            mOperationAsset.CopyTo(name, mSelectFolderView.CurrentDir.RNameType).AddWaitTask((task) =>
                                            {
                                                IsAssetOprating = false;
                                                InvalidateAssetScan();
                                            });
                                        }
                                        break;
                                    case EAssetOperationType.PackTo:
                                        {
                                            var name = mSelectFolderView.CurrentDir.Name;// + sourceName.PureName + sourceName.ExtName;
                                            IsAssetOprating = true;
                                            mOperationAsset.PackRefAssetsTo(RName.GetRName(name, mSelectFolderView.CurrentDir.RNameType)).AddWaitTask((task) =>
                                            {
                                                IsAssetOprating = false;
                                                InvalidateAssetScan();
                                            });
                                        }
                                        break;
                                    case EAssetOperationType.Rename:
                                        {
                                            var dir = sourceName.Name.Substring(0, sourceName.Name.Length - sourceName.PureName.Length - sourceName.ExtName.Length);
                                            var name = dir + NewName + sourceName.ExtName;
                                            IsAssetOprating = true;
                                            mOperationAsset.RenameTo(name, mSelectFolderView.CurrentDir.RNameType).AddWaitTask((task) =>
                                            {
                                                IsAssetOprating = false;
                                                InvalidateAssetScan();
                                            });
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Info, "Please wait for Action(MoveTo)");
                            }
                        }
                        catch
                        {

                        }
                        mOperationAsset = null;
                        mOperationType = EAssetOperationType.None;
                        ImGuiAPI.CloseCurrentPopup();
                    }
                    mSelectFolderOKButtonHeight = ImGuiAPI.GetItemRectSize().Y;
                    ImGuiAPI.SetItemDefaultFocus();
                    ImGuiAPI.SameLine(0, -1);
                    if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        mOperationAsset = null;
                        mOperationType = EAssetOperationType.None;
                        ImGuiAPI.CloseCurrentPopup();
                    }
                    mSelectFolderOKButtonHeight = Math.Max(mSelectFolderOKButtonHeight, ImGuiAPI.GetItemRectSize().Y);

                    ImGuiAPI.EndChild();
                }
                UIProxy.StyleConfig.Instance.PopPanelChildStyle();
                ImGuiAPI.EndPopup();

            }
            UIProxy.StyleConfig.Instance.PopPopupStyle();
        }

        unsafe void DrawDeleteAssetDialog(RName sourceName)
        {
            if (sourceName == null || mOperationAsset == null)
            {
                mOperationAsset = null;
                mOperationAssets.Clear();
                mOperationType = EAssetOperationType.None;
                return;
            }
            if (mOperationAssets.Count == 0)
                AddOperationAsset(mOperationAsset);

            var deleteCount = mOperationAssets.Count;
            var keyName = deleteCount > 1 ? $"Delete {deleteCount} assets?" : $"Delete {sourceName.Name}?";
            ImGuiAPI.OpenPopup(keyName, ImGuiPopupFlags_.ImGuiPopupFlags_None | ImGuiPopupFlags_.ImGuiPopupFlags_NoOpenOverExistingPopup);

            var pos = ImGuiAPI.GetWindowPos();
            var min = ImGuiAPI.GetWindowContentRegionMin();
            var max = ImGuiAPI.GetWindowContentRegionMax();
            var pivot = new Vector2(0.5f, 0.5f);
            ImGuiAPI.SetNextWindowPos((min + max) * 0.5f + pos, ImGuiCond_.ImGuiCond_Appearing, in pivot);
            UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopupModal(keyName, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
            {
                ImGuiAPI.Text(deleteCount > 1 ? $"Delete {deleteCount} selected assets?" : "Delete this asset?");
                var previewCount = Math.Min(deleteCount, 5);
                for (int i = 0; i < previewCount; i++)
                {
                    var assetName = mOperationAssets[i]?.GetAssetName();
                    if (assetName != null)
                        ImGuiAPI.Text(assetName.Name);
                }
                if (deleteCount > previewCount)
                {
                    ImGuiAPI.Text($"... and {deleteCount - previewCount} more");
                }
                ImGuiAPI.Separator();

                if (ImGuiAPI.Button("Delete", in Vector2.Zero))
                {
                    var deletedAssets = new List<IO.IAssetMeta>();
                    var deletedNames = new List<RName>();
                    var deleteAssets = new List<IAssetMeta>(mOperationAssets);
                    var deleteNames = new List<RName>();
                    for (int i = 0; i < deleteAssets.Count; i++)
                    {
                        deleteNames.Add(deleteAssets[i]?.GetAssetName());
                    }

                    for (int i = 0; i < deleteAssets.Count; i++)
                    {
                        var deletedAsset = deleteAssets[i];
                        var deleteName = deleteNames[i];
                        if (deletedAsset == null || deleteName == null)
                            continue;

                        try
                        {
                            deletedAsset.DeleteAsset(deleteName.Name, deleteName.RNameType);
                            deletedAssets.Add(deletedAsset);
                            deletedNames.Add(deleteName);
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                    }

                    if (deletedAssets.Count > 0 || deletedNames.Count > 0)
                    {
                        NotifyAssetsDeleted(deletedAssets, deletedNames);
                    }

                    mOperationAsset = null;
                    mOperationAssets.Clear();
                    mOperationType = EAssetOperationType.None;
                    ImGuiAPI.CloseCurrentPopup();
                }
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                {
                    mOperationAsset = null;
                    mOperationAssets.Clear();
                    mOperationType = EAssetOperationType.None;
                    ImGuiAPI.CloseCurrentPopup();
                }

                ImGuiAPI.EndPopup();
            }
            UIProxy.StyleConfig.Instance.PopPopupStyle();
        }
    }
}
