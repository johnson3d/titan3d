using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UInputLayoutDesc : AuxPtrType<NxRHI.FInputLayoutDesc>
    {
        public UInputLayoutDesc(FInputLayoutDesc ptr)
        {
            mCoreObject = ptr;
        }
        public UInputLayoutDesc()
        {
            mCoreObject = FInputLayoutDesc.CreateInstance();
        }
        public void AddElement(string SemanticName,
            uint SemanticIndex,
			EPixelFormat Format,
            EVertexStreamType InputSlot,
            uint AlignedByteOffset,
            bool IsInstanceData,
            uint InstanceDataStepRate)
        {
            mCoreObject.AddElement(SemanticName, SemanticIndex, Format, InputSlot, AlignedByteOffset,
                IsInstanceData?1:0, InstanceDataStepRate);
        }
        public ulong GetLayoutHash64()
        {
            return mCoreObject.GetLayoutHash64();
        }
    }
    public class UInputLayout : AuxPtrType<NxRHI.IInputLayout>
    {
    }
}
