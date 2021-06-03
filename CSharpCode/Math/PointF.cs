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
    }
}
