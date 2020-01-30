using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Camera
{
    [Rtti.MetaClassAttribute]
    public class CameraSpringArmComponentInitializer : GComponent.GComponentInitializer
    {
        [Rtti.MetaData]
        public float ArmLength { get; set; } = 3.0f;
        [Rtti.MetaData]
        public float ProbeSize
        {
            get;
            set;
        } = 0.01f;
        [Rtti.MetaData]
        public float SpringDamping { get; set; } = 0.2f;
        [Rtti.MetaData]
        public bool DoCollisionTest { get; set; } = true;
        [Rtti.MetaData]
        public Bricks.PhysicsCore.CollisionComponent.CollisionLayers SelfLayer { get; set; } = Bricks.PhysicsCore.CollisionComponent.CollisionLayers.Camera;
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(CameraSpringArmComponentInitializer), "弹簧臂组件", "Camera", "CameraSpringArmComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/camerarig_crane_64x.txpic", RName.enRNameType.Editor)]
    public class CameraSpringArmComponent : GamePlay.Component.GComponentsContainer
    {
        GPlacementComponent PlacementComponent
        {
            get;
            set;
        } = new GPlacementComponent();
        public float ArmLength
        {
            get { return CameraSpringArmComponentInitializer.ArmLength; }
            set { CameraSpringArmComponentInitializer.ArmLength = value; }
        }
        public float ProbeSize
        {
            get { return CameraSpringArmComponentInitializer.ProbeSize; }
            set { CameraSpringArmComponentInitializer.ProbeSize = value; }
        }
        public float SpringDamping
        {
            get { return CameraSpringArmComponentInitializer.SpringDamping; }
            set { CameraSpringArmComponentInitializer.SpringDamping = value; }
        }
        public bool DoCollisionTest
        {
            get { return CameraSpringArmComponentInitializer.DoCollisionTest; }
            set { CameraSpringArmComponentInitializer.DoCollisionTest = value; }
        }
        public Bricks.PhysicsCore.CollisionComponent.CollisionLayers SelfLayer
        {
            get { return CameraSpringArmComponentInitializer.SelfLayer; }
            set
            {
                CameraSpringArmComponentInitializer.SelfLayer = value;
                RayCastQueryFilterData.data.word1 = (uint)value;
                OverlapQueryFilterData.data.word1 = (uint)value;
            }
        }
        Bricks.PhysicsCore.CPhyShape mOverlapShape = null;
        public float OverlapPrecision { get => ProbeSize * 0.2f; }
        public CameraComponent CameraComponent { get; set; }
        Bricks.PhysicsCore.PhyQueryFilterData RayCastQueryFilterData;
        Bricks.PhysicsCore.PhyQueryFilterData OverlapQueryFilterData;

        public CameraSpringArmComponentInitializer CameraSpringArmComponentInitializer
        {
            get { return Initializer as CameraSpringArmComponentInitializer; }
        }
        public CameraSpringArmComponent()
        {
            Bricks.PhysicsCore.PhyFilterData filterData = new Bricks.PhysicsCore.PhyFilterData();
            filterData.word1 = (uint)Bricks.PhysicsCore.CollisionComponent.CollisionLayers.Camera;
            RayCastQueryFilterData = new Bricks.PhysicsCore.PhyQueryFilterData(filterData);
            OverlapQueryFilterData = new Bricks.PhysicsCore.PhyQueryFilterData(filterData, Bricks.PhysicsCore.PhyQueryFlag.eDYNAMIC | Bricks.PhysicsCore.PhyQueryFlag.eSTATIC | Bricks.PhysicsCore.PhyQueryFlag.eANY_HIT);
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            RayCastQueryFilterData.data.word1 = (uint)SelfLayer;
            OverlapQueryFilterData.data.word1 = (uint)SelfLayer;
            return true;
        }
        bool SerachCamera()
        {
            var camera = GetComponentRecursion<CameraComponent>();
            if (camera != null)
            {
                CameraComponent = camera;
                return true;
            }
            else
            {
                camera = Host.GetComponentRecursion<CameraComponent>();
                if (camera != null)
                {
                    CameraComponent = camera;
                    return true;
                }
            }
            return false;
        }
        public override void Tick(GPlacementComponent placement)
        {
            if (!DoCollisionTest)
                return;
            if (Host.Scene == null)
                return;
            if (CameraComponent == null)
            {
                if (!SerachCamera())
                    return;
            }
            CameraComponent.PreCalculateCameraData(placement);
            var target2Camera = -CameraComponent.DesireDirection;
            var cameraCurrentDeampLoolAtOffset = placement.Rotation * CameraComponent.DeampLookAtOffset;

            //var realDir = Vector3.Slerp(target2Camera, -CameraComponent.CameraDirection, HookeLaw(SpringDamping, (target2Camera + CameraComponent.CameraDirection).Length()));
            var realDir = Vector3.Slerp(-CameraComponent.CameraDirection, target2Camera, SpringDamping);
            var worldLoc = placement.Location + PlacementComponent.Location + cameraCurrentDeampLoolAtOffset + realDir * 0.5f;
            GamePlay.SceneGraph.VHitResult result = new GamePlay.SceneGraph.VHitResult();
            var distance = ArmLength;
            //反向Raycast 测试是否有物体在中间
            if (Host.Scene.PhyScene.RaycastWithFilter(ref worldLoc, ref realDir, ArmLength, ref RayCastQueryFilterData, ref result))
            {
                distance = (result.Distance - ProbeSize);
                distance = Math.Max(0, distance);
            }
            if (mOverlapShape == null)
            {
                mOverlapShape = CEngine.Instance.PhyContext.CreateShapeSphere(Bricks.PhysicsCore.CPhyMaterial.DefaultPhyMaterial, ProbeSize);
            }

            var overlapPos = worldLoc + realDir * distance;
            //Overlap 测试是否靠障碍物太近, 找到合适的位置
            while (Host.Scene.PhyScene.OverlapWithFilter(mOverlapShape, overlapPos, Quaternion.Identity, ref OverlapQueryFilterData, ref result))
            {
                distance -= OverlapPrecision;
                //distance = Math.Max(0.3f, distance);
                overlapPos = worldLoc + realDir * distance;
            }
            var realPos = placement.Location + cameraCurrentDeampLoolAtOffset - CameraComponent.DesireDirection * distance;
            var logicOrgin = placement.Location + PlacementComponent.Location + cameraCurrentDeampLoolAtOffset;
            //var realDir = Vector3.Slerp(CameraComponent.WorldLocation- logicOrgin, realPos- logicOrgin, HookeLaw(SpringDamping,(realPos- CameraComponent.WorldLocation).Length()));
            //var realDir = Vector3.Slerp((CameraComponent.WorldLocation - logicOrgin).NormalizeValue, (realPos - logicOrgin).NormalizeValue, 1f);

            //用距离反算摄像机位置
            CameraComponent.DesirePosition = realDir * distance + logicOrgin;
            //CameraComponent.Tick(placement);

            base.Tick(placement);
        }
        float LimitMoveSpeed = 0.6f;
        private float HookeLaw(float k, float delta)
        {
            float result = k * delta;
            if (k == float.NaN || k == float.PositiveInfinity || k == float.NegativeInfinity)
                return LimitMoveSpeed;
            return Math.Min(result, LimitMoveSpeed);
        }
    }
}
