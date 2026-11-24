using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Titan.Build
{
    /// <summary>
    /// 宿主环境探测（不是平台策略）：求值器只做“已知属性替换”，属性值由消费方注入。
    /// </summary>
    public static class WindowsSdk
    {
        /// <summary>注册表里的 Windows SDK include 根（ucrt/shared/um），顺序与 clang 期望一致；非 Windows 或未安装返回空串。</summary>
        public static string IncludePath()
        {
            if (!OperatingSystem.IsWindows())
                return string.Empty;
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");
            if (!(key?.GetValue("KitsRoot10") is string kitsRoot10) || string.IsNullOrEmpty(kitsRoot10))
                return string.Empty;
            var includeRoot = Path.Combine(kitsRoot10, "Include");
            if (!Directory.Exists(includeRoot))
                return string.Empty;
            var latest = Directory.GetDirectories(includeRoot).OrderByDescending(v => v, StringComparer.Ordinal).First();
            return string.Join(";", Path.Combine(latest, "ucrt"), Path.Combine(latest, "shared"), Path.Combine(latest, "um"));
        }
    }
}
