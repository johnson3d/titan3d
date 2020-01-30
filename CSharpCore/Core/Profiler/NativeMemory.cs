using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Profiler
{
    public class NativeMemory
    {
        public static NativeProfiler CurrentProfiler = new NativeProfiler();
        public static void BeginProfile()
        {
            SDK_vfxMemory_SetMemAllocCallBack(mOnMemAlloc);
            SDK_vfxMemory_SetMemFreeCallBack(mOnMemFree);
        }
        public static void EndProfile()
        {
            SDK_vfxMemory_SetMemAllocCallBack(null);
            SDK_vfxMemory_SetMemFreeCallBack(null);
        }
        private static FOnMemAlloc mOnMemAlloc = OnMemAlloc;
        private static FOnMemFree mOnMemFree = OnMemFree;
        private static void OnMemAlloc(IntPtr size, IntPtr file, IntPtr line, IntPtr id)
        {
            if (id == (IntPtr)313201)
                return;
            if (CurrentProfiler == null)
                return;

            CurrentProfiler.OnMemAlloc(size, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(file), line, id);
        }
        private static void OnMemFree(IntPtr size, IntPtr file, IntPtr line, IntPtr id)
        {
            if (CurrentProfiler == null)
                return;

            CurrentProfiler.OnMemFree(size, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(file), line, id);
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        private delegate void FOnMemAlloc(IntPtr size, IntPtr file, IntPtr line, IntPtr id);
        private delegate void FOnMemFree(IntPtr size, IntPtr file, IntPtr line, IntPtr id);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vfxMemory_SetMemAllocCallBack(FOnMemAlloc cb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vfxMemory_SetMemFreeCallBack(FOnMemFree cb);
        #endregion
    }
    public class NativeProfiler
    {
        public class NMDesc
        {
            public Int32 Count;
            public Int64 Size;
        }
        public Dictionary<string, NMDesc> Mems
        {
            get;
        } = new Dictionary<string, NMDesc>();
        public Int64 AllocCount = 0;
        public Int64 FreeCount = 0;
        public Int64 TotalCount = 0;
        public virtual void OnMemAlloc(IntPtr size, string file, IntPtr line, IntPtr id)
        {
            lock(this)
            {
                AllocCount++;
                TotalCount++;
                NMDesc desc;
                if (false == Mems.TryGetValue($"{file}:{line}", out desc))
                {
                    desc = new NMDesc();
                    desc.Count = 1;
                    desc.Size = (Int64)size;
                    Mems.Add($"{file}:{line}", desc);
                    return;
                }
                desc.Size += (Int64)size;
                desc.Count++;
            }
        }
        public virtual void OnMemFree(IntPtr size, string file, IntPtr line, IntPtr id)
        {
            lock (this)
            {
                FreeCount++;
                TotalCount--;
                if (TotalCount < 0)
                    TotalCount = 0;
                NMDesc desc;
                if (false == Mems.TryGetValue($"{file}:{line}", out desc))
                {
                    //Log.WriteLine(ELogTag.Fatal, "NativeMemory", $"Native Memory Free Invalid=>{file}:{line}");
                    return;
                }
                desc.Size -= (Int64)size;
                desc.Count--;
                if(desc.Size<0|| desc.Count<0)
                {
                    desc.Size = 0;
                    desc.Count = 0;
                    //Log.WriteLine(ELogTag.Fatal, "NativeMemory", $"Native Memory Free Invalid=>{file}:{line}:{desc.Size}:{desc.Count}");
                }
            }
        }
        public Dictionary<string, NMDesc> CaptureMemory()
        {
            lock(this)
            {
                var result = new Dictionary<string, NMDesc>();
                foreach (var i in Mems)
                {
                    var desc = new NMDesc();
                    desc.Count = i.Value.Count;
                    desc.Size = i.Value.Size;
                    result.Add(i.Key, desc);
                }
                return result;
            }
        }
        public Dictionary<string, NMDesc> GetDelta(Dictionary<string, NMDesc> lh, Dictionary<string, NMDesc> rh)
        {
            var result = new Dictionary<string, NMDesc>();
            foreach (var i in rh)
            {
                var desc = new NMDesc();
                NMDesc lhDesc;
                if(lh.TryGetValue(i.Key, out lhDesc)==false)
                {
                    desc.Count = i.Value.Count;
                    desc.Size = i.Value.Size;
                }
                else
                {
                    desc.Count = i.Value.Count - lhDesc.Count;
                    desc.Size = i.Value.Size - lhDesc.Size;
                }
                result.Add(i.Key, desc);
            }
            return result;
        }
    }
}
