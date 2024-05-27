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
        System.Threading.Tasks.Task<bool> CurLoadTask { get; set; }
        System.Threading.Tasks.Task<bool> LoadLOD(int level);
    }

    public class TtStreamingManager
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeUpdateStreamingState = Profiler.TimeScopeManager.GetTimeScope(typeof(TtStreamingManager), nameof(UpdateStreamingState));
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
                            if (i.CurLoadTask.IsCompleted == false)
                                continue;
                            else
                                i.CurLoadTask = null;
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
        public virtual bool UpdateTargetLOD(IStreaming asset)
        {
            return true;
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
