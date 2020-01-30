using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct CRenderTargetViewDesc
    {
        public void Init()
        {
            mCanBeSampled = vBOOL.FromBoolean(true);
            Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            mTexture2D = CTexture2D.GetEmptyNativePointer();
        }
        [Editor.Editor_ShowInPropertyGrid]
        [System.Xml.Serialization.XmlIgnore]
        public CTexture2D.NativePointer mTexture2D;
        [Editor.Editor_ShowInPropertyGrid]
        public vBOOL mCanBeSampled;
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 Width;
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 Height;
        [Editor.Editor_ShowInPropertyGrid]
        public EPixelFormat Format;
    };
    public class CRenderTargetView : AuxCoreObject<CRenderTargetView.NativePointer>
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

        public CRenderTargetView(NativePointer self)
        {
            mCoreObject = self;
        }
        

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static CShaderResourceView.NativePointer SDK_IRenderTargetView_GetTargetTexture(NativePointer self);
        #endregion
    }
}
