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
        public struct FNullableVector3
        {
            public Nullable<float> X { get; set; } = null;
            public Nullable<float> Y { get; set; } = null;
            public Nullable<float> Z { get; set; } = null;
            public static readonly FNullableVector3 Empty = new FNullableVector3();
            public static readonly FNullableVector3 One = new FNullableVector3() { X = 1.0f, Y = 1.0f, Z = 1.0f };
            public FNullableVector3()
            {

            }
            public static FNullableVector3 FromRotator(in FRotator value)
            {
                FNullableVector3 nullableVector3 = new FNullableVector3();
                nullableVector3.X = value.Yaw; nullableVector3.Y = value.Pitch; nullableVector3.Z = value.Roll;
                return nullableVector3;
            }
            public static FNullableVector3 FromVector3(in Vector3 value)
            {
                FNullableVector3 nullableVector3 = new FNullableVector3();
                nullableVector3.X = value.X; nullableVector3.Y = value.Y; nullableVector3.Z = value.Z;
                return nullableVector3;
            }

            public static FNullableVector3 operator* (FNullableVector3 nullableVector3Value, float floatValue)
            {
                FNullableVector3 result = nullableVector3Value;
                if(result.X.HasValue)
                {
                    result.X = result.X.Value * floatValue;
                }
                if (result.Y.HasValue)
                {
                    result.Y = result.Y.Value * floatValue;
                }
                if (result.Z.HasValue)
                {
                    result.Z = result.Z.Value * floatValue;
                }
                return result;
            }
        }
        [StructLayout(LayoutKind.Explicit, Size = 3)]
        public struct FCurveValue
        {
            [FieldOffset(0)]
            public Nullable<float> FloatValue;
            [FieldOffset(0)]
            public FNullableVector3 Vector3Value;

            public FCurveValue(Nullable<float> floatValue)
            {
                Vector3Value = FNullableVector3.Empty;
                FloatValue = floatValue;
            }
            public FCurveValue(FNullableVector3 nullableVector3Value)
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
           public FCurveValue Evaluate(float time);
        }
        [Rtti.Meta]
        public class TtVector3Curve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Guid Id { get; set; } = Guid.NewGuid();
            [Rtti.Meta]
            public TtTrack XTrack { get;set; } = null;
            [Rtti.Meta]
            public TtTrack YTrack { get; set; } = null;
            [Rtti.Meta]
            public TtTrack ZTrack { get; set; } = null;
            private FTrackCache mXCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache XCache { get=>mXCache; set=>mXCache = value; }
            private FTrackCache mYCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache YCache { get => mYCache; set => mYCache = value; }
            private FTrackCache mZCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache ZCache { get => mZCache; set => mZCache = value; }
            //public Vector3 Evaluate(float time)
            //{
            //    Vector3 temp = Vector3.Zero;
            //    temp.X = XCurve.Evaluate(time, ref mXCache);
            //    temp.Y = YCurve.Evaluate(time, ref mYCache);
            //    temp.Z = ZCurve.Evaluate(time, ref mZCache);
            //    return temp;
            //}

            public FCurveValue Evaluate(float time)
            {
                FNullableVector3 temp = FNullableVector3.Empty;
                if (XTrack != null)
                    temp.X = XTrack.Evaluate(time, ref mXCache);
                if (YTrack != null)
                    temp.Y = YTrack.Evaluate(time, ref mYCache);
                if (ZTrack != null)
                    temp.Z = ZTrack.Evaluate(time, ref mZCache);
                FCurveValue value = new FCurveValue(temp);
                return value;
            }
        }
        [Rtti.Meta]
        public class TtQuaternionCurve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Guid Id { get; set; } = Guid.NewGuid();
            [Rtti.Meta]
            public TtTrack XTrack { get; set; } = null;
            [Rtti.Meta]
            public TtTrack YTrack { get; set; } = null;
            [Rtti.Meta]
            public TtTrack ZTrack { get; set; } = null;
            [Rtti.Meta]
            public TtTrack WTrack { get; set; } = null;
            private FTrackCache mXCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache XCache { get => mXCache; set => mXCache = value; }
            private FTrackCache mYCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache YCache { get => mYCache; set => mYCache = value; }
            private FTrackCache mZCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache ZCache { get => mZCache; set => mZCache = value; }
            private FTrackCache mWCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache WCache { get => mWCache; set => mWCache = value; }

            public FCurveValue Evaluate(float time)
            {
                System.Diagnostics.Debug.Assert(XTrack != null && YTrack != null && ZTrack != null && WTrack != null);
                var xResult = XTrack.EvaluateFrame(time, ref mXCache);
                var yResult = YTrack.EvaluateFrame(time, ref mYCache);
                var zResult = ZTrack.EvaluateFrame(time, ref mZCache);
                var wResult = WTrack.EvaluateFrame(time, ref mWCache);
                System.Diagnostics.Debug.Assert(xResult.Left.Time == yResult.Left.Time && yResult.Left.Time == zResult.Left.Time && zResult.Left.Time == wResult.Left.Time);
                System.Diagnostics.Debug.Assert(xResult.Right.Time == yResult.Right.Time && yResult.Right.Time == zResult.Right.Time && zResult.Right.Time == wResult.Right.Time);

                Quaternion leftQuat = new Quaternion(xResult.Left.Value, yResult.Left.Value, zResult.Left.Value, wResult.Left.Value);
                if(xResult.Left.Time == xResult.Right.Time)
                {
                    FNullableVector3 temp = FNullableVector3.FromRotator(leftQuat.ToEuler());
                    FCurveValue value = new FCurveValue(temp);
                    return value;
                }
                else
                {
                    Quaternion rightQuat = new Quaternion(xResult.Right.Value, yResult.Right.Value, zResult.Right.Value, wResult.Right.Value);
                    var duration = xResult.Right.Time - xResult.Left.Time;
                    Quaternion lerped = Quaternion.Lerp(leftQuat, rightQuat, (time - xResult.Left.Time) / duration);
                    FNullableVector3 temp = FNullableVector3.FromRotator(lerped.ToEuler());
                    FCurveValue value = new FCurveValue(temp);
                    return value;
                }
                
            }
        }
        public class TtFloatCurve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Guid Id { get; set; } = Guid.NewGuid();
            [Rtti.Meta]
            public TtTrack Track { get; set; } = null;

            private FTrackCache mCache = new FTrackCache() { Index = 0 };
            [Rtti.Meta]
            public FTrackCache Cache { get=>mCache; set=>mCache = value; }
            //public float Evaluate(float time)
            //{
            //    return Curve.Evaluate(time, ref mCache);
            //}
            public FCurveValue Evaluate(float time)
            {
                Nullable<float> temp = null;
                if (Track != null)
                    temp = Track.Evaluate(time, ref mCache);
                FCurveValue value = new FCurveValue(temp);
                return value;
            }
        }
    }
}
