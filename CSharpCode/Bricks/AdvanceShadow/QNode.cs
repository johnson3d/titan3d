using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;

namespace EngineNS.Bricks.AdvanceShadow
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowNodeData")]
    public struct FAdvShadowNodeData
    {
        public void SetDefault()
        {
            mChildIndex00 = -1;
            mChildIndex01 = -1;
            mChildIndex10 = -1;
            mChildIndex11 = -1;
            mPageIndex = -1;
        }
        public Matrix mShadowMatrix;
        public int mChildIndex00;
        public int mChildIndex01;
        public int mChildIndex10;
        public int mChildIndex11;

        public int mNodeType;
        public int mPageIndex;
        //public int mObjectNum;
        public float mZNear;
        public float mZFar;
    }

    public class TtShadowObject
    {
        public TtNode SceneNode;
        public DBoundingBox AABB;
        public bool IsDynamic;
    }
    public partial class TtQNode
    {
        public TtQNode(int deepLevel)
        {
            DeepLevel = deepLevel;
            Leaf = new TtQLeaf(this);
        }
        public enum ENodeType
        {
            Node,
            Leaf,
            DeathNode,
        }
        public ENodeType NodeType = ENodeType.DeathNode;
        public TtQTree QTree;
        public int NodeIndex = -1;
        public int DeepLevel;
        public DBoundingBox2D AABB;
        public DPlane[] CullPlanes = new DPlane[4];
        public TtQNode Parent = null;
        public TtQNode Child00 = null;
        public TtQNode Child01 = null;
        public TtQNode Child10 = null;
        public TtQNode Child11 = null;
        public TtQLeaf Leaf = null;
        public int PageIndex = -1;
        public Matrix ShadowMatrix;

        public List<TtShadowObject> ShadowObjects = new List<TtShadowObject>();
        public uint UpdateShadowMapTime
        {
            get => Leaf.UpdateShadowMapTime;
        }
        public void BuildCullPlanes()
        {
            /* 
               1
             0   2
               3
            */
            var norm = Vector3.Cross(in Vector3.UnitZ, in QTree.LightDirection);
            norm.Normalize();
            var norm_d = norm.AsDVector();
            CullPlanes[0] = new DPlane(new DVector3(AABB.Minimum.X, 0, AABB.Minimum.Y), norm_d);
            CullPlanes[2] = new DPlane(new DVector3(AABB.Maximum.X, 0, AABB.Maximum.Y), norm_d);
            norm = Vector3.Cross(in Vector3.UnitX, in QTree.LightDirection);
            norm.Normalize();
            norm_d = norm.AsDVector();
            CullPlanes[1] = new DPlane(new DVector3(AABB.Maximum.X, 0, AABB.Maximum.Y), norm_d);
            CullPlanes[3] = new DPlane(new DVector3(AABB.Minimum.X, 0, AABB.Minimum.Y), norm_d);

            var center = AABB.GetCenter();
            var center3d = new DVector3(center.X, 0, center.Y);
            for (int i = 0; i < 4; i++)
            {
                if (DPlane.DotCoordinate(CullPlanes[i], center3d) > 0)
                {
                    CullPlanes[i].Normal = -CullPlanes[i].Normal;
                    CullPlanes[i].D = -CullPlanes[i].D;
                }
            }
        }
        public void SetToGpuData(ref FAdvShadowNodeData data)
        {
            data.mNodeType = (int)NodeType;
            data.mPageIndex = PageIndex;
            data.mShadowMatrix = Matrix.Transpose(ShadowMatrix);
            //data.mObjectNum = ShadowObjects.Count;
            data.mZNear = Leaf.ShadowCamera.ZNear;
            data.mZFar = Leaf.ShadowCamera.ZFar;
            if (Child00 != null)
            {
                data.mChildIndex00 = Child00.NodeIndex;
                data.mChildIndex01 = Child01.NodeIndex;
                data.mChildIndex10 = Child10.NodeIndex;
                data.mChildIndex11 = Child11.NodeIndex;
            }
        }
        internal void AsLeaf()
        {
            NodeType = ENodeType.Leaf;
            if (PageIndex < 0 && ShadowObjects.Count > 0)
            {
                PageIndex = QTree.AllocPage();
            }
            else if (PageIndex >= 0)
            {
                if (ShadowObjects.Count == 0)
                {
                    QTree.FreePage(PageIndex);
                    PageIndex = -1;
                }
            }
        }
        public void SurePage()
        {
            if (PageIndex < 0 && ShadowObjects.Count > 0)
            {
                PageIndex = QTree.AllocPage();
            }
        }
        internal void AsNode()
        {
            NodeType = ENodeType.Node;
            //if (PageIndex < 0 && ShadowObjects.Count > 0)
            //{
            //    PageIndex = QTree.AllocPage();
            //}
            //else 
            if (PageIndex >= 0)
            {
                if (ShadowObjects.Count == 0)
                {
                    QTree.FreePage(PageIndex);
                    PageIndex = -1;
                }
            }
        }
        internal void AsDeathNode()
        {
            NodeType = ENodeType.DeathNode;
            if (PageIndex >= 0)
            {
                QTree.FreePage(PageIndex);
            }
            ShadowObjects = new List<TtShadowObject>();
            PageIndex = -1;
        }
        internal bool PushObject(in TtShadowObject shadowObj)
        {
            for(int i = 0; i < 4; i++)
            {
                if(DPlane.Intersects(CullPlanes[i], in shadowObj.SceneNode.BoundVolume.AbsAABB) == PlaneIntersectionType.Front)
                {
                    return false;
                }
            }

            ShadowObjects.Add(shadowObj);

            if (NodeType == ENodeType.Leaf)
            {
                Leaf.IsDirty = true;

                if (shadowObj.IsDynamic)
                    Leaf.IsContainDynamicObject = true;
            }

            return true;
        }
        public void ClearShadowNodes()
        {
            ShadowObjects.Clear();
            if (Child00 != null)
            {
                Child00.ClearShadowNodes();
                Child01.ClearShadowNodes();
                Child10.ClearShadowNodes();
                Child11.ClearShadowNodes();
            }
            else
            {
                Leaf.IsDirty = true;
            }
        }
        public void RemoveShadowNode(TtNode node)
        {
            bool bFind = false;
            for (int i = 0; i < ShadowObjects.Count; i++)
            {
                if (ShadowObjects[i].SceneNode == node)
                {
                    ShadowObjects.RemoveAt(i);
                    bFind = true;
                    break;
                }
            }
            if (bFind == false)
                return;

            if (Child00 != null)
            {
                Child00.RemoveShadowNode(node);
                Child01.RemoveShadowNode(node);
                Child10.RemoveShadowNode(node);
                Child11.RemoveShadowNode(node);
            }
            else
            {
                Leaf.IsDirty = true;
            }
        }

        public void Initialize(TtQTree tree, in DVector2 min, in DVector2 max)
        {
            QTree = tree;
            AABB.Minimum = min;
            AABB.Maximum = max;
        }
        public void PushShadowObjects(TtQTree tree, List<TtShadowObject> nodes)
        {
            foreach (var i in nodes)
            {
                this.PushObject(i);
            }
        }

        public void GatherLeafs(List<TtQNode> leafs)
        {
            if (this.NodeType == ENodeType.Leaf)
            {
                leafs.Add(this);
                return;
            }
            if (Child00 != null)
            {
                Child00.GatherLeafs(leafs);
                Child01.GatherLeafs(leafs);
                Child10.GatherLeafs(leafs);
                Child11.GatherLeafs(leafs);
            }
        }
    }
}
