using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [RName.PGRName]
    public class RName : IComparable<RName>, IComparable
    {
        public class PGRNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public string FilterExts;
            public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, EGui.Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
            {
                var name = value as RName;
                var newName = EGui.Controls.CtrlUtility.DrawRName(name, prop.Name, FilterExts, ReadOnly);
                if (newName != name)
                {
                    foreach (var j in pg.TargetObjects)
                    {
                        EGui.Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, newName);
                    }
                }
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
        private void ChangeAddressWithRNameType()
        {
            switch (mRNameType)
            {
                case ERNameType.Engine:
                    mAddress = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + Name;
                    break;
                case ERNameType.Game:
                    mAddress = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game) + Name;
                    break;
                default:
                    {
                        break;
                    }
            }
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
            Dictionary<string, RName>[] mNameSets = null;
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
