using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS
{
    [Rtti.Meta("")]
    public partial class BezierPointBase : EngineNS.IO.BaseSerializer
    {
        protected EngineNS.Vector2 mPosition = EngineNS.Vector2.Zero;
        [Rtti.Meta("")]
        public EngineNS.Vector2 Position
        {
            get => mPosition;
            set
            {
                mPosition = value;
            }
        }

        protected EngineNS.Vector2 mControlPoint = EngineNS.Vector2.Zero;
        [Rtti.Meta("")]
        public EngineNS.Vector2 ControlPoint
        {
            get => mControlPoint;
            set
            {
                mControlPoint = value;
            }
        }

        public BezierPointBase()
        {

        }
        public BezierPointBase(in EngineNS.Vector2 pos, in EngineNS.Vector2 ctrlPt)
        {
            Position = pos;
            ControlPoint = ctrlPt;
        }
        public override string ToString()
        {
            string result = "";
            result += Position.ToString();
            result += ControlPoint.ToString();
            return result;
        }
    }
    public class BezierCalculate
    {
        public static double ValueOnBezier(List<BezierPointBase> bezierPtList, double xValue,
                                                                double MinX, double MaxX,
                                                                double MinY, double MaxY,
                                                                double MinBezierX, double MaxBezierX,
                                                                double MinBezierY, double MaxBezierY,
                                                                bool bLoopX)
        {
            if (bezierPtList == null || bezierPtList.Count < 2 ||
                !double.IsFinite(xValue) || !double.IsFinite(MinX) || !double.IsFinite(MaxX) ||
                !double.IsFinite(MinY) || !double.IsFinite(MaxY) ||
                !double.IsFinite(MinBezierX) || !double.IsFinite(MaxBezierX) ||
                !double.IsFinite(MinBezierY) || !double.IsFinite(MaxBezierY))
                return 0;

            double inputRange = MaxX - MinX;
            double bezierRangeX = MaxBezierX - MinBezierX;
            double bezierRangeY = MaxBezierY - MinBezierY;
            if (Math.Abs(inputRange) <= double.Epsilon || Math.Abs(bezierRangeY) <= double.Epsilon)
                return 0;

            double mappedX;
            if (!bLoopX)
            {
                if (xValue < MinX || xValue > MaxX)
                    return 0;
                mappedX = (xValue - MinX) / inputRange * bezierRangeX + MinBezierX;
            }
            else
            {
                double wrapped = (xValue - MinX) % inputRange;
                if (wrapped < 0)
                    wrapped += inputRange;
                mappedX = wrapped / inputRange * bezierRangeX + MinBezierX;
            }

            var pt = ValueOnBezier(bezierPtList, mappedX);
            return (pt.Y - MinBezierY) / bezierRangeY * (MaxY - MinY) + MinY;
        }

        // xValue范围从bezierPtList起始点到结束点
        public static EngineNS.Vector2 ValueOnBezier(List<BezierPointBase> bezierPtList, double xValue, bool bAlongValue = false)
        {
            if (bezierPtList == null || bezierPtList.Count < 2 || !double.IsFinite(xValue))
                return EngineNS.Vector2.Zero;

            int pairedCount = bezierPtList.Count & ~1;
            if (pairedCount < 2)
                return EngineNS.Vector2.Zero;
            var first = bezierPtList[0];
            var last = bezierPtList[pairedCount - 1];
            if (!IsFinite(first) || !IsFinite(last))
                return EngineNS.Vector2.Zero;

            if (xValue < first.Position.X)
                return bAlongValue ? first.Position : EngineNS.Vector2.Zero;
            if (xValue > last.Position.X)
                return bAlongValue ? last.Position : EngineNS.Vector2.Zero;

            for (int i = 0; i + 1 < pairedCount; i += 2)
            {
                var pt0 = bezierPtList[i];
                var pt1 = bezierPtList[i + 1];
                if (!IsFinite(pt0) || !IsFinite(pt1))
                    return EngineNS.Vector2.Zero;

                double minX = Math.Min(pt0.Position.X, pt1.Position.X);
                double maxX = Math.Max(pt0.Position.X, pt1.Position.X);
                if (xValue < minX || xValue > maxX)
                    continue;

                if (!TrySolveBezierX(pt0, pt1, (float)xValue, out float t))
                    return Math.Abs(xValue - pt0.Position.X) <= Math.Abs(xValue - pt1.Position.X) ?
                        pt0.Position : pt1.Position;
                return ValueOnBezierSegment(pt0, pt1, t);
            }

            return bAlongValue ?
                (xValue <= first.Position.X ? first.Position : last.Position) : EngineNS.Vector2.Zero;
        }

        static bool TrySolveBezierX(BezierPointBase pt0, BezierPointBase pt1, float xValue, out float t)
        {
            t = 0.0f;
            float x0 = pt0.Position.X;
            float x1 = pt1.Position.X;
            float range = x1 - x0;
            float tolerance = Math.Max(1e-6f, Math.Abs(range) * 1e-5f);
            if (Math.Abs(range) <= tolerance)
                return Math.Abs(xValue - x0) <= tolerance;

            float low = 0.0f;
            float high = 1.0f;
            t = MathHelper.Clamp((xValue - x0) / range, 0.0f, 1.0f);
            bool ascending = range > 0.0f;
            for (int iteration = 0; iteration < 20; iteration++)
            {
                float value = ValueOnBezierSegment(pt0, pt1, t).X;
                float error = value - xValue;
                if (!float.IsFinite(value) || !float.IsFinite(error))
                    return false;
                if (Math.Abs(error) <= tolerance)
                    return true;

                if ((error < 0.0f) == ascending)
                    low = t;
                else
                    high = t;

                float derivative = BezierDerivativeX(pt0, pt1, t);
                float candidate = float.NaN;
                if (float.IsFinite(derivative) && Math.Abs(derivative) > 1e-7f)
                    candidate = t - error / derivative;
                if (!float.IsFinite(candidate) || candidate <= low || candidate >= high)
                    candidate = (low + high) * 0.5f;
                t = candidate;
            }
            t = (low + high) * 0.5f;
            return true;
        }

        static float BezierDerivativeX(BezierPointBase pt0, BezierPointBase pt1, float t)
        {
            float oneMinusT = 1.0f - t;
            return 3.0f * ((pt0.ControlPoint.X - pt0.Position.X) * oneMinusT * oneMinusT +
                2.0f * (pt1.ControlPoint.X - pt0.ControlPoint.X) * oneMinusT * t +
                (pt1.Position.X - pt1.ControlPoint.X) * t * t);
        }

        static bool IsFinite(BezierPointBase point)
        {
            return point != null && float.IsFinite(point.Position.X) && float.IsFinite(point.Position.Y) &&
                float.IsFinite(point.ControlPoint.X) && float.IsFinite(point.ControlPoint.Y);
        }

        public static Vector2 ValueOnBezierSegment(BezierPointBase pt0, BezierPointBase pt1, float t)
        {
            EngineNS.Vector2 retPt;

            var yt = 1 - t;
            retPt.X = (float)(pt0.Position.X * yt * yt * yt +
                      3 * pt0.ControlPoint.X * yt * yt * t +
                      3 * pt1.ControlPoint.X * yt * t * t +
                      pt1.Position.X * t * t * t);
            retPt.Y = (float)(pt0.Position.Y * yt * yt * yt +
                          3 * pt0.ControlPoint.Y * yt * yt * t +
                          3 * pt1.ControlPoint.Y * yt * t * t +
                          pt1.Position.Y * t * t * t);

            return retPt;
        }
    }
}
