using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Editor;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Component
{
    [Editor.Editor_PlantAbleActor("GamePlay", "PlayerStart")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPlayerStartComponentInitializer), "初始位置组件", "Player Start", "PlayerStartComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/playerstart_64x.txpic", RName.enRNameType.Editor)]
    public class GPlayerStartComponent : GamePlay.Component.GVisualComponent, EngineNS.Editor.IPlantable, GamePlay.Component.IComponentHostSelectOperation, IPlaceable
    {
        [Rtti.MetaClass]
        public class GPlayerStartComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public bool ShowInGame
            {
                get;
                set;
            } = false;
        }
        public GPlayerStartComponent()
        {
            OnlyForGame = false;
        }

        [DisplayName("游戏中显示")]
        public bool ShowInGame
        {
            get
            {
                return ((GPlayerStartComponentInitializer)Initializer).ShowInGame;
            }
            set
            {
                ((GPlayerStartComponentInitializer)Initializer).ShowInGame = value;
            }
        }

        GamePlay.Component.GMeshComponent mMeshComponent;
        
        public GamePlay.Component.GMeshComponent MeshComponent
        {
            get => mMeshComponent;
        }
        public GPlacementComponent Placement { get => ((IPlaceable)MeshComponent).Placement; set => ((IPlaceable)MeshComponent).Placement = value; }

        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (await base.SetInitializer(rc, host, hostContainer, v) == false)
                return false;

            mMeshComponent = new GMeshComponent();
            var meshCompInit = new GMeshComponent.GMeshComponentInitializer();
            meshCompInit.SpecialName = "VisualMesh";
            meshCompInit.MeshName = RName.GetRName("editor/icon/icon_3D/mesh/play_start.gms", RName.enRNameType.Game);
            await mMeshComponent.SetInitializer(rc, Host, hostContainer, meshCompInit);
            var mat = host.Placement.WorldMatrix;
            mMeshComponent.OnUpdateDrawMatrix(ref mat);

            return true;
        }

        public async Task<GActor> CreateActor(PlantableItemCreateActorParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actorInit = new GActor.GActorInitializer();
            actorInit.InPVS = false;
            var actor = new GActor();
            actor.SetInitializer(actorInit);
            actor.ActorId = Guid.NewGuid();
            var placement = new GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new GPlayerStartComponentInitializer();
            init.SpecialName = "PlayerStartData";
            await SetInitializer(rc, actor, actor, init);

            actor.AddComponent(this);
            return actor;
        }

        public override void OnAdded()
        {
            var aabb = mMeshComponent.SceneMesh.MeshPrimitives.AABB;
            BoundingBox.Merge(ref Host.LocalBoundingBox, ref aabb, out Host.LocalBoundingBox);
            OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            base.OnAdded();
        }

        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            mMeshComponent.OnUpdateDrawMatrix(ref drawMatrix);
        }

        public void OnHostSelected(bool isSelect)
        {
            mMeshComponent?.OnHostSelected(isSelect);
        }


        public override void CommitVisual(CCommandList cmd, CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor && ShowInGame == false)
                return;

            base.CommitVisual(cmd, camera, param);
            mMeshComponent?.CommitVisual(cmd, camera, param);
        }

        public void OnPlacementChanged(GPlacementComponent placement)
        {
            ((IPlaceable)MeshComponent).OnPlacementChanged(placement);
        }

        //public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        //{
        //    ((IPlaceable)MeshComponent).OnPlacementChangedUninfluencePhysics(placement);
        //}
    }
}
