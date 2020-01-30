using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Bricks.AI.BehaviorTree.Service
{
    public class ServiceBehavior
    {
        long mInterval = 1000;
        public long Interval
        {
            get =>mInterval;
            set
            {
                mInterval = value;
                mCurrentInterval = CalNextInterval();
            }
        }
        long mRandomDeviation = 200;
        public long RandomDeviation
        {
            get => mRandomDeviation;
            set
            {
                mRandomDeviation = value;
                mCurrentInterval = CalNextInterval();
            }
        }
        long mCurrentTick = 0;
        long mCurrentInterval = 0;
        long CurrentInterval
        {
            get
            {
                if(mCurrentInterval==0)
                    mCurrentInterval = CalNextInterval();
                return mCurrentInterval;
            }
        }
        public void Tick(long timeElapse, GamePlay.Actor.GCenterData context)
        {
            mCurrentTick += timeElapse;
            
            if(mCurrentTick% CurrentInterval != 0)
            {
                mCurrentTick = mCurrentTick / CurrentInterval;
                mCurrentInterval = CalNextInterval();
                Update(timeElapse,context);
            }
        }
        int CalNextInterval()
        {
            var start = (int)(Interval - RandomDeviation);
            if (start <= 0)
                start = 1;
            return MathHelper.RandomRange(start, (int)(Interval + RandomDeviation));
        }
        public virtual BehaviorStatus Update(long timeElapse, GCenterData context)
        {
            return BehaviorStatus.Success;
        }
    }
}
