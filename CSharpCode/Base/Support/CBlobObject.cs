using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class CBlobObject : AuxPtrType<IBlobObject>
    {
        public CBlobObject()
        {
            mCoreObject = IBlobObject.CreateInstance();
        }
    }
}
