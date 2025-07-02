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
		private static EngineNS.Macross.TtMacrossBreak macross_break_ToString_1155064225 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Converter->static string ToString(float value)");
		public static unsafe string macross_ToString (string nodeName, float value) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":value", value);
				}
			}
			var _return_value = ToString(value);
			macross_break_ToString_1155064225.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_ToString_2527355654 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Converter->static string ToString(double value)");
		public static unsafe string macross_ToString (string nodeName, double value) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":value", value);
				}
			}
			var _return_value = ToString(value);
			macross_break_ToString_2527355654.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross