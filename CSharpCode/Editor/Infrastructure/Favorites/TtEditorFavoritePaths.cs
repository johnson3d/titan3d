using System;
using System.Collections.Generic;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 收藏夹里的一条路径记录。Path 是唯一键(判重与写回都用它), Display 只影响下拉里的显示文字。
    /// </summary>
    public class TtFavoritePathEntry
    {
        /// <summary>规范化的路径字符串, 例如骨骼: "tutorials/animation/graychan.skt:TwintailE_L"</summary>
        public string Path;
        /// <summary>下拉里显示的短名, 为空时退化成 Path</summary>
        public string Display;
        /// <summary>添加来源(哪个编辑器/哪个资产加进来的), 只用于 tooltip</summary>
        public string Source;

        public string DisplayOrPath => string.IsNullOrEmpty(Display) ? Path : Display;
    }

    /// <summary>
    /// 编辑器级"路径收藏夹": 按 channel(字符串通道) 保存一组路径字符串, 供跨编辑器写入与读取。
    ///
    /// 设计要点:
    ///   - 只活在本次会话(纯静态内存), 关掉编辑器即清空, 不落盘、不进资产。
    ///   - 按 channel 键而不是按类型键, 这样同一个类型可以分出多个用途通道(骨骼 / 末端骨骼 / 插槽 ...);
    ///     同时支持 RegisterTypeChannel 给某个属性类型登记默认 channel, 属性上不写 Channel 时按类型推导。
    ///   - 只存字符串, 具体语义由 channel 的生产/消费双方约定, 机制本身与类型无关。
    ///   - 添加动作统一由 PropertyGrid 上的 TtPGFavoritePathAttribute 触发(详见该类注释),
    ///     不在各编辑器里散落"加入收藏夹"的入口。
    /// </summary>
    public static class TtEditorFavoritePaths
    {
        /// <summary>骨骼通道: 条目格式 "骨架资产名:骨骼名"</summary>
        public const string ChannelBone = "Bone";

        /// <summary>每个 channel 最多保留多少条, 超出后丢弃最旧的</summary>
        public static int Capacity = 32;

        static Dictionary<string, List<TtFavoritePathEntry>> mChannels = new Dictionary<string, List<TtFavoritePathEntry>>();
        static Dictionary<Type, string> mTypeChannels = new Dictionary<Type, string>();

        /// <summary>
        /// 给某个属性类型登记默认 channel。属性上没显式写 Channel 时, 按属性类型查这里。
        /// </summary>
        public static void RegisterTypeChannel(Type type, string channel)
        {
            if (type == null || string.IsNullOrEmpty(channel))
                return;
            mTypeChannels[type] = channel;
        }

        /// <summary>
        /// 解析实际使用的 channel: 优先属性上显式指定的, 否则按属性类型查登记表, 都没有则返回 null。
        /// </summary>
        public static string ResolveChannel(string explicitChannel, Type propertyType)
        {
            if (!string.IsNullOrEmpty(explicitChannel))
                return explicitChannel;
            if (propertyType != null && mTypeChannels.TryGetValue(propertyType, out var channel))
                return channel;
            return null;
        }

        /// <summary>
        /// 取某个 channel 的收藏列表(最近添加的在最前)。channel 不存在时返回空列表, 不会返回 null。
        /// </summary>
        public static List<TtFavoritePathEntry> Get(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return mEmpty;
            if (mChannels.TryGetValue(channel, out var list))
                return list;
            return mEmpty;
        }
        static List<TtFavoritePathEntry> mEmpty = new List<TtFavoritePathEntry>();

        public static bool Contains(string channel, string path)
        {
            return Find(channel, path) != null;
        }

        public static TtFavoritePathEntry Find(string channel, string path)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(path))
                return null;
            if (!mChannels.TryGetValue(channel, out var list))
                return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Path == path)
                    return list[i];
            }
            return null;
        }

        /// <summary>
        /// 添加一条收藏。已存在则把它提到最前(当成"最近使用"), 返回 false 表示是重复项没有新增。
        /// </summary>
        public static bool Add(string channel, string path, string display = null, string source = null)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(path))
                return false;

            if (!mChannels.TryGetValue(channel, out var list))
            {
                list = new List<TtFavoritePathEntry>();
                mChannels.Add(channel, list);
            }

            var exist = Find(channel, path);
            if (exist != null)
            {
                list.Remove(exist);
                list.Insert(0, exist);
                if (!string.IsNullOrEmpty(display))
                    exist.Display = display;
                return false;
            }

            list.Insert(0, new TtFavoritePathEntry() { Path = path, Display = display, Source = source });
            while (list.Count > Capacity && Capacity > 0)
                list.RemoveAt(list.Count - 1);
            return true;
        }

        public static bool Remove(string channel, string path)
        {
            var exist = Find(channel, path);
            if (exist == null)
                return false;
            return mChannels[channel].Remove(exist);
        }

        public static void Clear(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;
            if (mChannels.TryGetValue(channel, out var list))
                list.Clear();
        }

        public static void ClearAll()
        {
            mChannels.Clear();
        }

        /// <summary>
        /// 已经有内容的 channel 名字列表, 用于调试面板之类的场合。
        /// </summary>
        public static IEnumerable<string> Channels => mChannels.Keys;

        // ─── 骨骼通道的路径拼装/解析 ────────────────────────────────
        // 约定 "骨架资产名:骨骼名"。同一骨架内骨骼名唯一(TtSkinSkeleton.HashDic 以 NameHash 为键),
        // 所以不需要全层级路径; 也刻意不存骨骼 Index, 避免骨架重导入后索引漂移。

        public static string MakeBonePath(RName skeletonAssetName, string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return null;
            var skeleton = skeletonAssetName?.Name;
            if (string.IsNullOrEmpty(skeleton))
                return boneName;
            return skeleton + ":" + boneName;
        }

        /// <summary>
        /// 拆出骨架资产名与骨骼名。没有 ':' 时视为只给了骨骼名, skeletonName 返回 null。
        /// </summary>
        public static bool TryParseBonePath(string path, out string skeletonName, out string boneName)
        {
            skeletonName = null;
            boneName = null;
            if (string.IsNullOrEmpty(path))
                return false;
            var idx = path.LastIndexOf(':');
            if (idx < 0)
            {
                boneName = path;
                return true;
            }
            skeletonName = path.Substring(0, idx);
            boneName = path.Substring(idx + 1);
            return !string.IsNullOrEmpty(boneName);
        }
    }
}
