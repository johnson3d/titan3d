using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class UWorldRootNode : Scene.UScene
    {
        public UWorldRootNode(Scene.UNodeData data)
            : base(data)
        {
            
        }
        public override string NodeName
        {
            get
            {
                return "WorldRootNode";
            }
        }        
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            //base.OnGatherVisibleMeshes(rp);
        }
    }

    public class UWorld
    {
        public UWorld()
        {
            mOnVisitNode_GatherVisibleMeshesAll = this.OnVisitNode_GatherVisibleMeshesAll;
            mOnVisitNode_GatherBoundShapes = this.OnVisitNode_GatherBoundShapes;
        }
        public void Cleanup()
        {
            Root.ClearChildren();
        }
        public UWorldRootNode Root
        {
            get;
        } = new UWorldRootNode(new Scene.UNodeData());
        public UDirectionLight DirectionLight { get; } = new UDirectionLight();
        #region Culling
        public class UVisParameter
        {
            public Graphics.Pipeline.CCamera CullCamera;
            public List<Graphics.Mesh.UMesh> VisibleMeshes = new List<Graphics.Mesh.UMesh>();
        }
        public virtual void GatherVisibleMeshes(UVisParameter rp)
        {
            rp.VisibleMeshes.Clear();

            OnVisitNode_GatherVisibleMeshes(Root, rp);
        }
        private unsafe bool OnVisitNode_GatherVisibleMeshes(Scene.UNode node, object arg)
        {
            if (node.HasStyle(Scene.UNode.ENodeStyles.VisibleMeshProvider) == false)
            {
                return false;
            }
            var rp = arg as UVisParameter;
            
            CONTAIN_TYPE type;
            if (node.HasStyle(Scene.UNode.ENodeStyles.VisibleFollowParent))
            {
                type = CONTAIN_TYPE.CONTAIN_TEST_INNER;
            }
            else
            {
                var frustom = rp.CullCamera.mCoreObject.GetFrustum();
                var absAABB = BoundingBox.Transform(in node.AABB, in node.Placement.mAbsTransform);
                type = frustom->whichContainTypeFast(in absAABB, 1);
                //这里还没想明白，把Frustum的6个平面变换到AABB所在坐标为啥不行
                //type = frustom->whichContainTypeFast(ref node.AABB, ref node.Placement.AbsTransformInv, 1);
            }
            switch (type)
            {
                case CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_INNER:
                    node.DFS_VisitNodeTree(mOnVisitNode_GatherVisibleMeshesAll, rp);
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_REFER:
                    {
                        node.OnGatherVisibleMeshes(rp);
                        foreach (var i in node.Children)
                        {
                            OnVisitNode_GatherVisibleMeshes(i, arg);
                        }
                    }
                    break;
            }
            return false;
        }
        Scene.UNode.FOnVisitNode mOnVisitNode_GatherVisibleMeshesAll;
        private unsafe bool OnVisitNode_GatherVisibleMeshesAll(Scene.UNode node, object arg)
        {
            var rp = arg as UVisParameter;
            
            node.OnGatherVisibleMeshes(rp);
            return false;
        }
        #endregion

        #region DebugAssist
        
        public void GatherBoundShapes(List<Graphics.Mesh.UMesh> boundVolumes, Scene.UNode node = null)
        {
            if (node == null)
                node = Root;
            node.DFS_VisitNodeTree(mOnVisitNode_GatherBoundShapes, boundVolumes);
        }
        Scene.UNode.FOnVisitNode mOnVisitNode_GatherBoundShapes;
        private unsafe bool OnVisitNode_GatherBoundShapes(Scene.UNode node, object arg)
        {
            var bvs = arg as List<Graphics.Mesh.UMesh>;

            ref var aabb = ref node.AABB;
            var size = aabb.GetSize();
            var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeBoxWireframe(aabb.Minimum.X, aabb.Minimum.Y, aabb.Minimum.Z,
                size.X, size.Y, size.Z).ToMesh();
            var mesh2 = new Graphics.Mesh.UMesh();

            var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
            materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
            if (materials1[0] == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            mesh2.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            mesh2.SetWorldMatrix(ref node.Placement.mAbsTransform);

            bvs.Add(mesh2);

            return false;
        }
        #endregion

        #region GamePlay
        public virtual void TickLogic()
        {
            Root.TickLogic();
        }
        #endregion
    }
}
