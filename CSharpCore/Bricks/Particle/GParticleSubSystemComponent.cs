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

    //[Editor.Editor_MacrossClassAttribute(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Createable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    //[Editor.Editor_PlantAbleActor("Particle", "CGfxParticleSubStateComponent")]
    //[GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMeshComponentInitializer), "特效发射器类型组件", "Particle", "ParticleSubSystemComponent")]
    public partial class GParticleSubSystemComponent : GamePlay.Component.GMeshComponent
    {
        //
        public string EditorVisibleName
        {
            get;
            set;
        }

        public Particle.CGfxParticleModifier ParticleModifier
        {
            get;
            set;
        }

        //public override void Tick(GPlacementComponent placement)
        //{
        //    base.Tick(placement);
        //}

        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var init = v as GMeshComponentInitializer;
            if (init == null)
                return false;

            if (ParticleModifier != null)
            {
                if (ParticleModifier.ParticleSys.IsBillBoard && ParticleModifier.ParticleSys.BillBoardType != CGfxParticleSystem.BILLBOARDTYPE.BILLBOARD_DISABLE)
                {
                    init.MeshName = RName.GetRName("@createplane");
                }
                else
                {
                    init.MeshName = ParticleModifier.ParticleSys.UseMeshRName;
                }
            }
            
            
            //设置默认值
            if (init.MeshName == null)
            {
                init.MeshName = RName.GetRName("ParticleResource/models/sphere.gms");
            }
            
            if (false == await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            Host = host as GActor;
            if (ParticleModifier != null && ParticleModifier.ParticleSys.UseMaterialRName != null)
            {
                if (mSceneMesh != null && mSceneMesh.MtlMeshArray != null)
                {
                    //await SetMaterialInstance(rc, 0, ParticleModifier.ParticleSys.UseMaterialRName, null);
                    var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, ParticleModifier.ParticleSys.UseMaterialRName);

                    base.SetMaterialInstance(rc, 0, mtl, null);
                }
                    
            }

            if (ParticleModifier != null && this.SceneMesh != null)
            {
                this.SceneMesh.MdfQueue.AddModifier(ParticleModifier);
                this.AddDefaultColorBuff(rc, this.SceneMesh.MeshPrimitives);
            }
            return true;
        }

        partial void AddHelpMeshComponent(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EngineNS.Bricks.Particle.CGfxParticleSystem sys);

        public void AddHelpMeshComponentHelp(CRenderContext rc, EngineNS.GamePlay.Actor.GActor actor, EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            AddHelpMeshComponent(rc, actor, sys);
        }

        public async Task InitParticleSubSystemComponent(EngineNS.GamePlay.Actor.GActor actor, GParticleComponent particlecomponent, CGfxParticleSystem sys)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            ParticleModifier = new CGfxParticleModifier();
            ParticleModifier.SetParticleSystem(sys);
          
            GMeshComponentInitializer initializer = new GMeshComponentInitializer();
            await SetInitializer(rc, actor, particlecomponent, initializer);
            SpecialName = string.IsNullOrEmpty(sys.Name) ? Guid.NewGuid().ToString() : sys.Name;
            EditorVisibleName = sys.EditorVisibleName;
            particlecomponent.AddComponent(this);
            
            Host = actor;

            var mPlacementData = Placement.mPlacementData;
            Placement = new GParticlePlacementComponent();
            await Placement.SetInitializer(rc, actor, this, mPlacementData as GComponentInitializer);
            Placement.SetMatrix(ref sys.Matrix);

            ((GParticlePlacementComponent)Placement).IsIgnore = !sys.IsBind;

            sys.HostActorMesh = this;
        }

        public void AddDefaultColorBuff(CRenderContext rc, CGfxMeshPrimitives result)
        {
            if (result == null)
                return;

            var mesh = result.GeometryMesh;
            if (mesh == null)
                return;

            CVertexBuffer colorbuff = mesh.GetVertexBuffer(EVertexSteamType.VST_Color);
            if (colorbuff != null)
            {
                return;
            }

            CVertexBuffer posbuff = mesh.GetVertexBuffer(EVertexSteamType.VST_Position);

            if (posbuff == null)
            {
                return;
            }

            var blob = new Support.CBlobObject();
            posbuff.GetBufferData(rc, blob);
            int vertNum = 0;
            unsafe
            {
                vertNum = (int)blob.Size / sizeof(Vector3);
            }

            if (vertNum == 0)
                return;

            Support.NativeList<Byte4> Colors = new Support.NativeList<Byte4>();
            var color = new Byte4();
            color.X = 255;
            color.Y = 255;
            color.Z = 255;
            color.W = 255;
            for (int i = 0; i < vertNum; i++)
            {
                Colors.Add( color);
            }

            var dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.NumPrimitives = (UInt32)vertNum;
            result.SetAtom(0, 0, ref dpDesc);

            UInt32 resourceSize = 0;
            unsafe
            {
                var vbDesc = new CVertexBufferDesc();
                vbDesc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                
                {
                    vbDesc.InitData = Colors.UnsafeAddressAt(0);
                    vbDesc.Stride = (UInt32)sizeof(Byte4);
                    vbDesc.ByteWidth = (UInt32)(sizeof(Byte4) * Colors.Count);
                    var vb = rc.CreateVertexBuffer(vbDesc);
                    mesh.BindVertexBuffer(EVertexSteamType.VST_Color, vb);
                    resourceSize += vbDesc.ByteWidth;
                }

                result.ResourceState.ResourceSize = (UInt32)(resourceSize);
                mesh.Dirty = true;
            }
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            result.ResourceState.KeepValid = true;
        }
    }

    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class GParticlePlacementComponent : GPlacementComponent
    {
        [System.ComponentModel.Browsable(false)]
        public bool IsIgnore = false;

        
        public override Matrix WorldMatrix
        {
            get
            {
                if (IsIgnore)
                    return Matrix.Identity;

                return base.WorldMatrix;
            }
        }
        
    }
}

