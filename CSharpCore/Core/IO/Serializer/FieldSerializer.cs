using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EngineNS.Rtti;

namespace EngineNS.IO.Serializer
{
    [Flags]
    public enum EIOType : byte
    {
        None        = 0,
        Normal      = 1,
        File        = 1 << 1,
        Network     = 1 << 2,

        All         = byte.MaxValue,
    }
    public interface IReader
    {
        EIOType IOType
        {
            get;
        }
        unsafe void ReadPtr(void* p, int length);
        void OnReadError();

        void Read(ISerializer v);
        void Read(out string v);
        void Read(out byte[] v);
        void Read(out ChunkReader v);
        void Read(out Support.BitSet data);

        void Read(out bool v);
        void Read(out sbyte v);
        void Read(out Int16 v);
        void Read(out Int32 v);
        void Read(out Int64 v);
        void Read(out byte v);
        void Read(out UInt16 v);
        void Read(out UInt32 v);
        void Read(out UInt64 v);
        void Read(out float v);
        void Read(out double v);
        void Read(out Vector2 v);
        void Read(out Vector3 v);
        void Read(out Vector4 v);
        void Read(out Quaternion v);
        void Read(out Matrix v);
        void Read(out Guid v);
    }
    public interface IWriter
    {
        EIOType IOType
        {
            get;
        }
        unsafe void WritePtr(void* p, int length);
        
        void Write(ISerializer v);
        void Write(string v);
        void Write(byte[] v);
        void Write(ChunkWriter v);
        void Write(Support.BitSet data);

        void Write(bool v);
        void Write(sbyte v);
        void Write(Int16 v);
        void Write(Int32 v);
        void Write(Int64 v);
        void Write(byte v);
        void Write(UInt16 v);
        void Write(UInt32 v);
        void Write(UInt64 v);
        void Write(float v);
        void Write(double v);
        void Write(Vector2 v);
        void Write(Vector3 v);
        void Write(Vector4 v);
        void Write(Quaternion v);
        void Write(Matrix v);
        void Write(Guid v);
    }

    public abstract class AuxIReader : IReader
    {
        public virtual EIOType IOType
        {
            get;
        }
        public virtual void OnReadError()
        {

        }
        public virtual void Read(out string v)
        {
            unsafe
            {
                unsafe
                {
                    UInt16 len = 0;
                    ReadPtr(&len, sizeof(UInt16));
                    if (len == 0)
                    {
                        v = "";
                        return;
                    }
                    var str = new System.Char[len];
                    fixed (System.Char* pChar = &str[0])
                    {
                        ReadPtr(pChar, sizeof(System.Char) * len);
                        v = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                    }
                }
            }
        }
        public virtual void Read(out byte[] v)
        {
            unsafe
            {
                UInt16 len;
                ReadPtr(&len, sizeof(UInt16));
                v = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &v[0])
                    {
                        ReadPtr(p, len);
                    }
                }
            }
        }
        public virtual void Read(out ChunkReader v)
        {
            v = new IO.Serializer.ChunkReader();
            unsafe
            {
                UInt16 len = 0;
                ReadPtr(&len, sizeof(UInt16));
                byte[] data = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &data[0])
                    {
                        ReadPtr(p, len);
                    }
                }
                v.SetBuffer(data, 0);
            }
        }
        public virtual void Read(out Support.BitSet v)
        {
            unsafe
            {
                int bitCount = 0;
                ReadPtr(&bitCount, sizeof(int));
                v = new Support.BitSet();
                int byteCount = 0;
                ReadPtr(&byteCount, sizeof(int));
                byte[] bitData = new byte[byteCount];
                fixed (byte* p = &bitData[0])
                {
                    ReadPtr(p, sizeof(System.Byte) * byteCount);
                }
                v.Init((UInt32)bitCount, bitData);
            }
        }
        public virtual void Read<T>(out T v) where T : unmanaged
        {
            unsafe
            {
                fixed(T* p = &v)
                {
                    ReadPtr(p, sizeof(T));
                }
            }
        }
        public virtual void Read(ISerializer v)
        {
            v.ReadObject(this);
        }
        public T Read<T>() where T : ISerializer, new()
        {
            T t = new T();
            Read(t);
            return t;
        }
        public List<T> ReadList<T>() where T : ISerializer, new()
        {
            List<T> result = new List<T>();
            var sr = TypeDescGenerator.Instance.GetSerializer(typeof(ISerializer));
            sr.ReadValueList(result, this);
            return result;
        }

        public void ReadList<T>(List<T> v) where T : ISerializer, new()
        {
            var sr = TypeDescGenerator.Instance.GetSerializer(typeof(ISerializer));
            sr.ReadValueList(v, this);
        }

        public void ReadPODObject(System.Type ValueType, out object Value)
        {
            var sr = TypeDescGenerator.Instance.GetSerializer(ValueType);
            Value = sr.ReadValue(this);
        }

        public abstract unsafe void ReadPtr(void* p, int length);

        public void Read(out bool v1)
        {
            unsafe
            {
                sbyte v = 0;
                ReadPtr(&v, sizeof(sbyte));
                v1 = v == 1 ? true : false;
            }
        }
        public void Read(out sbyte v)
        {
            unsafe
            {
                fixed (sbyte* p = &v)
                {
                    ReadPtr(p, sizeof(sbyte));
                }
            }
        }
        public void Read(out Int16 v)
        {
            unsafe
            {
                fixed (Int16* p = &v)
                {
                    ReadPtr(p, sizeof(Int16));
                }
            }
        }
        public void Read(out Int32 v)
        {
            unsafe
            {
                fixed (Int32* p = &v)
                {
                    ReadPtr(p, sizeof(Int32));
                }
            }
        }
        public void Read(out Int64 v)
        {
            unsafe
            {
                fixed (Int64* p = &v)
                {
                    ReadPtr(p, sizeof(Int64));
                }
            }
        }
        public void Read(out byte v)
        {
            unsafe
            {
                fixed (byte* p = &v)
                {
                    ReadPtr(p, sizeof(byte));
                }
            }
        }
        public void Read(out UInt16 v)
        {
            unsafe
            {
                fixed (UInt16* p = &v)
                {
                    ReadPtr(p, sizeof(UInt16));
                }
            }
        }
        public void Read(out UInt32 v)
        {
            unsafe
            {
                fixed (UInt32* p = &v)
                {
                    ReadPtr(p, sizeof(UInt32));
                }
            }
        }
        public void Read(out UInt64 v)
        {
            unsafe
            {
                fixed (UInt64* p = &v)
                {
                    ReadPtr(p, sizeof(UInt64));
                }
            }
        }
        public void Read(out float v)
        {
            unsafe
            {
                fixed (float* p = &v)
                {
                    ReadPtr(p, sizeof(float));
                }
            }
        }
        public void Read(out double v)
        {
            unsafe
            {
                fixed (double* p = &v)
                {
                    ReadPtr(p, sizeof(double));
                }
            }
        }
        public void Read(out Vector2 v)
        {
            unsafe
            {
                fixed (Vector2* p = &v)
                {
                    ReadPtr(p, sizeof(Vector2));
                }
            }
        }
        public void Read(out Vector3 v)
        {
            unsafe
            {
                fixed (Vector3* p = &v)
                {
                    ReadPtr(p, sizeof(Vector3));
                }
            }
        }
        public void Read(out Vector4 v)
        {
            unsafe
            {
                fixed (Vector4* p = &v)
                {
                    ReadPtr(p, sizeof(Vector4));
                }
            }
        }
        public void Read(out Quaternion v)
        {
            unsafe
            {
                fixed (Quaternion* p = &v)
                {
                    ReadPtr(p, sizeof(Quaternion));
                }
            }
        }
        public void Read(out Matrix v)
        {
            unsafe
            {
                fixed (Matrix* p = &v)
                {
                    ReadPtr(p, sizeof(Matrix));
                }
            }
        }
        public void Read(out Guid v)
        {
            unsafe
            {
                fixed (Guid* p = &v)
                {
                    ReadPtr(p, sizeof(Guid));
                }
            }
        }
    }
    public abstract class AuxIWriter : IWriter
    {
        public abstract EIOType IOType
        {
            get;
        }
        public void WritePODObject(System.Object obj)
        {
            var tDesc = TypeDescGenerator.Instance.GetSerializer(obj.GetType());
            if (tDesc == null)
                return;
            tDesc.WriteValue(obj, this);
        }
        public void Write<T>(List<T> v) where T : ISerializer, new()
        {
            var sr = TypeDescGenerator.Instance.GetSerializer(typeof(ISerializer));
            sr.WriteValueList(v, this);
        }
        public void WriteList<T>(List<T> v) where T : ISerializer, new()
        {
            var sr = TypeDescGenerator.Instance.GetSerializer(typeof(ISerializer));
            sr.WriteValueList(v, this);
        }

        public abstract unsafe void WritePtr(void* p, int length);

        public virtual void Write(string v)
        {
            unsafe
            {
                var len = (UInt16)v.Length;
                WritePtr(&len, sizeof(UInt16));
                if (len > 0)
                {
                    fixed (System.Char* pPtr = v)
                    {
                        WritePtr(pPtr, sizeof(System.Char) * len);
                    }
                }
            }
        }
        public virtual void Write(byte[] v)
        {
            unsafe
            {
                var len = (UInt16)v.Length;
                WritePtr(&len, sizeof(UInt16));
                fixed (byte* p = &v[0])
                {
                    WritePtr(p, len);
                }
            }
        }
        public virtual void Write(ISerializer v)
        {
            v.WriteObject(this);
        }
        public virtual void Write(ChunkWriter v)
        {
            this.Write(v.CurPtr());
            unsafe
            {
                fixed (byte* p = &v.Ptr[0])
                {
                    this.WritePtr(p, v.CurPtr());
                }
            }
        }
        public virtual void Write(Support.BitSet v)
        {
            unsafe
            {
                var bitCount = v.BitCount;
                WritePtr(&bitCount, sizeof(int));
                byte[] bitData = v.Data;
                int byteCount = bitData.Length;
                WritePtr(&byteCount, sizeof(int));
                fixed (byte* p = &bitData[0])
                {
                    WritePtr(p, sizeof(System.Byte) * byteCount);
                }
            }
        }
        
        public void Write(bool v)
        {
            unsafe
            {
                sbyte v1 = v ? (sbyte)1 : (sbyte)0;
                WritePtr(&v1, sizeof(sbyte));
            }
        }
        public void Write(sbyte v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(sbyte));
            }
        }
        public void Write(Int16 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int16));
            }
        }
        public void Write(Int32 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int32));
            }
        }
        public void Write(Int64 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int64));
            }
        }
        public void Write(byte v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(byte));
            }
        }
        public void Write(UInt16 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt16));
            }
        }
        public void Write(UInt32 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt32));
            }
        }
        public void Write(UInt64 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt64));
            }
        }
        public void Write(float v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(float));
            }
        }
        public void Write(double v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(double));
            }
        }
        public void Write(Vector2 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector2));
            }
        }
        public void Write(Vector3 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector3));
            }
        }
        public void Write(Vector4 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector4));
            }
        }
        public void Write(Quaternion v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Quaternion));
            }
        }
        public void Write(Matrix v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Matrix));
            }
        }
        public void Write(Guid v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Guid));
            }
        }
    }

    public abstract class FieldSerializer
    {
        public abstract object ReadValue(IReader pkg);
        public abstract void WriteValue(object obj, IWriter pkg);

        #region static helper
        public static string GetValueTypeSerializeCode(Rtti.MemberDesc p)
        {
            //var sr = TypeSerializer.Instance.GetSerializer(p.MemberType);
            //if (sr == null)
            //    return "";
            var klass = p.DeclaringType.GetHashCode() + "_" + p.Name;
            string code = $"//{p.DeclaringType.FullName}.{p.Name}\n";
            //code = $"public class Field_{klass} : {sr.GetType().FullName}\n";
            code += $"public class Field_{klass} : {typeof(FieldSerializer).FullName}\n";
            code += "{\n";
            code += GetValueTypeReadCode(p);
            code += GetValueTypeWriteCode(p);
            //code += GetValueTypeListReadCode(p);
            //code += GetValueTypeListWriteCode(p);
            code += "}\n";
            return code;
        }
        private static string GetValueTypeReadCode(Rtti.MemberDesc p)
        {
            var klass = p.DeclaringType.FullName;
            var property = p.MemberType.FullName;
            string code = $" public override void ReadValue(object host, Rtti.MemberDesc p, {typeof(IReader).FullName} pkg)\n";
            code += "   {\n";
            code += "       unsafe\n";
            code += "       {\n";
            code += $"          {property} v;\n";
            code += $"          pkg.ReadPtr(&v, sizeof({property}));\n";
            if (p.CanWrite)
            {
                code += $"          (({klass})host).{p.Name} = v;\n";
            }
            code += "       }\n";
            code += "   }\n";
            code += $"   public override object ReadValue({typeof(IReader).FullName} pkg)" + "{ return null;}\n";
            code += $"   public override void ReadValueList(System.Collections.IList lst, {typeof(IReader).FullName} pkg)" + "{}\n";
            return code;
        }
        private static string GetValueTypeWriteCode(Rtti.MemberDesc p)
        {
            var klass = p.DeclaringType.FullName;
            var property = p.MemberType.FullName;
            string code = $" public override void WriteValue(object host, Rtti.MemberDesc p, {typeof(IWriter).FullName} pkg)\n";
            code += "   {\n";
            code += "       unsafe\n";
            code += "       {\n";
            code += $"          var v = (({klass})host).{p.Name};\n";
            code += $"          pkg.WritePtr(&v, sizeof({property}));\n";
            code += "       }\n";
            code += "   }\n";
            code += $"   public override void WriteValue(object obj, {typeof(IWriter).FullName} pkg)" + "{ }\n";
            code += $"   public override void WriteValueList(System.Collections.IList lst, {typeof(IWriter).FullName} pkg)" + "{ }\n";
            return code;
        }
        private static string GetValueTypeListReadCode(Rtti.MemberDesc p)
        {
            var klass = p.DeclaringType.FullName;
            var property = p.MemberType.GenericTypeArguments[0];
            string code = $" public override void ReadValueList(System.Collections.IList obj, {typeof(IReader).FullName} pkg)\n";
            code += "   {\n";
            code += $"       var lst = (List<{property}>)(obj);\n";
            code += "       UInt16 count;\n";
            code += "       unsafe\n";
            code += "       {\n";
            code += "           pkg.ReadPtr(&count, sizeof(UInt16));\n";
            code += "           for (UInt16 i = 0; i < count; i++)\n";
            code += "           {\n";
            code += $"              {property} v;\n";
            code += $"              pkg.ReadPtr(&v, sizeof({property}));\n";
            code += "               lst.Add(v);\n";
            code += "           }\n";
            code += "       }\n";
            code += "   }\n";
            return code;
        }
        private static string GetValueTypeListWriteCode(Rtti.MemberDesc p)
        {
            var klass = p.DeclaringType.FullName;
            var property = p.MemberType.GenericTypeArguments[0];
            string code = $" public override void WriteValueList(System.Collections.IList obj, {typeof(IWriter).FullName} pkg)\n";
            code += "   {\n";
            code += $"       var lst = (List<{property}>)(obj);\n";
            code += "       UInt16 count;\n";
            code += "       if (lst != null)\n";
            code += "           count = (UInt16)lst.Count;\n";
            code += "       unsafe\n";
            code += "       {\n";
            code += "           pkg.WritePtr(&count, sizeof(UInt16));\n";
            code += "           for (UInt16 i = 0; i < count; i++)\n";
            code += "           {\n";
            code += $"              var v = lst[i];\n";
            code += $"              pkg.WritePtr(&v, sizeof({property}));\n";
            code += "           }\n";
            code += "       }\n";
            code += "   }\n";
            return code;
        }
        #endregion
        public virtual void ReadValue(object host, Rtti.MemberDesc p, IReader pkg)
        {
            //如果是ValueType，这里要做一次装箱操作，因为SetValue只能是object
            //如果最后发现这里是性能瓶颈，那就要做代码生成操作，将SetValue转换成  ((p.DeclaringType.FullName)host).(p.Name) = (p.MemberType.FullName)obj;

            var obj = ReadValue(pkg);
            if (p.CanWrite)
            {
                try
                {
                    p.SetValue(host, obj);
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                //if(obj == null)
                //    p.SetValue(host, obj);
                //else if(p.MemberType == obj.GetType() || obj.GetType().IsSubclassOf(p.MemberType))
                //    p.SetValue(host, obj);
            }
        }
        public virtual void WriteValue(object host, Rtti.MemberDesc p, IWriter pkg)
        {
            var obj = p.GetValue(host);
            WriteValue(obj, pkg);
        }

        public abstract void ReadValueList(System.Collections.IList lst, IReader pkg);
        public abstract void WriteValueList(System.Collections.IList lst, IWriter pkg);

        public virtual void ReadValueList(object host, Rtti.MemberDesc p, IReader pkg)
        {
            var lst = p.GetValue(host) as System.Collections.IList;
            if (lst == null)
                lst = System.Activator.CreateInstance(p.MemberType) as System.Collections.IList;
            else
                lst.Clear();
            ReadValueList(lst, pkg);
            if (p.CanWrite)
                p.SetValue(host, lst);
        }
        public virtual void WriteValueList(object host, Rtti.MemberDesc p, IWriter pkg)
        {
            var lst = p.GetValue(host) as System.Collections.IList;
            WriteValueList(lst, pkg);
        }

        #region XML
        public virtual string ObjectToString(Rtti.MemberDesc p, object o)
        {
            return o.ToString();
        }
        public abstract object ObjectFromString(Rtti.MemberDesc p, string str);
        public virtual IXmlElement WriteValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var obj = p.GetValue(host);
            if (obj != null)
                return node.AddAttrib(p.Name, ObjectToString(p, obj));
            else
                return node.AddAttrib(p.Name, "null");
        }
        public virtual IXmlElement WriteValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var lst = p.GetValue(host) as System.Collections.IList;

            var lstNode = node.AddNode(p.Name, p.MemberType.FullName, node.mHolder);
            UInt16 count = 0;
            if (lst != null)
                count = (UInt16)lst.Count;
            for (UInt16 i = 0; i < count; i++)
            {
                var v = lst[i];
                if (v == null)
                    continue;
                lstNode.AddAttrib($"{i}", ObjectToString(p, v));
            }
            return lstNode;
        }
        public virtual void ReadValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var attr = node.FindAttrib(p.Name);
            if (attr == null)
                return;

            var obj = ObjectFromString(p, attr.Value);
            if (p.CanWrite)
            {
                p.SetValue(host, obj);
            }
        }
        public virtual void ReadValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var lst = p.GetValue(host) as System.Collections.IList;
            if (lst == null)
            {
                lst = System.Activator.CreateInstance(p.MemberType) as System.Collections.IList;
            }
            lst.Clear();
            var lstNode = node.FindNode(p.Name);
            if(lstNode != null)
            {
                var attrs = lstNode.GetAttribs();
                //p.MemberType.GetGenericArguments()[0];
                foreach (var i in attrs)
                {
                    var obj = ObjectFromString(p, i.Value);
                    lst.Add(obj);
                }
                if (p.CanWrite)
                    p.SetValue(host, lst);
            }
        }
        #endregion
    }
    public class ValueObject
    {

    }
    public class ValueObjectSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(ValueObject);
        public override void ReadValue(object host, Rtti.MemberDesc p, IReader pkg)
        {
            var nowSize = System.Runtime.InteropServices.Marshal.SizeOf(p.MemberType);
            UInt16 size;
            unsafe
            {
                pkg.Read(out size);
                if(nowSize != size)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Type {p.MemberType.FullName} is changed, Some files need save");
                }
                var data = stackalloc byte[size];
                pkg.ReadPtr(data, size);
                var vobj = System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)data, p.MemberType);
                if (p.CanWrite)
                {
                    p.SetValue(host, vobj);
                }
            }
        }
        public override void WriteValue(object host, Rtti.MemberDesc p, IWriter pkg)
        {
            var obj = p.GetValue(host);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(p.MemberType);
            unsafe
            {
                pkg.Write((UInt16)size);
                var data = stackalloc byte[size];
                System.Runtime.InteropServices.Marshal.StructureToPtr(obj, (IntPtr)data, true);
                pkg.WritePtr(data, size);
            }
        }
        public override object ReadValue(IReader pkg)
        {
            return null;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            
        }
        public override void ReadValueList(object host, Rtti.MemberDesc p, IReader pkg)
        {
            var lst = p.GetValue(host) as System.Collections.IList;
            if (lst == null)
            {
                lst = System.Activator.CreateInstance(p.MemberType) as System.Collections.IList;
            }
            var typeElem = p.MemberType.GenericTypeArguments[0];
            var size = System.Runtime.InteropServices.Marshal.SizeOf(typeElem);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = System.Activator.CreateInstance(typeElem);
                    var data = stackalloc byte[size];
                    pkg.ReadPtr(data, size);
                    System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)data, v);
                    lst.Add(v);
                }
            }
            if (p.CanWrite)
                p.SetValue(host, lst);
        }
        public override void WriteValueList(object host, Rtti.MemberDesc p, IWriter pkg)
        {
            var typeElem = p.MemberType.GenericTypeArguments[0];
            var size = System.Runtime.InteropServices.Marshal.SizeOf(typeElem);
            var lst = p.GetValue(host) as System.Collections.IList;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var data = stackalloc byte[size];
                    var v = lst[i];
                    System.Runtime.InteropServices.Marshal.StructureToPtr(v, (IntPtr)data, true);
                    pkg.WritePtr(data, sizeof(sbyte));
                }
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            
        }
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var size = System.Runtime.InteropServices.Marshal.SizeOf(o.GetType());
            unsafe
            {
                var data = new byte[size];
                fixed(byte* bytePtr = &data[0])
                {
                    System.Runtime.InteropServices.Marshal.StructureToPtr(o, (IntPtr)bytePtr, true);
                }
                var retStr = System.Convert.ToBase64String(data);
                return retStr;
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return System.Activator.CreateInstance(p.MemberType);

                unsafe
                {
                    var data = System.Convert.FromBase64String(str);
                    fixed (byte* bytePtr = &data[0])
                    {
                        var obj = System.Runtime.InteropServices.Marshal.PtrToStructure((IntPtr)bytePtr, p.MemberType);
                        return obj;
                    }
                }
            }
            catch(System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Load Item Exception: ObjectFromString", e.ToString());
            }

            return null;
        }
        //public override IXmlElement WriteValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        //{
        //    if(p.MemberType.IsEnum)
        //    {
        //        return base.WriteValueXML(host, p, node);
        //    }
        //    var obj = p.GetValue(host);
        //    var cnode =  node.AddNode(p.Name, Rtti.RttiHelper.GetTypeSaveString(p.MemberType), node.mHolder);
        //    var fields = p.MemberType.GetFields();
        //    foreach(var i in fields)
        //    {
        //        var subObj = i.GetValue(obj);
        //        var attrs = i.FieldType.GetCustomAttributes(typeof(IO.Serializer.IncludeFieldAttribute), true);
        //        if (attrs != null && attrs.Length != 0)
        //        {
        //            WriteValueXML(subObj, new Rtti.FieldMemberDesc(i), cnode);
        //        }
        //        else
        //        {
        //            cnode.AddAttrib(i.Name, subObj.ToString());
        //        }
        //    }
        //    return cnode;
        //}
        //public override IXmlElement WriteValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        //{
        //    if (p.MemberType.IsEnum)
        //    {
        //        return base.WriteValueListXML(host, p, node);
        //    }
        //    return null;
        //}
        //public override void ReadValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        //{
        //    if (p.MemberType.IsEnum)
        //    {
        //        base.ReadValueXML(host, p, node);
        //        return;
        //    }
        //    var cnode = node.FindNode(p.Name);
        //    if (cnode == null)
        //        return;
        //    var obj = System.Activator.CreateInstance(p.MemberType);
        //    var attrs = cnode.GetAttribs();
        //    foreach(var i in attrs)
        //    {
        //        var finfo = p.MemberType.GetField(i.Name);
        //        if (finfo == null)
        //            continue;
        //        finfo.SetValue(obj, XmlString2Object(finfo.FieldType, i.Value));
        //    }
        //    var nodes = cnode.GetNodes();
        //    foreach (var i in nodes)
        //    {
        //        var finfo = p.MemberType.GetField(i.Name);
        //        var subType = Rtti.RttiHelper.GetTypeFromSaveString(i.Value);
        //        if (subType == null)
        //            continue;
                
        //        var subObj = System.Activator.CreateInstance(subType);
        //        ReadValueXML(obj, new Rtti.FieldMemberDesc(finfo), i);
        //        finfo.SetValue(obj, subObj);
        //    }
        //}
        //public override void ReadValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        //{
        //    if (p.MemberType.IsEnum)
        //    {
        //        base.ReadValueListXML(host, p, node);
        //        return;
        //    }
        //}
        public static object XmlString2Object(System.Type type, string str)
        {
            if (type == typeof(sbyte))
            {
                return System.Convert.ToSByte(str);
            }
            else if (type == typeof(Int16))
            {
                return System.Convert.ToInt16(str);
            }
            else if (type == typeof(Int32))
            {
                return System.Convert.ToInt32(str);
            }
            else if (type == typeof(Int64))
            {
                return System.Convert.ToInt64(str);
            }
            else if (type == typeof(byte))
            {
                return System.Convert.ToByte(str);
            }
            else if (type == typeof(UInt16))
            {
                return System.Convert.ToUInt16(str);
            }
            else if (type == typeof(UInt32))
            {
                return System.Convert.ToUInt32(str);
            }
            else if (type == typeof(UInt64))
            {
                return System.Convert.ToUInt64(str);
            }
            else if (type == typeof(float))
            {
                return System.Convert.ToSingle(str);
            }
            else if (type == typeof(double))
            {
                return System.Convert.ToDouble(str);
            }
            else if (type == typeof(string))
            {
                return str;
            }
            return str;
        }
    }
    public class ObjectSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Serializer);
        
        public override object ReadValue(IReader pkg)
        {
            unsafe
            {
                if (pkg.IOType != EIOType.Network)
                {
                    string cname;
                    pkg.Read(out cname);
                    if (cname == "None")
                        return null;
                    UInt32 metaHash;
                    pkg.Read(out metaHash);

                    MetaData metaData = null;
                    bool isRedirection;
                    var type = Rtti.RttiHelper.GetTypeFromSaveString(cname, out isRedirection);
                    if (type == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}) is missing");
                        return null;
                    }
                    if (isRedirection)
                    {
                        var metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(cname);
                        if(metaClass == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}) can't find MetaClass");
                            return null;
                        }
                        metaData = metaClass.FindMetaData(metaHash);
                    }
                    else
                    {
                        var metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(type);
                        if (metaClass == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}) can't find MetaClass");
                            return null;
                        }
                        metaData = metaClass.FindMetaData(metaHash);
                    }
                    var obj = System.Activator.CreateInstance(type) as ISerializer;
                    if (obj == null)
                    {
                        //严重错误
                        pkg.OnReadError();
                        return null;
                    }
                    if (metaData == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}:{metaHash}) can't find MetaData");
                        return null;
                    }
                    obj.ReadObject(pkg, metaData);
                    return obj;
                }
                else
                {
                    UInt32 hash;
                    pkg.ReadPtr(&hash, sizeof(UInt32));
                    if (hash == 0)//空指针
                        return null;
                    var tDesc = TypeDescGenerator.Instance.GetTypeDesc(hash);
                    if (tDesc == null)
                    {
                        //tDesc = TypeSerializer.Instance.FindTypeDesc(typeof(CSUtility.Data.AccountInfo).FullName);
                        System.Diagnostics.Debug.WriteLine($"GetTypeDecs failed,HashCode = {tDesc.HashCode}");
                        //严重错误
                        pkg.OnReadError();
                        return null;
                    }
                    var obj = System.Activator.CreateInstance(tDesc.ClassType) as ISerializer;
                    if (obj == null)
                    {
                        //严重错误
                        pkg.OnReadError();
                        return null;
                    }
                    obj.ReadObject(pkg);
                    return obj;
                }
            }
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            unsafe
            {
                var so = (ISerializer)obj;

                if (pkg.IOType != EIOType.Network)
                {
                    if(so == null)
                    {
                        pkg.Write("None");
                        return;
                    }
                    else
                    {
                        var metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(so.GetType());
                        var cname = Rtti.RttiHelper.GetTypeSaveString(so.GetType());
                        if (metaClass == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"FieldSerializer can't find Metaclass {cname}");
                            //pkg.WritePtr(&hash, sizeof(UInt32));
                            pkg.Write("None");
                            return;
                        }

                        pkg.Write(cname);
                        pkg.Write(metaClass.CurrentVersion.MetaHash);
                        so.WriteObject(pkg, metaClass.CurrentVersion);
                    }
                }
                else
                {
                    if(so == null)
                    {
                        UInt32 hash = 0;
                        pkg.WritePtr(&hash, sizeof(UInt32));
                    }
                    else
                    {
                        UInt32 hash = 0;
                        var tDesc = TypeDescGenerator.Instance.GetTypeDesc(so.GetType());
                        if (tDesc == null)
                        {
                            //严重错误
                            pkg.WritePtr(&hash, sizeof(UInt32));
                            return;
                        }
                        hash = tDesc.HashCode;
                        pkg.WritePtr(&hash, sizeof(UInt32));
                        so.WriteObject(pkg);
                    }
                }
            }
        }
        
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = (UInt16)lst.Count;
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    if (pkg.IOType != EIOType.Network)
                    {
                        string cname;
                        pkg.Read(out cname);
                        if (string.IsNullOrEmpty(cname))
                        {
                            lst.Add(null);
                            continue;
                        }
                        UInt32 metaHash;
                        pkg.Read(out metaHash);
                        var type = Rtti.RttiHelper.GetTypeFromSaveString(cname);
                        if(type==null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}) is missing");
                            continue;
                        }
                        var metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(type);
                        if(metaClass==null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}) can't find MetaClass");
                            continue;
                        }
                        var metaData = metaClass.FindMetaData(metaHash);
                        if(metaData==null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Serializer", $"Class({cname}:{metaHash}) can't find MetaData");
                            continue;
                        }
                        var elem = System.Activator.CreateInstance(type) as ISerializer;
                        elem.ReadObject(pkg, metaData);
                        lst.Add(elem);
                    }
                    else
                    {
                        UInt32 hash = 0;
                        pkg.ReadPtr(&hash, sizeof(UInt32));
                        if (hash == 0)
                        {
                            lst.Add(null);
                            continue;
                        }
                        var tDesc = TypeDescGenerator.Instance.GetTypeDesc(hash);
                        if (tDesc == null)
                        {
                            //这是真出大事了
                            pkg.OnReadError();
                            continue;
                        }
                        var elem = System.Activator.CreateInstance(tDesc.ClassType) as ISerializer;
                        elem.ReadObject(pkg);
                        lst.Add(elem);
                    }
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if(lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                if(count > 0)
                {
                    foreach (ISerializer i in lst)
                    {
                        if (pkg.IOType != EIOType.Network)
                        {
                            if (i == null)
                            {
                                string cname = "";
                                pkg.Write(cname);
                            }
                            else
                            {
                                var metaData = CEngine.Instance.MetaClassManager.FindMetaClass(i.GetType()).CurrentVersion;
                                string cname = Rtti.RttiHelper.GetTypeSaveString(i.GetType());
                                pkg.Write(cname);
                                pkg.Write(metaData.MetaHash);
                                i.WriteObject(pkg, metaData);
                            }
                        }
                        else
                        {
                            if (i == null)
                            {
                                UInt32 hash = 0;
                                pkg.Write(hash);
                            }
                            else
                            {
                                var tDesc = TypeDescGenerator.Instance.GetTypeDesc(i.GetType());
                                var Hash = tDesc.HashCode;
                                pkg.Write(Hash);
                                i.WriteObject(pkg);
                            }
                        }
                    }
                }
            }
        }

        #region XML
        public override IXmlElement WriteValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var obj = p.GetValue(host) as ISerializer;
            if (obj == null)
                return null;
            var typeStr = Rtti.RttiHelper.GetTypeSaveString(p.MemberType);
            var cnode = node.AddNode(p.Name, "", node.mHolder);
            cnode.AddAttrib("__TypeStr__", typeStr);
            obj.WriteObjectXML(cnode);
            return cnode;
        }
        public override IXmlElement WriteValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var lst = p.GetValue(host) as System.Collections.IList;

            var typeStr = Rtti.RttiHelper.GetTypeSaveString(p.MemberType);
            var lstNode = node.AddNode(p.Name, "", node.mHolder);
            lstNode.AddAttrib("__TypeStr__", typeStr);
            UInt16 count = 0;
            if (lst != null)
                count = (UInt16)lst.Count;
            for (UInt16 i = 0; i < count; i++)
            {
                var v = lst[i] as ISerializer;
                if (v == null)
                    continue;
                var valTypeStr = Rtti.RttiHelper.GetTypeSaveString(v.GetType());
                var cnode = lstNode.AddNode($"{i}", valTypeStr, lstNode.mHolder);
                cnode.AddAttrib("__TypeStr__", valTypeStr);
                v.WriteObjectXML(cnode);
            }
            return lstNode;
        }
        public override void ReadValueXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var cnode = node.FindNode(p.Name);
            if (cnode == null)
                return;

            Type type;
            var att = cnode.FindAttrib("__TypeStr__");
            if (att == null)
                type = Rtti.RttiHelper.GetTypeFromSaveString(cnode.Value);
            else
                type = Rtti.RttiHelper.GetTypeFromSaveString(att.Value);
            if (type == null)
                return;
            var obj = System.Activator.CreateInstance(type) as ISerializer;
            if (obj == null)
                return;
            if (p.CanWrite)
            {
                obj.ReadObjectXML(cnode);
                p.SetValue(host, obj);
            }
        }
        public override void ReadValueListXML(object host, Rtti.MemberDesc p, XmlNode node)
        {
            var lst = p.GetValue(host) as System.Collections.IList;
            if (lst == null)
            {
                lst = System.Activator.CreateInstance(p.MemberType) as System.Collections.IList;
            }
            lst.Clear();
            var lstNode = node.FindNode(p.Name);
            if (lstNode == null)
                return;
            var subNodes = lstNode.GetNodes();
            foreach (var i in subNodes)
            {
                Type type;
                var att = i.FindAttrib("__TypeStr__");
                if (att == null)
                    type = Rtti.RttiHelper.GetTypeFromSaveString(i.Value);
                else
                    type = Rtti.RttiHelper.GetTypeFromSaveString(att.Value);
                if (type == null)
                    return;
                var obj = System.Activator.CreateInstance(type) as ISerializer;
                if (obj == null)
                    return;
                obj.ReadObjectXML(i);
                lst.Add(obj);
            }
            if (p.CanWrite)
                p.SetValue(host, lst);
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var obj = System.Activator.CreateInstance(SerializeType);
            return obj;
        }
        #endregion
    }

    #region PODType
    public class ChunkSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(MemChunk);
        public override object ReadValue(IReader pkg)
        {
            IReader pkg_reader = pkg;
            if (pkg_reader == null)
                return null;
            var v = new MemChunk();
            pkg_reader.Read(out v.Reader);
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var pkg_writer = pkg;
            var v = (MemChunk)obj;
            if (v.Writer == null)
            {
                UInt16 len = 0;
                pkg_writer.Write(len);
            }
            else
            {
                pkg_writer.Write(v.Writer);
            }
        }
        public override void ReadValueList(System.Collections.IList lst, IReader pkg)
        {
            
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var obj = System.Activator.CreateInstance(SerializeType);
            return obj;
        }
    }
    public class StringSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(string);
        public override object ReadValue(IReader pkg)
        {
            string v;
            unsafe
            {
                UInt16 len;
                pkg.ReadPtr(&len, sizeof(UInt16));
                if (len == 0)
                    return "";
                if (pkg.IOType == EIOType.Network)
                {
                    byte* pChar = stackalloc byte[len];
                    //var data = new byte[len];
                    //fixed (byte* pChar = &data[0])
                    {
                        pkg.ReadPtr(pChar, sizeof(byte) * len);
                        v = System.Text.ASCIIEncoding.ASCII.GetString(pChar, len);
                    }
                }
                else
                {
                    System.Char* pChar = stackalloc System.Char[len+1];
                    //var str = new System.Char[len];
                    //fixed (System.Char* pChar = &str[0])
                    {
                        pkg.ReadPtr(pChar, sizeof(System.Char) * len);
                        v = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                    }
                }
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (string)obj;
            if (v == null)
                v = "";
            unsafe
            {
                if (pkg.IOType == EIOType.Network)
                {
                    var data = System.Text.ASCIIEncoding.ASCII.GetBytes(v);
                    var len = (UInt16)data.Length;
                    pkg.WritePtr(&len, sizeof(UInt16));
                    if (len > 0)
                    {
                        fixed (byte* pPtr = &data[0])
                        {
                            pkg.WritePtr(pPtr, sizeof(byte) * len);
                        }
                    }
                }
                else
                {
                    var len = (UInt16)v.Length;
                    pkg.WritePtr(&len, sizeof(UInt16));
                    if (len > 0)
                    {
                        fixed (System.Char* pPtr = v)
                        {
                            pkg.WritePtr(pPtr, sizeof(System.Char) * len);
                        }
                    }
                }
            }
        }
        public override void ReadValueList(System.Collections.IList lst, IReader pkg)
        {
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    string v;
                    UInt16 len;
                    pkg.ReadPtr(&len, sizeof(UInt16));
                    if (len == 0)
                    {
                        lst.Add("");
                        continue;
                    }
                    if (pkg.IOType == EIOType.Network)
                    {
                        byte* pChar = stackalloc byte[len + 1];
                        //var data = new byte[len];
                        //fixed (byte* pChar = &data[0])
                        {
                            pkg.ReadPtr(pChar, sizeof(byte) * len);
                        }
                        v = System.Text.ASCIIEncoding.ASCII.GetString(pChar, len);
                    }
                    else
                    {
                        System.Char* pChar = stackalloc System.Char[len + 1];
                        //var str = new System.Char[len];
                        //fixed (System.Char* pChar = &str[0])
                        {
                            pkg.ReadPtr(pChar, sizeof(System.Char) * len);
                            v = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                        }
                    }
                        
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<string>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    if (pkg.IOType == EIOType.Network)
                    {
                        if(v==null)
                        {
                            UInt16 nl = 0;
                            pkg.WritePtr(&nl, sizeof(UInt16));
                            continue;
                        }    
                        var data = System.Text.ASCIIEncoding.ASCII.GetBytes(v);
                        var len = (UInt16)data.Length;
                        pkg.WritePtr(&len, sizeof(UInt16));
                        if (len > 0)
                        {
                            fixed (byte* pPtr = &data[0])
                            {
                                pkg.WritePtr(pPtr, sizeof(byte) * len);
                            }
                        }
                    }
                    else
                    {
                        var len = (UInt16)v.Length;
                        pkg.WritePtr(&len, sizeof(UInt16));
                        if (len > 0)
                        {
                            fixed (System.Char* pPtr = v)
                            {
                                pkg.WritePtr(pPtr, sizeof(System.Char) * len);
                            }
                        }
                    }
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return str;
        }
    }
    public class ByteArraySerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(byte[]);
        public override object ReadValue(IReader pkg)
        {
            byte[] v;
            unsafe
            {
                UInt16 len;
                pkg.ReadPtr(&len, sizeof(UInt16));
                v = new byte[len];
                if (len > 0)
                {
                    fixed (byte* ptr = &v[0])
                    {
                        pkg.ReadPtr(ptr, sizeof(byte) * len);
                    }
                }
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (byte[])obj;
            unsafe
            {
                var len = (UInt16)v.Length;
                pkg.WritePtr(&len, sizeof(UInt16));
                if (len > 0)
                {
                    fixed (byte* ptr = &v[0])
                    {
                        pkg.WritePtr(ptr, sizeof(byte) * len);
                    }
                }
            }
        }
        public override void ReadValueList(System.Collections.IList lst, IReader pkg)
        {
            
        }
        public override void WriteValueList(System.Collections.IList lst, IWriter pkg)
        {
            
        }
        public override string ObjectToString(Rtti.MemberDesc p, object obj)
        {
            return "";//Base64
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var obj = System.Activator.CreateInstance(SerializeType);
            return obj;
        }
    }
    public class BoolSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(bool);
        public override object ReadValue(IReader pkg)
        {
            sbyte v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(sbyte));
            }
            return v==0?false:true;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (sbyte)((bool)obj?1:0);
            unsafe
            {
                pkg.WritePtr(&v, sizeof(sbyte));
            }
        }
        public override void ReadValueList(System.Collections.IList lst, IReader pkg)
        {
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    sbyte v;
                    pkg.ReadPtr(&v, sizeof(sbyte));
                    lst.Add(v==0?false:true);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList objList, IWriter pkg)
        {
            var lst = (List<bool>)objList;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = (sbyte)(lst[i]?1:0);
                    pkg.WritePtr(&v, sizeof(sbyte));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToBoolean(str);
        }
    }

    public abstract class ValueSerializerHelper<T> : FieldSerializer where T : unmanaged
    {
        public override object ReadValue(IReader pkg)
        {
            T v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(T));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (T)obj;
            unsafe
            {
                pkg.WritePtr(&v, sizeof(T));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<T>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    T v;
                    pkg.ReadPtr(&v, sizeof(T));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<T>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(T));
                }
            }
        }
    }

    public class SingleSerializer : ValueSerializerHelper<Single>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Single);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToSingle(str);
        }
    }
    public class DoubleSerializer : ValueSerializerHelper<double>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(double);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToDouble(str);
        }
    }
    public class DecimalSerializer : ValueSerializerHelper<decimal>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(decimal);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToDecimal(str);
        }
    }
    public class ByteSerializer : ValueSerializerHelper<byte>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(byte);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToByte(str);
        }
    }
    public class UInt16Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(UInt16);
        public static UInt16 ReadUInt16(IReader pkg)
        {
            UInt16 v;
            unsafe
            {
                //pkg.ReadPtr(&v, sizeof(UInt16));
                //return v;
                byte v0_7;
                pkg.ReadPtr(&v0_7, sizeof(byte));
                if ((v0_7 & 0x80) == 0)
                {
                    return (UInt16)v0_7;
                }
                byte v8_14;
                pkg.ReadPtr(&v8_14, sizeof(byte));
                if ((v8_14 & 0x80) == 0)
                {
                    v = (UInt16)((v0_7 & 0x7F) | (v8_14 << 7));
                    return v;
                }
                byte v15_16;
                pkg.ReadPtr(&v15_16, sizeof(byte));
                v = (UInt16)((v0_7 & 0x7F) | ((v8_14 << 7) & 0x3FFF));

                v = (UInt16)((v & 0x3FFF) | ((v15_16 & 0x3) << 14));
                return v;
            }
        }
        public override object ReadValue(IReader pkg)
        {
            return ReadUInt16(pkg);
        }
        public static void WriteUInt16(UInt16 v, IWriter pkg)
        {
            unsafe
            {
                //pkg.WritePtr(&v, sizeof(UInt16));
                //return;
                if (v < 0x7F)
                {
                    byte wv = (byte)v;
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else if (v < 0x3FFF)
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 7) & 0x7F);
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)(((v >> 7) & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)(((v >> 14) & 0x3));
                    pkg.WritePtr(&wv, sizeof(byte));
                }
            }
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (UInt16)obj;
            WriteUInt16(v, pkg);
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<UInt16>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    UInt16 v;
                    pkg.ReadPtr(&v, sizeof(UInt16));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<UInt16>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(UInt16));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToUInt16(str);
        }
    }
    public class UInt32Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(UInt32);
        public static UInt32 ReadUInt32(IReader pkg)
        {
            UInt32 v;
            unsafe
            {
                byte v0_7;
                pkg.ReadPtr(&v0_7, sizeof(byte));
                if ((v0_7 & 0x80) == 0)
                {
                    return (UInt32)v0_7;
                }
                byte v8_14;
                pkg.ReadPtr(&v8_14, sizeof(byte));
                if ((v8_14 & 0x80) == 0)
                {
                    v = (UInt32)((v0_7 & 0x7F) | ((v8_14 & 0x7F) << 7));
                    return v;
                }
                byte v15_21;
                pkg.ReadPtr(&v15_21, sizeof(byte));
                if ((v15_21 & 0x80) == 0)
                {
                    v = (UInt32)((v0_7 & 0x7F) | ((v8_14&0x7F) << 7) | ((v15_21 & 0x7F) << 14));
                    return v;
                }
                byte v22_28;
                pkg.ReadPtr(&v22_28, sizeof(byte));
                if ((v22_28 & 0x80) == 0)
                {
                    v = (UInt32)((v0_7 & 0x7F) | ((v8_14 & 0x7F) << 7) | ((v15_21 & 0x7F) << 14) | ((v22_28 & 0x7F) << 21));
                    return v;
                }
                else
                {
                    byte v29_32;
                    pkg.ReadPtr(&v29_32, sizeof(byte));

                    v = (UInt32)((v0_7 & 0x7F) | ((v8_14 & 0x7F) << 7) | ((v15_21 & 0x7F) << 14) | ((v22_28 & 0x7F) << 21) | ((v29_32 & 0xF) << 28));
                    return v;
                }   
            }
        }
        public override object ReadValue(IReader pkg)
        {
            //UInt32 v;
            //unsafe
            //{
            //    pkg.ReadPtr(&v, sizeof(UInt32));
            //}
            //return v;
            return ReadUInt32(pkg);
        }
        public static void WriteUInt32(UInt32 v, IWriter pkg)
        {
            unsafe
            {
                if (v < 0x7F)
                {
                    byte wv = (byte)v;
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else if (v < 0x3FFF)
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 7) & 0x7F);
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else if(v<0x1FFFFF)
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 7) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 14) & 0x7F & 0x7F);
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else if (v < 0x0FFFFFFF)
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 7) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 14) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 21) & 0x7F & 0x7F);
                    pkg.WritePtr(&wv, sizeof(byte));
                }
                else
                {
                    byte wv = (byte)((v & 0x7F) | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 7) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 14) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 21) & 0x7F | 0x80);
                    pkg.WritePtr(&wv, sizeof(byte));

                    wv = (byte)((v >> 28) & 0xF);
                    pkg.WritePtr(&wv, sizeof(byte));
                }
            }
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            //var v = (UInt32)obj;
            //unsafe
            //{
            //    pkg.WritePtr(&v, sizeof(UInt32));
            //}
            var v = (UInt32)obj;
            WriteUInt32(v, pkg);
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<UInt32>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    UInt32 v;
                    pkg.ReadPtr(&v, sizeof(UInt32));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<UInt32>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(UInt32));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToUInt32(str);
        }
    }
    public class UInt64Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(UInt64);
        public override object ReadValue(IReader pkg)
        {
            UInt64 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(UInt64));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (UInt64)obj;
            unsafe
            {
                pkg.WritePtr(&v, sizeof(UInt64));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<UInt64>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    UInt64 v;
                    pkg.ReadPtr(&v, sizeof(UInt64));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<UInt32>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(UInt64));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToUInt64(str);
        }
    }
    public class SByteSerializer : ValueSerializerHelper<sbyte>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(sbyte);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToSByte(str);
        }
    }
    public class Int16Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Int16);
        public override object ReadValue(IReader pkg)
        {
            return (Int16)UInt16Serializer.ReadUInt16(pkg);
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (UInt16)((Int16)obj);
            UInt16Serializer.WriteUInt16(v, pkg);
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<Int16>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int16 v;
                    pkg.ReadPtr(&v, sizeof(Int16));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<Int16>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(Int16));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToInt16(str);
        }
    }
    public class Int32Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Int32);
        public override object ReadValue( IReader pkg)
        {
            Int32 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(Int32));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (Int32)obj;
            unsafe
            {
                pkg.WritePtr(&v, sizeof(Int32));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<Int32>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int32 v;
                    pkg.ReadPtr(&v, sizeof(Int32));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<Int32>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(Int32));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToInt32(str);
        }
    }
    public class Int64Serializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Int64);
        public override object ReadValue( IReader pkg)
        {
            Int64 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(Int64));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (Int64)obj;
            unsafe
            {
                pkg.WritePtr(&v, sizeof(Int64));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (List<Int64>)(obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int64 v;
                    pkg.ReadPtr(&v, sizeof(Int64));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = (List<Int64>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    pkg.WritePtr(&v, sizeof(Int64));
                }
            }
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return System.Convert.ToInt64(str);
        }
    }
    public class Vector2Serializer : ValueSerializerHelper<Vector2>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Vector2);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var obj = (Vector2)o;
            return $"{obj.X}:{obj.Y}";
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var ret = new Vector2();
            var segs = str.Split(':');
            if (segs.Length < 2)
                return ret;
            ret.X = System.Convert.ToSingle(segs[0]);
            ret.Y = System.Convert.ToSingle(segs[1]);
            return ret;
        }
    }
    public class Vector3Serializer : ValueSerializerHelper<Vector3>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Vector3);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var obj = (Vector3)o;
            return $"{obj.X}:{obj.Y}:{obj.Z}";
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var ret = new Vector3();
            var segs = str.Split(':');
            if (segs.Length < 3)
                return ret;
            ret.X = System.Convert.ToSingle(segs[0]);
            ret.Y = System.Convert.ToSingle(segs[1]);
            ret.Z = System.Convert.ToSingle(segs[2]);
            return ret;
        }
    }
    public class Vector4Serializer : ValueSerializerHelper<Vector4>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Vector4);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var obj = (Vector4)o;
            return $"{obj.X}:{obj.Y}:{obj.Z}:{obj.W}";
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var ret = new Vector4();
            var segs = str.Split(':');
            if (segs.Length < 4)
                return ret;
            ret.X = System.Convert.ToSingle(segs[0]);
            ret.Y = System.Convert.ToSingle(segs[1]);
            ret.Z = System.Convert.ToSingle(segs[2]);
            ret.W = System.Convert.ToSingle(segs[3]);
            return ret;
        }
    }
    public class Color4Serializer : ValueSerializerHelper<Color4>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Color4);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var obj = (Color4)o;
            return $"{obj.Red}:{obj.Green}:{obj.Blue}:{obj.Alpha}";
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var ret = new Color4();
            var segs = str.Split(':');
            if (segs.Length < 4)
                return ret;
            ret.Red = System.Convert.ToSingle(segs[0]);
            ret.Green = System.Convert.ToSingle(segs[1]);
            ret.Blue = System.Convert.ToSingle(segs[2]);
            ret.Alpha = System.Convert.ToSingle(segs[3]);
            return ret;
        }
    }
    public class QuaternionSerializer : ValueSerializerHelper<Quaternion>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Quaternion);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var obj = (Quaternion)o;
            return $"{obj.X}:{obj.Y}:{obj.Z}:{obj.W}";
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var ret = new Quaternion();
            var segs = str.Split(':');
            if (segs.Length < 4)
                return ret;
            ret.X = System.Convert.ToSingle(segs[0]);
            ret.Y = System.Convert.ToSingle(segs[1]);
            ret.Z = System.Convert.ToSingle(segs[2]);
            ret.W = System.Convert.ToSingle(segs[3]);
            return ret;
        }
    }
    public class MatrixSerializer : ValueSerializerHelper<Matrix>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Matrix);
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            string result = "";
            var matrix  = (Matrix)o;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result += matrix[i, j].ToString();
                    result += ",";
                }
            }
            return result;
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var matrix = new Matrix();
            var segs = str.Split(',');
            if (segs.Length < 26)
                return matrix;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = System.Convert.ToSingle(segs[i * 4 + j]);
                }
            }
            return matrix;
        }
    }
    public class GuidSerializer : ValueSerializerHelper<Guid>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Guid);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            Guid obj;
            Guid.TryParse(str, out obj);
            return obj;
        }
    }
    public class DateTimeSerializer : ValueSerializerHelper<System.DateTime>
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(System.DateTime);
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            DateTime obj;
            DateTime.TryParse(str, out obj);
            return obj;
        }
    }
    public class ColorSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(EngineNS.Color);

        public override object ReadValue(IReader pkg)
        {
            unsafe
            {
                Int32 val = 0;
                pkg.ReadPtr(&val, sizeof(Int32));
                return EngineNS.Color.FromArgb(val);
            }
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = (EngineNS.Color)obj;
            unsafe
            {
                Int32 val = v.ToArgb();
                pkg.WritePtr(&val, sizeof(Int32));
            }
        }

        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return EngineNS.Color.FromString(str);
        }


        public override void ReadValueList(IList obj, IReader pkg)
        {
            var lst = (List<EngineNS.Color>)(obj);
            unsafe
            {
                UInt16 count;
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int32 val = 0;
                    pkg.ReadPtr(&val, sizeof(Int32));
                    lst.Add(EngineNS.Color.FromArgb(val));
                }
            }
        }

        public override void WriteValueList(IList obj, IWriter pkg)
        {
            var lst = (List<EngineNS.Color>)(obj);
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    var vInt = v.ToArgb();
                    pkg.WritePtr(&vInt, sizeof(Int32));
                }
            }
        }
    }
    public class TypeSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(System.Type);
        public override object ReadValue(IReader pkg)
        {
            unsafe
            {
                UInt16 len;
                pkg.ReadPtr(&len, sizeof(UInt16));
                if (len == 0)
                    return null;
                var str = new System.Char[len];
                fixed(System.Char* pChar = &str[0])
                {
                    pkg.ReadPtr(pChar, sizeof(System.Char) * len);
                    var typeStr = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                    var retType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                    if(retType == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Serializer", $"TypeSerializer.ReadValue == null: {typeStr}");
                    }
                    return retType;
                }
            }
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var type = (Type)obj;
            unsafe
            {
                var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(type);
                var len = (UInt16)typeStr.Length;
                pkg.WritePtr(&len, sizeof(UInt16));
                if(len > 0)
                {
                    fixed(System.Char* pPtr = typeStr)
                    {
                        pkg.WritePtr(pPtr, sizeof(System.Char) * len);
                    }
                }
            }
        }
        public override void ReadValueList(IList lst, IReader pkg)
        {
            unsafe
            {
                UInt16 count;
                pkg.ReadPtr(&count, sizeof(UInt16));
                for(UInt16 i = 0; i < count; i++)
                {
                    UInt16 len;
                    pkg.ReadPtr(&len, sizeof(UInt16));
                    if(len == 0)
                    {
                        lst.Add(null);
                        continue;
                    }
                    var str = new System.Char[len];
                    fixed(System.Char* pChar = &str[0])
                    {
                        pkg.ReadPtr(pChar, sizeof(System.Char) * len);
                        var typeStr = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                        lst.Add(typeStr);
                    }
                }
            }
        }
        public override void WriteValueList(IList obj, IWriter pkg)
        {
            var lst = (List<Type>)obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for(UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i];
                    var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(v);
                    var len = (UInt16)typeStr.Length;
                    pkg.WritePtr(&len, sizeof(UInt16));
                    if(len > 0)
                    {
                        fixed(System.Char* pPTr = typeStr)
                        {
                            pkg.WritePtr(pPTr, sizeof(System.Char) * len);
                        }
                    }
                }
            }
        }
        public override object ObjectFromString(MemberDesc p, string str)
        {
            return EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(str);
        }
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            return EngineNS.Rtti.RttiHelper.GetTypeSaveString((Type)o);
        }
    }
    #endregion

    #region enum
    public class SByteEnum
    {
    }
    public class SByteEnumSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(SByteEnum);
        public override object ReadValue(IReader pkg)
        {
            sbyte v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(sbyte));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            sbyte v = System.Convert.ToSByte(obj);
            unsafe
            {
                pkg.WritePtr(&v, sizeof(sbyte));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    sbyte v;
                    pkg.ReadPtr(&v, sizeof(sbyte));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = System.Convert.ToSByte(lst[i]);
                    pkg.WritePtr(&v, sizeof(sbyte));
                }
            }
        }
        public override string ObjectToString(MemberDesc p, object o)
        {
            return o.ToString();
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return Enum.Parse(p.MemberType, str);
        }
    }
    public class Int16Enum
    {
    }    
    public class In16EnumSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Int16Enum);
        public override object ReadValue(IReader pkg)
        {
            Int16 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(Int16));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = System.Convert.ToInt16(obj);
            unsafe
            {
                pkg.WritePtr(&v, sizeof(Int16));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int16 v;
                    pkg.ReadPtr(&v, sizeof(Int16));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = System.Convert.ToInt16(lst[i]);
                    pkg.WritePtr(&v, sizeof(Int16));
                }
            }
        }
        public override string ObjectToString(MemberDesc p, object o)
        {
            return o.ToString();
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return Enum.Parse(p.MemberType, str);
        }
    }
    public class Int32Enum
    {
    }
    public class Int32EnumSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(Int32Enum);
        public override object ReadValue(IReader pkg)
        {
            Int32 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(Int32));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = System.Convert.ToInt32(obj);
            unsafe
            {
                pkg.WritePtr(&v, sizeof(Int32));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    Int32 v;
                    pkg.ReadPtr(&v, sizeof(Int32));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = System.Convert.ToInt32(lst[i]);
                    pkg.WritePtr(&v, sizeof(Int32));
                }
            }
        }
        public override string ObjectToString(MemberDesc p, object o)
        {
            return o.ToString();
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return Enum.Parse(p.MemberType, str);
        }
    }
    public class UInt64Enum { }
    public class UInt64EnumSerializer : FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(UInt64Enum);
        public override object ReadValue(IReader pkg)
        {
            UInt64 v;
            unsafe
            {
                pkg.ReadPtr(&v, sizeof(UInt64));
            }
            return v;
        }
        public override void WriteValue(object obj, IWriter pkg)
        {
            var v = System.Convert.ToUInt64(obj);
            unsafe
            {
                pkg.WritePtr(&v, sizeof(UInt64));
            }
        }
        public override void ReadValueList(System.Collections.IList obj, IReader pkg)
        {
            var lst = (obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    UInt64 v;
                    pkg.ReadPtr(&v, sizeof(UInt64));
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = System.Convert.ToUInt64(lst[i]);
                    pkg.WritePtr(&v, sizeof(UInt64));
                }
            }
        }
        public override string ObjectToString(MemberDesc p, object o)
        {
            return o.ToString();
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return Enum.Parse(p.MemberType, str);
        }
    }

    #endregion

    public class TypeDescGenerator
    {
        public static TypeDescGenerator Instance
        {
            get;
        } = new TypeDescGenerator();
        public class TypeDesc
        {
            protected object DefaultObject;
            public System.Type ClassType;
            public UInt32 HashCode;
            public bool IsDefaultValue(object obj, Rtti.MemberDesc prop)
            {
                if (obj.GetType() != ClassType)
                    return false;
                var pvalue1 = prop.GetValue(obj);
                
                var pvalue2 = prop.GetValue(DefaultObject);
                return Equals(pvalue1, pvalue2);
            }

            public List<Rtti.MetaData.FieldDesc> Members = new List<Rtti.MetaData.FieldDesc>();
            public bool BuildType(System.Type type)
            {
                try
                {
                    if (type.ContainsGenericParameters || type.IsAbstract)
                    {
                        return false;
                    }
                    else
                    {
                        if (type.IsClass && type.GetConstructor(new Type[0]) == null)
                            return false;
                        DefaultObject = System.Activator.CreateInstance(type);
                    }
                }
                catch
                {
                    return false;//无法实例化的类不用做出来了
                }
                ClassType = type;
                foreach (var i in type.GetProperties())
                {
                    if (i.CanWrite == false)
                        continue;
                    var desc = new Rtti.MetaData.FieldDesc();
                    
                    desc.PropInfo = new Rtti.PropMemberDesc(i);
                    System.Type elemType = null;
                    if (IgnoreSerialize(desc.PropInfo, out desc.IsList, out elemType))
                        continue;

                    if (elemType.IsValueType && desc.IsList==false)
                    {
                        //如果是简单的值类型序列化器，可以做代码生成优化
                        //这里windows或者android等支持jit的系统，可以动态生成代码，然后吧desc.Serializer替换了
                        //这样就完全没有了GetValue,SetValue等的拆箱装箱性能损失，也没有他们的invoke调用了
                        //var srCode = FieldSerializer.GetValueTypeSerializeCode(i);
                        //System.Diagnostics.Debug.WriteLine(srCode);
                    }
                    desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                    if (desc.Serializer == null)
                    {
                        if (elemType.IsValueType)
                        {
                            elemType = typeof(ValueObject);
                            desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                        }
                        else if(elemType.IsGenericType && elemType.GetInterface(typeof(IList).FullName) != null)
                        {
                            elemType = elemType.GenericTypeArguments[0];
                            desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                            desc.IsList = true;
                        }
                        if(desc.Serializer==null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Meta", $"class {elemType} don't have Serializer!");
                        }
                        Members.Add(desc);
                    }
                    else
                    {
                        Members.Add(desc);
                    }
                }
                foreach (var i in type.GetFields())
                {
                    var desc = new Rtti.MetaData.FieldDesc();

                    desc.PropInfo = new Rtti.FieldMemberDesc(i);
                    System.Type elemType = null;
                    if (IgnoreSerialize(desc.PropInfo, out desc.IsList, out elemType))
                        continue;

                    if (elemType.IsValueType && desc.IsList == false)
                    {
                        //如果是简单的值类型序列化器，可以做代码生成优化
                        //这里windows或者android等支持jit的系统，可以动态生成代码，然后吧desc.Serializer替换了
                        //这样就完全没有了GetValue,SetValue等的拆箱装箱性能损失，也没有他们的invoke调用了
                        //var srCode = FieldSerializer.GetValueTypeSerializeCode(i);
                        //System.Diagnostics.Debug.WriteLine(srCode);
                    }
                    desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                    if (desc.Serializer == null)
                    {
                        if (elemType.IsValueType)
                        {
                            elemType = typeof(ValueObject);
                            desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                        }
                        else if (elemType.IsGenericType && elemType.GetInterface(typeof(IList).FullName) != null)
                        {
                            elemType = elemType.GenericTypeArguments[0];
                            desc.Serializer = TypeDescGenerator.Instance.GetSerializer(elemType);
                            desc.IsList = true;
                        }
                        Members.Add(desc);
                    }
                    else
                    {
                        Members.Add(desc);
                    }
                }
                //string hashStr = Rtti.RttiHelper.GetAppTypeString(type) + "->\n";
                //Members.Sort((Rtti.MetaData.FieldDesc a, Rtti.MetaData.FieldDesc b)=>
                //{
                //    return a.PropInfo.Name.CompareTo(b.PropInfo.Name);
                //});
                //foreach(var i in Members)
                //{
                //    hashStr += Rtti.RttiHelper.GetAppTypeString(i.PropInfo.MemberType) + ":" + i.PropInfo.Name + ";\n";
                //}
                //HashCode = UniHash.APHash(hashStr);
                //DebugInfo = hashStr;
                HashCode = Rtti.MetaData.FieldDesc.SortAndCalHash_1(type, Members);
                return true;
            }
            public static bool IgnoreSerialize(Rtti.MemberDesc p, out bool isList, out System.Type elemType)
            {
                isList = false;
                elemType = p.MemberType;

                var atts = p.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), true);
                if(atts != null && atts.Length > 0)
                {
                    var metaAtt = atts[0] as EngineNS.Rtti.MetaDataAttribute;
                    if (metaAtt.HasSaveType(MetaDataAttribute.enSaveType.Xml))
                        return false;
                }
                return true;

                //if (p.GetType() == typeof(Rtti.PropMemberDesc))
                //{
                //    var attrs = p.GetCustomAttributes(typeof(ExcludeAttribute), true);
                //    if (attrs != null && attrs.Length != 0)
                //        return true;
                //}
                //else if (p.GetType()==typeof(Rtti.FieldMemberDesc))
                //{
                //    var attrs = p.GetCustomAttributes(typeof(IncludeFieldAttribute), true);
                //    if (attrs == null || attrs.Length == 0)
                //        return true; 
                //}
                //if (p.MemberType.IsEnum)
                //{
                //    var attr = p.MemberType.GetCustomAttributes(typeof(EnumSizeAttribute), true);
                //    if (attr != null && attr.Length > 0)
                //    {
                //        var esattr = attr[0] as EnumSizeAttribute;
                //        elemType = esattr.SizeType;
                //    }
                //    else
                //    {
                //        elemType = typeof(SByteEnum);
                //    }
                //    return false;
                //}
                //else if (p.MemberType.IsValueType)
                //    return false;
                //else if (p.MemberType == typeof(Type))
                //    return false;
                //else if (p.MemberType == typeof(string))
                //    return false;
                //else if (p.MemberType == typeof(MemChunk))
                //    return false;
                //else if (p.MemberType.IsSubclassOf(typeof(Serializer)))
                //    return false;
                //else if (p.MemberType.GetInterface(typeof(ISerializer).FullName) != null)
                //    return false;
                //else if (p.MemberType.IsGenericType)
                //{
                //    //if (p.MemberType.GetGenericTypeDefinition() == typeof(List<>))
                //    if(p.MemberType.GetInterface(typeof(IList).FullName) != null)
                //    {
                //        var type0 = p.MemberType.GenericTypeArguments[0];
                //        if (type0.IsEnum)
                //        {
                //            var attr = type0.GetCustomAttributes(typeof(EnumSizeAttribute), true);
                //            if (attr != null && attr.Length > 0)
                //            {
                //                var esattr = attr[0] as EnumSizeAttribute;
                //                elemType = esattr.SizeType;
                //            }
                //            else
                //            {
                //                elemType = typeof(SByteEnum);
                //            }
                //            isList = true;
                //            return false;
                //        }
                //        else if (type0.IsSubclassOf(typeof(Serializer)) || (type0.GetInterface(typeof(ISerializer).FullName) != null) || type0.IsValueType || type0 == typeof(string))
                //        {
                //            elemType = type0;
                //            isList = true;
                //            return false;
                //        }
                //        else
                //        {
                //            var attrs = type0.GetCustomAttributes(typeof(IO.Serializer.IncludeFieldAttribute), true);
                //            if (attrs != null && attrs.Length != 0)
                //            {
                //                elemType = type0;
                //                isList = true;
                //                return false;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (p is EngineNS.Rtti.FieldMemberDesc)
                //    {
                //        var attrs = p.MemberType.GetCustomAttributes(typeof(IO.Serializer.IncludeFieldAttribute), true);
                //        if (attrs != null && attrs.Length != 0)
                //        {
                //            elemType = p.MemberType;
                //            isList = true;
                //            return false;
                //        }
                //    }
                //    else
                //        return false;
                //}
                //return true;
            }
        }
        protected Dictionary<System.Type, FieldSerializer> Type2Serializer = new Dictionary<Type, FieldSerializer>();
        protected Dictionary<System.Type, TypeDesc> Type2HashCode = new Dictionary<Type, TypeDesc>();
        protected Dictionary<UInt32, TypeDesc> HashCode2Type = new Dictionary<UInt32, TypeDesc>();
        public void BuildTypes(VAssembly assembly)
        {
            BuildTypes(new System.Reflection.Assembly[] { assembly.Assembly }, assembly.CSType);
        }
        public void BuildTypes(System.Reflection.Assembly[] assemblies, EngineNS.ECSType csType)
        {
            lock (this)
            {
                foreach (var assembly in assemblies)
                {
                    foreach (var i in assembly.GetTypes())
                    {
                        if (i.IsSubclassOf(typeof(FieldSerializer)))
                        {
                            var srType = i.GetProperty("SerializeType");
                            if (srType == null || srType.PropertyType!=typeof(System.Type))
                                continue;
                            var type = srType.GetValue(null) as System.Type;
                            Type2Serializer[type] = System.Activator.CreateInstance(i) as FieldSerializer;
                        }
                    }
                }
                foreach (var assembly in assemblies)
                {
                    foreach (var i in assembly.GetTypes())
                    {
                        var test = (i.IsSubclassOf(typeof(Serializer))) ||
                            (i.GetInterface(typeof(ISerializer).FullName) != null);
                        if (test == false)
                        {
                            continue;
                        }
                        var tDesc = new TypeDesc();
                        if (false == tDesc.BuildType(i))
                            continue;
                        Type2HashCode[i] = tDesc;
                        HashCode2Type[tDesc.HashCode] = tDesc;
                    }
                }
            }
        }
        public FieldSerializer GetSerializer(System.Type type)
        {
            if (type == null)
                return null;
            FieldSerializer io;
            if (Type2Serializer.TryGetValue(type, out io))
                return io;
            if ((type.IsSubclassOf(typeof(Serializer))) ||
                (type.GetInterface(typeof(ISerializer).FullName) != null))
                type = typeof(Serializer);
            else if(type.IsEnum)
            {
                var attr = type.GetCustomAttributes(typeof(EnumSizeAttribute), true);
                if (attr != null && attr.Length > 0)
                {
                    var esattr = attr[0] as EnumSizeAttribute;
                    type = esattr.SizeType;
                }
                else
                {
                    type = typeof(SByteEnum);
                }
            }
            if (Type2Serializer.TryGetValue(type, out io))
                return io;
            if (type.IsValueType)
            {
                if(Type2Serializer.TryGetValue(typeof(ValueObject), out io))
                    return io;
            }
            return null;
        }
        public TypeDesc GetTypeDesc(System.Type type)
        {
            lock (this)
            {
                TypeDesc tDesc;
                if (Type2HashCode.TryGetValue(type, out tDesc))
                    return tDesc;
                if (type.IsClass)
                {
                    if ((type.IsSubclassOf(typeof(Serializer)) == false) &&
                        (type.GetInterface(typeof(ISerializer).FullName) == null))
                        return null;
                }
                tDesc = new TypeDesc();
                tDesc.BuildType(type);
                Type2HashCode[type] = tDesc;
                HashCode2Type[tDesc.HashCode] = tDesc;
                return tDesc;
            }
        }
        public TypeDesc GetTypeDesc(UInt32 hash)
        {
            TypeDesc tDesc;
            if (HashCode2Type.TryGetValue(hash, out tDesc))
                return tDesc;
            return null;
        }
        public TypeDesc FindTypeDesc(string name)
        {
            foreach(var i in HashCode2Type.Values)
            {
                if (i.ClassType.FullName == name)
                    return i;
            }
            return null;
        }
    }
}
