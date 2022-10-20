using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct PointF
    {
        public float _X;
        [Rtti.Meta]
        public float X
        {
            get { return _X; }
            set { _X = value; }
        }
        public float _Y;
        [Rtti.Meta]
        public float Y
        {
            get { return _Y; }
            set { _Y = value; }
        }

        static PointF mEmpty = new PointF(0.0f, 0.0f);
        [Rtti.Meta]
        public static PointF Empty
        {
            get { return mEmpty; }
        }

        public PointF(float x, float y)
        {
            _X = x;
            _Y = y;
        }

        public static float DistanceToLine(float pointX, float pointY, float lineX1, float lineY1, float lineX2, float lineY2)
        {
            var a = pointX - lineX1;
            var b = pointY - lineY1;
            var c = lineX2 - lineX1;
            var d = lineY2 - lineY1;

            var dot = a * c + b * d;
            var len_sq = c * c + d * d;
            var param = -1.0f;
            if (len_sq != 0)
                param = dot / len_sq;

            float xx, yy;
            if(param < 0)
            {
                xx = lineX1;
                yy = lineY1;
            }
            else if(param > 1)
            {
                xx = lineX2;
                yy = lineY2;
            }
            else
            {
                xx = lineX1 + param * c;
                yy = lineY1 + param * d;
            }

            var dx = pointX - xx;
            var dy = pointY - yy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
