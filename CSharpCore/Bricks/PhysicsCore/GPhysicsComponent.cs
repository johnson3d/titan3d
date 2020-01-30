using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Component;


namespace EngineNS.Bricks.PhysicsCore
{
    #region mutiShape Actor
    public class Editor_PhysicsCapsuleShape : Editor_PhysicsShape
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Capsule;
        private float mRadius = 0.5f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;

            }
        }
        private float mHalfHeight = 0.5f;
        public float HalfHeight
        {
            get
            {
                return mHalfHeight;
            }
            set
            {
                mHalfHeight = value;

            }
        }
        public Editor_PhysicsCapsuleShape(CPhyShape shape) : base(shape)
        {
        }
    }
    public class Editor_PhysicsSphereShape : Editor_PhysicsShape
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Sphere;
        private float mRadius = 1.0f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;

            }
        }
        public Editor_PhysicsSphereShape(CPhyShape shape) : base(shape)
        {
        }
    }
    public class Editor_PhysicsBoxShape : Editor_PhysicsShape
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Box;
        Vector3 mExtend = Vector3.Zero;
        public Vector3 Extend
        {
            get => mExtend;
            set
            {
                mExtend = value;
            }
        }
        public Editor_PhysicsBoxShape(CPhyShape shape) : base(shape)
        {
        }
    }
    public class Editor_PhysicsShape
    {
        CPhyShape mHostShape = null;
        bool mIsTrigger = false;
        public bool IsTrigger
        {
            get => mIsTrigger;
            set
            {
                mIsTrigger = value;
            }
        }
        GPlacementComponent mPlacement = new GPlacementComponent();
        public GPlacementComponent Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }
        Byte mAreaType = 0;
        public Byte AreaType
        {
            get
            {
                return mAreaType;
            }
            set
            {
                if (value > 0 && value < 16)
                    mAreaType = value;
            }
        }

        public virtual EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Unknown;
        public RName PhyMtlName
        {
            get;
            set;
        } = new RName("Physics/PhyMtl/default.phymtl", 0);
        public Editor_PhysicsShape(CPhyShape shape)
        {
            mHostShape = shape;
        }
    }
    public class Editor_MutiShapePhysicsCollision
    {
        [Browsable(false)]
        public CPhyActor mHostActor { get; set; } = null;
        public EPhyActorType Type { get; set; } = EPhyActorType.PAT_Static;
        List<Editor_PhysicsShape> mShape = new List<Editor_PhysicsShape>();
        public Editor_MutiShapePhysicsCollision(CPhyActor actor)
        {
            mHostActor = actor;
            Type = actor.PhyType;
            using (var it = actor.Shapes.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    Editor_PhysicsShape eShape = null;
                    var shape = it.Current;
                    switch (shape.ShapeType)
                    {
                        case EPhysShapeType.PST_Box:
                            {
                                eShape = new Editor_PhysicsBoxShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                eShape = new Editor_PhysicsCapsuleShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                eShape = new Editor_PhysicsShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_Convex:
                            {
                                eShape = new Editor_PhysicsBoxShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_TriangleMesh:
                            {
                                eShape = new Editor_PhysicsBoxShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_Plane:
                            {
                                eShape = new Editor_PhysicsBoxShape(shape);

                            }
                            break;
                        case EPhysShapeType.PST_HeightField:
                            {
                                eShape = new Editor_PhysicsBoxShape(shape);

                            }
                            break;
                    }
                    if (eShape != null)
                        mShape.Add(eShape);
                }
            }
        }
    }
    #endregion
    #region singleShape Actor
    public class Editor_PhysicsCollision
    {
        [Browsable(false)]
        public CPhyActor mHostActor { get; set; } = null;
        CPhyShape mHostShape = null;
        public EPhyActorType Type { get; set; } = EPhyActorType.PAT_Static;
        bool mIsTrigger = false;
        public bool IsTrigger
        {
            get => mIsTrigger;
            set
            {
                mIsTrigger = value;
            }
        }
        GPlacementComponent mPlacement = new GPlacementComponent();
        public GPlacementComponent Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }
        Byte mAreaType = 0;
        public Byte AreaType
        {
            get
            {
                return mAreaType;
            }
            set
            {
                if (value > 0 && value < 16)
                    mAreaType = value;
            }
        }
        [Browsable(false)]
        public virtual EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Unknown;
        public RName PhyMtlName
        {
            get;
            set;
        } = new RName("Physics/PhyMtl/default.phymtl", 0);
        public Editor_PhysicsCollision(CPhyActor actor)
        {
            mHostActor = actor;
            Type = actor.PhyType;
            using (var it = actor.Shapes.GetEnumerator())
            {
                it.MoveNext();
                mHostShape = it.Current;
            }
        }
    }

    public class Editor_PhysicsCapsuleCollision : Editor_PhysicsCollision
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Capsule;
        private float mRadius = 0.5f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;

            }
        }
        private float mHalfHeight = 0.5f;
        public float HalfHeight
        {
            get
            {
                return mHalfHeight;
            }
            set
            {
                mHalfHeight = value;

            }
        }
        public Editor_PhysicsCapsuleCollision(CPhyActor actor) : base(actor)
        {
        }
    }
    public class Editor_PhysicsSphereCollision : Editor_PhysicsCollision
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Sphere;
        private float mRadius = 1.0f;
        public float Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;

            }
        }
        public Editor_PhysicsSphereCollision(CPhyActor actor) : base(actor)
        {
        }
    }
    public class Editor_PhysicsBoxCollision : Editor_PhysicsCollision
    {
        public override EPhysShapeType ShapeType
        {
            get;
            set;
        } = EPhysShapeType.PST_Box;
        Vector3 mExtend = Vector3.Zero;
        public Vector3 Extend
        {
            get => mExtend;
            set
            {
                mExtend = value;
            }
        }
        public Editor_PhysicsBoxCollision(CPhyActor actor) : base(actor)
        {
        }
    }
    #endregion

    //[Editor.Editor_PlantAbleActor("Physics", "GPhysicsComponent")]
    //[GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhysicsComponentInitializer), "物理钢体组件", "Physics Component")]
    [Obsolete("Deprecated")]
    public partial class GPhysicsComponent : GamePlay.Component.GComponent, EngineNS.Editor.IPlantable
    {
        public static async System.Threading.Tasks.Task<EngineNS.Bricks.PhysicsCore.GPhysicsComponent> CreatePhysicsComponent(
            GamePlay.Actor.GActor actor,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyMaterial)]
            RName mtlName,
            EngineNS.Bricks.PhysicsCore.EPhyActorType type,
            EngineNS.Bricks.PhysicsCore.EPhysShapeType shapeType,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyGeom)]
            RName shapeName,
            Vector3 scale,
            float sphereRadius = 1.0f)
        {
            var phyComp = new EngineNS.Bricks.PhysicsCore.GPhysicsComponent();
            var phyInit = new EngineNS.Bricks.PhysicsCore.GPhysicsComponent.GPhysicsComponentInitializer();
            phyInit.ActorType = type;
            phyInit.ShapeList = new List<EngineNS.Bricks.PhysicsCore.GPhysicsComponent.GPhysicsComponentInitializer.ShapeDesc>();
            var shape = new EngineNS.Bricks.PhysicsCore.GPhysicsComponent.GPhysicsComponentInitializer.ShapeDesc();
            shape.ShapeType = shapeType;
            shape.MeshName = shapeName;
            shape.PhyMtlName = mtlName;
            shape.Scale = scale;
            shape.SphereRadius = sphereRadius;
            phyInit.ShapeList.Add(shape);
            await phyComp.SetInitializer(CEngine.Instance.RenderContext, actor, actor, phyInit);
            actor.AddComponent(phyComp);
            return phyComp;
        }
        public override object GetShowPropertyObject()
        {
            if (PhyActor.Shapes.Count == 1)
            {
                Editor_PhysicsCollision collision = null;
                using (var it = PhyActor.Shapes.GetEnumerator())
                {
                    it.MoveNext();
                    var first = it.Current;
                    switch(first.ShapeType)
                    {
                        case EPhysShapeType.PST_Box:
                            {
                                collision = new Editor_PhysicsBoxCollision(PhyActor);
                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                collision = new Editor_PhysicsSphereCollision(PhyActor);
                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                collision = new Editor_PhysicsCapsuleCollision(PhyActor);
                            }
                            break;
                    }
                }
                return collision;
            }
            else
            {
                return new Editor_MutiShapePhysicsCollision(PhyActor);
            }
        }

        [Rtti.MetaClassAttribute]
        public class GPhysicsComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaClassAttribute]
            public class ShapeDesc : IO.Serializer.Serializer
            {
                public delegate void OnChangePlacement(Vector3 location, Quaternion rotation);
                public OnChangePlacement onChangePlacement;

                public delegate void OnChangeShapeSize();
                public OnChangeShapeSize onChangeShapeSize;

                Byte mAreaType = 0;
                [Rtti.MetaData]
                [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
                public Byte AreaType
                {
                    get
                    {
                        return mAreaType;
                    }
                    set
                    {
                        if (value > 0 && value < 16)
                            mAreaType = value;
                    }
                }

                [Rtti.MetaData]
                public EPhysShapeType ShapeType
                {
                    get;
                    set;
                } = EPhysShapeType.PST_Box;
                [Rtti.MetaData]
                [Editor.Editor_PackData()]
                public RName PhyMtlName
                {
                    get;
                    set;
                } = new RName("Physics/PhyMtl/default.phymtl", 0);

                private Vector3 mLocation = Vector3.Zero;
                [Rtti.MetaData]
                public Vector3 Location
                {
                    get
                    {
                        return mLocation;
                    }
                    set
                    {
                        mLocation = value;
                        if (onChangePlacement != null)
                            onChangePlacement(mLocation, mRotation);
                    }
                }

                [Rtti.MetaData]
                public Vector3 Scale
                {
                    get;
                    set;
                } = Vector3.UnitXYZ;

                private Quaternion mRotation = Quaternion.Identity;
                [Rtti.MetaData]
                public Quaternion Rotation
                {
                    get
                    {
                        return mRotation;
                    }
                    set
                    {
                        mRotation = value;
                        if (onChangePlacement != null)
                            onChangePlacement(mLocation, mRotation);
                    }
                }

                //TriMesh Or Convex
                [Rtti.MetaData]
                [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
                [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyGeom)]
                [Editor.Editor_PackData()]
                public RName MeshName
                {
                    get;
                    set;
                } = new RName("", 0);
                //Sphere
                private float mSphereRadius = 1.0f;
                [Rtti.MetaData]
                public float SphereRadius
                {
                    get
                    {
                        return mSphereRadius;
                    }
                    set
                    {
                        mSphereRadius = value;
                        if (onChangeShapeSize != null)
                            onChangeShapeSize();
                    }
                }
                //Sphere
                private Vector3 mBoxExtend = Vector3.UnitXYZ;
                [Rtti.MetaData]
                public Vector3 BoxExtend
                {
                    get
                    {
                        return mBoxExtend;
                    }
                    set
                    {
                        mBoxExtend = value;
                        if (onChangeShapeSize != null)
                            onChangeShapeSize();
                    }
                }
            }

            [Rtti.MetaData]
            public bool IsEnableNavgation
            {
                get;
                set;
            } = false;

            public List<WeakReference> components = new List<WeakReference>();
            [Rtti.MetaData]
            
            public List<ShapeDesc> ShapeList
            {
                get;
                set;
            } = new List<ShapeDesc>();

            private EPhyActorType mActorType = EPhyActorType.PAT_Static;
            [Rtti.MetaData]
            public EPhyActorType ActorType
            {
                get
                {
                    return mActorType;
                }
                set
                {
                    mActorType = value;
                    List<GPhysicsComponent> coms = new List<GPhysicsComponent>();
                    foreach (var i in components)
                    {
                        coms.Add(i.Target as GPhysicsComponent);
                    }
                    components.Clear();

                    foreach (var i in coms)
                    {
                        var host = i.Host;
                        if (host != null)
                        {
                            host.RemoveComponent(i.SpecialName);
                            GPhysicsComponent com = new GPhysicsComponent();
                            var test = com.SetInitializer(EngineNS.CEngine.Instance.RenderContext, host, host, this);
                            host.AddComponent(com);
                        }
                    }
                }
            }
        }

        
        public CPhyActor PhyActor
        {
            get;
            set;
        }

        [DisplayName("是否作为寻路信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsEnableNavgation
        {
            get
            {
                var initializer = Initializer as GPhysicsComponentInitializer;
                if (initializer != null)
                {
                    return initializer.IsEnableNavgation;
                }
                return false;
            }
            set
            {
                var initializer = Initializer as GPhysicsComponentInitializer;
                if (initializer != null)
                {
                    initializer.IsEnableNavgation = value;
                }
            }
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GPhysicsComponentInitializer;
            if (init == null)
                return false;
            var phyTrans = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(host.Placement.Location, host.Placement.Rotation);
            var phyActor = CEngine.Instance.PhyContext.CreateActor(init.ActorType, ref phyTrans);

            //如果为空 加一个默认值..
            if (init.ShapeList.Count == 0)
                init.ShapeList.Add(new GPhysicsComponentInitializer.ShapeDesc());

            this.PhyActor = phyActor;
            phyActor.HostActor = Host;
            phyActor.PCInitializer = init;
            host.Placement.PropertyChanged += Placement_PropertyChanged; ;
            foreach (var i in init.ShapeList)
            {
                var mtl = CEngine.Instance.PhyContext.LoadMaterial(i.PhyMtlName);
                i.Scale = host.Placement.Scale;
                var shapetype = await CreatePhyShapeType(i.ShapeType, mtl, i);
            }

            init.components.Add(new WeakReference(this));

            return true;
        }
        public override async Task<GComponent> CloneComponent(CRenderContext rc, GamePlay.Actor.GActor host, IComponentContainer hostContainer)
        {
            var type = this.GetType();
            GComponent result = null;
            var atttibutes = type.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), true);
            if (atttibutes.Length > 0)
            {
                //result = await GMacrossComponent.CreateComponent(type);
                //result = EngineNS.CEngine.Instance.MacrossDataManager.NewObject(type) as EngineNS.GamePlay.Component.GComponent;
                throw new InvalidOperationException("不能直接操作Macross类型");
            }
            else
            {
                result = Activator.CreateInstance(type) as GComponent;
            }
            if (Initializer != null)
            {
                var init = Initializer.CloneObject() as GComponentInitializer;
                await result.SetInitializer(rc, host, hostContainer, init);
            }
            else
            {
                var init = new GComponentInitializer() { OnlyForGame = true };
                await result.SetInitializer(rc, host, hostContainer, init);
            }
            return result;
        }

        private void Placement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Host == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Editor", $"GPhiscsComponent.PropertyChanged: Host is null");
                return;
            }
            if (e.PropertyName == "Scale" || e.PropertyName == "Matrix")
            {
                foreach (var shapeDesc in this.PhyActor.PCInitializer.ShapeList)
                {
                    if (shapeDesc.Scale == Host.Placement.Scale)
                        return;
                }
                CPhyShape[] shapes = new CPhyShape[PhyActor.Shapes.Count];
                PhyActor.Shapes.CopyTo(shapes);
                foreach (var phyShape in shapes)
                {
                    phyShape.RemoveFraomActor();
                }
                foreach (var shapeDesc in this.PhyActor.PCInitializer.ShapeList)
                {
                    shapeDesc.Scale = Host.Placement.Scale;
                    var shape = CreatePhyShapeType(shapeDesc.ShapeType, CEngine.Instance.PhyContext.LoadMaterial(shapeDesc.PhyMtlName), shapeDesc);
                }
                OnPropertyChanged("PhyActor");
            }
        }

        public void onChangeShapeType(EngineNS.Bricks.PhysicsCore.EPhysShapeType shapetype, CPhyShape cps)
        {
            cps.RemoveFraomActor();
            var shape = CreatePhyShapeType(shapetype, cps.Material, cps.ShapeDesc);

            OnPropertyChanged("PhyActor");
        }

        public async System.Threading.Tasks.Task<CPhyShape> CreatePhyShapeType(EPhysShapeType shapetype, CPhyMaterial mtl, GPhysicsComponentInitializer.ShapeDesc sd)
        {
            CPhyShape shape = null;
            switch (shapetype)
            {
                case EPhysShapeType.PST_Convex:
                    {

                        if (sd.MeshName == null || sd.MeshName.Name.Equals(""))
                        {
                            //TODO..
                        }
                        shape = await CEngine.Instance.PhyContext.CreateShapeConvex(mtl, sd.MeshName, sd.Scale, Quaternion.Identity);
                    }
                    break;
                case EPhysShapeType.PST_TriangleMesh:
                    {
                        shape = await CEngine.Instance.PhyContext.CreateShapeTriMesh(mtl, sd.MeshName, sd.Scale, Quaternion.Identity);
                    }
                    break;
                case EPhysShapeType.PST_Box:
                    {
                        shape = CEngine.Instance.PhyContext.CreateShapeBox(mtl, sd.BoxExtend.X * sd.Scale.X, sd.BoxExtend.Y * sd.Scale.Y, sd.BoxExtend.Z * sd.Scale.Z);
                    }
                    break;
                case EPhysShapeType.PST_Sphere:
                    {
                        shape = CEngine.Instance.PhyContext.CreateShapeSphere(mtl, sd.SphereRadius * sd.Scale.X);
                    }
                    break;
                default:
                    break;
            }

            if (shape != null)
            {
                var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(sd.Location, sd.Rotation);
                shape.AddToActor(this.PhyActor, ref relativePose);
                shape.ShapeDesc = sd;
                //是否成为动态阻挡
                shape.AreaType = sd.AreaType;
                sd.ShapeType = shapetype;

                shape.onChangeShapeType = this.onChangeShapeType;
                if (sd.onChangePlacement == null)
                    sd.onChangePlacement = shape.onChangePlacement;
                else
                    sd.onChangePlacement += shape.onChangePlacement;

                if (sd.onChangeShapeSize == null)
                    sd.onChangeShapeSize = shape.onChangeShapeSize;
                else
                    sd.onChangeShapeSize += shape.onChangeShapeSize;
            }
            return shape;
        }

        public override void OnAdded()
        {
            var sg = Host.Scene;
            if (sg != null && Host.Placement != null)
            {
                PhyActor?.AddToScene(sg.PhyScene);
                //var phyTrans = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(Host.Placement.Location, Host.Placement.Rotation);
                //PhyActor.SetPose2Physics(ref phyTrans);
                OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            }

            Host.mComponentFlags |= EngineNS.GamePlay.Actor.GActor.EComponentFlags.HasInstancing;
            base.OnAdded();
        }
        public override void OnAddedScene()
        {
            OnAdded();
        }

        public override void OnRemove()
        {
            PhyActor?.AddToScene(null);
            Host.mComponentFlags &= (~EngineNS.GamePlay.Actor.GActor.EComponentFlags.HasInstancing);
            base.OnRemove();
        }

        public override void OnRemoveScene()
        {
            OnRemove();
        }

        private bool IsPlacementSetting = false;
        public override void Tick(GPlacementComponent placement)
        {
            if (PhyActor == null)
                return;
            base.Tick(placement);
            if (PhyActor.PhyType == EPhyActorType.PAT_Dynamic)
            {
                IsPlacementSetting = true;

                //if (PhyActor.Shapes.Count == 1)
                //{
                //    System.Collections.Generic.HashSet<CPhyShape>.Enumerator enumerator = PhyActor.Shapes.GetEnumerator();
                //    if (enumerator.Current == null)
                //        enumerator.MoveNext();

                //    if (enumerator.Current != null && (enumerator.Current.ShapeType == EPhysShapeType.PST_Box || enumerator.Current.ShapeType == EPhysShapeType.PST_Sphere || enumerator.Current.ShapeType == EPhysShapeType.PST_Plane))
                //    {
                //        //placement.Location = PhyActor.Position - enumerator.Current.CenterOffset;
                //    }
                //}
                //else
                //{
                placement.Location = PhyActor.Position;
                //}
                placement.Rotation = PhyActor.Rotation;
                IsPlacementSetting = false;
            }
        }
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            if (IsPlacementSetting)
                return;

            var trans = PhyTransform.CreateTransformFromMatrix(ref drawMatrix);
            ////放到工具界面 TODO..
            //if (PhyActor.PhyType == EPhyActorType.PAT_Static && PhyActor.Shapes.Count == 1)
            //{
            //    System.Collections.Generic.HashSet<CPhyShape>.Enumerator enumerator = PhyActor.Shapes.GetEnumerator();
            //    if (enumerator.Current == null)
            //        enumerator.MoveNext();

            //    if (enumerator.Current != null && (enumerator.Current.ShapeType == EPhysShapeType.PST_Box || enumerator.Current.ShapeType == EPhysShapeType.PST_Sphere || enumerator.Current.ShapeType == EPhysShapeType.PST_Plane))
            //    {
            //        Vector3 center = Host.GetAABBCenter();
            //        Vector3 pos = Host.Placement.Location;
            //        enumerator.Current.CenterOffset = center - pos;
            //        //trans.P += enumerator.Current.CenterOffset;
            //        PhyActor.SetPose2Physics(ref trans);
            //        return;
            //    }
            //}

            PhyActor.SetPose2Physics(ref trans);
        }

        public override void OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CEngine.PhysicsDebug == false)
                return;

            foreach (var shape in PhyActor.Shapes)
            {
                shape.OnEditorCommitVisual(cmd, camera, PhyActor.HostActor, param);
            }
        }

        public async Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new GPhysicsComponentInitializer();
            init.SpecialName = "PhysicsData";
            await SetInitializer(rc, actor, actor, init);

            actor.AddComponent(this);
            return actor;
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public partial class CGfxMeshPrimitives
    {
        public void CookAndSavePhyiscsGeomAsConvex(CRenderContext rc, Bricks.PhysicsCore.CPhyContext ctx)
        {
            this.PreUse(true);
            Support.CBlobObject convex = new Support.CBlobObject();
            ctx.CookConvexMesh(rc, GeometryMesh, convex);

            var xnd = IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("Convex");
            attr.BeginWrite();
            attr.Write(convex);
            attr.EndWrite();

            //var file = EngineNS.CEngine.Instance.FileManager.RemoveExtension(this.Name.Address) + CEngineDesc.PhyConvexGeom;
            var file = this.Name.Address + CEngineDesc.PhyConvexGeom;
            IO.XndHolder.SaveXND(file, xnd);
        }
        public void CookAndSavePhyiscsGeomAsConvex(CRenderContext rc, Bricks.PhysicsCore.CPhyContext ctx,string savePath)
        {
            this.PreUse(true);
            Support.CBlobObject convex = new Support.CBlobObject();
            ctx.CookConvexMesh(rc, GeometryMesh, convex);

            var xnd = IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("Convex");
            attr.BeginWrite();
            attr.Write(convex);
            attr.EndWrite();

            IO.XndHolder.SaveXND(savePath, xnd);
        }
        public void CookAndSavePhyiscsGeomAsTriMesh(CRenderContext rc, Bricks.PhysicsCore.CPhyContext ctx)
        {
            this.PreUse(true);
            Support.CBlobObject tri = new Support.CBlobObject();
            Support.CBlobObject uvblob = new Support.CBlobObject();
            Support.CBlobObject faceblob = new Support.CBlobObject();
            Support.CBlobObject posblob = new Support.CBlobObject();
            ctx.CookTriMesh(rc, GeometryMesh, tri, uvblob, faceblob, posblob);

            var xnd = IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("TriMesh");
            attr.BeginWrite();
            attr.Write(tri);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PhyiscsGeomPos");
            attr.BeginWrite();
            attr.Write(posblob);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PhyiscsGeomUV");
            attr.BeginWrite();
            attr.Write(uvblob);
            attr.EndWrite();


            attr = xnd.Node.AddAttrib("PhyiscsGeomFace");
            attr.BeginWrite();
            if (GeometryMesh.GetIndexBuffer().Desc.Type == EIndexBufferType.IBT_Int16)
                attr.Write("IBT_Int16");
            else
                attr.Write("IBT_Int32");
            attr.Write(faceblob);
            attr.EndWrite();
            var file = this.Name.Address + CEngineDesc.PhyTriangleMeshGeom;
            IO.XndHolder.SaveXND(file, xnd);
        }
        public void CookAndSavePhyiscsGeomAsTriMesh(CRenderContext rc, Bricks.PhysicsCore.CPhyContext ctx, string savePath)
        {
            this.PreUse(true);
            Support.CBlobObject tri = new Support.CBlobObject();
            Support.CBlobObject uvblob = new Support.CBlobObject();
            Support.CBlobObject faceblob = new Support.CBlobObject();
            Support.CBlobObject posblob = new Support.CBlobObject();
            ctx.CookTriMesh(rc, GeometryMesh, tri, uvblob, faceblob, posblob);

            var xnd = IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("TriMesh");
            attr.BeginWrite();
            attr.Write(tri);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PhyiscsGeomPos");
            attr.BeginWrite();
            attr.Write(posblob);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PhyiscsGeomUV");
            attr.BeginWrite();
            attr.Write(uvblob);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PhyiscsGeomFace");
            attr.BeginWrite();
            if (GeometryMesh.GetIndexBuffer().Desc.Type == EIndexBufferType.IBT_Int16)
                attr.Write("IBT_Int16");
            else
                attr.Write("IBT_Int32");
            attr.Write(faceblob);
            attr.EndWrite();

            IO.XndHolder.SaveXND(savePath, xnd);
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool PhysicsDebug = false;
    }
}
