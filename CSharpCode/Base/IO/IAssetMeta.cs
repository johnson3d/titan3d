using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.IO
{
    //资源导入引擎的接口
    public class IAssetCreateAttribute : Attribute
    {
        public virtual void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
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
        protected System.Threading.Tasks.Task<bool> PGAssetInitTask;
        public RName GetAssetRName()
        {
            if (mName == null)
                return null;
            return RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
        }
        public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
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
            ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
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
                    if (IO.FileManager.FileExists(rn.Address))
                        eErrorType = enErrorType.IsExisting;
                }

                ImGuiAPI.Separator();

                if (CheckAsset())
                {
                    if (ImGuiAPI.Button("Create Asset", &sz))
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if (IO.FileManager.FileExists(rn.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
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

                if (PGAssetInitTask != null && !PGAssetInitTask.IsCompleted)
                {
                }
                else
                {
                    PGAsset.OnDraw(false, false, false);
                    PGAssetInitTask = null;
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
            ameta.Description = $"This is a {mAsset.GetType().FullName}\nHahah";
            ameta.SaveAMeta();

            UEngine.Instance.AssetMetaManager.RegAsset(ameta);

            mAsset.SaveAssetTo(mAsset.AssetName);
            return true;
        }
    }
    public class AssetCreateMenuAttribute : Attribute
    {
        public string MenuName = "Unknow";
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
            await Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public virtual void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.FileManager.DeleteFile(address);
            IO.FileManager.DeleteFile(address + MetaExt);
            if (IO.FileManager.FileExists(address + ".snap"))
            {
                IO.FileManager.DeleteFile(address + ".snap");
            }
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
                IO.FileManager.SaveObjectToXml(mAssetName.Address + MetaExt, this);
        }
        public virtual bool CanRefAssetType(IAssetMeta ameta)
        {
            return true;
        }
        EGui.UIProxy.MenuItemProxy.MenuState mRefGraphMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        EGui.UIProxy.MenuItemProxy.MenuState mDeleteMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        internal System.Threading.Tasks.Task<Editor.USnapshot> Task;
        protected virtual Color GetBorderColor()
        {
            return EGui.UCoreStyles.Instance.SnapBorderColor;
        }
        static Vector2 tempDelta0 = new Vector2(8, 8);
        static Vector2 tempDelta = new Vector2(12, 15);
        public virtual unsafe void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        {
            var snapSize = sz.X * 0.9f;
            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;

            var shadowImg = UEngine.Instance.UIManager[UAssetMetaManager.ItemShadowImgName] as EGui.UIProxy.ImageProxy;
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

            var name = IO.FileManager.GetPureName(GetAssetName().Name, 11);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            tpos.Y = snapEnd.Y + 8;
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)EGui.Slate.UBaseRenderer.enFont.Font_15px);
            cmdlist.AddText(in tpos, nameColor, name, null);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();
            //ImGuiAPI.PopClipRect();


            DrawPopMenu(ContentBrowser);
        }
        protected virtual void DrawPopMenu(EGui.Controls.UContentBrowser ContentBrowser)
        {
            var createNewAssetValueStore = ContentBrowser.CreateNewAssets;
            if (ImGuiAPI.BeginPopupContextItem(mAssetName.Address, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                ContentBrowser.CreateNewAssets = false;
                var drawList = ImGuiAPI.GetWindowDrawList();
                Support.UAnyPointer menuData = new Support.UAnyPointer();

                if (EGui.UIProxy.MenuItemProxy.MenuItem("RefGraph", null, false, null, ref drawList, ref menuData, ref mRefGraphMenuState))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    var rn = RName.GetRName(mAssetName.Name + ".ameta", mAssetName.RNameType);
                    var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, typeof(Editor.Forms.UAssetReferViewer), rn, this);
                }
                if (EGui.UIProxy.MenuItemProxy.MenuItem("Delete", null, false, null, ref drawList, ref menuData, ref mDeleteMenuState))
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

                if (OnDrawContextMenu(ref drawList))
                    ContentBrowser.CreateNewAssets = createNewAssetValueStore;
                ImGuiAPI.EndPopup();
            }
            else
                ContentBrowser.CreateNewAssets = createNewAssetValueStore;
        }
        protected virtual bool OnDrawContextMenu(ref ImDrawList cmdlist)
        {
            return false;
        }
        public virtual void OnShowIconTimout(int time)
        {
            if (Task != null && Task.Result != null)
            {
                Task.Result.mTextureRSV?.Dispose();
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
        [Rtti.Meta]
        public EGui.UUvAnim Icon
        {
            get;
            set;
        } = new EGui.UUvAnim();
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
        public virtual void OnDragTo(Graphics.Pipeline.UViewportSlate vpSlate)
        {
            
        }
    }
    public class UAssetMetaManager
    {
        ~UAssetMetaManager()
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
            var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine);
            var metas = IO.FileManager.GetFiles(RName.GetRName("", RName.ERNameType.Engine).Address, "*" + IAssetMeta.MetaExt, true);
            foreach(var i in metas)
            {
                var m = IO.FileManager.LoadXmlToObject(i) as IAssetMeta;
                if (m == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{i} is not a IAssetMeta");
                    continue;
                }
                var rn = IO.FileManager.GetRelativePath(root, i);
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

            root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game);
            metas = IO.FileManager.GetFiles(RName.GetRName("", RName.ERNameType.Game).Address, "*" + IAssetMeta.MetaExt, true);
            foreach (var i in metas)
            {
                var m = IO.FileManager.LoadXmlToObject<IAssetMeta>(i);
                var rn = IO.FileManager.GetRelativePath(root, i);
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

            if (UEngine.Instance.UIManager[ItemShadowImgName] == null)
                UEngine.Instance.UIManager[ItemShadowImgName] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(ItemShadowImgName, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f));
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
        public IAssetCreateAttribute ImportAsset(RName dir, Rtti.UTypeDesc type, string ext)
        {
            var attrs = type.GetCustomAttributes(typeof(IAssetCreateAttribute), true);
            if (attrs.Length > 0)
            {
                var importer = attrs[0] as IAssetCreateAttribute;
                importer.DoCreate(dir, type, ext);
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
            var tickCount = UEngine.Instance.CurrentTickCount;
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
}

namespace EngineNS
{
    partial class UEngine
    {
        public IO.UAssetMetaManager AssetMetaManager { get; } = new IO.UAssetMetaManager();
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
