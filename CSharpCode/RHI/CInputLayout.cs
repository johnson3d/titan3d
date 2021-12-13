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
        public void SetShaderDesc(CShaderProgram prog)
        {
            mCoreObject.SetShaderDesc(prog.mCoreObject.GetVertexShader().NativeSuper.GetDesc());
        }
    }
    public class CInputLayout : AuxPtrType<IInputLayout>
    {
        public CInputLayout()
        {
            
        }
    }
}
