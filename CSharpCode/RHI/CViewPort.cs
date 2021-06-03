using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CViewPort : AuxPtrType<IViewPort>
    {
        public CViewPort()
        {
            mCoreObject = IViewPort.CreateInstance();
        }
    }
    public class CScissorRect : AuxPtrType<IScissorRect>
    {
        public CScissorRect()
        {
            mCoreObject = IScissorRect.CreateInstance();
        }
    }
}
