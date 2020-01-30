using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Mesh
{
   
    public class CGfxMdfQueue : AuxCoreObject<CGfxMdfQueue.NativePointer>
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
        public override string ToString()
        {
            string mdfStr = "";
            for (int i = 0; i < Modifiers.Count; i++)
            {
                mdfStr += Modifiers[i].Name;
            }
            return mdfStr;
        }
        public CGfxMdfQueue()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMdfQueue");
        }
        public CGfxMdfQueue(CRenderContext rc, NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();

            for (UInt32 i = 0; i < MdfNumber; i++)
            {
                var mdfPtr =  SDK_GfxMdfQueue_GetModifier(CoreObject, i);
                var mdfName = CGfxModifier.SDK_GfxModifier_GetName(mdfPtr);
                var mdfType = Rtti.RttiHelper.GetTypeFromSaveString(mdfName);

                //这里好像最好不要克隆，因为我们希望这种调用是CGfxMeshPrimitives加载的时候用，他就是表征vms，
                //而不是自己独立一份的数据
                //var mdfPtr = CGfxModifier.SDK_GfxModifier_CloneModifier(mdfPtr, rc.CoreObject);
                if (mdfPtr.GetPointer() == IntPtr.Zero)
                    continue;

                var modifier = System.Activator.CreateInstance(mdfType, new object[]{ mdfPtr }) as CGfxModifier;
                if (modifier == null)
                    continue;
                AddModifier(modifier);
            }
            UpdateHash64();
        }
        internal void UnsafeReInit(CRenderContext rc, NativePointer self)
        {
            Core_Release();
            mCoreObject = self;
            Core_AddRef();

            for (UInt32 i = 0; i < MdfNumber; i++)
            {
                var mdfPtr = SDK_GfxMdfQueue_GetModifier(CoreObject, i);
                var mdfName = CGfxModifier.SDK_GfxModifier_GetName(mdfPtr);
                var mdfType = Rtti.RttiHelper.GetTypeFromSaveString(mdfName);

                //这里好像最好不要克隆，因为我们希望这种调用是CGfxMeshPrimitives加载的时候用，他就是表征vms，
                //而不是自己独立一份的数据
                //var mdfPtr = CGfxModifier.SDK_GfxModifier_CloneModifier(mdfPtr, rc.CoreObject);
                if (mdfPtr.GetPointer() == IntPtr.Zero)
                    continue;

                var modifier = System.Activator.CreateInstance(mdfType, new object[] { mdfPtr }) as CGfxModifier;
                if (modifier == null)
                    continue;
                AddModifier(modifier);
            }
            UpdateHash64();
        }
        private Hash64 mHash64;
        private void UpdateHash64()
        {
            if (Modifiers.Count == 0)
            {
                mHash64 = Hash64.Empty;
                return;
            }
            string mdfStr = "";
            for (int i = 0; i < Modifiers.Count; i++)
            {
                mdfStr += Modifiers[i].Name;
            }
            Hash64.CalcHash64(ref mHash64, mdfStr);
        }
        public override Hash64 GetHash64()
        {
            return mHash64;
        }
        public string GetMdfQueueCaller()
        {
            if (Modifiers.Count == 0)
                return null;
            string result = "void MdfQueueDoModifiers(inout PS_INPUT vsOut, inout VS_INPUT vert)";
            result += "{";
            foreach (var i in Modifiers)
            {
                result += i.FunctionName + "(vsOut, vert);";
            }
            result += "}";
            return result;
        }
        public string[] GetShaderIncludes()
        {
            if (MdfNumber == 0)
                return null;

            string[] result = new string[Modifiers.Count];
            for (int i = 0; i < Modifiers.Count; i++)
            {
                result[i] = "@Engine/" + Modifiers[i].ShaderModuleName.GetNameWithType() + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension;
            }
            return result;
        }
        public string[] GetShaderDefines()
        {
            if(MdfNumber==0)
                return null;

            string[] result = new string[Modifiers.Count];
            for (int i = 0; i < Modifiers.Count; i++)
            {
                result[i] = "@Engine/" + Modifiers[i].ShaderModuleName.GetNameWithType() + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension;
            }
            return result;
        }
        public CGfxMesh HostMesh;
        public UInt32 MdfNumber
        {
            get
            {
                return SDK_GfxMdfQueue_GetMdfNumber(CoreObject);
            }
        }
        public void AddModifier(CGfxModifier modifier)
        {
            Modifiers.Add(modifier);
            SDK_GfxMdfQueue_AddModifier(CoreObject, modifier.CoreObject);
            UpdateHash64();

            HostMesh?.MdfQueueChanged();
        }
        public UInt32 FindModifier(string name)
        {
            for (UInt32 i = 0; i < Modifiers.Count; i++)
            {
                if (Modifiers[(int)i].Name == name)
                    return i;
            }
            return UInt32.MaxValue;
        }
        public void RemoveModifier(UInt32 index)
        {
            if (index >= Modifiers.Count)
                return;
            Modifiers.RemoveAt((int)index);
            SDK_GfxMdfQueue_RemoveModifier(CoreObject, index);
            UpdateHash64();

            HostMesh?.MdfQueueChanged();
        }
        public void ClearModifiers()
        {
            Modifiers.Clear();
            SDK_GfxMdfQueue_ClearModifiers(CoreObject);
            UpdateHash64();
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxMdfQueue), nameof(TickLogic));
        public void TickLogic(CRenderContext rc, CGfxMesh mesh, Int64 time)
        {
            ScopeTick.Begin();
            //SDK_GfxMdfQueue_TickLogic(CoreObject, rc.CoreObject, mesh.CoreObject);
            for (int i = 0; i < Modifiers.Count; i++)
            {
                var scope = Modifiers[i].GetTickTimeLogicScope();
                if (scope != null)
                    scope.Begin();
                Modifiers[i].TickLogic(rc, mesh, time);
                if (scope != null)
                    scope.End();
            }
            ScopeTick.End();
        }
        public void OnSetPassData(CPass pass, bool shadow)
        {
            for (int i = 0; i < Modifiers.Count; i++)
            {
                Modifiers[i].OnSetPassData(pass, shadow);
            }
        }
       
        public List<CGfxModifier> Modifiers
        {
            get;
        } = new List<CGfxModifier>();

        public TModifier FindModifier<TModifier>() where TModifier : CGfxModifier
        {
            foreach(var i in Modifiers)
            {
                if (i.GetType() == typeof(TModifier))
                    return (TModifier)i;
            }
            return null;
        }

        public CGfxMdfQueue CloneMdfQueue(CRenderContext rc, CGfxMdfQueue oldMdfQueue)
        {
            if (oldMdfQueue == null)
            {
                var mdfQueue = new CGfxMdfQueue();
                foreach (var i in Modifiers)
                {
                    var modifier = i.CloneModifier(rc);
                    mdfQueue.AddModifier(modifier);
                }
                return mdfQueue;
            }
            else
            {
                return oldMdfQueue;
            }
        }
        //Only For Import
        public void SyncNativeModifiers()
        {
            for(uint i =0; i<MdfNumber;++i)
            {
                Modifiers.Clear();
                var native = SDK_GfxMdfQueue_GetModifier(CoreObject, i);
                if (native.Pointer == IntPtr.Zero)
                    return;
                var rtti = SDK_VIUnknown_GetRtti(native.Pointer);
                if(Rtti.NativeRtti.SDK_CoreRtti_GetClassId(rtti) == CGfxSkinModifier.CoreClassId)
                {
                    var skinModifier = new CGfxSkinModifier(native);
                    skinModifier.AnimationPose = skinModifier.SkinSkeleton.CreateSkeletonPose();
                    Modifiers.Add(skinModifier);
                }
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMdfQueue_GetMdfNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMdfQueue_AddModifier(NativePointer self, CGfxModifier.NativePointer modifier);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMdfQueue_FindModifier(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMdfQueue_RemoveModifier(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMdfQueue_ClearModifiers(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxModifier.NativePointer SDK_GfxMdfQueue_GetModifier(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMdfQueue_TickLogic(NativePointer self, CRenderContext.NativePointer rc, CGfxMesh.NativePointer mesh, Int64 time);
        #endregion
    }
}
