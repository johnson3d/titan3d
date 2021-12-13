using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    partial struct IRenderTargetViewDesc
    {
        [System.Runtime.InteropServices.FieldOffset(36)]
        public BUFFER_RTV Buffer;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public TEX1D_RTV Texture1D;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public TEX1D_ARRAY_RTV Texture1DArray;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public TEX2D_RTV Texture2D;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public TEX2D_ARRAY_RTV Texture2DArray;
    }
}


namespace EngineNS.RHI
{
    public class CRenderTargetView : AuxPtrType<IRenderTargetView>
    {
    }
}
