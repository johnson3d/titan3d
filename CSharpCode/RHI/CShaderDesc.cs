using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CShaderDefinitions : AuxPtrType<IShaderDefinitions>
    {
        public CShaderDefinitions()
        {
            mCoreObject = IShaderDefinitions.CreateInstance();
        }
    }
    public class CShaderDesc : AuxPtrType<IShaderDesc>
    {
        public CShaderDesc()
        {
            mCoreObject = IShaderDesc.CreateInstance();
        }
        public CShaderDesc(IShaderDesc ptr)
        {
            mCoreObject = ptr;
        }
        public CShaderDesc(EShaderType type)
        {
            mCoreObject = IShaderDesc.CreateInstance(type);
        }
    }
}
