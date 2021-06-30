using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CRasterizerState : AuxPtrType<IRasterizerState>
    {
        public IRasterizerStateDesc Desc
        {
            get
            {
                var result = new IRasterizerStateDesc();
                mCoreObject.GetDesc(ref result);
                return result;
            }
        }
    }
}
