using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO.Serializer
{
    public class SerializerHelper
    {
        protected static Support.BitSet WriteSaveFields<T>(ref T obj, TypeDescGenerator.TypeDesc tDesc, IWriter pkg) 
            where T : struct
        {
            var bitSet = new Support.BitSet();
            bitSet.Init((UInt16)tDesc.Members.Count);
            for (int i = 0; i < tDesc.Members.Count; i++)
            {
                bool cmp = tDesc.IsDefaultValue(obj, tDesc.Members[i].PropInfo);
                if (cmp == false)
                    bitSet.SetBit((UInt32)i, true);
            }
            var byteArray = TypeDescGenerator.Instance.GetSerializer(typeof(byte[]));
            byteArray.WriteValue(bitSet.Data, pkg);
            return bitSet;
        }
        protected static Support.BitSet WriteSaveFields(ISerializer obj, TypeDescGenerator.TypeDesc tDesc, IWriter pkg)
        {
            var bitSet = new Support.BitSet();
            bitSet.Init((UInt16)tDesc.Members.Count);
            for (int i = 0; i < tDesc.Members.Count; i++)
            {
                bool cmp = tDesc.IsDefaultValue(obj, tDesc.Members[i].PropInfo);
                if (cmp == false)
                    bitSet.SetBit((UInt32)i, true);
            }
            var byteArray = TypeDescGenerator.Instance.GetSerializer(typeof(byte[]));
            byteArray.WriteValue(bitSet.Data, pkg);
            return bitSet;
        }
        protected static Support.BitSet ReadSaveFields(TypeDescGenerator.TypeDesc tDesc, IReader pkg)
        {
            var bitSet = new Support.BitSet();
            var byteArray = TypeDescGenerator.Instance.GetSerializer(typeof(byte[]));
            byte[] bits = byteArray.ReadValue(pkg) as byte[];
            bitSet.Init((UInt32)tDesc.Members.Count, bits);
            return bitSet;
        }
        public static void ReadObject(object obj, IReader pkg)
        {
            if (obj == null)
                return;
            var tDesc = TypeDescGenerator.Instance.GetTypeDesc(obj.GetType());
            var bitSet = ReadSaveFields(tDesc, pkg);
            for (var i = 0; i < tDesc.Members.Count; i++)
            {
                if (bitSet.IsBit(i) == false)
                    continue;
                var mbr = tDesc.Members[i];
                if ((mbr.PropInfo.AllowIOType & pkg.IOType) != pkg.IOType)
                    continue;
                if (mbr.IsList)
                    mbr.Serializer.ReadValueList(obj, mbr.PropInfo, pkg);
                else
                    mbr.Serializer.ReadValue(obj, mbr.PropInfo, pkg);
            }
        }
        public static void ReadObject(ISerializer obj, IReader pkg, Rtti.MetaData metaData)
        {
            var srObj = obj as IO.Serializer.Serializer;
            if (srObj != null)
                srObj.BeforeRead();
            for (var i = 0; i < metaData.Members.Count; i++)
            {
                var mbr = metaData.Members[i];
                if (mbr.PropInfo != null && (mbr.PropInfo.AllowIOType & pkg.IOType) != pkg.IOType)
                    continue;
                if (mbr.IsList)
                    mbr.Serializer.ReadValueList(obj, mbr.PropInfo, pkg);
                else
                    mbr.Serializer.ReadValue(obj, mbr.PropInfo, pkg);
            }
        }
        public static void WriteObject(ISerializer obj, IWriter pkg)
        {
            if (obj == null)
                return;
            var tDesc = TypeDescGenerator.Instance.GetTypeDesc(obj.GetType());
            var bitSet = WriteSaveFields(obj, tDesc, pkg);
            for (var i = 0; i < tDesc.Members.Count; i++)
            {
                if (bitSet.IsBit(i) == false)
                    continue;
                var mbr = tDesc.Members[i];
                if ((mbr.PropInfo.AllowIOType & pkg.IOType) != pkg.IOType)
                    continue;
                if (mbr.IsList)
                    mbr.Serializer.WriteValueList(obj, mbr.PropInfo, pkg);
                else
                    mbr.Serializer.WriteValue(obj, mbr.PropInfo, pkg);
            }
        }
        public static void WriteObject<T>(ref T obj, IWriter pkg) where T : struct
        {
            var tDesc = TypeDescGenerator.Instance.GetTypeDesc(obj.GetType());
            var bitSet = WriteSaveFields<T>(ref obj, tDesc, pkg);
            for (var i = 0; i < tDesc.Members.Count; i++)
            {
                if (bitSet.IsBit(i) == false)
                    continue;
                var mbr = tDesc.Members[i];
                if ((mbr.PropInfo.AllowIOType & pkg.IOType) != pkg.IOType)
                    continue;
                if (mbr.IsList)
                    mbr.Serializer.WriteValueList(obj, mbr.PropInfo, pkg);
                else
                    mbr.Serializer.WriteValue(obj, mbr.PropInfo, pkg);
            }
        }
        public static void WriteObject(ISerializer obj, IWriter pkg, Rtti.MetaData metaData)
        {
            var srObj = obj as IO.Serializer.Serializer;
            if (srObj != null)
                srObj.BeforeWrite();
            for (var i = 0; i < metaData.Members.Count; i++)
            {
                var mbr = metaData.Members[i];
                if ((mbr.PropInfo.AllowIOType & pkg.IOType) != pkg.IOType)
                    continue;
                if (mbr.IsList)
                    mbr.Serializer.WriteValueList(obj, mbr.PropInfo, pkg);
                else
                    mbr.Serializer.WriteValue(obj, mbr.PropInfo, pkg);
            }
        }

        public static object CloneObject(object obj, Type type)
        {
            if (obj == null)
                return null;
            if (type.GetInterface(typeof(ISerializer).FullName) != null)
            {
                var elem = obj as ISerializer;
                var clonedElem = elem.CloneObject();
                return clonedElem;
            }
            else if (type.IsSubclassOf(typeof(MemChunk)))
            {
                var elem = obj as MemChunk;
                var clonedElem = elem.CloneObject();
                return clonedElem;
            }
            else if (type == typeof(string))
            {
                return obj;
            }
            else if (type.IsValueType)
            {
                var proKey = type.GetProperty("Key");
                var proValue = type.GetProperty("Value");
                if (proKey == null || proValue == null)
                    return obj;

                var key = proKey.GetValue(obj, null);
                var value = proKey.GetValue(obj, null);
                var keyType = proKey.PropertyType;
                if (key != null)
                    keyType = key.GetType();
                var cloneKey = CloneObject(key, keyType);
                var valType = proValue.PropertyType;
                if (value != null)
                    valType = value.GetType();
                var cloneValue = CloneObject(value, valType);
                var retValue = System.Activator.CreateInstance(type, new object[] { cloneKey, cloneValue });
                return retValue;
            }
            else if (type == typeof(EngineNS.RName))
            {
                return obj;
            }
            else if (type.IsGenericType && (type.GetInterface(typeof(IEnumerable).FullName) != null))
            {
                var retValue = System.Activator.CreateInstance(type);
                var methodAdd = type.GetMethod("Add");
                if (methodAdd == null)
                    return retValue;
                var methodParameters = methodAdd.GetParameters();
                var proCount = type.GetProperty("Count");
                if (proCount == null)
                    return retValue;

                var enumerableValue = obj as IEnumerable;
                if (enumerableValue == null)
                    return retValue;

                foreach (var item in enumerableValue)
                {
                    object clonedItem;
                    if (item == null)
                        clonedItem = null;
                    else
                        clonedItem = CloneObject(item, item.GetType());

                    if (methodParameters.Length == 1)
                        methodAdd.Invoke(retValue, new object[] { clonedItem });
                    else if (methodParameters.Length == 2)
                    {
                        var proKey = item.GetType().GetProperty("Key");
                        var proValue = item.GetType().GetProperty("Value");
                        if (proKey == null || proValue == null)
                            return retValue;

                        methodAdd.Invoke(retValue, new object[] { proKey.GetValue(clonedItem, null), proValue.GetValue(clonedItem, null) });
                    }
                }

                return retValue;
            }
            else if (type.IsArray)
            {
                var valArray = obj as System.Array;
                var rank = valArray.Rank;
                var longLengths = new Int64[rank];
                var idxs = new Int64[rank];
                for (int i = 0; i < rank; i++)
                {
                    longLengths[i] = valArray.GetLongLength(i);
                    idxs[i] = 0;
                }
                var cloneArray = Array.CreateInstance(type.GetElementType(), longLengths);
                for (int i = 0; i < valArray.Length; i++)
                {
                    var srcVal = valArray.GetValue(idxs);
                    var cloneElem = CloneObject(srcVal, srcVal.GetType());
                    cloneArray.SetValue(cloneElem, idxs);

                    idxs[rank - 1]++;
                    for (int j = rank - 1; j >= 0; j--)
                    {
                        if (idxs[j] >= longLengths[j])
                        {
                            if (j == 0)
                            {
                                idxs[0] = 0;
                                break;
                            }
                            idxs[j - 1]++;
                            idxs[j] = 0;
                        }
                    }
                }
                return cloneArray;
            }
            return null;
        }
        public static ISerializer CloneObject(ISerializer obj)
        {
            var objType = obj.GetType();
            var result = (ISerializer)System.Activator.CreateInstance(objType);

            var desc = TypeDescGenerator.Instance.GetTypeDesc(objType);
            foreach (var f in desc.Members)
            {
                var i = f.PropInfo;
                try
                {
                    var atts = i.GetCustomAttributes(typeof(DisableCloneAttribute), false);
                    if (atts.Length > 0)
                        continue;
                    if (!i.CanWrite)
                        continue;

                    System.Type type = i.MemberType;
                    var val = i.GetValue(obj);
                    var cloneVal = CloneObject(val, type);
                    i.SetValue(result, cloneVal);
                }
                catch (System.Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", string.Format("ISerializer.CloneObject {0}:{1}", objType.FullName, i.Name));
                }
            }
            return result;
        }
        public static void ReadObjectXML(ISerializer obj, XmlNode node)
        {
            if (obj == null)
                return;

            var tDesc = TypeDescGenerator.Instance.GetTypeDesc(obj.GetType());
            for (var i = 0; i < tDesc.Members.Count; i++)
            {
                var mbr = tDesc.Members[i];
                if (mbr.IsList)
                    mbr.Serializer.ReadValueListXML(obj, mbr.PropInfo, node);
                else
                    mbr.Serializer.ReadValueXML(obj, mbr.PropInfo, node);
            }
        }
        public static void WriteObjectXML(ISerializer obj, XmlNode node)
        {
            if (obj == null)
                return;

            var tDesc = TypeDescGenerator.Instance.GetTypeDesc(obj.GetType());
            for (var i = 0; i < tDesc.Members.Count; i++)
            {
                var mbr = tDesc.Members[i];
                IXmlElement elem = null;
                if (mbr.IsList)
                {
                    elem = mbr.Serializer.WriteValueListXML(obj, mbr.PropInfo, node);
                }
                else
                {
                    elem = mbr.Serializer.WriteValueXML(obj, mbr.PropInfo, node);
                }
            }
        }
    }
}
