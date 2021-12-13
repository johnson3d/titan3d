using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class UDummyAttribute : Attribute
    {
    }
    public class MetaParameterAttribute : Attribute
    {
        [Flags]
        public enum EArgumentFilter
        {
            A1 = 1,
            A2 = 1 << 1,
            A3 = 1 << 2,
            A4 = 1 << 3,
            A5 = 1 << 4,
            A6 = 1 << 5,
            A7 = 1 << 6,
            A8 = 1 << 7,
            R = 1 << 23
        }
        public System.Type FilterType;//参数类型为Type的时候，macross用来筛选可选择类型
        public EArgumentFilter ConvertOutArguments;//最高位是控制返回值，其他是参数的顺序 
        
    }
    public class GenMetaClassAttribute : Attribute
    {
        public bool IsOverrideBitset = true;
    }
    public class GenMetaAttribute : Attribute
    {//使用在Field上，系统自动产生Meta标志过的Property
        public MetaAttribute.EMetaFlags Flags = 0;
    }
    public class UStructAttrubte : Attribute
    {
        public int ReadSize;
    }
    public class MetaAttribute : Attribute
    {
        [Flags]
        public enum EMetaFlags : uint
        {
            NoMacrossUseable = 1,//可使用在类型，成员变量，成员属性，函数上，不允许Macross使用
            NoMacrossCreate = (1 << 1),//只可使用在类型上，不允许Macross申明，也就是不能在macross中CreateInstance
            NoMacrossInherit = (1 << 2),//只可使用在类型上，不允许Macross派生
            NoMacrossOverride = (1 << 3),//只可使用在虚函数上，不允许Macross重载

            MacrossReadOnly = (1 << 4),//只可使用在成员变量，成员属性上，该属性对Macross只读

            DiscardWhenCooked = (1 << 5),//在cook资源中不序列化
            DiscardWhenRPC = (1 << 6),//在做RPC的时候不序列化
        }
        public int Order = 0;
        public EMetaFlags Flags = 0;
        public bool IsReadOnly
        {
            get
            {
                return (Flags & EMetaFlags.MacrossReadOnly) != 0;
            }
        }
    }
    public class UClassMeta
    {
        public MetaAttribute MetaAttribute { get; private set; }
        private List<UClassMeta> mSubClasses = null;
        public void CopyObjectMetaField(object tar, object src)
        {
            if(tar is System.Collections.IList && src.GetType() == tar.GetType())
            {
                var Tarlst = tar as System.Collections.IList;
                var Srclst = src as System.Collections.IList;
                Tarlst.Clear();
                for (int i = 0; i < Srclst.Count; i++)
                {
                    Tarlst.Add(Srclst[i]);
                }
                return;
            }
            else if (tar is System.Collections.IDictionary && src.GetType() == tar.GetType())
            {
                var Tarlst = tar as System.Collections.IDictionary;
                var Srclst = src as System.Collections.IDictionary;
                Tarlst.Clear();
                var i = Srclst.GetEnumerator();
                while(i.MoveNext())
                {
                    Tarlst.Add(i.Key, i.Value);
                }
                return;
            }

            var tarType = tar.GetType();
            var srcType = src.GetType();
            foreach (var i in CurrentVersion.Fields)
            {
                var tarProp = tarType.GetProperty(i.FieldName);
                if (tarProp == null)
                    continue;
                var srcProp = srcType.GetProperty(i.FieldName);
                if (srcProp == null)
                    continue;
                if (tarProp.PropertyType == srcProp.PropertyType)
                {
                    var v = srcProp.GetValue(src);
                    tarProp.SetValue(tar, v);
                }
            }
        }
        public struct UMetaFieldValue
        {
            public string Name;
            public Rtti.UTypeDesc FieldType;
            public object Value;
        }
        public void CopyObjectMetaField(List<UMetaFieldValue> tar, object src)
        {
            var srcType = src.GetType();
            foreach (var i in CurrentVersion.Fields)
            {
                var srcProp = srcType.GetProperty(i.FieldName);
                if (srcProp == null)
                    continue;

                var fld = new UMetaFieldValue();
                fld.Name = i.FieldName;
                fld.FieldType = Rtti.UTypeDesc.TypeOf(srcProp.PropertyType);
                fld.Value = srcProp.GetValue(src);
                var lst = fld.Value as System.Collections.IList;
                var dict = fld.Value as System.Collections.IDictionary;
                if (lst != null)
                {
                    var tarlst = Rtti.UTypeDescManager.CreateInstance(fld.FieldType.SystemType) as System.Collections.IList;
                    for (int j = 0; j < lst.Count; j++)
                    {
                        tarlst.Add(lst[j]);
                    }
                    fld.Value = tarlst;
                }
                if (dict != null)
                {
                    var tardict = Rtti.UTypeDescManager.CreateInstance(fld.FieldType.SystemType) as System.Collections.IDictionary;
                    var j = dict.GetEnumerator();
                    while (j.MoveNext())
                    {
                        tardict.Add(j.Key, j.Value);
                    }
                    fld.Value = tardict;
                }
            }
        }
        public void SetObjectMetaField(object tar, List<UMetaFieldValue> fldValues)
        {
            var srcType = tar.GetType();
            foreach(var i in fldValues)
            {
                var srcProp = srcType.GetProperty(i.Name);
                if (srcProp == null)
                    continue;

                if (srcProp.PropertyType != i.FieldType.SystemType)
                    continue;

                var old = srcProp.GetValue(tar);
                if (old == i.Value)
                    continue;

                srcProp.SetValue(tar, i.Value);
            }
        }
        public List<UClassMeta> SubClasses
        {
            get
            {
                if (mSubClasses != null)
                    return mSubClasses;

                mSubClasses = new List<UClassMeta>();
                foreach (var i in Rtti.UTypeDescManager.Instance.Services)
                {
                    foreach (var j in i.Value.Types)
                    {
                        if(j.Value.SystemType.IsSubclassOf(this.ClassType.SystemType))
                        {
                            var klsMeta = Rtti.UClassMetaManager.Instance.GetMeta(j.Value);
                            if (klsMeta == null)
                                continue;
                            if (mSubClasses.Contains(klsMeta) == false)
                                mSubClasses.Add(klsMeta);
                        }
                    }
                }
                return mSubClasses;
            }
        }
        public bool CanConvertTo(UClassMeta tarKls)
        {
            if (tarKls == null)
                return false;
            if (ClassType == tarKls.ClassType)
                return true;
            return ClassType.SystemType.IsSubclassOf(tarKls.ClassType.SystemType);
        }
        public bool CanConvertFrom(UClassMeta srcKls)
        {
            if (srcKls == null)
                return false;
            if (ClassType == srcKls.ClassType)
                return true;
            return srcKls.ClassType.SystemType.IsSubclassOf(ClassType.SystemType);
        }
        public UClassMeta(UTypeDesc t)
        {
            ClassType = t;
        }
        public UTypeDesc ClassType
        {
            get;
            set;
        }
        public string ClassMetaName
        {
            get
            {
                return UTypeDesc.TypeStr(ClassType.SystemType);
            }
        }
        public string MetaDirectoryName
        {
            get
            {
                return Hash160.CreateHash160(ClassMetaName).ToString();
            }
        }
        public Dictionary<uint, UMetaVersion> MetaVersions
        {
            get;
        } = new Dictionary<uint, UMetaVersion>();
        UMetaVersion mCurrentVersion;
        public UMetaVersion CurrentVersion
        {
            get { return mCurrentVersion; }
        }
        public UMetaVersion GetMetaVersion(uint hash)
        {
            UMetaVersion result;
            if (MetaVersions.TryGetValue(hash, out result))
            {
                return result;
            }

            //var myXmlDoc = new System.Xml.XmlDocument();
            //var file = Path + "\\" + hash.ToString() + ".metadata";
            //file = file.Replace("/", "\\");
            //myXmlDoc.Load(file);
            //var ver = new UMetaVersion(this);
            //ver.LoadVersion(hash, myXmlDoc.LastChild);
            //MetaVersions[ver.MetaHash] = ver;
            return null;
        }
        internal string Path { get; set; }
        public bool LoadClass(string path)
        {
            Path = path;
            var versions = EngineNS.IO.FileManager.GetFiles(path, "*.metadata", false);
            foreach(var i in versions)
            {
                var filename = EngineNS.IO.FileManager.GetPureName(i);
                var myXmlDoc = new System.Xml.XmlDocument();
                myXmlDoc.Load(i);
                var ver = new UMetaVersion(this);
                ver.LoadVersion(System.Convert.ToUInt32(filename), myXmlDoc.LastChild);
                MetaVersions[ver.MetaHash] = ver;
            }
            return true;
        }
        public void SaveClass()
        {
            //var typeStr = ClassType.TypeString;
            //var typeDesc = UTypeDescManager.Instance.GetTypeDescFromString(typeStr);
            //var typeDesc = ClassType;
            //var rootDir = IO.FileManager.ERootDir.Engine;
            //if (typeDesc.Assembly.IsGameModule)
            //{
            //    rootDir = IO.FileManager.ERootDir.Game;
            //}
            //var metaRoot = UEngine.Instance.FileManager.GetPath(rootDir, IO.FileManager.ESystemDir.MetaData);
            //var path = EngineNS.IO.FileManager.CombinePath(metaRoot, typeDesc.Assembly.Service);
            //path = EngineNS.IO.FileManager.CombinePath(path, typeDesc.Assembly.Name);
            var tmpPath = EngineNS.IO.FileManager.CombinePath(Path, $"{MetaDirectoryName}");
            if (!EngineNS.IO.FileManager.DirectoryExists(tmpPath))
            {
                EngineNS.IO.FileManager.CreateDirectory(tmpPath);
            }
            var txtFilepath = EngineNS.IO.FileManager.CombinePath(tmpPath, $"typename.txt");
            EngineNS.IO.FileManager.WriteAllText(txtFilepath, ClassType.TypeString);
        }
        public UMetaVersion BuildCurrentVersion()
        {
            var type = ClassType;
            {
                var attrs = type.SystemType.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 1)
                {
                    MetaAttribute = attrs[0] as MetaAttribute;
                }
            }
            var result = new UMetaVersion(this);
            var props = type.SystemType.GetProperties();
            foreach(var i in props)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 0)
                    continue;
                var meta = attrs[0] as MetaAttribute;
                var fd = new UMetaVersion.MetaField();
                fd.Meta = meta;
                fd.PropInfo = i;
                fd.Order = meta.Order;
                fd.FieldType = Rtti.UTypeDesc.TypeOf(i.PropertyType);
                fd.FieldName = i.Name;
                fd.FieldTypeStr = UTypeDescManager.Instance.GetTypeStringFromType(i.PropertyType);
                fd.Build();
                result.Fields.Add(fd);
            }
            result.Fields.Sort();
            string hashString = "";
            foreach(var i in result.Fields)
            {
                hashString += i.ToString();
            }
            result.MetaHash = EngineNS.UniHash.APHash(hashString);
            if (MetaAttribute != null || type.SystemType.GetInterface(nameof(IO.ISerializer)) != null)
            {
                if (MetaVersions.TryGetValue(result.MetaHash, out mCurrentVersion) == false)
                {
                    result.SaveVersion();
                }
                MetaVersions[result.MetaHash] = result;
            }
            mCurrentVersion = result;

            return result;
        }
        #region MacrossMethod
        public class MethodMeta
        {
            public class ParamMeta
            {
                public MetaParameterAttribute Meta;
                public System.Reflection.ParameterInfo ParamInfo;
            }
            public MetaAttribute Meta;
            public System.Reflection.MethodInfo Method;
            public ParamMeta[] Parameters;
            private string Name;
            public void Init()
            {
                var parameters = Method.GetParameters();

                Parameters = new ParamMeta[parameters.Length];
                Name = $"{Method.ReturnType.FullName} {Method.DeclaringType.FullName}.{Method.Name}(";                
                for (int i = 0; i < parameters.Length; i++)
                {
                    Parameters[i] = new ParamMeta();
                    Parameters[i].ParamInfo = parameters[i];
                    Parameters[i].Meta = GetParameterMeta(i);
                    Name += $"{parameters[i].ParameterType.FullName} {parameters[i].Name}";
                    if (i < parameters.Length - 1)
                        Name += ",";
                }
                Name += ')';
            }
            public ParamMeta GetParameter(int index)
            {
                return Parameters[index];
            }
            public ParamMeta FindParameter(string name)
            {
                foreach(var i in Parameters)
                {
                    if (i.ParamInfo.Name == name)
                        return i;
                }
                return null;
            }
            private MetaParameterAttribute GetParameterMeta(int index)
            {
                var param = Method.GetParameters()[index];
                var attrs = param.GetCustomAttributes(typeof(MetaParameterAttribute), false);
                if (attrs.Length == 0)
                    return null;
                return attrs[index] as MetaParameterAttribute;
            }
            public override string ToString()
            {
                return Name;
            }
            public string GetMethodDeclareString()
            {
                var result = $"{Method.ReturnType.FullName} {Method.Name}(";
                var parameters = Method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    result += $"{parameters[i].ParameterType.FullName}";
                    if (i < parameters.Length - 1)
                        result += ",";
                }
                result += ')';
                return result;
            }
            public void CheckMetaField()
            {

            }
        }
        public List<MethodMeta> Methods { get; } = new List<MethodMeta>();
        public MethodMeta GetMethod(string declString)
        {
            foreach(var i in Methods)
            {
                if (i.GetMethodDeclareString() == declString)
                    return i;
            }
            return null;
        }
        public void BuildMethods()
        {
            Methods.Clear();
            if (ClassType.IsRemoved)
                return;
            var methods = ClassType.SystemType.GetMethods();
            foreach (var i in methods)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 0)
                    continue;

                var mth = new MethodMeta();
                mth.Meta = attrs[0] as MetaAttribute;
                mth.Method = i;
                mth.Init();
                Methods.Add(mth);
            }
        }
        #endregion

        #region MacrossField
        public class FieldMeta
        {
            public MetaAttribute Meta;
            public System.Reflection.FieldInfo Field;
            private string Name;
            public void Init()
            {
                Name = $"{Field.FieldType.FullName} {Field.Name}";
            }
            public override string ToString()
            {
                return Name;
            }
        }
        public List<FieldMeta> Fields { get; } = new List<FieldMeta>();
        public void BuildFields()
        {
            Fields.Clear();
            var flds = ClassType.SystemType.GetFields();
            foreach (var i in flds)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 0)
                    continue;

                var mth = new FieldMeta();
                mth.Meta = attrs[0] as MetaAttribute;
                mth.Field = i;
                mth.Init();
                Fields.Add(mth);
            }
        }
        #endregion

        public void CheckMetaField()
        {
            foreach(var i in MetaVersions)
            {
                i.Value.CheckMetaField();
            }
            BuildMethods();
        }
    }
    public class UMetaVersion
    {
        public UMetaVersion(UClassMeta kls)
        {
            HostClass = kls;
        }
        public UClassMeta HostClass
        {
            get;
            private set;
        }
        public uint MetaHash
        {
            get;
            internal set;
        } = 0;
        public class MetaField : IComparable<MetaField>
        {
            public MetaAttribute Meta
            {
                get;
                set;
            }
            public System.Reflection.PropertyInfo PropInfo
            {
                get;
                set;
            }
            public void Build()
            {
                if (PropInfo == null)
                    return;
                var attrs = PropInfo.GetCustomAttributes(typeof(IO.UCustomSerializerAttribute), false);
                if (attrs.Length == 0)
                    return;
                var custom = attrs[0] as IO.UCustomSerializerAttribute;

                if (custom.CustomType == null)
                    return;

                var types1 = new Type[] { typeof(IO.IWriter), typeof(IO.ISerializer), typeof(string) };
                var methodSave = custom.CustomType.GetMethod("Save", types1);
                if (methodSave.IsStatic == false)
                    return;

                var types2 = new Type[] { typeof(IO.IReader), typeof(IO.ISerializer), typeof(string) };
                var methodLoad = custom.CustomType.GetMethod("Load", types2);
                if (methodLoad.IsStatic == false)
                    return;

                CustomWriter = methodSave;
                CustomReader = methodLoad;

                //if JIT
                CustomWriterAction = (Action<IO.IWriter, IO.ISerializer, string>)Delegate.CreateDelegate(typeof(Action<IO.IWriter, IO.ISerializer, string>), CustomWriter);
                CustomReaderAction = (Action<IO.IReader, IO.ISerializer, string>)Delegate.CreateDelegate(typeof(Action<IO.IReader, IO.ISerializer, string>), CustomReader);
            }
            public void CustomSave(IO.IWriter ar, IO.ISerializer host, string propName)
            {
                CustomWriterAction(ar, host, propName);
            }
            public void CustomLoad(IO.IReader ar, IO.ISerializer host, string propName)
            {
                CustomReaderAction(ar, host, propName);
            }
            public System.Reflection.MethodInfo CustomWriter;
            public System.Reflection.MethodInfo CustomReader;
            //这个需要Jit CreateDelegate
            private Action<IO.IWriter, IO.ISerializer, string> CustomWriterAction;
            private Action<IO.IReader, IO.ISerializer, string> CustomReaderAction;
            public Rtti.UTypeDesc FieldType
            {
                get;
                set;
            }
            public int Order { get; set; } = 0;
            private string mFieldTypeStr;
            public string FieldTypeStr
            {
                get => mFieldTypeStr;
                set
                {
                    mFieldTypeStr = value;
                }
            }
            public string FieldName
            {
                get;
                set;
            }
            public int CompareTo(MetaField other)
            {
                var cmp = Order.CompareTo(other.Order);
                if (cmp > 0)
                    return 1;
                else if (cmp < 0)
                    return -1;
                else
                {
                    cmp = FieldName.CompareTo(other.FieldName);
                    if (cmp > 0)
                        return 1;
                    else if (cmp < 0)
                        return -1;
                    else
                    {
                        return FieldTypeStr.CompareTo(other.FieldTypeStr);
                    }
                }
            }
            public override string ToString()
            {
                return FieldTypeStr + FieldName;
            }
        }
        public List<MetaField> Fields
        {
            get;
        } = new List<MetaField>();
        public void CheckMetaField()
        {
            if (HostClass.ClassType.IsRemoved)
            {
                foreach (var i in Fields)
                {
                    i.PropInfo = null;
                }
                return;
            }
            foreach(var i in Fields)
            {
                i.PropInfo = HostClass.ClassType.SystemType.GetProperty(i.FieldName);
            }
        }
        public bool LoadVersion(uint hash, System.Xml.XmlNode node)
        {
            MetaHash = hash;
            foreach (System.Xml.XmlNode i in node.ChildNodes)
            {
                var fd = new MetaField();
                fd.FieldName = i.Name;
                foreach (System.Xml.XmlAttribute j in i.Attributes)
                {
                    if (j.Name == "Type")
                    {
                        fd.FieldTypeStr = j.Value;
                    }
                    else if (j.Name == "Order")
                    {
                        fd.Order = System.Convert.ToInt32(j.Value);
                    }
                }
                fd.FieldType = Rtti.UTypeDesc.TypeOf(fd.FieldTypeStr);
                fd.PropInfo = HostClass.ClassType.SystemType.GetProperty(fd.FieldName);
                fd.Build();
                if (fd.FieldType == null)
                {
                    fd.FieldType = UMissingTypeManager.Instance.GetConvertType(fd.FieldTypeStr);
                    if (fd.PropInfo != null && fd.FieldType == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"Property lost: Name = {fd.FieldName}; Type = {fd.FieldTypeStr}");
                    }
                }
                Fields.Add(fd);
            }
            return true;
        }
        public void SaveVersion()
        {
            var xml = new System.Xml.XmlDocument();
            var root = xml.CreateElement($"Hash_{MetaHash}", xml.NamespaceURI);
            var fdType = xml.CreateAttribute("Type");
            fdType.Value = HostClass.ClassMetaName;
            root.Attributes.Append(fdType);
            xml.AppendChild(root);
            foreach (var i in Fields)
            {
                var fd = xml.CreateElement($"{i.FieldName}", root.NamespaceURI);
                fdType = xml.CreateAttribute("Type");
                fdType.Value = i.FieldTypeStr;
                fd.Attributes.Append(fdType);
                fdType = xml.CreateAttribute("Order");
                fdType.Value = i.Order.ToString();
                fd.Attributes.Append(fdType);
                root.AppendChild(fd);
            }
            var xmlText = EngineNS.IO.FileManager.GetXmlText(xml);

            //var typeStr = UTypeDescManager.Instance.GetTypeStringFromType(HostClass.ClassType.SystemType);
            //var typeDesc = UTypeDescManager.Instance.GetTypeDescFromString(typeStr);
            //var typeStr = HostClass.ClassType.TypeString;
            //var typeDesc = HostClass.ClassType;
            //EngineNS.IO.FileManager.ERootDir rootDirType = IO.FileManager.ERootDir.Engine;
            //if (typeDesc.Assembly.IsGameModule)
            //{
            //    rootDirType = IO.FileManager.ERootDir.Game;
            //}
            //var assmemblyDir = typeDesc.Assembly.Service + "/" + typeDesc.Assembly.Name + "/";
            //var rootDir = UEngine.Instance.FileManager.GetPath(rootDirType, IO.FileManager.ESystemDir.MetaData);
            //rootDir += assmemblyDir;
            //var tmpPath = EngineNS.IO.FileManager.CombinePath(rootDir, $"{HostClass.MetaDirectoryName}");
            var tmpPath = HostClass.Path;
            if (!EngineNS.IO.FileManager.DirectoryExists(tmpPath))
            {
                EngineNS.IO.FileManager.CreateDirectory(tmpPath);
            }
            var xmlFilepath = EngineNS.IO.FileManager.CombinePath(tmpPath, $"{MetaHash}.metadata");
            EngineNS.IO.FileManager.WriteAllText(xmlFilepath, xmlText);
        }
    }

    public class UClassMetaManager
    {
        public static UClassMetaManager Instance { get; } = new UClassMetaManager();
        Dictionary<string, UClassMeta> mMetas = new Dictionary<string, UClassMeta>();
        public Dictionary<string, UClassMeta> Metas
        {
            get => mMetas;
        }
        Dictionary<Hash64, UClassMeta> mHashMetas = new Dictionary<Hash64, UClassMeta>();
        public TypeTreeManager TreeManager = new TypeTreeManager();
        public string MetaRoot;
        public void LoadMetas1()
        {
            var rootTypes = new IO.FileManager.ERootDir[2] { IO.FileManager.ERootDir.Engine, IO.FileManager.ERootDir.Game };
            foreach(var r in rootTypes)
            {
                var metaRoot = UEngine.Instance.FileManager.GetPath(r, IO.FileManager.ESystemDir.MetaData);
                if (EngineNS.IO.FileManager.DirectoryExists(metaRoot) == false)
                {
                    continue;
                }
                var services = EngineNS.IO.FileManager.GetDirectories(metaRoot, "*.*", false);
                foreach (var i in services)
                {
                    var assemblies = EngineNS.IO.FileManager.GetDirectories(i, "*.*", false);
                    foreach (var j in assemblies)
                    {
                        var kls = EngineNS.IO.FileManager.GetDirectories(j, "*.*", false);
                        foreach (var k in kls)
                        {
                            var tmpPath = EngineNS.IO.FileManager.CombinePath(k, $"typename.txt");
                            var strName = EngineNS.IO.FileManager.ReadAllText(tmpPath);
                            if (strName == null)
                                continue;
                            var type = UTypeDesc.TypeOf(strName);// EngineNS.Rtti.UTypeDescManager.Instance.GetTypeDescFromString(strName);
                            if (type != null)
                            {
                                var meta = new UClassMeta(type);
                                meta.LoadClass(k);

                                mMetas.Add(meta.ClassMetaName, meta);
                            }
                        }
                    }
                }
            }

            //编辑器状态，程序修改过执行
            BuildMeta();

            foreach(var i in mMetas)
            {
                var hashCode = Hash64.FromString(i.Key);
                UClassMeta meta;
                if (mHashMetas.TryGetValue(hashCode, out meta))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "Meta", $"Same Hash:{i.Value.ClassMetaName} == {meta.ClassMetaName}");
                    System.Diagnostics.Debug.Assert(false);
                    continue;
                }
                mHashMetas[hashCode] = i.Value;

                TreeManager.RegType(i.Value);
                i.Value.BuildMethods();
                i.Value.BuildFields();
            }

            ForceSaveAll();
        }
        public void LoadMetas()
        {
            var rootTypes = new IO.FileManager.ERootDir[2] { IO.FileManager.ERootDir.Engine, IO.FileManager.ERootDir.Game };
            foreach (var r in rootTypes)
            {
                var metaRoot = UEngine.Instance.FileManager.GetPath(r, IO.FileManager.ESystemDir.MetaData);
                if (EngineNS.IO.FileManager.DirectoryExists(metaRoot) == false)
                {
                    continue;
                }
                var services = EngineNS.IO.FileManager.GetDirectories(metaRoot, "*.*", false);
                foreach (var i in services)
                {
                    var assemblies = EngineNS.IO.FileManager.GetDirectories(i, "*.*", true);
                    foreach (var j in assemblies)
                    {
                        var kls = EngineNS.IO.FileManager.GetDirectories(j, "*.*", false);
                        foreach (var k in kls)
                        {
                            var tmpPath = EngineNS.IO.FileManager.CombinePath(k, $"typedesc.txt");
                            var strName = EngineNS.IO.FileManager.ReadAllText(tmpPath);
                            if (strName == null)
                                continue;
                            var type = UTypeDesc.TypeOf(strName);// EngineNS.Rtti.UTypeDescManager.Instance.GetTypeDescFromString(strName);
                            if (type != null)
                            {
                                var meta = new UClassMeta(type);
                                meta.LoadClass(k);

                                mMetas.Add(meta.ClassMetaName, meta);
                            }
                        }
                    }
                }
            }

            //编辑器状态，程序修改过执行
            BuildMeta();

            foreach (var i in mMetas)
            {
                var hashCode = Hash64.FromString(i.Key);
                UClassMeta meta;
                if (mHashMetas.TryGetValue(hashCode, out meta))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Fatal, "Meta", $"Same Hash:{i.Value.ClassMetaName} == {meta.ClassMetaName}");
                    System.Diagnostics.Debug.Assert(false);
                    continue;
                }
                mHashMetas[hashCode] = i.Value;

                TreeManager.RegType(i.Value);
                i.Value.BuildMethods();
                i.Value.BuildFields();
            }

            ForceSaveAll();
        }
        public void BuildMeta()
        {
            foreach (var i in UTypeDescManager.Instance.Services)
            {
                foreach(var j in i.Value.Types)
                {
                    MetaAttribute meta;
                    if (j.Value.SystemType.GetInterface(nameof(EngineNS.IO.ISerializer)) == null)
                    {
                        var attrs = j.Value.SystemType.GetCustomAttributes(typeof(MetaAttribute), true);
                        if (attrs.Length == 0)
                            continue;
                        meta = attrs[0] as MetaAttribute;
                    }

                    UClassMeta kls = null;
                    var metaName = j.Value.TypeString;// Rtti.UTypeDescManager.Instance.GetTypeStringFromType(j.Value.SystemType);
                    if (mMetas.TryGetValue(metaName, out kls) == false)
                    {
                        var root = UEngine.Instance.FileManager.GetPath(IO.FileManager.ERootDir.Engine, IO.FileManager.ESystemDir.MetaData);
                        if (j.Value.Assembly.IsGameModule)
                            root = UEngine.Instance.FileManager.GetPath(IO.FileManager.ERootDir.Game, IO.FileManager.ESystemDir.MetaData);

                        kls = new UClassMeta(j.Value);

                        var dir = kls.ClassType.Assembly.Service;
                        dir += "." + kls.ClassType.Assembly.Name;
                        dir += "." + kls.ClassType.Namespace;

                        dir = dir.Replace('.', '/');
                        dir = root + dir;

                        var name = kls.ClassType.FullName.Substring(kls.ClassType.Namespace.Length);
                        if (name.StartsWith('.'))
                        {
                            name = name.Substring(1);
                        }
                        if (name.Length < 250)
                        {   
                            kls.Path = dir + "/a0." + name;
                        }
                        else
                        {
                            kls.Path = dir + "/a0." + kls.MetaDirectoryName;
                        }
                        
                        var ver = kls.BuildCurrentVersion();
                        mMetas.Add(metaName, kls);
                        kls.SaveClass();
                    }
                    else
                    {
                        var ver = kls.BuildCurrentVersion();
                    }
                }
            }
        }

        public void ForceSaveAll()
        {
            var root = UEngine.Instance.FileManager.GetPath(IO.FileManager.ERootDir.Engine, IO.FileManager.ESystemDir.MetaData);
            foreach (var i in mMetas)
            {
                var dir = i.Value.ClassType.Assembly.Service;
                dir += "." + i.Value.ClassType.Assembly.Name;
                dir += "." + i.Value.ClassType.Namespace;

                dir = dir.Replace('.', '/');
                dir = root + dir;

                var name = i.Value.ClassType.FullName.Substring(i.Value.ClassType.Namespace.Length);
                if (name.StartsWith('.'))
                {
                    name = name.Substring(1);
                }
                if (name.Length <250)
                {
                    i.Value.Path = dir + "/a0." + name;
                }
                else
                {
                    i.Value.Path = dir + "/a0." + i.Value.MetaDirectoryName;
                }
                IO.FileManager.SureDirectory(i.Value.Path);
                foreach (var j in i.Value.MetaVersions)
                {
                    j.Value.SaveVersion();
                }

                var txtFilepath = EngineNS.IO.FileManager.CombinePath(i.Value.Path, $"typedesc.txt");
                EngineNS.IO.FileManager.WriteAllText(txtFilepath, i.Value.ClassType.TypeString);
            }
        }
        public UClassMeta GetMeta(string name, bool bTryBuild = true)
        {
            UClassMeta meta;
            if (mMetas.TryGetValue(name, out meta))
                return meta;
            if (bTryBuild == false)
                return null;
            lock (this)
            {
                var type = Rtti.UTypeDesc.TypeOf(name);
                if (type == null)
                    return null;
                UClassMeta result;
                if (TypeMetas.TryGetValue(type, out result))
                {
                    return result;
                }
                else
                {
                    result = new UClassMeta(type);
                    result.BuildCurrentVersion();
                    TypeMetas.Add(type, result);
                    return result;
                }
            }
        }
        public Dictionary<UTypeDesc, UClassMeta> TypeMetas { get; } = new Dictionary<UTypeDesc, UClassMeta>();
        public UClassMeta GetMetaFromFullName(string fname)
        {
            var desc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(fname);
            if (desc == null)
                return null;
            return GetMeta(desc);
        }
        public UClassMeta GetMeta(UTypeDesc type)
        {
            if (type == null)
                return null;
            lock (this)
            {
                UClassMeta result;
                if (TypeMetas.TryGetValue(type, out result))
                {
                    return result;
                }
                var typeStr = type.TypeString; //Rtti.UTypeDesc.TypeStr(type.SystemType);
                return GetMeta(typeStr);
            }
        }
        public UClassMeta GetMeta(Hash64 hash)
        {
            UClassMeta meta;
            if (mHashMetas.TryGetValue(hash, out meta))
                return meta;
            return null;
        }
    }
}
