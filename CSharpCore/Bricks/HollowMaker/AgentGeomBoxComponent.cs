using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.Editor;

namespace EngineNS.Bricks.HollowMaker
{
    [Editor.Editor_PlantAbleActor("Volumn", "Agent GeomBox")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(AgentGeomBoxComponentInitializer), "组件", "Agent GeomBox Component")]
    public class AgentGeomBoxComponent : EngineNS.LooseOctree.BoxComponent, EngineNS.Editor.IPlantable
    {
        [Rtti.MetaClass]
        public class AgentGeomBoxComponentInitializer : BoxComponentInitializer
        {
            [Rtti.MetaData]
            public Vector3 StartPos
            {
                get;
                set;
            }

            [Rtti.MetaData]
            public float AgentGridSize
            {
                get;
                set;
            } = 2.0f;
        }
        public AgentGeomBoxComponent()
        {
        }
 
        BoundingBox mBox = new BoundingBox();
        public BoundingBox Box
        {
            get
            {
                //逻辑上host不能为空 为空的话解决问题 不能在这里加判断
                mHost.GetAABB(ref mBox);
                return mBox;
            }
        }

        public Vector3 StartPos
        {
            get
            {
                return Initializer.StartPos;
            }
            set
            {
                Initializer.StartPos = value;
                //Matrix.Translation(ref value, out DrawMatrix);
                UpdateMatrix();
            }
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AgentGridSize
        {
            get
            {
                return Initializer.AgentGridSize;
            }
            set
            {
                Initializer.AgentGridSize = value;
            }
        }

        public override void CommitVisual(CRenderContext rc, EngineNS.Graphics.CGfxCamera camera)
        {
            base.CommitVisual(rc, camera);
            if (mMeshComponent != null)
                mMeshComponent?.CommitVisual(rc, camera);
        }

        public void UpdateMatrix()
        {
            mHost.GetAABB(ref mBox);
            DrawMatrix.M41 = Initializer.StartPos.X + mBox.Minimum.X;
            DrawMatrix.M42 = Initializer.StartPos.Y + mBox.Minimum.Y;
            DrawMatrix.M43 = Initializer.StartPos.Z + mBox.Minimum.Z;
            mMeshComponent.OnUpdateDrawMatrix(ref DrawMatrix);
        }

        Matrix DrawMatrix = Matrix.Identity;
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            UpdateMatrix();
        }

        GamePlay.Component.GMeshComponent mMeshComponent;
        AgentGeomBoxComponentInitializer Initializer;
        public override async Task<bool> SetInitializer(CRenderContext rc, GActor host, GComponentInitializer v)
        {
            if (await base.SetInitializer(rc, host, v) == false)
                return false;

            mMeshComponent = new GamePlay.Component.GMeshComponent();
            var meshCompInit = new GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "VisualMesh";
            meshCompInit.MeshName = RName.GetRName("editor/icon/icon_3D/mesh/play_start.gms", RName.enRNameType.Game);
            await mMeshComponent.SetInitializer(rc, Host, meshCompInit);
            //var mat = host.Placement.WorldMatrix;
            //mMeshComponent.OnUpdateDrawMatrix(ref mat);

            Initializer = v as AgentGeomBoxComponentInitializer;
            if (Initializer != null)
            {
                AgentGridSize = Initializer.AgentGridSize;
                StartPos = Initializer.StartPos;
            }
            else
            {
                Initializer = new AgentGeomBoxComponentInitializer();
            }

            return true;
        }

        public async Task<GActor> CreateActor(PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new AgentGeomBoxComponentInitializer();
            init.SpecialName = "AgentStart";
            await SetInitializer(rc, actor, init);

            actor.AddComponent(this);
            return actor;
        }
    }
}
