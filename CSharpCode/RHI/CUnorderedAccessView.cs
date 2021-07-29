using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS
{
    partial struct IUnorderedAccessViewDesc
    {
        //union
        //{
        [FieldOffset(8)]
        public IBufferUAV Buffer;
        [FieldOffset(8)]
        public ITex1DUAV Texture1D;
        [FieldOffset(8)]
        public ITex1DArrayUAV Texture1DArray;
        [FieldOffset(8)]
        public ITex2DUAV Texture2D;
        [FieldOffset(8)]
        public ITex2DArrayUAV Texture2DArray;
        [FieldOffset(8)]
        public ITex3DUAV Texture3D;
        //};
    }
}

namespace EngineNS.RHI
{
    public class CUnorderedAccessView : AuxPtrType<IUnorderedAccessView>
    {
    }
}
