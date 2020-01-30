using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO.Serializer;

namespace EngineNS.IO
{
    public class XndSDK
    {
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDNode_GetR2M(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDAttrib_GetR2M(XndAttrib.NativePointer attrib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XndNode.NativePointer XNDNode_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDNode_TryReleaseHolder(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_GetResourceRefCount(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr XNDNode_SetName(XndNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDNode_GetName(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt64 XNDNode_GetClassID(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XndNode.NativePointer XNDNode_AddNode(XndNode.NativePointer node, IntPtr name, Int64 classId, UInt32 flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XndNode.NativePointer XNDNode_AddNodeWithSource(XndNode.NativePointer node, XndNode.NativePointer srcNode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_DelNode(XndNode.NativePointer node, XndNode.NativePointer childNode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XndNode.NativePointer XNDNode_FindNode(XndNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XndAttrib.NativePointer XNDNode_AddAttrib(XndNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XndAttrib.NativePointer XNDNode_AddAttribWithSource(XndNode.NativePointer node, XndAttrib.NativePointer childAtt);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static XndAttrib.NativePointer XNDNode_FindAttrib(XndNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int XNDNode_DelAttrib(XndNode.NativePointer node, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_GetNodeNumber(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XndNode.NativePointer XNDNode_GetNode(XndNode.NativePointer node, int iNode);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_GetAttribNumber(XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static XndAttrib.NativePointer XNDNode_GetAttrib(XndNode.NativePointer node, int iAttrib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_Save(XndNode.NativePointer node, FileWriter.NativePointer file);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDNode_Load(XndNode.NativePointer node, FileReader.NativePointer pRes);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static Byte XNDAttrib_GetVersion(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_SetVersion(XndAttrib.NativePointer node, Byte ver);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 XNDAttrib_GetLength(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDAttrib_GetKey(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void XNDAttrib_SetKey(XndAttrib.NativePointer node, string key);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDAttrib_GetName(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_BeginRead(XndAttrib.NativePointer node, string file, int line);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_EndRead(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_BeginWrite(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_EndWrite(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDAttrib_Read(XndAttrib.NativePointer node, IntPtr data, int size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int XNDAttrib_Write(XndAttrib.NativePointer node, IntPtr data, int size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr XNDAttrib_ReadStringA(XndAttrib.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static void XNDAttrib_FreeStringA(IntPtr str);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void XNDAttrib_WriteStringA(XndAttrib.NativePointer node, string data);
    }
    public class XndAttrib : AuxCoreObject<XndAttrib.NativePointer>, IWriter, IReader
    {
        public EIOType IOType => EIOType.File;

        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        public Byte Version
        {
            get
            {
                return XndSDK.XNDAttrib_GetVersion(CoreObject);
            }
            set
            {
                XndSDK.XNDAttrib_SetVersion(CoreObject, value);
            }
        }

        public UInt32 Length
        {
            get
            {
                return XndSDK.XNDAttrib_GetLength(CoreObject);
            }
        }
        public bool IsWritable
        {
            get
            {
                return XndSDK.XNDAttrib_GetR2M(CoreObject) == IntPtr.Zero;
            }
        }
        public string Key
        {
            get
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(XndSDK.XNDAttrib_GetKey(CoreObject));
            }
            set
            {
                XndSDK.XNDAttrib_SetKey(CoreObject, value);
            }
        }

        public XndAttrib(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }

        public string GetName()
        {
            unsafe
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(XndSDK.XNDAttrib_GetName(CoreObject));
            }
        }
        public bool Reading
        {
            get
            {
                return SDK_XNDAttrib_GetReading(mCoreObject);
            }
        }
        public bool BeginRead()
        {
            if (CoreObject.Pointer == IntPtr.Zero)
                return false;

#if DEBUG && PWindow
            XndSDK.XNDAttrib_BeginRead(CoreObject, GetCurSourceFileName(), GetLineNum());
#else
            XndSDK.XNDAttrib_BeginRead(CoreObject, "", 0);
#endif
            return true;
        }
        private static string GetCurSourceFileName()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(2, true);
            var frame = st.GetFrame(0);

            return st.GetFrame(0).GetFileName();
            //return frame.GetMethod().DeclaringType.FullName + "->" + frame.GetMethod().ToString();
        }
        private static int GetLineNum()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(2, true);
            return st.GetFrame(0).GetFileLineNumber();
        }
        public void EndRead()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                XndSDK.XNDAttrib_EndRead(CoreObject);
            }
        }
        public bool Writing
        {
            get
            {
                return SDK_XNDAttrib_GetWriting(mCoreObject);
            }
        }
        public bool BeginWrite()
        {
            if (CoreObject.Pointer == IntPtr.Zero)
                return false;
            
            XndSDK.XNDAttrib_BeginWrite(CoreObject);

            return true;
        }

        public void EndWrite()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                XndSDK.XNDAttrib_EndWrite(CoreObject);
            }
        }

        public bool Write(Support.CBlobObject blob)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }

            Support.CBlobObject.SDK_IBlobObject_Write2Xnd(blob.CoreObject, CoreObject);

            return true;
        }

        public bool Read(out Support.CBlobObject blob)
        {
            blob = null;
            System.Diagnostics.Debug.Assert(Reading);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }

            blob = new Support.CBlobObject();
            Support.CBlobObject.SDK_IBlobObject_ReadFromXnd(blob.CoreObject, CoreObject);

            return true;
        }

#region 原生数据读取
        public virtual void OnReadError()
        {
            throw new NotImplementedException();
        }
        public void Read(ISerializer v)
        {
            this.ReadMetaObject(v);
        }
        public void Read(out ChunkReader v)
        {
            unsafe
            {
                int len = 0;
                Read(out len);
                unsafe
                {
                    byte[] data;
                    Read(out data, len);
                    v = new ChunkReader(data);
                }
            }
        }
        public unsafe void ReadPtr(void* p, int length)
        {
            Read((IntPtr)p, length);
        }
        public bool Read(IntPtr buffer, int length)
        {
            System.Diagnostics.Debug.Assert(Reading);
            if (CoreObject.Pointer == IntPtr.Zero)
                return false;

            XndSDK.XNDAttrib_Read(CoreObject, buffer, length);

            return true;
        }
        public bool Read<T>(out T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    return Read((IntPtr)(p), sizeof(T));
                }
            }
        }
        public void Read(out byte[] v)
        {
            unsafe
            {
                int len;
                ReadPtr(&len, sizeof(int));
                Read(out v, len);
            }
        }
        public bool Read(out System.Byte[] data, int length)
        {
            System.Diagnostics.Debug.Assert(Reading);
            data = new System.Byte[length];
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }

            unsafe
            {
                fixed (void* pinData = &data[0])
                {
                    XndSDK.XNDAttrib_Read(CoreObject, (IntPtr)pinData, sizeof(System.Byte) * length);
                }
            }

            return true;
        }
        public void Read(out System.String data)
        {
            System.Diagnostics.Debug.Assert(Reading);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                data = "";
                return;
            }

            unsafe
            {
                IntPtr strPtr = XndSDK.XNDAttrib_ReadStringA(CoreObject);
                data = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(strPtr);
                XndSDK.XNDAttrib_FreeStringA(strPtr);
            }
        }
        public void Read(out Support.BitSet data)
        {
            System.Diagnostics.Debug.Assert(Reading);
            data = null;
            if (CoreObject.Pointer == IntPtr.Zero)
                return;

            unsafe
            {
                int bitCount = 0;
                XndSDK.XNDAttrib_Read(CoreObject, (IntPtr)(&bitCount), sizeof(int));
                data = new Support.BitSet();
                int byteCount = 0;
                XndSDK.XNDAttrib_Read(CoreObject, (IntPtr)(&byteCount), sizeof(int));
                byte[] bitData = new byte[byteCount];
                fixed (byte* p = &bitData[0])
                {
                    XndSDK.XNDAttrib_Read(CoreObject, (IntPtr)(p), sizeof(System.Byte) * byteCount);
                }
                data.Init((UInt32)bitCount, bitData);
            }
        }
        public bool Read(out EngineNS.Color color)
        {
            System.Diagnostics.Debug.Assert(Reading);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                color = EngineNS.Color.White;
                return false;
            }

            unsafe
            {
                Int32 val = 0;
                XndSDK.XNDAttrib_Read(CoreObject, (IntPtr)(&val), sizeof(Int32));
                color = Color.FromArgb(val);
            }
            return true;
        }
        public bool Read(out EngineNS.RName rname)
        {
            System.Diagnostics.Debug.Assert(Reading);
            rname = EngineNS.RName.EmptyName;
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }

            byte ver = 0;
            Read(out ver);
            unsafe
            {
                switch (ver)
                {
                    case 0:
                        {
                            IntPtr strPtr = XndSDK.XNDAttrib_ReadStringA(CoreObject);
                            var name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(strPtr);
                            XndSDK.XNDAttrib_FreeStringA(strPtr);
                            byte type;
                            Read(out type);
                            rname = EngineNS.RName.GetRName(name, (RName.enRNameType)type);
                        }
                        break;
                    case 1:
                        {
                            var se = EngineNS.IO.Serializer.TypeDescGenerator.Instance.GetSerializer(typeof(EngineNS.RName));
                            rname = (EngineNS.RName)se.ReadValue(this);
                        }
                        break;
                }
            }
            return true;
        }
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

        #endregion 原生数据读取

        #region 原生数据写入
        public void Write(ISerializer v)
        {
            this.WriteMetaObject(v);
        }
        public void Write(ChunkWriter v)
        {
            int len = v.CurPtr();
            this.Write(len);
            unsafe
            { 
                fixed(byte* p = &v.Ptr[0])
                {
                    this.WritePtr(p, len);
                }
            }
        }
        public bool Write(IntPtr data, int length)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }

            XndSDK.XNDAttrib_Write(CoreObject, data, length);

            return true;
        }
        public unsafe void WritePtr(void* p, int length)
        {
            Write((IntPtr)p, length);
        }
        public void Write(System.Byte[] data, int length)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return;
            }

            if (length > data.Length)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            unsafe
            {
                fixed (byte* p = &data[0])
                {
                    WritePtr(p, length);
                }
            }
        }
        public void Write(System.Byte[] data)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return;
            }

            unsafe
            {
                int len = data.Length;
                WritePtr(&len, sizeof(int));
                fixed(byte* p = &data[0])
                {
                    WritePtr(p, len);
                }
            }
            //unsafe
            //{
            //    fixed (byte* p = &data[0])
            //    {
            //        XndSDK.XNDAttrib_Write(CoreObject, (IntPtr)(p), sizeof(System.Byte) * data.Length);
            //    }
            //}
        }
        public bool Write<T>(T v) where T : unmanaged
        {
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return false;
            }
            unsafe
            {
                return Write((IntPtr)(&v), sizeof(T));
            }
        }
        public void Write(System.String data)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
            {
                return;
            }

            unsafe
            {
                if (data == null)
                    data = "";
                //IntPtr strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(data);
                XndSDK.XNDAttrib_WriteStringA(CoreObject, data);
                //System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
            }
        }
        public void Write(Support.BitSet data)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
                return;

            unsafe
            {
                var bitCount = data.BitCount;
                XndSDK.XNDAttrib_Write(CoreObject, (IntPtr)(&bitCount), sizeof(int));
                byte[] bitData = data.Data;
                int byteCount = bitData.Length;
                XndSDK.XNDAttrib_Write(CoreObject, (IntPtr)(&byteCount), sizeof(int));
                fixed (byte* p = &bitData[0])
                {
                    XndSDK.XNDAttrib_Write(CoreObject, (IntPtr)(p), sizeof(System.Byte) * byteCount);
                }

            }
        }
        public bool Write(EngineNS.Color color)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
                return false;

            unsafe
            {
                Int32 val = color.ToArgb();
                XndSDK.XNDAttrib_Write(CoreObject, (IntPtr)(&val), sizeof(Int32));
            }
            return true;
        }
        public bool Write(EngineNS.RName rname)
        {
            System.Diagnostics.Debug.Assert(Writing);
            if (CoreObject.Pointer == IntPtr.Zero)
                return false;

            unsafe
            {
                byte ver = 1;
                Write(ver);
                //XndSDK.XNDAttrib_WriteStringA(CoreObject, rname.Name);
                //byte val = (byte)rname.RNameType;
                //Write(val);
                var se = EngineNS.IO.Serializer.TypeDescGenerator.Instance.GetSerializer(typeof(EngineNS.RName));
                se.WriteValue(rname, this);
            }
            return true;
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
        #endregion

        #region Meta对象读写
        public void WriteMetaObject(object obj)
        {
            var metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(obj.GetType());
            if (metaClass == null)
                return;

            this.Write("<CK>");

            var chunk = new ChunkWriter();
            chunk.Write(Rtti.RttiHelper.GetTypeSaveString(metaClass.CurrentVersion.MetaType));
            chunk.Write(metaClass.CurrentVersion.MetaHash);

            var srObj = obj as IO.Serializer.Serializer;
            if(srObj!=null)
                srObj.BeforeWrite();
            foreach (var i in metaClass.CurrentVersion.Members)
            {
                if (i.IsList)
                {
                    i.Serializer.WriteValueList(obj, i.PropInfo, chunk);
                }
                else
                {
                    i.Serializer.WriteValue(obj, i.PropInfo, chunk);
                }
            }
            Write(chunk);

            //this.Write(Rtti.RttiHelper.GetTypeSaveString(metaClass.CurrentVersion.MetaType));
            //this.Write(metaClass.CurrentVersion.MetaHash);

            //var writer = new XndAttribWriter(this);
            //foreach (var i in metaClass.CurrentVersion.Members)
            //{
            //    if (i.IsList)
            //    {
            //        i.Serializer.WriteValueList(obj, i.PropInfo, writer);
            //    }
            //    else
            //    {
            //        i.Serializer.WriteValue(obj, i.PropInfo, writer);
            //    }
            //}
        }
        public object ReadMetaObject(object obj=null)
        {
            string typeStr;
            this.Read(out typeStr);
            if(typeStr== "<CK>")
            {
                ChunkReader chunk;
                Read(out chunk);

                chunk.Read(out typeStr);

                try
                {
                    bool isRedirection;
                    var type = Rtti.RttiHelper.GetTypeFromSaveString(typeStr, out isRedirection);
                    Rtti.MetaClass metaClass;
                    if (isRedirection)
                        metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(typeStr);
                    else
                        metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(type);
                    if (metaClass == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"MetaClass({typeStr}) is not found");
                        return null;
                    }
                    if (obj == null)
                    {
                        obj = System.Activator.CreateInstance(type);
                    }
                    else
                    {
                        if (type != obj.GetType())
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"ReadMetaObject {typeStr} != {obj.GetType().FullName}");
                            //return null;
                        }
                    }
                    UInt32 hash;
                    chunk.Read(out hash);
                    var metaData = metaClass.FindMetaData(hash);
                    if (metaData == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"MetaData({metaClass.ClassName}:{hash}) is not found");
                        return null;
                    }
                    var srObj = obj as IO.Serializer.Serializer;
                    if (srObj != null)
                        srObj.BeforeRead();
                    foreach (var i in metaData.Members)
                    {
                        if (i.IsList)
                            i.Serializer.ReadValueList(obj, i.PropInfo, chunk);
                        else
                            i.Serializer.ReadValue(obj, i.PropInfo, chunk);
                    }
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                return obj;
            }
            else
            {
                bool isRedirection;
                var type = Rtti.RttiHelper.GetTypeFromSaveString(typeStr, out isRedirection);
                Rtti.MetaClass metaClass;
                if (isRedirection)
                    metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(typeStr);
                else
                    metaClass = CEngine.Instance.MetaClassManager.FindMetaClass(type);
                if (metaClass == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"MetaClass({typeStr}) is not found");
                    return null;
                }
                if (obj == null)
                {
                    obj = System.Activator.CreateInstance(type);
                }
                else
                {
                    if (type != obj.GetType())
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"ReadMetaObject {typeStr} != {obj.GetType().FullName}");
                        return null;
                    }
                }
                UInt32 hash;
                this.Read(out hash);
                var metaData = metaClass.FindMetaData(hash);
                if (metaData == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "MetaData", $"MetaData({metaClass.ClassName}:{hash}) is not found");
                    return null;
                }
                foreach (var i in metaData.Members)
                {
                    if (i.IsList)
                        i.Serializer.ReadValueList(obj, i.PropInfo, this);
                    else
                        i.Serializer.ReadValue(obj, i.PropInfo, this);
                }
                return obj;
            }
        }
#endregion

#region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_XNDAttrib_GetReading(NativePointer attrib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_XNDAttrib_GetWriting(NativePointer attrib);
#endregion
    }
    public class XndNode : AuxCoreObject<XndNode.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public XndNode()
        {
            mCoreObject = XndSDK.XNDNode_New();
        }
        public XndNode(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public int RefCount
        {
            get
            {
                return XndSDK.XNDNode_GetResourceRefCount(CoreObject);
            }
        }
        public void TryReleaseHolder()
        {
            unsafe
            {
                XndSDK.XNDNode_TryReleaseHolder(CoreObject);
            }
        }
        public bool IsWritable
        {
            get
            {
                return XndSDK.XNDNode_GetR2M(CoreObject) == IntPtr.Zero;
            }
        }
        public void SetName(string name)
        {
            unsafe
            {
                XndSDK.XNDNode_SetName(CoreObject, name);
            }
        }

        public string GetName()
        {
            unsafe
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(XndSDK.XNDNode_GetName(CoreObject));
            }
        }

        public UInt64 GetClassId()
        {
            return XndSDK.XNDNode_GetClassID(CoreObject);
        }

        public XndNode AddNode(System.String name, Int64 classId, UInt32 userFlags)
        {
            unsafe
            {
                IntPtr namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(name);
                var childNode = XndSDK.XNDNode_AddNode(CoreObject, namePtr, classId, userFlags);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(namePtr);
                return new XndNode(childNode);
            }
        }
        public XndNode AddNode(XndNode childNode)
        {
            var node = XndSDK.XNDNode_AddNodeWithSource(CoreObject, childNode.CoreObject);
            return new XndNode(node);
        }
        public bool DelNode(XndNode node)
        {
            unsafe
            {
                int ret = XndSDK.XNDNode_DelNode(CoreObject, node.CoreObject);
                if (ret == 0)
                    return false;
                return true;
            }
        }

        public XndNode FindNode(System.String name)
        {
            unsafe
            {
                var node = XndSDK.XNDNode_FindNode(CoreObject, name);
                if (node.Pointer == IntPtr.Zero)
                    return null;
                return new XndNode(node);
            }
        }

        public XndAttrib AddAttrib(System.String name)
        {
            unsafe
            {
                //IntPtr namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(name);
                var attr = XndSDK.XNDNode_AddAttrib(CoreObject, name);
                return new XndAttrib(attr);
            }
        }
        public XndAttrib AddAttrib(XndAttrib srcAtt)
        {
            var attr = XndSDK.XNDNode_AddAttribWithSource(CoreObject, srcAtt.CoreObject);
            return new XndAttrib(attr);
        }
        public XndAttrib FindAttrib(System.String name)
        {
            unsafe
            {
                //IntPtr namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(name);
                var attr = XndSDK.XNDNode_FindAttrib(CoreObject, name);
                if (attr.Pointer == IntPtr.Zero)
                    return null;

                return new XndAttrib(attr);
            }
        }

        public bool DelAttrib(System.String name)
        {
            unsafe
            {
                //IntPtr namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(name);
                int ret = XndSDK.XNDNode_DelAttrib(CoreObject, name);
                if (ret == 0)
                    return false;
                return true;
            }
        }

        public List<XndNode> GetNodes()
        {
            unsafe
            {
                List<XndNode> nodeList = new List<XndNode>();
                int count = XndSDK.XNDNode_GetNodeNumber(CoreObject);
                for (int i = 0; i < count; i++)
                {
                    var childHandle = XndSDK.XNDNode_GetNode(CoreObject, i);
                    XndNode nd = new XndNode(childHandle);
                    nodeList.Add(nd);
                }
                return nodeList;
            }
        }

        public List<XndAttrib> GetAttribs()
        {
            unsafe
            {
                List<XndAttrib> attribList = new List<XndAttrib>();
                int count = XndSDK.XNDNode_GetAttribNumber(CoreObject);
                for (int i = 0; i < count; i++)
                {
                    var attrHandle = XndSDK.XNDNode_GetAttrib(CoreObject, i);
                    XndAttrib nd = new XndAttrib(attrHandle);
                    attribList.Add(nd);
                }
                return attribList;
            }
        }

        public bool Save(FileWriter io)
        {
            unsafe
            {
                int ret = XndSDK.XNDNode_Save(CoreObject, io.CoreObject);
                if (ret == 0)
                    return false;
                return true;
            }
        }
        public bool Load(FileReader pRes)
        {
            unsafe
            {
                int ret = XndSDK.XNDNode_Load(CoreObject, pRes.CoreObject);
                if (ret == 0)
                    return false;
                return true;
            }
        }
        public class XndHolder
        {
            protected XndNode mNode;
            public XndNode Node
            {
                get { return mNode; }
            }
            ~XndHolder()
            {
                Cleanup();
            }
            public void Cleanup()
            {
                if (mNode != null)
                {
                    mNode.Cleanup();
                    mNode = null;
                }
            }

            public static async System.Threading.Tasks.Task<XndHolder> LoadXND(System.String file)
            {
                return await CEngine.Instance.EventPoster.Post(() =>
                {
                    var io = CEngine.Instance.FileManager.OpenFileForRead(file, EFileType.Xnd, false);
                    if (io == null)
                        return null;

                    XndNode node = new XndNode(GetEmptyNativePointer());
                    if (false == node.Load(io))
                        return null;

                    XndHolder holder = new XndHolder();
                    holder.mNode = node;

                    return holder;
                });
            }

            public static void SaveXND(System.String file, XndHolder node)
            {
                if (node == null || node.mNode == null)
                    return;

                var io = CEngine.Instance.FileManager.OpenFileForWrite(file, EFileType.Xnd);
                if (io == null)
                    return;

                node.mNode.Save(io);
            }

            public static XndHolder NewXNDHolder()
            {
                XndHolder holder = new XndHolder();
                holder.mNode = new XndNode();
                return holder;
            }
        }
    }
    public class XndHolder : IDisposable
    {
        protected XndNode mNode;
        public XndNode Node
        {
            get { return mNode; }
        }
        ~XndHolder()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (mNode != null)
            {
                mNode.TryReleaseHolder();
                mNode = null;
            }
        }

        public static async System.Threading.Tasks.Task<XndHolder> LoadXND(System.String file, Thread.Async.EAsyncTarget threadTarget = Thread.Async.EAsyncTarget.AsyncIO)
        {
            return await CEngine.Instance.EventPoster.Post(() =>
            {
                var io = CEngine.Instance.FileManager.OpenFileForRead(file, EFileType.Xnd, false);
                if (io == null)
                    return null;

                XndNode node = new XndNode();
                if (false == node.Load(io))
                    return null;

                XndHolder holder = new XndHolder();
                holder.mNode = node;

                return holder;
            }, threadTarget);
        }

        public static XndHolder SyncLoadXND(System.String file)
        {
            var io = CEngine.Instance.FileManager.OpenFileForRead(file, EFileType.Xnd, false);
            if (io == null)
                return null;

            XndNode node = new XndNode();
            if (false == node.Load(io))
                return null;

            XndHolder holder = new XndHolder();
            holder.mNode = node;

            return holder;
        }

        public bool LoadFromFile(System.String file)
        {
            Dispose();

            var io = CEngine.Instance.FileManager.OpenFileForRead(file, EFileType.Xnd, false);
            if (io == null)
                return false;
            mNode = new XndNode();
            if (false == mNode.Load(io))
                return false;
            return true;
        }

        public static void SaveXND(System.String file, XndHolder node)
        {
            if (node == null || node.mNode == null)
                return;

            var io = CEngine.Instance.FileManager.OpenFileForWrite(file, EFileType.Xnd);
            if (io == null)
                return;

            node.mNode.Save(io);
            io.Cleanup();
        }

        public static XndHolder NewXNDHolder()
        {
            XndHolder holder = new XndHolder();
            holder.mNode = new XndNode();
            return holder;
        }
    }

    //public class XndAttribWriter : Serializer.AuxIWriter
    //{
    //    public override EIOType IOType
    //    {
    //        get
    //        {
    //            return EIOType.File;
    //        }
    //    }
    //    protected XndAttrib mAttrib;
    //    public XndAttribWriter(XndAttrib attrib)
    //    {
    //        mAttrib = attrib;
    //    }
    //    public unsafe override void WritePtr(void* p, int length)
    //    {
    //        mAttrib.Write((IntPtr)p,length);
    //    }
    //    public override void Write(string v)
    //    {
    //        mAttrib.Write(v);
    //    }
    //    public override void Write(byte[] v)
    //    {
    //        int count = v.Length;
    //        mAttrib.Write(count);
    //        mAttrib.Write(v);
    //    }
    //    public override void Write(ChunkWriter v)
    //    {
    //        int length = v.CurPtr();
    //        mAttrib.Write(length);
    //        unsafe
    //        {
    //            fixed (byte* pData = &v.Ptr[0])
    //            {
    //                mAttrib.Write((IntPtr)pData, length);
    //            }
    //        }
    //    }

    //    public override void Write(Support.BitSet v)
    //    {
    //        mAttrib.Write(v);
    //    }
    //}

    //public class XndAttribReader : AuxIReader
    //{
    //    public override EIOType IOType
    //    {
    //        get { return EIOType.File; }
    //    }
    //    protected XndAttrib mAttrib;
    //    public XndAttribReader(XndAttrib attrib)
    //    {
    //        mAttrib = attrib;
    //    }

    //    public override void OnReadError()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Read(Serializer.ISerializer v)
    //    {
    //        mAttrib.ReadMetaObject(v);
    //    }
    //    public override void Read(out string v)
    //    {
    //        mAttrib.Read(out v);
    //    }
    //    public override void Read(out byte[] v)
    //    {
    //        int count;
    //        mAttrib.Read(out count);
    //        mAttrib.Read(out v, count);
    //    }

    //    public override void Read(out ChunkReader v)
    //    {
    //        int length = 0;
    //        mAttrib.Read(out length);
    //        unsafe
    //        {
    //            byte[] data;
    //            mAttrib.Read(out data, length);
    //            v = new ChunkReader(data);
    //        }
    //    }

    //    public override void Read(out Support.BitSet v)
    //    {
    //        mAttrib.Read(out v);
    //    }

    //    public unsafe override void ReadPtr(void* p, int length)
    //    {
    //        mAttrib.Read((IntPtr)p, length);
    //    }
    //}
}
