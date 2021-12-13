using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CVertexArray : AuxPtrType<IVertexArray>
    {
        public CVertexArray()
        {
            mCoreObject = IVertexArray.CreateInstance();
        }
    }
}
