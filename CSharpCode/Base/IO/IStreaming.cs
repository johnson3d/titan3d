using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public interface IStreaming
    {
        RName AssetName { get; }
        int LevelOfDetail { get; set; }
        int TargetLOD { get; }
        int MaxLOD { get; }
        Thread.Async.TtTask<bool>? CurLoadTask { get; set; }
        Thread.Async.TtTask<bool> LoadLOD(int level);
    }

    public class TtStreamingManager
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScopeUpdateStreamingState;
        private static Profiler.TimeScope ScopeUpdateStreamingState
        {
            get
            {
                if (mScopeUpdateStreamingState == null)
                    mScopeUpdateStreamingState = new Profiler.TimeScope(typeof(TtStreamingManager), nameof(UpdateStreamingState));
                return mScopeUpdateStreamingState;
            }
        }
        
        public Dictionary<RName, IStreaming> StreamingAssets { get; } = new Dictionary<RName, IStreaming>();
        public void UpdateStreamingState()
        {
            using (new Profiler.TimeScopeHelper(ScopeUpdateStreamingState))
            {
                lock (StreamingAssets)
                {
                    foreach (var i in StreamingAssets.Values)
                    {
                        if (i.CurLoadTask != null)
                        {
                            if (i.CurLoadTask.Value.IsCompleted == false)
                                continue;
                            else
                            {
                                i.CurLoadTask.Value.Dispose();
                                i.CurLoadTask = null;
                            }   
                        }

                        if (false == UpdateTargetLOD(i))
                        {
                            continue;
                        }

                        if (i.LevelOfDetail == i.TargetLOD)
                        {
                            continue;
                        }
                        else if (i.LevelOfDetail < i.TargetLOD)
                        {
                            i.LevelOfDetail++;
                            i.CurLoadTask = i.LoadLOD(i.LevelOfDetail);
                        }
                        else
                        {
                            i.LevelOfDetail = i.TargetLOD;
                            i.CurLoadTask = i.LoadLOD(i.LevelOfDetail);
                        }
                    }
                }   
            }
        }
        public unsafe virtual bool UpdateTargetLOD(IO.IStreaming asset)
        {
            return true;
        }
        public int GetNeedStreamingNumber()
        {
            int count = 0;
            foreach (var i in StreamingAssets.Values)
            {
                if (i.LevelOfDetail == i.TargetLOD)
                {
                    continue;
                }
                count++;
            }
            return count;
        }
        internal IStreaming UnsafeRemove(RName name)
        {
            lock (StreamingAssets)
            {
                if (StreamingAssets.TryGetValue(name, out var result))
                {
                    return result;
                }
                return null;
            }
        }
        internal void UnsafeAdd(RName name, IStreaming obj)
        {
            lock (StreamingAssets)
            {
                StreamingAssets.Add(name, obj);
            }
        }
    }
}
