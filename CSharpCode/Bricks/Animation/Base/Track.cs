using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    namespace Curve
    {
        [Rtti.Meta]
        public struct FKeyframe
        {
            public FKeyframe()
            {
            }

            [Rtti.Meta]
            public float Time { get; set; } = 0;
            [Rtti.Meta]
            public float Value { get; set; } = 0;
            [Rtti.Meta]
            public float InSlope { get; set; } = 0;
            [Rtti.Meta]
            public float OutSlope { get; set; } = 0;
        }
        [Rtti.Meta]
        public struct FTrackCache
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
        public class TtTrack : IO.BaseSerializer
        {
            [Rtti.Meta]
            public List<FKeyframe> KeyFramesList { get; set; } = new List<FKeyframe>();
            public bool AddKeyframeBack(ref FKeyframe keyframe)
            {
                //should check time and sort by  time
                if (!KeyFramesList.Contains(keyframe))
                {
                    KeyFramesList.Add(keyframe);
                    return true;
                }
                return false;
            }
            public bool InsertKeyframe(uint index, ref FKeyframe keyframe)
            {
                //should check time and sort by  time
                KeyFramesList.Insert((int)index, keyframe);
                return true;
            }
            (int Index, float Time) GetCurrentIndexRange(float curveT)
            {
                int rightIndex = 0;
                float rightTime = 0;
                for (int i = 0; i < KeyFramesList.Count; ++i)
                {
                    if (KeyFramesList[i].Time > curveT)
                    {
                        rightIndex = i;
                        rightTime = KeyFramesList[i].Time;
                        break;
                    }
                }
                return (rightIndex, rightTime);
            }
            /// Evaluates the AnimationCurve caching the segment.
            public float Evaluate(float curveT, ref FTrackCache animCurveCache)
            {
                if (curveT <= KeyFramesList[0].Time)
                {
                    return KeyFramesList[0].Value;
                }
                if (curveT >= KeyFramesList[KeyFramesList.Count - 1].Time)
                {
                    return KeyFramesList[KeyFramesList.Count - 1].Value;
                }
                int rightIndex = 0;
                for (int i = 1; i < KeyFramesList.Count; ++i)
                {
                    if (KeyFramesList[i].Time > curveT)
                    {
                        rightIndex = i;
                        break;
                    }
                }

                var delta = KeyFramesList[rightIndex].Time - KeyFramesList[rightIndex - 1].Time;
                var percent = (curveT - KeyFramesList[rightIndex - 1].Time) / delta;
                var lerpValue = MathHelper.Lerp(KeyFramesList[rightIndex-1].Value, KeyFramesList[rightIndex].Value, percent);
                return lerpValue;
            }
            public (FKeyframe Left, FKeyframe Right) EvaluateFrame(float curveT, ref FTrackCache animCurveCache)
            {
                if (curveT <= KeyFramesList[0].Time)
                {
                    return (KeyFramesList[0], KeyFramesList[0]);
                }
                if (curveT >= KeyFramesList[KeyFramesList.Count - 1].Time)
                {
                    return (KeyFramesList[KeyFramesList.Count - 1], KeyFramesList[KeyFramesList.Count - 1]);
                }
                int rightIndex = 0;
                for (int i = 1; i < KeyFramesList.Count; ++i)
                {
                    if (KeyFramesList[i].Time > curveT)
                    {
                        rightIndex = i;
                        break;
                    }
                }
                return (KeyFramesList[rightIndex - 1], KeyFramesList[rightIndex]);
            }
            public float EvaluateClamp(float curveT)
            {
                return default;
            }
        }
    }
}