using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Actor
{
    public partial class GActor
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McActor))]
        [Category("Macross")]
        public RName ActorMacross
        {
            get
            {
                if (Initializer == null)
                    return null;
                return Initializer.ActorMacross;
            }
            set
            {
                if (Initializer == null)
                    return;
                if (Initializer.ActorMacross == value && mMcActorGetter != null)
                    return;

                McActor?.OnUnsetMacross(this);
                if (mMcActorGetter != null)
                {
                    var getter = mMcActorGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostActor = null;
                }
                Initializer.ActorMacross = value;
                mMcActorGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McActor>(value);
                if (mMcActorGetter != null)
                {
                    var getter = mMcActorGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostActor = this;
                }
                mMcActorGetter?.Get(OnlyForGame)?.OnSetMacross(this);
            }
        }
        protected Macross.MacrossGetter<McActor> mMcActorGetter;
        [Category("Macross")]
        public McActor McActor
        {
            get
            {
                if (mMcActorGetter != null)
                    return mMcActorGetter.Get(OnlyForGame);
                return null;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public McActor GetMcActor(
            [Editor.Editor_TypeFilterAttribute(typeof(McActor))]
            Type type)
        {
            return McActor;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Graphics.Mesh.CGfxMesh GetComponentMesh(string name = null)
        {
            Component.GMeshComponent comp = null;
            if (string.IsNullOrEmpty(name))
            {
                name = "GMeshComponent";
            }
            comp = FindComponentBySpecialName(name) as Component.GMeshComponent;
            if (comp == null)
                return null;
            return comp.SceneMesh;
        }
        public delegate bool FOnVisitComponent(Component.GComponent comp);
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Component.GComponent FindComponent(FOnVisitComponent visitor)
        {
            if (visitor == null)
                return null;
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                if (visitor(comp) == true)
                    return comp;
            }
            return null;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Component.GComponent FindComponentRecursion(FOnVisitComponent visitor)
        {
            if (visitor == null)
                return null;
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                if (visitor(comp) == true)
                    return comp;
                var container = comp as Component.GComponentsContainer;
                if (container != null)
                {
                    var childCom = container.FindComponentRecursion(visitor);
                    if (childCom != null)
                        return childCom;
                    continue;
                }
            }
            return null;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void TriggerCustomEvent(Component.GComponent sender, int msg, object arg)
        {
            if (mMcActorGetter != null)
            {
                if (mMcActorGetter.Get(OnlyForGame).OnTriggerCustomEvent(sender, msg, arg) == true)
                    return;
            }
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                if (comp.OnlyForGame && CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    continue;
                if (comp.TriggerCustomEvent(sender, msg, arg) == true)
                    return;

            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void TriggerCustomEvent2Component(Component.GComponent sender, int msg, object arg,
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            Type rcvCompType)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                if (comp.OnlyForGame && CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    continue;
                if (rcvCompType != null)
                {
                    if (comp.GetType().IsSubclassOf(rcvCompType) == false &&
                        comp.GetType() != rcvCompType)
                        continue;
                }
                if (comp.TriggerCustomEvent(sender, msg, arg) == true)
                    return;

            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveFromScene()
        {
            this.Scene.RemoveActor(this);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveFromWorld(bool ClearActor)
        {
            if (Scene != null)
            {
                Scene.World.RemoveActor(this.ActorId);
                Scene.RemoveActor(this);
            }
            if (ClearActor)
            {
                CEngine.Instance.EventPoster.RunOn(() =>
                {
                    Cleanup();
                    return null;
                }, Thread.Async.EAsyncTarget.Logic);
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AddToScene(SceneGraph.GSceneGraph scene)
        {
            scene.AddActor(this);
            scene.World.AddActor(this);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float GetActorDistance(GActor target)
        {
            return Vector3.Distance(ref Placement.mPlacementData.mLocation, ref target.Placement.mPlacementData.mLocation);
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcactor_64.txpic", RName.enRNameType.Editor)]
    public class McActor : Macross.INewMacross, Input.IInputable
    {
        #region IInputable
        public virtual void OnRegisterInput()
        {

        }
        public virtual void OnUnRegisterInput()
        {

        }
        #endregion
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public GActor HostActor
        {
            get;
            set;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Camera.CameraComponent ActorCamera
        {
            get
            {
                return HostActor?.GetComponentRecursion<Camera.CameraComponent>();
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Controller.GPlayerController PlayerController
        {
            get
            {
                return HostActor?.GetComponent<Controller.GPlayerController>();
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public static McGameInstance GetGameInstance(
            [Editor.Editor_TypeFilterAttribute(typeof(McGameInstance))]
            Type type)
        {
            return CEngine.Instance.McGameInstance;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static Type GetObjectType(object obj)
        {
            if (obj == null)
                return null;
            return obj.GetType();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnSetMacross(GActor actor)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnNewMacross()
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnUnsetMacross(GActor actor)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnInit(GActor actor)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            //actor.CreateCenterData();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnLoadedAll(GActor actor)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTick(GActor actor)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnCleanup(GActor actor)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual bool OnTriggerCustomEvent(Component.GComponent sender, int msg, object arg)
        {
            return false;
        }

        public virtual void OnAddedScene()
        {
            OnRegisterInput();
        }
        public virtual void OnRemoveScene()
        {
            OnUnRegisterInput();
        }
    }
}
