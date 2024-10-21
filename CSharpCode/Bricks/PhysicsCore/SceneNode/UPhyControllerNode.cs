using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.Bricks.PhysicsCore.SceneNode.TtCapsulePhyControllerNode;
using static EngineNS.Bricks.PhysicsCore.SceneNode.TtPhyControllerNodeBase;

namespace EngineNS.Bricks.PhysicsCore.SceneNode
{
    public class TtPhyControllerNodeBase : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtPhyControllerNodeDataBase : TtNodeData
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
        public Bricks.PhysicsCore.TtPhyController PhyController { get; set; } = null;

        public bool TryMove(DVector3 dist, float deltaTimeSecond, out DVector3 newPosition)
        {
            if (PhyController == null)
            {
                newPosition = DVector3.Zero;
                return false;
            }
            var data = NodeData as TtCapsulePhyControllerNodeData;
            var phyResult = PhyController.mCoreObject.Move(dist.ToSingleVector3(), 0.001f, deltaTimeSecond, data.QueryFilterData, data.PhyQueryFlags);
            newPosition = PhyController.mCoreObject.GetFootPosition().AsDVector();
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("PhyCapsule", "PhyCapsule", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCapsulePhyControllerNode.TtCapsulePhyControllerNodeData), DefaultNamePrefix = "PhyCapsule")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtCapsulePhyControllerNode : TtPhyControllerNodeBase
    {
        public class TtCapsulePhyControllerNodeData : TtPhyControllerNodeDataBase
        {
            [Rtti.Meta]
            public float Radius { get; set; } = 1.0f;
            [Rtti.Meta]
            public float Height { get; set; } = 1.0f;
        }
        public TtCapsulePhyControllerNodeData CapsulePhyControllerNodeData
        {
            get => NodeData as TtCapsulePhyControllerNodeData;
        }
        TtPhyCapsuleControllerDesc PhyControllerDesc = null;

        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            PhyControllerDesc = new Bricks.PhysicsCore.TtPhyCapsuleControllerDesc();
            PhyControllerDesc.mCoreObject.SetCapsuleHeight(CapsulePhyControllerNodeData.Height);
            PhyControllerDesc.mCoreObject.SetCapsuleRadius(CapsulePhyControllerNodeData.Radius);
            
            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (CapsulePhyControllerNodeData.PxMaterial != null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(CapsulePhyControllerNodeData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
            PhyControllerDesc.SetMaterial(mtl);
            return true;
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
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
    [Bricks.CodeBuilder.ContextMenu("PhyBox", "PhyBox", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtBoxPhyControllerNode.TtBoxPhyControllerNodeData), DefaultNamePrefix = "PhyBox")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtBoxPhyControllerNode : TtPhyControllerNodeBase
    {
        public class TtBoxPhyControllerNodeData : TtPhyControllerNodeDataBase
        {
            [Rtti.Meta]
            public Vector3 Extent { get; set; } = Vector3.One;
        }
        public TtBoxPhyControllerNodeData BoxPhyControllerNodeData
        {
            get => NodeData as TtBoxPhyControllerNodeData;
        }
        TtPhyBoxControllerDesc PhyControllerDesc = null;
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var baseResult = await base.InitializeNode(world, data, bvType, placementType);
            if (!baseResult)
                return false;
            PhyControllerDesc = new Bricks.PhysicsCore.TtPhyBoxControllerDesc();
            PhyControllerDesc.mCoreObject.SetExtent(BoxPhyControllerNodeData.Extent);
            Bricks.PhysicsCore.TtPhyMaterial mtl;
            if (BoxPhyControllerNodeData.PxMaterial != null)
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterialSync(BoxPhyControllerNodeData.PxMaterial);
            else
                mtl = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
            PhyControllerDesc.SetMaterial(mtl);
            return true;
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
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
