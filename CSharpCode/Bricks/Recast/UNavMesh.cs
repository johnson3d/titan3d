using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class UNavMesh : AuxPtrType<RcNavMesh>
    {
        public UNavMesh(RcNavMesh ptr)
        {
            mCoreObject = ptr;
        }
        public UNavQuery CreateQuery(int maxNodes)
        {
            var ptr = mCoreObject.CreateQuery(maxNodes);
            if (ptr.IsValidPointer == false)
                return null;
            return new UNavQuery(ptr);
        }
        public int TilesCount
        {
            get
            {
                return mCoreObject.GetTilesCount();
            }
        }
        public bool CheckVaildAt(int tileindex, int layer)
        {
            return mCoreObject.CheckVaildAt(tileindex, layer);
        }
        public Vector3 GetPositionAt(int tileindex, int layer)
        {
            return mCoreObject.GetPositionAt(tileindex, layer);
        }
        public Vector3 GetBoundBoxMaxAt(int tileindex)
        {
            return mCoreObject.GetBoundBoxMaxAt(tileindex);
        }
        public Vector3 GetBoundBoxMinAt(int tileindex)
        {
            return mCoreObject.GetBoundBoxMinAt(tileindex);
        }
        public bool LoadXnd(IO.CXndNode node)
        {
            return mCoreObject.LoadXnd(node.mCoreObject);
        }
        public void Save2Xnd(IO.CXndNode node)
        {
            mCoreObject.Save2Xnd(node.mCoreObject);
        }
        public Graphics.Mesh.UMeshDataProvider CreateRenderMesh()
        {
            var ptr = mCoreObject.CreateRenderMesh();
            return new Graphics.Mesh.UMeshDataProvider(ptr);
        }
    }
}
