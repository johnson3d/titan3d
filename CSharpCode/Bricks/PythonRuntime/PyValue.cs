using NPOI.HSSF.Record.AutoFilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.PythonWrapper
{
    partial struct FPyClassWrapper
    {
        public unsafe T GetObject<T>() where T : class
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)UserData);
            return handle.Target as T;
        }
        public static unsafe T GetObject<T>(void* pSelf) where T : class
        {
            var wrapper = (PythonWrapper.FPyClassWrapper*)pSelf;
            return wrapper->GetObject<T>();
        }
    }
}

namespace EngineNS.Bricks.PythonRuntime
{
    public unsafe struct FPyValue : IDisposable
    {
        public unsafe void* mPyObject;
        public void Dispose()
        {
            if (mPyObject != IntPtr.Zero.ToPointer())
            {
                PythonWrapper.FPyUtility.DecPyRef(mPyObject);
                mPyObject = IntPtr.Zero.ToPointer();
            }
        }
        public FPyValue(void* ptr)
        {
            mPyObject = ptr;
        }
        public static FPyValue CreateInstance(int value)
        {
            return new FPyValue(PythonWrapper.FPyUtility.From(value));
        }
        public int I32
        {
            get
            {
                return PythonWrapper.FPyUtility.ToInt32(mPyObject);
            }
        }
        public uint UI32
        {
            get
            {
                return PythonWrapper.FPyUtility.ToUInt32(mPyObject);
            }
        }
    }
}
