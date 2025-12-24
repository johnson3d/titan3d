using EngineNS.Bricks.DataCopyer;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Bricks.DataCopyer
{
    public abstract class TtDataCopyer : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public delegate void FCopy(object tar, object src);
        public delegate void FWrite(IO.IWriter ar, object obj);
        public delegate void FReader(IO.IReader ar, object obj);
        public class TtClassCopyer
        {
            public FWrite Writer;
            public FCopy Copy;
            public Dictionary<UInt64, FReader> VersionReaders = new Dictionary<UInt64, FReader>();
            public void RegVersion(UInt64 v, FReader fn)
            {
                VersionReaders[v] = fn;
            }
            public FReader FindReaderVersion(UInt64 hash)
            {
                if (VersionReaders.TryGetValue(hash, out FReader fn))
                    return fn;
                return null;
            }
        }
        public Dictionary<string, TtClassCopyer> ClassCopyer = new Dictionary<string, TtClassCopyer>();
        public TtClassCopyer GetClassCopyer(string typeStr)
        {
            TtClassCopyer result;
            if (ClassCopyer.TryGetValue(typeStr, out result))
                return result;
            result = new TtClassCopyer();
            ClassCopyer.Add(typeStr, result);
            return result;
        }
        public FCopy FindCopyer(string typeStr)
        {
            if (ClassCopyer.TryGetValue(typeStr, out var result))
            {
                return result.Copy;
            }
            return null;
        }
        public FWrite FindWriter(string typeStr)
        {
            if (ClassCopyer.TryGetValue(typeStr, out var result))
            {
                return result.Writer;
            }
            return null;
        }
        public FReader FindReader(string typeStr, Hash64 version)
        {
            if (ClassCopyer.TryGetValue(typeStr, out var result))
            {
                return result.FindReaderVersion(version.AllData);
            }
            return null;
        }
        public static void WriteMember(IO.IWriter ar, object obj, Rtti.TtMetaVersion metaVersion)
        {
            foreach (var i in metaVersion.Propertys)
            {
                if (i.PropInfo.CanRead == false || (i.PropInfo.GetGetMethod() != null && i.PropInfo.GetGetMethod().IsStatic))
                {
                    continue;
                }
                if (i.PropInfo.GetCustomAttribute<Rtti.MetaAttribute>().IsNoSerializable)
                {
                    continue;
                }
                if (i.CustumSerializer != null)
                {
                    i.CustumSerializer.Save(ar, obj, i.PropertyName);
                    continue;
                }

                object value;
                try
                {
                    value = i.PropInfo.GetValue(obj);
                }
                catch
                {
                    var prop = obj.GetType().GetProperty(i.PropertyName);
                    if (prop != null)
                    {
                        value = prop.GetValue(obj);
                    }
                    else
                    {
                        value = Rtti.TtTypeDescManager.CreateInstance(i.PropInfo.PropertyType);
                    }
                }

                (obj as ISerializerNotifyEx)?.OnPropertyWrite(i.PropInfo.Name, false);
                WriteObject(ar, i.PropInfo.PropertyType, value);
            }
        }
        public static void ReadMember(IO.IReader ar, object obj, Rtti.TtMetaVersion metaVersion, bool hasSkip)
        {
            foreach (var i in metaVersion.Propertys)
            {
                if (i.PropInfo != null)
                {
                    if (i.PropInfo.CanRead == false || (i.PropInfo.GetGetMethod() != null && i.PropInfo.GetGetMethod().IsStatic))
                    {
                        continue;
                    }
                    if (i.PropInfo.GetCustomAttribute<Rtti.MetaAttribute>().IsNoSerializable)
                    {
                        continue;
                    }
                }
                if (i.CustumSerializer != null)
                {
                    var value = i.CustumSerializer.Load(ar, obj, i.PropertyName);
                    if (i.PropInfo.CanWrite)
                    {
                        i.PropInfo.SetValue(obj, value);
                        if (obj is IO.ISerializer sr)
                            sr.OnPropertyRead(ar.Tag, i.PropInfo.Name, false);
                    }
                }
                else
                {
                    if (i.PropInfo == null)
                    {
                        var type = Rtti.TtTypeDesc.TypeOf(i.FieldTypeStr);
                        object discardObj = null;
                        if (type != null)
                        {
                            discardObj = ReadObject(ar, type.SystemType, obj, null, hasSkip);
                        }
                        return;
                    }
                    if (IsPrimitiveType(i.PropInfo.PropertyType))
                    {
                        var value = ReadObject(ar, i.PropInfo.PropertyType, obj, null, hasSkip);
                        if (i.PropInfo.CanWrite)
                        {
                            i.PropInfo.SetValue(obj, value);
                            if (obj is IO.ISerializer sr)
                                sr.OnPropertyRead(ar.Tag, i.PropInfo.Name, false);
                        }
                    }
                    else
                    {
                        var value = i.PropInfo.GetValue(obj);
                        if (value == null)
                        {
                            value = ReadObject(ar, i.PropInfo.PropertyType, obj, null, hasSkip);
                            if (i.PropInfo.CanWrite)
                            {
                                i.PropInfo.SetValue(obj, value);
                                if (obj is IO.ISerializer sr)
                                    sr.OnPropertyRead(ar.Tag, i.PropInfo.Name, false);
                            }
                        }
                        else
                        {
                            ReadObject(ar, i.PropInfo.PropertyType, obj, value, hasSkip);
                            if (i.PropInfo.CanWrite)
                            {
                                i.PropInfo.SetValue(obj, value);
                                if (obj is IO.ISerializer sr)
                                    sr.OnPropertyRead(ar.Tag, i.PropInfo.Name, false);
                            }
                        }
                    }
                }
            }
        }
        public static unsafe object ReadObject(IReader ar, Type t, object hostObject, object obj, bool hasSkip)
        {
            if (t.IsEnum)
            {
                int v;
                ar.Read(out v);
                return Enum.ToObject(t, v);
            }
            else if (IsUnmanagedType(t))
            {
                var size = System.Runtime.InteropServices.Marshal.SizeOf(t);
                var pBuffer = stackalloc byte[size];
                ar.ReadPtr(pBuffer, size);
                return System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)pBuffer, t);
            }
            else if (t == typeof(string))
            {
                string v;
                ar.Read(out v);
                return v;
            }
            else if (t == typeof(RName))
            {
                RName v;
                ar.Read(out v);
                return v;
            }
            else if (t == typeof(Rtti.TtTypeDesc))
            {
                Rtti.TtTypeDesc v;
                ar.Read(out v);
                return v;
            }
            else if (t == typeof(byte[]))
            {
                byte[] v;
                ar.Read(out v);
                return v;
            }
            else if (t == typeof(Support.TtBitset))
            {
                Support.TtBitset v = null;
                ar.Read(ref v);
                return v;
            }
            else if (t == typeof(TtMemWriter))
            {
                TtMemWriter v;
                ar.Read(out v);
                return v;
            }
            else if (IsMetaType(t))
            {
                bool isNull;
                ar.Read(out isNull);
                if (isNull)
                    return null;
                Hash64 hash;
                ar.Read(out hash);
                ulong skipPoint = 0;
                if (hasSkip)
                    skipPoint = SerializerHelper.GetSkipOffset(ar);
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(hash);
                if (meta != null)
                {
                    object v;
                    if (obj != null && obj.GetType() == meta.ClassType.SystemType)
                        v = obj;
                    else
                        v = Rtti.TtTypeDescManager.CreateInstance(meta.ClassType);
                    Hash64 version;
                    ar.Read(out version);
                    var ver = meta.GetMetaVersion(version.AllData);
                    if (ver != null)
                    {
                        var serial = v as ISerializer;
                        if (serial != null)
                            serial.OnPreRead(ar.Tag, hostObject, false);
                        ReadMember(ar, v, ver, hasSkip);
                        if(serial != null)
                            serial.OnPostRead(ar.Tag, hostObject, false);
                    }
                    else
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Meta({meta.ClassMetaName}:{version}) not found");
                        if (hasSkip)
                        {
                            ar.Seek(skipPoint);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    return v;
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Meta({hash}) not found");
                    if (hasSkip)
                        ar.Seek(skipPoint);
                }
                return null;
            }
            else if (t.IsGenericType && t.GetInterface("IList") != null)
            {
                var gt = t.GetGenericArguments()[0];
                System.Collections.IList v;
                if (obj != null && obj.GetType() == t)
                    v = obj as System.Collections.IList;
                else
                    v = Rtti.TtTypeDescManager.CreateInstance(t) as System.Collections.IList;
                int count;
                ar.Read(out count);
                v.Clear();
                for (int i = 0; i < count; i++)
                {
                    var m = ReadMetaObject(ar, gt, hostObject, null, hasSkip);
                    v.Add(m);
                }
                return v;
            }
            else if (t.IsGenericType && t.GetInterface("IDictionary") != null)
            {
                var kt = t.GetGenericArguments()[0];
                var vt = t.GetGenericArguments()[1];
                System.Collections.IDictionary v;
                if (obj != null && obj.GetType() == t)
                    v = obj as System.Collections.IDictionary;
                else
                    v = Rtti.TtTypeDescManager.CreateInstance(t) as System.Collections.IDictionary;
                int count;
                ar.Read(out count);
                v.Clear();
                for (int i = 0; i < count; i++)
                {
                    var rk = ReadMetaObject(ar, kt, hostObject, null, hasSkip);
                    var rv = ReadMetaObject(ar, vt, hostObject, null, hasSkip);
                    v[rk] = rv;
                }
                return v;
            }
            return null;
        }
        public static unsafe void WriteObject(IWriter ar, Type t, object obj)
        {
            if (t.IsEnum)
            {
                var v = System.Convert.ToInt32(obj);
                ar.Write(v);
            }
            else if (IsUnmanagedType(t))
            {
                var size = System.Runtime.InteropServices.Marshal.SizeOf(t);
                var pBuffer = stackalloc byte[size];
                System.Runtime.InteropServices.Marshal.StructureToPtr(obj, (IntPtr)pBuffer, false);
                ar.WritePtr(pBuffer, size);
            }
            else if (t == typeof(string))
            {
                ar.Write((string)obj);
            }
            else if (t == typeof(RName))
            {
                ar.Write((RName)obj);
            }
            else if (t == typeof(Rtti.TtTypeDesc))
            {
                ar.Write((Rtti.TtTypeDesc)obj);
            }
            else if (t == typeof(byte[]))
            {
                ar.Write((byte[])obj);
            }
            else if (t == typeof(Support.TtBitset))
            {
                ar.Write((Support.TtBitset)obj);
            }
            else if (t == typeof(TtMemWriter))
            {
                ar.Write((TtMemWriter)obj);
            }
            else if (IsMetaType(t))
            {
                if (obj == null)
                {
                    ar.Write(true);
                }
                else
                {
                    ar.Write(false);
                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeStr(obj.GetType()));
                    if (meta != null)
                    {
                        ar.Write(meta.TypeHash);
                        var offset = SerializerHelper.WriteSkippable(ar);
                        ar.Write(meta.CurrentVersion.MetaHash);
                        WriteMember(ar, obj, meta.CurrentVersion);
                        SerializerHelper.SureSkippable(ar, offset);
                    }
                    else
                    {
                        ar.Write(Hash64.Empty);
                    }
                }
            }
            else if (t.IsGenericType && t.GetInterface("IList") != null)
            {
                var gt = t.GetGenericArguments()[0];
                var v = obj as System.Collections.IList;
                if (v == null)
                {
                    ar.Write((int)0);
                }
                else
                {
                    ar.Write(v.Count);
                    for (int i = 0; i < v.Count; i++)
                    {
                        WriteMetaObject(ar, gt, v[i]);
                    }
                }
            }
            else if (t.IsGenericType && t.GetInterface("IDictionary") != null)
            {
                var kt = t.GetGenericArguments()[0];
                var vt = t.GetGenericArguments()[1];
                var v = obj as System.Collections.IDictionary;
                if (v == null)
                {
                    ar.Write((int)0);
                }
                else
                {
                    ar.Write(v.Count);
                    System.Collections.IDictionaryEnumerator j = v.GetEnumerator();
                    while (j.MoveNext())
                    {
                        WriteMetaObject(ar, kt, j.Key);
                        WriteMetaObject(ar, vt, j.Value);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
        private static void WriteMetaObject(IWriter ar, Type kt, object obj)
        {
            if (IsPrimitiveType(kt))
            {
                WriteObject(ar, kt, obj);
            }
            else
            {
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeStr(obj.GetType()));
                if (meta != null)
                {
                    ar.Write(meta.TypeHash);
                    var offset = SerializerHelper.WriteSkippable(ar);
                    ar.Write(meta.CurrentVersion.MetaHash);
                    WriteMember(ar, obj, meta.CurrentVersion);
                    SerializerHelper.SureSkippable(ar, offset);
                }
                else
                {
                    ar.Write(Hash64.Empty);
                }
            }
        }
        private static object ReadMetaObject(IReader ar, Type gt, object hostObject, object obj, bool hasSkipPoint)
        {
            if (IsPrimitiveType(gt))
            {
                return ReadObject(ar, gt, hostObject, null, hasSkipPoint);
            }
            else
            {
                Hash64 mh;
                ar.Read(out mh);
                ulong skipPoint = 0;
                if (hasSkipPoint)
                    skipPoint = SerializerHelper.GetSkipOffset(ar);
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(mh);
                if (meta != null)
                {
                    Hash64 version;
                    ar.Read(out version);
                    var ver = meta.GetMetaVersion(version.AllData);
                    if (ver == null)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Meta({meta.ClassMetaName}:{version}) not found");
                        if (hasSkipPoint)
                            ar.Seek(skipPoint);
                        return null;
                    }
                    object v;
                    if (obj != null)
                        v = obj;
                    else
                        v = Rtti.TtTypeDescManager.CreateInstance(meta.ClassType);
                    var serial = v as ISerializer;
                    if (serial != null)
                        serial.OnPreRead(ar.Tag, hostObject, false);
                    ReadMember(ar, v, ver, hasSkipPoint);
                    if (serial != null)
                        serial.OnPostRead(ar.Tag, hostObject, false);
                    return v;
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Meta({mh}) not found");
                    if (hasSkipPoint)
                        ar.Seek(skipPoint);
                    return null;
                }
            }
        }
        public abstract Hash160 GetVersionHash();
        public static Hash160 CalcVersionHash()
        {
            var metas = Rtti.TtClassMetaManager.Instance.Metas.Values.ToList();
            for (int i = metas.Count - 1; i >= 0; i--)
            {
                if (metas[i].ClassType == null || metas[i].CurrentVersion == null)
                    metas.RemoveAt(i);
            }
            metas.Sort((x, y) =>
            {
                return x.ClassMetaName.CompareTo(y.ClassMetaName);
            });
            string hashStr = "";
            foreach (var met in metas)
            {
                hashStr += met.ClassMetaName;
                var vers = met.MetaVersions.Values.ToList();
                vers.Sort((x, y) =>
                {
                    return x.MetaHash.CompareTo(y.MetaHash);
                });
                foreach (var v in vers)
                {
                    hashStr += v;
                }
            }
            return Hash160.CreateHash160(hashStr);
        }
        public bool DataCopy(object target, object src)
        {
            if (target.GetType().IsSubclassOf(src.GetType()) == false && target.GetType() != src.GetType())
                return false;
            
            var fn = FindCopyer(Rtti.TtTypeDesc.TypeStr(src.GetType()));
            if (fn == null)
                return false;
            fn(target, src);
            return true;
        }
        public class TtCodeWriter : CodeBuilder.TtCodeCreator
        {
            public TtCodeWriter()
            {
                mSegmentStartStr = "{";
                mSegmentEndStr = "}";
                mIndentStr = "\t";
            }
        }
        
        public string GenCode(Hash160 hash)
        {
            string code = "";
            var creator = new TtCodeWriter();
            creator.AddLine("//Gen by engine", ref code);
            List<Rtti.TtClassMeta> metas = Rtti.TtClassMetaManager.Instance.Metas.Values.ToList();
            for (int i = metas.Count - 1; i >= 0; i--)
            {
                if (metas[i].ClassType == null || metas[i].CurrentVersion == null)
                    metas.RemoveAt(i);
            }
            metas.Sort((x, y) =>
            {
                return x.ClassMetaName.CompareTo(y.ClassMetaName);
            });
            creator.AddLine($"namespace EngineNS.Plugins.DataCopyer", ref code);
            creator.PushSegment(ref code);
            {
                var klsCodes = new string[metas.Count];
                TtEngine.Instance.EventPoster.ParallelFor(metas.Count, (index, state) =>
                {
                    var met = metas[index];
                    if (met.ClassType.IsValueType)
                        return;

                    string klsCode = "";
                    var klsCreator = new TtCodeWriter();
                    klsCreator.IntentCount = creator.IntentCount;
                    GenCode(met, klsCreator, ref klsCode);
                    klsCodes[index] = klsCode;
                }, -1, null);
                foreach(var c in klsCodes)
                {
                    if (c == null)
                        continue;
                    code += c;
                }
                //foreach (var met in metas)
                //{
                //    if (met.ClassType.IsValueType)
                //        continue;
                //    string klsCode = "";
                //    GenCode(met, creator, ref klsCode);
                //    code += klsCode;
                //}
            }
            creator.PopSegment(ref code);
            
            creator.AddLine($"namespace EngineNS.Plugins.DataCopyer", ref code);
            creator.PushSegment(ref code);
            {
                creator.AddLine($"partial class TtDataCopyerPlugin", ref code);
                creator.PushSegment(ref code);
                {
                    creator.AddLine($"internal TtDataCopyerPlugin()", ref code);
                    creator.PushSegment(ref code);
                    {
                        foreach (var met in metas)
                        {
                            if (met.ClassType.IsValueType)
                                continue;
                            
                            string klsCode = "";
                            var klsCreator = new TtCodeWriter();
                            klsCreator.IntentCount = creator.IntentCount;

                            klsCreator.PushSegment(ref klsCode);
                            {
                                var name = met.ClassType.FullName.Replace('.', '_');
                                name = name.Replace('+', '_');
                                klsCreator.AddLine($"var kls = this.GetClassCopyer(\"{met.ClassMetaName}\");", ref klsCode);
                                klsCreator.AddLine($"kls.Writer = {name}.WriteCurrentVersion;", ref klsCode);
                                klsCreator.AddLine($"kls.Copy = {name}.CopyCurrentVersion;", ref klsCode);
                                foreach (var v in met.MetaVersions)
                                {
                                    klsCreator.AddLine($"kls.RegVersion({v.Key}, {name}.Read_{v.Key});", ref klsCode);
                                }
                            }
                            klsCreator.PopSegment(ref klsCode);
                            code += klsCode;
                        }

                        creator.AddLine($"this.VersionHash = EngineNS.Hash160.Parse(\"{hash.ToString()}\");", ref code);
                    }
                    creator.PopSegment(ref code);
                }
                creator.PopSegment(ref code);
            }
            creator.PopSegment(ref code);

            return code;
        }
        private static bool IsUnmanagedType(Type type)
        {
            if (type.IsEnum)
                return true;
            if (type.IsValueType == false)
                return false;
            try
            {
                var size = System.Runtime.InteropServices.Marshal.SizeOf(type);
                return size > 0;
            }
            catch
            {
                return false;
            }
        }
        private static bool IsPrimitiveType(Type type)
        {
            bool isValue = IsUnmanagedType(type)|| type == typeof(string) ||
                type == typeof(RName) || type == typeof(Rtti.TtTypeDesc) ||
                type == typeof(byte[]) ||
                type == typeof(Support.TtBitset) ||
                type == typeof(TtMemWriter);
            return isValue;
        }
        private static bool IsMetaType(Type type)
        {
            bool value = type.IsGenericType || type.IsArray ||
                            type.GetInterface("IList") != null ||
                            type.GetInterface("IDictionary") != null;
            return !value;
        }
        public void GenCode(Rtti.TtClassMeta meta, TtCodeWriter creator, ref string code)
        {
            if (meta.CurrentVersion == null)
            {
                return;
            }
            var vers = meta.MetaVersions.Values.ToList();
            vers.Sort((x, y) =>
            {
                return x.MetaHash.CompareTo(y.MetaHash);
            });
            var name = meta.ClassType.FullName.Replace('.', '_');
            name = name.Replace('+', '_');
            creator.AddLine($"static class {name}", ref code);
            creator.PushSegment(ref code);
            {
                GenWriteCurrentVersion(meta, creator, ref code);
                GenCopyVersion(meta, creator, ref code, meta.CurrentVersion);

                foreach (var i in vers)
                {
                    //GenCopyVersion(meta, creator, ref code, i);
                    GenReadVersion(meta, creator, ref code, i);
                }
            }
            creator.PopSegment(ref code);
        }

        public void GenWriteCurrentVersion(Rtti.TtClassMeta meta, TtCodeWriter creator, ref string code)
        {
            creator.AddLine($"internal static {typeof(FWrite).FullName.Replace('+', '.')} WriteCurrentVersion = (EngineNS.IO.IWriter ar, object obj)=>", ref code);
            creator.PushSegment(ref code);
            {
                creator.AddLine($"var srcObj = obj as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                foreach (var j in meta.CurrentVersion.Propertys)
                {
                    if (j.PropInfo != null && j.PropInfo.CanRead && j.PropInfo.GetGetMethod() != null && j.PropInfo.GetGetMethod().IsPublic)
                    {
                        var attr = j.PropInfo.GetCustomAttribute<Rtti.MetaAttribute>();
                        if (j.PropInfo.CanRead == false ||
                            attr.IsNoSerializable ||
                            j.IsGetStatic)
                        {
                            continue;
                        }
                        if (IsPrimitiveType(j.PropInfo.PropertyType))
                        {
                            creator.AddLine($"ar.Write(srcObj.{j.PropertyName});", ref code);
                        }
                        else
                        {
                            if (IsMetaType(j.PropInfo.PropertyType))
                            {
                                creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"var typeStr = EngineNS.Rtti.TtTypeDesc.TypeStr(srcObj.{j.PropertyName}.GetType());", ref code);
                                    creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindWriter(typeStr);", ref code);
                                    creator.AddLine($"var meta = EngineNS.Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);", ref code);
                                    creator.AddLine($"if (fn != null && meta != null)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"ar.Write(false);", ref code);
                                        creator.AddLine($"ar.Write(EngineNS.Hash64.FromString(typeStr));", ref code);
                                        creator.AddLine($"ar.Write(meta.CurrentVersion.MetaHash);", ref code);
                                        creator.AddLine($"fn(ar, srcObj.{j.PropertyName});", ref code);
                                    }
                                    creator.PopSegment(ref code);
                                    creator.AddLine($"else", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"ar.Write(true);", ref code);
                                    }
                                    creator.PopSegment(ref code);
                                }
                                creator.PopSegment(ref code);
                                creator.AddLine($"else", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"ar.Write(true);", ref code);
                                }
                                creator.PopSegment(ref code);
                            }
                            else if (j.PropInfo.PropertyType.IsGenericType && j.PropInfo.PropertyType.GetInterface("IList") != null)
                            {
                                creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    Type type = j.PropInfo.PropertyType.GetGenericArguments()[0];
                                    var typeName = type.FullName.Replace('+', '.');
                                    creator.AddLine($"var Srclst = srcObj.{j.PropertyName} as System.Collections.Generic.List<{typeName}>;", ref code);

                                    creator.AddLine($"ar.Write(Srclst.Count);", ref code);
                                    bool isValue = IsPrimitiveType(type);
                                    creator.AddLine($"for (int i = 0; i < Srclst.Count; i++)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        if (isValue)
                                        {
                                            creator.AddLine($"ar.Write(Srclst[i]);", ref code);
                                        }
                                        else
                                        {
                                            creator.AddLine($"if (Srclst[i] != null)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"var typeStr = EngineNS.Rtti.TtTypeDesc.TypeStr(Srclst[i].GetType());", ref code);
                                                creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindWriter(typeStr);", ref code);
                                                creator.AddLine($"var meta = EngineNS.Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);", ref code);
                                                creator.AddLine($"if (fn != null && meta != null)", ref code);
                                                creator.PushSegment(ref code);
                                                {
                                                    creator.AddLine($"ar.Write(EngineNS.Hash64.FromString(typeStr));", ref code);
                                                    creator.AddLine($"ar.Write(meta.CurrentVersion.MetaHash);", ref code);
                                                    creator.AddLine($"fn(ar, Srclst[i]);", ref code);
                                                }
                                                creator.PopSegment(ref code);
                                                creator.AddLine($"else", ref code);
                                                creator.PushSegment(ref code);
                                                {
                                                    creator.AddLine($"ar.Write(EngineNS.Hash64.Empty);", ref code);
                                                }
                                                creator.PopSegment(ref code);
                                            }
                                            creator.PopSegment(ref code);
                                            creator.AddLine($"else", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"ar.Write(EngineNS.Hash64.Empty);", ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                    }
                                    creator.PopSegment(ref code);
                                }
                                creator.PopSegment(ref code);
                                creator.AddLine($"else", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"ar.Write((int)0);", ref code);
                                }
                                creator.PopSegment(ref code);
                            }
                        }
                    }
                }
            }
            creator.PopSegment(ref code, true);
        }
        public void GenCopyVersion(Rtti.TtClassMeta meta, TtCodeWriter creator, ref string code, Rtti.TtMetaVersion i)
        {
            creator.AddLine($"internal static {typeof(FCopy).FullName.Replace('+', '.')} CopyCurrentVersion = (object tar, object src)=>", ref code);
            creator.PushSegment(ref code);
            {
                creator.AddLine($"var tarObj = tar as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                creator.AddLine($"var srcObj = src as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                foreach (var j in i.Propertys)
                {
                    if (j.PropInfo != null && j.PropInfo.CanWrite && j.PropInfo.GetSetMethod() != null && j.PropInfo.GetSetMethod().IsPublic)
                    {
                        if (j.PropInfo.PropertyType.IsValueType ||
                            j.PropInfo.PropertyType == typeof(string) ||
                            j.PropInfo.PropertyType == typeof(RName) ||
                            j.PropInfo.PropertyType == typeof(Rtti.TtTypeDesc))
                        {
                            creator.AddLine($"tarObj.{j.PropertyName} = srcObj.{j.PropertyName};", ref code);
                        }
                        else
                        {
                            if (j.PropInfo.PropertyType.IsClass &&
                                !j.PropInfo.PropertyType.IsGenericType &&
                                !j.PropInfo.PropertyType.IsArray &&
                                j.PropInfo.PropertyType.GetInterface("IList") == null &&
                                j.PropInfo.PropertyType.GetInterface("IDictionary") == null &&
                                j.PropInfo.GetGetMethod() != null)
                            {
                                creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    if (j.PropInfo.GetSetMethod() != null)
                                    {
                                        if (j.PropInfo.PropertyType.GetConstructor(new Type[0]) != null)
                                        {
                                            creator.AddLine($"if (tarObj.{j.PropertyName} == null || tarObj.{j.PropertyName}.GetType() != srcObj.{j.PropertyName}.GetType())", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"tarObj.{j.PropertyName} = EngineNS.Rtti.TtTypeDescManager.CreateInstance(srcObj.{j.PropertyName}.GetType()) as {j.PropInfo.PropertyType.FullName.Replace('+', '.')};", ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                    }

                                    creator.AddLine($"if (tarObj.{j.PropertyName} != null)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindCopyer(Rtti.TtTypeDescGetter<{j.PropInfo.PropertyType.FullName.Replace('+', '.')}>.TypeDesc.TypeString);", ref code);
                                        creator.AddLine($"if (fn != null)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            creator.AddLine($"fn(tarObj.{j.PropertyName}, srcObj.{j.PropertyName});", ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                    creator.PopSegment(ref code);
                                }
                                creator.PopSegment(ref code);
                                creator.AddLine($"else if (srcObj.{j.PropertyName} == null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"tarObj.{j.PropertyName} = null;", ref code);
                                }
                                creator.PopSegment(ref code);
                            }
                            else if (j.PropInfo.PropertyType.IsGenericType && j.PropInfo.PropertyType.GetInterface("IList") != null)
                            {
                                creator.AddLine($"if (srcObj.{j.PropertyName} != null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    if (j.PropInfo.GetSetMethod() != null)
                                    {
                                        if (j.PropInfo.PropertyType.GetConstructor(new Type[0]) != null)
                                        {
                                            creator.AddLine($"if (tarObj.{j.PropertyName} == null)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"tarObj.{j.PropertyName} = new();", ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                    }

                                    creator.AddLine($"if (tarObj.{j.PropertyName} != null)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        var type = j.PropInfo.PropertyType.GetGenericArguments()[0];
                                        string typeName = type.FullName.Replace('+', '.');
                                        creator.AddLine($"var Tarlst = tarObj.{j.PropertyName} as System.Collections.Generic.List<{typeName}>;", ref code);
                                        creator.AddLine($"var Srclst = srcObj.{j.PropertyName} as System.Collections.Generic.List<{typeName}>;", ref code);
                                        bool isValue = type.IsValueType || type == typeof(string) || type == typeof(RName) || type == typeof(Rtti.TtTypeDesc);
                                        creator.AddLine($"Tarlst.Clear();", ref code);
                                        creator.AddLine($"for (int i = 0; i < Srclst.Count; i++)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            if (isValue)
                                            {
                                                creator.AddLine($"Tarlst.Add(Srclst[i]);", ref code);
                                            }
                                            else
                                            {
                                                creator.AddLine($"{typeName} tmp = Rtti.TtClassMeta.CloneProperty(Srclst[i]) as {typeName};", ref code);
                                                creator.AddLine($"Tarlst.Add(tmp);", ref code);
                                            }
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                    creator.PopSegment(ref code);
                                }
                                creator.PopSegment(ref code);
                            }
                        }
                    }
                }
            }
            creator.PopSegment(ref code, true);
        }
        public void GenReadVersion(Rtti.TtClassMeta meta, TtCodeWriter creator, ref string code, Rtti.TtMetaVersion i)
        {
            creator.AddLine($"internal static {typeof(FReader).FullName.Replace('+', '.')} Read_{i.MetaHash} = (EngineNS.IO.IReader ar, object obj)=>", ref code);
            creator.PushSegment(ref code);
            {
                creator.AddLine($"var srcObj = obj as {meta.ClassType.FullName.Replace('+', '.')};", ref code);
                foreach (var j in i.Propertys)
                {
                    if (j.FieldType==null)
                        continue;
                    if (j.PropInfo != null)
                    {
                        var attr = j.PropInfo.GetCustomAttribute<Rtti.MetaAttribute>();
                        if(attr != null && attr.IsNoSerializable)
                        {
                            continue;
                        }
                    }
                    if (j.CustumSerializer != null)
                    {
                        
                    }
                    else
                    {
                        var type = j.FieldType.FullName.Replace('+', '.');
                        if (IsPrimitiveType(j.FieldType.SystemType))
                        {
                            creator.AddLine($"{type} t_{j.PropertyName};", ref code);
                            creator.AddLine($"ar.Read(out t_{j.PropertyName});", ref code);
                            if (j.PropInfo != null && j.PropInfo.CanWrite && j.PropInfo.GetSetMethod() != null && j.PropInfo.GetSetMethod().IsPublic)
                            {
                                creator.AddLine($"srcObj.{j.PropInfo.Name} = t_{j.PropertyName};", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"if (srcObj is IO.ISerializer sr)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"sr.OnPropertyRead(ar.Tag, \"{j.PropInfo.Name}\", false);", ref code);
                                    }
                                    creator.PopSegment(ref code);
                                }
                                creator.PopSegment(ref code);
                            }
                        }
                        else if (IsMetaType(j.FieldType.SystemType))
                        {
                            creator.AddLine($"EngineNS.Hash64 type_{j.PropertyName};", ref code);
                            creator.AddLine($"ar.Read(out type_{j.PropertyName});", ref code);
                            creator.AddLine($"var meta_{j.PropertyName} = EngineNS.Rtti.TtClassMetaManager.Instance.GetMeta(type_{j.PropertyName});", ref code);
                            creator.AddLine($"if(meta_{j.PropertyName} != null)", ref code);
                            creator.PushSegment(ref code);
                            {
                                creator.AddLine($"EngineNS.Hash64 ver_{j.PropertyName};", ref code);
                                creator.AddLine($"ar.Read(out ver_{j.PropertyName});", ref code);
                                creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindReader(meta_{j.PropertyName}.ClassType.TypeString, ver_{j.PropertyName} );", ref code);
                                creator.AddLine($"if (fn != null)", ref code);
                                creator.PushSegment(ref code);
                                {
                                    creator.AddLine($"{type} t_{j.PropertyName} = null;", ref code);
                                    if (j.PropInfo != null && j.PropInfo.CanRead && j.PropInfo.GetGetMethod() != null && j.PropInfo.GetGetMethod().IsPublic)
                                    {
                                        creator.AddLine($"t_{j.PropertyName} = srcObj.{j.PropertyName};", ref code);
                                    }
                                    creator.AddLine($"if (t_{j.PropertyName} == null)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"t_{j.PropertyName} = EngineNS.Rtti.TtTypeDescManager.CreateInstance(meta_{j.PropertyName}.ClassType) as {type};", ref code);
                                    }
                                    creator.PopSegment(ref code);
                                    creator.AddLine($"fn(ar, t_{j.PropertyName});", ref code);
                                    if (j.PropInfo != null && j.PropInfo.CanWrite && j.PropInfo.GetSetMethod() != null && j.PropInfo.GetSetMethod().IsPublic)
                                    {
                                        creator.AddLine($"srcObj.{j.PropInfo.Name} = t_{j.PropertyName};", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            creator.AddLine($"if (srcObj is IO.ISerializer sr)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"sr.OnPropertyRead(ar.Tag, \"{j.PropInfo.Name}\", false);", ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                }
                                creator.PopSegment(ref code);
                            }
                            creator.PopSegment(ref code);
                        }
                        else if (j.FieldType.SystemType.IsGenericType && j.FieldType.SystemType.GetInterface("IList") != null)
                        {
                            var kt = j.FieldType.SystemType.GetGenericArguments()[0];
                            var ktStr = kt.FullName.Replace('+', '.');
                            type = $"System.Collections.Generic.List<{ktStr}>";
                            creator.AddLine($"{type} t_{j.PropertyName} = null;", ref code);
                            if (j.PropInfo != null && j.PropInfo.CanRead && j.PropInfo.GetGetMethod() != null && j.PropInfo.GetGetMethod().IsPublic)
                            {
                                creator.AddLine($"t_{j.PropertyName} = srcObj.{j.PropertyName};", ref code);
                            }
                            creator.AddLine($"if (t_{j.PropertyName} == null)", ref code);
                            creator.PushSegment(ref code);
                            {
                                creator.AddLine($"t_{j.PropertyName} = EngineNS.Rtti.TtTypeDescManager.CreateInstance(typeof({type})) as {type};", ref code);
                            }
                            creator.PopSegment(ref code);

                            creator.AddLine($"int count_{j.PropertyName};", ref code);
                            creator.AddLine($"ar.Read(out count_{j.PropertyName});", ref code);
                            creator.AddLine($"for(int i = 0; i<count_{j.PropertyName}; i++)", ref code);
                            creator.PushSegment(ref code);
                            {
                                if (IsPrimitiveType(kt))
                                {
                                    creator.AddLine($"{ktStr} t;", ref code);
                                    creator.AddLine($"ar.Read(out t);", ref code);
                                    creator.AddLine($"t_{j.PropertyName}.Add(t);", ref code);
                                }
                                else if (IsMetaType(kt))
                                {
                                    creator.AddLine($"{ktStr} t = null;", ref code);
                                    creator.AddLine($"EngineNS.Hash64 typeHash;", ref code);
                                    creator.AddLine($"ar.Read(out typeHash);", ref code);
                                    creator.AddLine($"var meta = EngineNS.Rtti.TtClassMetaManager.Instance.GetMeta(typeHash);", ref code);
                                    creator.AddLine($"if (meta != null)", ref code);
                                    creator.PushSegment(ref code);
                                    {
                                        creator.AddLine($"EngineNS.Hash64 verHash;", ref code);
                                        creator.AddLine($"ar.Read(out verHash);", ref code);
                                        creator.AddLine($"var fn = EngineNS.TtEngine.Instance.DataCopyer.FindReader(meta.ClassType.TypeString, verHash);", ref code);
                                        creator.AddLine($"if (fn != null)", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            creator.AddLine($"t = EngineNS.Rtti.TtTypeDescManager.CreateInstance(meta.ClassType) as {ktStr};", ref code);
                                            creator.AddLine($"fn(ar, t);", ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                    creator.PopSegment(ref code);

                                    creator.AddLine($"t_{j.PropertyName}.Add(t);", ref code);
                                    if (j.PropInfo != null && j.PropInfo.CanWrite && j.PropInfo.GetSetMethod() != null && j.PropInfo.GetSetMethod().IsPublic)
                                    {
                                        creator.AddLine($"srcObj.{j.PropInfo.Name} = t_{j.PropertyName};", ref code);
                                        creator.PushSegment(ref code);
                                        {
                                            creator.AddLine($"if (srcObj is IO.ISerializer sr)", ref code);
                                            creator.PushSegment(ref code);
                                            {
                                                creator.AddLine($"//sr.OnPropertyRead(ar.Tag, typeof({type}), false);", ref code);
                                            }
                                            creator.PopSegment(ref code);
                                        }
                                        creator.PopSegment(ref code);
                                    }
                                }
                            }
                            creator.PopSegment(ref code);
                        }
                        else if (j.FieldType.SystemType.IsGenericType && j.FieldType.SystemType.GetInterface("IDictionary") != null)
                        {

                        }
                    }
                }
            }
            creator.PopSegment(ref code, true);
        }
    }
}
namespace EngineNS
{
    partial class TtEngine
    {
        Bricks.DataCopyer.TtDataCopyer mDataCopyer;
        public Bricks.DataCopyer.TtDataCopyer DataCopyer
        {
            get
            {
                if (mDataCopyer == null)
                {
                    var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("DataCopyer");
                    if (serverPlugin != null)
                    {
                        mDataCopyer = serverPlugin.GetPluginObject<TtDataCopyer>();
                        var hash = Bricks.DataCopyer.TtDataCopyer.CalcVersionHash();
                        if (hash != mDataCopyer.GetVersionHash())
                        {
                            var code = mDataCopyer.GenCode(hash);
                            var file = this.FileManager.GetRoot(IO.TtFileManager.ERootDir.PluginSource) + "DataCopyer/DataCopyer/Copyer.gen.cs";
                            TtFileManager.WriteAllText(file, code);

                            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Plugin DataCopyer need build");
#if PWindow
                            mDataCopyer = null;
                            var path = TtEngine.Instance.FileManager.GetRoot(TtFileManager.ERootDir.PluginSource);
                            var proj = TtFileManager.CombinePath(path, "DataCopyer/DataCopyer.All/DataCopyer.All.csproj");
                            if (Bricks.AssemblyLoader.TtPluginModule.BuildProject(proj))
                            {
                                serverPlugin.ForceReload();
                                mDataCopyer = serverPlugin.GetPluginObject<TtDataCopyer>();
                            }
                            else
                            {
                                TtNativeWindow.MessageBoxA(IntPtr.Zero, "Plugin DataCopyer need build", "DataCopyer", 0);
                                TtEngine.Instance.PostQuitMessage();
                            }
#endif
                            //rebuild plugin
                        }
                    }
                }
                return mDataCopyer;
            }
        }
    }
}
