using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.PythonWrapper
{
    partial struct FPyTuple : IDisposable
    {
        public void Dispose()
        {
            //release
        }
    }
}

namespace EngineNS.Bricks.PythonRuntime
{
    public class TtPyTuple : AuxPtrType<PythonWrapper.FPyTuple>
    {
        public TtPyTuple(uint num)
        {
            mCoreObject = PythonWrapper.FPyTuple.CreateInstance(num);
        }
        public unsafe void SetItem(uint index, FPyValue value)
        {
            mCoreObject.SetItem(index, value.mPyObject);
        }
    }
}
