using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class CStaticModifier : AuxPtrType<IStaticModifier>
    {
        public CStaticModifier()
        {
            mCoreObject = IStaticModifier.CreateInstance();
        }
    }
}
