using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    namespace Curve
    {
        [Rtti.Meta]
        public struct Keyframe
        {
            [Rtti.Meta]
            public float Time { get; set; }
            [Rtti.Meta]
            public float Value { get; set; }
            [Rtti.Meta]
            public float InSlope { get; set; }
            [Rtti.Meta]
            public float OutSlope { get; set; }
        }
        [Rtti.Meta]
        public struct CurveCache
        {
            [Rtti.Meta]
            public int Index { get; set; }
            [Rtti.Meta]
            public float Time { get; set; }
            [Rtti.Meta]
            public float TimeEnd { get; set; }
            [Rtti.Meta]
            public float Coeff0 { get; set; }
            [Rtti.Meta]
            public float Coeff1 { get; set; }
            [Rtti.Meta]
            public float Coeff2 { get; set; }
            [Rtti.Meta]
            public float Coeff3 { get; set; }
        }
        [Rtti.Meta]
        public class UCurve : IO.BaseSerializer
        {
            [Rtti.Meta]
            public List<Keyframe> KeyFramesList { get; set; } = new List<Keyframe>();
            public bool AddKeyframeBack(ref Keyframe keyframe)
            {
                //should check time and sort by  time
                if (!KeyFramesList.Contains(keyframe))
                {
                    KeyFramesList.Add(keyframe);
                    return true;
                }
                return false;
            }
            public bool InsertKeyframe(uint index, ref Keyframe keyframe)
            {
                //should check time and sort by  time
                KeyFramesList.Insert((int)index, keyframe);
                return true;
            }

            /// Evaluates the AnimationCurve caching the segment.
            public float Evaluate(float curveT, ref CurveCache animCurveCache)
            {
                int index = Math.Min(KeyFramesList.Count - 1, (int)Math.Truncate(curveT * 30));
                var key = KeyFramesList[index];
                var cache = animCurveCache;
                return key.Value;
            }
            public float EvaluateClamp(float curveT)
            {
                return default;
            }
        }
    }
}