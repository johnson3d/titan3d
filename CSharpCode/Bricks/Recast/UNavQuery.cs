using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class UNavQuery : AuxPtrType<RcNavQuery>
    {
        public UNavQuery(RcNavQuery ptr)
        {
            mCoreObject = ptr;
        }
        public ushort IncludeFlags
        {
            get
            {
                return mCoreObject.GetIncludeFlags();
            }
            set
            {
                mCoreObject.SetIncludeFlags(value);
            }
        }
        public ushort ExcludeFlags
        {
            get
            {
                return mCoreObject.GetExcludeFlags();
            }
            set
            {
                mCoreObject.SetExcludeFlags(value);
            }
        }
        public bool GetHeight(in Vector3 pos, in Vector3 pickext, ref float h)
        {
            return mCoreObject.GetHeight(in pos, in pickext, ref h);
        }
        public bool FindStraightPath(in Vector3 start, in Vector3 end, Support.TtBlobObject blob)
        {
            return mCoreObject.FindStraightPath(in start, in end, blob.mCoreObject);
        }
        public void ClosestPointOnPoly(in Vector3 start, in Vector3 end)
        {
            mCoreObject.ClosestPointOnPoly(in start, in end);
        }
        public void AutoRecalcStraightPaths(in Vector3 start, in Vector3 end)
        {
            mCoreObject.AutoRecalcStraightPaths(in start, in end);
        }
    }
}
