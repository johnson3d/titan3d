using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace EngineNS.IO
{
    //资源导入引擎的接口
    public class IAssetCreateAttribute : Attribute
    {
        public virtual void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
        {

        }
        //导入的窗口渲染接口
        public virtual void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
        {

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
        protected EGui.Controls.TypeSelector TypeSlt = new EGui.Controls.TypeSelector();
        public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
        {
            ExtName = ext;
            mName = null;
            mDir = dir;
            TypeSlt.BaseType = type;
            TypeSlt.SelectedType = type;

            mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType.SystemType) as IAsset;
            PGAsset.Target = mAsset;
        }
        public override unsafe void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
        {
            if (bPopOpen == false)
                ImGuiAPI.OpenPopup($"New {TypeSlt.BaseType.Name}", ImGuiPopupFlags_.ImGuiPopupFlags_None);

            var visible = true;
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
                    mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType.SystemType) as IAsset;
                    PGAsset.Target = mAsset;
                }

                var sz = new Vector2(0, 0);
                if (mAsset != null)
                {
                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mName);
                    ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    var name = buffer.AsText();
                    eErrorType = enErrorType.None;
                    if(string.IsNullOrEmpty(name))
                    {
                        eErrorType = enErrorType.EmptyName;
                    }
                    else if (mName != name)
                    {
                        mName = name;
                        mAsset.AssetName = RName.GetRName(mDir.Name + name + ExtName, mDir.RNameType);
                        if(IO.FileManager.FileExists(mAsset.AssetName.Address))
                            eErrorType = enErrorType.IsExisting;
                    }
                    buffer.DestroyMe();

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    if (CheckAsset())
                    {
                        if (ImGuiAPI.Button("Create Asset", &sz))
                        {
                            if (IO.FileManager.FileExists(mAsset.AssetName.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
                            {
                                var ameta = mAsset.CreateAMeta();
                                ameta.SetAssetName(mAsset.AssetName);
                                ameta.AssetId = Guid.NewGuid();
                                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(mAsset.GetType());
                                ameta.Description = $"This is a {mAsset.GetType().FullName}\nHahah";
                                ameta.SaveAMeta();

                                UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                mAsset.SaveAssetTo(mAsset.AssetName);

                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", &sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                }
                else
                {
                    if (ImGuiAPI.Button("Cancel", &sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                }
                ImGuiAPI.EndPopup();
            }
        }
        protected virtual bool CheckAsset()
        {
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
    public class IAssetMeta
    {
        RName mAssetName;
        public void SetAssetName(RName rn)
        {
            mAssetName = rn;
            System.Diagnostics.Debug.Assert(rn.Name.EndsWith(".ameta") == false);
        }
        public RName GetAssetName()
        {
            return mAssetName;
        }
        public void SaveAMeta()
        {
            if (mAssetName != null)
                IO.FileManager.SaveObjectToXml(mAssetName.Address + ".ameta", this);
        }
        public virtual bool CanRefAssetType(IAssetMeta ameta)
        {
            return true;
        }
        //在ContentBrowser里面渲染Asset信息
        public unsafe virtual void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;
            //icon.OnDraw(ref cmdlist, ref start, ref end);
            var name = IO.FileManager.GetPureName(mAssetName.Name);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            Vector2 tpos;
            tpos.Y = start.Y + sz.Y - tsz.Y;
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            ImGuiAPI.PushClipRect(&start, &end, true);
            cmdlist.AddText(&tpos, 0xFFFF00FF, name, null);
            ImGuiAPI.PopClipRect();
        }
        public virtual void OnShowIconTimout(int time)
        {

        }
        [Rtti.Meta]
        public EGui.UVAnim Icon
        {
            get;
            set;
        } = new EGui.UVAnim();
        [Rtti.Meta]
        public string TypeStr { get; set; }
        [Rtti.Meta]
        public string Description { get; set; } = "This is a Asset";
        [Rtti.Meta]
        public Guid AssetId { get; set; }
        [Rtti.Meta]
        public List<RName> RefAssetRNames { get; } = new List<RName>();

        public long ShowIconTime;
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
            }
            Assets.Clear();
        }
        public Dictionary<Guid, IAssetMeta> Assets { get; } = new Dictionary<Guid, IAssetMeta>();
        public Dictionary<RName, IAssetMeta> RNameAssets { get; } = new Dictionary<RName, IAssetMeta>();
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
        public void LoadMetas()
        {
            var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine);
            var metas = IO.FileManager.GetFiles(RName.GetRName("", RName.ERNameType.Engine).Address, "*.ameta", true);
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
            metas = IO.FileManager.GetFiles(RName.GetRName("", RName.ERNameType.Game).Address, "*.ameta", true);
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
            var attrs = type.SystemType.GetCustomAttributes(typeof(IAssetCreateAttribute), true);
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
            IO.FileManager.SureDirectory(RName.GetRName("UTest").Address);
            var rn = RName.GetRName("UTest/test_icon.uvanim");
            var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
            if (ameta == null)
            {
                var rnAsset = RName.GetRName("UTest/test_icon.uvanim");
                var asset = UEngine.Instance.AssetMetaManager.NewAsset<EGui.UVAnim>(rnAsset);
                asset.SaveAssetTo(rnAsset);
            }
        }
    }
}
