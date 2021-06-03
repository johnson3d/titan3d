using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    namespace Curve
    {
        public struct TKeyframe<T, K> where T : unmanaged where K : unmanaged
        {
            public float Time { get; set; }
            public T Value { get; set; }
            public K InSlope { get; set; }
            public K OutSlope { get; set; }
        }
        public struct TCurveCache<K>
        {
            public int Index { get; set; }
            public float Time { get; set; }
            public float TimeEnd { get; set; }
            public K Coeff0 { get; set; }
            public K Coeff1 { get; set; }
            public K Coeff2 { get; set; }
            public K Coeff3 { get; set; }
        }

        public class TCurve<T, K> where T : unmanaged where K : unmanaged
        {
            public List<TKeyframe<T, K>> KeyFramesList { get; set; }
            public bool AddKeyframeBack(ref TKeyframe<T, K> keyframe)
            {
                return true;
            }
            public bool InsertKeyframe(uint index, ref TKeyframe<T, K> keyframe)
            {
                return true;
            }

            /// Evaluates the AnimationCurve caching the segment.
            public T Evaluate(float curveT, ref TCurveCache<K> animCurveCache)
            {
                var key = KeyFramesList[0];
                var cache = animCurveCache;
                return default;
            }
            public T EvaluateClamp(float curveT)
            {
                return default;
            }

            public bool LoadXnd(XndAttribute att)
            {
                att.BeginRead();
                int length = 0;
                att.Read(ref length);
                for (int i = 0; i < length; ++i)
                {
                    TKeyframe<T, K> keyframe = new TKeyframe<T, K>();
                    att.Read(ref keyframe);
                    KeyFramesList.Add(keyframe);
                }
                att.EndRead();
                return true;
            }

            public bool SaveXnd(XndAttribute att)
            {
                att.BeginWrite(1000);
                att.Write(KeyFramesList.Count);
                for (int i = 0; i < KeyFramesList.Count; ++i)
                {
                    att.Write(KeyFramesList[i]);
                }
                att.EndWrite();
                return true;
            }

        }
    }
}