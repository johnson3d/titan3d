using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MetaClassAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MetaDataAttribute : Attribute
    {
        public EngineNS.IO.Serializer.EIOType AllowIOType = EngineNS.IO.Serializer.EIOType.All;
        [Flags]
        public enum enSaveType : byte
        {
            None = 0,
            Xnd = 1 << 0,
            Xml = 1 << 1,
            All = byte.MaxValue,
        }
        public enSaveType SaveType = enSaveType.All;
        public MetaDataAttribute()
        {

        }
        public MetaDataAttribute(enSaveType saveType)
        {
            SaveType = saveType;
        }
        public MetaDataAttribute(EngineNS.IO.Serializer.EIOType allowIOType)
        {
            AllowIOType = allowIOType;
        }
        public bool HasSaveType(enSaveType type)
        {
            return (SaveType & type) == type;
        }
    }
    public abstract class MemberDesc
    {
        public abstract System.Type MemberType
        {
            get;
        }
        public abstract System.Type DeclaringType
        {
            get;
        }
        public abstract string Name
        {
            get;
        }
        public abstract bool CanWrite
        {
            get;
        }
        protected EngineNS.IO.Serializer.EIOType mAllowIOType = IO.Serializer.EIOType.All;
        public EngineNS.IO.Serializer.EIOType AllowIOType
        {
            get => mAllowIOType;
        }
        public abstract void SetValue(object host, object v);
        public abstract object GetValue(object host);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);
    }
    public class PropMemberDesc : MemberDesc
    {
        public System.Reflection.PropertyInfo mInfo;
        public PropMemberDesc(System.Reflection.PropertyInfo p)
        {
            mInfo = p;
            var atts = mInfo.GetCustomAttributes(typeof(MetaDataAttribute), true);
            if(atts.Length > 0)
            {
                var att = atts[0] as MetaDataAttribute;
                mAllowIOType = att.AllowIOType;
            }
        }
        public override System.Type MemberType
        {
            get { return mInfo.PropertyType; }
        }
        public override System.Type DeclaringType
        {
            get
            {
                return mInfo.DeclaringType;
            }
        }
        public override string Name
        {
            get { return mInfo.Name; }
        }
        public override bool CanWrite
        {
            get { return mInfo.CanWrite; }
        }
        public override void SetValue(object host, object v)
        {
            try
            {
                if (mInfo.DeclaringType == host.GetType() || host.GetType().IsSubclassOf(mInfo.DeclaringType))
                {
                    mInfo.SetValue(host, v);
                }
                else
                {
                    var tmp = host.GetType().GetProperty(mInfo.Name);
                    if (tmp != null)
                    {
                        mInfo = tmp;
                        tmp.SetValue(host, v);
                    }
                }
            }
            catch(System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public override object GetValue(object host)
        {
            try
            {
                if (mInfo.DeclaringType == host.GetType() || host.GetType().IsSubclassOf(mInfo.DeclaringType))
                {
                    return mInfo.GetValue(host);
                }
                else
                {
                    var tmp = host.GetType().GetProperty(mInfo.Name);
                    if (tmp != null)
                    {
                        mInfo = tmp;
                        return tmp.GetValue(host);
                    }
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return mInfo.GetCustomAttributes(attributeType, inherit);
        }
    }
    public class FieldMemberDesc : MemberDesc
    {
        public System.Reflection.FieldInfo mInfo;
        public FieldMemberDesc(System.Reflection.FieldInfo p)
        {
            mInfo = p;
            var atts = mInfo.GetCustomAttributes(typeof(MetaDataAttribute), true);
            if (atts.Length > 0)
            {
                var att = atts[0] as MetaDataAttribute;
                mAllowIOType = att.AllowIOType;
            }
        }
        public override System.Type MemberType
        {
            get { return mInfo.FieldType; }
        }
        public override System.Type DeclaringType
        {
            get
            {
                return mInfo.DeclaringType;
            }
        }
        public override string Name
        {
            get { return mInfo.Name; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }
        public override void SetValue(object host, object v)
        {
            try
            {
                mInfo.SetValue(host, v);
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public override object GetValue(object host)
        {
            System.Diagnostics.Debug.Assert(mInfo.IsPublic);
            return mInfo.GetValue(host);
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return mInfo.GetCustomAttributes(attributeType, inherit);
        }
    }
    public class LostedMemberDesc : MemberDesc
    {
        public System.Reflection.FieldInfo mInfo;
        public LostedMemberDesc(System.Type type, System.Type declType, string name)
        {
            mName = name;
            mMemberType = type;
            mDeclaringType = declType;
        }
        private System.Type mMemberType;
        public override System.Type MemberType
        {
            get { return mMemberType; }
        }
        private System.Type mDeclaringType;
        public override System.Type DeclaringType
        {
            get
            {
                return mDeclaringType;
            }
        }
        private string mName;
        public override string Name
        {
            get { return mName; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override void SetValue(object host, object v)
        {
            
        }
        public override object GetValue(object host)
        {
            //System.Diagnostics.Debug.Assert(false);
            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Rtti", $"LostedMemberDesc.GetValue: {Name},{mMemberType.FullName}");
            return System.Activator.CreateInstance(mMemberType);
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return null;
        }
    }
    public class MetaData
    {
        private string mAbsFileName;
        private bool mIsValid = false;
        public bool IsValid
        {
            get { return mIsValid; }
        }
        public MetaData(string absFileName)
        {
            mAbsFileName = absFileName;
            mIsValid = false;
        }
        public MetaData()
        {
            mIsValid = false;
        }
        public UInt32 MetaHash
        {
            get;
            protected set;
        }
        public System.Type MetaType
        {
            get;
            protected set;
        }
        public class FieldDesc
        {
            public string Name;
            public MemberDesc PropInfo;
            public MetaDataAttribute Descripter;
            public IO.Serializer.FieldSerializer Serializer;
            public bool IsList;
            public static UInt32 SortAndCalHash_1(Type type, List<FieldDesc> Members)
            {
                string hashStr = Rtti.RttiHelper.GetTypeMetaHashString(type) + "->\n";
                Members.Sort((Rtti.MetaData.FieldDesc a, Rtti.MetaData.FieldDesc b) =>
                {
                    return a.PropInfo.Name.CompareTo(b.PropInfo.Name);
                });
                foreach (var i in Members)
                {
                    hashStr += Rtti.RttiHelper.GetTypeMetaHashString(i.PropInfo.MemberType) + ":" + i.PropInfo.Name + ";\n";
                }
                return UniHash.APHash(hashStr);
            }
            public static UInt32 SortAndCalHash_2(Type type, List<FieldDesc> Members)
            {
                Members.Sort((x, y) => x.Name.CompareTo(y.Name));
                string str = Rtti.RttiHelper.GetTypeMetaHashString(type);
                foreach (var i in Members)
                {
                    str += Rtti.RttiHelper.GetTypeMetaHashString(i.PropInfo.MemberType) + i.Name;
                }
                return UniHash.APHash(str);
            }
            public static UInt32 SortAndCalHash(Type type, List<FieldDesc> Members)
            {
                return SortAndCalHash_2(type, Members);
            }
        }
        public List<FieldDesc> Members
        {
            get;
        } = new List<FieldDesc>();
        public int FindMemberIndex(string name)
        {
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }
        public void UpdateMataHash()
        {
            MetaHash = FieldDesc.SortAndCalHash(MetaType, Members);
            //string str = Rtti.RttiHelper.GetTypeSaveString(MetaType);
            //foreach(var i in Members)
            //{
            //    str += Rtti.RttiHelper.GetTypeSaveString(i.PropInfo.MemberType) + i.Name;
            //}
            //MetaHash = UniHash.APHash(str);
        }
        public void BuildMetaData(Type type)
        {
            MetaType = type;
            Members.Clear();
            var props = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            foreach(var i in props)
            {
                if (i.CanRead == false || i.CanWrite == false)
                    continue;
                var attrs = i.GetCustomAttributes(typeof(MetaDataAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var metaAttr = attrs[0] as MetaDataAttribute;
                if (!metaAttr.HasSaveType(MetaDataAttribute.enSaveType.Xnd))
                    continue;

                IO.Serializer.CustomSerializer custSerializer = null;
                attrs = i.GetCustomAttributes(typeof(IO.Serializer.CustomFieldSerializerAttribute), true);
                if (attrs != null && attrs.Length != 0)
                {
                    var fsdesc = attrs[0] as IO.Serializer.CustomFieldSerializerAttribute;
                    if (fsdesc != null && fsdesc.SerializerType != null)
                    {
                        custSerializer = System.Activator.CreateInstance(fsdesc.SerializerType) as IO.Serializer.CustomSerializer;
                    }
                }

                var item = new FieldDesc();
                item.Name = i.Name;
                item.PropInfo = new Rtti.PropMemberDesc(i);
                item.Descripter = metaAttr;
                if (custSerializer != null)
                {
                    item.Serializer = custSerializer;
                    item.IsList = false;
                }
                else
                {
                    item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(item.PropInfo.MemberType);
                    if (item.PropInfo.MemberType.IsGenericType && (item.PropInfo.MemberType.GetInterface(typeof(System.Collections.IList).FullName) != null))
                    {
                        var argType = item.PropInfo.MemberType.GenericTypeArguments[0];
                        item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(argType);
                        item.IsList = true;
                    }
                    else
                        item.IsList = false;
                }
                if(item.Serializer==null)
                {
                    throw new TypeInitializationException("", null);
                }
                Members.Add(item);
            }
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            foreach (var i in fields)
            {
                var attrs = i.GetCustomAttributes(typeof(MetaDataAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var metaAttr = attrs[0] as MetaDataAttribute;
                if (!metaAttr.HasSaveType(MetaDataAttribute.enSaveType.Xnd))
                    continue;

                IO.Serializer.CustomSerializer custSerializer = null;
                attrs = i.GetCustomAttributes(typeof(IO.Serializer.CustomFieldSerializerAttribute), true);
                if (attrs != null && attrs.Length != 0)
                {
                    var fsdesc = attrs[0] as IO.Serializer.CustomFieldSerializerAttribute;
                    if (fsdesc != null && fsdesc.SerializerType != null)
                    {
                        custSerializer = System.Activator.CreateInstance(fsdesc.SerializerType) as IO.Serializer.CustomSerializer;
                    }
                }

                var item = new FieldDesc();
                item.Name = i.Name;
                item.PropInfo = new Rtti.FieldMemberDesc(i);
                item.Descripter = metaAttr;
                if (custSerializer != null)
                {
                    item.Serializer = custSerializer;
                    item.IsList = false;
                }
                else
                {
                    item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(item.PropInfo.MemberType);
                    if (item.PropInfo.MemberType.IsGenericType && (item.PropInfo.MemberType.GetInterface(typeof(System.Collections.IList).FullName) != null))
                    {
                        var argType = item.PropInfo.MemberType.GenericTypeArguments[0];
                        item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(argType);
                        item.IsList = true;
                    }
                    else
                        item.IsList = false;
                }
                Members.Add(item);
            }
            Members.Sort((x, y) => x.Name.CompareTo(y.Name));
            UpdateMataHash();

            mIsValid = true;
        }
        public void Save2Xnd(IO.XndNode node)
        {
            var attr = node.AddAttrib("Properties");
            attr.BeginWrite();
            attr.Write(RttiHelper.GetTypeSaveString(MetaType));
            attr.Write(MetaHash);
            attr.Write(Members.Count);
            foreach(var i in Members)
            {
                attr.Write(RttiHelper.GetTypeSaveString(i.PropInfo.MemberType));
                attr.Write(i.Name);
            }
            attr.EndWrite();
        }
        private bool LoadXnd(IO.XndNode node, out bool isRedirection, out bool needSave)
        {
            needSave = false;
            isRedirection = false;
            var attr = node.FindAttrib("Properties");
            if (attr == null)
                return false;
            try
            {
                attr.BeginRead();
                string metaStr;
                attr.Read(out metaStr);
                MetaType = RttiHelper.GetTypeFromSaveString(metaStr, out isRedirection);
                if (MetaType == null)
                {
                    return false;
                }
                var method = MetaType.GetMethod("WhenMetaDataBeginLoad");
                if (method != null && method.IsStatic)
                {
                    method.Invoke(null, null);
                }

                UInt32 hash;
                attr.Read(out hash);
                MetaHash = hash;

                Members.Clear();
                int count = 0;
                attr.Read(out count);
                bool hasRedirectionType = false;
                for (int i = 0; i < count; i++)
                {
                    var item = new FieldDesc();
                    string typeStr;
                    attr.Read(out typeStr);
                    attr.Read(out item.Name);
                    var saveType = RttiHelper.GetTypeFromSaveString(typeStr, out hasRedirectionType);
                    if (hasRedirectionType)
                    {//如果有成员变量被重定向成别的类型，需要用到的metadata存盘成新的定向后的数据
                        needSave = true;
                    }
                    isRedirection = isRedirection || hasRedirectionType;
                    if (saveType == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"RttiHelper.GetTypeFromSaveString == null: {typeStr}");
                    }
                    var pinfo = MetaType.GetProperty(item.Name);
                    if (pinfo != null)
                    {
                        if (saveType == pinfo.PropertyType)
                        {
                            item.PropInfo = new Rtti.PropMemberDesc(pinfo);
                        }
                        else
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"MetaData Property {item.Name} SaveType({typeStr}) != Type({pinfo.PropertyType.ToString()})");
                            item.PropInfo = new Rtti.LostedMemberDesc(saveType, MetaType, item.Name);
                        }
                    }
                    var finfo = MetaType.GetField(item.Name);
                    if (finfo != null)
                    {
                        if (saveType == finfo.FieldType)
                        {
                            System.Diagnostics.Debug.Assert(item.PropInfo == null);
                            item.PropInfo = new Rtti.FieldMemberDesc(finfo);
                        }
                        else
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"MetaData Field {item.Name} SaveType({typeStr}) != Type({finfo.FieldType.ToString()})");
                            //item.PropInfo = new Rtti.FieldMemberDesc(finfo);
                            item.PropInfo = new Rtti.LostedMemberDesc(saveType, MetaType, item.Name);
                        }
                    }

                    IO.Serializer.CustomSerializer custSerializer = null;
                    if (item.PropInfo == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"MetaData can't find PropInfo: {item.Name}");
                        item.PropInfo = new Rtti.LostedMemberDesc(saveType, MetaType, item.Name);
                    }
                    else
                    {
                        var attrs = item.PropInfo.GetCustomAttributes(typeof(IO.Serializer.CustomFieldSerializerAttribute), true);
                        if (attrs != null && attrs.Length != 0)
                        {
                            var fsdesc = attrs[0] as IO.Serializer.CustomFieldSerializerAttribute;
                            if (fsdesc != null && fsdesc.SerializerType != null)
                            {
                                custSerializer = System.Activator.CreateInstance(fsdesc.SerializerType) as IO.Serializer.CustomSerializer;
                            }
                        }
                    }
                    if (custSerializer != null)
                    {
                        item.Serializer = custSerializer;
                        item.IsList = false;
                    }
                    else if (saveType != null)
                    {
                        item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(saveType);
                        if (saveType.IsGenericType && (saveType.GetInterface(typeof(System.Collections.IList).FullName) != null))
                        {
                            var argType = saveType.GenericTypeArguments[0];
                            item.Serializer = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(argType);
                            item.IsList = true;
                        }
                        else
                            item.IsList = false;
                    }
                    if (item.Serializer == null)
                    {
                        throw new TypeInitializationException(typeStr, null);
                    }
                    Members.Add(item);
                }
            }
            finally
            {
                attr.EndRead();
            }
            
            
            return true;
        }
        public void LoadMetaData(string absFileName, out bool isRedirection)
        {
            lock(this)
            {
                isRedirection = false;
                bool isNeedSave = false;
                using (var xnd = IO.XndHolder.SyncLoadXND(absFileName))
                {
                    try
                    {
                        if (this.LoadXnd(xnd.Node, out isRedirection, out isNeedSave) == false)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"MetaData {absFileName} Load failed");
                        }
                        else
                        {
                            mIsValid = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }

                if (isNeedSave)
                {
                    var saveXnd = IO.XndHolder.NewXNDHolder();
                    this.Save2Xnd(saveXnd.Node);
                    IO.XndHolder.SaveXND(absFileName, saveXnd);
                }
            }
        }
        public void SureLoaded()
        {
            lock (this)
            {
                if (mIsValid)
                    return;

                bool isRedirection = false;
                LoadMetaData(mAbsFileName, out isRedirection);
            }
        }
    }
    public class MetaClass
    {
        private Dictionary<UInt32, MetaData> Metas
        {
            get;
        } = new Dictionary<uint, MetaData>();
        public RName ClassName
        {
            get;
            set;
        }
        public Type MetaType
        {
            get;
            set;
        }
        public MetaData CurrentVersion
        {
            get;
            set;
        }
        public int MetaNum
        {
            get { return Metas.Count; }
        }
        public MetaData FindMetaData(UInt32 hash)
        {
            MetaData result;
            if (Metas.TryGetValue(hash, out result) == false)
                return null;

            result.SureLoaded();
            return result;
        }
        public void RegMetaData(UInt32 hash, MetaData meta)
        {
            Metas[hash] = meta;
        }
        public void LoadXnd(out bool hasRedirectionType)
        {
            hasRedirectionType = false;

            Hash64 hash = GetFolderHash();
            var address = ClassName.GetDirectory() + hash.ToString().ToLower() + "/";
            var files = CEngine.Instance.FileManager.GetFiles(address, "*.metadata");
            if(files==null || files.Count==0)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"MetaClass {address} is empty");
            }
            
            foreach (var i in files)
            {
                var MetaHash = CEngine.Instance.FileManager.GetPureFileFromFullName(i, false);
                var meta = new MetaData(i);
                this.Metas[System.Convert.ToUInt32(MetaHash)] = meta;

                if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
                {
                    bool isRedirection = false;
                    meta.LoadMetaData(i, out isRedirection);
                    System.Diagnostics.Debug.Assert(MetaHash == meta.MetaHash.ToString());
                    if (isRedirection)
                    {
                        hasRedirectionType = true;
                    }
                }
            }
        }
        public void Save2Xnd(bool saveTypeNameFile)
        {
            var address = ClassName.Address.Replace("+", ".");
            if(saveTypeNameFile)
            {
                var sw = new System.IO.StreamWriter(address + "/typename.txt", false);
                var saveStr = Rtti.RttiHelper.GetTypeSaveString(MetaType);
                sw.WriteLine(saveStr);
                sw.Close();
            }
            foreach (var i in Metas)
            {
                var xnd = IO.XndHolder.NewXNDHolder();

                i.Value.Save2Xnd(xnd.Node);

                IO.XndHolder.SaveXND(address + "/" + i.Key + ".MetaData", xnd);
            }
        }

        public Hash64 GetFolderHash()
        {
            var cname = RttiHelper.GetTypeSaveString(MetaType);
            cname = cname.Replace('+', '.');
            cname = cname.ToLower();
            var idx = cname.IndexOf('|');
            if (idx >= 0)
                cname = cname.Substring(idx + 1);
            Hash64 hash = new Hash64();
            Hash64.CalcHash64(ref hash, cname);
            return hash;
        }

        public static void CopyData(object oldObj, object newObj)
        {
            if (oldObj == null || newObj == null)
                return;
            var oldType = oldObj.GetType();
            var newType = newObj.GetType();

            var props = oldType.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var attrs = props[i].GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var macrosAttr = attrs[0] as EngineNS.Editor.MacrossMemberAttribute;
                if (macrosAttr.HasType(Editor.MacrossMemberAttribute.enMacrossType.IgnoreCopy))
                    continue;

                try
                {
                    var op = newType.GetProperty(props[i].Name);
                    if (op != null && props[i].PropertyType == op.PropertyType)
                    {
                        if (op.CanWrite)
                        {
                            var value = props[i].GetValue(oldObj);
                            op.SetValue(newObj, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
        }
    }

    public class MetaClassManager
    {
        public Dictionary<string, MetaClass> Klasses
        {
            get;
        } = new Dictionary<string, MetaClass>();
        //public Dictionary<UInt32, MetaData> MetaDatas
        //{
        //    get;
        //} = new Dictionary<UInt32, MetaData>();
        public RName MetaDirectory
        {
            get;
            set;
        }

        public void LoadMetaClasses()
        {
            var t1 = Support.Time.HighPrecision_GetTickCount();
            var dirs = CEngine.Instance.FileManager.GetDirectories(MetaDirectory.Address);
            if(dirs==null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"MetaDirectory.Address is null:{MetaDirectory.Address}");
                return;
            }
            foreach(var i in dirs)
            {
                if (false == CEngine.Instance.FileManager.FileExists(i + "/typename.txt"))
                    continue;

                //System.IO.StreamReader sr = new System.IO.StreamReader(i + "/typename.txt", System.Text.Encoding.ASCII);
                //string className = sr.ReadLine();
                //sr.Close();

                byte[] bytes = IO.FileManager.ReadFile(i + "/typename.txt");
                string className = System.Text.Encoding.ASCII.GetString(bytes);
                className = className.Replace("\r\n", "");

                ///////////////////////////////////////////////////////////////////
                //if (className.Contains("enginecore"))
                //{
                //    using (var sw = new System.IO.StreamWriter(i + "/typename.txt", false, System.Text.Encoding.ASCII))
                //    {
                //        className = className.Replace("enginecore", "EngineCore");
                //        sw.WriteLine(className);
                //    }
                //}
                //if (className.Contains("Common|EngineCore"))
                //{
                //    using (var sw = new System.IO.StreamWriter(i + "/typename.txt", false, System.Text.Encoding.ASCII))
                //    {
                //        className = className.Replace("Common|EngineCore", "Client|EngineCore");
                //        sw.WriteLine(className);
                //    }
                //}
                ///////////////////////////////////////////////////////////////////
                bool isRedirection;
                var type = RttiHelper.GetTypeFromSaveString(className, out isRedirection);
                if (type == null)
                    continue;

                if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
                {
                    var noUsedFile = i + "/" + type.FullName + ".noused";
                    if (CEngine.Instance.FileManager.FileExists(noUsedFile) == false)
                        CEngine.Instance.FileManager.CreateFile(noUsedFile);
                }

                MetaClass klass = new MetaClass();
                klass.MetaType = type;
                var csIdx = className.IndexOf('|');
                var fileStr = className;
                if (csIdx >= 0)
                    fileStr = className.Substring(csIdx + 1);
                klass.ClassName = RName.GetRName(MetaDirectory.Name + "/" + fileStr);
                bool hasRedirection = false;
                klass.LoadXnd(out hasRedirection);
#if PWindow
                //if (hasRedirection)
                //    klass.Save2Xnd(false);
#endif

                // 重定向后由于原来的类型已不存在，不再进行当前版本的处理
                if(!isRedirection)
                {
                    MetaData curVer = new MetaData();
                    curVer.BuildMetaData(type);

                    MetaData data = klass.FindMetaData(curVer.MetaHash);
                    if (data == null)
                    {
                        klass.RegMetaData(curVer.MetaHash, curVer);

#if PWindow
                        var xnd = IO.XndHolder.NewXNDHolder();
                        curVer.Save2Xnd(xnd.Node);
                        var hash = klass.GetFolderHash();
                        var file = klass.ClassName.GetRootFolder() + "MetaClasses/" + hash.ToString() + "/" + curVer.MetaHash + ".MetaData";
                        IO.XndHolder.SaveXND(file, xnd);
                        klass.CurrentVersion = curVer;
#endif
                    }
                    else
                    {
                        klass.CurrentVersion = data;
                    }
                }

                Klasses.Add(className, klass);
            }

            var t2 = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "MetaData", $"LoadMetaClasses Time:{t2-t1}");
            //MetaDatas.Clear();
            //foreach (var i in Klasses)
            //{
            //    foreach (var j in i.Value.Metas)
            //    {
            //        MetaDatas.Add(j.Key, j.Value);
            //    }
            //}
        }
        public void CheckNewMetaClass()
        {
            var assems = Rtti.RttiHelper.GetAnalyseAssemblys(ECSType.All);
            foreach(var i in assems)
            {
                var types = i.Assembly.GetTypes();
                foreach (var j in types)
                {
                    var attrs = j.GetCustomAttributes(typeof(MetaClassAttribute), true);
                    if (attrs == null || attrs.Length == 0)
                        continue;
                    var cname = RttiHelper.GetTypeSaveString(j);
                    MetaClass klass;
                    if (Klasses.TryGetValue(cname, out klass))
                        continue;

                    CreateMetaClass(j);
                }
            }
        }
        public void RefreshMetaClass(Rtti.VAssembly assm)
        {
            if (assm == null)
                return;
            var types = assm.Assembly.GetTypes();
            foreach (var j in types)
            {
                var attrs = j.GetCustomAttributes(typeof(MetaClassAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var cname = RttiHelper.GetTypeSaveString(j);
                MetaClass klass;
                if (Klasses.TryGetValue(cname, out klass))
                {
                    bool hasRedirection = false;
                    if (klass.MetaNum == 0)
                    {
                        klass.LoadXnd(out hasRedirection);
                    }
                    if (!hasRedirection)
                    {
                        MetaData curVer = new MetaData();
                        curVer.BuildMetaData(j);

                        MetaData data = klass.FindMetaData(curVer.MetaHash);
                        if (data == null)
                        {
                            klass.RegMetaData(curVer.MetaHash, curVer);
#if PWindow
                            var xnd = IO.XndHolder.NewXNDHolder();
                            curVer.Save2Xnd(xnd.Node);
                            var hash = klass.GetFolderHash();
                            var file = klass.ClassName.GetRootFolder() + "MetaClasses/" + hash.ToString() + "/" + curVer.MetaHash + ".MetaData";
                            IO.XndHolder.SaveXND(file, xnd);
                            klass.CurrentVersion = curVer;
#endif
                        }
                        else
                        {
                            klass.CurrentVersion = data;
                        }
                    }
                }
                else
                {
                    CreateMetaClass(j);
                }
            }
        }

        MetaClass CreateMetaClass(System.Type type)
        {
            var cname = RttiHelper.GetTypeSaveString(type);

            var klass = new MetaClass();
            klass.MetaType = type;
            var csIdx = cname.IndexOf('|');
            var fileName = cname;
            if(csIdx >= 0)
            {
                fileName = cname.Substring(csIdx + 1);
            }
            klass.ClassName = RName.GetRName(MetaDirectory.Name + "/" + fileName);

            MetaData curVer = new MetaData();
            curVer.BuildMetaData(type);

            Hash64 hash = klass.GetFolderHash();
            string dir = klass.ClassName.GetDirectory() + hash.ToString();
            if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(dir))
            {
                bool hasRedirection = false;
                klass.LoadXnd(out hasRedirection);
#if PWindow
                //if (hasRedirection)
                //    klass.Save2Xnd(false);
#endif
                MetaData data = klass.FindMetaData(curVer.MetaHash);
                if(data == null)
                {
                    klass.RegMetaData(curVer.MetaHash, curVer);
                    var xnd = IO.XndHolder.NewXNDHolder();
                    curVer.Save2Xnd(xnd.Node);
                    IO.XndHolder.SaveXND(dir + "/" + curVer.MetaHash + ".MetaData", xnd);
                }
            }
            else
            {
                CEngine.Instance.FileManager.CreateDirectory(dir);

                var sw = new System.IO.StreamWriter(dir + "/typename.txt", false);
                sw.WriteLine(cname);
                sw.Close();

                klass.RegMetaData(curVer.MetaHash, curVer);
                var xnd = IO.XndHolder.NewXNDHolder();
                curVer.Save2Xnd(xnd.Node);
                IO.XndHolder.SaveXND(dir + "/" + curVer.MetaHash + ".MetaData", xnd);
            }

            klass.CurrentVersion = curVer;
            Klasses.Add(cname, klass);

            return klass;
        }
        public MetaClass FindMetaClass(string typeSaveStr)
        {
            MetaClass result;
            if (Klasses.TryGetValue(typeSaveStr, out result) == false)
                return null;
            return result;
        }
        // 查找，没有返回null
        public MetaClass FindMetaClass(System.Type type)
        {
            MetaClass result;
            if (Klasses.TryGetValue(Rtti.RttiHelper.GetTypeSaveString(type), out result) == false)
                return null;
            return result;
        }
        // 查找，没有则创建
        public MetaClass GetMetaClass(System.Type type)
        {
            var className = Rtti.RttiHelper.GetTypeSaveString(type);
            MetaClass result;
            if (Klasses.TryGetValue(className, out result))
            {
                return result;
            }

            result = CreateMetaClass(type);
            return result;
        }

        //public MetaData FindMetaData(UInt32 hash)
        //{
        //    MetaData meta;
        //    if (this.MetaDatas.TryGetValue(hash, out meta))
        //        return meta;
        //    return null;
        //}
    }
}
