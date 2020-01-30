using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace EngineNS.GamePlay.SceneGraph
{
    public class ModuleInitInfoAttribute : System.Attribute
    {
        public Type IniterType;
        public string Description;
        public string[] ModuleGroup;
        public ModuleInitInfoAttribute(Type initType, string description, params string[] group)
        {
            IniterType = initType;
            Description = description;
            ModuleGroup = group;
        }
    }

    [ModuleInitInfo(typeof(GSceneModule.GSceneModuleInitializer), "", "SceneModule")]
    public class GSceneModule : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [Rtti.MetaClassAttribute]
        public class GSceneModuleInitializer : IO.Serializer.Serializer, INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion
            [Rtti.MetaData]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public RName ModuleMacross
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public bool OnlyForGame
            {
                get;
                set;
            } = true;
            [Browsable(false)]
            [Rtti.MetaData]
            public string Name
            {
                get;
                set;
            } = "";
        }
        GSceneGraph mHost;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public GSceneGraph Host
        {
            get { return mHost; }
            set { mHost = value; }
        }

        public string Name
        {
            get => Initializer?.Name;
            set
            {
                if (Initializer != null)
                    Initializer.Name = value;
                OnPropertyChanged("Name");
            }
        }

        [Browsable(false)]
        
        public GSceneModuleInitializer Initializer
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, GSceneGraph host, GSceneModuleInitializer v)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            mHost = host;
            Initializer = v;

            if (Initializer != null)
            {
                ModuleMacross = Initializer.ModuleMacross;
                this.mMcModuleGetter?.Get(Initializer.OnlyForGame)?.OnInit(this);
            }
            return true;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McSceneModule))]
        [Editor.Editor_PackData()]
        public virtual RName ModuleMacross
        {
            get
            {
                if (this.Initializer == null)
                    return null;
                return Initializer.ModuleMacross;
            }
            set
            {
                if (Initializer == null)
                    return;
                if (Initializer.ModuleMacross == value && mMcModuleGetter != null)
                    return;

                mMcModuleGetter?.Get()?.OnUnsetMacross(this);
                if (mMcModuleGetter != null)
                {
                    var getter = mMcModuleGetter.Get(Initializer.OnlyForGame);
                    if (getter != null)
                        getter.HostModule = null;
                }
                Initializer.ModuleMacross = value;
                mMcModuleGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McSceneModule>(value);
                if (mMcModuleGetter != null)
                {
                    var getter = mMcModuleGetter.Get(Initializer.OnlyForGame);
                    if (getter != null)
                        getter.HostModule = this;
                }
                mMcModuleGetter?.Get()?.OnSetMacross(this);
                OnPropertyChanged("McComponent");
            }
        }
        protected Macross.MacrossGetter<McSceneModule> mMcModuleGetter;
        [Browsable(false)]
        public Macross.MacrossGetter<McSceneModule> McModuleGetter
        {
            get { return mMcModuleGetter; }
        }
        public McSceneModule McModule
        {
            get
            {
                return mMcModuleGetter?.Get(false);
            }
        }

        public virtual void OnAdded()
        {
            foreach (var comp in mHost.Modules.Values)
            {
                if (comp == this)
                    continue;
                comp.OnAddedModule(this);
            }
        }
        protected virtual void OnAddedModule(GSceneModule addedComponent)
        {
            this.McModuleGetter?.Get()?.OnAddedModule(this, addedComponent);
        }
        public virtual void OnRemove()
        {
            foreach (var comp in mHost.Modules.Values)
            {
                if (comp == this)
                    continue;
                comp.OnRemovedModule(this);
            }
        }
        protected virtual void OnRemovedModule(GSceneModule removedComponent)
        {
            this.McModuleGetter?.Get()?.OnRemovedModule(this, removedComponent);
        }
        public virtual void Tick()
        {
            this.McModuleGetter?.Get()?.OnTick(this);
        }
        public virtual void Cleanup()
        {
            this.McModuleGetter?.Get()?.OnCleanup(this);
        }
    }

    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class McSceneModule
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Macross.MacrossFieldAttribute()]
        [Browsable(false)]
        public GSceneModule HostModule
        {
            get;
            set;
        }
        
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnNewMacross()
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnSetMacross(GSceneModule comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnUnsetMacross(GSceneModule comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnInit(GSceneModule comp)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnTick(GSceneModule comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnCleanup(GSceneModule comp)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnAddedModule(GSceneModule comp, GSceneModule addedComponent)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnRemovedModule(GSceneModule comp, GSceneModule removedComponent)
        {

        }
    }

    public partial class GSceneGraph
    {
        [Browsable(false)]
        public Dictionary<System.Type, GSceneModule> Modules
        {
            get;
        } = new Dictionary<System.Type, GSceneModule>();

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GSceneModule GetModule(System.Type type)
        {
            using (var i = Modules.Values.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    var cur = i.Current;
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
            }
            return null;
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GSceneModule AddModule(GSceneModule comp)
        {
            GSceneModule old;
            if (Modules.TryGetValue(comp.GetType(), out old) == true)
            {
                comp.Host = this;
                Modules[comp.GetType()] = comp;
                return old;
            }
            else
            {
                comp.Host = this;
                Modules[comp.GetType()] = comp;
                comp.OnAdded();
                return null;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveModule(System.Type type)
        {
            GSceneModule old;
            if (Modules.TryGetValue(type, out old) == true)
            {
                old.OnRemove();
                old.Host = null;
                Modules.Remove(type);
            }
        }

        async System.Threading.Tasks.Task<bool> Load_Modules(CRenderContext rc, IO.XndNode node)
        {
            var moduleNode = node.FindNode("SceneModules");
            if (moduleNode == null)
                return true;
            var nodes = moduleNode.GetNodes();
            foreach (var i in nodes)
            {
                var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
                var attr = i.FindAttrib("Initializer");
                attr.BeginRead();
                var initializer = attr.ReadMetaObject() as GSceneModule.GSceneModuleInitializer;
                if(initializer!=null)
                {
                    var module = Rtti.RttiHelper.CreateInstance(type) as GSceneModule;
                    if (false == await module.SetInitializer(rc, this, initializer))
                    {
                        //
                    }
                    else
                    {
                        AddModule(module);
                    }
                }
                attr.EndRead();
            }
            return true;
        }
        void Save_Modules(IO.XndNode node)
        {
            var moduleNode = node.AddNode("SceneModules", 0, 0);
            foreach(var i in Modules)
            {
                var curNode = moduleNode.AddNode(Rtti.RttiHelper.GetTypeSaveString(i.Key), 0, 0);
                var attr = curNode.AddAttrib("Initializer");
                attr.BeginWrite();
                attr.WriteMetaObject(i.Value.Initializer);
                attr.EndWrite();
            }
        }

        void Tick_Modules()
        {
            using (var i = Modules.Values.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    i.Current.Tick();
                }
            }
        }
        void Cleanup_Modules()
        {
            using (var i = Modules.Values.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    i.Current.Cleanup();
                }
            }
        }
    }
}
