using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class TtTileMeshBuilder : AuxPtrType<TileMeshBuilder>
    {
        public TtTileMeshBuilder()
        {
            mCoreObject = TileMeshBuilder.CreateInstance();
        }
        public void SetInputGeom(TtInputGeom geom)
        {
            mCoreObject.NativeSuper.SetInputGeom(geom.mCoreObject);
        }
        public TtNavMesh BuildNavi()
        {
            var ptr = mCoreObject.BuildNavi();
            return new TtNavMesh(ptr);
        }
    }
}
