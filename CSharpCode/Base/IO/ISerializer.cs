using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class UCustomSerializerAttribute : Attribute
    {
        public virtual void Save(IWriter ar, object host, string propName)
        {

        }
        public virtual object Load(IReader ar, object host, string propName)
        {
            return null;
        }
    }
    //凡是派生了ISerizlizer接口的类，都会产生MetaClass信息
    public interface ISerializer
    {
        void OnPreRead(object tagObject, object hostObject, bool fromXml);
        //第一个参数通常传入一个Root一类的对象，用于查找对象关系
        void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml);
    }
    public partial class BaseSerializer : ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        public virtual void OnWriteMember(IWriter ar, ISerializer obj, Rtti.UMetaVersion metaVersion)
        {
            SerializerHelper.WriteMember(ar, obj, metaVersion);
        }
        public virtual void OnReadMember(IReader ar, ISerializer obj, Rtti.UMetaVersion metaVersion)
        {
            SerializerHelper.ReadMember(ar, obj, metaVersion);
        }
    }

    public class IOException : UException
    {
        public string ErrorInfo;
        public IOException(string info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
            : base(info, memberName, sourceFilePath, sourceLineNumber)
        {
            ErrorInfo = info;
        }
    }
    public class SerializerHelper
    {
        public delegate void Delegate_ReadMetaVersion(EngineNS.IO.IReader ar, EngineNS.IO.ISerializer hostObject);
        public static UInt64 WriteSkippable(IWriter ar)
        {
            var offset = ar.GetPosition();
            ar.Write((UInt32)0);
            return offset;
        }
        public static void SureSkippable(IWriter ar, UInt64 offset)
        {
            var cur = ar.GetPosition();
            ar.Seek(offset);
            ar.Write((UInt32)(cur - offset));
            ar.Seek(cur);
        }
        public static UInt64 GetSkipOffset(IReader ar)
        {
            var offset = ar.GetPosition();
            UInt32 size;
            ar.Read(out size);
            return offset + size;
        }

        public static bool Read(IReader ar, out ISerializer obj, object hostObject)
        {
            bool isNull;
            ar.Read(out isNull);
            if (isNull)
            {
                obj = null;
                return true;
            }
            obj = null;
            Hash64 typeHash;
            ar.Read(out typeHash);
            uint versionHash;
            ar.Read(out versionHash);

            var savePos = ar.GetPosition();
            uint ObjDataSize = 0;
            ar.Read(out ObjDataSize);
            savePos += ObjDataSize;

            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeHash);
            if (meta == null)
            {
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.UMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                throw new Exception($"MetaVersion lost:{versionHash}");
            }
            obj = Rtti.UTypeDescManager.CreateInstance(meta.ClassType) as ISerializer;
            obj.OnPreRead(ar.Tag, hostObject, false);
            return Read(ar, obj, metaVersion);
        }
        public static void Write(IWriter ar, ISerializer obj)
        {
            if (obj == null)
            {
                Write(ar, null, null);
                return;
            }
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(obj.GetType());
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
            Write(ar, obj, meta.CurrentVersion);
        }
        public static bool Read(IReader ar, ISerializer obj, Rtti.UMetaVersion metaVersion = null)
        {
            var baseSerializer = obj as BaseSerializer;
            if (baseSerializer != null)
            {
                baseSerializer.OnReadMember(ar, obj, metaVersion);
                return true;
            }
            else
            {
                ReadMember(ar, obj, metaVersion);
                return true;
            }
        }
        public static void ReadMember(IReader ar, ISerializer obj, Rtti.UMetaVersion metaVersion = null)
        {
#if UseSerializerCodeGen
            var srName = metaVersion.HostClass.ClassType.SystemType.FullName.Replace("+", "_CIC_") + "_Serializer";
            //Type.GetType(utilityReader)
            var utilityReader = Rtti.UTypeDesc.TypeOfFullName(srName);
            if(utilityReader!=null)
            {
                var call = utilityReader.SystemType.GetField($"mfn_Read_{metaVersion.MetaHash}", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (call != null)
                {
                    //call.Invoke(null, new object[] { ar, obj });
                    var dlgt = call.GetValue(null) as EngineNS.IO.SerializerHelper.Delegate_ReadMetaVersion;
                    dlgt(ar, obj);
                    return;
                }
            }
#endif
            foreach (var i in metaVersion.Propertys)
            {
                if (i.CustumSerializer != null)
                {
                    var t = i.CustumSerializer.Load(ar, obj, i.PropertyName);
                    if (i.PropInfo.CanWrite)
                    {
                        i.PropInfo.SetValue(obj, t);
                        obj.OnPropertyRead(ar.Tag, i.PropInfo, false);
                    }
                    continue;
                }
                var value = ReadObject(ar, i.FieldType.SystemType, obj);
                
                if (value != null && i.PropInfo != null)
                {
                    if (Rtti.UTypeDesc.CanCast(value.GetType(), i.PropInfo.PropertyType) == false)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Serializer", $"ProperySet {i.PropertyName}: {value.GetType().FullName}!={i.PropInfo.PropertyType.FullName}");
                        continue;
                    }
                    try
                    {
                        if (i.PropInfo.CanWrite && value != null)
                        {
                            i.PropInfo.SetValue(obj, value);
                            obj.OnPropertyRead(ar.Tag, i.PropInfo, false);
                        }
                        else if (i.PropInfo.CanWrite == false && value != null)
                        {
                            var target = i.PropInfo.GetValue(obj, null);
                            if (target != null)
                            {
                                metaVersion.HostClass.CopyObjectMetaField(target, value);
                                obj.OnPropertyRead(ar.Tag, i.PropInfo, false);
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
        public static bool Write(IWriter ar, ISerializer obj, Rtti.UMetaVersion metaVersion)
        {
            bool isNull = false;
            if (obj == null)
            {
                isNull = true;
                ar.Write(isNull);
                return true;
            }
            ar.Write(isNull);
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(obj.GetType());
            if (metaVersion == null)
            {
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
                if(meta!=null)
                {
                    metaVersion = meta.CurrentVersion;
                }
                else
                {
                    isNull = true;
                    ar.Write(isNull);
                    return false;
                }
            }

            var typeHash = Hash64.FromString(typeStr);
            ar.Write(typeHash);

            uint versionHash = metaVersion.MetaHash;
            ar.Write(versionHash);

            var savePos = ar.GetPosition();
            uint ObjDataSize = 0;
            ar.Write(ObjDataSize);

            var baseSerializer = obj as BaseSerializer;
            if (baseSerializer != null)
            {
                baseSerializer.OnWriteMember(ar, obj, metaVersion);
            }
            else
            {
                WriteMember(ar, obj, metaVersion);
            }

            var nowPos = ar.GetPosition();
            ObjDataSize = (uint)(nowPos - savePos);
            ar.Seek(savePos);
            ar.Write(ObjDataSize);
            ar.Seek(nowPos);
            return true;
        }
        public static void WriteMember(IWriter ar, ISerializer obj, Rtti.UMetaVersion metaVersion)
        {
            foreach (var i in metaVersion.Propertys)
            {
                if (i.PropInfo != null && i.PropInfo.CanRead)
                {
                    if (i.CustumSerializer != null)
                    {
                        i.CustumSerializer.Save(ar, obj, i.PropertyName);
                        continue;
                    }

                    var value = i.PropInfo.GetValue(obj, null);
                    if (value != null)
                        WriteObject(ar, value.GetType(), value);
                    else
                        WriteObject(ar, i.PropInfo.PropertyType, value);
                }
            }
        }
        public static void WriteObject(IWriter ar, Type t, object obj)
        {
            bool isNull = false;
            if (obj == null)
            {
                isNull = true;
                ar.Write(isNull);
                return;
            }

            if (t.IsEnum)
            {
                ar.Write(isNull);
                var v = System.Convert.ToString(obj);
                ar.Write(v);
            }
            else if (t.IsValueType)
            {
                ar.Write(isNull);
                unsafe
                {
                    var size = System.Runtime.InteropServices.Marshal.SizeOf(t);
                    var pBuffer = stackalloc byte[size];
                    System.Runtime.InteropServices.Marshal.StructureToPtr(obj, (IntPtr)pBuffer, false);
                    ar.WritePtr(pBuffer, size);
                }
            }
            else if (t == typeof(string))
            {
                ar.Write(isNull);
                var v = (string)obj;
                ar.Write(v);
            }
            else if (t == typeof(RName))
            {
                ar.Write(isNull);
                var v = (RName)obj;
                if (v == null)
                {
                    ar.Write(true);
                }
                else
                {
                    ar.Write(false);
                    ar.Write(v.AssetId);
                    ar.Write(v.RNameType);
                    ar.Write(v.Name);
                }
            }
            else if(t == typeof(Rtti.UTypeDesc))
            {
                ar.Write(isNull);
                var v = (Rtti.UTypeDesc)obj;
                if(v == null)
                {
                    ar.Write(true);
                }
                else
                {
                    ar.Write(false);
                    var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(v);
                    ar.Write(typeStr);
                }
            }
            else if (t.GetInterface(nameof(ISerializer)) != null)
            {
                ar.Write(isNull);
                Write(ar, obj as ISerializer, null);
            }            
            else if(obj is System.Collections.IList)
            {
                ar.Write(isNull);
                var lst = obj as System.Collections.IList;
                var elemType = obj.GetType().GetGenericArguments()[0];
                ar.Write(elemType.IsValueType);
                ar.Write(lst.Count);
                if (elemType.IsValueType)
                {
                    var elemTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemType);
                    ar.Write(elemTypeStr);
                    var offset = WriteSkippable(ar);
                    for (int j = 0; j < lst.Count; j++)
                    {
                        var e = lst[j];
                        WriteObject(ar, e.GetType(), e);
                    }
                    SureSkippable(ar, offset);
                }
                else
                {
                    for (int j = 0; j < lst.Count; j++)
                    {
                        var e = lst[j];
                        var elemTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(e.GetType());
                        ar.Write(elemTypeStr);
                        var offset = WriteSkippable(ar);
                        WriteObject(ar, e.GetType(), e);
                        SureSkippable(ar, offset);
                    }
                }
            }
            else if (obj is System.Collections.IDictionary)
            {
                ar.Write(isNull);
                var lst = obj as System.Collections.IDictionary;
                var elemKeyType = obj.GetType().GetGenericArguments()[0];
                var elemValueType = obj.GetType().GetGenericArguments()[1];
                bool isKeyValueType = elemKeyType.IsValueType || elemKeyType == typeof(string);
                bool isValueValueType = elemValueType.IsValueType || elemValueType == typeof(string);
                ar.Write(isKeyValueType);
                ar.Write(isValueValueType);
                ar.Write(lst.Count);
                if (isKeyValueType && isValueValueType)
                {
                    var elemKeyTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemKeyType);
                    ar.Write(elemKeyTypeStr);
                    var elemValueTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemValueType);
                    ar.Write(elemValueTypeStr);
                    var offset = WriteSkippable(ar);
                    System.Collections.IDictionaryEnumerator j = lst.GetEnumerator();
                    while (j.MoveNext())
                    {
                        WriteObject(ar, elemKeyType, j.Key);
                        WriteObject(ar, elemValueType, j.Value);
                    }
                    SureSkippable(ar, offset);
                }
                else if (isKeyValueType && !isValueValueType)
                {
                    System.Collections.IDictionaryEnumerator j = lst.GetEnumerator();
                    var elemKeyTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemKeyType);
                    ar.Write(elemKeyTypeStr);
                    var offset = WriteSkippable(ar);
                    while (j.MoveNext())
                    {
                        var elemValueTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemValueType);
                        ar.Write(elemValueTypeStr);
                        var offset1 = WriteSkippable(ar);
                        WriteObject(ar, elemKeyType, j.Key);
                        WriteObject(ar, elemValueType, j.Value);
                        SureSkippable(ar, offset1);
                    }
                    SureSkippable(ar, offset);
                }
                else if (!isKeyValueType && isValueValueType)
                {
                    System.Collections.IDictionaryEnumerator j = lst.GetEnumerator();
                    var elemValueTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemValueType);
                    ar.Write(elemValueTypeStr);
                    var offset = WriteSkippable(ar);
                    while (j.MoveNext())
                    {
                        var elemKeyTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemKeyType);
                        ar.Write(elemKeyTypeStr);
                        var offset1 = WriteSkippable(ar);
                        WriteObject(ar, elemKeyType, j.Key);
                        WriteObject(ar, elemValueType, j.Value);
                        SureSkippable(ar, offset1);
                    }
                    SureSkippable(ar, offset);
                }
                else if (!isKeyValueType && !isValueValueType)
                {
                    System.Collections.IDictionaryEnumerator j = lst.GetEnumerator();
                    while (j.MoveNext())
                    {
                        var elemValueTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemValueType);
                        ar.Write(elemValueTypeStr);
                        var elemKeyTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(elemKeyType);
                        ar.Write(elemKeyTypeStr);
                        var offset = WriteSkippable(ar);
                        WriteObject(ar, elemKeyType, j.Key);
                        WriteObject(ar, elemValueType, j.Value);
                        SureSkippable(ar, offset);
                    }
                }
            }
            else
            {
                // 无法存盘默认null
                ar.Write(true);

                var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(t);
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
            }
        }
        //public static bool DoIsNull = true;
        public static object ReadObject(IReader ar, Type t, object hostObject)
        {
            //if (DoIsNull)
            {
                bool isNull = false;
                ar.Read(out isNull);
                if (isNull)
                    return null;
            }
            if (t.IsEnum)
            {
                string v;
                ar.Read(out v);
                return Support.TConvert.ToEnumValue(t, v);
            }
            else if (t.IsValueType)
            {
                unsafe
                {
                    var size = System.Runtime.InteropServices.Marshal.SizeOf(t);
                    var attrs = t.GetCustomAttributes(typeof(Rtti.UStructAttrubte), false);
                    if(attrs.Length>0)
                    {
                        size = (attrs[0] as Rtti.UStructAttrubte).ReadSize;
                    }                    
                    var pBuffer = stackalloc byte[size];
                    ar.ReadPtr(pBuffer, size);
                    var v = System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)pBuffer, t);
                    return v;
                }
            }
            else if (t == typeof(string))
            {
                string v;
                ar.Read(out v);
                return v;
            }
            else if (t == typeof(RName))
            {
                bool isNull;
                ar.Read(out isNull);
                if (!isNull)
                {
                    Guid assetId;
                    ar.Read(out assetId);
                    RName.ERNameType rnType;
                    ar.Read(out rnType);
                    string name;
                    ar.Read(out name);
                    var v = RName.GetRName(name, rnType);
                    v.AssetId = assetId;
                    return v;
                }
                else
                {
                    return null;
                }
            }
            else if(t == typeof(Rtti.UTypeDesc))
            {
                bool isNull;
                ar.Read(out isNull);
                if(!isNull)
                {
                    string typeStr;
                    ar.Read(out typeStr);
                    return Rtti.UTypeDesc.TypeOf(typeStr);
                }
                return null;
            }
            else if (t.GetInterface(nameof(ISerializer)) != null)
            {
                bool isNull = false;
                ar.Read(out isNull);
                if (isNull)
                {
                    return null;
                }

                Hash64 typeHash;
                ar.Read(out typeHash);
                uint versionHash;
                ar.Read(out versionHash);

                var savePos = ar.GetPosition();
                uint ObjDataSize = 0;
                ar.Read(out ObjDataSize);
                savePos += ObjDataSize;

                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeHash);
                Rtti.UMetaVersion metaVersion;
                if (meta != null && (metaVersion = meta.GetMetaVersion(versionHash)) != null)
                {
                    var obj = Rtti.UTypeDescManager.CreateInstance(meta.ClassType) as ISerializer;
                    obj.OnPreRead(ar.Tag, hostObject, false);
                    Read(ar, obj, metaVersion);
                    return obj;
                }
                else
                {
                    ar.Seek(savePos);
                    return null;
                }
            }            
            else if (t.GetInterface(nameof(System.Collections.IList)) != null)
            {
                var lst = Rtti.UTypeDescManager.CreateInstance(t) as System.Collections.IList;
                bool isValueType;
                ar.Read(out isValueType);
                int count = 0;
                ar.Read(out count);
                if (isValueType)
                {
                    string elemTypeStr;
                    ar.Read(out elemTypeStr);
                    var skipPoint = GetSkipOffset(ar);
                    try
                    {
                        var elemType = Rtti.UTypeDesc.TypeOf(elemTypeStr).SystemType;
                        if (elemType == null)
                        {
                            throw new IOException($"Read List: {elemTypeStr} is missing");
                        }
                        for (int j = 0; j < count; j++)
                        {
                            var e = ReadObject(ar, elemType, hostObject);
                            lst.Add(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        ar.Seek(skipPoint);
                    }
                }
                else
                {
                    for (int j = 0; j < count; j++)
                    {
                        string elemTypeStr;
                        ar.Read(out elemTypeStr);
                        var skipPoint = GetSkipOffset(ar);
                        try
                        {
                            var elemType = Rtti.UTypeDesc.TypeOf(elemTypeStr).SystemType;
                            if (elemType == null)
                            {
                                throw new IOException($"Read List: {elemTypeStr} is missing");
                            }
                            var e = ReadObject(ar, elemType, hostObject);
                            lst.Add(e);
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ar.Seek(skipPoint);
                        }
                    }
                }
                return lst;
            }
            else if (t.GetInterface(nameof(System.Collections.IDictionary)) != null)
            {
                var lst = Rtti.UTypeDescManager.CreateInstance(t) as System.Collections.IDictionary;
                bool isKeyValueType;
                bool isValueValueType;
                ar.Read(out isKeyValueType);
                ar.Read(out isValueValueType);
                int count;
                ar.Read(out count);
                if (isKeyValueType && isValueValueType)
                {
                    string elemKeyTypeStr;
                    ar.Read(out elemKeyTypeStr);
                    string elemValueTypeStr;
                    ar.Read(out elemValueTypeStr);

                    var skipPoint = GetSkipOffset(ar);
                    try
                    {
                        var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;
                        if (elemKeyType == null)
                            throw new IOException($"Read Dictionary: KeyType {elemKeyType} is missing");
                        var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;
                        if (elemValueType == null)
                            throw new IOException($"Read Dictionary: ValueType {elemValueType} is missing");
                        for (int i = 0; i < count; i++)
                        {
                            var key = ReadObject(ar, elemKeyType, hostObject);
                            var value = ReadObject(ar, elemValueType, hostObject);
                            lst[key] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        ar.Seek(skipPoint);
                    }
                }
                else if (isKeyValueType && !isValueValueType)
                {
                    string elemKeyTypeStr;
                    ar.Read(out elemKeyTypeStr);
                    var skipPoint = GetSkipOffset(ar);
                    try
                    {
                        var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;
                        if (elemKeyType == null)
                            throw new IOException($"Read Dictionary: KeyType {elemKeyType} is missing");
                        for (int i = 0; i < count; i++)
                        {
                            string elemValueTypeStr;
                            ar.Read(out elemValueTypeStr);
                            var skipPoint1 = GetSkipOffset(ar);
                            var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;
                            if (elemValueType == null)
                                throw new IOException($"Read Dictionary: ValueType {elemValueType} is missing");
                            try
                            {
                                var key = ReadObject(ar, elemKeyType, hostObject);
                                var value = ReadObject(ar, elemValueType, hostObject);
                                lst[key] = value;
                            }
                            catch (Exception ex)
                            {
                                Profiler.Log.WriteException(ex);
                                ar.Seek(skipPoint1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        ar.Seek(skipPoint);
                    }
                }
                else if (!isKeyValueType && isValueValueType)
                {
                    string elemValueTypeStr;
                    ar.Read(out elemValueTypeStr);
                    var skipPoint = GetSkipOffset(ar);
                    try
                    {
                        var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;
                        if (elemValueType == null)
                            throw new IOException($"Read Dictionary: ValueType {elemValueType} is missing");

                        for (int i = 0; i < count; i++)
                        {
                            string elemKeyTypeStr;
                            ar.Read(out elemKeyTypeStr);
                            var skipPoint1 = GetSkipOffset(ar);
                            try
                            {
                                var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;
                                if (elemKeyType == null)
                                    throw new IOException($"Read Dictionary: KeyType {elemKeyType} is missing");
                                var key = ReadObject(ar, elemKeyType, hostObject);
                                var value = ReadObject(ar, elemValueType, hostObject);
                                lst[key] = value;
                            }
                            catch (Exception ex)
                            {
                                Profiler.Log.WriteException(ex);
                                ar.Seek(skipPoint1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        ar.Seek(skipPoint);
                    }
                }
                else if (!isKeyValueType && !isValueValueType)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string elemKeyTypeStr;
                        ar.Read(out elemKeyTypeStr);
                        var skipPoint = GetSkipOffset(ar);
                        try
                        {
                            var elemKeyType = Rtti.UTypeDesc.TypeOf(elemKeyTypeStr).SystemType;
                            if (elemKeyType == null)
                                throw new IOException($"Read Dictionary: KeyType {elemKeyType} is missing");
                            string elemValueTypeStr;
                            ar.Read(out elemValueTypeStr);
                            var elemValueType = Rtti.UTypeDesc.TypeOf(elemValueTypeStr).SystemType;
                            if (elemValueType == null)
                                throw new IOException($"Read Dictionary: ValueType {elemValueType} is missing");
                            var key = ReadObject(ar, elemKeyType, hostObject);
                            var value = ReadObject(ar, elemValueType, hostObject);
                            lst[key] = value;
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                            ar.Seek(skipPoint);
                        }
                    }
                }
                return lst;
            }
            else
            {
                var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(t);
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
            }
            return null;
        }

        public static bool WriteObjectMetaFields(System.Xml.XmlDocument xml, System.Xml.XmlElement node, object obj)
        {
            if (obj == null)
                return false;
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(obj.GetType());
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);

            if (meta.MetaAttribute == null && obj.GetType().GetInterface(nameof(IO.ISerializer)) == null)
            {
                if (obj.GetType() == typeof(RName))
                {
                    var rn = obj as RName;
                    var attr = xml.CreateAttribute($"Value");
                    attr.Value = $"{rn.RNameType},{rn.Name},{rn.AssetId}";
                    node.Attributes.Append(attr);
                }
                else if (obj.GetType() == typeof(Rtti.UTypeDesc))
                {
                    var rn = obj as Rtti.UTypeDesc;
                    var attr = xml.CreateAttribute($"Value");
                    attr.Value = rn.TypeString;
                    node.Attributes.Append(attr);
                }
                else if (obj.GetType().IsValueType || obj.GetType() == typeof(string))
                {
                    var attr = xml.CreateAttribute($"Value");
                    attr.Value = obj.ToString();
                    node.Attributes.Append(attr);
                }
                return false;
            }
            else
            {
                var nodeType = xml.CreateAttribute("Type");
                nodeType.Value = typeStr;
                node.Attributes.Append(nodeType);
            }

            var metaVer = meta.CurrentVersion;
            foreach (var i in metaVer.Propertys)
            {
                var fv = i.PropInfo.GetValue(obj, null);
                if (fv == null)
                    continue;

                var prop = xml.CreateElement($"{i.PropertyName}", xml.NamespaceURI);
                var attr = xml.CreateAttribute($"Type");
                //attr.Value = i.FieldTypeStr;
                attr.Value = Rtti.UTypeDesc.TypeOf(fv.GetType()).TypeString;
                prop.Attributes.Append(attr);

                if (i.PropInfo.PropertyType.IsValueType || i.PropInfo.PropertyType == typeof(string))
                {
                    attr = xml.CreateAttribute($"Value");
                    attr.Value = fv.ToString();
                    prop.Attributes.Append(attr);
                }
                else if (fv is System.Collections.IList)
                {
                    var lst = fv as System.Collections.IList;
                    attr = xml.CreateAttribute($"Count");
                    attr.Value = lst.Count.ToString();
                    prop.Attributes.Append(attr);
                    for (int j = 0; j < lst.Count; j++)
                    {
                        var e = lst[j];
                        var lstElement = xml.CreateElement($"e_{j}", xml.NamespaceURI);
                        if (e != null)
                        {
                            WriteObjectMetaFields(xml, lstElement, e);
                        }
                        else
                        {
                            attr = xml.CreateAttribute($"IsNull");
                            attr.Value = "True";
                            lstElement.Attributes.Append(attr);
                        }
                        prop.AppendChild(lstElement);
                    }
                }
                else if (fv is System.Collections.IDictionary)
                {
                    var dict = fv as System.Collections.IDictionary;
                    attr = xml.CreateAttribute($"Count");
                    attr.Value = dict.Count.ToString();
                    prop.Attributes.Append(attr);
                    System.Collections.IDictionaryEnumerator j = dict.GetEnumerator();
                    int index = 0;
                    while (j.MoveNext())
                    {
                        var dictElement = xml.CreateElement($"e_{index}", xml.NamespaceURI);
                        prop.AppendChild(dictElement);
                        var keyNode = xml.CreateElement($"Key", xml.NamespaceURI);
                        var valueNode = xml.CreateElement($"Value", xml.NamespaceURI);
                        dictElement.AppendChild(keyNode);
                        dictElement.AppendChild(valueNode);
                        WriteObjectMetaFields(xml, keyNode, j.Key);
                        WriteObjectMetaFields(xml, valueNode, j.Value);
                        index++;
                    }
                }
                else
                {
                    WriteObjectMetaFields(xml, prop, fv);
                }
                node.AppendChild(prop);
            }
            return true;
        }
        /// <summary>
        /// 根据xml内容读取对象
        /// </summary>
        /// <param name="paramObject">参数对象</param>
        /// <param name="node">xml节点</param>
        /// <param name="obj">目标对象</param>
        /// <param name="hostObject">当前读取对象的父读取对象</param>
        public static void ReadObjectMetaFields(object paramObject, System.Xml.XmlElement node, ref object obj, object hostObject)
        {
            var thisTypeStr = node.GetAttribute("Type");
            if (string.IsNullOrEmpty(thisTypeStr))
            {
                obj = Support.TConvert.ToObject(obj.GetType(), node.GetAttribute("Value"));
                return;
            }
            var typeDesc = Rtti.UTypeDesc.TypeOf(thisTypeStr);
            if (typeDesc == null)
            {
                if (hostObject != null)
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"{hostObject.GetType()}: MetaField({thisTypeStr}) Type Missing");
                else
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "MetaData", $"null: MetaField({thisTypeStr}) Type Missing");
                if (obj != null)
                    obj = Support.TConvert.ToObject(obj.GetType(), node.GetAttribute("Value"));
                return;
            }
            var thisType = typeDesc.SystemType;
            if (obj == null && thisType == null)
                return;
            if (obj == null)
                obj = Rtti.UTypeDescManager.CreateInstance(thisType);            
            (obj as ISerializer)?.OnPreRead(paramObject, hostObject, true);
            foreach (System.Xml.XmlElement i in node.ChildNodes)
            {
                var typeAttr = i.GetAttribute("Type");
                if (string.IsNullOrEmpty(typeAttr))
                    continue;

                var prop = obj.GetType().GetProperty(i.Name);
                if (prop == null)
                    continue;

                object readOnlyObject = null;
                if (prop.PropertyType == typeof(object))
                {
                    if (typeAttr == Rtti.UTypeDescGetter<Rtti.UTypeDesc>.TypeDesc.TypeString)
                    {
                        var valueAttr = i.GetAttribute("Value");
                        if (string.IsNullOrEmpty(valueAttr))
                        {
                            continue;
                        }
                        var v = Rtti.UTypeDesc.TypeOf(valueAttr);
                        if (prop.CanWrite)
                            prop.SetValue(obj, v);
                        continue;
                    }
                }

                if (prop.PropertyType == typeof(RName))
                {
                    var valueAttr = i.GetAttribute("Value");
                    if (string.IsNullOrEmpty(valueAttr))
                    {
                        continue;
                    }
                    var segs = valueAttr.Split(',');
                    Guid assetId;
                    Guid.TryParse(segs[2], out assetId);
                    var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[0]);
                    var v = RName.GetRName(segs[1], rnType);                    
                    v.AssetId = assetId;
                    if (prop.CanWrite)
                        prop.SetValue(obj, v);
                    continue;
                }
                else if (prop.PropertyType == typeof(Rtti.UTypeDesc))
                {
                    var valueAttr = i.GetAttribute("Value");
                    if (string.IsNullOrEmpty(valueAttr))
                    {
                        continue;
                    }
                    var v = Rtti.UTypeDesc.TypeOf(valueAttr);
                    if (prop.CanWrite)
                        prop.SetValue(obj, v);
                    continue;
                }
                else if (prop.PropertyType.IsValueType == false &&
                    prop.PropertyType != typeof(string) &&
                    prop.CanWrite == false)
                {
                    readOnlyObject = prop.GetValue(obj);
                    if (readOnlyObject == null)
                        continue;
                }

                //var type = Rtti.TypeManager.Instance.GetTypeFromString(typeAttr);
                var type = prop.PropertyType;
                if (type.IsValueType || type == typeof(string))
                {
                    var valueAttr = i.GetAttribute("Value");
                    if (!string.IsNullOrEmpty(valueAttr))
                    {
                        var v = Support.TConvert.ToObject(type, valueAttr);
                        if (v != null)
                        {
                            if (prop.CanWrite)
                                prop.SetValue(obj, v);
                        }
                    }
                }
                else if (type.GetInterface(nameof(System.Collections.IList)) != null)
                {
                    var countAttr = i.GetAttribute("Count");
                    if (string.IsNullOrEmpty(countAttr))
                        continue;
                    var lst = readOnlyObject as System.Collections.IList;
                    if (lst == null)
                        lst = Rtti.UTypeDescManager.CreateInstance(type) as System.Collections.IList;
                    foreach (System.Xml.XmlElement j in i.ChildNodes)
                    {
                        var keyType = type.GetGenericArguments()[0];
                        typeAttr = j.GetAttribute("Type");
                        if (!string.IsNullOrEmpty(typeAttr))
                        {
                            var elemType = Rtti.UTypeDesc.TypeOf(typeAttr)?.SystemType;
                            if (elemType != null)
                                keyType = elemType;
                        }

                        object e = null;
                        if (j.GetAttributeNode("IsNull") == null)
                        {
                            if (type.GetGenericArguments()[0] == typeof(RName))
                            {
                                var valueAttr = j.GetAttribute("Value");
                                if (string.IsNullOrEmpty(valueAttr))
                                {
                                    continue;
                                }
                                var rn = obj as RName;
                                var segs = valueAttr.Split(',');
                                Guid assetId;
                                Guid.TryParse(segs[2], out assetId);
                                var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[0]);
                                var v = RName.GetRName(segs[1], rnType);
                                v.AssetId = assetId;

                                e = v;
                            }
                            else
                            {
                                e = Rtti.UTypeDescManager.CreateInstance(keyType);
                                ReadObjectMetaFields(paramObject, j, ref e, obj);
                            }
                        }
                        lst.Add(e);
                    }
                    if (prop.CanWrite)
                        prop.SetValue(obj, lst);
                }
                else if (type.GetInterface(nameof(System.Collections.IDictionary)) != null)
                {
                    var countAttr = i.GetAttribute("Count");
                    if (string.IsNullOrEmpty(countAttr))
                        continue;

                    var dict = readOnlyObject as System.Collections.IDictionary;
                    if (dict == null)
                        dict = Rtti.UTypeDescManager.CreateInstance(type) as System.Collections.IDictionary;
                    foreach (System.Xml.XmlElement j in i.ChildNodes)
                    {
                        if (j.ChildNodes.Count != 2)
                            continue;
                        var key = j.ChildNodes[0] as System.Xml.XmlElement;
                        var value = j.ChildNodes[1] as System.Xml.XmlElement;

                        var keyType = type.GetGenericArguments()[0];
                        typeAttr = key.GetAttribute("Type");
                        if (!string.IsNullOrEmpty(typeAttr))
                        {
                            var kt = Rtti.UTypeDesc.TypeOf(typeAttr).SystemType;
                            if (kt != null)
                                keyType = kt;
                        }
                        object keyValue;
                        if (type.GetGenericArguments()[0] == typeof(RName))
                        {
                            var valueAttr = j.GetAttribute("Value");
                            if (string.IsNullOrEmpty(valueAttr))
                            {
                                continue;
                            }
                            var rn = obj as RName;
                            var segs = valueAttr.Split(',');
                            Guid assetId;
                            Guid.TryParse(segs[2], out assetId);
                            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[0]);
                            var v = RName.GetRName(segs[1], rnType);
                            v.AssetId = assetId;

                            keyValue = v;
                        }
                        else
                        {
                            keyValue = Rtti.UTypeDescManager.CreateInstance(keyType);
                            ReadObjectMetaFields(paramObject, key, ref keyValue, obj);
                        }

                        var valueType = type.GetGenericArguments()[1];
                        typeAttr = value.GetAttribute("Type");
                        if (!string.IsNullOrEmpty(typeAttr))
                        {
                            var kt = Rtti.UTypeDesc.TypeOf(typeAttr).SystemType;
                            if (kt != null)
                                valueType = kt;
                        }
                        object valueValue;
                        if (type.GetGenericArguments()[0] == typeof(RName))
                        {
                            var valueAttr = j.GetAttribute("Value");
                            if (string.IsNullOrEmpty(valueAttr))
                            {
                                continue;
                            }
                            var rn = obj as RName;
                            var segs = valueAttr.Split(',');
                            Guid assetId;
                            Guid.TryParse(segs[2], out assetId);
                            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[0]);
                            var v = RName.GetRName(segs[1], rnType);
                            v.AssetId = assetId;

                            valueValue = v;
                        }
                        else
                        {
                            valueValue = Rtti.UTypeDescManager.CreateInstance(valueType);
                            ReadObjectMetaFields(paramObject, value, ref valueValue, obj);
                        }

                        dict[keyValue] = valueValue;
                    }

                    if(prop.CanWrite)
                        prop.SetValue(obj, dict);
                }
                else
                {
                    var tempType = Rtti.UTypeDescManager.Instance.GetTypeFromString(typeAttr);
                    if (tempType != null)
                        type = tempType;
                    var subObject = readOnlyObject;
                    if (subObject == null)
                        subObject = Rtti.UTypeDescManager.CreateInstance(type);
                    ReadObjectMetaFields(paramObject, i, ref subObject, obj);
                    if (prop.CanWrite)
                        prop.SetValue(obj, subObject);
                }
                (obj as ISerializer)?.OnPropertyRead(paramObject, prop, true);
            }
        }

        public static string SaveAsJson(object obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        public static T LoadFromJson<T>(string txt)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(txt);
        }
    }
}

namespace EngineNS.UTest
{
    [Rtti.Meta]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial class UTest_MetaObject : EngineNS.IO.ISerializer
    {
        [Rtti.Meta]
        public class TestSubClass : EngineNS.IO.ISerializer
        {
            public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
            public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
            [Rtti.Meta]
            public int A { get; set; }
            [Rtti.Meta]
            public float B { get; set; }
            [Rtti.Meta]
            public string C { get; set; }
        }
        [Rtti.Meta]
        public int A { get; set; } = 2;
        [Rtti.Meta]
        public float B { get; set; } = 3.0f;
        [Rtti.Meta]
        public string C { get; set; } = "4";
        [Rtti.Meta]
        public TestSubClass D { get; } = new TestSubClass();
        [Rtti.Meta]
        public TestSubClass E { get; set; } = new TestSubClass()
        {
            A = 10,
        };
        [Rtti.Meta]
        public FTransform F { get; set; } = new FTransform();
        [Rtti.Meta]
        public List<int> G { get; set; } = new List<int>();
        [Rtti.Meta]
        public List<TestSubClass> H { get; set; } = new List<TestSubClass>();
        [Rtti.Meta]
        public Dictionary<int, string> I { get; set; } = new Dictionary<int, string>();
        [Rtti.Meta]
        public Dictionary<int, TestSubClass> J { get; set; } = new Dictionary<int, TestSubClass>();
        [Rtti.Meta]
        public void TestFunction1(float a)
        {

        }
        [Rtti.Meta]
        public static void TestStaticFunction1(float a)
        {

        }
        [Rtti.Meta]
        public static object TestOutArguments(
            [Rtti.MetaParameter(FilterType = typeof(Bricks.CodeBuilder.MacrossNode.VarNode), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType)
        {
            return null;
        }
        [Rtti.Meta]
        public static object TestOut2(
            [Rtti.MetaParameter(FilterType = typeof(UTest_MetaObject), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type rType)
        {
            return null;
        }

        [Rtti.Meta(Order = 100)]
        public bool ReadSignal
        {
            get => true;
        }
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }
        public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml)
        {
            if(prop.Name == nameof(ReadSignal))
            {
                return;
            }
        }
    }

    [UTest]
    public class UTest_Serializer
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        class RMemStructTest
        {
            public int mA;
            public int A;
            public float B;
            public IntPtr C_P;
            public int C_N;
            public IntPtr D;
            public IntPtr E;
            public float mB;
            public int Vtb;
        }
        public void UnitTestEntrance()
        {
            var rn = RName.GetRName("UTest/t1.xnd");
            IO.TtFileManager.SureDirectory(RName.GetRName("UTest").Address);
            {
                var xnd = new EngineNS.IO.TtXndHolder("TestRoot", 1, 0);
                var tobj = new UTest_MetaObject();

                using (var attr = xnd.NewAttribute("Att0", 1, 0))
                {
                    var attrProxy = new EngineNS.IO.TtXndAttributeWriter(attr);
                    var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.TtXndAttributeWriter>(attrProxy);
                    attr.BeginWrite(100);
                    int a = 1;
                    ar.Write(a);
                    a = 2;
                    ar.Write(a);                    
                    tobj.D.A = 30;
                    tobj.G.Add(1);
                    tobj.G.Add(2);
                    tobj.G.Add(15);
                    tobj.H.Add(new UTest_MetaObject.TestSubClass() { A = 5 });
                    tobj.H.Add(new UTest_MetaObject.TestSubClass() { A = 9 });
                    tobj.H.Add(new UTest_MetaObject.TestSubClass() { A = 10 });
                    tobj.I.Add(1, "a");
                    tobj.I.Add(2, "b");
                    tobj.J.Add(1, new UTest_MetaObject.TestSubClass() { A = 5 });
                    tobj.J.Add(2, new UTest_MetaObject.TestSubClass() { A = 9 });
                    var trans = new FTransform();
                    trans.InitData();
                    trans.Position = new DVector3(1, 1, 1);
                    tobj.F = trans;
                    ar.Write(tobj);
                    attr.EndWrite();
                    xnd.RootNode.AddAttribute(attr);
                }
                xnd.SaveXnd(rn.Address);

                var xml = new System.Xml.XmlDocument();
                var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
                xml.AppendChild(xmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, tobj);
                var xmlText = IO.TtFileManager.GetXmlText(xml);
                IO.TtFileManager.WriteAllText(RName.GetRName("UTest/t1.xml").Address, xmlText);
            }
            {
                var xnd = IO.TtXndHolder.LoadXnd(rn.Address);
                for (uint i = 0; i < xnd.RootNode.NumOfAttribute; i++)
                {
                    var attr = xnd.RootNode.GetAttribute(i);
                    if (!attr.IsValidPointer)
                        continue;

                    if (attr.Name == "Att0")
                    {
                        var attrProxy = new EngineNS.IO.TtXndAttributeReader(attr);
                        var ar = new EngineNS.IO.AuxReader<EngineNS.IO.TtXndAttributeReader>(attrProxy, this);
                        int a;
                        IO.ISerializer tObj;
                        try
                        {
                            attr.BeginRead();
                            ar.Read(out a);
                            ar.Read(out a);
                            ar.Read(out tObj, this);
                        }
                        finally
                        {
                            attr.EndRead();
                        }

                        var tmo = tObj as UTest_MetaObject;
                        UnitTestManager.TAssert(tmo != null, "tmo!=null");
                        if (tmo != null)
                        {
                            UnitTestManager.TAssert(tmo.D.A == 30, "tmo!=null");
                        }
                    }
                }

                object xmlLoader = new UTest_MetaObject();
                var xml = IO.TtFileManager.LoadXml(RName.GetRName("UTest/t1.xml").Address);
                IO.SerializerHelper.ReadObjectMetaFields(xmlLoader, xml.LastChild as System.Xml.XmlElement, ref xmlLoader, null);
            }
        }
    }
}