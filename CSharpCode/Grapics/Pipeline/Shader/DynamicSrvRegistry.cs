using System;
using System.Collections.Generic;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.Shader
{
    /// <summary>
    /// 动态 SRV 注册表条目，描述一个运行时产生的 GPU 纹理资源
    /// </summary>
    public class TtDynamicSrvEntry
    {
        public string Name { get; set; }
        public TtSrView Srv { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public EPixelFormat Format { get; set; }

        /// <summary>
        /// 注册者（弱引用），用于在生产者销毁时自动清理
        /// </summary>
        public WeakReference ProducerRef { get; set; }
    }

    /// <summary>
    /// 全局动态 SRV 注册表。
    /// 生产者（如 TtSWEWaterNode）在运行时注册动态纹理 SRV，
    /// 消费者（材质系统 GetSRV）按名字查找。
    /// </summary>
    public class TtDynamicSrvRegistry
    {
        readonly Dictionary<string, TtDynamicSrvEntry> mEntries = new Dictionary<string, TtDynamicSrvEntry>();

        /// <summary>
        /// 注册或更新一个动态 SRV
        /// </summary>
        public void Register(string name, TtSrView srv, uint width, uint height, EPixelFormat format, object producer)
        {
            if (mEntries.TryGetValue(name, out var existing))
            {
                existing.Srv = srv;
                existing.Width = width;
                existing.Height = height;
                existing.Format = format;
                existing.ProducerRef = new WeakReference(producer);
            }
            else
            {
                mEntries[name] = new TtDynamicSrvEntry()
                {
                    Name = name,
                    Srv = srv,
                    Width = width,
                    Height = height,
                    Format = format,
                    ProducerRef = new WeakReference(producer),
                };
            }
        }

        /// <summary>
        /// 按名字查找动态 SRV，不存在则返回 null
        /// </summary>
        public TtSrView Find(string name)
        {
            if (mEntries.TryGetValue(name, out var entry))
                return entry.Srv;
            return null;
        }

        /// <summary>
        /// 查找完整条目（含分辨率、格式等元信息）
        /// </summary>
        public TtDynamicSrvEntry FindEntry(string name)
        {
            mEntries.TryGetValue(name, out var entry);
            return entry;
        }

        /// <summary>
        /// 注销一个动态 SRV
        /// </summary>
        public void Unregister(string name)
        {
            mEntries.Remove(name);
        }

        /// <summary>
        /// 清理所有已失效的条目（生产者已被 GC 回收）
        /// </summary>
        public void PurgeStale()
        {
            var staleKeys = new List<string>();
            foreach (var kvp in mEntries)
            {
                if (kvp.Value.ProducerRef != null && !kvp.Value.ProducerRef.IsAlive)
                    staleKeys.Add(kvp.Key);
            }
            foreach (var key in staleKeys)
            {
                mEntries.Remove(key);
            }
        }

        public void Cleanup()
        {
            mEntries.Clear();
        }
    }
}
