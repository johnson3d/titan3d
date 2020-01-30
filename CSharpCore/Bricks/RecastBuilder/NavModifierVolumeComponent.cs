using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;


namespace EngineNS.Bricks.RecastRuntime
{
    [Editor.Editor_PlantAbleActor("Navigation", "Nav Modifier Volume")]
    public class NavModifierVolumeComponent : NavMeshBoundVolumeComponent
    {
        public NavModifierVolumeComponent()
        {
            RCAreaType = NavMeshBoundVolumeComponent.AreaType.NoWalk;//区域不可行走
        }

        //public override async Task<bool> SetInitializer(CRenderContext rc, GActor host, GComponentInitializer v)
        //{
        //    return await base.SetInitializer(rc, host, v);

        //}

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
            meshCompInit.MeshName = EngineNS.RName.GetRName("meshes/go_off.gms", EngineNS.RName.enRNameType.Editor);
            await meshComp.SetInitializer(rc, actor, actor, meshCompInit);
            meshComp.HideInGame = true;
            actor.AddComponent(meshComp);

            var init = new BoxComponentInitializer();
            init.SpecialName = "NavModifierVolume";
            await SetInitializer(rc, actor, actor, init);

            mLineMeshComponent.Host = actor;
            var mat = actor.Placement.WorldMatrix;
            mLineMeshComponent.OnUpdateDrawMatrix(ref mat);

            actor.AddComponent(this);

            return actor;
        }
    }
}