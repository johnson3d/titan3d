using EngineNS.Thread;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        public Rtti.UTypeDesc MacrossBase = null;

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

            if (UEngine.Instance.UIProxyManager[FolderOpenImgName] == null)
                UEngine.Instance.UIProxyManager[FolderOpenImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(FolderOpenImgName, RName.ERNameType.Engine));
            if (UEngine.Instance.UIProxyManager[FolderClosedImgName] == null)
                UEngine.Instance.UIProxyManager[FolderClosedImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(FolderClosedImgName, RName.ERNameType.Engine));
            if (UEngine.Instance.UIProxyManager[PreFolderImgName] == null)
                UEngine.Instance.UIProxyManager[PreFolderImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(PreFolderImgName, RName.ERNameType.Engine));
            if (UEngine.Instance.UIProxyManager[NextFolderImgName] == null)
                UEngine.Instance.UIProxyManager[NextFolderImgName] = new EGui.UIProxy.ImageProxy(RName.GetRName(NextFolderImgName, RName.ERNameType.Engine));

            InitializeDirContextMenu();

            return true;
        }
        public void Draw(in Vector2 size)
        {
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, 0xFF1A1A1A);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, true, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
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
                DrawDirectories(RName.GetRName("", RName.ERNameType.Game));
                DrawDirectories(RName.GetRName("", RName.ERNameType.Engine));
                ImGuiAPI.PopStyleColor(3);
            }
            ImGuiAPI.EndChild();
            ImGuiAPI.PopStyleColor(1);

            if (!string.IsNullOrEmpty(mCreateFolderDir))
            {
                var pathName = IO.TtFileManager.GetLastestPathName(mCreateFolderDir);
                if (UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) == mCreateFolderDir)
                {
                    pathName = "Game";
                }
                else if (UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine) == mCreateFolderDir)
                {
                    pathName = "Engine";
                }
                else if (UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Editor) == mCreateFolderDir)
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
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)EGui.Slate.UBaseRenderer.enFont.Font_Bold_13px);
            var treeNodeResult = ImGuiAPI.TreeNodeEx(root.RNameType.ToString(), flags);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();
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
            };
        }
        struct stDirMenuData
        {
        }
        void DrawDirContextMenu(string path)
        {
            if (ImGuiAPI.BeginPopupContextItem(path, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (mDirContextMenu != null)
                {
                    Support.UAnyPointer menuData = new Support.UAnyPointer();
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
        }
        private unsafe void DrawTree(RName.ERNameType type, string parentDir, string dirName)
        {
            var nextParent = parentDir + dirName + "/";
            var path = RName.GetRName(nextParent, type).Address;

            if (ExtNameArray != null && ExtNameArray.Length > 0)
            {
                if (!DirectoryShowFlags.ContainsKey(path))
                {
                    bool hasTarget = false;
                    for (int i = 0; i < ExtNameArray.Length; i++)
                    {
                        var files = IO.TtFileManager.GetFiles(path, "*" + ExtNameArray[i] + ".ameta");
                        if (files.Length == 0)
                            continue;
                        if (MacrossBase != null && ExtNameArray[i] == Bricks.CodeBuilder.UMacross.AssetExt)
                        {
                            foreach (var f in files)
                            {
                                var ff = f.Substring(0, f.Length - ".ameta".Length);
                                var ameta1 = UEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRNameFromAbsPath(ff)) as Bricks.CodeBuilder.UMacrossAMeta;
                                if (ameta1 == null)
                                    continue;

                                if (ameta1.BaseTypeStr != MacrossBase.TypeString)
                                {
                                    continue;
                                }
                                hasTarget = true;
                                break;
                            }
                            if (hasTarget)
                                break;
                        }
                        else
                        {
                            hasTarget = true;
                            break;
                        }
                    }
                    DirectoryShowFlags[path] = hasTarget;
                }
                if (!DirectoryShowFlags[path])
                    return;

            }

            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            var rn = RName.GetRName(nextParent, type);
            var textColor = EGui.UIProxy.StyleConfig.Instance.TextColor;
            if (rn == CurrentDir)
            {
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                textColor = 0xffffffff;
            }

            var style = ImGuiAPI.GetStyle();
            ImGuiAPI.PushID(dirName);
            Vector2 itemRectStart = ImGuiAPI.GetCursorScreenPos();
            var treeNodeResult = ImGuiAPI.TreeNodeEx("", flags, "");

            var cmdList = ImGuiAPI.GetWindowDrawList();
            var start = ImGuiAPI.GetItemRectMin();
            //var end = ImGuiAPI.GetItemRectMax();
            //cmdList.AddRect(start, end, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
            //ImGuiAPI.SameLine(0, -1);
            var curPos = ImGuiAPI.GetCursorScreenPos();
            Vector2 rectSize = Vector2.Zero;
            start.X = curPos.X;
            var imgSize = 16;
            if (treeNodeResult)
            {
                var shadowImg = UEngine.Instance.UIProxyManager[FolderOpenImgName] as EGui.UIProxy.ImageProxy;
                if (shadowImg != null)
                    shadowImg.OnDraw(cmdList, start, start + new Vector2(imgSize, imgSize), 0xff558fb6);
            }
            else
            {
                start.X += style->IndentSpacing;
                var shadowImg = UEngine.Instance.UIProxyManager[FolderClosedImgName] as EGui.UIProxy.ImageProxy;
                if (shadowImg != null)
                    shadowImg.OnDraw(cmdList, start, start + new Vector2(imgSize, imgSize), 0xff558fb6);
            }
            rectSize.X = imgSize + style->ItemSpacing.X;
            rectSize.Y = imgSize;
            start.X += rectSize.X;
            var textSize = ImGuiAPI.CalcTextSize(dirName, false, 0.0f);
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
