using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Rtti
{
    public class NativeStruct
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
        public NativeStruct(NativePointer self)
        {
            mCoreObject = self;
        }
        public bool IsEnum
        {
            get
            {
                return (bool)SDK_RttiStruct_GetIsEnum(CoreObject);
            }
        }
        public string ClassName
        {
            get
            {
                return SDK_RttiStruct_GetName(CoreObject);
            }
        }
        public string NameSpace
        {
            get
            {
                return SDK_RttiStruct_GetNameSpace(CoreObject);
            }
        }
        public string FullName
        {
            get { return NameSpace + "::" + ClassName; }
        }
        public UInt32 Size
        {
            get
            {
                return SDK_RttiStruct_GetSize(CoreObject);
            }
        }
        public UInt32 MemberNumber
        {
            get
            {
                return SDK_RttiStruct_GetMemberNumber(CoreObject);
            }
        }
        public NativeStruct GetMemberType(UInt32 index)
        {
            NativeStruct.NativePointer inner = SDK_RttiStruct_GetMemberType(CoreObject, index);
            var temp = new NativeStruct(inner);
            return CEngine.Instance.NativeStructManager.FindRtti(temp.FullName);
        }
        public string GetMemberName(UInt32 index)
        {
            return SDK_RttiStruct_GetMemberName(CoreObject, index);
        }
        public UInt32 FindMemberIndex(string name)
        {
            return SDK_RttiStruct_FindMemberIndex(CoreObject, name);
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_RttiStruct_GetIsEnum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiStruct_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiStruct_GetNameSpace(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiStruct_GetSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiStruct_GetMemberNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_RttiStruct_GetMemberName(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativeStruct.NativePointer SDK_RttiStruct_GetMemberType(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiStruct_FindMemberIndex(NativePointer self, string name);
        #endregion
    }
    public class NativeStructManager
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
        public NativeStructManager()
        {
            mCoreObject = SDK_RttiStructManager_GetInstance();
            for (UInt32 i = 0; i < StructNumber; i++)
            {
                var info = GetStruct(i);
                Rttis.Add(info.FullName, info);
            }
        }
        public Dictionary<string, NativeStruct> Rttis
        {
            get;
        } = new Dictionary<string, NativeStruct>();
        public UInt32 StructNumber
        {
            get
            {
                return SDK_RttiStructManager_GetStructNumber(CoreObject);
            }
        }
        public NativeStruct GetStruct(UInt32 index)
        {
            return new NativeStruct(SDK_RttiStructManager_GetStruct(CoreObject, index));
        }
        public NativeStruct FindRtti(string name)
        {
            NativeStruct rtti;
            if (Rttis.TryGetValue(name, out rtti))
                return rtti;
            return null;
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_RttiStructManager_GetInstance();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_RttiStructManager_GetStructNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativeStruct.NativePointer SDK_RttiStructManager_GetStruct(NativePointer self, UInt32 index);
        #endregion
    }
}
