using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class CEngine
    {
        public void ClearCache()
        {
            foreach(var member in AutoMembers)
            {
                member.Key.ClearCache(member.Value);
            }

            Rtti.RttiHelper.ClearTypeTable();
            this.PrefabManager.Prefabs.Clear();
            Rtti.RttiHelper.ClearRuntimAssemblyCache();
            Rtti.VAssemblyManager.Instance.ClearCache();
        }

        public delegate object FShowPropertyGridInWindows(object insObj, object inspector);
        public static FShowPropertyGridInWindows mShowPropertyGridInWindows;
        public static object ShowPropertyGridInWindows(object insObj, object inspector)
        {
            if (mShowPropertyGridInWindows != null)
            {
                return mShowPropertyGridInWindows(insObj, inspector);
            }
            return null;
        }
    }
}
