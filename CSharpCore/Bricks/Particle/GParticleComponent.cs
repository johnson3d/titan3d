#define MacrossDebug
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Bricks.Particle
{

    [Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [Editor.Editor_PlantAbleActor("Particle", "GParticleComponent")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GParticleComponentInitializer), "粒子组件", "Particle", "ParticleComponent")]
    public partial class GParticleComponent : GamePlay.Component.GMeshComponent, EngineNS.Editor.IPlantable
    {
        [Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
        [Rtti.MetaClassAttribute]
        public class GParticleComponentInitializer : GMeshComponentInitializer
        {
            [Rtti.MetaData]
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [Editor.Editor_RNameMacrossType(typeof(McParticleEffector))]
            public RName MacrossName
            {
                get;
                set;
            } = RName.GetRName("ParticleResource/particle1.macross");//初始值 应该为只读 所有特效的数据以此为基础编辑
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var init = v as GParticleComponentInitializer;
            if (init == null)
                return false;

            mParticleModifier = new Particle.CGfxParticleModifier();
            var sys = mParticleModifier.ParticleSys;
            sys.Effector = CEngine.Instance.MacrossDataManager.NewObjectGetter<McParticleEffector>(init.MacrossName);

            if (sys.Effector != null)
            {
                await sys.Effector.Get(false).InitSystem(sys, host as GamePlay.Actor.GActor, this, init);
            }
            
            //default..
            if (init.MeshName == null)
                init.MeshName = RName.GetRName("ParticleResource/models/sphere.gms");

            if (false == await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            sys.HostActor = Host;

            if (sys.UseMaterialRName != null)
            {
                await SetMaterialInstance(rc, 0, sys.UseMaterialRName, null);
            }

            if (this.SceneMesh != null && this.SceneMesh.MdfQueue != null)
            {
                this.SceneMesh.MdfQueue.AddModifier(mParticleModifier);
            }

            //Box
            {
                BoundingBox.Merge(ref Host.LocalBoundingBox, ref sys.AABB.Box, out Host.LocalBoundingBox);
                OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            }
            return true;
        }

        public async Task<GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new GParticleComponentInitializer();
            init.SpecialName = "ParticleData";
            await SetInitializer(rc, actor, actor, init);

            actor.AddComponent(this);

            return actor;
        }

        public async Task<GActor> CreateActor(Editor.PlantableItemCreateActorParam param, RName MacrossName)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new GParticleComponentInitializer();
            init.SpecialName = "ParticleData";
            init.MacrossName = MacrossName;
            await SetInitializer(rc, actor, actor, init);

            actor.AddComponent(this);

            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "EditorShow";
            meshCompInit.MeshName = EngineNS.RName.GetRName("meshes/icon_particle.gms", EngineNS.RName.enRNameType.Editor);
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            meshComp.HideInGame = true;
            actor.AddComponent(meshComp);
            
            return actor;
        }
        
        public void ResetMacross(RName MacrossName)
        {
            ClearComponents();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var init = this.Initializer as GParticleComponentInitializer;
            init.MacrossName = MacrossName;
            var test = SetInitializer(rc, Host, HostContainer, init);
        }

        Particle.CGfxParticleModifier mParticleModifier;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Particle.CGfxParticleModifier ParticleModifier
        {
            get { return mParticleModifier; }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McParticleEffector))]
        public RName MacrossName
        {
            get
            {
                var init = this.Initializer as GParticleComponentInitializer;
                if (init == null)
                    return null;
                return init.MacrossName;
            }
            set
            {
                var init = this.Initializer as GParticleComponentInitializer;
                if (init == null)
                    return;
                init.MacrossName = value;
                //if (mParticleModifier != null)
                //{
                //    mParticleModifier.ParticleSys.Effector = CEngine.Instance.MacrossDataManager.NewObjectGetter<McParticleEffector>(value);
                //}

                ResetMacross(value);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void ResetTime()
        {
            if (ParticleModifier != null)
            {
                ParticleModifier.ParticleSys.ResetTime();
            }
            //TODO..
            //ResetMacross(MacrossName);
        }
        partial void TickEditor();
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            TickEditor();
        }

        public override void Save2Xnd(IO.XndNode node)
        {
            if (Initializer != null)
            {
                var attr = node.AddAttrib("Initializer");
                attr.BeginWrite();
                attr.WriteMetaObject(Initializer);
                attr.EndWrite();
            }

        }

        #region PropertyChanged
        public delegate void ParticleSystemPropertyChangeDelegate(CGfxParticleSystem sys);
        public event ParticleSystemPropertyChangeDelegate ParticleSystemPropertyChangedEvent;
        public void ParticleSystemPropertyChanged(CGfxParticleSystem sys)
        {
            ParticleSystemPropertyChangedEvent?.Invoke(sys);
        }

        public delegate void ParticleShapePropertyChangeDelegate(EmitShape.CGfxParticleEmitterShape shape);
        public event ParticleShapePropertyChangeDelegate ParticleShapePropertyChangedEvent;
        public void ParticleShapePropertyChanged(EmitShape.CGfxParticleEmitterShape shape)
        {
            ParticleShapePropertyChangedEvent?.Invoke(shape);
        }
        #endregion

    }
}
