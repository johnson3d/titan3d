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
