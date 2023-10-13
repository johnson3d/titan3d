using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UVertexArray : AuxPtrType<NxRHI.FVertexArray>
    {
        public UVertexArray(NxRHI.FVertexArray ptr)
        {
            mCoreObject = ptr;
        }
        public void BindVB(EVertexStreamType stream, UVbView buffer)
        {
            mCoreObject.BindVB(stream, buffer.mCoreObject);
        }
    }    
    public class UGeomMesh : AuxPtrType<NxRHI.FGeomMesh>
    {
        public UGeomMesh(NxRHI.FGeomMesh ptr)
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
        public void BindVertexArray(UVertexArray va)
        {
            mCoreObject.BindVertexArray(va.mCoreObject);
        }
        public void BindIndexBuffer(UIbView buffer)
        {
            mCoreObject.BindIndexBuffer(buffer.mCoreObject);
        }
        public void PushAtomDesc(uint index, uint lod, in FMeshAtomDesc desc)
        {
            mCoreObject.SetAtomDesc(index, lod, in desc);
        }
    }
}
