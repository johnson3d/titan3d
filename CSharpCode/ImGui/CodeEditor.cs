using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class TtCodeEditor : AuxPtrType<FCodeEditor>
    {
        public TtCodeEditor()
        {
            mCoreObject = FCodeEditor.CreateInstance();
        }
    }
}
