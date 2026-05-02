using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class ULevelStreaming : IDisposable
    {
        ~ULevelStreaming()
        {
            Dispose();
        }
        public void Dispose()
        {
            lock (this)
            {
                IsStreaming = false;
                StreamingLevels.Clear();

                foreach (var i in UnloadingLevels)
                {
                    if (i.LevelData == null)
                        continue;

                    i.LevelData?.Dispose();
                    i.LevelData = null;
                }
                UnloadingLevels.Clear();
            }
        }
        protected bool IsStreaming = true;
        public HashSet<UTerrainLevel> StreamingLevels = new HashSet<UTerrainLevel>();
        public List<UTerrainLevel> UnloadingLevels = new List<UTerrainLevel>();
        public void PushStreamingLevel(UTerrainLevel level, bool bForce)
        {
            lock (this)
            {
                if (IsStreaming == false)
                    return;
                if (UnloadingLevels.Contains(level))
                {
                    UnloadingLevels.Remove(level);
                    if (level.LevelData != null)
                        return;
                }

                if (level.LevelData != null)
                    return;
                if (StreamingLevels.Contains(level))
                {
                    if (bForce)
                    {
                        var t1 = Support.TtTime.GetTickCount();
                        while (level.LevelData != null)
                        {
                            var t2 = Support.TtTime.GetTickCount();
                            if (t2 - t1 > 1000 * 15)
                            {
                                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelX}, force = true) time out");
                                return;
                            }
                            System.Threading.Thread.Sleep(10);
                        }
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelX})Level ForceLoad in async streaming == 1");
                    }
                    return;
                }
                StreamingLevels.Add(level);

                var task = CreateLevelData(level, bForce);
            }
        }
        public async System.Threading.Tasks.Task CreateLevelData(UTerrainLevel level, bool bForce)
        {
            var LevelData = new UTerrainLevelData();
            await LevelData.CreateLevelData(level, bForce);
            if (level.LevelData == null)
            {
                level.LevelData = LevelData;
            }
            else
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelX})Level ForceLoad in async streaming == 2");
            }

            lock (this)
            {
                StreamingLevels.Remove(level);
                if (IsStreaming == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelX}): IsStreaming == false");
                    level.LevelData?.Dispose();
                    level.LevelData = null;
                }
            }
        }
        public void PushUnloadLevel(UTerrainLevel level)
        {
            lock(this)
            {
                if (level.LevelData == null)
                    return;
                if (UnloadingLevels.Contains(level))
                    return;
                level.LevelData.RemainUnloadTime = 10.0f;
                UnloadingLevels.Add(level);
            }
        }
        public void Tick(float elapsedSecond)
        {
            lock (this)
            {
                for (int i = 0; i < UnloadingLevels.Count; i++)
                {
                    var tmp = UnloadingLevels[i];
                    if (tmp.LevelData == null)
                    {
                        UnloadingLevels.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        tmp.LevelData.RemainUnloadTime -= elapsedSecond;
                        if (tmp.LevelData.RemainUnloadTime < 0)
                        {
                            tmp.LevelData?.Dispose();
                            tmp.LevelData = null;
                            
                            UnloadingLevels.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}
