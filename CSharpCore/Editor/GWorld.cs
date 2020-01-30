using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class GWorld
    {
        public Dictionary<Guid, Actor.GActor> EditorActors
        {
            get;
        } = new Dictionary<Guid, Actor.GActor>();

        public void AddEditorActor(Actor.GActor actor)
        {
            if (EditorActors.ContainsKey(actor.ActorId))
                return;
            EditorActors[actor.ActorId] = actor;
            actor.OnAddToWorld(this);
            for(int i = 0;i<actor.Children.Count;++i)
            {
                AddEditorActor(actor.Children[i]);
            }
        }
        public void RemoveEditorActor(Guid actorId)
        {
            Actor.GActor actor;
            if (EditorActors.TryGetValue(actorId, out actor) == false)
                return;
            for (int i = 0; i < actor.Children.Count; ++i)
            {
                RemoveEditorActor(actor.Children[i].ActorId);
            }
            EditorActors.Remove(actorId);
            actor.OnRemoveWorld(this);
        }
        public Actor.GActor FindEditorActor(Guid actorId)
        {
            Actor.GActor actor;
            if (EditorActors.TryGetValue(actorId, out actor) == false)
                return null;
            return actor;
        }
        partial void CheckVisible_Editor(CCommandList cmd, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam param)
        {
            foreach (var i in EditorActors.Values)
            {
                i.OnCheckVisible(cmd, null, camera, param);
            }
        }
        partial void Tick_Editor()
        {
            foreach(var i in EditorActors.Values)
            {
                i.TryTick();
            }
        }
        partial void Cleanup_Editor()
        {
            foreach (var i in EditorActors.Values)
            {
                i.OnRemoveWorld(this);
            }
            EditorActors.Clear();
        }
    }
}
