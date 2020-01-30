using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct CDepthStencilViewDesc
    {
        public void Init()
        {
            Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            mCanBeSampled = vBOOL.FromBoolean(true);
            mUseStencil = vBOOL.FromBoolean(false);
            mTexture2D = CTexture2D.GetEmptyNativePointer();
            CPUAccess = 0;
        }
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 Width;
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 Height;
        [Editor.Editor_ShowInPropertyGrid]
        public EPixelFormat Format;
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 CPUAccess;
        public vBOOL mCanBeSampled;
        public vBOOL mUseStencil;
        public CTexture2D.NativePointer mTexture2D;
    }
    public class CDepthStencilView : AuxCoreObject<CDepthStencilView.NativePointer>
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

        public CDepthStencilView(NativePointer self)
        {
            mCoreObject = self;
        }

        
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderResourceView.NativePointer SDK_IDepthStencilView_GetTexture2D(NativePointer self);
        #endregion
    }
}
