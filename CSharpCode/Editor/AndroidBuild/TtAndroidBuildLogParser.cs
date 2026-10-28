namespace EngineNS.Editor.AndroidBuild
{
    // 从构建输出中提取面板需要的信息。build-android.ps1 结束时打印 APK=<绝对路径>。
    public static class TtAndroidBuildLogParser
    {
        public static string ExtractApkPath(string log)
        {
            if (string.IsNullOrEmpty(log))
                return null;
            string result = null;
            foreach (var rawLine in log.Replace("\r\n", "\n").Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.StartsWith("APK="))
                    result = line.Substring(4).Trim();
            }
            return result;
        }

        // build-android.ps1 在各阶段块开头输出的机器可读标记：[TITAN-STAGE] Native/Managed/Content/Apk。
        static readonly string[] KnownStages = { "Native", "Managed", "Content", "Apk" };

        // 非 [TITAN-STAGE] 行、未知阶段名或空行返回 null。
        public static string ParseStageMarker(string line)
        {
            if (string.IsNullOrEmpty(line))
                return null;
            var text = line.Trim();
            if (!text.StartsWith("[TITAN-STAGE]"))
                return null;
            var stage = text.Substring("[TITAN-STAGE]".Length).Trim();
            foreach (var known in KnownStages)
            {
                if (stage == known)
                    return stage;
            }
            return null;
        }

        // 整段输出中最后一次出现的阶段标记；没有则 null。
        public static string FindLastStageMarker(string log)
        {
            if (string.IsNullOrEmpty(log))
                return null;
            string result = null;
            foreach (var rawLine in log.Replace("\r\n", "\n").Split('\n'))
            {
                var stage = ParseStageMarker(rawLine);
                if (stage != null)
                    result = stage;
            }
            return result;
        }

        // 宽匹配提示用：面板只拿来标红日志，真正的成败以进程退出码为准。
        public static bool LooksLikeFailure(string line)
        {
            if (string.IsNullOrEmpty(line))
                return false;
            var lower = line.ToLowerInvariant();
            if (lower.Contains("error") || lower.Contains("failed") || lower.Contains("exception"))
                return true;
            return line.Contains("错误") || line.Contains("失败");
        }
    }
}
