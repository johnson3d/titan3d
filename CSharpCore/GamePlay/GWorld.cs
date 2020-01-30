using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class GWorld
    {
        bool mActorsDictionaryDirty = false;
        public List<Actor.GActor> mAllActors = new List<Actor.GActor>();
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public List<Actor.GActor> AllActors
        {
            get
            {
                if (mActorsDictionaryDirty)
                {
                    mAllActors.Clear();
                    var actorList = new Actor.GActor[Actors.Count];
                    Actors.Values.CopyTo(actorList, 0);
                    mAllActors.AddRange(actorList);
                    mActorsDictionaryDirty = false;
                }
                return mAllActors;
            }
        }
        public Dictionary<Guid, Actor.GActor> Actors
        {
            get;
        } = new Dictionary<Guid, Actor.GActor>();
        public Dictionary<string, Actor.GActor> SpecialNamedActors
        {
            get;
        } = new Dictionary<string, Actor.GActor>();
        public delegate void Delegate_OnOperationActor(Actor.GActor actor);
        public event Delegate_OnOperationActor OnAddActor;
        public event Delegate_OnOperationActor OnRemoveActor;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AddActor(Actor.GActor actor)
        {
            if (Actors.ContainsKey(actor.ActorId))
                return;
            mActorsDictionaryDirty = true;
            Actors[actor.ActorId] = actor;
            actor.OnAddToWorld(this);

            for (int i = 0; i < actor.Children.Count; ++i)
            {
                AddActor(actor.Children[i]);
            }
            OnAddActor?.Invoke(actor);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveActor(Guid actorId)
        {
            Actor.GActor actor;
            if (Actors.TryGetValue(actorId, out actor) == false)
            {
                return;
            }
            mActorsDictionaryDirty = true;
            Actors.Remove(actorId);
            actor.OnRemoveWorld(this);

            for (int i = 0; i < actor.Children.Count; ++i)
            {
                RemoveActor(actor.Children[i].ActorId);
            }
            OnRemoveActor?.Invoke(actor);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor FindActor(Guid actorId)
        {
            Actor.GActor actor;
            if (Actors.TryGetValue(actorId, out actor) == false)
            {
                return null;
            }
            return actor;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor FindActorBySpecialName(string name)
        {
            Actor.GActor actor;
            if (this.SpecialNamedActors.TryGetValue(name, out actor))
                return actor;
            return null;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor FindNearestActor(Vector3 pos, float radius, List<Actor.GActor> ignoreList = null)
        {
            Actor.GActor result = null;
            float curDist = float.MaxValue;
            using (var it = Actors.Values.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (it.Current.Placement == null)
                        continue;
                    if (ignoreList != null)
                    {
                        if (ignoreList.Contains(it.Current))
                            continue;
                    }

                    var dist = Vector3.Distance(ref it.Current.Placement.mPlacementData.mLocation, ref pos);
                    if (dist > radius)
                        continue;
                    if (dist < curDist)
                    {
                        curDist = dist;
                        result = it.Current;
                    }
                }
            }
            return result;
        }

        public delegate bool FOnVisitActor(Actor.GActor actor);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor FindNearActor(Vector3 pos, float radius, FOnVisitActor visitor)
        {
            if (visitor == null)
                return null;
            using (var it = Actors.Values.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (it.Current.Placement == null)
                        continue;

                    var dist = Vector3.Distance(ref it.Current.Placement.mPlacementData.mLocation, ref pos);
                    if (dist > radius)
                        continue;
                    if (visitor(it.Current))
                        return it.Current;
                }
            }
            return null;
        }
        protected SceneGraph.GSceneGraph mDefaultScene;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetDefaultScene(Guid sceneId)
        {
            mDefaultScene = FindScene(sceneId);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public SceneGraph.GSceneGraph DefaultScene
        {
            get
            {
                if (mDefaultScene == null)
                {
                    var it = Scenes.GetEnumerator();
                    while (it.MoveNext())
                    {
                        mDefaultScene = it.Current.Value;
                        break;
                    }
                    it.Dispose();
                }
                return mDefaultScene;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public SceneGraph.GSceneGraph FindScene(Guid sceneId)
        {
            foreach (var i in Scenes.Values)
            {
                if (i.SceneId == sceneId)
                    return i;
            }
            return null;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public SceneGraph.GSceneGraph FindSceneByName(RName name)
        {
            SceneGraph.GSceneGraph result;
            if (Scenes.TryGetValue(name, out result))
                return result;
            return null;
        }
        protected Dictionary<RName, SceneGraph.GSceneGraph> Scenes
        {
            get;
        } = new Dictionary<RName, SceneGraph.GSceneGraph>(new RName.EqualityComparer());
        public Dictionary<RName, SceneGraph.GSceneGraph>.Enumerator GetSceneEnumerator()
        {
            return Scenes.GetEnumerator();
        }
        public int SceneNumber
        {
            get
            {
                return Scenes.Count;
            }
        }
        public SceneGraph.GSceneGraph GetScene(Guid id)
        {
            lock (Scenes)
            {
                using (var i = Scenes.Values.GetEnumerator())
                {
                    while (i.MoveNext())
                    {
                        if (i.Current.SceneId == id)
                            return i.Current;
                    }
                }
                return null;
            }
        }
        public SceneGraph.GSceneGraph GetScene(RName name)
        {
            lock (Scenes)
            {
                SceneGraph.GSceneGraph sg;
                if (Scenes.TryGetValue(name, out sg))
                    return sg;
                return null;
            }
        }

        public void AddScene(RName name, SceneGraph.GSceneGraph sg)
        {
            lock (Scenes)
            {
                Scenes.Add(name, sg);
            }
        }
        public void RemoveScene(RName name)
        {
            lock (Scenes)
            {
                Scenes.Remove(name);
            }
        }
        public void ClearAllScenes()
        {
            lock (Scenes)
            {
                foreach (var i in Scenes)
                {
                    i.Value.Cleanup();
                }
                Scenes.Clear();
            }
        }
        public bool Init()
        {
            return true;
        }
        partial void Tick_Editor();
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GWorld), nameof(Tick));
        public void Tick()
        {
            ScopeTick.Begin();
            using (var it = Scenes.Values.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (it.Current.NeedTick == false)
                        continue;
                    it.Current.Tick();
                }
            }
            Tick_Editor();
            ScopeTick.End();
        }
        partial void Cleanup_Editor();
        public void Cleanup()
        {
            var it = Scenes.Values.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Cleanup();
            }
            it.Dispose();
            Scenes.Clear();

            var it2 = Actors.Values.GetEnumerator();
            while (it2.MoveNext())
            {
                it2.Current.OnRemoveWorld(this);
            }
            it2.Dispose();
            Actors.Clear();
            Cleanup_Editor();
            mDefaultScene = null;
        }
        public static Profiler.TimeScope ScopeCheckVisible = Profiler.TimeScopeManager.GetTimeScope(typeof(GWorld), nameof(CheckVisible));

        SceneGraph.CheckVisibleParam mCheckVisibleParam = new SceneGraph.CheckVisibleParam();
        public SceneGraph.CheckVisibleParam CheckVisibleParam
        {
            get { return mCheckVisibleParam; }
        }
        public void CheckVisible(CCommandList cmd, Graphics.CGfxCamera camera)
        {
            ScopeCheckVisible.Begin();
            var it = Scenes.Values.GetEnumerator();
            while (it.MoveNext())
            {
                mCheckVisibleParam.Reset();
                mCheckVisibleParam.FrustumCulling = true;
                it.Current.CheckVisible(cmd, camera, mCheckVisibleParam, true);
            }
            it.Dispose();
            CheckVisible_Editor(cmd, camera, mCheckVisibleParam);
            mCheckVisibleParam.SerialID++;

            if (CEngine.EnableShadow)
            {
                CheckVisibleForShadow(cmd, camera);
            }

            camera.SceneView?.UpdatePointLights(this.mDefaultScene);
            ScopeCheckVisible.End();
        }
        private void CheckVisibleForShadow(CCommandList cmd, Graphics.CGfxCamera camera)
        {
            var it = Scenes.Values.GetEnumerator();
            while (it.MoveNext())
            {
                mCheckVisibleParam.Reset();
                mCheckVisibleParam.FrustumCulling = false;
                mCheckVisibleParam.ForShadow = true;
                mCheckVisibleParam.mShadowCullDistance = camera.mShadowDistance;
                it.Current.CheckVisible(cmd, camera, mCheckVisibleParam, true);
            }
            it.Dispose();
            mCheckVisibleParam.SerialID++;
        }
        public void SlowDrawAll(CCommandList cmd, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam param)
        {
            var it = Scenes.Values.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.SlowDrawAll(cmd, camera, param);
            }
            it.Dispose();
        }
        partial void CheckVisible_Editor(CCommandList cmd, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam param);
    }
}
