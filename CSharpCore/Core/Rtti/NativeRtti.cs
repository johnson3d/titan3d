using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Rtti
{
    public class NativeRtti
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
        protected NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        public NativeRtti(NativePointer self)
        {
            mCoreObject = self;
        }
        public UInt32 Size
        {
            get
            {
                return SDK_CoreRtti_GetSize(CoreObject);
            }
        }
        public string ClassName
        {
            get
            {
                return SDK_CoreRtti_GetClassName(CoreObject);
            }
        }
        public string SuperClassName
        {
            get
            {
                return SDK_CoreRtti_GetSuperClassName(CoreObject);
            }
        }
        public UInt64 ClassId
        {
            get
            {
                return SDK_CoreRtti_GetClassId(CoreObject);
            }
        }
        public IntPtr CreateInstance(string file, int line)
        {
            return SDK_CoreRtti_CreateInstance(CoreObject, file, line);
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CoreRtti_GetSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_CoreRtti_GetClassName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_CoreRtti_GetSuperClassName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt64 SDK_CoreRtti_GetClassId(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_CoreRtti_CreateInstance(NativePointer self, string file, int line);
        #endregion
    }
    public class NativeRttiManager
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
        protected NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        public NativeRttiManager()
        {
            mCoreObject = SDK_CoreRttiManager_GetInstance();
            for(UInt32 i=0; i < RttiNumber;i++)
            {
                var info = GetRtti(i);
                Rttis.Add(info.ClassName, info);
                RttiIDs[info.ClassId] = info;
            }
        }
        public Dictionary<string, NativeRtti> Rttis
        {
            get;
        } = new Dictionary<string, NativeRtti>();
        public Dictionary<UInt64, NativeRtti> RttiIDs
        {
            get;
        } = new Dictionary<UInt64, NativeRtti>();
        public UInt32 RttiNumber
        {
            get
            {
                return SDK_CoreRttiManager_GetRttiNumber(CoreObject);
            }
        }
        public NativeRtti GetRtti(UInt32 index)
        {
            return new NativeRtti(SDK_CoreRttiManager_GetRtti(CoreObject, index));
        }
        public NativeRtti FindRtti(string name)
        {
            NativeRtti rtti;
            if (Rttis.TryGetValue(name, out rtti))
                return rtti;
            return null;
        }
        public NativeRtti FindRttiById(UInt64 classId)
        {
            NativeRtti rtti;
            if (RttiIDs.TryGetValue(classId, out rtti))
                return rtti;
            return null;
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_CoreRttiManager_GetInstance();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CoreRttiManager_GetRttiNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativeRtti.NativePointer SDK_CoreRttiManager_GetRtti(NativePointer self, UInt32 index);
        #endregion
    }
}
