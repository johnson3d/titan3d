using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Macross
{
    public class UMacrossAttribute : Attribute
    {
    }
    [UMacross]
    public partial class BaseClass
    {
        [Rtti.Meta]
        public void BaseFunc() { }
    }
    [UMacross]
    public partial class SubClass1 : BaseClass
    {
        [Rtti.Meta]
        public void SubFunc1() { }

        [Rtti.Meta]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
    }
    [UMacross]
    public partial class SubClass2 : BaseClass
    {
        [Rtti.Meta]
        public void SubFunc2() { }

        [Rtti.Meta]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
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
        public float FuncFloat(float fVal) { return fVal; }

        [Rtti.Meta]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
        [Rtti.Meta]
        public BaseClass UseBaseClassFuncRef(ref BaseClass item) { return null; }
        [Rtti.Meta]
        public SubClass1 UseSubClass1Func(SubClass1 item) { return null; }
        [Rtti.Meta]
        public SubClass2 UseSubClass2Func(SubClass2 item) { return null; }

        [Rtti.Meta]
        public int IntProperty { get; set; } = 10;

        public delegate Task<bool> Delegate_DelegateTest(int intParam);
        [Rtti.Meta]
        public void DelegateFunc(Delegate_DelegateTest func) { }

        [Rtti.Meta]
        public virtual async Task<bool> TaskFunction() 
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return false;
        }

        [Rtti.Meta]
        public unsafe int* UnsafeFunction(float inValue)
        {
            return (int*)IntPtr.Zero;
        }
    }
}
