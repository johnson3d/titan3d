using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.EGui.Controls;
using EngineNS.Thread;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.IO
{
    //资源导入引擎的接口
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class IAssetCreateAttribute : Attribute
    {
        public virtual async Thread.Async.TtTask DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
        {

        }
        //导入的窗口渲染接口
        public virtual bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
        {
            return true;
        }
        public virtual bool IsAssetSource(string fileExt)
        {
            return false;
        }
        public virtual void ImportSource(string sourceFile, RName dir)
        {
            throw new NotImplementedException("Need override this method!");
        }
    }
    public class CommonCreateAttribute : IO.IAssetCreateAttribute
    {
        public enum enErrorType
        {
            None = 0,
            IsExisting,
            EmptyName,
        };
        protected enErrorType eErrorType = enErrorType.None;
        protected bool bPopOpen = false;
        protected string ExtName;
        protected RName mDir;
        protected IAsset mAsset;
        protected string mName;
        protected EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
        protected EGui.Controls.UTypeSelector TypeSlt = new EGui.Controls.UTypeSelector();
        protected Thread.Async.TtTask<bool> PGAssetInitTask;
        public RName GetAssetRName()
        {
            if (mName == null)
                return null;
            return RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
        }
        public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
        {
            ExtName = ext;
            mName = null;
            mDir = dir;
            TypeSlt.BaseType = type;
            TypeSlt.SelectedType = type;

            PGAssetInitTask = PGAsset.Initialize();
            mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IAsset;
            PGAsset.Target = mAsset;
        }
        public override unsafe bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
        {
            if (bPopOpen == false)
                ImGuiAPI.OpenPopup($"New {TypeSlt.BaseType.Name}", ImGuiPopupFlags_.ImGuiPopupFlags_None);

            bool retValue = false;
            var visible = true;

            float windowWidth = 200;
            float windowHeight = 500;
            //float posX = (ImGuiAPI.GetIO().DisplaySize.X - windowWidth) * 0.5f;
            //float posY = (ImGuiAPI.GetIO().DisplaySize.Y - windowHeight) * 0.5f;
            ImGuiAPI.SetNextWindowPos(new Vector2(0, 0), ImGuiCond_.ImGuiCond_FirstUseEver, new Vector2(0, 0));
            ImGuiAPI.SetNextWindowSize(new Vector2(windowWidth, windowHeight), ImGuiCond_.ImGuiCond_FirstUseEver);
            if (ImGuiAPI.BeginPopupModal($"New {TypeSlt.BaseType.Name}", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiAPI.Text($"New {TypeSlt.BaseType.Name}");
                switch (eErrorType)
                {
                    case enErrorType.IsExisting:
                        {
                            var clr = new Vector4(1, 0, 0, 1);
                            ImGuiAPI.TextColored(&clr, $"{mName} is existing");
                        }
                        break;
                    case enErrorType.EmptyName:
                        {
                            var clr = new Vector4(1, 0, 0, 1);
                            ImGuiAPI.TextColored(&clr, $"Name is empty");
                        }
                        break;
                }

                var saved = TypeSlt.SelectedType;
                TypeSlt.OnDraw(-1, 6);
                if (TypeSlt.SelectedType != saved)
                {
                    mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IAsset;
                    PGAsset.Target = mAsset;
                }

                var sz = new Vector2(0, 0);

                bool nameChanged = ImGuiAPI.InputText("##in_rname", ref mName);
                eErrorType = enErrorType.None;
                if (string.IsNullOrEmpty(mName))
                {
                    eErrorType = enErrorType.EmptyName;
                }
                else if (nameChanged)
                {
                    var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                    if (mAsset != null)
                    {
                        mAsset.AssetName = rn;
                    }
                    if (IO.TtFileManager.FileExists(rn.Address))
                        eErrorType = enErrorType.IsExisting;
                }

                ImGuiAPI.Separator();

                if (CheckAsset())
                {
                    if (ImGuiAPI.Button("Create Asset", &sz))
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if (IO.TtFileManager.FileExists(rn.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
                        {
                            if (DoImportAsset())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                    }
                    ImGuiAPI.SameLine(0, 20);
                }
                if (ImGuiAPI.Button("Cancel", &sz))
                {
                    ImGuiAPI.CloseCurrentPopup();
                    retValue = true;
                }

                if (!PGAssetInitTask.IsCompleted)
                {
                }
                else
                {
                    PGAsset.OnDraw(false, false, false);
                }
                
                ImGuiAPI.EndPopup();
            }
            return retValue;
        }
        protected virtual bool CheckAsset()
        {
            return true;
        }
        protected virtual bool DoImportAsset()
        {
            var ameta = mAsset.CreateAMeta();
            ameta.SetAssetName(mAsset.AssetName);
            ameta.AssetId = Guid.NewGuid();
            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mAsset.GetType());
            ameta.Description = $"This is a {mAsset.GetType().FullName}\n";
            ameta.SaveAMeta();

            UEngine.Instance.AssetMetaManager.RegAsset(ameta);

            mAsset.SaveAssetTo(mAsset.AssetName);
            return true;
        }
    }
    public class AssetCreateMenuAttribute : Attribute
    {
        public string MenuName = "Unknow"; // menuParent/menuChild/menuSubChild
        public string Shortcut = null;
    }
    public interface IAsset
    {
        RName AssetName { get; set; }
        IAssetMeta CreateAMeta();
        IAssetMeta GetAMeta();
        void UpdateAMetaReferences(IAssetMeta ameta);
        void SaveAssetTo(RName name);
    }
    public enum EAssetState
    {
        Initialized,
        LoadFinished,
        Loading,
        LoadFailed,
    }
    [Rtti.Meta]
    public partial class IAssetMeta
    {
        public static readonly string MetaExt = ".ameta";
        protected RName mAssetName;
        public bool HasSnapshot { get; set; } = true;
        public bool IsSelected = false;
        public IAssetMeta()
        {
            mDeleteMenuState.Reset();
            mDeleteMenuState.HasIndent = false;
            mRefGraphMenuState.Reset();
            mRefGraphMenuState.HasIndent = false;
            mExplorerToMenuState.Reset();
            mExplorerToMenuState.HasIndent = false;
            mCopyRNameMenuState.Reset();
            mCopyRNameMenuState.HasIndent = false;
            mMoveToMenuState.Reset();
            mMoveToMenuState.HasIndent = false;
            mCopyToMenuState.Reset();
            mCopyToMenuState.HasIndent = false;
        }
        public virtual string GetAssetTypeName()
        {
            throw new NotImplementedException("Need override this method!");
        }
        public virtual string GetAssetExtType()
        {
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
        public virtual void Cleanup()
        {
            if (Task != null)
            {
                if (Task.Result != null)
                    Task.Result.mTextureRSV = null;
                Task = null;
            }
        }
        public virtual async System.Threading.Tasks.Task<IAsset> LoadAsset()
        {
            System.Diagnostics.Debug.Assert(false);
            await Thread.TtAsyncDummyClass.DummyFunc();
            return null;
        }
        public virtual void OnBeforeRenamedAsset(IAsset asset, RName name)
        {
            //Manager.RemapAsset
        }
        public virtual void OnAfterRenamedAsset(IAsset asset, RName name)
        {
            //Manager.RemapAsset
        }
        public virtual void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.TtFileManager.DeleteFile(address);
            IO.TtFileManager.DeleteFile(address + MetaExt);
            IO.TtFileManager.DeleteFile(address + ".snap");
        }
        public virtual async System.Threading.Tasks.Task MoveTo(string name, RName.ERNameType type)
        {
            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;
            IAsset asset = await LoadAsset();
            List<EngineNS.IO.IAssetMeta> holders = new List<EngineNS.IO.IAssetMeta>();
            UEngine.Instance.AssetMetaManager.GetAssetHolder(this, holders);
            List<EngineNS.IO.IAsset> holdAssets = new List<EngineNS.IO.IAsset>();
            foreach (var i in holders)
            {
                var holdAsset = await i.LoadAsset();
                if (holdAsset != null)
                {
                    holdAssets.Add(holdAsset);
                }
            }

            var savedName = mAssetName.Name;
            var savedType = mAssetName.RNameType;
            OnBeforeRenamedAsset(asset, mAssetName);
            
            var tarAddress = RName.GetAddress(type, name);
            IO.TtFileManager.CopyFile(mAssetName.Address + ".snap", tarAddress + ".snap");

            mAssetName.UnsafeUpdate(name, type);
            this.SaveAMeta();
            asset.SaveAssetTo(mAssetName);
            
            OnAfterRenamedAsset(asset, mAssetName);

            foreach (var i in holdAssets)
            {
                i.SaveAssetTo(i.GetAMeta().GetAssetName());
            }

            DeleteAsset(savedName, savedType);
        }
        public virtual async System.Threading.Tasks.Task CopyTo(string name, RName.ERNameType type)
        {
            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;
            IAsset asset = await LoadAsset();
            if (asset == null)
                return;
            var tarName = RName.GetRName(name, type);
            var ameta = UEngine.Instance.AssetMetaManager.NewAMeta(tarName, asset.GetAMeta().GetType());
            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(asset.GetType());
            foreach (var i in this.RefAssetRNames)
            {
                ameta.RefAssetRNames.Add(i);
            }

            ameta.SaveAMeta();
            asset.SaveAssetTo(tarName);

            IO.TtFileManager.MoveFile(mAssetName.Address + ".snap", tarName.Address + ".snap");
        }
        public virtual async System.Threading.Tasks.Task RenameTo(string name, RName.ERNameType type)
        {
            if (name == mAssetName.Name && type == mAssetName.RNameType)
                return;
            await MoveTo(name, type);
        }
        public virtual void ResetSnapshot()
        {
            HasSnapshot = true;

            OnShowIconTimout(0);
        }
        public void SetAssetName(RName rn)
        {
            mAssetName = rn;
            System.Diagnostics.Debug.Assert(rn.Name.EndsWith(MetaExt) == false);
        }
        public RName GetAssetName()
        {
            return mAssetName;
        }
        public void SaveAMeta()
        {
            if (mAssetName != null)
            {
                var fileName = mAssetName.Address + MetaExt;
                SaveAMeta(fileName);
            }
        }

        public void SaveAMeta(string fileName)
        {
            IO.TtFileManager.SaveObjectToXml(fileName, this);
            UEngine.Instance.SourceControlModule.AddFile(fileName, true);
        }
        public virtual bool CanRefAssetType(IAssetMeta ameta)
        {
            return true;
        }
        protected EGui.UIProxy.MenuItemProxy.MenuState mExplorerToMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mRefGraphMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mCopyRNameMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mDeleteMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mMoveToMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mCopyToMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        internal System.Threading.Tasks.Task<Editor.USnapshot> Task;
        protected virtual Color4b GetBorderColor()
        {
            return EGui.UCoreStyles.Instance.SnapBorderColor;
        }
        static Vector2 tempDelta0 = new Vector2(8, 8);
        static Vector2 tempDelta = new Vector2(12, 15);
        public virtual unsafe void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        {
            OnDraw(in cmdlist, in Vector2.Zero, in sz, ContentBrowser);
        }
        public virtual unsafe void OnDraw(in ImDrawList cmdlist, in Vector2 offset, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        {
            var snapSize = sz.X * 0.9f;
            var start = ImGuiAPI.GetItemRectMin() + offset;
            var end = start + sz;

            var shadowImg = UEngine.Instance.UIProxyManager[TtAssetMetaManager.ItemShadowImgName] as EGui.UIProxy.ImageProxy;
            if (shadowImg != null)
                shadowImg.OnDraw(cmdlist, start - tempDelta0, end + tempDelta);
            var color = 0xff383838;
            var typeFontColor = 0xff757575;
            var nameColor = EGui.UIProxy.StyleConfig.Instance.TextColor;
            if (IsSelected)
            {
                color = EGui.UIProxy.StyleConfig.Instance.TVHeaderActive;
                typeFontColor = EGui.UIProxy.StyleConfig.Instance.TextColor;
                nameColor = 0xFFFFFFFF;
            }
            else if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_RectOnly) && 
                ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_DockHierarchy))
                color = EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered;
            cmdlist.AddRectFilled(start, end, color, EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll);

            var delta = (sz.X - snapSize) * 0.5f;
            var typeName = GetAssetTypeName();
            //var tsz = ImGuiAPI.CalcTextSize(typeName, false, -1);
            Vector2 tpos;
            tpos.X = start.X + delta;
            tpos.Y = start.Y + delta;
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)EGui.Slate.UBaseRenderer.enFont.Font_13px);
            cmdlist.AddText(in tpos, typeFontColor, typeName, null);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();

            //ImGuiAPI.PushClipRect(in start, in end, true);

            var snapStart = new Vector2(start.X + delta, tpos.Y + 18);
            var snapEnd = snapStart + new Vector2(snapSize, snapSize);
            OnDrawSnapshot(in cmdlist, ref snapStart, ref snapEnd);

            //var titleImg = UEngine.Instance.UIManager.GetUIProxy("uestyle/graph/regularnode_shadow_selected.srv", new Thickness(18.0f / 64.0f)) as EGui.UIProxy.ImageProxy;
            //if (titleImg != null)
            //    titleImg.OnDraw(cmdlist, in start, in end);

            cmdlist.AddRect(in snapStart, in snapEnd, (uint)GetBorderColor().ToAbgr(), 
                0.0f, ImDrawFlags_.ImDrawFlags_None, EGui.UCoreStyles.Instance.SnapThinkness);

            var name = IO.TtFileManager.GetPureName(GetAssetName().Name, 11);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            tpos.Y = snapEnd.Y + 8;
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)EGui.Slate.UBaseRenderer.enFont.Font_15px);
            cmdlist.AddText(in tpos, nameColor, name, null);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();
            //ImGuiAPI.PopClipRect();


            DrawPopMenu(ContentBrowser);
        }
        protected void DrawPopMenu(EGui.Controls.UContentBrowser ContentBrowser)
        {
            if (ImGuiAPI.BeginPopupContextItem(mAssetName.Address, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                OnDrawPopMenu(ContentBrowser);
                ImGuiAPI.EndPopup();
            }
            else
            {
                var createNewAssetValueStore = ContentBrowser.CreateNewAssets;
                ContentBrowser.CreateNewAssets = createNewAssetValueStore;
            }
        }
        protected virtual void OnDrawPopMenu(EGui.Controls.UContentBrowser ContentBrowser)
        {
            var createNewAssetValueStore = ContentBrowser.CreateNewAssets;
            ContentBrowser.CreateNewAssets = false;
            var drawList = ImGuiAPI.GetWindowDrawList();
            Support.UAnyPointer menuData = new Support.UAnyPointer();

            if (EGui.UIProxy.MenuItemProxy.MenuItem("ExplorerTo", null, false, null, in drawList, in menuData, ref mExplorerToMenuState))
            {
                var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + mAssetName.Address.Replace("/", "\\");
                System.Diagnostics.Process.Start(psi);
            }
            if (EGui.UIProxy.MenuItemProxy.MenuItem("RefGraph", null, false, null, in drawList, in menuData, ref mRefGraphMenuState))
            {
                var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                var rn = RName.GetRName(mAssetName.Name + ".ameta", mAssetName.RNameType);
                var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, typeof(Editor.Forms.UAssetReferViewer), rn, this);
            }
            if (EGui.UIProxy.MenuItemProxy.MenuItem("CopyRName", null, false, null, in drawList, in menuData, ref mCopyRNameMenuState))
            {
                ImGuiAPI.SetClipboardText(RName.GetRName(mAssetName.Name + ".ameta", mAssetName.RNameType).ToString());
            }
            ImGuiAPI.Separator();
            if (EGui.UIProxy.MenuItemProxy.MenuItem("Delete", null, false, null, in drawList, in menuData, ref mDeleteMenuState))
            {
                try
                {
                    DeleteAsset(mAssetName.Name, mAssetName.RNameType);
                }
                catch
                {

                }
                ContentBrowser.CreateNewAssets = createNewAssetValueStore;
            }
            if (EGui.UIProxy.MenuItemProxy.MenuItem("Rename", null, false, null, in drawList, in menuData, ref mMoveToMenuState))
            {
                ContentBrowser.OperationAsset(this, EGui.Controls.UContentBrowser.EAssetOperationType.Rename);
            }
            if (EGui.UIProxy.MenuItemProxy.MenuItem("MoveTo", null, false, null, in drawList, in menuData, ref mMoveToMenuState))
            {
                ContentBrowser.OperationAsset(this, EGui.Controls.UContentBrowser.EAssetOperationType.MoveTo);
            }
            if (EGui.UIProxy.MenuItemProxy.MenuItem("CopyTo", null, false, null, in drawList, in menuData, ref mCopyToMenuState))
            {
                ContentBrowser.OperationAsset(this, EGui.Controls.UContentBrowser.EAssetOperationType.CopyTo);
            }

            if (OnDrawContextMenu(ref drawList))
                ContentBrowser.CreateNewAssets = createNewAssetValueStore;
        }
        protected virtual bool OnDrawContextMenu(ref ImDrawList cmdlist)
        {
            return false;
        }
        public virtual void OnShowIconTimout(int time)
        {
            if (Task != null && Task.IsCompleted)
            {
                Task.Result?.mTextureRSV?.Dispose();
                Task = null;
            }
        }
        public unsafe virtual void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            if (HasSnapshot == false)
                return;
            
            if (Task == null)
            {
                Task = Editor.USnapshot.Load(GetAssetName().Address + ".snap");
                return;
            }
            else if (Task.IsCompleted == true)
            {
                if (Task.Result == null)
                {
                    HasSnapshot = false;
                    Task = null;
                }
                else
                {
                    cmdlist.AddImage(Task.Result.mTextureRSV.GetTextureHandle().ToPointer(), in start, in end, in Vector2.Zero, in Vector2.One, 0xFFFFFFFF);
                }
            }
        }
        public virtual async Thread.Async.TtTask<bool> AutoGenSnap()
        {
            return true;
        }
        [Rtti.Meta]
        public EGui.TtUVAnim Icon
        {
            get;
            set;
        } = new EGui.TtUVAnim();
        [Rtti.Meta]
        public string TypeStr { get; set; }
        [Rtti.Meta]
        public string Description { get; set; } = "This is a Asset";
        [Rtti.Meta]
        public Guid AssetId { get; set; }
        [Rtti.Meta]
        public List<RName> RefAssetRNames { get; set; } = new List<RName>();

        public long ShowIconTime;

        public void AddReferenceAsset(RName rn)
        {
            if (rn == null)
                return;
            if (RefAssetRNames.Contains(rn))
                return;
            RefAssetRNames.Add(rn);
        }
        bool mDraggingInViewport = false;
        public virtual bool DraggingInViewport
        {
            get => mDraggingInViewport;
            set
            {
                mDraggingInViewport = value;
            }
        }
        public virtual async Thread.Async.TtTask OnDragTo(Graphics.Pipeline.TtViewportSlate vpSlate)
        {
            DraggingInViewport = false;
            await TtAsyncDummyClass.DummyFunc();
        }
        public virtual async Thread.Async.TtTask OnDragging(Graphics.Pipeline.TtViewportSlate vpSlate)
        {
            await TtAsyncDummyClass.DummyFunc();
        }
        public virtual bool CanDrawOnDragging()
        {
            if (DraggingInViewport)
                return false;
            return true;
        }

        public virtual void DrawTooltip()
        {
            var name = GetAssetName();
            CtrlUtility.DrawHelper(
                "Name: " + name.Name,
                "Desc: " + Description,
                "Address: " + name.Address);
        }
    }
    public class TtAssetMetaManager
    {
        ~TtAssetMetaManager()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            foreach (var i in Assets)
            {
                i.Value.OnShowIconTimout(-1);
                i.Value.ShowIconTime = 0;

                i.Value.Cleanup();
            }
            Assets.Clear();

            RNameAssets.Clear();
        }
        public Dictionary<Guid, IAssetMeta> Assets { get; } = new Dictionary<Guid, IAssetMeta>();
        public Dictionary<RName, IAssetMeta> RNameAssets { get; } = new Dictionary<RName, IAssetMeta>();
        public void RemoveAMeta(IAssetMeta ameta)
        {
            Assets.Remove(ameta.AssetId);
            RNameAssets.Remove(ameta.GetAssetName());
        }
        public IAssetMeta GetAssetMeta(Guid id)
        {
            IAssetMeta result;
            if (Assets.TryGetValue(id, out result))
                return result;
            return null;
        }
        public IAssetMeta GetAssetMeta(RName name)
        {
            if (name == null)
                return null;
            IAssetMeta result;
            if (RNameAssets.TryGetValue(name, out result))
                return result;
            return null;
        }
        public static string ItemShadowImgName = "uestyle/content/uniformshadow.srv";
        public void LoadMetas()
        {
            var root = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine);
            var metas = IO.TtFileManager.GetFiles(RName.GetRName("", RName.ERNameType.Engine).Address, "*" + IAssetMeta.MetaExt, true);
            foreach(var i in metas)
            {
                var m = IO.TtFileManager.LoadXmlToObject(i) as IAssetMeta;
                if (m == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{i} is not a IAssetMeta");
                    continue;
                }
                var rn = IO.TtFileManager.GetRelativePath(root, i);
                m.SetAssetName(RName.GetRName(rn.Substring(0, rn.Length - 6), RName.ERNameType.Engine));
                IAssetMeta om;
                if (Assets.TryGetValue(m.AssetId, out om))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{m.RefAssetRNames} ID repeat:{om.GetAssetName()}");
                    continue;
                }
                Assets.Add(m.AssetId, m);
                RNameAssets[m.GetAssetName()] = m;
            }

            root = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            metas = IO.TtFileManager.GetFiles(RName.GetRName("", RName.ERNameType.Game).Address, "*" + IAssetMeta.MetaExt, true);
            foreach (var i in metas)
            {
                var m = IO.TtFileManager.LoadXmlToObject<IAssetMeta>(i);
                if(m == null)
                {
                    continue;
                }
                var rn = IO.TtFileManager.GetRelativePath(root, i);
                m.SetAssetName(RName.GetRName(rn.Substring(0, rn.Length - 6), RName.ERNameType.Game));
                IAssetMeta om;
                if (Assets.TryGetValue(m.AssetId, out om))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{m.RefAssetRNames} ID repeat:{om.GetAssetName()}");
                    continue;
                }
                Assets.Add(m.AssetId, m);
                RNameAssets[m.GetAssetName()] = m;
            }

            if (UEngine.Instance.UIProxyManager[ItemShadowImgName] == null)
                UEngine.Instance.UIProxyManager[ItemShadowImgName] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(ItemShadowImgName, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f));
        }
        public T NewAsset<T>(RName name) where T : class, IAsset, new()
        {
            IAssetMeta ameta;
            if (RNameAssets.TryGetValue(name, out ameta))
            {
                return null;
            }
            var asset = new T();
            asset.AssetName = name;
            ameta = asset.CreateAMeta();
            ameta.SetAssetName(name);
            ameta.AssetId = Guid.NewGuid();
            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(T));
            ameta.SaveAMeta();
            Assets.Add(ameta.AssetId, ameta);
            RNameAssets.Add(ameta.GetAssetName(), ameta);

            return asset;
        }
        public IAsset NewAsset(RName name, System.Type type)
        {
            IAssetMeta ameta;
            if (RNameAssets.TryGetValue(name, out ameta))
            {
                return null;
            }
            var asset = Rtti.UTypeDescManager.CreateInstance(type) as IAsset;
            if (asset == null)
                return null;
            asset.AssetName = name;
            ameta = asset.CreateAMeta();
            ameta.SetAssetName(name);
            ameta.AssetId = Guid.NewGuid();
            ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(type);
            ameta.SaveAMeta();
            Assets.Add(ameta.AssetId, ameta);
            RNameAssets.Add(ameta.GetAssetName(), ameta);

            return asset;
        }
        public IAssetMeta NewAMeta(RName name, System.Type metaType)
        {
            IAssetMeta ameta;
            if (RNameAssets.TryGetValue(name, out ameta))
            {
                return null;
            }
            ameta = Rtti.UTypeDescManager.CreateInstance(metaType) as IAssetMeta;
            ameta.SetAssetName(name);
            ameta.AssetId = Guid.NewGuid();
            //ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(type);
            ameta.SaveAMeta();
            Assets.Add(ameta.AssetId, ameta);
            RNameAssets.Add(ameta.GetAssetName(), ameta);
            return ameta;
        }
        public IAssetCreateAttribute ImportAsset(RName dir, Rtti.UTypeDesc type, string ext)
        {
            var attrs = type.GetCustomAttributes(typeof(IAssetCreateAttribute), true);
            if (attrs.Length > 0)
            {
                var importer = attrs[0] as IAssetCreateAttribute;
                _ = importer.DoCreate(dir, type, ext);
                return importer;
            }
            return null;
        }
        public async Thread.Async.TtTask<IAssetCreateAttribute> AwaitImportAsset(RName dir, Rtti.UTypeDesc type, string ext)
        {
            var attrs = type.GetCustomAttributes(typeof(IAssetCreateAttribute), true);
            if (attrs.Length > 0)
            {
                var importer = attrs[0] as IAssetCreateAttribute;
                await importer.DoCreate(dir, type, ext);
                return importer;
            }
            return null;
        }
        public bool RegAsset(IAssetMeta ameta)
        {
            if (Assets.ContainsKey(ameta.AssetId) ||
                RNameAssets.ContainsKey(ameta.GetAssetName()) )
            {
                return false;
            }
            Assets.Add(ameta.AssetId, ameta);
            RNameAssets.Add(ameta.GetAssetName(), ameta);
            return true;
        }
        public void GetAssetHolder(IAssetMeta ameta, List<IAssetMeta> holders)
        {
            foreach(var i in Assets)
            {
                if (i.Value.CanRefAssetType(ameta) == false)
                    continue;
                if (i.Value.RefAssetRNames.Contains(ameta.GetAssetName()))
                {
                    holders.Add(i.Value);
                }
            }
        }
        public async System.Threading.Tasks.Task GetAssetHolder(IAssetMeta ameta, Dictionary<RName, IAsset> holders)
        {
            foreach (var i in Assets)
            {
                if (i.Value.CanRefAssetType(ameta) == false)
                    continue;
                var name = i.Value.GetAssetName();
                if (holders.ContainsKey(name))
                {
                    continue;
                }
                if (i.Value.RefAssetRNames.Contains(ameta.GetAssetName()))
                {
                    var ast = await i.Value.LoadAsset();
                    holders.Add(name, ast);
                }
            }
        }
        public long LastestCheckTime = 0;
        public void EditorCheckShowIconTimeout()
        {
            var tickCount = UEngine.Instance.CurrentTickCountUS;
            if(tickCount - LastestCheckTime > 15*1000 * 1000)
            {
                LastestCheckTime = tickCount;
            }
            else
            {
                return;
            }
            foreach(var i in Assets)
            {
                if (i.Value.ShowIconTime != 0 && (int)(tickCount - i.Value.ShowIconTime) > 1000 * 1000 * 15)
                {//15秒没有被预览，通知AssetMeta做资源卸载处理
                    i.Value.OnShowIconTimout((int)((tickCount - i.Value.ShowIconTime) / 1000 * 1000));
                    i.Value.ShowIconTime = 0;
                }
            }
        }
    }

    public class TtAssetManager
    {
        public Dictionary<RName, WeakReference<IAsset>> Assets { get; } = new Dictionary<RName, WeakReference<IAsset>>();
        public async Thread.Async.TtTask<IAsset> GetAsset(RName rn)
        {
            IAsset asset;
            WeakReference<IAsset> result;
            lock (Assets)
            {
                if (Assets.TryGetValue(rn, out result))
                {
                    if (result.TryGetTarget(out asset))
                    {
                        return asset;
                    }
                    else
                    {
                        Assets.Remove(rn);
                    }
                }
            }
            var meta = UEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
            if (meta == null)
                return null;
            asset = await meta.LoadAsset();
            lock (Assets)
            {
                Assets[rn] = new WeakReference<IAsset>(asset);
                return asset;
            }
        }
        public async Thread.Async.TtTask<T> GetAsset<T>(RName rn) where T : class, IAsset
        {
            return GetAsset(rn) as T;
        }
        List<RName> mRmvAssets = new List<RName>();
        public void TickSync()
        {
            mRmvAssets.Clear();
            lock (Assets)
            {   
                foreach (var i in Assets)
                {
                    if (i.Value.TryGetTarget(out var asset) == false)
                    {
                        mRmvAssets.Add(i.Key);
                    }
                }
                foreach (var i in mRmvAssets)
                {
                    Assets.Remove(i);
                }
            }
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public IO.TtAssetMetaManager AssetMetaManager { get; } = new IO.TtAssetMetaManager();
        public IO.TtAssetManager AssetManager { get; } = new IO.TtAssetManager();
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_AssetMeta
    {
        public void UnitTestEntrance()
        {
            //IO.FileManager.SureDirectory(RName.GetRName("UTest").Address);
            //var rn = RName.GetRName("UTest/test_icon.uvanim");
            //var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
            //if (ameta == null)
            //{
            //    var rnAsset = RName.GetRName("UTest/test_icon.uvanim");
            //    var asset = UEngine.Instance.AssetMetaManager.NewAsset<EGui.UUvAnim>(rnAsset);
            //    asset.SaveAssetTo(rnAsset);
            //}
        }
    }
}
