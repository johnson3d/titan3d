using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor.UI
{
    [EngineNS.Rtti.Meta]
    public partial class TtBattleStatics
    {
        [EngineNS.Rtti.Meta]
        public float GetHP()
        {
            return 0;
        }
        [EngineNS.Rtti.Meta]
        public float GetBigLevel()
        {
            return 0;
        }
        [EngineNS.Rtti.Meta]
        public float GetSmallLevel()
        {
            return 0;
        }
        [EngineNS.Rtti.Meta]
        public float GetCurentExp()
        {
            return 0;
        }
        [EngineNS.Rtti.Meta]
        public string BingLevelString { get; } = "����";
		
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor.UI
{
	partial class TtBattleStatics
	{
		public unsafe float macross_GetHP (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetHP();
			return _return_value;
		}
		public unsafe float macross_GetBigLevel (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetBigLevel();
			return _return_value;
		}
		public unsafe float macross_GetSmallLevel (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetSmallLevel();
			return _return_value;
		}
		public unsafe float macross_GetCurentExp (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetCurentExp();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross