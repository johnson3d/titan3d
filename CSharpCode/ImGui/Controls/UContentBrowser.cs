using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public class UContentBrowser : Graphics.Pipeline.IRootForm, EGui.IPanel
    {
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        EGui.UIProxy.SearchBarProxy mSearchBar;

        public string Name = "";
        public bool CreateNewAssets = true;
        public string ExtNames { get; set; } = null;
        public Rtti.UTypeDesc MacrossBase = null;
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
                        mDirContextMenu[i].OnDraw(in drawList, in menuData);
                    }
                }
                ImGuiAPI.EndPopup();
            }
        }
        public RName CurrentDir;
        public void DrawDirectories(RName root)
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
        public unsafe void DrawFiles(RName dir, in Vector2 size)
        {
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var itemSize = new Vector2(80, 100);
                CreateNewAssets = true;
            
            //var cldPos = ImGuiAPI.GetWindowPos();
            //var cldMin = ImGuiAPI.GetWindowContentRegionMin();
            //var cldMax = ImGuiAPI.GetWindowContentRegionMax();
            //cldMin += cldPos;
            //cldMax += cldPos;
            //////cldMin.Y += 30;
            //cmdlist.PushClipRect(in cldMin, in cldMax, true);

            var width = ImGuiAPI.GetWindowContentRegionWidth();
            var files = IO.FileManager.GetFiles(dir.Address, "*" + IO.IAssetMeta.MetaExt, false);
            float curPos = 0;
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var name = IO.FileManager.GetPureName(file);
                if (!string.IsNullOrEmpty(ExtNames))
                {
                    var splits = ExtNames.Split(',');
                    var ext = IO.FileManager.GetExtName(name);
                    bool find = false;
                    for (int extIdx = 0; extIdx < splits.Length; extIdx++)
                    {
                        if (string.Equals(ext, splits[extIdx], StringComparison.OrdinalIgnoreCase))
                        {
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                        continue;

                    if (MacrossBase != null && ext == Bricks.CodeBuilder.UMacross.AssetExt)
                    {
                        var ameta1 = UEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType)) as Bricks.CodeBuilder.UMacrossAMeta;
                        if (ameta1 == null)
                            continue;

                        if (ameta1.BaseTypeStr != MacrossBase.TypeString)
                        {
                            continue;
                        }
                    }
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

                DrawItem(in cmdlist, ameta.Icon, ameta, in itemSize);
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

            var metafiles = IO.FileManager.GetFiles(dir.Address, "*.metadata", false);
            if (metafiles.Length > 0)
            {
                var ameta = new Rtti.UMetaVersionMeta();
                ameta.TypeStr = Rtti.UTypeDescGetter<Rtti.UMetaVersion>.TypeDesc.TypeString;
                foreach (var i in metafiles)
                {
                    var name = IO.FileManager.GetPureName(i);

                    var rootType = UEngine.Instance.FileManager.GetRootDirType(i);
                    var rPath = IO.FileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(rootType), i);
                    if (rootType == IO.FileManager.ERootDir.Game)
                        ameta.SetAssetName(RName.GetRName(rPath, RName.ERNameType.Game));
                    else if (rootType == IO.FileManager.ERootDir.Engine)
                        ameta.SetAssetName(RName.GetRName(rPath, RName.ERNameType.Engine));
                    else
                        continue;

                    DrawItem(in cmdlist, ameta.Icon, ameta, in itemSize);
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
            }

            //cmdlist.PopClipRect();

            ////////////////////////////////////////////////////////////
            //drawList.AddRect(ref min, ref max, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
            ////////////////////////////////////////////////////////////

            if (CreateNewAssets && ImGuiAPI.BeginPopupContextWindow("##ContentFilesMenuWindow", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                var menuData = new Support.UAnyPointer();

                mNewAssetMenuItem.OnDraw(in drawList, in menuData);
                ImGuiAPI.EndPopup();
            }
        }
        private void DrawItem(in ImDrawList cmdlist, UUvAnim icon, IO.IAssetMeta ameta, in Vector2 sz)
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
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    GlobalSelectedAsset = ameta;
                    SelectedAsset = ameta;
                    ItemSelectedAction?.Invoke(ameta);
                }
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None) && ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 8))
                {
                    if (ItemDragging.GetCurItem() == null)
                    {
                        var curItem = new UItemDragging.UItem();
                        curItem.Tag = ameta;
                        curItem.AMeta = ameta;
                        curItem.Size = sz;
                        curItem.Browser = this;
                        ItemDragging.SetCurItem(curItem, () =>
                        {
                            curItem.AMeta.OnDragTo(UEngine.Instance.ViewportSlateManager.GetPressedViewport());
                            return;
                        });
                    }
                }
                ameta.ShowIconTime = UEngine.Instance.CurrentTickCount;
                ameta.OnDraw(in cmdlist, in sz, this);
            }
        }
        public UItemDragging ItemDragging = new UItemDragging();
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
            if (DrawInWindow)
            {
                ImGuiAPI.SetNextWindowSize(new Vector2(800, 300), ImGuiCond_.ImGuiCond_FirstUseEver);
                draw = ImGuiAPI.Begin(name, null, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar);
            }
            if(draw)
            {
//                if (ImGuiAPI.IsWindowDocked())
//                {
//                    DockId = ImGuiAPI.GetWindowDockID();
//                }
                ImGuiAPI.Columns(2, null, true);

                if (LeftSize.X == 0)
                {
                    var cltSize = ImGuiAPI.GetWindowContentRegionWidth();
                    ImGuiAPI.SetColumnWidth(0, ((float)cltSize) * 0.3f);
                }

                if (ImGuiAPI.BeginChild("LeftWindow", in Vector2.MinusOne, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                {
                    var min = ImGuiAPI.GetWindowContentRegionMin();
                    var max = ImGuiAPI.GetWindowContentRegionMax();
                    LeftSize = max - min;
                    
                    DrawDirectories(RName.GetRName("", RName.ERNameType.Game));
                    DrawDirectories(RName.GetRName("", RName.ERNameType.Engine));
                }
                ImGuiAPI.EndChild();
                
                ImGuiAPI.NextColumn();

                if (CurrentDir != null)
                {
                    if (mSearchBar != null)
                    {
                        mSearchBar.Width = ImGuiAPI.GetColumnWidth(1);
                        var cmdlist = ImGuiAPI.GetWindowDrawList();
                        if (mSearchBar.OnDraw(in cmdlist, in Support.UAnyPointer.Default))
                            FilterText = mSearchBar.SearchText;
                    }
                    else
                    {
                        SureSearchBar();
                    }
                    if (ImGuiAPI.BeginChild("RightWindow", in Vector2.MinusOne, false, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysVerticalScrollbar))
                    {
                        var min = ImGuiAPI.GetWindowContentRegionMin();
                        var max = ImGuiAPI.GetWindowContentRegionMax();
                        RightSize = max - min;

                        DrawFiles(CurrentDir, in RightSize);
                    }
                    ImGuiAPI.EndChild();
                }
                    
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
