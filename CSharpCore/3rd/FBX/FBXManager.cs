using EngineNS;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FBX
{
    class FBXManager: AuxCoreObject<FBXManager.NativePointer>
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

        static FBXManager mInstance = null;
        public static FBXManager Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new FBXManager();
                }
                return mInstance;
            }
        }

        Dictionary<string, FBXAnalyzer> mAnalyzers = new Dictionary<string, FBXAnalyzer>();
         private FBXManager()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::FBXManager");
            SDK_FBXManager_Init(mCoreObject);
        }

        public override void Cleanup()
        {
            foreach(var pair in mAnalyzers)
            {
                pair.Value.Cleanup();
            }
            mAnalyzers.Clear();
            base.Cleanup();
        }

        public FBXAnalyzer GetFBXAnalyzer(string file)
        {
            var fbxAnalyzer = new FBX.FBXAnalyzer();
            var reslut = fbxAnalyzer.LoadFile(mInstance, file);
            if (reslut == false)
            {
                //Log(file load failed)
                return null;
            }
            AddAnalyzer(file, fbxAnalyzer);
            return fbxAnalyzer;
        }

        void AddAnalyzer(string file, FBXAnalyzer analyzer)
        {
            if(mAnalyzers.ContainsKey(file))
            {
                var temp = mAnalyzers[file];
                mAnalyzers[file] = analyzer;
                temp.Cleanup();
            }
            else
            {
                mAnalyzers.Add(file, analyzer);
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EngineNS.vBOOL SDK_FBXManager_Init(NativePointer self);
        #endregion
    }
}
