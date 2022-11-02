using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.Bricks.PhysicsCore.SceneNode.UCapsulePhyControllerNode;
using static EngineNS.Bricks.PhysicsCore.SceneNode.UPhyControllerNodeBase;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{
    public class UPhyControllerNodeBase : GamePlay.Scene.ULightWeightNodeBase
    {
        public class UPhyControllerNodeDataBase : UNodeData
        {
            [Rtti.Meta]
            public RName PxMaterial { get; set; }
            [Rtti.Meta]
            public PhyFilterData QueryFilterData { get; set; }
            [Rtti.Meta]
            public PhyFilterData SimulationFilterData { get; set; }
            [Rtti.Meta]
            public PhyQueryFlag PhyQueryFlags { get; set; } = PhyQueryFlag.eSTATIC | PhyQueryFlag.eDYNAMIC | PhyQueryFlag.ePREFILTER;

        }
        public Bricks.PhysicsCore.UPhyController PhyController { get; set; } = null;

        public bool TryMove(DVector3 dist, float deltaTimeSecond, out DVector3 newPosition)
        {
            if (PhyController == null)
            {
                newPosition = DVector3.Zero;
                return false;
            }
            var data = NodeData as UCapsulePhyControllerNodeData;
            var phyResult = PhyController.mCoreObject.Move(dist.ToSingleVector3(), 0.001f, deltaTimeSecond, data.QueryFilterData, data.PhyQueryFlags);
            newPosition = PhyController.mCoreObject.GetFootPosition().AsDVector();
            return true;
        }
    }
    public class UCapsulePhyControllerNode : UPhyControllerNodeBase
    {
        public class UCapsulePhyControllerNodeData : UPhyControllerNodeDataBase
        {
            [Rtti.Meta]
            public float Radius { get; set; } = 1.0f;
            [Rtti.Meta]
            public float Height { get; set; } = 1.0f;
        }
        public UCapsulePhyControllerNodeData CapsulePhyControllerNodeData
        {
            get => NodeData as UCapsulePhyControllerNodeData;
        }
        UPhyCapsuleControllerDesc PhyControllerDesc = null;

        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            PhyControllerDesc = new Bricks.PhysicsCore.UPhyCapsuleControllerDesc();
            PhyControllerDesc.mCoreObject.SetCapsuleHeight(CapsulePhyControllerNodeData.Height);
            PhyControllerDesc.mCoreObject.SetCapsuleRadius(CapsulePhyControllerNodeData.Radius);
            
            Bricks.PhysicsCore.UPhyMaterial mtl;
            if (CapsulePhyControllerNodeData.PxMaterial != null)
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CapsulePhyControllerNodeData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
            PhyControllerDesc.SetMaterial(mtl);
            return true;
        }
        protected override void OnParentChanged(UNode prev, UNode cur)
        {
            PhyController = ParentScene.PxSceneMB.PxScene.CreateCapsuleController(PhyControllerDesc.mCoreObject);
            if (PhyController == null)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            PhyController.mCoreObject.SetFootPosition(cur.Placement.AbsTransform.Position.ToSingleVector3());
            PhyController.mCoreObject.SetQueryFilterData(CapsulePhyControllerNodeData.QueryFilterData);
            PhyController.mCoreObject.SetSimulationFilterData(CapsulePhyControllerNodeData.SimulationFilterData);
            base.OnParentChanged(prev, cur);
        }
       
    }
    public class UBoxPhyControllerNode : UPhyControllerNodeBase
    {
        public class UBoxPhyControllerNodeData : UPhyControllerNodeDataBase
        {
            [Rtti.Meta]
            public Vector3 Extent { get; set; } = Vector3.One;
        }
        public UBoxPhyControllerNodeData BoxPhyControllerNodeData
        {
            get => NodeData as UBoxPhyControllerNodeData;
        }
        UPhyBoxControllerDesc PhyControllerDesc = null;
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            PhyControllerDesc = new Bricks.PhysicsCore.UPhyBoxControllerDesc();
            PhyControllerDesc.mCoreObject.SetExtent(BoxPhyControllerNodeData.Extent);
            Bricks.PhysicsCore.UPhyMaterial mtl;
            if (BoxPhyControllerNodeData.PxMaterial != null)
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(BoxPhyControllerNodeData.PxMaterial);
            else
                mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
            PhyControllerDesc.SetMaterial(mtl);
            return true;
        }
        protected override void OnParentChanged(UNode prev, UNode cur)
        {
            PhyController = ParentScene.PxSceneMB.PxScene.CreateBoxController(PhyControllerDesc.mCoreObject);
            if (PhyController == null)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            PhyController.mCoreObject.SetFootPosition(cur.Placement.AbsTransform.Position.ToSingleVector3());
            PhyController.mCoreObject.SetQueryFilterData(BoxPhyControllerNodeData.QueryFilterData);
            PhyController.mCoreObject.SetSimulationFilterData(BoxPhyControllerNodeData.SimulationFilterData);
            base.OnParentChanged(prev, cur);
        }
    }
}
