using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CInputLayoutDesc : AuxPtrType<IInputLayoutDesc>
    {
        public CInputLayoutDesc()
        {
            mCoreObject = IInputLayoutDesc.CreateInstance();
        }
    }
    public class CInputLayout : AuxPtrType<IInputLayout>
    {
        public CInputLayout()
        {
            
        }
    }
}
