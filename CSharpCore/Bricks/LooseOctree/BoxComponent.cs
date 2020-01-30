using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics;

namespace EngineNS.LooseOctree
{
    // 包围盒几何中心处于此范围内的对象会放入八叉树内，包围盒接触但几何中心不在范围内的不做处理

    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(BoxComponentInitializer), "包围盒几何中心处于此范围内的对象会放入八叉树内，包围盒接触但几何中心不在范围内的不做处理", "LooseOctree", "BoxComponent")]
    public class BoxComponent : GamePlay.Component.GVisualComponent, GamePlay.Component.IComponentHostSelectOperation, EngineNS.Editor.IPlantable
    {
        [Rtti.MetaClass]
        public class BoxComponentInitializer : GamePlay.Component.GComponent.GComponentInitializer
        {

            [Rtti.MetaData]
            public bool ShowInGame
            {
                get;
                set;
            } = false;
        }

        [Browsable(false)]
        public override GamePlay.Actor.GActor Host
        {
            get
            {
                return base.Host;
            }
            set
            {
                base.Host = value;
                if(Host != null)
                     Host.CallPlacementChangeDefault();
            }
        }

        [DisplayName("游戏中显示")]
        public bool ShowInGame
        {
            get
            {
                return ((BoxComponentInitializer)Initializer).ShowInGame;
            }
            set
            {
                ((BoxComponentInitializer)Initializer).ShowInGame = value;
            }
        }

        protected EngineNS.Bricks.GraphDrawer.GraphLines mGraphLine;
        protected GamePlay.Component.GMeshComponent mLineMeshComponent;
        public GamePlay.Component.GMeshComponent LineMeshComponent
        {
            get=>mLineMeshComponent;
        }

        public Vector3 Center
        {
            get;
            set;
        } = Vector3.Zero;

        public Vector3 Size
        {
            get;
            set;
        } = Vector3.UnitXYZ;


        protected async System.Threading.Tasks.Task InitDrawMesh()
        {
            var rc = CEngine.Instance.RenderContext;

            //var mesh = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(rc, EngineNS.Graphics.Mesh.CGfxMeshCooker.CookBoxName, 1);
            //var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("editor/volume/mi_volume_octree.instmtl", RName.enRNameType.Game));
            //Graphics.Mesh.CGfxMeshCooker.MakeBox(rc, mesh, -0.5f, -0.5f, -0.5f, 1, 1, 1);
            //var m = CEngine.Instance.MeshManager.CreateMesh(rc, mesh);
            //await m.SetMaterial(rc, 0, mtlInst, null);// new CGfxShadingEnv[] { CEngine.Instance.ShadingEnvManager.DefaultShadingEnv });
            //mesh.ResourceState.KeepValid = true;

            //mMeshComponent = new GamePlay.Component.GMeshComponent();
            //mMeshComponent.SetSceneMesh(rc, m);
            ////await mMeshComponent.SetMaterial(rc, 0, mtlInst, null);
            ///
            var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, CEngineDesc.OctreeMaterialName);
            var gen = new Bricks.GraphDrawer.McBoxGen()
            {
                Interval = 0f,
                Segement = 5f,
            };
            gen.SetBox(Center, Size.X, Size.Y, Size.Z);
            mGraphLine = new Bricks.GraphDrawer.GraphLines();
            mGraphLine.LinesGen = gen;
            mGraphLine.UseGeometry = true;
            await mGraphLine.Init(mtlInst, 0);
            mLineMeshComponent = mGraphLine.GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
        }

        public override void CommitVisual(CCommandList cmd, CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (EngineNS.CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor || ShowInGame)
                mLineMeshComponent?.CommitVisual(cmd, camera, param);
        }

        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            mLineMeshComponent.OnUpdateDrawMatrix(ref drawMatrix);
        }

        public virtual async Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new BoxComponentInitializer();
            init.SpecialName = "BoxVolumeData";
            await SetInitializer(rc, actor, actor, init);

            mLineMeshComponent.Host = actor;
            var mat = actor.Placement.WorldMatrix;
            mLineMeshComponent.OnUpdateDrawMatrix(ref mat);
            //var aabb = new BoundingBox(-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f);
            //BoundingBox.Merge(ref actor.LocalBoundingBox, ref aabb, out actor.LocalBoundingBox);

            actor.AddComponent(this);
            return actor;
        }

        public override void OnAdded()
        {
            var aabb = new BoundingBox(-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f);
            BoundingBox.Merge(ref Host.LocalBoundingBox, ref aabb, out Host.LocalBoundingBox);
            OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            base.OnAdded();
        }

        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, GamePlay.Component.IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (!await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            if (mLineMeshComponent == null)
            {
                await InitDrawMesh();
                mLineMeshComponent.Entity = host;

                var mat = Host.Placement.WorldMatrix;
                mLineMeshComponent.OnUpdateDrawMatrix(ref mat);
            }

            return true;
        }

        public void OnHostSelected(bool isSelect)
        {
            var rc = CEngine.Instance.RenderContext;
            if (isSelect)
            {
                mGraphLine.UpdateGeomMesh(rc, 0.01f);
            }
            else
            {
                mGraphLine.UpdateGeomMesh(rc, 0);
            }
        }
    }
}
