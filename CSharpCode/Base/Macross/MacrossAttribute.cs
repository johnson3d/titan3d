using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Macross
{
    public class TtMacrossCustomCodeGenAttribute : Attribute
    {
        public virtual void GenCustomCode(TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {

		}
	}
	public class TtMacrossSignAttribute : Attribute
	{
		public string RName_Name;
		public RName.ERNameType RName_Type;
    }
	public class TtMacrossAttribute : Attribute
    {
        public bool IsGenShader = false;
    }
    [TtMacross]
    public partial class BaseClass
    {
        [Rtti.Meta("")]
        public void BaseFunc() { }
    }
    [TtMacross]
    public partial class SubClass1 : BaseClass
    {
        [Rtti.Meta("")]
        public void SubFunc1() { }

        [Rtti.Meta("")]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
    }
    [TtMacross]
    public partial class SubClass2 : BaseClass
    {
        [Rtti.Meta("")]
        public void SubFunc2() { }

        [Rtti.Meta("")]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
    }

    [TtMacross]
    public partial class UMacrossTestClass
    {
        [Rtti.Meta("")]
        public virtual void VirtualFunc1()
        {

        }
        [Rtti.Meta("")]
        protected virtual void ProtectedVirtualFunc()
        {

        }
        [Rtti.Meta("")]
        public virtual int VirtualFunc2()
        {
            return 0;
        }
        [Rtti.Meta("")]
        public virtual void VirtualFunc3(int val1)
        {

        }
        [Rtti.Meta("")]
        public virtual void VirtualFunc4(in int inValue, out int outValue, ref int refValue)
        {
            outValue = 0;
        }
        [Rtti.Meta("")]
        public virtual void VirtualFunc_params(params int[] values)
        {

        }
        [Rtti.Meta("")]
        public float FuncFloat(float fVal) { return fVal; }

        [Rtti.Meta("")]
        public BaseClass UseBaseClassFunc(BaseClass item) { return null; }
        [Rtti.Meta("")]
        public BaseClass UseBaseClassFuncRef(ref BaseClass item) { return null; }
        [Rtti.Meta("")]
        public SubClass1 UseSubClass1Func(SubClass1 item) { return null; }
        [Rtti.Meta("")]
        public SubClass2 UseSubClass2Func(SubClass2 item) { return null; }

        [Rtti.Meta("")]
        public int IntProperty { get; set; } = 10;

        public delegate Task<bool> Delegate_DelegateTest(int intParam);
        [Rtti.Meta("")]
        public void DelegateFunc(Delegate_DelegateTest func) { }

        [Rtti.Meta("")]
        public virtual async Task<bool> TaskFunction() 
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return false;
        }

        [Rtti.Meta("")]
        public unsafe int* UnsafeFunction(float inValue)
        {
            return (int*)IntPtr.Zero;
        }
    }
}



#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Macross
{
	partial class BaseClass
	{
		public unsafe void macross_BaseFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			BaseFunc();
		}
	}
}


namespace EngineNS.Macross
{
	partial class SubClass1
	{
		public unsafe void macross_SubFunc1 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SubFunc1();
		}
		public unsafe BaseClass macross_UseBaseClassFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, BaseClass item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseBaseClassFunc(item);
			return _return_value;
		}
	}
}


namespace EngineNS.Macross
{
	partial class SubClass2
	{
		public unsafe void macross_SubFunc2 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SubFunc2();
		}
		public unsafe BaseClass macross_UseBaseClassFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, BaseClass item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseBaseClassFunc(item);
			return _return_value;
		}
	}
}


namespace EngineNS.Macross
{
	partial class UMacrossTestClass
	{
		public unsafe void macross_VirtualFunc1 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			VirtualFunc1();
		}
		public unsafe void macross_ProtectedVirtualFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			ProtectedVirtualFunc();
		}
		public unsafe int macross_VirtualFunc2 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = VirtualFunc2();
			return _return_value;
		}
		public unsafe void macross_VirtualFunc3 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, int val1) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			VirtualFunc3(val1);
		}
		public unsafe void macross_VirtualFunc4 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, in int inValue, out int outValue, ref int refValue) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			VirtualFunc4(in inValue, out outValue, ref refValue);
		}
		public unsafe void macross_VirtualFunc_params (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, int[] values) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			VirtualFunc_params(values);
		}
		public unsafe float macross_FuncFloat (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float fVal) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FuncFloat(fVal);
			return _return_value;
		}
		public unsafe BaseClass macross_UseBaseClassFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, BaseClass item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseBaseClassFunc(item);
			return _return_value;
		}
		public unsafe BaseClass macross_UseBaseClassFuncRef (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, ref BaseClass item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseBaseClassFuncRef(ref item);
			return _return_value;
		}
		public unsafe SubClass1 macross_UseSubClass1Func (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, SubClass1 item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseSubClass1Func(item);
			return _return_value;
		}
		public unsafe SubClass2 macross_UseSubClass2Func (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, SubClass2 item) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UseSubClass2Func(item);
			return _return_value;
		}
		public unsafe void macross_DelegateFunc (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Delegate_DelegateTest func) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			DelegateFunc(func);
		}
		public async Task<bool> macross_TaskFunction (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await TaskFunction();
			return _return_value;
		}
		public unsafe int* macross_UnsafeFunction (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float inValue) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = UnsafeFunction(inValue);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross