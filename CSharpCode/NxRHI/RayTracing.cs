using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtAccelerationStructure : AuxPtrType<NxRHI.IAccelerationStructure>
    {
        public TtAccelerationStructure(IAccelerationStructure ptr)
        {
            mCoreObject = ptr;
        }
    }
    public class TtAStructureInstance : AuxPtrType<NxRHI.IAStructureInstance>
    {
        public TtAStructureInstance(IAStructureInstance ptr)
        {
            mCoreObject = ptr;
        }
        public unsafe ref FAStructureInstanceDesc GetDescPtr()
        {
            return ref *mCoreObject.GetDescPtr();
        }
    }
    public class TtTopAccelerationStructure : AuxPtrType<NxRHI.ITopAccelerationStructure>
    {
        public TtTopAccelerationStructure(ITopAccelerationStructure ptr)
        {
            mCoreObject = ptr;
        }
        public uint BLASInstanceCount
        {
            get
            {
                return mCoreObject.GetBLASInstanceCount();
            }
        }
        public IAStructureInstance GetBLASInstance(uint index)
        {
            return mCoreObject.GetBLASInstance(index);
        }
        public void AddBLASInstance(TtAStructureInstance instance)
        {
            mCoreObject.AddBLASInstance(instance.mCoreObject);
        }
        public TtSrView mGpuBufferSRV = null;
        public bool BuildAcclerationStruture()
        {
            var ret = mCoreObject.BuildAcclerationStruture();
            return ret;
        }
    }
}
