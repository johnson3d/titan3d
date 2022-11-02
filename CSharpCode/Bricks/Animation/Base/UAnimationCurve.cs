using EngineNS.Animation.Curve;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation
{
    namespace Curve
    {
        public struct NullableVector3
        {
            public Nullable<float> X;
            public Nullable<float> Y;
            public Nullable<float> Z;
            public static readonly NullableVector3 Empty = new NullableVector3();
            public static readonly NullableVector3 One = new NullableVector3() { X = 1.0f, Y = 1.0f, Z = 1.0f };
            public static NullableVector3 FromVector3(Vector3 value)
            {
                NullableVector3 nullableVector3;
                nullableVector3.X = value.X; nullableVector3.Y = value.Y; nullableVector3.Z = value.Z;
                return nullableVector3;
            }
        }
        [StructLayout(LayoutKind.Explicit, Size = 3)]
        public struct CurveValue
        {
            [FieldOffset(0)]
            public Nullable<float> FloatValue;
            [FieldOffset(0)]
            public NullableVector3 Vector3Value;

            public CurveValue(Nullable<float> floatValue)
            {
                Vector3Value = NullableVector3.Empty;
                FloatValue = floatValue;
            }
            public CurveValue(NullableVector3 nullableVector3Value)
            {
                FloatValue = 0;
                Vector3Value = nullableVector3Value;
            }
        }
        // curve can be a asset 
        [Rtti.Meta]
        public interface ICurve : IO.ISerializer
        {
            public Guid Id { get; set; }
           public CurveValue Evaluate(float time);
        }
        [Rtti.Meta]
        public class Vector3Curve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Guid Id { get; set; } = Guid.NewGuid();
            [Rtti.Meta]
            public UCurve XCurve { get;set; } = null;
            [Rtti.Meta]
            public UCurve YCurve { get; set; } = null;
            [Rtti.Meta]
            public UCurve ZCurve { get; set; } = null;
            private CurveCache mXCache;
            [Rtti.Meta]
            public CurveCache XCache { get=>mXCache; set=>mXCache = value; }
            private CurveCache mYCache;
            [Rtti.Meta]
            public CurveCache YCache { get => mYCache; set => mYCache = value; }
            private CurveCache mZCache;
            [Rtti.Meta]
            public CurveCache ZCache { get => mZCache; set => mZCache = value; }
            //public Vector3 Evaluate(float time)
            //{
            //    Vector3 temp = Vector3.Zero;
            //    temp.X = XCurve.Evaluate(time, ref mXCache);
            //    temp.Y = YCurve.Evaluate(time, ref mYCache);
            //    temp.Z = ZCurve.Evaluate(time, ref mZCache);
            //    return temp;
            //}

            public CurveValue Evaluate(float time)
            {
                NullableVector3 temp = NullableVector3.Empty;
                if (XCurve != null)
                    temp.X = XCurve.Evaluate(time, ref mXCache);
                if (YCurve != null)
                    temp.Y = YCurve.Evaluate(time, ref mYCache);
                if (ZCurve != null)
                    temp.Z = ZCurve.Evaluate(time, ref mZCache);
                CurveValue value = new CurveValue(temp);
                return value;
            }
        }
        public class FloatCurve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Guid Id { get; set; } = Guid.NewGuid();
            [Rtti.Meta]
            public UCurve Curve { get; set; } = null;

            private CurveCache mCache;
            [Rtti.Meta]
            public CurveCache Cache { get=>mCache; set=>mCache = value; }
            //public float Evaluate(float time)
            //{
            //    return Curve.Evaluate(time, ref mCache);
            //}
            public CurveValue Evaluate(float time)
            {
                Nullable<float> temp = null;
                if (Curve != null)
                    temp = Curve.Evaluate(time, ref mCache);
                CurveValue value = new CurveValue(temp);
                return value;
            }
        }
    }
}
