using System;
using System.Collections.Generic;
using System.IO;

namespace EngineNS.Editor.AndroidBuild
{
    // 扫描 androidSdk 目录，给面板展示已安装的 platforms / build-tools / adb。
    public static class TtAndroidSdkInventory
    {
        public static List<string> GetPlatforms(string sdkRoot)
        {
            return ListSorted(sdkRoot, "platforms", "android-");
        }

        public static List<string> GetBuildTools(string sdkRoot)
        {
            return ListSorted(sdkRoot, "build-tools", null);
        }

        // platform-tools/adb.exe 存在才返回完整路径，否则空串（面板据此禁用部署按钮）。
        public static string GetAdbPath(string sdkRoot)
        {
            if (string.IsNullOrWhiteSpace(sdkRoot))
                return string.Empty;
            var adb = Path.Combine(sdkRoot, "platform-tools", "adb.exe");
            return File.Exists(adb) ? adb : string.Empty;
        }

        private static List<string> ListSorted(string sdkRoot, string subDir, string prefix)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(sdkRoot))
                return result;
            var dir = Path.Combine(sdkRoot, subDir);
            if (!Directory.Exists(dir))
                return result;
            foreach (var entry in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileName(entry);
                if (prefix != null && !name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;
                result.Add(name);
            }
            // 平台按 API 等级降序、build-tools 按版本号降序；都由最后一段数字决定。
            result.Sort((a, b) => CompareByVersion(b, a));
            return result;
        }

        private static int CompareByVersion(string a, string b)
        {
            var partsA = a.Split('-', '.');
            var partsB = b.Split('-', '.');
            var len = Math.Max(partsA.Length, partsB.Length);
            for (var i = 0; i < len; i++)
            {
                var textA = i < partsA.Length ? partsA[i] : "0";
                var textB = i < partsB.Length ? partsB[i] : "0";
                int numA = int.TryParse(textA, out var a2) ? a2 : 0;
                int numB = int.TryParse(textB, out var b2) ? b2 : 0;
                var cmp = numA.CompareTo(numB);
                if (cmp != 0)
                    return cmp;
            }
            return string.CompareOrdinal(a, b);
        }
    }
}
