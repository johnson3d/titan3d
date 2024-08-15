using EngineNS;
using EngineNS.EGui;
using EngineNS.Macross;
using EngineNS.UI.Animation;
using EngineNS.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI
{
    public partial class TtUIManager
    {
        Dictionary<Rtti.UTypeDesc, TtObjectPool<Timeline>> mTimelinePoolDic = new Dictionary<Rtti.UTypeDesc, TtObjectPool<Timeline>>();
        List<Timeline> mActivedTimelines = new List<Timeline>();

        public Timeline QueryTimelineSync(Rtti.UTypeDesc type)
        {
            TtObjectPool<Timeline> pool;
            if(!mTimelinePoolDic.TryGetValue(type, out pool))
            {
                pool = new TtObjectPool<Timeline>();
                mTimelinePoolDic[type] = pool;
            }
            return pool.QueryObjectSync();
        }
        public void ReleaseEventSync(Timeline timeline)
        {
            TtObjectPool<Timeline> pool;
            if(mTimelinePoolDic.TryGetValue(Rtti.UTypeDesc.TypeOf(timeline.GetType()), out pool))
            {
                pool.ReleaseObject(timeline);
                timeline.Reset();
            }
        }

        public void PlayTimeline(Timeline timeline)
        {
            TtEngine.Instance.EventPoster.RunOn(static (state) =>
            {
                var tl = state.UserArguments.Obj0 as Timeline;
                TtEngine.Instance.UIManager.mActivedTimelines.Add(tl);
                return true;
            }, Thread.Async.EAsyncTarget.Logic, timeline);
        }
        public void StopTimeline(Timeline timeline)
        {
            TtEngine.Instance.EventPoster.RunOn(static (state) =>
            {
                var tl = state.UserArguments.Obj0 as Timeline;
                TtEngine.Instance.UIManager.mActivedTimelines.Remove(tl);
                return true;
            }, Thread.Async.EAsyncTarget.Logic, timeline);
        }
        void TickTimeline(float elapsedSecond)
        {
            for(int i= mActivedTimelines.Count - 1; i>=0; i--)
            {
                mActivedTimelines[i].Tick(elapsedSecond);
            }
        }
    }
}