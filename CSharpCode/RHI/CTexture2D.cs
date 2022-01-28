using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CTexture2D : CGpuBuffer //AuxPtrType<ITexture2D>
    {
        public ITexture2D GetTextureCore()
        {
            unsafe
            {
                return new ITexture2D(mCoreObject.CppPointer);
            }
        }
    }
}
