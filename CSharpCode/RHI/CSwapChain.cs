using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CSwapChain : AuxPtrType<ISwapChain>
    {
        public unsafe void OnResize(float width, float height)
        {
            var desc = new ISwapChainDesc();
            mCoreObject.GetDesc(&desc);
            mCoreObject.OnLost();
            desc.Width = (uint)width;
            desc.Height = (uint)height;
            mCoreObject.OnRestore(&desc);
        }
        public CShaderResourceView[] BackBufferSRV { get; set; }
    }
}
