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
		private static EngineNS.Macross.TtMacrossBreak macross_break_BaseFunc_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.BaseClass->void BaseFunc()");
		public unsafe void macross_BaseFunc (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			BaseFunc();
			macross_break_BaseFunc_2609910045.TryBreak();
		}
	}
}


namespace EngineNS.Macross
{
	partial class SubClass1
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_SubFunc1_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.SubClass1->void SubFunc1()");
		public unsafe void macross_SubFunc1 (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			SubFunc1();
			macross_break_SubFunc1_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseBaseClassFunc_368511151 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.SubClass1->BaseClass UseBaseClassFunc(BaseClass item)");
		public unsafe BaseClass macross_UseBaseClassFunc (string nodeName, BaseClass item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseBaseClassFunc(item);
			macross_break_UseBaseClassFunc_368511151.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS.Macross
{
	partial class SubClass2
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_SubFunc2_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.SubClass2->void SubFunc2()");
		public unsafe void macross_SubFunc2 (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			SubFunc2();
			macross_break_SubFunc2_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseBaseClassFunc_368511151 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.SubClass2->BaseClass UseBaseClassFunc(BaseClass item)");
		public unsafe BaseClass macross_UseBaseClassFunc (string nodeName, BaseClass item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseBaseClassFunc(item);
			macross_break_UseBaseClassFunc_368511151.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS.Macross
{
	partial class UMacrossTestClass
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_VirtualFunc1_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void VirtualFunc1()");
		public unsafe void macross_VirtualFunc1 (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			VirtualFunc1();
			macross_break_VirtualFunc1_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ProtectedVirtualFunc_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void ProtectedVirtualFunc()");
		public unsafe void macross_ProtectedVirtualFunc (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			ProtectedVirtualFunc();
			macross_break_ProtectedVirtualFunc_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_VirtualFunc2_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->int VirtualFunc2()");
		public unsafe int macross_VirtualFunc2 (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = VirtualFunc2();
			macross_break_VirtualFunc2_2609910045.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_VirtualFunc3_3538359930 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void VirtualFunc3(int val1)");
		public unsafe void macross_VirtualFunc3 (string nodeName, int val1) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":val1", val1);
				}
			}
			VirtualFunc3(val1);
			macross_break_VirtualFunc3_3538359930.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_VirtualFunc4_611716433 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void VirtualFunc4(in int inValue, out int outValue, ref int refValue)");
		public unsafe void macross_VirtualFunc4 (string nodeName, in int inValue, out int outValue, ref int refValue) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":inValue", inValue);
					stackframe.SetWatchVariable(nodeName + ":refValue", refValue);
				}
			}
			VirtualFunc4(in inValue, out outValue, ref refValue);
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":outValue", outValue);
				}
			}
			macross_break_VirtualFunc4_611716433.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_VirtualFunc_params_1097178958 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void VirtualFunc_params(int[] values)");
		public unsafe void macross_VirtualFunc_params (string nodeName, int[] values) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":values", values);
				}
			}
			VirtualFunc_params(values);
			macross_break_VirtualFunc_params_1097178958.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FuncFloat_2659928112 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->float FuncFloat(float fVal)");
		public unsafe float macross_FuncFloat (string nodeName, float fVal) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":fVal", fVal);
				}
			}
			var _return_value = FuncFloat(fVal);
			macross_break_FuncFloat_2659928112.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseBaseClassFunc_368511151 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->BaseClass UseBaseClassFunc(BaseClass item)");
		public unsafe BaseClass macross_UseBaseClassFunc (string nodeName, BaseClass item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseBaseClassFunc(item);
			macross_break_UseBaseClassFunc_368511151.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseBaseClassFuncRef_2095380354 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->BaseClass UseBaseClassFuncRef(ref BaseClass item)");
		public unsafe BaseClass macross_UseBaseClassFuncRef (string nodeName, ref BaseClass item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseBaseClassFuncRef(ref item);
			macross_break_UseBaseClassFuncRef_2095380354.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseSubClass1Func_2236911147 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->SubClass1 UseSubClass1Func(SubClass1 item)");
		public unsafe SubClass1 macross_UseSubClass1Func (string nodeName, SubClass1 item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseSubClass1Func(item);
			macross_break_UseSubClass1Func_2236911147.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseSubClass2Func_2266639564 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->SubClass2 UseSubClass2Func(SubClass2 item)");
		public unsafe SubClass2 macross_UseSubClass2Func (string nodeName, SubClass2 item) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":item", item);
				}
			}
			var _return_value = UseSubClass2Func(item);
			macross_break_UseSubClass2Func_2266639564.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_DelegateFunc_1915121968 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->void DelegateFunc(Delegate_DelegateTest func)");
		public unsafe void macross_DelegateFunc (string nodeName, Delegate_DelegateTest func) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":func", func);
				}
			}
			DelegateFunc(func);
			macross_break_DelegateFunc_1915121968.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TaskFunction_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->Task<bool> TaskFunction()");
		public async Task<bool> macross_TaskFunction (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await TaskFunction();
			macross_break_TaskFunction_2609910045.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_UnsafeFunction_1055634221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Macross.UMacrossTestClass->int* UnsafeFunction(float inValue)");
		public unsafe int* macross_UnsafeFunction (string nodeName, float inValue) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":inValue", inValue);
				}
			}
			var _return_value = UnsafeFunction(inValue);
			macross_break_UnsafeFunction_1055634221.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross