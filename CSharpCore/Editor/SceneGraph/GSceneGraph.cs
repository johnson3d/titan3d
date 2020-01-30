using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        public static string GeneratorActorSpecialNameInEditor(string origionName, GWorld world)
        {
            if (world == null || string.IsNullOrEmpty(origionName))
                return origionName;
            int index = 1;
            bool isSameName = false;
            var actors = new List<EngineNS.GamePlay.Actor.GActor>(world.Actors.Values);
            actors.Sort((EngineNS.GamePlay.Actor.GActor a, EngineNS.GamePlay.Actor.GActor b) =>
            {
                if (a.SpecialName == null && b.SpecialName == null)
                    return 0;
                if (a.SpecialName != null && b.SpecialName == null)
                    return 1;
                if (a.SpecialName == null && b.SpecialName != null)
                    return -1;
                return a.SpecialName.CompareTo(b.SpecialName);
            });
            foreach (var actor in actors)
            {
                var spName = actor.SpecialName;
                if (string.IsNullOrEmpty(spName))
                    continue;
                if (!spName.Contains(origionName))
                    continue;
                isSameName = true;
                var idx1 = spName.LastIndexOf('(');
                if (idx1 < 0)
                {
                    continue;
                }
                else
                {
                    try
                    {
                        origionName = origionName.Substring(0, idx1);
                        var idx2 = spName.LastIndexOf(')');
                        int idx = -1;
                        if (int.TryParse(spName.Substring(idx1 + 1, idx2 - idx1 - 1), out idx))
                        {
                            if (index <= idx)
                                index = idx + 1;
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            if (!isSameName)
                return origionName;
            else
            {
                return origionName + "(" + (index) + ")";
            }
        }
    }
}
