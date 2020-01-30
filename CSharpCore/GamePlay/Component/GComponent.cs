using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.IO;

namespace EngineNS.GamePlay.Component
{
    public interface IComponentContainer
    {
        List<Component.GComponent> Components { get; }
        Component.GComponent AddComponent(Component.GComponent comp);
        bool InsertComponent(int index, Component.GComponent comp);
        void RemoveComponent(Component.GComponent component);
        void RemoveComponent(string specialName);
        void ClearComponents();
        Component.GComponent FindComponentBySpecialName(string name);
        Component.GComponent FindComponentBySpecialNameRecursion(string name);
        Component.GComponent GetComponent(System.Type type);
        Component.GComponent GetComponentRecursion(System.Type type);
        List<Component.GComponent> GetComponents(System.Type type);
        List<Component.GComponent> GetComponentsRecursion(System.Type type);
        GComp GetComponent<GComp>() where GComp : Component.GComponent;
        GComp GetComponentRecursion<GComp>() where GComp : Component.GComponent;
        List<GComp> GetComponents<GComp>() where GComp : Component.GComponent;
        List<GComp> GetComponentsRecursion<GComp>() where GComp : Component.GComponent;
        Component.GComponent FindComponent(GActor.FOnVisitComponent visitor);
        Component.GComponent FindComponentRecursion(GActor.FOnVisitComponent visitor);
    }
    public class GComponentsContainer : GComponent, IComponentContainer
    {
        #region IComponentContainer
        public List<Component.GComponent> Components
        {
            get;
        } = new List<Component.GComponent>();
        public override void Save2Xnd(IO.XndNode node)
        {
            base.Save2Xnd(node);
            var cpNodes = node.AddNode("CompChildren", 0, 0);
            for (int i = 0; i < Components.Count; ++i)
            {
                var typeName = Rtti.RttiHelper.GetTypeSaveString(Components[i].GetType());
                var cnode = cpNodes.AddNode(typeName, 0, 0);
                Components[i].Save2Xnd(cnode);
            }
        }
        public override async Task<bool> LoadXnd(CRenderContext rc, GActor host, IComponentContainer hostContainer, XndNode node)
        {
            Host = host;
            if (false == await base.LoadXnd(rc, host, hostContainer, node))
                return false;

            var cpNodes = node.FindNode("CompChildren");
            if (cpNodes != null)
            {
                Components.Clear();
                var nodes = cpNodes.GetNodes();
                foreach (var i in nodes)
                {
                    var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
                    if (type == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "ACC", $"Component {i.GetName()} is invalid");
                        continue;
                    }
                    Component.GComponent comp = Activator.CreateInstance(type) as Component.GComponent;
                    if (false == await comp.LoadXnd(rc, Host, this, i))
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "ACC", $"Component {i.GetName()} load failed");
                        continue;
                    }
                    this.AddComponent(comp);
                }
            }

            return true;
        }

        public override async System.Threading.Tasks.Task<GComponent> CloneComponent(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer)
        {
            var comp = await base.CloneComponent(rc, host, hostContainer);
            if (comp == null)
                return null;
            comp.HostContainer = host;
            var container = comp as GComponentsContainer;
            if (container != null)
            {
                for (int i = 0; i < Components.Count; ++i)
                {
                    var ccp = await Components[i].CloneComponent(rc, host, container);
                    container.AddComponent(ccp);
                }
            }
            return comp;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Component.GComponent FindComponentBySpecialName(string name)
        {

            for (int i = 0; i < Components.Count; ++i)
            {
                if (Components[i].SpecialName == name)
                    return Components[i];
            }
            return null;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Bricks.Particle.GParticleComponent FindParticleByName(string name)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var com = Components[i] as Bricks.Particle.GParticleComponent;
                if (com != null)
                {
                    if (com.MacrossName != null && com.MacrossName.GetFileName().Equals(name))
                    {
                        return com;
                    }
                }

            }
            return null;
        }

        public void ResetParticleByName(string name)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var com = Components[i] as Bricks.Particle.GParticleComponent;
                if (com != null)
                {
                    if (com.MacrossName != null && com.MacrossName.GetFileName().Equals(name))
                    {
                        com.ResetTime();
                    }
                }
               
            }
        }

        public Component.GComponent FindComponentBySpecialNameRecursion(string name)
        {

            for (int i = 0; i < Components.Count; ++i)
            {
                if (Components[i].SpecialName == name)
                    return Components[i];
                else
                {
                    var compContain = Components[i] as Component.GComponentsContainer;
                    if (compContain == null)
                        continue;
                    var childCom = compContain.FindComponentBySpecialNameRecursion(name);
                    if (childCom != null)
                        return childCom;
                    continue;
                }
            }
            return null;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(1)]
        public Component.GComponent FindComponentBySpecialNameAs(string name,
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            Type type)
        {
            return FindComponentBySpecialName(name);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public Component.GComponent GetComponent(
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            System.Type type)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var cur = Components[i];
                var compType = cur.GetType();
                if (compType == type)
                    return cur;
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        return cur;
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        return cur;
                }
            }
            return null;
        }
        public Component.GComponent GetComponentRecursion(
           [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            System.Type type)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var cur = Components[i];
                var compType = cur.GetType();
                if (compType == type)
                    return cur;
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        return cur;
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        return cur;
                }
                var container = cur as Component.GComponentsContainer;
                if (container != null)
                {
                    var childCom = container.GetComponent(type);
                    if (childCom != null)
                        return childCom;
                    continue;
                }
            }
            return null;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public List<Component.GComponent> GetComponents(System.Type type)
        {
            List<Component.GComponent> coms = new List<Component.GComponent>();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                var compType = comp.GetType();
                if (compType == type)
                    coms.Add(comp);
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        coms.Add(comp);
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        coms.Add(comp);
                }
            }
            return coms;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public List<Component.GComponent> GetComponentsRecursion(System.Type type)
        {
            List<Component.GComponent> coms = new List<Component.GComponent>();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                var compType = comp.GetType();
                if (compType == type)
                    coms.Add(comp);
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        coms.Add(comp);
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        coms.Add(comp);
                }
                var container = comp as Component.GComponentsContainer;
                if (container != null)
                {
                    var subComps = container.GetComponentsRecursion(type);
                    coms.AddRange(subComps);
                }
            }
            return coms;
        }
        public GComp GetComponent<GComp>() where GComp : Component.GComponent
        {
            return (GComp)GetComponent(typeof(GComp));
        }
        public GComp GetComponentRecursion<GComp>() where GComp : Component.GComponent
        {
            return (GComp)GetComponentRecursion(typeof(GComp));
        }
        public List<GComp> GetComponents<GComp>() where GComp : Component.GComponent
        {
            var type = typeof(GComp);
            List<GComp> coms = new List<GComp>();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                var compType = comp.GetType();
                if (compType == type)
                    coms.Add((GComp)comp);
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        coms.Add((GComp)comp);
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        coms.Add((GComp)comp);
                }
            }

            return coms;
        }
        public List<GComp> GetComponentsRecursion<GComp>() where GComp : Component.GComponent
        {
            var type = typeof(GComp);
            List<GComp> coms = new List<GComp>();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                var compType = comp.GetType();
                if (compType == type)
                    coms.Add((GComp)comp);
                if (type.IsInterface)
                {
                    if (compType.GetInterface(type.FullName) != null)
                        coms.Add((GComp)comp);
                }
                else
                {
                    if (compType.IsSubclassOf(type))
                        coms.Add((GComp)comp);
                }
                var container = comp as Component.GComponentsContainer;
                if (container != null)
                {
                    var subComps = container.GetComponentsRecursion<GComp>();
                    coms.AddRange(subComps);
                }
            }
            return coms;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Component.GComponent FindComponent(GActor.FOnVisitComponent visitor)
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
        public Component.GComponent FindComponentRecursion(GActor.FOnVisitComponent visitor)
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
        public Component.GComponent AddComponent(Component.GComponent comp)
        {
            if (!Components.Contains(comp))
            {
                comp.Host = Host;
                comp.HostContainer = this;
                Components.Add(comp);
                comp.OnAdded();
                return null;
            }
            return comp;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool InsertComponent(int index, Component.GComponent comp)
        {
            if (!Components.Contains(comp))
            {
                comp.Host = Host;
                comp.HostContainer = this;
                Components.Insert(index, comp);
                comp.OnAdded();
                return true;
            }
            else
            {
                return false;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveComponent(string specialName)
        {
            Component.GComponent old = FindComponentBySpecialName(specialName);
            if (old != null)
            {
                old.OnRemove();
                old.Host = null;
                old.HostContainer = null;
                Components.Remove(old);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void ClearComponents()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                var com = Components[i] as IComponentContainer;
                if (com != null)
                {
                    com.ClearComponents();
                }

                RemoveComponent(Components[i]);
            }
        }
        public void RemoveComponent(Component.GComponent component)
        {
            if (Components.Contains(component))
            {
                component.OnRemove();
                component.Host = null;
                component.HostContainer = null;
                Components.Remove(component);
            }
        }
        #endregion
        public override void CommitVisual(CCommandList cmd, Graphics.CGfxCamera Camera, SceneGraph.CheckVisibleParam param)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                comp.CommitVisual(cmd, Camera, param);
            }
            base.CommitVisual(cmd, Camera, param);
        }

        public override void OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                comp.OnEditorCommitVisual(cmd, camera, param);
            }
            base.OnEditorCommitVisual(cmd, camera, param);
        }
        public override void Tick(GPlacementComponent placement)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                var scope = comp.GetTickTimeScope();
                if (scope != null)
                    scope.Begin();
                comp.Tick(placement);
                if (scope != null)
                    scope.End();
            }
            base.Tick(placement);
        }

        public override void OnAddedScene()
        {
            this.McCompGetter?.Get()?.OnAddedScene(this);
            this.McCompGetter?.Get()?.OnRegisterInput();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                comp.VisitChildComponents((self, arg) =>
                {
                    self.OnAddedScene();
                }, null);
            }
        }
        public override void OnRemoveScene()
        {
            this.McCompGetter?.Get()?.OnRemoveScene(this);
            this.McCompGetter?.Get()?.OnUnRegisterInput();
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                comp.VisitChildComponents((self, arg) =>
                {
                    self.OnRemoveScene();
                }, null);
            }
        }
        public override void OnActorLoaded(GActor actor)
        {
            base.OnActorLoaded(actor);
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                comp.OnActorLoaded(actor);
            }
        }
    }
    // 
    public interface IComponentHostSelectOperation
    {
        void OnHostSelected(bool isSelect);
    }
    // 场景图组件
    public interface ISceneGraphComponent
    {
        /// <summary>
        /// 构建场景管理节点数据
        /// </summary>
        /// <param name="actors">需要构建处理的对象列表</param>
        /// <param name="containActors">该场景节点构建完包含的对象列表</param>
        /// <returns></returns>
        System.Threading.Tasks.Task<bool> Build(List<EngineNS.GamePlay.Actor.GActor> actors, List<EngineNS.GamePlay.Actor.GActor> unPVSActors, List<EngineNS.GamePlay.Actor.GActor> containActors);
        EngineNS.GamePlay.SceneGraph.ISceneNode GetSceneNode();
    }

    public class CustomConstructionParamsAttribute : System.Attribute
    {
        public Type ConstructionParamsType;
        public string Describe;
        public bool LoadOnce;
        public string ComponentGroup;
        public string ComponentDisplayName;
        public CustomConstructionParamsAttribute(Type consParamsType, string describe, string group = null, bool loadOnce = false)
        {
            ConstructionParamsType = consParamsType;

            //必须有描述
            //System.Diagnostics.Debug.Assert(describe != null && describe.Equals("") == false);
            Describe = describe;
            LoadOnce = loadOnce;
            ComponentGroup = group;
        }
        public CustomConstructionParamsAttribute(Type consParamsType, string describe, string group, string displayName, bool loadOnce = false)
        {
            ConstructionParamsType = consParamsType;

            //必须有描述
            //System.Diagnostics.Debug.Assert(describe != null && describe.Equals("") == false);
            Describe = describe;
            LoadOnce = loadOnce;
            ComponentGroup = group;
            ComponentDisplayName = displayName;
        }
    }

    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class GComponent : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        [Category("Common")]
        public bool EnableTick
        {
            get;
            set;
        } = true;
        [Category("Common")]
        public bool TickBeforeCheckVisible
        {
            get;
            set;
        } = false;
        [Category("Common")]
        public bool OnlyForGame
        {
            get
            {
                if (Initializer != null)
                {
                    return Initializer.OnlyForGame;
                }
                return false;
            }
            set
            {
                if (Initializer != null)
                {
                    Initializer.OnlyForGame = value;
                }
            }
        }
        [Category("Common")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool DontSave
        {
            get
            {
                if (Initializer != null)
                {
                    return Initializer.DontSave;
                }
                return false;
            }
            set
            {
                if (Initializer != null)
                {
                    Initializer.DontSave = value;
                }
            }
        }
        public static int InstanceNumber = 0;
        public GComponent()
        {
            InstanceNumber++;
        }
        ~GComponent()
        {
            InstanceNumber--;
        }
        public virtual void Cleanup()
        {
            if (McCompGetter != null && McCompGetter.IsInited)
                McCompGetter.Get(OnlyForGame)?.OnCleanup(this);
        }
        public virtual object GetShowPropertyObject()
        {
            return this;
        }

        protected IEntity mEntity;
        [Browsable(false)]
        public IEntity Entity
        {
            get => mEntity;
            set => mEntity = value;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public virtual Actor.GActor Host
        {
            get { return mEntity as Actor.GActor; }
            set { mEntity = value; }
        }
        protected IComponentContainer mHostContainer;
        [Browsable(false)]
        public virtual IComponentContainer HostContainer
        {
            get { return mHostContainer; }
            set { mHostContainer = value; }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [DisplayName("Name")]
        [Category("Common")]
        public virtual string SpecialName
        {
            get
            {
                if (Initializer != null)
                {
                    if (!string.IsNullOrEmpty(Initializer.SpecialName))
                        return Initializer.SpecialName;
                }
                return GetType().Name;
            }
            set
            {
                if (Initializer != null)
                {
                    //if (mHostContainer != null)
                    //{
                    //    if (mHostContainer.Components.ContainsKey(value))
                    //        return;
                    //    if (!string.IsNullOrEmpty(Initializer.SpecialName))
                    //        mHostContainer.Components.Remove(Initializer.SpecialName);
                    //    else
                    //        mHostContainer.Components.Remove(GetType().Name);
                    //    mHostContainer.Components[value] = this;
                    //}
                    Initializer.SpecialName = value;
                    OnPropertyChanged("SpecialName");

                }
            }
        }
        [Rtti.MetaClassAttribute]
        public class GComponentInitializer : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string SpecialName
            {
                get;
                set;
            }
            [Rtti.MetaData]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [Browsable(false)]
            public RName ComponentMacross
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public virtual bool OnlyForGame
            {
                get;
                set;
            } = false;
            [Rtti.MetaData]
            public bool DontSave
            {
                get;
                set;
            } = false;
        }
        [Browsable(false)]

        public GComponentInitializer Initializer
        {
            get;
            set;
        }
        public T GetInitializer<T>() where T : GComponentInitializer
        {
            return Initializer as T;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            mEntity = host;
            mHostContainer = hostContainer;
            Initializer = v;

            if (Initializer != null)
            {
                ComponentMacross = Initializer.ComponentMacross;
                this.McCompGetter?.Get(Initializer.OnlyForGame)?.OnInit(this);
            }
            return true;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual async System.Threading.Tasks.Task<GComponent> CloneComponent(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer)
        {
            var type = this.GetType();
            Component.GComponent result = null;
            var atttibutes = type.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), true);
            if (atttibutes.Length > 0)
            {
                //result = await GMacrossComponent.CreateComponent(type);
                //result = EngineNS.CEngine.Instance.MacrossDataManager.NewObject(type) as EngineNS.GamePlay.Component.GComponent;
                throw new InvalidOperationException("不能直接操作Macross类型");
            }
            else
            {
                result = Activator.CreateInstance(type) as Component.GComponent;
            }
            var init = Initializer.CloneObject() as GComponentInitializer;
            await result.SetInitializer(rc, host, hostContainer, init);
            return result;
        }

        public virtual void Save2Xnd(IO.XndNode node)
        {
            if (Initializer != null)
            {
                var attr = node.AddAttrib("Initializer");
                attr.BeginWrite();
                attr.WriteMetaObject(Initializer);
                attr.EndWrite();
            }

        }
        public virtual async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer, IO.XndNode node)
        {
            var attr = node.FindAttrib("Initializer");
            if (attr == null)
            {
                return true;
            }

            GComponentInitializer init = null;
            var ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                attr.BeginRead();
                init = attr.ReadMetaObject(null) as GComponentInitializer;
                if (init == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Component({this.GetType().FullName}) ReadMetaObject failed");
                    attr.EndRead();
                    return false;
                }
                attr.EndRead();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (ret == false)
                return false;

            return await this.SetInitializer(rc, host, hostContainer, init);
        }
        public virtual void PreUse(bool force)
        {

        }
        public virtual Profiler.TimeScope GetTickTimeScope()
        {
            return null;
        }
        public virtual void Tick(GPlacementComponent placement)
        {

        }
        public delegate void FOnVisitComponent(GComponent comp, object arg);
        public void VisitChildComponents(FOnVisitComponent visitor, object arg)
        {
            var container = this as IComponentContainer;
            if(container!=null)
            {
                for (int i = 0; i < container.Components.Count; ++i)
                {
                    var comp = container.Components[i];
                    comp.VisitChildComponents(visitor, arg);
                }
            }
            visitor(this, arg);
        }
        // 重新初始化时调用，如Macross组件更新等
        public virtual void OnReInitComponent()
        {

        }
        public virtual void OnActorLoaded(Actor.GActor actor)
        {
            this.McCompGetter?.Get(OnlyForGame)?.OnActorLoaded(this);
        }
        // 其他组件加入host后调用
        protected virtual void OnAddedComponent(GComponent addedComponent)
        {
            this.McCompGetter?.Get(OnlyForGame)?.OnAddedComponent(this, addedComponent);
        }
        // 当前组件加入host后调用
        public virtual void OnAdded()
        {
            if (Host != null)
            {
                for (int i = 0; i < Host.Components.Count; ++i)
                {
                    var comp = Host.Components[i];
                    comp.OnAddedComponent(this);
                    comp.VisitChildComponents((cur, arg) =>
                    {
                        if (cur != arg)
                        {
                            cur.OnAddedComponent(arg as GComponent);
                        }
                    }, this);
                }
            }
        }
        // 其他组件从host删除后调用
        protected virtual void OnRemovedComponent(GComponent removedComponent)
        {
            this.McCompGetter?.Get(OnlyForGame)?.OnRemovedComponent(this, removedComponent);
        }
        // 当前组件从host删除后调用
        public virtual void OnRemove()
        {
            if (Host != null)
            {
                for (int i = 0; i < Host.Components.Count; ++i)
                {
                    var comp = Host.Components[i];
                    comp.OnRemovedComponent(this);
                    comp.VisitChildComponents((cur, arg) =>
                    {
                        if(cur!=arg)
                        {
                            cur.OnRemovedComponent(arg as GComponent);
                        }
                    }, this);
                }
            }
        }

        public virtual void OnAddedScene()
        {
            this.McCompGetter?.Get(OnlyForGame)?.OnAddedScene(this);
            this.McCompGetter?.Get(OnlyForGame)?.OnRegisterInput();
        }
        public virtual void OnRemoveScene()
        {
            this.McCompGetter?.Get(OnlyForGame)?.OnRemoveScene(this);
            this.McCompGetter?.Get(OnlyForGame)?.OnUnRegisterInput();
        }
        public virtual void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {

        }
        public virtual void OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam param)
        {

        }
        public virtual bool OnTryMove(GPlacementComponent placement, ref Vector3 delta, float minDist, float elapsedTime)
        {
            return false;
        }
        [Category("Common")]
        public virtual bool IsVisualComponent
        {
            get => false;
        }
        public virtual void CommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam param)
        {

        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McComponent))]
        [Editor.Editor_PackData()]
        [Category("Common")]
        public virtual RName ComponentMacross
        {
            get
            {
                if (this.Initializer == null)
                    return null;
                return Initializer.ComponentMacross;
            }
            set
            {
                if (Initializer == null)
                    return;
                if (Initializer.ComponentMacross == value && mMcCompGetter != null)
                    return;

                mMcCompGetter?.Get()?.OnUnsetMacross(this);
                if (mMcCompGetter != null)
                {
                    var getter = mMcCompGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostComp = null;
                }
                Initializer.ComponentMacross = value;
                mMcCompGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McComponent>(value);
                if (mMcCompGetter != null)
                {
                    var getter = mMcCompGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostComp = this;
                }
                mMcCompGetter?.Get()?.OnSetMacross(this);
                OnPropertyChanged("McComponent");
            }
        }
        protected Macross.MacrossGetter<McComponent> mMcCompGetter;
        [Browsable(false)]
        public Macross.MacrossGetter<McComponent> McCompGetter
        {
            get { return mMcCompGetter; }
        }
        [Category("Common")]
        public McComponent McComponent
        {
            get
            {
                return mMcCompGetter?.Get(OnlyForGame);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public McComponent GetMcComponent(
            [Editor.Editor_TypeFilterAttribute(typeof(McComponent))]
            Type type)
        {
            return McComponent;
        }
        public virtual bool TriggerCustomEvent(Component.GComponent sender, int msg, object arg)
        {
            if (mMcCompGetter != null)
                return mMcCompGetter.Get(OnlyForGame).OnTriggerCustomEvent(sender, msg, arg);
            else
                return false;
        }
    }

    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/macross_64.txpic", RName.enRNameType.Editor)]
    public class McComponent : Macross.INewMacross, Input.IInputable
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
        public bool IsEditorMode
        {
            get
            {
                return CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Macross.MacrossFieldAttribute()]
        [Browsable(false)]
        public GComponent HostComp
        {
            get;
            set;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public GActor HostActor
        {
            get
            {
                return HostComp?.Host;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public SceneGraph.GSceneGraph HostScene
        {
            get
            {
                return HostComp?.Host?.Scene;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GCenterData CenterData
        {
            get
            {
                return HostComp?.Host?.CenterData;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public GCenterData GetCenterData(
            [Editor.Editor_TypeFilterAttribute(typeof(GCenterData))]
            System.Type type)
        {
            return CenterData;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SureCenterData(
            [Editor.Editor_RNameMacrossType(typeof(GCenterData))]
            RName value)
        {
            if (HostComp.Host.CenterData == null)
            {
                HostComp.Host.CreateCenterData(value);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void TriggerActorCustomEvent(int msg, object arg)
        {
            HostComp.Host.TriggerCustomEvent(this.HostComp, msg, arg);
        }
        [System.ComponentModel.Browsable(false)]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 ActorLocation
        {
            get
            {
                if (HostComp != null && HostComp.Host != null)
                    return HostComp.Host.Placement.Location;
                return Vector3.Zero;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnNewMacross()
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnSetMacross(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnUnsetMacross(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnInit(GComponent comp)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTick(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnCleanup(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnActorLoaded(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnAddedComponent(GComponent comp, GComponent addedComponent)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnRemovedComponent(GComponent comp, GComponent removedComponent)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnAddedScene(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnRemoveScene(GComponent comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnPlacementChanged(GActor actor)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual bool OnTriggerCustomEvent(Component.GComponent sender, int msg, object arg)
        {
            return false;
        }
    }
}
