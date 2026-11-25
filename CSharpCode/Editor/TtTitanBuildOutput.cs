using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;

namespace EngineNS.Editor
{
    // 托管构建产物布局：所有托管程序集都输出到 binaries/<TFM>/（见 BuildScript/msbuild/TitanManagedEngine.props 的 OutputPath）。
    // TFM 的单一事实源是 BuildScript/common/dependencies.json 的 dotnetSdk.targetFramework，代码里不写死版本号；
    // 清单缺失（例如已部署的产物）时退回当前程序集自身编译时的 TFM。
    public static class TtTitanBuildOutput
    {
        public const string ManagedBinariesDirectoryName = "binaries";
        public const string TitanCmdFileName = "TitanCMD.exe";

        // 从 startDirectory 向上找含 BuildScript/common/dependencies.json 的仓库根；找不到返回 null。
        public static string FindRepositoryRoot(string startDirectory)
        {
            if (string.IsNullOrEmpty(startDirectory))
                return null;
            var dir = new DirectoryInfo(Path.GetFullPath(startDirectory));
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "BuildScript", "common", "dependencies.json")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        // 读 dependencies.json 的 dotnetSdk.targetFramework；文件缺失、JSON 非法或字段为空都返回 null。
        public static string ReadTargetFramework(string repositoryRoot)
        {
            if (string.IsNullOrEmpty(repositoryRoot))
                return null;
            var path = Path.Combine(repositoryRoot, "BuildScript", "common", "dependencies.json");
            if (!File.Exists(path))
                return null;
            try
            {
                using (var doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    JsonElement dotnet;
                    JsonElement tfm;
                    if (!doc.RootElement.TryGetProperty("dotnetSdk", out dotnet))
                        return null;
                    if (!dotnet.TryGetProperty("targetFramework", out tfm))
                        return null;
                    var value = tfm.GetString();
                    return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                }
            }
            catch (Exception)
            {
                // 清单损坏不该让编辑器报错：调用方会退回程序集自身的 TFM
                return null;
            }
        }

        // 读 dependencies.json 的 androidToolchain.<name>：面板探测与 android-toolchain.json 的缺省值都从这里取，
        // 升级 NDK / 提高 apiLevel / 改 CMake 下限时只改清单，不在代码里写死版本号。缺项或非法时返回 fallback。
        public static string ReadAndroidToolchainString(string repositoryRoot, string name, string fallback)
        {
            JsonElement value;
            if (!TryReadAndroidToolchain(repositoryRoot, name, out value))
                return fallback;
            if (value.ValueKind != JsonValueKind.String)
                return fallback;
            var text = value.GetString();
            return string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
        }

        // 同上，取整数属性（ndkMajor / apiLevel / dotnetSdkMajor）；数字与数字字符串都接受。
        public static int ReadAndroidToolchainInt(string repositoryRoot, string name, int fallback)
        {
            JsonElement value;
            if (!TryReadAndroidToolchain(repositoryRoot, name, out value))
                return fallback;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                return number;
            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number))
                return number;
            return fallback;
        }

        private static bool TryReadAndroidToolchain(string repositoryRoot, string name, out JsonElement value)
        {
            value = default(JsonElement);
            if (string.IsNullOrEmpty(repositoryRoot) || string.IsNullOrEmpty(name))
                return false;
            var path = Path.Combine(repositoryRoot, "BuildScript", "common", "dependencies.json");
            if (!File.Exists(path))
                return false;
            try
            {
                using (var doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    JsonElement android;
                    if (!doc.RootElement.TryGetProperty("androidToolchain", out android))
                        return false;
                    JsonElement found;
                    if (!android.TryGetProperty(name, out found))
                        return false;
                    // JsonDocument 释放后元素失效，必须先克隆。
                    value = found.Clone();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 当前程序集编译时的 TFM 短名（net10.0 这类）：读 TargetFrameworkAttribute，退回程序集所在目录名。
        public static string GetAssemblyTargetFramework()
        {
            var assembly = typeof(TtTitanBuildOutput).Assembly;
            var attr = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            var name = attr != null ? attr.FrameworkName : null;
            const string versionMark = "Version=v";
            if (!string.IsNullOrEmpty(name))
            {
                var index = name.IndexOf(versionMark, StringComparison.Ordinal);
                if (index >= 0)
                    return "net" + name.Substring(index + versionMark.Length);
            }
            var dir = Path.GetFileName(Path.GetDirectoryName(assembly.Location));
            return string.IsNullOrEmpty(dir) ? null : dir;
        }

        // 解析 TFM：优先 dependencies.json，退回程序集自身 TFM。
        public static string ResolveTargetFramework(string repositoryRoot)
        {
            return ReadTargetFramework(repositoryRoot) ?? GetAssemblyTargetFramework();
        }

        // 仓库内 binaries/<TFM>/<fileName> 的相对路径；TFM 无法解析时返回 null。
        public static string GetManagedOutputRelativePath(string repositoryRoot, string fileName)
        {
            var tfm = ResolveTargetFramework(repositoryRoot);
            if (string.IsNullOrEmpty(tfm))
                return null;
            return Path.Combine(ManagedBinariesDirectoryName, tfm, fileName);
        }

        // <binariesDirectory>/<TFM>/<fileName> 的绝对路径；binariesDirectory 一般是 binaries 或其子目录（如已是 <TFM> 目录则不再重复拼），
        // TFM 无法解析时返回 null。
        public static string GetManagedOutputPathIn(string binariesDirectory, string fileName)
        {
            if (string.IsNullOrEmpty(binariesDirectory))
                return null;
            var tfm = ResolveTargetFramework(FindRepositoryRoot(binariesDirectory));
            if (string.IsNullOrEmpty(tfm))
                return null;
            var trimmed = binariesDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.Equals(Path.GetFileName(trimmed), tfm, StringComparison.OrdinalIgnoreCase))
                return Path.Combine(trimmed, fileName);
            return Path.Combine(binariesDirectory, tfm, fileName);
        }
    }
}
