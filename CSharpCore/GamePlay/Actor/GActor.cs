using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace EngineNS.GamePlay.Actor
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_PlantAbleActor("Actor", "GActor")]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public partial class GActor
        : INotifyPropertyChanged, EngineNS.Editor.IPlantable, Component.IPlaceable, Component.IComponentContainer
    {

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static bool IsNull(GActor actor)
        {
            return actor == null ? true : false;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static GActor Null()
        {
            return null;
        }
        #region static Creator
        public static GActor NewMeshActorDirect(Graphics.Mesh.CGfxMesh SceneMesh)
        {
            var rc = CEngine.Instance.RenderContext;
            // MeshActor
            var actor = new GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            //var placement = new GamePlay.Component.GPlacementComponent();
            var placement = new GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new GamePlay.Component.GMeshComponent();
            meshComp.SetSceneMesh(rc.ImmCommandList, SceneMesh);
            actor.AddComponent(meshComp);
            return actor;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task<GActor> NewMeshActorAsync(
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]
            RName gms)
        {
            var rc = CEngine.Instance.RenderContext;

            var actor = new GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new GamePlay.Component.GMeshComponent();
            var meshCompInit = new GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.MeshName = gms;
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            actor.AddComponent(meshComp);
            return actor;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task<GActor> NewPrefabActorAsync(
            [Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Prefab)]
            RName prefabname)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = await CEngine.Instance.PrefabManager.GetPrefab(rc, prefabname, true);

            return actor;
        }
        static Profiler.TimeClip mClip_NewPrefabeActorTo = new Profiler.TimeClip("NewPrefabActorTo", 1000, (clip, time) =>
        {
            System.Diagnostics.Debug.WriteLine($"TimeClip {clip.Name} time out: {time}/{clip.MaxElapse}");
        });
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task<GActor> NewPrefabActorTo(
            [Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Prefab)]
            RName prefabname,
            SceneGraph.GSceneGraph scene,
            Vector3 location, Quaternion quaternion, Vector3 scale)
        {
            using( new Profiler.TimeClipHelper(mClip_NewPrefabeActorTo))
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
                GActor actor = await CEngine.Instance.PrefabManager.GetPrefab(rc, prefabname, true);
                if (actor == null)
                    return null;
                if (actor.Children.Count == 1)
                {//老版本的prefab多了一层，这里强行换一下
                 //actor = actor.Children[0];
                }
                if (scene != null)
                    actor.AddToScene(scene);
                actor.Placement.Location = location;
                actor.Placement.Rotation = quaternion;
                actor.Placement.Scale = scale;
                return actor;
            }
        }
        public static async System.Threading.Tasks.Task<GActor> NewMeshActorAsync(List<RName> gmses/*, RName senv*/)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new GamePlay.Component.GMutiMeshComponent();
            foreach (var gms in gmses)
            {
                var drawMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, gms/*, shadingEnv*/);
                meshComp.AddSubMesh(rc, drawMesh);
            }
            actor.AddComponent(meshComp);
            return actor;
        }

        public static async System.Threading.Tasks.Task<GActor> NewSunActorAsync(RName gms)
        {
            var rc = CEngine.Instance.RenderContext;

            var actor = await NewMeshActorAsync(gms);

            var dirlightComp = new GamePlay.Component.GDirLightComponent();
            var init = new GamePlay.Component.GDirLightComponent.GDirLightComponentInitializer();
            await dirlightComp.SetInitializer(rc, actor, actor, init);
            actor.AddComponent(dirlightComp);
            actor.SpecialName = "SunActor";

            return actor;
        }
        #endregion

        public static int InstanceNumber = 0;
        public GActor()
        {
            InstanceNumber++;
        }
        ~GActor()
        {
            Cleanup();
            InstanceNumber--;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void Cleanup()
        {
            if (SpecialName == "@@DistroyedActor")
            {
                return;
            }
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i].OnRemove();
                Components[i].Cleanup();
            }
            Components.Clear();

            //这里有GActor和Initializer垃圾回收析构调用顺序问题，所以这不能再访问Initializer了
            mMcActorGetter?.Get(false)?.OnCleanup(this);
            mMcActorGetter?.Get(false)?.OnUnRegisterInput();
            if (HitProxyId != 0)
            {
                CEngine.Instance.HitProxyManager.UnmapActor(this.HitProxyId);
                HitProxyId = 0;
            }
            SpecialName = "@@DistroyedActor";
        }
        [ReadOnly(true)]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Editor.Editor_UseCustomEditor]
        public Guid ActorId
        {
            get
            {
                if (Initializer != null)
                    return Initializer.ActorId;
                return Guid.Empty;
            }
            set
            {
                if (Initializer != null)
                    Initializer.ActorId = value;
            }
        }
        [Browsable(false)]
        public UInt32 PVSId
        {
            get
            {
                if (Initializer != null)
                    return Initializer.PVSId;
                return UInt32.MaxValue;
            }
            set
            {
                if (Initializer != null)
                    Initializer.PVSId = value;
            }
        }
        protected UInt64 mRenderSerialId;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [DisplayName("Name")]
        public string SpecialName
        {
            get
            {
                if (Initializer != null)
                    return Initializer.SpecialName;
                return "";
            }
            set
            {
                if (Initializer != null)
                {
                    if (Scene != null && Scene.World != null)
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            var dict = Scene.World.SpecialNamedActors;
                            lock (dict)
                            {
                                if (dict.ContainsKey(Initializer.SpecialName))
                                    dict.Remove(Initializer.SpecialName);
                            }
                        }
                        else
                        {
                            var dict = Scene.World.SpecialNamedActors;
                            lock (dict)
                            {
                                dict[value] = this;
                            }
                        }
                    }

                    Initializer.SpecialName = value;
                    OnPropertyChanged("SpecialName");
                }
            }
        }
        [Rtti.MetaClassAttribute]
        [Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
        public class GActorInitializer : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            [ReadOnly(true)]
            public Guid ActorId
            {
                get;
                set;
            } = Guid.Empty;

            // 在Scene中Actor的唯一ID
            [Rtti.MetaData]
            [Browsable(false)]
            public UInt32 PVSId
            {
                get;
                set;
            } = UInt32.MaxValue;
            [Rtti.MetaData]
            public string SpecialName
            {
                get;
                set;
            }
            [Rtti.MetaData]
            [Editor.Editor_PackData()]
            public RName ActorMacross
            {
                get;
                set;
            }
            [Rtti.MetaData]
            [Editor.Editor_PackData()]
            public RName CenterDataName
            {
                get;
                set;
            }
            [Rtti.MetaData]
            [Browsable(false)]
            public List<Guid> DependActors
            {
                get;
                set;
            } = new List<Guid>();
            [Rtti.MetaData]
            [DisplayName("参与遮挡剔除")]
            public bool InPVS
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.InPVS);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.InPVS, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否投射阴影")]
            public bool CastShadow
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.CastShadow);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.CastShadow, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否接受光照")]
            public bool AcceptLights
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.AcceptLights);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.AcceptLights, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否静态物体")]
            public bool StaticObject
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.StaticObject);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.StaticObject, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否可见")]
            public bool Visible
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.Visible);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.Visible, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否需要Tick")]
            public bool NeedTick
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.NeedTick);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.NeedTick, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否参与导航网格计算")]
            public bool IsNavgation
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.Navgation);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.Navgation, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否参与绑定体计算")]
            public bool IsBoundingVolume
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.BoundVolume);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.BoundVolume, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否在游戏模式下隐藏")]
            public bool HideInGame
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.HideInGame);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.HideInGame, value);
                }
            }
            [Rtti.MetaData]
            [DisplayName("是否只是游戏运行对象")]
            public bool OnlyForGame
            {
                get
                {
                    return mActorBits.IsBit(GActorBits.EBitDefine.OnlyForGame);
                }
                set
                {
                    mActorBits.SetBit(GActorBits.EBitDefine.OnlyForGame, value);
                }
            }
            private GActorBits mActorBits = new GActorBits();
            public GActorBits ActorBits
            {
                get { return mActorBits; }
            }
        }
        [Browsable(false)]
        public GActorInitializer Initializer
        {
            get;
            set;
        } = new GActorInitializer();
        protected GActor mParent;
        [Browsable(false)]
        public GActor Parent
        {//只是位置跟随的逻辑上的父，和存储等组织没有任何关系
            get { return mParent; }
        }
        private List<GActor> mChildren;
        internal List<GActor> Children
        {
            get
            {
                if (mChildren == null)
                    mChildren = new List<GActor>();
                return mChildren;
            }
        }

        //数据不能直接添加和删除 需要使用SetParent方法
        public List<GActor> GetChildrenUnsafe()
        {
            return Children;
        }
        public delegate void Delegate_OnChildOperation(Actor.GActor actor);
        public event Delegate_OnChildOperation OnAddChild;
        public event Delegate_OnChildOperation OnRemoveChild;
        public void _OnAddChild(Actor.GActor actor)
        {
            OnAddChild?.Invoke(actor);
        }
        public void _OnRemoveChild(Actor.GActor actor)
        {
            OnRemoveChild?.Invoke(actor);
        }
        public virtual void SetParent(GActor parent, bool isRelativeCoord = true)
        {
            if (parent == mParent)
                return;
            if (isRelativeCoord == false && parent != null)
            {
                var parentMatrix = parent.Placement.WorldMatrix;
                var invParent = Matrix.Invert(ref parentMatrix);
                var relativeMatrix = invParent * this.Placement.WorldMatrix;

                this.Placement.SetMatrix(ref relativeMatrix);
            }
            if (mParent != null)
            {
                lock (mParent)
                {
                    for (int i = 0; i < mParent.Children.Count; i++)
                    {
                        if (mParent.Children[i] == this)
                        {
                            mParent.Children.RemoveAt(i);
                            mParent._OnRemoveChild(this);
                            if (mParent.Children.Count == 0)
                                mParent.mChildren = null;
                            break;
                        }
                    }
                }
            }
            mParent = parent;
            if (mParent != null)
            {
                lock (mParent)
                {
                    for (int i = 0; i < mParent.Children.Count; i++)
                    {
                        if (mParent.Children[i] == this)
                            return;
                    }
                    mParent.Children.Add(this);
                    mParent._OnAddChild(this);
                }
            }
        }
        public virtual void SetInitializer(GActorInitializer initializer)
        {
            Initializer = initializer;
            CenterDataName = initializer.CenterDataName;
            this.ActorMacross = initializer.ActorMacross;
            McActor?.OnInit(this);
        }
        public async System.Threading.Tasks.Task<GPrefab> ConvertToPrefab(CRenderContext rc)
        {
            if (this is GPrefab)
                return this as GPrefab;
            var result = new GPrefab();
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
        public async virtual System.Threading.Tasks.Task<GActor> Clone(CRenderContext rc)
        {
            var result = new GActor();
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
            return result;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public SceneGraph.GSceneGraph Scene
        {
            get;
            set;
        }
        [ReadOnly(true)]
        public UInt32 HitProxyId
        {
            get;
            set;
        }
        public BoundingBox LocalBoundingBox = BoundingBox.EmptyBox();// new BoundingBox(Vector3.UnitXYZ, -Vector3.UnitXYZ);
        [Browsable(false)]
        public bool Selected
        {
            get { return Initializer.ActorBits.IsBit(GActorBits.EBitDefine.Selected); }
            set
            {
                Initializer.ActorBits.SetBit(GActorBits.EBitDefine.Selected, value);
                for (int i = 0; i < Components.Count; ++i)
                {
                    var opComp = Components[i] as Component.IComponentHostSelectOperation;
                    opComp?.OnHostSelected(value);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Visible
        {
            get
            {
                return Initializer.Visible;
            }
            set
            {
                Initializer.Visible = value;
                OnPropertyChanged("Visible");
                for (int i = 0; i < Children.Count; ++i)
                {
                    Children[i].Visible = value;
                }
            }
        }
        public bool NeedTick
        {
            get
            {
                return Initializer.NeedTick;
            }
            set
            {
                Initializer.NeedTick = value;
                for (int i = 0; i < Children.Count; ++i)
                {
                    Children[i].NeedTick = value;
                }
            }
        }
        public bool CastShadow
        {
            get
            {
                return Initializer.CastShadow;
            }
            set
            {
                Initializer.CastShadow = value;
                for (int i = 0; i < Children.Count; ++i)
                {
                    Children[i].CastShadow = value;
                }
            }
        }
        public bool AcceptLights
        {
            get
            {
                return Initializer.AcceptLights;
            }
            set
            {
                Initializer.AcceptLights = value;
            }
        }
        public bool StaticObject
        {
            get
            {
                return Initializer.StaticObject;
            }
            set
            {
                Initializer.StaticObject = value;
            }
        }
        public bool IsBoundingVolume
        {
            get
            {
                return Initializer.IsBoundingVolume;
            }
            set
            {
                Initializer.IsBoundingVolume = value;
            }
        }
        public bool HideInGame
        {
            get
            {
                return Initializer.HideInGame;
            }
            set
            {
                Initializer.HideInGame = value;
                for (int i = 0; i < Children.Count; ++i)
                {
                    Children[i].HideInGame = value;
                }
            }
        }
        public bool OnlyForGame
        {
            get
            {
                return Initializer.OnlyForGame;
            }
            set
            {
                Initializer.OnlyForGame = value;
                for (int i = 0; i < Children.Count; ++i)
                {
                    Children[i].OnlyForGame = value;
                }
            }
        }
        public object Tag
        {
            get;
            set;
        }

        #region CallBack
        public virtual void OnAddToWorld(GWorld world)
        {
            if (string.IsNullOrEmpty(this.SpecialName) == false)
            {
                var dict = world.SpecialNamedActors;
                lock (dict)
                {
                    dict[this.SpecialName] = this;
                }
            }
        }
        public virtual void OnRemoveWorld(GWorld world)
        {
            if (string.IsNullOrEmpty(this.SpecialName) == false)
            {
                var dict = world.SpecialNamedActors;
                lock (dict)
                {
                    if (dict.ContainsKey(this.SpecialName))
                        dict.Remove(this.SpecialName);
                }
            }
        }
        public virtual void OnAddToSceneGraph(SceneGraph.GSceneGraph scene)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i]?.OnAddedScene();
            }
            mMcActorGetter?.Get()?.OnAddedScene();
        }
        public virtual void OnRemoveSceneGraph(SceneGraph.GSceneGraph scene)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i]?.OnRemoveScene();
            }
            mMcActorGetter?.Get()?.OnRemoveScene();
        }
        public bool NeedCheckVisible(SceneGraph.CheckVisibleParam param)
        {
            if (mRenderSerialId == param.SerialID)
                return false;

            if (!Visible)
                return false;

            if (HideInGame && CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                return false;
            }

            if (param.ForShadow)
            {
                if (this.CastShadow == false)
                    return false;
            }
            return true;
        }

        internal UInt32 PointLightsSerialId = 0;
        public List<SceneGraph.GSceneGraph.AffectLight> StaticLights = new List<SceneGraph.GSceneGraph.AffectLight>(4);
        public List<SceneGraph.GSceneGraph.AffectLight> AffectLights = new List<SceneGraph.GSceneGraph.AffectLight>(4);
        public int AffectLightNum
        {
            get { return AffectLights.Count; }
        }
        public static Profiler.TimeScope ScopeOnVisible = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), nameof(OnCheckVisible));
        public static Profiler.TimeScope ScopeOnVisPointLights = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), "OnVisPointLights");
        public static Profiler.TimeScope ScopeOnVisPushInstance = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), "OnCheckVisible.PushInstance");
        public static Profiler.TimeScope ScopeOnVisCommitVisual = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), "OnCheckVisible.CommitVisual");
        public static Profiler.TimeScope ScopeOnVisLightIndices = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), "OnCheckVisible.LightIndices");
        public virtual void OnCheckVisible(CCommandList cmd, SceneGraph.GSceneGraph scene, Graphics.CGfxCamera camera, SceneGraph.CheckVisibleParam SceneCullDesc)
        {
            bool bSetCBuffer = false;
            using (new Profiler.TimeScopeHelper(ScopeOnVisible))
            {
                if (false == NeedCheckVisible(SceneCullDesc))
                    return;

                mRenderSerialId = SceneCullDesc.SerialID;
                if ((CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor) &&
                    ((mComponentFlags & EComponentFlags.HasInstancing) != 0) &&
                    CEngine.UseInstancing)
                {
                    Component.GMeshComponent meshComp = null;
                    Component.GInstancingComponent instComp = null;
                    for (int i = 0; i < Components.Count; ++i)
                    {
                        if (Components[i].IsVisualComponent)
                        {
                            using (new Profiler.TimeScopeWindows(ScopeOnVisLightIndices))
                            {
                                meshComp = Components[i] as Component.GMeshComponent;
                                if (meshComp != null)
                                {
                                    instComp = meshComp.mInstancingComp;
                                }
                            }   
                            if (instComp != null)
                            {
                                using (new Profiler.TimeScopeWindows(ScopeOnVisPushInstance))
                                {
                                    UInt32_4 lightIndices = new UInt32_4();
                                    lightIndices.SetValue(0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF);
                                    if (SceneCullDesc.ForShadow)
                                    {
                                        CEngine.Instance.MeshManager.InstancingManager.PushInstance(instComp, 0, ref lightIndices, meshComp.SceneMesh.OrigionMesh, Component.GInstancingManager.EPoolType.Shadow);
                                    }
                                    else
                                    {
                                        int usedLightCount = Math.Min(AffectLights.Count, 4);
                                        for (int j = 0; j < usedLightCount; j++)
                                        {
                                            lightIndices[(uint)j] = (UInt32)AffectLights[j].Light.IndexInScene;
                                        }
                                        CEngine.Instance.MeshManager.InstancingManager.PushInstance(instComp, AffectLights.Count, ref lightIndices, meshComp.SceneMesh.OrigionMesh, Component.GInstancingManager.EPoolType.Normal);
                                    }
                                }
                            }
                            else
                            {
                                using (new Profiler.TimeScopeWindows(ScopeOnVisCommitVisual))
                                {
                                    Components[i].CommitVisual(cmd, camera, SceneCullDesc);
                                    bSetCBuffer = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (new Profiler.TimeScopeWindows(ScopeOnVisCommitVisual))
                    {
                        bSetCBuffer = true;
                        for (int i = 0; i < Components.Count; ++i)
                        {
                            if (Components[i].IsVisualComponent)
                            {
                                Components[i].CommitVisual(cmd, camera, SceneCullDesc);
                            }
                        }
                    }   
                }

                if (SceneCullDesc.ForShadow == false)
                {
                    if (this.AcceptLights)
                    {
                        using (new Profiler.TimeScopeHelper(ScopeOnVisPointLights))
                        {
                            Scene?.GetAffectLights(camera.SceneView, this, bSetCBuffer);
                        }
                    }
                }
                if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    OnEditorCheckVisible(cmd, camera, SceneCullDesc);
            }
        }
        partial void OnEditorCheckVisible(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param);
        #endregion

        #region Component
        [Flags]
        public enum EComponentFlags : UInt32
        {
            HasInstancing = 1,
            HasParticle = (1 << 1),
            HasPhysics = (1 << 2),
        }
        public EComponentFlags mComponentFlags = 0;
        [Browsable(false)]
        public List<Component.GComponent> Components
        {
            get;
        } = new List<Component.GComponent>();
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void VisitChildComponents(Component.GComponentsContainer.FOnVisitComponent visit, object arg)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                visit(Components[i], arg);

                var compContain = Components[i] as Component.GComponentsContainer;
                if (compContain == null)
                    continue;

                compContain.VisitChildComponents(visit, arg);
            }
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
                else
                {
                    var compContain = Components[i] as Component.GComponentsContainer;
                    if (compContain == null)
                        continue;
                    var childCom = compContain.FindParticleByName(name);
                    if (childCom != null)
                        return childCom;
                }
              
            }
            return null;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
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
                else
                {
                    var compContain = Components[i] as Component.GComponentsContainer;
                    if (compContain == null)
                        continue;
                    compContain.ResetParticleByName(name);
                }

            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
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
                    var childCom = compContain.FindComponentBySpecialName(name);
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
        [return: Editor.Editor_TypeChangeWithParam(1)]
        public Component.GComponent FindComponentBySpecialNameRecursionAs(string name,
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            Type type)
        {
            return FindComponentBySpecialNameRecursion(name);
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
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
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
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public List<Component.GComponent> GetComponents(
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            System.Type type)
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
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public List<Component.GComponent> GetComponentsRecursion(
            [Editor.Editor_TypeFilterAttribute(typeof(Component.GComponent))]
            System.Type type)
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
        public Component.GComponent AddComponent(Component.GComponent comp)
        {
            if (!Components.Contains(comp))
            {
                comp.Host = this;
                comp.HostContainer = this;
                Components.Add(comp);
                comp.OnAdded();
                if (comp is Component.IPlaceable)
                {
                    var placeable = comp as Component.IPlaceable;
                    comp.McCompGetter?.Get()?.OnPlacementChanged(this);
                    placeable.OnPlacementChanged(Placement);
                }
                return null;
            }
            if (IsActorLoaded)
            {
                comp.OnActorLoaded(this);
            }
            return comp;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool InsertComponent(int index, Component.GComponent comp)
        {
            if (!Components.Contains(comp))
            {
                comp.Host = this;
                comp.HostContainer = this;
                Components.Insert(index, comp);
                comp.OnAdded();
                OnPlacementChanged(Placement);
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
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void ClearComponents()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                var com = Components[i] as EngineNS.GamePlay.Component.IComponentContainer;
                if (com != null)
                {
                    com.ClearComponents();
                }

                RemoveComponent(Components[i]);
            }
        }
        protected Component.GPlacementComponent mPlacement;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Component.GPlacementComponent Placement
        {
            get { return mPlacement; }
            set
            {
                mPlacement = value;
                if (mPlacement != null)
                {
                    mPlacement.Host = this;
                    mPlacement.HostContainer = this;
                    Components.Add(mPlacement);
                }
            }
        }
        public void OnDrawMatrixChanged()
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i].OnUpdateDrawMatrix(ref Placement.mDrawTransform);
            }
        }

        partial void PlacementChangedCallback();
        partial void PlacementChangeDefault();

        public void CallPlacementChangeDefault()
        {
            PlacementChangeDefault();
        }
        public void OnPlacementChanged(Component.GPlacementComponent placement)
        {
            if (this.Parent != null)
            {
                Placement.DrawTransform = Placement.Transform * Parent.Placement.WorldMatrix;
            }
            else
            {
                Placement.DrawTransform = Placement.Transform;
            }
            //每个组件自己管自己的DrawMatrix
            //OnDrawMatrixChanged();
            for (int i = 0; i < Components.Count; ++i)
            {
                var placeable = Components[i] as Component.IPlaceable;
                if (placeable != null)
                {
                    Components[i].McCompGetter?.Get()?.OnPlacementChanged(this);
                    placeable.OnPlacementChanged(placement);
                }
                else
                {
                    var phyComp = Components[i] as Bricks.PhysicsCore.CollisionComponent.GPhysicsCollisionComponent;
                    if (phyComp != null)
                        phyComp.OnPlacementChanged(placement);
                }

            }
            if (mChildren != null)
            {
                var worldMatrix = this.Placement.WorldMatrix;
                for (int i = 0; i < Children.Count; i++)
                {
                    var cld = Children[i];
                    if (cld.Placement == null)
                        continue;
                    cld.OnPlacementChanged(placement);
                }
            }

            PlacementChangedCallback();
        }

        public void OnPlacementChangedUninfluencePhysics(Component.GPlacementComponent placement)
        {
            if (this.Parent != null)
            {
                Placement.DrawTransform = Placement.Transform * Parent.Placement.WorldMatrix;
            }
            else
            {
                Placement.DrawTransform = Placement.Transform;
            }
            //每个组件自己管自己的DrawMatrix
            //OnDrawMatrixChanged();
            for (int i = 0; i < Components.Count; ++i)
            {
                var placeable = Components[i] as Component.IPlaceable;
                if (placeable != null)
                {
                    placeable.OnPlacementChanged(placement);
                }
            }
            if (mChildren != null)
            {
                var worldMatrix = this.Placement.WorldMatrix;
                for (int i = 0; i < Children.Count; i++)
                {
                    var cld = Children[i];
                    if (cld.Placement == null)
                        continue;
                    cld.Placement.DrawTransform = cld.Placement.Transform * worldMatrix;
                    cld.OnDrawMatrixChanged();
                }
            }

            PlacementChangedCallback();
        }
        #endregion

        #region IO
        public virtual void Save2Xnd(IO.XndNode node)
        {
            OnSave2Xnd(node);
            var cpNodes = node.AddNode("Comps", 0, 0);
            for (int i = 0; i < Components.Count; ++i)
            {
                if (Components[i].DontSave)
                    continue;
                var typeName = Rtti.RttiHelper.GetTypeSaveString(Components[i].GetType());
                var cnode = cpNodes.AddNode(typeName, 0, 0);
                Components[i].Save2Xnd(cnode);
            }

            if (Children.Count > 0)
            {
                var cNodes = node.AddNode("Children", 0, 0);
                foreach (var i in Children)
                {
                    var typeName = Rtti.RttiHelper.GetTypeSaveString(i.GetType());
                    var cnode = cNodes.AddNode(typeName, 0, 0);
                    i.Save2Xnd(cnode);
                }
            }

            if (CenterData != null)
            {
                var attr = node.AddAttrib("CenterData");
                attr.BeginWrite();
                attr.WriteMetaObject(CenterData);
                attr.EndWrite();
            }
        }
        private bool IsActorLoaded = false;
        public void OnActorLoaded()
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i].OnActorLoaded(this);
            }
            McActor?.OnLoadedAll(this);
            IsActorLoaded = true;
            Placement?.UpdateActorAABB();
        }
        public virtual async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, IO.XndNode node)
        {
            try
            {
                for (int i = 0; i < Components.Count; ++i)
                {
                    Components[i].OnRemove();
                }
                Components.Clear();

                if (await OnLoadXnd(node) == false)
                    return true;

                var cpNodes = node.FindNode("Comps");
                if (cpNodes != null)
                {
                    var nodes = cpNodes.GetNodes();
                    foreach (var i in nodes)
                    {
                        var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
                        if (type == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "ACC", $"Component {i.GetName()} is invalid");
                            continue;
                        }
                        Component.GComponent comp = null;
                        var atttibutes = type.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), true);
                        if (atttibutes.Length > 0)
                        {
                            //comp = await EngineNS.GamePlay.Component.GMacrossComponent.CreateComponent(type);
                            //comp = EngineNS.CEngine.Instance.MacrossDataManager.NewObject(type) as EngineNS.GamePlay.Component.GComponent;
                            continue;
                        }
                        else
                        {
                            comp = Activator.CreateInstance(type) as Component.GComponent;
                        }
                        if (false == await comp.LoadXnd(rc, this, this, i))
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "ACC", $"Component {i.GetName()} load failed");
                            continue;
                        }
                        comp.HostContainer = this;
                        this.AddComponent(comp);
                        //Components[type] = comp;
                    }
                }

                var Nodes = node.FindNode("Children");
                if (Nodes != null)
                {
                    var actNodes = Nodes.GetNodes();

                    var progressDelta = 1.0f / actNodes.Count;

                    var smp = Thread.ASyncSemaphore.CreateSemaphore(actNodes.Count);
                    if (actNodes != null)
                    {
                        while (actNodes.Count > 0)
                        {
                            var delayNodes = new List<IO.XndNode>();
                            for (int i = 0; i < actNodes.Count; i++)
                            {
                                var actNode = actNodes[i];
                                bool dependFinished = true;
                                var depends = actNode.FindAttrib("DependActors");
                                if (depends != null)
                                {
                                    depends.BeginRead();
                                    int actNum = 0;
                                    depends.Read(out actNum);
                                    for (int dpIdx = 0; dpIdx < actNum; dpIdx++)
                                    {
                                        Guid dependActorId;
                                        depends.Read(out dependActorId);
                                        if (Scene.FindActor(dependActorId) == null)
                                        {
                                            dependFinished = false;
                                            break;
                                        }
                                    }
                                    depends.EndRead();
                                }
                                if (dependFinished)
                                {
                                    CEngine.Instance.EventPoster.RunOn(async () =>
                                    {
                                        var actTypeName = actNode.GetName();
                                        var type = Rtti.RttiHelper.GetTypeFromSaveString(actTypeName);
                                        if (type == null)
                                        {
                                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{actTypeName} is invalid");
                                            smp.Release();
                                            return false;
                                        }
                                        var act = System.Activator.CreateInstance(type) as Actor.GActor;
                                        if (act == null)
                                        {
                                            smp.Release();
                                            return false;
                                        }
                                        if (false == await act.LoadXnd(rc, actNode))
                                        {
                                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{actTypeName} LoadXnd failed");
                                        }

                                        act.SetParent(this);
                                        smp.Release();
                                        return true;
                                    }, Thread.Async.EAsyncTarget.AsyncIO);
                                }
                                else
                                {
                                    delayNodes.Add(actNode);
                                }
                            }
                            actNodes = delayNodes;
                        }
                    }
                    await smp.Await();
                }

                if (CenterData != null)
                {
                    var ret = await CEngine.Instance.EventPoster.Post(() =>
                    {
                        var attr = node.FindAttrib("CenterData");
                        if (attr != null)
                        {
                            attr.BeginRead();
                            var savedCenterData = attr.ReadMetaObject(null) as GCenterData;
                            attr.EndRead();
                            if (savedCenterData != null)
                            {
                                savedCenterData.HostActor = this;
                                Rtti.MetaClass.CopyData(savedCenterData, CenterData);
                            }
                        }
                        return true;
                    }, Thread.Async.EAsyncTarget.AsyncIO);
                }
                OnActorLoaded();

                OnPlacementChanged(Placement);
                return true;
            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Actor Load", e.ToString());
            }
            return false;
        }
        protected virtual void OnSave2Xnd(IO.XndNode node)
        {
            if (Initializer != null)
            {
                var attr = node.AddAttrib("Initializer");
                attr.BeginWrite();
                attr.WriteMetaObject(Initializer);
                attr.EndWrite();

                if (Initializer.DependActors != null && Initializer.DependActors.Count > 0)
                {
                    attr = node.AddAttrib("DependActors");
                    attr.BeginWrite();
                    attr.Write(Initializer.DependActors.Count);
                    foreach (var i in Initializer.DependActors)
                    {
                        attr.Write(i);
                    }
                    attr.EndWrite();
                }
            }
        }
        protected virtual async System.Threading.Tasks.Task<bool> OnLoadXnd(IO.XndNode node)
        {
            var attr = node.FindAttrib("Initializer");
            if (attr == null)
            {
                return true;
            }

            GActorInitializer ainit = null;
            var ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                attr.BeginRead();
                ainit = attr.ReadMetaObject(null) as GActorInitializer;
                if (ainit == null)
                {
                    attr.EndRead();
                    return false;
                }
                attr.EndRead();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (ret)
                SetInitializer(ainit);
            return ret;
        }
        #endregion

        public void PreUse(bool force)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                Components[i].PreUse(force);
            }
            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].PreUse(force);
            }
        }
        //性能不好，调用的时候小心，一般来说请用Placement.ActorAABB替代是正确的
        public virtual void GetAABB(ref BoundingBox aabb)
        {
            if (Placement == null)
            {
                aabb.Minimum = LocalBoundingBox.Minimum;
                aabb.Maximum = LocalBoundingBox.Maximum;
            }
            else
            {
                if (LocalBoundingBox.IsEmpty() && Children.Count == 0)
                {
                    aabb = LocalBoundingBox;
                    return;
                }

                var worldMatrix = Placement.WorldMatrix;
                BoundingBox.Transform(ref LocalBoundingBox, ref worldMatrix, out aabb);
                
                if (Children.Count > 0)
                {
                    var childAABB = new BoundingBox();
                    for (int i = 0; i < Children.Count; ++i)
                    {
                        Children[i].GetAABB(ref childAABB);
                        aabb = BoundingBox.Merge(aabb, childAABB);
                    }
                }
                //var corner = Vector3.TransformCoordinate(LocalBoundingBox.Minimum, Placement.WorldMatrix);
                //aabb.Merge(ref corner);
                //corner = Vector3.TransformCoordinate(LocalBoundingBox.Maximum, Placement.WorldMatrix);
                //aabb.Merge(ref corner);
            }
        }
        public virtual Vector3 GetAABBCenter()
        {
            BoundingBox aabb = new BoundingBox();
            GetAABB(ref aabb);
            return aabb.GetCenter();
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), nameof(Tick));
        public void TryTick()
        {
            if (NeedTick == false)
                return;

            ScopeTick.Begin();
            Tick();
            ScopeTick.End();
        }
        public virtual void Tick()
        {
            if (this.HideInGame && CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                return;
            }
            for (int i = 0; i < Components.Count; ++i)
            {
                var comp = Components[i];
                if (comp.EnableTick == false)
                    continue;
                if (comp.OnlyForGame && CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                {
                    continue;
                }
                var scope = comp.GetTickTimeScope();
                if (scope != null)
                    scope.Begin();
                if (comp.TickBeforeCheckVisible)
                {
                    CEngine.Instance.ThreadRender.RegBeforeRenderCommitComp(comp);
                }
                else
                {
                    comp.Tick(Placement);
                }
                //Macross只能在LocigThread执行，否则无法调试
                comp.McCompGetter?.Get(comp.OnlyForGame)?.OnTick(comp);
                if (scope != null)
                    scope.End();

            }
            McActor?.OnTick(this);
            //for (int i = 0; i < Children.Count; ++i)
            //{
            //    Children[i].Tick();
            //}
        }

        public async Task<GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var init = new GActorInitializer();
            SetInitializer(init);

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            Placement = placement;
            placement.Location = param.Location;

            return this;
        }

        public static Profiler.TimeScope ScopeCheckContain = Profiler.TimeScopeManager.GetTimeScope(typeof(GActor), nameof(CheckContain));
        public Graphics.CGfxCamera.CFrustum.CONTAIN_TYPE CheckContain(Graphics.CGfxCamera.CFrustum frustum, bool checkFrustum)
        {
            if (checkFrustum == false)
                return Graphics.CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_INNER;
            if (frustum == null)
                return Graphics.CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER;
            using (new Profiler.TimeScopeWindows(ScopeCheckContain))
            {
                return frustum.WhichContainType(ref Placement.ActorAABB, false);
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable, "根据轴和角速度（度/秒）旋转对象")]
        public void RotationWithAxisAndSpeed(EngineNS.Vector3 axis, float speed)
        {
            if (this.Placement == null)
                return;

            var spd = (float)(EngineNS.CEngine.Instance.EngineElapseTime * (speed * 0.001f / 180.0f * System.Math.PI));
            var rot = Quaternion.RotationAxis(axis, spd);
            Placement.Rotation = Quaternion.Multiply(Placement.Rotation, rot);
        }
    }

    //public class GCloneActor : GActor
    //{

    //}
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool UseInstancing = false;
        public static bool EnableShadow = true;
        public static bool EnableBloom = true;
        public static bool EnableSunShaft = true;
        public static bool UsePVS = true;
        public static bool EnableMobileAo = true;
    }
}
