using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Rtti
{
    public class NativeEnum
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
        public NativeEnum(NativePointer self)
        {
            mCoreObject = self;
        }
        public string ClassName
        {
            get
            {
                return SDK_RttiEnum_GetName(CoreObject);
            }
        }
        public string NameSpace
        {
            get
            {
                return SDK_RttiEnum_GetNameSpace(CoreObject);
            }
        }
        public string FullName
        {
            get { return NameSpace + "::" + ClassName; }
        }
        public UInt32 MemberNumber
        {
            get
            {
                return SDK_RttiEnum_GetMemberNumber(CoreObject);
            }
        }
        public string GetMemberName(UInt32 index)
        {
            return SDK_RttiEnum_GetMemberName(CoreObject, index);
        }
        public int GetMemberValue(UInt32 index)
        {
            return SDK_RttiEnum_GetMemberValue(CoreObject, index);
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiEnum_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiEnum_GetNameSpace(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiEnum_GetMemberNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiEnum_GetMemberName(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_RttiEnum_GetMemberValue(NativePointer self, UInt32 index);
        #endregion
    }
    public class NativeEnumManager
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
        public NativeEnumManager()
        {
            mCoreObject = SDK_RttiEnumManager_GetInstance();
            for (UInt32 i = 0; i < EnumNumber; i++)
            {
                var info = GetEnum(i);
                Rttis.Add(info.FullName, info);
            }
        }
        public Dictionary<string, NativeEnum> Rttis
        {
            get;
        } = new Dictionary<string, NativeEnum>();
        public UInt32 EnumNumber
        {
            get
            {
                return SDK_RttiEnumManager_GetEnumNumber(CoreObject);
            }
        }
        public NativeEnum GetEnum(UInt32 index)
        {
            return new NativeEnum(SDK_RttiEnumManager_GetEnum(CoreObject, index));
        }
        public NativeEnum FindRtti(string name)
        {
            NativeEnum rtti;
            if (Rttis.TryGetValue(name, out rtti))
                return rtti;
            return null;
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_RttiEnumManager_GetInstance();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiEnumManager_GetEnumNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativeEnum.NativePointer SDK_RttiEnumManager_GetEnum(NativePointer self, UInt32 index);
        #endregion
    }
}
