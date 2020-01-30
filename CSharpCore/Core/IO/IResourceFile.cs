using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS
{
    public class RNameKey
    {
        public string Name;
        public RName.enRNameType NameType;

        public override bool Equals(object obj)
        {
            var key = obj as RNameKey;
            if (key == null)
                return false;
            if ((string.Equals(Name, key.Name, StringComparison.CurrentCultureIgnoreCase)) &&
                (NameType == key.NameType))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (Name + NameType.ToString()).GetHashCode();
        }
        public bool ContainsString(string str)
        {
            return Name.Contains(str);
        }
    }

    [EngineNS.Editor.Editor_LinkSystemCustomClassPropertyEnableShowAttribute]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class RName : IComparable<RName>, IComparable, INotifyPropertyChanged
    {//所有资源都用RName来替换资源相对路径，以后如果要做资源该换路径的时候还有机会做手脚
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        int RNameHashValue = 0;
        internal RName(string n, int pls_use_static_GetRName, enRNameType nameType = enRNameType.Game)
        {
            mName = n;
            mRNameType = nameType;
            ChangeAddressWithRNameType();
            //Address = n;
            RNameHashValue = (Name + RNameType.ToString()).GetHashCode();
        }
        public bool IsExtension(string ext)
        {
            return "." + GetExtension() == ext;
        }
        public string GetExtension(bool withDot = false)
        {
            return CEngine.Instance.FileManager.GetFileExtension(Name, withDot);
        }
        public string RemoveExtension(string file)
        {
            var _file = file.Replace("\\", "/");
            int pos = _file.LastIndexOf('.');
            if (pos < 0)
                return _file;
            return _file.Remove(pos);
        }
        public string GetDirectory()
        {
            return CEngine.Instance.FileManager.GetPathFromFullName(Address);
        }
        public string GetFileName()
        {
            return Address.Replace(GetDirectory(), "");
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
        public int CompareTo(object obj)
        {
            var rName = (RName)obj;
            if (rName == null)
                return -1;
            return Name.CompareTo(rName.Name);
        }
        public override bool Equals(object obj)
        {
            var rName = obj as RName;
            if (rName == null)
                return false;
            return ((this.Name == rName.Name) && (this.RNameType == rName.RNameType));
        }
        public override int GetHashCode()
        {
            return RNameHashValue;
        }
        string mName;
        public int NameIndexInPakage = -1;
        public string GetNameWithType()
        {
            switch(RNameType)
            {
                case enRNameType.Game:
                    return mName;
                case enRNameType.Engine:
                    return "@Engine/" + mName;
                case enRNameType.Editor:
                    return "@Editor/" + mName;
                default:
                    return mName;
            }

        }
        public string Name
        {
            get => mName;
            set
            {
                mName = value.ToLower();
                ChangeAddressWithRNameType();
                OnPropertyChanged("Name");
                RNameHashValue = (Name + RNameType.ToString()).GetHashCode();
            }
        }
        string mAddress;
        public string Address
        {
            get => mAddress;
            protected set
            {
                mAddress = value;
                //OnPropertyChanged("Address");
            }
        }
        internal void FixAddressDirect()
        {
            ChangeAddressWithRNameType();
        }

        // 数据存盘，请勿修改顺序及名称
        public enum enRNameType
        {
            Game    = 0,
            Engine  = 1,
            Editor  = 2,
            Package = 3,

            ProjectEditor = 4,

            ExternDef0 = 100,
            ExternDef1,
            ExternDef2,
            ExternDef3,
            ExternDef4,
            ExternDef5,
            ExternDef6,
            ExternDef7,
            ExternDef8,
            ExternDef9,
            ExternDefEnd,

            TypeUnknown = 10000,
        }
        enRNameType mRNameType = enRNameType.Game;
        public enRNameType RNameType
        {
            get => mRNameType;
            set
            {
                mRNameType = value;
                ChangeAddressWithRNameType();
            }
        }
        public delegate string FGetMountPointFromRNameType(enRNameType type);
        public static FGetMountPointFromRNameType GetMountPointFromExternRNameType;

        void ChangeAddressWithRNameType()
        {
            if (string.IsNullOrEmpty(Name) == false)
            {
                switch (mRNameType)
                {
                    case enRNameType.Editor:
                        Address = CEngine.Instance.FileManager.EditorContent + Name;
                        break;
                    case enRNameType.Engine:
                        Address = CEngine.Instance.FileManager.EngineContent + Name;
                        break;
                    case enRNameType.Game:
                        Address = CEngine.Instance.FileManager.ProjectContent + Name;
                        break;
                    case enRNameType.Package:
                        Address = Name;
                        break;
                    default:
                        {
                            if(GetMountPointFromExternRNameType!=null)
                            {
                                Address = GetMountPointFromExternRNameType(mRNameType) + Name;
                            }
                            else
                            {
                                throw new InvalidOperationException($"使用了未实现的RNameType {mRNameType}");
                            }
                            break;
                        }
                }
            }
            else
                Address = "";
            RNameHashValue = (Name + RNameType.ToString()).GetHashCode();
        }
        public string GetRootFolder()
        {
            switch(mRNameType)
            {
                case enRNameType.Editor:
                    return CEngine.Instance.FileManager.EditorContent;
                case enRNameType.Engine:
                    return CEngine.Instance.FileManager.EngineContent;
                case enRNameType.Game:
                    return CEngine.Instance.FileManager.ProjectContent;
                default:
                    throw new InvalidOperationException("未实现");
            }
        }
        public static string GetRootFolder(enRNameType rNameType)
        {
            switch(rNameType)
            {
                case enRNameType.Editor:
                    return CEngine.Instance.FileManager.EditorContent;
                case enRNameType.Engine:
                    return CEngine.Instance.FileManager.EngineContent;
                case enRNameType.Game:
                    return CEngine.Instance.FileManager.ProjectContent;
                default:
                    throw new InvalidOperationException("未实现");
            }
        }
        public static enRNameType GetRNameTypeFromAbsFileName(string absFileName)
        {
            absFileName = absFileName.Replace("\\", "/").ToLower();
            if (absFileName.Contains(CEngine.Instance.FileManager.ProjectContent.TrimEnd('/').ToLower()))
                return enRNameType.Game;
            if (absFileName.Contains(CEngine.Instance.FileManager.EngineContent.TrimEnd('/').ToLower()))
                return enRNameType.Engine;
            if (absFileName.Contains(CEngine.Instance.FileManager.EditorContent.TrimEnd('/').ToLower()))
                return enRNameType.Editor;
            throw new InvalidOperationException("未实现");
        }
        public string GetNameWithRootFolder()
        {
            switch(mRNameType)
            {
                case enRNameType.Editor:
                    return "EditContent/" + Name;
                case enRNameType.Engine:
                    return "EngineContent/" + Name;
                case enRNameType.Game:
                    return "Content/" + Name;
            }
            return "";
        }
        public static string GetRootFolderName(enRNameType rNameType)
        {
            switch(rNameType)
            {
                case enRNameType.Editor:
                    return "EditContent/";
                case enRNameType.Engine:
                    return "EngineContent/";
                case enRNameType.Game:
                    return "Content/";
            }
            return "";
        }

        public static RName EmptyName
        {
            get
            {
                return GetRName(null);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable, "RName.GetRName", "")]
        public static RName GetRName(string name, enRNameType rNameType = enRNameType.Game)
        {
#if DEBUG
            if(System.IO.Path.IsPathRooted(name) && rNameType != enRNameType.Package)
            {
                //System.Diagnostics.Debugger.Break();
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Resource", $"System.IO.Path.IsPathRooted failed:{name}");
            }
#endif
            return CEngine.Instance.FileManager.GetRName(name, rNameType);
        }
        public static RName GetRName(int index)
        {
            return CEngine.Instance.FileManager.GetRName(index);
        }
        public static RName EditorOnly_GetRNameFromAbsFile(string absName)
        {
            absName = absName.Replace("\\", "/").ToLower();
            enRNameType rNameType = enRNameType.Package;
            if (absName.Contains(CEngine.Instance.FileManager.ProjectContent.TrimEnd('/').ToLower()))
                rNameType = enRNameType.Game;
            else if (absName.Contains(CEngine.Instance.FileManager.EngineContent.TrimEnd('/').ToLower()))
                rNameType = enRNameType.Engine;
            else if (absName.Contains(CEngine.Instance.FileManager.EditorContent.TrimEnd('/').ToLower()))
                rNameType = enRNameType.Editor;
            else
            {
                rNameType = enRNameType.TypeUnknown;
                if (GetMountPointFromExternRNameType != null)
                {
                    for (enRNameType i = enRNameType.ExternDef0; i < enRNameType.ExternDefEnd; i++)
                    {
                        var mountPoint = GetMountPointFromExternRNameType(i);
                        if (absName.Contains(mountPoint.TrimEnd('/').ToLower()))
                        {
                            rNameType = i;
                        }
                    }
                }
            }

            string relName = "";
            switch(rNameType)
            {
                case enRNameType.Game:
                    relName = CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absName, CEngine.Instance.FileManager.ProjectContent.ToLower());
                    break;
                case enRNameType.Engine:
                    relName = CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absName, CEngine.Instance.FileManager.EngineContent.ToLower());
                    break;
                case enRNameType.Editor:
                    relName = CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absName, CEngine.Instance.FileManager.EditorContent.ToLower());
                    break;
                case enRNameType.Package:
                    relName = absName;
                    break;
                default:
                    {
                        if (GetMountPointFromExternRNameType != null)
                        {
                            var mountPoint = GetMountPointFromExternRNameType(rNameType);
                            relName = CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absName, mountPoint.ToLower());
                        }
                        else
                        {
                            throw new InvalidOperationException($"使用了未实现的RNameType {rNameType}");
                        }
                        break;
                    }
            }
            return GetRName(relName, rNameType);
        }
        public string PureName(bool withExt = false)
        {//no path,no ext
            var file = CEngine.Instance.FileManager.GetPureFileFromFullName(Address, withExt);
            return file;
        }
        public string RelativePath()
        {
            return CEngine.Instance.FileManager.GetPathFromFullName(Name, false);
        }
        public override string ToString()
        {
            return Name;
        }

        public class EqualityComparer : IEqualityComparer<RName>
        {
            public bool Equals(RName x, RName y)
            {
                return x.CompareTo(y)==0;
            }

            public int GetHashCode(RName obj)
            {
                return obj.GetHashCode();
            }
        }
    }

    namespace IO
    {
        namespace Serializer
        {
            public class RNameSerializer : IO.Serializer.FieldSerializer
            {
                public static bool SaveUseIndex = false;
                public static System.Type SerializeType
                {
                    get;
                } = typeof(RName);
                public override object ReadValue(IO.Serializer.IReader pkg)
                {
                    unsafe
                    {
                        string name;
                        pkg.Read(out name);
                        if (string.IsNullOrEmpty(name))
                            return RName.EmptyName;
                        if(name[0] == '@')
                        {
                            var idxStr = name.Substring(1);
                            var idx = System.Convert.ToInt32(idxStr);
                            return RName.GetRName(idx);
                        }
                        else
                        {
                            var splits = name.Split('|');
                            if (splits.Length > 1)
                            {
                                var rNameTypeStr = splits[0];
                                var rNameType = (RName.enRNameType)System.Convert.ToInt32(rNameTypeStr);
                                return RName.GetRName(splits[1], rNameType);
                            }
                            else
                                return RName.GetRName(name);
                        }
                    }
                }
                public override void WriteValue(object obj, IO.Serializer.IWriter pkg)
                {
                    var v = (RName)obj;
                    unsafe
                    {
                        if (v == null)
                            pkg.Write("");
                        else
                        {
                            if (v.NameIndexInPakage >= 0 && SaveUseIndex)
                            {
                                var str = "@" + v.NameIndexInPakage;
                                pkg.Write(str);
                            }
                            else
                            {
                                var str = ((int)v.RNameType) + "|" + v.Name;
                                pkg.Write(str);
                            }
                        }
                    }
                }
                public override void ReadValueList(System.Collections.IList lst, IO.Serializer.IReader pkg)
                {
                    UInt16 count;
                    unsafe
                    {
                        pkg.ReadPtr(&count, sizeof(UInt16));
                        for (UInt16 i = 0; i < count; i++)
                        {
                            string v;
                            pkg.Read(out v);
                            if (string.IsNullOrEmpty(v))
                            {
                                lst.Add(RName.EmptyName);
                            }
                            else if (v[0] == '@')
                            {
                                var idxStr = v.Substring(1);
                                var idx = System.Convert.ToInt32(idxStr);
                                lst.Add(RName.GetRName(idx));
                            }
                            else
                            {
                                var splits = v.Split('|');
                                if (splits.Length > 1)
                                {
                                    var rNameTypeStr = splits[0];
                                    var rNameType = (RName.enRNameType)System.Convert.ToInt32(rNameTypeStr);
                                    lst.Add(RName.GetRName(splits[1], rNameType));
                                }
                                else
                                    lst.Add(RName.GetRName(v));
                            }
                        }
                    }
                }
                public override void WriteValueList(System.Collections.IList objList, IO.Serializer.IWriter pkg)
                {
                    var lst = (List<RName>)objList;
                    unsafe
                    {
                        UInt16 count = 0;
                        if (lst != null)
                            count = (UInt16)lst.Count;
                        pkg.WritePtr(&count, sizeof(UInt16));
                        for (UInt16 i = 0; i < count; i++)
                        {
                            var v = (RName)lst[i];
                            if(v == null)
                                pkg.Write("");
                            else
                            {
                                if(v.NameIndexInPakage >= 0 && SaveUseIndex)
                                {
                                    var str = "@" + v.NameIndexInPakage;
                                    pkg.Write(str);
                                }
                                else
                                {
                                    var str = ((int)v.RNameType) + "|" + v.Name;
                                    pkg.Write(str);
                                }
                            }
                        }
                    }
                }
                public override string ObjectToString(Rtti.MemberDesc p, object obj)
                {
                    var v = (RName)obj;
                    if(v.NameIndexInPakage >= 0 && SaveUseIndex)
                        return "@" + v.NameIndexInPakage;
                    else
                        return ((int)v.RNameType) + "|" + v.ToString();//Base64
                }
                public override object ObjectFromString(Rtti.MemberDesc p, string str)
                {
                    if (string.IsNullOrEmpty(str))
                        return RName.EmptyName;
                    if(str[0] == '@')
                    {
                        var idxStr = str.Substring(1);
                        var idx = System.Convert.ToInt32(idxStr);
                        return RName.GetRName(idx);
                    }
                    else
                    {
                        var splits = str.Split('|');
                        if (splits.Length > 1)
                        {
                            var rNameTypeStr = splits[0];
                            var rNameType = (RName.enRNameType)System.Convert.ToInt32(rNameTypeStr);
                            return RName.GetRName(splits[1], rNameType);
                        }
                        else
                            return RName.GetRName(str);
                    }
                }
            }
        }
        public interface IResourceFile
        {
            RName Name
            {
                get;
            }
        }
        partial class FileManager
        {
            public Dictionary<RNameKey, RName> RNameManager
            {
                get;
            } = new Dictionary<RNameKey, RName>();
            public FileManager()
            {
                BadResourceName = new RName("", 0);
            }
            RName BadResourceName;
            public RName GetRName(string name, RName.enRNameType nameType = RName.enRNameType.Game)
            {
                if (string.IsNullOrEmpty(name))
                    return BadResourceName;
                name = name.Replace('\\', '/');
                name = RemapName(name);
                var key = new RNameKey()
                {   
                    Name = name.ToLower(),
                    NameType = nameType,
                };
                RName result;
                lock(RNameManager)
                {
                    if (RNameManager.TryGetValue(key, out result) == false)
                    {
                        result = new RName(key.Name, 0, nameType);
                        //result.Address = CEngine.Instance.FileManager.Content + name;
                        RNameManager.Add(key, result);
                    }
                }
                return result;
            }
            private string RemapName(string name)
            {
                //非常临时的代码，解决一个错误资源路径
                if (name.Contains("editor/base mesh/"))
                {
                    return name.Replace("editor/base mesh/", "editor/basemesh/");
                }
                string result;
                if(RNameRemap.TryGetValue(name, out result))
                {
                    return result;
                }
                return name;
            }

            public Dictionary<string, string> RNameRemap
            {
                get;
            } = new Dictionary<string, string>();

            public Dictionary<int, RName> PackageRNameCache
            {
                get;
            } = new Dictionary<int, RName>();
            public void ClearPackageRNameCache()
            {
                PackageRNameCache.Clear();
            }
            public RName GetRName(int index)
            {
                if (index < 0)
                    return RName.EmptyName;

                RName result;
                lock(RNameManager)
                {
                    if(PackageRNameCache.TryGetValue(index, out result) == false)
                    {
                        result = new RName("", 0, RName.enRNameType.Game);
                        result.NameIndexInPakage = index;
                        PackageRNameCache.Add(index, result);
                    }
                }
                return result;
            }
            // 将PackageRNameCache中的RName合并到RNameManager中
            public void MergePackageRNameCacheToRNameManager()
            {
                lock(RNameManager)
                {
                    var prcEnumer = PackageRNameCache.GetEnumerator();
                    while(prcEnumer.MoveNext())
                    {
                        var val = prcEnumer.Current;
                        if (string.IsNullOrEmpty(val.Value.Name))
                            throw new InvalidOperationException("RName name not valid");
                        var key = new RNameKey()
                        {
                            Name = val.Value.Name,
                            NameType = val.Value.RNameType,
                        };
                        RName resultRName;
                        if (!RNameManager.TryGetValue(key, out resultRName))
                        {
                            //val.Value.NameIndexInPakage = -1;
                            RNameManager.Add(key, val.Value);
                        }
                    }
                }
            }

            public delegate void FTrySaveRInfo(string resType, RName name, object obj);
            public static FTrySaveRInfo OnTrySaveRInfo = null;
            public static void TrySaveRInfo(string resType, RName name, object obj)
            {
                if (OnTrySaveRInfo != null)
                {
                    OnTrySaveRInfo(resType, name, obj);
                }
            }
        }
    }
}
