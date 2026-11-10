using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    /// <summary>
    /// level LevelData 的加载/卸载队列。
    ///
    /// 除了队列本身, 这里还负责维护每个 level 的 <see cref="ELevelDataState"/>:
    /// 状态只在下面的转移点写入, 外部(goto / 编辑器网格)只读, 避免多处各判一套条件导致不一致。
    ///
    ///   入队加载            → Loading
    ///   异步任务完成回写    → Loaded  (在飞期间被 Dispose 则 → Unloaded)
    ///   PushUnloadLevel     → PendingDestroy  (还在队列里倒计时, LevelData 依然有效, 可以捞回来直接复用)
    ///   Tick 到期开始 Dispose → Destroying (结束后 → Unloaded)
    /// </summary>
    public class TtLevelStreaming : IDisposable
    {
        ~TtLevelStreaming()
        {
            Dispose();
        }
        public void Dispose()
        {
            lock (this)
            {
                IsStreaming = false;

                foreach (var i in StreamingLevels)
                {
                    // 在飞任务完成后会走 CreateLevelData 的 IsStreaming == false 分支丢弃结果
                    i?.SetDataState(ELevelDataState.Unloaded);
                }
                StreamingLevels.Clear();

                foreach (var i in UnloadingLevels)
                {
                    if (i.LevelData == null)
                    {
                        i.SetDataState(ELevelDataState.Unloaded);
                        continue;
                    }

                    i.SetDataState(ELevelDataState.Destroying);
                    i.LevelData?.Dispose();
                    i.LevelData = null;
                    i.SetDataState(ELevelDataState.Unloaded);
                }
                UnloadingLevels.Clear();
            }
        }
        protected bool IsStreaming = true;
        public HashSet<TtTerrainLevel> StreamingLevels = new HashSet<TtTerrainLevel>();
        public List<TtTerrainLevel> UnloadingLevels = new List<TtTerrainLevel>();
        public void PushStreamingLevel(TtTerrainLevel level, bool bForce)
        {
            bool waitInFlight = false;
            lock (this)
            {
                if (IsStreaming == false)
                    return;
                if (UnloadingLevels.Contains(level))
                {
                    // 还在延迟卸载队列里倒计时: 直接捞回来复用, 不重建 LevelData
                    UnloadingLevels.Remove(level);
                    if (level.LevelData != null)
                    {
                        level.SetDataState(ELevelDataState.Loaded);
                        return;
                    }
                }

                if (level.LevelData != null)
                {
                    level.SetDataState(ELevelDataState.Loaded);
                    return;
                }
                if (StreamingLevels.Contains(level))
                {
                    // 已有在飞加载; bForce 时需要等它落地, 但绝不能持有锁等待:
                    // 在飞任务完成时也要拿这把锁才能把 LevelData 赋值进来, 锁内 spin 会死锁到超时
                    waitInFlight = bForce;
                }
                else
                {
                    StreamingLevels.Add(level);
                    level.SetDataState(ELevelDataState.Loading);
                    CreateLevelData(level, bForce).AddWaitTask();
                }
            }

            if (waitInFlight)
            {
                // 在飞加载尚未完成 (LevelData 还是 null), 锁外阻塞等待它落地;
                // 原实现在锁内且条件写反成 != null, force 等待静默失效
                var t1 = Support.TtTime.GetTickCount();
                while (level.LevelData == null)
                {
                    var t2 = Support.TtTime.GetTickCount();
                    if (t2 - t1 > 1000 * 15)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelZ}, force = true) time out");
                        return;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        public async Thread.Async.TtTask CreateLevelData(TtTerrainLevel level, bool bForce)
        {
            var LevelData = new TtTerrainLevelData();
            await LevelData.CreateLevelData(level, bForce);
            lock (this)
            {
                StreamingLevels.Remove(level);
                // Dispose 后完成的在飞任务: 新建的 LevelData 必须在这里自行释放,
                // 否则会挂到已被重建摘除的UTerrainLevel 上永久泄漏 GPU 资源
                if (IsStreaming == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelZ}): IsStreaming == false");
                    LevelData.Dispose();
                    level.SetDataState(ELevelDataState.Unloaded);
                    return;
                }
                if (level.LevelData == null)
                {
                    level.LevelData = LevelData;
                    level.SetDataState(ELevelDataState.Loaded);
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"CreateLevelData({level.LevelX},{level.LevelZ})Level ForceLoad in async streaming == 2");
                    LevelData.Dispose();
                    level.SetDataState(ELevelDataState.Loaded);
                }
            }
        }
        public void PushUnloadLevel(TtTerrainLevel level)
        {
            lock(this)
            {
                if (level.LevelData == null)
                {
                    // LevelData 为空时可能是 Loading 在飞 (还在 StreamingLevels 里), 不能把状态改成 Unloaded,
                    // 否则异步完成前 goto 会误判为"已彻底销毁"又排一次加载
                    return;
                }
                if (UnloadingLevels.Contains(level))
                    return;
                level.LevelData.RemainUnloadTime = 10.0f;
                UnloadingLevels.Add(level);
                level.SetDataState(ELevelDataState.PendingDestroy);
            }
        }

        /// <summary>
        /// 把还在延迟卸载队列里倒计时的 level 捞回来继续用 (不重建 LevelData)。
        /// 返回 false 表示它已经不在队列里 (可能正在/已经销毁), 调用方要按当前状态重新判定。
        /// </summary>
        public bool ReviveUnloadingLevel(TtTerrainLevel level)
        {
            lock (this)
            {
                if (level == null || level.LevelData == null)
                    return false;
                if (UnloadingLevels.Contains(level) == false)
                    return false;

                UnloadingLevels.Remove(level);
                level.SetDataState(ELevelDataState.Loaded);
                return true;
            }
        }

        /// <summary>
        /// 阻塞等待某个 level 的 Dispose 结束 (状态离开 Destroying)。
        /// Dispose 在 Tick 里是持本对象锁执行的, 所以拿到锁即说明本轮销毁已完成;
        /// 超时只打日志并返回, 由调用方自行复核状态。
        /// </summary>
        public void WaitDestroyComplete(TtTerrainLevel level, int timeoutMs = 15000)
        {
            if (level == null)
                return;

            var t1 = Support.TtTime.GetTickCount();
            while (true)
            {
                lock (this)
                {
                    if (level.DataState != ELevelDataState.Destroying)
                        return;
                }

                if (Support.TtTime.GetTickCount() - t1 > timeoutMs)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning,
                        $"WaitDestroyComplete({level.LevelX},{level.LevelZ}) time out, state={level.DataState}");
                    return;
                }
                System.Threading.Thread.Sleep(10);
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
                        tmp.SetDataState(ELevelDataState.Unloaded);
                        UnloadingLevels.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        tmp.LevelData.RemainUnloadTime -= elapsedSecond;
                        if (tmp.LevelData.RemainUnloadTime < 0)
                        {
                            tmp.SetDataState(ELevelDataState.Destroying);
                            tmp.LevelData?.Dispose();
                            tmp.LevelData = null;
                            tmp.SetDataState(ELevelDataState.Unloaded);

                            UnloadingLevels.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}
