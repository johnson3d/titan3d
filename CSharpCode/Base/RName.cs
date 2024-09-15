using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS
{
    public abstract class URNameTagObjectAttribute : Attribute 
    {
        public abstract object GetTagObject(RName rn);
    }

    [RName.PGRName]
    public class RName : IComparable<RName>, IComparable
    {
        public WeakReference mTagReference = null;
        public T GetTagObject<T>()
        {
            if (mTagReference == null || mTagReference.IsAlive == false)
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(URNameTagObjectAttribute), true);
                if (attrs.Length > 0)
                {
                    var attrTag = attrs[0] as URNameTagObjectAttribute;
                    mTagReference = new WeakReference(attrTag.GetTagObject(this));
                }
            }
            return (T)mTagReference.Target;
        }

        public class PGRNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
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

            public EGui.Controls.UContentBrowser ContentBrowser;

            public float MaxWidth = -1;
            public float MinWidth = -1;
            protected override async Task<bool> Initialize_Override()
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
                mComboBox.Name = TName.FromString2("##", info.Name != null ? info.Name : "").ToString();
                mComboBox.PreviewValue = preViewStr;
                var contentBrowserSize = new Vector2(500, 600);
                ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Appearing);
                ContentBrowser.ExtNames = FilterExts;
                ContentBrowser.MacrossBase = Rtti.TtTypeDesc.TypeOf(MacrossType);
                ContentBrowser.ShaderType = ShaderType;
                ContentBrowser.SelectedAssets.Clear();
                mComboBox.OnDraw(in drawList, in anyPt);
                if(ContentBrowser.SelectedAssets.Count > 0 &&
                    ContentBrowser.SelectedAssets[0].GetAssetName() != name)
                {
                    mDrawData.NewValue = ContentBrowser.SelectedAssets[0].GetAssetName();
                }
                var pos = ImGuiAPI.GetCursorScreenPos();
                pos.X += snapSize.X + 8;
                ImGuiAPI.SetCursorScreenPos(in pos);
                if (info.Readonly)
                {
                    Vector4 color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                    ImGuiAPI.TextColored(in color, "readonly");
                }
                else
                {
                    var sz = new Vector2(0, 0);
                    if(ImGuiAPI.Button("F", in sz))
                    {
                        EGui.Controls.UContentBrowser.GlobalFocusAsset = mDrawData.NewValue;
                    }
                    ImGuiAPI.SameLine(0, 8);
                    if (ImGuiAPI.Button("<", in sz))
                    {
                        mDrawData.NewValue = EGui.Controls.UContentBrowser.GlobalSelectedAsset.GetAssetName();
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

        internal RName(string name, ERNameType type)
        {
            mName = name;
            mRNameType = type;
            ChangeAddressWithRNameType();
        }
        public enum ERNameType : ushort
        {
            Game = 0,
            Engine,
            Transient,
            Count,
        }
        ERNameType mRNameType = ERNameType.Game;
        string mName;
        string mAddress;
        int RNameHashValue;
        public Guid AssetId
        {
            get
            {
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(this);
                if (ameta != null)
                    return ameta.AssetId;
                
                return Guid.Empty;
            }
            set
            {
                if (value == Guid.Empty)
                    return;
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(value);
                if (ameta != null)
                {
                    if (ameta.GetAssetName().Name != Name || ameta.GetAssetName().mRNameType != mRNameType)
                    {

                    }
                }
            }
        }
        public ERNameType RNameType
        {
            get { return mRNameType; }
            //private set
            //{
            //    mRNameType = value;
            //    ChangeAddressWithRNameType();
            //}
        }
        public string Name
        {
            get => mName;
            //private set
            //{
            //    mName = value.ToLower();
            //    ChangeAddressWithRNameType();
            //}
        }

        public void UnsafeUpdate(string name, ERNameType type)
        {
            RNameManager.Instance.UnsafeUpdateRName(this, name, type);
            mRNameType = type;
            mName = name.ToLower();
            ChangeAddressWithRNameType();
        }
        public string Address
        {
            get => mAddress;
        }
        public string ExtName
        {
            get
            {
                return IO.TtFileManager.GetExtName(mName);
            }
        }
        public string PureName => IO.TtFileManager.GetPureName(mName);
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
        public int CompareTo(RName other)
        {
            if (this.mRNameType > other.mRNameType)
                return 1;
            else if (this.mRNameType < other.mRNameType)
                return -1;
            else
                return Name.CompareTo(other.Name);
        }
        public static string GetAddress(ERNameType type, string name)
        {
            switch (type)
            {
                case ERNameType.Engine:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine) + name;
                case ERNameType.Game:
                    return TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) + name;
                default:
                    {
                        return null;
                    }
            }
        }
        private void ChangeAddressWithRNameType()
        {
            mAddress = GetAddress(mRNameType, Name);
            RNameHashValue = (Name + RNameType.ToString()).GetHashCode();
        }
        public override int GetHashCode()
        {
            return RNameHashValue;
        }
        public int CompareTo(object obj)
        {
            var rName = (RName)obj;
            if (rName == null)
                return -1;
            return Name.CompareTo(rName.Name);
        }

        public RName GetDirectoryRName()
        {
            var dir = IO.TtFileManager.GetBaseDirectory(mName);
            return GetRName(dir, RNameType);
        }

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
            public void UnsafeUpdateRName(RName rn, string name, ERNameType type)
            {
                lock (this)
                {
                    var dict = mNameSets[(int)rn.RNameType];
                    if (dict.TryGetValue(rn.Name, out var result))
                    {
                        System.Diagnostics.Debug.Assert(result == rn);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    dict.Remove(rn.Name);
                    dict = mNameSets[(int)type];
                    dict.Add(name, rn);
                }
            }
        }
    }    
}
