using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Actor
{
    partial class GActor
    {
        partial void OnEditorCheckVisible(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game)
                return;
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i].OnEditorCommitVisual(cmd, camera, param);
            }
        }
        public static async System.Threading.Tasks.Task AnalysisActorRefResource(EngineNS.GamePlay.Actor.GActor actor, HashSet<RName> refObjectHashSet, HashSet<object> visitedObjects = null)
        {
            if (visitedObjects == null)
                visitedObjects = new HashSet<object>();
            await CollectRNameFromObject(actor, refObjectHashSet, visitedObjects);
            foreach (var i in actor.Components)
            {
                await CollectRNameFromObject(i, refObjectHashSet, visitedObjects);
            }
            var children = actor.GetChildrenUnsafe();
            foreach (var i in children)
            {
                await AnalysisActorRefResource(i, refObjectHashSet, visitedObjects);
            }
        }
        public static async System.Threading.Tasks.Task CollectRNameFromObject(object obj, HashSet<RName> refObjectHashSet, HashSet<object> visitedObjects = null)
        {
            if (visitedObjects == null)
                visitedObjects = new HashSet<object>();
            if (obj == null || obj.GetType().FullName == "System.RuntimeType")
                return;
            var props = obj.GetType().GetProperties();
            foreach (var i in props)
            {
                if (i.PropertyType == typeof(RName))
                {
                    var rn = i.GetValue(obj) as RName;
                    if (rn != null)
                    {
                        var ext = CEngine.Instance.FileManager.GetFileExtension(rn.Address);
                        if (string.IsNullOrEmpty(ext))
                            continue;

                        if (refObjectHashSet.Contains(rn) == false)
                        {
                            refObjectHashSet.Add(rn);
                        }
                    }
                }
                else if (i.PropertyType.IsValueType == false)
                {
                    if (i.GetIndexParameters().Length != 0)
                        continue;
                    object member = null;
                    try
                    {
                        member = i.GetValue(obj);
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        continue;
                    }
                    if (member != null)
                    {
                        if (visitedObjects.Contains(member) == false)
                        {
                            visitedObjects.Add(member);

                            var lst = member as System.Collections.IList;
                            var dict = member as System.Collections.IDictionary;
                            if (lst != null)
                            {
                                foreach (var elem in lst)
                                {
                                    if (elem == null)
                                        continue;
                                    if (elem.GetType().IsValueType == false)
                                    {
                                        await CollectRNameFromObject(elem, refObjectHashSet, visitedObjects);
                                    }
                                }
                            }
                            else if (dict != null)
                            {
                                foreach (var elem in dict.Values)
                                {
                                    if (elem == null)
                                        continue;
                                    if (elem.GetType().IsValueType == false)
                                    {
                                        await CollectRNameFromObject(elem, refObjectHashSet, visitedObjects);
                                    }
                                }
                            }
                            else
                            {
                                await CollectRNameFromObject(member, refObjectHashSet, visitedObjects);
                            }
                        }
                    }
                }
            }
        }
    }
}
