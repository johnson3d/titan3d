using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CSamplerState : AuxPtrType<ISamplerState>
    {
        public ISamplerStateDesc Desc
        {
            get
            {
                return mCoreObject.mDesc;
            }
        }
    }
}
