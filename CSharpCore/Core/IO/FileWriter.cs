using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class FileWriter
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }

        public FileWriter(NativePointer self)
        {
            mCoreObject = self;
        }
        ~FileWriter()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                FileManager.VFile_Close(CoreObject);
                FileManager.VFile_Delete(CoreObject);
                mCoreObject.Pointer = IntPtr.Zero;
            }
        }
    }
}
