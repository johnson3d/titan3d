using EngineNS.UI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EngineNS.Rtti
{
    public class TtDummyAttribute : Attribute
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
        public System.Type[] TypeList;
        public EArgumentFilter ConvertOutArguments;//最高位是控制返回值，其他是参数的顺序 
        
    }
    //public class GenMetaClassAttribute : Attribute
    //{
    //    public bool IsOverrideBitset = true;
    //}
    public class TtStructAttrubte : Attribute
    {
        public int ReadSize;
    }
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
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
            //MacrossDeclareable = (1<<5),//可在Macross中申明实例

            DiscardWhenCooked = (1 << 6),//在cook资源中不序列化
            DiscardWhenRPC = (1 << 7),//在做RPC的时候不序列化

            ManualMarshal = (1 << 8),

            Unserializable = (1 << 9),// 不能序列化

            CanRefForMacross = (1 << 11),// Macross代码生成时，可以用ref做传引用，这里有一个潜规则，需要提供对应名为m{PropertyName}的public成员变量
        }
        public int Order = 0;
        public EMetaFlags Flags = 0;
        public string ShaderName;
        public string[] NameAlias = null;
        public System.Type[] MethodGenericParameters = null;
        public string[] MacrossDisplayPath = null;
        public bool IsNoMacrossUseable => (Flags & EMetaFlags.NoMacrossUseable) != 0;
        public bool IsNoMacrossCreate => (Flags & EMetaFlags.NoMacrossCreate) != 0;
        public bool IsNoMacrossInherit => (Flags & EMetaFlags.NoMacrossInherit) != 0;
        public bool IsNoMacrossOverride => (Flags & EMetaFlags.NoMacrossOverride) != 0;
        public bool IsMacrossReadOnly => (Flags & EMetaFlags.MacrossReadOnly) != 0;
        //public bool IsMacrossDeclareable => (Flags & EMetaFlags.MacrossDeclareable) != 0;
        public bool IsUnserializable => (Flags & EMetaFlags.Unserializable) != 0;
        public bool IsCanRefForMacross => (Flags & EMetaFlags.CanRefForMacross) != 0; 
    }
    public class TtClassMeta
    {
        public MetaAttribute MetaAttribute { get; private set; }
        private List<TtClassMeta> mSubClasses = null;
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
            foreach (var i in CurrentVersion.Propertys)
            {
                var tarProp = tarType.GetProperty(i.PropertyName);
                if (tarProp == null)
                    continue;
                var srcProp = srcType.GetProperty(i.PropertyName);
                if (srcProp == null)
                    continue;
                if (tarProp.PropertyType == srcProp.PropertyType && tarProp.CanWrite)
                {
                    var v = srcProp.GetValue(src);
                    tarProp.SetValue(tar, v);
                }
            }
        }
        public struct UMetaFieldValue
        {
            public string Name;
            public Rtti.TtTypeDesc FieldType;
            public object Value;
        }
        public void CopyObjectMetaField(List<UMetaFieldValue> tar, object src)
        {
            var srcType = src.GetType();
            foreach (var i in CurrentVersion.Propertys)
            {
                var srcProp = srcType.GetProperty(i.PropertyName);
                if (srcProp == null)
                    continue;

                var fld = new UMetaFieldValue();
                fld.Name = i.PropertyName;
                fld.FieldType = Rtti.TtTypeDesc.TypeOf(srcProp.PropertyType);
                fld.Value = srcProp.GetValue(src);
                var lst = fld.Value as System.Collections.IList;
                var dict = fld.Value as System.Collections.IDictionary;
                if (lst != null)
                {
                    var tarlst = Rtti.TtTypeDescManager.CreateInstance(fld.FieldType) as System.Collections.IList;
                    for (int j = 0; j < lst.Count; j++)
                    {
                        tarlst.Add(lst[j]);
                    }
                    fld.Value = tarlst;
                }
                if (dict != null)
                {
                    var tardict = Rtti.TtTypeDescManager.CreateInstance(fld.FieldType) as System.Collections.IDictionary;
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
        public List<TtClassMeta> SubClasses
        {
            get
            {
                if (mSubClasses != null)
                    return mSubClasses;

                mSubClasses = new List<TtClassMeta>();
                foreach (var i in Rtti.TtTypeDescManager.Instance.Services)
                {
                    foreach (var j in i.Value.Types)
                    {
                        if(j.Value.IsSubclassOf(this.ClassType))
                        {
                            var klsMeta = Rtti.TtClassMetaManager.Instance.GetMeta(j.Value);
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
        public bool CanConvertTo(TtClassMeta tarKls)
        {
            if (tarKls == null)
                return false;
            if (ClassType == tarKls.ClassType)
                return true;
            return ClassType.IsSubclassOf(tarKls.ClassType);
        }
        public bool CanConvertFrom(TtClassMeta srcKls)
        {
            if (srcKls == null)
                return false;
            if (ClassType == srcKls.ClassType)
                return true;
            return srcKls.ClassType.IsSubclassOf(ClassType);
        }
        public TtClassMeta(TtTypeDesc t)
        {
            ClassType = t;
        }
        public TtTypeDesc ClassType
        {
            get;
            set;
        }
        public string ClassMetaName
        {
            get
            {
                return TtTypeDesc.TypeStr(ClassType);
            }
        }
        public string MetaDirectoryName
        {
            get
            {
                return Hash160.CreateHash160(ClassMetaName).ToString();
            }
        }
        public Dictionary<UInt64, TtMetaVersion> MetaVersions
        {
            get;
        } = new Dictionary<UInt64, TtMetaVersion>();
        TtMetaVersion mCurrentVersion;
        public TtMetaVersion CurrentVersion
        {
            get { return mCurrentVersion; }
        }
        public void ResetSystemRef()
        {
            foreach (var i in MetaVersions)
            {
                i.Value.ResetSystemRef();
            }
            foreach (var i in Fields)
            {
                i.ResetSystemRef();
            }
        }
        public TtMetaVersion GetMetaVersion(UInt64 hash)
        {
            TtMetaVersion result;
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
            try
            {
                Path = path;
                var versions = EngineNS.IO.TtFileManager.GetFiles(path, "*.metadata", false);
                foreach (var i in versions)
                {
                    var filename = EngineNS.IO.TtFileManager.GetPureName(i);
                    var myXmlDoc = new System.Xml.XmlDocument();
                    myXmlDoc.Load(i);
                    var ver = new TtMetaVersion(this);
                    ver.LoadVersion(System.Convert.ToUInt64(filename), myXmlDoc.LastChild);
                    MetaVersions[ver.MetaHash] = ver;
                }
            }
            catch (System.Exception)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Fatal, "meta load field in " + path);
                return false;
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
            //var metaRoot = TtEngine.Instance.FileManager.GetPath(rootDir, IO.FileManager.ESystemDir.MetaData);
            //var path = EngineNS.IO.FileManager.CombinePath(metaRoot, typeDesc.Assembly.Service);
            //path = EngineNS.IO.FileManager.CombinePath(path, typeDesc.Assembly.Name);
            var tmpPath = Path;// EngineNS.IO.FileManager.CombinePath(Path, $"{MetaDirectoryName}");
            if (!EngineNS.IO.TtFileManager.DirectoryExists(tmpPath))
            {
                EngineNS.IO.TtFileManager.CreateDirectory(tmpPath);
            }
            var txtFilepath = EngineNS.IO.TtFileManager.CombinePath(tmpPath, $"typedesc.txt");
            EngineNS.IO.TtFileManager.WriteAllText(txtFilepath, ClassType.TypeString);
            TtEngine.Instance.SourceControlModule.AddFile(txtFilepath);
        }
        public TtMetaVersion BuildCurrentVersion()
        {
            var type = ClassType;
            {
                var attrs = type.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 1)
                {
                    var att = attrs[0] as MetaAttribute;
                    if(!att.IsUnserializable)
                        MetaAttribute = att;
                }
            }
            var result = new TtMetaVersion(this);
            var props = type.SystemType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            foreach(var i in props)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 0)
                    continue;
                var meta = attrs[0] as MetaAttribute;
                var fd = new TtPropertyMeta();
                fd.Build(result, TtTypeDesc.TypeOf(i.PropertyType), i.Name, true);
                Properties.Add(fd);
                if (!meta.IsUnserializable)
                    result.Propertys.Add(fd);
            }
            result.Propertys.Sort();
            string hashString = "";
            foreach(var i in result.Propertys)
            {
                hashString += i.ToString();
            }
            result.MetaHash = EngineNS.Hash64.FromString(hashString).AllData;
            if (MetaAttribute != null || type.GetInterface(nameof(IO.ISerializer)) != null)
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
        public class TtMethodMeta
        {
            public class TtParamMeta
            {
                public MetaParameterAttribute Meta;
                public TtMethodMeta HostMethod;
                public int ParamIndex = -1;
                public System.Reflection.ParameterInfo GetParamInfo()
                {
                    return HostMethod.GetMethod().GetParameters()[ParamIndex];
                }
                public object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    return GetParamInfo().GetCustomAttributes(attributeType, inherit);
                }
                public Rtti.TtTypeDesc ParameterType;
                public bool IsParamArray = false;
                public bool IsDelegate = false;
                public string Name;
                public Bricks.CodeBuilder.EMethodArgumentAttribute ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.Default;
                public object DefaultValue = null;

                public bool HasDefaultValue
                {
                    get => DefaultValue != null;
                }

                public void Init(System.Reflection.ParameterInfo info, TtMethodMeta method, int paramIndex)
                {
                    HostMethod = method;
                    ParamIndex = paramIndex;
                    Name = info.Name;
                    var att = info.GetCustomAttributes(typeof(System.ParamArrayAttribute), true);
                    IsParamArray = (att.Length > 0);
                    IsDelegate = typeof(Delegate).IsAssignableFrom(info.ParameterType);
                    if (info.IsOut)
                    {
                        ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.Out;
                        ParameterType = Rtti.TtTypeDesc.TypeOf(info.ParameterType.GetElementType());
                    }
                    else if (info.IsIn)
                    {
                        ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.In;
                        ParameterType = Rtti.TtTypeDesc.TypeOf(info.ParameterType.GetElementType());
                    }
                    else if (info.ParameterType.IsByRef)
                    {
                        ArgumentAttribute = Bricks.CodeBuilder.EMethodArgumentAttribute.Ref;
                        ParameterType = Rtti.TtTypeDesc.TypeOf(info.ParameterType.GetElementType());
                    }
                    else
                        ParameterType = Rtti.TtTypeDesc.TypeOf(info.ParameterType);
                    if (info.DefaultValue is System.DBNull == false)
                        DefaultValue = info.DefaultValue;
                }
                public bool IsOut
                {
                    get => (ArgumentAttribute == Bricks.CodeBuilder.EMethodArgumentAttribute.Out);
                }
                public bool IsIn
                {
                    get => (ArgumentAttribute == Bricks.CodeBuilder.EMethodArgumentAttribute.In);
                }
                public bool IsRef
                {
                    get => (ArgumentAttribute == Bricks.CodeBuilder.EMethodArgumentAttribute.Ref);
                }
            }
            public MetaAttribute Meta;
            public TtParamMeta[] Parameters;
            public string MethodName;
            public Rtti.TtTypeDesc ReturnType;
            public Rtti.TtTypeDesc DeclaringType;
            public bool IsStatic = false;
            public bool IsVirtual = false;

            private string DeclarName;

            //System.Reflection.MethodInfo mMethodRef = null;
            //public void ResetSystemRef()
            //{
            //    mMethodRef = null;
            //}
            public System.Reflection.MethodInfo GetMethod()
            {
                //return mMethodRef;
                Type[] argTypes = null;
                if (Parameters.Length > 0)
                {
                    argTypes = new Type[Parameters.Length];
                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        argTypes[i] = Parameters[i].ParameterType.SystemType;
                    }
                }
                var result = DeclaringType.GetMethod(this.MethodName, argTypes);
                if (result == null)
                {
                    var methods = DeclaringType.SystemType.GetMethods();
                    foreach (var i in methods)
                    {
                        var ps = i.GetParameters();
                        if (i.Name == this.MethodName && ps.Length == Parameters.Length)
                        {
                            bool bFailed = false;
                            for (int j = 0; j < Parameters.Length; j++)
                            {
                                bool isIn = ps[j].IsIn && Parameters[j].IsIn;
                                bool isOut = ps[j].IsOut && Parameters[j].IsOut;
                                bool isRef = ps[j].ParameterType.IsByRef && Parameters[j].IsRef;
                                if (isIn || isOut || isRef)
                                {
                                    if (ps[j].ParameterType.GetElementType() != Parameters[j].ParameterType.SystemType)
                                    {
                                        bFailed = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (ps[j].ParameterType != Parameters[j].ParameterType.SystemType)
                                    {
                                        bFailed = true;
                                        break;
                                    }
                                }
                            }
                            if (bFailed == false)
                            {
                                result = i;
                                break;
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.Assert(result != null);
                return result;
            }
            public void Build(System.Reflection.MethodInfo method)
            {
                //mMethodRef = method;
                MethodName = method.Name;
                IsStatic = method.IsStatic;
                IsVirtual = method.IsVirtual;
                ReturnType = Rtti.TtTypeDesc.TypeOf(method.ReturnType);
                DeclaringType = Rtti.TtTypeDesc.TypeOf(method.DeclaringType);

                var parameters = method.GetParameters();
                Parameters = new TtParamMeta[parameters.Length];
                DeclarName = $"{method.ReturnType.FullName} {method.DeclaringType.FullName}.{method.Name}(";                
                for (int i = 0; i < parameters.Length; i++)
                {
                    Parameters[i] = new TtParamMeta();
                    Parameters[i].Init(parameters[i], this, i);
                    Parameters[i].Meta = GetParameterMeta(parameters[i]);
                    DeclarName += $"{parameters[i].ParameterType.FullName} {parameters[i].Name}";
                    if (i < parameters.Length - 1)
                        DeclarName += ",";
                }
                DeclarName += ')';
            }
            public TtParamMeta[] GetParameters()
            {
                return Parameters;
            }
            public TtParamMeta GetParameter(int index)
            {
                return Parameters[index];
            }
            public TtParamMeta FindParameter(string name)
            {
                foreach(var i in Parameters)
                {
                    if (i.Name == name)
                        return i;
                }
                return null;
            }
            private MetaParameterAttribute GetParameterMeta(System.Reflection.ParameterInfo info)
            {
                var attrs = info.GetCustomAttributes(typeof(MetaParameterAttribute), false);
                if (attrs.Length == 0)
                    return null;
                //return attrs[index] as MetaParameterAttribute;
                return attrs[0] as MetaParameterAttribute;
            }
            public override string ToString()
            {
                return DeclarName;
            }
            public string GetMethodDeclareString(bool removeDllVersion)
            {
                var result = $"{ReturnType.FullName} {MethodName}(";
                var parameters = GetMethod().GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    result += $"{parameters[i].ParameterType.FullName}";
                    if (i < parameters.Length - 1)
                        result += ",";
                }
                result += ')';
                if (removeDllVersion)
                    return RemoveDeclstringDllVersion(result);
                else
                    return result;
            }
            public object[] GetCustomAttributes(Type type, bool inherit)
            {
                return this.GetMethod().GetCustomAttributes(type, inherit);
            }
            public T GetFirstCustomAttribute<T>(bool inherit) where T : Attribute
            {
                var attrs = this.GetMethod().GetCustomAttributes(typeof(T), inherit);
                if (attrs.Length == 0)
                    return default(T);
                return attrs[0] as T;
            }
            public bool HasReturnValue()
            {
                if (ReturnType == null)
                    return false;
                if (ReturnType.IsEqual(typeof(void)) || ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)))
                    return false;
                return true;
            }
            public bool IsAsync()
            {
                return (ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)) || ReturnType.IsSubclassOf(typeof(System.Threading.Tasks.Task)) ||
                        ReturnType.IsEqual(typeof(Thread.Async.TtTask)) || ReturnType.IsSubclassOf(typeof(Thread.Async.TtTask)));
            }
        }
        public static string GetNameByDeclstring(string declString)
        {
            var start = declString.IndexOf(" ") + 1;
            var end = declString.IndexOf("(");
            return declString.Substring(start, end - start);
        }
        public static string RemoveDeclstringDllVersion(string declString)
        {
            var verIdx = declString.IndexOf("Version=");
            if (verIdx >= 0)
            {
                var endIdx = declString.IndexOf(",", verIdx);
                declString = declString.Remove(verIdx, endIdx - verIdx);
            }
            return declString;
        }
        public List<TtMethodMeta> Methods { get; } = new List<TtMethodMeta>();
        public TtMethodMeta GetMethod(string declString)
        {
            declString = RemoveDeclstringDllVersion(declString);
            foreach(var i in Methods)
            {
                var str = i.GetMethodDeclareString(true);
                if (str == declString)
                    return i;
            }
            return null;
        }
        public TtMethodMeta GetMethodByName(string name)
        {
            foreach (var i in Methods)
            {
                if (name == i.MethodName)
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

                var mth = new TtMethodMeta();
                mth.Meta = attrs[0] as MetaAttribute;
                //mth.Method = i;
                mth.Build(i);
                Methods.Add(mth);
            }
        }
        #endregion

        #region MacrossField
        public class TtFieldMeta
        {
            public MetaAttribute Meta;
            public TtTypeDesc FieldType;
            public string FieldName;
            public TtTypeDesc DeclaringType;
            private string Name;
            System.Reflection.FieldInfo mFieldInfoRef;
            public bool IsStatic
            {
                get
                {
                    var info = GetFieldInfo();
                    if (info != null)
                        return info.IsStatic;
                    return false;
                }
            }
            public bool IsPublic
            {
                get
                {
                    var info = GetFieldInfo();
                    if (info != null)
                        return info.IsPublic;
                    return false;
                }
            }
            public void ResetSystemRef()
            {
                mFieldInfoRef = null;
            }
            public System.Reflection.FieldInfo GetFieldInfo()
            {
                if (mFieldInfoRef == null)
                {
                    mFieldInfoRef = DeclaringType.SystemType.GetField(FieldName);
                }
                return mFieldInfoRef;
            }
            public void Init(System.Reflection.FieldInfo info)
            {
                FieldType = TtTypeDesc.TypeOf(info.FieldType);
                FieldName = info.Name;
                DeclaringType = TtTypeDesc.TypeOf(info.DeclaringType);
                Name = $"{info.FieldType.FullName} {info.Name}";
            }
            public override string ToString()
            {
                return Name;
            }
        }
        public List<TtFieldMeta> Fields { get; } = new List<TtFieldMeta>();
        public void BuildFields()
        {
            Fields.Clear();
            var flds = ClassType.SystemType.GetFields();
            foreach (var i in flds)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length == 0)
                    continue;

                var mth = new TtFieldMeta();
                mth.Meta = attrs[0] as MetaAttribute;
                mth.Init(i);
                Fields.Add(mth);
            }
        }
        public TtFieldMeta GetField(string declString)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].FieldName == declString)
                    return Fields[i];
            }
            return null;
        }
        // MetaVersion中只存储能够保存的property，所以这里需要单独一份包含所有metaattribute的property列表
        public List<TtPropertyMeta> Properties { get; } = new List<TtPropertyMeta>();
        public TtClassMeta.TtPropertyMeta GetProperty(string declString)
        {
            for(int i=0; i<Properties.Count; i++)
            {
                if (Properties[i].PropertyName == declString)
                    return Properties[i];
            }
            return null;
        }
        #endregion

        #region MacrossProperty
        public class TtPropertyMeta : IComparable<TtPropertyMeta>
        {
            public MetaAttribute Meta
            {
                get;
                set;
            }
            System.Reflection.PropertyInfo mPropInfoRef;
            public void ResetSystemRef()
            {
                mPropInfoRef = null;
            }
            public System.Reflection.PropertyInfo PropInfo
            {
                get
                {
                    if (mPropInfoRef == null)
                    {
                        mPropInfoRef = Rtti.TtTypeDesc.GetProperty(HostType.SystemType, PropertyName);
                    }
                    return mPropInfoRef;
                }
            }
            TtTypeDesc mHostType;
            public TtTypeDesc HostType { get => mHostType; }
            string mPropertyName;
            public string PropertyName { get => mPropertyName; }
            public bool IsGetStatic
            {
                get
                {
                    var info = PropInfo;
                    if (info == null)
                        return false;
                    if (info.GetMethod == null)
                        return false;
                    return info.GetMethod.IsStatic;
                }
            }
            public bool IsGetPublic
            {
                get
                {
                    var info = PropInfo;
                    if (info == null)
                        return false;
                    if (info.GetMethod == null)
                        return false;
                    return info.GetMethod.IsPublic;
                }
            }
            public bool IsSetStatic
            {
                get
                {
                    var info = PropInfo;
                    if (info == null)
                        return false;
                    if (info.SetMethod == null)
                        return false;
                    return info.SetMethod.IsStatic;
                }
            }
            public bool IsSetPublic
            {
                get
                {
                    var info = PropInfo;
                    if (info == null)
                        return false;
                    if (info.SetMethod == null)
                        return false;
                    return !info.SetMethod.IsPublic;
                }
            }
            
            public void Build(TtMetaVersion metaVersion, TtTypeDesc propType, string name, bool bUpdateOrder)
            {
                mHostType = metaVersion.HostClass.ClassType;
                mPropertyName = name;
                var info = metaVersion.FindPropertyByName(propType?.SystemType, name);
                if (info == null)
                {
                    mFieldType = propType;
                    CustumSerializer = null;
                    return;
                }
                if (propType == null || propType.SystemType != info.PropertyType)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Property {mHostType}.{name}'s type is not match: {propType}!={info.PropertyType}");
                }
                mPropInfoRef = info;
                mHostType = metaVersion.HostClass.ClassType;
                mFieldType = TtTypeDesc.TypeOf(info.PropertyType);// propType;

                System.Diagnostics.Debug.Assert(info.DeclaringType == metaVersion.HostClass.ClassType.SystemType || metaVersion.HostClass.ClassType.SystemType.IsSubclassOf(info.DeclaringType));
                //System.Diagnostics.Debug.Assert(info.PropertyType == propType.SystemType);
                //System.Diagnostics.Debug.Assert(info.Name == name || );
                
                var attrs = info.GetCustomAttributes(typeof(MetaAttribute), true);
                if (attrs.Length != 0)
                {
                    var meta = attrs[0] as MetaAttribute;
                    this.Meta = meta;
                    if (bUpdateOrder)
                        this.Order = meta.Order;
                }

                attrs = info.GetCustomAttributes(typeof(IO.UCustomSerializerAttribute), true);
                if (attrs.Length != 0)
                {
                    CustumSerializer = attrs[0] as IO.UCustomSerializerAttribute;
                }
            }
            public IO.UCustomSerializerAttribute CustumSerializer;
            Rtti.TtTypeDesc mFieldType;
            public Rtti.TtTypeDesc FieldType
            {
                get => mFieldType;
            }
            public int Order { get; set; } = 0;
            public string FieldTypeStr
            {
                get => mFieldType.TypeString;
            }
            public int CompareTo(TtPropertyMeta other)
            {
                if (other == this)
                    return 0;
                var cmp = Order.CompareTo(other.Order);
                if (cmp > 0)
                    return 1;
                else if (cmp < 0)
                    return -1;
                else
                {
                    cmp = PropertyName.CompareTo(other.PropertyName);
                    if (cmp > 0)
                        return 1;
                    else if (cmp < 0)
                        return -1;
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                        return 0;
                        //return FieldTypeStr.CompareTo(other.FieldTypeStr);
                    }
                }
            }
            public override string ToString()
            {
                return FieldTypeStr + PropertyName;
            }
        }
        #endregion

        public void CheckMetaField()
        {
            foreach (var i in MetaVersions)
            {
                i.Value.CheckMetaProperty();
            }
            BuildMethods();
        }
    }

    public class TtMetaVersionMeta : IO.IAssetMeta
    {
        public override string GetAssetTypeName()
        {
            return "Metadata";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return null;
        }
        //public unsafe override void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        //{
        //    var start = ImGuiAPI.GetItemRectMin();
        //    var end = start + sz;

        //    var name = IO.FileManager.GetPureName(GetAssetName().Name);
        //    var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
        //    Vector2 tpos;
        //    tpos.Y = start.Y + sz.Y - tsz.Y;
        //    tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
        //    ImGuiAPI.PushClipRect(in start, in end, true);

        //    end.Y -= tsz.Y;
        //    OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddRect(in start, in end, (uint)EGui.UCoreStyles.Instance.SnapBorderColor.ToArgb(),
        //        EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

        //    cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
        //    ImGuiAPI.PopClipRect();
        //}
    }
    [Editor.UAssetEditor(EditorType = typeof(Editor.TtMetaVersionViewer))]
    public class TtMetaVersion
    {
        public TtMetaVersion(TtClassMeta kls)
        {
            HostClass = kls;
        }
        public TtClassMeta HostClass
        {
            get;
            private set;
        }
        public UInt64 MetaHash
        {
            get;
            internal set;
        } = 0;
        public void ResetSystemRef()
        {
            foreach (var i in Propertys)
            {
                i.ResetSystemRef();
            }
        }
        public List<TtClassMeta.TtPropertyMeta> Propertys
        {
            get;
        } = new List<TtClassMeta.TtPropertyMeta>();
        public TtClassMeta.TtPropertyMeta GetProperty(string declString)
        {
            for (int i = 0; i < Propertys.Count; i++)
            {
                if (Propertys[i].PropertyName == declString)
                    return Propertys[i];
            }
            return null;
        }
        public void CheckMetaProperty()
        {
            if (HostClass.ClassType.IsRemoved)
            {
                foreach (var i in Propertys)
                {
                    i.Build(this, i.FieldType, null, false);
                }
                return;
            }
            foreach (var i in Propertys)
            {
                //i.PropInfo = FindPropertyByName(i.PropInfo.PropertyType, i.PropertyName);
                i.Build(this, i.FieldType, i.PropertyName, false);
            }
        }
        public System.Reflection.PropertyInfo FindPropertyByName(Type type, string name)
        {
            //var result = HostClass.ClassType.SystemType.GetProperty(name);
            var result = Rtti.TtTypeDesc.GetProperty(HostClass.ClassType.SystemType, name);
            if (result == null && type != null)
            {//if renamed property,check NameAlias
                var props = HostClass.ClassType.SystemType.GetProperties();
                foreach (var i in props)
                {
                    if (i.PropertyType != type)
                        continue;

                    var attrs = i.GetCustomAttributes(typeof(MetaAttribute), false);
                    if (attrs.Length == 0)
                        continue;
                    var metaAttr = attrs[0] as MetaAttribute;
                    if (metaAttr.NameAlias == null)
                        continue;
                    foreach (var j in metaAttr.NameAlias)
                    {
                        if (j == name)
                            return i;
                    }
                }
            }
            else
            {
                //System.Diagnostics.Debug.Assert(result.PropertyType == type);
            }
            return result;
        }
        public bool LoadVersion(UInt64 hash, System.Xml.XmlNode node)
        {
            MetaHash = hash;
            foreach (System.Xml.XmlNode i in node.ChildNodes)
            {
                var fd = new TtClassMeta.TtPropertyMeta();
                string fieldTypeStr = null;
                foreach (System.Xml.XmlAttribute j in i.Attributes)
                {
                    if (j.Name == "Type")
                    {
                        fieldTypeStr = j.Value;
                    }
                    else if (j.Name == "Order")
                    {
                        fd.Order = System.Convert.ToInt32(j.Value);
                    }
                }
                var fieldType = Rtti.TtTypeDesc.TypeOf(fieldTypeStr);
                fd.Build(this, fieldType, i.Name, false);
                if (fd.FieldType == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Property lost: Name = {i.Name}; Type = {fieldTypeStr}");
                }
                Propertys.Add(fd);
            }
            return true;
        }
        public void SaveVersion()
        {
            var tmpPath = HostClass.Path;
            var xmlFilepath = EngineNS.IO.TtFileManager.CombinePath(tmpPath, $"{MetaHash}.metadata");
            if (EngineNS.IO.TtFileManager.FileExists(xmlFilepath))
            {
                return;
            }
            var xml = new System.Xml.XmlDocument();
            var root = xml.CreateElement($"Hash_{MetaHash}", xml.NamespaceURI);
            var fdType = xml.CreateAttribute("Type");
            fdType.Value = HostClass.ClassMetaName;
            root.Attributes.Append(fdType);
            xml.AppendChild(root);
            foreach (var i in Propertys)
            {
                var fd = xml.CreateElement($"{i.PropertyName}", root.NamespaceURI);
                fdType = xml.CreateAttribute("Type");
                fdType.Value = i.FieldTypeStr;
                fd.Attributes.Append(fdType);
                fdType = xml.CreateAttribute("Order");
                fdType.Value = i.Order.ToString();
                fd.Attributes.Append(fdType);
                root.AppendChild(fd);
            }
            var xmlText = EngineNS.IO.TtFileManager.GetXmlText(xml);

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
            //var rootDir = TtEngine.Instance.FileManager.GetPath(rootDirType, IO.FileManager.ESystemDir.MetaData);
            //rootDir += assmemblyDir;
            //var tmpPath = EngineNS.IO.FileManager.CombinePath(rootDir, $"{HostClass.MetaDirectoryName}");
            
            if (!EngineNS.IO.TtFileManager.DirectoryExists(tmpPath))
            {
                EngineNS.IO.TtFileManager.CreateDirectory(tmpPath);
            }
            
            EngineNS.IO.TtFileManager.WriteAllText(xmlFilepath, xmlText);

            TtEngine.Instance.SourceControlModule.AddFile(xmlFilepath);
        }
    }

    public class TtClassMetaManager
    {
        public static TtClassMetaManager Instance { get; } = new TtClassMetaManager();
        Dictionary<string, TtClassMeta> mMetas = new Dictionary<string, TtClassMeta>();
        public Dictionary<string, TtClassMeta> Metas
        {
            get => mMetas;
        }
        Dictionary<Hash64, TtClassMeta> mHashMetas = new Dictionary<Hash64, TtClassMeta>();

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public UTypeTreeManager TreeManager = new UTypeTreeManager();
        public string MetaRoot;
在此之后:
        public TtTypeTreeManager TreeManager = new UTypeTreeManager();
        public string MetaRoot;
*/
        public TtTypeTreeManager TreeManager = new TtTypeTreeManager();
        public string MetaRoot;
        public void LoadMetas(string moduleName = null)
        {
            var rootTypes = new IO.TtFileManager.ERootDir[2] { IO.TtFileManager.ERootDir.Engine, IO.TtFileManager.ERootDir.Game };
            foreach (var r in rootTypes)
            {
                var metaRoot = TtEngine.Instance.FileManager.GetPath(r, IO.TtFileManager.ESystemDir.MetaData);
                if (EngineNS.IO.TtFileManager.DirectoryExists(metaRoot) == false)
                {
                    continue;
                }
                var services = EngineNS.IO.TtFileManager.GetDirectories(metaRoot, "*.*", false);
                foreach (var i in services)
                {
                    var assemblies = EngineNS.IO.TtFileManager.GetDirectories(i, "*.*", true);
                    foreach (var j in assemblies)
                    {
                        var kls = EngineNS.IO.TtFileManager.GetDirectories(j, "*.*", false);
                        foreach (var k in kls)
                        {
                            var tmpPath = EngineNS.IO.TtFileManager.CombinePath(k, $"typedesc.txt");
                            var strName = EngineNS.IO.TtFileManager.ReadAllText(tmpPath);
                            if (strName == null)
                                continue;
                            var type = TtTypeDesc.TypeOf(strName);// EngineNS.Rtti.UTypeDescManager.Instance.GetTypeDescFromString(strName);
                            if (type != null)
                            {
                                if (moduleName == null || (moduleName != null && type.Assembly.Name == moduleName))
                                {
                                    var meta = new TtClassMeta(type);
                                    meta.LoadClass(k);

                                    //mMetas.Add(meta.ClassMetaName, meta);
                                    mMetas[meta.ClassMetaName] = meta;
                                }
                            }
                        }
                    }
                }
            }

            //编辑器状态，程序修改过执行
            BuildMeta(moduleName);

            foreach (var i in mMetas)
            {
                var hashCode = Hash64.FromString(i.Key);
                TtClassMeta meta;
                if (mHashMetas.TryGetValue(hashCode, out meta))
                {
                    if (i.Value.ClassMetaName != meta.ClassMetaName)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Fatal, $"Same Hash:{i.Value.ClassMetaName} == {meta.ClassMetaName}");
                        System.Diagnostics.Debug.Assert(false);
                    }
                    continue;
                }
                var attr = i.Value.ClassType.GetCustomAttribute<Rtti.MetaAttribute>(false);
                if (attr != null && attr.NameAlias != null)
                {
                    foreach (var n in attr.NameAlias)
                    {
                        var aliasCode = Hash64.FromString(n);
                        mHashMetas[aliasCode] = i.Value;
                    }
                }
                
                mHashMetas[hashCode] = i.Value;

                TreeManager.RegType(i.Value);
                i.Value.BuildMethods();
                i.Value.BuildFields();
            }

            if (moduleName != null)
                return;
            
            //ForceSaveAll();

            GetMeta(TtTypeDesc.TypeOf(typeof(void)));
            GetMeta(TtTypeDesc.TypeOf(typeof(char)));
            GetMeta(TtTypeDesc.TypeOf(typeof(byte)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Int16)));
            GetMeta(TtTypeDesc.TypeOf(typeof(UInt16)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Int32)));
            GetMeta(TtTypeDesc.TypeOf(typeof(UInt32)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Int64)));
            GetMeta(TtTypeDesc.TypeOf(typeof(UInt64)));
            GetMeta(TtTypeDesc.TypeOf(typeof(float)));
            GetMeta(TtTypeDesc.TypeOf(typeof(double)));
            GetMeta(TtTypeDesc.TypeOf(typeof(string)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Vector2)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Vector3)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Vector4)));
            GetMeta(TtTypeDesc.TypeOf(typeof(Quaternion)));
        }
        public void BuildMeta(string moduleName = null)
        {
            foreach (var i in TtTypeDescManager.Instance.Services)
            {
                var klsColloector = new List<TtClassMeta>();

                bool bFinished = false;
                while (bFinished == false)
                {
                    try
                    {
                        foreach (var j in i.Value.Types)
                        {
                            if (moduleName != null && j.Value.Assembly.Name != moduleName)
                            {
                                continue;
                            }
                            MetaAttribute meta;
                            if (j.Value.GetInterface(nameof(EngineNS.IO.ISerializer)) == null)
                            {
                                var attrs = j.Value.GetCustomAttributes(typeof(MetaAttribute), true);
                                if (attrs.Length == 0)
                                    continue;
                                meta = attrs[0] as MetaAttribute;
                            }

                            TtClassMeta kls = null;
                            var metaName = j.Value.TypeString;// Rtti.UTypeDescManager.Instance.GetTypeStringFromType(j.Value.SystemType);
                            if (mMetas.TryGetValue(metaName, out kls) == false)
                            {
                                kls = new TtClassMeta(j.Value);
                                kls.Path = GetPath(kls);
                                var ver = kls.BuildCurrentVersion();
                                mMetas.Add(metaName, kls);
                                kls.SaveClass();
                            }
                            else
                            {
                                klsColloector.Add(kls);
                                //var ver = kls.BuildCurrentVersion();
                            }
                        }
                        bFinished = true;
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        Profiler.Log.WriteException(ex);
                        bFinished = false;
                    }
                }

                foreach (var j in klsColloector)
                {
                    var ver = j.BuildCurrentVersion();
                }
            }
        }
        public void ResetSystemRef()
        {
            foreach (var i in mMetas)
            {
                i.Value.ResetSystemRef();
            }
        }
        public void ForceSaveAll()
        {
            foreach (var i in mMetas)
            {
                i.Value.Path = GetPath(i.Value);
                IO.TtFileManager.SureDirectory(i.Value.Path);
                foreach (var j in i.Value.MetaVersions)
                {
                    j.Value.SaveVersion();
                }

                var txtFilepath = EngineNS.IO.TtFileManager.CombinePath(i.Value.Path, $"typedesc.txt");
                if (EngineNS.IO.TtFileManager.FileExists(txtFilepath) == false)
                {
                    EngineNS.IO.TtFileManager.WriteAllText(txtFilepath, i.Value.ClassType.TypeString);
                    TtEngine.Instance.SourceControlModule.AddFile(txtFilepath);
                }
            }
        }
        string GetPath(TtClassMeta classMeta)
        {
            string root = "";
            if (classMeta.ClassType.Assembly.IsGameModule)
                root = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Game, IO.TtFileManager.ESystemDir.MetaData);
            else
                root = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Engine, IO.TtFileManager.ESystemDir.MetaData);

            var dir = classMeta.ClassType.Assembly.Service;
            dir += "." + classMeta.ClassType.Assembly.Name;
            if(!string.IsNullOrEmpty(classMeta.ClassType.Namespace))
                dir += "." + classMeta.ClassType.Namespace;

            dir = dir.Replace('.', '/');
            dir = root + dir;

            string name;
            if (string.IsNullOrEmpty(classMeta.ClassType.Namespace))
                name = classMeta.ClassType.FullName;
            else
                name = classMeta.ClassType.FullName.Substring(classMeta.ClassType.Namespace.Length);
            if (name.StartsWith('.'))
            {
                name = name.Substring(1);
            }
            if (name.Length <250)
            {
                return dir + "/a0." + name;
            }
            else
            {
                return dir + "/a0." + classMeta.MetaDirectoryName;
            }
        }
        public TtClassMeta GetMeta(string name, bool bTryBuild = true)
        {
            TtClassMeta meta;
            if (mMetas.TryGetValue(name, out meta))
                return meta;
            if (bTryBuild == false)
                return null;
            lock (this)
            {
                var type = Rtti.TtTypeDesc.TypeOf(name);
                if (type == null)
                    return null;
                TtClassMeta result;
                if (TypeMetas.TryGetValue(type, out result))
                {
                    return result;
                }
                else
                {
                    result = new TtClassMeta(type);
                    result.Path = GetPath(result);
                    result.BuildMethods();
                    result.BuildFields();
                    result.BuildCurrentVersion();
                    TypeMetas.Add(type, result);
                    return result;
                }
            }
        }
        public Dictionary<TtTypeDesc, TtClassMeta> TypeMetas { get; } = new Dictionary<TtTypeDesc, TtClassMeta>();
        public TtClassMeta GetMetaFromFullName(string fname)
        {
            var desc = Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(fname);
            if (desc == null)
                return null;
            return GetMeta(desc);
        }
        public TtClassMeta GetMeta(TtTypeDesc type)
        {
            if (type == null)
                return null;
            lock (this)
            {
                TtClassMeta result;
                if (TypeMetas.TryGetValue(type, out result))
                {
                    return result;
                }
                var typeStr = type.TypeString; //Rtti.TtTypeDesc.TypeStr(type.SystemType);
                return GetMeta(typeStr);
            }
        }
        public TtClassMeta GetMeta(in Hash64 hash)
        {
            TtClassMeta meta;
            if (mHashMetas.TryGetValue(hash, out meta))
                return meta;
            return null;
        }
    }
}
