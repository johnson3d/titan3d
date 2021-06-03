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

    public class UStreamingManager
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeUpdateStreamingState = Profiler.TimeScopeManager.GetTimeScope(typeof(UStreamingManager), nameof(UpdateStreamingState));
        public Dictionary<RName, IStreaming> StreamingAssets { get; } = new Dictionary<RName, IStreaming>();
        public void UpdateStreamingState()
        {
            using (new Profiler.TimeScopeHelper(ScopeUpdateStreamingState))
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

                    UpdateTargetLOD(i);

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
        public virtual void UpdateTargetLOD(IStreaming asset)
        {

        }
    }
}
