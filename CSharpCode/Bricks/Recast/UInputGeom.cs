using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class UInputGeom : AuxPtrType<InputGeom>
    {
        public UInputGeom()
        {
            mCoreObject = InputGeom.CreateInstance();
        }
        public bool LoadMesh(Graphics.Mesh.UMeshDataProvider mesh, float scale)
        {
            return mCoreObject.LoadMesh(mesh.mCoreObject, scale);
        }
        public bool IsInWalkArea(in Vector3 min, in Vector3 max)
        {
            return mCoreObject.IsInWalkArea(in min, in max);
        }
    }
}
