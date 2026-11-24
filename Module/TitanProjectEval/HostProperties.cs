using System;
using System.Collections.Generic;

namespace Titan.Build
{
    /// <summary>
    /// 宿主环境能提供的 MSBuild 属性。求值器本身只做“已知属性替换”，不认识 $(...) 一律按未解析失败，
    /// 因此需要真实值的消费方（Windows codegen、TitanCMD eval）从这里取，避免各写一份。
    /// </summary>
    public static class HostProperties
    {
        /// <summary>
        /// 这两个属性在 MSBuild 里始终有定义，因此按目标平台给值而不是“缺键”：
        /// 非 Windows 目标的 SDK include 为空串，host-only 的 include 项由 clang 投影侧按原始文本丢弃；
        /// 求值阶段只负责让 $(...) 全部能被替换，不认识的一律按未解析失败。
        /// </summary>
        public static Dictionary<string, string> ForPlatform(string platform)
        {
            var windows = string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase);
            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WindowsSDK_IncludePath"] = windows ? WindowsSdk.IncludePath() : string.Empty,
                // VC 工具集 include 不属于工程声明，统一按空处理，不把机器上的 CRT 头带进求值结果。
                ["VC_IncludePath"] = string.Empty,
            };
            return properties;
        }
    }
}
