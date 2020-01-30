using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.RecastRuntime
{
    [Editor.Editor_PlantAbleActor("Navigation", "Nav Mesh Bound Volume")]
    public partial class NavMeshBoundVolumeComponent : EngineNS.LooseOctree.BoxComponent, Editor.IPlantable
    {
        public enum AreaType : int
        {
            NoWalk = 0,
            Walk = 64,
        }

        public AreaType RCAreaType = AreaType.Walk;
        public BoundingBox GetBox()
        {
            BoundingBox aabb = new BoundingBox();
            if (Host == null)
                return aabb;
            Host.GetAABB(ref aabb);

            return aabb;
        }

        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, GamePlay.Component.IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (!await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            Host.IsNavgation = true;

            return true;
        }

        partial void UpdateMatrixEditor();
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            UpdateMatrix();
            UpdateMatrixEditor();
        }

        public void UpdateMatrix()
        {
            Matrix wmat = Host.Placement.WorldMatrix;
            //Matrix mat = Matrix.Transformation(wmat.WorldMatrix, mHost.Placement.Rotation, -mHost.Placement.Location);
   
            mLineMeshComponent.OnUpdateDrawMatrix(ref wmat);
        }

        partial void TickEditor();
        public override void Tick(GamePlay.Component.GPlacementComponent placement)
        {
            base.Tick(placement);
            TickEditor();
        }

        public override void CommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (EngineNS.CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor || ShowInGame)
            {
                if (CEngine.IsRenderBoundBox)
                {
                    mLineMeshComponent?.CommitVisual(cmd, camera, param);
                }
               
            }
                
        }

        public override async Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;
            placement.Rotation = EngineNS.Quaternion.GetQuaternion(Vector3.UnitZ, -Vector3.UnitY);

            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var meshCompInit = new EngineNS.GamePlay.Component.GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "EditorShow";
            meshCompInit.MeshName = EngineNS.RName.GetRName("meshes/go_on.gms", EngineNS.RName.enRNameType.Editor);
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            meshComp.HideInGame = true;
            meshComp.Placement.InheritScale = false;
            meshComp.Placement.InheritRotation = false;
            actor.AddComponent(meshComp);

            var init = new BoxComponentInitializer();
            init.SpecialName = "NavMeshBoundVolume";
            await SetInitializer(rc, actor, actor, init);

            mLineMeshComponent.Host = actor;
            var mat = actor.Placement.WorldMatrix;
            mLineMeshComponent.OnUpdateDrawMatrix(ref mat);

            actor.AddComponent(this);

            return actor;
        }
    }

}
