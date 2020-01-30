using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Actor
{
    public class GPrefab : GActor
    {
        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task AwaitLoad()
        {
            await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        public async override System.Threading.Tasks.Task<GActor> Clone(CRenderContext rc)
        {
            var result = new GPrefab();
            //var result = new GCloneActor();
            var init = Initializer.CloneObject() as GActorInitializer;
            init.ActorId = Guid.NewGuid();
            result.SetInitializer(init);

            if (CenterData != null)
            {
                Rtti.MetaClass.CopyData(CenterData, result.CenterData);
            }
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = await Components[i]?.CloneComponent(rc, result, result);
                result.AddComponent(comp);
            }

            for (int i = 0; i < Children.Count; ++i)
            {
                var actor = await Children[i].Clone(rc);
                actor.SetParent(result);
                actor.OnActorLoaded();
            }
            result.OnActorLoaded();
            result.OnPlacementChanged(result.Placement);
            result.WaitContext.Result = result;
            result.WaitContext.SetFinished(WaitContext.IsFinished);
            return result;
        }
        public static async System.Threading.Tasks.Task<GPrefab> CreatePrefab(List<GActor> actors, bool firstCoord = false)
        {
            GPrefab result = null;
            if (actors.Count == 1)
            {
                result = await actors[0].ConvertToPrefab(CEngine.Instance.RenderContext);
            }
            else
            {
                result = new GPrefab();
                result.ActorId = Guid.NewGuid();
                var placement = new GamePlay.Component.GPlacementComponent();
                result.Placement = placement;
                for (int i = 0; i < actors.Count; ++i)
                {
                    if (actors[i] is GPrefab)
                    {
                        var prefab = actors[i] as GPrefab;
                        var actor = await prefab.ConvertToActor(CEngine.Instance.RenderContext);
                        actor.SetParent(result, false);
                    }
                    else
                    {
                        actors[i].SetParent(result, false);
                    }
                }
            }
            return result;
        }
        public RName Name
        {
            get;
            set;
        }
        public async System.Threading.Tasks.Task<GActor> ConvertToActor(CRenderContext rc)
        {
            var result = new GActor();
            var init = Initializer.CloneObject() as GActorInitializer;
            init.ActorId = Guid.NewGuid();
            result.SetInitializer(init);
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = await Components[i]?.CloneComponent(rc, result, result);
                result.AddComponent(comp);
            }

            foreach (var i in Children)
            {
                var actor = await i.Clone(rc);
                actor.SetParent(result);
                actor.OnActorLoaded();
            }
            result.OnActorLoaded();
            return result;
        }
        public override void Save2Xnd(IO.XndNode node)
        {
            base.Save2Xnd(node);
            //var packs = node.AddNode("PackActors", 0, 0);
            //foreach(var i in Children)
            //{
            //    var typeStr = Rtti.RttiHelper.GetTypeSaveString(i.GetType());
            //    var an = packs.AddNode(typeStr, 0, 0);
            //    i.Save2Xnd(an);
            //}
        }
        public void SavePrefab(RName name)
        {
            SavePrefab(name.Address);
        }
        public void SavePrefab(string address)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            Save2Xnd(xnd.Node);
            IO.XndHolder.SaveXND(address, xnd);
        }
        public override async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, IO.XndNode node)
        {
            if (false == await base.LoadXnd(rc, node))
                return false;

            //兼容老的版本..
            if (Children.Count == 0)
            {
                var packs = node.FindNode("PackActors");
                if (packs != null)
                {
                    var nodes = packs.GetNodes();
                    foreach (var i in nodes)
                    {
                        var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
                        if (type == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} is invalid");
                            continue;
                        }
                        var act = System.Activator.CreateInstance(type) as Actor.GActor;
                        if (act == null)
                        {
                            continue;
                        }
                        if (false == await act.LoadXnd(rc, i))
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} LoadXnd failed");
                            continue;
                        }
                        act.SetParent(this);
                    }
                }
            }

            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);

            return true;
        }
    }
    public class GPrefabManager
    {
        public Dictionary<RName, GPrefab> Prefabs
        {
            get;
        } = new Dictionary<RName, GPrefab>(new RName.EqualityComparer());
        public void Remove(RName name)
        {
            if(Prefabs.ContainsKey(name))
            {
                Prefabs.Remove(name); ;
            }
        }
        public async System.Threading.Tasks.Task<GPrefab> GetPrefab(CRenderContext rc, RName name, bool clone = false)
        {
            GPrefab result;
            bool found = false;
            lock (Prefabs)
            {
                if (Prefabs.TryGetValue(name, out result) == false)
                {
                    result = new GPrefab();
                    Prefabs.Add(name, result);
                }
                else
                    found = true;
            }

            if (found)
            {
                await result.AwaitLoad();
                if (clone)
                {
                    GPrefab cloneprefab = await result.Clone(EngineNS.CEngine.Instance.RenderContext) as GPrefab;
                    //cloneprefab.Placement.Transform = cloneprefab.Placement.Transform * result.Placement.Transform;
                    return cloneprefab;
                }
                return result;
            }

            var xnd = await IO.XndHolder.LoadXND(name.Address);
            if (xnd == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", "Prefab RName error!");
                return null;
            }
            if (false == await result.LoadXnd(rc, xnd.Node))
            {
                xnd.Node.TryReleaseHolder();
                return null;
            }
            xnd.Node.TryReleaseHolder();
            result.Name = name;
            ///兼容以前的没有名字的prefab
            if(string.IsNullOrEmpty(result.SpecialName) && result.Children.Count == 0)
            {
                result.SpecialName = $"Prefab";
            }
            if (string.IsNullOrEmpty(result.SpecialName) && result.Children.Count>0)
            {
                result.SpecialName = $"Prefab_{result.Children[0].SpecialName}";
            }
            /////兼容以前的没有名字的prefab
            //if (result.Children.Count == 1)
            //{
            //    result = await result.Children[0].ConvertToPrefab(EngineNS.CEngine.Instance.RenderContext);
            //    Prefabs[name] = result;
            //}
            if (clone)
            {
                GPrefab cloneprefab = await result.Clone(EngineNS.CEngine.Instance.RenderContext) as GPrefab;
                //cloneprefab.Placement.Transform = cloneprefab.Placement.Transform * result.Placement.Transform;
                return cloneprefab;
            }
           
            return result;
        }
        public async System.Threading.Tasks.Task<GActor> GetPrefabFistChild(CRenderContext rc, RName name, bool clone = false)
        {
            GPrefab result;
            bool found = false;
            lock (Prefabs)
            {
                if (Prefabs.TryGetValue(name, out result) == false)
                {
                    result = new GPrefab();
                    Prefabs.Add(name, result);
                }
                else
                    found = true;
            }

            if (found)
            {
                await result.AwaitLoad();
                if (clone)
                {
                    GActor cloneprefab = await result.Children[0].Clone(EngineNS.CEngine.Instance.RenderContext);
                    //cloneprefab.Placement.Transform = cloneprefab.Placement.Transform * result.Placement.Transform;
                    return cloneprefab;
                }
                return result.Children[0];
            }
 
            var xnd = await IO.XndHolder.LoadXND(name.Address);
            if (xnd == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", "Prefab RName error!");
                return null;
            }
            if (false == await result.LoadXnd(rc, xnd.Node))
            {
                xnd.Node.TryReleaseHolder();
                return null;
            }
            xnd.Node.TryReleaseHolder();
            result.Name = name;

            if (clone)
            {
                GActor cloneprefab = await result.Children[0].Clone(EngineNS.CEngine.Instance.RenderContext);
                //cloneprefab.Placement.Transform = cloneprefab.Placement.Transform * result.Placement.Transform;
                return cloneprefab;
            }
            return result.Children[0];
        }
    }
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class GPrefabPool : PooledObject<GActor>
    {
        public static int InstanceNumber = 0;
        public GPrefabPool()
        {
            InstanceNumber++;
            GrowStep = 2;
        }
        ~GPrefabPool()
        {
            Cleanup();
            InstanceNumber--;
        }
        protected override async System.Threading.Tasks.Task<GActor> CreateObject(RName name)
        {
            var rc = CEngine.Instance.RenderContext;
            GActor actor = await CEngine.Instance.PrefabManager.GetPrefab(rc, name, true);
            return actor;
        }
        protected override void OnFinalObject(GActor obj)
        {
            obj.Cleanup();
        }
        static Profiler.TimeClip mClip_NewPrefabeActorTo = new Profiler.TimeClip("GPrefabPool.NewPrefabActorTo", 1000);

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task<GActor> NewPrefabActorTo(
            [Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Prefab)]
            RName prefabname,
            SceneGraph.GSceneGraph scene,
            Vector3 location, Quaternion quaternion, Vector3 scale)
        {
            mClip_NewPrefabeActorTo.TimeOutAction = (clip, time) =>
            {
                System.Diagnostics.Debug.WriteLine($"Pooled Prefab [{prefabname}] NewPrefabActorTo time out: {time}/{clip.MaxElapse}");
            };
            using (new Profiler.TimeClipHelper(mClip_NewPrefabeActorTo))
            {
                if (!quaternion.IsValid)
                {
                    quaternion = Quaternion.Identity;
                }
                if (scale == Vector3.Zero)
                {
                    scale = Vector3.UnitXYZ;
                }
                var rc = CEngine.Instance.RenderContext;
                GActor actor = await this.QueryObject(prefabname);
                if (actor == null)
                    return null;

                if (scene != null)
                    actor.AddToScene(scene);
                actor.Placement.Location = location;
                actor.Placement.Rotation = quaternion;
                actor.Placement.Scale = scale;
                return actor;
            }   
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void ReturnPrefabActor(GActor actor)
        {
            actor.RemoveFromWorld(false);
            ReleaseObject(actor);
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public GamePlay.Actor.GPrefabManager PrefabManager
        {
            get;
        } = new GamePlay.Actor.GPrefabManager();
    }
}
