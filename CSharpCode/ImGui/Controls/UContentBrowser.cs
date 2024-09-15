using EngineNS.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public partial class UContentBrowser : IRootForm, EGui.IPanel
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
                mFolderView.DirectoryShowFlags.Clear();
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
        
        public unsafe void DrawFiles(RName dir, in Vector2 size)
        {
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var itemSize = new Vector2(100, 150);

            var imViewPort = ImGuiAPI.GetWindowViewport();
            var dpiScale = imViewPort->DpiScale;
            itemSize *= dpiScale;

            CreateNewAssets = true;
            
            if(FirstClickIndex != LastClickIndex)
            {
                for(int i=0; i<SelectedAssets.Count; i++)
                {
                    SelectedAssets[i].IsSelected = false;
                }
                SelectedAssets.Clear();
                mShiftSelection = true;
            }

            //var cldPos = ImGuiAPI.GetWindowPos();
            //var cldMin = ImGuiAPI.GetWindowContentRegionMin();
            //var cldMax = ImGuiAPI.GetWindowContentRegionMax();
            //cldMin += cldPos;
            //cldMax += cldPos;
            //////cldMin.Y += 30;
            //cmdlist.PushClipRect(in cldMin, in cldMax, true);
            var style = ImGuiAPI.GetStyle();
            var width = ImGuiAPI.GetWindowContentRegionWidth();
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, new Vector2(8, 8));
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, 0x00000000);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, 0x00000000);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, 0x00000000);
            var files = IO.TtFileManager.GetFiles(dir.Address, "*" + IO.IAssetMeta.MetaExt, mWithChildFolders);
            float curPos = 0;
            int drawIndex = 0;
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                file = file.Substring(0, file.Length - IO.IAssetMeta.MetaExt.Length);
                var name = IO.TtFileManager.GetRelativePath(dir.Address, file);
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
                        continue;

                    if (MacrossBase != null && ext == Bricks.CodeBuilder.UMacross.AssetExt)
                    {
                        var ameta1 = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType)) as Bricks.CodeBuilder.UMacrossAMeta;
                        if (ameta1 == null)
                            continue;

                        if (ameta1.BaseTypeStr != MacrossBase.TypeString)
                        {
                            continue;
                        }
                    }
                    else if (ShaderType != null && ext == Graphics.Pipeline.Shader.TtShaderAsset.AssetExt)
                    {
                        var ameta1 = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType)) as Graphics.Pipeline.Shader.TtShaderAssetAMeta;
                        if (ameta1 == null)
                            continue;

                        if (ameta1.ShaderType != ShaderType)
                        {
                            continue;
                        }
                    }
                }
                

                var filterName = IO.TtFileManager.GetPureName(name);
                if (!string.IsNullOrEmpty(FilterText))
                {
                    if (filterName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == false)
                        continue;
                }

                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType));
                if (ameta == null)
                    continue;
                var assetTypeName = ameta.GetAssetTypeName();
                if ((mActiveFiltersCount > 0) && !((UIProxy.MenuItemProxy)mFilterMenus[assetTypeName]).Selected)
                    continue;

                DrawItem(in cmdlist, ameta.Icon, ameta, in itemSize, drawIndex++, ItemScale);
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

            var metafiles = IO.TtFileManager.GetFiles(dir.Address, "*.metadata", false);
            if (metafiles.Length > 0)
            {
                var ameta = new Rtti.TtMetaVersionMeta();
                ameta.TypeStr = Rtti.TtTypeDescGetter<Rtti.TtMetaVersion>.TypeDesc.TypeString;
                foreach (var i in metafiles)
                {
                    var name = IO.TtFileManager.GetPureName(i);

                    var rootType = TtEngine.Instance.FileManager.GetRootDirType(i);
                    var rPath = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(rootType), i);
                    if (rootType == IO.TtFileManager.ERootDir.Game)
                        ameta.SetAssetName(RName.GetRName(rPath, RName.ERNameType.Game));
                    else if (rootType == IO.TtFileManager.ERootDir.Engine)
                        ameta.SetAssetName(RName.GetRName(rPath, RName.ERNameType.Engine));
                    else
                        continue;

                    DrawItem(in cmdlist, ameta.Icon, ameta, in itemSize, drawIndex++, ItemScale);
                    curPos += itemSize.X + style->ItemSpacing.X;
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
            ImGuiAPI.PopStyleVar(1);
            ImGuiAPI.PopStyleColor(3);
            //cmdlist.PopClipRect();


            if (mShiftSelection)
            {
                LastClickIndex = FirstClickIndex;
                mShiftSelection = false;
            }

            ////////////////////////////////////////////////////////////
            //drawList.AddRect(ref min, ref max, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
            ////////////////////////////////////////////////////////////

            if (CreateNewAssets && ImGuiAPI.BeginPopupContextWindow("##ContentFilesMenuWindow", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                var popMenuWidth = ImGuiAPI.GetWindowContentRegionWidth();
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mContextMenuFilterFocused, in drawList, "search items", ref mContextMenuFilterStr, popMenuWidth);
                var wsize = new Vector2(200, 400);
                if (ImGuiAPI.BeginChild("GraphContextMenu", in wsize, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
                {
                    for (var childIdx = 0; childIdx < mContextMenu.SubMenuItems.Count; childIdx++)
                    {
                        TtMenuItem.Draw(mContextMenu.SubMenuItems[childIdx], this, this, mContextMenuFilterStr, in drawList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, null);
                    }
                }
                ImGuiAPI.EndChild();
                ImGuiAPI.EndPopup();

                mContextMenuOpenCheck = true;
            }
            else
            {
                mContextMenuOpenCheck = false;
            }
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

        private unsafe void DrawItem(in ImDrawList cmdlist, TtUVAnim icon, IO.IAssetMeta ameta, in Vector2 sz, int index, float scale)
        {
            ImGuiAPI.PushID($"##{ameta.GetAssetName().Name}");
            bool isSelected = false;
            ImGuiAPI.Selectable("", ref isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz);
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
            if (ImGuiAPI.IsItemVisible())
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
                            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                            if (mainEditor != null)
                            {
                                var type = Rtti.TtTypeDesc.TypeOf(ameta.TypeStr).SystemType;
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
                        else if(ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
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
                }
                ameta.ShowIconTime = TtEngine.Instance.CurrentTickCountUS;

                ameta.OnDraw(in cmdlist, in sz, this, scale);

                if (ImGuiAPI.BeginDragDropSource(ImGuiDragDropFlags_.ImGuiDragDropFlags_SourceNoDisableHover))
                {
                    IsInDragDropMode = true;
                    if(!ameta.IsSelected)
                    {
                        for(int i=0; i<SelectedAssets.Count; i++)
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
                        if(SelectedAssets[i].CanDrawOnDragging())
                            drawCount++;
                    }
                    var dragDropCmdlist = ImGuiAPI.GetWindowDrawList();
                    var offsetOri = new Vector2(16, 16);
                    var offsetDelta = 1.0f;
                    if(drawCount > 0)
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
                            for(int i = totalCount - 1; i>=0; i--)
                            {
                                if(i != (totalCount - 1))
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
            ImGuiAPI.PopID();

            if (mShiftSelection)
            {
                var min = Math.Min(FirstClickIndex, LastClickIndex);
                var max = Math.Max(FirstClickIndex, LastClickIndex);
                if(index >= min && index <= max)
                {
                    ameta.IsSelected = true;
                    SelectedAssets.Add(ameta);
                }
            }
        }
        Dictionary<string, UIProxy.IUIProxyBase> mFilterMenus = new Dictionary<string, UIProxy.IUIProxyBase>();
        int mActiveFiltersCount = 0;
        bool mWithChildFolders = false;
        void InitializeFilterMenu()
        {
            mFilterMenus.Clear();
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
                }
            };
            mFilterMenus["##show child"] = new UIProxy.MenuItemProxy()
            {
                MenuName = "With child folders",
                Action = (item, data)=>
                {
                    mWithChildFolders = !mWithChildFolders;
                    item.Selected = mWithChildFolders;
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
                            },
                        };
                        mFilterMenus[name] = menu;
                    }
                }
            }
        }
        void DrawFilterMenu()
        {
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
            EGui.UIProxy.Toolbar.EndToolbar();
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
        IO.IAssetCreateAttribute mAssetImporter;
        public string CurrentImporterFile;
        public bool DrawInWindow = true;

        internal System.Threading.Tasks.Task AssetOpTask = null;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            if (AssetOpTask != null && AssetOpTask.IsCompleted)
                AssetOpTask = null;

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
                var cmd = ImGuiAPI.GetWindowDrawList();
                var style = ImGuiAPI.GetStyle();
                DrawToolbar(cmd);

                //                if (ImGuiAPI.IsWindowDocked())
                //                {
                //                    DockId = ImGuiAPI.GetWindowDockID();
                //                }
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
                            FilterText = mSearchBar.SearchText;
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

                    if (ImGuiAPI.BeginChild("RightWindow", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                    {
                        var min = ImGuiAPI.GetWindowContentRegionMin();
                        var max = ImGuiAPI.GetWindowContentRegionMax();
                        RightSize = max - min;

                        if(TtEngine.Instance.InputSystem.IsDropFiles)
                        {
                            var pos = ImGuiAPI.GetWindowPos();
                            var size = ImGuiAPI.GetWindowSize();
                            var mousePos = new Vector2(TtEngine.Instance.InputSystem.Mouse.GlobalMouseX, TtEngine.Instance.InputSystem.Mouse.GlobalMouseY);
                            if(mousePos.X >= pos.X && mousePos.X <= (pos.X + size.X) &&
                               mousePos.Y >= pos.Y && mousePos.Y <= (pos.Y + size.Y))
                            {
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

                                for (int i = 0; i < TtEngine.Instance.InputSystem.DropFiles.Count; i++)
                                {
                                    var file = TtEngine.Instance.InputSystem.DropFiles[i];
                                    var fileExt = IO.TtFileManager.GetExtName(file);
                                    for (int j = 0; j < importers.Count; j++)
                                    {
                                        try
                                        {
                                            var attrs = importers[j].GetCustomAttributes(typeof(IO.IAssetCreateAttribute), true);
                                            if (((IO.IAssetCreateAttribute)(attrs[0])).IsAssetSource(fileExt))
                                            {
                                                var assetExtField = Rtti.TtTypeDesc.GetField(importers[j].SystemType, "AssetExt");
                                                EnqueueAssetImporter(TtEngine.Instance.AssetMetaManager.ImportAsset(mFolderView.CurrentDir, importers[j], (string)assetExtField.GetValue(null)), file);
                                                break;
                                            }
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                }
                                TtEngine.Instance.InputSystem.ClearFilesDrop();
                            }
                        }

                        DrawFiles(mFolderView.CurrentDir, in RightSize);

                    }
                    ImGuiAPI.EndChild();
                }
                    
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
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
                    mAssetImporter = null;
                    CurrentImporterFile = "";
                }
            }
            else if(mAssetImporterQueue.Count > 0)
            {
                var importer = mAssetImporterQueue.Dequeue();
                mAssetImporter = importer.Creater;
                CurrentImporterFile = importer.FileName;
            }

        }

        public enum EAssetOperationType
        {
            None,
            MoveTo,
            CopyTo,
            Rename,
        }
        EAssetOperationType mOperationType = EAssetOperationType.None;
        IAssetMeta mOperationAsset = null;
        FolderView mSelectFolderView = new FolderView();
        float mSelectFolderOKButtonHeight = 20;
        public void OperationAsset(IAssetMeta sourceAsset, EAssetOperationType opType)
        {
            mOperationAsset = sourceAsset;
            mOperationType = opType;
            NewName = mOperationAsset.GetAssetName().PureName;
        }
        string NewName;
        unsafe void DrawAssetOperationDialog()
        {
            if (mOperationAsset == null || mOperationType == EAssetOperationType.None)
                return;

            var sourceName = mOperationAsset.GetAssetName();
            string keyName = "AssetOperation";
            switch (mOperationType)
            {
                case EAssetOperationType.MoveTo:
                    keyName = $"Move {sourceName.Name} To";
                    break;
                case EAssetOperationType.CopyTo:
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
            if(ImGuiAPI.BeginPopupModal(keyName, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
            {
                if(ImGuiAPI.BeginChild(keyName + "CWin", in Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
                {
                    var winSize = new Vector2(ImGuiAPI.GetWindowWidth(), ImGuiAPI.GetWindowHeight() - mSelectFolderOKButtonHeight);
                    if (mOperationType == EAssetOperationType.MoveTo ||
                        mOperationType == EAssetOperationType.CopyTo)
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
                            if (AssetOpTask == null || AssetOpTask.IsCompleted)
                            {
                                switch(mOperationType)
                                {
                                    case EAssetOperationType.MoveTo:
                                        {
                                            var name = mSelectFolderView.CurrentDir.Address + sourceName.PureName + sourceName.ExtName;
                                            AssetOpTask = mOperationAsset.MoveTo(name, sourceName.RNameType);
                                        }
                                        break;
                                    case EAssetOperationType.CopyTo:
                                        {
                                            var name = mSelectFolderView.CurrentDir.Address + sourceName.PureName + sourceName.ExtName;
                                            AssetOpTask = mOperationAsset.CopyTo(name, sourceName.RNameType);
                                        }
                                        break;
                                    case EAssetOperationType.Rename:
                                        {
                                            var dir = sourceName.Name.Substring(0, sourceName.Name.Length - sourceName.PureName.Length - sourceName.ExtName.Length);
                                            var name = dir + NewName + sourceName.ExtName;
                                            AssetOpTask = mOperationAsset.RenameTo(name, sourceName.RNameType);
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
                ImGuiAPI.EndPopup();

            }
        }
    }
}
