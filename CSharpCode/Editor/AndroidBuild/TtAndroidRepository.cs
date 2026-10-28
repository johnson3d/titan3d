using System.Collections.Generic;
using System.IO;

namespace EngineNS.Editor.AndroidBuild
{
    public static class TtAndroidRepository
    {
        public const string BuildScriptRelativePath = "BuildScript/android/build-android.ps1";
        public const string ToolchainConfigRelativePath = "cache/config/android-toolchain.json";
        public const string ContentSpecRelativePath = "Module/AloneGame.Maui/TitanContent.txt";
        public const string BuildUiStateRelativePath = "cache/config/android-build.json";

        // 从任意工作目录向上找 BuildScript/android/build-android.ps1，避免把仓库根写死。
        public static string FindRoot(string startDirectory)
        {
            if (string.IsNullOrEmpty(startDirectory))
                return null;
            var dir = new DirectoryInfo(Path.GetFullPath(startDirectory));
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "BuildScript", "android", "build-android.ps1")))
                    return dir.FullName.TrimEnd('\\');
                dir = dir.Parent;
            }
            return null;
        }

        public static string ToFullPath(string root, string path)
        {
            var text = (path ?? string.Empty).Trim().Replace('/', '\\');
            if (text.Length >= 2 && text[1] == ':')
                return text.TrimEnd('\\');
            if (text.Length == 0)
                return root;
            return Path.Combine(root, text).TrimEnd('\\');
        }
    }
}
