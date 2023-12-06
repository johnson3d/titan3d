using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS
{
    [Rtti.Meta]
    public partial class BezierPointBase : EngineNS.IO.ISerializer
    {
        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        protected EngineNS.Vector2 mPosition = EngineNS.Vector2.Zero;
        [Rtti.Meta]
        public EngineNS.Vector2 Position
        {
            get => mPosition;
            set
            {
                mPosition = value;
            }
        }

        protected EngineNS.Vector2 mControlPoint = EngineNS.Vector2.Zero;
        [Rtti.Meta]
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
            if (bezierPtList.Count < 2)
                return 0;

            //var bezierMinX = bezierPtList[0].Position.X;
            //var bezierMaxX = bezierPtList[bezierPtList.Count - 1].Position.X;

            var xValueWithMinMax = xValue;  // 在MinX,MaxX之间的值
            var xValueWithBezier = xValue;  // 在贝塞尔曲线X范围的值
            if (!bLoopX)
            {
                if (xValue < MinX || xValue > MaxX)
                    return 0;

                xValueWithBezier = (xValue - MinX) / (MaxX - MinX) * (MaxBezierX - MinBezierX);
            }
            else
            {
                xValueWithMinMax = xValue % (MaxX - MinX) + MinX;
                xValueWithBezier = (xValueWithMinMax - MinX) / (MaxX - MinX) * (MaxBezierX - MinBezierX);
            }

            var pt = ValueOnBezier(bezierPtList, xValueWithBezier);

            return (pt.Y - MinBezierY) / (MaxBezierY - MinBezierY) * (MaxY - MinY) + MinY;
        }

        // xValue范围从bezierPtList起始点到结束点
        public static EngineNS.Vector2 ValueOnBezier(List<BezierPointBase> bezierPtList, double xValue, bool bAlongValue = false)
        {
            int i = 0;
            foreach (var pt in bezierPtList)
            {
                if (pt.Position.X > xValue)
                {
                    break;
                }
                i++;
            }

            EngineNS.Vector2 retPt;

            if (i == 0 || i >= bezierPtList.Count)
            {
                if(bAlongValue && bezierPtList.Count > 0)
                {
                    if (i == 0)
                        return bezierPtList[0].Position;
                    else if (i >= bezierPtList.Count)
                        return bezierPtList[bezierPtList.Count - 1].Position;
                }
                else
                    return EngineNS.Vector2.Zero;
            }

            var pt0 = bezierPtList[i - 1];
            var pt1 = bezierPtList[i];
            var t = (xValue - pt0.Position.X) / (pt1.Position.X - pt0.Position.X);

            //var cx = 3 * (pt0.ControlPoint.X - pt0.Position.X);
            //var bx = 3 * (pt1.ControlPoint.X - pt0.ControlPoint.X) - cx;
            //var ax = pt1.Position.X - pt0.Position.X - cx - bx;
            //var cy = 3 * (pt0.ControlPoint.Y - pt0.Position.Y);
            //var by = 3 * (pt1.ControlPoint.Y - pt0.ControlPoint.Y) - cy;
            //var ay = pt1.Position.Y - pt0.Position.Y - cy - by;
            //var tSquared = t * t;
            //var tCubed = tSquared * t;

            //var resultX = (ax * tCubed) + (bx * tSquared) + (cx * t) + pt0.Position.X;
            //var resultY = (ay * tCubed) + (by * tSquared) + (cy * t) + pt0.Position.Y;

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
