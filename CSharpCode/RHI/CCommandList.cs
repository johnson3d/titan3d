using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CCommandList : AuxPtrType<ICommandList>
    {
    }

    public class CPipelineStat : AuxPtrType<IPipelineStat>
    {
        public CPipelineStat()
        {
            mCoreObject = IPipelineStat.CreateInstance();
        }
    }
}
