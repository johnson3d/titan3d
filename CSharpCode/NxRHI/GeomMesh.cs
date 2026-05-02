using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtVertexArray : AuxPtrType<NxRHI.FVertexArray>
    {
        public TtVertexArray(NxRHI.FVertexArray ptr)
        {
            mCoreObject = ptr;
        }
        public void BindVB(EVertexStreamType stream, TtVbView buffer)
        {
            mCoreObject.BindVB(stream, buffer.mCoreObject);
        }
    }    
    public class TtGeomMesh : AuxPtrType<NxRHI.FGeomMesh>
    {
        public TtGeomMesh(NxRHI.FGeomMesh ptr)
        {
            mCoreObject = ptr;
        }
        public void SetAtomNum(uint num)
        {
            mCoreObject.SetAtomNum(num);
        }
        public uint GetAtomNum()
        {
            return mCoreObject.GetAtomNum();
        }
        public ref FMeshAtomDesc GetAtomDesc(uint index, uint lod)
        {
            unsafe
            {
                return ref *mCoreObject.GetAtomDesc(index, lod);
            }
        }
        public void BindVertexArray(TtVertexArray va)
        {
            mCoreObject.BindVertexArray(va.mCoreObject);
        }
        public void BindIndexBuffer(TtIbView buffer)
        {
            mCoreObject.BindIndexBuffer(buffer.mCoreObject);
        }
        public void PushAtomDesc(uint index, uint lod, in FMeshAtomDesc desc)
        {
            mCoreObject.SetAtomDesc(index, lod, in desc);
        }
    }
}
