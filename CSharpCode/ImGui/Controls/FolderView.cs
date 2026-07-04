using EngineNS.Thread;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public partial class FolderView
    {
        public Vector2 ContentSize = Vector2.Zero;
        public RName CurrentDir;
        public int CurrentDirHistoryIdx = 0;
        public List<RName> DirHistory = new List<RName>();
        public bool IsPreFolderDisable = true;
        public bool IsNextFolderDisable = true;
        public string[] ExtNameArray;
        public Dictionary<string, bool> DirectoryShowFlags = new Dictionary<string, bool>();
        public Rtti.TtTypeDesc MacrossBase = null;
        public string ShaderType = null;
        readonly HashSet<string> mPendingDirectoryShowChecks = new HashSet<string>();
        readonly ConcurrentQueue<DirectoryShowCheckResult> mDirectoryShowCheckResults = new ConcurrentQueue<DirectoryShowCheckResult>();
        static readonly SemaphoreSlim DirectoryShowCheckSemaphore = new SemaphoreSlim(1, 1);
        int mDirectoryShowCheckVersion = 0;

        struct DirectoryShowCheckResult
        {
            public string Path;
            public bool HasTarget;
            public int Version;
        }

        public void ClearDirectoryShowCache()
        {
            DirectoryShowFlags.Clear();
            mPendingDirectoryShowChecks.Clear();
            Interlocked.Increment(ref mDirectoryShowCheckVersion);
            while (mDirectoryShowCheckResults.TryDequeue(out _))
            {
            }
        }

        void ApplyDirectoryShowCheckResults()
        {
            while (mDirectoryShowCheckResults.TryDequeue(out var result))
            {
                if (result.Version != mDirectoryShowCheckVersion)
                    continue;

                mPendingDirectoryShowChecks.Remove(result.Path);
                DirectoryShowFlags[result.Path] = result.HasTarget;
            }
        }

        void RequestDirectoryShowCheck(string path)
        {
            if (string.IsNullOrEmpty(path) || mPendingDirectoryShowChecks.Contains(path))
                return;

            var extNames = ExtNameArray;
            if (extNames == null || extNames.Length == 0)
                return;

            var version = mDirectoryShowCheckVersion;
            var extCopy = new string[extNames.Length];
            Array.Copy(extNames, extCopy, extNames.Length);
            mPendingDirectoryShowChecks.Add(path);
            Task.Run(async () =>
            {
                await DirectoryShowCheckSemaphore.WaitAsync();
                try
                {
                    if (version != mDirectoryShowCheckVersion)
                        return;

                    var hasTarget = DirectoryContainsTargetAsset(path, extCopy);
                    mDirectoryShowCheckResults.Enqueue(new DirectoryShowCheckResult()
                    {
                        Path = path,
                        HasTarget = hasTarget,
                        Version = version,
                    });
                }
                finally
                {
                    DirectoryShowCheckSemaphore.Release();
                }
            });
        }

        static bool DirectoryContainsTargetAsset(string path, string[] extNames)
        {
            try
            {
                // Keep folder filtering cheap: subtype checks still happen when the content pane loads asset metadata.
                foreach (var file in System.IO.Directory.EnumerateFiles(path, "*" + IO.IAssetMeta.MetaExt, System.IO.SearchOption.AllDirectories))
                {
                    for (int i = 0; i < extNames.Length; i++)
                    {
                        if (file.EndsWith(extNames[i] + IO.IAssetMeta.MetaExt, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            catch
            {
                return true;
            }
            return false;
        }

        public static string FolderOpenImgName = "uestyle/content/folderopen.srv";
        public static string FolderClosedImgName = "uestyle/content/folderclosed.srv";
        public static string PreFolderImgName = "uestyle/content/circle-arrow-left.srv";
        public static string NextFolderImgName = "uestyle/content/circle-arrow-right.srv";

        // 引擎预留目录名称
        static List<string> ReservationFolderNames = new List<string>()
        {
            "metadata",
        };

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await TtAsyncDummyClass.DummyFunc();

            if (TtEngine.Instance.UIProxyManager[FolderOpenImgName] == null)
                TtEngine.Instance.UIProxyManager[FolderOpenImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(FolderOpenImgName, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[FolderClosedImgName] == null)
                TtEngine.Instance.UIProxyManager[FolderClosedImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(FolderClosedImgName, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[PreFolderImgName] == null)
                TtEngine.Instance.UIProxyManager[PreFolderImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(PreFolderImgName, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[NextFolderImgName] == null)
                TtEngine.Instance.UIProxyManager[NextFolderImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(NextFolderImgName, RName.ERNameType.Engine));

            InitializeDirContextMenu();

            if (TtEngine.Instance.DynConfigData.TryGetConfig<RName>("ContentCurrentDir", out var cfgContentCurrentDir))
            {
                this.CurrentDir = cfgContentCurrentDir;
                if (IO.TtFileManager.DirectoryExists(this.CurrentDir.Address) == false)
                {
                    this.CurrentDir = null;
                }
            }

            return true;
        }
        public void Draw(in Vector2 size)
        {
            ApplyDirectoryShowCheckResults();
            var styleConfig = UIProxy.StyleConfig.Instance;
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ChildRounding, 3.0f);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in styleConfig.ContentBrowserFolderItemSpacing);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in styleConfig.ContentBrowserFolderFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, styleConfig.ContentBrowserFolderBg);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, styleConfig.BorderColor);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                //var winMin = ImGuiAPI.GetWindowPos();
                //var winMax = winMin + ImGuiAPI.GetWindowSize();
                //cmd.AddRect(winMin - style->WindowPadding, winMax + style->WindowPadding, 0xff0000ff, 0, ImDrawFlags_.ImDrawFlags_None, 1);
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                ContentSize = max - min;

                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, UIProxy.StyleConfig.Instance.TVHeader);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, UIProxy.StyleConfig.Instance.TVHeaderActive);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, UIProxy.StyleConfig.Instance.TVHeaderHovered);
                TtEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)EGui.Slate.TtBaseRenderer.enFont.Font_18px);
                DrawDirectories(RName.GetRName("", RName.ERNameType.Game));
                DrawDirectories(RName.GetRName("", RName.ERNameType.Engine));
                DrawDirectories(RName.GetRName("", RName.ERNameType.Cloud));
                TtEngine.Instance.GfxDevice.SlateRenderer.PopFont();
                ImGuiAPI.PopStyleColor(3);
            }
            ImGuiAPI.EndChild();
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(3);

            if (!string.IsNullOrEmpty(mCreateFolderDir))
            {
                var pathName = IO.TtFileManager.GetLastestPathName(mCreateFolderDir);
                if (TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) == mCreateFolderDir)
                {
                    pathName = "Game";
                }
                else if (TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine) == mCreateFolderDir)
                {
                    pathName = "Engine";
                }
                else if (TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cloud) == mCreateFolderDir)
                {
                    pathName = "Cloud";
                }
                else if (TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Editor) == mCreateFolderDir)
                {
                    pathName = "Editor";
                }
                var keyName = $"Create Folder in {pathName}";
                EGui.UIProxy.SingleInputDialog.Open(keyName);
                DrawCreateFolderDialog(keyName);
            }
        }

        public void PushHistory(RName dir)
        {
            if (CurrentDir == dir)
                return;
            CurrentDir = dir;
            TtEngine.Instance.DynConfigData.SetConfig("ContentCurrentDir", dir);
            if (CurrentDirHistoryIdx + 1 < DirHistory.Count)
            {
                DirHistory.RemoveRange(CurrentDirHistoryIdx + 1, DirHistory.Count - CurrentDirHistoryIdx - 1);
            }
            if (DirHistory.Count > 20)
                DirHistory.RemoveAt(0);
            CurrentDirHistoryIdx = DirHistory.Count;
            DirHistory.Add(dir);
            IsPreFolderDisable = false;
            IsNextFolderDisable = true;
        }
        static bool IsReservationFolder(string folderName)
        {
            for (int rIdx = 0; rIdx < ReservationFolderNames.Count; rIdx++)
            {
                if (ReservationFolderNames[rIdx] == folderName)
                {
                    return true;
                }
            }
            return false;
        }
        void DrawDirectories(RName root)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            if (root == CurrentDir)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            if (CurrentDir != null && CurrentDir.Address.Contains(root.Address))
            {
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
            }
            var treeNodeResult = ImGuiAPI.TreeNodeEx(root.RNameType.ToString(), flags);
            DrawDirContextMenu(root.Address);
            if (treeNodeResult)
            {
                if (ImGuiAPI.IsItemActivated())
                {
                    PushHistory(root);
                }
                var dirs = IO.TtFileManager.GetDirectories(root.Address, "*.*", false);
                foreach (var i in dirs)
                {
                    var nextDirName = IO.TtFileManager.GetRelativePath(root.Address, i);
                    if (IsReservationFolder(nextDirName))
                        continue;
                    DrawTree(root.RNameType, root.Name, nextDirName);
                }
                ImGuiAPI.TreePop();
            }
        }
        List<UIProxy.MenuItemProxy> mDirContextMenu;
        void InitializeDirContextMenu()
        {
            mDirContextMenu = new List<UIProxy.MenuItemProxy>()
            {
                new UIProxy.MenuItemProxy()
                {
                    MenuName = "Browser",
                    Action = (item, data)=>
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                        psi.Arguments = "/e,/select," + data.RefObject.ToString().Replace("/", "\\");
                        System.Diagnostics.Process.Start(psi);
                    },
                },
                new UIProxy.MenuItemProxy()
                {
                    MenuName = "Create Folder",
                    Action = (item, data)=>
                    {
                        mCreateFolderDir = data.RefObject.ToString();
                    },
                },
                new UIProxy.MenuItemProxy()
                {
                    MenuName = "Copy Folder Address",
                    Action = (item, data)=>
                    {
                        ImGuiAPI.SetClipboardText(data.RefObject.ToString());
                    },
                }
            };
        }
        struct stDirMenuData
        {
        }
        void DrawDirContextMenu(string path)
        {
            UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopupContextItem(path, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (mDirContextMenu != null)
                {
                    Support.TtAnyPointer menuData = new Support.TtAnyPointer();
                    menuData.RefObject = path;
                    menuData.Value.SetStruct<stDirMenuData>(new stDirMenuData());
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    for (int i = 0; i < mDirContextMenu.Count; i++)
                    {
                        mDirContextMenu[i].OnDraw(in drawList, in menuData);
                    }
                }
                ImGuiAPI.EndPopup();
            }
            UIProxy.StyleConfig.Instance.PopPopupStyle();
        }
        private unsafe void DrawTree(RName.ERNameType type, string parentDir, string dirName)
        {
            var nextParent = parentDir + dirName + "/";
            var path = RName.GetRName(nextParent, type).Address;

            if (ExtNameArray != null && ExtNameArray.Length > 0)
            {
                if (!DirectoryShowFlags.TryGetValue(path, out var hasTarget))
                {
                    RequestDirectoryShowCheck(path);
                    hasTarget = true;
                }
                if (!hasTarget)
                    return;

            }

            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            var rn = RName.GetRName(nextParent, type);
            var textColor = EGui.UIProxy.StyleConfig.Instance.TextColor;
            if (rn == CurrentDir)
            {
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                //ImGuiAPI.SetScrollHereX(0.5f);
                //ImGuiAPI.SetScrollHereY(0.5f);
                textColor = 0xffffffff;
            }
            else if (CurrentDir != null && CurrentDir.Address.Contains(path))
            {
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
            }

            var style = ImGuiAPI.GetStyle();
            ImGuiAPI.PushID(dirName);
            Vector2 itemRectStart = ImGuiAPI.GetCursorScreenPos();
            var treeNodeResult = ImGuiAPI.TreeNodeEx("", flags, "");

            var cmdList = ImGuiAPI.GetWindowDrawList();
            var itemMin = ImGuiAPI.GetItemRectMin();
            var itemMax = ImGuiAPI.GetItemRectMax();
            var start = itemMin;
            //cmdList.AddRect(start, end, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
            //ImGuiAPI.SameLine(0, -1);
            var curPos = ImGuiAPI.GetCursorScreenPos();
            Vector2 rectSize = Vector2.Zero;
            var imViewPort = ImGuiAPI.GetWindowViewport();
            var dpiScale = imViewPort->DpiScale;
            var styleConfig = UIProxy.StyleConfig.Instance;
            float imgSize = styleConfig.ContentBrowserFolderIconSize * dpiScale;
            float iconIndent = styleConfig.ContentBrowserFolderIconIndent * dpiScale;

            start.X = curPos.X + iconIndent;
            start.Y = itemMin.Y + ((itemMax.Y - itemMin.Y) - imgSize) * 0.5f;

            if (treeNodeResult)
            {
                var shadowImg = TtEngine.Instance.UIProxyManager[FolderOpenImgName] as EGui.UIProxy.ImageProxy;
                if (shadowImg != null)
                    shadowImg.OnDraw(cmdList, start, start + new Vector2(imgSize, imgSize), UIProxy.StyleConfig.Instance.LinkStringColor);
            }
            else
            {
                start.X += style->IndentSpacing * dpiScale;
                var shadowImg = TtEngine.Instance.UIProxyManager[FolderClosedImgName] as EGui.UIProxy.ImageProxy;
                if (shadowImg != null)
                    shadowImg.OnDraw(cmdList, start, start + new Vector2(imgSize, imgSize), UIProxy.StyleConfig.Instance.TextDisableColor);
            }
            rectSize.X = imgSize + styleConfig.ContentBrowserFolderTextSpacing * dpiScale;
            rectSize.Y = imgSize;
            start.X += rectSize.X;
            var textSize = ImGuiAPI.CalcTextSize(dirName, false, 0.0f);
            start.Y = itemMin.Y + ((itemMax.Y - itemMin.Y) - textSize.Y) * 0.5f;
            cmdList.AddText(start, textColor, dirName, null);
            rectSize.X += textSize.X;
            rectSize.Y = MathF.Max(rectSize.Y, textSize.Y);
            rectSize.X += ImGuiAPI.GetFontSize() + style->FramePadding.X;
            ImGuiAPI.SetCursorScreenPos(in itemRectStart);
            ImGuiAPI.ItemSize(in itemRectStart, itemRectStart + rectSize, 0);
            //ImGuiAPI.SameLine(0, 32);
            //ImGuiAPI.Text("_" + dirName);

            DrawDirContextMenu(path);
            if (treeNodeResult)
            {
                if (ImGuiAPI.IsItemActivated())
                {
                    PushHistory(rn);
                }

                var dirs = IO.TtFileManager.GetDirectories(path, "*.*", false);
                foreach (var i in dirs)
                {
                    if (IO.TtFileManager.FileExists(i + IO.IAssetMeta.MetaExt))
                        continue;
                    var nextDirName = IO.TtFileManager.GetRelativePath(path, i);
                    if (IsReservationFolder(nextDirName))
                        continue;
                    DrawTree(type, nextParent, nextDirName);
                }
                ImGuiAPI.TreePop();
            }
            else
            {
                if (ImGuiAPI.IsItemActivated())
                {
                    PushHistory(rn);
                }
            }

            ImGuiAPI.PopID();
        }

        string mCreateFolderDir = null;
        string mNewFolderName = "NewFolder";
        void DrawCreateFolderDialog(string keyName)
        {
            if (string.IsNullOrEmpty(mCreateFolderDir))
                return;

            var pos = ImGuiAPI.GetWindowPos();
            var min = ImGuiAPI.GetWindowContentRegionMin();
            var max = ImGuiAPI.GetWindowContentRegionMax();
            var pivot = new Vector2(0.5f, 0.5f);
            ImGuiAPI.SetNextWindowPos((min + max) * 0.5f + pos, ImGuiCond_.ImGuiCond_Appearing, in pivot);

            var result = EGui.UIProxy.SingleInputDialog.Draw(keyName, "Folder Name:", ref mNewFolderName, (val) =>
            {
                if (string.IsNullOrEmpty(val))
                    return "Empty folder name";
                if (!Regex.IsMatch(val, @"^[a-zA-Z0-9\\_]+$"))
                    return "Invalid folder name!";
                if (IO.TtFileManager.DirectoryExists(mCreateFolderDir + mNewFolderName))
                    return $"Directory {mNewFolderName} is exist";

                return null;
            });
            switch (result)
            {
                case UIProxy.SingleInputDialog.enResult.OK:
                    IO.TtFileManager.CreateDirectory(mCreateFolderDir + mNewFolderName);
                    ClearDirectoryShowCache();
                    mNewFolderName = "NewFolder";
                    mCreateFolderDir = null;
                    break;
                case UIProxy.SingleInputDialog.enResult.Cancel:
                    mCreateFolderDir = null;
                    mNewFolderName = "NewFolder";
                    break;
            }
        }
    }
}
