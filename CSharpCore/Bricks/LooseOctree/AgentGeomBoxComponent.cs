using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.Editor;

namespace EngineNS.Bricks.HollowMaker
{
    [Editor.Editor_PlantAbleActor("Volumn", "Agent GeomBox")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(AgentGeomBoxComponentInitializer), "AgentGeomBoxComponent", "Agent GeomBox", "AgentGeomBoxComponent")]
    public partial class AgentGeomBoxComponent : EngineNS.LooseOctree.BoxComponent, EngineNS.Editor.IPlantable
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
                Host.GetAABB(ref mBox);
                return mBox;
            }
        }

        public Vector3 StartPos
        {
            get
            {
                return AgentGeomBoxInitializer.StartPos;
            }
            set
            {
                AgentGeomBoxInitializer.StartPos = value;
                //Matrix.Translation(ref value, out DrawMatrix);
                UpdateMatrix();
            }
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float AgentGridSize
        {
            get
            {
                return AgentGeomBoxInitializer.AgentGridSize;
            }
            set
            {
                AgentGeomBoxInitializer.AgentGridSize = value;
            }
        }

        public override void CommitVisual(CCommandList cmd, EngineNS.Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            base.CommitVisual(cmd, camera, param);
            if (mMeshComponent != null)
                mMeshComponent?.CommitVisual(cmd, camera, param);
        }

        public void UpdateMatrix()
        {
            //mHost.Placement.Location;
            DrawMatrix.M41 = AgentGeomBoxInitializer.StartPos.X;
            DrawMatrix.M42 = AgentGeomBoxInitializer.StartPos.Y;
            DrawMatrix.M43 = AgentGeomBoxInitializer.StartPos.Z;
            Matrix mat = DrawMatrix * Matrix.Transformation(Vector3.UnitXYZ, Host.Placement.Rotation, Host.Placement.Location);
            //DrawMatrix.M41 = Initializer.StartPos.X + mHost.Placement.Location.X;
            //DrawMatrix.M42 = Initializer.StartPos.Y + mHost.Placement.Location.Y;
            //DrawMatrix.M43 = Initializer.StartPos.Z + mHost.Placement.Location.Z;
            mMeshComponent.OnUpdateDrawMatrix(ref mat);
        }

        Matrix DrawMatrix = Matrix.Identity;
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            UpdateMatrix();
        }

        GamePlay.Component.GMeshComponent mMeshComponent;
        AgentGeomBoxComponentInitializer AgentGeomBoxInitializer;
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, GamePlay.Component.IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (await base.SetInitializer(rc, host, hostContainer, v) == false)
                return false;

            mMeshComponent = new GamePlay.Component.GMeshComponent();
            var meshCompInit = new GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "VisualMesh";
            meshCompInit.MeshName = CEngineDesc.PlayerStartMesh;
            await mMeshComponent.SetInitializer(rc, Host, Host, meshCompInit);
            //var mat = host.Placement.WorldMatrix;
            //mMeshComponent.OnUpdateDrawMatrix(ref mat);

            AgentGeomBoxInitializer = v as AgentGeomBoxComponentInitializer;
            if (AgentGeomBoxInitializer != null)
            {
                AgentGridSize = AgentGeomBoxInitializer.AgentGridSize;
                StartPos = AgentGeomBoxInitializer.StartPos;
            }
            else
            {
                AgentGeomBoxInitializer = new AgentGeomBoxComponentInitializer();
            }

            return true;
        }

        public override async Task<GActor> CreateActor(PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actorInit = new GActor.GActorInitializer();
            actorInit.InPVS = false;
            var actor = new GActor();
            actor.SetInitializer(actorInit);
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new AgentGeomBoxComponentInitializer();
            init.SpecialName = "AgentStart";
            await SetInitializer(rc, actor, actor, init);

            actor.AddComponent(this);
            return actor;
        }
    }
}
