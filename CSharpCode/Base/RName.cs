using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Macross;
using EngineNS.Rtti;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EngineNS
{
    public abstract class TtRNameTagObjectAttribute : Attribute 
    {
        public abstract object GetTagObject(RName rn);
    }

    [RName.PGRName]
    public partial class RName : IComparable<RName>, IComparable
    {
        public const ushort MinVersion = 100;//don't change this value any time
        public const ushort Version100 = MinVersion + 0;
        //public void Write(RName v)
        //{
        //    Write(RName.CurrentVersion);
        //    if (v== null)
        //    {
        //        Write(RName.ERNameType.Unkown);
        //        return;
        //    }
        //    Write(v.RNameType);
        //    Write(v.Name);
        //}
        public const ushort Version101 = MinVersion + 1;//increase this value when serialization changed
        //public void Write(RName v)
        //{
        //    Write(RName.CurrentVersion);
        //    if (v== null)
        //    {
        //        Write(RName.ERNameType.Unkown);
        //        return;
        //    }
        //    else
        //    {
        //        Write(v.RNameType);
        //    }
        //    Write(v.Name);
        //    Write(v.AssetId);
        //}
        public const ushort CurrentVersion = Version101;
        public class PGRNameAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
        {
            public string FilterExts;   // "ext1" / "ext1,ext2"
            public System.Type MacrossType;
            public string ShaderType;
            EGui.UIProxy.ComboBox mComboBox;

            class DrawData
            {
                public RName NewValue;
            }
            [System.ThreadStatic]
            static DrawData mDrawData = new DrawData();

            public EGui.Controls.TtContentBrowser ContentBrowser;

            public float MaxWidth = -1;
            public float MinWidth = -1;
            protected override async Thread.Async.TtTask<bool> Initialize_Override()
            {
                ContentBrowser = Editor.TtEditor.NewPopupContentBrowser();
                mComboBox = new EGui.UIProxy.ComboBox()
                {
                    ComboOpenAction = ComboOpenAction
                };
                await mComboBox.Initialize();

                return await base.Initialize_Override();
            }
            ~PGRNameAttribute()
            {
                Cleanup();
            }
            protected override void Cleanup_Override()
            {
                mComboBox?.Cleanup();
                base.Cleanup_Override();
            }
            void ComboOpenAction(in Support.TtAnyPointer data)
            {
                ContentBrowser.OnDraw();
                //TtEngine.Instance.EditorInstance.RNamePopupContentBrowser.OnDraw();
            }
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;

                var name = info.Value as RName;
                mDrawData.NewValue = name;
                //var newName = EGui.Controls.CtrlUtility.DrawRName(name, info.Name, FilterExts, info.Readonly, mSnap);

                var drawList = ImGuiAPI.GetWindowDrawList();
                var cursorPos = ImGuiAPI.GetCursorScreenPos();
                ImGuiAPI.BeginGroup();

                var snapSize = new Vector2(64, 64);
                var snapEnd = cursorPos + snapSize;
                var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name);
                assetMeta?.OnDrawSnapshot(in drawList, ref cursorPos, ref snapEnd);

                var preViewStr = "null";
                if (name != null)
                    preViewStr = name.ToString();
                var textSize = ImGuiAPI.CalcTextSize(preViewStr, false, 0);
                var preViewStrDrawPos = cursorPos + new Vector2(snapSize.X + 8, 0);
                ImGuiAPI.SetCursorScreenPos(in preViewStrDrawPos);
                ImGuiAPI.Dummy(in Vector2.Zero);
                Support.TtAnyPointer anyPt = new Support.TtAnyPointer()
                {
                    RefObject = mDrawData,
                };
                var index = ImGuiAPI.TableGetColumnIndex();
                mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_.ImGuiComboFlags_HeightMask_;
                mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                                     ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
                mComboBox.Width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
                mComboBox.Name = "##" + info.Name != null ? info.Name : "";
                mComboBox.PreviewValue = preViewStr;
                var contentBrowserSize = new Vector2(500, 600);
                ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Appearing);
                ContentBrowser.ExtNames = FilterExts;
                ContentBrowser.MacrossBase = TtTypeDesc.TypeOf(MacrossType);
                ContentBrowser.ShaderType = ShaderType;
                ContentBrowser.SelectedAssets.Clear();
                mComboBox.OnDraw(in drawList, in anyPt);
                if (ContentBrowser.SelectedAssets.Count > 0 &&
                    ContentBrowser.SelectedAssets[0].GetAssetName() != name)
                {
                    mDrawData.NewValue = ContentBrowser.SelectedAssets[0].GetAssetName();
                }
                var pos = ImGuiAPI.GetCursorScreenPos();
                pos.X += snapSize.X + 8;
                ImGuiAPI.SetCursorScreenPos(in pos);
                ImGuiAPI.Dummy(in Vector2.Zero);
                if (info.Readonly)
                {
                    Vector4 color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                    ImGuiAPI.TextColored(in color, "readonly");
                }
                else
                {
                    var sz = new Vector2(0, 0);
                    if (ImGuiAPI.Button("F", in sz))
                    {
                        EGui.Controls.TtContentBrowser.GlobalFocusAsset = mDrawData.NewValue;
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("<", in sz))
                    {
                        mDrawData.NewValue = EGui.Controls.TtContentBrowser.GlobalSelectedAsset.GetAssetName();
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("-", in sz))
                    {
                        mDrawData.NewValue = null;
                    }
                }

                ImGuiAPI.EndGroup();

                if (mDrawData.NewValue != name)
                {
                    newValue = mDrawData.NewValue;
                    return true;
                }
                return false;
            }
        }
        public class TtRNameStats
        {
            public uint NameUniqueIdAllocator = 0;
        }
        private static TtRNameStats Stats = new TtRNameStats();
        private uint NameUniqueId;
        public enum ERNameType : ushort
        {
            Game = 0,
            Engine,
            Transient,
            Cloud,
            Count,
            Unkown = ushort.MaxValue,
        }
        ERNameType mRNameType = ERNameType.Game;

        public static string GetRNameTypeCodeString(ERNameType type)
        {
            return $"{type.GetType().FullName.Replace("+", ".")}.{type.ToString()}";
        }

        string mName;
        string mAddress;
        public WeakReference mTagReference = null;
        internal RName(string name, ERNameType type)
        {
            lock (Stats)
            {
                System.Diagnostics.Debug.Assert(Stats.NameUniqueIdAllocator < uint.MaxValue - 1);
                NameUniqueId = Stats.NameUniqueIdAllocator;
                Stats.NameUniqueIdAllocator++;
            }
            VeryDangrouseUpdate(name, type);
        }
        public T GetTagObject<T>()
        {
            if (mTagReference == null || mTagReference.IsAlive == false)
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(TtRNameTagObjectAttribute), true);
                if (attrs.Length > 0)
                {
                    var attrTag = attrs[0] as TtRNameTagObjectAttribute;
                    mTagReference = new WeakReference(attrTag.GetTagObject(this));
                }
            }
            return (T)mTagReference.Target;
        }

        public class PGMacrossRNameAttribute<T> : PGRNameAttribute where T : class
        {
            //public UMacrossGetter<T> MacrossGetter;

            public PGMacrossRNameAttribute()
            {
                MacrossType = typeof(T);
            }

            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                var changed = base.OnDraw(info, out newValue);

                //if(MacrossGetter == null)
                //    MacrossGetter = UMacrossGetter<T>.NewInstance();
                //var newRName = (RName)newValue;
                //if (MacrossGetter.Name != newRName)
                //{
                //    MacrossGetter.Reset(TtEngine.Instance.MacrossModule);
                //    MacrossGetter.Name = newRName;
                //}
                //var obj = MacrossGetter.Get();

                ISceneNodeMacrossInterface mi = null;
                var enumrableInterface = info.ObjectInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach (var objIns in (IEnumerable)info.ObjectInstance)
                    {
                        if (objIns == null)
                            continue;

                        mi = objIns as ISceneNodeMacrossInterface;
                        if (mi != null)
                            break;
                    }
                }
                else
                {
                    mi = info.ObjectInstance as ISceneNodeMacrossInterface;
                }
                if (mi != null)
                {
                    var obj = mi.GetMacrossObject();
                    if (obj != null)
                    {
                        var macrossObjInfo = new EditorInfo()
                        {
                            Name = info.Name,
                            Type = Rtti.TtTypeDesc.TypeOf(obj.GetType()),
                            Value = obj,
                            Readonly = info.Readonly,
                            HostPropertyGrid = info.HostPropertyGrid,
                            Flags = info.Flags,
                            Expand = true,
                            HostProperty = info.HostProperty,
                        };
                        object macrossObjVal;
                        TtPropertyGrid.DrawPropertyGridObjectItem(ref macrossObjInfo, out macrossObjVal);
                    }
                }

                return changed;
            }
        }

        public static bool IsEmpty(RName rName)
        {
            return string.IsNullOrEmpty(rName.mName);
        }
        public static bool IsExist(RName rName)
        {
            if (rName == null)
                return false;
            return IO.TtFileManager.FileExists(rName.Address) || IO.TtFileManager.DirectoryExists(rName.Address);
        }

        #region AMeta
        public IO.IAssetMeta AMeta
        {
            get
            {
                return TtEngine.Instance.AssetMetaManager.GetAssetMeta(this);
            }
        }
        public async Thread.Async.TtTask<T> GetAsset<T>(params object[] args) where T : class, IO.IAsset
        {
            var ameta = AMeta;
            if (ameta == null || ameta.AssetStatus != IO.IAssetMeta.EAssetStatus.Valid)
                return default(T);
            return await ameta.LoadAsset(args) as T;
        }
        public async Thread.Async.TtTask<T> CreateAsset<T>(params object[] args) where T : class, IO.IAsset
        {
            var ameta = AMeta;
            if (ameta == null || ameta.AssetStatus != IO.IAssetMeta.EAssetStatus.Valid)
                return default(T);
            return await ameta.CreateAsset(args) as T;
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public Guid AssetId
        {
            get
            {
                if (AMeta != null)
                    return AMeta.AssetId;
                
                return Guid.Empty;
            }
            set
            {
                if (value == Guid.Empty)
                    return;
                if (AMeta != null)
                {
                    if (AMeta.GetAssetName().Name != Name || AMeta.GetAssetName().mRNameType != mRNameType)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
            }
        }
        #endregion

        [Rtti.Meta("")]
        public ERNameType RNameType
        {
            get { return mRNameType; }
        }
        [Rtti.Meta("")]
        public string Name
        {
            get => mName;
        }
        [Rtti.Meta("")]
        public string Address
        {
            get => mAddress;
        }
        [Rtti.Meta("")]
        public string ExtName
        {
            get
            {
                return IO.TtFileManager.GetExtName(mName);
            }
        }
        [Rtti.Meta("")]
        public string PureName => IO.TtFileManager.GetPureName(mName);
        public string NoExtName => IO.TtFileManager.RemoveExtName(mName);
        public string AbsParentPath => IO.TtFileManager.GetParentPathName(Address);
        public string ParentPath => IO.TtFileManager.GetParentPathName(mName);
        public static RName GetRNameFromAbsPath(string path)
        {
            path = IO.TtFileManager.GetValidFileName(path);
            ERNameType rNameType = ERNameType.Count;
            string name = null;
            for (var i = IO.TtFileManager.ERootDir.Game; i < IO.TtFileManager.ERootDir.Count; i++)
            {
                var root = TtEngine.Instance.FileManager.GetRoot(i);
                if (path.StartsWith(root))
                {
                    switch (i)
                    {
                        case IO.TtFileManager.ERootDir.Game:
                            rNameType = ERNameType.Game;
                            break;
                        case IO.TtFileManager.ERootDir.Engine:
                            rNameType = ERNameType.Engine;
                            break;
                    }
                    name = path.Substring(root.Length);
                    break;
                }
            }
            return RNameManager.Instance.GetRName(name, rNameType);
        }
        public static RName GetRName(string name, ERNameType rNameType = ERNameType.Game)
        {
            return RNameManager.Instance.GetRName(name, rNameType);
        }
        public static RName TryGetRName(string name, ERNameType type)
        {
            return RNameManager.Instance.TryGetRName(name, type);
        }
        public static RName ParseFrom(string nameStr)
        {
            if (nameStr == null)
                return null;
            var segs = nameStr.Split(':');
            if (segs.Length < 2)
                return null;
            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[1]);
            return RNameManager.Instance.GetRName(segs[0], rnType);
        }
        public override string ToString()
        {
            return $"{mName}:{mRNameType}";
        }
        public static string GetAddress(ERNameType type, string name, RName rn = null)
        {
            if (TtEngine.Instance==null)
                return null;
            switch (type)
            {
                case ERNameType.Engine:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine) + name;
                case ERNameType.Game:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) + name;
                case ERNameType.Cloud:
                    if (rn==null)
                        return null;
                    if (SureCloudAMeta(rn)==false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning, "RName", $"Cloud RName get address failed, AMeta not exist! RName:{rn} is a directory?");
                    }
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cloud) + name;
                default:
                    {
                        return null;
                    }
            }
        }
        public RName GetDirectoryRName()
        {
            var dir = IO.TtFileManager.GetBaseDirectory(mName);
            return GetRName(dir, RNameType);
        }

        #region IComparable
        internal void VeryDangrouseUpdate(string name, ERNameType type)
        {
            mName = name;
            mRNameType = type;
            mAddress = GetAddress(mRNameType, Name, this);
        }
        public override int GetHashCode()
        {
            return (int)NameUniqueId;
        }
        public int CompareTo(object obj)
        {
            var rName = (RName)obj;
            if (rName == null)
                return -1;
            return NameUniqueId.CompareTo(rName.NameUniqueId);
        }
        public int CompareTo(RName other)
        {
            return NameUniqueId.CompareTo(other.NameUniqueId);
        }
        #endregion
        internal class RNameManager
        {
            public static RNameManager Instance = new RNameManager();
            internal Dictionary<string, RName>[] mNameSets = null;
            public RNameManager()
            {
                mNameSets = new Dictionary<string, RName>[(int)ERNameType.Count];
                for (int i = 0; i < mNameSets.Length; i++)
                {
                    mNameSets[i] = new Dictionary<string, RName>();
                }
            }
            public RName GetRName(string name, ERNameType rNameType = ERNameType.Game)
            {
                name = name.Replace('\\', '/');
                name = name.ToLower();
                lock (this)
                {
                    if ((int)rNameType >= (int)ERNameType.Count)
                        return null;
                    RName result;
                    var dict = mNameSets[(int)rNameType];
                    if (dict.TryGetValue(name, out result) == false)
                    {
                        result = new RName(name, rNameType);
                        dict.Add(name, result);
                        return result;
                    }
                    return result;
                }
            }
            public RName TryGetRName(string name, ERNameType type)
            {
                name = name.Replace('\\', '/');
                name = name.ToLower();
                RName result;
                var dict = mNameSets[(int)type];
                if (dict.TryGetValue(name, out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            internal void VeryDangrouseRemove(RName rn)
            {
                foreach (var i in mNameSets[(int)rn.RNameType])
                {
                    if (i.Value == rn)
                        mNameSets[(int)rn.RNameType].Remove(i.Key);
                }
            }
            internal void VeryDangrouseAdd(RName rn)
            {
                mNameSets[(int)rn.RNameType][rn.Name] = rn;
            }
        }
    }    
}
