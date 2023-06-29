using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler
{
    public class UNativeMemory
    {
        ~UNativeMemory()
        {
            EndProfiler();
        }
        public void BeginProfiler()
        {
            unsafe
            {
                OnNativeMemAlloc = this.OnNativeMemAllocImpl;
                CoreSDK.SetMemAllocCallBack(OnNativeMemAlloc);
                OnNativeMemFree = this.OnNativeMemFreeImpl;
                CoreSDK.SetMemFreeCallBack(OnNativeMemFree);
                OnNativeMemLeak = this.OnNativeMemLeakImpl;
                CoreSDK.SetMemLeakCallBack(OnNativeMemLeak);
            }
        }
        public void EndProfiler()
        {
            CoreSDK.SetMemAllocCallBack(null);
            CoreSDK.SetMemFreeCallBack(null);
            CoreSDK.SetMemLeakCallBack(null);
        }
        CoreSDK.FDelegate_FOnNativeMemAlloc OnNativeMemAlloc;
        //在做ClrProfiler的时候，不要MarshalString，否则会干扰ObjectAllocate统计!!!!!
        static int testSize = 1048624;
        private unsafe void OnNativeMemAllocImpl(IntPtr size, sbyte* file, IntPtr line, IntPtr id)
        {
            //if ((uint)id == 2085)
            //{
            //    return;
            //}
        }
        CoreSDK.FDelegate_FOnNativeMemAlloc OnNativeMemFree;
        private unsafe void OnNativeMemFreeImpl(IntPtr size, sbyte* file, IntPtr line, IntPtr id)
        {
            //var sourceFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
        }
        CoreSDK.FDelegate_FOnNativeMemLeak OnNativeMemLeak;
        private unsafe void OnNativeMemLeakImpl(void* ptr, IntPtr size, sbyte* file, IntPtr line, IntPtr id, sbyte* debugInfo)
        {
            //var sourceFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
            //var info = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)debugInfo);
        }
    }

    public struct FNativeMemState
    {
        public string File;
        public int Line;
        public UInt64 Size;
        public UInt32 Count;
    }
    public class TtNativeMemCapture : AuxPtrType<FNativeMemCapture>
    {
        public TtNativeMemCapture()
        {
            mCoreObject = FNativeMemCapture.CreateInstance();
        }
        public void CaptureNativeMemoryState()
        {
            mCoreObject.CaptureNativeMemoryState();
        }
        public unsafe void GetIncreaseTypes(TtNativeMemCapture old)
        {
            var iter = mCoreObject.NewIterate();
            if (iter == IntPtr.Zero.ToPointer())
                return;

            do
            {
                var type = mCoreObject.GetMemType(iter);
                var oType = old.mCoreObject.FindMemType((int)type.Size, type.File, type.Line);
                int changed;
                if (oType.IsValidPointer)
                {
                    changed = (int)type.Count - (int)oType.Count;
                    if (changed != 0)
                        Profiler.Log.WriteLine(ELogTag.Info, "NativeMemory", $"[{type.Size}]{type.File}({type.Line}):{changed} = {type.Count} - {oType.Count}");
                }
                else
                {
                    changed = type.Count;
                    Profiler.Log.WriteLine(ELogTag.Info, "NativeMemory", $"[{type.Size}]{type.File}({type.Line}):{changed} = {type.Count} - 0");
                }
            }
            while (mCoreObject.NextIterate(iter));
            mCoreObject.DestroyIterate(iter);
        }
    }
}
