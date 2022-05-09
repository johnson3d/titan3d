using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS
{
    [RName.PGRName]
    public class RName : IComparable<RName>, IComparable
    {
        public class PGRNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public string FilterExts;   // "ext1" / "ext1,ext2"
            EGui.UIProxy.ComboBox mComboBox;

            class DrawData
            {
                public RName NewValue;
            }
            [System.ThreadStatic]
            DrawData mDrawData = new DrawData();

            public float MaxWidth = -1;
            public float MinWidth = -1;
            protected override async Task<bool> Initialize_Override()
            {
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
            void ComboOpenAction(in Support.UAnyPointer data)
            {
                UEngine.Instance.EditorInstance.RNamePopupContentBrowser.OnDraw();
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
                var assetMeta = UEngine.Instance.AssetMetaManager.GetAssetMeta(name);
                assetMeta?.OnDrawSnapshot(in drawList, ref cursorPos, ref snapEnd);

                var preViewStr = "null";
                if (name != null)
                    preViewStr = name.ToString();
                var textSize = ImGuiAPI.CalcTextSize(preViewStr, false, 0);
                var preViewStrDrawPos = cursorPos + new Vector2(snapSize.X + 8, 0);
                ImGuiAPI.SetCursorScreenPos(in preViewStrDrawPos);
                Support.UAnyPointer anyPt = new Support.UAnyPointer()
                {
                    RefObject = mDrawData,
                };
                var index = ImGuiAPI.TableGetColumnIndex();
                mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_.ImGuiComboFlags_HeightLarge;
                mComboBox.Width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
                mComboBox.Name = TName.FromString2("##", info.Name).ToString();
                mComboBox.PreviewValue = preViewStr;
                var contentBrowserSize = new Vector2(500, 600);
                ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Always);
                UEngine.Instance.EditorInstance.RNamePopupContentBrowser.ExtNames = FilterExts;
                UEngine.Instance.EditorInstance.RNamePopupContentBrowser.SelectedAsset = null;
                mComboBox.OnDraw(in drawList, in anyPt);
                if(UEngine.Instance.EditorInstance.RNamePopupContentBrowser.SelectedAsset != null &&
                    UEngine.Instance.EditorInstance.RNamePopupContentBrowser.SelectedAsset.GetAssetName() != name)
                {
                    mDrawData.NewValue = UEngine.Instance.EditorInstance.RNamePopupContentBrowser.SelectedAsset.GetAssetName();
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
                    if (ImGuiAPI.Button("<", in sz))
                    {
                        mDrawData.NewValue = EGui.Controls.ContentBrowser.GlobalSelectedAsset.GetAssetName();
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
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(this);
                if (ameta != null)
                    return ameta.AssetId;
                
                return Guid.Empty;
            }
            set
            {
                if (value == Guid.Empty)
                    return;
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(value);
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
            set
            {
                mRNameType = value;
                ChangeAddressWithRNameType();
            }
        }
        public string Name
        {
            get => mName;
            set
            {
                mName = value.ToLower();
                ChangeAddressWithRNameType();
            }
        }
        public string Address
        {
            get => mAddress;
        }
        public string ExtName
        {
            get
            {
                return IO.FileManager.GetExtName(mName);
            }
        }
        public string PureName => IO.FileManager.GetPureName(mName);
        public static RName GetRName(string name, ERNameType rNameType = ERNameType.Game)
        {
            return RNameManager.Instance.GetRName(name, rNameType);
        }
        public static RName ParseFrom(string nameStr)
        {
            if (nameStr == null)
                return null;
            var segs = nameStr.Split(',');
            if (segs.Length < 2)
                return null;
            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[0]);
            return RNameManager.Instance.GetRName(segs[1], rnType);
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
                    return UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + name;
                case ERNameType.Game:
                    return UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game) + name;
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
                    var dict = mNameSets[(int)rNameType];
                    RName result;
                    if (dict.TryGetValue(name, out result) == false)
                    {
                        result = new RName(name, rNameType);
                        dict.Add(name, result);
                        return result;
                    }
                    return result;
                }
            }
        }
    }    
}
