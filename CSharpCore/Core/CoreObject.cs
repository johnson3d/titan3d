using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace EngineNS
{
    public interface INativePointer
    {
        IntPtr GetPointer();
        void SetPointer(IntPtr value);
    }
    public struct TNativePointer<T> : INativePointer
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

    public interface ICloneable
    {
        object CloneObject();
    }

    public class CResourceState
    {
        public struct NativePointer : INativePointer
        {
            IntPtr Pointer;
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

        NativePointer mCoreObject;
        [Browsable(false)]
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        public static CResourceState CreateResourceState()
        {
            CResourceState result = new CResourceState(SDK_New_IResourceState());
            return result;
        }
        public static void DestroyResourceState(CResourceState state)
        {
            SDK_Delete_IResourceState(state.mCoreObject);
            state.mCoreObject.SetPointer(IntPtr.Zero);
        }
        public CResourceState(NativePointer self)
        {
            mCoreObject = self;
        }
        internal void UnsafeReInit(NativePointer self)
        {
            mCoreObject = self;
        }

        public EStreamingState StreamState
        {
            get
            {
                return SDK_IResourceState_GetStreamState(CoreObject);
            }
            set
            {
                SDK_IResourceState_SetStreamState(CoreObject, value);
            }
        }
        public Int64 AccessTime
        {
            get
            {
                return SDK_IResourceState_GetAccessTime(CoreObject);
            }
            set
            {
                SDK_IResourceState_SetAccessTime(CoreObject, value);
            }
        }

        public UInt32 ResourceSize
        {
            get
            {
                return SDK_IResourceState_GetResourceSize(CoreObject);
            }
            set
            {
                SDK_IResourceState_SetResourceSize(CoreObject, value);
            }
        }
        public bool KeepValid
        {
            get
            {
                return SDK_IResourceState_GetKeepValid(CoreObject)==0?false:true;
            }
            set
            {
                SDK_IResourceState_SetKeepValid(CoreObject, value?1:0);
            }
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_New_IResourceState();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_Delete_IResourceState(NativePointer state);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EStreamingState SDK_IResourceState_GetStreamState(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IResourceState_SetStreamState(NativePointer self, EStreamingState state);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IResourceState_GetResourceSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IResourceState_SetResourceSize(NativePointer self, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Int64 SDK_IResourceState_GetAccessTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IResourceState_SetAccessTime(NativePointer self, Int64 t);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_IResourceState_GetKeepValid(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IResourceState_SetKeepValid(NativePointer self, int keep);
        #endregion
    }

    public class CoreObjectBase
    {
#if PWindow
        public const string ModuleNC = @"Core.Windows.dll";
#elif PAndroid
        public const string ModuleNC = @"libCore.Droid.so";
#else
        public const string ModuleNC = @"__Internal";
#endif

        //public static T GetEmptyNativePointer<T>() where T : INativePointer, new()
        //{
        //    T tmp = new T();
        //    tmp.SetPointer(IntPtr.Zero);
        //    return tmp;
        //}

        public static T NewNativeObjectByNativeName<T>(string name) where T : INativePointer, new()
        {
            return NewNativeObjectByName<T>($"{CEngine.NativeNS}::{name}");
        }
        public static T NewNativeObjectByName<T>(string name) where T : INativePointer, new()
        {
            T tmp = new T();
            var ptr = SDK_CoreRttiManager_NewObjectByName(name);
            if(ptr==IntPtr.Zero)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Factory", $"New Native Object Failed:{name}");
            }
            tmp.SetPointer(ptr);
            return tmp;
        }

        #region SDK   
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_CoreRttiManager_NewObjectByName(string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_VIUnknown_AddRef(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_VIUnknown_Release(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_VIUnknown_Cleanup(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_VIUnknown_GetHash64(IntPtr self, Hash64* hash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CResourceState.NativePointer SDK_VIUnknown_GetResourceState(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_VIUnknown_InvalidateResource(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_VIUnknown_RestoreResource(IntPtr self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Rtti.NativeRtti.NativePointer SDK_VIUnknown_GetRtti(IntPtr self);
        #endregion
    }

    public class AuxCoreObject<HandleType> : CoreObjectBase, IDisposable where HandleType : unmanaged, INativePointer
    {
        ~AuxCoreObject()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            if (mCoreObject.GetPointer() != IntPtr.Zero)
            {
                SDK_VIUnknown_Release(mCoreObject.GetPointer());
                mCoreObject.SetPointer(IntPtr.Zero);
            }
        }
        static HandleType EmtpyNativePointer = new HandleType();
        public static HandleType GetEmptyNativePointer()
        {
            return EmtpyNativePointer;
        }
        protected HandleType mCoreObject;
        [Browsable(false)]
        public HandleType CoreObject
        {
            get { return mCoreObject; }
        }
        public virtual void Cleanup()
        {
            if (mCoreObject.GetPointer() != IntPtr.Zero)
            {
                SDK_VIUnknown_Cleanup(mCoreObject.GetPointer());
            }
        }
        public int Core_GetRef()
        {
            var ptr = CoreObject.GetPointer();
            if (ptr == IntPtr.Zero)
                return 0;
            int count = Core_AddRef();
            Core_Release();
            return count - 1;
        }
        public int Core_AddRef()
        {
            if (CoreObject.GetPointer() != IntPtr.Zero)
                return SDK_VIUnknown_AddRef(CoreObject.GetPointer());
            return 0;
        }
        public void Core_Release(bool setNull = false)
        {
            if (CoreObject.GetPointer() != IntPtr.Zero)
            {
                SDK_VIUnknown_Release(CoreObject.GetPointer());
                if(setNull)
                {
                    mCoreObject.SetPointer(IntPtr.Zero);
                }
            }
        }
        public virtual Hash64 GetHash64()
        {
            if (mCoreObject.GetPointer() == IntPtr.Zero)
                return Hash64.Empty;
            unsafe
            {
                Hash64 tmp;
                SDK_VIUnknown_GetHash64(mCoreObject.GetPointer(), &tmp);
                return tmp;
            }
        }
        public void InvalidateResource()
        {
            if (mCoreObject.GetPointer() == IntPtr.Zero)
                return;

            SDK_VIUnknown_InvalidateResource(mCoreObject.GetPointer());
        }
        public bool RestoreResource()
        {
            if (mCoreObject.GetPointer() == IntPtr.Zero)
                return false;

            return (bool)SDK_VIUnknown_RestoreResource(mCoreObject.GetPointer());
        }
        [Browsable(false)]
        public Rtti.NativeRtti RTTI
        {
            get
            {
                return new Rtti.NativeRtti(SDK_VIUnknown_GetRtti(mCoreObject.GetPointer()));
            }
        }
    }
    public class AuxIOCoreObject<HandleType> : AuxCoreObject<HandleType>,IO.Serializer.ISerializer 
                    where HandleType : unmanaged, INativePointer
    {
          #region Serializer
        public void ReadObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }

        public void ReadObject(IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }

        public void ReadObject(IReader pkg, MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void WriteObject(IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }

        public void WriteObject(IWriter pkg, MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion
    }

    public class CStringObject : AuxCoreObject<CStringObject.NativePointer>
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
        public CStringObject()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::VStringObject");
        }
        public CStringObject(NativePointer self)
        {
            mCoreObject = self;
        }
        public string Text
        {
            get
            {
                var ptr = SDK_VStringObject_GetTextString(mCoreObject);
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_VStringObject_GetTextString(NativePointer self);
        #endregion
    }
}
