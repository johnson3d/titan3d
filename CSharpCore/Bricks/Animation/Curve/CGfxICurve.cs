using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Animation
{
    public enum CurveType
    {
        CT_Bool = 0,
        CT_Int = 1,
        CT_Float = 2,
        CT_Vector2 = 3,
        CT_Vector3 = 4,
        CT_Vector4 = 5,
        CT_Quaternion = 6,
        CT_Bone = 7,
        CT_Skeleton = 8,
        CT_Invalid = 9
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct CurveResult
    {
        [FieldOffset(0)]
        public CurveType Type;
        [FieldOffset(4)]
        public vBOOL BoolResult;
        [FieldOffset(4)]
        public int IntResult;
        [FieldOffset(4)]
        public float FloatResult;
        [FieldOffset(4)]
        public Vector2 Vector2Result;
        [FieldOffset(4)]
        public Vector3 Vector3Result;
        [FieldOffset(4)]
        public Quaternion QuaternionResult;
        [FieldOffset(4)]
        public Skeleton.CGfxBoneTransform BoneSRTResult;
    }
}
namespace EngineNS.Bricks.Animation.Curve
{
    public class CGfxICurve : AuxCoreObject<CGfxICurve.NativePointer>
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
        public CGfxICurve()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxICurve");
        }
        public CGfxICurve(string deriveClass)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>(deriveClass);
        }
        public CGfxICurve(NativePointer nativePointer)
        {
            mCoreObject = nativePointer;
        }
        public CurveResult Evaluate(float curveT,ref CurveResult result)
        {
            //CurveResult result = new CurveResult();
            unsafe
            {
                fixed (CurveResult* p = &result)
                {
                    SDK_GfxICurve_EvaluateNative(mCoreObject, curveT, p);
                }
            }
            return result;
        }
        public virtual CurveResult EvaluateClamp(float curveT)
        {
            return default(CurveResult);
        }
        public virtual uint GetKeyCount() { return SDK_GfxICurve_GetKeyCount(CoreObject); }
        public virtual bool IsValid() { return false; }
        public CurveType CurveType { get => mType; }
        protected CurveType mType = CurveType.CT_Invalid;

        public virtual async System.Threading.Tasks.Task<bool> Load(CRenderContext rc, IO.XndNode node)
        {
            await CEngine.Instance.EventPoster.Post(() =>
            {
                SyncLoad(rc, node);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public virtual bool SyncLoad(CRenderContext rc, IO.XndNode node)
        {
            return SDK_GfxICurve_LoadXnd(CoreObject, rc.CoreObject, node.CoreObject);
        }
        public virtual void Save(IO.XndNode node)
        {
            SDK_GfxICurve_Save2Xnd(CoreObject, node.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static uint SDK_GfxICurve_GetKeyCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxICurve_EvaluateNative(NativePointer self, float curveT, CurveResult* result);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxICurve_LoadXnd(NativePointer self, CRenderContext.NativePointer rc, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxICurve_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CurveType SDK_GfxICurve_GetCurveType(NativePointer self);
        #endregion
    }
}
