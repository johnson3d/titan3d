using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public class UMacrossAttribute : Attribute
    {
    }

    [UMacross]
    public class UMacrossTestClass
    {
        public virtual void VirtualFunc1()
        {

        }
        protected virtual void ProtectedVirtualFunc()
        {

        }
        public virtual int VirtualFunc2()
        {
            return 0;
        }
        public virtual void VirtualFunc3(int val1)
        {

        }
        public virtual void VirtualFunc4(in int inValue, out int outValue, ref int refValue)
        {
            outValue = 0;
        }
        public virtual void VirtualFunc_params(params int[] values)
        {

        }
    }
}
