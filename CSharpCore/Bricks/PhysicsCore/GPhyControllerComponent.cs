using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Bricks.PhysicsCore.CollisionComponent;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.PhysicsCore
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPhyControllerComponentInitializer), "物理控制器组件", "Collision", "ControllerComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/character_64x.txpic", RName.enRNameType.Editor)]
    public class GPhyControllerComponent : GamePlay.Component.GComponent, IPlaceable
    {
        [Rtti.MetaClass]
        public class GPhyControllerComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public bool IsBox
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public Vector3 BoxExtent
            {
                get;
                set;
            } = Vector3.UnitXYZ;
            [Rtti.MetaData]
            public float CapsuleRadius
            {
                get;
                set;
            } = 1.0f;
            [Rtti.MetaData]
            public float CapsuleHeight
            {
                get;
                set;
            } = 0.5f;
            [Rtti.MetaData]
            public RName MaterialName
            {
                get;
                set;
            } = new RName("Physics\\PhyMtl\\default.phymtl", 0);
            [Rtti.MetaData]
            public CollisionLayers SelfCollisionLayer { get; set; } = CollisionLayers.Actor;
            [Rtti.MetaData]
            public CollisionLayers BlockCollisionLayers { get; set; } = CollisionLayers.Static | CollisionLayers.Dynamic | CollisionLayers.Actor;
        }
        [Browsable(false)]
        public CPhyController Controller
        {
            get;
            protected set;
        }
        [Browsable(false)]
        public GPhyControllerComponentInitializer PhyCtrlInitializer
        {
            get
            {
                var result = this.Initializer as GPhyControllerComponentInitializer;
                if (result == null)
                {
                    result = new GPhyControllerComponentInitializer();
                    this.Initializer = result;
                }
                return result;
            }
        }
        #region properties
        public bool IsBox
        {
            get => PhyCtrlInitializer.IsBox;
            set => PhyCtrlInitializer.IsBox = value;
        }
        public Vector3 BoxExtent
        {
            get=> PhyCtrlInitializer.BoxExtent;
            set=> PhyCtrlInitializer.BoxExtent =value;
        } 
        public float CapsuleRadius
        {
            get=> PhyCtrlInitializer.CapsuleRadius;
            set=> PhyCtrlInitializer.CapsuleRadius =value;
        } 
        public float CapsuleHeight
        {
            get => PhyCtrlInitializer.CapsuleHeight;
            set => PhyCtrlInitializer.CapsuleHeight =value;
        }
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyMaterial)]
        public RName MaterialName
        {
            get => PhyCtrlInitializer.MaterialName;
            set => PhyCtrlInitializer.MaterialName = value;
        }
        [Rtti.MetaData]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CollisionLayers SelfCollisionLayer
        { 
            get => PhyCtrlInitializer.SelfCollisionLayer;
            set
            {
                PhyCtrlInitializer.SelfCollisionLayer = value;
                RefreshQueryFilterData();
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Rtti.MetaData]
        [EngineNS.Editor.Editor_FlagsEnumSetter]
        public CollisionLayers BlockCollisionLayers 
        { 
            get => PhyCtrlInitializer.BlockCollisionLayers;
            set
            {
                PhyCtrlInitializer.BlockCollisionLayers = value;
                RefreshQueryFilterData();
            }
        } 
        #endregion
        void RefreshQueryFilterData()
        {
            if (Controller == null)
                return;
            PhyFilterData data = new PhyFilterData();
            data.word0 = (uint)SelfCollisionLayer;
            data.word1 = (uint)BlockCollisionLayers;
            Controller.SetQueryFilterData(data);
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);

            var init = v as GPhyControllerComponentInitializer;
            if (init.IsBox)
            {
                var phyScene = this.Host?.Scene?.PhyScene;
                if (phyScene != null)
                {
                    var desc = new CPhyBoxControllerDesc();
                    desc.Extent = init.BoxExtent;
                    var mtl = CEngine.Instance.PhyContext.LoadMaterial(init.MaterialName);
                    desc.SetMaterial(mtl);
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)init.SelfCollisionLayer;
                    data.word1 = (uint)init.BlockCollisionLayers;
                    desc.SetQueryFilterData(data);
                    Controller = phyScene.CreateBoxController(desc);
                    Controller.HostComponent = this;
                }
            }
            else
            {
                var phyScene = this.Host?.Scene?.PhyScene;
                if (phyScene != null)
                {
                    var desc = new CPhyCapsuleControllerDesc();
                    desc.CapsuleRadius = init.CapsuleRadius;
                    desc.CapsuleHeight = init.CapsuleHeight;
                    var mtl = CEngine.Instance.PhyContext.LoadMaterial(init.MaterialName);
                    desc.SetMaterial(mtl);
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)init.SelfCollisionLayer;
                    data.word1 = (uint)init.BlockCollisionLayers;
                    desc.SetQueryFilterData(data);
                    Controller = phyScene.CreateCapsuleController(desc);
                    Controller.HostComponent = this;
                }
            }

            return true;
        }
        public override void OnAddedScene()
        {
            base.OnAddedScene();

            var init = this.Initializer as GPhyControllerComponentInitializer;
            if (init.IsBox)
            {
                var phyScene = this.Host.Scene.PhyScene;
                if (phyScene != null)
                {
                    var desc = new CPhyBoxControllerDesc();
                    desc.Extent = init.BoxExtent;
                    var mtl = CEngine.Instance.PhyContext.LoadMaterial(init.MaterialName);
                    desc.SetMaterial(mtl);
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)init.SelfCollisionLayer;
                    data.word1 = (uint)init.BlockCollisionLayers;
                    desc.SetQueryFilterData(data);
                    Controller = phyScene.CreateBoxController(desc);
                    Controller.HostComponent = this;
                }
            }
            else
            {
                var phyScene = this.Host.Scene.PhyScene;
                if (phyScene != null)
                {
                    var desc = new CPhyCapsuleControllerDesc();
                    desc.CapsuleRadius = init.CapsuleRadius;
                    desc.CapsuleHeight = init.CapsuleHeight;
                    var mtl = CEngine.Instance.PhyContext.LoadMaterial(init.MaterialName);
                    desc.SetMaterial(mtl);
                    PhyFilterData data = new PhyFilterData();
                    data.word0 = (uint)init.SelfCollisionLayer;
                    data.word1 = (uint)init.BlockCollisionLayers;
                    desc.SetQueryFilterData(data);
                    Controller = phyScene.CreateCapsuleController(desc);
                    Controller.HostComponent = this;
                }
            }
            Controller.FootPosition = Host.Placement.Location;
        }
        public override void OnRemoveScene()
        {
            if (Controller != null)
            {
                Controller.Cleanup();
                Controller = null;
            }
            base.OnRemoveScene();
        }
        PhyControllerCollisionFlag mCollisionFlags = PhyControllerCollisionFlag.eCOLLISION_None;
        [Browsable(false)]
        public PhyControllerCollisionFlag CollisionFlags
        {
            get => mCollisionFlags;
        }
        [Browsable(false)]
        public GPlacementComponent Placement { get => null; set { } }
        public void OnPlacementChanged(GPlacementComponent placement)
        {
            //throw new NotImplementedException();
            if (Controller == null)
                return;
            if (placement.Location != Controller.FootPosition)
            {
                Controller.FootPosition = placement.Location;
            }

        }

        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            //throw new NotImplementedException();
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void TryMove(GPlacementComponent placement, ref Vector3 position, float minDist, float elapsedTime)
        {
            var delta = (position - placement.Location);
            OnTryMove(placement, ref delta, 0.0001f, CEngine.Instance.EngineElapseTimeSecond);
        }

        public override bool OnTryMove(GPlacementComponent placement, ref Vector3 dir, float minDist, float elapsedTime)
        {
            if (Controller == null)
                return false;
            if (!dir.IsValid())
                return false;
            PhyFilterData data = new PhyFilterData();
            data.word0 = (uint)SelfCollisionLayer;
            data.word1 = (uint)BlockCollisionLayers;
            mCollisionFlags = Controller.Move(ref dir, minDist, elapsedTime, ref data, PhyQueryFlag.eSTATIC);
            //if ((collisionFlags & PhyControllerCollisionFlag.eCOLLISION_SIDES) != 0)
            //{
            //    int xx = 0;
            //}
            //if ((collisionFlags & PhyControllerCollisionFlag.eCOLLISION_UP) != 0)
            //{
            //    int xx = 0;
            //}
            //if ((collisionFlags & PhyControllerCollisionFlag.eCOLLISION_DOWN) != 0)
            //{
            //    int xx = 0;
            //}
            if (placement.Location != Controller.FootPosition)
            {
                placement.Location = Controller.FootPosition;
            }
            return true;
        }


    }
}
