using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Animation.Curve
{
    public enum WrapMode
    {
        Default = 0,
        Once = 1,
        Loop = 2,
        PingPong = 4,
        ClampForever = 8,
        Clamp = 1,

    }
    public struct CurveKey
    {
        float Time;
        float Value;
        float InSlope;
        float OutSlope;
        public CurveKey(float t, float v)
        {
            Time = t;
            Value = v;
            InSlope = 0.0F;
            OutSlope = 0.0F;
        }
        public CurveKey(float t, float v,float inSlope, float outSlope)
        {
            Time = t;
            Value = v;
            InSlope = inSlope;
            OutSlope = outSlope;
        }

    }
    //public struct CurveCache
    //{
    //    int Index;
    //    float Time;
    //    float TimeEnd;
    //    float[] Coeff;
    //};
    public class CGfxCurve: AuxCoreObject<CGfxCurve.NativePointer>//, EngineNS.IO.Serializer.ISerializer
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
        public CGfxCurve()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxCurve");
        }
        //public int AddKey(CurveKey key)
        //{
        //    return SDK_GfxCurve_AddKey(mCoreObject, key);
        //}
        //public int GetKeyCount()
        //{
        //    return SDK_GfxCurve_GetKeyCount(mCoreObject);
        //}
        //public float Evaluate(float time)
        //{
        //    return SDK_GfxCurve_Evaluate(mCoreObject,time);
        //}

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static int SDK_GfxCurve_AddKey(NativePointer self,CurveKey key);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static int SDK_GfxCurve_GetKeyCount(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static float SDK_GfxCurve_Evaluate(NativePointer self,float time);
        #endregion
    }
}
