using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Support;
using EngineNS.UI.Bind;
using EngineNS.UI.Event;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace EngineNS.UI.Animation
{
    public enum EAnimationType : byte
    {
        Automatic,
        From,
        To,
        By,
        FromTo,
        FromBy
    }

    public struct Duration
    {
        enum EDurationType
        {
            Automatic,
            TimeSpan,
            Forever,
        }
        EDurationType mDurationType;
        TimeSpan mTimeSpan;
        public TimeSpan TimeSpan => mTimeSpan;

        public static Duration Automatic
        {
            get
            {
                var duration = new Duration();
                duration.mDurationType = EDurationType.Automatic;
                return duration;
            }
        }
        public static Duration Forever
        {
            get
            {
                var duration = new Duration();
                duration.mDurationType = EDurationType.Forever;
                return duration;
            }
        }

        public bool HasTimeSpan => mDurationType == EDurationType.TimeSpan;

        public Duration(in TimeSpan timeSpan)
        {
            if (timeSpan >= TimeSpan.Zero)
            {
                mDurationType = EDurationType.TimeSpan;
                mTimeSpan = timeSpan;
            }
        }

        public static Duration operator +(in Duration t1, in Duration t2)
        {
            if (t1.HasTimeSpan && t2.HasTimeSpan)
                return new Duration(t1.mTimeSpan + t2.mTimeSpan);
            else if (t1.mDurationType != EDurationType.Automatic &&
                     t2.mDurationType != EDurationType.Automatic)
                return Duration.Forever;
            else
                return Duration.Automatic;
        }
        public static Duration operator -(in Duration t1, in Duration t2)
        {
            if (t1.HasTimeSpan && t2.HasTimeSpan)
                return new Duration(t1.mTimeSpan - t2.mTimeSpan);
            else if (t1.mDurationType == EDurationType.Forever &&
                    t2.HasTimeSpan)
                return Duration.Forever;
            else
            {
                // Forever - Forever
                // TimeSpan - Forever
                // TimeSpan - Automatic
                // Forever - Automatic
                // Automatic - Automatic
                // Automatic - Forever
                // Automatic - TimeSpan
                return Duration.Automatic;
            }
        }
        public static bool operator ==(in Duration t1, in Duration t2)
        {
            return t1.Equals(t2);
        }
        public static bool operator !=(in Duration t1, in Duration t2)
        {
            return !(t1.Equals(t2));
        }
        public static bool operator >(in Duration t1, in Duration t2)
        {
            if (t1.HasTimeSpan && t2.HasTimeSpan)
                return t1.mTimeSpan > t2.mTimeSpan;
            else if (t1.HasTimeSpan && t2.mDurationType == EDurationType.Forever)
                return false;
            else if (t1.mDurationType == EDurationType.Forever && t2.HasTimeSpan)
                return true;
            else
                return false;
        }
        public static bool operator >=(in Duration t1, in Duration t2)
        {
            if (t1.mDurationType == EDurationType.Automatic && t2.mDurationType == EDurationType.Automatic)
                return true;
            else if (t1.mDurationType == EDurationType.Automatic || t2.mDurationType == EDurationType.Automatic)
                return false;
            else
                return !(t1 < t2);
        }
        public static bool operator <(in Duration t1, in Duration t2)
        {
            if (t1.HasTimeSpan && t2.HasTimeSpan)
                return t1.mTimeSpan < t2.mTimeSpan;
            else if (t1.HasTimeSpan && t2.mDurationType == EDurationType.Forever)
                return true;
            else if (t1.mDurationType == EDurationType.Forever && t2.HasTimeSpan)
                return false;
            else
                return false;
        }
        public static bool operator <=(in Duration t1, in Duration t2)
        {
            if (t1.mDurationType == EDurationType.Automatic && t2.mDurationType == EDurationType.Automatic)
                return true;
            else if (t1.mDurationType == EDurationType.Automatic || t2.mDurationType == EDurationType.Automatic)
                return false;
            else
                return !(t1 > t2);
        }
        public static int Compare(in Duration t1, Duration t2)
        {
            if (t1.mDurationType == EDurationType.Automatic)
            {
                if (t2.mDurationType == EDurationType.Automatic)
                    return 0;
                else
                    return -1;
            }
            else if (t2.mDurationType == EDurationType.Automatic)
                return 1;
            else
            {
                if (t1 < t2)
                    return -1;
                else if (t1 > t2)
                    return 1;
                else
                    return 0;
            }
        }
        public Duration Add(in Duration duration)
        {
            return this + duration;
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null)
                return false;
            else if (obj is Duration)
                return Equals((Duration)obj);
            else
                return false;
        }
        public bool Equals(in Duration duration)
        {
            if (HasTimeSpan)
            {
                if (duration.HasTimeSpan)
                    return mTimeSpan == duration.mTimeSpan;
                else
                    return false;
            }
            else
                return mDurationType == duration.mDurationType;
        }
        public static bool Equals(in Duration t1, in Duration t2)
        {
            return t1.Equals(t2);
        }
        public override int GetHashCode()
        {
            if (HasTimeSpan)
                return mTimeSpan.GetHashCode();
            else
                return mDurationType.GetHashCode() + 17;
        }
        public Duration Subtract(in Duration duration)
        {
            return this - duration;
        }
        public override string ToString()
        {
            if (HasTimeSpan)
            {
                return mTimeSpan.ToString();
            }
            else if (mDurationType == EDurationType.Forever)
                return "Forever";
            else
                return "Automatic";
        }
        public void Parse(string value)
        {
            value = value.Trim();
            if (value == "Automatic")
                mDurationType = EDurationType.Automatic;
            else if (value == "Forever")
                mDurationType = EDurationType.Forever;
            else
                mTimeSpan = TimeSpan.Parse(value);
        }
    }

    public struct RepeatBeahavior
    {
        enum ERepeatBehaviorType
        {
            IterationCount,
            RepeatDuration,
            Forever,
        }
        float mIterationCount;
        TimeSpan mRepeatDuration;
        ERepeatBehaviorType mType;

        public RepeatBeahavior(float count)
        {
            if(float.IsInfinity(count) ||
               float.IsNaN(count) ||
               count < 0.0f)
            {
                throw new ArgumentOutOfRangeException("count", "RepeatBehavior constructor param error!");
            }

            mRepeatDuration = new TimeSpan(0);
            mIterationCount = count;
            mType = ERepeatBehaviorType.IterationCount;
        }
        public RepeatBeahavior(in TimeSpan duration)
        {
            if(duration < new TimeSpan(0))
            {
                throw new ArgumentOutOfRangeException("duration", "RepeatBehavior constructor param error!");
            }

            mIterationCount = 0.0f;
            mRepeatDuration = duration;
            mType = ERepeatBehaviorType.RepeatDuration;
        }
        public static RepeatBeahavior Forever
        {
            get
            {
                var forever = new RepeatBeahavior();
                forever.mType = ERepeatBehaviorType.Forever;
                return forever;
            }
        }
        public bool HasCount => mType == ERepeatBehaviorType.IterationCount;
        public bool HasDuration => mType == ERepeatBehaviorType.RepeatDuration;
        public float Count
        {
            get
            {
                if(mType != ERepeatBehaviorType.IterationCount)
                {
                    throw new InvalidOperationException("RepeatBeahavior not with iteration count!");
                }
                return mIterationCount;
            }
        }
        public TimeSpan Duration
        {
            get
            {
                if(mType != ERepeatBehaviorType.RepeatDuration)
                {
                    throw new InvalidOperationException("RepeatBeahavior not with repeat duration!");
                }
                return mRepeatDuration;
            }
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is RepeatBeahavior)
                return this.Equals((RepeatBeahavior)obj);
            else
                return false;
        }
        public bool Equals(in RepeatBeahavior repeatBeahavior)
        {
            if (mType == repeatBeahavior.mType)
            {
                switch(mType)
                {
                    case ERepeatBehaviorType.Forever:
                        return true;
                    case ERepeatBehaviorType.IterationCount:
                        return mIterationCount == repeatBeahavior.mIterationCount;
                    case ERepeatBehaviorType.RepeatDuration:
                        return mRepeatDuration == repeatBeahavior.mRepeatDuration;
                    default:
                        Debug.Fail("Unhandled ERepeatBehaviorType");
                        return false;
                }
            }
            else
                return false;
        }
        public static bool Equals(in RepeatBeahavior val1, in RepeatBeahavior val2)
        {
            return val1.Equals(in val2);
        }
        public override int GetHashCode()
        {
            switch(mType)
            {
                case ERepeatBehaviorType.IterationCount:
                    return mIterationCount.GetHashCode();
                case ERepeatBehaviorType.RepeatDuration:
                    return mRepeatDuration.GetHashCode();
                case ERepeatBehaviorType.Forever:
                    return int.MaxValue - 42;
                default:
                    Debug.Fail("Unhandled ERepeatBehaviorType");
                    return base.GetHashCode();
            }
        }
        public override string ToString()
        {
            switch(mType)
            {
                case ERepeatBehaviorType.Forever:
                    return "Forever";
                case ERepeatBehaviorType.IterationCount:
                    return "{" + mIterationCount + ":}x";
                case ERepeatBehaviorType.RepeatDuration:
                    return mRepeatDuration.ToString();
                default:
                    Debug.Fail("Unhandled ERepeatBehaviorType.");
                    return null;
            }
        }

        public static bool operator == (RepeatBeahavior val1, RepeatBeahavior val2)
        {
            return val1.Equals(in val2);
        }
        public static bool operator != (RepeatBeahavior val1, RepeatBeahavior val2)
        {
            return !val1.Equals(in val2);
        }
    }

    public class Timeline : IPooledObject
    {
        public bool IsAlloc { get; set; } = false;
        public TimeSpan CurrentTime = TimeSpan.Zero;
        public virtual void Reset() { }
        public virtual void Tick(float elapsedSecond) { }

        public virtual void Play()
        {
            TtEngine.Instance.UIManager.PlayTimeline(this);
        }
        public virtual void Stop()
        {
            TtEngine.Instance.UIManager.StopTimeline(this);
        }
    }


    [BindableObject]
    public partial class AnimationTimeline<T> : Timeline
    {
        [BindProperty]
        public string Name
        {
            get => GetValue<string>();
            set
            {
                SetValue(in value);
            }
        }

        [BindProperty]
        public TimeSpan BeginTime
        {
            get => GetValue<TimeSpan>();
            set
            {
                SetValue(value);
            }
        }

        [BindProperty]
        public Duration Duration
        {
            get => GetValue<Duration>();
            set
            {
                SetValue(in value);
            }
        }

        [BindProperty]
        public bool AutoReverse
        {
            get => GetValue<bool>();
            set
            {
                SetValue(in value);
            }
        }

        [BindProperty]
        public RepeatBeahavior RepeatBeahavior
        {
            get => GetValue<RepeatBeahavior>();
            set
            {
                SetValue(in value);
            }
        }

        public enum EFillBehavior
        {
            HoldEnd,
            HoldBegin,
            HoldBeginAndEnd,
            Stop,
        }
        [BindProperty]
        public EFillBehavior FillBehavior
        {
            get => GetValue<EFillBehavior>();
            set
            {
                SetValue(in value);
            }
        }

        [BindProperty(DefaultValue = 1.0f)]
        public float SpeedRatio
        {
            get => GetValue<float>();
            set
            {
                SetValue(value);
            }
        }

        public TtAnyValue UserData;

        public delegate void Delegate_OnCompleted(AnimationTimeline<T> animTimeline);
        public event Delegate_OnCompleted OnCompleted = null;

        public virtual T GetCurrentValue() { return default(T); }

        public override void Reset()
        {
            //no warning
            OnCompleted = null;
            if (OnCompleted != null)
            {
                OnCompleted(null);
            }
        }
    }
}

