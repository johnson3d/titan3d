using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    public class UMacrossAttribute : Attribute
    {
    }

    [UMacross]
    public partial class UMacrossTestClass
    {
        [Rtti.Meta]
        public virtual void VirtualFunc1()
        {

        }
        [Rtti.Meta]
        protected virtual void ProtectedVirtualFunc()
        {

        }
        [Rtti.Meta]
        public virtual int VirtualFunc2()
        {
            return 0;
        }
        [Rtti.Meta]
        public virtual void VirtualFunc3(int val1)
        {

        }
        [Rtti.Meta]
        public virtual void VirtualFunc4(in int inValue, out int outValue, ref int refValue)
        {
            outValue = 0;
        }
        [Rtti.Meta]
        public virtual void VirtualFunc_params(params int[] values)
        {

        }

        [Rtti.Meta]
        public int IntProperty { get; set; } = 10;
    }
}
