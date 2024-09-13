using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtInputLayoutDesc : AuxPtrType<NxRHI.FInputLayoutDesc>
    {
        public TtInputLayoutDesc(FInputLayoutDesc ptr)
        {
            mCoreObject = ptr;
        }
        public TtInputLayoutDesc()
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
    public class TtInputLayout : AuxPtrType<NxRHI.IInputLayout>
    {
    }
}
