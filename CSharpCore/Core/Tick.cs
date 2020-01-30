using System;
using System.Collections.Generic;

namespace EngineNS
{
    public interface ILogicTick
    {
        bool EnableTick
        {
            get;
            set;
        }
        void TickLogic();
    }
    public interface ITickInfo
    {
        void BeforeFrame();
        void TickLogic();
        void TickRender();
        void TickSync();
        bool EnableTick
        {
            get;
            set;
        }
        Profiler.TimeScope GetLogicTimeScope();
    }

    public class TickInfoManager
    {
        public void FinalCleanup()
        {
            
        }

        List<ITickInfo> mTickInfosList = new List<ITickInfo>();
        public void BeforeFrame()
        {
            for (int i = mTickInfosList.Count - 1; i >= 0; i--)
            {
                if (mTickInfosList[i].EnableTick)
                    mTickInfosList[i].BeforeFrame();
            }
        }
        public void TickLogic()
        {
            for (int i = mTickInfosList.Count - 1; i >= 0; i--)
            {
                if (mTickInfosList[i].EnableTick)
                {
                    var scope = mTickInfosList[i].GetLogicTimeScope();
                    if (scope != null)
                        scope.Begin();
                    mTickInfosList[i].TickLogic();
                    if (scope != null)
                        scope.End();
                }
            }
            DoTickUntil();
        }
        public void TickRender()
        {
            for (int i = mTickInfosList.Count - 1; i >= 0; i--)
            {
                if (mTickInfosList[i].EnableTick)
                    mTickInfosList[i].TickRender();
            }
        }
        public void TickSync()
        {
            for (int i = mTickInfosList.Count - 1; i >= 0; i--)
            {
                if (mTickInfosList[i].EnableTick)
                    mTickInfosList[i].TickSync();
            }
        }
        public int GetTickableNum()
        {
            return mTickInfosList.Count;
        }

        public void AddTickInfo(ITickInfo tickInfo)
        {
            if (!mTickInfosList.Contains(tickInfo))
            {
                tickInfo.EnableTick = true;
                mTickInfosList.Add(tickInfo);
            }
        }

        public void RemoveTickInfo(ITickInfo tickInfo)
        {
            mTickInfosList.Remove(tickInfo);
            tickInfo.EnableTick = false;
        }
        public delegate bool FTickUntil();
        private List<FTickUntil> mTickUntils = new List<FTickUntil>();

        //返回false就不再做了
        public void AddTickUntil(FTickUntil action)
        {
            mTickUntils.Add(action);
        }
        private void DoTickUntil()
        {
            for (int i = 0; i < mTickUntils.Count; i++)
            {
                if(mTickUntils[i]()==false)
                {
                    mTickUntils.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
