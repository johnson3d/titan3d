using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    namespace Curve
    {
        public struct Keyframe
        {
            public float Time { get; set; }
            public float Value { get; set; }
            public float InSlope { get; set; }
            public float OutSlope { get; set; }
        }
        public struct CurveCache
        {
            public int Index { get; set; }
            public float Time { get; set; }
            public float TimeEnd { get; set; }
            public float Coeff0 { get; set; }
            public float Coeff1 { get; set; }
            public float Coeff2 { get; set; }
            public float Coeff3 { get; set; }
        }

        public class Curve : IO.BaseSerializer
        {
            [Rtti.Meta]
            public List<Keyframe> KeyFramesList { get; set; }
            public bool AddKeyframeBack(ref Keyframe keyframe)
            {
                return true;
            }
            public bool InsertKeyframe(uint index, ref Keyframe keyframe)
            {
                return true;
            }

            /// Evaluates the AnimationCurve caching the segment.
            public float Evaluate(float curveT, ref CurveCache animCurveCache)
            {
                var key = KeyFramesList[0];
                var cache = animCurveCache;
                return default;
            }
            public float EvaluateClamp(float curveT)
            {
                return default;
            }
        }
    }
}