using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain
{
    public class TtQTree : AuxPtrType<QTree>
    {
        public TtQTree()
        {
            mCoreObject = QTree.CreateInstance();
        }
        public int PatchSideNum
        {
            get
            {
                return mCoreObject.GetPatchSide();
            }
        }
        public void SetPatch(int x, int z, object patch)
        {
            unsafe
            {
                var leaf = mCoreObject.GetLeaf(x, z);
                if (leaf.TagObject != (void*)0)
                {
                    System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)leaf.TagObject).Free();
                }
                if (patch == null)
                {
                    leaf.TagObject = (void*)0;
                }
                else
                {
                    leaf.TagObject = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(patch, System.Runtime.InteropServices.GCHandleType.Weak)).ToPointer();
                }
            }
        }
    }
}

namespace EngineNS.UnitTest
{
    [UnitTest.TtTest]
    public class UTest_UQTree
    {
        public unsafe void UnitTestEntrance()
        {
            var qtree = new Bricks.Terrain.TtQTree();
            qtree.mCoreObject.Initialize(4, 64.0f);
        }
    }
}

