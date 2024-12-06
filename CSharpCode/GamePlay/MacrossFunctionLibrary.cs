using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Rtti.Meta]
    public partial class TtMacrossFunctionLibrary
    {
        [Rtti.Meta]
        public static bool InstantiatePrefab(RName prefab, TtScene scene)
        {

            return false;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay
{
	partial class TtMacrossFunctionLibrary
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_InstantiatePrefab_659189569 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossFunctionLibrary->static bool InstantiatePrefab(RName prefab, TtScene scene)");
		public static unsafe bool macross_InstantiatePrefab (string nodeName, RName prefab, TtScene scene) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":prefab", prefab);
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
				}
			}
			var _return_value = InstantiatePrefab(prefab, scene);
			macross_break_InstantiatePrefab_659189569.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross