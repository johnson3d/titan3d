using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class Converter
    {
        [Rtti.Meta("")]
        public static string ToString(float value)
        {
            return System.Convert.ToString(value);
        }
        [Rtti.Meta("")]
        public static string ToString(double value)
        {
            return System.Convert.ToString(value);
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS
{
	partial class Converter
	{
		public static unsafe string macross_ToString (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, float value) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = ToString(value);
			return _return_value;
		}
		public static unsafe string macross_ToString (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, double value) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = ToString(value);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross