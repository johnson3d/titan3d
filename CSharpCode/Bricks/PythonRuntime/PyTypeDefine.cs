using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PythonRuntime
{
    public class TtPyTypeDefine : AuxPtrType<PythonWrapper.FPyTypeDefine>
    {
        public TtPyTypeDefine(TtPyModule module, string name)
        {
            mCoreObject = PythonWrapper.FPyTypeDefine.CreateInstance(module.mCoreObject, name);
        }
        public static unsafe void Bind<T>(PythonWrapper.FPyClassWrapper* pWrapper) where T : class, new()
        {
            var handle = System.Runtime.InteropServices.GCHandle.Alloc(new TtTestPythonBinder(), System.Runtime.InteropServices.GCHandleType.Normal);
            pWrapper->UserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(handle).ToPointer();
        }
        public static unsafe void UnBind(PythonWrapper.FPyClassWrapper* pWrapper)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)pWrapper->UserData);
            handle.Free();
            pWrapper->UserData = IntPtr.Zero.ToPointer();
        }
        TtPyMethodDefine mPyMethodDefine;
        public TtPyMethodDefine PyMethodDefine
        {
            get => mPyMethodDefine;
            set
            {
                mPyMethodDefine = value;
                mCoreObject.SetMethods(mPyMethodDefine.mCoreObject);
            }
        }
    }

    public class TtPyMethodDefine : AuxPtrType<PythonWrapper.FPyMethodDefine>
    {
        public TtPyMethodDefine()
        {
            mCoreObject = PythonWrapper.FPyMethodDefine.CreateInstance();
        }
    }
}
