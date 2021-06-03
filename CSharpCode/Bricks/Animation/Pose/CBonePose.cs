using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pose
{
    public class CBonePose : AuxPtrType<IBonePose>
    {
        public CBonePose()
        {
            mCoreObject = IBonePose.CreateInstance();
        }
        
    }
}
