using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class ULevelStreaming
    {
        ~ULevelStreaming()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            lock (this)
            {
                StreamingLevels.Clear();

                foreach (var i in UnloadingLevels)
                {
                    if (i.LevelData == null)
                        continue;

                    i.LevelData?.Cleanup();
                    i.LevelData = null;
                }
                UnloadingLevels.Clear();
            }
        }
        public HashSet<UTerrainLevel> StreamingLevels = new HashSet<UTerrainLevel>();
        public List<UTerrainLevel> UnloadingLevels = new List<UTerrainLevel>();
        public void PushStreamingLevel(UTerrainLevel level, bool bForce)
        {
            lock (this)
            {
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
                        StreamingLevels.Remove(level);
                        var LevelData = new UTerrainLevelData();
                        var task1 = LevelData.CreateLevelData(level, bForce);
                        task1.Wait();
                        level.LevelData = LevelData;
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "LevelStreaming", $"({level.LevelX},{level.LevelX})Level ForceLoad in async streaming == 1");
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
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "LevelStreaming", $"({level.LevelX},{level.LevelX})Level ForceLoad in async streaming == 2");
            }

            lock (this)
            {
                StreamingLevels.Remove(level);
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
                            tmp.LevelData?.Cleanup();
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
