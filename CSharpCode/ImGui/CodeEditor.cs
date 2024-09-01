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
        public unsafe string Text
        {
            get
            {
                var blob = new Support.TtBlobObject();
                mCoreObject.GetText(blob.mCoreObject);
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)blob.mCoreObject.GetData(), (int)blob.mCoreObject.GetSize());
            }
        }
    }
}
