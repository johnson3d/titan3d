using EngineNS.Animation.Curve;
using EngineNS.IO;
using EngineNS.RHI;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation
{
    namespace Curve
    {
        // curve can be a asset 
        public interface ICurve //: IO.IAsset
        {

        }

        public class Vector3Curve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Curve XCurve { get;set; } = null;
            [Rtti.Meta]
            public Curve YCurve { get; set; } = null;
            [Rtti.Meta]
            public Curve ZCurve { get; set; } = null;
            private CurveCache mXCache;
            [Rtti.Meta]
            public CurveCache XCache { get=>mXCache; set=>mXCache = value; }
            private CurveCache mYCache;
            [Rtti.Meta]
            public CurveCache YCache { get => mYCache; set => mYCache = value; }
            private CurveCache mZCache;
            [Rtti.Meta]
            public CurveCache ZCache { get => mZCache; set => mZCache = value; } 
            public Vector3 Evaluate(float time)
            {
                Vector3 temp = Vector3.Zero;
                temp.X = XCurve.Evaluate(time, ref mXCache);
                temp.Y = YCurve.Evaluate(time, ref mYCache);
                temp.Z = ZCurve.Evaluate(time, ref mZCache);
                return temp;
            }
          
        }
        public class FloatCurve : IO.BaseSerializer, ICurve
        {
            [Rtti.Meta]
            public Curve Curve { get; set; } = null;

            private CurveCache mCache;
            [Rtti.Meta]
            public CurveCache Cache { get=>mCache; set=>mCache = value; }
            public float Evaluate(float time)
            {
                return Curve.Evaluate(time, ref mCache);
            }

        }
    }
}
