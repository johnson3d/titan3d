using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EIndexBufferType : int
    {
        IBT_Int16,
        IBT_Int32,
    };
    public struct CIndexBufferDesc
    {
        public CIndexBufferDesc(EIndexBufferType type = EIndexBufferType.IBT_Int16)
        {
            //CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
            CPUAccess = 0;
            Type = type;
            ByteWidth = 0;
            InitData = IntPtr.Zero;
        }
        public void ToDefault()
        {
            CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
            Type = EIndexBufferType.IBT_Int16;
        }
        public UInt32 CPUAccess;
        public UInt32 ByteWidth;
        public EIndexBufferType Type;
        public IntPtr InitData;
    }
    public class CIndexBuffer : AuxCoreObject<CIndexBuffer.NativePointer>
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

        public CIndexBuffer(NativePointer self)
        {
            mCoreObject = self;
        }
        public CIndexBufferDesc Desc
        {
            get
            {
                unsafe
                {
                    CIndexBufferDesc desc = new CIndexBufferDesc();
                    SDK_IIndexBuffer_GetDesc(CoreObject, &desc);
                    return desc;
                }
            }
        }
        public void GetBufferData(CRenderContext rc, Support.CBlobObject blob)
        {
            SDK_IIndexBuffer_GetBufferData(CoreObject, rc.CoreObject, blob.CoreObject);
        }

        public void UpdateBuffData(CCommandList cmd, IntPtr ptr, UInt32 size)
        {
            lock (cmd)
            {
                SDK_IIndexBuffer_UpdateGPUBuffData(CoreObject, cmd.CoreObject, ptr, size);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IIndexBuffer_GetDesc(NativePointer self, CIndexBufferDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IIndexBuffer_GetBufferData(NativePointer self, CRenderContext.NativePointer rc, Support.CBlobObject.NativePointer blob);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IIndexBuffer_UpdateGPUBuffData(NativePointer self, CCommandList.NativePointer cmd, IntPtr ptr, UInt32 size);
        #endregion
    }
}
