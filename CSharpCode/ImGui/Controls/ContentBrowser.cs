using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public class ContentBrowser : Graphics.Pipeline.IRootForm, EGui.IPanel
    {
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.SearchBarProxy mSearchBar;

        public string Name = "";
        public bool CreateNewAssets = true;
        public string ExtNames = null;
        public string FilterText = "";
        public static IO.IAssetMeta GlobalSelectedAsset = null;
        public IO.IAssetMeta SelectedAsset = null;
        public Action<IO.IAssetMeta> ItemSelectedAction = null;

        public void Cleanup()
        {
            GlobalSelectedAsset = null;
            mSearchBar?.Cleanup();
            mSearchBar = null;
        }

        public async Task<bool> Initialize()
        {
            await Thread.AsyncDummyClass.DummyFunc();

            if(mNewAssetMenuItem == null)
            {
                mNewAssetMenuItem = new UIProxy.MenuItemProxy()
                {
                    MenuName = "New Asset",
                };
                await mNewAssetMenuItem.Initialize();

                OnTypeChanged();
            }

            mSearchBar = new UIProxy.SearchBarProxy();
            await mSearchBar.Initialize();
            mSearchBar.InfoText = "Search Assets";
            //Rtti.UTypeDescManager.Instance.OnTypeChanged += OnTypeChanged;

            InitializeDirContextMenu();

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

        EGui.UIProxy.MenuItemProxy mNewAssetMenuItem;
        void OnTypeChanged()
        {
            mNewAssetMenuItem.CleanupSubMenus();

            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach(var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(IO.AssetCreateMenuAttribute), false);
                    if (atts.Length == 0)
                        continue;

                    var assetExtField = Rtti.UTypeDesc.GetField(typeDesc.SystemType, "AssetExt");
                    mNewAssetMenuItem.SubMenus.Add(new EGui.UIProxy.MenuItemProxy()
                    {
                        MenuName = ((IO.AssetCreateMenuAttribute)atts[0]).MenuName,
                        Shortcut = ((IO.AssetCreateMenuAttribute)atts[0]).Shortcut,
                        Action = (proxy, data)=>
                        {
                            mAssetImporter = UEngine.Instance.AssetMetaManager.ImportAsset(CurrentDir, typeDesc, (string)assetExtField.GetValue(null));
                        }
                    });
                }
            }
        }
        void DrawDirContextMenu(string path)
        {
            if (ImGuiAPI.BeginPopupContextItem(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (mDirContextMenu != null)
                {
                    Support.UAnyPointer menuData = new Support.UAnyPointer();
                    menuData.RefObject = path;
                    menuData.Value.SetStruct<stDirMenuData>(new stDirMenuData());
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    for (int i = 0; i < mDirContextMenu.Count; i++)
                    {
                        mDirContextMenu[i].OnDraw(ref drawList, ref menuData);
                    }
                }
                ImGuiAPI.EndPopup();
            }
        }
        public RName CurrentDir;
        public void DrawDirectories(RName root, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
                if (root == CurrentDir)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;
                var treeNodeResult = ImGuiAPI.TreeNodeEx(root.RNameType.ToString(), flags);
                DrawDirContextMenu(root.Address);
                if (treeNodeResult)
                {
                    if (ImGuiAPI.IsItemActivated())
                    {
                        CurrentDir = root;
                    }
                    var dirs = IO.FileManager.GetDirectories(root.Address, "*.*", false);
                    foreach (var i in dirs)
                    {
                        var nextDirName = IO.FileManager.GetRelativePath(root.Address, i);
                        DrawTree(root.RNameType, root.Name, nextDirName);
                    }
                    ImGuiAPI.TreePop();
                }
            }
            ImGuiAPI.EndChild();
        }
        struct stDirMenuData
        {
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
                if (IO.FileManager.DirectoryExists(mCreateFolderDir + mNewFolderName))
                    return $"Directory {mNewFolderName} is exist";

                return null;
            });
            switch(result)
            {
                case UIProxy.SingleInputDialog.enResult.OK:
                    IO.FileManager.CreateDirectory(mCreateFolderDir + mNewFolderName);
                    mNewFolderName = "NewFolder";
                    mCreateFolderDir = null;
                    break;
                case UIProxy.SingleInputDialog.enResult.Cancel:
                    mCreateFolderDir = null;
                    mNewFolderName = "NewFolder";
                    break;
            }
        }
        private void DrawTree(RName.ERNameType type, string parentDir, string dirName)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
            var nextParent = parentDir + dirName + "/";
            var rn = RName.GetRName(nextParent, type);
            if (rn == CurrentDir)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;

            var treeNodeResult = ImGuiAPI.TreeNodeEx(dirName, flags);
            var path = RName.GetRName(nextParent, type).Address;
            DrawDirContextMenu(path);
            if (treeNodeResult)
            {   
                if (ImGuiAPI.IsItemActivated())
                {
                    CurrentDir = rn;
                }

                var dirs = IO.FileManager.GetDirectories(path, "*.*", false);
                foreach(var i in dirs)
                {
                    if(IO.FileManager.FileExists(i + IO.IAssetMeta.MetaExt))
                        continue;
                    var nextDirName = IO.FileManager.GetRelativePath(path, i);
                    DrawTree(type, nextParent, nextDirName);
                }
                ImGuiAPI.TreePop();
            }
            else
            {
                if (ImGuiAPI.IsItemActivated())
                {
                    CurrentDir = rn;
                }
            }
        }
        public unsafe void DrawFiles(RName dir, Vector2 size)
        {
            var itemSize = new Vector2(80, 100);
            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
            if (mSearchBar != null)
            {
                mSearchBar.Width = size.X;
                if (mSearchBar.OnDraw(ref cmdlist, ref Support.UAnyPointer.Default))
                    FilterText = mSearchBar.SearchText;
            }
            else
            {
                SureSearchBar();
            }
            if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                ////////////////////////////////////////////////////////////
                //var winPos = ImGuiAPI.GetWindowPos();
                //var drawList = ImGuiAPI.GetForegroundDrawList();
                //var viewPort = ImGuiAPI.GetWindowViewport();
                //var min = ImGuiAPI.GetWindowContentRegionMin() + winPos;
                //var max = ImGuiAPI.GetWindowContentRegionMax() + winPos;
                ////////////////////////////////////////////////////////////

                var width = ImGuiAPI.GetWindowContentRegionWidth();
                var files = IO.FileManager.GetFiles(dir.Address, "*" + IO.IAssetMeta.MetaExt, false);
                float curPos = 0;
                for(int i=0; i<files.Length; i++)
                {
                    var file = files[i];
                    var name = IO.FileManager.GetPureName(file);
                    if (!string.IsNullOrEmpty(ExtNames))
                    {
                        var splits = ExtNames.Split(',');
                        var ext = IO.FileManager.GetExtName(name);
                        bool find = false;
                        for(int extIdx = 0; extIdx < splits.Length; extIdx++)
                        {
                            if (string.Equals(ext, splits[extIdx], StringComparison.OrdinalIgnoreCase))
                            {
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                            continue;
                    }

                    var filterName = IO.FileManager.GetPureName(name);
                    if (!string.IsNullOrEmpty(FilterText))
                    {
                        if (filterName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == false)
                            continue;
                    }

                    var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType));
                    if (ameta == null)
                        continue;

                    DrawItem(ref cmdlist, ameta.Icon, ameta, ref itemSize);
                    curPos += itemSize.X + 2;
                    if (curPos + itemSize.X < width)
                    {
                        ImGuiAPI.SameLine(0, 2);
                    }
                    else
                    {
                        curPos = 0;
                    }
                }

                ////////////////////////////////////////////////////////////
                //drawList.AddRect(ref min, ref max, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
                ////////////////////////////////////////////////////////////

                if (CreateNewAssets && ImGuiAPI.BeginPopupContextWindow("##ContentFilesMenuWindow", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    var menuData = new Support.UAnyPointer();

                    mNewAssetMenuItem.OnDraw(ref drawList, ref menuData);
                    ImGuiAPI.EndPopup();
                }
            }
            ImGuiAPI.EndChild();
        }
        private void DrawItem(ref ImDrawList cmdlist, UUvAnim icon, IO.IAssetMeta ameta, ref Vector2 sz)
        {
            ImGuiAPI.Selectable($"##{ameta.GetAssetName().Name}", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz);
            if (ImGuiAPI.IsItemVisible())
            {
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    CtrlUtility.DrawHelper(ameta.GetAssetName().Name, ameta.Description);
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left) && ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                    {
                        var type = Rtti.UTypeDesc.TypeOf(ameta.TypeStr).SystemType;
                        if (type != null)
                        {
                            var attrs = type.GetCustomAttributes(typeof(Editor.UAssetEditorAttribute), false);
                            if (attrs.Length > 0)
                            {
                                var editorAttr = attrs[0] as Editor.UAssetEditorAttribute;
                                var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, editorAttr.EditorType, ameta.GetAssetName(), null);
                            }
                        }
                    }
                }
                if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    GlobalSelectedAsset = ameta;
                    SelectedAsset = ameta;
                    ItemSelectedAction?.Invoke(ameta);
                }
                ameta.ShowIconTime = UEngine.Instance.CurrentTickCount;
                ameta.OnDraw(ref cmdlist, ref sz, this);
            }
        }
        Vector2 LeftSize;
        Vector2 RightSize;
        public IO.IAssetCreateAttribute mAssetImporter;
        public bool DrawInWindow = true;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            //            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var name = Name;
            if (string.IsNullOrEmpty(name))
                name = "ContentBrowser";
            bool draw = true;
            if(DrawInWindow)
                draw = ImGuiAPI.Begin(name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar);
            if(draw)
            {
//                if (ImGuiAPI.IsWindowDocked())
//                {
//                    DockId = ImGuiAPI.GetWindowDockID();
//                }
                var cltMin = ImGuiAPI.GetWindowContentRegionMin();
                var cltMax = ImGuiAPI.GetWindowContentRegionMax();

                ImGuiAPI.Columns(2, null, true);

                LeftSize.X = ImGuiAPI.GetColumnWidth(0);
                LeftSize.Y = cltMax.Y - cltMin.Y;

                DrawDirectories(RName.GetRName("", RName.ERNameType.Game), LeftSize);

                DrawDirectories(RName.GetRName("", RName.ERNameType.Engine), LeftSize);

                ImGuiAPI.NextColumn();

                RightSize.X = ImGuiAPI.GetColumnWidth(1);
                RightSize.Y = cltMax.Y - cltMin.Y;
                if (CurrentDir != null)
                    DrawFiles(CurrentDir, RightSize);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);

                if (!string.IsNullOrEmpty(mCreateFolderDir))
                {
                    var pathName = IO.FileManager.GetLastestPathName(mCreateFolderDir);
                    if(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game) == mCreateFolderDir)
                    {
                        pathName = "Game";
                    }
                    else if(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) == mCreateFolderDir)
                    {
                        pathName = "Engine";
                    }
                    else if(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Editor) == mCreateFolderDir)
                    {
                        pathName = "Editor";
                    }
                    var keyName = $"Create Folder in {pathName}";
                    EGui.UIProxy.SingleInputDialog.Open(keyName);
                    DrawCreateFolderDialog(keyName);
                }

            }
            if (DrawInWindow)
                ImGuiAPI.End();

            if (mAssetImporter != null)
                mAssetImporter.OnDraw(this);

        }
    }
}
