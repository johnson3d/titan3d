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
        int CountOfAlloc = 0;
        CoreSDK.FDelegate_FOnNativeMemAlloc OnNativeMemAlloc;
        private unsafe void OnNativeMemAllocImpl(IntPtr size, sbyte* file, IntPtr line, IntPtr id)
        {
            var sourceFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
            if(size == (IntPtr)32 && (int)id > 25000)
            {
                CountOfAlloc++;
            }
        }
        CoreSDK.FDelegate_FOnNativeMemFree OnNativeMemFree;
        private unsafe void OnNativeMemFreeImpl(IntPtr size, sbyte* file, IntPtr line, IntPtr id)
        {
            var sourceFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
        }
        CoreSDK.FDelegate_FOnNativeMemLeak OnNativeMemLeak;
        private unsafe void OnNativeMemLeakImpl(void* ptr, IntPtr size, sbyte* file, IntPtr line, IntPtr id, sbyte* debugInfo)
        {
            var sourceFile = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)file);
            var info = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)debugInfo);
        }
    }
}
