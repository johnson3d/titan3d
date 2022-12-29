using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class UTileMeshBuilder : AuxPtrType<TileMeshBuilder>
    {
        public UTileMeshBuilder()
        {
            mCoreObject = TileMeshBuilder.CreateInstance();
        }
        public void SetInputGeom(UInputGeom geom)
        {
            mCoreObject.NativeSuper.SetInputGeom(geom.mCoreObject);
        }
        public UNavMesh BuildNavi()
        {
            var ptr = mCoreObject.BuildNavi();
            return new UNavMesh(ptr);
        }
    }
}
