using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [Flags]
    public enum ECpuAccess : UInt32
    {
        CAS_WRITE = 0x10000,
        CAS_READ = 0x20000,
    }
    public struct CVertexBufferDesc
    {
        public UInt32 CPUAccess
        {
            get;
            set;
        }
        public UInt32 ByteWidth
        {
            get;
            set;
        }
        public UInt32 Stride
        {
            get;
            set;
        }
        public IntPtr InitData
        {
            get;
            set;
        }
    }
    public class CVertexBuffer : AuxCoreObject<CVertexBuffer.NativePointer>
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

        public CVertexBuffer(NativePointer self)
        {
            mCoreObject = self;
        }
        //这个函数要从显卡取数据，根据目前的渲染器结构，要求在Render线程执行，调用的时候要注意Post
        public void GetBufferData(CRenderContext rc, Support.CBlobObject blob)
        {
            SDK_IVertexBuffer_GetBufferData(CoreObject, rc.CoreObject, blob.CoreObject);
        }
        public void UpdateBuffData(CCommandList cmd, IntPtr ptr, UInt32 size)
        {
            lock (cmd)
            {
                SDK_IVertexBuffer_UpdateGPUBuffData(CoreObject, cmd.CoreObject, ptr, size);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IVertexBuffer_GetBufferData(NativePointer self, CRenderContext.NativePointer rc, Support.CBlobObject.NativePointer blob);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IVertexBuffer_UpdateGPUBuffData(NativePointer self, CCommandList.NativePointer cmd, IntPtr ptr, UInt32 size);
        #endregion
    }
}
